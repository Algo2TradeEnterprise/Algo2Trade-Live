Imports NLog
Imports System.Threading
Imports Utilities.Numbers
Imports Algo2TradeCore.Adapter
Imports Algo2TradeCore.Entities
Imports Algo2TradeCore.Strategies

Public Class LowSLStrategyInstrument
    Inherits StrategyInstrument
    Implements IDisposable

#Region "Logging and Status Progress"
    Public Shared Shadows logger As Logger = LogManager.GetCurrentClassLogger
#End Region

    Private _lastPrevPayloadPlaceOrder As String = ""
    Private _quantity As Integer = Integer.MinValue

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
                RawPayloadDependentConsumers.Add(chartConsumer)
            Else
                Throw New ApplicationException(String.Format("Signal Timeframe is 0 or Nothing, does not adhere to the strategy:{0}", Me.ParentStrategy.ToString))
            End If
        End If
    End Sub

    Public Overrides Async Function MonitorAsync() As Task
        Try
            Dim userSettings As LowSLUserInputs = Me.ParentStrategy.UserSettings
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
                If placeOrderTriggers IsNot Nothing AndAlso placeOrderTriggers.Count > 0 Then
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

    Public Overrides Function MonitorAsync(command As ExecuteCommands, data As Object) As Task
        Throw New NotImplementedException()
    End Function

    Protected Overrides Async Function IsTriggerReceivedForPlaceOrderAsync(forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)))
        Dim ret As List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        Dim userSettings As LowSLUserInputs = Me.ParentStrategy.UserSettings
        Dim runningCandlePayload As OHLCPayload = GetXMinuteCurrentCandle(userSettings.SignalTimeFrame)
        Dim currentTick As ITick = Me.TradableInstrument.LastTick
        Dim currentTime As Date = Now()

        Try
            If runningCandlePayload IsNot Nothing AndAlso runningCandlePayload.PreviousPayload IsNot Nothing AndAlso
                (Not runningCandlePayload.PreviousPayload.ToString = _lastPrevPayloadPlaceOrder OrElse forcePrint) Then
                _lastPrevPayloadPlaceOrder = runningCandlePayload.PreviousPayload.ToString
                logger.Debug("PlaceOrder-> Potential Signal Candle is:{0}. Will check rest parameters.", runningCandlePayload.PreviousPayload.ToString)
                GetEntrySignal(runningCandlePayload.PreviousPayload, currentTick, True)
                logger.Debug("PlaceOrder-> Rest all parameters: Trade Start Time:{0}, Last Trade Entry Time:{1}, RunningCandlePayloadSnapshotDateTime:{2}, PayloadGeneratedBy:{3}, IsHistoricalCompleted:{4}, Signal Candle Time:{5}, Is Active Instrument:{6}, Number Of Trade:{7}, OverAll PL:{8}, Stock PL:{9}, Current Time:{10}, Current LTP:{11}, TradingSymbol:{12}",
                                userSettings.TradeStartTime.ToString,
                                userSettings.LastTradeEntryTime.ToString,
                                runningCandlePayload.SnapshotDateTime.ToString,
                                runningCandlePayload.PayloadGeneratedBy.ToString,
                                Me.TradableInstrument.IsHistoricalCompleted,
                                runningCandlePayload.PreviousPayload.SnapshotDateTime.ToShortTimeString,
                                IsActiveInstrument(),
                                Me.GetTotalExecutedOrders(),
                                Me.ParentStrategy.GetTotalPLAfterBrokerage(),
                                Me.GetOverallPLAfterBrokerage(),
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
            runningCandlePayload.PayloadGeneratedBy = OHLCPayload.PayloadSource.CalculatedTick AndAlso
            runningCandlePayload.PreviousPayload IsNot Nothing AndAlso Me.TradableInstrument.IsHistoricalCompleted AndAlso
            Not IsActiveInstrument() AndAlso GetTotalExecutedOrders() < userSettings.NumberOfTradePerStock AndAlso
            Me.ParentStrategy.GetTotalPLAfterBrokerage() > userSettings.OverallMaxLossPerDay AndAlso
            Me.ParentStrategy.GetTotalPLAfterBrokerage() < userSettings.OverallMaxProfitPerDay AndAlso Not Me.StrategyExitAllTriggerd AndAlso
            Me.GetOverallPLAfterBrokerage() > userSettings.StockMaxLossPerDay AndAlso Me.GetOverallPLAfterBrokerage < userSettings.StockMaxProfitPerDay Then

            If _quantity = Integer.MinValue Then
                _quantity = CalculateQuantityFromInvestment(currentTick.LastPrice, userSettings.InstrumentsData(Me.TradableInstrument.TradingSymbol).MarginMultiplier, userSettings.MinInvestmentPerStock, True)
            End If
            Dim signal As Tuple(Of Boolean, Decimal, IOrder.TypeOfTransaction) = GetEntrySignal(runningCandlePayload.PreviousPayload, currentTick, forcePrint)
            If signal IsNot Nothing AndAlso signal.Item1 Then
                Dim lastExecutedOrder As IBusinessOrder = GetLastExecutedOrder()
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
                    Dim targetPoint As Decimal = signal.Item2
                    If Me.ParentStrategy.GetTotalPLAfterBrokerage() < 0 Then
                        Dim targetPrice As Decimal = CalculateTargetFromPL(signalCandle.OpenPrice.Value, _quantity, (userSettings.MinStoplossPerTrade + userSettings.MaxStoplossPerTrade) / 2)
                        targetPoint = targetPrice - signalCandle.OpenPrice.Value
                    End If

                    If signal.Item3 = IOrder.TypeOfTransaction.Buy Then
                        Dim slPrice As Decimal = Decimal.MinValue
                        If signalCandle.CandleColor = Color.Red Then
                            slPrice = signalCandle.OpenPrice.Value
                        Else
                            slPrice = signalCandle.ClosePrice.Value
                        End If
                        Dim buffer As Decimal = CalculateBuffer(signalCandle.HighPrice.Value, Me.TradableInstrument.TickSize, RoundOfType.Floor)

                        Dim triggerPrice As Decimal = signalCandle.HighPrice.Value + buffer
                        Dim price As Decimal = triggerPrice + ConvertFloorCeling(triggerPrice * 0.3 / 100, TradableInstrument.TickSize, RoundOfType.Celing)
                        Dim slPoint As Decimal = triggerPrice - slPrice

                        If currentTick.LastPrice < triggerPrice Then
                            parameters = New PlaceOrderParameters(runningCandlePayload.PreviousPayload) With
                                        {.EntryDirection = IOrder.TypeOfTransaction.Buy,
                                         .TriggerPrice = triggerPrice,
                                         .Price = price,
                                         .StoplossValue = slPoint,
                                         .SquareOffValue = targetPoint,
                                         .Quantity = _quantity}
                        End If
                    ElseIf signal.Item3 = IOrder.TypeOfTransaction.Sell Then
                        Dim slPrice As Decimal = Decimal.MinValue
                        If signalCandle.CandleColor = Color.Green Then
                            slPrice = signalCandle.OpenPrice.Value
                        Else
                            slPrice = signalCandle.ClosePrice.Value
                        End If
                        Dim buffer As Decimal = CalculateBuffer(signalCandle.LowPrice.Value, Me.TradableInstrument.TickSize, RoundOfType.Floor)

                        Dim triggerPrice As Decimal = signalCandle.LowPrice.Value - buffer
                        Dim price As Decimal = triggerPrice - ConvertFloorCeling(triggerPrice * 0.3 / 100, TradableInstrument.TickSize, RoundOfType.Celing)
                        Dim slPoint As Decimal = slPrice - triggerPrice

                        If currentTick.LastPrice > triggerPrice Then
                            parameters = New PlaceOrderParameters(runningCandlePayload.PreviousPayload) With
                                        {.EntryDirection = IOrder.TypeOfTransaction.Sell,
                                         .TriggerPrice = triggerPrice,
                                         .Price = price,
                                         .StoplossValue = slPoint,
                                         .SquareOffValue = targetPoint,
                                         .Quantity = _quantity}
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

    Protected Overrides Function IsTriggerReceivedForPlaceOrderAsync(forcePrint As Boolean, data As Object) As Task(Of List(Of Tuple(Of ExecuteCommandAction, StrategyInstrument, PlaceOrderParameters, String)))
        Throw New NotImplementedException()
    End Function

    Protected Overrides Async Function IsTriggerReceivedForModifyStoplossOrderAsync(forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)))
        Dim ret As List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        Dim userSettings As LowSLUserInputs = Me.ParentStrategy.UserSettings
        Dim currentTick As ITick = Me.TradableInstrument.LastTick
        If OrderDetails IsNot Nothing AndAlso OrderDetails.Count > 0 Then
            For Each runningOrderID In OrderDetails.Keys
                Dim bussinessOrder As IBusinessOrder = OrderDetails(runningOrderID)
                If bussinessOrder.SLOrder IsNot Nothing AndAlso bussinessOrder.SLOrder.Count > 0 Then
                    Dim signalCandle As OHLCPayload = GetSignalCandleOfAnOrder(bussinessOrder.ParentOrderIdentifier, Me.ParentStrategy.UserSettings.SignalTimeFrame)
                    If signalCandle IsNot Nothing Then
                        For Each slOrder In bussinessOrder.SLOrder
                            If Not slOrder.Status = IOrder.TypeOfStatus.Complete AndAlso
                            Not slOrder.Status = IOrder.TypeOfStatus.Cancelled AndAlso
                            Not slOrder.Status = IOrder.TypeOfStatus.Rejected Then
                                Dim triggerPrice As Decimal = Decimal.MinValue
                                If bussinessOrder.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Buy Then
                                    If signalCandle.CandleColor = Color.Red Then
                                        triggerPrice = signalCandle.OpenPrice.Value
                                    Else
                                        triggerPrice = signalCandle.ClosePrice.Value
                                    End If
                                ElseIf bussinessOrder.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Sell Then
                                    If signalCandle.CandleColor = Color.Green Then
                                        triggerPrice = signalCandle.OpenPrice.Value
                                    Else
                                        triggerPrice = signalCandle.ClosePrice.Value
                                    End If
                                End If
                                If triggerPrice <> Decimal.MinValue AndAlso slOrder.TriggerPrice <> triggerPrice Then
                                    'Below portion have to be done in every modify stoploss order trigger
                                    Dim currentSignalActivities As ActivityDashboard = Me.ParentStrategy.SignalManager.GetSignalActivities(slOrder.Tag)
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
                                    ret.Add(New Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)(ExecuteCommandAction.Take, slOrder, triggerPrice, "Target Protection"))
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

    Protected Overrides Function IsTriggerReceivedForModifyTargetOrderAsync(forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)))
        Throw New NotImplementedException()
    End Function

    Protected Overrides Async Function IsTriggerReceivedForExitOrderAsync(forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, String)))
        Dim ret As List(Of Tuple(Of ExecuteCommandAction, IOrder, String)) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        Dim allActiveOrders As List(Of IOrder) = GetAllActiveOrders(IOrder.TypeOfTransaction.None)
        If allActiveOrders IsNot Nothing AndAlso allActiveOrders.Count > 0 Then
            Dim parentOrders As List(Of IOrder) = allActiveOrders.FindAll(Function(x)
                                                                              Return x.ParentOrderIdentifier Is Nothing AndAlso
                                                                              x.Status = IOrder.TypeOfStatus.TriggerPending
                                                                          End Function)
            If parentOrders IsNot Nothing AndAlso parentOrders.Count > 0 Then
                Dim runningCandle As OHLCPayload = GetXMinuteCurrentCandle(Me.ParentStrategy.UserSettings.SignalTimeFrame)
                If runningCandle IsNot Nothing AndAlso runningCandle.PayloadGeneratedBy = OHLCPayload.PayloadSource.CalculatedTick AndAlso runningCandle.PreviousPayload IsNot Nothing Then
                    Dim signal As Tuple(Of Boolean, Decimal, IOrder.TypeOfTransaction) = GetEntrySignal(runningCandle.PreviousPayload, Me.TradableInstrument.LastTick, forcePrint)
                    If signal IsNot Nothing Then
                        For Each parentOrder In parentOrders
                            Dim parentBussinessOrder As IBusinessOrder = OrderDetails(parentOrder.OrderIdentifier)
                            Dim signalCandle As OHLCPayload = GetSignalCandleOfAnOrder(parentBussinessOrder.ParentOrderIdentifier, Me.ParentStrategy.UserSettings.SignalTimeFrame)
                            If signalCandle IsNot Nothing Then
                                If signal.Item3 <> parentOrder.TransactionType OrElse
                                    signalCandle.SnapshotDateTime <> runningCandle.PreviousPayload.SnapshotDateTime Then
                                    'Below portion have to be done in every cancel order trigger
                                    Dim currentSignalActivities As ActivityDashboard = Me.ParentStrategy.SignalManager.GetSignalActivities(parentOrder.Tag)
                                    If currentSignalActivities IsNot Nothing Then
                                        If currentSignalActivities.CancelActivity.RequestStatus = ActivityDashboard.SignalStatusType.Handled OrElse
                                            currentSignalActivities.CancelActivity.RequestStatus = ActivityDashboard.SignalStatusType.Activated OrElse
                                            currentSignalActivities.CancelActivity.RequestStatus = ActivityDashboard.SignalStatusType.Completed Then
                                            Continue For
                                        End If
                                    End If
                                    If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, IOrder, String))
                                    ret.Add(New Tuple(Of ExecuteCommandAction, IOrder, String)(ExecuteCommandAction.Take, parentBussinessOrder.ParentOrder, "Opposite Direction trade"))
                                End If
                            End If
                        Next
                    End If
                End If
            End If
        End If
        Return ret
    End Function

    Protected Overrides Function IsTriggerReceivedForExitOrderAsync(forcePrint As Boolean, data As Object) As Task(Of List(Of Tuple(Of ExecuteCommandAction, StrategyInstrument, IOrder, String)))
        Throw New NotImplementedException()
    End Function

    Protected Overrides Async Function ForceExitSpecificTradeAsync(order As IOrder, reason As String) As Task
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

    Private Function GetEntrySignal(ByVal candle As OHLCPayload, ByVal currentTick As ITick, ByVal print As Boolean) As Tuple(Of Boolean, Decimal, IOrder.TypeOfTransaction)
        Dim ret As Tuple(Of Boolean, Decimal, IOrder.TypeOfTransaction) = Nothing
        Dim userSettings As LowSLUserInputs = Me.ParentStrategy.UserSettings
        If candle IsNot Nothing AndAlso candle.PreviousPayload IsNot Nothing AndAlso
            Not candle.DeadCandle AndAlso Not candle.PreviousPayload.DeadCandle Then
            Dim firstDirectionToCheck As IOrder.TypeOfTransaction = IOrder.TypeOfTransaction.None
            If candle.CandleColor = Color.Green Then
                firstDirectionToCheck = IOrder.TypeOfTransaction.Buy
            ElseIf candle.CandleColor = Color.Red Then
                firstDirectionToCheck = IOrder.TypeOfTransaction.Sell
            Else
                firstDirectionToCheck = IOrder.TypeOfTransaction.Buy
            End If
            Dim buySLPrice As Decimal = GetStoplossPrice(candle, IOrder.TypeOfTransaction.Buy)
            Dim sellSLPrice As Decimal = GetStoplossPrice(candle, IOrder.TypeOfTransaction.Sell)
            If firstDirectionToCheck = IOrder.TypeOfTransaction.Buy Then
                If buySLPrice <> Decimal.MinValue AndAlso Math.Abs(buySLPrice) >= Math.Abs(userSettings.MinStoplossPerTrade) AndAlso Math.Abs(buySLPrice) <= Math.Abs(userSettings.MaxStoplossPerTrade) Then
                    Dim minStockMaxProft As Decimal = userSettings.StockMaxProfitPerDay
                    Dim targetPL As Decimal = Math.Max(minStockMaxProft, Math.Abs(buySLPrice) * userSettings.TargetMultiplier)
                    Dim targetPrice As Decimal = CalculateTargetFromPL(candle.HighPrice.Value, _quantity, targetPL)

                    ret = New Tuple(Of Boolean, Decimal, IOrder.TypeOfTransaction)(True, targetPrice - candle.HighPrice.Value, IOrder.TypeOfTransaction.Buy)
                ElseIf sellSLPrice <> Decimal.MinValue AndAlso Math.Abs(sellSLPrice) >= Math.Abs(userSettings.MinStoplossPerTrade) AndAlso Math.Abs(sellSLPrice) <= Math.Abs(userSettings.MaxStoplossPerTrade) Then
                    Dim minStockMaxProft As Decimal = userSettings.StockMaxProfitPerDay
                    Dim targetPL As Decimal = Math.Max(minStockMaxProft, Math.Abs(sellSLPrice) * userSettings.TargetMultiplier)
                    Dim targetPrice As Decimal = CalculateTargetFromPL(candle.LowPrice.Value, _quantity, targetPL)

                    ret = New Tuple(Of Boolean, Decimal, IOrder.TypeOfTransaction)(True, targetPrice - candle.LowPrice.Value, IOrder.TypeOfTransaction.Sell)
                End If
            ElseIf firstDirectionToCheck = IOrder.TypeOfTransaction.Sell Then
                If sellSLPrice <> Decimal.MinValue AndAlso Math.Abs(sellSLPrice) >= Math.Abs(userSettings.MinStoplossPerTrade) AndAlso Math.Abs(sellSLPrice) <= Math.Abs(userSettings.MaxStoplossPerTrade) Then
                    Dim minStockMaxProft As Decimal = userSettings.StockMaxProfitPerDay
                    Dim targetPL As Decimal = Math.Max(minStockMaxProft, Math.Abs(sellSLPrice) * userSettings.TargetMultiplier)
                    Dim targetPrice As Decimal = CalculateTargetFromPL(candle.LowPrice.Value, _quantity, targetPL)

                    ret = New Tuple(Of Boolean, Decimal, IOrder.TypeOfTransaction)(True, targetPrice - candle.LowPrice.Value, IOrder.TypeOfTransaction.Sell)
                ElseIf buySLPrice <> Decimal.MinValue AndAlso Math.Abs(buySLPrice) >= Math.Abs(userSettings.MinStoplossPerTrade) AndAlso Math.Abs(buySLPrice) <= Math.Abs(userSettings.MaxStoplossPerTrade) Then
                    Dim minStockMaxProft As Decimal = userSettings.StockMaxProfitPerDay
                    Dim targetPL As Decimal = Math.Max(minStockMaxProft, Math.Abs(buySLPrice) * userSettings.TargetMultiplier)
                    Dim targetPrice As Decimal = CalculateTargetFromPL(candle.HighPrice.Value, _quantity, targetPL)

                    ret = New Tuple(Of Boolean, Decimal, IOrder.TypeOfTransaction)(True, targetPrice - candle.HighPrice.Value, IOrder.TypeOfTransaction.Buy)
                End If
            End If
            If print Then
                Try
                    logger.Debug("Signal Details -> Top Wick:{0}, Bottom Wick:{1}, Buy SL Amount:{2}, Sell SL Amount:{3}, Trading Symbol:{4}",
                                 candle.CandleWicks.Top, candle.CandleWicks.Bottom, buySLPrice, sellSLPrice)
                Catch ex As Exception
                    logger.Error(ex.ToString)
                End Try
            End If
        End If
        Return ret
    End Function

    Private Function GetStoplossPrice(ByVal candle As OHLCPayload, ByVal direction As IOrder.TypeOfTransaction) As Decimal
        Dim ret As Decimal = Decimal.MinValue
        If direction = IOrder.TypeOfTransaction.Buy Then
            Dim buffer As Decimal = CalculateBuffer(candle.HighPrice.Value, Me.TradableInstrument.TickSize, RoundOfType.Floor)
            If candle.CandleWicks.Top >= buffer Then
                Dim slPoint As Decimal = candle.CandleWicks.Top + buffer
                ret = _APIAdapter.CalculatePLWithBrokerage(Me.TradableInstrument, candle.HighPrice.Value, candle.HighPrice.Value - slPoint, _quantity)
            End If
        ElseIf direction = IOrder.TypeOfTransaction.Sell Then
            Dim buffer As Decimal = CalculateBuffer(candle.LowPrice.Value, Me.TradableInstrument.TickSize, RoundOfType.Floor)
            If candle.CandleWicks.Bottom >= buffer Then
                Dim slPoint As Decimal = candle.CandleWicks.Bottom + buffer
                ret = _APIAdapter.CalculatePLWithBrokerage(Me.TradableInstrument, candle.LowPrice.Value + slPoint, candle.LowPrice.Value, _quantity)
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
