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

    Private _lastPrevPayloadPlaceOrder As String = ""
    Private _entryOrderPlaced As Boolean = False
    Private _myOptionStrategyInstruments As IEnumerable(Of StrategyInstrument)
    Private ReadOnly _dummySupertrendConsumer As SupertrendConsumer
    Private ReadOnly _strikeFileName As String = Nothing

    Public NumberOfLotsToTrade As Integer = 1
    Public StopInstrument As Boolean
    Public StopInstrumentReason As String = ""
    Public TakeTrade As Boolean
    Public MyOppositeOptionInstrument As NFOStrategyInstrument

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
            Dim chartConsumer As PayloadToChartConsumer = New PayloadToChartConsumer(userInputs.SignalTimeFrame)
            chartConsumer.OnwardLevelConsumers = New List(Of IPayloadConsumer)
            chartConsumer.OnwardLevelConsumers.Add(New SupertrendConsumer(chartConsumer, userInputs.SupertrendPeriod, userInputs.SupertrendMultiplier))
            RawPayloadDependentConsumers.Add(chartConsumer)
            _dummySupertrendConsumer = New SupertrendConsumer(chartConsumer, userInputs.SupertrendPeriod, userInputs.SupertrendMultiplier)
        End If

        If Me.TradableInstrument.InstrumentType = IInstrument.TypeOfInstrument.Options Then
            Me.TradableInstrument.FetchHistorical = False
        End If
        Me.TakeTrade = False
        Me.StopInstrument = False

        _strikeFileName = Path.Combine(My.Application.Info.DirectoryPath, String.Format("{0} {1}.StrikeDetails.a2t", Me.TradableInstrument.TradingSymbol, Now.ToString("yy_MM_dd")))
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
            If Me.TradableInstrument.InstrumentType <> IInstrument.TypeOfInstrument.Options Then
                Dim optionRawInstrumentName As String = Me.TradableInstrument.TradingSymbol
                If Me.TradableInstrument.TradingSymbol.Trim.ToUpper = "NIFTY 50" Then
                    optionRawInstrumentName = "NIFTY"
                ElseIf Me.TradableInstrument.TradingSymbol.Trim.ToUpper = "NIFTY BANK" Then
                    optionRawInstrumentName = "BANKNIFTY"
                End If

                _myOptionStrategyInstruments = Me.ParentStrategy.TradableStrategyInstruments.Where(Function(x)
                                                                                                       Return x.TradableInstrument.RawInstrumentName.ToUpper = optionRawInstrumentName.ToUpper AndAlso
                                                                                                        x.TradableInstrument.InstrumentType = IInstrument.TypeOfInstrument.Options
                                                                                                   End Function)

                If _myOptionStrategyInstruments IsNot Nothing AndAlso _myOptionStrategyInstruments.Count > 0 Then
                    Dim optionSelectionDone As Boolean = False
                    Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
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
                        If Not optionSelectionDone AndAlso Me.TradableInstrument.LastTick IsNot Nothing AndAlso
                            Me.TradableInstrument.LastTick.Timestamp.Value.Date = Now.Date AndAlso
                            Now >= New Date(Now.Year, Now.Month, Now.Day, 9, 10, 0) AndAlso
                            Me.TradableInstrument.LastTick.Open > 0 Then
                            If Not File.Exists(_strikeFileName) Then
                                Dim openPrice As Decimal = Me.TradableInstrument.LastTick.Open
                                OnHeartbeat(String.Format("{0}: Open Price={1}. Now it will eliminate instruments outside {2}% range", Me.TradableInstrument.TradingSymbol, openPrice, userSettings.StrikePriceSelectionRangePercentage))
                                For Each runningStrategyInstrument In _myOptionStrategyInstruments
                                    If runningStrategyInstrument.TradableInstrument.Strike >= openPrice - openPrice * userSettings.StrikePriceSelectionRangePercentage / 100 AndAlso
                                    runningStrategyInstrument.TradableInstrument.Strike <= openPrice + openPrice * userSettings.StrikePriceSelectionRangePercentage / 100 Then
                                        runningStrategyInstrument.TradableInstrument.FetchHistorical = True
                                    Else
                                        CType(runningStrategyInstrument, NFOStrategyInstrument).StopInstrumentReason = "+++ Outside allowable range according to open price"
                                        CType(runningStrategyInstrument, NFOStrategyInstrument).StopInstrument = True
                                    End If
                                Next
                            Else
                                Dim strikePrice As Decimal = Utilities.Strings.DeserializeToCollection(Of Tuple(Of Decimal, Color))(_strikeFileName).Item1
                                For Each runningStrategyInstrument In _myOptionStrategyInstruments
                                    If runningStrategyInstrument.TradableInstrument.Strike = strikePrice Then
                                        runningStrategyInstrument.TradableInstrument.FetchHistorical = True
                                    Else
                                        CType(runningStrategyInstrument, NFOStrategyInstrument).StopInstrumentReason = "+++ Not an ATM instrument"
                                        CType(runningStrategyInstrument, NFOStrategyInstrument).StopInstrument = True
                                    End If
                                Next
                            End If
                            optionSelectionDone = True
                        End If
                        If optionSelectionDone Then
                            Dim runningCandlePayload As OHLCPayload = GetXMinuteCurrentCandle(userSettings.SignalTimeFrame)
                            Dim stConsumer As SupertrendConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummySupertrendConsumer)
                            Try
                                If runningCandlePayload IsNot Nothing AndAlso runningCandlePayload.PreviousPayload IsNot Nothing AndAlso
                                    Me.TradableInstrument.IsHistoricalCompleted Then
                                    If Not runningCandlePayload.PreviousPayload.ToString = _lastPrevPayloadPlaceOrder Then
                                        _lastPrevPayloadPlaceOrder = runningCandlePayload.PreviousPayload.ToString
                                        logger.Debug("PlaceOrder-> Potential Signal Candle is:{0}. Will check rest parameters.", runningCandlePayload.ToString)
                                        logger.Debug("PlaceOrder-> Rest all parameters: RunningCandleTime:{0}, PayloadGeneratedBy:{1}, IsHistoricalCompleted:{2}, IsFirstTimeInformationCollected:{3}, Supertrend Color:{4}, Exchange Start Time:{5}, Exchange End Time:{6}, Current Time:{7}, TradingSymbol:{8}",
                                                    runningCandlePayload.SnapshotDateTime.ToString,
                                                    runningCandlePayload.PayloadGeneratedBy.ToString,
                                                    Me.TradableInstrument.IsHistoricalCompleted,
                                                    Me.ParentStrategy.IsFirstTimeInformationCollected,
                                                    CType(stConsumer.ConsumerPayloads(runningCandlePayload.SnapshotDateTime), SupertrendConsumer.SupertrendPayload).SupertrendColor.Name,
                                                    Me.TradableInstrument.ExchangeDetails.ExchangeStartTime.ToString,
                                                    Me.TradableInstrument.ExchangeDetails.ExchangeEndTime.ToString,
                                                    Me.TradableInstrument.LastTick.Timestamp.Value.ToString("dd-MMM-yyyy HH:mm:ss"),
                                                    Me.TradableInstrument.TradingSymbol)
                                    End If
                                End If
                            Catch ex As Exception
                                logger.Error(ex)
                            End Try

                            If Me.TradableInstrument.IsHistoricalCompleted AndAlso Me.TradableInstrument.LastTick.Timestamp.Value >= userSettings.TradeStartTime AndAlso
                                runningCandlePayload IsNot Nothing AndAlso stConsumer.ConsumerPayloads.ContainsKey(runningCandlePayload.SnapshotDateTime) Then
                                Dim atmStrikePrice As Decimal = Decimal.MinValue
                                Dim supertrendColor As Color = Color.White
                                If File.Exists(_strikeFileName) Then
                                    atmStrikePrice = Utilities.Strings.DeserializeToCollection(Of Tuple(Of Decimal, Color))(_strikeFileName).Item1
                                    supertrendColor = Utilities.Strings.DeserializeToCollection(Of Tuple(Of Decimal, Color))(_strikeFileName).Item2
                                    OnHeartbeat(String.Format("Selected ATM Strike={0}, Supertrend Color={1}", atmStrikePrice, supertrendColor.Name))
                                Else
                                    supertrendColor = CType(stConsumer.ConsumerPayloads(runningCandlePayload.SnapshotDateTime), SupertrendConsumer.SupertrendPayload).SupertrendColor
                                    Dim currentPrice As Decimal = Me.TradableInstrument.LastTick.LastPrice
                                    Dim upperStrikes As IEnumerable(Of StrategyInstrument) = _myOptionStrategyInstruments.Where(Function(x)
                                                                                                                                    Return x.TradableInstrument.FetchHistorical AndAlso
                                                                                                                                        x.TradableInstrument.Strike >= currentPrice
                                                                                                                                End Function)
                                    Dim lowerStrikes As IEnumerable(Of StrategyInstrument) = _myOptionStrategyInstruments.Where(Function(x)
                                                                                                                                    Return x.TradableInstrument.FetchHistorical AndAlso
                                                                                                                                        x.TradableInstrument.Strike <= currentPrice
                                                                                                                                End Function)
                                    Dim upperStrikePrice As Decimal = Decimal.MaxValue
                                    Dim lowerStrikePrice As Decimal = Decimal.MinValue
                                    If upperStrikes IsNot Nothing AndAlso upperStrikes.Count > 0 Then
                                        upperStrikePrice = upperStrikes.OrderBy(Function(x)
                                                                                    Return x.TradableInstrument.Strike
                                                                                End Function).FirstOrDefault.TradableInstrument.Strike
                                    End If
                                    If lowerStrikes IsNot Nothing AndAlso lowerStrikes.Count > 0 Then
                                        lowerStrikePrice = lowerStrikes.OrderBy(Function(x)
                                                                                    Return x.TradableInstrument.Strike
                                                                                End Function).LastOrDefault.TradableInstrument.Strike
                                    End If

                                    If upperStrikePrice <> Decimal.MaxValue AndAlso lowerStrikePrice <> Decimal.MinValue Then
                                        If upperStrikePrice - currentPrice < currentPrice - lowerStrikePrice Then
                                            atmStrikePrice = upperStrikePrice
                                        Else
                                            atmStrikePrice = lowerStrikePrice
                                        End If
                                    ElseIf upperStrikePrice <> Decimal.MaxValue Then
                                        atmStrikePrice = upperStrikePrice
                                    ElseIf lowerStrikePrice <> Decimal.MinValue Then
                                        atmStrikePrice = lowerStrikePrice
                                    End If
                                    OnHeartbeat(String.Format("Price={0}, Selected ATM Strike={1}, Supertrend Color={2}", currentPrice, atmStrikePrice, supertrendColor.Name))
                                    If atmStrikePrice <> Decimal.MinValue Then Utilities.Strings.SerializeFromCollection(Of Tuple(Of Decimal, Color))(_strikeFileName, New Tuple(Of Decimal, Color)(atmStrikePrice, supertrendColor))
                                End If
                                If atmStrikePrice <> Decimal.MinValue Then
                                    Dim atmCall As NFOStrategyInstrument = _myOptionStrategyInstruments.Where(Function(x)
                                                                                                                  Return x.TradableInstrument.Strike = atmStrikePrice AndAlso x.TradableInstrument.RawInstrumentType = "CE"
                                                                                                              End Function).FirstOrDefault
                                    Dim atmPut As NFOStrategyInstrument = _myOptionStrategyInstruments.Where(Function(x)
                                                                                                                 Return x.TradableInstrument.Strike = atmStrikePrice AndAlso x.TradableInstrument.RawInstrumentType = "PE"
                                                                                                             End Function).FirstOrDefault
                                    If atmCall IsNot Nothing AndAlso atmPut IsNot Nothing Then
                                        For Each runningStrategyInstrument In _myOptionStrategyInstruments
                                            If runningStrategyInstrument.TradableInstrument.InstrumentIdentifier <> atmCall.TradableInstrument.InstrumentIdentifier AndAlso
                                                runningStrategyInstrument.TradableInstrument.InstrumentIdentifier <> atmPut.TradableInstrument.InstrumentIdentifier Then
                                                CType(runningStrategyInstrument, NFOStrategyInstrument).StopInstrumentReason = "+++ Not an ATM instrument"
                                                CType(runningStrategyInstrument, NFOStrategyInstrument).StopInstrument = True
                                            End If
                                        Next

                                        atmCall.NumberOfLotsToTrade = userSettings.InstrumentsData(Me.TradableInstrument.TradingSymbol.ToUpper).NumberOfLots
                                        atmPut.NumberOfLotsToTrade = userSettings.InstrumentsData(Me.TradableInstrument.TradingSymbol.ToUpper).NumberOfLots
                                        If supertrendColor = Color.Green Then
                                            atmPut.TakeTrade = True
                                            atmPut.MyOppositeOptionInstrument = atmCall
                                        Else
                                            atmCall.TakeTrade = True
                                            atmCall.MyOppositeOptionInstrument = atmPut
                                        End If
                                        Exit While
                                    End If
                                End If
                            End If
                        End If

                        _cts.Token.ThrowIfCancellationRequested()
                        Await Task.Delay(1000, _cts.Token).ConfigureAwait(False)
                    End While
                Else
                    Me.StopInstrumentReason = "No option instrument found"
                End If
            Else
                While True
                    Me.TradableInstrument.FetchHistorical = True
                    If Me.ParentStrategy.ParentController.OrphanException IsNot Nothing Then
                        Throw Me.ParentStrategy.ParentController.OrphanException
                    End If
                    If Me._RMSException IsNot Nothing AndAlso
                        _RMSException.ExceptionType = Algo2TradeCore.Exceptions.AdapterBusinessException.TypeOfException.RMSError Then
                        OnHeartbeat(String.Format("{0}:Will not take no more action in this instrument as RMS Error occured. Error-{1}", Me.TradableInstrument.TradingSymbol, _RMSException.Message))
                        Throw Me._RMSException
                    End If
                    If Me.StopInstrument Then
                        Exit While
                    End If
                    _cts.Token.ThrowIfCancellationRequested()

                    'Place Order block start
                    Dim placeOrderTriggers As List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)) = Await IsTriggerReceivedForPlaceOrderAsync(False).ConfigureAwait(False)
                    If placeOrderTriggers IsNot Nothing AndAlso placeOrderTriggers.Count > 0 AndAlso
                        placeOrderTriggers.FirstOrDefault.Item1 = ExecuteCommandAction.Take Then
                        Dim orderResponse = Await ExecuteCommandAsync(ExecuteCommands.PlaceRegularMarketMISOrder, Nothing).ConfigureAwait(False)
                        If orderResponse IsNot Nothing AndAlso orderResponse.Count > 0 Then
                            Dim placeOrderResponse = CType(orderResponse, Concurrent.ConcurrentBag(Of Object)).FirstOrDefault
                            If placeOrderResponse.ContainsKey("data") AndAlso
                                placeOrderResponse("data").ContainsKey("order_id") Then
                                If placeOrderTriggers.FirstOrDefault.Item2.EntryDirection = IOrder.TypeOfTransaction.Sell Then
                                    _entryOrderPlaced = True
                                Else
                                    If Me.MyOppositeOptionInstrument IsNot Nothing Then
                                        Me.MyOppositeOptionInstrument.TakeTrade = True
                                    End If
                                    Exit While
                                End If
                            End If
                        End If
                    End If
                    'Place Order block end

                    _cts.Token.ThrowIfCancellationRequested()
                    Await Task.Delay(1000, _cts.Token).ConfigureAwait(False)
                End While
            End If
        Catch ex As Exception
            'To log exceptions getting created from this function as the bubble up of the exception
            'will anyways happen to Strategy.MonitorAsync but it will not be shown until all tasks exit
            logger.Error("Strategy Instrument:{0}, error:{1}", Me.ToString, ex.ToString)
            Throw ex
        Finally
            Me.TradableInstrument.FetchHistorical = False
            _strategyInstrumentRunning = False
            If Not Me.StopInstrumentReason.StartsWith("+++") Then
                OnHeartbeat(String.Format("Strategy Instrument Stopped. {0}", Me.StopInstrumentReason))
            End If
        End Try
    End Function

    Protected Overrides Async Function IsTriggerReceivedForPlaceOrderAsync(forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)))
        Dim ret As List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
        Dim runningCandlePayload As OHLCPayload = GetXMinuteCurrentCandle(userSettings.SignalTimeFrame)
        Dim stConsumer As SupertrendConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummySupertrendConsumer)
        Dim currentTick As ITick = Me.TradableInstrument.LastTick
        Dim currentTime As Date = Now()
        Dim log As Boolean = False

        Try
            If runningCandlePayload IsNot Nothing AndAlso runningCandlePayload.PreviousPayload IsNot Nothing AndAlso Me.TradableInstrument.IsHistoricalCompleted Then
                If Not runningCandlePayload.PreviousPayload.ToString = _lastPrevPayloadPlaceOrder OrElse forcePrint Then
                    _lastPrevPayloadPlaceOrder = runningCandlePayload.PreviousPayload.ToString
                    log = True
                    logger.Debug("PlaceOrder-> Potential Signal Candle is:{0}. Will check rest parameters.", runningCandlePayload.PreviousPayload.ToString)
                    logger.Debug("PlaceOrder-> Rest all parameters: RunningCandleTime:{0}, PayloadGeneratedBy:{1}, IsHistoricalCompleted:{2}, IsFirstTimeInformationCollected:{3}, Supertrend Color:{4}, Exchange Start Time:{5}, Exchange End Time:{6}, Current Time:{7}, Take Trade:{8}, Entry Order Placed:{9}, TradingSymbol:{10}",
                                runningCandlePayload.SnapshotDateTime.ToString,
                                runningCandlePayload.PayloadGeneratedBy.ToString,
                                Me.TradableInstrument.IsHistoricalCompleted,
                                Me.ParentStrategy.IsFirstTimeInformationCollected,
                                CType(stConsumer.ConsumerPayloads(runningCandlePayload.PreviousPayload.SnapshotDateTime), SupertrendConsumer.SupertrendPayload).SupertrendColor.Name,
                                Me.TradableInstrument.ExchangeDetails.ExchangeStartTime.ToString,
                                Me.TradableInstrument.ExchangeDetails.ExchangeEndTime.ToString,
                                Now.ToString("dd-MMM-yyyy HH:mm:ss"),
                                Me.TakeTrade,
                                _entryOrderPlaced,
                                Me.TradableInstrument.TradingSymbol)
                End If
            End If
        Catch ex As Exception
            logger.Warn(ex)
        End Try

        Dim parameters As PlaceOrderParameters = Nothing
        If currentTime >= userSettings.TradeStartTime AndAlso runningCandlePayload IsNot Nothing AndAlso runningCandlePayload.SnapshotDateTime >= userSettings.TradeStartTime AndAlso
            runningCandlePayload.PayloadGeneratedBy = OHLCPayload.PayloadSource.CalculatedTick AndAlso runningCandlePayload.PreviousPayload IsNot Nothing AndAlso
            Me.TradableInstrument.IsHistoricalCompleted AndAlso Me.ParentStrategy.IsFirstTimeInformationCollected AndAlso stConsumer.ConsumerPayloads IsNot Nothing AndAlso
            stConsumer.ConsumerPayloads.Count > 0 AndAlso stConsumer.ConsumerPayloads.ContainsKey(runningCandlePayload.PreviousPayload.SnapshotDateTime) Then
            Dim supertrendColor As Color = CType(stConsumer.ConsumerPayloads(runningCandlePayload.PreviousPayload.SnapshotDateTime), SupertrendConsumer.SupertrendPayload).SupertrendColor
            Dim quantity As Integer = Me.TradableInstrument.LotSize * Me.NumberOfLotsToTrade
            If currentTime <= userSettings.EODExitTime Then
                If Not _entryOrderPlaced Then
                    If Me.TakeTrade AndAlso currentTime <= userSettings.LastTradeEntryTime Then
                        If Me.OrderDetails IsNot Nothing AndAlso Me.OrderDetails.Count >= 1 Then
                            _entryOrderPlaced = True
                        Else
                            If supertrendColor = Color.Red Then
                                If forcePrint Then OnHeartbeat("***** Supertrend Color:Red. So it will place entry order.")
                                parameters = New PlaceOrderParameters(runningCandlePayload.PreviousPayload) With
                                    {.EntryDirection = IOrder.TypeOfTransaction.Sell,
                                     .Quantity = quantity}
                            Else
                                If log Then
                                    OnHeartbeat("Supertrend Color:Green. So it will wait now for entry.")
                                End If
                            End If
                        End If
                    End If
                Else
                    If Me.OrderDetails IsNot Nothing AndAlso Me.OrderDetails.Count >= 2 Then
                        If Me.MyOppositeOptionInstrument IsNot Nothing Then
                            Me.MyOppositeOptionInstrument.TakeTrade = True
                        End If
                        Me.StopInstrumentReason = "Both trades taken"
                        Me.StopInstrument = True
                    Else
                        If supertrendColor = Color.Green Then
                            If forcePrint Then OnHeartbeat("***** Supertrend Color:Green. So it will place exit order.")
                            parameters = New PlaceOrderParameters(runningCandlePayload.PreviousPayload) With
                                    {.EntryDirection = IOrder.TypeOfTransaction.Buy,
                                     .Quantity = quantity}
                        Else
                            If log Then OnHeartbeat(String.Format("Supertrend Color:{0}. So it will not place exit order", supertrendColor.Name))
                        End If
                    End If
                End If
            Else
                If _entryOrderPlaced Then
                    If Me.OrderDetails IsNot Nothing AndAlso Me.OrderDetails.Count >= 2 Then
                        If Me.MyOppositeOptionInstrument IsNot Nothing Then
                            Me.MyOppositeOptionInstrument.TakeTrade = True
                        End If
                        Me.StopInstrumentReason = "Both trades taken"
                        Me.StopInstrument = True
                    Else
                        If forcePrint Then OnHeartbeat(String.Format("***** Current Time({0}) > EOD Exit Time({1}). So it will place exit order.", currentTime.ToString("HH:mm:ss"), userSettings.EODExitTime.ToString("HH:mm:ss")))
                        parameters = New PlaceOrderParameters(runningCandlePayload.PreviousPayload) With
                                {.EntryDirection = IOrder.TypeOfTransaction.Buy,
                                 .Quantity = quantity,
                                 .Supporting = New List(Of Object) From {"EOD Exit"}}
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

            If parameters IsNot Nothing AndAlso currentTime >= Me.TradableInstrument.ExchangeDetails.ExchangeStartTime AndAlso currentTime <= Me.TradableInstrument.ExchangeDetails.ExchangeEndTime Then
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