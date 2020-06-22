Imports System.Threading
Imports Algo2TradeCore.Adapter
Imports Algo2TradeCore.Entities
Imports Algo2TradeCore.Strategies
Imports Utilities.Numbers
Imports NLog
Imports Algo2TradeCore.ChartHandler.ChartStyle

Public Class NFOStrategyInstrument
    Inherits StrategyInstrument
    Implements IDisposable

#Region "Logging and Status Progress"
    Public Shared Shadows logger As Logger = LogManager.GetCurrentClassLogger
#End Region

    Public ReadOnly Property TakeTrade As Boolean = False
    Public Property DirectionToTrade As IOrder.TypeOfTransaction = IOrder.TypeOfTransaction.None

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
    End Sub

#Region "Signal Check"
    Public Async Function CheckSignalAsync() As Task
        Try
            Dim userInputs As NFOUserInputs = Me.ParentStrategy.UserSettings
            Await Task.Delay((Me.ParentStrategy.ParentController.UserInputs.GetInformationDelay + 2) * 1000, _cts.Token).ConfigureAwait(False)
            If Now >= userInputs.TradeStartTime Then
                For Each runningStrategyInstrument In Me.ParentStrategy.TradableStrategyInstruments
                    If runningStrategyInstrument.OrderDetails IsNot Nothing AndAlso runningStrategyInstrument.OrderDetails.Count > 0 Then
                        logger.Debug("Entry done for instrument: {0}", runningStrategyInstrument.TradableInstrument.TradingSymbol)
                        runningStrategyInstrument.MonitorAsync()
                    End If
                Next
            End If

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

                If Me.TradableInstrument.LastTick IsNot Nothing Then
                    If Now >= userInputs.TradeStartTime Then
                        Dim nfoStrategyInstruments As IEnumerable(Of StrategyInstrument) =
                            Me.ParentStrategy.TradableStrategyInstruments.Where(Function(x)
                                                                                    Return x.TradableInstrument.TradingSymbol <> Me.TradableInstrument.TradingSymbol AndAlso
                                                                                    x.TradableInstrument.InstrumentType = IInstrument.TypeOfInstrument.Futures
                                                                                End Function)
                        If nfoStrategyInstruments IsNot Nothing AndAlso nfoStrategyInstruments.Count > 0 Then
                            For Each runningStrategyInstrument As NFOStrategyInstrument In nfoStrategyInstruments
                                Dim cashInstrumnet As NFOStrategyInstrument = GetSimilarCashStrategyInstrument(runningStrategyInstrument.TradableInstrument)
                                If cashInstrumnet IsNot Nothing AndAlso Not cashInstrumnet.TakeTrade Then
                                    Dim bidAskRatio As Decimal = runningStrategyInstrument.GetBidToAskRatio()
                                    Dim askBidRatio As Decimal = runningStrategyInstrument.GetAskToBidRatio()
                                    If bidAskRatio <> Decimal.MinValue AndAlso bidAskRatio > userInputs.BidAskRatio Then
                                        OnHeartbeat(String.Format("{0} -> BidToAskRatio:{1}, AskToBidRatio:{2}, Will take buy trade in this instrument.",
                                                                  cashInstrumnet.TradableInstrument.TradingSymbol,
                                                                  If(bidAskRatio <> Decimal.MinValue, bidAskRatio, "∞"),
                                                                  If(askBidRatio <> Decimal.MinValue, askBidRatio, "∞")))

                                        cashInstrumnet.DirectionToTrade = IOrder.TypeOfTransaction.Buy
                                        cashInstrumnet.MonitorAsync()
                                    ElseIf askBidRatio <> Decimal.MinValue AndAlso askBidRatio > userInputs.BidAskRatio Then
                                        OnHeartbeat(String.Format("{0} -> BidToAskRatio:{1}, AskToBidRatio:{2}, Will take sell trade in this instrument.",
                                                                  cashInstrumnet.TradableInstrument.TradingSymbol,
                                                                  If(bidAskRatio <> Decimal.MinValue, bidAskRatio, "∞"),
                                                                  If(askBidRatio <> Decimal.MinValue, askBidRatio, "∞")))

                                        cashInstrumnet.DirectionToTrade = IOrder.TypeOfTransaction.Sell
                                        cashInstrumnet.MonitorAsync()
                                    Else
                                        OnHeartbeat(String.Format("{0} -> BidToAskRatio:{1}, AskToBidRatio:{2}, Will not take trade in this instrument.",
                                                                  cashInstrumnet.TradableInstrument.TradingSymbol,
                                                                  If(bidAskRatio <> Decimal.MinValue, bidAskRatio, "∞"),
                                                                  If(askBidRatio <> Decimal.MinValue, askBidRatio, "∞")))
                                    End If
                                End If
                            Next
                        End If
                    End If
                End If

                _cts.Token.ThrowIfCancellationRequested()
                Await Task.Delay(1000 * 60, _cts.Token).ConfigureAwait(False)
            End While
        Catch ex As Exception
            'To log exceptions getting created from this function as the bubble up of the exception
            'will anyways happen to Strategy.MonitorAsync but it will not be shown until all tasks exit
            logger.Error("Strategy Instrument:{0}, error:{1}", Me.ToString, ex.ToString)
            Throw ex
        End Try
    End Function

    Private Function GetSimilarCashStrategyInstrument(ByVal futureInstrument As IInstrument) As NFOStrategyInstrument
        Dim ret As NFOStrategyInstrument = Nothing
        ret = Me.ParentStrategy.TradableStrategyInstruments.Where(Function(x)
                                                                      Return x.TradableInstrument.TradingSymbol = futureInstrument.RawInstrumentName AndAlso
                                                                    x.TradableInstrument.InstrumentType = IInstrument.TypeOfInstrument.Cash
                                                                  End Function).FirstOrDefault
        Return ret
    End Function

    Public Function GetAskToBidRatio() As Decimal
        Dim ret As Decimal = Decimal.MinValue
        If Me.TradableInstrument.LastTick IsNot Nothing Then
            Dim lastTick As ITick = Me.TradableInstrument.LastTick
            If lastTick.BuyQuantity <> UInteger.MaxValue AndAlso lastTick.BuyQuantity <> UInteger.MinValue AndAlso lastTick.BuyQuantity <> 0 AndAlso
                lastTick.SellQuantity <> UInteger.MaxValue AndAlso lastTick.SellQuantity <> UInteger.MinValue AndAlso lastTick.SellQuantity <> 0 Then
                ret = Math.Round(lastTick.SellQuantity / lastTick.BuyQuantity, 4)
            End If
        End If
        Return ret
    End Function

    Public Function GetBidToAskRatio() As Decimal
        Dim ret As Decimal = Decimal.MinValue
        If Me.TradableInstrument.LastTick IsNot Nothing Then
            Dim lastTick As ITick = Me.TradableInstrument.LastTick
            If lastTick.BuyQuantity <> UInteger.MaxValue AndAlso lastTick.BuyQuantity <> UInteger.MinValue AndAlso lastTick.BuyQuantity <> 0 AndAlso
                lastTick.SellQuantity <> UInteger.MaxValue AndAlso lastTick.SellQuantity <> UInteger.MinValue AndAlso lastTick.SellQuantity <> 0 Then
                ret = Math.Round(lastTick.BuyQuantity / lastTick.SellQuantity, 4)
            End If
        End If
        Return ret
    End Function
#End Region

    Public Overrides Async Function MonitorAsync() As Task
        Try
            _TakeTrade = True
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
                    Await ExecuteCommandAsync(ExecuteCommands.PlaceCOMarketMISOrder, Nothing).ConfigureAwait(False)
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
                    Await ExecuteCommandAsync(ExecuteCommands.CancelCOOrder, Nothing).ConfigureAwait(False)
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
        Dim currentTick As ITick = Me.TradableInstrument.LastTick
        Dim currentTime As Date = Now()

        Dim parameter1 As PlaceOrderParameters = Nothing
        Dim parameter2 As PlaceOrderParameters = Nothing
        If currentTime >= userSettings.TradeStartTime AndAlso currentTime <= userSettings.EODExitTime AndAlso currentTick IsNot Nothing AndAlso
            GetTotalExecutedOrders() < 1 AndAlso Not IsActiveInstrument() AndAlso Not Me.StrategyExitAllTriggerd Then
            Dim signalCandle As OHLCPayload = New OHLCPayload(OHLCPayload.PayloadSource.CalculatedTick)
            signalCandle.SnapshotDateTime = Now
            signalCandle.OpenPrice.Value = currentTick.LastPrice
            signalCandle.LowPrice.Value = currentTick.LastPrice
            signalCandle.HighPrice.Value = currentTick.LastPrice
            signalCandle.ClosePrice.Value = currentTick.LastPrice

            Dim quantity As Integer = 0
            If userSettings.CalculateQuantityFromCapital Then
                quantity = CalculateQuantityFromInvestment(currentTick.LastPrice, userSettings.MarginMultiplier, userSettings.Capital, False)
            Else
                quantity = userSettings.Quantity
            End If

            Dim buffer As Decimal = CalculateBuffer(currentTick.Open, Me.TradableInstrument.TickSize, RoundOfType.Floor)
            If quantity <> 0 AndAlso DirectionToTrade = IOrder.TypeOfTransaction.Buy Then
                Dim triggerPrice As Decimal = currentTick.Open - buffer
                Dim modifiedQuantity As Integer = Math.Ceiling(quantity / 2)

                parameter1 = New PlaceOrderParameters(signalCandle) With
                                {.EntryDirection = IOrder.TypeOfTransaction.Buy,
                                 .TriggerPrice = triggerPrice,
                                 .OrderType = IOrder.TypeOfOrder.Market,
                                 .Quantity = modifiedQuantity}

                If quantity - modifiedQuantity > 0 Then
                    parameter2 = New PlaceOrderParameters(signalCandle) With
                                {.EntryDirection = IOrder.TypeOfTransaction.Buy,
                                 .TriggerPrice = triggerPrice,
                                 .OrderType = IOrder.TypeOfOrder.Market,
                                 .Quantity = quantity - modifiedQuantity}
                End If
            ElseIf quantity <> 0 AndAlso DirectionToTrade = IOrder.TypeOfTransaction.Sell Then
                Dim triggerPrice As Decimal = currentTick.Open + buffer
                Dim modifiedQuantity As Integer = Math.Ceiling(quantity / 2)

                parameter1 = New PlaceOrderParameters(signalCandle) With
                                    {.EntryDirection = IOrder.TypeOfTransaction.Sell,
                                     .TriggerPrice = triggerPrice,
                                     .OrderType = IOrder.TypeOfOrder.Market,
                                     .Quantity = modifiedQuantity}

                If quantity - modifiedQuantity > 0 Then
                    parameter2 = New PlaceOrderParameters(signalCandle) With
                                    {.EntryDirection = IOrder.TypeOfTransaction.Sell,
                                     .TriggerPrice = triggerPrice,
                                     .OrderType = IOrder.TypeOfOrder.Market,
                                     .Quantity = quantity - modifiedQuantity}
                End If
            End If
        End If

        'Below portion have to be done in every place order trigger
        If parameter1 IsNot Nothing Then
            Try
                If forcePrint Then
                    logger.Debug("PlaceOrder-> ************************************************ {0}", Me.TradableInstrument.TradingSymbol)
                    logger.Debug("PlaceOrder Parameters-> {0},{1}", parameter1.ToString, Me.TradableInstrument.TradingSymbol)
                    If parameter2 IsNot Nothing Then logger.Debug("PlaceOrder Parameters-> {0},{1}", parameter2.ToString, Me.TradableInstrument.TradingSymbol)
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
                        ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.WaitAndTake, parameter1, parameter1.ToString))
                        If parameter2 IsNot Nothing Then ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.WaitAndTake, parameter2, parameter2.ToString))
                    ElseIf lastPlacedActivity.EntryActivity.RequestStatus = ActivityDashboard.SignalStatusType.Handled Then
                        If lastPlacedActivity.SignalDirection = parameter1.EntryDirection Then
                            If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                            ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.DonotTake, parameter1, parameter1.ToString))
                            If parameter2 IsNot Nothing Then ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.DonotTake, parameter1, parameter1.ToString))
                            Try
                                logger.Debug("Will not take this trade as similar previous trade detected. Current Trade->{0}, Last Activity Direction:{1}, Last Activity Status:{2}, Trading Symbol:{3}",
                                             parameter1.ToString, lastPlacedActivity.SignalDirection, lastPlacedActivity.EntryActivity.RequestStatus, Me.TradableInstrument.TradingSymbol)
                            Catch ex As Exception
                                logger.Warn(ex.ToString)
                            End Try
                        Else
                            If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                            ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.Take, parameter1, parameter1.ToString))
                            If parameter2 IsNot Nothing Then ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.Take, parameter2, parameter2.ToString))
                        End If
                    ElseIf lastPlacedActivity.EntryActivity.RequestStatus = ActivityDashboard.SignalStatusType.Activated Then
                        If lastPlacedActivity.SignalDirection = parameter1.EntryDirection Then
                            If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                            ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.DonotTake, parameter1, parameter1.ToString))
                            If parameter2 IsNot Nothing Then ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.DonotTake, parameter2, parameter2.ToString))
                            Try
                                logger.Debug("Will not take this trade as similar previous trade detected. Current Trade->{0}, Last Activity Direction:{1}, Last Activity Status:{2}, Trading Symbol:{3}",
                                             parameter1.ToString, lastPlacedActivity.SignalDirection, lastPlacedActivity.EntryActivity.RequestStatus, Me.TradableInstrument.TradingSymbol)
                            Catch ex As Exception
                                logger.Warn(ex.ToString)
                            End Try
                        Else
                            If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                            ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.Take, parameter1, parameter1.ToString))
                            If parameter2 IsNot Nothing Then ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.Take, parameter2, parameter2.ToString))
                        End If
                    ElseIf lastPlacedActivity.EntryActivity.RequestStatus = ActivityDashboard.SignalStatusType.Rejected Then
                        If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                        ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.DonotTake, parameter1, parameter1.ToString))
                        If parameter2 IsNot Nothing Then ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.DonotTake, parameter2, parameter2.ToString))
                    Else
                        If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                        ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.Take, parameter1, parameter1.ToString))
                        If parameter2 IsNot Nothing Then ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.Take, parameter2, parameter2.ToString))
                    End If
                Else
                    If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                    ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.Take, parameter1, parameter1.ToString))
                    If parameter2 IsNot Nothing Then ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.Take, parameter2, parameter2.ToString))
                End If
            Else
                If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.Take, parameter1, parameter1.ToString))
                If parameter2 IsNot Nothing Then ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.Take, parameter2, parameter2.ToString))
            End If
        End If
        Return ret
    End Function

    Protected Overrides Async Function IsTriggerReceivedForModifyStoplossOrderAsync(ByVal forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)))
        Dim ret As List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        If OrderDetails IsNot Nothing AndAlso OrderDetails.Count > 0 Then
            Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
            Dim currentTick As ITick = Me.TradableInstrument.LastTick
            For Each runningParentOrder In OrderDetails.Keys
                Dim parentBussinessOrder As IBusinessOrder = OrderDetails(runningParentOrder)
                If parentBussinessOrder.ParentOrder.Status = IOrder.TypeOfStatus.Complete AndAlso
                    parentBussinessOrder.SLOrder IsNot Nothing AndAlso parentBussinessOrder.SLOrder.Count > 0 Then
                    For Each runningSLOrder In parentBussinessOrder.SLOrder
                        If Not runningSLOrder.Status = IOrder.TypeOfStatus.Complete AndAlso
                            Not runningSLOrder.Status = IOrder.TypeOfStatus.Cancelled AndAlso
                            Not runningSLOrder.Status = IOrder.TypeOfStatus.Rejected Then
                            Dim triggerPrice As Decimal = Decimal.MinValue
                            Dim reason As String = Nothing
                            Dim entryPrice As Decimal = parentBussinessOrder.ParentOrder.AveragePrice
                            If parentBussinessOrder.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Buy Then
                                Dim gainLoss As Decimal = ((currentTick.LastPrice - entryPrice) / entryPrice) * 100
                                Dim multiplier As Decimal = Math.Floor(gainLoss / userSettings.StoplossTrailingPercentage)
                                If multiplier > 1 Then
                                    Dim stoploss As Decimal = ConvertFloorCeling(entryPrice + entryPrice * 0.1 / 100, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                                    triggerPrice = stoploss + ConvertFloorCeling(stoploss * userSettings.StoplossTrailingPercentage / 100 * (multiplier - 1), Me.TradableInstrument.TickSize, RoundOfType.Floor)
                                    reason = String.Format("Gain:{0}%, So move to {1}%", Math.Round(gainLoss, 2), userSettings.StoplossTrailingPercentage * (multiplier - 1))
                                ElseIf multiplier > 0 Then
                                    triggerPrice = ConvertFloorCeling(entryPrice + entryPrice * 0.1 / 100, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                                    reason = String.Format("Gain:{0}%, So cost to cost movement", Math.Round(gainLoss, 2))
                                End If
                                If triggerPrice <> Decimal.MinValue AndAlso triggerPrice <= runningSLOrder.TriggerPrice Then
                                    triggerPrice = Decimal.MinValue
                                End If
                            ElseIf parentBussinessOrder.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Sell Then
                                Dim gainLoss As Decimal = ((entryPrice - currentTick.LastPrice) / entryPrice) * 100
                                Dim multiplier As Decimal = Math.Floor(gainLoss / userSettings.StoplossTrailingPercentage)
                                If multiplier > 1 Then
                                    Dim stoploss As Decimal = ConvertFloorCeling(entryPrice - entryPrice * 0.1 / 100, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                                    triggerPrice = stoploss - ConvertFloorCeling(stoploss * userSettings.StoplossTrailingPercentage / 100 * (multiplier - 1), Me.TradableInstrument.TickSize, RoundOfType.Floor)
                                    reason = String.Format("Gain:{0}%, So move to {1}%", Math.Round(gainLoss, 2), userSettings.StoplossTrailingPercentage * (multiplier - 1))
                                ElseIf multiplier > 0 Then
                                    triggerPrice = ConvertFloorCeling(entryPrice - entryPrice * 0.1 / 100, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                                    reason = String.Format("Gain:{0}%, So cost to cost movement", Math.Round(gainLoss, 2))
                                End If
                                If triggerPrice <> Decimal.MinValue AndAlso triggerPrice >= runningSLOrder.TriggerPrice Then
                                    triggerPrice = Decimal.MinValue
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
                                ret.Add(New Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)(ExecuteCommandAction.Take, runningSLOrder, triggerPrice, reason))
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

    Protected Overrides Async Function IsTriggerReceivedForExitOrderAsync(ByVal forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, String)))
        Dim ret As List(Of Tuple(Of ExecuteCommandAction, IOrder, String)) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        If OrderDetails IsNot Nothing AndAlso OrderDetails.Count > 0 Then
            Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
            Dim currentTick As ITick = Me.TradableInstrument.LastTick
            Dim parentBussinessOrder As IBusinessOrder = Nothing
            If OrderDetails.Count = 2 Then
                If OrderDetails.FirstOrDefault.Value.ParentOrder.Quantity > OrderDetails.LastOrDefault.Value.ParentOrder.Quantity Then
                    parentBussinessOrder = OrderDetails.FirstOrDefault.Value
                Else
                    parentBussinessOrder = OrderDetails.LastOrDefault.Value
                End If
            Else
                parentBussinessOrder = OrderDetails.FirstOrDefault.Value
            End If
            If parentBussinessOrder IsNot Nothing AndAlso parentBussinessOrder.ParentOrder.Status = IOrder.TypeOfStatus.Complete AndAlso
               parentBussinessOrder.SLOrder IsNot Nothing AndAlso parentBussinessOrder.SLOrder.Count > 0 Then
                For Each runningSLOrder In parentBussinessOrder.SLOrder
                    If Not runningSLOrder.Status = IOrder.TypeOfStatus.Complete AndAlso
                       Not runningSLOrder.Status = IOrder.TypeOfStatus.Cancelled AndAlso
                       Not runningSLOrder.Status = IOrder.TypeOfStatus.Rejected Then
                        Dim exitTrade As Boolean = False
                        Dim reason As String = Nothing
                        Dim entryPrice As Decimal = parentBussinessOrder.ParentOrder.AveragePrice
                        If parentBussinessOrder.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Buy Then
                            Dim gainLoss As Decimal = ((currentTick.LastPrice - entryPrice) / entryPrice) * 100
                            If gainLoss >= userSettings.StoplossTrailingPercentage Then
                                exitTrade = True
                                reason = String.Format("Gain:{0}%. So hard close.", Math.Round(gainLoss, 2))
                            End If
                        ElseIf parentBussinessOrder.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Sell Then
                            Dim gainLoss As Decimal = ((entryPrice - currentTick.LastPrice) / entryPrice) * 100
                            If gainLoss >= userSettings.StoplossTrailingPercentage Then
                                exitTrade = True
                                reason = String.Format("Gain:{0}%. So hard close.", Math.Round(gainLoss, 2))
                            End If
                        End If

                        If exitTrade Then
                            'Below portion have to be done in every cancel order trigger
                            Dim currentSignalActivities As ActivityDashboard = Me.ParentStrategy.SignalManager.GetSignalActivities(runningSLOrder.Tag)
                            If currentSignalActivities IsNot Nothing Then
                                If currentSignalActivities.CancelActivity.RequestStatus = ActivityDashboard.SignalStatusType.Handled OrElse
                                   currentSignalActivities.CancelActivity.RequestStatus = ActivityDashboard.SignalStatusType.Activated OrElse
                                   currentSignalActivities.CancelActivity.RequestStatus = ActivityDashboard.SignalStatusType.Completed Then
                                    Continue For
                                End If
                            End If
                            If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, IOrder, String))
                            ret.Add(New Tuple(Of ExecuteCommandAction, IOrder, String)(ExecuteCommandAction.Take, runningSLOrder, reason))
                        End If
                    End If
                Next
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

            Await ExecuteCommandAsync(ExecuteCommands.ForceCancelCOOrder, cancellableOrder).ConfigureAwait(False)
        End If
    End Function

#Region "Not Required For This Strategy"
    Public Overrides Function PopulateChartAndIndicatorsAsync(candleCreator As Chart, currentCandle As OHLCPayload) As Task
        Throw New NotImplementedException()
    End Function

    Public Overrides Function MonitorAsync(ByVal command As ExecuteCommands, ByVal data As Object) As Task
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