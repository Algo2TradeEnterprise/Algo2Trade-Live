Imports System.Threading
Imports Algo2TradeCore.Adapter
Imports Algo2TradeCore.Entities
Imports Algo2TradeCore.Strategies
Imports Utilities.Numbers
Imports NLog
Imports Algo2TradeCore.Entities.Indicators

Public Class NearFarHedgingStrategyInstrument
    Inherits StrategyInstrument
    Implements IDisposable

    Private lastPrevPayloadPlaceOrder As String = ""
    Public ReadOnly DummySpreadRatioConsumer As SpreadRatioConsumer
    Public ReadOnly DummySpreadBollingerConsumer As BollingerConsumer

    Private _placeOrderParameter As PlaceOrderParameters = Nothing
    Private _cancelTrades As Boolean = Nothing

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
            If Me.ParentStrategy.UserSettings.SignalTimeFrame > 0 AndAlso Not Me.IsPairInstrument Then
                Dim chartConsumer As PayloadToChartConsumer = New PayloadToChartConsumer(Me.ParentStrategy.UserSettings.SignalTimeFrame)
                RawPayloadDependentConsumers.Add(chartConsumer)
            ElseIf Me.ParentStrategy.UserSettings.SignalTimeFrame > 0 AndAlso Me.IsPairInstrument Then
                Dim pairConsumer As PayloadToPairConsumer = New PayloadToPairConsumer()
                pairConsumer.OnwardLevelConsumers = New List(Of IPayloadConsumer)
                Dim spreadRatioData As SpreadRatioConsumer = New SpreadRatioConsumer(pairConsumer, TypeOfField.Close)
                spreadRatioData.OnwardLevelConsumers = New List(Of IPayloadConsumer)
                Dim spreadBollinger As BollingerConsumer = New BollingerConsumer(spreadRatioData, CType(Me.ParentStrategy.UserSettings, NearFarHedgingUserInputs).BollingerPeriod, CType(Me.ParentStrategy.UserSettings, NearFarHedgingUserInputs).BollingerMultiplier, TypeOfField.Spread)
                spreadRatioData.OnwardLevelConsumers.Add(spreadBollinger)
                pairConsumer.OnwardLevelConsumers.Add(spreadRatioData)
                RawPayloadDependentConsumers.Add(pairConsumer)
                DummySpreadRatioConsumer = New SpreadRatioConsumer(pairConsumer, TypeOfField.Close)
                DummySpreadBollingerConsumer = New BollingerConsumer(spreadRatioData, CType(Me.ParentStrategy.UserSettings, NearFarHedgingUserInputs).BollingerPeriod, CType(Me.ParentStrategy.UserSettings, NearFarHedgingUserInputs).BollingerMultiplier, TypeOfField.Spread)
            Else
                Throw New ApplicationException(String.Format("Signal Timeframe is 0 or Nothing, does not adhere to the strategy:{0}", Me.ParentStrategy.ToString))
            End If
        End If
    End Sub
    Public Overrides Async Function MonitorAsync() As Task
        Try
            If Me.IsPairInstrument Then
                Dim lastTriggerTime As Date = Date.MinValue
                Dim hedgingUserInputs As NearFarHedgingUserInputs = Me.ParentStrategy.UserSettings
                While True
                    If Me.ParentStrategy.ParentController.OrphanException IsNot Nothing Then
                        Throw Me.ParentStrategy.ParentController.OrphanException
                    End If

                    _cts.Token.ThrowIfCancellationRequested()
                    Dim placeOrderTrigger As List(Of Tuple(Of ExecuteCommandAction, StrategyInstrument, PlaceOrderParameters, String)) = Await IsTriggerReceivedForPlaceOrderAsync(False, Nothing).ConfigureAwait(False)
                    If placeOrderTrigger IsNot Nothing AndAlso placeOrderTrigger.Count > 0 Then
                        For Each runningPlaceOrderTrigger In placeOrderTrigger
                            runningPlaceOrderTrigger.Item2.MonitorAsync(StrategyInstrument.ExecuteCommands.PlaceCOMarketMISOrder, runningPlaceOrderTrigger)
                        Next
                    End If

                    _cts.Token.ThrowIfCancellationRequested()

                    Dim cancelOrderTrigger As List(Of Tuple(Of ExecuteCommandAction, StrategyInstrument, IOrder, String)) = Await IsTriggerReceivedForExitOrderAsync(False, Nothing).ConfigureAwait(False)
                    If cancelOrderTrigger IsNot Nothing AndAlso cancelOrderTrigger.Count > 0 Then
                        For Each runningCancelOrderTrigger In cancelOrderTrigger
                            runningCancelOrderTrigger.Item2.MonitorAsync(StrategyInstrument.ExecuteCommands.CancelCOOrder, runningCancelOrderTrigger)
                        Next
                    End If

                    _cts.Token.ThrowIfCancellationRequested()
                    Await Task.Delay(1000, _cts.Token).ConfigureAwait(False)
                End While
                _cts.Token.ThrowIfCancellationRequested()
            End If
        Catch ex As Exception
            'To log exceptions getting created from this function as the bubble up of the exception
            'will anyways happen to Strategy.MonitorAsync but it will not be shown until all tasks exit
            logger.Error("Strategy Instrument:{0}, error:{1}", Me.ToString, ex.ToString)
            Throw ex
        End Try
    End Function

    Public Overrides Async Function MonitorAsync(ByVal command As ExecuteCommands, ByVal data As Object) As Task
        Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
        Try
            If Not Me.IsPairInstrument Then
                If Me.ParentStrategy.ParentController.OrphanException IsNot Nothing Then
                    Throw Me.ParentStrategy.ParentController.OrphanException
                End If
                _cts.Token.ThrowIfCancellationRequested()
                Select Case command
                    Case ExecuteCommands.PlaceCOMarketMISOrder
                        Dim placeOrderTrigger As Tuple(Of ExecuteCommandAction, StrategyInstrument, PlaceOrderParameters, String) = data
                        If placeOrderTrigger IsNot Nothing AndAlso placeOrderTrigger.Item1 = ExecuteCommandAction.Take AndAlso
                            (Not Me.IsActiveInstrument OrElse (Me.IsActiveInstrument AndAlso Me.PairStrategyCancellationRequest)) Then
                            _placeOrderParameter = placeOrderTrigger.Item3
                            Dim placeOrderResponse As Object = Await ExecuteCommandAsync(ExecuteCommands.PlaceCOMarketMISOrder, Nothing).ConfigureAwait(False)
                            If placeOrderResponse IsNot Nothing Then
                                _placeOrderParameter = Nothing
                            End If
                        End If
                    Case ExecuteCommands.CancelCOOrder
                        Dim cancelOrderTrigger As Tuple(Of ExecuteCommandAction, StrategyInstrument, IOrder, String) = data
                        If cancelOrderTrigger IsNot Nothing AndAlso cancelOrderTrigger.Item1 = ExecuteCommandAction.Take Then
                            _cancelTrades = True
                            Dim exitOrderResponse As Object = Await ExecuteCommandAsync(ExecuteCommands.CancelCOOrder, Nothing).ConfigureAwait(False)
                            If exitOrderResponse IsNot Nothing Then
                                _cancelTrades = False
                            End If
                        End If
                End Select
                _cts.Token.ThrowIfCancellationRequested()
            End If
        Catch ex As Exception
            'To log exceptions getting created from this function as the bubble up of the exception
            'will anyways happen to Strategy.MonitorAsync but it will not be shown until all tasks exit
            logger.Error("Strategy Instrument:{0}, error:{1}", Me.ToString, ex.ToString)
            Throw ex
        End Try
    End Function

    Protected Overrides Async Function IsTriggerReceivedForPlaceOrderAsync(ByVal forcePrint As Boolean, ByVal data As Object) As Task(Of List(Of Tuple(Of ExecuteCommandAction, StrategyInstrument, PlaceOrderParameters, String)))
        Dim ret As List(Of Tuple(Of ExecuteCommandAction, StrategyInstrument, PlaceOrderParameters, String)) = Nothing
        Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
        Dim hedgingUserInputs As NearFarHedgingUserInputs = Me.ParentStrategy.UserSettings
        Dim potentialSignalData As Tuple(Of Boolean, IOrder.TypeOfTransaction, PairPayload, OHLCPayload) = GetCurrentSignal(False)
        Dim runningCandlePayload As PairPayload = Nothing
        Dim currentCandlePayload As OHLCPayload = Nothing
        If potentialSignalData IsNot Nothing Then
            runningCandlePayload = potentialSignalData.Item3
            currentCandlePayload = potentialSignalData.Item4
        End If

        Dim virtualInstrument As IInstrument = Me.TradableInstrument

        Dim parameters As PlaceOrderParameters = Nothing
        If Now >= hedgingUserInputs.TradeStartTime AndAlso Now <= hedgingUserInputs.LastTradeEntryTime AndAlso runningCandlePayload IsNot Nothing AndAlso
            (runningCandlePayload.Instrument1Payload IsNot Nothing OrElse runningCandlePayload.Instrument2Payload IsNot Nothing) AndAlso
            Not IsLogicalActiveInstrument() AndAlso Me.IsPairInstrument AndAlso currentCandlePayload IsNot Nothing Then

            Dim spreadRatioConsumer As SpreadRatioConsumer = GetConsumer(Me.RawPayloadDependentConsumers, DummySpreadRatioConsumer)

            If currentCandlePayload.PayloadGeneratedBy = OHLCPayload.PayloadSource.CalculatedTick AndAlso currentCandlePayload.PreviousPayload IsNot Nothing AndAlso
                spreadRatioConsumer IsNot Nothing AndAlso spreadRatioConsumer.HigherContract IsNot Nothing Then

                If potentialSignalData IsNot Nothing AndAlso potentialSignalData.Item1 Then
                    Dim triggerPrice As Decimal = Decimal.MinValue
                    Dim quantity As Integer = 0

                    If Me.ParentStrategyInstruments IsNot Nothing AndAlso Me.ParentStrategyInstruments.Count = 2 Then
                        For Each runningParentStrategyInstrument In Me.ParentStrategyInstruments
                            If Not runningParentStrategyInstrument.IsActiveInstrument OrElse (runningParentStrategyInstrument.IsActiveInstrument AndAlso runningParentStrategyInstrument.PairStrategyCancellationRequest) Then
                                Dim pair1StrategyInstrument As NearFarHedgingStrategyInstrument = Me.ParentStrategyInstruments.FirstOrDefault
                                Dim pair2StrategyInstrument As NearFarHedgingStrategyInstrument = Me.ParentStrategyInstruments.LastOrDefault
                                Dim higherContract As NearFarHedgingStrategyInstrument = Nothing
                                Dim lowerContract As NearFarHedgingStrategyInstrument = Nothing
                                If pair1StrategyInstrument.TradableInstrument.TradingSymbol.ToUpper = spreadRatioConsumer.HigherContract.TradingSymbol.ToUpper Then
                                    higherContract = pair1StrategyInstrument
                                    lowerContract = pair2StrategyInstrument
                                Else
                                    higherContract = pair2StrategyInstrument
                                    lowerContract = pair1StrategyInstrument
                                End If

                                quantity = runningParentStrategyInstrument.TradableInstrument.LotSize
                                If hedgingUserInputs.InstrumentsData(Me.TradableInstrument.TradingSymbol).Pair1TradingSymbol = runningParentStrategyInstrument.TradableInstrument.TradingSymbol Then
                                    quantity = quantity * hedgingUserInputs.InstrumentsData(Me.TradableInstrument.TradingSymbol).Pair1Quantity
                                Else
                                    quantity = quantity * hedgingUserInputs.InstrumentsData(Me.TradableInstrument.TradingSymbol).Pair2Quantity
                                End If
                                triggerPrice = 0

                                If higherContract.TradableInstrument.TradingSymbol = runningParentStrategyInstrument.TradableInstrument.TradingSymbol Then
                                    parameters = New PlaceOrderParameters(currentCandlePayload.PreviousPayload) With
                                                         {
                                                           .EntryDirection = potentialSignalData.Item2,
                                                           .Quantity = Math.Floor(quantity),
                                                           .TriggerPrice = triggerPrice
                                                         }
                                ElseIf lowerContract.TradableInstrument.TradingSymbol = runningParentStrategyInstrument.TradableInstrument.TradingSymbol Then
                                    parameters = New PlaceOrderParameters(currentCandlePayload.PreviousPayload) With
                                                         {
                                                           .EntryDirection = If(potentialSignalData.Item2 = IOrder.TypeOfTransaction.Buy, IOrder.TypeOfTransaction.Sell, IOrder.TypeOfTransaction.Buy),
                                                           .Quantity = Math.Floor(quantity),
                                                           .TriggerPrice = triggerPrice
                                                         }
                                End If
                                If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, StrategyInstrument, PlaceOrderParameters, String))
                                ret.Add(New Tuple(Of ExecuteCommandAction, StrategyInstrument, PlaceOrderParameters, String)(ExecuteCommandAction.Take, runningParentStrategyInstrument, parameters, String.Format("Signal Time:{0}, Signal Direction:{1}", currentCandlePayload.PreviousPayload.SnapshotDateTime.ToString, potentialSignalData.Item2.ToString)))
                            End If
                        Next
                    End If
                End If
            End If
        End If
        Return ret
    End Function

    Protected Overrides Async Function IsTriggerReceivedForPlaceOrderAsync(forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)))
        Dim ret As List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)

        If _placeOrderParameter IsNot Nothing Then
            Dim currentSignalActivities As IEnumerable(Of ActivityDashboard) = Me.ParentStrategy.SignalManager.GetSignalActivities(_placeOrderParameter.SignalCandle.SnapshotDateTime, Me.TradableInstrument.InstrumentIdentifier)
            If currentSignalActivities IsNot Nothing AndAlso currentSignalActivities.Count > 0 Then
                Dim placedActivities As IEnumerable(Of ActivityDashboard) = currentSignalActivities.Where(Function(x)
                                                                                                              Return x.EntryActivity.RequestRemarks = _placeOrderParameter.ToString
                                                                                                          End Function)
                If placedActivities IsNot Nothing AndAlso placedActivities.Count > 0 Then
                    Dim lastPlacedActivity As ActivityDashboard = placedActivities.OrderBy(Function(x)
                                                                                               Return x.EntryActivity.RequestTime
                                                                                           End Function).LastOrDefault
                    If lastPlacedActivity.EntryActivity.RequestStatus = ActivityDashboard.SignalStatusType.Discarded AndAlso
                            lastPlacedActivity.EntryActivity.LastException IsNot Nothing AndAlso
                            lastPlacedActivity.EntryActivity.LastException.Message.ToUpper.Contains("TIME") Then
                        If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                        ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.WaitAndTake, _placeOrderParameter, _placeOrderParameter.ToString))
                    ElseIf lastPlacedActivity.EntryActivity.RequestStatus = ActivityDashboard.SignalStatusType.Handled Then
                        If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                        ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.DonotTake, _placeOrderParameter, _placeOrderParameter.ToString))
                    ElseIf lastPlacedActivity.EntryActivity.RequestStatus = ActivityDashboard.SignalStatusType.Activated Then
                        If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                        ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.DonotTake, _placeOrderParameter, _placeOrderParameter.ToString))
                    ElseIf lastPlacedActivity.EntryActivity.RequestStatus = ActivityDashboard.SignalStatusType.Rejected Then
                        If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                        ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.DonotTake, _placeOrderParameter, _placeOrderParameter.ToString))
                    Else
                        If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                        ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.Take, _placeOrderParameter, _placeOrderParameter.ToString))
                    End If
                Else
                    If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                    ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.Take, _placeOrderParameter, _placeOrderParameter.ToString))
                End If
            Else
                If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.Take, _placeOrderParameter, _placeOrderParameter.ToString))
            End If
        End If
        Return ret
    End Function

    Protected Overrides Async Function IsTriggerReceivedForModifyStoplossOrderAsync(forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)))
        Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
        Throw New NotImplementedException()
    End Function

    Protected Overrides Async Function IsTriggerReceivedForModifyTargetOrderAsync(forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)))
        Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
        Throw New NotImplementedException()
    End Function

    Protected Overrides Async Function IsTriggerReceivedForExitOrderAsync(forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, String)))
        Dim ret As List(Of Tuple(Of ExecuteCommandAction, IOrder, String)) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        Dim allActiveOrders As List(Of IOrder) = GetAllActiveOrders(IOrder.TypeOfTransaction.None)
        If allActiveOrders IsNot Nothing AndAlso allActiveOrders.Count > 0 AndAlso Me.TradableInstrument.IsHistoricalCompleted AndAlso _cancelTrades Then
            Dim parentOrders As List(Of IOrder) = allActiveOrders.FindAll(Function(x)
                                                                              Return x.ParentOrderIdentifier Is Nothing
                                                                          End Function)
            If parentOrders IsNot Nothing AndAlso parentOrders.Count > 0 Then
                For Each parentOrder In parentOrders
                    Dim parentBusinessOrder As IBusinessOrder = OrderDetails(parentOrder.OrderIdentifier)
                    If parentBusinessOrder.SLOrder IsNot Nothing AndAlso parentBusinessOrder.SLOrder.Count > 0 Then
                        For Each slOrder In parentBusinessOrder.SLOrder
                            If Not slOrder.Status = IOrder.TypeOfStatus.Complete AndAlso
                                    Not slOrder.Status = IOrder.TypeOfStatus.Cancelled AndAlso
                                    Not slOrder.Status = IOrder.TypeOfStatus.Rejected Then
                                If Not _cancelTrades Then
                                    If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, IOrder, String))
                                    ret.Add(New Tuple(Of ExecuteCommandAction, IOrder, String)(ExecuteCommandAction.Take, slOrder, "Opposite Trade"))
                                End If
                            End If
                        Next
                    End If
                Next
            End If
        End If
        Return ret
    End Function

    Protected Overrides Async Function IsTriggerReceivedForExitOrderAsync(forcePrint As Boolean, data As Object) As Task(Of List(Of Tuple(Of ExecuteCommandAction, StrategyInstrument, IOrder, String)))
        Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
        Dim ret As List(Of Tuple(Of ExecuteCommandAction, StrategyInstrument, IOrder, String)) = Nothing
        Dim hedgingUserInputs As NearFarHedgingUserInputs = Me.ParentStrategy.UserSettings
        Dim potentialSignalData As Tuple(Of Boolean, IOrder.TypeOfTransaction, PairPayload, OHLCPayload) = GetCurrentSignal(False)
        Dim runningCandlePayload As PairPayload = Nothing
        Dim currentCandlePayload As OHLCPayload = Nothing
        If potentialSignalData IsNot Nothing Then
            runningCandlePayload = potentialSignalData.Item3
            currentCandlePayload = potentialSignalData.Item4
        End If

        If Me.IsPairInstrument AndAlso Me.ParentStrategyInstruments IsNot Nothing AndAlso Me.ParentStrategyInstruments.Count > 0 Then
            Dim spreadRatioConsumer As SpreadRatioConsumer = GetConsumer(Me.RawPayloadDependentConsumers, DummySpreadRatioConsumer)
            If spreadRatioConsumer IsNot Nothing AndAlso spreadRatioConsumer.HigherContract IsNot Nothing Then
                For Each runningParentStrategyInstrument In Me.ParentStrategyInstruments
                    Dim allActiveOrders As List(Of IOrder) = runningParentStrategyInstrument.GetAllActiveOrders(IOrder.TypeOfTransaction.None)
                    If allActiveOrders IsNot Nothing AndAlso allActiveOrders.Count > 0 AndAlso runningParentStrategyInstrument.TradableInstrument.IsHistoricalCompleted Then
                        Dim parentOrders As List(Of IOrder) = allActiveOrders.FindAll(Function(x)
                                                                                          Return x.ParentOrderIdentifier Is Nothing
                                                                                      End Function)
                        If parentOrders IsNot Nothing AndAlso parentOrders.Count > 0 Then
                            For Each parentOrder In parentOrders
                                Dim parentBusinessOrder As IBusinessOrder = runningParentStrategyInstrument.OrderDetails(parentOrder.OrderIdentifier)
                                If parentBusinessOrder.SLOrder IsNot Nothing AndAlso parentBusinessOrder.SLOrder.Count > 0 Then
                                    For Each slOrder In parentBusinessOrder.SLOrder
                                        Dim tradeWillExit As Boolean = False
                                        If Not slOrder.Status = IOrder.TypeOfStatus.Complete AndAlso
                                    Not slOrder.Status = IOrder.TypeOfStatus.Cancelled AndAlso
                                    Not slOrder.Status = IOrder.TypeOfStatus.Rejected Then
                                            If potentialSignalData IsNot Nothing AndAlso potentialSignalData.Item1 Then
                                                Dim isHigherContract As Boolean = False
                                                If Me.ParentStrategyInstruments IsNot Nothing AndAlso Me.ParentStrategyInstruments.Count = 2 Then
                                                    Dim pair1StrategyInstrument As NearFarHedgingStrategyInstrument = Me.ParentStrategyInstruments.FirstOrDefault
                                                    Dim pair2StrategyInstrument As NearFarHedgingStrategyInstrument = Me.ParentStrategyInstruments.LastOrDefault
                                                    Dim higherContract As NearFarHedgingStrategyInstrument = Nothing
                                                    If pair1StrategyInstrument.TradableInstrument.TradingSymbol.ToUpper = spreadRatioConsumer.HigherContract.TradingSymbol.ToUpper Then
                                                        higherContract = pair1StrategyInstrument
                                                    Else
                                                        higherContract = pair2StrategyInstrument
                                                    End If
                                                    If runningParentStrategyInstrument.TradableInstrument.TradingSymbol = higherContract.TradableInstrument.TradingSymbol Then
                                                        isHigherContract = True
                                                    Else
                                                        isHigherContract = False
                                                    End If
                                                End If

                                                If isHigherContract Then
                                                    If potentialSignalData.Item2 = slOrder.TransactionType Then
                                                        tradeWillExit = True
                                                    End If
                                                Else
                                                    If potentialSignalData.Item2 <> slOrder.TransactionType Then
                                                        tradeWillExit = True
                                                    End If
                                                End If
                                            End If
                                        End If
                                        If tradeWillExit Then
                                            If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, StrategyInstrument, IOrder, String))
                                            ret.Add(New Tuple(Of ExecuteCommandAction, StrategyInstrument, IOrder, String)(ExecuteCommandAction.Take, runningParentStrategyInstrument, slOrder, "Opposite Direction signal"))
                                        End If
                                    Next
                                End If
                            Next
                        End If
                    End If
                Next
            End If
        End If
        Return ret
    End Function

    Protected Overrides Async Function ForceExitSpecificTradeAsync(order As IOrder, reason As String) As Task
        If order IsNot Nothing AndAlso Not order.Status = IOrder.TypeOfStatus.Complete AndAlso
            Not order.Status = IOrder.TypeOfStatus.Cancelled AndAlso
            Not order.Status = IOrder.TypeOfStatus.Rejected Then
            Dim cancellableOrder As New List(Of Tuple(Of ExecuteCommandAction, IOrder, String)) From
            {
                New Tuple(Of ExecuteCommandAction, IOrder, String)(ExecuteCommandAction.Take, order, reason)
            }

            Dim exitOrderResponse As Object = Await ExecuteCommandAsync(ExecuteCommands.ForceCancelCOOrder, cancellableOrder).ConfigureAwait(False)
        End If
    End Function

    Private Function CheckSignal(ByVal spreadRatioData As SpreadRatioConsumer.SpreadRatioPayload,
                                 ByVal spreadBollingerData As BollingerConsumer.BollingerPayload,
                                 ByVal forcePrint As Boolean) As Tuple(Of Boolean, IOrder.TypeOfTransaction)
        Dim ret As Tuple(Of Boolean, IOrder.TypeOfTransaction) = Nothing
        Dim hedgingUserInputs As NearFarHedgingUserInputs = Me.ParentStrategy.UserSettings
        If spreadRatioData IsNot Nothing AndAlso spreadBollingerData IsNot Nothing Then
            If forcePrint Then
                logger.Debug("{0}, {1}", spreadRatioData.ToString, spreadBollingerData.ToString)
            End If
            Dim spreadSignal As Tuple(Of Boolean, IOrder.TypeOfTransaction) = Nothing
            If spreadRatioData.Spread IsNot Nothing AndAlso
                spreadBollingerData.HighBollinger IsNot Nothing AndAlso
                spreadBollingerData.LowBollinger IsNot Nothing Then
                If spreadRatioData.Spread.Value >= spreadBollingerData.HighBollinger.Value Then
                    spreadSignal = New Tuple(Of Boolean, IOrder.TypeOfTransaction)(True, IOrder.TypeOfTransaction.Sell)
                ElseIf spreadRatioData.Spread.Value <= spreadBollingerData.LowBollinger.Value Then
                    spreadSignal = New Tuple(Of Boolean, IOrder.TypeOfTransaction)(True, IOrder.TypeOfTransaction.Buy)
                End If
            End If

            If spreadSignal IsNot Nothing AndAlso spreadSignal.Item1 Then ret = New Tuple(Of Boolean, IOrder.TypeOfTransaction)(True, spreadSignal.Item2)
        End If
        If forcePrint Then
            If ret IsNot Nothing Then
                logger.Debug("Is Signal Received:{0}, Signal Direction:{1}", ret.Item1, ret.Item2.ToString)
            Else
                logger.Debug("Is Signal Received:False, Signal Direction:None")
            End If
        End If
        Return ret
    End Function

    Private Function GetCurrentSignal(ByVal forcePrint As Boolean) As Tuple(Of Boolean, IOrder.TypeOfTransaction, PairPayload, OHLCPayload)
        Dim ret As Tuple(Of Boolean, IOrder.TypeOfTransaction, PairPayload, OHLCPayload) = Nothing
        Dim hedgingUserInputs As NearFarHedgingUserInputs = Me.ParentStrategy.UserSettings

        Dim runningCandlePayload As PairPayload = Nothing
        Dim spreadRatioConsumer As SpreadRatioConsumer = Nothing
        Dim spreadBollingerConsumer As BollingerConsumer = Nothing

        If Me.IsPairInstrument Then
            runningCandlePayload = GetXMinuteCurrentCandle()
            spreadRatioConsumer = GetConsumer(Me.RawPayloadDependentConsumers, DummySpreadRatioConsumer)
            spreadBollingerConsumer = GetConsumer(Me.RawPayloadDependentConsumers, DummySpreadBollingerConsumer)
        Else
            If Me.DependendStrategyInstruments IsNot Nothing AndAlso Me.DependendStrategyInstruments.Count > 0 Then
                Dim virtualStrategyInstrument As NearFarHedgingStrategyInstrument = Me.DependendStrategyInstruments.FirstOrDefault
                runningCandlePayload = virtualStrategyInstrument.GetXMinuteCurrentCandle()
                spreadRatioConsumer = virtualStrategyInstrument.GetConsumer(virtualStrategyInstrument.RawPayloadDependentConsumers, virtualStrategyInstrument.DummySpreadRatioConsumer)
                spreadBollingerConsumer = virtualStrategyInstrument.GetConsumer(virtualStrategyInstrument.RawPayloadDependentConsumers, virtualStrategyInstrument.DummySpreadBollingerConsumer)
            End If
        End If

        If runningCandlePayload IsNot Nothing Then
            Dim currentCandle As OHLCPayload = Nothing
            If runningCandlePayload.Instrument1Payload IsNot Nothing AndAlso
                                    runningCandlePayload.Instrument1Payload.PayloadGeneratedBy = OHLCPayload.PayloadSource.CalculatedTick AndAlso
                                    runningCandlePayload.Instrument1Payload.PreviousPayload IsNot Nothing AndAlso
                                    runningCandlePayload.Instrument1Payload.SnapshotDateTime >= hedgingUserInputs.TradeStartTime Then
                currentCandle = runningCandlePayload.Instrument1Payload
            ElseIf runningCandlePayload.Instrument2Payload IsNot Nothing AndAlso
                                    runningCandlePayload.Instrument2Payload.PayloadGeneratedBy = OHLCPayload.PayloadSource.CalculatedTick AndAlso
                                    runningCandlePayload.Instrument2Payload.PreviousPayload IsNot Nothing AndAlso
                                    runningCandlePayload.Instrument2Payload.SnapshotDateTime >= hedgingUserInputs.TradeStartTime Then
                currentCandle = runningCandlePayload.Instrument2Payload
            End If
            If currentCandle IsNot Nothing AndAlso currentCandle.PayloadGeneratedBy = OHLCPayload.PayloadSource.CalculatedTick AndAlso currentCandle.PreviousPayload IsNot Nothing AndAlso
                spreadRatioConsumer.ConsumerPayloads IsNot Nothing AndAlso spreadRatioConsumer.ConsumerPayloads.Count > 0 AndAlso
                spreadBollingerConsumer.ConsumerPayloads IsNot Nothing AndAlso spreadBollingerConsumer.ConsumerPayloads.Count > 0 AndAlso
                spreadRatioConsumer.ConsumerPayloads.ContainsKey(currentCandle.PreviousPayload.SnapshotDateTime) AndAlso
                spreadBollingerConsumer.ConsumerPayloads.ContainsKey(currentCandle.PreviousPayload.SnapshotDateTime) Then

                'For Each runningPayload In spreadRatioConsumer.ConsumerPayloads.OrderBy(Function(x)
                '                                                                            Return x.Key
                '                                                                        End Function)
                '    Dim sprdPayload As SpreadRatioConsumer.SpreadRatioPayload = runningPayload.Value
                '    Dim blngrPayload As BollingerConsumer.BollingerPayload = spreadBollingerConsumer.ConsumerPayloads(runningPayload.Key)

                '    Console.WriteLine(String.Format("{0},{1},{2},{3},{4}",
                '                                    runningPayload.Key.ToString("dd-MM-yyyy HH:mm:ss"),
                '                                    sprdPayload.Spread.Value,
                '                                    blngrPayload.SMABollinger.Value,
                '                                    blngrPayload.HighBollinger.Value,
                '                                    blngrPayload.LowBollinger.Value))
                'Next


                Dim signalCandleTime As Date = currentCandle.PreviousPayload.SnapshotDateTime
                Dim potentialSignalData As Tuple(Of Boolean, IOrder.TypeOfTransaction) = Nothing
                potentialSignalData = CheckSignal(spreadRatioConsumer.ConsumerPayloads(signalCandleTime),
                                                  spreadBollingerConsumer.ConsumerPayloads(signalCandleTime),
                                                  forcePrint)

                If potentialSignalData IsNot Nothing Then
                    ret = New Tuple(Of Boolean, IOrder.TypeOfTransaction, PairPayload, OHLCPayload)(potentialSignalData.Item1, potentialSignalData.Item2, runningCandlePayload, currentCandle)
                Else
                    ret = New Tuple(Of Boolean, IOrder.TypeOfTransaction, PairPayload, OHLCPayload)(False, IOrder.TypeOfTransaction.None, runningCandlePayload, currentCandle)
                End If
            End If
        End If
        Return ret
    End Function

    Private Function IsLogicalActiveInstrument() As Boolean
        Dim ret As Boolean = False
        If Me.IsPairInstrument Then
            If Me.ParentStrategyInstruments IsNot Nothing AndAlso Me.ParentStrategyInstruments.Count >= 2 Then
                ret = True
                For Each runningParentStrategyInstrumentStrategy In Me.ParentStrategyInstruments
                    ret = ret And runningParentStrategyInstrumentStrategy.IsActiveInstrument
                Next
            End If
        End If
        Return ret
    End Function

    Private Function GetAnotherPairStrategyInstrument() As NearFarHedgingStrategyInstrument
        Dim ret As NearFarHedgingStrategyInstrument = Nothing
        If Not Me.IsPairInstrument Then
            If Me.DependendStrategyInstruments IsNot Nothing AndAlso Me.DependendStrategyInstruments.Count > 0 Then
                Dim virtualStrategyInstrument As NearFarHedgingStrategyInstrument = Me.DependendStrategyInstruments.FirstOrDefault
                If virtualStrategyInstrument.IsPairInstrument Then
                    If virtualStrategyInstrument.ParentStrategyInstruments IsNot Nothing AndAlso
                        virtualStrategyInstrument.ParentStrategyInstruments.Count > 0 Then
                        For Each runningStrategyInstrument In virtualStrategyInstrument.ParentStrategyInstruments
                            If Me.TradableInstrument.TradingSymbol <> runningStrategyInstrument.TradableInstrument.TradingSymbol Then
                                ret = runningStrategyInstrument
                                Exit For
                            End If
                        Next
                    End If
                End If
            End If
        End If
        Return ret
    End Function

    Public Function GetPairPLAfterBrokerage() As Decimal
        Dim ret As Decimal = 0
        If Me.IsPairInstrument Then
            If Me.ParentStrategyInstruments IsNot Nothing AndAlso Me.ParentStrategyInstruments.Count > 0 Then
                For Each runningParentStrategyInstrument In Me.ParentStrategyInstruments
                    ret += runningParentStrategyInstrument.GetOverallPLAfterBrokerage
                Next
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
