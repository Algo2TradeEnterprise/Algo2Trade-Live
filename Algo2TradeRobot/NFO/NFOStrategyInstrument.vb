Imports System.Threading
Imports Algo2TradeCore.Adapter
Imports Algo2TradeCore.Entities
Imports Algo2TradeCore.Strategies
Imports Utilities.Numbers
Imports NLog
Imports Algo2TradeCore.Entities.Indicators
Imports Algo2TradeCore.ChartHandler.ChartStyle
Imports Algo2TradeCore
Imports Utilities.Network
Imports Utilities.Time
Imports System.Net.Http

Public Class NFOStrategyInstrument
    Inherits StrategyInstrument
    Implements IDisposable

#Region "Logging and Status Progress"
    Public Shared Shadows logger As Logger = LogManager.GetCurrentClassLogger
#End Region

    Private _PreProcessingDone As Boolean
    Public ReadOnly Property PreProcessingDone As Boolean
        Get
            Return _PreProcessingDone
        End Get
    End Property

    Private _lastDayMA As Decimal = Decimal.MinValue
    Private _lastDayATR As Decimal = Decimal.MinValue

    Private _lastPrevPayloadPlaceOrder As String = ""
    Private _lastMessage As String = ""
    Private _lastMessageSend As Date = Date.MinValue

    Private ReadOnly _ChartRawURL As String = "https://ant.aliceblueonline.com/ext/chart/?token={0}&id={1}&exchange={2}&symbol={3}&fullscreen=true"
    Private _ChartURL As String

    Private ReadOnly _dummyHKConsumer As HeikinAshiConsumer
    Private ReadOnly _dummyVWAPConsumer As VWAPConsumer
    Private ReadOnly _dummyEMAConsumer As EMAConsumer
    Private ReadOnly _dummyPivotConsumer As PivotsConsumer

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
                Dim vwapConsumer As VWAPConsumer = New VWAPConsumer(hkConsumer)
                vwapConsumer.OnwardLevelConsumers = New List(Of IPayloadConsumer) From {New EMAConsumer(vwapConsumer, userSettings.VWAP_EMAPeriod, TypeOfField.VWAP)}
                hkConsumer.OnwardLevelConsumers = New List(Of IPayloadConsumer) From {vwapConsumer, New PivotsConsumer(hkConsumer)}
                chartConsumer.OnwardLevelConsumers = New List(Of IPayloadConsumer) From {hkConsumer}
                RawPayloadDependentConsumers.Add(chartConsumer)

                _dummyHKConsumer = New HeikinAshiConsumer(chartConsumer)
                _dummyVWAPConsumer = New VWAPConsumer(hkConsumer)
                _dummyEMAConsumer = New EMAConsumer(vwapConsumer, userSettings.VWAP_EMAPeriod, TypeOfField.VWAP)
                _dummyPivotConsumer = New PivotsConsumer(hkConsumer)
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
                                Dim counter As Integer = 0
                                For Each runningConsumer In consumer.OnwardLevelConsumers
                                    counter += 1
                                    If counter = 1 Then
                                        candleCreator.IndicatorCreator.CalculateVWAP(currentXMinute, runningConsumer)
                                        candleCreator.IndicatorCreator.CalculateEMA(currentXMinute, runningConsumer.OnwardLevelConsumers.FirstOrDefault)
                                    ElseIf counter = 2 Then
                                        candleCreator.IndicatorCreator.CalculatePivots(currentXMinute, runningConsumer)
                                    End If
                                Next
                            Next
                        End If
                    End If
                End If
            Next
        End If
    End Function

    Public Overrides Async Function MonitorAsync() As Task
        Try
            _ChartURL = String.Format(_ChartRawURL, Me.ParentStrategy.ParentController.APIConnection.ENCToken, Me.ParentStrategy.ParentController.APIConnection.APIUser.UserId, Me.TradableInstrument.RawExchange, Me.TradableInstrument.InstrumentIdentifier)
            Dim preProcess As Boolean = Await CompletePreProcessing().ConfigureAwait(False)
            If preProcess AndAlso _lastDayATR <> Decimal.MinValue Then
                _PreProcessingDone = True

                While True
                    If Me.ParentStrategy.ParentController.OrphanException IsNot Nothing Then
                        Throw Me.ParentStrategy.ParentController.OrphanException
                    End If
                    _cts.Token.ThrowIfCancellationRequested()

                    Await CheckSignalAsync().ConfigureAwait(False)

                    _cts.Token.ThrowIfCancellationRequested()
                    Await Task.Delay(1000, _cts.Token).ConfigureAwait(False)
                End While
            End If
        Catch ex As Exception
            'To log exceptions getting created from this function as the bubble up of the exception
            'will anyways happen to Strategy.MonitorAsync but it will not be shown until all tasks exit
            logger.Error("Strategy Instrument:{0}, error:{1}", Me.ToString, ex.ToString)
            Throw ex
        End Try
    End Function

    Private Async Function CheckSignalAsync() As Task
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        Dim currentTime As Date = Now()
        Dim currentTick As ITick = Me.TradableInstrument.LastTick
        Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
        Dim instrumentData As NFOUserInputs.InstrumentDetails = userSettings.InstrumentsData.Find(Function(x)
                                                                                                      Return x.InstrumentName.ToUpper = Me.TradableInstrument.RawInstrumentName.ToUpper AndAlso
                                                                                                      x.InstrumentType.ToUpper = Me.TradableInstrument.RawExchange.ToUpper
                                                                                                  End Function)
        Dim runningCandlePayload As OHLCPayload = GetXMinuteCurrentCandle(userSettings.SignalTimeFrame)
        Dim hkData As HeikinAshiConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummyHKConsumer)
        Dim vwapData As VWAPConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummyVWAPConsumer)
        Dim emaData As EMAConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummyEMAConsumer)
        Dim pivotData As PivotsConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummyPivotConsumer)

        If currentTime >= Me.TradableInstrument.ExchangeDetails.ExchangeStartTime AndAlso currentTime <= Me.TradableInstrument.ExchangeDetails.ExchangeEndTime AndAlso
            runningCandlePayload IsNot Nothing AndAlso runningCandlePayload.SnapshotDateTime >= Me.TradableInstrument.ExchangeDetails.ExchangeStartTime AndAlso
            runningCandlePayload.PayloadGeneratedBy = OHLCPayload.PayloadSource.CalculatedTick AndAlso Me.TradableInstrument.IsHistoricalCompleted Then
            If runningCandlePayload IsNot Nothing AndAlso runningCandlePayload.PreviousPayload IsNot Nothing Then
                If Not runningCandlePayload.PreviousPayload.ToString = _lastPrevPayloadPlaceOrder Then
                    _lastPrevPayloadPlaceOrder = runningCandlePayload.PreviousPayload.ToString
                    _lastMessage = ""
                    If hkData.ConsumerPayloads IsNot Nothing AndAlso hkData.ConsumerPayloads.ContainsKey(runningCandlePayload.PreviousPayload.SnapshotDateTime) Then
                        Dim hkCandle As OHLCPayload = hkData.ConsumerPayloads(runningCandlePayload.PreviousPayload.SnapshotDateTime)
                        If vwapData.ConsumerPayloads IsNot Nothing AndAlso vwapData.ConsumerPayloads.ContainsKey(runningCandlePayload.PreviousPayload.SnapshotDateTime) Then
                            Dim vwap As VWAPConsumer.VWAPPayload = vwapData.ConsumerPayloads(runningCandlePayload.PreviousPayload.SnapshotDateTime)
                            If emaData.ConsumerPayloads IsNot Nothing AndAlso emaData.ConsumerPayloads.ContainsKey(runningCandlePayload.PreviousPayload.SnapshotDateTime) Then
                                Dim vwapEMA As EMAConsumer.EMAPayload = emaData.ConsumerPayloads(runningCandlePayload.PreviousPayload.SnapshotDateTime)
                                If pivotData.ConsumerPayloads IsNot Nothing AndAlso pivotData.ConsumerPayloads.ContainsKey(runningCandlePayload.PreviousPayload.SnapshotDateTime) Then
                                    Dim pivots As PivotsConsumer.PivotsPayload = pivotData.ConsumerPayloads(runningCandlePayload.PreviousPayload.SnapshotDateTime)
                                    If hkCandle IsNot Nothing AndAlso hkCandle.PreviousPayload IsNot Nothing Then
                                        Dim signalCandle As OHLCPayload = hkCandle
                                        Dim message As String = String.Format("{0} ->Signal Candle Time:{1}.",
                                                                              Me.TradableInstrument.TradingSymbol,
                                                                              signalCandle.SnapshotDateTime.ToString("HH:mm:ss"))

                                        If vwap.VWAP.Value > vwapEMA.EMA.Value Then 'Buy
                                            Dim takeTrade As Boolean = True
                                            message = String.Format("{0} VWAP({1})>MVWAP({2})[BUY].",
                                                                    message, Math.Round(vwap.VWAP.Value, 2), Math.Round(vwapEMA.EMA.Value, 2))

                                            takeTrade = takeTrade And (signalCandle.CandleColor = Color.Green)
                                            message = String.Format("{0} Signal Candle Color({1})=Green[{2}].",
                                                                    message, signalCandle.CandleColor.Name, signalCandle.CandleColor = Color.Green)

                                            takeTrade = takeTrade And (signalCandle.PreviousPayload.CandleColor = Color.Red)
                                            message = String.Format("{0} Previous Candle Color({1})=Red[{2}].",
                                                                    message, signalCandle.PreviousPayload.CandleColor.Name, signalCandle.PreviousPayload.CandleColor = Color.Red)

                                            takeTrade = takeTrade And (signalCandle.HighPrice.Value > signalCandle.PreviousPayload.HighPrice.Value)
                                            message = String.Format("{0} Signal Candle High({1})>Previous Candle High({2})[{3}].",
                                                                    message, Math.Round(signalCandle.HighPrice.Value, 2),
                                                                    Math.Round(signalCandle.PreviousPayload.HighPrice.Value, 2),
                                                                    signalCandle.HighPrice.Value > signalCandle.PreviousPayload.HighPrice.Value)

                                            takeTrade = takeTrade And (signalCandle.LowPrice.Value > signalCandle.PreviousPayload.LowPrice.Value)
                                            message = String.Format("{0} Signal Candle Low:({1})>Previous Candle Low({2})[{3}]",
                                                                    message, Math.Round(signalCandle.LowPrice.Value, 2),
                                                                    Math.Round(signalCandle.PreviousPayload.LowPrice.Value, 2),
                                                                    signalCandle.LowPrice.Value > signalCandle.PreviousPayload.LowPrice.Value)

                                            takeTrade = takeTrade And (signalCandle.ClosePrice.Value > vwap.VWAP.Value)
                                            message = String.Format("{0} Signal Candle Close({1})>VWAP({2})[{3}].",
                                                                    message, Math.Round(signalCandle.ClosePrice.Value, 2),
                                                                    Math.Round(vwap.VWAP.Value, 2),
                                                                    signalCandle.ClosePrice.Value > vwap.VWAP.Value)

                                            takeTrade = takeTrade And (vwap.VWAP.Value > pivots.Pivot.Value)
                                            message = String.Format("{0} VWAP({1})>Central Pivot({2})[{3}].",
                                                                    message, Math.Round(vwap.VWAP.Value, 2),
                                                                    Math.Round(pivots.Pivot.Value, 2),
                                                                    vwap.VWAP.Value > pivots.Pivot.Value)

                                            If Me.TradableInstrument.InstrumentType = IInstrument.TypeOfInstrument.Cash Then
                                                takeTrade = takeTrade And (currentTick.LastPrice > _lastDayMA)
                                                message = String.Format("{0} LTP({1})>Last Day MA({2})[{3}].",
                                                                        message, currentTick.LastPrice, Math.Round(_lastDayMA, 2), currentTick.LastPrice > _lastDayMA)
                                            End If

                                            If takeTrade Then
                                                Dim entryPrice As Decimal = ConvertFloorCeling(signalCandle.HighPrice.Value, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                                                Dim stoploss As Decimal = ConvertFloorCeling(signalCandle.PreviousPayload.LowPrice.Value, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                                                Dim slRemark As String = "Candle Low"
                                                If vwap.VWAP.Value > stoploss AndAlso vwap.VWAP.Value < entryPrice Then
                                                    stoploss = ConvertFloorCeling(vwap.VWAP.Value, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                                                    slRemark = "VWAP"
                                                End If
                                                If pivots.Pivot.Value > stoploss AndAlso pivots.Pivot.Value < entryPrice Then
                                                    stoploss = ConvertFloorCeling(pivots.Pivot.Value, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                                                    slRemark = "Central Pivot"
                                                End If
                                                If pivots.Resistance1 > stoploss AndAlso pivots.Resistance1 < entryPrice Then
                                                    stoploss = ConvertFloorCeling(pivots.Resistance1, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                                                    slRemark = "Resistance1"
                                                End If
                                                If pivots.Resistance2 > stoploss AndAlso pivots.Resistance2 < entryPrice Then
                                                    stoploss = ConvertFloorCeling(pivots.Resistance2, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                                                    slRemark = "Resistance2"
                                                End If
                                                If pivots.Resistance3 > stoploss AndAlso pivots.Resistance3 < entryPrice Then
                                                    stoploss = ConvertFloorCeling(pivots.Resistance3, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                                                    slRemark = "Resistance3"
                                                End If
                                                If pivots.Support1 > stoploss AndAlso pivots.Support1 < entryPrice Then
                                                    stoploss = ConvertFloorCeling(pivots.Support1, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                                                    slRemark = "Support1"
                                                End If
                                                If pivots.Support2 > stoploss AndAlso pivots.Support2 < entryPrice Then
                                                    stoploss = ConvertFloorCeling(pivots.Support2, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                                                    slRemark = "Support2"
                                                End If
                                                If pivots.Support3 > stoploss AndAlso pivots.Support3 < entryPrice Then
                                                    stoploss = ConvertFloorCeling(pivots.Support3, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                                                    slRemark = "Support3"
                                                End If

                                                Dim slPoint As Decimal = entryPrice - stoploss
                                                If Me.TradableInstrument.InstrumentType <> IInstrument.TypeOfInstrument.Cash AndAlso slPoint > instrumentData.Range Then
                                                    takeTrade = False
                                                End If
                                                message = String.Format("{0} Entry:{1}, Stoploss:{2}({3}), SL Point:{4}, Range:{5}.",
                                                                    message, entryPrice, stoploss, slRemark, slPoint, If(instrumentData.Range = Decimal.MinValue, "∞", instrumentData.Range))

                                                If takeTrade Then
                                                    Dim targetPoint As Decimal = ConvertFloorCeling(slPoint * userSettings.TargetMultiplier, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                                                    Dim moved As Decimal = entryPrice - currentTick.Low
                                                    Dim leftOverMovement As Decimal = _lastDayATR - moved
                                                    If targetPoint > leftOverMovement * userSettings.TargetToLeftMovementPercentage / 100 Then
                                                        takeTrade = False
                                                    End If
                                                    message = String.Format("{0} Target Point:{1}, Day Low:{2}, Moved:{3}, Last Day ATR:{4}, Movement left:{5}.",
                                                                        message, targetPoint, currentTick.Low, moved, Math.Round(_lastDayATR, 2), leftOverMovement)

                                                    If takeTrade Then
                                                        Dim quantity As Integer = Me.TradableInstrument.LotSize
                                                        If Me.TradableInstrument.InstrumentType = IInstrument.TypeOfInstrument.Cash Then
                                                            quantity = CalculateQuantityFromStoploss(entryPrice, stoploss, instrumentData.Range)
                                                        End If
                                                        message = String.Format("{0} Target:{1}, Quantity:{2}. BUY",
                                                                            message, entryPrice + targetPoint, quantity)

                                                        _lastMessage = String.Format("BUY - {0} - Entry:{1} - Stoploss:{2}({3}) - Target:{4} - Quantity:{5} - Signal Candle:{6}.{7}{8}",
                                                                                     Me.TradableInstrument.TradingSymbol,
                                                                                     entryPrice,
                                                                                     stoploss,
                                                                                     slRemark,
                                                                                     entryPrice + targetPoint,
                                                                                     quantity,
                                                                                     signalCandle.SnapshotDateTime.ToString("HH:mm:ss"),
                                                                                     vbNewLine,
                                                                                     _ChartURL)
                                                    End If
                                                End If
                                            End If
                                        ElseIf vwap.VWAP.Value < vwapEMA.EMA.Value Then 'Sell
                                            Dim takeTrade As Boolean = True
                                            message = String.Format("{0} VWAP({1})<MVWAP({2})[SELL].",
                                                                    message, Math.Round(vwap.VWAP.Value, 2), Math.Round(vwapEMA.EMA.Value, 2))

                                            takeTrade = takeTrade And (signalCandle.CandleColor = Color.Red)
                                            message = String.Format("{0} Signal Candle Color({1})=Red[{2}].",
                                                                    message, signalCandle.CandleColor.Name, signalCandle.CandleColor = Color.Red)

                                            takeTrade = takeTrade And (signalCandle.PreviousPayload.CandleColor = Color.Green)
                                            message = String.Format("{0} Previous Candle Color({1})=Green[{2}].",
                                                                    message, signalCandle.PreviousPayload.CandleColor.Name, signalCandle.PreviousPayload.CandleColor = Color.Green)

                                            takeTrade = takeTrade And (signalCandle.HighPrice.Value < signalCandle.PreviousPayload.HighPrice.Value)
                                            message = String.Format("{0} Signal Candle High({1})<Previous Candle High({2})[{3}].",
                                                                    message, Math.Round(signalCandle.HighPrice.Value, 2),
                                                                    Math.Round(signalCandle.PreviousPayload.HighPrice.Value, 2),
                                                                    signalCandle.HighPrice.Value < signalCandle.PreviousPayload.HighPrice.Value)

                                            takeTrade = takeTrade And (signalCandle.LowPrice.Value < signalCandle.PreviousPayload.LowPrice.Value)
                                            message = String.Format("{0} Signal Candle Low:({1})<Previous Candle Low({2})[{3}]",
                                                                    message, Math.Round(signalCandle.LowPrice.Value, 2),
                                                                    Math.Round(signalCandle.PreviousPayload.LowPrice.Value, 2),
                                                                    signalCandle.LowPrice.Value < signalCandle.PreviousPayload.LowPrice.Value)

                                            takeTrade = takeTrade And (signalCandle.ClosePrice.Value < vwap.VWAP.Value)
                                            message = String.Format("{0} Signal Candle Close({1})<VWAP({2})[{3}].",
                                                                    message, Math.Round(signalCandle.ClosePrice.Value, 2),
                                                                    Math.Round(vwap.VWAP.Value, 2),
                                                                    signalCandle.ClosePrice.Value < vwap.VWAP.Value)

                                            takeTrade = takeTrade And (vwap.VWAP.Value < pivots.Pivot.Value)
                                            message = String.Format("{0} VWAP({1})<Central Pivot({2})[{3}].",
                                                                    message, Math.Round(vwap.VWAP.Value, 2),
                                                                    Math.Round(pivots.Pivot.Value, 2),
                                                                    vwap.VWAP.Value < pivots.Pivot.Value)

                                            If Me.TradableInstrument.InstrumentType = IInstrument.TypeOfInstrument.Cash Then
                                                takeTrade = takeTrade And (currentTick.LastPrice < _lastDayMA)
                                                message = String.Format("{0} LTP({1})>Last Day MA({2})[{3}].",
                                                                        message, currentTick.LastPrice, Math.Round(_lastDayMA, 2), currentTick.LastPrice < _lastDayMA)
                                            End If

                                            If takeTrade Then
                                                Dim entryPrice As Decimal = ConvertFloorCeling(signalCandle.LowPrice.Value, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                                                Dim stoploss As Decimal = ConvertFloorCeling(signalCandle.PreviousPayload.HighPrice.Value, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                                                Dim slRemark As String = "Candle High"
                                                If vwap.VWAP.Value < stoploss AndAlso vwap.VWAP.Value > entryPrice Then
                                                    stoploss = ConvertFloorCeling(vwap.VWAP.Value, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                                                    slRemark = "VWAP"
                                                End If
                                                If pivots.Pivot.Value < stoploss AndAlso pivots.Pivot.Value > entryPrice Then
                                                    stoploss = ConvertFloorCeling(pivots.Pivot.Value, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                                                    slRemark = "Central Pivot"
                                                End If
                                                If pivots.Resistance1 < stoploss AndAlso pivots.Resistance1 > entryPrice Then
                                                    stoploss = ConvertFloorCeling(pivots.Resistance1, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                                                    slRemark = "Resistance1"
                                                End If
                                                If pivots.Resistance2 < stoploss AndAlso pivots.Resistance2 > entryPrice Then
                                                    stoploss = ConvertFloorCeling(pivots.Resistance2, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                                                    slRemark = "Resistance2"
                                                End If
                                                If pivots.Resistance3 < stoploss AndAlso pivots.Resistance3 > entryPrice Then
                                                    stoploss = ConvertFloorCeling(pivots.Resistance3, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                                                    slRemark = "Resistance3"
                                                End If
                                                If pivots.Support1 < stoploss AndAlso pivots.Support1 > entryPrice Then
                                                    stoploss = ConvertFloorCeling(pivots.Support1, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                                                    slRemark = "Support1"
                                                End If
                                                If pivots.Support2 < stoploss AndAlso pivots.Support2 > entryPrice Then
                                                    stoploss = ConvertFloorCeling(pivots.Support2, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                                                    slRemark = "Support2"
                                                End If
                                                If pivots.Support3 < stoploss AndAlso pivots.Support3 > entryPrice Then
                                                    stoploss = ConvertFloorCeling(pivots.Support3, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                                                    slRemark = "Support3"
                                                End If

                                                Dim slPoint As Decimal = stoploss - entryPrice
                                                If Me.TradableInstrument.InstrumentType <> IInstrument.TypeOfInstrument.Cash AndAlso slPoint > instrumentData.Range Then
                                                    takeTrade = False
                                                End If
                                                message = String.Format("{0} Entry:{1}, Stoploss:{2}({3}), SL Point:{4}, Range:{5}.",
                                                                    message, entryPrice, stoploss, slRemark, slPoint, If(instrumentData.Range = Decimal.MinValue, "∞", instrumentData.Range))

                                                If takeTrade Then
                                                    Dim targetPoint As Decimal = ConvertFloorCeling(slPoint * userSettings.TargetMultiplier, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                                                    Dim moved As Decimal = currentTick.High - entryPrice
                                                    Dim leftOverMovement As Decimal = _lastDayATR - moved
                                                    If targetPoint > leftOverMovement * userSettings.TargetToLeftMovementPercentage / 100 Then
                                                        takeTrade = False
                                                    End If
                                                    message = String.Format("{0} Target Point:{1}, Day Low:{2}, Moved:{3}, Last Day ATR:{4}, Movement left:{5}.",
                                                                        message, targetPoint, currentTick.Low, moved, Math.Round(_lastDayATR, 2), leftOverMovement)

                                                    If takeTrade Then
                                                        Dim quantity As Integer = Me.TradableInstrument.LotSize
                                                        If Me.TradableInstrument.InstrumentType = IInstrument.TypeOfInstrument.Cash Then
                                                            quantity = CalculateQuantityFromStoploss(stoploss, entryPrice, instrumentData.Range)
                                                        End If
                                                        message = String.Format("{0} Target:{1}, Quantity:{2}. SELL",
                                                                            message, entryPrice - targetPoint, quantity)

                                                        _lastMessage = String.Format("SELL - {0} - Entry:{1} - Stoploss:{2}({3}) - Target:{4} - Quantity:{5} - Signal Candle:{6}.{7}{8}",
                                                                                     Me.TradableInstrument.TradingSymbol,
                                                                                     entryPrice,
                                                                                     stoploss,
                                                                                     slRemark,
                                                                                     entryPrice - targetPoint,
                                                                                     quantity,
                                                                                     signalCandle.SnapshotDateTime.ToString("HH:mm:ss"),
                                                                                     vbNewLine,
                                                                                     _ChartURL)
                                                    End If
                                                End If
                                            End If
                                        End If
                                        If message IsNot Nothing AndAlso message.Trim <> "" Then
                                            OnHeartbeat(message)
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
            End If
        End If

        If _lastMessage IsNot Nothing AndAlso _lastMessage.Trim <> "" Then
            If _lastMessageSend = Date.MinValue Then
                _lastMessageSend = Now
                Await SendTradeAlertMessageAsync(_lastMessage).ConfigureAwait(False)
            Else
                If currentTime >= _lastMessageSend.AddSeconds(10) Then
                    _lastMessageSend = Date.MinValue
                    Await SendTradeAlertMessageAsync(_lastMessage).ConfigureAwait(False)
                    _lastMessage = ""
                End If
            End If
        End If
    End Function

    Private Async Function CompletePreProcessing() As Task(Of Boolean)
        Dim ret As Boolean = False
        Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
        Dim eodPayload As Dictionary(Of Date, OHLCPayload) = Await GetEODHistoricalDataAsync(Me.TradableInstrument, Now.Date.AddYears(-1), Now.Date).ConfigureAwait(False)
        If eodPayload IsNot Nothing AndAlso eodPayload.Count > 0 Then
            Dim atrPayload As Dictionary(Of Date, Decimal) = Nothing
            CalculateATR(userSettings.DayClose_ATRPeriod, eodPayload, atrPayload)
            _lastDayATR = atrPayload.LastOrDefault.Value

            If Me.TradableInstrument.InstrumentType = IInstrument.TypeOfInstrument.Cash Then
                Dim smaPayload As Dictionary(Of Date, Decimal) = Nothing
                CalculateSMA(userSettings.DayClose_SMAPeriod, eodPayload, smaPayload)
                _lastDayMA = smaPayload.LastOrDefault.Value
                ret = True
            Else
                ret = True
            End If
        End If
        Return ret
    End Function

    Private Async Function SendTradeAlertMessageAsync(ByVal message As String) As Task
        Try
            Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
            _cts.Token.ThrowIfCancellationRequested()
            If message.Contains("&") Then
                message = message.Replace("&", "_")
            End If
            Dim userInputs As NFOUserInputs = Me.ParentStrategy.UserSettings
            If userInputs.TelegramAPIKey IsNot Nothing AndAlso Not userInputs.TelegramAPIKey.Trim = "" AndAlso
                userInputs.TelegramChatID IsNot Nothing AndAlso Not userInputs.TelegramChatID.Trim = "" Then
                Using tSender As New Utilities.Notification.Telegram(userInputs.TelegramAPIKey.Trim, userInputs.TelegramChatID.Trim, _cts)
                    Dim encodedString As String = Utilities.Strings.URLEncodeString(message)
                    Await tSender.SendMessageGetAsync(encodedString).ConfigureAwait(False)
                End Using
            End If
        Catch ex As Exception
            logger.Warn(ex.ToString)
        End Try
    End Function

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
    Private Sub CalculateSMA(ByVal SMAPeriod As Integer, ByVal inputPayload As Dictionary(Of Date, OHLCPayload), ByRef outputPayload As Dictionary(Of Date, Decimal))
        If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
            Dim finalPriceToBeAdded As Decimal = 0
            For Each runningInputPayload In inputPayload

                'If it is less than IndicatorPeriod, we will need to take SMA of all previous prices, hence the call to GetSubPayload
                Dim previousNInputFieldPayload As List(Of KeyValuePair(Of DateTime, OHLCPayload)) = GetSubPayload(inputPayload, runningInputPayload.Key, SMAPeriod - 1, False)
                If previousNInputFieldPayload Is Nothing Then
                    finalPriceToBeAdded += runningInputPayload.Value.ClosePrice.Value
                ElseIf previousNInputFieldPayload IsNot Nothing AndAlso previousNInputFieldPayload.Count <= SMAPeriod - 1 Then 'Because the first field is handled outside
                    Dim totalOfAllPrices As Decimal = 0

                    totalOfAllPrices = runningInputPayload.Value.ClosePrice.Value
                    totalOfAllPrices += previousNInputFieldPayload.Sum(Function(s) s.Value.ClosePrice.Value)
                    finalPriceToBeAdded = totalOfAllPrices / (previousNInputFieldPayload.Count + 1)
                Else
                    Dim totalOfAllPrices As Decimal = 0
                    totalOfAllPrices = runningInputPayload.Value.ClosePrice.Value
                    totalOfAllPrices += previousNInputFieldPayload.Sum(Function(s) s.Value.ClosePrice.Value)
                    finalPriceToBeAdded = Math.Round((totalOfAllPrices / (previousNInputFieldPayload.Count + 1)), 2)
                End If
                If outputPayload Is Nothing Then outputPayload = New Dictionary(Of Date, Decimal)
                outputPayload.Add(runningInputPayload.Key, finalPriceToBeAdded)
            Next
        End If
    End Sub

    Private Function GetSubPayload(ByVal inputPayload As Dictionary(Of Date, OHLCPayload), ByVal beforeThisTime As DateTime, ByVal numberOfItemsToRetrive As Integer, ByVal includeTimePassedAsOneOftheItems As Boolean) As List(Of KeyValuePair(Of DateTime, OHLCPayload))
        Dim ret As List(Of KeyValuePair(Of DateTime, OHLCPayload)) = Nothing
        If inputPayload IsNot Nothing Then
            'Find the index of the time passed
            Dim firstIndexOfKey As Integer = -1
            Dim loopTerminatedOnCondition As Boolean = False
            For Each item In inputPayload

                firstIndexOfKey += 1
                If item.Key >= beforeThisTime Then
                    loopTerminatedOnCondition = True
                    Exit For
                End If
            Next
            If loopTerminatedOnCondition Then 'Specially useful for only 1 count of item is there
                If Not includeTimePassedAsOneOftheItems Then
                    firstIndexOfKey -= 1
                End If
            End If
            If firstIndexOfKey >= 0 Then
                Dim startIndex As Integer = Math.Max((firstIndexOfKey - numberOfItemsToRetrive) + 1, 0)
                Dim revisedNumberOfItemsToRetrieve As Integer = Math.Min(numberOfItemsToRetrive, (firstIndexOfKey - startIndex) + 1)
                Dim referencePayLoadAsList = inputPayload.ToList
                ret = referencePayLoadAsList.GetRange(startIndex, revisedNumberOfItemsToRetrieve)
            End If
        End If
        Return ret
    End Function
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

#Region "Not Implemented Functions"
    Protected Overrides Function IsTriggerReceivedForPlaceOrderAsync(ByVal forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)))
        Throw New NotImplementedException()
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