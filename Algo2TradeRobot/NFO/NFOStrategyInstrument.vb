﻿Imports System.Threading
Imports Algo2TradeCore.Adapter
Imports Algo2TradeCore.Entities
Imports Algo2TradeCore.Strategies
Imports Utilities.Numbers
Imports NLog
Imports Algo2TradeCore.Entities.Indicators

Public Class NFOStrategyInstrument
    Inherits StrategyInstrument
    Implements IDisposable

#Region "Logging and Status Progress"
    Public Shared Shadows logger As Logger = LogManager.GetCurrentClassLogger
#End Region

    Private _lastPrevPayloadPlaceOrder As String = ""
    Private ReadOnly _dummyHKConsumer As HeikinAshiConsumer
    Private ReadOnly _slab As Decimal

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
        RawPayloadDependentConsumers = New List(Of IPayloadConsumer)
        If Me.ParentStrategy.IsStrategyCandleStickBased Then
            If Me.ParentStrategy.UserSettings.SignalTimeFrame > 0 Then
                Dim chartConsumer As PayloadToChartConsumer = New PayloadToChartConsumer(Me.ParentStrategy.UserSettings.SignalTimeFrame)
                chartConsumer.OnwardLevelConsumers = New List(Of IPayloadConsumer) From
                {New HeikinAshiConsumer(chartConsumer)}
                RawPayloadDependentConsumers.Add(chartConsumer)
                _dummyHKConsumer = New HeikinAshiConsumer(chartConsumer)
            Else
                Throw New ApplicationException(String.Format("Signal Timeframe is 0 or Nothing, does not adhere to the strategy:{0}", Me.ParentStrategy.ToString))
            End If
        End If

        _slab = CType(Me.ParentStrategy.UserSettings, NFOUserInputs).InstrumentsData(Me.TradableInstrument.TradingSymbol).Slab
    End Sub
    Public Overrides Function MonitorAsync(ByVal command As ExecuteCommands, ByVal data As Object) As Task
        Throw New NotImplementedException()
    End Function
    Public Overrides Async Function MonitorAsync() As Task
        Try
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

                'Place Order block start
                Dim placeOrderTriggers As List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)) = Await IsTriggerReceivedForPlaceOrderAsync(False).ConfigureAwait(False)
                If placeOrderTriggers IsNot Nothing AndAlso placeOrderTriggers.Count > 0 AndAlso
                    placeOrderTriggers.FirstOrDefault.Item1 = ExecuteCommandAction.Take Then
                    Await ExecuteCommandAsync(ExecuteCommands.PlaceBOSLMISOrder, Nothing).ConfigureAwait(False)
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
                    Await ExecuteCommandAsync(ExecuteCommands.CancelBOOrder, Nothing).ConfigureAwait(False)
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
        Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
        Dim runningCandlePayload As OHLCPayload = GetXMinuteCurrentCandle(userSettings.SignalTimeFrame)
        Dim hkConsumer As HeikinAshiConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummyHKConsumer)
        Dim currentTick As ITick = Me.TradableInstrument.LastTick
        Dim currentTime As Date = Now()
        Dim lastExecutedOrder As IBusinessOrder = GetLastExecutedOrder()
        Dim buyActiveTrades = GetAllActiveOrders(IOrder.TypeOfTransaction.Buy)
        Dim sellActiveTrades = GetAllActiveOrders(IOrder.TypeOfTransaction.Sell)

        'Try
        '    If runningLFCandlePayload IsNot Nothing AndAlso runningLFCandlePayload.PreviousPayload IsNot Nothing AndAlso
        '        runningHFCandlePayload IsNot Nothing AndAlso runningHFCandlePayload.PreviousPayload IsNot Nothing AndAlso
        '        Me.TradableInstrument.IsHistoricalCompleted Then
        '        If Not runningLFCandlePayload.PreviousPayload.ToString = _lastPrevPayloadPlaceOrder Then
        '            _lastPrevPayloadPlaceOrder = runningLFCandlePayload.PreviousPayload.ToString
        '            logger.Debug("PlaceOrder-> Potential LF Signal Candle is:{0}. Will check rest parameters.", runningLFCandlePayload.PreviousPayload.ToString)
        '            logger.Debug("PlaceOrder-> Potential HF Signal Candle is:{0}. Will check rest parameters.", runningHFCandlePayload.PreviousPayload.ToString)
        '            logger.Debug("PlaceOrder-> Rest all parameters: Running LF Candle:{0}, Running HF Candle:{1}, LF PayloadGeneratedBy:{2}, HF PayloadGeneratedBy:{3}, IsHistoricalCompleted:{4}, IsFirstTimeInformationCollected:{5}, LF {6}, HF {7}, IsActiveInstrument:{8}, Current Time:{9}, Current Tick:{10}, TradingSymbol:{11}",
        '                        runningLFCandlePayload.SnapshotDateTime.ToString("dd-MM-yyyy HH:mm:ss"),
        '                        runningHFCandlePayload.SnapshotDateTime.ToString("dd-MM-yyyy HH:mm:ss"),
        '                        runningLFCandlePayload.PayloadGeneratedBy.ToString,
        '                        runningHFCandlePayload.PayloadGeneratedBy.ToString,
        '                        Me.TradableInstrument.IsHistoricalCompleted,
        '                        Me.ParentStrategy.IsFirstTimeInformationCollected,
        '                        supertrendConsumerLF.ConsumerPayloads(runningLFCandlePayload.PreviousPayload.SnapshotDateTime).ToString,
        '                        supertrendConsumerHF.ConsumerPayloads(runningHFCandlePayload.PreviousPayload.SnapshotDateTime).ToString,
        '                        IsActiveInstrument(),
        '                        currentTime.ToString,
        '                        currentTick.LastPrice,
        '                        Me.TradableInstrument.TradingSymbol)
        '        End If
        '    End If
        'Catch ex As Exception
        '    logger.Error(ex)
        'End Try

        Dim parameters As PlaceOrderParameters = Nothing
        If currentTime >= userSettings.TradeStartTime AndAlso currentTime <= userSettings.LastTradeEntryTime AndAlso currentTime <= userSettings.EODExitTime AndAlso
            runningCandlePayload IsNot Nothing AndAlso runningCandlePayload.SnapshotDateTime >= userSettings.TradeStartTime AndAlso
            runningCandlePayload.PayloadGeneratedBy = OHLCPayload.PayloadSource.CalculatedTick AndAlso runningCandlePayload.PreviousPayload IsNot Nothing AndAlso
            Me.TradableInstrument.IsHistoricalCompleted AndAlso GetTotalExecutedOrders() < userSettings.NumberOfTradePerStock AndAlso
            Me.ParentStrategy.GetTotalPLAfterBrokerage() > userSettings.OverallMaxLossPerDay AndAlso
            Me.ParentStrategy.GetTotalPLAfterBrokerage() < userSettings.OverallMaxProfitPerDay AndAlso Not Me.StrategyExitAllTriggerd AndAlso
            hkConsumer.ConsumerPayloads IsNot Nothing AndAlso hkConsumer.ConsumerPayloads.Count > 0 AndAlso
            hkConsumer.ConsumerPayloads.ContainsKey(runningCandlePayload.PreviousPayload.SnapshotDateTime) Then
            Dim hkCandle As HeikinAshiConsumer.HeikinAshiPayload = hkConsumer.ConsumerPayloads(runningCandlePayload.PreviousPayload.SnapshotDateTime)
            Dim signal As Tuple(Of Boolean, Decimal, HeikinAshiConsumer.HeikinAshiPayload, IOrder.TypeOfTransaction) = GetSignalCandle(hkCandle)
            If signal IsNot Nothing AndAlso signal.Item1 Then
                Dim signalCandle As OHLCPayload = Nothing
                If lastExecutedOrder Is Nothing Then
                    signalCandle = runningCandlePayload.PreviousPayload
                Else
                    Dim lastOrderSignalCandle As OHLCPayload = GetSignalCandleOfAnOrder(lastExecutedOrder.ParentOrderIdentifier, userSettings.SignalTimeFrame)
                    If lastOrderSignalCandle IsNot Nothing AndAlso lastOrderSignalCandle.SnapshotDateTime <> runningCandlePayload.PreviousPayload.SnapshotDateTime Then
                        signalCandle = runningCandlePayload.PreviousPayload
                    End If
                End If
                If signalCandle IsNot Nothing Then
                    Dim quantity As Decimal = Me.TradableInstrument.LotSize
                    Dim buffer As Decimal = CalculateBuffer(signal.Item2, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                    If Me.TradableInstrument.ExchangeDetails.ExchangeType = TypeOfExchage.MCX Then
                        buffer = 0
                    End If
                    Dim slPoint As Decimal = _slab
                    Dim targetPoint As Decimal = _slab * 25
                    If signal.Item4 = IOrder.TypeOfTransaction.Buy Then
                        If buyActiveTrades Is Nothing OrElse buyActiveTrades.Count = 0 Then
                            Dim triggerPrice As Decimal = signal.Item2 + buffer
                            Dim price As Decimal = triggerPrice + ConvertFloorCeling(triggerPrice * 0.3 / 100, TradableInstrument.TickSize, RoundOfType.Celing)

                            If currentTick.LastPrice < triggerPrice Then
                                parameters = New PlaceOrderParameters(signalCandle) With
                                            {.EntryDirection = IOrder.TypeOfTransaction.Buy,
                                             .TriggerPrice = triggerPrice,
                                             .Price = price,
                                             .StoplossValue = slPoint + 2 * buffer,
                                             .SquareOffValue = targetPoint,
                                             .Quantity = quantity}
                            End If
                        End If
                    ElseIf signal.Item4 = IOrder.TypeOfTransaction.Sell Then
                        If sellActiveTrades Is Nothing OrElse sellActiveTrades.Count = 0 Then
                            Dim triggerPrice As Decimal = signal.Item2 - buffer
                            Dim price As Decimal = triggerPrice - ConvertFloorCeling(triggerPrice * 0.3 / 100, TradableInstrument.TickSize, RoundOfType.Celing)

                            If currentTick.LastPrice > triggerPrice Then
                                parameters = New PlaceOrderParameters(signalCandle) With
                                            {.EntryDirection = IOrder.TypeOfTransaction.Sell,
                                             .TriggerPrice = triggerPrice,
                                             .Price = price,
                                             .StoplossValue = slPoint + 2 * buffer,
                                             .SquareOffValue = targetPoint,
                                             .Quantity = quantity}
                            End If
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

            Dim currentSignalActivities As IEnumerable(Of ActivityDashboard) = Me.ParentStrategy.SignalManager.GetSignalActivities(parameters.SignalCandle.SnapshotDateTime, Me.TradableInstrument.InstrumentIdentifier)
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
        Dim allActiveOrders As List(Of IOrder) = GetAllActiveOrders(IOrder.TypeOfTransaction.None)
        If allActiveOrders IsNot Nothing AndAlso allActiveOrders.Count > 0 Then
            Dim slOrders As List(Of IOrder) = allActiveOrders.FindAll(Function(x)
                                                                          Return x.ParentOrderIdentifier IsNot Nothing AndAlso
                                                                          x.Status = IOrder.TypeOfStatus.TriggerPending
                                                                      End Function)
            If slOrders IsNot Nothing AndAlso slOrders.Count > 0 Then
                Dim parentOrder As IOrder = allActiveOrders.Find(Function(x)
                                                                     Return x.ParentOrderIdentifier Is Nothing AndAlso
                                                                     x.Status = IOrder.TypeOfStatus.TriggerPending
                                                                 End Function)
                If parentOrder IsNot Nothing Then
                    For Each runningSLOrder In slOrders
                        If Not runningSLOrder.Status = IOrder.TypeOfStatus.Complete AndAlso
                            Not runningSLOrder.Status = IOrder.TypeOfStatus.Cancelled AndAlso
                            Not runningSLOrder.Status = IOrder.TypeOfStatus.Rejected Then
                            Dim bussinessOrder As IBusinessOrder = GetParentFromChildOrder(runningSLOrder)
                            If bussinessOrder.ParentOrder.TransactionType <> parentOrder.TransactionType Then
                                Dim triggerPrice As Decimal = Decimal.MinValue
                                If bussinessOrder.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Buy Then
                                    If parentOrder.TriggerPrice > runningSLOrder.TriggerPrice Then
                                        triggerPrice = parentOrder.TriggerPrice
                                    End If
                                ElseIf bussinessOrder.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Sell Then
                                    If parentOrder.TriggerPrice < runningSLOrder.TriggerPrice Then
                                        triggerPrice = parentOrder.TriggerPrice
                                    End If
                                End If
                                If triggerPrice <> Decimal.MinValue AndAlso runningSLOrder.TriggerPrice <> triggerPrice Then
                                    'Below portion have to be done in every modify stoploss order trigger
                                    Dim currentSignalActivities As ActivityDashboard = Me.ParentStrategy.SignalManager.GetSignalActivities(runningSLOrder.Tag)
                                    If currentSignalActivities IsNot Nothing Then
                                        If currentSignalActivities.StoplossModifyActivity.RequestStatus = ActivityDashboard.SignalStatusType.Handled OrElse
                                        currentSignalActivities.StoplossModifyActivity.RequestStatus = ActivityDashboard.SignalStatusType.Activated OrElse
                                        currentSignalActivities.StoplossModifyActivity.RequestStatus = ActivityDashboard.SignalStatusType.Completed Then
                                            If Val(currentSignalActivities.StoplossModifyActivity.Supporting) = triggerPrice Then
                                                Continue For
                                            End If
                                        End If
                                    End If
                                    If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String))
                                    ret.Add(New Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)(ExecuteCommandAction.Take, runningSLOrder, triggerPrice, "Opposite Direction signal trigger"))
                                End If
                            End If
                        End If
                    Next
                End If
            End If
        End If
        If forcePrint AndAlso ret IsNot Nothing AndAlso ret.Count > 0 Then
            For Each runningOrder In ret
                logger.Debug("***** Modify Stoploss ***** Order ID:{0}, Reason:{1}, {2}", runningOrder.Item2.OrderIdentifier, runningOrder.Item4, Me.TradableInstrument.TradingSymbol)
            Next
        End If
        Return ret
    End Function
    Protected Overrides Function IsTriggerReceivedForModifyTargetOrderAsync(forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)))
        Throw New NotImplementedException()
    End Function
    Protected Overrides Async Function IsTriggerReceivedForExitOrderAsync(ByVal forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, String)))
        Dim ret As List(Of Tuple(Of ExecuteCommandAction, IOrder, String)) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
        Dim hkConsumer As HeikinAshiConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummyHKConsumer)
        Dim runningCandlePayload As OHLCPayload = GetXMinuteCurrentCandle(userSettings.SignalTimeFrame)

        If runningCandlePayload IsNot Nothing AndAlso runningCandlePayload.PreviousPayload IsNot Nothing AndAlso
            Me.TradableInstrument.IsHistoricalCompleted AndAlso
            hkConsumer.ConsumerPayloads IsNot Nothing AndAlso hkConsumer.ConsumerPayloads.Count > 0 AndAlso
            hkConsumer.ConsumerPayloads.ContainsKey(runningCandlePayload.PreviousPayload.SnapshotDateTime) Then
            Dim allActiveOrders As List(Of IOrder) = GetAllActiveOrders(IOrder.TypeOfTransaction.None)
            If allActiveOrders IsNot Nothing AndAlso allActiveOrders.Count > 0 Then
                Dim parentOrders As List(Of IOrder) = allActiveOrders.FindAll(Function(x)
                                                                                  Return x.ParentOrderIdentifier Is Nothing AndAlso
                                                                              x.Status = IOrder.TypeOfStatus.TriggerPending
                                                                              End Function)
                If parentOrders IsNot Nothing AndAlso parentOrders.Count > 0 Then
                    Dim hkCandle As HeikinAshiConsumer.HeikinAshiPayload = hkConsumer.ConsumerPayloads(runningCandlePayload.PreviousPayload.SnapshotDateTime)
                    For Each runningOrder In parentOrders
                        If runningOrder.Status = IOrder.TypeOfStatus.TriggerPending Then
                            Dim bussinessOrder As IBusinessOrder = OrderDetails(runningOrder.OrderIdentifier)
                            Dim exitTrade As Boolean = False
                            Dim signal As Tuple(Of Boolean, Decimal, HeikinAshiConsumer.HeikinAshiPayload, IOrder.TypeOfTransaction) = GetSignalCandle(hkCandle)
                            If signal IsNot Nothing AndAlso signal.Item1 Then
                                If bussinessOrder.ParentOrder.TransactionType = signal.Item4 Then
                                    Dim orderSignalCandle As OHLCPayload = GetSignalCandleOfAnOrder(bussinessOrder.ParentOrderIdentifier, userSettings.SignalTimeFrame)
                                    If orderSignalCandle.SnapshotDateTime <> runningCandlePayload.PreviousPayload.SnapshotDateTime Then
                                        exitTrade = True
                                    End If
                                End If
                            End If
                            If exitTrade Then
                                'Below portion have to be done in every cancel order trigger
                                Dim currentSignalActivities As ActivityDashboard = Me.ParentStrategy.SignalManager.GetSignalActivities(runningOrder.Tag)
                                If currentSignalActivities IsNot Nothing Then
                                    If currentSignalActivities.CancelActivity.RequestStatus = ActivityDashboard.SignalStatusType.Handled OrElse
                                        currentSignalActivities.CancelActivity.RequestStatus = ActivityDashboard.SignalStatusType.Activated OrElse
                                        currentSignalActivities.CancelActivity.RequestStatus = ActivityDashboard.SignalStatusType.Completed Then
                                        Continue For
                                    End If
                                End If
                                If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, IOrder, String))
                                ret.Add(New Tuple(Of ExecuteCommandAction, IOrder, String)(ExecuteCommandAction.Take, runningOrder, "Invalid signal"))
                            End If
                        End If
                    Next
                End If
            End If
        End If
        If forcePrint AndAlso ret IsNot Nothing AndAlso ret.Count > 0 Then
            Try
                For Each runningOrder In ret
                    logger.Debug("***** Exit Order ***** Order ID:{0}, Reason:{1}, {2}",
                                 runningOrder.Item2.OrderIdentifier,
                                 runningOrder.Item3,
                                 Me.TradableInstrument.TradingSymbol)
                Next
            Catch ex As Exception
                logger.Error(ex)
            End Try
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

            Await ExecuteCommandAsync(ExecuteCommands.ForceCancelBOOrder, cancellableOrder).ConfigureAwait(False)
        End If
    End Function

    Private Function GetSlabBasedLevel(ByVal price As Decimal, ByVal direction As IOrder.TypeOfTransaction) As Decimal
        Dim ret As Decimal = Decimal.MinValue
        If direction = IOrder.TypeOfTransaction.Buy Then
            ret = ConvertFloorCeling(Math.Ceiling(price / _slab) * _slab, Me.TradableInstrument.TickSize, RoundOfType.Celing)
        ElseIf direction = IOrder.TypeOfTransaction.Sell Then
            ret = ConvertFloorCeling(Math.Floor(price / _slab) * _slab, Me.TradableInstrument.TickSize, RoundOfType.Floor)
        End If
        Return ret
    End Function

    Private Function GetSignalCandle(ByVal hkCandle As HeikinAshiConsumer.HeikinAshiPayload) As Tuple(Of Boolean, Decimal, HeikinAshiConsumer.HeikinAshiPayload, IOrder.TypeOfTransaction)
        Dim ret As Tuple(Of Boolean, Decimal, HeikinAshiConsumer.HeikinAshiPayload, IOrder.TypeOfTransaction) = Nothing
        If hkCandle IsNot Nothing Then
            If Math.Round(hkCandle.High.Value, 4) = Math.Round(hkCandle.Open.Value, 4) Then
                Dim buyLevel As Decimal = GetSlabBasedLevel(hkCandle.High.Value, IOrder.TypeOfTransaction.Buy)
                ret = New Tuple(Of Boolean, Decimal, HeikinAshiConsumer.HeikinAshiPayload, IOrder.TypeOfTransaction)(True, buyLevel, hkCandle, IOrder.TypeOfTransaction.Buy)
            ElseIf Math.Round(hkCandle.Low.Value, 4) = Math.Round(hkCandle.Open.Value, 4) Then
                Dim sellLevel As Decimal = GetSlabBasedLevel(hkCandle.Low.Value, IOrder.TypeOfTransaction.Sell)
                ret = New Tuple(Of Boolean, Decimal, HeikinAshiConsumer.HeikinAshiPayload, IOrder.TypeOfTransaction)(True, sellLevel, hkCandle, IOrder.TypeOfTransaction.Sell)
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