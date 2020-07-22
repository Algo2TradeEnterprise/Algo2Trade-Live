Imports System.Threading
Imports Algo2TradeCore.Adapter
Imports Algo2TradeCore.Entities
Imports Algo2TradeCore.Strategies
Imports Utilities.Numbers
Imports NLog
Imports Algo2TradeCore.ChartHandler.ChartStyle
Imports Algo2TradeCore

Public Class NFOStrategyInstrument
    Inherits StrategyInstrument
    Implements IDisposable

#Region "Logging and Status Progress"
    Public Shared Shadows logger As Logger = LogManager.GetCurrentClassLogger
#End Region

    Private _lastPrevPayloadPlaceOrder As String = ""
    Private _eligibleToTakeTrade As Boolean = True

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

                Me.RawPayloadDependentConsumers.Add(chartConsumer)
            Else
                Throw New ApplicationException(String.Format("Signal Timeframe is 0 or Nothing, does not adhere to the strategy:{0}", Me.ParentStrategy.ToString))
            End If
        End If
    End Sub

    Public Overrides Async Function PopulateChartAndIndicatorsAsync(candleCreator As Chart, currentCandle As OHLCPayload) As Task
        If RawPayloadDependentConsumers IsNot Nothing AndAlso RawPayloadDependentConsumers.Count > 0 Then
            For Each runningRawPayloadConsumer In RawPayloadDependentConsumers
                If runningRawPayloadConsumer.TypeOfConsumer = IPayloadConsumer.ConsumerType.Chart Then
                    candleCreator.ConvertTimeframe(CType(runningRawPayloadConsumer, PayloadToChartConsumer).Timeframe, currentCandle, runningRawPayloadConsumer)
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
                    Await ExecuteCommandAsync(ExecuteCommands.PlaceBOSLMISOrder, Nothing).ConfigureAwait(False)
                End If
                'Place Order block end
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
        Dim currentTick As ITick = Me.TradableInstrument.LastTick
        Dim currentTime As Date = Now()
        Dim buyActiveOrder As List(Of IOrder) = GetAllActiveOrders(IOrder.TypeOfTransaction.Buy)
        Dim sellActiveOrder As List(Of IOrder) = GetAllActiveOrders(IOrder.TypeOfTransaction.Sell)

        Try
            If runningCandlePayload IsNot Nothing AndAlso runningCandlePayload.PreviousPayload IsNot Nothing AndAlso
                Me.TradableInstrument.IsHistoricalCompleted Then
                If Not runningCandlePayload.PreviousPayload.ToString = _lastPrevPayloadPlaceOrder Then
                    _lastPrevPayloadPlaceOrder = runningCandlePayload.PreviousPayload.ToString
                    logger.Debug("PlaceOrder-> Potential Signal Candle is:{0}. Will check rest parameters.", runningCandlePayload.PreviousPayload.ToString)
                    logger.Debug("PlaceOrder-> Rest all parameters: Running Candle:{0}, PayloadGeneratedBy:{1}, IsHistoricalCompleted:{2}, IsFirstTimeInformationCollected:{3}, Buy Active Trades:{4}, Sell Active Trades:{5}, Number Of Trades:{6}, Total PL:{7}, Strategy Exit All Triggered:{8}, Eligible To Take Trade:{9}, Current Time:{10}, Current Tick:{11}, TradingSymbol:{12}",
                                runningCandlePayload.SnapshotDateTime.ToString("dd-MM-yyyy HH:mm:ss"),
                                runningCandlePayload.PayloadGeneratedBy.ToString,
                                Me.TradableInstrument.IsHistoricalCompleted,
                                Me.ParentStrategy.IsFirstTimeInformationCollected,
                                If(buyActiveOrder Is Nothing, 0, buyActiveOrder.Count),
                                If(sellActiveOrder Is Nothing, 0, sellActiveOrder.Count),
                                Me.GetTotalExecutedOrders(),
                                Me.ParentStrategy.GetTotalPLAfterBrokerage(),
                                Me.StrategyExitAllTriggerd,
                                _eligibleToTakeTrade,
                                currentTime.ToString,
                                currentTick.LastPrice,
                                Me.TradableInstrument.TradingSymbol)
                End If
            End If
        Catch ex As Exception
            logger.Error(ex)
        End Try

        Dim parameterList As List(Of PlaceOrderParameters) = Nothing
        If currentTime >= Me.TradableInstrument.ExchangeDetails.ExchangeStartTime AndAlso currentTime <= userSettings.EODExitTime AndAlso
            runningCandlePayload IsNot Nothing AndAlso runningCandlePayload.SnapshotDateTime >= Me.TradableInstrument.ExchangeDetails.ExchangeStartTime AndAlso
            runningCandlePayload.PayloadGeneratedBy = OHLCPayload.PayloadSource.CalculatedTick AndAlso runningCandlePayload.PreviousPayload IsNot Nothing AndAlso
            Me.TradableInstrument.IsHistoricalCompleted AndAlso GetTotalExecutedOrders() < userSettings.NumberOfTradePerStock AndAlso
            Me.ParentStrategy.GetTotalPLAfterBrokerage() > userSettings.MTMLoss AndAlso Me.ParentStrategy.GetTotalPLAfterBrokerage() < userSettings.MTMProfit AndAlso
            Not Me.StrategyExitAllTriggerd AndAlso _eligibleToTakeTrade Then
            Dim signal As Tuple(Of Boolean, Decimal, Decimal) = GetEntrySignal(runningCandlePayload, userSettings)
            If signal IsNot Nothing AndAlso signal.Item1 Then
                If buyActiveOrder Is Nothing OrElse buyActiveOrder.Count = 0 Then
                    Dim triggerPrice As Decimal = signal.Item2
                    Dim price As Decimal = triggerPrice + ConvertFloorCeling(triggerPrice * 0.3 / 100, TradableInstrument.TickSize, RoundOfType.Celing)
                    Dim slPoint As Decimal = ConvertFloorCeling((signal.Item2 - signal.Item3) * userSettings.RangeStoplossPercentage / 100, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                    Dim targetPoint As Decimal = ConvertFloorCeling(slPoint * userSettings.InstrumentsData(Me.TradableInstrument.TradingSymbol).TargetMultiplier, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                    Dim quantity As Integer = CalculateQuantityFromStoploss(triggerPrice, triggerPrice - slPoint, userSettings.InstrumentsData(Me.TradableInstrument.TradingSymbol).MaxLossPerStock / userSettings.NumberOfTradePerStock)
                    If quantity <> 0 AndAlso currentTick.LastPrice < triggerPrice Then
                        Dim parameter As PlaceOrderParameters = New PlaceOrderParameters(runningCandlePayload.PreviousPayload) With
                                                                {.EntryDirection = IOrder.TypeOfTransaction.Buy,
                                                                 .TriggerPrice = triggerPrice,
                                                                 .Price = price,
                                                                 .StoplossValue = slPoint,
                                                                 .SquareOffValue = targetPoint,
                                                                 .OrderType = IOrder.TypeOfOrder.SL,
                                                                 .Quantity = quantity}

                        If parameterList Is Nothing Then parameterList = New List(Of PlaceOrderParameters)
                        parameterList.Add(parameter)
                    End If
                End If
                If sellActiveOrder Is Nothing OrElse sellActiveOrder.Count = 0 Then
                    Dim triggerPrice As Decimal = signal.Item3
                    Dim price As Decimal = triggerPrice - ConvertFloorCeling(triggerPrice * 0.3 / 100, TradableInstrument.TickSize, RoundOfType.Celing)
                    Dim slPoint As Decimal = ConvertFloorCeling((signal.Item2 - signal.Item3) * userSettings.RangeStoplossPercentage / 100, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                    Dim targetPoint As Decimal = ConvertFloorCeling(slPoint * userSettings.InstrumentsData(Me.TradableInstrument.TradingSymbol).TargetMultiplier, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                    Dim quantity As Integer = CalculateQuantityFromStoploss(triggerPrice + slPoint, triggerPrice, userSettings.InstrumentsData(Me.TradableInstrument.TradingSymbol).MaxLossPerStock / userSettings.NumberOfTradePerStock)
                    If quantity <> 0 AndAlso currentTick.LastPrice > triggerPrice Then
                        Dim parameter As PlaceOrderParameters = New PlaceOrderParameters(runningCandlePayload.PreviousPayload) With
                                                                {.EntryDirection = IOrder.TypeOfTransaction.Sell,
                                                                 .TriggerPrice = triggerPrice,
                                                                 .Price = price,
                                                                 .StoplossValue = slPoint,
                                                                 .SquareOffValue = targetPoint,
                                                                 .OrderType = IOrder.TypeOfOrder.SL,
                                                                 .Quantity = quantity}

                        If parameterList Is Nothing Then parameterList = New List(Of PlaceOrderParameters)
                        parameterList.Add(parameter)
                    End If
                End If
            End If
        End If

        'Below portion have to be done in every place order trigger
        If parameterList IsNot Nothing AndAlso parameterList.Count > 0 Then
            For Each runningParameter In parameterList
                Try
                    If forcePrint Then
                        logger.Debug("PlaceOrder-> ************************************************ {0}", Me.TradableInstrument.TradingSymbol)
                        logger.Debug("PlaceOrder Parameters-> Direction:{0}, Trigger Price:{1}, Price:{2}, Stoploss Value:{3}, Squareoff Value:{4}, Quantity:{5}, Order Type:{6}, Trading Symbol:{7}",
                                     runningParameter.EntryDirection.ToString,
                                     runningParameter.TriggerPrice,
                                     runningParameter.Price,
                                     runningParameter.StoplossValue,
                                     runningParameter.SquareOffValue,
                                     runningParameter.Quantity,
                                     runningParameter.OrderType.ToString,
                                     Me.TradableInstrument.TradingSymbol)
                    End If
                Catch ex As Exception
                    logger.Error(ex)
                End Try

                Dim currentSignalActivities As IEnumerable(Of ActivityDashboard) = Me.ParentStrategy.SignalManager.GetActiveSignalActivities(Me.TradableInstrument.InstrumentIdentifier)
                If currentSignalActivities IsNot Nothing AndAlso currentSignalActivities.Count > 0 Then
                    Dim placedActivities As IEnumerable(Of ActivityDashboard) = currentSignalActivities.Where(Function(x)
                                                                                                                  Return x.EntryActivity.RequestRemarks = runningParameter.ToString
                                                                                                              End Function)
                    'Dim placedActivities As IEnumerable(Of ActivityDashboard) = currentSignalActivities
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
        Return ret
    End Function

    Protected Overrides Async Function IsTriggerReceivedForExitOrderAsync(ByVal forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, String)))
        Dim ret As List(Of Tuple(Of ExecuteCommandAction, IOrder, String)) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
        If GetTotalExecutedOrders() >= userSettings.NumberOfTradePerStock Then
            Dim runningCandlePayload As OHLCPayload = GetXMinuteCurrentCandle(userSettings.SignalTimeFrame)
            If runningCandlePayload IsNot Nothing AndAlso runningCandlePayload.PreviousPayload IsNot Nothing AndAlso
                Me.TradableInstrument.IsHistoricalCompleted AndAlso Me.ParentStrategy.IsFirstTimeInformationCollected Then
                Dim allActiveOrders As List(Of IOrder) = GetAllActiveOrders(IOrder.TypeOfTransaction.None)
                If allActiveOrders IsNot Nothing AndAlso allActiveOrders.Count > 0 Then
                    Dim parentOrders As List(Of IOrder) = allActiveOrders.FindAll(Function(x)
                                                                                      Return x.ParentOrderIdentifier Is Nothing AndAlso
                                                                                        x.Status = IOrder.TypeOfStatus.TriggerPending
                                                                                  End Function)
                    If parentOrders IsNot Nothing AndAlso parentOrders.Count > 0 Then
                        For Each runningOrder In parentOrders
                            If runningOrder.Status = IOrder.TypeOfStatus.TriggerPending Then
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
                                ret.Add(New Tuple(Of ExecuteCommandAction, IOrder, String)(ExecuteCommandAction.Take, runningOrder, "Opposite direction triggered"))
                            End If
                        Next
                    End If
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

    Private Function GetEntrySignal(ByVal runningCandle As OHLCPayload, ByVal userSettings As NFOUserInputs) As Tuple(Of Boolean, Decimal, Decimal)
        Dim ret As Tuple(Of Boolean, Decimal, Decimal) = Nothing
        If runningCandle IsNot Nothing AndAlso runningCandle.PreviousPayload IsNot Nothing Then
            Dim buyLevel As Decimal = Decimal.MinValue
            Dim sellLevel As Decimal = Decimal.MinValue
            If userSettings.RangeType = NFOUserInputs.TypeOfRanges.Previous_Day Then
                Dim previousTradingDay As Date = GetPreviousTradingDay(userSettings.SignalTimeFrame)
                If previousTradingDay <> Date.MinValue Then
                    buyLevel = GetDayHigh(userSettings.SignalTimeFrame, previousTradingDay)
                    sellLevel = GetDayLow(userSettings.SignalTimeFrame, previousTradingDay)
                End If
            Else
                If runningCandle.PreviousPayload.SnapshotDateTime.Date = Now.Date AndAlso
                    runningCandle.PreviousPayload.PreviousPayload IsNot Nothing AndAlso
                    runningCandle.PreviousPayload.PreviousPayload.SnapshotDateTime.Date <> Now.Date Then
                    buyLevel = runningCandle.PreviousPayload.HighPrice.Value
                    sellLevel = runningCandle.PreviousPayload.LowPrice.Value
                End If
            End If
            If buyLevel <> Decimal.MinValue AndAlso sellLevel <> Decimal.MinValue Then
                If runningCandle.OpenPrice.Value < buyLevel AndAlso runningCandle.OpenPrice.Value > sellLevel Then
                    Dim buffer As Decimal = CalculateBuffer(sellLevel, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                    buyLevel = buyLevel + buffer
                    sellLevel = sellLevel - buffer
                    ret = New Tuple(Of Boolean, Decimal, Decimal)(True, buyLevel, sellLevel)
                Else
                    _eligibleToTakeTrade = False
                    OnHeartbeat(String.Format("Unable to take trade for gap on {0}", Me.TradableInstrument.TradingSymbol))
                End If
            End If
        End If
        Return ret
    End Function

    Private Function GetPreviousTradingDay(ByVal timeframe As Integer) As Date
        Dim ret As Date = Date.MinValue
        If Me.RawPayloadDependentConsumers IsNot Nothing AndAlso Me.RawPayloadDependentConsumers.Count > 0 Then
            Dim XMinutePayloadConsumer As PayloadToChartConsumer = RawPayloadDependentConsumers.Find(Function(x)
                                                                                                         If x.GetType Is GetType(PayloadToChartConsumer) Then
                                                                                                             Return CType(x, PayloadToChartConsumer).Timeframe = timeframe
                                                                                                         Else
                                                                                                             Return Nothing
                                                                                                         End If
                                                                                                     End Function)

            If XMinutePayloadConsumer IsNot Nothing AndAlso XMinutePayloadConsumer.ConsumerPayloads IsNot Nothing AndAlso XMinutePayloadConsumer.ConsumerPayloads.Count > 0 Then
                ret = XMinutePayloadConsumer.ConsumerPayloads.Keys.Max(Function(x)
                                                                           If x.Date < Now.Date Then
                                                                               Return x
                                                                           Else
                                                                               Return Date.MinValue
                                                                           End If
                                                                       End Function)
            End If
        End If
        Return ret
    End Function

    Private Function GetDayHigh(ByVal timeframe As Integer, ByVal checkDate As Date) As Decimal
        Dim ret As Decimal = Decimal.MinValue
        If Me.RawPayloadDependentConsumers IsNot Nothing AndAlso Me.RawPayloadDependentConsumers.Count > 0 Then
            Dim XMinutePayloadConsumer As PayloadToChartConsumer = RawPayloadDependentConsumers.Find(Function(x)
                                                                                                         If x.GetType Is GetType(PayloadToChartConsumer) Then
                                                                                                             Return CType(x, PayloadToChartConsumer).Timeframe = timeframe
                                                                                                         Else
                                                                                                             Return Nothing
                                                                                                         End If
                                                                                                     End Function)

            If XMinutePayloadConsumer IsNot Nothing AndAlso XMinutePayloadConsumer.ConsumerPayloads IsNot Nothing AndAlso XMinutePayloadConsumer.ConsumerPayloads.Count > 0 Then
                ret = XMinutePayloadConsumer.ConsumerPayloads.Max(Function(x)
                                                                      If x.Key.Date = checkDate.Date Then
                                                                          Return CType(x.Value, OHLCPayload).HighPrice.Value
                                                                      Else
                                                                          Return Decimal.MinValue
                                                                      End If
                                                                  End Function)
            End If
        End If
        Return ret
    End Function

    Private Function GetDayLow(ByVal timeframe As Integer, ByVal checkDate As Date) As Decimal
        Dim ret As Decimal = Decimal.MaxValue
        If Me.RawPayloadDependentConsumers IsNot Nothing AndAlso Me.RawPayloadDependentConsumers.Count > 0 Then
            Dim XMinutePayloadConsumer As PayloadToChartConsumer = RawPayloadDependentConsumers.Find(Function(x)
                                                                                                         If x.GetType Is GetType(PayloadToChartConsumer) Then
                                                                                                             Return CType(x, PayloadToChartConsumer).Timeframe = timeframe
                                                                                                         Else
                                                                                                             Return Nothing
                                                                                                         End If
                                                                                                     End Function)

            If XMinutePayloadConsumer IsNot Nothing AndAlso XMinutePayloadConsumer.ConsumerPayloads IsNot Nothing AndAlso XMinutePayloadConsumer.ConsumerPayloads.Count > 0 Then
                ret = XMinutePayloadConsumer.ConsumerPayloads.Min(Function(x)
                                                                      If x.Key.Date = checkDate.Date Then
                                                                          Return CType(x.Value, OHLCPayload).LowPrice.Value
                                                                      Else
                                                                          Return Decimal.MaxValue
                                                                      End If
                                                                  End Function)
            End If
        End If
        Return ret
    End Function

#Region "Not Required For This Strategy"
    Public Overrides Function MonitorAsync(ByVal command As ExecuteCommands, ByVal data As Object) As Task
        Throw New NotImplementedException()
    End Function

    Protected Overrides Function IsTriggerReceivedForModifyStoplossOrderAsync(ByVal forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)))
        Throw New NotImplementedException()
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