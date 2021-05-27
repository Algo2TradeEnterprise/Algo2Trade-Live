Imports NLog
Imports Algo2TradeCore
Imports System.Threading
Imports Algo2TradeCore.Adapter
Imports Algo2TradeCore.Entities
Imports Algo2TradeCore.Strategies
Imports Algo2TradeCore.Entities.Indicators
Imports Algo2TradeCore.ChartHandler.ChartStyle
Imports System.IO
Imports Utilities.Numbers

Public Class SpreadStrategyInstrument
    Inherits StrategyInstrument
    Implements IDisposable

#Region "Logging and Status Progress"
    Public Shared Shadows logger As Logger = LogManager.GetCurrentClassLogger
#End Region

    Public DependentOptionStrategyInstruments As IEnumerable(Of SpreadStrategyInstrument)

    Private _direction As IOrder.TypeOfTransaction = IOrder.TypeOfTransaction.None

    Private _lastPrevPayloadPlaceOrder As String = ""
    Private ReadOnly _dummySupertrendConsumer As SupertrendConsumer

    Public Sub New(ByVal associatedInstrument As IInstrument,
                   ByVal associatedParentStrategy As Strategy,
                   ByVal isPairInstrumnet As Boolean,
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
        If Me.ParentStrategy.IsStrategyCandleStickBased AndAlso Me.TradableInstrument.InstrumentType = IInstrument.TypeOfInstrument.Cash Then
            Dim rawInstrumentName As String = Me.TradableInstrument.TradingSymbol.ToUpper
            If Me.TradableInstrument.TradingSymbol.ToUpper = "NIFTY 50" Then
                rawInstrumentName = "NIFTY"
            ElseIf Me.TradableInstrument.RawInstrumentName.ToUpper = "NIFTY BANK" Then
                rawInstrumentName = "BANKNIFTY"
            End If

            Dim userInputs As SpreadUserInputs = Me.ParentStrategy.UserSettings
            Dim instrumentData As SpreadUserInputs.InstrumentDetails = userInputs.InstrumentsData(rawInstrumentName)
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

    Private Async Function SubscribeOptionInstrumentsFromPosition() As Task
        Dim availablePositions As Concurrent.ConcurrentBag(Of IPosition) = Await Me.ParentStrategy.ParentController.GetPositionDetailsAsync().ConfigureAwait(False)
        If availablePositions IsNot Nothing AndAlso availablePositions.Count > 0 Then
            Dim allInstruments As IEnumerable(Of IInstrument) = CType(Me.ParentStrategy, SpreadStrategy).DependentInstruments
            If allInstruments IsNot Nothing AndAlso allInstruments.Count > 0 Then
                Dim tradableInstruments As List(Of IInstrument) = Nothing
                For Each runningPosition In availablePositions
                    If runningPosition.Product.ToUpper = "NRML" OrElse runningPosition.Product.ToUpper = "CNC" Then
                        Dim instrument As IInstrument = allInstruments.ToList.Find(Function(x)
                                                                                       Return x.InstrumentIdentifier = runningPosition.InstrumentIdentifier
                                                                                   End Function)
                        If instrument IsNot Nothing Then
                            Dim rawInstrumentName As String = Me.TradableInstrument.TradingSymbol.ToUpper
                            If Me.TradableInstrument.TradingSymbol.ToUpper = "NIFTY 50" Then
                                rawInstrumentName = "NIFTY"
                            ElseIf Me.TradableInstrument.RawInstrumentName.ToUpper = "NIFTY BANK" Then
                                rawInstrumentName = "BANKNIFTY"
                            End If

                            If instrument.RawInstrumentName = rawInstrumentName Then
                                If tradableInstruments Is Nothing Then tradableInstruments = New List(Of IInstrument)
                                tradableInstruments.Add(instrument)
                            End If
                        End If
                    End If
                Next
                If tradableInstruments IsNot Nothing AndAlso tradableInstruments.Count > 0 Then
                    Await CreateStrategyInstrumentAndPopulate(tradableInstruments).ConfigureAwait(False)
                    Await Task.Delay(5000).ConfigureAwait(False)
                End If
            End If
        End If
    End Function

    Private Async Function CreateStrategyInstrumentAndPopulate(ByVal instrumentList As List(Of IInstrument)) As Task
        Await CType(Me.ParentStrategy, SpreadStrategy).CreateDependentTradableStrategyInstrumentsAsync(instrumentList).ConfigureAwait(False)
        Dim tradableStrategyInstruments As List(Of SpreadStrategyInstrument) = New List(Of SpreadStrategyInstrument)
        For Each runningInstrument In instrumentList
            Dim subscribedStrategyInstrument As StrategyInstrument =
                Me.ParentStrategy.TradableStrategyInstruments.ToList.Find(Function(x)
                                                                              Return x.TradableInstrument.InstrumentIdentifier = runningInstrument.InstrumentIdentifier
                                                                          End Function)
            tradableStrategyInstruments.Add(subscribedStrategyInstrument)
        Next
        If DependentOptionStrategyInstruments IsNot Nothing AndAlso DependentOptionStrategyInstruments.Count > 0 Then
            DependentOptionStrategyInstruments = DependentOptionStrategyInstruments.Concat(tradableStrategyInstruments)
        Else
            DependentOptionStrategyInstruments = tradableStrategyInstruments
        End If
        Await Task.Delay(1000).ConfigureAwait(False)
    End Function

    Public Overrides Async Function MonitorAsync() As Task
        Try
            _strategyInstrumentRunning = True

            Dim rawInstrumentName As String = Me.TradableInstrument.TradingSymbol.ToUpper
            If Me.TradableInstrument.TradingSymbol.ToUpper = "NIFTY 50" Then
                rawInstrumentName = "NIFTY"
            ElseIf Me.TradableInstrument.RawInstrumentName.ToUpper = "NIFTY BANK" Then
                rawInstrumentName = "BANKNIFTY"
            End If

            Dim userInputs As SpreadUserInputs = Me.ParentStrategy.UserSettings
            Dim instrumentData As SpreadUserInputs.InstrumentDetails = userInputs.InstrumentsData(rawInstrumentName)
            Dim stConsumer As SupertrendConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummySupertrendConsumer)

            Await SubscribeOptionInstrumentsFromPosition().ConfigureAwait(False)
            While True
                If Me.ParentStrategy.ParentController.OrphanException IsNot Nothing Then
                    Throw Me.ParentStrategy.ParentController.OrphanException
                End If
                If Me._RMSException IsNot Nothing AndAlso _RMSException.ExceptionType = Algo2TradeCore.Exceptions.AdapterBusinessException.TypeOfException.RMSError Then
                    OnHeartbeat(String.Format("{0}:Will not take no more action in this instrument as RMS Error occured. Error-{1}", Me.TradableInstrument.TradingSymbol, _RMSException.Message))
                    Throw Me._RMSException
                End If
                _cts.Token.ThrowIfCancellationRequested()

                If Me.TradableInstrument.IsHistoricalCompleted Then
                    Dim runningCandlePayload As OHLCPayload = GetXMinuteCurrentCandle(instrumentData.Timeframe)
                    If runningCandlePayload IsNot Nothing AndAlso runningCandlePayload.PreviousPayload IsNot Nothing Then
                        Dim log As Boolean = False
                        Dim activeInstruments As List(Of SpreadStrategyInstrument) = GetActiveInstruments()
                        Dim buyActive As Boolean = False
                        Dim sellActive As Boolean = False
                        If activeInstruments IsNot Nothing AndAlso activeInstruments.Count > 0 Then
                            For Each runningInstrument In activeInstruments
                                If runningInstrument.TradableInstrument.RawInstrumentType.ToUpper = "PE" Then
                                    buyActive = True
                                ElseIf runningInstrument.TradableInstrument.RawInstrumentType.ToUpper = "CE" Then
                                    sellActive = True
                                End If
                            Next
                        End If
                        Try
                            If runningCandlePayload IsNot Nothing AndAlso runningCandlePayload.PreviousPayload IsNot Nothing AndAlso Me.TradableInstrument.IsHistoricalCompleted Then
                                If Not runningCandlePayload.PreviousPayload.ToString = _lastPrevPayloadPlaceOrder Then
                                    _lastPrevPayloadPlaceOrder = runningCandlePayload.PreviousPayload.ToString
                                    log = True
                                    logger.Debug("PlaceOrder-> Potential Signal Candle is:{0}. Will check rest parameters.", runningCandlePayload.PreviousPayload.ToString)
                                    logger.Debug("PlaceOrder-> Rest all parameters: RunningCandleTime:{0}, PayloadGeneratedBy:{1}, IsHistoricalCompleted:{2}, IsFirstTimeInformationCollected:{3}, Supertrend Color:{4}, Active Instrument Count:{5}, Buy Active:{6}, Sell Active:{7}, Exchange Start Time:{8}, Exchange End Time:{9}, Current Time:{10}, TradingSymbol:{11}",
                                                runningCandlePayload.SnapshotDateTime.ToString,
                                                runningCandlePayload.PayloadGeneratedBy.ToString,
                                                Me.TradableInstrument.IsHistoricalCompleted,
                                                Me.ParentStrategy.IsFirstTimeInformationCollected,
                                                CType(stConsumer.ConsumerPayloads(runningCandlePayload.PreviousPayload.SnapshotDateTime), SupertrendConsumer.SupertrendPayload).SupertrendColor.Name,
                                                If(activeInstruments Is Nothing, 0, activeInstruments.Count),
                                                buyActive,
                                                sellActive,
                                                Me.TradableInstrument.ExchangeDetails.ExchangeStartTime.ToString,
                                                Me.TradableInstrument.ExchangeDetails.ExchangeEndTime.ToString,
                                                Now.ToString("dd-MMM-yyyy HH:mm:ss"),
                                                Me.TradableInstrument.TradingSymbol)
                                End If
                            End If
                        Catch ex As Exception
                            logger.Warn(ex)
                        End Try

                        If stConsumer.ConsumerPayloads.ContainsKey(runningCandlePayload.PreviousPayload.SnapshotDateTime) Then
                            Dim stColor As Color = CType(stConsumer.ConsumerPayloads(runningCandlePayload.PreviousPayload.SnapshotDateTime), SupertrendConsumer.SupertrendPayload).SupertrendColor
                            If log Then OnHeartbeat(String.Format("Supertrend Color:{0}, Buy Active:{1}, Sell Active:{2}", stColor.Name, buyActive, sellActive))
                            If stColor = Color.Green Then
                                If sellActive Then
                                    Dim exitInstrument1 As SpreadStrategyInstrument = Nothing
                                    Dim exitInstrument2 As SpreadStrategyInstrument = Nothing
                                    For Each runningInstrument In activeInstruments
                                        If runningInstrument.TradableInstrument.RawInstrumentType.ToUpper = "CE" Then
                                            If runningInstrument.GetQuantityToTrade() < 0 Then
                                                exitInstrument1 = runningInstrument
                                            ElseIf runningInstrument.GetQuantityToTrade() > 0 Then
                                                exitInstrument2 = runningInstrument
                                            End If
                                        End If
                                    Next
                                    If exitInstrument1 IsNot Nothing Then
                                        Await exitInstrument1.MonitorAsync(command:=ExecuteCommands.PlaceRegularMarketCNCOrder, "BUY").ConfigureAwait(False)
                                    End If
                                    If exitInstrument2 IsNot Nothing Then
                                        Await exitInstrument2.MonitorAsync(command:=ExecuteCommands.PlaceRegularMarketCNCOrder, "SELL").ConfigureAwait(False)
                                    End If
                                End If
                                If Not buyActive Then
                                    Dim allInstruments As IEnumerable(Of IInstrument) = CType(Me.ParentStrategy, SpreadStrategy).DependentInstruments
                                    Dim expiryDate As Date = GetExpiryDate()
                                    Dim allPEStrikes As Dictionary(Of Decimal, IInstrument) = Nothing
                                    For Each runningInstrument In allInstruments
                                        If runningInstrument.RawInstrumentName = instrumentData.InstrumentName AndAlso
                                            runningInstrument.RawInstrumentType.ToUpper = "PE" AndAlso
                                            runningInstrument.Expiry.Value.Date = expiryDate.Date Then
                                            If allPEStrikes Is Nothing Then allPEStrikes = New Dictionary(Of Decimal, IInstrument)
                                            If Not allPEStrikes.ContainsKey(runningInstrument.Strike) Then allPEStrikes.Add(runningInstrument.Strike, runningInstrument)
                                        End If
                                    Next

                                    Dim atmStrike As Decimal = GetATMStrike(runningCandlePayload.PreviousPayload.ClosePrice.Value - instrumentData.EntryGap, allPEStrikes.Keys.ToList)
                                    Dim otmStrike As Decimal = GetATMStrike(atmStrike + instrumentData.Distance, allPEStrikes.Keys.ToList)

                                    Await CreateStrategyInstrumentAndPopulate(New List(Of IInstrument) From {allPEStrikes(atmStrike), allPEStrikes(otmStrike)}).ConfigureAwait(False)

                                    Dim otmStrategyInstrument As SpreadStrategyInstrument = Nothing
                                    Dim atmStrategyInstrument As SpreadStrategyInstrument = Nothing
                                    If Me.DependentOptionStrategyInstruments IsNot Nothing AndAlso Me.DependentOptionStrategyInstruments.Count > 0 Then
                                        otmStrategyInstrument = DependentOptionStrategyInstruments.ToList.Find(Function(x)
                                                                                                                   Return x.TradableInstrument.TradingSymbol = allPEStrikes(otmStrike).TradingSymbol
                                                                                                               End Function)
                                        atmStrategyInstrument = DependentOptionStrategyInstruments.ToList.Find(Function(x)
                                                                                                                   Return x.TradableInstrument.TradingSymbol = allPEStrikes(atmStrike).TradingSymbol
                                                                                                               End Function)
                                    End If
                                    If otmStrategyInstrument IsNot Nothing AndAlso atmStrategyInstrument IsNot Nothing Then
                                        Await otmStrategyInstrument.MonitorAsync(command:=ExecuteCommands.PlaceRegularMarketCNCOrder, "BUY").ConfigureAwait(False)
                                        Await Task.Delay(1000).ConfigureAwait(False)
                                        Await atmStrategyInstrument.MonitorAsync(command:=ExecuteCommands.PlaceRegularMarketCNCOrder, "SELL").ConfigureAwait(False)
                                        Await Task.Delay(5000).ConfigureAwait(False)
                                    End If
                                End If
                            ElseIf stColor = Color.Red Then
                                If buyActive Then
                                    Dim exitInstrument1 As SpreadStrategyInstrument = Nothing
                                    Dim exitInstrument2 As SpreadStrategyInstrument = Nothing
                                    For Each runningInstrument In activeInstruments
                                        If runningInstrument.TradableInstrument.RawInstrumentType.ToUpper = "PE" Then
                                            If runningInstrument.GetQuantityToTrade() < 0 Then
                                                exitInstrument1 = runningInstrument
                                            ElseIf runningInstrument.GetQuantityToTrade() > 0 Then
                                                exitInstrument2 = runningInstrument
                                            End If
                                        End If
                                    Next
                                    If exitInstrument1 IsNot Nothing Then
                                        Await exitInstrument1.MonitorAsync(command:=ExecuteCommands.PlaceRegularMarketCNCOrder, "BUY").ConfigureAwait(False)
                                    End If
                                    If exitInstrument2 IsNot Nothing Then
                                        Await exitInstrument2.MonitorAsync(command:=ExecuteCommands.PlaceRegularMarketCNCOrder, "SELL").ConfigureAwait(False)
                                    End If
                                End If
                                If Not sellActive Then
                                    Dim allInstruments As IEnumerable(Of IInstrument) = CType(Me.ParentStrategy, SpreadStrategy).DependentInstruments
                                    Dim expiryDate As Date = GetExpiryDate()
                                    Dim allCEStrikes As Dictionary(Of Decimal, IInstrument) = Nothing
                                    For Each runningInstrument In allInstruments
                                        If runningInstrument.RawInstrumentName = instrumentData.InstrumentName AndAlso
                                            runningInstrument.RawInstrumentType.ToUpper = "CE" AndAlso
                                            runningInstrument.Expiry.Value.Date = expiryDate.Date Then
                                            If allCEStrikes Is Nothing Then allCEStrikes = New Dictionary(Of Decimal, IInstrument)
                                            If Not allCEStrikes.ContainsKey(runningInstrument.Strike) Then allCEStrikes.Add(runningInstrument.Strike, runningInstrument)
                                        End If
                                    Next

                                    Dim atmStrike As Decimal = GetATMStrike(runningCandlePayload.PreviousPayload.ClosePrice.Value + instrumentData.EntryGap, allCEStrikes.Keys.ToList)
                                    Dim otmStrike As Decimal = GetATMStrike(atmStrike - instrumentData.Distance, allCEStrikes.Keys.ToList)

                                    Await CreateStrategyInstrumentAndPopulate(New List(Of IInstrument) From {allCEStrikes(atmStrike), allCEStrikes(otmStrike)}).ConfigureAwait(False)

                                    Dim otmStrategyInstrument As SpreadStrategyInstrument = Nothing
                                    Dim atmStrategyInstrument As SpreadStrategyInstrument = Nothing
                                    If Me.DependentOptionStrategyInstruments IsNot Nothing AndAlso Me.DependentOptionStrategyInstruments.Count > 0 Then
                                        otmStrategyInstrument = DependentOptionStrategyInstruments.ToList.Find(Function(x)
                                                                                                                   Return x.TradableInstrument.TradingSymbol = allCEStrikes(otmStrike).TradingSymbol
                                                                                                               End Function)
                                        atmStrategyInstrument = DependentOptionStrategyInstruments.ToList.Find(Function(x)
                                                                                                                   Return x.TradableInstrument.TradingSymbol = allCEStrikes(atmStrike).TradingSymbol
                                                                                                               End Function)
                                    End If
                                    If otmStrategyInstrument IsNot Nothing AndAlso atmStrategyInstrument IsNot Nothing Then
                                        Await otmStrategyInstrument.MonitorAsync(command:=ExecuteCommands.PlaceRegularMarketCNCOrder, "BUY").ConfigureAwait(False)
                                        Await Task.Delay(1000).ConfigureAwait(False)
                                        Await atmStrategyInstrument.MonitorAsync(command:=ExecuteCommands.PlaceRegularMarketCNCOrder, "SELL").ConfigureAwait(False)
                                        Await Task.Delay(5000).ConfigureAwait(False)
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If

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

    Public Overrides Async Function MonitorAsync(command As ExecuteCommands, data As Object) As Task
        If data = "BUY" Then
            _direction = IOrder.TypeOfTransaction.Buy
        ElseIf data = "SELL" Then
            _direction = IOrder.TypeOfTransaction.Sell
        End If
        If command = ExecuteCommands.PlaceRegularMarketCNCOrder Then
            Dim orderResponse = Await ExecuteCommandAsync(ExecuteCommands.PlaceRegularMarketCNCOrder, Nothing).ConfigureAwait(False)
            If orderResponse IsNot Nothing AndAlso orderResponse.Count > 0 Then
                Dim placeOrderResponse = CType(orderResponse, Concurrent.ConcurrentBag(Of Object)).FirstOrDefault
                If placeOrderResponse IsNot Nothing AndAlso placeOrderResponse.ContainsKey("data") AndAlso
                        placeOrderResponse("data").ContainsKey("order_id") Then
                    _direction = IOrder.TypeOfTransaction.None
                ElseIf Me.GetQuantityToTrade() = 0 Then
                    _direction = IOrder.TypeOfTransaction.None
                End If
            ElseIf Me.GetQuantityToTrade() = 0 Then
                _direction = IOrder.TypeOfTransaction.None
            End If
        End If
    End Function

    Protected Overrides Async Function IsTriggerReceivedForPlaceOrderAsync(forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)))
        Dim ret As List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        Dim parameters As PlaceOrderParameters = Nothing
        Dim userSettings As SpreadUserInputs = Me.ParentStrategy.UserSettings
        Dim currentTick As ITick = Me.TradableInstrument.LastTick
        While currentTick Is Nothing
            currentTick = Me.TradableInstrument.LastTick
            Await Task.Delay(1000).ConfigureAwait(False)
        End While

        Dim runningCandlePayload As OHLCPayload = New OHLCPayload(OHLCPayload.PayloadSource.CalculatedTick)
        runningCandlePayload.OpenPrice.Value = currentTick.LastPrice
        runningCandlePayload.LowPrice.Value = currentTick.LastPrice
        runningCandlePayload.HighPrice.Value = currentTick.LastPrice
        runningCandlePayload.ClosePrice.Value = currentTick.LastPrice
        runningCandlePayload.TradingSymbol = Me.TradableInstrument.TradingSymbol
        runningCandlePayload.SnapshotDateTime = Now

        Dim quantity As Integer = GetQuantityToTrade()
        If quantity <> 0 Then
            Dim price As Decimal = Decimal.MinValue
            If _direction = IOrder.TypeOfTransaction.Buy AndAlso quantity < 0 Then
                parameters = New PlaceOrderParameters(runningCandlePayload) With
                                       {.EntryDirection = _direction,
                                        .Quantity = Math.Abs(quantity),
                                        .OrderType = IOrder.TypeOfOrder.Market}
            ElseIf _direction = IOrder.TypeOfTransaction.Sell AndAlso quantity > 0 Then
                parameters = New PlaceOrderParameters(runningCandlePayload) With
                                       {.EntryDirection = _direction,
                                        .Quantity = Math.Abs(quantity),
                                        .OrderType = IOrder.TypeOfOrder.Market}
            End If
        Else
            quantity = Me.TradableInstrument.LotSize * userSettings.InstrumentsData(Me.TradableInstrument.RawInstrumentName).NumberOfLots
            parameters = New PlaceOrderParameters(runningCandlePayload) With
                                       {.EntryDirection = _direction,
                                        .Quantity = quantity,
                                        .OrderType = IOrder.TypeOfOrder.Market}
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
                Dim currentSignalActivities As IEnumerable(Of ActivityDashboard) = Me.ParentStrategy.SignalManager.GetSignalActivities(parameters.SignalCandle.SnapshotDateTime, Me.TradableInstrument.InstrumentIdentifier)
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
                        ElseIf lastPlacedActivity.EntryActivity.RequestStatus = ActivityDashboard.SignalStatusType.Completed Then
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

    Public Function GetQuantityToTrade() As Integer
        Dim ret As Integer = 0
        If Me.PositionDetails IsNot Nothing Then
            ret = Me.PositionDetails.Quantity
        End If
        Return ret
    End Function

    Private Function GetActiveInstruments() As List(Of SpreadStrategyInstrument)
        Dim ret As List(Of SpreadStrategyInstrument) = Nothing
        If Me.DependentOptionStrategyInstruments IsNot Nothing AndAlso Me.DependentOptionStrategyInstruments.Count > 0 Then
            For Each runningInstrument In Me.DependentOptionStrategyInstruments
                If runningInstrument.GetQuantityToTrade <> 0 Then
                    If ret Is Nothing Then ret = New List(Of SpreadStrategyInstrument)
                    ret.Add(runningInstrument)
                End If
            Next
        End If
        Return ret
    End Function

    Private Function GetATMStrike(ByVal price As Decimal, ByVal allStrikes As List(Of Decimal)) As Decimal
        Dim ret As Decimal = Decimal.MinValue
        If allStrikes IsNot Nothing AndAlso allStrikes.Count > 0 Then
            Dim upperStrikes As List(Of Decimal) = allStrikes.FindAll(Function(x)
                                                                          Return x >= price
                                                                      End Function)
            Dim lowerStrikes As List(Of Decimal) = allStrikes.FindAll(Function(x)
                                                                          Return x <= price
                                                                      End Function)
            Dim upperStrikePrice As Decimal = Decimal.MaxValue
            Dim lowerStrikePrice As Decimal = Decimal.MinValue
            If upperStrikes IsNot Nothing AndAlso upperStrikes.Count > 0 Then
                upperStrikePrice = upperStrikes.OrderBy(Function(x)
                                                            Return x
                                                        End Function).FirstOrDefault
            End If
            If lowerStrikes IsNot Nothing AndAlso lowerStrikes.Count > 0 Then
                lowerStrikePrice = lowerStrikes.OrderBy(Function(x)
                                                            Return x
                                                        End Function).LastOrDefault
            End If

            If upperStrikePrice <> Decimal.MaxValue AndAlso lowerStrikePrice <> Decimal.MinValue Then
                If upperStrikePrice - price < price - lowerStrikePrice Then
                    ret = upperStrikePrice
                Else
                    ret = lowerStrikePrice
                End If
            ElseIf upperStrikePrice <> Decimal.MaxValue Then
                ret = upperStrikePrice
            ElseIf lowerStrikePrice <> Decimal.MinValue Then
                ret = lowerStrikePrice
            End If
        End If
        Return ret
    End Function

    Private Function GetExpiryDate() As Date
        Dim allInstruments As IEnumerable(Of IInstrument) = CType(Me.ParentStrategy, SpreadStrategy).DependentInstruments
        Dim minExpiry As Date = allInstruments.Min(Function(x)
                                                       Return x.Expiry.Value.Date
                                                   End Function)
        If minExpiry.Date = Now.Date AndAlso Now >= Me.TradableInstrument.ExchangeDetails.ContractRolloverTime Then
            Dim nextExpiry As Date = allInstruments.Min(Function(x)
                                                            If x.Expiry.Value.Date <> minExpiry.Date Then
                                                                Return x.Expiry.Value.Date
                                                            Else
                                                                Return Date.MaxValue
                                                            End If
                                                        End Function)
            Return nextExpiry
        Else
            Return minExpiry.Date
        End If
    End Function

    Public Async Function ContractRolloverAsync() As Task
        Dim userSettings As SpreadUserInputs = Me.ParentStrategy.UserSettings
        Try
            While True
                If Me.ParentStrategy.ParentController.OrphanException IsNot Nothing Then
                    Throw Me.ParentStrategy.ParentController.OrphanException
                End If
                _cts.Token.ThrowIfCancellationRequested()

                If Now >= Me.TradableInstrument.ExchangeDetails.ContractRolloverTime AndAlso
                    Me.DependentOptionStrategyInstruments IsNot Nothing AndAlso
                    Me.DependentOptionStrategyInstruments.Count > 0 Then
                    Dim exitInstrument1 As SpreadStrategyInstrument = Nothing
                    Dim exitInstrument2 As SpreadStrategyInstrument = Nothing
                    For Each runningInstrument In Me.DependentOptionStrategyInstruments
                        If runningInstrument.TradableInstrument.Expiry.Value.Date = Now.Date Then
                            If runningInstrument.GetQuantityToTrade() < 0 Then
                                exitInstrument1 = runningInstrument
                            ElseIf runningInstrument.GetQuantityToTrade() > 0 Then
                                exitInstrument2 = runningInstrument
                            End If
                        End If
                    Next
                    OnHeartbeat("Contract Rollover")
                    If exitInstrument1 IsNot Nothing Then
                        Await exitInstrument1.MonitorAsync(command:=ExecuteCommands.PlaceRegularMarketCNCOrder, "BUY").ConfigureAwait(False)
                    End If
                    If exitInstrument2 IsNot Nothing Then
                        Await exitInstrument2.MonitorAsync(command:=ExecuteCommands.PlaceRegularMarketCNCOrder, "SELL").ConfigureAwait(False)
                    End If

                    Exit While
                End If

                Await Task.Delay(60000, _cts.Token).ConfigureAwait(False)
            End While
        Catch ex As Exception
            logger.Error("Strategy Instrument:{0}, error:{1}", Me.ToString, ex.ToString)
            Throw ex
        End Try
    End Function

#Region "Not required functions"
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