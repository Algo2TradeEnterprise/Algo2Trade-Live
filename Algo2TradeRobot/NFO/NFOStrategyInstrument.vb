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
    Private _executeCommand As ExecuteCommands = ExecuteCommands.CancelBOOrder
    Private _eodMessageSend As Boolean = False

    Private _eodPayload As Dictionary(Of Date, OHLCPayload) = Nothing
    Private _trendPayload As Dictionary(Of Date, Color) = Nothing
    Private _atrPayload As Dictionary(Of Date, Decimal) = Nothing
    Private _smaPayload As Dictionary(Of Date, Decimal) = Nothing

    Public Sub New(ByVal associatedInstrument As IInstrument,
                   ByVal associatedParentStrategy As Strategy,
                   ByVal isPairInstrumnet As Boolean,
                   ByVal canceller As CancellationTokenSource,
                   ByVal myParentStrategyInstrument As NFOStrategyInstrument)
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
                If Me.SignalData.AllTrades IsNot Nothing AndAlso Me.SignalData.AllTrades.Count > 0 Then
                    For Each runningTrade In Me.SignalData.AllTrades
                        runningTrade.UpdateParentSignal(Me.SignalData)
                    Next
                End If
            Else
                Me.SignalData = New SignalDetails(Me.TradableInstrument.RawInstrumentName)
                'Utilities.Strings.SerializeFromCollection(Of SignalDetails)(Me.SignalData.SignalDetailsFilename, Me.SignalData)
            End If

            If Me.SignalData.IsActiveSignal Then
                CType(Me.ParentStrategy, NFOStrategy).TotalActiveInstrumentCount += 1
                Try
                    logger.Debug("{0} is Active instrument. Total Active Instrument Count:{1}",
                                 Me.TradableInstrument.TradingSymbol,
                                 CType(Me.ParentStrategy, NFOStrategy).TotalActiveInstrumentCount)
                Catch ex As Exception
                    logger.Warn(ex.ToString)
                End Try
            Else
                Dim lastCompleteTrade As Trade = Me.SignalData.GetLastCompleteTrade()
                If lastCompleteTrade IsNot Nothing AndAlso lastCompleteTrade.TypeOfExit <> ExitType.Target Then
                    CType(Me.ParentStrategy, NFOStrategy).TotalActiveInstrumentCount += 1
                    Try
                        logger.Debug("{0} is Active instrument. Total Active Instrument Count:{1}",
                                     Me.TradableInstrument.TradingSymbol,
                                     CType(Me.ParentStrategy, NFOStrategy).TotalActiveInstrumentCount)
                    Catch ex As Exception
                        logger.Warn(ex.ToString)
                    End Try
                End If
            End If
        Else
            Me.SignalData = myParentStrategyInstrument.SignalData
        End If
    End Sub

#Region "Spot Monitor Async"
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
                    If Now >= userSettings.TradeEntryTime AndAlso Not _eodPayload.ContainsKey(Now.Date) AndAlso currentTick IsNot Nothing Then
                        Try
                            logger.Debug("{0}: Adding current day candle and calculating indicator.", Me.TradableInstrument.TradingSymbol)
                        Catch ex As Exception
                            logger.Warn(ex.ToString)
                        End Try

                        Dim currentCandle As OHLCPayload = New OHLCPayload(OHLCPayload.PayloadSource.Tick)
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
                        _trendPayload = Nothing
                        CalculateHKMATrend(userSettings.SMAPeriod, _eodPayload, _trendPayload)
                        _smaPayload = Nothing
                        CalculateSMA(userSettings.SMAPeriod, _eodPayload, _smaPayload)
                    End If

                    _cts.Token.ThrowIfCancellationRequested()
                    'Place Order block start
                    Await PlaceEntryOrderIfRequired(currentTick).ConfigureAwait(False)
                    'Place Order block end
                    _cts.Token.ThrowIfCancellationRequested()
                    'Exit Order block start
                    Await PlaceExitOrderIfRequired().ConfigureAwait(False)
                    'Exit Order block end
                    _cts.Token.ThrowIfCancellationRequested()

                    Await Task.Delay(1000, _cts.Token).ConfigureAwait(False)
                End While
            End If
        Catch ex As Exception
            'To log exceptions getting created from this function as the bubble up of the exception
            'will anyways happen to Strategy.MonitorAsync but it will not be shown until all tasks exit
            OnHeartbeat(String.Format("Strategy Instrument:{0} stopped, error:{1}", Me.ToString, ex.Message))
            logger.Error(String.Format("Strategy Instrument:{0} stopped, error:{1}", Me.ToString, ex.ToString))
            Throw ex
        Finally
            _strategyInstrumentRunning = False
        End Try
    End Function
#End Region

#Region "Place Order"

#Region "Place Contract Rollover Order"
    Private Async Function PlaceContractRolloverEntryOrder(ByVal currentSpotTick As ITick, ByVal lastCompleteTrade As Trade, ByVal userSettings As NFOUserInputs) As Task
        Try
            logger.Debug("{0}: Last complete Order: {1}. Exit Type:{2}",
                                 Me.TradableInstrument.TradingSymbol,
                                 lastCompleteTrade.EntryOrderID,
                                 lastCompleteTrade.TypeOfExit.ToString)
        Catch ex As Exception
            logger.Warn(ex.ToString)
        End Try

        Dim drctn As IOrder.TypeOfTransaction = IOrder.TypeOfTransaction.None
        If lastCompleteTrade.Direction = TradeDirection.Buy Then
            drctn = IOrder.TypeOfTransaction.Buy
        ElseIf lastCompleteTrade.Direction = TradeDirection.Sell Then
            drctn = IOrder.TypeOfTransaction.Sell
        End If
        Dim currentATMOption As NFOStrategyInstrument = Await GetOptionToTrade(currentSpotTick, drctn).ConfigureAwait(False)
        If currentATMOption IsNot Nothing AndAlso currentATMOption.TradableInstrument.LastTick IsNot Nothing Then
            Dim lossToRecover As Decimal = GetOverallSignalPL(lastCompleteTrade, currentATMOption, 0)
            Dim potentialTarget As Decimal = lastCompleteTrade.PotentialTarget
            Dim entryPrice As Decimal = currentATMOption.TradableInstrument.LastTick.LastPrice
            Dim capital As Decimal = entryPrice * lastCompleteTrade.Quantity
            If lossToRecover < 0 Then capital = capital + Math.Abs(lossToRecover)
            potentialTarget = capital * userSettings.TargetPercentage / 100

            Dim dummyTrade As Trade = New Trade(Nothing)
            dummyTrade.UpdateTrade(TradingSymbol:=currentATMOption.TradableInstrument.TradingSymbol,
                                    ChildTag:=lastCompleteTrade.ChildTag,
                                    ParentTag:=lastCompleteTrade.ParentTag,
                                    CurrentStatus:=TradeStatus.Open,
                                    Direction:=lastCompleteTrade.Direction,
                                    LossToRecover:=lastCompleteTrade.LossToRecover,
                                    PotentialTarget:=Math.Max(potentialTarget, 1000),
                                    Quantity:=lastCompleteTrade.Quantity,
                                    EntrySignalDate:=lastCompleteTrade.EntrySignalDate,
                                    SpotATR:=lastCompleteTrade.SpotATR,
                                    SpotPrice:=lastCompleteTrade.SpotPrice,
                                    IterationNumber:=lastCompleteTrade.IterationNumber,
                                    TypeOfEntry:=lastCompleteTrade.TypeOfEntry,
                                    TypeOfEntryDetails:=lastCompleteTrade.TypeOfEntryDetails,
                                    ATRConsumed:=lastCompleteTrade.ATRConsumed,
                                    AttemptedEntryPrice:=entryPrice)

            Await currentATMOption.MonitorAsync(ExecuteCommands.PlaceRegularMarketCNCOrder, dummyTrade).ConfigureAwait(False)
            Dim lastTrade As Trade = Me.SignalData.GetLastTrade()
            If lastTrade IsNot Nothing AndAlso lastTrade.CurrentStatus = TradeStatus.InProgress Then
                SendEntryOrderNotificationAsync(lastTrade, currentATMOption)
            End If
        End If
    End Function
#End Region

#Region "Place Reversal Order"
    Private Async Function PlaceReversalEntryOrder(ByVal currentSpotTick As ITick, ByVal lastCompleteTrade As Trade, ByVal userSettings As NFOUserInputs) As Task
        Try
            logger.Debug("{0}: Last complete Order: {1}. Exit Type:{2}",
                         Me.TradableInstrument.TradingSymbol,
                         lastCompleteTrade.EntryOrderID,
                         lastCompleteTrade.TypeOfExit.ToString)
        Catch ex As Exception
            logger.Warn(ex.ToString)
        End Try

        Dim directionToCheck As TradeDirection = TradeDirection.None
        Dim directionToCheckOrder As IOrder.TypeOfTransaction = IOrder.TypeOfTransaction.None
        If lastCompleteTrade.Direction = TradeDirection.Buy Then
            directionToCheck = TradeDirection.Sell
            directionToCheckOrder = IOrder.TypeOfTransaction.Sell
        ElseIf lastCompleteTrade.Direction = TradeDirection.Sell Then
            directionToCheck = TradeDirection.Buy
            directionToCheckOrder = IOrder.TypeOfTransaction.Buy
        End If

        Dim currentATMOption As NFOStrategyInstrument = Await GetOptionToTrade(currentSpotTick, directionToCheckOrder).ConfigureAwait(False)
        If currentATMOption IsNot Nothing AndAlso currentATMOption.TradableInstrument.LastTick IsNot Nothing Then
            Dim childTag As String = System.Guid.NewGuid.ToString()
            Dim parentTag As String = lastCompleteTrade.ParentTag
            Dim tradeNumber As Integer = lastCompleteTrade.IterationNumber + 1

            Dim lossToRecover As Decimal = GetOverallSignalPL(lastCompleteTrade, currentATMOption, 0)
            Dim potentialTarget As Decimal = Math.Abs(lossToRecover)

            Dim spotPrice As Decimal = currentSpotTick.LastPrice
            Dim spotATR As Decimal = _atrPayload.LastOrDefault.Value

            Dim quantity As Integer = currentATMOption.TradableInstrument.LotSize
            Dim entryPrice As Decimal = currentATMOption.TradableInstrument.LastTick.LastPrice
            Dim targetPrice As Decimal = ConvertFloorCeling(entryPrice + spotATR, currentATMOption.TradableInstrument.TickSize, RoundOfType.Celing)
            For ctr As Integer = 1 To Integer.MaxValue
                Dim pl As Decimal = _APIAdapter.CalculatePLWithBrokerage(currentATMOption.TradableInstrument, entryPrice, targetPrice, ctr * currentATMOption.TradableInstrument.LotSize)
                If pl >= lossToRecover * -1 Then
                    quantity = ctr * currentATMOption.TradableInstrument.LotSize + currentATMOption.TradableInstrument.LotSize
                    Exit For
                End If
            Next

            Dim capital As Decimal = entryPrice * quantity
            If lossToRecover < 0 Then capital = capital + Math.Abs(lossToRecover)
            potentialTarget = capital * userSettings.TargetPercentage / 100

            Dim dummyTrade As Trade = New Trade(Nothing)
            dummyTrade.UpdateTrade(TradingSymbol:=currentATMOption.TradableInstrument.TradingSymbol,
                                    ChildTag:=childTag,
                                    ParentTag:=parentTag,
                                    CurrentStatus:=TradeStatus.Open,
                                    Direction:=directionToCheck,
                                    LossToRecover:=lossToRecover,
                                    PotentialTarget:=Math.Max(potentialTarget, 1000),
                                    Quantity:=quantity,
                                    EntrySignalDate:=lastCompleteTrade.EntrySignalDate,
                                    SpotATR:=spotATR,
                                    SpotPrice:=spotPrice,
                                    IterationNumber:=tradeNumber,
                                    TypeOfEntry:=EntryType.LossMakeup,
                                    TypeOfEntryDetails:=ExitType.Reverse,
                                    ATRConsumed:=0,
                                    AttemptedEntryPrice:=entryPrice)

            Await currentATMOption.MonitorAsync(ExecuteCommands.PlaceRegularMarketCNCOrder, dummyTrade).ConfigureAwait(False)
            Dim lastTrade As Trade = Me.SignalData.GetLastTrade()
            If lastTrade IsNot Nothing AndAlso lastTrade.CurrentStatus = TradeStatus.InProgress Then
                Try
                    If lastTrade.Direction = TradeDirection.Buy Then
                        Dim spotMoved As Decimal = Me.TradableInstrument.LastTick.LastPrice - _eodPayload(lastTrade.EntrySignalDate).ClosePrice.Value
                        If spotMoved > 0 Then lastTrade.UpdateTrade(ATRConsumed:=(spotMoved / lastTrade.SpotATR) * 100)
                    ElseIf lastTrade.Direction = TradeDirection.Sell Then
                        Dim spotMoved As Decimal = _eodPayload(lastTrade.EntrySignalDate).ClosePrice.Value - Me.TradableInstrument.LastTick.LastPrice
                        If spotMoved > 0 Then lastTrade.UpdateTrade(ATRConsumed:=(spotMoved / lastTrade.SpotATR) * 100)
                    End If
                Catch ex As Exception
                    logger.Warn(ex.ToString)
                End Try

                SendEntryOrderNotificationAsync(lastTrade, currentATMOption)
            End If
        End If
    End Function
#End Region

#Region "Place Fresh Trade"
    Private Async Function PlaceFreshEntryOrder(ByVal currentSpotTick As ITick, ByVal signal As Tuple(Of Boolean, OHLCPayload, IOrder.TypeOfTransaction, Decimal), ByVal userSettings As NFOUserInputs) As Task
        If signal IsNot Nothing AndAlso signal.Item1 AndAlso
            CType(Me.ParentStrategy, NFOStrategy).TotalActiveInstrumentCount < userSettings.ActiveInstrumentCount Then
            Dim currentATMOption As NFOStrategyInstrument = Await GetOptionToTrade(currentSpotTick, signal.Item3).ConfigureAwait(False)
            If currentATMOption IsNot Nothing AndAlso currentATMOption.TradableInstrument.LastTick IsNot Nothing Then
                Dim childTag As String = System.Guid.NewGuid.ToString()
                Dim parentTag As String = childTag
                Dim lossToRecover As Decimal = 0
                Dim potentialTarget As Decimal = 0

                Dim spotPrice As Decimal = currentSpotTick.LastPrice
                Dim spotATR As Decimal = _atrPayload.LastOrDefault.Value

                Dim quantity As Integer = currentATMOption.TradableInstrument.LotSize
                Dim entryPrice As Decimal = currentATMOption.TradableInstrument.LastTick.LastPrice
                Dim capital As Decimal = entryPrice * quantity
                If lossToRecover < 0 Then capital = capital + Math.Abs(lossToRecover)
                potentialTarget = capital * userSettings.TargetPercentage / 100

                Dim drctn As TradeDirection = TradeDirection.None
                If signal.Item3 = IOrder.TypeOfTransaction.Buy Then
                    drctn = TradeDirection.Buy
                ElseIf signal.Item3 = IOrder.TypeOfTransaction.Sell Then
                    drctn = TradeDirection.Sell
                End If

                Dim dummyTrade As Trade = New Trade(Nothing)
                dummyTrade.UpdateTrade(TradingSymbol:=currentATMOption.TradableInstrument.TradingSymbol,
                                        ChildTag:=childTag,
                                        ParentTag:=parentTag,
                                        CurrentStatus:=TradeStatus.Open,
                                        Direction:=drctn,
                                        LossToRecover:=lossToRecover,
                                        PotentialTarget:=Math.Max(potentialTarget, 1000),
                                        Quantity:=quantity,
                                        EntrySignalDate:=signal.Item2.SnapshotDateTime,
                                        SpotATR:=spotATR,
                                        SpotPrice:=spotPrice,
                                        IterationNumber:=1,
                                        TypeOfEntry:=EntryType.Fresh,
                                        TypeOfEntryDetails:=ExitType.None,
                                        ATRConsumed:=signal.Item4,
                                        AttemptedEntryPrice:=entryPrice)

                If Interlocked.Exchange(CType(Me.ParentStrategy, NFOStrategy).TradePlacementLock, 1) = 0 Then
                    Try
                        Await currentATMOption.MonitorAsync(ExecuteCommands.PlaceRegularMarketCNCOrder, dummyTrade).ConfigureAwait(False)
                        Dim lastTrade As Trade = Me.SignalData.GetLastTrade()
                        If lastTrade IsNot Nothing AndAlso lastTrade.CurrentStatus = TradeStatus.InProgress Then
                            CType(Me.ParentStrategy, NFOStrategy).TotalActiveInstrumentCount += 1

                            Try
                                If lastTrade.Direction = TradeDirection.Buy Then
                                    Dim spotMoved As Decimal = Me.TradableInstrument.LastTick.LastPrice - _eodPayload(lastTrade.EntrySignalDate).ClosePrice.Value
                                    If spotMoved > 0 Then lastTrade.UpdateTrade(ATRConsumed:=(spotMoved / lastTrade.SpotATR) * 100)
                                ElseIf lastTrade.Direction = TradeDirection.Sell Then
                                    Dim spotMoved As Decimal = _eodPayload(lastTrade.EntrySignalDate).ClosePrice.Value - Me.TradableInstrument.LastTick.LastPrice
                                    If spotMoved > 0 Then lastTrade.UpdateTrade(ATRConsumed:=(spotMoved / lastTrade.SpotATR) * 100)
                                End If
                            Catch ex As Exception
                                logger.Warn(ex.ToString)
                            End Try

                            SendEntryOrderNotificationAsync(lastTrade, currentATMOption)
                        End If
                    Finally
                        Interlocked.Exchange(CType(Me.ParentStrategy, NFOStrategy).TradePlacementLock, 0)
                    End Try
                Else
                    Console.WriteLine(String.Format("{0}: Unable to get trade placement lock", Me.TradableInstrument.RawInstrumentName))
                End If
            End If
        End If
    End Function
#End Region

    Private Async Function PlaceEntryOrderIfRequired(ByVal currentSpotTick As ITick) As Task
        If Not Me.SignalData.IsActiveSignal() Then
            Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
            Dim lastCompleteTrade As Trade = Me.SignalData.GetLastCompleteTrade()
            If lastCompleteTrade IsNot Nothing AndAlso lastCompleteTrade.TypeOfExit = ExitType.ContractRollover Then
                Await PlaceContractRolloverEntryOrder(currentSpotTick, lastCompleteTrade, userSettings).ConfigureAwait(False)
            ElseIf lastCompleteTrade IsNot Nothing AndAlso lastCompleteTrade.TypeOfExit = ExitType.Reverse Then
                Await PlaceReversalEntryOrder(currentSpotTick, lastCompleteTrade, userSettings).ConfigureAwait(False)
            Else
                Dim lastTrade As Trade = Me.SignalData.GetLastTrade()
                Dim signal As Tuple(Of Boolean, OHLCPayload, IOrder.TypeOfTransaction, Decimal) = Nothing
                If Now >= userSettings.TradeEntryTime Then
                    signal = GetFreshEntrySignal(currentSpotTick)
                    If signal IsNot Nothing AndAlso signal.Item1 Then
                        If userSettings.SameSignalEntry Then
                            If signal.Item2.SnapshotDateTime.Date <> Now.Date AndAlso Now < userSettings.TradeEntryTime.AddMinutes(1) Then
                                signal = Nothing
                            End If
                        Else
                            If lastTrade IsNot Nothing AndAlso lastTrade.TypeOfExit = ExitType.Target AndAlso
                                lastTrade.EntrySignalDate = signal.Item2.SnapshotDateTime Then
                                signal = Nothing
                            Else
                                If signal.Item2.SnapshotDateTime.Date <> Now.Date AndAlso Now < userSettings.TradeEntryTime.AddMinutes(1) Then
                                    signal = Nothing
                                End If
                            End If
                        End If
                    End If
                End If
                If signal IsNot Nothing AndAlso signal.Item1 Then
                    Await PlaceFreshEntryOrder(currentSpotTick, signal, userSettings).ConfigureAwait(False)
                End If
            End If
        End If
    End Function
#End Region

#Region "Exit Order"
    Private Async Function PlaceExitOrderIfRequired() As Task
        Dim lastRunningTrade As Trade = Me.SignalData.GetLastTrade()
        If lastRunningTrade IsNot Nothing AndAlso lastRunningTrade.CurrentStatus = TradeStatus.InProgress Then
            Dim optnStrgInstrmnt As NFOStrategyInstrument = Await GetStrategyInstrumentFromTradingSymbol(lastRunningTrade.TradingSymbol).ConfigureAwait(False)
            If optnStrgInstrmnt IsNot Nothing AndAlso optnStrgInstrmnt.TradableInstrument.LastTick IsNot Nothing Then
                Dim currentOptionTick As ITick = optnStrgInstrmnt.TradableInstrument.LastTick
                If Not IsTradeTargetOrderPlaced(lastRunningTrade) Then
                    Dim allTrades As List(Of Trade) = Me.SignalData.GetAllTradesByParentTag(lastRunningTrade.ParentTag)
                    If allTrades IsNot Nothing AndAlso allTrades.Count > 0 Then
                        Dim totalRealisedPL As Decimal = 0
                        For Each runningTrade In allTrades
                            If runningTrade.CurrentStatus = TradeStatus.Complete Then
                                totalRealisedPL += _APIAdapter.CalculatePLWithBrokerage(optnStrgInstrmnt.TradableInstrument, runningTrade.EntryPrice, runningTrade.ExitPrice, runningTrade.Quantity)
                            End If
                        Next

                        Dim plToAchieveFromThisTrade As Decimal = lastRunningTrade.PotentialTarget - totalRealisedPL
                        Dim targetPrice As Decimal = optnStrgInstrmnt.CalculateTargetFromPL(lastRunningTrade.EntryPrice, lastRunningTrade.Quantity, plToAchieveFromThisTrade)

                        If currentOptionTick.LastPrice >= targetPrice Then
                            Dim dummyTrade As Trade = New Trade(Nothing)
                            dummyTrade.UpdateTrade(Quantity:=lastRunningTrade.Quantity, TypeOfExit:=ExitType.Target, AttemptedExitPrice:=currentOptionTick.LastPrice)

                            Await optnStrgInstrmnt.MonitorAsync(ExecuteCommands.CancelRegularOrder, dummyTrade).ConfigureAwait(False)
                            Dim lastTrade As Trade = Me.SignalData.GetLastTrade()
                            If lastTrade IsNot Nothing AndAlso lastTrade.CurrentStatus = TradeStatus.Complete Then
                                CType(Me.ParentStrategy, NFOStrategy).TotalActiveInstrumentCount -= 1
                                SendExitOrderNotificationAsync(lastTrade, optnStrgInstrmnt)
                            End If
                        Else
                            Dim dummyTrade As Trade = New Trade(Nothing)
                            dummyTrade.UpdateTrade(Quantity:=lastRunningTrade.Quantity, TypeOfExit:=ExitType.Target, AttemptedExitPrice:=targetPrice)

                            Await optnStrgInstrmnt.MonitorAsync(ExecuteCommands.PlaceRegularLimitCNCOrder, dummyTrade).ConfigureAwait(False)
                        End If
                    End If
                ElseIf IsTradeTargetReached(lastRunningTrade, optnStrgInstrmnt) Then
                    Dim lastTrade As Trade = Me.SignalData.GetLastTrade()
                    If lastTrade IsNot Nothing AndAlso lastTrade.CurrentStatus = TradeStatus.Complete Then
                        CType(Me.ParentStrategy, NFOStrategy).TotalActiveInstrumentCount -= 1
                        SendExitOrderNotificationAsync(lastTrade, optnStrgInstrmnt)
                    End If
                ElseIf IsTradeReverseSignalReceived(lastRunningTrade) Then
                    Dim dummyCancelTrade As Trade = New Trade(Nothing)
                    dummyCancelTrade.UpdateTrade(TargetOrderID:=lastRunningTrade.TargetOrderID)
                    Await optnStrgInstrmnt.MonitorAsync(ExecuteCommands.ForceCancelRegularOrder, dummyCancelTrade).ConfigureAwait(False)
                    Dim lastTrade As Trade = Me.SignalData.GetLastTrade()
                    If lastTrade IsNot Nothing AndAlso lastTrade.TargetOrderID IsNot Nothing AndAlso lastTrade.TargetOrderID = "" Then
                        Dim dummyTrade As Trade = New Trade(Nothing)
                        dummyTrade.UpdateTrade(Quantity:=lastRunningTrade.Quantity, TypeOfExit:=ExitType.Reverse, AttemptedExitPrice:=currentOptionTick.LastPrice, ExitSignalDate:=_trendPayload.LastOrDefault.Key)
                        Await optnStrgInstrmnt.MonitorAsync(ExecuteCommands.CancelRegularOrder, dummyTrade).ConfigureAwait(False)
                        lastTrade = Me.SignalData.GetLastTrade()
                        SendExitOrderNotificationAsync(lastTrade, optnStrgInstrmnt)
                    End If
                ElseIf IsTradeContractRollover(lastRunningTrade, optnStrgInstrmnt) Then
                    Dim dummyCancelTrade As Trade = New Trade(Nothing)
                    dummyCancelTrade.UpdateTrade(TargetOrderID:=lastRunningTrade.TargetOrderID)
                    Await optnStrgInstrmnt.MonitorAsync(ExecuteCommands.ForceCancelRegularOrder, dummyCancelTrade).ConfigureAwait(False)
                    Dim lastTrade As Trade = Me.SignalData.GetLastTrade()
                    If lastTrade IsNot Nothing AndAlso lastTrade.TargetOrderID IsNot Nothing AndAlso lastTrade.TargetOrderID = "" Then
                        Dim dummyTrade As Trade = New Trade(Nothing)
                        dummyTrade.UpdateTrade(Quantity:=lastRunningTrade.Quantity, TypeOfExit:=ExitType.ContractRollover, AttemptedExitPrice:=currentOptionTick.LastPrice)
                        Await optnStrgInstrmnt.MonitorAsync(ExecuteCommands.CancelRegularOrder, dummyTrade).ConfigureAwait(False)
                        lastTrade = Me.SignalData.GetLastTrade()
                        SendExitOrderNotificationAsync(lastTrade, optnStrgInstrmnt)
                    End If
                End If
            End If
        End If
    End Function

    Private Function IsTradeTargetOrderPlaced(ByVal runningTrade As Trade) As Boolean
        Dim ret As Boolean = True
        If runningTrade IsNot Nothing AndAlso runningTrade.CurrentStatus = TradeStatus.InProgress Then
            If runningTrade.TargetOrderID Is Nothing Then
                ret = False
            ElseIf runningTrade.TargetOrderTime = Date.MinValue OrElse runningTrade.TargetOrderTime = Date.MaxValue OrElse
                runningTrade.TargetOrderTime.Date <> Now.Date Then
                ret = False
            ElseIf runningTrade.TargetOrderTime.Date = Now.Date AndAlso Now >= runningTrade.TargetOrderTime.AddMinutes(1) AndAlso runningTrade.TargetOrderID = "" Then
                ret = False
            End If
        End If
        Return ret
    End Function

    Private Function IsTradeReverseSignalReceived(ByVal runningTrade As Trade) As Boolean
        Dim ret As Boolean = False
        If runningTrade IsNot Nothing AndAlso runningTrade.CurrentStatus = TradeStatus.InProgress Then
            Dim trend As Color = _trendPayload.LastOrDefault.Value
            If runningTrade.Direction = TradeDirection.Buy AndAlso trend = Color.Red Then
                ret = True
            ElseIf runningTrade.Direction = TradeDirection.Sell AndAlso trend = Color.Green Then
                ret = True
            End If
        End If
        Return ret
    End Function

    Private Function IsTradeContractRollover(ByVal runningTrade As Trade, ByVal optionStrategyInstrument As NFOStrategyInstrument) As Boolean
        Dim ret As Boolean = False
        If runningTrade IsNot Nothing AndAlso runningTrade.CurrentStatus = TradeStatus.InProgress Then
            Dim expiryDate As Date = optionStrategyInstrument.TradableInstrument.Expiry.Value.AddDays(-2)
            expiryDate = New Date(expiryDate.Year, expiryDate.Month, expiryDate.Day, 15, 29, 0)
            If Now >= expiryDate Then
                ret = True
            End If
        End If
        Return ret
    End Function

    Private Function IsTradeTargetReached(ByVal runningTrade As Trade, ByVal optionStrategyInstrument As NFOStrategyInstrument) As Boolean
        Dim ret As Boolean = False
        If runningTrade IsNot Nothing AndAlso runningTrade.CurrentStatus = TradeStatus.InProgress Then
            If runningTrade.TargetOrderID IsNot Nothing AndAlso runningTrade.TargetOrderID.Trim <> "" Then
                If optionStrategyInstrument.OrderDetails IsNot Nothing AndAlso
                    optionStrategyInstrument.OrderDetails.ContainsKey(runningTrade.TargetOrderID) Then
                    Dim order As IBusinessOrder = optionStrategyInstrument.OrderDetails(runningTrade.TargetOrderID)
                    If order IsNot Nothing AndAlso order.ParentOrder IsNot Nothing Then
                        If order.ParentOrder.Status = IOrder.TypeOfStatus.Complete Then
                            Dim lastTrade As Trade = Me.SignalData.GetLastTrade()
                            lastTrade.UpdateTrade(CurrentStatus:=TradeStatus.Complete,
                                                    ExitPrice:=order.ParentOrder.AveragePrice,
                                                    TypeOfExit:=ExitType.Target,
                                                    ExitOrderID:=runningTrade.TargetOrderID,
                                                    ExitTime:=Now)

                            ret = True
                        ElseIf order.ParentOrder.Status = IOrder.TypeOfStatus.Cancelled Then
                            Dim lastTrade As Trade = Me.SignalData.GetLastTrade()
                            lastTrade.UpdateTrade(TargetOrderID:="", TargetOrderTime:=Date.MaxValue)
                        End If
                    End If
                End If
            End If
        End If
        Return ret
    End Function
#End Region

#Region "Option Monitor Async"
    Public Overrides Async Function MonitorAsync(ByVal command As ExecuteCommands, ByVal data As Object) As Task
        Try
            If Now >= Me.TradableInstrument.ExchangeDetails.ExchangeStartTime AndAlso Now <= Me.TradableInstrument.ExchangeDetails.ExchangeEndTime Then
                If command = ExecuteCommands.PlaceRegularMarketCNCOrder AndAlso data IsNot Nothing Then 'Entry Trade
                    _executeCommandData = data
                    _executeCommand = ExecuteCommands.PlaceRegularMarketCNCOrder
                    Try
                        logger.Debug("{0}: Place Order. Option Contract:{1}, Signal Direction:{2}, Child Tag:{3}, Parent Tag:{4}, Quantity:{5}, Potential Target:{6}, Signal Date:{7}, Spot Price:{8}, Spot ATR:{9}, Trade Number:{10}, Entry Type:{11}",
                                     Me.TradableInstrument.RawInstrumentName,
                                     _executeCommandData.TradingSymbol,
                                     _executeCommandData.Direction.ToString,
                                     _executeCommandData.ChildTag,
                                     _executeCommandData.ParentTag,
                                     _executeCommandData.Quantity,
                                     _executeCommandData.PotentialTarget,
                                     _executeCommandData.EntrySignalDate.ToString("dd-MMM-yyyy"),
                                     _executeCommandData.SpotPrice,
                                     _executeCommandData.SpotATR,
                                     _executeCommandData.IterationNumber,
                                     _executeCommandData.TypeOfEntry.ToString)
                    Catch ex As Exception
                        logger.Warn(ex.ToString)
                    End Try

                    Dim orderResponse = Await ExecuteCommandAsync(ExecuteCommands.PlaceRegularMarketCNCOrder, Nothing).ConfigureAwait(False)
                    If orderResponse IsNot Nothing AndAlso orderResponse.Count > 0 Then
                        Dim placeOrderResponse = CType(orderResponse, Concurrent.ConcurrentBag(Of Object)).FirstOrDefault
                        If placeOrderResponse.ContainsKey("data") AndAlso
                            placeOrderResponse("data").ContainsKey("order_id") Then
                            Dim orderID As String = placeOrderResponse("data")("order_id")
                            Dim placedTime As Date = Now
                            _executeCommandData.UpdateTrade(EntryTime:=placedTime, EntryOrderID:=orderID)
                            Me.SignalData.InsertOrder(_executeCommandData, orderID)
                            While True
                                _cts.Token.ThrowIfCancellationRequested()
                                'If Me._RMSException IsNot Nothing AndAlso
                                '    _RMSException.ExceptionType = Algo2TradeCore.Exceptions.AdapterBusinessException.TypeOfException.RMSError Then
                                '    OnHeartbeat(String.Format("{0}:Will not take no more action in this instrument as RMS Error occured. Error-{1}", Me.TradableInstrument.TradingSymbol, _RMSException.Message))
                                '    Throw Me._RMSException
                                'End If
                                If Me.OrderDetails IsNot Nothing AndAlso Me.OrderDetails.ContainsKey(orderID) Then
                                    Dim order As IBusinessOrder = Me.OrderDetails(orderID)
                                    If order IsNot Nothing AndAlso order.ParentOrder IsNot Nothing Then
                                        If order.ParentOrder.Status = IOrder.TypeOfStatus.Complete Then
                                            Dim lastTrade As Trade = Me.SignalData.GetLastTrade()
                                            lastTrade.UpdateTrade(CurrentStatus:=TradeStatus.InProgress, EntryPrice:=order.ParentOrder.AveragePrice)
                                            Exit While
                                        Else
                                            If Now >= Me.TradableInstrument.ExchangeDetails.ExchangeEndTime Then
                                                Dim lastTrade As Trade = Me.SignalData.GetLastTrade()
                                                lastTrade.UpdateTrade(CurrentStatus:=TradeStatus.Cancel)
                                                Exit While
                                            ElseIf Now >= placedTime.AddMinutes(1) Then
                                                Exit While
                                            End If
                                        End If
                                    End If
                                End If
                                Await Task.Delay(500).ConfigureAwait(False)
                            End While
                            'If Me.OrderDetails IsNot Nothing AndAlso Me.OrderDetails.ContainsKey(orderID) Then
                            Dim placedOrder As IBusinessOrder = Me.OrderDetails(orderID)
                            If placedOrder IsNot Nothing AndAlso placedOrder.ParentOrder IsNot Nothing Then
                                If placedOrder.ParentOrder.Status = IOrder.TypeOfStatus.Complete Then
                                    Dim lastTrade As Trade = Me.SignalData.GetLastTrade()
                                    lastTrade.UpdateTrade(CurrentStatus:=TradeStatus.InProgress, EntryPrice:=placedOrder.ParentOrder.AveragePrice)
                                Else
                                    Await ForceExitSpecificTradeAsync(placedOrder.ParentOrder, "Invalid Order").ConfigureAwait(False)
                                    While True
                                        _cts.Token.ThrowIfCancellationRequested()
                                        If Me.OrderDetails IsNot Nothing AndAlso Me.OrderDetails.ContainsKey(orderID) Then
                                            Dim order As IBusinessOrder = Me.OrderDetails(orderID)
                                            If order IsNot Nothing AndAlso order.ParentOrder IsNot Nothing Then
                                                If order.ParentOrder.Status = IOrder.TypeOfStatus.Complete Then
                                                    Dim lastTrade As Trade = Me.SignalData.GetLastTrade()
                                                    lastTrade.UpdateTrade(CurrentStatus:=TradeStatus.InProgress, EntryPrice:=placedOrder.ParentOrder.AveragePrice)
                                                    Exit While
                                                ElseIf order.ParentOrder.Status = IOrder.TypeOfStatus.Cancelled Then
                                                    Dim lastTrade As Trade = Me.SignalData.GetLastTrade()
                                                    lastTrade.UpdateTrade(CurrentStatus:=TradeStatus.Cancel)
                                                    Exit While
                                                ElseIf order.ParentOrder.Status = IOrder.TypeOfStatus.Rejected Then
                                                    Dim lastTrade As Trade = Me.SignalData.GetLastTrade()
                                                    lastTrade.UpdateTrade(CurrentStatus:=TradeStatus.Cancel)
                                                    Exit While
                                                Else
                                                    If Now > Me.TradableInstrument.ExchangeDetails.ExchangeEndTime Then
                                                        Dim lastTrade As Trade = Me.SignalData.GetLastTrade()
                                                        Utilities.Strings.SerializeFromCollection(Of SignalDetails)(Me.SignalData.SignalDetailsFilename, Me.SignalData)
                                                        Exit While
                                                    End If
                                                End If
                                            End If
                                        End If
                                        Await Task.Delay(500).ConfigureAwait(False)
                                    End While
                                End If
                            End If
                            'End If
                        End If
                    End If
                ElseIf command = ExecuteCommands.PlaceRegularLimitCNCOrder AndAlso data IsNot Nothing Then 'Target Trade Place
                    _executeCommandData = data
                    _executeCommand = ExecuteCommands.PlaceRegularLimitCNCOrder

                    Try
                        logger.Debug("{0}: Target Order. Option Contract:{1}, Quantity:{2}, Exit Type:{3}",
                                     Me.TradableInstrument.RawInstrumentName,
                                     Me.TradableInstrument.TradingSymbol,
                                     _executeCommandData.Quantity,
                                     _executeCommandData.TypeOfExit.ToString)
                    Catch ex As Exception
                        logger.Warn(ex.ToString)
                    End Try

                    Dim orderResponse = Await ExecuteCommandAsync(ExecuteCommands.PlaceRegularLimitCNCOrder, Nothing).ConfigureAwait(False)
                    If orderResponse IsNot Nothing AndAlso orderResponse.Count > 0 Then
                        Dim placeOrderResponse = CType(orderResponse, Concurrent.ConcurrentBag(Of Object)).FirstOrDefault
                        If placeOrderResponse.ContainsKey("data") AndAlso
                            placeOrderResponse("data").ContainsKey("order_id") Then
                            Dim orderID As String = placeOrderResponse("data")("order_id")
                            Dim lastTrade As Trade = Me.SignalData.GetLastTrade()
                            lastTrade.UpdateTrade(TargetOrderID:=orderID, TargetOrderTime:=Now, AttemptedExitPrice:=_executeCommandData.AttemptedExitPrice)
                            While True
                                _cts.Token.ThrowIfCancellationRequested()
                                If Me.OrderDetails IsNot Nothing AndAlso Me.OrderDetails.ContainsKey(orderID) Then
                                    Dim order As IBusinessOrder = Me.OrderDetails(orderID)
                                    If order IsNot Nothing AndAlso order.ParentOrder IsNot Nothing Then
                                        If order.ParentOrder.Status = IOrder.TypeOfStatus.Rejected Then
                                            lastTrade.UpdateTrade(TargetOrderID:="", TargetOrderTime:=Now)
                                            Exit While
                                        Else
                                            Exit While
                                        End If
                                    End If
                                End If
                                Await Task.Delay(500).ConfigureAwait(False)
                            End While
                        End If
                    End If
                ElseIf command = ExecuteCommands.ForceCancelRegularOrder AndAlso data IsNot Nothing Then 'Target Trade cancel
                    _executeCommandData = data
                    _executeCommand = ExecuteCommands.CancelRegularOrder

                    If Me.OrderDetails IsNot Nothing AndAlso _executeCommandData.TargetOrderID IsNot Nothing AndAlso
                        Me.OrderDetails.ContainsKey(_executeCommandData.TargetOrderID) Then
                        Dim placedOrder As IBusinessOrder = Me.OrderDetails(_executeCommandData.TargetOrderID)
                        If placedOrder IsNot Nothing AndAlso placedOrder.ParentOrder IsNot Nothing Then
                            Await ForceExitSpecificTradeAsync(placedOrder.ParentOrder, "Invalid Order").ConfigureAwait(False)
                            Dim orderID As String = _executeCommandData.TargetOrderID
                            While True
                                _cts.Token.ThrowIfCancellationRequested()
                                If Me.OrderDetails IsNot Nothing AndAlso Me.OrderDetails.ContainsKey(orderID) Then
                                    Dim order As IBusinessOrder = Me.OrderDetails(orderID)
                                    If order IsNot Nothing AndAlso order.ParentOrder IsNot Nothing Then
                                        If order.ParentOrder.Status = IOrder.TypeOfStatus.Cancelled Then
                                            Dim lastTrade As Trade = Me.SignalData.GetLastTrade()
                                            lastTrade.UpdateTrade(TargetOrderID:="", TargetOrderTime:=Date.MaxValue)
                                            Exit While
                                        Else
                                            Exit While
                                        End If
                                    End If
                                End If
                                Await Task.Delay(500).ConfigureAwait(False)
                            End While
                        End If
                    Else
                        Dim lastTrade As Trade = Me.SignalData.GetLastTrade()
                        lastTrade.UpdateTrade(TargetOrderID:="", TargetOrderTime:=Date.MaxValue)
                    End If
                ElseIf command = ExecuteCommands.CancelRegularOrder AndAlso data IsNot Nothing Then 'Exit Trade
                    _executeCommandData = data
                    _executeCommand = ExecuteCommands.CancelRegularOrder

                    Try
                        logger.Debug("{0}: Exit Order. Option Contract:{1}, Quantity:{2}, Exit Type:{3}",
                                     Me.TradableInstrument.RawInstrumentName,
                                     Me.TradableInstrument.TradingSymbol,
                                     _executeCommandData.Quantity,
                                     _executeCommandData.TypeOfExit.ToString)
                    Catch ex As Exception
                        logger.Warn(ex.ToString)
                    End Try

                    Dim orderResponse = Await ExecuteCommandAsync(ExecuteCommands.PlaceRegularMarketCNCOrder, Nothing).ConfigureAwait(False)
                    If orderResponse IsNot Nothing AndAlso orderResponse.Count > 0 Then
                        Dim placeOrderResponse = CType(orderResponse, Concurrent.ConcurrentBag(Of Object)).FirstOrDefault
                        If placeOrderResponse.ContainsKey("data") AndAlso
                            placeOrderResponse("data").ContainsKey("order_id") Then
                            Dim orderID As String = placeOrderResponse("data")("order_id")
                            Dim lastTrade As Trade = Me.SignalData.GetLastTrade()
                            lastTrade.UpdateTrade(TypeOfExit:=_executeCommandData.TypeOfExit,
                                                  ExitOrderID:=orderID,
                                                  ExitTime:=Now,
                                                  AttemptedExitPrice:=_executeCommandData.AttemptedExitPrice,
                                                  ExitSignalDate:=_executeCommandData.ExitSignalDate)
                            While True
                                _cts.Token.ThrowIfCancellationRequested()
                                If Me.OrderDetails IsNot Nothing AndAlso Me.OrderDetails.ContainsKey(orderID) Then
                                    Dim order As IBusinessOrder = Me.OrderDetails(orderID)
                                    If order IsNot Nothing AndAlso order.ParentOrder IsNot Nothing Then
                                        If order.ParentOrder.Status = IOrder.TypeOfStatus.Complete Then
                                            lastTrade.UpdateTrade(CurrentStatus:=TradeStatus.Complete, ExitPrice:=order.ParentOrder.AveragePrice)
                                            Exit While
                                        Else
                                            If Now > Me.TradableInstrument.ExchangeDetails.ExchangeEndTime Then
                                                Exit While
                                            End If
                                        End If
                                    End If
                                End If
                                Await Task.Delay(500).ConfigureAwait(False)
                            End While
                        End If
                    End If
                End If
            End If
        Catch ex As Exception
            logger.Error(String.Format("Strategy Instrument:{0} stopped, error:{1}", Me.ToString, ex.ToString))
            Throw ex
        Finally
            _executeCommandData = Nothing
            _executeCommand = ExecuteCommands.CancelBOOrder
        End Try
    End Function


    Protected Overrides Async Function ForceExitSpecificTradeAsync(order As IOrder, ByVal reason As String) As Task
        If order IsNot Nothing AndAlso Not order.Status = IOrder.TypeOfStatus.Complete AndAlso
            Not order.Status = IOrder.TypeOfStatus.Cancelled AndAlso
            Not order.Status = IOrder.TypeOfStatus.Rejected Then
            Dim cancellableOrder As New List(Of Tuple(Of ExecuteCommandAction, IOrder, String)) From
            {
                New Tuple(Of ExecuteCommandAction, IOrder, String)(ExecuteCommandAction.Take, order, reason)
            }

            Await ExecuteCommandAsync(ExecuteCommands.ForceCancelRegularOrder, cancellableOrder).ConfigureAwait(False)
        End If
    End Function
#End Region

#Region "Place Order Trigger"
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
                ElseIf _executeCommand = ExecuteCommands.PlaceRegularLimitCNCOrder Then
                    parameters = New PlaceOrderParameters(signalCandle) With
                                     {
                                        .EntryDirection = IOrder.TypeOfTransaction.Sell,
                                        .OrderType = IOrder.TypeOfOrder.Limit,
                                        .Quantity = _executeCommandData.Quantity,
                                        .Price = _executeCommandData.AttemptedExitPrice
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
#End Region

#Region "Option Selection"
    Private Async Function GetOptionToTrade(ByVal currentTick As ITick, ByVal direction As IOrder.TypeOfTransaction) As Task(Of NFOStrategyInstrument)
        Dim ret As NFOStrategyInstrument = Nothing
        If direction = IOrder.TypeOfTransaction.Buy Then
            ret = Await GetCurrentATMOption(currentTick, IOrder.TypeOfTransaction.Buy, "CE").ConfigureAwait(False)
        ElseIf direction = IOrder.TypeOfTransaction.Sell Then
            ret = Await GetCurrentATMOption(currentTick, IOrder.TypeOfTransaction.Sell, "PE").ConfigureAwait(False)
        End If
        Try
            logger.Debug("{0}: Signal Direction: {1}. Current Price:{2}, Option Instrument:{3}",
                        Me.TradableInstrument.TradingSymbol, direction.ToString, currentTick.LastPrice,
                        If(ret IsNot Nothing, ret.TradableInstrument.TradingSymbol, "Nothing"))
        Catch ex As Exception
            logger.Warn(ex.ToString)
        End Try

        Return ret
    End Function

    Private Async Function CreateStrategyInstrumentAndPopulate(ByVal instrument As IInstrument) As Task(Of NFOStrategyInstrument)
        Dim ret As NFOStrategyInstrument = Nothing
        ret = Await CType(Me.ParentStrategy, NFOStrategy).CreateDependentTradableStrategyInstrumentsAsync(instrument, Me).ConfigureAwait(False)
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

    Public Async Function GetStrategyInstrumentFromTradingSymbol(ByVal tradingSymbol As String) As Task(Of NFOStrategyInstrument)
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

#Region "Signal Check"
    Private Function GetFreshEntrySignal(ByVal currentTick As ITick) As Tuple(Of Boolean, OHLCPayload, IOrder.TypeOfTransaction, Decimal)
        Dim ret As Tuple(Of Boolean, OHLCPayload, IOrder.TypeOfTransaction, Decimal) = Nothing
        Dim signal As Tuple(Of Boolean, OHLCPayload, IOrder.TypeOfTransaction) = Nothing
        If currentTick IsNot Nothing AndAlso Not Me.SignalData.IsActiveSignal() Then
            Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
            Dim potentialSignalCandle As OHLCPayload = _eodPayload.LastOrDefault.Value
            Dim trend As Color = _trendPayload(potentialSignalCandle.SnapshotDateTime)
            Dim previousTrend As Color = _trendPayload(potentialSignalCandle.PreviousPayload.SnapshotDateTime)
            If trend = Color.Green Then
                If previousTrend = Color.Red Then
                    Dim sma As Decimal = _smaPayload(potentialSignalCandle.SnapshotDateTime)
                    If potentialSignalCandle.ClosePrice.Value - sma < _atrPayload(potentialSignalCandle.SnapshotDateTime.Date) * userSettings.DistanceFromMA Then
                        signal = New Tuple(Of Boolean, OHLCPayload, IOrder.TypeOfTransaction)(True, potentialSignalCandle, IOrder.TypeOfTransaction.Buy)
                    End If
                Else
                    Dim rolloverDay As Date = GetChangeoverDay(trend)
                    If rolloverDay <> Date.MinValue Then
                        potentialSignalCandle = _eodPayload(rolloverDay)
                        Dim sma As Decimal = _smaPayload(potentialSignalCandle.SnapshotDateTime)
                        If potentialSignalCandle.ClosePrice.Value - sma < _atrPayload(potentialSignalCandle.SnapshotDateTime.Date) * userSettings.DistanceFromMA Then
                            signal = New Tuple(Of Boolean, OHLCPayload, IOrder.TypeOfTransaction)(True, potentialSignalCandle, IOrder.TypeOfTransaction.Buy)
                        End If
                    End If
                End If
            ElseIf trend = Color.Red Then
                If previousTrend = Color.Green Then
                    Dim sma As Decimal = _smaPayload(potentialSignalCandle.SnapshotDateTime)
                    If sma - potentialSignalCandle.ClosePrice.Value < _atrPayload(potentialSignalCandle.SnapshotDateTime.Date) * userSettings.DistanceFromMA Then
                        signal = New Tuple(Of Boolean, OHLCPayload, IOrder.TypeOfTransaction)(True, potentialSignalCandle, IOrder.TypeOfTransaction.Sell)
                    End If
                Else
                    Dim rolloverDay As Date = GetChangeoverDay(trend)
                    If rolloverDay <> Date.MinValue Then
                        potentialSignalCandle = _eodPayload(rolloverDay)
                        Dim sma As Decimal = _smaPayload(potentialSignalCandle.SnapshotDateTime)
                        If sma - potentialSignalCandle.ClosePrice.Value < _atrPayload(potentialSignalCandle.SnapshotDateTime.Date) * userSettings.DistanceFromMA Then
                            signal = New Tuple(Of Boolean, OHLCPayload, IOrder.TypeOfTransaction)(True, potentialSignalCandle, IOrder.TypeOfTransaction.Sell)
                        End If
                    End If
                End If
            End If
            If signal IsNot Nothing AndAlso signal.Item1 Then
                Dim neglectReason As String = Nothing
                'If CType(Me.ParentStrategy, NFOStrategy).TotalActiveInstrumentCount < userSettings.ActiveInstrumentCount Then
                Dim targetReached As Boolean = True
                Dim targetLeftPercentage As Decimal = 0
                If signal.Item3 = IOrder.TypeOfTransaction.Buy Then
                    Dim highestHigh As Decimal = _eodPayload.Max(Function(x)
                                                                     If x.Key > signal.Item2.SnapshotDateTime AndAlso x.Key <= Now.Date Then
                                                                         Return CDec(x.Value.HighPrice.Value)
                                                                     Else
                                                                         Return Decimal.MinValue
                                                                     End If
                                                                 End Function)
                    If signal.Item2.SnapshotDateTime.Date <> Now.Date Then
                        highestHigh = Math.Max(highestHigh, currentTick.High)
                    End If
                    Dim atr As Decimal = _atrPayload(signal.Item2.SnapshotDateTime)
                    If highestHigh < signal.Item2.ClosePrice.Value + atr Then
                        targetReached = False
                        If highestHigh <> Decimal.MinValue Then
                            targetLeftPercentage = ((atr - (highestHigh - signal.Item2.ClosePrice.Value)) / atr) * 100
                        Else
                            targetLeftPercentage = 100
                        End If
                    End If
                ElseIf signal.Item3 = IOrder.TypeOfTransaction.Sell Then
                    Dim lowestLow As Decimal = _eodPayload.Min(Function(x)
                                                                   If x.Key > signal.Item2.SnapshotDateTime AndAlso x.Key <= Now.Date Then
                                                                       Return CDec(x.Value.LowPrice.Value)
                                                                   Else
                                                                       Return Decimal.MaxValue
                                                                   End If
                                                               End Function)
                    If signal.Item2.SnapshotDateTime.Date <> Now.Date Then
                        lowestLow = Math.Min(lowestLow, currentTick.Low)
                    End If
                    Dim atr As Decimal = _atrPayload(signal.Item2.SnapshotDateTime)
                    If lowestLow > signal.Item2.ClosePrice.Value - atr Then
                        targetReached = False
                        If lowestLow <> Decimal.MaxValue Then
                            targetLeftPercentage = ((atr - (signal.Item2.ClosePrice.Value - lowestLow)) / atr) * 100
                        Else
                            targetLeftPercentage = 100
                        End If
                    End If
                End If
                If Not targetReached AndAlso targetLeftPercentage >= 75 Then
                    ret = New Tuple(Of Boolean, OHLCPayload, IOrder.TypeOfTransaction, Decimal)(True, signal.Item2, signal.Item3, 100 - targetLeftPercentage)
                Else
                    neglectReason = String.Format("Target Left:{0}% which is < 75%", Math.Round(targetLeftPercentage, 2))
                End If
                'Else
                '    neglectReason = "Active instrument count filled"
                'End If
                'If Not _eodMessageSend AndAlso Now > Me.TradableInstrument.ExchangeDetails.ExchangeEndTime.AddMinutes(1) Then
                '    If ret IsNot Nothing AndAlso ret.Item1 Then neglectReason = "Insufficient Volume"
                '    If neglectReason IsNot Nothing AndAlso neglectReason.Trim <> "" Then
                '        Dim message As String = String.Format("{0}-> Signal Date: {1}, Neglect Reason:{2}",
                '                                                Me.TradableInstrument.RawInstrumentName,
                '                                                signal.Item2.SnapshotDateTime.ToString("dd-MMM-yyyy"),
                '                                                neglectReason)

                '        SendNotificationAsync(message)
                '        _eodMessageSend = True
                '    End If
                'End If
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
                Dim trend As Color = _trendPayload(runningPayload.Value.PreviousPayload.SnapshotDateTime)
                If trend <> currentTrend Then
                    ret = runningPayload.Key
                    Exit For
                End If
            End If
        Next
        Return ret
    End Function
#End Region

#Region "PL Calculation"
    Public Function GetOverallSignalPL(ByVal currentTrade As Trade, ByVal currentStrategyInstrument As NFOStrategyInstrument, ByVal potentialExitPrice As Decimal) As Decimal
        Dim ret As Decimal = 0
        Dim allTrades As List(Of Trade) = Me.SignalData.GetAllTradesByParentTag(currentTrade.ParentTag)
        If allTrades IsNot Nothing AndAlso allTrades.Count > 0 Then
            For Each runningTrade In allTrades
                If runningTrade.CurrentStatus = TradeStatus.InProgress Then
                    ret += _APIAdapter.CalculatePLWithBrokerage(currentStrategyInstrument.TradableInstrument, runningTrade.EntryPrice, potentialExitPrice, runningTrade.Quantity)
                ElseIf runningTrade.CurrentStatus <> TradeStatus.Cancel Then
                    ret += _APIAdapter.CalculatePLWithBrokerage(currentStrategyInstrument.TradableInstrument, runningTrade.EntryPrice, runningTrade.ExitPrice, runningTrade.Quantity)
                End If
            Next
        End If
        Return ret
    End Function

    Private Function GetLossMakeupTradePL(ByVal currentTrade As Trade, ByVal currentStrategyInstrument As NFOStrategyInstrument, ByVal potentialExitPrice As Decimal) As Decimal
        Dim ret As Decimal = 0
        Dim allTrades As List(Of Trade) = Me.SignalData.GetAllTradesByChildTag(currentTrade.ChildTag)
        If allTrades IsNot Nothing AndAlso allTrades.Count > 0 Then
            For Each runningTrade In allTrades
                If runningTrade.TypeOfEntry = EntryType.LossMakeup Then
                    If runningTrade.CurrentStatus = TradeStatus.InProgress Then
                        ret += _APIAdapter.CalculatePLWithBrokerage(currentStrategyInstrument.TradableInstrument, runningTrade.EntryPrice, potentialExitPrice, runningTrade.Quantity - currentStrategyInstrument.TradableInstrument.LotSize)
                    ElseIf runningTrade.CurrentStatus <> TradeStatus.Cancel Then
                        ret += _APIAdapter.CalculatePLWithBrokerage(currentStrategyInstrument.TradableInstrument, runningTrade.EntryPrice, runningTrade.ExitPrice, runningTrade.Quantity - currentStrategyInstrument.TradableInstrument.LotSize)
                    End If
                End If
            Next
        End If
        Return ret
    End Function

    Private Function GetFreshTradePL(ByVal currentTrade As Trade, ByVal currentStrategyInstrument As NFOStrategyInstrument, ByVal potentialExitPrice As Decimal) As Decimal
        Dim ret As Decimal = 0
        Dim allTrades As List(Of Trade) = Me.SignalData.GetAllTradesByChildTag(currentTrade.ChildTag)
        If allTrades IsNot Nothing AndAlso allTrades.Count > 0 Then
            For Each runningTrade In allTrades
                If runningTrade.TypeOfEntry = EntryType.Fresh Then
                    If runningTrade.CurrentStatus = TradeStatus.InProgress Then
                        ret += _APIAdapter.CalculatePLWithBrokerage(currentStrategyInstrument.TradableInstrument, runningTrade.EntryPrice, potentialExitPrice, runningTrade.Quantity)
                    ElseIf runningTrade.CurrentStatus <> TradeStatus.Cancel Then
                        ret += _APIAdapter.CalculatePLWithBrokerage(currentStrategyInstrument.TradableInstrument, runningTrade.EntryPrice, runningTrade.ExitPrice, runningTrade.Quantity)
                    End If
                End If
            Next
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

    Private Sub ConvertToHeikenAshi(ByVal inputPayload As Dictionary(Of Date, OHLCPayload), ByRef outputPayload As Dictionary(Of Date, OHLCPayload))
        If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
            Dim tempHAPayload As OHLCPayload = Nothing
            Dim tempPreHAPayload As OHLCPayload = Nothing

            For Each runningInputPayload In inputPayload

                tempHAPayload = New OHLCPayload(OHLCPayload.PayloadSource.CalculatedHistorical)
                tempHAPayload.PreviousPayload = tempPreHAPayload
                If tempPreHAPayload Is Nothing Then
                    tempHAPayload.OpenPrice.Value = (runningInputPayload.Value.OpenPrice.Value + runningInputPayload.Value.ClosePrice.Value) / 2
                Else
                    tempHAPayload.OpenPrice.Value = (tempPreHAPayload.OpenPrice.Value + tempPreHAPayload.ClosePrice.Value) / 2
                End If
                tempHAPayload.ClosePrice.Value = (runningInputPayload.Value.OpenPrice.Value + runningInputPayload.Value.ClosePrice.Value + runningInputPayload.Value.HighPrice.Value + runningInputPayload.Value.LowPrice.Value) / 4
                tempHAPayload.HighPrice.Value = Math.Max(runningInputPayload.Value.HighPrice.Value, Math.Max(tempHAPayload.OpenPrice.Value, tempHAPayload.ClosePrice.Value))
                tempHAPayload.LowPrice.Value = Math.Min(runningInputPayload.Value.LowPrice.Value, Math.Min(tempHAPayload.OpenPrice.Value, tempHAPayload.ClosePrice.Value))
                tempHAPayload.Volume = runningInputPayload.Value.Volume
                tempHAPayload.SnapshotDateTime = runningInputPayload.Value.SnapshotDateTime
                tempHAPayload.TradingSymbol = runningInputPayload.Value.TradingSymbol
                tempPreHAPayload = tempHAPayload
                If outputPayload Is Nothing Then outputPayload = New Dictionary(Of Date, OHLCPayload)
                outputPayload.Add(runningInputPayload.Key, tempHAPayload)
            Next
        End If
    End Sub

    Private Sub CalculateSMA(ByVal smaPeriod As Integer, ByVal inputPayload As Dictionary(Of Date, OHLCPayload), ByRef outputPayload As Dictionary(Of Date, Decimal))
        If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
            Dim finalPriceToBeAdded As Decimal = 0
            For Each runningInputPayload In inputPayload
                Dim previousNInputFieldPayload As List(Of KeyValuePair(Of Date, OHLCPayload)) = GetSubPayload(inputPayload, runningInputPayload.Key, smaPeriod - 1, False)
                If previousNInputFieldPayload Is Nothing Then
                    finalPriceToBeAdded += runningInputPayload.Value.ClosePrice.Value
                ElseIf previousNInputFieldPayload IsNot Nothing AndAlso previousNInputFieldPayload.Count <= smaPeriod - 1 Then
                    Dim totalOfAllPrices As Decimal = 0
                    totalOfAllPrices = runningInputPayload.Value.ClosePrice.Value
                    totalOfAllPrices += previousNInputFieldPayload.Sum(Function(s) CDec(s.Value.ClosePrice.Value))

                    finalPriceToBeAdded = totalOfAllPrices / (previousNInputFieldPayload.Count + 1)
                Else
                    Dim totalOfAllPrices As Decimal = 0
                    totalOfAllPrices = runningInputPayload.Value.ClosePrice.Value
                    totalOfAllPrices += previousNInputFieldPayload.Sum(Function(s) CDec(s.Value.ClosePrice.Value))

                    finalPriceToBeAdded = Math.Round(totalOfAllPrices / (previousNInputFieldPayload.Count + 1), 2)
                End If
                If outputPayload Is Nothing Then outputPayload = New Dictionary(Of Date, Decimal)
                outputPayload.Add(runningInputPayload.Key, finalPriceToBeAdded)
            Next
        End If
    End Sub

    Private Sub CalculateHKMATrend(ByVal smaPeriod As Integer, ByVal inputPayload As Dictionary(Of Date, OHLCPayload), ByRef outputPayload As Dictionary(Of Date, Color))
        If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
            Dim hkPayload As Dictionary(Of Date, OHLCPayload) = Nothing
            ConvertToHeikenAshi(inputPayload, hkPayload)
            Dim smaPayload As Dictionary(Of Date, Decimal) = Nothing
            CalculateSMA(smaPeriod, hkPayload, smaPayload)

            Dim trend As Color = Color.White
            For Each runningPayload In hkPayload
                If Math.Round(CDec(runningPayload.Value.OpenPrice.Value), 2) = Math.Round(CDec(runningPayload.Value.LowPrice.Value), 2) Then
                    If runningPayload.Value.ClosePrice.Value > smaPayload(runningPayload.Key) Then
                        trend = Color.Green
                    End If
                ElseIf Math.Round(CDec(runningPayload.Value.OpenPrice.Value), 2) = Math.Round(CDec(runningPayload.Value.HighPrice.Value), 2) Then
                    If runningPayload.Value.ClosePrice.Value < smaPayload(runningPayload.Key) Then
                        trend = Color.Red
                    End If
                End If

                If outputPayload Is Nothing Then outputPayload = New Dictionary(Of Date, Color)
                outputPayload.Add(runningPayload.Key, trend)
            Next
        End If
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
            CalculateHKMATrend(userSettings.SMAPeriod, _eodPayload, _trendPayload)
            CalculateATR(userSettings.ATRPeriod, _eodPayload, _atrPayload)
            CalculateSMA(userSettings.SMAPeriod, _eodPayload, _smaPayload)

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

#Region "Telegram"
    Private Async Function SendEntryOrderNotificationAsync(ByVal order As Trade, ByVal optnStrgInstrmnt As NFOStrategyInstrument) As Task
        Try
            Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
            _cts.Token.ThrowIfCancellationRequested()
            If order IsNot Nothing Then
                Dim targetPrice As Decimal = Decimal.MinValue
                Dim allTrades As List(Of Trade) = Me.SignalData.GetAllTradesByParentTag(order.ParentTag)
                If allTrades IsNot Nothing AndAlso allTrades.Count > 0 Then
                    Dim totalRealisedPL As Decimal = 0
                    For Each runningTrade In allTrades
                        If runningTrade.CurrentStatus = TradeStatus.Complete Then
                            totalRealisedPL += _APIAdapter.CalculatePLWithBrokerage(optnStrgInstrmnt.TradableInstrument, runningTrade.EntryPrice, runningTrade.ExitPrice, runningTrade.Quantity)
                        End If
                    Next

                    Dim plToAchieveFromThisTrade As Decimal = order.PotentialTarget - totalRealisedPL
                    targetPrice = optnStrgInstrmnt.CalculateTargetFromPL(order.EntryPrice, order.Quantity, plToAchieveFromThisTrade)
                End If

                Dim msg As String = String.Format("{1}: Entry {0}Signal Direction:{2}{0}Entry Type:{3}{0}Logical Iteration Number:{4}{0}Quantity:{5}{0}ATR Consumed:{6}%{0}Loss To Recover:{7}{0}Signal Date:{8}{0}Spot Price:{9}{0}Spot ATR:{10}{0}Option Contract:{11}{0}Entry Price:{12}{0}Entry Time:{13}{0}Capital:{14}{0}Attempted Entry Price:{15}{0}Potential Target:{16}{0}Potential Target PL:{17}",
                                                   vbNewLine,
                                                   Me.TradableInstrument.RawInstrumentName,
                                                   order.Direction.ToString,
                                                   If(order.TypeOfEntryDetails = ExitType.None, order.TypeOfEntry.ToString, String.Format("{0} {1}", order.TypeOfEntryDetails.ToString, order.TypeOfEntry.ToString)),
                                                   order.IterationNumber,
                                                   order.Quantity,
                                                   Math.Round(order.ATRConsumed, 2),
                                                   order.LossToRecover,
                                                   order.EntrySignalDate.ToString("dd-MMM-yyyy"),
                                                   order.SpotPrice,
                                                   Math.Round(order.SpotATR, 2),
                                                   order.TradingSymbol,
                                                   order.EntryPrice,
                                                   order.EntryTime.ToString("dd-MMM-yyyy HH:mm:ss"),
                                                   order.EntryPrice * order.Quantity,
                                                   order.AttemptedEntryPrice,
                                                   targetPrice,
                                                   order.PotentialTarget)

                Await SendNotificationAsync(msg).ConfigureAwait(False)
            End If
        Catch ex As Exception
            logger.Warn("Telegram Error: {0}", ex.ToString)
        End Try
    End Function

    Private Async Function SendExitOrderNotificationAsync(ByVal order As Trade, ByVal optionInstrument As NFOStrategyInstrument) As Task
        Try
            Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
            _cts.Token.ThrowIfCancellationRequested()
            If order IsNot Nothing Then
                Dim msg As String = String.Format("{1}: Exit {0}Signal Direction:{2}{0}Exit Type:{3}{0}Entry Price:{4}{0}Entry Time:{5}{0}Exit Price:{6}{0}Exit Time:{7}{0}Quantity:{8}{0}Signal PL:{9}{0}Attempted Exit Price:{10}{0}Expected Exit PL:{11}",
                                                   vbNewLine,
                                                   Me.TradableInstrument.RawInstrumentName,
                                                   order.Direction.ToString,
                                                   order.TypeOfExit.ToString,
                                                   order.EntryPrice,
                                                   order.EntryTime.ToString("dd-MMM-yyyy HH:mm:ss"),
                                                   order.ExitPrice,
                                                   order.ExitTime.ToString("dd-MMM-yyyy HH:mm:ss"),
                                                   order.Quantity,
                                                   GetOverallSignalPL(order, optionInstrument, order.AttemptedExitPrice),
                                                   order.AttemptedExitPrice,
                                                   order.PotentialTarget)

                Await SendNotificationAsync(msg).ConfigureAwait(False)
            End If
        Catch ex As Exception
            logger.Warn("Telegram Error: {0}", ex.ToString)
        End Try
    End Function

    Private Async Function SendNotificationAsync(ByVal message As String) As Task
        Try
            Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
            _cts.Token.ThrowIfCancellationRequested()
            If message IsNot Nothing Then
                If Me.ParentStrategy.ParentController.UserInputs.TelegramAPIKey IsNot Nothing AndAlso
                    Me.ParentStrategy.ParentController.UserInputs.TelegramAPIKey.Trim <> "" AndAlso
                    Me.ParentStrategy.ParentController.UserInputs.TelegramChatID IsNot Nothing AndAlso
                    Me.ParentStrategy.ParentController.UserInputs.TelegramChatID.Trim <> "" Then
                    Using tSender As New Utilities.Notification.Telegram(Me.ParentStrategy.ParentController.UserInputs.TelegramAPIKey.Trim, Me.ParentStrategy.ParentController.UserInputs.TelegramChatID.Trim, _cts)
                        Dim encodedString As String = Utilities.Strings.UrlEncodeString(message)
                        Await tSender.SendMessageGetAsync(encodedString).ConfigureAwait(False)
                    End Using
                End If
            End If
        Catch ex As Exception
            logger.Warn("Telegram Error: {0}", ex.ToString)
        End Try
    End Function
#End Region

#Region "Not Required For This Strategy"
    Public Overrides Function PopulateChartAndIndicatorsAsync(candleCreator As Chart, currentCandle As OHLCPayload) As Task
        Throw New NotImplementedException
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