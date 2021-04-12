Imports NLog
Imports Algo2TradeCore
Imports System.Threading
Imports Algo2TradeCore.Adapter
Imports Algo2TradeCore.Entities
Imports Algo2TradeCore.Strategies
Imports Algo2TradeCore.Entities.Indicators
Imports Algo2TradeCore.ChartHandler.ChartStyle
Imports System.IO

Public Class NFOStrategyInstrument
    Inherits StrategyInstrument
    Implements IDisposable

#Region "Logging and Status Progress"
    Public Shared Shadows logger As Logger = LogManager.GetCurrentClassLogger
#End Region

    'Public ForceExitForContractRolloverDone As Boolean
    Public ForceExitForContractRollover As Boolean
    Public ForceEntryForContractRolloverDone As Boolean
    Public ForceEntryForContractRollover As Boolean

    Private _lastPrevPayloadPlaceOrder As String = ""
    Private ReadOnly _dummySupertrendConsumer As SupertrendConsumer

    Public Sub New(ByVal associatedInstrument As IInstrument,
                   ByVal associatedParentStrategy As Strategy,
                   ByVal isPairInstrumnet As Boolean,
                   ByVal parentInstrument As Boolean,
                   ByVal canceller As CancellationTokenSource)
        MyBase.New(associatedInstrument, associatedParentStrategy, isPairInstrumnet, canceller)
        Select Case Me.ParentStrategy.ParentController.BrokerSource
            Case APISource.Zerodha
                _APIAdapter = New ZerodhaAdapter(ParentStrategy.ParentController, Me.TradableInstrument, _cts)
            Case APISource.AliceBlue
                _APIAdapter = New AliceAdapter(ParentStrategy.ParentController, Me.TradableInstrument, _cts)
            Case APISource.Upstox
                Throw New NotImplementedException
            Case Else
                Throw New NotImplementedException
        End Select
        AddHandler _APIAdapter.Heartbeat, AddressOf OnHeartbeat
        AddHandler _APIAdapter.WaitingFor, AddressOf OnWaitingFor
        AddHandler _APIAdapter.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
        AddHandler _APIAdapter.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete

        RawPayloadDependentConsumers = New List(Of IPayloadConsumer)
        If Me.ParentStrategy.IsStrategyCandleStickBased Then
            Dim userInputs As NFOUserInputs = Me.ParentStrategy.UserSettings
            Dim instrumentData As NFOUserInputs.InstrumentDetails = userInputs.InstrumentsData(Me.TradableInstrument.RawInstrumentName.ToUpper)
            Dim chartConsumer As PayloadToChartConsumer = New PayloadToChartConsumer(instrumentData.Timeframe)
            chartConsumer.OnwardLevelConsumers = New List(Of IPayloadConsumer)
            chartConsumer.OnwardLevelConsumers.Add(New SupertrendConsumer(chartConsumer, instrumentData.SupertrendPeriod, instrumentData.SupertrendMultiplier))
            RawPayloadDependentConsumers.Add(chartConsumer)
            _dummySupertrendConsumer = New SupertrendConsumer(chartConsumer, instrumentData.SupertrendPeriod, instrumentData.SupertrendMultiplier)
        End If
    End Sub

    Public Overrides Async Function PopulateChartAndIndicatorsAsync(candleCreator As Chart, currentCandle As OHLCPayload) As Task
        If RawPayloadDependentConsumers IsNot Nothing AndAlso RawPayloadDependentConsumers.Count > 0 Then
            For Each runningRawPayloadConsumer In RawPayloadDependentConsumers
                If runningRawPayloadConsumer.TypeOfConsumer = IPayloadConsumer.ConsumerType.Chart Then
                    Dim currentXMinute As Date = candleCreator.ConvertTimeframe(CType(runningRawPayloadConsumer, PayloadToChartConsumer).Timeframe, currentCandle, runningRawPayloadConsumer)
                    If candleCreator.IndicatorCreator Is Nothing Then candleCreator.IndicatorCreator = New ChartHandler.Indicator.IndicatorManeger(Me.ParentStrategy.ParentController, candleCreator, _cts)
                    If currentXMinute <> Date.MaxValue Then
                        If runningRawPayloadConsumer.OnwardLevelConsumers IsNot Nothing AndAlso runningRawPayloadConsumer.OnwardLevelConsumers.Count > 0 Then
                            For Each consumer In runningRawPayloadConsumer.OnwardLevelConsumers
                                candleCreator.IndicatorCreator.CalculateSupertrend(currentXMinute, consumer)
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
                If Me._RMSException IsNot Nothing AndAlso _RMSException.ExceptionType = Algo2TradeCore.Exceptions.AdapterBusinessException.TypeOfException.RMSError Then
                    OnHeartbeat(String.Format("{0}:Will not take no more action in this instrument as RMS Error occured. Error-{1}", Me.TradableInstrument.TradingSymbol, _RMSException.Message))
                    Throw Me._RMSException
                End If
                _cts.Token.ThrowIfCancellationRequested()

                'Place Order block start
                Dim placeOrderTriggers As List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)) = Await IsTriggerReceivedForPlaceOrderAsync(False).ConfigureAwait(False)
                If placeOrderTriggers IsNot Nothing AndAlso placeOrderTriggers.Count > 0 AndAlso
                    placeOrderTriggers.FirstOrDefault.Item1 = ExecuteCommandAction.Take Then
                    If placeOrderTriggers.FirstOrDefault.Item2.Quantity <> 0 Then
                        Dim orderResponse = Await ExecuteCommandAsync(ExecuteCommands.PlaceRegularMarketCNCOrder, Nothing).ConfigureAwait(False)
                        If orderResponse IsNot Nothing AndAlso orderResponse.Count > 0 Then
                            Dim placeOrderResponse = CType(orderResponse, Concurrent.ConcurrentBag(Of Object)).FirstOrDefault
                            If placeOrderResponse.ContainsKey("data") AndAlso
                                placeOrderResponse("data").ContainsKey("order_id") Then
                                If ForceExitForContractRollover Then ForceExitForContractRollover = False
                                If ForceEntryForContractRollover Then ForceEntryForContractRollover = False
                            End If
                        End If
                    Else
                        If ForceExitForContractRollover Then
                            ForceExitForContractRollover = False
                            OnHeartbeat(String.Format("No position available for contract rollover force exit: {0}", Me.TradableInstrument.TradingSymbol))
                        End If
                        If ForceEntryForContractRollover Then
                            ForceEntryForContractRollover = False
                            ForceEntryForContractRolloverDone = True
                            OnHeartbeat(String.Format("No position available for contract rollover force entry: {0}", Me.TradableInstrument.TradingSymbol))
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
        Finally
            Me.TradableInstrument.FetchHistorical = False
            OnHeartbeat("Strategy Instrument Stopped")
            _strategyInstrumentRunning = False
        End Try
    End Function

    Protected Overrides Async Function IsTriggerReceivedForPlaceOrderAsync(forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)))
        Dim ret As List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
        Dim runningCandlePayload As OHLCPayload = GetXMinuteCurrentCandle(userSettings.InstrumentsData(Me.TradableInstrument.RawInstrumentName.ToUpper).Timeframe)
        Dim stConsumer As SupertrendConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummySupertrendConsumer)
        Dim currentTick As ITick = Me.TradableInstrument.LastTick
        Dim currentTime As Date = Now()
        Dim log As Boolean = False

        Try
            If runningCandlePayload IsNot Nothing AndAlso runningCandlePayload.PreviousPayload IsNot Nothing AndAlso
                runningCandlePayload.PreviousPayload.PreviousPayload IsNot Nothing AndAlso Me.TradableInstrument.IsHistoricalCompleted Then
                If Not runningCandlePayload.PreviousPayload.ToString = _lastPrevPayloadPlaceOrder OrElse forcePrint Then
                    _lastPrevPayloadPlaceOrder = runningCandlePayload.PreviousPayload.ToString
                    log = True
                    logger.Debug("PlaceOrder-> Potential Signal Candle is:{0}. Will check rest parameters.", runningCandlePayload.PreviousPayload.ToString)
                    logger.Debug("PlaceOrder-> Rest all parameters: RunningCandleTime:{0}, PayloadGeneratedBy:{1}, IsHistoricalCompleted:{2}, IsFirstTimeInformationCollected:{3}, Pre Supertrend Color:{4}, Supertrend Color:{5}, Exchange Start Time:{6}, Exchange End Time:{7}, Is My Another Contract Available:{8}, Force Exit For Contract Rollover:{9}, Force Entry For Contract Rollover:{10}, Traded Quantity:{11}, Current Time:{12}, TradingSymbol:{13}",
                                runningCandlePayload.SnapshotDateTime.ToString,
                                runningCandlePayload.PayloadGeneratedBy.ToString,
                                Me.TradableInstrument.IsHistoricalCompleted,
                                Me.ParentStrategy.IsFirstTimeInformationCollected,
                                CType(stConsumer.ConsumerPayloads(runningCandlePayload.PreviousPayload.PreviousPayload.SnapshotDateTime), SupertrendConsumer.SupertrendPayload).SupertrendColor.Name,
                                CType(stConsumer.ConsumerPayloads(runningCandlePayload.PreviousPayload.SnapshotDateTime), SupertrendConsumer.SupertrendPayload).SupertrendColor.Name,
                                Me.TradableInstrument.ExchangeDetails.ExchangeStartTime.ToString,
                                Me.TradableInstrument.ExchangeDetails.ExchangeEndTime.ToString,
                                IsMyAnotherContractAvailable(),
                                Me.ForceExitForContractRollover,
                                Me.ForceEntryForContractRollover,
                                GetQuantityToTrade(),
                                currentTime.ToString("dd-MMM-yyyy HH:mm:ss"),
                                Me.TradableInstrument.TradingSymbol)
                End If
            End If
        Catch ex As Exception
            logger.Warn(ex)
        End Try

        Dim parameters As PlaceOrderParameters = Nothing
        If (Me.ForceExitForContractRollover OrElse Me.ForceEntryForContractRollover) AndAlso
            currentTime >= Me.TradableInstrument.ExchangeDetails.ExchangeStartTime AndAlso currentTime <= Me.TradableInstrument.ExchangeDetails.ExchangeEndTime AndAlso
            Me.TradableInstrument.IsHistoricalCompleted AndAlso runningCandlePayload IsNot Nothing AndAlso runningCandlePayload.PreviousPayload IsNot Nothing Then
            If log Then
                OnHeartbeat(String.Format("Force Exit For Contract Rollover: {0}, Force Entry For Contract Rollover: {1}", ForceExitForContractRollover, ForceEntryForContractRollover))
            End If
            Dim quantity As Integer = GetQuantityToTrade()

            If ForceExitForContractRollover Then
                userSettings.InstrumentsData(Me.TradableInstrument.RawInstrumentName.ToUpper).ModifiedNumberOfLots = quantity / Me.TradableInstrument.LotSize
            End If

            If quantity > 0 Then
                parameters = New PlaceOrderParameters(runningCandlePayload.PreviousPayload) With
                               {.EntryDirection = IOrder.TypeOfTransaction.Sell,
                                .Quantity = Math.Abs(quantity),
                                .Supporting = New List(Of Object) From {"Exit"}}
            Else
                parameters = New PlaceOrderParameters(runningCandlePayload.PreviousPayload) With
                              {.EntryDirection = IOrder.TypeOfTransaction.Buy,
                               .Quantity = Math.Abs(quantity),
                               .Supporting = New List(Of Object) From {"Exit"}}
            End If

            If Me.ForceEntryForContractRollover Then
                quantity = userSettings.InstrumentsData(Me.TradableInstrument.RawInstrumentName.ToUpper).ModifiedNumberOfLots * Me.TradableInstrument.LotSize
                If quantity > 0 Then
                    parameters = New PlaceOrderParameters(runningCandlePayload.PreviousPayload) With
                                   {.EntryDirection = IOrder.TypeOfTransaction.Buy,
                                    .Quantity = Math.Abs(quantity)}
                Else
                    parameters = New PlaceOrderParameters(runningCandlePayload.PreviousPayload) With
                                  {.EntryDirection = IOrder.TypeOfTransaction.Sell,
                                   .Quantity = Math.Abs(quantity)}
                End If
            End If
        ElseIf currentTime >= userSettings.TradeStartTime AndAlso currentTime >= Me.TradableInstrument.ExchangeDetails.ExchangeStartTime AndAlso
            currentTime <= Me.TradableInstrument.ExchangeDetails.ExchangeEndTime AndAlso runningCandlePayload IsNot Nothing AndAlso runningCandlePayload.PreviousPayload IsNot Nothing AndAlso
            runningCandlePayload.PreviousPayload.PreviousPayload IsNot Nothing AndAlso runningCandlePayload.PayloadGeneratedBy = OHLCPayload.PayloadSource.CalculatedTick AndAlso
            Me.TradableInstrument.IsHistoricalCompleted AndAlso Me.ParentStrategy.IsFirstTimeInformationCollected AndAlso stConsumer.ConsumerPayloads IsNot Nothing AndAlso
            stConsumer.ConsumerPayloads.Count > 0 AndAlso stConsumer.ConsumerPayloads.ContainsKey(runningCandlePayload.PreviousPayload.SnapshotDateTime) AndAlso
            stConsumer.ConsumerPayloads.ContainsKey(runningCandlePayload.PreviousPayload.PreviousPayload.SnapshotDateTime) Then
            If (Me.TradableInstrument.Expiry.Value.Date.AddDays(userSettings.ExpireDaysBefore * -1) <> Now.Date AndAlso Not IsMyAnotherContractAvailable.Item1) OrElse
                (Me.TradableInstrument.Expiry.Value.Date.AddDays(userSettings.ExpireDaysBefore * -1) <> Now.Date AndAlso IsMyAnotherContractAvailable.Item1 AndAlso currentTime >= Me.TradableInstrument.ExchangeDetails.ContractRolloverTime AndAlso Me.ForceEntryForContractRolloverDone) OrElse
                (Me.TradableInstrument.Expiry.Value.Date.AddDays(userSettings.ExpireDaysBefore * -1) = Now.Date AndAlso IsMyAnotherContractAvailable.Item1 AndAlso currentTime < Me.TradableInstrument.ExchangeDetails.ContractRolloverTime) Then
                Dim preSupertrendColor As Color = CType(stConsumer.ConsumerPayloads(runningCandlePayload.PreviousPayload.PreviousPayload.SnapshotDateTime), SupertrendConsumer.SupertrendPayload).SupertrendColor
                Dim supertrendColor As Color = CType(stConsumer.ConsumerPayloads(runningCandlePayload.PreviousPayload.SnapshotDateTime), SupertrendConsumer.SupertrendPayload).SupertrendColor
                If log Then OnHeartbeat(String.Format("Supertrend Color:{0}, Previous Supertrend Color:{1}, Traded Quantity:{2}", supertrendColor.Name, preSupertrendColor.Name, GetQuantityToTrade))
                Dim quantity As Integer = GetQuantityToTrade()
                If quantity = 0 Then
                    If supertrendColor <> preSupertrendColor Then
                        quantity = Me.TradableInstrument.LotSize * userSettings.InstrumentsData(Me.TradableInstrument.RawInstrumentName.ToUpper).NumberOfLots
                        If supertrendColor = Color.Green Then
                            If log Then OnHeartbeat(String.Format("Supertrend Color:{0}, Previous Supertrend Color:{1}, Traded Quantity:{2}. So Buy trade will be taken", supertrendColor.Name, preSupertrendColor.Name, GetQuantityToTrade))
                            parameters = New PlaceOrderParameters(runningCandlePayload.PreviousPayload) With
                                           {.EntryDirection = IOrder.TypeOfTransaction.Buy,
                                            .Quantity = Math.Abs(quantity)}
                        ElseIf supertrendColor = Color.Red Then
                            If log Then OnHeartbeat(String.Format("Supertrend Color:{0}, Previous Supertrend Color:{1}, Traded Quantity:{2}. So Sell trade will be taken", supertrendColor.Name, preSupertrendColor.Name, GetQuantityToTrade))
                            parameters = New PlaceOrderParameters(runningCandlePayload.PreviousPayload) With
                                           {.EntryDirection = IOrder.TypeOfTransaction.Sell,
                                            .Quantity = Math.Abs(quantity)}
                        End If
                    End If
                Else
                    If supertrendColor = Color.Green AndAlso quantity < 0 Then
                        If log Then OnHeartbeat(String.Format("Supertrend Color:{0}, Previous Supertrend Color:{1}, Traded Quantity:{2}. So Buy trade will be taken", supertrendColor.Name, preSupertrendColor.Name, GetQuantityToTrade))
                        parameters = New PlaceOrderParameters(runningCandlePayload.PreviousPayload) With
                                       {.EntryDirection = IOrder.TypeOfTransaction.Buy,
                                        .Quantity = Math.Abs(quantity) * 2}
                    ElseIf supertrendColor = Color.Red AndAlso quantity > 0 Then
                        If log Then OnHeartbeat(String.Format("Supertrend Color:{0}, Previous Supertrend Color:{1}, Traded Quantity:{2}. So Sell trade will be taken", supertrendColor.Name, preSupertrendColor.Name, GetQuantityToTrade))
                        parameters = New PlaceOrderParameters(runningCandlePayload.PreviousPayload) With
                                       {.EntryDirection = IOrder.TypeOfTransaction.Sell,
                                        .Quantity = Math.Abs(quantity) * 2}
                    End If
                End If
            End If
        End If

        'Below portion have to be done in every place order trigger
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
                                If lastPlacedActivity.CancelActivity.RequestStatus = ActivityDashboard.SignalStatusType.Activated Then
                                    If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                                    ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.Take, parameters, parameters.ToString))
                                Else
                                    If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                                    ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.DonotTake, parameters, parameters.ToString))
                                    Try
                                        logger.Debug("Will not take this trade as similar previous trade detected. Current Trade->{0}, Last Activity Direction:{1}, Last Activity Status:{2}, Trading Symbol:{3}",
                                             parameters.ToString, lastPlacedActivity.SignalDirection, lastPlacedActivity.EntryActivity.RequestStatus, Me.TradableInstrument.TradingSymbol)
                                    Catch ex As Exception
                                        logger.Warn(ex.ToString)
                                    End Try
                                End If
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
        End If
        Return ret
    End Function

    Private Function GetQuantityToTrade() As Integer
        Dim ret As Integer = 0
        If PositionDetails IsNot Nothing Then
            ret = Me.PositionDetails.Quantity
        End If
        Return ret
    End Function

    Private Function IsMyAnotherContractAvailable() As Tuple(Of Boolean, NFOStrategyInstrument)
        Dim ret As Tuple(Of Boolean, NFOStrategyInstrument) = New Tuple(Of Boolean, NFOStrategyInstrument)(False, Nothing)
        For Each runningStrategyInstrument As NFOStrategyInstrument In Me.ParentStrategy.TradableStrategyInstruments
            If runningStrategyInstrument.TradableInstrument.InstrumentIdentifier <> Me.TradableInstrument.InstrumentIdentifier AndAlso
                runningStrategyInstrument.TradableInstrument.RawInstrumentName.ToUpper = Me.TradableInstrument.RawInstrumentName.ToUpper Then
                ret = New Tuple(Of Boolean, NFOStrategyInstrument)(True, runningStrategyInstrument)
                Exit For
            End If
        Next
        Return ret
    End Function

    Public Async Function ContractRolloverAsync() As Task
        Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
        If Me.TradableInstrument.Expiry.Value.Date.AddDays(userSettings.ExpireDaysBefore * -1) = Now.Date AndAlso IsMyAnotherContractAvailable().Item1 Then
            Try
                While True
                    If Me.ParentStrategy.ParentController.OrphanException IsNot Nothing Then
                        Throw Me.ParentStrategy.ParentController.OrphanException
                    End If
                    _cts.Token.ThrowIfCancellationRequested()

                    If Now >= Me.TradableInstrument.ExchangeDetails.ContractRolloverTime Then
                        Me.ForceExitForContractRollover = True
                        While Me.ForceExitForContractRollover
                            Await Task.Delay(1000, _cts.Token).ConfigureAwait(False)
                        End While
                        'Me.ForceExitForContractRolloverDone = True

                        IsMyAnotherContractAvailable.Item2.ForceEntryForContractRollover = True
                        IsMyAnotherContractAvailable.Item2.ForceEntryForContractRolloverDone = False

                        Exit While
                    End If

                    Await Task.Delay(60000, _cts.Token).ConfigureAwait(False)
                End While
            Catch ex As Exception
                logger.Error("Strategy Instrument:{0}, error:{1}", Me.ToString, ex.ToString)
                Throw ex
            End Try
        End If
    End Function

#Region "Not required functions"
    Public Overrides Function MonitorAsync(command As ExecuteCommands, data As Object) As Task
        Throw New NotImplementedException
    End Function

    Protected Overrides Function IsTriggerReceivedForPlaceOrderAsync(forcePrint As Boolean, data As Object) As Task(Of List(Of Tuple(Of ExecuteCommandAction, StrategyInstrument, PlaceOrderParameters, String)))
        Throw New NotImplementedException()
    End Function

    Protected Overrides Function IsTriggerReceivedForModifyStoplossOrderAsync(forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)))
        Throw New NotImplementedException()
    End Function

    Protected Overrides Function IsTriggerReceivedForModifyTargetOrderAsync(forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)))
        Throw New NotImplementedException()
    End Function

    Protected Overrides Function IsTriggerReceivedForExitOrderAsync(forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, String)))
        Throw New NotImplementedException()
    End Function

    Protected Overrides Function IsTriggerReceivedForExitOrderAsync(forcePrint As Boolean, data As Object) As Task(Of List(Of Tuple(Of ExecuteCommandAction, StrategyInstrument, IOrder, String)))
        Throw New NotImplementedException()
    End Function

    Protected Overrides Function ForceExitSpecificTradeAsync(order As IOrder, reason As String) As Task
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