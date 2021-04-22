﻿Imports NLog
Imports System.IO
Imports Algo2TradeCore
Imports System.Threading
Imports Utilities.Numbers
Imports Algo2TradeCore.Adapter
Imports Algo2TradeCore.Entities
Imports Algo2TradeCore.Strategies
Imports Algo2TradeCore.Entities.Indicators
Imports Algo2TradeCore.ChartHandler.ChartStyle

Public Class StrangleStrategyInstrument
    Inherits StrategyInstrument
    Implements IDisposable

#Region "Logging and Status Progress"
    Public Shared Shadows logger As Logger = LogManager.GetCurrentClassLogger
#End Region

    Private _lastPrevPayloadPlaceOrder As String = ""
    Private _myOptionStrategyInstruments As IEnumerable(Of StrategyInstrument)
    Private ReadOnly _dummySupertrendConsumer As SupertrendConsumer
    Private ReadOnly _strangleFileName As String = Nothing

    Public StopInstrument As Boolean
    Public StopInstrumentReason As String = ""

    Public MyParentInstrumentDetails As StrangleUserInputs.InstrumentDetails
    Public MainTrade As Boolean
    Public SupportTrade As Boolean

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
            Dim userInputs As StrangleUserInputs = Me.ParentStrategy.UserSettings
            Dim instrumentData As StrangleUserInputs.InstrumentDetails = Nothing
            If Me.TradableInstrument.InstrumentType = IInstrument.TypeOfInstrument.Options Then
                If Me.TradableInstrument.RawInstrumentName = "NIFTY" Then
                    instrumentData = userInputs.InstrumentsData("NIFTY 50")
                ElseIf Me.TradableInstrument.RawInstrumentName = "BANKNIFTY" Then
                    instrumentData = userInputs.InstrumentsData("NIFTY BANK")
                Else
                    instrumentData = userInputs.InstrumentsData(Me.TradableInstrument.RawInstrumentName.ToUpper)
                End If
            Else
                instrumentData = userInputs.InstrumentsData(Me.TradableInstrument.TradingSymbol.ToUpper)
            End If
            Dim chartConsumer As PayloadToChartConsumer = New PayloadToChartConsumer(instrumentData.Timeframe)
            chartConsumer.OnwardLevelConsumers = New List(Of IPayloadConsumer)
            chartConsumer.OnwardLevelConsumers.Add(New SupertrendConsumer(chartConsumer, instrumentData.SupertrendPeriod, instrumentData.SupertrendMultiplier))
            RawPayloadDependentConsumers.Add(chartConsumer)
            _dummySupertrendConsumer = New SupertrendConsumer(chartConsumer, instrumentData.SupertrendPeriod, instrumentData.SupertrendMultiplier)
        End If

        If Me.TradableInstrument.InstrumentType = IInstrument.TypeOfInstrument.Options Then
            Me.TradableInstrument.FetchHistorical = False
        End If
        Me.MainTrade = False
        Me.SupportTrade = False
        Me.StopInstrument = False

        _strangleFileName = Path.Combine(My.Application.Info.DirectoryPath, String.Format("{0} {1}.Strangle.a2t", Me.TradableInstrument.TradingSymbol, Now.ToString("yy_MM_dd")))
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
                    Dim userSettings As StrangleUserInputs = Me.ParentStrategy.UserSettings
                    Dim instrumentDetails As StrangleUserInputs.InstrumentDetails = userSettings.InstrumentsData(Me.TradableInstrument.TradingSymbol.ToUpper)
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
                            If Not File.Exists(_strangleFileName) Then
                                Dim openPrice As Decimal = Me.TradableInstrument.LastTick.Open
                                OnHeartbeat(String.Format("{0}: Open Price={1}. Now it will eliminate instruments outside {2}% range", Me.TradableInstrument.TradingSymbol, openPrice, userSettings.StrikePriceSelectionRangePercentage))
                                For Each runningStrategyInstrument In _myOptionStrategyInstruments
                                    If runningStrategyInstrument.TradableInstrument.Strike >= openPrice - openPrice * userSettings.StrikePriceSelectionRangePercentage / 100 AndAlso
                                    runningStrategyInstrument.TradableInstrument.Strike <= openPrice + openPrice * userSettings.StrikePriceSelectionRangePercentage / 100 Then
                                        runningStrategyInstrument.TradableInstrument.FetchHistorical = True
                                    Else
                                        CType(runningStrategyInstrument, StrangleStrategyInstrument).StopInstrumentReason = "+++ Outside allowable range according to open price"
                                        CType(runningStrategyInstrument, StrangleStrategyInstrument).StopInstrument = True
                                    End If
                                Next
                            Else
                                Dim mainIns As String = Utilities.Strings.DeserializeToCollection(Of Tuple(Of String, String))(_strangleFileName).Item1
                                Dim supportIns As String = Utilities.Strings.DeserializeToCollection(Of Tuple(Of String, String))(_strangleFileName).Item2
                                For Each runningStrategyInstrument In _myOptionStrategyInstruments
                                    If runningStrategyInstrument.TradableInstrument.TradingSymbol.ToUpper = mainIns.ToUpper OrElse
                                        runningStrategyInstrument.TradableInstrument.TradingSymbol.ToUpper = supportIns.ToUpper Then
                                        runningStrategyInstrument.TradableInstrument.FetchHistorical = True
                                    Else
                                        CType(runningStrategyInstrument, StrangleStrategyInstrument).StopInstrumentReason = "+++ Not an ATM instrument"
                                        CType(runningStrategyInstrument, StrangleStrategyInstrument).StopInstrument = True
                                    End If
                                Next
                            End If
                            optionSelectionDone = True
                        End If
                        If optionSelectionDone Then
                            Dim runningCandlePayload As OHLCPayload = GetXMinuteCurrentCandle(instrumentDetails.Timeframe)
                            Dim stConsumer As SupertrendConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummySupertrendConsumer)
                            Try
                                If runningCandlePayload IsNot Nothing AndAlso runningCandlePayload.PreviousPayload IsNot Nothing AndAlso
                                    Me.TradableInstrument.IsHistoricalCompleted Then
                                    If Not runningCandlePayload.PreviousPayload.ToString = _lastPrevPayloadPlaceOrder Then
                                        _lastPrevPayloadPlaceOrder = runningCandlePayload.PreviousPayload.ToString
                                        logger.Debug("PlaceOrder-> Potential Signal Candle is:{0}. Will check rest parameters.", runningCandlePayload.ToString)
                                        logger.Debug("PlaceOrder-> Rest all parameters: RunningCandleTime:{0}, PayloadGeneratedBy:{1}, IsHistoricalCompleted:{2}, IsFirstTimeInformationCollected:{3}, Supertrend:{4}, Supertrend Color:{5}, Exchange Start Time:{6}, Exchange End Time:{7}, Current Time:{8}, TradingSymbol:{9}",
                                                    runningCandlePayload.SnapshotDateTime.ToString,
                                                    runningCandlePayload.PayloadGeneratedBy.ToString,
                                                    Me.TradableInstrument.IsHistoricalCompleted,
                                                    Me.ParentStrategy.IsFirstTimeInformationCollected,
                                                    CType(stConsumer.ConsumerPayloads(runningCandlePayload.SnapshotDateTime), SupertrendConsumer.SupertrendPayload).Supertrend.Value,
                                                    CType(stConsumer.ConsumerPayloads(runningCandlePayload.SnapshotDateTime), SupertrendConsumer.SupertrendPayload).SupertrendColor.Name,
                                                    Me.TradableInstrument.ExchangeDetails.ExchangeStartTime.ToString,
                                                    Me.TradableInstrument.ExchangeDetails.ExchangeEndTime.ToString,
                                                    Now.ToString("dd-MMM-yyyy HH:mm:ss"),
                                                    Me.TradableInstrument.TradingSymbol)
                                    End If
                                End If
                            Catch ex As Exception
                                logger.Error(ex)
                            End Try

                            If Me.TradableInstrument.IsHistoricalCompleted AndAlso Me.TradableInstrument.LastTick.Timestamp.Value >= userSettings.TradeStartTime AndAlso
                                runningCandlePayload IsNot Nothing AndAlso stConsumer.ConsumerPayloads.ContainsKey(runningCandlePayload.SnapshotDateTime) Then
                                Dim mainInstrument As StrangleStrategyInstrument = Nothing
                                Dim supportInstrument As StrangleStrategyInstrument = Nothing
                                If File.Exists(_strangleFileName) Then
                                    Dim mainIns As String = Utilities.Strings.DeserializeToCollection(Of Tuple(Of String, String))(_strangleFileName).Item1
                                    Dim supportIns As String = Utilities.Strings.DeserializeToCollection(Of Tuple(Of String, String))(_strangleFileName).Item2

                                    mainInstrument = _myOptionStrategyInstruments.Where(Function(x)
                                                                                            Return x.TradableInstrument.TradingSymbol.ToUpper = mainIns.ToUpper
                                                                                        End Function).FirstOrDefault
                                    supportInstrument = _myOptionStrategyInstruments.Where(Function(x)
                                                                                               Return x.TradableInstrument.TradingSymbol.ToUpper = supportIns.ToUpper
                                                                                           End Function).FirstOrDefault

                                    OnHeartbeat(String.Format("Selected Main Instrument={0}, Supporting Instrument={1}",
                                                              mainInstrument.TradableInstrument.TradingSymbol, supportInstrument.TradableInstrument.TradingSymbol))
                                Else
                                    Dim allStrikes As List(Of Decimal) = Nothing
                                    For Each runningInstrument In _myOptionStrategyInstruments
                                        If runningInstrument.StrategyInstrumentRunning Then
                                            If allStrikes Is Nothing Then allStrikes = New List(Of Decimal)
                                            If Not allStrikes.Contains(runningInstrument.TradableInstrument.Strike) Then
                                                allStrikes.Add(runningInstrument.TradableInstrument.Strike)
                                            End If
                                        End If
                                    Next
                                    Dim supertrend As SupertrendConsumer.SupertrendPayload = stConsumer.ConsumerPayloads(runningCandlePayload.SnapshotDateTime)
                                    Dim currentPrice As Decimal = Me.TradableInstrument.LastTick.LastPrice
                                    Dim supertrendATM As Decimal = GetATMStrike(supertrend.Supertrend.Value, allStrikes)
                                    If supertrendATM <> Decimal.MinValue Then
                                        If supertrend.SupertrendColor = Color.Green Then
                                            mainInstrument = _myOptionStrategyInstruments.Where(Function(x)
                                                                                                    Return x.TradableInstrument.Strike = supertrendATM AndAlso
                                                                                                    x.TradableInstrument.RawInstrumentType = "PE"
                                                                                                End Function).FirstOrDefault

                                            Dim closeATM As Decimal = GetATMStrike(currentPrice + instrumentDetails.DistanceFromClose, allStrikes)
                                            If closeATM <> Decimal.MinValue Then
                                                supportInstrument = _myOptionStrategyInstruments.Where(Function(x)
                                                                                                           Return x.TradableInstrument.Strike = closeATM AndAlso
                                                                                                           x.TradableInstrument.RawInstrumentType = "CE"
                                                                                                       End Function).FirstOrDefault
                                            End If

                                            OnHeartbeat(String.Format("Supertrend={0}, Supertrend Color={1}, Close={2}, Supertrend Strike={3}, Close Strike={4}",
                                                                      Math.Round(supertrend.Supertrend.Value, 2), supertrend.SupertrendColor.Name,
                                                                      currentPrice, supertrendATM, closeATM))
                                        ElseIf supertrend.SupertrendColor = Color.Red Then
                                            mainInstrument = _myOptionStrategyInstruments.Where(Function(x)
                                                                                                    Return x.TradableInstrument.Strike = supertrendATM AndAlso
                                                                                                    x.TradableInstrument.RawInstrumentType = "CE"
                                                                                                End Function).FirstOrDefault

                                            Dim closeATM As Decimal = GetATMStrike(currentPrice - instrumentDetails.DistanceFromClose, allStrikes)
                                            If closeATM <> Decimal.MinValue Then
                                                supportInstrument = _myOptionStrategyInstruments.Where(Function(x)
                                                                                                           Return x.TradableInstrument.Strike = closeATM AndAlso
                                                                                                           x.TradableInstrument.RawInstrumentType = "PE"
                                                                                                       End Function).FirstOrDefault
                                            End If

                                            OnHeartbeat(String.Format("Supertrend={0}, Supertrend Color={1}, Close={2}, Supertrend Strike={3}, Close Strike={4}",
                                                                      Math.Round(supertrend.Supertrend.Value, 2), supertrend.SupertrendColor.Name,
                                                                      currentPrice, supertrendATM, closeATM))
                                        End If
                                    Else
                                        OnHeartbeat(String.Format("Supertrend={0}, Supertrend Strike={1}, Supertrend Color={2}", supertrend.Supertrend.Value, supertrendATM, supertrend.SupertrendColor.Name))
                                    End If
                                    If mainInstrument IsNot Nothing AndAlso supportInstrument IsNot Nothing Then
                                        Utilities.Strings.SerializeFromCollection(Of Tuple(Of String, String))(_strangleFileName, New Tuple(Of String, String)(mainInstrument.TradableInstrument.TradingSymbol, supportInstrument.TradableInstrument.TradingSymbol))
                                    End If
                                End If
                                If mainInstrument IsNot Nothing AndAlso supportInstrument IsNot Nothing Then
                                    For Each runningStrategyInstrument In _myOptionStrategyInstruments
                                        If runningStrategyInstrument.TradableInstrument.InstrumentIdentifier <> mainInstrument.TradableInstrument.InstrumentIdentifier AndAlso
                                            runningStrategyInstrument.TradableInstrument.InstrumentIdentifier <> supportInstrument.TradableInstrument.InstrumentIdentifier Then
                                            CType(runningStrategyInstrument, StrangleStrategyInstrument).StopInstrumentReason = "+++ Not an ATM instrument"
                                            CType(runningStrategyInstrument, StrangleStrategyInstrument).StopInstrument = True
                                        End If
                                    Next

                                    mainInstrument.MyParentInstrumentDetails = instrumentDetails
                                    mainInstrument.MainTrade = True
                                    supportInstrument.MyParentInstrumentDetails = instrumentDetails
                                    supportInstrument.SupportTrade = True

                                    Exit While
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
                    If Me.MyParentInstrumentDetails IsNot Nothing AndAlso (Me.MainTrade OrElse Me.SupportTrade) Then
                        Dim placeOrderTriggers As List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)) = Await IsTriggerReceivedForPlaceOrderAsync(False).ConfigureAwait(False)
                        If placeOrderTriggers IsNot Nothing AndAlso placeOrderTriggers.Count > 0 AndAlso
                            placeOrderTriggers.FirstOrDefault.Item1 = ExecuteCommandAction.Take Then
                            Dim orderResponse = Nothing
                            If placeOrderTriggers.FirstOrDefault.Item2.OrderType = IOrder.TypeOfOrder.SL Then
                                orderResponse = Await ExecuteCommandAsync(ExecuteCommands.PlaceRegularSLMISOrder, Nothing).ConfigureAwait(False)
                            ElseIf placeOrderTriggers.FirstOrDefault.Item2.OrderType = IOrder.TypeOfOrder.Market Then
                                orderResponse = Await ExecuteCommandAsync(ExecuteCommands.PlaceRegularMarketMISOrder, Nothing).ConfigureAwait(False)
                            End If
                            'If orderResponse IsNot Nothing AndAlso orderResponse.Count > 0 Then
                            '    Dim placeOrderResponse = CType(orderResponse, Concurrent.ConcurrentBag(Of Object)).FirstOrDefault
                            '    If placeOrderResponse.ContainsKey("data") AndAlso
                            '        placeOrderResponse("data").ContainsKey("order_id") Then

                            '    End If
                            'End If
                        End If
                    End If
                    'Place Order block end

                    'Cancel Order Block start
                    Dim cancelOrderTrigger As List(Of Tuple(Of ExecuteCommandAction, IOrder, String)) = Await IsTriggerReceivedForExitOrderAsync(False).ConfigureAwait(False)
                    If cancelOrderTrigger IsNot Nothing AndAlso cancelOrderTrigger.Count > 0 Then
                        Dim orderResponse = Await ExecuteCommandAsync(ExecuteCommands.CancelRegularOrder, Nothing).ConfigureAwait(False)
                        'If orderResponse IsNot Nothing AndAlso orderResponse.Count > 0 Then
                        '    Dim exitOrderResponse = CType(orderResponse, Concurrent.ConcurrentBag(Of Object)).FirstOrDefault
                        '    If exitOrderResponse.ContainsKey("data") AndAlso
                        '        exitOrderResponse("data").ContainsKey("status") AndAlso exitOrderResponse("data")("status") = "Ok" Then

                        '    End If
                        'End If
                    End If
                    'Cancel Order Block end

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
            If Not Me.StopInstrumentReason.StartsWith("+++") Then OnHeartbeat(String.Format("Strategy Instrument Stopped. {0}", Me.StopInstrumentReason))
            _strategyInstrumentRunning = False
        End Try
    End Function

    Protected Overrides Async Function IsTriggerReceivedForPlaceOrderAsync(forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)))
        Dim ret As List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        Dim userSettings As StrangleUserInputs = Me.ParentStrategy.UserSettings
        Dim runningCandlePayload As OHLCPayload = GetXMinuteCurrentCandle(MyParentInstrumentDetails.Timeframe)
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
                    logger.Debug("PlaceOrder-> Rest all parameters: RunningCandleTime:{0}, PayloadGeneratedBy:{1}, IsHistoricalCompleted:{2}, IsFirstTimeInformationCollected:{3}, Supertrend Color:{4}, Exchange Start Time:{5}, Exchange End Time:{6}, Current Time:{7}, Main Trade:{8}, Support Trade:{9}, Active Instrument:{10}, Open Instrument:{11}, Support Complete:{12}, TradingSymbol:{13}",
                                runningCandlePayload.SnapshotDateTime.ToString,
                                runningCandlePayload.PayloadGeneratedBy.ToString,
                                Me.TradableInstrument.IsHistoricalCompleted,
                                Me.ParentStrategy.IsFirstTimeInformationCollected,
                                CType(stConsumer.ConsumerPayloads(runningCandlePayload.PreviousPayload.SnapshotDateTime), SupertrendConsumer.SupertrendPayload).SupertrendColor.Name,
                                Me.TradableInstrument.ExchangeDetails.ExchangeStartTime.ToString,
                                Me.TradableInstrument.ExchangeDetails.ExchangeEndTime.ToString,
                                Now.ToString("dd-MMM-yyyy HH:mm:ss"),
                                Me.MainTrade,
                                Me.SupportTrade,
                                IsRunningInstrument(),
                                IsOpenInstrument(),
                                If(Me.SupportTrade, IsTradeComplete(), False),
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
            Dim quantity As Integer = Me.TradableInstrument.LotSize * Me.MyParentInstrumentDetails.NumberOfLots
            If currentTime <= userSettings.EODExitTime Then
                If Not IsRunningInstrument() AndAlso Not IsOpenInstrument() Then
                    If currentTime <= userSettings.LastTradeEntryTime Then
                        If Me.MainTrade Then
                            If supertrendColor = Color.Red Then
                                If forcePrint Then OnHeartbeat("***** Supertrend Color:Red. So it will place entry order.")
                                parameters = New PlaceOrderParameters(runningCandlePayload.PreviousPayload) With
                                    {.EntryDirection = IOrder.TypeOfTransaction.Sell,
                                     .Quantity = quantity,
                                     .OrderType = IOrder.TypeOfOrder.Market}
                            Else
                                If log Then OnHeartbeat("Supertrend Color:Green. So it will wait now for entry.")
                            End If
                        ElseIf Me.SupportTrade Then
                            If Not IsTradeComplete() Then
                                parameters = New PlaceOrderParameters(runningCandlePayload.PreviousPayload) With
                                    {.EntryDirection = IOrder.TypeOfTransaction.Sell,
                                     .Quantity = quantity,
                                     .OrderType = IOrder.TypeOfOrder.Market}
                            End If
                        End If
                    End If
                ElseIf IsRunningInstrument() Then
                    If Me.MainTrade Then
                        If supertrendColor = Color.Green Then
                            If forcePrint Then OnHeartbeat("***** Supertrend Color:Green. So it will place exit order.")
                            parameters = New PlaceOrderParameters(runningCandlePayload.PreviousPayload) With
                                    {.EntryDirection = IOrder.TypeOfTransaction.Buy,
                                     .Quantity = quantity,
                                     .OrderType = IOrder.TypeOfOrder.Market}
                        Else
                            If log Then OnHeartbeat(String.Format("Supertrend Color:{0}. So it will not place exit order", supertrendColor.Name))
                        End If
                    ElseIf Me.SupportTrade Then
                        If Me.OrderDetails IsNot Nothing AndAlso Me.OrderDetails.Count > 0 Then
                            For Each runningOrder In Me.OrderDetails.Values
                                If runningOrder.ParentOrder IsNot Nothing AndAlso runningOrder.ParentOrder.Status = IOrder.TypeOfStatus.Complete AndAlso
                                    runningOrder.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Sell Then
                                    Dim triggerPrice As Double = ConvertFloorCeling(runningOrder.ParentOrder.AveragePrice * MyParentInstrumentDetails.StoplossMultiplier, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                                    If currentTick.LastPrice < triggerPrice Then
                                        'parameters = New PlaceOrderParameters(runningCandlePayload.PreviousPayload) With
                                        '                {.EntryDirection = IOrder.TypeOfTransaction.Buy,
                                        '                 .Quantity = quantity,
                                        '                 .TriggerPrice = triggerPrice,
                                        '                 .OrderType = IOrder.TypeOfOrder.SL_M}

                                        Dim price As Double = ConvertFloorCeling(triggerPrice + triggerPrice * 10 / 100, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                                        parameters = New PlaceOrderParameters(runningCandlePayload.PreviousPayload) With
                                                        {.EntryDirection = IOrder.TypeOfTransaction.Buy,
                                                         .Quantity = quantity,
                                                         .TriggerPrice = triggerPrice,
                                                         .Price = price,
                                                         .OrderType = IOrder.TypeOfOrder.SL}
                                    Else
                                        parameters = New PlaceOrderParameters(runningCandlePayload.PreviousPayload) With
                                                        {.EntryDirection = IOrder.TypeOfTransaction.Buy,
                                                         .Quantity = quantity,
                                                         .OrderType = IOrder.TypeOfOrder.Market}
                                    End If
                                End If
                            Next
                        End If
                    End If
                End If
            Else
                If IsRunningInstrument() Then
                    If forcePrint Then OnHeartbeat(String.Format("***** Current Time({0}) > EOD Exit Time({1}). So it will place exit order.", currentTime.ToString("HH:mm:ss"), userSettings.EODExitTime.ToString("HH:mm:ss")))
                    parameters = New PlaceOrderParameters(runningCandlePayload.PreviousPayload) With
                            {.EntryDirection = IOrder.TypeOfTransaction.Buy,
                             .Quantity = quantity,
                             .OrderType = IOrder.TypeOfOrder.Market,
                             .Supporting = New List(Of Object) From {"EOD Exit"}}
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

    Protected Overrides Async Function IsTriggerReceivedForExitOrderAsync(forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, String)))
        Dim ret As List(Of Tuple(Of ExecuteCommandAction, IOrder, String)) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        Dim currentTick As ITick = Me.TradableInstrument.LastTick

        If currentTick IsNot Nothing AndAlso currentTick.Timestamp.Value >= Me.ParentStrategy.UserSettings.EODExitTime Then
            If Me.OrderDetails IsNot Nothing AndAlso Me.OrderDetails.Count > 0 Then
                For Each runningOrder In Me.OrderDetails.Values
                    If runningOrder.ParentOrder.Status = IOrder.TypeOfStatus.TriggerPending OrElse
                        runningOrder.ParentOrder.Status = IOrder.TypeOfStatus.Open Then
                        'Below portion have to be done in every cancel order trigger
                        Dim currentSignalActivities As ActivityDashboard = Me.ParentStrategy.SignalManager.GetSignalActivities(runningOrder.ParentOrder.Tag)
                        If currentSignalActivities IsNot Nothing Then
                            If currentSignalActivities.CancelActivity.RequestStatus = ActivityDashboard.SignalStatusType.Handled OrElse
                                currentSignalActivities.CancelActivity.RequestStatus = ActivityDashboard.SignalStatusType.Activated OrElse
                                currentSignalActivities.CancelActivity.RequestStatus = ActivityDashboard.SignalStatusType.Completed Then
                                Continue For
                            End If
                        End If
                        If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, IOrder, String))
                        ret.Add(New Tuple(Of ExecuteCommandAction, IOrder, String)(ExecuteCommandAction.Take, runningOrder.ParentOrder, "EOD Exit"))
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

    Private Function IsRunningInstrument() As Boolean
        Dim ret As Boolean = False
        If Me.OrderDetails IsNot Nothing AndAlso Me.OrderDetails.Count > 0 Then
            Dim totalTraded As Integer = 0
            For Each runningOrder In Me.OrderDetails
                If runningOrder.Value.ParentOrder IsNot Nothing Then
                    If runningOrder.Value.ParentOrder.Status <> IOrder.TypeOfStatus.Rejected AndAlso
                        runningOrder.Value.ParentOrder.Status <> IOrder.TypeOfStatus.Cancelled Then
                        If runningOrder.Value.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Buy Then
                            totalTraded += Math.Abs(runningOrder.Value.ParentOrder.Quantity)
                        ElseIf runningOrder.Value.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Sell Then
                            totalTraded -= Math.Abs(runningOrder.Value.ParentOrder.Quantity)
                        End If
                    End If
                End If
            Next

            ret = totalTraded <> 0
        End If
        Return ret
    End Function

    Private Function IsOpenInstrument() As Boolean
        Dim ret As Boolean = False
        If Me.OrderDetails IsNot Nothing AndAlso Me.OrderDetails.Count > 0 Then
            For Each runningOrder In Me.OrderDetails
                If runningOrder.Value.ParentOrder IsNot Nothing Then
                    If runningOrder.Value.ParentOrder.Status = IOrder.TypeOfStatus.Open OrElse
                        runningOrder.Value.ParentOrder.Status = IOrder.TypeOfStatus.TriggerPending Then
                        ret = True
                        Exit For
                    End If
                End If
            Next
        End If
        Return ret
    End Function

    Private Function IsTradeComplete() As Boolean
        Dim ret As Boolean = False
        If Me.OrderDetails IsNot Nothing AndAlso Me.OrderDetails.Count > 0 Then
            Dim totalTraded As Integer = 0
            Dim validTradeFound As Boolean = False
            For Each runningOrder In Me.OrderDetails
                If runningOrder.Value.ParentOrder IsNot Nothing Then
                    If runningOrder.Value.ParentOrder.Status <> IOrder.TypeOfStatus.Rejected AndAlso
                        runningOrder.Value.ParentOrder.Status <> IOrder.TypeOfStatus.Cancelled Then
                        validTradeFound = True
                        If runningOrder.Value.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Buy Then
                            totalTraded += Math.Abs(runningOrder.Value.ParentOrder.Quantity)
                        ElseIf runningOrder.Value.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Sell Then
                            totalTraded -= Math.Abs(runningOrder.Value.ParentOrder.Quantity)
                        End If
                    End If
                End If
            Next

            ret = validTradeFound AndAlso totalTraded = 0
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