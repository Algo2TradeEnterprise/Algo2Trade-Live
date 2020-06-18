Imports System.Threading
Imports Algo2TradeCore.Adapter
Imports Algo2TradeCore.Entities
Imports Algo2TradeCore.Strategies
Imports Utilities.Numbers
Imports NLog
Imports Algo2TradeCore.Entities.Indicators
Imports Algo2TradeCore.ChartHandler.ChartStyle
Imports Algo2TradeCore

Public Class NFOStrategyInstrument
    Inherits StrategyInstrument
    Implements IDisposable

#Region "Logging and Status Progress"
    Public Shared Shadows logger As Logger = LogManager.GetCurrentClassLogger
#End Region

    Private _cancellationDone As Boolean

    Private _slPoint As Decimal = Decimal.MinValue
    Private _targetPoint As Decimal = Decimal.MinValue
    Private _lastPrevPayloadPlaceOrder As String = ""
    Private ReadOnly _dummyHKConsumer As HeikinAshiConsumer
    Private ReadOnly _dummyATRConsumer As ATRConsumer
    Public ReadOnly Multiplier As Decimal = 0
    Public ReadOnly PreviousDayHighestATR As Decimal = 0

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
                Dim hkConsumer As HeikinAshiConsumer = New HeikinAshiConsumer(chartConsumer)
                hkConsumer.OnwardLevelConsumers = New List(Of IPayloadConsumer) From {New ATRConsumer(hkConsumer, 14)}
                chartConsumer.OnwardLevelConsumers = New List(Of IPayloadConsumer) From {hkConsumer}
                RawPayloadDependentConsumers.Add(chartConsumer)
                _dummyHKConsumer = New HeikinAshiConsumer(chartConsumer)
                _dummyATRConsumer = New ATRConsumer(hkConsumer, 14)
            Else
                Throw New ApplicationException(String.Format("Signal Timeframe is 0 or Nothing, does not adhere to the strategy:{0}", Me.ParentStrategy.ToString))
            End If
        End If
        Multiplier = CType(Me.ParentStrategy.UserSettings, NFOUserInputs).InstrumentsData(Me.TradableInstrument.TradingSymbol).Multiplier
        PreviousDayHighestATR = CType(Me.ParentStrategy.UserSettings, NFOUserInputs).InstrumentsData(Me.TradableInstrument.TradingSymbol).PreviousDayHighestATR
    End Sub

    Public Overrides Async Function PopulateChartAndIndicatorsAsync(candleCreator As Chart, currentCandle As OHLCPayload) As Task
        If RawPayloadDependentConsumers IsNot Nothing AndAlso RawPayloadDependentConsumers.Count > 0 Then
            For Each runningRawPayloadConsumer In RawPayloadDependentConsumers
                If runningRawPayloadConsumer.TypeOfConsumer = IPayloadConsumer.ConsumerType.Chart Then
                    Dim currentXMinute As Date = candleCreator.ConvertTimeframe(CType(runningRawPayloadConsumer, PayloadToChartConsumer).Timeframe,
                                                                currentCandle,
                                                                runningRawPayloadConsumer)
                    If candleCreator.IndicatorCreator Is Nothing Then candleCreator.IndicatorCreator = New ChartHandler.Indicator.IndicatorManeger(Me.ParentStrategy.ParentController, candleCreator, _cts)
                    If currentXMinute <> Date.MaxValue Then
                        Dim c As Integer = 1
                        If runningRawPayloadConsumer.OnwardLevelConsumers IsNot Nothing AndAlso runningRawPayloadConsumer.OnwardLevelConsumers.Count > 0 Then
                            For Each consumer In runningRawPayloadConsumer.OnwardLevelConsumers
                                candleCreator.IndicatorCreator.CalculateHeikinAshi(currentXMinute, consumer)
                                candleCreator.IndicatorCreator.CalculateATR(currentXMinute, consumer.OnwardLevelConsumers.FirstOrDefault)
                            Next
                        End If
                    End If
                End If
            Next
        End If
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
                    Dim placeOrderResponse = Await ExecuteCommandAsync(ExecuteCommands.PlaceBOSLMISOrder, Nothing).ConfigureAwait(False)
                    If placeOrderResponse IsNot Nothing AndAlso placeOrderResponse.ContainsKey("data") AndAlso
                        placeOrderResponse("data").ContainsKey("order_id") Then
                        _cancellationDone = False
                    End If
                End If
                'Place Order block end
                _cts.Token.ThrowIfCancellationRequested()
                'Modify Order block start
                Dim modifyStoplossOrderTrigger As List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)) = Await IsTriggerReceivedForModifyStoplossOrderAsync(False).ConfigureAwait(False)
                If modifyStoplossOrderTrigger IsNot Nothing AndAlso modifyStoplossOrderTrigger.Count > 0 Then
                    Await ExecuteCommandAsync(ExecuteCommands.ModifyStoplossOrder, Nothing).ConfigureAwait(False)
                End If
                Dim modifyTargetOrderTrigger As List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)) = Await IsTriggerReceivedForModifyTargetOrderAsync(False).ConfigureAwait(False)
                If modifyTargetOrderTrigger IsNot Nothing AndAlso modifyTargetOrderTrigger.Count > 0 Then
                    Await ExecuteCommandAsync(ExecuteCommands.ModifyTargetOrder, Nothing).ConfigureAwait(False)
                End If
                'Modify Order block end
                _cts.Token.ThrowIfCancellationRequested()
                'Exit Order block start
                Dim exitOrderTrigger As List(Of Tuple(Of ExecuteCommandAction, IOrder, String)) = Await IsTriggerReceivedForExitOrderAsync(False).ConfigureAwait(False)
                If exitOrderTrigger IsNot Nothing AndAlso exitOrderTrigger.Count > 0 Then
                    Dim exitOrderResponse = Await ExecuteCommandAsync(ExecuteCommands.CancelBOOrder, Nothing).ConfigureAwait(False)
                    If exitOrderResponse IsNot Nothing AndAlso exitOrderResponse.ContainsKey("data") AndAlso
                        exitOrderResponse("data").ContainsKey("status") AndAlso exitOrderResponse("data")("status") = "Ok" Then
                        _cancellationDone = True
                    End If
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
        Dim atrConsumer As ATRConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummyATRConsumer)
        Dim currentTick As ITick = Me.TradableInstrument.LastTick
        Dim currentTime As Date = Now()
        Dim lastExecutedOrder As IBusinessOrder = GetLastExecutedOrder()

        Try
            If runningCandlePayload IsNot Nothing AndAlso runningCandlePayload.PreviousPayload IsNot Nothing AndAlso
                Me.TradableInstrument.IsHistoricalCompleted Then
                If Not runningCandlePayload.PreviousPayload.ToString = _lastPrevPayloadPlaceOrder Then
                    _lastPrevPayloadPlaceOrder = runningCandlePayload.PreviousPayload.ToString
                    Dim highestATR As Decimal = GetHighestATR(atrConsumer, runningCandlePayload.PreviousPayload)
                    logger.Debug("PlaceOrder-> Potential Signal Candle is:{0}. Will check rest parameters.", hkConsumer.ConsumerPayloads(runningCandlePayload.PreviousPayload.SnapshotDateTime).ToString)
                    logger.Debug("PlaceOrder-> Rest all parameters: Running Candle:{0}, PayloadGeneratedBy:{1}, IsHistoricalCompleted:{2}, IsFirstTimeInformationCollected:{3}, Is Active Trades:{4}, Is Any Trade Target Reached:{5}, Highest ATR:{6}, Total PL:{7}, Stock PL:{8}, Number of Trade:{9}, Cancellation Done:{10}, Current Time:{11}, Current Tick:{12}, TradingSymbol:{13}",
                                runningCandlePayload.SnapshotDateTime.ToString("dd-MM-yyyy HH:mm:ss"),
                                runningCandlePayload.PayloadGeneratedBy.ToString,
                                Me.TradableInstrument.IsHistoricalCompleted,
                                Me.ParentStrategy.IsFirstTimeInformationCollected,
                                IsActiveInstrument(),
                                IsAnyTradeTargetReached(),
                                If(highestATR <> Decimal.MinValue, Math.Round(highestATR, 4), "∞"),
                                Me.ParentStrategy.GetTotalPLAfterBrokerage(),
                                Me.GetOverallPLAfterBrokerage(),
                                Me.GetTotalExecutedOrders(),
                                _cancellationDone,
                                currentTime.ToString,
                                currentTick.LastPrice,
                                Me.TradableInstrument.TradingSymbol)
                End If
            End If
        Catch ex As Exception
            logger.Error(ex)
        End Try

        Dim parameters As PlaceOrderParameters = Nothing
        If currentTime >= userSettings.TradeStartTime AndAlso currentTime <= userSettings.LastTradeEntryTime AndAlso currentTime <= userSettings.EODExitTime AndAlso
            runningCandlePayload IsNot Nothing AndAlso runningCandlePayload.SnapshotDateTime >= userSettings.TradeStartTime AndAlso
            runningCandlePayload.PayloadGeneratedBy = OHLCPayload.PayloadSource.CalculatedTick AndAlso runningCandlePayload.PreviousPayload IsNot Nothing AndAlso
            Me.TradableInstrument.IsHistoricalCompleted AndAlso GetTotalExecutedOrders() < userSettings.NumberOfTradePerStock AndAlso Not IsAnyTradeTargetReached() AndAlso
            (Not IsActiveInstrument() OrElse _cancellationDone) AndAlso Me.ParentStrategy.GetTotalPLAfterBrokerage() > userSettings.OverallMaxLossPerDay AndAlso
            Me.ParentStrategy.GetTotalPLAfterBrokerage() < userSettings.OverallMaxProfitPerDay AndAlso Not Me.StrategyExitAllTriggerd AndAlso
            hkConsumer.ConsumerPayloads IsNot Nothing AndAlso hkConsumer.ConsumerPayloads.Count > 0 AndAlso
            hkConsumer.ConsumerPayloads.ContainsKey(runningCandlePayload.PreviousPayload.SnapshotDateTime) AndAlso
            atrConsumer.ConsumerPayloads IsNot Nothing AndAlso atrConsumer.ConsumerPayloads.Count > 0 Then
            Dim hkCandle As OHLCPayload = hkConsumer.ConsumerPayloads(runningCandlePayload.PreviousPayload.SnapshotDateTime)
            Dim signal As Tuple(Of Boolean, Decimal, OHLCPayload, IOrder.TypeOfTransaction) = GetSignalCandle(hkCandle, forcePrint)
            If signal IsNot Nothing AndAlso signal.Item1 Then
                Dim signalCandle As OHLCPayload = Nothing
                Dim quantity As Integer = Integer.MinValue
                If lastExecutedOrder Is Nothing Then
                    signalCandle = signal.Item3
                    _slPoint = ConvertFloorCeling(GetHighestATR(atrConsumer, signalCandle), Me.TradableInstrument.TickSize, RoundOfType.Celing)
                    If _slPoint <> Decimal.MinValue Then
                        quantity = CalculateQuantityFromStoploss(signal.Item2, signal.Item2 - _slPoint, userSettings.MaxProfitPerTrade)
                        _targetPoint = CalculateTargetFromPL(signal.Item2, quantity, userSettings.MaxProfitPerTrade) - signal.Item2
                    End If
                Else
                    Dim lastOrderSignalCandle As OHLCPayload = GetSignalCandleOfAnOrder(lastExecutedOrder.ParentOrderIdentifier, userSettings.SignalTimeFrame)
                    If lastOrderSignalCandle IsNot Nothing AndAlso lastOrderSignalCandle.SnapshotDateTime <> runningCandlePayload.PreviousPayload.SnapshotDateTime Then
                        signalCandle = signal.Item3
                        quantity = lastExecutedOrder.ParentOrder.Quantity * 2
                        If _slPoint = Decimal.MinValue OrElse _targetPoint = Decimal.MinValue Then
                            Dim firstExecutedOrder As IBusinessOrder = GetFirstExecutedOrder()
                            If firstExecutedOrder IsNot Nothing Then
                                Dim firstTradeSignalCandle As OHLCPayload = GetSignalCandleOfAnOrder(firstExecutedOrder.ParentOrderIdentifier, userSettings.SignalTimeFrame)
                                If firstTradeSignalCandle IsNot Nothing Then
                                    _slPoint = ConvertFloorCeling(GetHighestATR(atrConsumer, firstTradeSignalCandle), Me.TradableInstrument.TickSize, RoundOfType.Celing)
                                    Dim firstTradeQuantity As Decimal = firstExecutedOrder.ParentOrder.Quantity
                                    _targetPoint = CalculateTargetFromPL(firstExecutedOrder.ParentOrder.TriggerPrice, firstTradeQuantity, userSettings.MaxProfitPerTrade) - firstExecutedOrder.ParentOrder.TriggerPrice
                                End If
                            End If
                        End If
                    End If
                End If
                If signalCandle IsNot Nothing AndAlso _slPoint <> Decimal.MinValue AndAlso _targetPoint <> Decimal.MinValue AndAlso quantity <> Integer.MinValue AndAlso _targetPoint >= _slPoint Then
                    If signal.Item4 = IOrder.TypeOfTransaction.Buy Then
                        Dim triggerPrice As Decimal = signal.Item2
                        Dim price As Decimal = triggerPrice + ConvertFloorCeling(triggerPrice * 0.3 / 100, TradableInstrument.TickSize, RoundOfType.Celing)

                        If currentTick.LastPrice < triggerPrice Then
                            parameters = New PlaceOrderParameters(signalCandle) With
                                        {.EntryDirection = IOrder.TypeOfTransaction.Buy,
                                         .TriggerPrice = triggerPrice,
                                         .Price = price,
                                         .StoplossValue = _slPoint,
                                         .SquareOffValue = _targetPoint,
                                         .OrderType = IOrder.TypeOfOrder.SL,
                                         .Quantity = quantity}
                        End If
                    ElseIf signal.Item4 = IOrder.TypeOfTransaction.Sell Then
                        Dim triggerPrice As Decimal = signal.Item2
                        Dim price As Decimal = triggerPrice - ConvertFloorCeling(triggerPrice * 0.3 / 100, TradableInstrument.TickSize, RoundOfType.Celing)

                        If currentTick.LastPrice > triggerPrice Then
                            parameters = New PlaceOrderParameters(signalCandle) With
                                        {.EntryDirection = IOrder.TypeOfTransaction.Sell,
                                         .TriggerPrice = triggerPrice,
                                         .Price = price,
                                         .StoplossValue = _slPoint,
                                         .SquareOffValue = _targetPoint,
                                         .OrderType = IOrder.TypeOfOrder.SL,
                                         .Quantity = quantity}
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

    Protected Overrides Async Function IsTriggerReceivedForModifyStoplossOrderAsync(ByVal forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)))
        Dim ret As List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        If OrderDetails IsNot Nothing AndAlso OrderDetails.Count > 0 Then
            For Each runningParentOrder In OrderDetails.Keys
                Dim parentBussinessOrder As IBusinessOrder = OrderDetails(runningParentOrder)
                If parentBussinessOrder.ParentOrder.Status = IOrder.TypeOfStatus.Complete AndAlso
                    parentBussinessOrder.SLOrder IsNot Nothing AndAlso parentBussinessOrder.SLOrder.Count > 0 Then
                    For Each runningSLOrder In parentBussinessOrder.SLOrder
                        If Not runningSLOrder.Status = IOrder.TypeOfStatus.Complete AndAlso
                            Not runningSLOrder.Status = IOrder.TypeOfStatus.Cancelled AndAlso
                            Not runningSLOrder.Status = IOrder.TypeOfStatus.Rejected Then
                            Dim triggerPrice As Decimal = Decimal.MinValue
                            If parentBussinessOrder.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Buy Then
                                triggerPrice = ConvertFloorCeling(parentBussinessOrder.ParentOrder.AveragePrice, Me.TradableInstrument.TickSize, RoundOfType.Celing) - _slPoint
                            ElseIf parentBussinessOrder.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Sell Then
                                triggerPrice = ConvertFloorCeling(parentBussinessOrder.ParentOrder.AveragePrice, Me.TradableInstrument.TickSize, RoundOfType.Floor) + _slPoint
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
                                ret.Add(New Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)(ExecuteCommandAction.Take, runningSLOrder, triggerPrice, "Sllipage adjustment"))
                            End If
                        End If
                    Next
                End If
            Next
        End If
        If forcePrint AndAlso ret IsNot Nothing AndAlso ret.Count > 0 Then
            For Each runningOrder In ret
                logger.Debug("***** Modify Stoploss ***** Order ID:{0}, Reason:{1}, {2}", runningOrder.Item2.OrderIdentifier, runningOrder.Item4, Me.TradableInstrument.TradingSymbol)
            Next
        End If
        Return ret
    End Function

    Protected Overrides Async Function IsTriggerReceivedForModifyTargetOrderAsync(forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)))
        Dim ret As List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        If OrderDetails IsNot Nothing AndAlso OrderDetails.Count > 0 Then
            For Each runningParentOrder In OrderDetails.Keys
                Dim parentBussinessOrder As IBusinessOrder = OrderDetails(runningParentOrder)
                If parentBussinessOrder.ParentOrder.Status = IOrder.TypeOfStatus.Complete AndAlso
                    parentBussinessOrder.TargetOrder IsNot Nothing AndAlso parentBussinessOrder.TargetOrder.Count > 0 Then
                    For Each runningTargetOrder In parentBussinessOrder.TargetOrder
                        If Not runningTargetOrder.Status = IOrder.TypeOfStatus.Complete AndAlso
                            Not runningTargetOrder.Status = IOrder.TypeOfStatus.Cancelled AndAlso
                            Not runningTargetOrder.Status = IOrder.TypeOfStatus.Rejected Then
                            Dim price As Decimal = Decimal.MinValue
                            If parentBussinessOrder.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Buy Then
                                price = ConvertFloorCeling(parentBussinessOrder.ParentOrder.AveragePrice, Me.TradableInstrument.TickSize, RoundOfType.Celing) + _targetPoint
                            ElseIf parentBussinessOrder.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Sell Then
                                price = ConvertFloorCeling(parentBussinessOrder.ParentOrder.AveragePrice, Me.TradableInstrument.TickSize, RoundOfType.Floor) - _targetPoint
                            End If
                            If price <> Decimal.MinValue AndAlso runningTargetOrder.Price <> price Then
                                'Below portion have to be done in every modify target order trigger
                                Dim currentSignalActivities As ActivityDashboard = Me.ParentStrategy.SignalManager.GetSignalActivities(runningTargetOrder.Tag)
                                If currentSignalActivities IsNot Nothing Then
                                    If currentSignalActivities.TargetModifyActivity.RequestStatus = ActivityDashboard.SignalStatusType.Handled OrElse
                                        currentSignalActivities.TargetModifyActivity.RequestStatus = ActivityDashboard.SignalStatusType.Activated OrElse
                                        currentSignalActivities.TargetModifyActivity.RequestStatus = ActivityDashboard.SignalStatusType.Completed Then
                                        If Val(currentSignalActivities.TargetModifyActivity.Supporting) = price Then
                                            Continue For
                                        End If
                                    End If
                                End If
                                If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String))
                                ret.Add(New Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)(ExecuteCommandAction.Take, runningTargetOrder, price, "Sllipage adjustment"))
                            End If
                        End If
                    Next
                End If
            Next
        End If
        If forcePrint AndAlso ret IsNot Nothing AndAlso ret.Count > 0 Then
            For Each runningOrder In ret
                logger.Debug("***** Modify Target ***** Order ID:{0}, Reason:{1}, {2}", runningOrder.Item2.OrderIdentifier, runningOrder.Item4, Me.TradableInstrument.TradingSymbol)
            Next
        End If
        Return ret
    End Function

    Protected Overrides Async Function IsTriggerReceivedForExitOrderAsync(ByVal forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, String)))
        Dim ret As List(Of Tuple(Of ExecuteCommandAction, IOrder, String)) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
        Dim hkConsumer As HeikinAshiConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummyHKConsumer)
        Dim runningCandlePayload As OHLCPayload = GetXMinuteCurrentCandle(userSettings.SignalTimeFrame)
        Dim currentTick As ITick = Me.TradableInstrument.LastTick

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
                    Dim hkCandle As OHLCPayload = hkConsumer.ConsumerPayloads(runningCandlePayload.PreviousPayload.SnapshotDateTime)
                    For Each runningOrder In parentOrders
                        If runningOrder.Status = IOrder.TypeOfStatus.TriggerPending Then
                            Dim bussinessOrder As IBusinessOrder = OrderDetails(runningOrder.OrderIdentifier)
                            Dim exitTrade As Boolean = False
                            Dim signal As Tuple(Of Boolean, Decimal, OHLCPayload, IOrder.TypeOfTransaction) = GetSignalCandle(hkCandle, forcePrint)
                            If signal IsNot Nothing AndAlso signal.Item1 Then
                                If bussinessOrder.ParentOrder.TransactionType = signal.Item4 Then
                                    If signal.Item4 = IOrder.TypeOfTransaction.Buy Then
                                        If bussinessOrder.ParentOrder.TriggerPrice <> signal.Item2 AndAlso
                                            currentTick.LastPrice < signal.Item2 Then
                                            exitTrade = True
                                        End If
                                    ElseIf signal.Item4 = IOrder.TypeOfTransaction.Sell Then
                                        If bussinessOrder.ParentOrder.TriggerPrice <> signal.Item2 AndAlso
                                            currentTick.LastPrice > signal.Item2 Then
                                            exitTrade = True
                                        End If
                                    End If
                                ElseIf bussinessOrder.ParentOrder.TransactionType <> signal.Item4 Then
                                    exitTrade = True
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

    Public Overrides Function MonitorAsync(ByVal command As ExecuteCommands, ByVal data As Object) As Task
        Throw New NotImplementedException()
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

    Private Function GetSignalCandle(ByVal hkCandle As OHLCPayload, ByVal forcePrint As Boolean) As Tuple(Of Boolean, Decimal, OHLCPayload, IOrder.TypeOfTransaction)
        Dim ret As Tuple(Of Boolean, Decimal, OHLCPayload, IOrder.TypeOfTransaction) = Nothing
        If hkCandle IsNot Nothing Then
            Dim lastBuySignal As Tuple(Of Boolean, Decimal, OHLCPayload, IOrder.TypeOfTransaction) = GetLastHistoricalSignal(hkCandle, IOrder.TypeOfTransaction.Buy)
            Dim lastSellSignal As Tuple(Of Boolean, Decimal, OHLCPayload, IOrder.TypeOfTransaction) = GetLastHistoricalSignal(hkCandle, IOrder.TypeOfTransaction.Sell)
            If lastBuySignal IsNot Nothing AndAlso lastSellSignal IsNot Nothing Then
                If lastBuySignal.Item3.SnapshotDateTime > lastSellSignal.Item3.SnapshotDateTime Then
                    ret = lastBuySignal
                Else
                    ret = lastSellSignal
                End If
            ElseIf lastBuySignal IsNot Nothing Then
                ret = lastBuySignal
            ElseIf lastSellSignal IsNot Nothing Then
                ret = lastSellSignal
            End If
        End If
        If ret IsNot Nothing AndAlso forcePrint Then
            Try
                logger.Debug("{0}, Direction:{1}, Level:{2}, Trading Symbol:{3}", ret.Item3.ToString, ret.Item4.ToString, ret.Item2, Me.TradableInstrument.TradingSymbol)
            Catch ex As Exception
                logger.Warn(ex.ToString)
            End Try
        End If
        Return ret
    End Function

    Private Function GetHighestATR(ByVal atrConsumer As ATRConsumer, ByVal signalCandle As OHLCPayload) As Decimal
        Dim ret As Decimal = Decimal.MinValue
        If atrConsumer IsNot Nothing AndAlso atrConsumer.ConsumerPayloads IsNot Nothing AndAlso atrConsumer.ConsumerPayloads.Count > 0 AndAlso signalCandle IsNot Nothing Then
            Dim todayHighestATR As Decimal = atrConsumer.ConsumerPayloads.Max(Function(x)
                                                                                  If x.Key.Date = Now.Date AndAlso x.Key <= signalCandle.SnapshotDateTime Then
                                                                                      Return CType(x.Value, ATRConsumer.ATRPayload).ATR.Value
                                                                                  Else
                                                                                      Return Decimal.MinValue
                                                                                  End If
                                                                              End Function)
            If todayHighestATR <> Decimal.MinValue Then
                'ret = Math.Min(todayHighestATR, Me.PreviousDayHighestATR)
                ret = (todayHighestATR + Me.PreviousDayHighestATR) / 2
            End If
        End If
        Return ret
    End Function

    Private Function IsAnyTradeTargetReached() As Boolean
        Dim ret As Boolean = False
        If OrderDetails IsNot Nothing AndAlso OrderDetails.Count > 0 AndAlso _slPoint <> Decimal.MinValue Then
            For Each parentOrder In OrderDetails.Keys
                Dim bussinessOrder As IBusinessOrder = OrderDetails(parentOrder)
                If bussinessOrder.AllOrder IsNot Nothing AndAlso bussinessOrder.AllOrder.Count > 0 Then
                    For Each order In bussinessOrder.AllOrder
                        If order.LogicalOrderType = IOrder.LogicalTypeOfOrder.Target AndAlso order.Status = IOrder.TypeOfStatus.Complete Then
                            Dim target As Decimal = 0
                            If bussinessOrder.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Buy Then
                                target = order.AveragePrice - bussinessOrder.ParentOrder.AveragePrice
                            ElseIf bussinessOrder.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Sell Then
                                target = bussinessOrder.ParentOrder.AveragePrice - order.AveragePrice
                            End If
                            If target >= _slPoint Then
                                ret = True
                                Exit For
                            End If
                        End If
                    Next
                End If
                If ret Then Exit For
            Next
        End If
        Return ret
    End Function

    Private Function GetLastHistoricalSignal(ByVal hkCandle As OHLCPayload, ByVal direction As IOrder.TypeOfTransaction) As Tuple(Of Boolean, Decimal, OHLCPayload, IOrder.TypeOfTransaction)
        Dim ret As Tuple(Of Boolean, Decimal, OHLCPayload, IOrder.TypeOfTransaction) = Nothing
        If hkCandle IsNot Nothing Then
            Dim candleToCheck As OHLCPayload = hkCandle
            While candleToCheck IsNot Nothing
                If candleToCheck.SnapshotDateTime.Date <> Now.Date Then
                    Exit While
                End If

                If direction = IOrder.TypeOfTransaction.Buy AndAlso Math.Round(candleToCheck.HighPrice.Value, 4) = Math.Round(candleToCheck.OpenPrice.Value, 4) Then
                    Dim buyLevel As Decimal = ConvertFloorCeling(candleToCheck.HighPrice.Value, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                    If buyLevel = candleToCheck.HighPrice.Value Then
                        Dim buffer As Decimal = CalculateBuffer(buyLevel, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                        buyLevel = buyLevel + buffer
                    End If
                    If Not IsSignalTriggered(buyLevel, IOrder.TypeOfTransaction.Buy, candleToCheck.SnapshotDateTime, Now) Then
                        ret = New Tuple(Of Boolean, Decimal, OHLCPayload, IOrder.TypeOfTransaction)(True, buyLevel, candleToCheck, IOrder.TypeOfTransaction.Buy)
                    End If
                    Exit While
                ElseIf direction = IOrder.TypeOfTransaction.Sell AndAlso Math.Round(candleToCheck.LowPrice.Value, 4) = Math.Round(candleToCheck.OpenPrice.Value, 4) Then
                    Dim sellLevel As Decimal = ConvertFloorCeling(candleToCheck.LowPrice.Value, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                    If sellLevel = candleToCheck.LowPrice.Value Then
                        Dim buffer As Decimal = CalculateBuffer(sellLevel, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                        sellLevel = sellLevel - buffer
                    End If
                    If Not IsSignalTriggered(sellLevel, IOrder.TypeOfTransaction.Sell, candleToCheck.SnapshotDateTime, Now) Then
                        ret = New Tuple(Of Boolean, Decimal, OHLCPayload, IOrder.TypeOfTransaction)(True, sellLevel, candleToCheck, IOrder.TypeOfTransaction.Sell)
                    End If
                    Exit While
                End If

                candleToCheck = candleToCheck.PreviousPayload
            End While
        End If
        Return ret
    End Function

    Private Function IsSignalTriggered(ByVal entryPrice As Decimal, ByVal entryDirection As IOrder.TypeOfTransaction, ByVal fromTime As Date, ByVal toTime As Date) As Boolean
        Dim ret As Boolean = False
        If fromTime < toTime Then
            If Me.RawPayloadDependentConsumers IsNot Nothing AndAlso Me.RawPayloadDependentConsumers.Count > 0 Then
                Dim XMinutePayloadConsumer As PayloadToChartConsumer = RawPayloadDependentConsumers.Find(Function(x)
                                                                                                             If x.GetType Is GetType(PayloadToChartConsumer) Then
                                                                                                                 Return CType(x, PayloadToChartConsumer).Timeframe = Me.ParentStrategy.UserSettings.SignalTimeFrame
                                                                                                             Else
                                                                                                                 Return Nothing
                                                                                                             End If
                                                                                                         End Function)

                If XMinutePayloadConsumer IsNot Nothing AndAlso XMinutePayloadConsumer.ConsumerPayloads IsNot Nothing AndAlso XMinutePayloadConsumer.ConsumerPayloads.Count > 0 Then
                    Dim lastExistingPayloads As IEnumerable(Of KeyValuePair(Of Date, IPayload)) =
                        XMinutePayloadConsumer.ConsumerPayloads.Where(Function(y)
                                                                          Return y.Key > fromTime AndAlso y.Key <= toTime
                                                                      End Function)
                    If lastExistingPayloads IsNot Nothing AndAlso lastExistingPayloads.Count > 0 Then
                        For Each runningPayload In lastExistingPayloads.OrderBy(Function(x)
                                                                                    Return x.Key
                                                                                End Function)
                            Dim candle As OHLCPayload = runningPayload.Value
                            If entryDirection = IOrder.TypeOfTransaction.Buy Then
                                If candle.HighPrice.Value >= entryPrice Then
                                    ret = True
                                    Exit For
                                End If
                            ElseIf entryDirection = IOrder.TypeOfTransaction.Sell Then
                                If candle.LowPrice.Value <= entryPrice Then
                                    ret = True
                                    Exit For
                                End If
                            End If
                        Next
                    End If
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