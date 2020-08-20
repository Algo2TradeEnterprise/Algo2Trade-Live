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

    Private _lastPrevPayloadPlaceOrder As String = ""
    Private _targetReached As Boolean = False
    Private _lastDayFractalLow As Decimal = Decimal.MinValue
    Private _lastDayFractalChanged As Boolean = False

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
                Dim chartConsumer As PayloadToChartConsumer = New PayloadToChartConsumer(Me.ParentStrategy.UserSettings.SignalTimeFrame)
                chartConsumer.OnwardLevelConsumers = New List(Of IPayloadConsumer) From {New FractalConsumer(chartConsumer)}
                RawPayloadDependentConsumers.Add(chartConsumer)

                _dummyFractalConsumer = New FractalConsumer(chartConsumer)
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
                                candleCreator.IndicatorCreator.CalculateFractal(currentXMinute, consumer)
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
                    Dim orderResponse = Await ExecuteCommandAsync(ExecuteCommands.PlaceRegularMarketCNCOrder, Nothing).ConfigureAwait(False)
                    If orderResponse IsNot Nothing AndAlso orderResponse.Count > 0 Then
                        Dim placeOrderResponse = CType(orderResponse, Concurrent.ConcurrentBag(Of Object)).FirstOrDefault
                        If placeOrderResponse.ContainsKey("data") AndAlso
                            placeOrderResponse("data").ContainsKey("order_id") Then
                            If placeOrderTriggers.FirstOrDefault.Item2.EntryDirection = IOrder.TypeOfTransaction.Sell Then
                                _targetReached = True
                            End If
                            OnHeartbeat(String.Format("Trade Placed. Signal Candle: {0}", placeOrderTriggers.FirstOrDefault.Item2.SignalCandle.SnapshotDateTime.ToString("HH:mm:ss")))
                        End If
                    End If
                End If
                'Place Order block end

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
        Dim fractalData As FractalConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummyFractalConsumer)
        Dim currentTick As ITick = Me.TradableInstrument.LastTick
        Dim currentTime As Date = Now()
        Dim parameters As PlaceOrderParameters = Nothing

        Dim targetPrice As Decimal = Decimal.MinValue
        Dim lastExecutedOrder As IBusinessOrder = GetLastExecutedOrder()
        If lastExecutedOrder IsNot Nothing Then
            targetPrice = GetTargetPrice(lastExecutedOrder.ParentOrder.AveragePrice)
        End If

        Try
            If runningCandlePayload IsNot Nothing AndAlso runningCandlePayload.PreviousPayload IsNot Nothing AndAlso
                Me.TradableInstrument.IsHistoricalCompleted Then
                If Not runningCandlePayload.PreviousPayload.ToString = _lastPrevPayloadPlaceOrder Then
                    _lastPrevPayloadPlaceOrder = runningCandlePayload.PreviousPayload.ToString
                    logger.Debug("PlaceOrder-> Potential Signal Candle is:{0}. Will check rest parameters.", runningCandlePayload.PreviousPayload.ToString)
                    logger.Debug("PlaceOrder-> Rest all parameters: Running Candle:{0}, PayloadGeneratedBy:{1}, IsHistoricalCompleted:{2}, IsFirstTimeInformationCollected:{3}, Fractal High:{4}, Fractal Low:{5}, Target Price:{6}, Target Reached:{7}, Total PL:{8}, Stock PL:{9}, Lock:{10}, Current Time:{11}, Current Tick:{12}, TradingSymbol:{13}",
                                runningCandlePayload.SnapshotDateTime.ToString("dd-MM-yyyy HH:mm:ss"),
                                runningCandlePayload.PayloadGeneratedBy.ToString,
                                Me.TradableInstrument.IsHistoricalCompleted,
                                Me.ParentStrategy.IsFirstTimeInformationCollected,
                                CType(fractalData.ConsumerPayloads(runningCandlePayload.PreviousPayload.SnapshotDateTime), FractalConsumer.FractalPayload).FractalHigh.Value,
                                CType(fractalData.ConsumerPayloads(runningCandlePayload.PreviousPayload.SnapshotDateTime), FractalConsumer.FractalPayload).FractalLow.Value,
                                If(targetPrice = Decimal.MinValue, "Not Set", targetPrice),
                                _targetReached,
                                Me.ParentStrategy.GetTotalPLAfterBrokerage(),
                                Me.GetOverallPLAfterBrokerage(),
                                CType(Me.ParentStrategy, NFOStrategy).TakeTradeLock,
                                currentTime.ToString,
                                currentTick.LastPrice,
                                Me.TradableInstrument.TradingSymbol)
                End If
            End If
        Catch ex As Exception
            logger.Warn(ex)
        End Try

        Dim checkOfEntryOrder As Boolean = False
        If runningCandlePayload IsNot Nothing AndAlso targetPrice <> Decimal.MinValue Then
            If Not _targetReached AndAlso (currentTick.LastPrice >= targetPrice OrElse runningCandlePayload.HighPrice.Value >= targetPrice) Then
                Dim totalQuantity As Long = GetTotalQuantityTraded()
                If totalQuantity > 0 Then
                    parameters = New PlaceOrderParameters(runningCandlePayload) With
                                           {.EntryDirection = IOrder.TypeOfTransaction.Sell,
                                            .OrderType = IOrder.TypeOfOrder.Market,
                                            .Quantity = totalQuantity}
                End If
            Else
                checkOfEntryOrder = True
            End If
        Else
            checkOfEntryOrder = True
        End If
        If Not _targetReached AndAlso checkOfEntryOrder AndAlso currentTime > userSettings.EODExitTime Then
            checkOfEntryOrder = False
            Dim totalQuantity As Long = GetTotalQuantityTraded()
            If totalQuantity > 0 Then
                parameters = New PlaceOrderParameters(runningCandlePayload) With
                                       {.EntryDirection = IOrder.TypeOfTransaction.Sell,
                                        .OrderType = IOrder.TypeOfOrder.Market,
                                        .Quantity = totalQuantity}
            End If
        End If

        If checkOfEntryOrder Then
            Dim parentStrategy As NFOStrategy = Me.ParentStrategy
            Try
                'Check Lock
                If forcePrint Then
                    While parentStrategy.TakeTradeLock >= userSettings.NumberOfStockToTrade
                        _cts.Token.ThrowIfCancellationRequested()
                        Await Task.Delay(10, _cts.Token).ConfigureAwait(False)
                    End While
                    Interlocked.Increment(parentStrategy.TakeTradeLock)
                End If

                If currentTime >= userSettings.TradeStartTime AndAlso currentTime <= userSettings.LastTradeEntryTime AndAlso currentTime <= userSettings.EODExitTime AndAlso
                    runningCandlePayload IsNot Nothing AndAlso runningCandlePayload.SnapshotDateTime >= userSettings.TradeStartTime AndAlso Not _targetReached AndAlso
                    runningCandlePayload.PayloadGeneratedBy = OHLCPayload.PayloadSource.CalculatedTick AndAlso runningCandlePayload.PreviousPayload IsNot Nothing AndAlso
                    Me.TradableInstrument.IsHistoricalCompleted AndAlso fractalData.ConsumerPayloads IsNot Nothing AndAlso fractalData.ConsumerPayloads.Count > 0 Then
                    Dim signal As Tuple(Of Boolean, OHLCPayload, Integer, Decimal) = GetEntrySignal(runningCandlePayload, currentTick, forcePrint)
                    If signal IsNot Nothing AndAlso signal.Item1 Then
                        Dim signalCandle As OHLCPayload = signal.Item2
                        Dim quantity As Integer = signal.Item3

                        If signalCandle IsNot Nothing AndAlso quantity > 0 Then
                            parameters = New PlaceOrderParameters(signalCandle) With
                                            {.EntryDirection = IOrder.TypeOfTransaction.Buy,
                                             .OrderType = IOrder.TypeOfOrder.Market,
                                             .Quantity = quantity,
                                             .Supporting = New List(Of Object) From {signal.Item4}}
                        End If
                    End If
                End If
            Finally
                If forcePrint Then Interlocked.Decrement(parentStrategy.TakeTradeLock)
            End Try
        End If

        ''Below portion have to be done in every place order trigger
        If parameters IsNot Nothing Then
            Try
                If forcePrint Then
                    logger.Debug("PlaceOrder-> ************************************************ {0}", Me.TradableInstrument.TradingSymbol)
                    logger.Debug("PlaceOrder Parameters-> {0}, {1}", parameters.ToString, Me.TradableInstrument.TradingSymbol)
                End If
            Catch ex As Exception
                logger.Warn(ex)
            End Try

            If parameters IsNot Nothing Then
                Dim currentSignalActivities As IEnumerable(Of ActivityDashboard) = Me.ParentStrategy.SignalManager.GetSignalActivities(parameters.SignalCandle.SnapshotDateTime, Me.TradableInstrument.InstrumentIdentifier)
                If currentSignalActivities IsNot Nothing AndAlso currentSignalActivities.Count > 0 Then
                    Dim lastPlacedActivity As ActivityDashboard = currentSignalActivities.OrderBy(Function(x)
                                                                                                      Return x.EntryActivity.RequestTime
                                                                                                  End Function).LastOrDefault
                    If lastPlacedActivity.EntryActivity.RequestStatus = ActivityDashboard.SignalStatusType.Discarded AndAlso
                            lastPlacedActivity.EntryActivity.LastException IsNot Nothing AndAlso
                            lastPlacedActivity.EntryActivity.LastException.Message.ToUpper.Contains("TIME") Then
                        Await Task.Delay(Me.ParentStrategy.ParentController.UserInputs.BackToBackOrderCoolOffDelay * 1000, _cts.Token).ConfigureAwait(False)

                        If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                        ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.WaitAndTake, parameters, parameters.ToString))
                    End If
                Else
                    If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                    ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.Take, parameters, parameters.ToString))
                End If
            End If
        End If
        Return ret
    End Function

    Protected Overrides Function IsTriggerReceivedForModifyStoplossOrderAsync(ByVal forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)))
        Throw New NotImplementedException()
    End Function

    Protected Overrides Function IsTriggerReceivedForModifyTargetOrderAsync(forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)))
        Throw New NotImplementedException()
    End Function

    Protected Overrides Function IsTriggerReceivedForExitOrderAsync(ByVal forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, String)))
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

    Private Function GetEntrySignal(ByVal runningCandle As OHLCPayload, ByVal currentTick As ITick, ByVal forcePrint As Boolean) As Tuple(Of Boolean, OHLCPayload, Integer, Decimal)
        Dim ret As Tuple(Of Boolean, OHLCPayload, Integer, Decimal) = Nothing
        If runningCandle IsNot Nothing AndAlso runningCandle.PreviousPayload IsNot Nothing Then
            Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
            Dim fractalData As FractalConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummyFractalConsumer)
            _lastDayFractalChanged = IsLastDayFractalChanged(fractalData, runningCandle)
            If _lastDayFractalChanged Then
                Dim lastExecutedOrder As IBusinessOrder = GetLastExecutedOrder()
                If fractalData.ConsumerPayloads IsNot Nothing AndAlso fractalData.ConsumerPayloads.ContainsKey(runningCandle.PreviousPayload.SnapshotDateTime) Then
                    Dim fractal As FractalConsumer.FractalPayload = fractalData.ConsumerPayloads(runningCandle.PreviousPayload.SnapshotDateTime)
                    If runningCandle.PreviousPayload.ClosePrice.Value < fractal.FractalLow.Value Then
                        Dim signalCandle As OHLCPayload = Nothing
                        If lastExecutedOrder IsNot Nothing Then
                            Dim targetPrice As Decimal = GetTargetPrice(lastExecutedOrder.ParentOrder.AveragePrice)
                            If targetPrice <> Decimal.MinValue Then
                                Dim lastOrderSignalCandle As OHLCPayload = GetSignalCandleOfAnOrder(lastExecutedOrder.ParentOrderIdentifier, userSettings.SignalTimeFrame)
                                If lastOrderSignalCandle IsNot Nothing AndAlso lastOrderSignalCandle.SnapshotDateTime <> runningCandle.PreviousPayload.SnapshotDateTime Then
                                    If fractalData.ConsumerPayloads.ContainsKey(lastOrderSignalCandle.SnapshotDateTime) Then
                                        Dim lastFractal As FractalConsumer.FractalPayload = fractalData.ConsumerPayloads(lastOrderSignalCandle.SnapshotDateTime)
                                        If lastFractal.FractalLow.Value <> fractal.FractalLow.Value AndAlso targetPrice <> fractal.FractalHigh.Value Then
                                            signalCandle = runningCandle.PreviousPayload
                                        End If
                                    End If
                                End If
                            End If
                        Else
                            signalCandle = runningCandle.PreviousPayload
                        End If

                        If signalCandle IsNot Nothing Then
                            Dim entryPrice As Decimal = currentTick.LastPrice
                            Dim targetPrice As Decimal = fractal.FractalHigh.Value
                            Dim quantity As Integer = CalculateQuantity(entryPrice, targetPrice, userSettings.MaxProfitPerStock)
                            If quantity * entryPrice > userSettings.MaxTurnoverPerTrade Then
                                While quantity * entryPrice > userSettings.MaxTurnoverPerTrade
                                    targetPrice += Me.TradableInstrument.TickSize
                                    quantity = CalculateQuantity(entryPrice, targetPrice, userSettings.MaxProfitPerStock)
                                End While
                            End If
                            If quantity > 0 Then
                                ret = New Tuple(Of Boolean, OHLCPayload, Integer, Decimal)(True, signalCandle, quantity, targetPrice)
                            End If
                        End If
                    End If
                End If
            End If
        End If
        Return ret
    End Function

    Private Function CalculateQuantity(ByVal entryPrice As Decimal, ByVal targetPrice As Decimal, ByVal targetPL As Decimal) As Integer
        Dim ret As Integer = 0
        Dim unrealizedPL As Decimal = 0
        If Me.OrderDetails IsNot Nothing AndAlso Me.OrderDetails.Count > 0 Then
            For Each runningOrder In Me.OrderDetails.Values
                If runningOrder.ParentOrder IsNot Nothing AndAlso runningOrder.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Buy AndAlso
                    runningOrder.ParentOrder.Status = IOrder.TypeOfStatus.Complete Then
                    unrealizedPL += _APIAdapter.CalculatePLWithBrokerage(Me.TradableInstrument, runningOrder.ParentOrder.AveragePrice, targetPrice, runningOrder.ParentOrder.Quantity)
                End If
            Next
        End If
        Dim plToAchive As Decimal = targetPL - unrealizedPL
        If plToAchive > 0 Then
            Dim qty As Integer = CalculateQuantityFromTarget(entryPrice, targetPrice, targetPL)
            ret = Math.Ceiling(qty / Me.TradableInstrument.LotSize) * Me.TradableInstrument.LotSize
        End If
        Return ret
    End Function

    Private Function GetTargetPrice(ByVal lastTradeEntryPrice As Decimal) As Decimal
        Dim ret As Decimal = Decimal.MinValue
        If Me.OrderDetails IsNot Nothing AndAlso Me.OrderDetails.Count > 0 Then
            Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
            For target As Decimal = lastTradeEntryPrice To Decimal.MaxValue Step Me.TradableInstrument.TickSize
                Dim plAfterBrokerage As Decimal = 0
                For Each runningOrder In Me.OrderDetails.Values
                    If runningOrder.ParentOrder IsNot Nothing AndAlso runningOrder.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Buy AndAlso
                        runningOrder.ParentOrder.Status = IOrder.TypeOfStatus.Complete Then
                        plAfterBrokerage += _APIAdapter.CalculatePLWithBrokerage(Me.TradableInstrument, runningOrder.ParentOrder.AveragePrice, target, runningOrder.ParentOrder.Quantity)
                    End If
                Next
                If plAfterBrokerage >= userSettings.MaxProfitPerStock Then
                    ret = target
                    Exit For
                End If
            Next
        End If
        Return ret
    End Function

    Private Function GetTotalQuantityTraded() As Long
        Dim ret As Long = 0
        If Me.OrderDetails IsNot Nothing AndAlso Me.OrderDetails.Count > 0 Then
            For Each runningOrder In Me.OrderDetails.Values
                If runningOrder.ParentOrder IsNot Nothing AndAlso runningOrder.ParentOrder.Status = IOrder.TypeOfStatus.Complete Then
                    If runningOrder.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Buy Then
                        ret += runningOrder.ParentOrder.Quantity
                    ElseIf runningOrder.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Sell Then
                        ret -= runningOrder.ParentOrder.Quantity
                    End If
                End If
            Next
        End If
        Return ret
    End Function

    Private Function IsLastDayFractalChanged(ByVal fractaldata As FractalConsumer, ByVal runningCandle As OHLCPayload) As Boolean
        Dim ret As Boolean = _lastDayFractalChanged
        If Not _lastDayFractalChanged AndAlso runningCandle.PreviousPayload IsNot Nothing Then
            If _lastDayFractalLow = Decimal.MinValue Then
                Dim firstCandleOfTheDay As OHLCPayload = GetXMinuteFirstCandleOfTheDay(Me.ParentStrategy.UserSettings.SignalTimeFrame)
                If firstCandleOfTheDay IsNot Nothing AndAlso firstCandleOfTheDay.PreviousPayload IsNot Nothing Then
                    If fractaldata.ConsumerPayloads IsNot Nothing AndAlso fractaldata.ConsumerPayloads.ContainsKey(firstCandleOfTheDay.PreviousPayload.SnapshotDateTime) Then
                        Dim fractal As FractalConsumer.FractalPayload = fractaldata.ConsumerPayloads(firstCandleOfTheDay.PreviousPayload.SnapshotDateTime)
                        _lastDayFractalLow = fractal.FractalLow.Value
                    End If
                End If
            End If
            If _lastDayFractalLow <> Decimal.MinValue Then
                If Me.RawPayloadDependentConsumers IsNot Nothing AndAlso Me.RawPayloadDependentConsumers.Count > 0 Then
                    Dim XMinutePayloadConsumer As PayloadToChartConsumer = RawPayloadDependentConsumers.Find(Function(x)
                                                                                                                 If x.GetType Is GetType(PayloadToChartConsumer) Then
                                                                                                                     Return CType(x, PayloadToChartConsumer).Timeframe = Me.ParentStrategy.UserSettings.SignalTimeFrame
                                                                                                                 Else
                                                                                                                     Return Nothing
                                                                                                                 End If
                                                                                                             End Function)

                    If XMinutePayloadConsumer IsNot Nothing AndAlso XMinutePayloadConsumer.ConsumerPayloads IsNot Nothing AndAlso XMinutePayloadConsumer.ConsumerPayloads.Count > 0 Then
                        Dim currentDayPayloads As IEnumerable(Of KeyValuePair(Of Date, IPayload)) = XMinutePayloadConsumer.ConsumerPayloads.Where(Function(y)
                                                                                                                                                      Return y.Key.Date = Now.Date
                                                                                                                                                  End Function)

                        If currentDayPayloads IsNot Nothing AndAlso currentDayPayloads.Count > 0 Then
                            For Each runningPayload In currentDayPayloads.OrderBy(Function(x)
                                                                                      Return x.Key
                                                                                  End Function)
                                If runningPayload.Key <= runningCandle.PreviousPayload.SnapshotDateTime Then
                                    If fractaldata.ConsumerPayloads.ContainsKey(runningPayload.Key) Then
                                        Dim fractal As FractalConsumer.FractalPayload = fractaldata.ConsumerPayloads(runningPayload.Key)
                                        If fractal.FractalLow.Value <> _lastDayFractalLow Then
                                            ret = True
                                            Exit For
                                        End If
                                    End If
                                End If
                            Next
                        End If
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