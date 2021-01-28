Imports NLog
Imports System.IO
Imports Utilities.Time
Imports System.Net.Http
Imports System.Threading
Imports Utilities.Numbers
Imports Utilities.Network
Imports Algo2TradeCore.Adapter
Imports Algo2TradeCore.Entities
Imports Algo2TradeCore.Strategies
Imports Algo2TradeCore.Calculator
Imports Algo2TradeCore.ChartHandler.ChartStyle

Public Class NFOStrategyInstrument
    Inherits StrategyInstrument
    Implements IDisposable

#Region "Logging and Status Progress"
    Public Shared Shadows logger As Logger = LogManager.GetCurrentClassLogger
#End Region

    Public ReadOnly Property PreProcessingDone As Boolean

    Public ReadOnly Property SignalData As SignalDetails

    Private _executeCommandData As Trade = Nothing
    Private _executeCommand As ExecuteCommands = ExecuteCommands.ForceCancelRegularOrder
    Private _lastAttempt As Integer = -1

    Private _eodPayload As Dictionary(Of Date, OHLCPayload) = Nothing
    Private _pivotTrendPayload As Dictionary(Of Date, Color) = Nothing
    Private _atrPayload As Dictionary(Of Date, Decimal) = Nothing

    Public Sub New(ByVal associatedInstrument As IInstrument,
                   ByVal associatedParentStrategy As Strategy,
                   ByVal isPairInstrumnet As Boolean,
                   ByVal canceller As CancellationTokenSource)
        MyBase.New(associatedInstrument, associatedParentStrategy, isPairInstrumnet, canceller)
        Select Case Me.ParentStrategy.ParentController.BrokerSource
            Case APISource.Zerodha
                _APIAdapter = New ZerodhaAdapter(ParentStrategy.ParentController, _cts)
            Case APISource.AliceBlue
                _APIAdapter = New AliceAdapter(ParentStrategy.ParentController, Me.TradableInstrument, _cts)
            Case APISource.Upstox
                Throw New NotImplementedException
            Case APISource.None
                Throw New NotImplementedException
        End Select
        AddHandler _APIAdapter.Heartbeat, AddressOf OnHeartbeat
        AddHandler _APIAdapter.WaitingFor, AddressOf OnWaitingFor
        AddHandler _APIAdapter.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
        AddHandler _APIAdapter.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete

        Me.TradableInstrument.FetchHistorical = False
        Dim dummySignal As SignalDetails = New SignalDetails(Me.TradableInstrument.RawInstrumentName)
        If Me.TradableInstrument.InstrumentType = IInstrument.TypeOfInstrument.Cash Then
            If File.Exists(dummySignal.SignalDetailsFilename) Then
                Me.SignalData = Utilities.Strings.DeserializeToCollection(Of SignalDetails)(dummySignal.SignalDetailsFilename)
            Else
                Me.SignalData = New SignalDetails(Me.TradableInstrument.RawInstrumentName)
                Utilities.Strings.SerializeFromCollection(Of SignalDetails)(Me.SignalData.SignalDetailsFilename, Me.SignalData)
            End If
        Else
            Me.SignalData = Utilities.Strings.DeserializeToCollection(Of SignalDetails)(dummySignal.SignalDetailsFilename)
        End If

        If Me.SignalData.IsActiveSignal Then
            CType(Me.ParentStrategy, NFOStrategy).TotalActiveInstrumentCount += 1
        End If
    End Sub

    Public Overrides Function PopulateChartAndIndicatorsAsync(candleCreator As Chart, currentCandle As OHLCPayload) As Task
        Throw New NotImplementedException
    End Function

    Public Overrides Async Function MonitorAsync() As Task
        Try
            _strategyInstrumentRunning = True
            Dim preProcess As Boolean = Await CompletePreProcessing().ConfigureAwait(False)
            If preProcess Then
                _PreProcessingDone = True
                Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
                Dim currentTick As ITick = Me.TradableInstrument.LastTick

                While True
                    If Me.ParentStrategy.ParentController.OrphanException IsNot Nothing Then
                        Throw Me.ParentStrategy.ParentController.OrphanException
                    End If
                    If Me._RMSException IsNot Nothing AndAlso
                        _RMSException.ExceptionType = Algo2TradeCore.Exceptions.AdapterBusinessException.TypeOfException.RMSError Then
                        OnHeartbeat(String.Format("{0}:Will not take no more action in this instrument as RMS Error occured. Error-{1}", Me.TradableInstrument.TradingSymbol, _RMSException.Message))
                        Throw Me._RMSException
                    End If
                    _cts.Token.ThrowIfCancellationRequested()

                    currentTick = Me.TradableInstrument.LastTick
                    If Now >= userSettings.TradeEntryTime AndAlso Not _eodPayload.ContainsKey(Now.Date) Then
                        Dim currentCandle As OHLCPayload = New OHLCPayload(OHLCPayload.PayloadSource.CalculatedTick)
                        currentCandle.TradingSymbol = Me.TradableInstrument.TradingSymbol
                        currentCandle.SnapshotDateTime = currentTick.Timestamp.Value.Date
                        currentCandle.OpenPrice.Value = currentTick.Open
                        currentCandle.LowPrice.Value = currentTick.Low
                        currentCandle.HighPrice.Value = currentTick.High
                        currentCandle.ClosePrice.Value = currentTick.LastPrice
                        currentCandle.Volume.Value = currentTick.Volume
                        currentCandle.PreviousPayload = _eodPayload.LastOrDefault.Value

                        _eodPayload.Add(currentCandle.SnapshotDateTime, currentCandle)

                        _atrPayload = Nothing
                        CalculateATR(userSettings.ATRPeriod, _eodPayload, _atrPayload)
                        _pivotTrendPayload = Nothing
                        CalculatePivotHighLowTrend(userSettings.PivotPeriod, userSettings.PivotTrendPeriod, _eodPayload, Nothing, Nothing, _pivotTrendPayload)
                    End If

                    _cts.Token.ThrowIfCancellationRequested()
                    'Place Order block start
                    Dim signal As Tuple(Of Boolean, OHLCPayload, IOrder.TypeOfTransaction) = GetEntrySignal(currentTick)
                    If signal IsNot Nothing AndAlso signal.Item1 Then
                        Dim currentMinute As Date = New Date(Now.Year, Now.Month, Now.Day, Now.Hour, Now.Minute, 0)
                        Dim currentATMOption As NFOStrategyInstrument = Nothing
                        If signal.Item3 = IOrder.TypeOfTransaction.Buy Then
                            If _lastAttempt = -1 Then
                                currentATMOption = Await GetCurrentATMOption(currentTick, IOrder.TypeOfTransaction.Buy, "CE").ConfigureAwait(False)
                                _lastAttempt = 1
                            ElseIf _lastAttempt = 1 Then
                                currentATMOption = Await GetCurrentATMOption(currentTick, IOrder.TypeOfTransaction.Sell, "CE").ConfigureAwait(False)
                                _lastAttempt = -1
                            End If
                        ElseIf signal.Item3 = IOrder.TypeOfTransaction.Sell Then
                            If _lastAttempt = -1 Then
                                currentATMOption = Await GetCurrentATMOption(currentTick, IOrder.TypeOfTransaction.Sell, "PE").ConfigureAwait(False)
                                _lastAttempt = 1
                            ElseIf _lastAttempt = 1 Then
                                currentATMOption = Await GetCurrentATMOption(currentTick, IOrder.TypeOfTransaction.Buy, "PE").ConfigureAwait(False)
                                _lastAttempt = -1
                            End If
                        End If
                        If currentATMOption IsNot Nothing AndAlso currentATMOption.TradableInstrument.LastTick IsNot Nothing Then
                            Dim childTag As String = System.Guid.NewGuid.ToString()
                            Dim parentTag As String = childTag
                            Dim tradeNumber As Integer = 1
                            Dim entryType As EntryType = EntryType.Fresh
                            Dim lossToRecover As Decimal = 0

                            Dim spotPrice As Decimal = currentTick.LastPrice
                            Dim spotATR As Decimal = _atrPayload.LastOrDefault.Value
                            Dim lastTrade As Trade = Me.SignalData.GetLastTrade()
                            If lastTrade IsNot Nothing Then
                                Dim allTrades As List(Of Trade) = Me.SignalData.GetAllTradesByParentTag(lastTrade.ParentTag)
                                If allTrades IsNot Nothing AndAlso allTrades.Count > 0 Then
                                    Dim pl As Decimal = 0
                                    For Each runningTrade In allTrades
                                        If runningTrade.CurrentStatus = TradeStatus.Complete Then
                                            pl += _APIAdapter.CalculatePLWithBrokerage(currentATMOption.TradableInstrument, runningTrade.EntryPrice, runningTrade.ExitPrice, runningTrade.Quantity)
                                        End If
                                    Next
                                    If pl < 0 Then
                                        parentTag = lastTrade.ParentTag
                                        tradeNumber = lastTrade.TradeNumber + 1
                                        entryType = EntryType.LossMakeup
                                        lossToRecover = Math.Abs(pl)
                                    End If
                                End If
                            End If

                            Dim quantity As Integer = currentATMOption.TradableInstrument.LotSize
                            If entryType = EntryType.LossMakeup Then
                                Dim entryPrice As Decimal = currentATMOption.TradableInstrument.LastTick.LastPrice
                                Dim targetPrice As Decimal = ConvertFloorCeling(entryPrice + spotATR, currentATMOption.TradableInstrument.TickSize, RoundOfType.Celing)
                                For ctr As Integer = 1 To Integer.MaxValue
                                    Dim pl As Decimal = _APIAdapter.CalculatePLWithBrokerage(currentATMOption.TradableInstrument, entryPrice, targetPrice, ctr * currentATMOption.TradableInstrument.LotSize)
                                    If pl >= lossToRecover Then
                                        lossToRecover = (pl - 1) * -1
                                        quantity = ctr * currentATMOption.TradableInstrument.LotSize + currentATMOption.TradableInstrument.LotSize
                                        Exit For
                                    End If
                                Next
                            End If

                            Dim drctn As TradeDirection = TradeDirection.None
                            If signal.Item3 = IOrder.TypeOfTransaction.Buy Then
                                drctn = TradeDirection.Buy
                            ElseIf signal.Item3 = IOrder.TypeOfTransaction.Sell Then
                                drctn = TradeDirection.Sell
                            End If

                            Dim dummyTrade As Trade = New Trade With {
                                .ChildTag = childTag,
                                .ContractRemark = _lastAttempt,
                                .CurrentStatus = TradeStatus.Open,
                                .Direction = drctn,
                                .ParentTag = parentTag,
                                .PotentialTarget = lossToRecover,
                                .Quantity = quantity,
                                .SignalDate = signal.Item2.SnapshotDateTime,
                                .SpotATR = spotATR,
                                .SpotPrice = spotPrice,
                                .TradeNumber = tradeNumber,
                                .TradingSymbol = currentATMOption.TradableInstrument.TradingSymbol,
                                .TypeOfEntry = entryType
                            }

                            Await currentATMOption.MonitorAsync(ExecuteCommands.PlaceRegularMarketCNCOrder, dummyTrade).ConfigureAwait(False)
                            lastTrade = Me.SignalData.GetLastTrade()
                            If lastTrade IsNot Nothing AndAlso lastTrade.CurrentStatus = TradeStatus.InProgress Then
                                _lastAttempt = -1
                            End If
                        End If
                    End If
                    'Place Order block end
                    _cts.Token.ThrowIfCancellationRequested()
                    'Exit Order block start
                    Dim lastRunningTrade As Trade = Me.SignalData.GetLastTrade()
                    If lastRunningTrade IsNot Nothing AndAlso lastRunningTrade.CurrentStatus = TradeStatus.InProgress Then
                        Dim optionType As String = lastRunningTrade.TradingSymbol.Substring(lastRunningTrade.TradingSymbol.Count - 2)
                        Dim optnStrgInstrmnt As NFOStrategyInstrument = Await GetStrategyInstrumentFromTradingSymbol(lastRunningTrade.TradingSymbol).ConfigureAwait(False)
                        If optnStrgInstrmnt IsNot Nothing Then
                            ''Target Exit
                            Dim targetReached As Boolean = False
                            If lastRunningTrade.TypeOfEntry = EntryType.Fresh Then
                                If optionType = "CE" AndAlso currentTick.LastPrice >= lastRunningTrade.SpotPrice + lastRunningTrade.SpotATR Then
                                    Dim pl As Decimal = GetFreshTradePL(lastRunningTrade, optnStrgInstrmnt)
                                    If pl > 0 Then
                                        targetReached = True
                                    End If
                                ElseIf optionType = "PE" AndAlso currentTick.LastPrice <= lastRunningTrade.SpotPrice - lastRunningTrade.SpotATR Then
                                    Dim pl As Decimal = GetFreshTradePL(lastRunningTrade, optnStrgInstrmnt)
                                    If pl > 0 Then
                                        targetReached = True
                                    End If
                                End If
                            Else
                                Dim pl As Decimal = GetLossMakeupTradePL(lastRunningTrade, optnStrgInstrmnt)
                                If pl >= lastRunningTrade.PotentialTarget Then
                                    targetReached = True
                                End If
                            End If
                            If targetReached Then
                                Dim dummyTrade As Trade = New Trade With {
                                        .Quantity = lastRunningTrade.Quantity,
                                        .TypeOfExit = ExitType.Target
                                    }

                                Await optnStrgInstrmnt.MonitorAsync(ExecuteCommands.CancelRegularOrder, dummyTrade).ConfigureAwait(False)
                            Else
                                ''Reverse Check
                                Dim reverseExit As Boolean = False
                                Dim trend As Color = _pivotTrendPayload.LastOrDefault.Value
                                If lastRunningTrade.Direction = TradeDirection.Buy AndAlso trend = Color.Red Then
                                    reverseExit = True
                                ElseIf lastRunningTrade.Direction = TradeDirection.Sell AndAlso trend = Color.Green Then
                                    reverseExit = True
                                End If
                                If reverseExit Then
                                    Dim dummyTrade As Trade = New Trade With {
                                        .Quantity = lastRunningTrade.Quantity,
                                        .TypeOfExit = ExitType.Reverse
                                    }

                                    Await optnStrgInstrmnt.MonitorAsync(ExecuteCommands.CancelRegularOrder, dummyTrade).ConfigureAwait(False)
                                Else
                                    ''Zero premium
                                    Dim zeroPremiumReached As Boolean = False
                                    If optnStrgInstrmnt.TradableInstrument.LastTick IsNot Nothing AndAlso
                                        optnStrgInstrmnt.TradableInstrument.LastTick.LastPrice <= optnStrgInstrmnt.TradableInstrument.TickSize Then
                                        zeroPremiumReached = True
                                    End If
                                    If zeroPremiumReached Then
                                        Dim dummyTrade As Trade = New Trade With {
                                            .Quantity = lastRunningTrade.Quantity,
                                            .TypeOfExit = ExitType.ZeroPremium
                                        }

                                        Await optnStrgInstrmnt.MonitorAsync(ExecuteCommands.CancelRegularOrder, dummyTrade).ConfigureAwait(False)
                                    Else
                                        ''Contract Rollover
                                        Dim expiryDate As Date = optnStrgInstrmnt.TradableInstrument.Expiry.Value.AddDays(-2)
                                        expiryDate = New Date(expiryDate.Year, expiryDate.Month, expiryDate.Day, 15, 29, 0)
                                        If Now >= expiryDate Then
                                            Dim dummyTrade As Trade = New Trade With {
                                                .Quantity = lastRunningTrade.Quantity,
                                                .TypeOfExit = ExitType.ContractRollover
                                            }

                                            Await optnStrgInstrmnt.MonitorAsync(ExecuteCommands.CancelRegularOrder, dummyTrade).ConfigureAwait(False)
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                    'Exit Order block end
                    _cts.Token.ThrowIfCancellationRequested()

                    Await Task.Delay(1000, _cts.Token).ConfigureAwait(False)
                End While
            End If
        Catch ex As Exception
            'To log exceptions getting created from this function as the bubble up of the exception
            'will anyways happen to Strategy.MonitorAsync but it will not be shown until all tasks exit
            logger.Error(String.Format("Strategy Instrument:{0} stopped, error:{1}", Me.ToString, ex.ToString))
            Throw ex
        Finally
            _strategyInstrumentRunning = False
        End Try
    End Function

    Public Overrides Async Function MonitorAsync(ByVal command As ExecuteCommands, ByVal data As Object) As Task
        Try
            If command = ExecuteCommands.PlaceRegularMarketCNCOrder AndAlso data IsNot Nothing Then
                _executeCommandData = data
                _executeCommand = ExecuteCommands.PlaceRegularMarketCNCOrder
                Dim orderResponse = Await ExecuteCommandAsync(ExecuteCommands.PlaceRegularMarketCNCOrder, Nothing).ConfigureAwait(False)
                If orderResponse IsNot Nothing AndAlso orderResponse.Count > 0 Then
                    Dim placeOrderResponse = CType(orderResponse, Concurrent.ConcurrentBag(Of Object)).FirstOrDefault
                    If placeOrderResponse.ContainsKey("data") AndAlso
                        placeOrderResponse("data").ContainsKey("order_id") Then
                        Dim orderID As String = placeOrderResponse("data")("order_id")
                        Me.SignalData.InsertOrder(_executeCommandData, orderID)
                        'TODO: Confirm trade entry and update last trade or cancel trade after 5 seconds
                        While True
                            If Me.OrderDetails IsNot Nothing AndAlso Me.OrderDetails.ContainsKey(orderID) Then
                                Dim order As IBusinessOrder = Me.OrderDetails(orderID)
                                If order IsNot Nothing AndAlso order.ParentOrder IsNot Nothing Then
                                    If order.ParentOrder.Status = IOrder.TypeOfStatus.Complete Then

                                    End If
                                End If
                            End If
                            Await Task.Delay(500).ConfigureAwait(False)
                        End While
                    End If
                End If
            ElseIf command = ExecuteCommands.CancelRegularOrder AndAlso data IsNot Nothing Then
                _executeCommandData = data
                _executeCommand = ExecuteCommands.CancelRegularOrder
                Dim orderResponse = Await ExecuteCommandAsync(ExecuteCommands.PlaceRegularMarketCNCOrder, Nothing).ConfigureAwait(False)
                If orderResponse IsNot Nothing AndAlso orderResponse.Count > 0 Then
                    Dim placeOrderResponse = CType(orderResponse, Concurrent.ConcurrentBag(Of Object)).FirstOrDefault
                    If placeOrderResponse.ContainsKey("data") AndAlso
                        placeOrderResponse("data").ContainsKey("order_id") Then
                        Dim orderID As String = placeOrderResponse("data")("order_id")
                        Dim lastTrade As Trade = Me.SignalData.GetLastTrade()
                        lastTrade.TypeOfExit = _executeCommandData.TypeOfExit
                        lastTrade.ExitOrderID = orderID
                        lastTrade.ExitTime = Now
                        'TODO: Confirm trade entry and update last trade
                        'TODO: Handle zero premium exit exceptional

                    End If
                End If
            End If
        Catch ex As Exception
            logger.Error(String.Format("Strategy Instrument:{0} stopped, error:{1}", Me.ToString, ex.ToString))
            Throw ex
        Finally
            _executeCommandData = Nothing
            _executeCommand = ExecuteCommands.ForceCancelRegularOrder
        End Try
    End Function

    Protected Overrides Async Function IsTriggerReceivedForPlaceOrderAsync(ByVal forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)))
        Dim ret As List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
        Dim currentTick As ITick = Me.TradableInstrument.LastTick
        Dim currentTime As Date = Now()
        Dim parameters As PlaceOrderParameters = Nothing
        If currentTime >= Me.TradableInstrument.ExchangeDetails.ExchangeStartTime AndAlso currentTime <= Me.TradableInstrument.ExchangeDetails.ExchangeEndTime AndAlso
            _executeCommandData IsNot Nothing AndAlso currentTick IsNot Nothing Then
            Dim signalCandle As OHLCPayload = New OHLCPayload(OHLCPayload.PayloadSource.CalculatedTick)
            signalCandle.SnapshotDateTime = currentTick.Timestamp.Value.Date
            signalCandle.OpenPrice.Value = currentTick.LastPrice
            signalCandle.LowPrice.Value = currentTick.LastPrice
            signalCandle.HighPrice.Value = currentTick.LastPrice
            signalCandle.ClosePrice.Value = currentTick.LastPrice
            signalCandle.Volume.Value = currentTick.Volume

            If signalCandle IsNot Nothing Then
                If _executeCommand = ExecuteCommands.PlaceRegularMarketCNCOrder Then
                    parameters = New PlaceOrderParameters(signalCandle) With
                                     {
                                        .EntryDirection = IOrder.TypeOfTransaction.Buy,
                                        .OrderType = IOrder.TypeOfOrder.Market,
                                        .Quantity = _executeCommandData.Quantity
                                     }
                ElseIf _executeCommand = ExecuteCommands.CancelRegularOrder Then
                    parameters = New PlaceOrderParameters(signalCandle) With
                                     {
                                        .EntryDirection = IOrder.TypeOfTransaction.Sell,
                                        .OrderType = IOrder.TypeOfOrder.Market,
                                        .Quantity = _executeCommandData.Quantity
                                     }
                End If
            End If
        End If

        'Below portion have to be done in every place order trigger
        If parameters IsNot Nothing Then
            Try
                If forcePrint Then
                    logger.Debug("PlaceOrder-> ************************************************ {0}", Me.TradableInstrument.TradingSymbol)
                    logger.Debug("PlaceOrder Parameters-> {0},{1}", parameters.ToString, Me.TradableInstrument.TradingSymbol)
                End If
            Catch ex As Exception
                logger.Error(ex)
            End Try

            Dim currentSignalActivities As IEnumerable(Of ActivityDashboard) = Me.ParentStrategy.SignalManager.GetActiveSignalActivities(Me.TradableInstrument.InstrumentIdentifier)
            If currentSignalActivities IsNot Nothing AndAlso currentSignalActivities.Count > 0 Then
                'Dim placedActivities As IEnumerable(Of ActivityDashboard) = currentSignalActivities.Where(Function(x)
                '                                                                                              Return x.EntryActivity.RequestRemarks = parameters.ToString
                '                                                                                          End Function)
                Dim placedActivities As IEnumerable(Of ActivityDashboard) = currentSignalActivities
                If placedActivities IsNot Nothing AndAlso placedActivities.Count > 0 Then
                    Dim lastPlacedActivity As ActivityDashboard = placedActivities.OrderBy(Function(x)
                                                                                               Return x.EntryActivity.RequestTime
                                                                                           End Function).LastOrDefault
                    If lastPlacedActivity.EntryActivity.RequestStatus = ActivityDashboard.SignalStatusType.Discarded AndAlso
                            lastPlacedActivity.EntryActivity.LastException IsNot Nothing AndAlso
                            lastPlacedActivity.EntryActivity.LastException.Message.ToUpper.Contains("TIME") Then
                        Await Task.Delay(Me.ParentStrategy.ParentController.UserInputs.BackToBackOrderCoolOffDelay * 1000, _cts.Token).ConfigureAwait(False)

                        If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                        ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.WaitAndTake, parameters, parameters.ToString))
                    ElseIf lastPlacedActivity.EntryActivity.RequestStatus = ActivityDashboard.SignalStatusType.Handled Then
                        If lastPlacedActivity.SignalDirection = parameters.EntryDirection Then
                            If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                            ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.DonotTake, parameters, parameters.ToString))
                            Try
                                logger.Debug("Will not take this trade as similar previous trade detected. Current Trade->{0}, Last Activity Direction:{1}, Last Activity Status:{2}, Trading Symbol:{3}",
                                             parameters.ToString, lastPlacedActivity.SignalDirection, lastPlacedActivity.EntryActivity.RequestStatus, Me.TradableInstrument.TradingSymbol)
                            Catch ex As Exception
                                logger.Warn(ex.ToString)
                            End Try
                        Else
                            If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                            ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.Take, parameters, parameters.ToString))
                        End If
                    ElseIf lastPlacedActivity.EntryActivity.RequestStatus = ActivityDashboard.SignalStatusType.Activated Then
                        If lastPlacedActivity.SignalDirection = parameters.EntryDirection Then
                            If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                            ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.DonotTake, parameters, parameters.ToString))
                            Try
                                logger.Debug("Will not take this trade as similar previous trade detected. Current Trade->{0}, Last Activity Direction:{1}, Last Activity Status:{2}, Trading Symbol:{3}",
                                             parameters.ToString, lastPlacedActivity.SignalDirection, lastPlacedActivity.EntryActivity.RequestStatus, Me.TradableInstrument.TradingSymbol)
                            Catch ex As Exception
                                logger.Warn(ex.ToString)
                            End Try
                        Else
                            If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                            ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.Take, parameters, parameters.ToString))
                        End If
                    ElseIf lastPlacedActivity.EntryActivity.RequestStatus = ActivityDashboard.SignalStatusType.Rejected Then
                        If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                        ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.DonotTake, parameters, parameters.ToString))
                    Else
                        If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                        ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.Take, parameters, parameters.ToString))
                    End If
                Else
                    If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                    ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.Take, parameters, parameters.ToString))
                End If
            Else
                If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.Take, parameters, parameters.ToString))
            End If
        End If
        Return ret
    End Function

    Protected Overrides Function IsTriggerReceivedForExitOrderAsync(ByVal forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, String)))
        Throw New NotImplementedException
    End Function

#Region "Private Functions"
    Private Async Function CreateStrategyInstrumentAndPopulate(ByVal instrument As IInstrument) As Task(Of NFOStrategyInstrument)
        Dim ret As NFOStrategyInstrument = Nothing
        ret = Await CType(Me.ParentStrategy, NFOStrategy).CreateDependentTradableStrategyInstrumentsAsync(instrument).ConfigureAwait(False)
        Return ret
    End Function

    Private Async Function GetCurrentATMOption(ByVal currentTick As ITick, ByVal direction As IOrder.TypeOfTransaction, ByVal optionType As String) As Task(Of NFOStrategyInstrument)
        Dim ret As NFOStrategyInstrument = Nothing
        If Me.ParentStrategy.TradableStrategyInstruments IsNot Nothing AndAlso Me.ParentStrategy.TradableStrategyInstruments.Count > 0 Then
            Dim myOptionInstruments As IEnumerable(Of IInstrument) = CType(Me.ParentStrategy, NFOStrategy).OptionInstruments.Where(Function(x)
                                                                                                                                       Return x.InstrumentType = IInstrument.TypeOfInstrument.Options AndAlso
                                                                                                                                                x.RawInstrumentName = Me.TradableInstrument.RawInstrumentName
                                                                                                                                   End Function)
            Dim maxExpiry As Date = myOptionInstruments.Max(Function(x)
                                                                Return x.Expiry.Value.Date
                                                            End Function)

            Dim maxExpryInstrmts As IEnumerable(Of IInstrument) = myOptionInstruments.Where(Function(x)
                                                                                                Return x.Expiry.Value.Date = maxExpiry.Date
                                                                                            End Function)
            If maxExpryInstrmts IsNot Nothing AndAlso maxExpryInstrmts.Count > 0 Then
                Dim optnStrks As Dictionary(Of Decimal, IInstrument) = Nothing
                For Each runningInstrument In maxExpryInstrmts
                    If runningInstrument.TradingSymbol.EndsWith(optionType.ToUpper) Then
                        If optnStrks Is Nothing Then optnStrks = New Dictionary(Of Decimal, IInstrument)
                        optnStrks.Add(runningInstrument.Strike, runningInstrument)
                    End If
                Next

                If optnStrks IsNot Nothing AndAlso optnStrks.Count > 0 Then
                    Dim optionInstrument As IInstrument = Nothing
                    If direction = IOrder.TypeOfTransaction.Buy Then
                        For Each runningStrike In optnStrks.OrderBy(Function(x)
                                                                        Return x.Key
                                                                    End Function)
                            If runningStrike.Key >= currentTick.LastPrice Then
                                optionInstrument = runningStrike.Value
                                Exit For
                            End If
                        Next
                    ElseIf direction = IOrder.TypeOfTransaction.Sell Then
                        For Each runningStrike In optnStrks.OrderByDescending(Function(x)
                                                                                  Return x.Key
                                                                              End Function)
                            If runningStrike.Key <= currentTick.LastPrice Then
                                optionInstrument = runningStrike.Value
                                Exit For
                            End If
                        Next
                    End If
                    If optionInstrument IsNot Nothing Then
                        ret = Await GetStrategyInstrumentFromTradingSymbol(optionInstrument.TradingSymbol).ConfigureAwait(False)
                    End If
                End If
            End If
        End If
        Return ret
    End Function

    Private Function GetEntrySignal(ByVal currentTick As ITick) As Tuple(Of Boolean, OHLCPayload, IOrder.TypeOfTransaction)
        Dim ret As Tuple(Of Boolean, OHLCPayload, IOrder.TypeOfTransaction) = Nothing
        If currentTick IsNot Nothing AndAlso Not Me.SignalData.IsActiveSignal() Then
            Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
            Dim trend As Color = _pivotTrendPayload.LastOrDefault.Value
            Dim previousTrend As Color = _pivotTrendPayload(_eodPayload.LastOrDefault.Value.PreviousPayload.SnapshotDateTime)
            Dim lastTrade As Trade = Me.SignalData.GetLastTrade()
            If trend = Color.Green Then
                If previousTrend = Color.Red Then
                    If Now >= userSettings.TradeEntryTime Then
                        ret = New Tuple(Of Boolean, OHLCPayload, IOrder.TypeOfTransaction)(True, _eodPayload.LastOrDefault.Value, IOrder.TypeOfTransaction.Buy)
                    End If
                Else
                    Dim rolloverDay As Date = GetChangeoverDay(trend)
                    If rolloverDay <> Date.MinValue AndAlso (lastTrade Is Nothing OrElse rolloverDay.Date <> lastTrade.SignalDate.Date OrElse lastTrade.CurrentStatus = TradeStatus.Cancel) Then
                        ret = New Tuple(Of Boolean, OHLCPayload, IOrder.TypeOfTransaction)(True, _eodPayload(rolloverDay), IOrder.TypeOfTransaction.Buy)
                    End If
                End If
            ElseIf trend = Color.Red Then
                If previousTrend = Color.Green Then
                    If Now >= userSettings.TradeEntryTime Then
                        ret = New Tuple(Of Boolean, OHLCPayload, IOrder.TypeOfTransaction)(True, _eodPayload.LastOrDefault.Value, IOrder.TypeOfTransaction.Sell)
                    End If
                Else
                    Dim rolloverDay As Date = GetChangeoverDay(trend)
                    If rolloverDay <> Date.MinValue AndAlso (lastTrade Is Nothing OrElse rolloverDay.Date <> lastTrade.SignalDate.Date OrElse lastTrade.CurrentStatus = TradeStatus.Cancel) Then
                        ret = New Tuple(Of Boolean, OHLCPayload, IOrder.TypeOfTransaction)(True, _eodPayload(rolloverDay), IOrder.TypeOfTransaction.Sell)
                    End If
                End If
            End If
            If ret IsNot Nothing AndAlso ret.Item1 Then
                If CType(Me.ParentStrategy, NFOStrategy).TotalActiveInstrumentCount < userSettings.ActiveInstrumentCount Then
                    Dim targetReached As Boolean = True
                    If ret.Item3 = IOrder.TypeOfTransaction.Buy Then
                        Dim highestHigh As Decimal = _eodPayload.Max(Function(x)
                                                                         If x.Key > ret.Item2.SnapshotDateTime AndAlso x.Key <= Now.Date Then
                                                                             Return x.Value.HighPrice.Value
                                                                         Else
                                                                             Return Decimal.MinValue
                                                                         End If
                                                                     End Function)
                        highestHigh = Math.Max(highestHigh, currentTick.High)
                        Dim atr As Decimal = _atrPayload(ret.Item2.SnapshotDateTime)
                        If highestHigh < ret.Item2.ClosePrice.Value + atr Then
                            targetReached = False
                        End If
                    ElseIf ret.Item3 = IOrder.TypeOfTransaction.Sell Then
                        Dim lowestLow As Decimal = _eodPayload.Min(Function(x)
                                                                       If x.Key > ret.Item2.SnapshotDateTime AndAlso x.Key <= Now.Date Then
                                                                           Return x.Value.LowPrice.Value
                                                                       Else
                                                                           Return Decimal.MaxValue
                                                                       End If
                                                                   End Function)
                        lowestLow = Math.Min(lowestLow, currentTick.Low)
                        Dim atr As Decimal = _atrPayload(ret.Item2.SnapshotDateTime)
                        If lowestLow > ret.Item2.ClosePrice.Value - atr Then
                            targetReached = False
                        End If
                    End If
                    If targetReached Then ret = Nothing
                End If
            End If
        End If
        Return ret
    End Function

    Private Function GetChangeoverDay(ByVal currentTrend As Color) As Date
        Dim ret As Date = Date.MinValue
        For Each runningPayload In _eodPayload.OrderByDescending(Function(x)
                                                                     Return x.Key
                                                                 End Function)
            If runningPayload.Value.PreviousPayload IsNot Nothing AndAlso
                runningPayload.Value.PreviousPayload.SnapshotDateTime < Now.Date Then
                Dim trend As Color = _pivotTrendPayload(runningPayload.Value.PreviousPayload.SnapshotDateTime)
                If trend <> currentTrend Then
                    ret = runningPayload.Key
                    Exit For
                End If
            End If
        Next
        Return ret
    End Function

    Private Function GetLossMakeupTradePL(ByVal currentTrade As Trade, ByVal currentStrategyInstrument As NFOStrategyInstrument) As Decimal
        Dim ret As Decimal = 0
        Dim allTrades As List(Of Trade) = Me.SignalData.GetAllTradesByChildTag(currentTrade.ChildTag)
        If allTrades IsNot Nothing AndAlso allTrades.Count > 0 Then
            For Each runningTrade In allTrades
                If runningTrade.TypeOfEntry = EntryType.LossMakeup Then
                    If runningTrade.CurrentStatus = TradeStatus.InProgress Then
                        ret += _APIAdapter.CalculatePLWithBrokerage(currentStrategyInstrument.TradableInstrument, runningTrade.EntryPrice, currentStrategyInstrument.TradableInstrument.LastTick.LastPrice, runningTrade.Quantity - currentStrategyInstrument.TradableInstrument.LotSize)
                    Else
                        ret += _APIAdapter.CalculatePLWithBrokerage(currentStrategyInstrument.TradableInstrument, runningTrade.EntryPrice, runningTrade.ExitPrice, runningTrade.Quantity - currentStrategyInstrument.TradableInstrument.LotSize)
                    End If
                End If
            Next
        End If
        Return ret
    End Function

    Private Function GetFreshTradePL(ByVal currentTrade As Trade, ByVal currentStrategyInstrument As NFOStrategyInstrument) As Decimal
        Dim ret As Decimal = 0
        Dim allTrades As List(Of Trade) = Me.SignalData.GetAllTradesByChildTag(currentTrade.ChildTag)
        If allTrades IsNot Nothing AndAlso allTrades.Count > 0 Then
            For Each runningTrade In allTrades
                If runningTrade.TypeOfEntry = EntryType.Fresh Then
                    If runningTrade.CurrentStatus = TradeStatus.InProgress Then
                        ret += _APIAdapter.CalculatePLWithBrokerage(currentStrategyInstrument.TradableInstrument, runningTrade.EntryPrice, currentStrategyInstrument.TradableInstrument.LastTick.LastPrice, runningTrade.Quantity)
                    Else
                        ret += _APIAdapter.CalculatePLWithBrokerage(currentStrategyInstrument.TradableInstrument, runningTrade.EntryPrice, runningTrade.ExitPrice, runningTrade.Quantity)
                    End If
                End If
            Next
        End If
        Return ret
    End Function

    Private Async Function GetStrategyInstrumentFromTradingSymbol(ByVal tradingSymbol As String) As Task(Of NFOStrategyInstrument)
        Dim ret As NFOStrategyInstrument = Nothing
        Dim instruments = CType(Me.ParentStrategy, NFOStrategy).OptionInstruments.Where(Function(x)
                                                                                            Return x.TradingSymbol = tradingSymbol
                                                                                        End Function)
        If instruments IsNot Nothing AndAlso instruments.Count > 0 Then
            Dim strgyInstruments = Me.ParentStrategy.TradableStrategyInstruments.Where(Function(x)
                                                                                           Return x.TradableInstrument.InstrumentIdentifier = instruments.LastOrDefault.InstrumentIdentifier
                                                                                       End Function)
            If strgyInstruments IsNot Nothing AndAlso strgyInstruments.Count > 0 Then
                ret = strgyInstruments.LastOrDefault
            Else
                ret = Await CreateStrategyInstrumentAndPopulate(instruments.LastOrDefault).ConfigureAwait(False)
            End If
        End If
        Return ret
    End Function
#End Region

#Region "Pre Process"

#Region "Indicator"
    Private Function GetSubPayload(ByVal inputPayload As Dictionary(Of Date, OHLCPayload),
                                   ByVal beforeThisTime As Date,
                                   ByVal numberOfItemsToRetrive As Integer,
                                   ByVal includeTimePassedAsOneOftheItems As Boolean) As List(Of KeyValuePair(Of Date, OHLCPayload))
        Dim ret As List(Of KeyValuePair(Of Date, OHLCPayload)) = Nothing
        If inputPayload IsNot Nothing Then
            'Find the index of the time passed
            Dim firstIndexOfKey As Integer = -1
            Dim loopTerminatedOnCondition As Boolean = False
            For Each item In inputPayload

                firstIndexOfKey += 1
                If item.Key >= beforeThisTime Then
                    loopTerminatedOnCondition = True
                    Exit For
                End If
            Next
            If loopTerminatedOnCondition Then 'Specially useful for only 1 count of item is there
                If Not includeTimePassedAsOneOftheItems Then
                    firstIndexOfKey -= 1
                End If
            End If
            If firstIndexOfKey >= 0 Then
                Dim startIndex As Integer = Math.Max((firstIndexOfKey - numberOfItemsToRetrive) + 1, 0)
                Dim revisedNumberOfItemsToRetrieve As Integer = Math.Min(numberOfItemsToRetrive, (firstIndexOfKey - startIndex) + 1)
                Dim referencePayLoadAsList = inputPayload.ToList
                ret = referencePayLoadAsList.GetRange(startIndex, revisedNumberOfItemsToRetrieve)
            End If
        End If
        Return ret
    End Function

    Private Class Pivot
        Public Property PivotHigh As Decimal
        Public Property PivotHighTime As Date
        Public Property PivotLow As Decimal
        Public Property PivotLowTime As Date
    End Class

    Private Sub CalculatePivotHighLow(ByVal period As Integer, ByVal inputPayload As Dictionary(Of Date, OHLCPayload), ByRef outputPayload As Dictionary(Of Date, Pivot))
        If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
            If inputPayload.Count <= period * 2 + 1 Then
                Throw New ApplicationException("Can not calculate pivot high low")
            End If
            For Each runningPayload In inputPayload.Keys
                Dim pivotData As Pivot = Nothing

                Dim previousNInputPayload As List(Of KeyValuePair(Of Date, OHLCPayload)) = GetSubPayload(inputPayload, runningPayload, period, True)
                If previousNInputPayload IsNot Nothing AndAlso previousNInputPayload.Count = period Then
                    Dim highestHigh As Decimal = previousNInputPayload.Max(Function(x)
                                                                               Return x.Value.HighPrice.Value
                                                                           End Function)
                    Dim lowestLow As Decimal = previousNInputPayload.Min(Function(x)
                                                                             Return x.Value.LowPrice.Value
                                                                         End Function)

                    Dim lastCandleTime As Date = previousNInputPayload.Min(Function(x)
                                                                               Return x.Key
                                                                           End Function)

                    Dim pivotCandle As OHLCPayload = inputPayload(lastCandleTime).PreviousPayload
                    If pivotCandle IsNot Nothing Then
                        Dim prePreviousNInputPayload As List(Of KeyValuePair(Of Date, OHLCPayload)) = GetSubPayload(inputPayload, pivotCandle.SnapshotDateTime, period, False)
                        If prePreviousNInputPayload IsNot Nothing AndAlso prePreviousNInputPayload.Count = period Then
                            Dim preHighestHigh As Decimal = prePreviousNInputPayload.Max(Function(x)
                                                                                             Return x.Value.HighPrice.Value
                                                                                         End Function)
                            Dim preLowestLow As Decimal = prePreviousNInputPayload.Min(Function(x)
                                                                                           Return x.Value.LowPrice.Value
                                                                                       End Function)

                            If pivotCandle.HighPrice.Value > highestHigh AndAlso pivotCandle.HighPrice.Value > preHighestHigh Then
                                If pivotData Is Nothing Then pivotData = New Pivot
                                pivotData.PivotHigh = pivotCandle.HighPrice.Value
                                pivotData.PivotHighTime = pivotCandle.SnapshotDateTime
                            Else
                                If outputPayload(inputPayload(runningPayload).PreviousPayload.SnapshotDateTime) IsNot Nothing Then
                                    If pivotData Is Nothing Then pivotData = New Pivot
                                    pivotData.PivotHigh = outputPayload(inputPayload(runningPayload).PreviousPayload.SnapshotDateTime).PivotHigh
                                    pivotData.PivotHighTime = outputPayload(inputPayload(runningPayload).PreviousPayload.SnapshotDateTime).PivotHighTime
                                End If
                            End If
                            If pivotCandle.LowPrice.Value < lowestLow AndAlso pivotCandle.LowPrice.Value < preLowestLow Then
                                If pivotData Is Nothing Then pivotData = New Pivot
                                pivotData.PivotLow = pivotCandle.LowPrice.Value
                                pivotData.PivotLowTime = pivotCandle.SnapshotDateTime
                            Else
                                If outputPayload(inputPayload(runningPayload).PreviousPayload.SnapshotDateTime) IsNot Nothing Then
                                    If pivotData Is Nothing Then pivotData = New Pivot
                                    pivotData.PivotLow = outputPayload(inputPayload(runningPayload).PreviousPayload.SnapshotDateTime).PivotLow
                                    pivotData.PivotLowTime = outputPayload(inputPayload(runningPayload).PreviousPayload.SnapshotDateTime).PivotLowTime
                                End If
                            End If
                        Else
                            If outputPayload(inputPayload(runningPayload).PreviousPayload.SnapshotDateTime) IsNot Nothing Then
                                pivotData = New Pivot With {
                                        .PivotHigh = outputPayload(inputPayload(runningPayload).PreviousPayload.SnapshotDateTime).PivotHigh,
                                        .PivotHighTime = outputPayload(inputPayload(runningPayload).PreviousPayload.SnapshotDateTime).PivotHighTime,
                                        .PivotLow = outputPayload(inputPayload(runningPayload).PreviousPayload.SnapshotDateTime).PivotLow,
                                        .PivotLowTime = outputPayload(inputPayload(runningPayload).PreviousPayload.SnapshotDateTime).PivotLowTime
                                    }
                            End If
                        End If
                    Else
                        If outputPayload(inputPayload(runningPayload).PreviousPayload.SnapshotDateTime) IsNot Nothing Then
                            pivotData = New Pivot With {
                                    .PivotHigh = outputPayload(inputPayload(runningPayload).PreviousPayload.SnapshotDateTime).PivotHigh,
                                    .PivotHighTime = outputPayload(inputPayload(runningPayload).PreviousPayload.SnapshotDateTime).PivotHighTime,
                                    .PivotLow = outputPayload(inputPayload(runningPayload).PreviousPayload.SnapshotDateTime).PivotLow,
                                    .PivotLowTime = outputPayload(inputPayload(runningPayload).PreviousPayload.SnapshotDateTime).PivotLowTime
                                }
                        End If
                    End If
                End If

                If outputPayload Is Nothing Then outputPayload = New Dictionary(Of Date, Pivot)
                outputPayload.Add(runningPayload, pivotData)
            Next
        End If
    End Sub

    Private Sub CalculatePivotHighLowTrend(ByVal period As Integer, ByVal trendPeriod As Integer, ByVal inputPayload As Dictionary(Of Date, OHLCPayload), ByRef outputHighPayload As Dictionary(Of Date, Decimal), ByRef outputLowPayload As Dictionary(Of Date, Decimal), ByRef outputTrendPayload As Dictionary(Of Date, Color))
        Dim pivotPayload As Dictionary(Of Date, Pivot) = Nothing
        CalculatePivotHighLow(period, inputPayload, pivotPayload)

        Dim trend As Color = Color.White
        For Each runningPayload In inputPayload.Keys
            Dim highTrend As Decimal = 0
            Dim lowTrend As Decimal = 0


            Dim lastPivotHighTime As Date = Date.MinValue
            Dim lastPivotLowTime As Date = Date.MinValue
            Dim highCount As Integer = 0
            Dim lowCount As Integer = 0
            Dim highSum As Decimal = 0
            Dim lowSum As Decimal = 0
            Dim previousPayloads As IEnumerable(Of KeyValuePair(Of Date, OHLCPayload)) = inputPayload.Where(Function(x)
                                                                                                                Return x.Key <= runningPayload
                                                                                                            End Function)
            If previousPayloads IsNot Nothing AndAlso previousPayloads.Count > 0 Then
                For Each innerPayload In previousPayloads.OrderByDescending(Function(x)
                                                                                Return x.Key
                                                                            End Function)
                    If pivotPayload.ContainsKey(innerPayload.Key) AndAlso pivotPayload(innerPayload.Key) IsNot Nothing Then
                        If highCount < trendPeriod AndAlso pivotPayload(innerPayload.Key).PivotHighTime <> Date.MinValue AndAlso pivotPayload(innerPayload.Key).PivotHighTime <> lastPivotHighTime Then
                            lastPivotHighTime = pivotPayload(innerPayload.Key).PivotHighTime
                            highCount += 1
                            highSum += (inputPayload(runningPayload).ClosePrice.Value - pivotPayload(innerPayload.Key).PivotHigh) / pivotPayload(innerPayload.Key).PivotHigh
                        End If
                        If lowCount < trendPeriod AndAlso pivotPayload(innerPayload.Key).PivotLowTime <> Date.MinValue AndAlso pivotPayload(innerPayload.Key).PivotLowTime <> lastPivotLowTime Then
                            lastPivotLowTime = pivotPayload(innerPayload.Key).PivotLowTime
                            lowCount += 1
                            lowSum += (inputPayload(runningPayload).ClosePrice.Value - pivotPayload(innerPayload.Key).PivotLow) / pivotPayload(innerPayload.Key).PivotLow
                        End If
                    End If
                    If highCount >= trendPeriod AndAlso lowCount >= trendPeriod Then
                        Exit For
                    End If
                Next
            End If
            highTrend = highSum / trendPeriod
            lowTrend = lowSum / trendPeriod

            If outputHighPayload Is Nothing Then outputHighPayload = New Dictionary(Of Date, Decimal)
            outputHighPayload.Add(runningPayload, highTrend)
            If outputLowPayload Is Nothing Then outputLowPayload = New Dictionary(Of Date, Decimal)
            outputLowPayload.Add(runningPayload, lowTrend)
            If highTrend > 0 AndAlso lowTrend > 0 Then
                trend = Color.Green
            ElseIf highTrend < 0 AndAlso lowTrend < 0 Then
                trend = Color.Red
            End If
            If outputTrendPayload Is Nothing Then outputTrendPayload = New Dictionary(Of Date, Color)
            outputTrendPayload.Add(runningPayload, trend)
        Next
    End Sub

    Private Sub CalculateATR(ByVal ATRPeriod As Integer, ByVal inputPayload As Dictionary(Of Date, OHLCPayload), ByRef outputPayload As Dictionary(Of Date, Decimal))
        'Using WILDER Formula
        If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
            If inputPayload.Count < 100 Then
                Throw New ApplicationException("Can't Calculate ATR")
            End If
            Dim firstPayload As Boolean = True
            Dim highLow As Double = Nothing
            Dim highClose As Double = Nothing
            Dim lowClose As Double = Nothing
            Dim TR As Double = Nothing
            Dim SumTR As Double = 0.00
            Dim AvgTR As Double = 0.00
            Dim counter As Integer = 0
            outputPayload = New Dictionary(Of Date, Decimal)
            For Each runningInputPayload In inputPayload
                counter += 1
                highLow = runningInputPayload.Value.HighPrice.Value - runningInputPayload.Value.LowPrice.Value
                If firstPayload = True Then
                    TR = highLow
                    firstPayload = False
                Else
                    highClose = Math.Abs(runningInputPayload.Value.HighPrice.Value - runningInputPayload.Value.PreviousPayload.ClosePrice.Value)
                    lowClose = Math.Abs(runningInputPayload.Value.LowPrice.Value - runningInputPayload.Value.PreviousPayload.ClosePrice.Value)
                    TR = Math.Max(highLow, Math.Max(highClose, lowClose))
                End If
                SumTR = SumTR + TR
                If counter = ATRPeriod Then
                    AvgTR = SumTR / ATRPeriod
                    outputPayload.Add(runningInputPayload.Value.SnapshotDateTime, AvgTR)
                ElseIf counter > ATRPeriod Then
                    AvgTR = (outputPayload(runningInputPayload.Value.PreviousPayload.SnapshotDateTime) * (ATRPeriod - 1) + TR) / ATRPeriod
                    outputPayload.Add(runningInputPayload.Value.SnapshotDateTime, AvgTR)
                Else
                    AvgTR = SumTR / counter
                    outputPayload.Add(runningInputPayload.Value.SnapshotDateTime, AvgTR)
                End If
            Next
        End If
    End Sub
#End Region

#Region "EOD Data Fetcher"
    Private Async Function GetEODHistoricalDataAsync(ByVal instrument As IInstrument, ByVal fromDate As Date, ByVal toDate As Date) As Task(Of Dictionary(Of Date, OHLCPayload))
        Dim ret As Dictionary(Of Date, OHLCPayload) = Nothing
        Dim historicalCandlesJSONDict As Dictionary(Of String, Object) = Nothing
        _cts.Token.ThrowIfCancellationRequested()
        Dim aliceEODHistoricalURL As String = "https://ant.aliceblueonline.com/api/v1/charts?exchange={0}&token={1}&candletype=3&starttime={2}&endtime={3}&type=historical"
        Dim historicalDataURL As String = Nothing
        If instrument.Segment.ToUpper = "INDICES" Then
            historicalDataURL = String.Format(aliceEODHistoricalURL.Replace("token", "name"), String.Format("{0}_{1}", instrument.RawExchange, instrument.Segment), instrument.TradingSymbol, DateTimeToUnix(fromDate), DateTimeToUnix(toDate))
        Else
            historicalDataURL = String.Format(aliceEODHistoricalURL, instrument.RawExchange, instrument.InstrumentIdentifier, DateTimeToUnix(fromDate), DateTimeToUnix(toDate))
        End If

        Dim proxyToBeUsed As HttpProxy = Nothing
        Using browser As New HttpBrowser(proxyToBeUsed, Net.DecompressionMethods.GZip, New TimeSpan(0, 1, 0), _cts)
            'AddHandler browser.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete
            'AddHandler browser.Heartbeat, AddressOf OnHeartbeat
            'AddHandler browser.WaitingFor, AddressOf OnWaitingFor
            'AddHandler browser.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
            browser.KeepAlive = True
            Dim headers As Dictionary(Of String, String) = New Dictionary(Of String, String)
            headers.Add("X-Authorization-Token", Me.ParentStrategy.ParentController.APIConnection.ENCToken)

            Dim l As Tuple(Of Uri, Object) = Await browser.NonPOSTRequestAsync(historicalDataURL, HttpMethod.Get, Nothing, False, headers, True, "application/json").ConfigureAwait(False)
            If l Is Nothing OrElse l.Item2 Is Nothing Then
                Throw New ApplicationException(String.Format("No response while getting eod historical data for: {0}", historicalDataURL))
            End If
            If l IsNot Nothing AndAlso l.Item2 IsNot Nothing Then
                historicalCandlesJSONDict = l.Item2
            End If

            'RemoveHandler browser.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete
            'RemoveHandler browser.Heartbeat, AddressOf OnHeartbeat
            'RemoveHandler browser.WaitingFor, AddressOf OnWaitingFor
            'RemoveHandler browser.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
        End Using
        If historicalCandlesJSONDict.ContainsKey("data") Then
            Dim historicalCandles As ArrayList = historicalCandlesJSONDict("data")
            Dim previousPayload As OHLCPayload = Nothing
            For Each historicalCandle In historicalCandles
                _cts.Token.ThrowIfCancellationRequested()
                Dim runningSnapshotTime As Date = UnixToDateTime(historicalCandle(0)).Date

                Dim runningPayload As OHLCPayload = New OHLCPayload(OHLCPayload.PayloadSource.Historical)
                With runningPayload
                    .SnapshotDateTime = runningSnapshotTime
                    .TradingSymbol = instrument.TradingSymbol
                    .OpenPrice.Value = historicalCandle(1) / instrument.PriceDivisor
                    .HighPrice.Value = historicalCandle(2) / instrument.PriceDivisor
                    .LowPrice.Value = historicalCandle(3) / instrument.PriceDivisor
                    .ClosePrice.Value = historicalCandle(4) / instrument.PriceDivisor
                    .Volume.Value = historicalCandle(5)
                    .PreviousPayload = previousPayload
                End With
                previousPayload = runningPayload

                If ret Is Nothing Then ret = New Dictionary(Of Date, OHLCPayload)
                If Not ret.ContainsKey(runningSnapshotTime) Then ret.Add(runningSnapshotTime, runningPayload)
            Next
        End If
        Return ret
    End Function
#End Region

    Private Async Function CompletePreProcessing() As Task(Of Boolean)
        Dim ret As Boolean = False
        Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
        _eodPayload = Await GetEODHistoricalDataAsync(Me.TradableInstrument, Now.Date.AddYears(-1), Now.Date).ConfigureAwait(False)
        If _eodPayload IsNot Nothing AndAlso _eodPayload.Count > 0 Then
            CalculatePivotHighLowTrend(userSettings.PivotPeriod, userSettings.PivotTrendPeriod, _eodPayload, Nothing, Nothing, _pivotTrendPayload)
            CalculateATR(userSettings.ATRPeriod, _eodPayload, _atrPayload)

            If Me.SignalData.AllTrades IsNot Nothing AndAlso Me.SignalData.AllTrades.Count > 0 Then
                For Each runningTrade In Me.SignalData.AllTrades
                    If runningTrade.CurrentStatus = TradeStatus.InProgress Then
                        Await GetStrategyInstrumentFromTradingSymbol(runningTrade.TradingSymbol).ConfigureAwait(False)
                    End If
                Next
            End If

            ret = True
        End If
        Return ret
    End Function
#End Region

#Region "Not Required For This Strategy"
    Protected Overrides Function IsTriggerReceivedForModifyStoplossOrderAsync(ByVal forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)))
        Throw New NotImplementedException
    End Function

    Protected Overrides Function IsTriggerReceivedForModifyTargetOrderAsync(forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)))
        Throw New NotImplementedException
    End Function

    Protected Overrides Function IsTriggerReceivedForPlaceOrderAsync(forcePrint As Boolean, data As Object) As Task(Of List(Of Tuple(Of ExecuteCommandAction, StrategyInstrument, PlaceOrderParameters, String)))
        Throw New NotImplementedException()
    End Function

    Protected Overrides Function IsTriggerReceivedForExitOrderAsync(forcePrint As Boolean, data As Object) As Task(Of List(Of Tuple(Of ExecuteCommandAction, StrategyInstrument, IOrder, String)))
        Throw New NotImplementedException()
    End Function

    Protected Overrides Function ForceExitSpecificTradeAsync(order As IOrder, ByVal reason As String) As Task
        Throw New NotImplementedException()
    End Function
#End Region

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
                If _APIAdapter IsNot Nothing Then
                    RemoveHandler _APIAdapter.Heartbeat, AddressOf OnHeartbeat
                    RemoveHandler _APIAdapter.WaitingFor, AddressOf OnWaitingFor
                    RemoveHandler _APIAdapter.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
                    RemoveHandler _APIAdapter.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete
                End If
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' TODO: set large fields to null.
        End If
        disposedValue = True
    End Sub

    ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        ' TODO: uncomment the following line if Finalize() is overridden above.
        ' GC.SuppressFinalize(Me)
    End Sub
#End Region
End Class