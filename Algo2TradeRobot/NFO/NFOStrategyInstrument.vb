Imports NLog
Imports System.IO
Imports Utilities.Time
Imports System.Net.Http
Imports System.Threading
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

#Region "Event and Handler"
    Public Event EndOfTheDay()
    Protected Overridable Sub OnEndOfTheDay()
        RaiseEvent EndOfTheDay()
    End Sub
#End Region

    Private ReadOnly _signalDetailsFilename As String = Path.Combine(My.Application.Info.DirectoryPath, String.Format("{0}.SignalDetails.a2t", Me.TradableInstrument.TradingSymbol))
    Private _allSignalDetails As Dictionary(Of Date, SignalDetails) = Nothing
    Public ReadOnly Property AllSignalDetails As Dictionary(Of Date, SignalDetails)
        Get
            Return _allSignalDetails
        End Get
    End Property

    Public ReadOnly Property PreProcessingDone As Boolean
    Public ReadOnly Property TradingDay As DayOfWeek
    Public Property TakeTradeToday As Boolean

    Private _eodPayload As Dictionary(Of Date, OHLCPayload) = Nothing
    Private _validRainbow As RainbowMA = Nothing
    Private _lastTick As ITick = Nothing
    Private _entryDoneForTheDay As Boolean = False
    Private _tempSignal As SignalDetails = Nothing
    Private _eodMessageSend As Boolean = False

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

        Select Case CType(Me.ParentStrategy.UserSettings, NFOUserInputs).InstrumentsData(Me.TradableInstrument.TradingSymbol).TradingDay.ToUpper.Trim
            Case DayOfWeek.Monday.ToString.ToUpper
                Me.TradingDay = DayOfWeek.Monday
            Case DayOfWeek.Tuesday.ToString.ToUpper
                Me.TradingDay = DayOfWeek.Tuesday
            Case DayOfWeek.Wednesday.ToString.ToUpper
                Me.TradingDay = DayOfWeek.Wednesday
            Case DayOfWeek.Thursday.ToString.ToUpper
                Me.TradingDay = DayOfWeek.Thursday
            Case DayOfWeek.Friday.ToString.ToUpper
                Me.TradingDay = DayOfWeek.Friday
            Case Else
                Me.TradingDay = DayOfWeek.Sunday
        End Select

        If File.Exists(_signalDetailsFilename) Then
            CType(Me.ParentStrategy, NFOStrategy).TotalActiveInstrumentCount += 1
        End If
    End Sub

    Public Overrides Function PopulateChartAndIndicatorsAsync(candleCreator As Chart, currentCandle As OHLCPayload) As Task
        Throw New NotImplementedException
    End Function

    Public Overrides Async Function MonitorAsync() As Task
        Try
            Me.TradableInstrument.FetchHistorical = False

            _strategyInstrumentRunning = True
            Dim preProcess As Boolean = Await CompletePreProcessing().ConfigureAwait(False)
            If preProcess Then
                _PreProcessingDone = True

                Dim eligibleToTakeTrade As Boolean = False
                Dim remarks As String = Nothing
                Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
                If Me.TradingDay = DayOfWeek.Sunday Then
                    If _validRainbow IsNot Nothing Then
                        If CType(Me.ParentStrategy, NFOStrategy).TotalActiveInstrumentCount < CType(Me.ParentStrategy.UserSettings, NFOUserInputs).ActiveInstrumentCount Then
                            eligibleToTakeTrade = True
                        Else
                            remarks = "Max Active Instrument count exceed"
                        End If
                    Else
                        remarks = "No valid rainbow is available"
                    End If
                Else
                    _validRainbow = Nothing
                    If Now.DayOfWeek = Me.TradingDay Then
                        If Not (CType(Me.ParentStrategy, NFOStrategy).NSEHolidays IsNot Nothing AndAlso
                            CType(Me.ParentStrategy, NFOStrategy).NSEHolidays.Contains(Now.Date)) Then
                            _TakeTradeToday = True
                        End If
                    Else
                        Dim daysUntilTradingDay As Integer = (CInt(Me.TradingDay) - CInt(Now.DayOfWeek) + 7) Mod 7
                        Dim nextTradingDate As Date = Now.Date.AddDays(daysUntilTradingDay).Date
                        If CType(Me.ParentStrategy, NFOStrategy).NSEHolidays IsNot Nothing AndAlso
                            CType(Me.ParentStrategy, NFOStrategy).NSEHolidays.Contains(nextTradingDate.Date) Then
                            While nextTradingDate.Date >= Now.Date
                                If CType(Me.ParentStrategy, NFOStrategy).NSEHolidays.Contains(nextTradingDate.Date) OrElse
                                    nextTradingDate.DayOfWeek = DayOfWeek.Saturday OrElse nextTradingDate.DayOfWeek = DayOfWeek.Sunday Then
                                    nextTradingDate = nextTradingDate.AddDays(-1)
                                Else
                                    If nextTradingDate.Date = Now.Date Then
                                        _TakeTradeToday = True
                                    End If
                                    Exit While
                                End If
                            End While
                        End If
                    End If
                    eligibleToTakeTrade = True
                End If

                If eligibleToTakeTrade Then
                    GetLastSignalDetails(Now.Date)
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

                        If _tempSignal IsNot Nothing Then
                            If _tempSignal.NoOfSharesToBuy = 0 Then
                                SetSignalDetails(_tempSignal.SnapshotDate, _tempSignal.ClosePrice, _tempSignal.ClosePrice, _tempSignal.DesireValue)
                                _tempSignal = Nothing
                                _entryDoneForTheDay = True

                                Dim frmDtls As New frmSignalDetails(Me, _cts)
                                OnEndOfTheDay()
                            Else
                                Dim lastExecutedTrade As IBusinessOrder = GetLastExecutedOrder()
                                If lastExecutedTrade IsNot Nothing AndAlso lastExecutedTrade.ParentOrder.Status = IOrder.TypeOfStatus.Complete Then
                                    SetSignalDetails(_tempSignal.SnapshotDate, _tempSignal.ClosePrice, lastExecutedTrade.ParentOrder.AveragePrice, _tempSignal.DesireValue)
                                    _tempSignal = Nothing
                                    _entryDoneForTheDay = True
                                End If
                            End If
                        End If

                        'Place Order block start
                        Dim placeOrderTriggers As List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)) = Await IsTriggerReceivedForPlaceOrderAsync(False).ConfigureAwait(False)
                        If placeOrderTriggers IsNot Nothing AndAlso placeOrderTriggers.Count > 0 AndAlso
                            placeOrderTriggers.FirstOrDefault.Item1 = ExecuteCommandAction.Take Then
                            Dim orderResponse = Await ExecuteCommandAsync(ExecuteCommands.PlaceRegularMarketCNCOrder, Nothing).ConfigureAwait(False)
                            If orderResponse IsNot Nothing AndAlso orderResponse.Count > 0 Then
                                Dim placeOrderResponse = CType(orderResponse, Concurrent.ConcurrentBag(Of Object)).FirstOrDefault
                                If placeOrderResponse.ContainsKey("data") AndAlso
                                    placeOrderResponse("data").ContainsKey("order_id") Then
                                    _entryDoneForTheDay = True
                                End If
                            End If
                        End If
                        'Place Order block end
                        _cts.Token.ThrowIfCancellationRequested()

                        If Now > Me.TradableInstrument.ExchangeDetails.ExchangeEndTime.AddMinutes(1) AndAlso Not _eodMessageSend AndAlso Not TakeTradeToday Then
                            _eodMessageSend = True

                            Dim lastSignal As SignalDetails = GetLastSignalDetails(Now.Date)
                            Dim desireValue As Double = userSettings.InitialInvestment
                            If lastSignal IsNot Nothing Then
                                desireValue = lastSignal.DesireValue + userSettings.ExpectedIncreaseEachPeriod
                            End If
                            Dim eodSignal As SignalDetails = New SignalDetails(Me, lastSignal, Me.TradableInstrument.TradingSymbol, Now.Date, Me.TradableInstrument.LastTick.LastPrice, Me.TradableInstrument.LastTick.LastPrice, desireValue)
                            Dim eodMessage As String = Nothing
                            eodMessage = String.Format("EOD Alert: Date={0}, Desire Value={1}, Total Value Before Rebalancing=(No. of Shares Owned Before Rebalancing[{2}]*Close Price[{3}])={4}, So Amount to Invest=(Desire Value[{5}]-Total Value Before Rebalancing[{6}])={7}, So No. of Shares To Buy/Sell={8}",
                                                        eodSignal.SnapshotDate.ToString("dd-MMM-yyyy"),
                                                        eodSignal.DesireValue,
                                                        eodSignal.NoOfSharesOwnedBeforeRebalancing,
                                                        eodSignal.ClosePrice,
                                                        eodSignal.TotalValueBeforeRebalancing,
                                                        eodSignal.DesireValue,
                                                        eodSignal.TotalValueBeforeRebalancing,
                                                        eodSignal.AmountToInvest,
                                                        eodSignal.NoOfSharesToBuy)
                            SendTradeAlertMessageAsync(eodMessage)

                            Dim frmDtls As New frmSignalDetails(Me, _cts)
                            OnEndOfTheDay()
                        End If

                        Await Task.Delay(2000, _cts.Token).ConfigureAwait(False)
                    End While
                Else
                    Dim message As String = String.Format("Will not run this instrument. {0}", remarks)
                    OnHeartbeat(message)
                    SendTradeAlertMessageAsync(message)
                End If
            End If
        Catch ex As Exception
            'To log exceptions getting created from this function as the bubble up of the exception
            'will anyways happen to Strategy.MonitorAsync but it will not be shown until all tasks exit
            Dim message As String = String.Format("Strategy Instrument:{0} stopped, error:{1}", Me.ToString, ex.ToString)
            logger.Error(message)

            If Me.ParentStrategy.ParentController.OrphanException Is Nothing Then
                SendTradeAlertMessageAsync(message)
            End If

            Throw ex
        Finally
            _strategyInstrumentRunning = False
        End Try
    End Function

    Protected Overrides Async Function IsTriggerReceivedForPlaceOrderAsync(ByVal forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)))
        Dim ret As List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
        Dim currentTick As ITick = Me.TradableInstrument.LastTick
        Dim currentTime As Date = Now()

        Dim log As Boolean = False
        If currentTime >= userSettings.TradeEntryTime Then
            If _lastTick Is Nothing Then
                log = True
                _lastTick = currentTick
            End If
        End If

        Dim parameters As PlaceOrderParameters = Nothing
        If currentTime >= Me.TradableInstrument.ExchangeDetails.ExchangeStartTime AndAlso currentTime <= Me.TradableInstrument.ExchangeDetails.ExchangeEndTime Then
            If Not Me.TakeTradeToday AndAlso _validRainbow IsNot Nothing AndAlso _lastTick IsNot Nothing Then
                If _lastTick.LastPrice > _lastTick.Open Then
                    If _eodPayload IsNot Nothing AndAlso _eodPayload.Count > 0 Then
                        If _eodPayload.ContainsKey(Now.Date) Then
                            _eodPayload(Now.Date).ClosePrice.Value = _lastTick.LastPrice
                        Else
                            Dim currentCandle As OHLCPayload = New OHLCPayload(OHLCPayload.PayloadSource.CalculatedTick)
                            currentCandle.TradingSymbol = Me.TradableInstrument.TradingSymbol
                            currentCandle.SnapshotDateTime = _lastTick.Timestamp.Value.Date
                            currentCandle.OpenPrice.Value = _lastTick.LastPrice
                            currentCandle.LowPrice.Value = _lastTick.LastPrice
                            currentCandle.HighPrice.Value = _lastTick.LastPrice
                            currentCandle.ClosePrice.Value = _lastTick.LastPrice
                            currentCandle.Volume.Value = _lastTick.Volume
                            currentCandle.PreviousPayload = _eodPayload.LastOrDefault.Value

                            _eodPayload.Add(currentCandle.SnapshotDateTime, currentCandle)
                        End If
                        Dim rainbowPayload As Dictionary(Of Date, RainbowMA) = Nothing
                        CalculateRainbowMovingAverage(7, _eodPayload, rainbowPayload)
                        Dim rainbow As RainbowMA = rainbowPayload(Now.Date)

                        Dim maxRainbow As Decimal = Math.Max(rainbow.SMA1, Math.Max(rainbow.SMA2, Math.Max(rainbow.SMA3, Math.Max(rainbow.SMA4, Math.Max(rainbow.SMA5, Math.Max(rainbow.SMA6, Math.Max(rainbow.SMA7, Math.Max(rainbow.SMA8, Math.Max(rainbow.SMA9, rainbow.SMA10)))))))))
                        logger.Debug("LTP {0}, Max Rainbow {1}", _lastTick.LastPrice, maxRainbow)
                        If _lastTick.LastPrice > maxRainbow Then
                            CType(Me.ParentStrategy, NFOStrategy).EligibleInstruments.Add(Me)
                        Else
                            If log Then
                                Dim message As String = "Rainbow not satisfied"
                                OnHeartbeat(message)
                            End If
                        End If
                    End If
                Else
                    If log Then
                        Dim message As String = "Candle Color not satisfied"
                        OnHeartbeat(message)
                    End If
                End If
            End If
            If Me.TakeTradeToday Then
                If currentTime >= userSettings.TradeEntryTime AndAlso _lastTick IsNot Nothing AndAlso Not _entryDoneForTheDay Then
                    log = True
                    Dim signalCandle As OHLCPayload = New OHLCPayload(OHLCPayload.PayloadSource.CalculatedTick)
                    signalCandle.SnapshotDateTime = _lastTick.Timestamp.Value.Date
                    signalCandle.OpenPrice.Value = _lastTick.LastPrice
                    signalCandle.LowPrice.Value = _lastTick.LastPrice
                    signalCandle.HighPrice.Value = _lastTick.LastPrice
                    signalCandle.ClosePrice.Value = _lastTick.LastPrice
                    signalCandle.Volume.Value = _lastTick.Volume
                    If signalCandle IsNot Nothing Then
                        Dim lastSignal As SignalDetails = GetLastSignalDetails(signalCandle.SnapshotDateTime)
                        Dim desireValue As Double = userSettings.InitialInvestment
                        If lastSignal IsNot Nothing Then
                            desireValue = lastSignal.DesireValue + userSettings.ExpectedIncreaseEachPeriod
                        End If
                        _tempSignal = New SignalDetails(Me, lastSignal, Me.TradableInstrument.TradingSymbol, signalCandle.SnapshotDateTime, signalCandle.ClosePrice.Value, 0, desireValue)
                        Dim quantity As Decimal = _tempSignal.NoOfSharesToBuy
                        If quantity > 0 Then
                            parameters = New PlaceOrderParameters(signalCandle) With
                                         {
                                            .EntryDirection = IOrder.TypeOfTransaction.Buy,
                                            .OrderType = IOrder.TypeOfOrder.Market,
                                            .Quantity = quantity
                                         }
                        ElseIf quantity < 0 Then
                            parameters = New PlaceOrderParameters(signalCandle) With
                                         {
                                            .EntryDirection = IOrder.TypeOfTransaction.Sell,
                                            .OrderType = IOrder.TypeOfOrder.Market,
                                            .Quantity = Math.Abs(quantity)
                                         }
                        End If
                        If log AndAlso Not forcePrint Then
                            Dim remarks As String = String.Format("Date={0}, Desire Value={1}, Total Value Before Rebalancing=(No. of Shares Owned Before Rebalancing[{2}]*Close Price[{3}])={4}, So Amount to Invest=(Desire Value[{5}]-Total Value Before Rebalancing[{6}])={7}, So No. of Shares To Buy/Sell={8}",
                                                                  _tempSignal.SnapshotDate.ToString("dd-MMM-yyyy"),
                                                                  _tempSignal.DesireValue,
                                                                  _tempSignal.NoOfSharesOwnedBeforeRebalancing,
                                                                  _tempSignal.ClosePrice,
                                                                  _tempSignal.TotalValueBeforeRebalancing,
                                                                  _tempSignal.DesireValue,
                                                                  _tempSignal.TotalValueBeforeRebalancing,
                                                                  _tempSignal.AmountToInvest,
                                                                  _tempSignal.NoOfSharesToBuy)

                            logger.Fatal(remarks)
                            SendTradeAlertMessageAsync(remarks)
                        End If
                    End If
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

#Region "Telegram"
    Private Async Function SendTradeAlertMessageAsync(ByVal message As String) As Task
        Try
            Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
            _cts.Token.ThrowIfCancellationRequested()
            'If message.Contains("&") Then
            '    message = message.Replace("&", "_")
            'End If
            message = String.Format("{0} -> {1}", Me.TradableInstrument.TradingSymbol, message)
            Dim userInputs As NFOUserInputs = Me.ParentStrategy.UserSettings
            If userInputs.TelegramBotAPIKey IsNot Nothing AndAlso Not userInputs.TelegramBotAPIKey.Trim = "" AndAlso
                userInputs.TelegramTradeChatID IsNot Nothing AndAlso Not userInputs.TelegramTradeChatID.Trim = "" Then
                Using tSender As New Utilities.Notification.Telegram(userInputs.TelegramBotAPIKey.Trim, userInputs.TelegramTradeChatID.Trim, _cts)
                    Dim encodedString As String = Utilities.Strings.UrlEncodeString(message)
                    Await tSender.SendMessageGetAsync(encodedString).ConfigureAwait(False)
                End Using
            End If
        Catch ex As Exception
            logger.Warn(ex.ToString)
        End Try
    End Function
#End Region

#Region "Signal Details"
    Public Function GetLastSignalDetails(ByVal snapshotDate As Date) As SignalDetails
        Dim ret As SignalDetails = Nothing
        If File.Exists(_signalDetailsFilename) Then
            _allSignalDetails = Utilities.Strings.DeserializeToCollection(Of Dictionary(Of Date, SignalDetails))(_signalDetailsFilename)
        End If
        If AllSignalDetails IsNot Nothing AndAlso AllSignalDetails.Count > 0 Then
            For Each runningSignal In AllSignalDetails.Values
                runningSignal.ParentStrategyInstrument = Me
                runningSignal.SetDefaultValues()
            Next
            ret = AllSignalDetails.Where(Function(y)
                                             Return y.Key < snapshotDate
                                         End Function).OrderBy(Function(x)
                                                                   Return x.Key
                                                               End Function).LastOrDefault.Value
        End If
        Return ret
    End Function

    Public Function SetSignalDetails(ByVal snapshotDate As Date, ByVal closePrice As Decimal, ByVal entryPrice As Decimal, ByVal desireValue As Double) As Boolean
        Dim ret As Boolean = False
        If _allSignalDetails Is Nothing Then _allSignalDetails = New Dictionary(Of Date, SignalDetails)
        If Not _allSignalDetails.ContainsKey(snapshotDate) Then
            Dim lastSignal As SignalDetails = GetLastSignalDetails(snapshotDate)
            Dim signal As SignalDetails = New SignalDetails(Me, lastSignal, Me.TradableInstrument.TradingSymbol, snapshotDate, closePrice, entryPrice, desireValue)
            _allSignalDetails.Add(signal.SnapshotDate, signal)

            Dim remarks As String = String.Format("Date={0}, {1}, No. of Shares Owned After Rebalancing={2}, Total Invested=(Previous Profit Adjusted Investment[{3}]+Entry Price[{4}]*No. of Shares To Buy[{5}]={6})",
                                                  signal.SnapshotDate.ToString("dd-MMM-yyyy"),
                                                  If(signal.NoOfSharesToBuy = 0, "Trades not taken", "Trades taken"),
                                                  signal.SharesOwnedAfterRebalancing,
                                                  If(signal.PreviousSignal IsNot Nothing, signal.PreviousSignal.TotalInvested, 0),
                                                  signal.EntryPrice,
                                                  signal.NoOfSharesToBuy,
                                                  signal.TotalInvested)
            logger.Fatal(remarks)
            SendTradeAlertMessageAsync(remarks)

            Utilities.Strings.SerializeFromCollection(Of Dictionary(Of Date, SignalDetails))(_signalDetailsFilename, AllSignalDetails)
            ret = True

            Dim frmDtls As New frmSignalDetails(Me, _cts)
            OnEndOfTheDay()
        End If
        Return ret
    End Function

    Private Function GetTotalTaxAndCharges(ByVal buy As Double, ByVal sell As Double, ByVal quantity As Integer) As Decimal
        Dim ret As Decimal = Decimal.MinValue
        If Math.Abs(quantity) > 0 Then
            Dim calculator As AliceBrokerageCalculator = New AliceBrokerageCalculator(Me.ParentStrategy.ParentController, _cts)
            Dim brokerageAttributes As IBrokerageAttributes = calculator.GetDeliveryEquityBrokerage(buy, sell, Math.Abs(quantity))
            ret = CType(brokerageAttributes, AliceBrokerageAttributes).TotalTax
        Else
            ret = 0
        End If
        Return ret
    End Function

    <Serializable>
    Public Class SignalDetails
        Public Sub New(ByVal parentStrategyInstrument As NFOStrategyInstrument,
                       ByVal previousSignal As SignalDetails,
                       ByVal tradingSymbol As String,
                       ByVal snapshotDate As Date,
                       ByVal closePrice As Decimal,
                       ByVal entryPrice As Decimal,
                       ByVal desireValue As Double)
            Me.ParentStrategyInstrument = parentStrategyInstrument
            Me.PreviousSignal = previousSignal
            Me.TradingSymbol = tradingSymbol
            Me.SnapshotDate = snapshotDate
            Me.ClosePrice = closePrice
            Me.EntryPrice = Math.Round(entryPrice, 2)
            Me.DesireValue = Math.Round(desireValue, 2)

            _NoOfSharesOwnedBeforeRebalancing = Long.MinValue
            _TotalInvested = Double.MinValue
            _PeriodicInvestment = Double.MinValue
        End Sub

        <NonSerialized>
        Public ParentStrategyInstrument As NFOStrategyInstrument

        Public ReadOnly Property PreviousSignal As SignalDetails

        Public ReadOnly Property TradingSymbol As String

        Public ReadOnly Property SnapshotDate As Date

        Public ReadOnly Property ClosePrice As Decimal

        Public ReadOnly Property EntryPrice As Decimal

        Public ReadOnly Property DesireValue As Double

        <NonSerialized>
        Private _NoOfSharesOwnedBeforeRebalancing As Long = Long.MinValue
        Public ReadOnly Property NoOfSharesOwnedBeforeRebalancing As Long
            Get
                If _NoOfSharesOwnedBeforeRebalancing = Long.MinValue Then
                    If Me.PreviousSignal IsNot Nothing Then
                        _NoOfSharesOwnedBeforeRebalancing = Me.PreviousSignal.SharesOwnedAfterRebalancing
                    Else
                        _NoOfSharesOwnedBeforeRebalancing = 0
                    End If
                End If

                Return _NoOfSharesOwnedBeforeRebalancing
            End Get
        End Property

        Public ReadOnly Property TotalValueBeforeRebalancing As Double
            Get
                Dim totalTax As Decimal = 0
                If Me.NoOfSharesOwnedBeforeRebalancing <> 0 Then
                    totalTax = ParentStrategyInstrument.GetTotalTaxAndCharges(0, Me.ClosePrice, Me.NoOfSharesOwnedBeforeRebalancing)
                End If
                Return Math.Round(Me.ClosePrice * Me.NoOfSharesOwnedBeforeRebalancing - totalTax, 2)
            End Get
        End Property

        Public ReadOnly Property AmountToInvest As Double
            Get
                Return Math.Round(Me.DesireValue - Me.TotalValueBeforeRebalancing, 2)
            End Get
        End Property

        Public ReadOnly Property NoOfSharesToBuy As Long
            Get
                Return Math.Ceiling(Me.AmountToInvest / Me.ClosePrice)
            End Get
        End Property

        Public ReadOnly Property SharesOwnedAfterRebalancing As Long
            Get
                Return Me.NoOfSharesOwnedBeforeRebalancing + Me.NoOfSharesToBuy
            End Get
        End Property

        <NonSerialized>
        Private _TotalInvested As Double = Double.MinValue
        Public ReadOnly Property TotalInvested As Double
            Get
                If _TotalInvested = Double.MinValue Then
                    Dim totalTax As Decimal = ParentStrategyInstrument.GetTotalTaxAndCharges(Me.EntryPrice, 0, Me.NoOfSharesToBuy)
                    If Me.PreviousSignal IsNot Nothing Then
                        _TotalInvested = Math.Round(Me.PreviousSignal.TotalInvested + Me.NoOfSharesToBuy * Me.EntryPrice + totalTax, 2)
                    Else
                        _TotalInvested = Math.Round(Me.NoOfSharesToBuy * Me.EntryPrice + totalTax, 2)
                    End If
                End If

                Return _TotalInvested
            End Get
        End Property

        <NonSerialized>
        Private _PeriodicInvestment As Double = Double.MinValue
        Public ReadOnly Property PeriodicInvestment As Double
            Get
                If _PeriodicInvestment = Double.MinValue Then
                    If Me.PreviousSignal IsNot Nothing Then
                        _PeriodicInvestment = Math.Round(Me.PreviousSignal.TotalInvested - Me.TotalInvested, 2)
                    Else
                        _PeriodicInvestment = Math.Round(0 - Me.TotalInvested, 2)
                    End If
                End If

                Return _PeriodicInvestment
            End Get
        End Property

        <NonSerialized>
        Private _AccumulatedCorpus As Double = Double.MinValue
        Public ReadOnly Property AccumulatedCorpus As Double
            Get
                If _AccumulatedCorpus = Double.MinValue Then
                    If Me.PreviousSignal IsNot Nothing Then
                        If Me.PreviousSignal.AccumulatedCorpus + Me.PeriodicInvestment < 0 Then
                            _AccumulatedCorpus = 0
                        Else
                            _AccumulatedCorpus = Math.Round(Me.PreviousSignal.AccumulatedCorpus + Me.PeriodicInvestment, 2)
                        End If
                    Else
                        _AccumulatedCorpus = 0
                    End If
                End If

                Return _AccumulatedCorpus
            End Get
        End Property

        <NonSerialized>
        Private _NetGoing As Double = Double.MinValue
        Public ReadOnly Property NetGoing As Double
            Get
                If _NetGoing = Double.MinValue Then
                    If Me.PreviousSignal IsNot Nothing Then
                        If Me.AccumulatedCorpus > 0 Then
                            _NetGoing = 0
                        Else
                            If Me.PeriodicInvestment < 0 Then
                                _NetGoing = Math.Round(Math.Abs(Me.PeriodicInvestment + Me.PreviousSignal.AccumulatedCorpus), 2)
                            Else
                                _NetGoing = 0
                            End If
                        End If
                    Else
                        _NetGoing = Math.Round(Math.Abs(Me.PeriodicInvestment), 2)
                    End If
                End If

                Return _NetGoing
            End Get
        End Property

        <NonSerialized>
        Private _TotalNetGoing As Double = Double.MinValue
        Public ReadOnly Property TotalNetGoing As Double
            Get
                If _TotalNetGoing = Double.MinValue Then
                    If Me.PreviousSignal IsNot Nothing Then
                        _TotalNetGoing = Me.NetGoing + Me.PreviousSignal.TotalNetGoing
                    Else
                        _TotalNetGoing = Me.NetGoing
                    End If
                End If

                Return _TotalNetGoing
            End Get
        End Property

        <NonSerialized>
        Private _CurrentValue As Double = Double.MinValue
        Public ReadOnly Property CurrentValue As Double
            Get
                If _CurrentValue = Double.MinValue Then
                    If Me.PreviousSignal IsNot Nothing Then
                        Dim totalTax As Decimal = 0
                        If Me.SharesOwnedAfterRebalancing <> 0 Then
                            totalTax = ParentStrategyInstrument.GetTotalTaxAndCharges(0, Me.ClosePrice, Me.SharesOwnedAfterRebalancing)
                        End If
                        _CurrentValue = Math.Round(Me.ClosePrice * Me.SharesOwnedAfterRebalancing - totalTax, 2)
                    Else
                        _CurrentValue = Math.Round(Math.Abs(Me.PeriodicInvestment), 2)
                    End If
                End If

                Return _CurrentValue
            End Get
        End Property

        Public ReadOnly Property AbsoluteReturns As Double
            Get
                Return Math.Round(((Me.AccumulatedCorpus + Me.CurrentValue) / Me.TotalNetGoing - 1) * 100, 2)
            End Get
        End Property

        <NonSerialized>
        Private _ContinuousInvestmentNeeded As Double = Double.MinValue
        Public ReadOnly Property ContinuousInvestmentNeeded As Double
            Get
                If _ContinuousInvestmentNeeded = Double.MinValue Then
                    If Me.PreviousSignal IsNot Nothing Then
                        If Me.PeriodicInvestment < 0 Then
                            If Me.PreviousSignal.ContinuousInvestmentNeeded < 0 Then
                                _ContinuousInvestmentNeeded = Math.Round(Me.PeriodicInvestment + Me.PreviousSignal.ContinuousInvestmentNeeded, 2)
                            Else
                                _ContinuousInvestmentNeeded = Me.PeriodicInvestment
                            End If
                        Else
                            _ContinuousInvestmentNeeded = 0
                        End If
                    Else
                        _ContinuousInvestmentNeeded = 0
                    End If
                End If

                Return _ContinuousInvestmentNeeded
            End Get
        End Property

        Public Sub SetDefaultValues()
            _NoOfSharesOwnedBeforeRebalancing = Long.MinValue
            _TotalInvested = Double.MinValue
            _PeriodicInvestment = Double.MinValue
            _AccumulatedCorpus = Double.MinValue
            _NetGoing = Double.MinValue
            _TotalNetGoing = Double.MinValue
            _CurrentValue = Double.MinValue
            _ContinuousInvestmentNeeded = Double.MinValue
        End Sub
    End Class
#End Region

#Region "Pre Process"

#Region "Indicator"
    Private Function GetSubPayload(ByVal inputPayload As Dictionary(Of Date, Decimal),
                                   ByVal beforeThisTime As Date,
                                   ByVal numberOfItemsToRetrive As Integer,
                                   ByVal includeTimePassedAsOneOftheItems As Boolean) As List(Of KeyValuePair(Of Date, Decimal))
        Dim ret As List(Of KeyValuePair(Of Date, Decimal)) = Nothing
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

    Private Sub CalculateSMA(ByVal smaPeriod As Integer, ByVal inputPayload As Dictionary(Of Date, Decimal), ByRef outputPayload As Dictionary(Of Date, Decimal))
        If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
            Dim finalPriceToBeAdded As Decimal = 0
            For Each runningInputPayload In inputPayload

                'If it is less than IndicatorPeriod, we will need to take SMA of all previous prices, hence the call to GetSubPayload
                Dim previousNInputFieldPayload As List(Of KeyValuePair(Of Date, Decimal)) = GetSubPayload(inputPayload, runningInputPayload.Key, smaPeriod - 1, False)
                If previousNInputFieldPayload Is Nothing Then
                    finalPriceToBeAdded += runningInputPayload.Value
                ElseIf previousNInputFieldPayload IsNot Nothing AndAlso previousNInputFieldPayload.Count <= smaPeriod - 1 Then 'Because the first field is handled outside
                    Dim totalOfAllPrices As Decimal = 0
                    totalOfAllPrices = runningInputPayload.Value
                    totalOfAllPrices += previousNInputFieldPayload.Sum(Function(s) s.Value)
                    finalPriceToBeAdded = totalOfAllPrices / (previousNInputFieldPayload.Count + 1)
                Else
                    Dim totalOfAllPrices As Decimal = 0
                    totalOfAllPrices = runningInputPayload.Value
                    totalOfAllPrices += previousNInputFieldPayload.Sum(Function(s) s.Value)
                    finalPriceToBeAdded = Math.Round((totalOfAllPrices / (previousNInputFieldPayload.Count + 1)), 2)
                End If
                If outputPayload Is Nothing Then outputPayload = New Dictionary(Of Date, Decimal)
                outputPayload.Add(runningInputPayload.Key, finalPriceToBeAdded)
            Next
        End If
    End Sub

    Private Class RainbowMA
        Public Property SMA1 As Decimal
        Public Property SMA2 As Decimal
        Public Property SMA3 As Decimal
        Public Property SMA4 As Decimal
        Public Property SMA5 As Decimal
        Public Property SMA6 As Decimal
        Public Property SMA7 As Decimal
        Public Property SMA8 As Decimal
        Public Property SMA9 As Decimal
        Public Property SMA10 As Decimal
    End Class

    Private Sub CalculateRainbowMovingAverage(ByVal period As Integer, ByVal inputPayload As Dictionary(Of Date, OHLCPayload), ByRef outputPayload As Dictionary(Of Date, RainbowMA))
        If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
            If inputPayload.Count < period + 1 Then
                Throw New ApplicationException("Can't Calculate Rainbow Moving Average")
            End If

            Dim conInputPayload As Dictionary(Of Date, Decimal) = Nothing
            For Each runningPayload In inputPayload
                If conInputPayload Is Nothing Then conInputPayload = New Dictionary(Of Date, Decimal)
                conInputPayload.Add(runningPayload.Key, runningPayload.Value.ClosePrice.Value)
            Next

            Dim outputSMAPayload1 As Dictionary(Of Date, Decimal) = Nothing
            CalculateSMA(period, conInputPayload, outputSMAPayload1)
            Dim outputSMAPayload2 As Dictionary(Of Date, Decimal) = Nothing
            CalculateSMA(period, outputSMAPayload1, outputSMAPayload2)
            Dim outputSMAPayload3 As Dictionary(Of Date, Decimal) = Nothing
            CalculateSMA(period, outputSMAPayload2, outputSMAPayload3)
            Dim outputSMAPayload4 As Dictionary(Of Date, Decimal) = Nothing
            CalculateSMA(period, outputSMAPayload3, outputSMAPayload4)
            Dim outputSMAPayload5 As Dictionary(Of Date, Decimal) = Nothing
            CalculateSMA(period, outputSMAPayload4, outputSMAPayload5)
            Dim outputSMAPayload6 As Dictionary(Of Date, Decimal) = Nothing
            CalculateSMA(period, outputSMAPayload5, outputSMAPayload6)
            Dim outputSMAPayload7 As Dictionary(Of Date, Decimal) = Nothing
            CalculateSMA(period, outputSMAPayload6, outputSMAPayload7)
            Dim outputSMAPayload8 As Dictionary(Of Date, Decimal) = Nothing
            CalculateSMA(period, outputSMAPayload7, outputSMAPayload8)
            Dim outputSMAPayload9 As Dictionary(Of Date, Decimal) = Nothing
            CalculateSMA(period, outputSMAPayload8, outputSMAPayload9)
            Dim outputSMAPayload10 As Dictionary(Of Date, Decimal) = Nothing
            CalculateSMA(period, outputSMAPayload9, outputSMAPayload10)

            For Each runningPayload In inputPayload.Keys
                Dim rainbow As RainbowMA = New RainbowMA With {
                        .SMA1 = outputSMAPayload1(runningPayload),
                        .SMA2 = outputSMAPayload2(runningPayload),
                        .SMA3 = outputSMAPayload3(runningPayload),
                        .SMA4 = outputSMAPayload4(runningPayload),
                        .SMA5 = outputSMAPayload5(runningPayload),
                        .SMA6 = outputSMAPayload6(runningPayload),
                        .SMA7 = outputSMAPayload7(runningPayload),
                        .SMA8 = outputSMAPayload8(runningPayload),
                        .SMA9 = outputSMAPayload9(runningPayload),
                        .SMA10 = outputSMAPayload10(runningPayload)
                    }

                If outputPayload Is Nothing Then outputPayload = New Dictionary(Of Date, RainbowMA)
                outputPayload.Add(runningPayload, rainbow)
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
            Dim rainbowPayload As Dictionary(Of Date, RainbowMA) = Nothing
            CalculateRainbowMovingAverage(7, _eodPayload, rainbowPayload)

            For Each runningPayload In _eodPayload.OrderByDescending(Function(x)
                                                                         Return x.Key
                                                                     End Function)
                Dim rainbow As RainbowMA = rainbowPayload(runningPayload.Key)
                If runningPayload.Value.ClosePrice.Value > Math.Max(rainbow.SMA1, Math.Max(rainbow.SMA2, Math.Max(rainbow.SMA3, Math.Max(rainbow.SMA4, Math.Max(rainbow.SMA5, Math.Max(rainbow.SMA6, Math.Max(rainbow.SMA7, Math.Max(rainbow.SMA8, Math.Max(rainbow.SMA9, rainbow.SMA10))))))))) Then
                    If runningPayload.Value.CandleColor = Color.Green Then
                        Exit For
                    End If
                ElseIf runningPayload.Value.ClosePrice.Value < Math.Min(rainbow.SMA1, Math.Min(rainbow.SMA2, Math.Min(rainbow.SMA3, Math.Min(rainbow.SMA4, Math.Min(rainbow.SMA5, Math.Min(rainbow.SMA6, Math.Min(rainbow.SMA7, Math.Min(rainbow.SMA8, Math.Min(rainbow.SMA9, rainbow.SMA10))))))))) Then
                    If runningPayload.Value.CandleColor = Color.Red Then
                        _validRainbow = rainbowPayload.LastOrDefault.Value
                        Exit For
                    End If
                End If
            Next

            ret = True
        End If
        Return ret
    End Function
#End Region

#Region "Not Required For This Strategy"
    Public Overrides Function MonitorAsync(ByVal command As ExecuteCommands, ByVal data As Object) As Task
        Throw New NotImplementedException()
    End Function

    Protected Overrides Function IsTriggerReceivedForModifyStoplossOrderAsync(ByVal forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)))
        Throw New NotImplementedException
    End Function

    Protected Overrides Function IsTriggerReceivedForModifyTargetOrderAsync(forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)))
        Throw New NotImplementedException
    End Function

    Protected Overrides Function IsTriggerReceivedForExitOrderAsync(ByVal forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, String)))
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