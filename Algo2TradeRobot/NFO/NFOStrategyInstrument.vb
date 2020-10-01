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

    Private _lastPrevPayloadPlaceOrder As String = ""
    Private _lastPrevPayloadCancelOrder As String = ""

    Private ReadOnly _dummyHKConsumer As HeikinAshiConsumer
    Private ReadOnly _dummyATRConsumer As ATRConsumer
    Private ReadOnly _dummyATRBandsConsumer As ATRBandsConsumer
    Private ReadOnly _dummyFractalConsumer As FractalConsumer

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
                Dim userInput As NFOUserInputs = Me.ParentStrategy.UserSettings
                Dim chartConsumer As PayloadToChartConsumer = New PayloadToChartConsumer(Me.ParentStrategy.UserSettings.SignalTimeFrame)
                Dim hkConsumer As HeikinAshiConsumer = New HeikinAshiConsumer(chartConsumer)
                hkConsumer.OnwardLevelConsumers = New List(Of IPayloadConsumer) From
                   {New ATRConsumer(hkConsumer, userInput.ATRPeriod),
                    New ATRBandsConsumer(hkConsumer, userInput.ATRBandPeriod, userInput.ATRBandShift, TypeOfField.Close),
                    New FractalConsumer(hkConsumer)}

                chartConsumer.OnwardLevelConsumers = New List(Of IPayloadConsumer) From {hkConsumer}
                RawPayloadDependentConsumers.Add(chartConsumer)

                _dummyHKConsumer = New HeikinAshiConsumer(chartConsumer)
                _dummyATRConsumer = New ATRConsumer(hkConsumer, userInput.ATRPeriod)
                _dummyATRBandsConsumer = New ATRBandsConsumer(hkConsumer, userInput.ATRBandPeriod, userInput.ATRBandShift, TypeOfField.Close)
                _dummyFractalConsumer = New FractalConsumer(hkConsumer)
            Else
                Throw New ApplicationException(String.Format("Signal Timeframe is 0 or Nothing, does not adhere to the strategy:{0}", Me.ParentStrategy.ToString))
            End If
        End If
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
                                For Each childConsumer In consumer.OnwardLevelConsumers
                                    counter += 1
                                    If counter = 1 Then
                                        candleCreator.IndicatorCreator.CalculateATR(currentXMinute, childConsumer)
                                    ElseIf counter = 2 Then
                                        candleCreator.IndicatorCreator.CalculateATRBands(currentXMinute, childConsumer)
                                    ElseIf counter = 3 Then
                                        candleCreator.IndicatorCreator.CalculateFractal(currentXMinute, childConsumer)
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
                    Dim orderResponse = Await ExecuteCommandAsync(ExecuteCommands.PlaceBOSLMISOrder, Nothing).ConfigureAwait(False)
                    If orderResponse IsNot Nothing AndAlso orderResponse.Count > 0 Then
                        Dim placeOrderResponse = CType(orderResponse, Concurrent.ConcurrentBag(Of Object)).FirstOrDefault
                        If placeOrderResponse.ContainsKey("data") AndAlso
                            placeOrderResponse("data").ContainsKey("order_id") Then
                            _cancellationDone = False
                            OnHeartbeat(String.Format("Place Order Response: {0}", Utilities.Strings.JsonSerialize(placeOrderResponse)))
                        End If
                    End If
                End If
                'Place Order block end
                _cts.Token.ThrowIfCancellationRequested()
                'Modify Order block start
                Dim modifyStoplossOrderTrigger As List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)) = Await IsTriggerReceivedForModifyStoplossOrderAsync(False).ConfigureAwait(False)
                If modifyStoplossOrderTrigger IsNot Nothing AndAlso modifyStoplossOrderTrigger.Count > 0 Then
                    Dim orderResponse = Await ExecuteCommandAsync(ExecuteCommands.ModifyStoplossOrder, Nothing).ConfigureAwait(False)
                    If orderResponse IsNot Nothing AndAlso orderResponse.Count > 0 Then
                        Dim modifyOrderResponse = CType(orderResponse, Concurrent.ConcurrentBag(Of Object)).FirstOrDefault
                        If modifyOrderResponse.ContainsKey("data") AndAlso
                            modifyOrderResponse("data").ContainsKey("status") AndAlso modifyOrderResponse("data")("status") = "Ok" Then
                            OnHeartbeat(String.Format("Modify Order Response: {0}", Utilities.Strings.JsonSerialize(modifyOrderResponse)))
                        End If
                    End If
                End If
                'Modify Order block end
                _cts.Token.ThrowIfCancellationRequested()
                'Exit Order block start
                Dim exitOrderTrigger As List(Of Tuple(Of ExecuteCommandAction, IOrder, String)) = Await IsTriggerReceivedForExitOrderAsync(False).ConfigureAwait(False)
                If exitOrderTrigger IsNot Nothing AndAlso exitOrderTrigger.Count > 0 Then
                    Dim orderResponse = Await ExecuteCommandAsync(ExecuteCommands.CancelBOOrder, Nothing).ConfigureAwait(False)
                    If orderResponse IsNot Nothing AndAlso orderResponse.Count > 0 Then
                        Dim exitOrderResponse = CType(orderResponse, Concurrent.ConcurrentBag(Of Object)).FirstOrDefault
                        If exitOrderResponse.ContainsKey("data") AndAlso
                            exitOrderResponse("data").ContainsKey("status") AndAlso exitOrderResponse("data")("status") = "Ok" Then
                            _cancellationDone = True
                            OnHeartbeat(String.Format("Exit Order Response: {0}", Utilities.Strings.JsonSerialize(exitOrderResponse)))
                        End If
                    End If
                End If
                'Exit Order block end
                _cts.Token.ThrowIfCancellationRequested()
                Await Task.Delay(1000, _cts.Token).ConfigureAwait(False)
            End While
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
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
        Dim runningCandle As OHLCPayload = GetXMinuteCurrentCandle(userSettings.SignalTimeFrame)
        Dim hkConsumer As HeikinAshiConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummyHKConsumer)
        Dim atrConsumer As ATRConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummyATRConsumer)
        Dim atrBandConsumer As ATRBandsConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummyATRBandsConsumer)
        Dim fractalConsumer As FractalConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummyFractalConsumer)

        Dim currentTick As ITick = Me.TradableInstrument.LastTick
        Dim currentTime As Date = Now()

        Try
            If runningCandle IsNot Nothing AndAlso runningCandle.PreviousPayload IsNot Nothing AndAlso Me.TradableInstrument.IsHistoricalCompleted Then
                If Not runningCandle.PreviousPayload.ToString = _lastPrevPayloadPlaceOrder OrElse forcePrint Then
                    _lastPrevPayloadPlaceOrder = runningCandle.PreviousPayload.ToString
                    Dim highestATR As Decimal = GetHighestATR(atrConsumer, runningCandle.PreviousPayload)
                    logger.Debug("PlaceOrder-> Potential Signal Candle is:{0}. Will check rest parameters.", hkConsumer.ConsumerPayloads(runningCandle.PreviousPayload.SnapshotDateTime).ToString)
                    logger.Debug("PlaceOrder-> Rest all parameters: Running Candle:{0}, PayloadGeneratedBy:{1}, IsHistoricalCompleted:{2}, IsFirstTimeInformationCollected:{3}, Is Active Trades:{4}, Is Target Reached:{5}, Highest ATR:{6}, Total PL:{7}, Stock PL:{8}, Number of Trade:{9}, {10}, {11}, {12}, Cancellation Done:{13}, Current Time:{14}, Current Tick:{15}, TradingSymbol:{16}",
                                runningCandle.SnapshotDateTime.ToString("dd-MM-yyyy HH:mm:ss"),
                                runningCandle.PayloadGeneratedBy.ToString,
                                Me.TradableInstrument.IsHistoricalCompleted,
                                Me.ParentStrategy.IsFirstTimeInformationCollected,
                                IsActiveInstrument(),
                                IsTargetReached(),
                                If(highestATR <> Decimal.MinValue, Math.Round(highestATR, 4), "∞"),
                                Me.ParentStrategy.GetTotalPLAfterBrokerage(),
                                Me.GetOverallPLAfterBrokerage(),
                                GetLogicalTradeCount(),
                                atrConsumer.ConsumerPayloads(runningCandle.PreviousPayload.SnapshotDateTime),
                                atrBandConsumer.ConsumerPayloads(runningCandle.PreviousPayload.SnapshotDateTime),
                                fractalConsumer.ConsumerPayloads(runningCandle.PreviousPayload.SnapshotDateTime),
                                _cancellationDone,
                                currentTime.ToString,
                                currentTick.LastPrice,
                                Me.TradableInstrument.TradingSymbol)
                End If
            End If
        Catch ex As Exception
            logger.Warn(ex)
        End Try

        Dim parameters As PlaceOrderParameters = Nothing
        If currentTime >= userSettings.TradeStartTime AndAlso currentTime <= userSettings.LastTradeEntryTime AndAlso currentTime <= userSettings.EODExitTime AndAlso
            runningCandle IsNot Nothing AndAlso runningCandle.SnapshotDateTime >= userSettings.TradeStartTime AndAlso
            runningCandle.PayloadGeneratedBy = OHLCPayload.PayloadSource.CalculatedTick AndAlso runningCandle.PreviousPayload IsNot Nothing AndAlso
            Me.TradableInstrument.IsHistoricalCompleted AndAlso GetLogicalTradeCount() < userSettings.NumberOfTradePerStock AndAlso Not IsTargetReached() AndAlso
            (Not IsActiveInstrument() OrElse _cancellationDone) AndAlso Me.ParentStrategy.GetTotalPLAfterBrokerage() > userSettings.OverallMaxLossPerDay AndAlso
            Me.ParentStrategy.GetTotalPLAfterBrokerage() < userSettings.OverallMaxProfitPerDay AndAlso Not Me.StrategyExitAllTriggerd AndAlso
            hkConsumer.ConsumerPayloads IsNot Nothing AndAlso hkConsumer.ConsumerPayloads.Count > 0 AndAlso
            hkConsumer.ConsumerPayloads.ContainsKey(runningCandle.PreviousPayload.SnapshotDateTime) Then
            Dim signal As Tuple(Of Boolean, Decimal, Decimal, OHLCPayload, IOrder.TypeOfTransaction) = GetSignalCandle(atrConsumer, atrBandConsumer, fractalConsumer, hkConsumer, runningCandle, forcePrint, currentTick)
            If signal IsNot Nothing AndAlso signal.Item1 Then
                Dim signalCandle As OHLCPayload = Nothing
                Dim lastExecutedOrder As IBusinessOrder = GetLastExecutedOrder()
                If lastExecutedOrder Is Nothing Then
                    signalCandle = signal.Item4
                Else
                    Dim lastOrderSignalCandle As OHLCPayload = GetSignalCandleOfAnOrder(lastExecutedOrder.ParentOrderIdentifier, userSettings.SignalTimeFrame)
                    If lastOrderSignalCandle IsNot Nothing AndAlso lastOrderSignalCandle.SnapshotDateTime <> signal.Item4.SnapshotDateTime Then
                        signalCandle = signal.Item4
                    End If
                End If
                If signalCandle IsNot Nothing Then
                    Dim triggerPrice As Decimal = signal.Item2
                    Dim slPoint As Decimal = signal.Item3

                    Dim currentPL As Decimal = Me.GetOverallPLAfterBrokerage()
                    Dim plToAchive As Decimal = userSettings.MaxLossPerTrade + currentPL
                    Dim quantity As Integer = CalculateQuantityFromStoploss(triggerPrice, triggerPrice - slPoint, plToAchive)

                    Dim targetPoint As Decimal = CalculateTargetFromPL(triggerPrice, quantity, Math.Abs(plToAchive)) - triggerPrice

                    If signal.Item5 = IOrder.TypeOfTransaction.Buy Then
                        Dim price As Decimal = triggerPrice + ConvertFloorCeling(triggerPrice * 0.3 / 100, TradableInstrument.TickSize, RoundOfType.Celing)

                        If currentTick.LastPrice < triggerPrice Then
                            parameters = New PlaceOrderParameters(signalCandle) With
                                        {.EntryDirection = IOrder.TypeOfTransaction.Buy,
                                         .TriggerPrice = triggerPrice,
                                         .Price = price,
                                         .StoplossValue = slPoint,
                                         .SquareOffValue = targetPoint,
                                         .OrderType = IOrder.TypeOfOrder.SL,
                                         .Quantity = quantity}
                        End If
                    ElseIf signal.Item5 = IOrder.TypeOfTransaction.Sell Then
                        Dim price As Decimal = triggerPrice - ConvertFloorCeling(triggerPrice * 0.3 / 100, TradableInstrument.TickSize, RoundOfType.Celing)

                        If currentTick.LastPrice > triggerPrice Then
                            parameters = New PlaceOrderParameters(signalCandle) With
                                        {.EntryDirection = IOrder.TypeOfTransaction.Sell,
                                         .TriggerPrice = triggerPrice,
                                         .Price = price,
                                         .StoplossValue = slPoint,
                                         .SquareOffValue = targetPoint,
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
                    logger.Debug("PlaceOrder Parameters-> {0}, Turnover:{1}, {2}", parameters.ToString, parameters.Quantity * parameters.TriggerPrice, Me.TradableInstrument.TradingSymbol)
                End If
            Catch ex As Exception
                logger.Warn(ex)
            End Try

            Dim parametersList As List(Of PlaceOrderParameters) = New List(Of PlaceOrderParameters)
            Dim turnover As Decimal = parameters.Quantity * parameters.TriggerPrice
            If turnover >= userSettings.MaxTurnoverOfATrade Then
                Dim split As Integer = Math.Ceiling(turnover / userSettings.MaxTurnoverOfATrade)
                Dim quantityOfEachSplit As Integer = Math.Ceiling(parameters.Quantity / split)
                For iteration As Integer = 1 To split
                    If iteration = split Then
                        Dim parameter As PlaceOrderParameters = New PlaceOrderParameters(parameters.SignalCandle) With
                                        {.EntryDirection = parameters.EntryDirection,
                                         .TriggerPrice = parameters.TriggerPrice,
                                         .Price = parameters.Price,
                                         .StoplossValue = parameters.StoplossValue,
                                         .SquareOffValue = parameters.SquareOffValue,
                                         .OrderType = parameters.OrderType,
                                         .Quantity = parameters.Quantity - (quantityOfEachSplit * (split - 1))}
                        parametersList.Add(parameter)
                    Else
                        Dim parameter As PlaceOrderParameters = New PlaceOrderParameters(parameters.SignalCandle) With
                                        {.EntryDirection = parameters.EntryDirection,
                                         .TriggerPrice = parameters.TriggerPrice,
                                         .Price = parameters.Price,
                                         .StoplossValue = parameters.StoplossValue,
                                         .SquareOffValue = parameters.SquareOffValue,
                                         .OrderType = parameters.OrderType,
                                         .Quantity = quantityOfEachSplit}
                        parametersList.Add(parameter)
                    End If
                Next
            Else
                parametersList.Add(parameters)
            End If

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
        Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
        Dim runningCandle As OHLCPayload = GetXMinuteCurrentCandle(userSettings.SignalTimeFrame)
        Dim atrBandConsumer As ATRBandsConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummyATRBandsConsumer)
        Dim fractalConsumer As FractalConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummyFractalConsumer)

        If OrderDetails IsNot Nothing AndAlso OrderDetails.Count > 0 AndAlso Me.TradableInstrument.IsHistoricalCompleted AndAlso
            runningCandle IsNot Nothing AndAlso runningCandle.PreviousPayload IsNot Nothing AndAlso fractalConsumer IsNot Nothing AndAlso
            atrBandConsumer IsNot Nothing AndAlso fractalConsumer.ConsumerPayloads IsNot Nothing AndAlso fractalConsumer.ConsumerPayloads.Count > 0 AndAlso
            fractalConsumer.ConsumerPayloads.ContainsKey(runningCandle.PreviousPayload.SnapshotDateTime) AndAlso atrBandConsumer.ConsumerPayloads IsNot Nothing AndAlso
            atrBandConsumer.ConsumerPayloads.Count > 0 AndAlso atrBandConsumer.ConsumerPayloads.ContainsKey(runningCandle.PreviousPayload.SnapshotDateTime) Then
            Dim fractal As FractalConsumer.FractalPayload = fractalConsumer.ConsumerPayloads(runningCandle.PreviousPayload.SnapshotDateTime)
            Dim highBandV As Decimal = GetATRUpperBandV(atrBandConsumer, runningCandle)
            Dim lowBandV As Decimal = GetATRLowerBandReverseV(atrBandConsumer, runningCandle)
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
                                Dim potentialSL As Decimal = Decimal.MinValue
                                If fractal.FractalLow.Value < parentBussinessOrder.ParentOrder.AveragePrice Then
                                    potentialSL = fractal.FractalLow.Value
                                    If lowBandV <> Decimal.MinValue AndAlso lowBandV > fractal.FractalLow.Value Then
                                        potentialSL = lowBandV
                                    End If
                                Else
                                    If lowBandV <> Decimal.MinValue Then potentialSL = lowBandV
                                End If
                                If potentialSL <> Decimal.MinValue AndAlso potentialSL > runningSLOrder.TriggerPrice Then
                                    triggerPrice = potentialSL
                                End If
                            ElseIf parentBussinessOrder.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Sell Then
                                Dim potentialSL As Decimal = Decimal.MinValue
                                If fractal.FractalHigh.Value > parentBussinessOrder.ParentOrder.AveragePrice Then
                                    potentialSL = fractal.FractalHigh.Value
                                    If highBandV <> Decimal.MinValue AndAlso highBandV < fractal.FractalHigh.Value Then
                                        potentialSL = highBandV
                                    End If
                                Else
                                    If highBandV <> Decimal.MinValue Then potentialSL = highBandV
                                End If
                                If potentialSL <> Decimal.MinValue AndAlso potentialSL < runningSLOrder.TriggerPrice Then
                                    triggerPrice = potentialSL
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
                                ret.Add(New Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)(ExecuteCommandAction.Take, runningSLOrder, triggerPrice, "SL adjustment"))
                            End If
                        End If
                    Next
                End If
            Next
        End If
        If forcePrint AndAlso ret IsNot Nothing AndAlso ret.Count > 0 Then
            For Each runningOrder In ret
                logger.Debug("***** Modify Stoploss ***** Order ID:{0}, Trigger Price:{1}, Reason:{2}, {3}",
                             runningOrder.Item2.OrderIdentifier, runningOrder.Item3, runningOrder.Item4, Me.TradableInstrument.TradingSymbol)
            Next
        End If
        Return ret
    End Function

    Protected Overrides Function IsTriggerReceivedForModifyTargetOrderAsync(forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)))
        Throw New NotImplementedException
    End Function

    Protected Overrides Async Function IsTriggerReceivedForExitOrderAsync(ByVal forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, String)))
        Dim ret As List(Of Tuple(Of ExecuteCommandAction, IOrder, String)) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
        Dim runningCandle As OHLCPayload = GetXMinuteCurrentCandle(userSettings.SignalTimeFrame)
        Dim hkConsumer As HeikinAshiConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummyHKConsumer)
        Dim atrConsumer As ATRConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummyATRConsumer)
        Dim atrBandConsumer As ATRBandsConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummyATRBandsConsumer)
        Dim fractalConsumer As FractalConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummyFractalConsumer)

        Dim currentTick As ITick = Me.TradableInstrument.LastTick

        Dim log As Boolean = False
        Try
            If runningCandle IsNot Nothing AndAlso runningCandle.PreviousPayload IsNot Nothing AndAlso
                Me.TradableInstrument.IsHistoricalCompleted Then
                If Not runningCandle.PreviousPayload.ToString = _lastPrevPayloadCancelOrder Then
                    _lastPrevPayloadCancelOrder = runningCandle.PreviousPayload.ToString
                    log = True
                End If
            End If
        Catch ex As Exception
            logger.Warn(ex)
        End Try

        If runningCandle IsNot Nothing AndAlso runningCandle.PreviousPayload IsNot Nothing AndAlso
            Me.TradableInstrument.IsHistoricalCompleted AndAlso
            hkConsumer.ConsumerPayloads IsNot Nothing AndAlso hkConsumer.ConsumerPayloads.Count > 0 AndAlso
            hkConsumer.ConsumerPayloads.ContainsKey(runningCandle.PreviousPayload.SnapshotDateTime) Then
            Dim allActiveOrders As List(Of IOrder) = GetAllActiveOrders(IOrder.TypeOfTransaction.None)
            If allActiveOrders IsNot Nothing AndAlso allActiveOrders.Count > 0 Then
                Dim parentOrders As List(Of IOrder) = allActiveOrders.FindAll(Function(x)
                                                                                  Return x.ParentOrderIdentifier Is Nothing AndAlso
                                                                              x.Status = IOrder.TypeOfStatus.TriggerPending
                                                                              End Function)
                If parentOrders IsNot Nothing AndAlso parentOrders.Count > 0 Then
                    Dim hkCandle As OHLCPayload = hkConsumer.ConsumerPayloads(runningCandle.PreviousPayload.SnapshotDateTime)
                    For Each runningOrder In parentOrders
                        If runningOrder.Status = IOrder.TypeOfStatus.TriggerPending Then
                            Dim bussinessOrder As IBusinessOrder = OrderDetails(runningOrder.OrderIdentifier)
                            Dim exitTrade As Boolean = False
                            Dim reason As String = ""
                            Dim signal As Tuple(Of Boolean, Decimal, Decimal, OHLCPayload, IOrder.TypeOfTransaction) = GetSignalCandle(atrConsumer, atrBandConsumer, fractalConsumer, hkConsumer, runningCandle, forcePrint, currentTick)
                            If signal IsNot Nothing AndAlso signal.Item1 Then
                                Dim validForCancellation As Boolean = False
                                Dim distance As Decimal = 0
                                If signal.Item5 = IOrder.TypeOfTransaction.Buy Then
                                    distance = ((signal.Item2 - currentTick.LastPrice) / currentTick.LastPrice) * 100
                                ElseIf signal.Item5 = IOrder.TypeOfTransaction.Sell Then
                                    distance = ((currentTick.LastPrice - signal.Item2) / currentTick.LastPrice) * 100
                                End If
                                If distance >= userSettings.MinDistancePercentageForCancellation Then
                                    validForCancellation = True
                                End If
                                'Try
                                '    If log OrElse forcePrint Then
                                '        OnHeartbeat(String.Format("New signal entry price:{0}, direction:{1}, ltp:{2}, Gap:{3}%, Signal Candle:{4}. So will{5} cancel trade, {6}",
                                '                              signal.Item2,
                                '                              signal.Item4.ToString,
                                '                              currentTick.LastPrice,
                                '                              Math.Round(distance, 3),
                                '                              signal.Item4.SnapshotDateTime.ToString("HH:mm:ss"),
                                '                              If(Not validForCancellation, " not", ""),
                                '                              Me.TradableInstrument.TradingSymbol))
                                '    Else
                                '        logger.Debug(String.Format("New signal entry price:{0}, direction:{1}, ltp:{2}, Gap:{3}%, Signal Candle:{4}. So will{5} cancel trade, {6}",
                                '                              signal.Item2,
                                '                              signal.Item4.ToString,
                                '                              currentTick.LastPrice,
                                '                              Math.Round(distance, 3),
                                '                              signal.Item4.SnapshotDateTime.ToString("HH:mm:ss"),
                                '                              If(Not validForCancellation, " not", ""),
                                '                              Me.TradableInstrument.TradingSymbol))
                                '    End If
                                'Catch ex As Exception
                                '    logger.Warn(ex.ToString)
                                'End Try
                                If validForCancellation Then
                                    If bussinessOrder.ParentOrder.TransactionType = signal.Item5 Then
                                        If signal.Item5 = IOrder.TypeOfTransaction.Buy Then
                                            If bussinessOrder.ParentOrder.TriggerPrice <> signal.Item2 AndAlso
                                                currentTick.LastPrice < signal.Item2 Then
                                                exitTrade = True
                                                reason = "New entry signal"
                                            End If
                                        ElseIf signal.Item5 = IOrder.TypeOfTransaction.Sell Then
                                            If bussinessOrder.ParentOrder.TriggerPrice <> signal.Item2 AndAlso
                                                currentTick.LastPrice > signal.Item2 Then
                                                exitTrade = True
                                                reason = "New entry signal"
                                            End If
                                        End If
                                    ElseIf bussinessOrder.ParentOrder.TransactionType <> signal.Item5 Then
                                        exitTrade = True
                                        reason = "Opposite direction signal"
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

    Private Function GetSignalCandle(ByVal atrConsumer As ATRConsumer, ByVal atrBandConsumer As ATRBandsConsumer,
                                     ByVal fractalConsumer As FractalConsumer, ByVal hkConsumer As HeikinAshiConsumer,
                                     ByVal currentCandle As OHLCPayload, ByVal forcePrint As Boolean, ByVal currentTick As ITick) As Tuple(Of Boolean, Decimal, Decimal, OHLCPayload, IOrder.TypeOfTransaction)
        Dim ret As Tuple(Of Boolean, Decimal, Decimal, OHLCPayload, IOrder.TypeOfTransaction) = Nothing
        If currentCandle IsNot Nothing AndAlso currentCandle.PreviousPayload IsNot Nothing Then
            Dim upperV As Decimal = Decimal.MinValue
            Dim upperVSignalCandle As OHLCPayload = Nothing
            Dim lowerV As Decimal = Decimal.MinValue
            Dim lowerVSignalCandle As OHLCPayload = Nothing

            If Me.OrderDetails IsNot Nothing AndAlso Me.OrderDetails.Count > 0 Then
                Dim lastPlacedOrder As IBusinessOrder = OrderDetails.OrderBy(Function(x)
                                                                                 Return x.Value.ParentOrder.TimeStamp
                                                                             End Function).LastOrDefault.Value
                For Each runningPayload In hkConsumer.ConsumerPayloads.OrderByDescending(Function(x)
                                                                                             Return x.Key
                                                                                         End Function)
                    If runningPayload.Key >= lastPlacedOrder.ParentOrder.TimeStamp AndAlso runningPayload.Key <= currentCandle.SnapshotDateTime Then
                        If upperV = Decimal.MinValue Then
                            upperV = GetATRUpperBandV(atrBandConsumer, runningPayload.Value)
                            If upperV <> Decimal.MinValue Then
                                'Dim entryPrice As Decimal = ConvertFloorCeling(upperV, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                                'If Not IsSignalTriggered(entryPrice, IOrder.TypeOfTransaction.Buy, CType(runningPayload.Value, OHLCPayload).PreviousPayload.SnapshotDateTime, currentCandle.PreviousPayload.SnapshotDateTime) Then
                                upperVSignalCandle = CType(runningPayload.Value, OHLCPayload).PreviousPayload
                                'End If
                            End If
                        End If
                        If lowerV = Decimal.MinValue Then
                            lowerV = GetATRLowerBandReverseV(atrBandConsumer, runningPayload.Value)
                            If lowerV <> Decimal.MinValue Then
                                'Dim entryPrice As Decimal = ConvertFloorCeling(lowerV, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                                'If Not IsSignalTriggered(entryPrice, IOrder.TypeOfTransaction.Sell, CType(runningPayload.Value, OHLCPayload).PreviousPayload.SnapshotDateTime, currentCandle.PreviousPayload.SnapshotDateTime) Then
                                lowerVSignalCandle = CType(runningPayload.Value, OHLCPayload).PreviousPayload
                                'End If
                            End If
                        End If
                        If upperV <> Decimal.MinValue AndAlso lowerV <> Decimal.MinValue Then
                            Exit For
                        End If
                    End If
                    If runningPayload.Key < lastPlacedOrder.ParentOrder.TimeStamp Then Exit For
                Next
            Else
                Dim fractalConstrictionDone As Tuple(Of Boolean, Date) = IsFractalConstrictionDone(fractalConsumer, hkConsumer, currentCandle)
                If fractalConstrictionDone IsNot Nothing AndAlso fractalConstrictionDone.Item1 Then
                    For Each runningPayload In hkConsumer.ConsumerPayloads.OrderBy(Function(x)
                                                                                       Return x.Key
                                                                                   End Function)
                        If runningPayload.Key >= fractalConstrictionDone.Item2 AndAlso runningPayload.Key <= currentCandle.SnapshotDateTime Then
                            If upperV = Decimal.MinValue Then
                                upperV = GetATRUpperBandV(atrBandConsumer, runningPayload.Value)
                                If upperV <> Decimal.MinValue Then
                                    'Dim entryPrice As Decimal = ConvertFloorCeling(upperV, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                                    'If Not IsSignalTriggered(entryPrice, IOrder.TypeOfTransaction.Buy, CType(runningPayload.Value, OHLCPayload).PreviousPayload.SnapshotDateTime, currentCandle.PreviousPayload.SnapshotDateTime) Then
                                    upperVSignalCandle = CType(runningPayload.Value, OHLCPayload).PreviousPayload
                                    'End If
                                End If
                            End If
                            If lowerV = Decimal.MinValue Then
                                lowerV = GetATRLowerBandReverseV(atrBandConsumer, runningPayload.Value)
                                If lowerV <> Decimal.MinValue Then
                                    'Dim entryPrice As Decimal = ConvertFloorCeling(lowerV, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                                    'If Not IsSignalTriggered(entryPrice, IOrder.TypeOfTransaction.Sell, CType(runningPayload.Value, OHLCPayload).PreviousPayload.SnapshotDateTime, currentCandle.PreviousPayload.SnapshotDateTime) Then
                                    lowerVSignalCandle = CType(runningPayload.Value, OHLCPayload).PreviousPayload
                                    'End If
                                End If
                            End If
                            If upperV <> Decimal.MinValue AndAlso lowerV <> Decimal.MinValue Then
                                Exit For
                            End If
                        End If
                    Next
                    If upperVSignalCandle IsNot Nothing AndAlso lowerVSignalCandle IsNot Nothing Then
                        Dim highEntryPrice As Decimal = ConvertFloorCeling(upperV, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                        Dim lowEntryPrice As Decimal = ConvertFloorCeling(lowerV, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                        If IsSignalTriggered(highEntryPrice, IOrder.TypeOfTransaction.Buy, lowerVSignalCandle.SnapshotDateTime, currentCandle.PreviousPayload.SnapshotDateTime) Then
                            upperVSignalCandle = Nothing
                            lowerVSignalCandle = Nothing
                        ElseIf IsSignalTriggered(lowEntryPrice, IOrder.TypeOfTransaction.Sell, lowerVSignalCandle.SnapshotDateTime, currentCandle.PreviousPayload.SnapshotDateTime) Then
                            upperVSignalCandle = Nothing
                            lowerVSignalCandle = Nothing
                        End If
                    End If
                End If
            End If
            If upperVSignalCandle IsNot Nothing AndAlso lowerVSignalCandle IsNot Nothing Then
                Dim buyEntryPrice As Decimal = ConvertFloorCeling(upperV, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                Dim sellEntryPrice As Decimal = ConvertFloorCeling(lowerV, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                Dim midPoint As Decimal = (buyEntryPrice + sellEntryPrice) / 2
                Dim range As Decimal = buyEntryPrice - midPoint
                If currentTick.LastPrice >= midPoint + range * 30 / 100 Then
                    Dim higestATR As Decimal = GetHighestATR(atrConsumer, upperVSignalCandle)
                    Dim fractalLow As Decimal = CType(fractalConsumer.ConsumerPayloads(upperVSignalCandle.SnapshotDateTime), FractalConsumer.FractalPayload).FractalLow.Value
                    Dim entryPrice As Decimal = ConvertFloorCeling(upperV, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                    Dim slPoint As Decimal = ConvertFloorCeling(higestATR, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                    If entryPrice - fractalLow < slPoint Then
                        slPoint = entryPrice - fractalLow
                    End If
                    ret = New Tuple(Of Boolean, Decimal, Decimal, OHLCPayload, IOrder.TypeOfTransaction)(True, entryPrice, slPoint, upperVSignalCandle, IOrder.TypeOfTransaction.Buy)
                ElseIf currentTick.LastPrice <= midPoint - range * 30 / 100 Then
                    Dim higestATR As Decimal = GetHighestATR(atrConsumer, lowerVSignalCandle)
                    Dim fractalHigh As Decimal = CType(fractalConsumer.ConsumerPayloads(lowerVSignalCandle.SnapshotDateTime), FractalConsumer.FractalPayload).FractalHigh.Value
                    Dim entryPrice As Decimal = ConvertFloorCeling(lowerV, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                    Dim slPoint As Decimal = ConvertFloorCeling(higestATR, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                    If fractalHigh - entryPrice < slPoint Then
                        slPoint = fractalHigh - entryPrice
                    End If
                    ret = New Tuple(Of Boolean, Decimal, Decimal, OHLCPayload, IOrder.TypeOfTransaction)(True, entryPrice, slPoint, lowerVSignalCandle, IOrder.TypeOfTransaction.Sell)
                End If
            ElseIf upperVSignalCandle IsNot Nothing Then
                Dim higestATR As Decimal = GetHighestATR(atrConsumer, upperVSignalCandle)
                Dim fractalLow As Decimal = CType(fractalConsumer.ConsumerPayloads(upperVSignalCandle.SnapshotDateTime), FractalConsumer.FractalPayload).FractalLow.Value
                Dim entryPrice As Decimal = ConvertFloorCeling(upperV, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                Dim slPoint As Decimal = ConvertFloorCeling(higestATR, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                If entryPrice - fractalLow < slPoint Then
                    slPoint = entryPrice - fractalLow
                End If
                ret = New Tuple(Of Boolean, Decimal, Decimal, OHLCPayload, IOrder.TypeOfTransaction)(True, entryPrice, slPoint, upperVSignalCandle, IOrder.TypeOfTransaction.Buy)
            ElseIf lowerVSignalCandle IsNot Nothing Then
                Dim higestATR As Decimal = GetHighestATR(atrConsumer, lowerVSignalCandle)
                Dim fractalHigh As Decimal = CType(fractalConsumer.ConsumerPayloads(lowerVSignalCandle.SnapshotDateTime), FractalConsumer.FractalPayload).FractalHigh.Value
                Dim entryPrice As Decimal = ConvertFloorCeling(lowerV, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                Dim slPoint As Decimal = ConvertFloorCeling(higestATR, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                If fractalHigh - entryPrice < slPoint Then
                    slPoint = fractalHigh - entryPrice
                End If
                ret = New Tuple(Of Boolean, Decimal, Decimal, OHLCPayload, IOrder.TypeOfTransaction)(True, entryPrice, slPoint, lowerVSignalCandle, IOrder.TypeOfTransaction.Sell)
            End If
        End If
        If ret IsNot Nothing AndAlso forcePrint Then
            Try
                logger.Debug("Entry Price:{0}, SL Point:{1}, Direction:{2}, Trading Symbol:{3}", ret.Item2, ret.Item3, ret.Item5.ToString, Me.TradableInstrument.TradingSymbol)
            Catch ex As Exception
                logger.Warn(ex.ToString)
            End Try
        End If
        Return ret
    End Function

    Private Function IsFractalConstrictionDone(ByVal fractalConsumer As FractalConsumer, ByVal hkConsumer As HeikinAshiConsumer, ByVal currentCandle As OHLCPayload) As Tuple(Of Boolean, Date)
        Dim ret As Tuple(Of Boolean, Date) = Nothing
        If hkConsumer IsNot Nothing AndAlso hkConsumer.ConsumerPayloads IsNot Nothing AndAlso hkConsumer.ConsumerPayloads.Count > 0 Then
            Dim startTime As Date = Now.Date
            Dim lastValidTime As Date = Date.MinValue
            While True
                Dim upperFractalU As Tuple(Of Date, Date) = GetUpperFractalReverseU(fractalConsumer, hkConsumer, startTime, currentCandle.SnapshotDateTime)
                Dim lowerFractalU As Tuple(Of Date, Date) = GetLowerFractalU(fractalConsumer, hkConsumer, startTime, currentCandle.SnapshotDateTime)
                If upperFractalU IsNot Nothing AndAlso upperFractalU.Item1 <> Date.MinValue AndAlso upperFractalU.Item2 <> Date.MinValue AndAlso
                    lowerFractalU IsNot Nothing AndAlso lowerFractalU.Item1 <> Date.MinValue AndAlso lowerFractalU.Item2 <> Date.MinValue Then
                    Dim chkStartTime As Date = upperFractalU.Item1
                    If lowerFractalU.Item1 < upperFractalU.Item1 Then chkStartTime = lowerFractalU.Item1
                    Dim chkEndTime As Date = upperFractalU.Item2
                    If lowerFractalU.Item2 > upperFractalU.Item2 Then chkEndTime = lowerFractalU.Item2
                    For Each runningPayload In hkConsumer.ConsumerPayloads.OrderBy(Function(x)
                                                                                       Return x.Key
                                                                                   End Function)
                        If runningPayload.Key > chkStartTime AndAlso runningPayload.Key <= chkEndTime Then
                            Dim hkCandle As OHLCPayload = runningPayload.Value
                            Dim fractal As FractalConsumer.FractalPayload = fractalConsumer.ConsumerPayloads(hkCandle.SnapshotDateTime)
                            If hkCandle.ClosePrice.Value >= fractal.FractalHigh.Value OrElse
                                hkCandle.ClosePrice.Value <= fractal.FractalLow.Value Then
                                startTime = hkCandle.SnapshotDateTime
                                Exit For
                            End If
                        End If
                    Next
                    If startTime <= chkStartTime Then
                        'ret = New Tuple(Of Boolean, Date)(True, chkEndTime)
                        'Exit While
                        startTime = chkEndTime
                        lastValidTime = chkEndTime
                    End If
                Else
                    Exit While
                End If
            End While
            If lastValidTime <> Date.MinValue Then
                ret = New Tuple(Of Boolean, Date)(True, lastValidTime)
            End If
        End If
        Return ret
    End Function

    Private Function GetUpperFractalReverseU(ByVal fractalConsumer As FractalConsumer, ByVal hkConsumer As HeikinAshiConsumer, ByVal startTime As Date, ByVal endTime As Date) As Tuple(Of Date, Date)
        Dim ret As Tuple(Of Date, Date) = Nothing
        If fractalConsumer IsNot Nothing AndAlso fractalConsumer.ConsumerPayloads.Count > 0 Then
            Dim firstTime As Date = Date.MinValue
            Dim secondTime As Date = Date.MinValue
            Dim firstFractal As Decimal = Decimal.MinValue
            Dim secondFractal As Decimal = Decimal.MinValue
            For Each runningPayload In fractalConsumer.ConsumerPayloads.OrderBy(Function(x)
                                                                                    Return x.Key
                                                                                End Function)
                If runningPayload.Key.Date = Now.Date AndAlso runningPayload.Key > startTime AndAlso runningPayload.Key < endTime Then
                    Dim fractal As FractalConsumer.FractalPayload = runningPayload.Value
                    If firstFractal = Decimal.MinValue Then
                        firstFractal = fractal.FractalHigh.Value
                        firstTime = runningPayload.Key
                    Else
                        If secondFractal = Decimal.MinValue Then
                            If fractal.FractalHigh.Value > firstFractal Then
                                secondFractal = fractal.FractalHigh.Value
                                secondTime = runningPayload.Key
                            Else
                                firstFractal = fractal.FractalHigh.Value
                                firstTime = runningPayload.Key
                            End If
                        Else
                            If fractal.FractalHigh.Value < secondFractal Then
                                Dim closeFound As Boolean = False
                                For Each runningCandle In hkConsumer.ConsumerPayloads.OrderBy(Function(x)
                                                                                                  Return x.Key
                                                                                              End Function)
                                    If runningCandle.Key > firstTime AndAlso runningCandle.Key <= runningPayload.Key Then
                                        Dim hkCandle As OHLCPayload = runningCandle.Value
                                        Dim fractalData As FractalConsumer.FractalPayload = fractalConsumer.ConsumerPayloads(hkCandle.SnapshotDateTime)
                                        If hkCandle.ClosePrice.Value >= fractalData.FractalHigh.Value OrElse
                                            hkCandle.ClosePrice.Value <= fractalData.FractalLow.Value Then
                                            closeFound = True
                                            Exit For
                                        End If
                                    End If
                                Next
                                If Not closeFound Then
                                    ret = New Tuple(Of Date, Date)(firstTime, runningPayload.Key)
                                    Exit For
                                Else
                                    firstFractal = fractal.FractalHigh.Value
                                    firstTime = runningPayload.Key
                                    secondFractal = Decimal.MinValue
                                End If
                            ElseIf fractal.FractalHigh.Value > secondFractal Then
                                firstFractal = secondFractal
                                firstTime = secondTime
                                secondFractal = Decimal.MinValue
                            Else
                                secondFractal = fractal.FractalHigh.Value
                                secondTime = runningPayload.Key
                            End If
                        End If
                    End If
                End If
            Next
        End If
        Return ret
    End Function

    Private Function GetLowerFractalU(ByVal fractalConsumer As FractalConsumer, ByVal hkConsumer As HeikinAshiConsumer, ByVal startTime As Date, ByVal endTime As Date) As Tuple(Of Date, Date)
        Dim ret As Tuple(Of Date, Date) = Nothing
        If fractalConsumer IsNot Nothing AndAlso fractalConsumer.ConsumerPayloads.Count > 0 Then
            Dim firstTime As Date = Date.MinValue
            Dim secondTime As Date = Date.MinValue
            Dim firstFractal As Decimal = Decimal.MinValue
            Dim secondFractal As Decimal = Decimal.MinValue
            For Each runningPayload In fractalConsumer.ConsumerPayloads.OrderBy(Function(x)
                                                                                    Return x.Key
                                                                                End Function)
                If runningPayload.Key.Date = Now.Date AndAlso runningPayload.Key > startTime AndAlso runningPayload.Key < endTime Then
                    Dim fractal As FractalConsumer.FractalPayload = runningPayload.Value
                    If firstFractal = Decimal.MinValue Then
                        firstFractal = fractal.FractalLow.Value
                        firstTime = runningPayload.Key
                    Else
                        If secondFractal = Decimal.MinValue Then
                            If fractal.FractalLow.Value < firstFractal Then
                                secondFractal = fractal.FractalLow.Value
                                secondTime = runningPayload.Key
                            Else
                                firstFractal = fractal.FractalLow.Value
                                firstTime = runningPayload.Key
                            End If
                        Else
                            If fractal.FractalLow.Value > secondFractal Then
                                Dim closeFound As Boolean = False
                                For Each runningCandle In hkConsumer.ConsumerPayloads.OrderBy(Function(x)
                                                                                                  Return x.Key
                                                                                              End Function)
                                    If runningCandle.Key > firstTime AndAlso runningCandle.Key <= runningPayload.Key Then
                                        Dim hkCandle As OHLCPayload = runningCandle.Value
                                        Dim fractalData As FractalConsumer.FractalPayload = fractalConsumer.ConsumerPayloads(hkCandle.SnapshotDateTime)
                                        If hkCandle.ClosePrice.Value >= fractalData.FractalHigh.Value OrElse
                                            hkCandle.ClosePrice.Value <= fractalData.FractalLow.Value Then
                                            closeFound = True
                                            Exit For
                                        End If
                                    End If
                                Next
                                If Not closeFound Then
                                    ret = New Tuple(Of Date, Date)(firstTime, runningPayload.Key)
                                    Exit For
                                Else
                                    firstFractal = fractal.FractalLow.Value
                                    firstTime = runningPayload.Key
                                    secondFractal = Decimal.MinValue
                                End If
                            ElseIf fractal.FractalLow.Value < secondFractal Then
                                firstFractal = secondFractal
                                firstTime = secondTime
                                secondFractal = Decimal.MinValue
                            Else
                                secondFractal = fractal.FractalLow.Value
                                secondTime = runningPayload.Key
                            End If
                        End If
                    End If
                End If
            Next
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

    Private Function GetATRUpperBandV(ByVal atrBandConsumer As ATRBandsConsumer, ByVal currentCandle As OHLCPayload) As Decimal
        Dim ret As Decimal = Decimal.MinValue
        If currentCandle IsNot Nothing AndAlso currentCandle.PreviousPayload IsNot Nothing Then
            If atrBandConsumer.ConsumerPayloads IsNot Nothing AndAlso atrBandConsumer.ConsumerPayloads.Count > 0 AndAlso
                atrBandConsumer.ConsumerPayloads.ContainsKey(currentCandle.PreviousPayload.SnapshotDateTime) AndAlso
                atrBandConsumer.ConsumerPayloads.ContainsKey(currentCandle.PreviousPayload.PreviousPayload.SnapshotDateTime) AndAlso
                atrBandConsumer.ConsumerPayloads.ContainsKey(currentCandle.PreviousPayload.PreviousPayload.PreviousPayload.SnapshotDateTime) Then
                Dim preATRBand As ATRBandsConsumer.ATRBandPayload = atrBandConsumer.ConsumerPayloads(currentCandle.PreviousPayload.SnapshotDateTime)
                Dim prePreATRBand As ATRBandsConsumer.ATRBandPayload = atrBandConsumer.ConsumerPayloads(currentCandle.PreviousPayload.PreviousPayload.SnapshotDateTime)
                Dim prePrePreATRBand As ATRBandsConsumer.ATRBandPayload = atrBandConsumer.ConsumerPayloads(currentCandle.PreviousPayload.PreviousPayload.PreviousPayload.SnapshotDateTime)
                If preATRBand.HighBand.Value > prePreATRBand.HighBand.Value AndAlso prePreATRBand.HighBand.Value < prePrePreATRBand.HighBand.Value Then
                    ret = prePreATRBand.HighBand.Value
                End If
            End If
        End If
        Return ret
    End Function

    Private Function GetATRLowerBandReverseV(ByVal atrBandConsumer As ATRBandsConsumer, ByVal currentCandle As OHLCPayload) As Decimal
        Dim ret As Decimal = Decimal.MinValue
        If currentCandle IsNot Nothing AndAlso currentCandle.PreviousPayload IsNot Nothing Then
            If atrBandConsumer.ConsumerPayloads IsNot Nothing AndAlso atrBandConsumer.ConsumerPayloads.Count > 0 AndAlso
                atrBandConsumer.ConsumerPayloads.ContainsKey(currentCandle.PreviousPayload.SnapshotDateTime) AndAlso
                atrBandConsumer.ConsumerPayloads.ContainsKey(currentCandle.PreviousPayload.PreviousPayload.SnapshotDateTime) AndAlso
                atrBandConsumer.ConsumerPayloads.ContainsKey(currentCandle.PreviousPayload.PreviousPayload.PreviousPayload.SnapshotDateTime) Then
                Dim preATRBand As ATRBandsConsumer.ATRBandPayload = atrBandConsumer.ConsumerPayloads(currentCandle.PreviousPayload.SnapshotDateTime)
                Dim prePreATRBand As ATRBandsConsumer.ATRBandPayload = atrBandConsumer.ConsumerPayloads(currentCandle.PreviousPayload.PreviousPayload.SnapshotDateTime)
                Dim prePrePreATRBand As ATRBandsConsumer.ATRBandPayload = atrBandConsumer.ConsumerPayloads(currentCandle.PreviousPayload.PreviousPayload.PreviousPayload.SnapshotDateTime)
                If preATRBand.LowBand.Value < prePreATRBand.LowBand.Value AndAlso prePreATRBand.LowBand.Value > prePrePreATRBand.LowBand.Value Then
                    ret = prePreATRBand.LowBand.Value
                End If
            End If
        End If
        Return ret
    End Function

    Private Function GetHighestATR(ByVal atrConsumer As ATRConsumer, ByVal signalCandle As OHLCPayload) As Decimal
        Dim ret As Decimal = Decimal.MinValue
        If atrConsumer IsNot Nothing AndAlso atrConsumer.ConsumerPayloads IsNot Nothing AndAlso atrConsumer.ConsumerPayloads.Count > 0 AndAlso signalCandle IsNot Nothing Then
            ret = atrConsumer.ConsumerPayloads.Max(Function(x)
                                                       If x.Key.Date = Now.Date AndAlso x.Key <= signalCandle.SnapshotDateTime Then
                                                           Return CType(x.Value, ATRConsumer.ATRPayload).ATR.Value
                                                       Else
                                                           Return Decimal.MinValue
                                                       End If
                                                   End Function)
        End If
        Return ret
    End Function

    Private Function IsTargetReached() As Boolean
        Return Me.GetOverallPLAfterBrokerage() >= Math.Abs(CType(Me.ParentStrategy.UserSettings, NFOUserInputs).MaxLossPerTrade)
    End Function

    Private Function GetLogicalTradeCount() As Integer
        Dim ret As Integer = 0
        If OrderDetails IsNot Nothing AndAlso OrderDetails.Count > 0 Then
            Dim signalCandleList As List(Of Date) = Nothing
            For Each parentOrderId In OrderDetails.Keys
                Dim parentBusinessOrder As IBusinessOrder = OrderDetails(parentOrderId)
                If parentBusinessOrder.ParentOrder IsNot Nothing AndAlso parentBusinessOrder.ParentOrder.Status = IOrder.TypeOfStatus.Complete Then
                    Dim signalCandle As OHLCPayload = GetSignalCandleOfAnOrder(parentOrderId, Me.ParentStrategy.UserSettings.SignalTimeFrame)
                    If signalCandle IsNot Nothing Then
                        If signalCandleList Is Nothing Then
                            signalCandleList = New List(Of Date)
                            signalCandleList.Add(signalCandle.SnapshotDateTime)
                        Else
                            If Not signalCandleList.Contains(signalCandle.SnapshotDateTime) Then
                                signalCandleList.Add(signalCandle.SnapshotDateTime)
                            End If
                        End If
                    End If
                End If
            Next
            If signalCandleList IsNot Nothing Then
                ret = signalCandleList.Count
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