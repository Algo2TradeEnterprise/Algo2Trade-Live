Imports System.Threading
Imports Algo2TradeCore.Adapter
Imports Algo2TradeCore.Entities
Imports Algo2TradeCore.Strategies
Imports Utilities.Numbers
Imports NLog
Imports Algo2TradeCore.Entities.Indicators
Imports Algo2TradeCore.ChartHandler.ChartStyle

Public Class CDSStrategyInstrument
    Inherits StrategyInstrument
    Implements IDisposable

#Region "Logging and Status Progress"
    Public Shared Shadows logger As Logger = LogManager.GetCurrentClassLogger
#End Region

    Private _lastPrevPayloadPlaceOrder As String = ""
    Private ReadOnly _dummyLFSupertrendConsumer As SupertrendConsumer
    Private ReadOnly _dummyHFSupertrendConsumer As SupertrendConsumer

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
                chartConsumer.OnwardLevelConsumers = New List(Of IPayloadConsumer) From
                {New SupertrendConsumer(chartConsumer, CType(Me.ParentStrategy.UserSettings, CDSUserInputs).Period, CType(Me.ParentStrategy.UserSettings, CDSUserInputs).Multiplier)}
                RawPayloadDependentConsumers.Add(chartConsumer)
                _dummyLFSupertrendConsumer = New SupertrendConsumer(chartConsumer, CType(Me.ParentStrategy.UserSettings, CDSUserInputs).Period, CType(Me.ParentStrategy.UserSettings, CDSUserInputs).Multiplier)

                Dim chartConsumer2 As PayloadToChartConsumer = New PayloadToChartConsumer(CType(Me.ParentStrategy.UserSettings, CDSUserInputs).HigherTimeframe)
                chartConsumer2.OnwardLevelConsumers = New List(Of IPayloadConsumer) From
                {New SupertrendConsumer(chartConsumer2, CType(Me.ParentStrategy.UserSettings, CDSUserInputs).Period, CType(Me.ParentStrategy.UserSettings, CDSUserInputs).Multiplier)}
                RawPayloadDependentConsumers.Add(chartConsumer2)
                _dummyHFSupertrendConsumer = New SupertrendConsumer(chartConsumer2, CType(Me.ParentStrategy.UserSettings, CDSUserInputs).Period, CType(Me.ParentStrategy.UserSettings, CDSUserInputs).Multiplier)
            Else
                Throw New ApplicationException(String.Format("Signal Timeframe is 0 or Nothing, does not adhere to the strategy:{0}", Me.ParentStrategy.ToString))
            End If
        End If
    End Sub
    Public Overrides Function PopulateChartAndIndicatorsAsync(candleCreator As Chart, currentCandle As OHLCPayload) As Task
        Throw New NotImplementedException()
    End Function
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
                    Await ExecuteCommandAsync(ExecuteCommands.PlaceCOMarketMISOrder, Nothing).ConfigureAwait(False)
                End If
                'Place Order block end
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
        Dim userSettings As CDSUserInputs = Me.ParentStrategy.UserSettings
        Dim runningLFCandlePayload As OHLCPayload = GetXMinuteCurrentCandle(userSettings.SignalTimeFrame)
        Dim runningHFCandlePayload As OHLCPayload = GetXMinuteCurrentCandle(userSettings.HigherTimeframe)
        Dim supertrendConsumerLF As SupertrendConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummyLFSupertrendConsumer)
        Dim supertrendConsumerHF As SupertrendConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummyHFSupertrendConsumer)
        Dim currentTick As ITick = Me.TradableInstrument.LastTick
        Dim currentTime As Date = Now()

        Try
            If runningLFCandlePayload IsNot Nothing AndAlso runningLFCandlePayload.PreviousPayload IsNot Nothing AndAlso
                runningHFCandlePayload IsNot Nothing AndAlso runningHFCandlePayload.PreviousPayload IsNot Nothing AndAlso
                Me.TradableInstrument.IsHistoricalCompleted Then
                If Not runningLFCandlePayload.PreviousPayload.ToString = _lastPrevPayloadPlaceOrder Then
                    _lastPrevPayloadPlaceOrder = runningLFCandlePayload.PreviousPayload.ToString
                    logger.Debug("PlaceOrder-> Potential LF Signal Candle is:{0}. Will check rest parameters.", runningLFCandlePayload.PreviousPayload.ToString)
                    logger.Debug("PlaceOrder-> Potential HF Signal Candle is:{0}. Will check rest parameters.", runningHFCandlePayload.PreviousPayload.ToString)
                    logger.Debug("PlaceOrder-> Rest all parameters: Running LF Candle:{0}, Running HF Candle:{1}, LF PayloadGeneratedBy:{2}, HF PayloadGeneratedBy:{3}, IsHistoricalCompleted:{4}, IsFirstTimeInformationCollected:{5}, LF {6}, HF {7}, IsActiveInstrument:{8}, Current Time:{9}, Current Tick:{10}, TradingSymbol:{11}",
                                runningLFCandlePayload.SnapshotDateTime.ToString("dd-MM-yyyy HH:mm:ss"),
                                runningHFCandlePayload.SnapshotDateTime.ToString("dd-MM-yyyy HH:mm:ss"),
                                runningLFCandlePayload.PayloadGeneratedBy.ToString,
                                runningHFCandlePayload.PayloadGeneratedBy.ToString,
                                Me.TradableInstrument.IsHistoricalCompleted,
                                Me.ParentStrategy.IsFirstTimeInformationCollected,
                                supertrendConsumerLF.ConsumerPayloads(runningLFCandlePayload.PreviousPayload.SnapshotDateTime).ToString,
                                supertrendConsumerHF.ConsumerPayloads(runningHFCandlePayload.PreviousPayload.SnapshotDateTime).ToString,
                                IsActiveInstrument(),
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
            runningLFCandlePayload IsNot Nothing AndAlso runningLFCandlePayload.SnapshotDateTime >= userSettings.TradeStartTime AndAlso
            runningLFCandlePayload.PayloadGeneratedBy = OHLCPayload.PayloadSource.CalculatedTick AndAlso
            runningLFCandlePayload.PreviousPayload IsNot Nothing AndAlso
            runningHFCandlePayload IsNot Nothing AndAlso runningHFCandlePayload.PayloadGeneratedBy = OHLCPayload.PayloadSource.CalculatedTick AndAlso
            runningHFCandlePayload.PreviousPayload IsNot Nothing AndAlso
            Not IsActiveInstrument() AndAlso Me.TradableInstrument.IsHistoricalCompleted AndAlso
            supertrendConsumerLF.ConsumerPayloads IsNot Nothing AndAlso supertrendConsumerLF.ConsumerPayloads.Count > 0 AndAlso
            supertrendConsumerLF.ConsumerPayloads.ContainsKey(runningLFCandlePayload.PreviousPayload.SnapshotDateTime) AndAlso
            supertrendConsumerHF.ConsumerPayloads IsNot Nothing AndAlso supertrendConsumerHF.ConsumerPayloads.Count > 0 AndAlso
            supertrendConsumerHF.ConsumerPayloads.ContainsKey(runningHFCandlePayload.PreviousPayload.SnapshotDateTime) Then
            Dim supertrendLF As SupertrendConsumer.SupertrendPayload = supertrendConsumerLF.ConsumerPayloads(runningLFCandlePayload.PreviousPayload.SnapshotDateTime)
            Dim supertrendHF As SupertrendConsumer.SupertrendPayload = supertrendConsumerHF.ConsumerPayloads(runningHFCandlePayload.PreviousPayload.SnapshotDateTime)
            If supertrendLF.SupertrendColor = Color.Green AndAlso supertrendHF.SupertrendColor = Color.Green Then
                Dim quantity As Integer = userSettings.InstrumentsData(Me.TradableInstrument.RawInstrumentName).NumberOfLots * Me.TradableInstrument.LotSize
                Dim slPoint As Decimal = currentTick.LastPrice * userSettings.InstrumentsData(Me.TradableInstrument.RawInstrumentName).MaxStoplossPercentage / 100
                Dim triggerPrice As Decimal = ConvertFloorCeling(currentTick.LastPrice - slPoint, Me.TradableInstrument.TickSize, RoundOfType.Floor)

                If quantity > 0 Then
                    parameters = New PlaceOrderParameters(runningLFCandlePayload) With
                                   {.EntryDirection = IOrder.TypeOfTransaction.Buy,
                                    .Quantity = quantity,
                                    .TriggerPrice = triggerPrice}
                End If
            ElseIf supertrendLF.SupertrendColor = Color.Red AndAlso supertrendHF.SupertrendColor = Color.Red Then
                Dim quantity As Integer = userSettings.InstrumentsData(Me.TradableInstrument.RawInstrumentName).NumberOfLots * Me.TradableInstrument.LotSize
                Dim slPoint As Decimal = currentTick.LastPrice * userSettings.InstrumentsData(Me.TradableInstrument.RawInstrumentName).MaxStoplossPercentage / 100
                Dim triggerPrice As Decimal = ConvertFloorCeling(currentTick.LastPrice + slPoint, Me.TradableInstrument.TickSize, RoundOfType.Celing)

                If quantity > 0 Then
                    parameters = New PlaceOrderParameters(runningLFCandlePayload) With
                                   {.EntryDirection = IOrder.TypeOfTransaction.Sell,
                                    .Quantity = quantity,
                                    .TriggerPrice = triggerPrice}
                End If
            End If
        End If

        'Below portion have to be done in every place order trigger
        If parameters IsNot Nothing Then
            Try
                If forcePrint Then
                    logger.Debug("PlaceOrder-> ************************************************ {0}", Me.TradableInstrument.TradingSymbol)
                    logger.Debug("PlaceOrder Parameters-> {0},{1}", parameters.ToString, Me.TradableInstrument.TradingSymbol)
                    logger.Debug("PlaceOrder-> Rest all parameters: Running LF Candle:{0}, Running HF Candle:{1}, 
                                LF PayloadGeneratedBy:{2}, HF PayloadGeneratedBy:{3}, 
                                IsHistoricalCompleted:{4}, IsFirstTimeInformationCollected:{5}, 
                                LF {6}, 
                                HF {7}, 
                                IsActiveInstrument:{8}, Current Time:{9}, Current Tick:{10}, TradingSymbol:{11}",
                                runningLFCandlePayload.SnapshotDateTime.ToString("dd-MM-yyyy HH:mm:ss"),
                                runningHFCandlePayload.SnapshotDateTime.ToString("dd-MM-yyyy HH:mm:ss"),
                                runningLFCandlePayload.PayloadGeneratedBy.ToString,
                                runningHFCandlePayload.PayloadGeneratedBy.ToString,
                                Me.TradableInstrument.IsHistoricalCompleted,
                                Me.ParentStrategy.IsFirstTimeInformationCollected,
                                supertrendConsumerLF.ConsumerPayloads(runningLFCandlePayload.PreviousPayload.SnapshotDateTime).ToString,
                                supertrendConsumerHF.ConsumerPayloads(runningHFCandlePayload.PreviousPayload.SnapshotDateTime).ToString,
                                IsActiveInstrument(),
                                currentTime.ToString,
                                currentTick.LastPrice,
                                Me.TradableInstrument.TradingSymbol)
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
    Protected Overrides Function IsTriggerReceivedForModifyStoplossOrderAsync(ByVal forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)))
        Throw New NotImplementedException()
    End Function
    Protected Overrides Function IsTriggerReceivedForModifyTargetOrderAsync(forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)))
        Throw New NotImplementedException()
    End Function
    Protected Overrides Async Function IsTriggerReceivedForExitOrderAsync(ByVal forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, String)))
        Dim ret As List(Of Tuple(Of ExecuteCommandAction, IOrder, String)) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        Dim userSettings As CDSUserInputs = Me.ParentStrategy.UserSettings
        Dim supertrendConsumerLF As SupertrendConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummyLFSupertrendConsumer)
        Dim supertrendConsumerHF As SupertrendConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummyHFSupertrendConsumer)
        Dim runningLFCandlePayload As OHLCPayload = GetXMinuteCurrentCandle(userSettings.SignalTimeFrame)
        Dim runningHFCandlePayload As OHLCPayload = GetXMinuteCurrentCandle(userSettings.HigherTimeframe)

        If runningLFCandlePayload IsNot Nothing AndAlso runningLFCandlePayload.PreviousPayload IsNot Nothing AndAlso
            runningHFCandlePayload IsNot Nothing AndAlso runningHFCandlePayload.PreviousPayload IsNot Nothing AndAlso
            Me.TradableInstrument.IsHistoricalCompleted AndAlso
            supertrendConsumerLF.ConsumerPayloads IsNot Nothing AndAlso supertrendConsumerLF.ConsumerPayloads.Count > 0 AndAlso
            supertrendConsumerLF.ConsumerPayloads.ContainsKey(runningLFCandlePayload.PreviousPayload.SnapshotDateTime) AndAlso
            supertrendConsumerHF.ConsumerPayloads IsNot Nothing AndAlso supertrendConsumerHF.ConsumerPayloads.Count > 0 AndAlso
            supertrendConsumerHF.ConsumerPayloads.ContainsKey(runningHFCandlePayload.PreviousPayload.SnapshotDateTime) Then
            Dim allActiveOrders As List(Of IOrder) = GetAllActiveOrders(IOrder.TypeOfTransaction.None)
            If allActiveOrders IsNot Nothing AndAlso allActiveOrders.Count > 0 Then
                Dim slOrders As List(Of IOrder) = allActiveOrders.FindAll(Function(x)
                                                                              Return x.ParentOrderIdentifier IsNot Nothing AndAlso
                                                                              (x.Status = IOrder.TypeOfStatus.TriggerPending OrElse
                                                                              x.Status = IOrder.TypeOfStatus.Open)
                                                                          End Function)
                If slOrders IsNot Nothing AndAlso slOrders.Count > 0 Then
                    For Each runningSLOrder In slOrders
                        If runningSLOrder.Status = IOrder.TypeOfStatus.TriggerPending OrElse
                        runningSLOrder.Status = IOrder.TypeOfStatus.Open Then
                            Dim bussinessOrder As IBusinessOrder = GetParentFromChildOrder(runningSLOrder)
                            Dim exitTrade As Boolean = False
                            Dim supertrendLF As SupertrendConsumer.SupertrendPayload = supertrendConsumerLF.ConsumerPayloads(runningLFCandlePayload.PreviousPayload.SnapshotDateTime)
                            Dim supertrendHF As SupertrendConsumer.SupertrendPayload = supertrendConsumerLF.ConsumerPayloads(runningLFCandlePayload.PreviousPayload.SnapshotDateTime)
                            If (supertrendLF.SupertrendColor = Color.Green OrElse supertrendHF.SupertrendColor = Color.Green) AndAlso
                                bussinessOrder.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Sell Then
                                exitTrade = True
                            ElseIf (supertrendLF.SupertrendColor = Color.Red OrElse supertrendHF.SupertrendColor = Color.Red) AndAlso
                                bussinessOrder.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Buy Then
                                exitTrade = True
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
                                ret.Add(New Tuple(Of ExecuteCommandAction, IOrder, String)(ExecuteCommandAction.Take, runningSLOrder, "Opposite direction signal"))
                            End If
                        End If
                    Next
                End If
            End If
        End If
        If forcePrint AndAlso ret IsNot Nothing AndAlso ret.Count > 0 Then
            Try
                For Each runningOrder In ret
                    logger.Debug("***** Exit Order ***** Order ID:{0}, Reason:{1}, LF {2}, HF {3}, {4}",
                                 runningOrder.Item2.OrderIdentifier,
                                 runningOrder.Item3,
                                 supertrendConsumerLF.ConsumerPayloads(runningLFCandlePayload.PreviousPayload.SnapshotDateTime).ToString,
                                 supertrendConsumerHF.ConsumerPayloads(runningHFCandlePayload.PreviousPayload.SnapshotDateTime).ToString,
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

            Await ExecuteCommandAsync(ExecuteCommands.ForceCancelCOOrder, cancellableOrder).ConfigureAwait(False)
        End If
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