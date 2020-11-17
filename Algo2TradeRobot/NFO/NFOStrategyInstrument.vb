Imports NLog
Imports System.IO
Imports Algo2TradeCore
Imports Utilities.Time
Imports System.Net.Http
Imports System.Threading
Imports Utilities.Numbers
Imports Utilities.Network
Imports Algo2TradeCore.Adapter
Imports Algo2TradeCore.Entities
Imports Algo2TradeCore.Strategies
Imports Algo2TradeCore.Entities.Indicators
Imports Algo2TradeCore.ChartHandler.ChartStyle

Public Class NFOStrategyInstrument
    Inherits StrategyInstrument
    Implements IDisposable

#Region "Logging and Status Progress"
    Public Shared Shadows logger As Logger = LogManager.GetCurrentClassLogger
#End Region

#Region "Required Class"
    <Serializable>
    Public Class SignalDetails
        Public Property SignalTime As Date
        Public Property EntryPrice As Decimal
        Public Property EntryReason As String
        Public Property Quantity As Double
        Public Property IterationNumber As Integer
    End Class
#End Region

    Private _PreProcessingDone As Boolean
    Public ReadOnly Property PreProcessingDone As Boolean
        Get
            Return _PreProcessingDone
        End Get
    End Property

    Private _tempSignal As SignalDetails = Nothing

    Private _lastTick As ITick = Nothing
    Private _entryDoneForTheDay As Boolean = False
    Private _dayStartLog As Boolean = True

    Private _lastDayHK As OHLCPayload = Nothing
    Private _lastDaySwing As Swing = Nothing
    Private _lastDayATR As Decimal = Decimal.MinValue

    Private _lastPrevPayloadString As String = ""

    Private ReadOnly _dummyHKConsumer As HeikinAshiConsumer

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
                Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
                Dim chartConsumer As PayloadToChartConsumer = New PayloadToChartConsumer(Me.ParentStrategy.UserSettings.SignalTimeFrame)
                Dim hkConsumer As HeikinAshiConsumer = New HeikinAshiConsumer(chartConsumer)
                chartConsumer.OnwardLevelConsumers = New List(Of IPayloadConsumer) From {hkConsumer}
                RawPayloadDependentConsumers.Add(chartConsumer)

                _dummyHKConsumer = New HeikinAshiConsumer(chartConsumer)
            Else
                Throw New ApplicationException(String.Format("Signal Timeframe is 0 or Nothing, does not adhere to the strategy:{0}", Me.ParentStrategy.ToString))
            End If
        End If
        _PreProcessingDone = False
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
                                candleCreator.IndicatorCreator.CalculateHeikinAshi(currentXMinute, consumer)
                            Next
                        End If
                    End If
                End If
            Next
        End If
    End Function

#Region "Pre Process"

#Region "Indicator"
    Private Sub CalculateATR(ByVal ATRPeriod As Integer, ByVal inputPayload As Dictionary(Of Date, OHLCPayload), ByRef outputPayload As Dictionary(Of Date, Decimal))
        If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
            Dim firstPayload As Boolean = True
            Dim highLow As Double = Nothing
            Dim highClose As Double = Nothing
            Dim lowClose As Double = Nothing
            Dim TR As Double = Nothing
            Dim SumTR As Double = 0.00
            Dim AvgTR As Double = 0.00
            Dim counter As Integer = 0
            outputPayload = New Dictionary(Of Date, Decimal)
            For Each runningInputPayload In inputPayload
                counter += 1
                highLow = runningInputPayload.Value.HighPrice.Value - runningInputPayload.Value.LowPrice.Value
                If firstPayload = True Then
                    TR = highLow
                    firstPayload = False
                Else
                    highClose = Math.Abs(runningInputPayload.Value.HighPrice.Value - runningInputPayload.Value.PreviousPayload.ClosePrice.Value)
                    lowClose = Math.Abs(runningInputPayload.Value.LowPrice.Value - runningInputPayload.Value.PreviousPayload.ClosePrice.Value)
                    TR = Math.Max(highLow, Math.Max(highClose, lowClose))
                End If
                SumTR = SumTR + TR
                If counter = ATRPeriod Then
                    AvgTR = SumTR / ATRPeriod
                    outputPayload.Add(runningInputPayload.Value.SnapshotDateTime, AvgTR)
                ElseIf counter > ATRPeriod Then
                    AvgTR = (outputPayload(runningInputPayload.Value.PreviousPayload.SnapshotDateTime) * (ATRPeriod - 1) + TR) / ATRPeriod
                    outputPayload.Add(runningInputPayload.Value.SnapshotDateTime, AvgTR)
                Else
                    AvgTR = SumTR / counter
                    outputPayload.Add(runningInputPayload.Value.SnapshotDateTime, AvgTR)
                End If
            Next
        End If
    End Sub

    Private Class Swing
        Public Property SwingHigh As Decimal
        Public Property SwingHighTime As Date
        Public Property SwingLow As Decimal
        Public Property SwingLowTime As Date
    End Class

    Private Sub CalculateSwingHighLow(ByVal inputPayload As Dictionary(Of Date, OHLCPayload), ByVal strict As Boolean, ByRef outputPayload As Dictionary(Of Date, Swing))
        If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
            For Each runningPayload In inputPayload.Keys
                Dim swingData As Swing = New Swing
                If strict Then
                    If inputPayload(runningPayload).PreviousPayload Is Nothing Then
                        swingData.SwingHigh = inputPayload(runningPayload).HighPrice.Value
                        swingData.SwingHighTime = inputPayload(runningPayload).SnapshotDateTime
                        swingData.SwingLow = inputPayload(runningPayload).LowPrice.Value
                        swingData.SwingLowTime = inputPayload(runningPayload).SnapshotDateTime
                    ElseIf inputPayload(runningPayload).PreviousPayload IsNot Nothing AndAlso inputPayload(runningPayload).PreviousPayload.PreviousPayload Is Nothing Then
                        If inputPayload(runningPayload).PreviousPayload.HighPrice.Value > inputPayload(runningPayload).HighPrice.Value Then
                            swingData.SwingHigh = inputPayload(runningPayload).PreviousPayload.HighPrice.Value
                            swingData.SwingHighTime = inputPayload(runningPayload).PreviousPayload.SnapshotDateTime
                        Else
                            swingData.SwingHigh = outputPayload(inputPayload(runningPayload).PreviousPayload.SnapshotDateTime).SwingHigh
                            swingData.SwingHighTime = outputPayload(inputPayload(runningPayload).PreviousPayload.SnapshotDateTime).SwingHighTime
                        End If
                        If inputPayload(runningPayload).PreviousPayload.LowPrice.Value < inputPayload(runningPayload).LowPrice.Value Then
                            swingData.SwingLow = inputPayload(runningPayload).PreviousPayload.LowPrice.Value
                            swingData.SwingLowTime = inputPayload(runningPayload).PreviousPayload.SnapshotDateTime
                        Else
                            swingData.SwingLow = outputPayload(inputPayload(runningPayload).PreviousPayload.SnapshotDateTime).SwingLow
                            swingData.SwingLowTime = outputPayload(inputPayload(runningPayload).PreviousPayload.SnapshotDateTime).SwingLowTime
                        End If
                    Else
                        If inputPayload(runningPayload).PreviousPayload.HighPrice.Value > inputPayload(runningPayload).HighPrice.Value AndAlso
                            inputPayload(runningPayload).PreviousPayload.HighPrice.Value > inputPayload(runningPayload).PreviousPayload.PreviousPayload.HighPrice.Value Then
                            swingData.SwingHigh = inputPayload(runningPayload).PreviousPayload.HighPrice.Value
                            swingData.SwingHighTime = inputPayload(runningPayload).PreviousPayload.SnapshotDateTime
                        Else
                            swingData.SwingHigh = outputPayload(inputPayload(runningPayload).PreviousPayload.SnapshotDateTime).SwingHigh
                            swingData.SwingHighTime = outputPayload(inputPayload(runningPayload).PreviousPayload.SnapshotDateTime).SwingHighTime
                        End If
                        If inputPayload(runningPayload).PreviousPayload.LowPrice.Value < inputPayload(runningPayload).LowPrice.Value AndAlso
                            inputPayload(runningPayload).PreviousPayload.LowPrice.Value < inputPayload(runningPayload).PreviousPayload.PreviousPayload.LowPrice.Value Then
                            swingData.SwingLow = inputPayload(runningPayload).PreviousPayload.LowPrice.Value
                            swingData.SwingLowTime = inputPayload(runningPayload).PreviousPayload.SnapshotDateTime
                        Else
                            swingData.SwingLow = outputPayload(inputPayload(runningPayload).PreviousPayload.SnapshotDateTime).SwingLow
                            swingData.SwingLowTime = outputPayload(inputPayload(runningPayload).PreviousPayload.SnapshotDateTime).SwingLowTime
                        End If
                    End If
                Else
                    If inputPayload(runningPayload).PreviousPayload Is Nothing Then
                        swingData.SwingHigh = inputPayload(runningPayload).HighPrice.Value
                        swingData.SwingHighTime = inputPayload(runningPayload).SnapshotDateTime
                        swingData.SwingLow = inputPayload(runningPayload).LowPrice.Value
                        swingData.SwingLowTime = inputPayload(runningPayload).SnapshotDateTime
                    ElseIf inputPayload(runningPayload).PreviousPayload IsNot Nothing AndAlso inputPayload(runningPayload).PreviousPayload.PreviousPayload Is Nothing Then
                        If inputPayload(runningPayload).PreviousPayload.HighPrice.Value >= inputPayload(runningPayload).HighPrice.Value Then
                            swingData.SwingHigh = inputPayload(runningPayload).PreviousPayload.HighPrice.Value
                            swingData.SwingHighTime = inputPayload(runningPayload).PreviousPayload.SnapshotDateTime
                        Else
                            swingData.SwingHigh = outputPayload(inputPayload(runningPayload).PreviousPayload.SnapshotDateTime).SwingHigh
                            swingData.SwingHighTime = outputPayload(inputPayload(runningPayload).PreviousPayload.SnapshotDateTime).SwingHighTime
                        End If
                        If inputPayload(runningPayload).PreviousPayload.LowPrice.Value <= inputPayload(runningPayload).LowPrice.Value Then
                            swingData.SwingLow = inputPayload(runningPayload).PreviousPayload.LowPrice.Value
                            swingData.SwingLowTime = inputPayload(runningPayload).PreviousPayload.SnapshotDateTime
                        Else
                            swingData.SwingLow = outputPayload(inputPayload(runningPayload).PreviousPayload.SnapshotDateTime).SwingLow
                            swingData.SwingLowTime = outputPayload(inputPayload(runningPayload).PreviousPayload.SnapshotDateTime).SwingLowTime
                        End If
                    Else
                        If inputPayload(runningPayload).PreviousPayload.HighPrice.Value >= inputPayload(runningPayload).HighPrice.Value AndAlso
                            inputPayload(runningPayload).PreviousPayload.HighPrice.Value >= inputPayload(runningPayload).PreviousPayload.PreviousPayload.HighPrice.Value Then
                            swingData.SwingHigh = inputPayload(runningPayload).PreviousPayload.HighPrice.Value
                            swingData.SwingHighTime = inputPayload(runningPayload).PreviousPayload.SnapshotDateTime
                        Else
                            swingData.SwingHigh = outputPayload(inputPayload(runningPayload).PreviousPayload.SnapshotDateTime).SwingHigh
                            swingData.SwingHighTime = outputPayload(inputPayload(runningPayload).PreviousPayload.SnapshotDateTime).SwingHighTime
                        End If
                        If inputPayload(runningPayload).PreviousPayload.LowPrice.Value <= inputPayload(runningPayload).LowPrice.Value AndAlso
                            inputPayload(runningPayload).PreviousPayload.LowPrice.Value <= inputPayload(runningPayload).PreviousPayload.PreviousPayload.LowPrice.Value Then
                            swingData.SwingLow = inputPayload(runningPayload).PreviousPayload.LowPrice.Value
                            swingData.SwingLowTime = inputPayload(runningPayload).PreviousPayload.SnapshotDateTime
                        Else
                            swingData.SwingLow = outputPayload(inputPayload(runningPayload).PreviousPayload.SnapshotDateTime).SwingLow
                            swingData.SwingLowTime = outputPayload(inputPayload(runningPayload).PreviousPayload.SnapshotDateTime).SwingLowTime
                        End If
                    End If
                End If

                If outputPayload Is Nothing Then outputPayload = New Dictionary(Of Date, Swing)
                outputPayload.Add(runningPayload, swingData)
            Next
        End If
    End Sub

    Private Sub ConvertToHeikenAshi(ByVal inputPayload As Dictionary(Of Date, OHLCPayload), ByRef outputPayload As Dictionary(Of Date, OHLCPayload))
        If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
            If inputPayload.Count < 30 Then Throw New ApplicationException("Can't Calculate Heikenshi Properly")

            Dim tempHAPayload As OHLCPayload = Nothing
            Dim tempPreHAPayload As OHLCPayload = Nothing
            For Each runningPayload In inputPayload
                tempHAPayload = New OHLCPayload(OHLCPayload.PayloadSource.Historical)
                tempHAPayload.PreviousPayload = tempPreHAPayload
                If tempPreHAPayload Is Nothing Then
                    tempHAPayload.OpenPrice.Value = (runningPayload.Value.OpenPrice.Value + runningPayload.Value.ClosePrice.Value) / 2
                Else
                    tempHAPayload.OpenPrice.Value = (tempPreHAPayload.OpenPrice.Value + tempPreHAPayload.ClosePrice.Value) / 2
                End If
                tempHAPayload.ClosePrice.Value = (runningPayload.Value.OpenPrice.Value + runningPayload.Value.ClosePrice.Value + runningPayload.Value.HighPrice.Value + runningPayload.Value.LowPrice.Value) / 4
                tempHAPayload.HighPrice.Value = Math.Max(runningPayload.Value.HighPrice.Value, Math.Max(tempHAPayload.OpenPrice.Value, tempHAPayload.ClosePrice.Value))
                tempHAPayload.LowPrice.Value = Math.Min(runningPayload.Value.LowPrice.Value, Math.Min(tempHAPayload.OpenPrice.Value, tempHAPayload.ClosePrice.Value))
                tempHAPayload.Volume.Value = runningPayload.Value.Volume.Value
                tempHAPayload.SnapshotDateTime = runningPayload.Value.SnapshotDateTime
                tempHAPayload.TradingSymbol = runningPayload.Value.TradingSymbol
                tempPreHAPayload = tempHAPayload
                If outputPayload Is Nothing Then outputPayload = New Dictionary(Of Date, OHLCPayload)
                outputPayload.Add(runningPayload.Key, tempHAPayload)
            Next
        End If
    End Sub
#End Region

#Region "EOD Data Fetcher"
    Private Async Function GetEODHistoricalDataAsync(ByVal instrument As IInstrument, ByVal fromDate As Date, ByVal toDate As Date) As Task(Of Dictionary(Of Date, OHLCPayload))
        Dim ret As Dictionary(Of Date, OHLCPayload) = Nothing
        Dim historicalCandlesJSONDict As Dictionary(Of String, Object) = Nothing
        _cts.Token.ThrowIfCancellationRequested()
        Dim AliceEODHistoricalURL As String = "https://ant.aliceblueonline.com/api/v1/charts?exchange={0}&token={1}&candletype=3&starttime={2}&endtime={3}&type=historical"
        Dim historicalDataURL As String = String.Format(AliceEODHistoricalURL, instrument.RawExchange, instrument.InstrumentIdentifier, DateTimeToUnix(fromDate), DateTimeToUnix(toDate))
        Dim proxyToBeUsed As HttpProxy = Nothing
        Using browser As New HttpBrowser(proxyToBeUsed, Net.DecompressionMethods.GZip, New TimeSpan(0, 1, 0), _cts)
            'AddHandler browser.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete
            'AddHandler browser.Heartbeat, AddressOf OnHeartbeat
            'AddHandler browser.WaitingFor, AddressOf OnWaitingFor
            'AddHandler browser.DocumentRetryStatus, AddressOf OnDocumentRetryStatus

            Dim headers As Dictionary(Of String, String) = New Dictionary(Of String, String)
            headers.Add("X-Authorization-Token", Me.ParentStrategy.ParentController.APIConnection.ENCToken)

            Dim l As Tuple(Of Uri, Object) = Await browser.NonPOSTRequestAsync(historicalDataURL, HttpMethod.Get, Nothing, False, headers, True, "application/json").ConfigureAwait(False)
            If l Is Nothing OrElse l.Item2 Is Nothing Then
                Throw New ApplicationException(String.Format("No response while getting eod historical data for: {0}", historicalDataURL))
            End If
            If l IsNot Nothing AndAlso l.Item2 IsNot Nothing Then
                historicalCandlesJSONDict = l.Item2
            End If

            'RemoveHandler browser.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete
            'RemoveHandler browser.Heartbeat, AddressOf OnHeartbeat
            'RemoveHandler browser.WaitingFor, AddressOf OnWaitingFor
            'RemoveHandler browser.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
        End Using
        If historicalCandlesJSONDict.ContainsKey("data") Then
            Dim historicalCandles As ArrayList = historicalCandlesJSONDict("data")
            Dim previousPayload As OHLCPayload = Nothing
            For Each historicalCandle In historicalCandles
                _cts.Token.ThrowIfCancellationRequested()
                Dim runningSnapshotTime As Date = UnixToDateTime(historicalCandle(0))

                Dim runningPayload As OHLCPayload = New OHLCPayload(OHLCPayload.PayloadSource.Historical)
                With runningPayload
                    .SnapshotDateTime = runningSnapshotTime
                    .TradingSymbol = instrument.TradingSymbol
                    .OpenPrice.Value = historicalCandle(1) / instrument.PriceDivisor
                    .HighPrice.Value = historicalCandle(2) / instrument.PriceDivisor
                    .LowPrice.Value = historicalCandle(3) / instrument.PriceDivisor
                    .ClosePrice.Value = historicalCandle(4) / instrument.PriceDivisor
                    .Volume.Value = historicalCandle(5)
                    .PreviousPayload = previousPayload
                End With
                previousPayload = runningPayload

                If ret Is Nothing Then ret = New Dictionary(Of Date, OHLCPayload)
                If Not ret.ContainsKey(runningSnapshotTime) Then ret.Add(runningSnapshotTime, runningPayload)
            Next
        End If
        Return ret
    End Function
#End Region

    Private Async Function CompletePreProcessing() As Task(Of Boolean)
        Dim ret As Boolean = False
        Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
        Dim eodPayload As Dictionary(Of Date, OHLCPayload) = Await GetEODHistoricalDataAsync(Me.TradableInstrument, Now.Date.AddYears(-1), Now.Date).ConfigureAwait(False)
        If eodPayload IsNot Nothing AndAlso eodPayload.Count > 0 Then
            Dim atrPayload As Dictionary(Of Date, Decimal) = Nothing
            CalculateATR(14, eodPayload, atrPayload)
            _lastDayATR = atrPayload.LastOrDefault.Value

            Dim swingPayload As Dictionary(Of Date, Swing) = Nothing
            CalculateSwingHighLow(eodPayload, False, swingPayload)
            _lastDaySwing = swingPayload.LastOrDefault.Value

            Dim hkPayload As Dictionary(Of Date, OHLCPayload) = Nothing
            ConvertToHeikenAshi(eodPayload, hkPayload)
            _lastDayHK = hkPayload.LastOrDefault.Value

            ret = True
        End If
        Return ret
    End Function
#End Region

#Region "Signal Process"
    Private ReadOnly _signalDetailsFilename As String = Path.Combine(My.Application.Info.DirectoryPath, String.Format("{0}.SignalDetails.a2t", Me.TradableInstrument.TradingSymbol))
    Private _lastSignalDetails As SignalDetails = Nothing
    Public Function GetSignalDetails() As SignalDetails
        If _lastSignalDetails Is Nothing Then
            If File.Exists(_signalDetailsFilename) Then
                _lastSignalDetails = Utilities.Strings.DeserializeToCollection(Of SignalDetails)(_signalDetailsFilename)

                logger.Debug("Last Signal Details-> Signal Time:{0}, Entry Price:{1}, Entry Reason:{2}, Quantity:{3}, Iteration Number:{4}, Trading Symbol:{5}",
                             _lastSignalDetails.SignalTime.ToString("dd-MM-yyyy HH:mm:ss"),
                             _lastSignalDetails.EntryPrice,
                             _lastSignalDetails.EntryReason,
                             _lastSignalDetails.Quantity,
                             _lastSignalDetails.IterationNumber,
                             Me.TradableInstrument.TradingSymbol)
            End If
        End If
        Return _lastSignalDetails
    End Function

    Public Sub SetSignalDetails(ByVal signalTime As Date, ByVal entryPrice As Decimal, ByVal entryReason As String, ByVal quantity As Long, ByVal iterationNumber As Integer)
        If _lastSignalDetails Is Nothing Then _lastSignalDetails = New SignalDetails
        _lastSignalDetails.SignalTime = signalTime
        _lastSignalDetails.EntryPrice = entryPrice
        _lastSignalDetails.EntryReason = entryReason
        _lastSignalDetails.Quantity = quantity
        _lastSignalDetails.IterationNumber = iterationNumber

        Utilities.Strings.SerializeFromCollection(Of SignalDetails)(_signalDetailsFilename, _lastSignalDetails)

        logger.Debug("Last Signal Details-> Signal Time:{0}, Entry Price:{1}, Entry Reason:{2}, Quantity:{3}, Iteration Number:{4}, Trading Symbol:{5}",
                     _lastSignalDetails.SignalTime.ToString("dd-MM-yyyy HH:mm:ss"),
                     _lastSignalDetails.EntryPrice,
                     _lastSignalDetails.EntryReason,
                     _lastSignalDetails.Quantity,
                     _lastSignalDetails.IterationNumber,
                     Me.TradableInstrument.TradingSymbol)
    End Sub
#End Region

    Public Overrides Async Function MonitorAsync() As Task
        Try
            _strategyInstrumentRunning = True
            Dim preProcess As Boolean = Await CompletePreProcessing().ConfigureAwait(False)
            If preProcess AndAlso _lastDayATR <> Decimal.MinValue AndAlso _lastDaySwing IsNot Nothing AndAlso _lastDayHK IsNot Nothing Then
                _PreProcessingDone = True

                While True
                    If Me.ParentStrategy.ParentController.OrphanException IsNot Nothing Then
                        Throw Me.ParentStrategy.ParentController.OrphanException
                    End If
                    _cts.Token.ThrowIfCancellationRequested()

                    If Me._RMSException IsNot Nothing AndAlso
                    _RMSException.ExceptionType = Algo2TradeCore.Exceptions.AdapterBusinessException.TypeOfException.RMSError Then
                        OnHeartbeat(String.Format("{0}:Will not take no more action in this instrument as RMS Error occured. Error-{1}", Me.TradableInstrument.TradingSymbol, _RMSException.Message))
                        Throw Me._RMSException
                    End If
                    _cts.Token.ThrowIfCancellationRequested()

                    Me.TradableInstrument.FetchHistorical = Not Me.TradableInstrument.IsHistoricalCompleted

                    If _tempSignal IsNot Nothing Then
                        Dim lastExecutedTrade As IBusinessOrder = GetLastExecutedOrder()
                        If lastExecutedTrade IsNot Nothing AndAlso lastExecutedTrade.ParentOrder.Status = IOrder.TypeOfStatus.Complete Then
                            SetSignalDetails(_tempSignal.SignalTime, lastExecutedTrade.ParentOrder.AveragePrice, _tempSignal.EntryReason, _tempSignal.Quantity, _tempSignal.IterationNumber)
                            _tempSignal = Nothing
                            _entryDoneForTheDay = True
                        End If
                    End If

                    'Place Order block start
                    Dim placeOrderTriggers As List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)) = Await IsTriggerReceivedForPlaceOrderAsync(False).ConfigureAwait(False)
                    If placeOrderTriggers IsNot Nothing AndAlso placeOrderTriggers.Count > 0 AndAlso
                        placeOrderTriggers.FirstOrDefault.Item1 = ExecuteCommandAction.Take Then
                        Dim orderResponse = Nothing
                        If placeOrderTriggers.FirstOrDefault.Item2.OrderType = IOrder.TypeOfOrder.Market Then
                            orderResponse = Await ExecuteCommandAsync(ExecuteCommands.PlaceRegularMarketCNCOrder, Nothing).ConfigureAwait(False)
                        Else
                            orderResponse = Await ExecuteCommandAsync(ExecuteCommands.PlaceRegularSLCNCOrder, Nothing).ConfigureAwait(False)
                        End If
                        'If orderResponse IsNot Nothing AndAlso orderResponse.Count > 0 Then
                        '    Dim placeOrderResponse = CType(orderResponse, Concurrent.ConcurrentBag(Of Object)).FirstOrDefault
                        '    If placeOrderResponse.ContainsKey("data") AndAlso
                        '        placeOrderResponse("data").ContainsKey("order_id") Then
                        '        _entryDoneForTheDay = True
                        '    End If
                        'End If
                    End If
                    'Place Order block end

                    'Exit Order block start
                    Dim exitOrderTrigger As List(Of Tuple(Of ExecuteCommandAction, IOrder, String)) = Await IsTriggerReceivedForExitOrderAsync(False).ConfigureAwait(False)
                    If exitOrderTrigger IsNot Nothing AndAlso exitOrderTrigger.Count > 0 Then
                        Await ExecuteCommandAsync(ExecuteCommands.CancelRegularOrder, Nothing).ConfigureAwait(False)
                    End If
                    'Exit Order block end

                    _cts.Token.ThrowIfCancellationRequested()
                    Await Task.Delay(1000, _cts.Token).ConfigureAwait(False)
                End While
            End If
        Catch ex As Exception
            'To log exceptions getting created from this function as the bubble up of the exception
            'will anyways happen to Strategy.MonitorAsync but it will not be shown until all tasks exit
            logger.Error("Strategy Instrument:{0} stopped, error:{1}", Me.ToString, ex.ToString)
            Throw ex
        Finally
            _strategyInstrumentRunning = False
        End Try
    End Function

    Protected Overrides Async Function IsTriggerReceivedForPlaceOrderAsync(ByVal forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)))
        Dim ret As List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
        Dim hkConsumer As HeikinAshiConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummyHKConsumer)
        Dim currentCandle As OHLCPayload = GetXMinuteCurrentCandle(userSettings.SignalTimeFrame)
        Dim currentTick As ITick = Me.TradableInstrument.LastTick
        Dim currentTime As Date = Now()
        Dim lastSignal As SignalDetails = GetSignalDetails()

        'For entry in swing logic
        If currentTime >= userSettings.TradeEntryTime AndAlso _lastTick Is Nothing AndAlso lastSignal Is Nothing Then
            _lastTick = currentTick
        End If

        Dim parameters As PlaceOrderParameters = Nothing
        If currentTime >= Me.TradableInstrument.ExchangeDetails.ExchangeStartTime AndAlso currentTime <= Me.TradableInstrument.ExchangeDetails.ExchangeEndTime AndAlso
            Me.TradableInstrument.IsHistoricalCompleted AndAlso hkConsumer.ConsumerPayloads IsNot Nothing AndAlso hkConsumer.ConsumerPayloads.Count > 0 AndAlso
            _lastDayATR <> Decimal.MinValue AndAlso _lastDaySwing IsNot Nothing AndAlso _lastDayHK IsNot Nothing AndAlso Not _entryDoneForTheDay Then
            If lastSignal Is Nothing Then 'First HK entry
                If currentCandle IsNot Nothing AndAlso currentCandle.PreviousPayload IsNot Nothing AndAlso _lastDayHK.CandleColor = Color.Green AndAlso
                    hkConsumer.ConsumerPayloads.ContainsKey(currentCandle.PreviousPayload.SnapshotDateTime) AndAlso currentTime < userSettings.TradeEntryTime Then
                    Dim hkCandle As OHLCPayload = hkConsumer.ConsumerPayloads(currentCandle.PreviousPayload.SnapshotDateTime)
                    If hkCandle IsNot Nothing AndAlso hkCandle.CandleColor = Color.Red Then
                        Dim entryPrice As Decimal = ConvertFloorCeling(hkCandle.HighPrice.Value, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                        If entryPrice = Math.Round(hkCandle.HighPrice.Value, 2) Then
                            Dim buffer As Decimal = CalculateBuffer(entryPrice, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                            entryPrice = entryPrice + buffer
                        End If
                        Dim quantity As Integer = CalculateQuantityFromInvestment(entryPrice, 1, userSettings.InitialCapital, False)
                        Dim iterationNumber As Integer = 1
                        If quantity <> 0 Then
                            Dim price As Decimal = entryPrice + ConvertFloorCeling(entryPrice * 0.3 / 100, TradableInstrument.TickSize, RoundOfType.Celing)
                            parameters = New PlaceOrderParameters(hkCandle) With
                                        {.EntryDirection = IOrder.TypeOfTransaction.Buy,
                                         .OrderType = IOrder.TypeOfOrder.SL,
                                         .TriggerPrice = entryPrice,
                                         .Price = price,
                                         .Quantity = quantity}

                            _tempSignal = New SignalDetails With
                                {
                                    .SignalTime = hkCandle.SnapshotDateTime,
                                    .EntryPrice = entryPrice,
                                    .EntryReason = String.Format("Previous Day HK Color:{0}, Signal Candle HK Color:{1}", _lastDayHK.CandleColor.Name, hkCandle.CandleColor.Name),
                                    .Quantity = parameters.Quantity,
                                    .IterationNumber = iterationNumber
                                }
                        End If
                    End If
                End If
            Else
                If _lastTick IsNot Nothing Then
                    Dim takeTrade As Boolean = _lastTick.LastPrice < _lastDaySwing.SwingLow
                    Dim reason As String = String.Format("Signal Time:{0}, Close({1})<Swing Low({2})={3}",
                                                         Now.ToString("dd-MMM-yyyy"),
                                                         _lastTick.LastPrice,
                                                         _lastDaySwing.SwingLow,
                                                         _lastTick.LastPrice < _lastDaySwing.SwingLow)

                    takeTrade = takeTrade AndAlso ((lastSignal.EntryPrice - _lastTick.LastPrice) > _lastDayATR)
                    reason = String.Format("Last Entry({0})-Close({1})>ATR({2})={3}",
                                            lastSignal.EntryPrice,
                                            _lastTick.LastPrice,
                                            Math.Round(_lastDayATR, 2),
                                            (lastSignal.EntryPrice - _lastTick.LastPrice) > _lastDayATR)

                    If takeTrade Then
                        Dim iterationNumber As Integer = lastSignal.IterationNumber + 1
                        Dim iterationCapital As Decimal = userSettings.InitialCapital
                        If iterationNumber <= userSettings.MaxMartingaleIteration Then
                            iterationNumber = userSettings.InitialCapital * Math.Pow(userSettings.MartingaleMultiplier, iterationNumber - 1)
                        End If
                        Dim quantity As Integer = CalculateQuantityFromInvestment(_lastTick.LastPrice, 1, iterationCapital, False)

                        If quantity <> 0 Then
                            parameters = New PlaceOrderParameters(currentCandle) With
                                        {.EntryDirection = IOrder.TypeOfTransaction.Buy,
                                         .OrderType = IOrder.TypeOfOrder.Market,
                                         .Quantity = quantity}

                            _tempSignal = New SignalDetails With
                                {
                                    .SignalTime = _lastDaySwing.SwingLowTime,
                                    .EntryPrice = _lastTick.LastPrice,
                                    .EntryReason = reason,
                                    .Quantity = parameters.Quantity,
                                    .IterationNumber = iterationNumber
                                }
                        End If
                    End If
                End If
            End If
        End If
        'Below portion have to be done in every place order trigger
        If parameters IsNot Nothing Then
            Try
                If forcePrint Then
                    logger.Debug("PlaceOrder-> ************************************************ {0}", Me.TradableInstrument.TradingSymbol)
                    logger.Debug("PlaceOrder Parameters-> {0},{1}", parameters.ToString, Me.TradableInstrument.TradingSymbol)
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

    Protected Overrides Async Function IsTriggerReceivedForExitOrderAsync(ByVal forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, String)))
        Dim ret As List(Of Tuple(Of ExecuteCommandAction, IOrder, String)) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        Dim lastSignal As SignalDetails = GetSignalDetails()
        If lastSignal Is Nothing Then
            Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
            Dim hkConsumer As HeikinAshiConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummyHKConsumer)
            Dim runningCandlePayload As OHLCPayload = GetXMinuteCurrentCandle(userSettings.SignalTimeFrame)
            Dim currentTick As ITick = Me.TradableInstrument.LastTick

            If runningCandlePayload IsNot Nothing AndAlso runningCandlePayload.PreviousPayload IsNot Nothing AndAlso
                Me.TradableInstrument.IsHistoricalCompleted AndAlso _lastDayHK IsNot Nothing AndAlso
                hkConsumer.ConsumerPayloads IsNot Nothing AndAlso hkConsumer.ConsumerPayloads.Count > 0 AndAlso
                hkConsumer.ConsumerPayloads.ContainsKey(runningCandlePayload.PreviousPayload.SnapshotDateTime) Then
                Dim allActiveOrders As List(Of IOrder) = GetAllActiveOrders(IOrder.TypeOfTransaction.None)
                If allActiveOrders IsNot Nothing AndAlso allActiveOrders.Count > 0 Then
                    Dim parentOrders As List(Of IOrder) = allActiveOrders.FindAll(Function(x)
                                                                                      Return x.ParentOrderIdentifier Is Nothing AndAlso
                                                                                             x.Status = IOrder.TypeOfStatus.TriggerPending
                                                                                  End Function)
                    If parentOrders IsNot Nothing AndAlso parentOrders.Count > 0 Then
                        Dim hkCandle As OHLCPayload = hkConsumer.ConsumerPayloads(runningCandlePayload.PreviousPayload.SnapshotDateTime)
                        For Each runningOrder In parentOrders
                            If runningOrder.Status = IOrder.TypeOfStatus.TriggerPending Then
                                Dim bussinessOrder As IBusinessOrder = OrderDetails(runningOrder.OrderIdentifier)
                                Dim exitTrade As Boolean = False
                                If _lastDayHK.CandleColor = Color.Red Then
                                    exitTrade = True
                                ElseIf hkCandle.CandleColor = Color.Red Then
                                    Dim entryPrice As Decimal = ConvertFloorCeling(hkCandle.HighPrice.Value, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                                    If entryPrice = Math.Round(hkCandle.HighPrice.Value, 2) Then
                                        Dim buffer As Decimal = CalculateBuffer(entryPrice, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                                        entryPrice = entryPrice + buffer
                                    End If
                                    If runningOrder.TriggerPrice <> entryPrice Then
                                        exitTrade = True
                                    End If
                                End If
                                If exitTrade Then
                                    'Below portion have to be done in every cancel order trigger
                                    Dim currentSignalActivities As ActivityDashboard = Me.ParentStrategy.SignalManager.GetSignalActivities(runningOrder.Tag)
                                    If currentSignalActivities IsNot Nothing Then
                                        If currentSignalActivities.CancelActivity.RequestStatus = ActivityDashboard.SignalStatusType.Handled OrElse
                                            currentSignalActivities.CancelActivity.RequestStatus = ActivityDashboard.SignalStatusType.Activated OrElse
                                            currentSignalActivities.CancelActivity.RequestStatus = ActivityDashboard.SignalStatusType.Completed Then
                                            Continue For
                                        End If
                                    End If
                                    If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, IOrder, String))
                                    ret.Add(New Tuple(Of ExecuteCommandAction, IOrder, String)(ExecuteCommandAction.Take, runningOrder, "Invalid signal"))

                                    If forcePrint Then
                                        '_lastCancelSignal = signal
                                        '_lastCancelOrder = bussinessOrder
                                    End If
                                End If
                            End If
                        Next
                    End If
                End If
            End If
            If forcePrint AndAlso ret IsNot Nothing AndAlso ret.Count > 0 Then
                Try
                    For Each runningOrder In ret
                        logger.Debug("***** Exit Order ***** Order ID:{0}, Reason:{1}, {2}",
                                     runningOrder.Item2.OrderIdentifier,
                                     runningOrder.Item3,
                                     Me.TradableInstrument.TradingSymbol)
                    Next
                Catch ex As Exception
                    logger.Error(ex)
                End Try
            End If
        End If
        Return ret
    End Function

#Region "Not Implemented Functions"
    Protected Overrides Function IsTriggerReceivedForModifyStoplossOrderAsync(ByVal forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)))
        Throw New NotImplementedException()
    End Function

    Protected Overrides Function IsTriggerReceivedForModifyTargetOrderAsync(forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)))
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