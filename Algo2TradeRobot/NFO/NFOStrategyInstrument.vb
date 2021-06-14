Imports NLog
Imports System.Threading
Imports Utilities.Numbers
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

    Private _lastLogTime As Date = Now.Date

    Private _quantityToTrade As Integer
    Private _orderType As IOrder.TypeOfOrder
    Private _allowedSpreadPercentage As Decimal

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
                RawPayloadDependentConsumers.Add(chartConsumer)
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

    Public Async Function TakeTradeAsync(ordertype As IOrder.TypeOfOrder, quantity As Integer, signalCandleTime As Date, allowedSpreadPercentage As Decimal) As Task(Of Boolean)
        Dim ret As Boolean = False
        _orderType = ordertype
        _quantityToTrade = quantity
        _allowedSpreadPercentage = allowedSpreadPercentage
        Try
            While True
                If Me.ParentStrategy.ParentController.OrphanException IsNot Nothing Then
                    Throw Me.ParentStrategy.ParentController.OrphanException
                End If
                'If Me._RMSException IsNot Nothing AndAlso
                '    _RMSException.ExceptionType = Algo2TradeCore.Exceptions.AdapterBusinessException.TypeOfException.RMSError Then
                '    OnHeartbeat(String.Format("{0}:Will not take no more action in this instrument as RMS Error occured. Error-{1}", Me.TradableInstrument.TradingSymbol, _RMSException.Message))
                '    Throw Me._RMSException
                'End If
                _cts.Token.ThrowIfCancellationRequested()
                Dim placeOrderTriggers As List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)) = Await IsTriggerReceivedForPlaceOrderAsync(False).ConfigureAwait(False)
                If placeOrderTriggers IsNot Nothing AndAlso placeOrderTriggers.Count > 0 AndAlso
                    placeOrderTriggers.FirstOrDefault.Item1 = ExecuteCommandAction.Take Then
                    Dim orderResponse
                    If _orderType = IOrder.TypeOfOrder.Limit Then
                        orderResponse = Await ExecuteCommandAsync(ExecuteCommands.PlaceRegularLimitCNCOrder, Nothing).ConfigureAwait(False)
                    Else
                        orderResponse = Await ExecuteCommandAsync(ExecuteCommands.PlaceRegularMarketCNCOrder, Nothing).ConfigureAwait(False)
                    End If
                    If orderResponse IsNot Nothing AndAlso orderResponse.Count > 0 Then
                        Dim placeOrderResponse = CType(orderResponse, Concurrent.ConcurrentBag(Of Object)).FirstOrDefault
                        If placeOrderResponse.ContainsKey("data") AndAlso
                            placeOrderResponse("data").ContainsKey("order_id") Then
                            Dim orderID As String = placeOrderResponse("data")("order_id")
                            ret = Await ModifyOrCancelTradeAsync(orderID, signalCandleTime).ConfigureAwait(False)
                        End If
                    End If
                End If

                If ret OrElse Now >= signalCandleTime.AddMinutes(Me.ParentStrategy.UserSettings.SignalTimeFrame * 2) Then
                    Exit While
                End If

                Await Task.Delay(1000, _cts.Token).ConfigureAwait(False)
            End While
        Catch ex As Exception
            logger.Error("Strategy Instrument:{0}, error:{1}", Me.ToString, ex.ToString)
            Throw ex
        End Try
        Return ret
    End Function

    Public Async Function ModifyOrCancelTradeAsync(orderID As String, signalCandleTime As Date) As Task(Of Boolean)
        Dim ret As Boolean = False
        Try
            Await Task.Delay(3000, _cts.Token).ConfigureAwait(False)
            While True
                If Me.ParentStrategy.ParentController.OrphanException IsNot Nothing Then
                    Throw Me.ParentStrategy.ParentController.OrphanException
                End If
                'If Me._RMSException IsNot Nothing AndAlso
                '    _RMSException.ExceptionType = Algo2TradeCore.Exceptions.AdapterBusinessException.TypeOfException.RMSError Then
                '    OnHeartbeat(String.Format("{0}:Will not take no more action in this instrument as RMS Error occured. Error-{1}", Me.TradableInstrument.TradingSymbol, _RMSException.Message))
                '    Throw Me._RMSException
                'End If
                _cts.Token.ThrowIfCancellationRequested()
                If Me.OrderDetails IsNot Nothing AndAlso Me.OrderDetails.ContainsKey(orderID) Then
                    If Me.OrderDetails(orderID).ParentOrder.Status = IOrder.TypeOfStatus.Complete Then
                        ret = True
                        Exit While
                    ElseIf Me.OrderDetails(orderID).ParentOrder.Status = IOrder.TypeOfStatus.Cancelled Then
                        Exit While
                    End If
                End If

                If Now < signalCandleTime.AddMinutes(Me.ParentStrategy.UserSettings.SignalTimeFrame * 2) Then
                    _cts.Token.ThrowIfCancellationRequested()
                    Dim modifyTargetOrderTrigger As List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)) = Await IsTriggerReceivedForModifyTargetOrderAsync(False).ConfigureAwait(False)
                    If modifyTargetOrderTrigger IsNot Nothing AndAlso modifyTargetOrderTrigger.Count > 0 Then
                        Dim orderResponse = Await ExecuteCommandAsync(ExecuteCommands.ModifyTargetOrder, Nothing).ConfigureAwait(False)
                    End If
                Else
                    _cts.Token.ThrowIfCancellationRequested()
                    Dim cancelOrderTrigger As List(Of Tuple(Of ExecuteCommandAction, IOrder, String)) = Await IsTriggerReceivedForExitOrderAsync(False).ConfigureAwait(False)
                    If cancelOrderTrigger IsNot Nothing AndAlso cancelOrderTrigger.Count > 0 Then
                        Dim orderResponse = Await ExecuteCommandAsync(ExecuteCommands.CancelRegularOrder, Nothing).ConfigureAwait(False)
                    End If
                End If

                Await Task.Delay(3000, _cts.Token).ConfigureAwait(False)
            End While
        Catch ex As Exception
            logger.Error("Strategy Instrument:{0}, error:{1}", Me.ToString, ex.ToString)
            Throw ex
        End Try
        Return ret
    End Function

    Protected Overrides Async Function IsTriggerReceivedForPlaceOrderAsync(ByVal forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)))
        Dim ret As List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        Dim currentTick As ITick = Me.TradableInstrument.LastTick
        Dim currentTime As Date = Now()
        Dim log As Boolean = False
        If currentTime >= _lastLogTime.AddMinutes(1) OrElse forcePrint Then
            _lastLogTime = currentTime
            log = True
        End If

        Dim parameters As PlaceOrderParameters = Nothing
        If Not IsActiveInstrument() AndAlso currentTick IsNot Nothing Then
            Dim spread As Decimal = Math.Abs(currentTick.FirstBidPrice - currentTick.FirstOfferPrice)
            Dim spreadPer As Decimal = (spread / currentTick.LastPrice) * 100

            Dim signalCandle As New OHLCPayload(OHLCPayload.PayloadSource.CalculatedTick)
            signalCandle.OpenPrice.Value = currentTick.LastPrice
            signalCandle.LowPrice.Value = currentTick.LastPrice
            signalCandle.HighPrice.Value = currentTick.LastPrice
            signalCandle.ClosePrice.Value = currentTick.LastPrice
            signalCandle.Volume.Value = currentTick.Volume
            signalCandle.SnapshotDateTime = currentTick.Timestamp.Value.Date
            signalCandle.TradingSymbol = Me.TradableInstrument.TradingSymbol

            If signalCandle IsNot Nothing Then
                If _orderType = IOrder.TypeOfOrder.Limit Then
                    If log Then
                        OnHeartbeat(String.Format("LTP={0}, Spread={1} [Bid({2})-Offer({3})], Spread %({4})<=Allowed Spread %({5})[{6}], Quantity={7}, Order Type={8}, LTP {9} Price[{10}]",
                                                  currentTick.LastPrice, spread, currentTick.FirstBidPrice, currentTick.FirstOfferPrice,
                                                  Math.Round(spreadPer, 4), _allowedSpreadPercentage, spreadPer <= _allowedSpreadPercentage,
                                                  _quantityToTrade, _orderType.ToString,
                                                  If(_quantityToTrade > 0, "> Offer", "< Bid"),
                                                  If(_quantityToTrade > 0, currentTick.LastPrice > currentTick.FirstOfferPrice, currentTick.LastPrice < currentTick.FirstBidPrice)))
                    End If
                    If spreadPer <= _allowedSpreadPercentage Then
                        If _quantityToTrade > 0 AndAlso currentTick.LastPrice > currentTick.FirstOfferPrice Then
                            parameters = New PlaceOrderParameters(signalCandle) With
                                     {
                                        .EntryDirection = IOrder.TypeOfTransaction.Buy,
                                        .OrderType = _orderType,
                                        .Price = currentTick.FirstOfferPrice,
                                        .Quantity = Math.Abs(_quantityToTrade)
                                     }
                        ElseIf _quantityToTrade < 0 AndAlso currentTick.LastPrice < currentTick.FirstBidPrice Then
                            parameters = New PlaceOrderParameters(signalCandle) With
                                     {
                                        .EntryDirection = IOrder.TypeOfTransaction.Sell,
                                        .OrderType = _orderType,
                                        .Price = currentTick.FirstBidPrice,
                                        .Quantity = Math.Abs(_quantityToTrade)
                                     }
                        End If
                    End If
                ElseIf _orderType = IOrder.TypeOfOrder.Market Then
                    If log Then
                        OnHeartbeat(String.Format("LTP={0}, Spread={1} [Bid({2})-Offer({3})], Quantity={4}, Order Type={5}",
                                                  currentTick.LastPrice, spread, currentTick.FirstBidPrice, currentTick.FirstOfferPrice,
                                                  _quantityToTrade, _orderType.ToString))
                    End If
                    If _quantityToTrade > 0 Then
                        parameters = New PlaceOrderParameters(signalCandle) With
                                 {
                                    .EntryDirection = IOrder.TypeOfTransaction.Buy,
                                    .OrderType = _orderType,
                                    .Quantity = Math.Abs(_quantityToTrade)
                                 }
                    ElseIf _quantityToTrade < 0 Then
                        parameters = New PlaceOrderParameters(signalCandle) With
                                 {
                                    .EntryDirection = IOrder.TypeOfTransaction.Sell,
                                    .OrderType = _orderType,
                                    .Quantity = Math.Abs(_quantityToTrade)
                                 }
                    End If
                Else
                    Throw New NotImplementedException
                End If
            End If
        Else
            If log Then
                Try
                    logger.Debug("Is Active Instrument:{0}, Current Tick:{1}",
                             IsActiveInstrument(), If(currentTick Is Nothing, "Nothing", currentTick.LastPrice))
                Catch ex As Exception
                    logger.Warn(ex.ToString)
                End Try
            End If
        End If

        'Below portion have to be done in every place order trigger
        If parameters IsNot Nothing AndAlso currentTime >= Me.TradableInstrument.ExchangeDetails.ExchangeStartTime AndAlso currentTime <= Me.TradableInstrument.ExchangeDetails.ExchangeEndTime Then
            Try
                If forcePrint Then
                    logger.Debug("PlaceOrder-> ************************************************ {0}", Me.TradableInstrument.TradingSymbol)
                    logger.Debug("PlaceOrder Parameters-> {0},{1}", parameters.ToString, Me.TradableInstrument.TradingSymbol)
                    logger.Fatal(Utilities.Strings.JsonSerialize(currentTick))
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

    Protected Overrides Async Function IsTriggerReceivedForModifyTargetOrderAsync(forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)))
        Dim ret As List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        Dim currentTick As ITick = Me.TradableInstrument.LastTick
        Dim currentTime As Date = Now()
        Dim spread As Decimal = Math.Abs(currentTick.FirstBidPrice - currentTick.FirstOfferPrice)
        Dim spreadPer As Decimal = (spread / currentTick.LastPrice) * 100
        If OrderDetails IsNot Nothing AndAlso OrderDetails.Count > 0 Then
            For Each runningParentOrder In OrderDetails.Keys
                Dim parentBussinessOrder As IBusinessOrder = OrderDetails(runningParentOrder)
                If parentBussinessOrder.ParentOrder.Status = IOrder.TypeOfStatus.Open Then
                    Dim price As Decimal = Decimal.MinValue
                    Dim reason As String = ""
                    If parentBussinessOrder.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Buy Then
                        If currentTick.LastPrice > currentTick.FirstOfferPrice AndAlso spreadPer <= _allowedSpreadPercentage Then
                            price = currentTick.FirstOfferPrice
                            reason = "Offer Price Change"
                        End If
                    ElseIf parentBussinessOrder.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Sell Then
                        If currentTick.LastPrice < currentTick.FirstBidPrice AndAlso spreadPer <= _allowedSpreadPercentage Then
                            price = currentTick.FirstBidPrice
                            reason = "Bid Price Change"
                        End If
                    End If
                    If price <> Decimal.MinValue AndAlso parentBussinessOrder.ParentOrder.Price <> price Then
                        'Below portion have to be done in every modify target order trigger
                        Dim currentSignalActivities As ActivityDashboard = Me.ParentStrategy.SignalManager.GetSignalActivities(parentBussinessOrder.ParentOrder.Tag)
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
                        ret.Add(New Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)(ExecuteCommandAction.Take, parentBussinessOrder.ParentOrder, price, reason))
                    End If
                End If
            Next
        End If
        If forcePrint AndAlso ret IsNot Nothing AndAlso ret.Count > 0 Then
            For Each runningOrder In ret
                logger.Debug("***** Modify Target ***** Order ID:{0}, Price:{1}, Reason:{2}, {3}", runningOrder.Item2.OrderIdentifier, runningOrder.Item3, runningOrder.Item4, Me.TradableInstrument.TradingSymbol)
            Next
        End If
        Return ret
    End Function

    Protected Overrides Async Function IsTriggerReceivedForExitOrderAsync(ByVal forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, String)))
        Dim ret As List(Of Tuple(Of ExecuteCommandAction, IOrder, String)) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        Dim currentTick As ITick = Me.TradableInstrument.LastTick
        Dim currentTime As Date = Now()
        If OrderDetails IsNot Nothing AndAlso OrderDetails.Count > 0 Then
            For Each runningParentOrder In OrderDetails.Keys
                Dim parentBussinessOrder As IBusinessOrder = OrderDetails(runningParentOrder)
                If parentBussinessOrder.ParentOrder.Status = IOrder.TypeOfStatus.Open Then
                    'Below portion have to be done in every cancel order trigger
                    Dim currentSignalActivities As ActivityDashboard = Me.ParentStrategy.SignalManager.GetSignalActivities(parentBussinessOrder.ParentOrder.Tag)
                    If currentSignalActivities IsNot Nothing Then
                        If currentSignalActivities.CancelActivity.RequestStatus = ActivityDashboard.SignalStatusType.Handled OrElse
                            currentSignalActivities.CancelActivity.RequestStatus = ActivityDashboard.SignalStatusType.Activated OrElse
                            currentSignalActivities.CancelActivity.RequestStatus = ActivityDashboard.SignalStatusType.Completed Then
                            Continue For
                        End If
                    End If
                    If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, IOrder, String))
                    ret.Add(New Tuple(Of ExecuteCommandAction, IOrder, String)(ExecuteCommandAction.Take, parentBussinessOrder.ParentOrder, "Time over"))
                End If
            Next
        End If
        If forcePrint AndAlso ret IsNot Nothing AndAlso ret.Count > 0 Then
            For Each runningOrder In ret
                logger.Debug("***** Cancel Order ***** Order ID:{0}, Reason:{1}, {2}", runningOrder.Item2.OrderIdentifier, runningOrder.Item3, Me.TradableInstrument.TradingSymbol)
            Next
        End If
        Return ret
    End Function

    Public Function GetXMinutePayload(ByVal timeFrame As Integer) As Concurrent.ConcurrentDictionary(Of Date, IPayload)
        Dim ret As Concurrent.ConcurrentDictionary(Of Date, IPayload) = Nothing
        If Me.RawPayloadDependentConsumers IsNot Nothing AndAlso Me.RawPayloadDependentConsumers.Count > 0 Then
            Dim XMinutePayloadConsumer As PayloadToChartConsumer =
                RawPayloadDependentConsumers.Find(Function(x)
                                                      If x.GetType Is GetType(PayloadToChartConsumer) Then
                                                          Return CType(x, PayloadToChartConsumer).Timeframe = timeFrame
                                                      Else
                                                          Return Nothing
                                                      End If
                                                  End Function)

            If XMinutePayloadConsumer IsNot Nothing AndAlso
                XMinutePayloadConsumer.ConsumerPayloads IsNot Nothing AndAlso
                XMinutePayloadConsumer.ConsumerPayloads.Count > 0 Then
                ret = XMinutePayloadConsumer.ConsumerPayloads
            End If
        End If
        Return ret
    End Function

    Public Function IsActiveOrder() As Boolean
        Dim ret As Boolean = False
        If Me.OrderDetails IsNot Nothing AndAlso Me.OrderDetails.Count > 0 Then
            For Each runningOrder In Me.OrderDetails
                If runningOrder.Value.ParentOrder IsNot Nothing AndAlso
                    runningOrder.Value.ParentOrder.Status = IOrder.TypeOfStatus.Open Then
                    ret = True
                    Exit For
                End If
            Next
        End If
        Return ret
    End Function

#Region "Not Required For This Strategy"
    Public Overrides Function MonitorAsync() As Task
        Throw New NotImplementedException
    End Function

    Public Overrides Function MonitorAsync(ByVal command As ExecuteCommands, ByVal data As Object) As Task
        Throw New NotImplementedException
    End Function

    Protected Overrides Function IsTriggerReceivedForModifyStoplossOrderAsync(ByVal forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)))
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