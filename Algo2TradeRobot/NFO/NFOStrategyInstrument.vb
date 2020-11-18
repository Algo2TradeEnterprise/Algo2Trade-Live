Imports System.Threading
Imports Algo2TradeCore.Adapter
Imports Algo2TradeCore.Entities
Imports Algo2TradeCore.Strategies
Imports Utilities.Numbers
Imports NLog
Imports Algo2TradeCore.Entities.Indicators
Imports Algo2TradeCore.ChartHandler.ChartStyle
Imports Algo2TradeCore
Imports Utilities.Network
Imports Utilities.Time
Imports System.Net.Http

Public Class NFOStrategyInstrument
    Inherits StrategyInstrument
    Implements IDisposable

#Region "Logging and Status Progress"
    Public Shared Shadows logger As Logger = LogManager.GetCurrentClassLogger
#End Region

    Private _PreProcessingDone As Boolean
    Public ReadOnly Property PreProcessingDone As Boolean
        Get
            Return _PreProcessingDone
        End Get
    End Property

    Private _lastDayMA As Decimal = Decimal.MinValue
    Private _lastDayATR As Decimal = Decimal.MinValue

    'Private _lastPrevPayload As OHLCPayload = Nothing
    Private _lastPrevPayloadString As String = ""
    'Private _lastMessage As String = ""
    'Private _lastMessageSend As Date = Now.Date

    Private ReadOnly _ChartRawURL As String = "https://ant.aliceblueonline.com/ext/chart/?token={0}&id={1}&exchange={2}&symbol={3}&fullscreen=true"
    Private _ChartURL As String

    Private ReadOnly _dummyHKConsumer As HeikinAshiConsumer
    Private ReadOnly _dummyVWAPConsumer As VWAPConsumer
    Private ReadOnly _dummyEMAConsumer As EMAConsumer
    Private ReadOnly _dummyPivotConsumer As PivotsConsumer
    Private ReadOnly _dummyRSIConsumer As RSIConsumer

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
                Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
                Dim chartConsumer As PayloadToChartConsumer = New PayloadToChartConsumer(Me.ParentStrategy.UserSettings.SignalTimeFrame)
                Dim hkConsumer As HeikinAshiConsumer = New HeikinAshiConsumer(chartConsumer)
                Dim vwapConsumer As VWAPConsumer = New VWAPConsumer(hkConsumer)
                vwapConsumer.OnwardLevelConsumers = New List(Of IPayloadConsumer) From {New EMAConsumer(vwapConsumer, userSettings.VWAP_EMAPeriod, TypeOfField.VWAP)}
                hkConsumer.OnwardLevelConsumers = New List(Of IPayloadConsumer) From {vwapConsumer, New PivotsConsumer(hkConsumer), New RSIConsumer(hkConsumer, userSettings.Close_RSIPeriod)}
                chartConsumer.OnwardLevelConsumers = New List(Of IPayloadConsumer) From {hkConsumer}
                RawPayloadDependentConsumers.Add(chartConsumer)

                _dummyHKConsumer = New HeikinAshiConsumer(chartConsumer)
                _dummyVWAPConsumer = New VWAPConsumer(hkConsumer)
                _dummyEMAConsumer = New EMAConsumer(vwapConsumer, userSettings.VWAP_EMAPeriod, TypeOfField.VWAP)
                _dummyPivotConsumer = New PivotsConsumer(hkConsumer)
                _dummyRSIConsumer = New RSIConsumer(hkConsumer, userSettings.Close_RSIPeriod)
            Else
                Throw New ApplicationException(String.Format("Signal Timeframe is 0 or Nothing, does not adhere to the strategy:{0}", Me.ParentStrategy.ToString))
            End If
        End If
        _PreProcessingDone = False
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
                        If runningRawPayloadConsumer.OnwardLevelConsumers IsNot Nothing AndAlso runningRawPayloadConsumer.OnwardLevelConsumers.Count > 0 Then
                            For Each consumer In runningRawPayloadConsumer.OnwardLevelConsumers
                                candleCreator.IndicatorCreator.CalculateHeikinAshi(currentXMinute, consumer)
                                Dim counter As Integer = 0
                                For Each runningConsumer In consumer.OnwardLevelConsumers
                                    counter += 1
                                    If counter = 1 Then
                                        candleCreator.IndicatorCreator.CalculateVWAP(currentXMinute, runningConsumer)
                                        candleCreator.IndicatorCreator.CalculateEMA(currentXMinute, runningConsumer.OnwardLevelConsumers.FirstOrDefault)
                                    ElseIf counter = 2 Then
                                        candleCreator.IndicatorCreator.CalculatePivots(currentXMinute, runningConsumer)
                                    ElseIf counter = 3 Then
                                        candleCreator.IndicatorCreator.CalculateRSI(currentXMinute, runningConsumer)
                                    End If
                                Next
                            Next
                        End If
                    End If
                End If
            Next
        End If
    End Function

    Public Overrides Async Function MonitorAsync() As Task
        Try
            _strategyInstrumentRunning = True
            _ChartURL = String.Format(_ChartRawURL, Me.ParentStrategy.ParentController.APIConnection.ENCToken, Me.ParentStrategy.ParentController.APIConnection.APIUser.UserId, Me.TradableInstrument.RawExchange, Me.TradableInstrument.InstrumentIdentifier)
            Dim preProcess As Boolean = Await CompletePreProcessing().ConfigureAwait(False)
            If preProcess AndAlso _lastDayATR <> Decimal.MinValue Then
                _PreProcessingDone = True

                While True
                    If Me.ParentStrategy.ParentController.OrphanException IsNot Nothing Then
                        Throw Me.ParentStrategy.ParentController.OrphanException
                    End If
                    _cts.Token.ThrowIfCancellationRequested()
                    'Place Order block start
                    Dim placeOrderTriggers As List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)) = Await IsTriggerReceivedForPlaceOrderAsync(False).ConfigureAwait(False)
                    If placeOrderTriggers IsNot Nothing AndAlso placeOrderTriggers.Count > 0 AndAlso
                        placeOrderTriggers.FirstOrDefault.Item1 = ExecuteCommandAction.Take Then
                        Dim orderResponse = Await ExecuteCommandAsync(ExecuteCommands.PlaceBOSLMISOrder, Nothing).ConfigureAwait(False)
                        'If orderResponse IsNot Nothing AndAlso orderResponse.Count > 0 Then
                        '    Dim placeOrderResponse = CType(orderResponse, Concurrent.ConcurrentBag(Of Object)).FirstOrDefault
                        '    If placeOrderResponse.ContainsKey("data") AndAlso
                        '    placeOrderResponse("data").ContainsKey("order_id") Then
                        '        _cancellationDone = False
                        '    End If
                        'End If
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
                        Dim orderResponse = Await ExecuteCommandAsync(ExecuteCommands.CancelBOOrder, Nothing).ConfigureAwait(False)
                        'If orderResponse IsNot Nothing AndAlso orderResponse.Count > 0 Then
                        '    Dim exitOrderResponse = CType(orderResponse, Concurrent.ConcurrentBag(Of Object)).FirstOrDefault
                        '    If exitOrderResponse.ContainsKey("data") AndAlso
                        '        exitOrderResponse("data").ContainsKey("status") AndAlso exitOrderResponse("data")("status") = "Ok" Then
                        '        _cancellationDone = True
                        '    End If
                        'End If
                    End If
                    'Exit Order block end
                    _cts.Token.ThrowIfCancellationRequested()
                    Await Task.Delay(1000, _cts.Token).ConfigureAwait(False)
                End While
            End If
        Catch ex As Exception
            'To log exceptions getting created from this function as the bubble up of the exception
            'will anyways happen to Strategy.MonitorAsync but it will not be shown until all tasks exit
            logger.Error("Strategy Instrument:{0} stopped, error:{1}", Me.ToString, ex.ToString)
            Throw ex
        Finally
            _strategyInstrumentRunning = False
        End Try
    End Function

    Protected Overrides Async Function IsTriggerReceivedForPlaceOrderAsync(ByVal forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)))
        Dim ret As List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)) = Nothing
        Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
        Dim runningCandle As OHLCPayload = GetXMinuteCurrentCandle(userSettings.SignalTimeFrame)
        Dim currentTick As ITick = Me.TradableInstrument.LastTick
        Dim currentTime As Date = Now()

        Dim parameters As PlaceOrderParameters = Nothing
        If runningCandle IsNot Nothing AndAlso runningCandle.PayloadGeneratedBy = OHLCPayload.PayloadSource.CalculatedTick AndAlso
            runningCandle.PreviousPayload IsNot Nothing AndAlso Me.TradableInstrument.IsHistoricalCompleted AndAlso Not IsActiveInstrument() AndAlso
            Me.ParentStrategy.GetTotalPLAfterBrokerage() > userSettings.OverallMaxLoss AndAlso
            Me.ParentStrategy.GetTotalPLAfterBrokerage() < userSettings.OverallMaxProfit AndAlso Not Me.StrategyExitAllTriggerd Then
            Dim signal As Tuple(Of Boolean, IOrder.TypeOfTransaction, Decimal, Decimal, Decimal, Integer) = Await CheckSignalAsync(runningCandle).ConfigureAwait(False)
            If signal IsNot Nothing AndAlso signal.Item1 Then
                Dim signalCandle As OHLCPayload = runningCandle.PreviousPayload
                If signal.Item2 = IOrder.TypeOfTransaction.Buy Then
                    Dim triggerPrice As Decimal = signal.Item3
                    Dim price As Decimal = triggerPrice + CalculateLimitBuffer(triggerPrice)

                    If currentTick.LastPrice < triggerPrice Then
                        parameters = New PlaceOrderParameters(signalCandle) With
                                    {.EntryDirection = IOrder.TypeOfTransaction.Buy,
                                     .TriggerPrice = triggerPrice,
                                     .Price = price,
                                     .StoplossValue = signal.Item4,
                                     .SquareOffValue = signal.Item5,
                                     .OrderType = IOrder.TypeOfOrder.SL,
                                     .Quantity = signal.Item6}
                    End If
                ElseIf signal.Item2 = IOrder.TypeOfTransaction.Sell Then
                    Dim triggerPrice As Decimal = signal.Item3
                    Dim price As Decimal = triggerPrice - CalculateLimitBuffer(triggerPrice)

                    If currentTick.LastPrice > triggerPrice Then
                        parameters = New PlaceOrderParameters(signalCandle) With
                                    {.EntryDirection = IOrder.TypeOfTransaction.Sell,
                                     .TriggerPrice = triggerPrice,
                                     .Price = price,
                                     .StoplossValue = signal.Item4,
                                     .SquareOffValue = signal.Item5,
                                     .OrderType = IOrder.TypeOfOrder.SL,
                                     .Quantity = signal.Item6}
                    End If
                End If
            End If
        End If

        'Below portion have to be done in every place order trigger
        If parameters IsNot Nothing Then
            Try
                If forcePrint Then
                    logger.Debug("PlaceOrder-> ************************************************ {0}", Me.TradableInstrument.TradingSymbol)
                    logger.Debug("PlaceOrder Parameters-> {0}, Turnover:{1}, {2}", parameters.ToString, parameters.Quantity * parameters.TriggerPrice, Me.TradableInstrument.TradingSymbol)
                End If
            Catch ex As Exception
                logger.Warn(ex)
            End Try

            Dim parametersList As List(Of PlaceOrderParameters) = New List(Of PlaceOrderParameters) From {parameters}
            If parametersList IsNot Nothing AndAlso parametersList.Count > 0 Then
                For Each runningParameter In parametersList
                    Dim currentSignalActivities As IEnumerable(Of ActivityDashboard) = Me.ParentStrategy.SignalManager.GetActiveSignalActivities(Me.TradableInstrument.InstrumentIdentifier)
                    If currentSignalActivities IsNot Nothing AndAlso currentSignalActivities.Count > 0 Then
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
                                ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.WaitAndTake, runningParameter, runningParameter.ToString))
                            ElseIf lastPlacedActivity.EntryActivity.RequestStatus = ActivityDashboard.SignalStatusType.Handled Then
                                If lastPlacedActivity.SignalDirection = runningParameter.EntryDirection Then
                                    If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                                    ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.DonotTake, runningParameter, runningParameter.ToString))
                                    Try
                                        logger.Debug("Will not take this trade as similar previous trade detected. Current Trade->{0}, Last Activity Direction:{1}, Last Activity Status:{2}, Trading Symbol:{3}",
                                             runningParameter.ToString, lastPlacedActivity.SignalDirection, lastPlacedActivity.EntryActivity.RequestStatus, Me.TradableInstrument.TradingSymbol)
                                    Catch ex As Exception
                                        logger.Warn(ex.ToString)
                                    End Try
                                Else
                                    If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                                    ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.Take, runningParameter, runningParameter.ToString))
                                End If
                            ElseIf lastPlacedActivity.EntryActivity.RequestStatus = ActivityDashboard.SignalStatusType.Activated Then
                                If lastPlacedActivity.SignalDirection = runningParameter.EntryDirection Then
                                    If lastPlacedActivity.CancelActivity.RequestStatus = ActivityDashboard.SignalStatusType.Activated Then
                                        If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                                        ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.Take, runningParameter, runningParameter.ToString))
                                    Else
                                        If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                                        ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.DonotTake, runningParameter, runningParameter.ToString))
                                        Try
                                            logger.Debug("Will not take this trade as similar previous trade detected. Current Trade->{0}, Last Activity Direction:{1}, Last Activity Status:{2}, Trading Symbol:{3}",
                                                 runningParameter.ToString, lastPlacedActivity.SignalDirection, lastPlacedActivity.EntryActivity.RequestStatus, Me.TradableInstrument.TradingSymbol)
                                        Catch ex As Exception
                                            logger.Warn(ex.ToString)
                                        End Try
                                    End If
                                Else
                                    If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                                    ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.Take, runningParameter, runningParameter.ToString))
                                End If
                            ElseIf lastPlacedActivity.EntryActivity.RequestStatus = ActivityDashboard.SignalStatusType.Rejected Then
                                If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                                ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.DonotTake, runningParameter, runningParameter.ToString))
                            Else
                                If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                                ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.Take, runningParameter, runningParameter.ToString))
                            End If
                        Else
                            If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                            ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.Take, runningParameter, runningParameter.ToString))
                        End If
                    Else
                        If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                        ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.Take, runningParameter, runningParameter.ToString))
                    End If
                Next
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
                    parentBussinessOrder.SLOrder IsNot Nothing AndAlso parentBussinessOrder.SLOrder.Count > 0 AndAlso
                    parentBussinessOrder.TargetOrder IsNot Nothing AndAlso parentBussinessOrder.TargetOrder.Count > 0 Then
                    Dim targetPrice As Decimal = parentBussinessOrder.TargetOrder.Average(Function(x) x.Price)
                    Dim currentTick As ITick = Me.TradableInstrument.LastTick
                    If targetPrice <> Decimal.MinValue Then
                        For Each runningSLOrder In parentBussinessOrder.SLOrder
                            If Not runningSLOrder.Status = IOrder.TypeOfStatus.Complete AndAlso
                            Not runningSLOrder.Status = IOrder.TypeOfStatus.Cancelled AndAlso
                            Not runningSLOrder.Status = IOrder.TypeOfStatus.Rejected Then
                                Dim triggerPrice As Decimal = Decimal.MinValue
                                If parentBussinessOrder.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Buy Then
                                    Dim plPoint As Decimal = currentTick.LastPrice - parentBussinessOrder.ParentOrder.AveragePrice
                                    Dim targetPoint As Decimal = targetPrice - parentBussinessOrder.ParentOrder.AveragePrice
                                    If plPoint >= targetPoint / 2 Then
                                        triggerPrice = ConvertFloorCeling(parentBussinessOrder.ParentOrder.AveragePrice, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                                    End If
                                ElseIf parentBussinessOrder.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Sell Then
                                    Dim plPoint As Decimal = parentBussinessOrder.ParentOrder.AveragePrice - currentTick.LastPrice
                                    Dim targetPoint As Decimal = parentBussinessOrder.ParentOrder.AveragePrice - targetPrice
                                    If plPoint >= targetPoint / 2 Then
                                        triggerPrice = ConvertFloorCeling(parentBussinessOrder.ParentOrder.AveragePrice, Me.TradableInstrument.TickSize, RoundOfType.Celing)
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
                                    ret.Add(New Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)(ExecuteCommandAction.Take, runningSLOrder, triggerPrice, "Breakeven Movement"))
                                End If
                            End If
                        Next
                    End If
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

    Protected Overrides Async Function IsTriggerReceivedForExitOrderAsync(ByVal forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, String)))
        Dim ret As List(Of Tuple(Of ExecuteCommandAction, IOrder, String)) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        Dim runningCandle As OHLCPayload = GetXMinuteCurrentCandle(Me.ParentStrategy.UserSettings.SignalTimeFrame)
        Dim currentTick As ITick = Me.TradableInstrument.LastTick

        If runningCandle IsNot Nothing AndAlso runningCandle.PreviousPayload IsNot Nothing AndAlso Me.TradableInstrument.IsHistoricalCompleted Then
            Dim allActiveOrders As List(Of IOrder) = GetAllActiveOrders(IOrder.TypeOfTransaction.None)
            If allActiveOrders IsNot Nothing AndAlso allActiveOrders.Count > 0 Then
                Dim parentOrders As List(Of IOrder) = allActiveOrders.FindAll(Function(x)
                                                                                  Return x.ParentOrderIdentifier Is Nothing AndAlso
                                                                                  x.Status = IOrder.TypeOfStatus.TriggerPending
                                                                              End Function)
                If parentOrders IsNot Nothing AndAlso parentOrders.Count > 0 Then
                    For Each runningOrder In parentOrders
                        If runningOrder.Status = IOrder.TypeOfStatus.TriggerPending Then
                            Dim bussinessOrder As IBusinessOrder = OrderDetails(runningOrder.OrderIdentifier)
                            Dim exitTrade As Boolean = False
                            Dim reason As String = ""
                            Dim signal As Tuple(Of Boolean, IOrder.TypeOfTransaction, Decimal, Decimal, Decimal, Integer) = Await CheckSignalAsync(runningCandle).ConfigureAwait(False)
                            If signal IsNot Nothing AndAlso signal.Item1 Then
                                If bussinessOrder.ParentOrder.TransactionType = signal.Item2 Then
                                    If signal.Item2 = IOrder.TypeOfTransaction.Buy Then
                                        If bussinessOrder.ParentOrder.TriggerPrice <> signal.Item3 AndAlso
                                            currentTick.LastPrice < signal.Item3 Then
                                            exitTrade = True
                                            reason = "New entry signal"
                                        End If
                                    ElseIf signal.Item2 = IOrder.TypeOfTransaction.Sell Then
                                        If bussinessOrder.ParentOrder.TriggerPrice <> signal.Item3 AndAlso
                                            currentTick.LastPrice > signal.Item3 Then
                                            exitTrade = True
                                            reason = "New entry signal"
                                        End If
                                    End If
                                ElseIf bussinessOrder.ParentOrder.TransactionType <> signal.Item2 Then
                                    exitTrade = True
                                    reason = "Opposite direction signal"
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
                                ret.Add(New Tuple(Of ExecuteCommandAction, IOrder, String)(ExecuteCommandAction.Take, runningOrder, reason))
                            End If
                        End If
                    Next
                End If
            End If
        End If
        If forcePrint AndAlso ret IsNot Nothing AndAlso ret.Count > 0 Then
            Try
                For Each runningOrder In ret
                    OnHeartbeat(String.Format("***** Exit Order ***** Order ID:{0}, Reason:{1}, {2}", runningOrder.Item2.OrderIdentifier, runningOrder.Item3, Me.TradableInstrument.TradingSymbol))
                Next
            Catch ex As Exception
                logger.Warn(ex)
            End Try
        End If
        Return ret
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

#Region "Signal Check"
    Private Async Function CheckSignalAsync(ByVal runningCandle As OHLCPayload) As Task(Of Tuple(Of Boolean, IOrder.TypeOfTransaction, Decimal, Decimal, Decimal, Integer))
        Dim ret As Tuple(Of Boolean, IOrder.TypeOfTransaction, Decimal, Decimal, Decimal, Integer) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        Dim currentTime As Date = Now()
        Dim currentTick As ITick = Me.TradableInstrument.LastTick
        Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
        'Dim runningCandle As OHLCPayload = GetXMinuteCurrentCandle(userSettings.SignalTimeFrame)
        Dim hkData As HeikinAshiConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummyHKConsumer)
        Dim vwapData As VWAPConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummyVWAPConsumer)
        Dim emaData As EMAConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummyEMAConsumer)
        Dim pivotData As PivotsConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummyPivotConsumer)
        Dim rsiData As RSIConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummyRSIConsumer)

        If currentTime >= Me.TradableInstrument.ExchangeDetails.ExchangeStartTime AndAlso currentTime <= Me.TradableInstrument.ExchangeDetails.ExchangeEndTime AndAlso
            runningCandle IsNot Nothing AndAlso runningCandle.SnapshotDateTime >= Me.TradableInstrument.ExchangeDetails.ExchangeStartTime AndAlso
            runningCandle.PayloadGeneratedBy = OHLCPayload.PayloadSource.CalculatedTick AndAlso Me.TradableInstrument.IsHistoricalCompleted Then
            If runningCandle IsNot Nothing AndAlso runningCandle.PreviousPayload IsNot Nothing AndAlso currentTime < userSettings.EODExitTime Then
                Dim log As Boolean = False
                If Not runningCandle.PreviousPayload.ToString = _lastPrevPayloadString Then
                    log = True
                    _lastPrevPayloadString = runningCandle.PreviousPayload.ToString
                End If

                If hkData.ConsumerPayloads IsNot Nothing AndAlso hkData.ConsumerPayloads.ContainsKey(runningCandle.PreviousPayload.SnapshotDateTime) Then
                    Dim hkCandle As OHLCPayload = hkData.ConsumerPayloads(runningCandle.PreviousPayload.SnapshotDateTime)
                    If vwapData.ConsumerPayloads IsNot Nothing AndAlso vwapData.ConsumerPayloads.ContainsKey(runningCandle.PreviousPayload.SnapshotDateTime) Then
                        Dim vwap As VWAPConsumer.VWAPPayload = vwapData.ConsumerPayloads(runningCandle.PreviousPayload.SnapshotDateTime)
                        If emaData.ConsumerPayloads IsNot Nothing AndAlso emaData.ConsumerPayloads.ContainsKey(runningCandle.PreviousPayload.SnapshotDateTime) Then
                            Dim vwapEMA As EMAConsumer.EMAPayload = emaData.ConsumerPayloads(runningCandle.PreviousPayload.SnapshotDateTime)
                            If pivotData.ConsumerPayloads IsNot Nothing AndAlso pivotData.ConsumerPayloads.ContainsKey(runningCandle.PreviousPayload.SnapshotDateTime) Then
                                Dim pivots As PivotsConsumer.PivotsPayload = pivotData.ConsumerPayloads(runningCandle.PreviousPayload.SnapshotDateTime)
                                If rsiData.ConsumerPayloads IsNot Nothing AndAlso rsiData.ConsumerPayloads.ContainsKey(runningCandle.PreviousPayload.SnapshotDateTime) Then
                                    Dim rsi As RSIConsumer.RSIPayload = rsiData.ConsumerPayloads(runningCandle.PreviousPayload.SnapshotDateTime)

                                    Dim signalCandle As OHLCPayload = hkCandle
                                    If signalCandle IsNot Nothing AndAlso signalCandle.PreviousPayload IsNot Nothing AndAlso
                                        signalCandle.PreviousPayload.SnapshotDateTime.Date = Now.Date Then
                                        Dim message As String = String.Format("{0} ->Signal Candle Time:{1}.",
                                                                              Me.TradableInstrument.TradingSymbol,
                                                                              signalCandle.SnapshotDateTime.ToString("HH:mm:ss"))
                                        Dim positiveSignal As Boolean = False

                                        If vwap.VWAP.Value > vwapEMA.EMA.Value Then 'Buy
                                            Dim takeTrade As Boolean = True
                                            message = String.Format("{0} VWAP({1})>MVWAP({2})[BUY]. [INFO1] [INFO2]",
                                                                    message, Math.Round(vwap.VWAP.Value, 2), Math.Round(vwapEMA.EMA.Value, 2))

                                            takeTrade = takeTrade And (signalCandle.CandleColor = Color.Green)
                                            message = String.Format("{0} Signal Candle Color({1})=Green[{2}].",
                                                                    message, signalCandle.CandleColor.Name, signalCandle.CandleColor = Color.Green)

                                            takeTrade = takeTrade And (signalCandle.PreviousPayload.CandleColor = Color.Red)
                                            message = String.Format("{0} Previous Candle Color({1})=Red[{2}].",
                                                                    message, signalCandle.PreviousPayload.CandleColor.Name, signalCandle.PreviousPayload.CandleColor = Color.Red)

                                            takeTrade = takeTrade And (signalCandle.HighPrice.Value > signalCandle.PreviousPayload.HighPrice.Value)
                                            message = String.Format("{0} Signal Candle High({1})>Previous Candle High({2})[{3}].",
                                                                    message, Math.Round(signalCandle.HighPrice.Value, 2),
                                                                    Math.Round(signalCandle.PreviousPayload.HighPrice.Value, 2),
                                                                    signalCandle.HighPrice.Value > signalCandle.PreviousPayload.HighPrice.Value)

                                            takeTrade = takeTrade And (signalCandle.LowPrice.Value > signalCandle.PreviousPayload.LowPrice.Value)
                                            message = String.Format("{0} Signal Candle Low:({1})>Previous Candle Low({2})[{3}]",
                                                                    message, Math.Round(signalCandle.LowPrice.Value, 2),
                                                                    Math.Round(signalCandle.PreviousPayload.LowPrice.Value, 2),
                                                                    signalCandle.LowPrice.Value > signalCandle.PreviousPayload.LowPrice.Value)

                                            'takeTrade = takeTrade And (signalCandle.ClosePrice.Value > vwap.VWAP.Value)
                                            'message = String.Format("{0} Signal Candle Close({1})>VWAP({2})[{3}].",
                                            '                        message, Math.Round(signalCandle.ClosePrice.Value, 2),
                                            '                        Math.Round(vwap.VWAP.Value, 2),
                                            '                        signalCandle.ClosePrice.Value > vwap.VWAP.Value)
                                            takeTrade = takeTrade And (signalCandle.HighPrice.Value > vwap.VWAP.Value)
                                            message = String.Format("{0} Signal Candle High({1})>VWAP({2})[{3}].",
                                                                    message, Math.Round(signalCandle.HighPrice.Value, 2),
                                                                    Math.Round(vwap.VWAP.Value, 2),
                                                                    signalCandle.HighPrice.Value > vwap.VWAP.Value)

                                            takeTrade = takeTrade And (vwap.VWAP.Value > pivots.Pivot.Value)
                                            message = String.Format("{0} VWAP({1})>Central Pivot({2})[{3}].",
                                                                    message, Math.Round(vwap.VWAP.Value, 2),
                                                                    Math.Round(pivots.Pivot.Value, 2),
                                                                    vwap.VWAP.Value > pivots.Pivot.Value)

                                            takeTrade = takeTrade And (rsi.RSI.Value > userSettings.RSILevel)
                                            message = String.Format("{0} RSI({1})>RSI Level({2})[{3}].",
                                                                    message, Math.Round(rsi.RSI.Value, 2),
                                                                    Math.Round(userSettings.RSILevel, 2),
                                                                    rsi.RSI.Value > userSettings.RSILevel)

                                            If Me.TradableInstrument.InstrumentType = IInstrument.TypeOfInstrument.Cash Then
                                                takeTrade = takeTrade And (currentTick.LastPrice > _lastDayMA)
                                                message = String.Format("{0} LTP({1})>Last Day MA({2})[{3}].",
                                                                        message, currentTick.LastPrice, Math.Round(_lastDayMA, 2), currentTick.LastPrice > _lastDayMA)
                                            End If

                                            If takeTrade Then
                                                Dim entryPrice As Decimal = ConvertFloorCeling(signalCandle.HighPrice.Value, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                                                Dim stoploss As Decimal = Decimal.MinValue
                                                Dim slRemark As String = ""
                                                If vwap.VWAP.Value > stoploss AndAlso vwap.VWAP.Value < entryPrice Then
                                                    stoploss = ConvertFloorCeling(vwap.VWAP.Value, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                                                    slRemark = "VWAP"
                                                End If
                                                If pivots.Pivot.Value > stoploss AndAlso pivots.Pivot.Value < entryPrice Then
                                                    stoploss = ConvertFloorCeling(pivots.Pivot.Value, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                                                    slRemark = "Central Pivot"
                                                End If
                                                If pivots.Resistance1 > stoploss AndAlso pivots.Resistance1 < entryPrice Then
                                                    stoploss = ConvertFloorCeling(pivots.Resistance1, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                                                    slRemark = "Resistance1"
                                                End If
                                                If pivots.Resistance2 > stoploss AndAlso pivots.Resistance2 < entryPrice Then
                                                    stoploss = ConvertFloorCeling(pivots.Resistance2, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                                                    slRemark = "Resistance2"
                                                End If
                                                If pivots.Resistance3 > stoploss AndAlso pivots.Resistance3 < entryPrice Then
                                                    stoploss = ConvertFloorCeling(pivots.Resistance3, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                                                    slRemark = "Resistance3"
                                                End If
                                                If pivots.Support1 > stoploss AndAlso pivots.Support1 < entryPrice Then
                                                    stoploss = ConvertFloorCeling(pivots.Support1, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                                                    slRemark = "Support1"
                                                End If
                                                If pivots.Support2 > stoploss AndAlso pivots.Support2 < entryPrice Then
                                                    stoploss = ConvertFloorCeling(pivots.Support2, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                                                    slRemark = "Support2"
                                                End If
                                                If pivots.Support3 > stoploss AndAlso pivots.Support3 < entryPrice Then
                                                    stoploss = ConvertFloorCeling(pivots.Support3, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                                                    slRemark = "Support3"
                                                End If

                                                message = message.Replace("[INFO1]", String.Format("Entry:{0}, Stoploss:{1}({2}).", entryPrice, stoploss, slRemark))

                                                'Dim slPoint As Decimal = entryPrice - stoploss
                                                'If Me.TradableInstrument.InstrumentType <> IInstrument.TypeOfInstrument.Cash Then
                                                '    takeTrade = takeTrade And (slPoint < instrumentData.Range)

                                                '    message = String.Format("{0} SL Point({1})<Range({2})[{3}].",
                                                '                            message, slPoint, instrumentData.Range, slPoint < instrumentData.Range)
                                                'End If

                                                If takeTrade Then
                                                    Dim entryBuffer As Decimal = CalculateTriggerBuffer(entryPrice)
                                                    entryPrice = entryPrice + entryBuffer
                                                    Dim slBuffer As Decimal = CalculateStoplossBuffer(stoploss)
                                                    stoploss = stoploss - slBuffer

                                                    Dim targetPoint As Decimal = ConvertFloorCeling((entryPrice - stoploss) * userSettings.TargetMultiplier, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                                                    Dim target As Decimal = entryPrice + targetPoint
                                                    Dim moved As Decimal = entryPrice - currentTick.Low

                                                    message = message.Replace("[INFO2]", String.Format("Target:{0}, Day Low:{1}, Moved:{2}, Last Day ATR:{3}.", target, currentTick.Low, moved, Math.Round(_lastDayATR, 2)))

                                                    Dim leftOverMovement As Decimal = _lastDayATR - moved
                                                    takeTrade = takeTrade And (targetPoint < leftOverMovement * userSettings.TargetToLeftMovementPercentage / 100)
                                                    message = String.Format("{0} Target Point({1})<{2}% Movement left({3})[{4}]",
                                                                            message,
                                                                            targetPoint,
                                                                            userSettings.TargetToLeftMovementPercentage,
                                                                            Math.Round(leftOverMovement, 2),
                                                                            targetPoint < leftOverMovement * userSettings.TargetToLeftMovementPercentage / 100)

                                                    If takeTrade Then
                                                        Dim quantity As Integer = Me.TradableInstrument.LotSize
                                                        If Me.TradableInstrument.InstrumentType = IInstrument.TypeOfInstrument.Cash Then
                                                            quantity = CalculateQuantityFromStoploss(entryPrice, stoploss, userSettings.NSEMaxLossPerTrade)
                                                        End If

                                                        takeTrade = takeTrade And (quantity > 0)
                                                        message = String.Format("{0} Quantity({1})>0[{2}]",
                                                                            message,
                                                                            quantity,
                                                                            quantity > 0)

                                                        If takeTrade Then
                                                            Dim entryMessage As String = String.Format("BUY - {0} - Entry:{1} - Stoploss:{2}(Rs. {3})({4}) - Target:{5}(Rs. {6}) - Quantity:{7} - Signal Candle:{8}.{9}{10}",
                                                                                                         Me.TradableInstrument.TradingSymbol,
                                                                                                         entryPrice,
                                                                                                         stoploss,
                                                                                                         entryPrice - stoploss,
                                                                                                         slRemark,
                                                                                                         target,
                                                                                                         target - entryPrice,
                                                                                                         quantity,
                                                                                                         signalCandle.SnapshotDateTime.ToString("HH:mm:ss"),
                                                                                                         vbNewLine,
                                                                                                         _ChartURL)

                                                            If log Then SendTradeAlertMessageAsync(entryMessage)
                                                            positiveSignal = True

                                                            ret = New Tuple(Of Boolean, IOrder.TypeOfTransaction, Decimal, Decimal, Decimal, Integer)(True, IOrder.TypeOfTransaction.Buy, entryPrice, entryPrice - stoploss, target - entryPrice, quantity)
                                                        End If
                                                    End If
                                                Else
                                                    message = message.Replace("[INFO2]", "")
                                                End If
                                            Else
                                                message = message.Replace("[INFO1]", "")
                                                message = message.Replace("[INFO2]", "")
                                            End If
                                        ElseIf vwap.VWAP.Value < vwapEMA.EMA.Value Then 'Sell
                                            Dim takeTrade As Boolean = True
                                            message = String.Format("{0} VWAP({1})<MVWAP({2})[SELL]. [INFO1] [INFO2]",
                                                                    message, Math.Round(vwap.VWAP.Value, 2), Math.Round(vwapEMA.EMA.Value, 2))

                                            takeTrade = takeTrade And (signalCandle.CandleColor = Color.Red)
                                            message = String.Format("{0} Signal Candle Color({1})=Red[{2}].",
                                                                    message, signalCandle.CandleColor.Name, signalCandle.CandleColor = Color.Red)

                                            takeTrade = takeTrade And (signalCandle.PreviousPayload.CandleColor = Color.Green)
                                            message = String.Format("{0} Previous Candle Color({1})=Green[{2}].",
                                                                    message, signalCandle.PreviousPayload.CandleColor.Name, signalCandle.PreviousPayload.CandleColor = Color.Green)

                                            takeTrade = takeTrade And (signalCandle.HighPrice.Value < signalCandle.PreviousPayload.HighPrice.Value)
                                            message = String.Format("{0} Signal Candle High({1})<Previous Candle High({2})[{3}].",
                                                                    message, Math.Round(signalCandle.HighPrice.Value, 2),
                                                                    Math.Round(signalCandle.PreviousPayload.HighPrice.Value, 2),
                                                                    signalCandle.HighPrice.Value < signalCandle.PreviousPayload.HighPrice.Value)

                                            takeTrade = takeTrade And (signalCandle.LowPrice.Value < signalCandle.PreviousPayload.LowPrice.Value)
                                            message = String.Format("{0} Signal Candle Low:({1})<Previous Candle Low({2})[{3}]",
                                                                    message, Math.Round(signalCandle.LowPrice.Value, 2),
                                                                    Math.Round(signalCandle.PreviousPayload.LowPrice.Value, 2),
                                                                    signalCandle.LowPrice.Value < signalCandle.PreviousPayload.LowPrice.Value)

                                            'takeTrade = takeTrade And (signalCandle.ClosePrice.Value < vwap.VWAP.Value)
                                            'message = String.Format("{0} Signal Candle Close({1})<VWAP({2})[{3}].",
                                            '                        message, Math.Round(signalCandle.ClosePrice.Value, 2),
                                            '                        Math.Round(vwap.VWAP.Value, 2),
                                            '                        signalCandle.ClosePrice.Value < vwap.VWAP.Value)
                                            takeTrade = takeTrade And (signalCandle.LowPrice.Value < vwap.VWAP.Value)
                                            message = String.Format("{0} Signal Candle Low({1})<VWAP({2})[{3}].",
                                                                    message, Math.Round(signalCandle.LowPrice.Value, 2),
                                                                    Math.Round(vwap.VWAP.Value, 2),
                                                                    signalCandle.LowPrice.Value < vwap.VWAP.Value)

                                            takeTrade = takeTrade And (vwap.VWAP.Value < pivots.Pivot.Value)
                                            message = String.Format("{0} VWAP({1})<Central Pivot({2})[{3}].",
                                                                    message, Math.Round(vwap.VWAP.Value, 2),
                                                                    Math.Round(pivots.Pivot.Value, 2),
                                                                    vwap.VWAP.Value < pivots.Pivot.Value)

                                            takeTrade = takeTrade And (rsi.RSI.Value < userSettings.RSILevel)
                                            message = String.Format("{0} RSI({1})<RSI Level({2})[{3}].",
                                                                    message, Math.Round(rsi.RSI.Value, 2),
                                                                    Math.Round(userSettings.RSILevel, 2),
                                                                    rsi.RSI.Value < userSettings.RSILevel)

                                            If Me.TradableInstrument.InstrumentType = IInstrument.TypeOfInstrument.Cash Then
                                                takeTrade = takeTrade And (currentTick.LastPrice < _lastDayMA)
                                                message = String.Format("{0} LTP({1})<Last Day MA({2})[{3}].",
                                                                        message, currentTick.LastPrice, Math.Round(_lastDayMA, 2), currentTick.LastPrice < _lastDayMA)
                                            End If

                                            If takeTrade Then
                                                Dim entryPrice As Decimal = ConvertFloorCeling(signalCandle.LowPrice.Value, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                                                Dim stoploss As Decimal = Decimal.MaxValue
                                                Dim slRemark As String = ""
                                                If vwap.VWAP.Value < stoploss AndAlso vwap.VWAP.Value > entryPrice Then
                                                    stoploss = ConvertFloorCeling(vwap.VWAP.Value, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                                                    slRemark = "VWAP"
                                                End If
                                                If pivots.Pivot.Value < stoploss AndAlso pivots.Pivot.Value > entryPrice Then
                                                    stoploss = ConvertFloorCeling(pivots.Pivot.Value, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                                                    slRemark = "Central Pivot"
                                                End If
                                                If pivots.Resistance1 < stoploss AndAlso pivots.Resistance1 > entryPrice Then
                                                    stoploss = ConvertFloorCeling(pivots.Resistance1, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                                                    slRemark = "Resistance1"
                                                End If
                                                If pivots.Resistance2 < stoploss AndAlso pivots.Resistance2 > entryPrice Then
                                                    stoploss = ConvertFloorCeling(pivots.Resistance2, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                                                    slRemark = "Resistance2"
                                                End If
                                                If pivots.Resistance3 < stoploss AndAlso pivots.Resistance3 > entryPrice Then
                                                    stoploss = ConvertFloorCeling(pivots.Resistance3, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                                                    slRemark = "Resistance3"
                                                End If
                                                If pivots.Support1 < stoploss AndAlso pivots.Support1 > entryPrice Then
                                                    stoploss = ConvertFloorCeling(pivots.Support1, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                                                    slRemark = "Support1"
                                                End If
                                                If pivots.Support2 < stoploss AndAlso pivots.Support2 > entryPrice Then
                                                    stoploss = ConvertFloorCeling(pivots.Support2, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                                                    slRemark = "Support2"
                                                End If
                                                If pivots.Support3 < stoploss AndAlso pivots.Support3 > entryPrice Then
                                                    stoploss = ConvertFloorCeling(pivots.Support3, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                                                    slRemark = "Support3"
                                                End If

                                                message = message.Replace("[INFO1]", String.Format("Entry:{0}, Stoploss:{1}({2}).", entryPrice, stoploss, slRemark))

                                                'Dim slPoint As Decimal = stoploss - entryPrice
                                                'If Me.TradableInstrument.InstrumentType <> IInstrument.TypeOfInstrument.Cash Then
                                                '    takeTrade = takeTrade And (slPoint < instrumentData.Range)

                                                '    message = String.Format("{0} SL Point({1})<Range({2})[{3}].",
                                                '                            message, slPoint, instrumentData.Range, slPoint < instrumentData.Range)
                                                'End If

                                                If takeTrade Then
                                                    Dim entryBuffer As Decimal = CalculateTriggerBuffer(entryPrice)
                                                    entryPrice = entryPrice - entryBuffer
                                                    Dim slBuffer As Decimal = CalculateStoplossBuffer(stoploss)
                                                    stoploss = stoploss + slBuffer

                                                    Dim targetPoint As Decimal = ConvertFloorCeling((stoploss - entryPrice) * userSettings.TargetMultiplier, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                                                    Dim target As Decimal = entryPrice - targetPoint
                                                    Dim moved As Decimal = currentTick.High - entryPrice

                                                    message = message.Replace("[INFO2]", String.Format("Target:{0}, Day High:{1}, Moved:{2}, Last Day ATR:{3}.", target, currentTick.High, moved, Math.Round(_lastDayATR, 2)))

                                                    Dim leftOverMovement As Decimal = _lastDayATR - moved
                                                    takeTrade = takeTrade And (targetPoint < leftOverMovement * userSettings.TargetToLeftMovementPercentage / 100)

                                                    message = String.Format("{0} Target Point({1})<{2}% Movement left({3})[{4}]",
                                                                            message,
                                                                            targetPoint,
                                                                            userSettings.TargetToLeftMovementPercentage,
                                                                            Math.Round(leftOverMovement, 2),
                                                                            targetPoint < leftOverMovement * userSettings.TargetToLeftMovementPercentage / 100)

                                                    If takeTrade Then
                                                        Dim quantity As Integer = Me.TradableInstrument.LotSize
                                                        If Me.TradableInstrument.InstrumentType = IInstrument.TypeOfInstrument.Cash Then
                                                            quantity = CalculateQuantityFromStoploss(stoploss, entryPrice, userSettings.NSEMaxLossPerTrade)
                                                        End If

                                                        takeTrade = takeTrade And (quantity > 0)
                                                        message = String.Format("{0} Quantity({1})>0[{2}]",
                                                                            message,
                                                                            quantity,
                                                                            quantity > 0)

                                                        If takeTrade Then
                                                            Dim entryMessage As String = String.Format("SELL - {0} - Entry:{1} - Stoploss:{2}(Rs. {3})({4}) - Target:{5}(Rs. {6}) - Quantity:{7} - Signal Candle:{8}.{9}{10}",
                                                                                                     Me.TradableInstrument.TradingSymbol,
                                                                                                     entryPrice,
                                                                                                     stoploss,
                                                                                                     stoploss - entryPrice,
                                                                                                     slRemark,
                                                                                                     target,
                                                                                                     entryPrice - target,
                                                                                                     quantity,
                                                                                                     signalCandle.SnapshotDateTime.ToString("HH:mm:ss"),
                                                                                                     vbNewLine,
                                                                                                     _ChartURL)

                                                            If log Then SendTradeAlertMessageAsync(entryMessage)
                                                            positiveSignal = True

                                                            ret = New Tuple(Of Boolean, IOrder.TypeOfTransaction, Decimal, Decimal, Decimal, Integer)(True, IOrder.TypeOfTransaction.Sell, entryPrice, stoploss - entryPrice, entryPrice - target, quantity)
                                                        End If
                                                    End If
                                                Else
                                                    message = message.Replace("[INFO2]", "")
                                                End If
                                            Else
                                                message = message.Replace("[INFO1]", "")
                                                message = message.Replace("[INFO2]", "")
                                            End If
                                        End If
                                        If message IsNot Nothing AndAlso message.Trim <> "" Then
                                            If positiveSignal Then
                                                message = String.Format("******* {0}", message)
                                            End If
                                            If log Then OnHeartbeat(message)
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
            End If
        End If
        Return ret
    End Function
#End Region

#Region "Required Functions"
    Private Function CalculateTriggerBuffer(ByVal price As Decimal) As Decimal
        Dim ret As Decimal = Me.TradableInstrument.TickSize
        If Me.TradableInstrument.ExchangeDetails.ExchangeType = TypeOfExchage.CDS Then
            ret = 0.0025
        ElseIf Me.TradableInstrument.ExchangeDetails.ExchangeType = TypeOfExchage.NSE Then
            If price <= 200 Then
                ret = 0.05
            ElseIf price > 200 AndAlso price <= 500 Then
                ret = 0.1
            ElseIf price > 500 AndAlso price <= 1000 Then
                ret = 0.1
            ElseIf price > 1000 Then
                ret = 0.5
            End If
        End If
        Return ret
    End Function

    Private Function CalculateLimitBuffer(ByVal price As Decimal) As Decimal
        Dim ret As Decimal = Me.TradableInstrument.TickSize
        If Me.TradableInstrument.ExchangeDetails.ExchangeType = TypeOfExchage.CDS Then
            ret = 0.0025
        ElseIf Me.TradableInstrument.ExchangeDetails.ExchangeType = TypeOfExchage.NSE Then
            If price <= 200 Then
                ret = 0.05
            ElseIf price > 200 AndAlso price <= 500 Then
                ret = 0.1
            ElseIf price > 500 AndAlso price <= 1000 Then
                ret = 0.2
            ElseIf price > 1000 Then
                ret = 0.5
            End If
        End If
        Return ret
    End Function

    Private Function CalculateStoplossBuffer(ByVal price As Decimal) As Decimal
        Dim ret As Decimal = Me.TradableInstrument.TickSize
        If Me.TradableInstrument.ExchangeDetails.ExchangeType = TypeOfExchage.CDS Then
            ret = 0.005
        ElseIf Me.TradableInstrument.ExchangeDetails.ExchangeType = TypeOfExchage.NSE Then
            If price <= 200 Then
                ret = 0.2
            ElseIf price > 200 AndAlso price <= 500 Then
                ret = 0.5
            ElseIf price > 500 AndAlso price <= 1000 Then
                ret = 1
            ElseIf price > 1000 Then
                ret = 2
            End If
        End If
        Return ret
    End Function

    Private Async Function CompletePreProcessing() As Task(Of Boolean)
        Dim ret As Boolean = False
        Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
        Dim eodPayload As Dictionary(Of Date, OHLCPayload) = Await GetEODHistoricalDataAsync(Me.TradableInstrument, Now.Date.AddYears(-1), Now.Date).ConfigureAwait(False)
        If eodPayload IsNot Nothing AndAlso eodPayload.Count > 0 Then
            Dim atrPayload As Dictionary(Of Date, Decimal) = Nothing
            CalculateATR(userSettings.DayClose_ATRPeriod, eodPayload, atrPayload)
            _lastDayATR = atrPayload.LastOrDefault.Value

            If Me.TradableInstrument.InstrumentType = IInstrument.TypeOfInstrument.Cash Then
                Dim smaPayload As Dictionary(Of Date, Decimal) = Nothing
                CalculateSMA(userSettings.DayClose_SMAPeriod, eodPayload, smaPayload)
                _lastDayMA = smaPayload.LastOrDefault.Value
                ret = True
            Else
                ret = True
            End If
        End If
        Return ret
    End Function

    Private Async Function SendTradeAlertMessageAsync(ByVal message As String) As Task
        Try
            Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
            _cts.Token.ThrowIfCancellationRequested()
            Dim userInputs As NFOUserInputs = Me.ParentStrategy.UserSettings
            If userInputs.TelegramAPIKey IsNot Nothing AndAlso Not userInputs.TelegramAPIKey.Trim = "" AndAlso
                userInputs.TelegramChatID IsNot Nothing AndAlso Not userInputs.TelegramChatID.Trim = "" Then
                Using tSender As New Utilities.Notification.Telegram(userInputs.TelegramAPIKey.Trim, userInputs.TelegramChatID.Trim, _cts)
                    Dim encodedString As String = Utilities.Strings.UrlEncodeString(message)
                    Await tSender.SendMessageGetAsync(encodedString).ConfigureAwait(False)
                End Using
            End If
        Catch ex As Exception
            logger.Warn(ex.ToString)
        End Try
    End Function
#End Region

#Region "Indicator"
    Private Sub CalculateATR(ByVal ATRPeriod As Integer, ByVal inputPayload As Dictionary(Of Date, OHLCPayload), ByRef outputPayload As Dictionary(Of Date, Decimal))
        If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
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
    Private Sub CalculateSMA(ByVal SMAPeriod As Integer, ByVal inputPayload As Dictionary(Of Date, OHLCPayload), ByRef outputPayload As Dictionary(Of Date, Decimal))
        If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
            Dim finalPriceToBeAdded As Decimal = 0
            For Each runningInputPayload In inputPayload

                'If it is less than IndicatorPeriod, we will need to take SMA of all previous prices, hence the call to GetSubPayload
                Dim previousNInputFieldPayload As List(Of KeyValuePair(Of DateTime, OHLCPayload)) = GetSubPayload(inputPayload, runningInputPayload.Key, SMAPeriod - 1, False)
                If previousNInputFieldPayload Is Nothing Then
                    finalPriceToBeAdded += runningInputPayload.Value.ClosePrice.Value
                ElseIf previousNInputFieldPayload IsNot Nothing AndAlso previousNInputFieldPayload.Count <= SMAPeriod - 1 Then 'Because the first field is handled outside
                    Dim totalOfAllPrices As Decimal = 0

                    totalOfAllPrices = runningInputPayload.Value.ClosePrice.Value
                    totalOfAllPrices += previousNInputFieldPayload.Sum(Function(s) s.Value.ClosePrice.Value)
                    finalPriceToBeAdded = totalOfAllPrices / (previousNInputFieldPayload.Count + 1)
                Else
                    Dim totalOfAllPrices As Decimal = 0
                    totalOfAllPrices = runningInputPayload.Value.ClosePrice.Value
                    totalOfAllPrices += previousNInputFieldPayload.Sum(Function(s) s.Value.ClosePrice.Value)
                    finalPriceToBeAdded = Math.Round((totalOfAllPrices / (previousNInputFieldPayload.Count + 1)), 2)
                End If
                If outputPayload Is Nothing Then outputPayload = New Dictionary(Of Date, Decimal)
                outputPayload.Add(runningInputPayload.Key, finalPriceToBeAdded)
            Next
        End If
    End Sub

    Private Function GetSubPayload(ByVal inputPayload As Dictionary(Of Date, OHLCPayload), ByVal beforeThisTime As DateTime, ByVal numberOfItemsToRetrive As Integer, ByVal includeTimePassedAsOneOftheItems As Boolean) As List(Of KeyValuePair(Of DateTime, OHLCPayload))
        Dim ret As List(Of KeyValuePair(Of DateTime, OHLCPayload)) = Nothing
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
#End Region

#Region "EOD Data Fetcher"
    Private Async Function GetEODHistoricalDataAsync(ByVal instrument As IInstrument, ByVal fromDate As Date, ByVal toDate As Date) As Task(Of Dictionary(Of Date, OHLCPayload))
        Dim ret As Dictionary(Of Date, OHLCPayload) = Nothing
        Dim historicalCandlesJSONDict As Dictionary(Of String, Object) = Nothing
        _cts.Token.ThrowIfCancellationRequested()
        Dim AliceEODHistoricalURL As String = "https://ant.aliceblueonline.com/api/v1/charts?exchange={0}&token={1}&candletype=3&starttime={2}&endtime={3}&type=historical"
        Dim historicalDataURL As String = String.Format(AliceEODHistoricalURL, instrument.RawExchange, instrument.InstrumentIdentifier, DateTimeToUnix(fromDate), DateTimeToUnix(toDate))
        Dim proxyToBeUsed As HttpProxy = Nothing
        Using browser As New HttpBrowser(proxyToBeUsed, Net.DecompressionMethods.GZip, New TimeSpan(0, 1, 0), _cts)
            'AddHandler browser.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete
            'AddHandler browser.Heartbeat, AddressOf OnHeartbeat
            'AddHandler browser.WaitingFor, AddressOf OnWaitingFor
            'AddHandler browser.DocumentRetryStatus, AddressOf OnDocumentRetryStatus

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
                Dim runningSnapshotTime As Date = UnixToDateTime(historicalCandle(0))

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

#Region "Not Implemented Functions"
    Protected Overrides Function IsTriggerReceivedForModifyTargetOrderAsync(forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)))
        Throw New NotImplementedException()
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