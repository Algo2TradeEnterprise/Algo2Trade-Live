﻿Imports System.Threading
Imports Algo2TradeCore.Adapter
Imports Algo2TradeCore.Entities
Imports Algo2TradeCore.Strategies
Imports Utilities.Numbers
Imports NLog
Imports Algo2TradeCore.Entities.Indicators

Public Class MomentumReversalStrategyInstrument
    Inherits StrategyInstrument
    Implements IDisposable

#Region "Logging and Status Progress"
    Public Shared Shadows logger As Logger = LogManager.GetCurrentClassLogger
#End Region

    Private _lastPrevPayloadPlaceOrder As String = ""
    Private ReadOnly _dummyATRConsumer As ATRConsumer

    Public Sub New(ByVal associatedInstrument As IInstrument,
                   ByVal associatedParentStrategy As Strategy,
                   ByVal isPairInstrumnet As Boolean,
                   ByVal canceller As CancellationTokenSource)
        MyBase.New(associatedInstrument, associatedParentStrategy, isPairInstrumnet, canceller)
        Select Case Me.ParentStrategy.ParentController.BrokerSource
            Case APISource.Zerodha
                _APIAdapter = New ZerodhaAdapter(ParentStrategy.ParentController, _cts)
            Case APISource.Upstox
                Throw New NotImplementedException
            Case APISource.None
                Throw New NotImplementedException
        End Select
        AddHandler _APIAdapter.Heartbeat, AddressOf OnHeartbeat
        AddHandler _APIAdapter.WaitingFor, AddressOf OnWaitingFor
        AddHandler _APIAdapter.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
        AddHandler _APIAdapter.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete
        RawPayloadDependentConsumers = New List(Of IPayloadConsumer)
        If Me.ParentStrategy.IsStrategyCandleStickBased Then
            If Me.ParentStrategy.UserSettings.SignalTimeFrame > 0 Then
                Dim chartConsumer As PayloadToChartConsumer = New PayloadToChartConsumer(Me.ParentStrategy.UserSettings.SignalTimeFrame)
                chartConsumer.OnwardLevelConsumers = New List(Of IPayloadConsumer) From
                {New ATRConsumer(chartConsumer, CType(Me.ParentStrategy.UserSettings, MomentumReversalUserInputs).ATRPeriod)}
                RawPayloadDependentConsumers.Add(chartConsumer)
                _dummyATRConsumer = New ATRConsumer(chartConsumer, CType(Me.ParentStrategy.UserSettings, MomentumReversalUserInputs).ATRPeriod)
            Else
                Throw New ApplicationException(String.Format("Signal Timeframe is 0 or Nothing, does not adhere to the strategy:{0}", Me.ParentStrategy.ToString))
            End If
        End If
    End Sub
    Public Overrides Function MonitorAsync(ByVal command As ExecuteCommands, ByVal data As Object) As Task
        Throw New NotImplementedException()
    End Function
    Public Overrides Async Function MonitorAsync() As Task
        Try
            Dim userSettings As MomentumReversalUserInputs = Me.ParentStrategy.UserSettings
            Dim historicalStopped As Boolean = False
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
                If Me.TradableInstrument.IsHistoricalCompleted AndAlso
                    Not historicalStopped AndAlso Now >= userSettings.TradeStartTime.AddSeconds(10) Then
                    Await Me.ParentStrategy.ParentController.CloseFetcherIfConnectedAsync(True).ConfigureAwait(False)
                    historicalStopped = True
                End If
                'Place Order block start
                Dim placeOrderTriggers As List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)) = Await IsTriggerReceivedForPlaceOrderAsync(False).ConfigureAwait(False)
                If placeOrderTriggers IsNot Nothing AndAlso placeOrderTriggers.Count > 0 Then
                    If placeOrderTriggers.FirstOrDefault.Item2.OrderType = IOrder.TypeOfOrder.Market Then
                        Await ExecuteCommandAsync(ExecuteCommands.PlaceRegularMarketMISOrder, Nothing).ConfigureAwait(False)
                    ElseIf placeOrderTriggers.FirstOrDefault.Item2.OrderType = IOrder.TypeOfOrder.SL Then
                        Await ExecuteCommandAsync(ExecuteCommands.PlaceRegularSLMMISOrder, Nothing).ConfigureAwait(False)
                    ElseIf placeOrderTriggers.FirstOrDefault.Item2.OrderType = IOrder.TypeOfOrder.Limit Then
                        Await ExecuteCommandAsync(ExecuteCommands.PlaceRegularLimitMISOrder, Nothing).ConfigureAwait(False)
                    End If
                End If
                'Place Order block end
                _cts.Token.ThrowIfCancellationRequested()
                'Modify Order block start
                Dim modifyStoplossOrderTrigger As List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)) = Await IsTriggerReceivedForModifyStoplossOrderAsync(False).ConfigureAwait(False)
                If modifyStoplossOrderTrigger IsNot Nothing AndAlso modifyStoplossOrderTrigger.Count > 0 Then
                    Await ExecuteCommandAsync(ExecuteCommands.ModifyStoplossOrder, Nothing).ConfigureAwait(False)
                End If
                'Modify Order block end
                _cts.Token.ThrowIfCancellationRequested()
                'Exit Order block start
                Dim exitOrderTrigger As List(Of Tuple(Of ExecuteCommandAction, IOrder, String)) = Await IsTriggerReceivedForExitOrderAsync(False).ConfigureAwait(False)
                If exitOrderTrigger IsNot Nothing AndAlso exitOrderTrigger.Count > 0 Then
                    Await ExecuteCommandAsync(ExecuteCommands.CancelRegularOrder, Nothing).ConfigureAwait(False)
                End If
                'Exit Order block end
                _cts.Token.ThrowIfCancellationRequested()
                Await Task.Delay(1000, _cts.Token).ConfigureAwait(False)
            End While
        Catch ex As Exception
            'To log exceptions getting created from this function as the bubble up of the exception
            'will anyways happen to Strategy.MonitorAsync but it will not be shown until all tasks exit
            logger.Error("Strategy Instrument:{0}, error:{1}", Me.ToString, ex.ToString)
            Throw ex
        End Try
    End Function
    Protected Overrides Async Function IsTriggerReceivedForPlaceOrderAsync(ByVal forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)))
        Dim ret As List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        Dim userSettings As MomentumReversalUserInputs = Me.ParentStrategy.UserSettings
        Dim runningCandlePayload As OHLCPayload = GetXMinuteCurrentCandle(userSettings.SignalTimeFrame)
        Dim atrConsumer As ATRConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummyATRConsumer)
        Dim currentTick As ITick = Me.TradableInstrument.LastTick
        Dim currentTime As Date = Now()
        Dim allActiveOrders As List(Of IOrder) = GetAllActiveOrders(IOrder.TypeOfTransaction.None)

        Try
            If runningCandlePayload IsNot Nothing AndAlso runningCandlePayload.PreviousPayload IsNot Nothing AndAlso
                (Not runningCandlePayload.PreviousPayload.ToString = _lastPrevPayloadPlaceOrder OrElse forcePrint) Then
                _lastPrevPayloadPlaceOrder = runningCandlePayload.PreviousPayload.ToString
                Dim signal As Tuple(Of Boolean, IOrder.TypeOfTransaction, OHLCPayload) = GetSignalCandle()
                Dim atr As Decimal = Decimal.MinValue
                If signal IsNot Nothing Then
                    logger.Debug("PlaceOrder-> Potential Signal Candle is:{0}. Will check rest parameters.", signal.Item3.ToString)
                    atr = Math.Round(CType(atrConsumer.ConsumerPayloads(signal.Item3.SnapshotDateTime), ATRConsumer.ATRPayload).ATR.Value, 3)
                End If
                logger.Debug("PlaceOrder-> Rest all parameters: Trade Start Time:{0}, Last Trade Entry Time:{1}, RunningCandlePayloadSnapshotDateTime:{2}, PayloadGeneratedBy:{3}, IsHistoricalCompleted:{4}, Is Active Instrument:{5}, Number Of Trade:{6}, Signal Candle Time:{7}, Signal Candle Color:{8}, Signal Direction:{9}, ATR:{10}, Day Open:{11}, Active trade count:{12}, Current Time:{13}, Current LTP:{14}, TradingSymbol:{15}",
                            userSettings.TradeStartTime.ToString,
                            userSettings.LastTradeEntryTime.ToString,
                            runningCandlePayload.SnapshotDateTime,
                            runningCandlePayload.PayloadGeneratedBy.ToString,
                            Me.TradableInstrument.IsHistoricalCompleted,
                            IsActiveInstrument(),
                            GetTotalExecutedOrders(),
                            If(signal IsNot Nothing, signal.Item3.SnapshotDateTime, "Nothing"),
                            If(signal IsNot Nothing, signal.Item3.CandleColor.Name, "Nothing"),
                            If(signal IsNot Nothing, signal.Item2, "Nothing"),
                            If(atr <> Decimal.MinValue, atr, "Nothing"),
                            currentTick.Open,
                            If(allActiveOrders IsNot Nothing, allActiveOrders.Count, 0),
                            currentTime.ToString,
                            currentTick.LastPrice,
                            Me.TradableInstrument.TradingSymbol)
            End If
        Catch ex As Exception
            logger.Error(ex.ToString)
        End Try

        Dim parameters As PlaceOrderParameters = Nothing
        If currentTime >= userSettings.TradeStartTime AndAlso currentTime <= userSettings.LastTradeEntryTime AndAlso
            runningCandlePayload IsNot Nothing AndAlso runningCandlePayload.SnapshotDateTime >= userSettings.TradeStartTime AndAlso
            runningCandlePayload.PreviousPayload IsNot Nothing AndAlso Me.TradableInstrument.IsHistoricalCompleted AndAlso
            Not Me.StrategyExitAllTriggerd Then
            If Not IsActiveInstrument() AndAlso GetTotalExecutedOrders() < 1 Then
                Dim signal As Tuple(Of Boolean, IOrder.TypeOfTransaction, OHLCPayload) = GetSignalCandle()
                If signal IsNot Nothing AndAlso signal.Item1 AndAlso atrConsumer.ConsumerPayloads IsNot Nothing AndAlso
                    atrConsumer.ConsumerPayloads.ContainsKey(signal.Item3.SnapshotDateTime) Then
                    If signal.Item2 = IOrder.TypeOfTransaction.Buy Then
                        parameters = New PlaceOrderParameters(runningCandlePayload.PreviousPayload) With
                                    {.EntryDirection = IOrder.TypeOfTransaction.Buy,
                                     .Quantity = userSettings.InstrumentsData(Me.TradableInstrument.TradingSymbol).Quantity,
                                     .OrderType = IOrder.TypeOfOrder.Market}
                    ElseIf signal.Item2 = IOrder.TypeOfTransaction.Sell Then
                        parameters = New PlaceOrderParameters(runningCandlePayload.PreviousPayload) With
                                    {.EntryDirection = IOrder.TypeOfTransaction.Sell,
                                     .Quantity = userSettings.InstrumentsData(Me.TradableInstrument.TradingSymbol).Quantity,
                                     .OrderType = IOrder.TypeOfOrder.Market}
                    End If
                End If
            ElseIf IsActiveInstrument() AndAlso GetTotalExecutedOrders() < 2 Then
                If allActiveOrders IsNot Nothing AndAlso allActiveOrders.Count > 0 AndAlso allActiveOrders.Count < 3 Then
                    Dim activeOrder As IOrder = allActiveOrders.Find(Function(x)
                                                                         Return x.Status = IOrder.TypeOfStatus.Complete
                                                                     End Function)
                    If activeOrder IsNot Nothing AndAlso OrderDetails.ContainsKey(activeOrder.OrderIdentifier) Then
                        Dim activeBussinessOrder As IBusinessOrder = OrderDetails(activeOrder.OrderIdentifier)
                        If activeBussinessOrder IsNot Nothing AndAlso activeBussinessOrder.ParentOrder IsNot Nothing Then
                            Dim firstCandle As OHLCPayload = GetFirstCandleOfTheDay()
                            If firstCandle IsNot Nothing AndAlso atrConsumer.ConsumerPayloads IsNot Nothing AndAlso
                            atrConsumer.ConsumerPayloads.ContainsKey(firstCandle.SnapshotDateTime) Then
                                Dim atr As Decimal = Math.Round(CType(atrConsumer.ConsumerPayloads(firstCandle.SnapshotDateTime), ATRConsumer.ATRPayload).ATR.Value, 3)
                                If allActiveOrders.Count = 1 Then
                                    Dim minSL As Decimal = firstCandle.ClosePrice.Value * userSettings.MinStoplossPercentage / 100
                                    Dim stoplossPoint As Decimal = ConvertFloorCeling(Math.Max(atr, minSL), Me.TradableInstrument.TickSize, RoundOfType.Floor)
                                    If activeBussinessOrder.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Buy Then
                                        Dim triggerPrice As Decimal = firstCandle.ClosePrice.Value - stoplossPoint
                                        parameters = New PlaceOrderParameters(runningCandlePayload.PreviousPayload) With
                                                    {.EntryDirection = IOrder.TypeOfTransaction.Sell,
                                                        .TriggerPrice = triggerPrice,
                                                        .Quantity = userSettings.InstrumentsData(Me.TradableInstrument.TradingSymbol).Quantity,
                                                        .OrderType = IOrder.TypeOfOrder.SL}
                                    ElseIf activeBussinessOrder.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Sell Then
                                        Dim triggerPrice As Decimal = firstCandle.ClosePrice.Value + stoplossPoint
                                        parameters = New PlaceOrderParameters(runningCandlePayload.PreviousPayload) With
                                                    {.EntryDirection = IOrder.TypeOfTransaction.Buy,
                                                     .TriggerPrice = triggerPrice,
                                                     .Quantity = userSettings.InstrumentsData(Me.TradableInstrument.TradingSymbol).Quantity,
                                                     .OrderType = IOrder.TypeOfOrder.SL}
                                    End If
                                ElseIf allActiveOrders.Count = 2 Then
                                    Dim minTgt As Decimal = firstCandle.ClosePrice.Value * userSettings.MinTargetPercentage / 100
                                    Dim targetPoint As Decimal = ConvertFloorCeling(Math.Max(atr, minTgt), Me.TradableInstrument.TickSize, RoundOfType.Celing)
                                    If activeBussinessOrder.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Buy Then
                                        Dim price As Decimal = firstCandle.ClosePrice.Value + targetPoint
                                        parameters = New PlaceOrderParameters(runningCandlePayload.PreviousPayload) With
                                                    {.EntryDirection = IOrder.TypeOfTransaction.Sell,
                                                        .Price = price,
                                                        .Quantity = userSettings.InstrumentsData(Me.TradableInstrument.TradingSymbol).Quantity,
                                                        .OrderType = IOrder.TypeOfOrder.Limit}
                                    ElseIf activeBussinessOrder.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Sell Then
                                        Dim price As Decimal = firstCandle.ClosePrice.Value - targetPoint
                                        parameters = New PlaceOrderParameters(runningCandlePayload.PreviousPayload) With
                                                    {.EntryDirection = IOrder.TypeOfTransaction.Buy,
                                                        .Price = price,
                                                        .Quantity = userSettings.InstrumentsData(Me.TradableInstrument.TradingSymbol).Quantity,
                                                        .OrderType = IOrder.TypeOfOrder.Limit}
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
            End If
        End If

        'Below portion have to be done in every place order trigger
        If parameters IsNot Nothing Then
            Try
                If forcePrint Then logger.Debug("***** Place Order Parameter ***** {0}, {1}", parameters.ToString, Me.TradableInstrument.TradingSymbol)
            Catch ex As Exception
                logger.Error(ex.ToString)
            End Try

            Dim currentSignalActivities As IEnumerable(Of ActivityDashboard) = Me.ParentStrategy.SignalManager.GetSignalActivities(parameters.SignalCandle.SnapshotDateTime, Me.TradableInstrument.InstrumentIdentifier)
            If currentSignalActivities IsNot Nothing AndAlso currentSignalActivities.Count > 0 Then
                Dim placedActivities As IEnumerable(Of ActivityDashboard) = currentSignalActivities.Where(Function(x)
                                                                                                              Return x.EntryActivity.RequestRemarks = parameters.ToString
                                                                                                          End Function)
                'Dim placedActivities As IEnumerable(Of ActivityDashboard) = currentSignalActivities
                If placedActivities IsNot Nothing AndAlso placedActivities.Count > 0 Then
                    Dim lastPlacedActivity As ActivityDashboard = placedActivities.OrderBy(Function(x)
                                                                                               Return x.EntryActivity.RequestTime
                                                                                           End Function).LastOrDefault
                    If lastPlacedActivity.EntryActivity.RequestStatus = ActivityDashboard.SignalStatusType.Discarded AndAlso
                            lastPlacedActivity.EntryActivity.LastException IsNot Nothing AndAlso
                            lastPlacedActivity.EntryActivity.LastException.Message.ToUpper.Contains("TIME") Then
                        If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                        ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.WaitAndTake, parameters, parameters.ToString))
                    ElseIf lastPlacedActivity.EntryActivity.RequestStatus = ActivityDashboard.SignalStatusType.Handled Then
                        If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                        ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.DonotTake, parameters, parameters.ToString))
                    ElseIf lastPlacedActivity.EntryActivity.RequestStatus = ActivityDashboard.SignalStatusType.Activated Then
                        If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                        ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.DonotTake, parameters, parameters.ToString))
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
    Protected Overrides Async Function IsTriggerReceivedForModifyStoplossOrderAsync(ByVal forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)))
        Dim ret As List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        Dim userSettings As MomentumReversalUserInputs = Me.ParentStrategy.UserSettings
        Dim currentTick As ITick = Me.TradableInstrument.LastTick
        Dim currentTime As Date = Now()
        If OrderDetails IsNot Nothing AndAlso OrderDetails.Count > 0 Then
            Dim activeTrades As List(Of IOrder) = GetAllActiveOrders(IOrder.TypeOfTransaction.None)
            Dim slOrder As IOrder = activeTrades.Find(Function(x)
                                                          Return x.Status = IOrder.TypeOfStatus.TriggerPending
                                                      End Function)
            If slOrder IsNot Nothing Then
                Dim parentOrder As IOrder = activeTrades.Find(Function(x)
                                                                  Return x.Status = IOrder.TypeOfStatus.Complete
                                                              End Function)
                If parentOrder IsNot Nothing Then
                    Dim entryPrice As Decimal = parentOrder.AveragePrice
                    Dim potentialGain As Decimal = entryPrice * userSettings.CostToCostMovementPercentage / 100
                    If Not slOrder.Status = IOrder.TypeOfStatus.Complete AndAlso
                        Not slOrder.Status = IOrder.TypeOfStatus.Cancelled AndAlso
                        Not slOrder.Status = IOrder.TypeOfStatus.Rejected Then
                        Dim triggerPrice As Decimal = Decimal.MinValue
                        Dim reason As String = Nothing
                        If parentOrder.TransactionType = IOrder.TypeOfTransaction.Buy Then
                            Dim gain As Decimal = currentTick.LastPrice - entryPrice
                            If gain >= potentialGain Then
                                Dim brkevnPoint As Decimal = GetBreakevenPoint(entryPrice, parentOrder.Quantity, IOrder.TypeOfTransaction.Buy)
                                If entryPrice + brkevnPoint > slOrder.TriggerPrice Then
                                    triggerPrice = entryPrice + brkevnPoint
                                    reason = "Cost to cost movement"
                                End If
                            ElseIf currentTime >= userSettings.EODExitTime Then
                                triggerPrice = currentTick.LastPrice
                                reason = "EOD Exit"
                            End If
                        ElseIf parentOrder.TransactionType = IOrder.TypeOfTransaction.Sell Then
                            Dim gain As Decimal = entryPrice - currentTick.LastPrice
                            If gain >= potentialGain Then
                                Dim brkevnPoint As Decimal = GetBreakevenPoint(entryPrice, parentOrder.Quantity, IOrder.TypeOfTransaction.Sell)
                                If entryPrice - brkevnPoint < slOrder.TriggerPrice Then
                                    triggerPrice = entryPrice - brkevnPoint
                                    reason = "Cost to cost movement"
                                ElseIf currentTime >= userSettings.EODExitTime Then
                                    triggerPrice = currentTick.LastPrice
                                    reason = "EOD Exit"
                                End If
                            End If
                        End If
                        If triggerPrice <> Decimal.MinValue AndAlso triggerPrice <> slOrder.TriggerPrice Then
                            'Below portion have to be done in every modify stoploss order trigger
                            Dim currentSignalActivities As ActivityDashboard = Me.ParentStrategy.SignalManager.GetSignalActivities(slOrder.Tag)
                            If currentSignalActivities IsNot Nothing Then
                                If currentSignalActivities.StoplossModifyActivity.RequestStatus = ActivityDashboard.SignalStatusType.Handled OrElse
                                    currentSignalActivities.StoplossModifyActivity.RequestStatus = ActivityDashboard.SignalStatusType.Activated OrElse
                                    currentSignalActivities.StoplossModifyActivity.RequestStatus = ActivityDashboard.SignalStatusType.Completed Then
                                    If Val(currentSignalActivities.StoplossModifyActivity.Supporting) = triggerPrice Then
                                        Return ret
                                        Exit Function
                                    End If
                                End If
                            End If
                            If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String))
                            ret.Add(New Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)(ExecuteCommandAction.Take, slOrder, triggerPrice, reason))
                        End If
                    End If
                End If
            End If
        End If
        If forcePrint AndAlso ret IsNot Nothing AndAlso ret.Count > 0 Then
            Try
                For Each runningOrder In ret
                    logger.Debug("***** Modify Stoploss ***** Order ID:{0}, Reason:{1}, {2}", runningOrder.Item2.OrderIdentifier, runningOrder.Item4, Me.TradableInstrument.TradingSymbol)
                Next
            Catch ex As Exception
                logger.Error(ex.ToString)
            End Try
        End If
        Return ret
    End Function
    Protected Overrides Function IsTriggerReceivedForModifyTargetOrderAsync(forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)))
        Throw New NotImplementedException()
    End Function
    Protected Overrides Async Function IsTriggerReceivedForExitOrderAsync(ByVal forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, String)))
        Dim ret As List(Of Tuple(Of ExecuteCommandAction, IOrder, String)) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        Dim allActiveOrders As List(Of IOrder) = GetAllActiveOrders(IOrder.TypeOfTransaction.None)
        If allActiveOrders IsNot Nothing AndAlso allActiveOrders.Count > 0 Then
            Dim parentOrders As List(Of IOrder) = allActiveOrders.FindAll(Function(x)
                                                                              Return x.Status = IOrder.TypeOfStatus.Complete
                                                                          End Function)
            If parentOrders IsNot Nothing AndAlso parentOrders.Count >= 2 Then
                Dim activeOrders As List(Of IOrder) = allActiveOrders.FindAll(Function(x)
                                                                                  Return x.Status = IOrder.TypeOfStatus.TriggerPending OrElse
                                                                                  x.Status = IOrder.TypeOfStatus.Open
                                                                              End Function)
                For Each orders In activeOrders
                    'Below portion have to be done in every cancel order trigger
                    Dim currentSignalActivities As ActivityDashboard = Me.ParentStrategy.SignalManager.GetSignalActivities(orders.Tag)
                    If currentSignalActivities IsNot Nothing Then
                        If currentSignalActivities.CancelActivity.RequestStatus = ActivityDashboard.SignalStatusType.Handled OrElse
                            currentSignalActivities.CancelActivity.RequestStatus = ActivityDashboard.SignalStatusType.Activated OrElse
                            currentSignalActivities.CancelActivity.RequestStatus = ActivityDashboard.SignalStatusType.Completed Then
                            Continue For
                        End If
                    End If
                    If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, IOrder, String))
                    ret.Add(New Tuple(Of ExecuteCommandAction, IOrder, String)(ExecuteCommandAction.Take, orders, "SL/Target Hit"))
                Next
            End If
        End If
        Return ret
    End Function
    Protected Overrides Function IsTriggerReceivedForPlaceOrderAsync(forcePrint As Boolean, data As Object) As Task(Of List(Of Tuple(Of ExecuteCommandAction, StrategyInstrument, PlaceOrderParameters, String)))
        Throw New NotImplementedException()
    End Function
    Protected Overrides Function IsTriggerReceivedForExitOrderAsync(forcePrint As Boolean, data As Object) As Task(Of List(Of Tuple(Of ExecuteCommandAction, StrategyInstrument, IOrder, String)))
        Throw New NotImplementedException()
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

    Private Function GetSignalCandle() As Tuple(Of Boolean, IOrder.TypeOfTransaction, OHLCPayload)
        Dim ret As Tuple(Of Boolean, IOrder.TypeOfTransaction, OHLCPayload) = Nothing
        Dim firstCandle As OHLCPayload = GetFirstCandleOfTheDay()
        If firstCandle IsNot Nothing Then
            If Me.TradableInstrument.LastTick.Open < firstCandle.ClosePrice.Value Then
                ret = New Tuple(Of Boolean, IOrder.TypeOfTransaction, OHLCPayload)(True, IOrder.TypeOfTransaction.Buy, firstCandle)
            ElseIf Me.TradableInstrument.LastTick.Open > firstCandle.ClosePrice.Value Then
                ret = New Tuple(Of Boolean, IOrder.TypeOfTransaction, OHLCPayload)(True, IOrder.TypeOfTransaction.Sell, firstCandle)
            End If
        End If
        Return ret
    End Function

    Private Function GetFirstCandleOfTheDay() As OHLCPayload
        Dim ret As OHLCPayload = Nothing
        'Dim runningCandlePayload As OHLCPayload = GetXMinuteCurrentCandle(Me.ParentStrategy.UserSettings.SignalTimeFrame)
        'If runningCandlePayload IsNot Nothing AndAlso runningCandlePayload.PreviousPayload IsNot Nothing Then
        '    ret = runningCandlePayload.PreviousPayload
        'End If
        Dim timeframe As Integer = Me.ParentStrategy.UserSettings.SignalTimeFrame
        If Me.RawPayloadDependentConsumers IsNot Nothing AndAlso Me.RawPayloadDependentConsumers.Count > 0 Then
            Dim XMinutePayloadConsumer As PayloadToChartConsumer = RawPayloadDependentConsumers.Find(Function(x)
                                                                                                         If x.GetType Is GetType(PayloadToChartConsumer) Then
                                                                                                             Return CType(x, PayloadToChartConsumer).Timeframe = timeframe
                                                                                                         Else
                                                                                                             Return Nothing
                                                                                                         End If
                                                                                                     End Function)

            If XMinutePayloadConsumer IsNot Nothing AndAlso
                    XMinutePayloadConsumer.ConsumerPayloads IsNot Nothing AndAlso XMinutePayloadConsumer.ConsumerPayloads.Count > 0 Then
                Dim existingPayloads As IEnumerable(Of KeyValuePair(Of Date, IPayload)) =
                    XMinutePayloadConsumer.ConsumerPayloads.Where(Function(y)
                                                                      Return Utilities.Time.IsDateTimeEqualTillMinutes(y.Key.Date, Now.Date)
                                                                  End Function)

                If existingPayloads IsNot Nothing AndAlso existingPayloads.Count > 0 Then
                    ret = existingPayloads.OrderBy(Function(x)
                                                       Return x.Key
                                                   End Function).FirstOrDefault.Value
                End If
            End If
        End If
        Return ret
    End Function

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