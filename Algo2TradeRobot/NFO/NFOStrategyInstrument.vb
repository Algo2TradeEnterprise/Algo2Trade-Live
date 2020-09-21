Imports System.Threading
Imports Algo2TradeCore.Adapter
Imports Algo2TradeCore.Entities
Imports Algo2TradeCore.Strategies
Imports Utilities.Numbers
Imports NLog
Imports Algo2TradeCore.Entities.Indicators
Imports Algo2TradeCore.ChartHandler.ChartStyle
Imports Algo2TradeCore
Imports System.IO

Public Class NFOStrategyInstrument
    Inherits StrategyInstrument
    Implements IDisposable

#Region "Logging and Status Progress"
    Public Shared Shadows logger As Logger = LogManager.GetCurrentClassLogger
#End Region

#Region "Inner Class"
    Public Class SelectionDetails
        Public Property LastDayFractalLowChanged As Boolean

        Public Property SignalCandle As OHLCPayload

        Public Property FractalHigh As Decimal
        Public Property FractalLow As Decimal
        Public Property CandleCloseBelowFractalLow As Boolean
        Public Property FractalHighGreaterThanFractalLow As Boolean

        Public Property FractalDiffernce As Decimal
        Public Property MaxFractalDiffernce As Decimal
        Public Property FractalDiffLessThanMaxFractalDiff As Boolean

        Public Property PotentialQuantityForMinimumTurnover As Long
        Public Property PLForPotentialQuantityForMinimumTurnover As Decimal
        Public Property PLForPotentialQuantityForMinimumTurnoverGratenThanZero As Boolean

        Public Property QuantityToGetRequiedTargetWithoutPreviousLoss As Long
        Public Property Turnover As Decimal
        Public Property TurnoverGreaterThanMinimumTurnover As Boolean
        Public Property TurnoverLessThanMaximumTurnover As Boolean

        Public Property CurrentDayTotalVolume As Long
        Public Property PreviousDayTotalVolume As Long
        Public Property VolumePercentage As Decimal

    End Class
#End Region

    Public ReadOnly Property StrikePrice As Decimal
    Public ReadOnly Property SelectionData As SelectionDetails

    Private _lastPrevPayloadAllDisplayLog As String = ""
    Private _lastPrevPayloadDisplayLog As String = ""
    Private _lastPrevPayloadPlaceOrder As String = ""

    Private _targetMessageSend As Boolean = False
    Private _targetReached As Boolean = False
    Private _candleClosedAboveTarget As Boolean = False
    Private _targetPrice As Decimal = Decimal.MinValue

    Private ReadOnly _dummyFractalConsumer As FractalConsumer
    Private ReadOnly _runningInstrumentFilename As String

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
                chartConsumer.OnwardLevelConsumers = New List(Of IPayloadConsumer) From {New FractalConsumer(chartConsumer)}
                RawPayloadDependentConsumers.Add(chartConsumer)

                _dummyFractalConsumer = New FractalConsumer(chartConsumer)
            Else
                Throw New ApplicationException(String.Format("Signal Timeframe is 0 or Nothing, does not adhere to the strategy:{0}", Me.ParentStrategy.ToString))
            End If
        End If

        _runningInstrumentFilename = Path.Combine(My.Application.Info.DirectoryPath, String.Format("{0} {1}.DerivedInstrument.a2t", Me.TradableInstrument.TradingSymbol, Now.ToString("yy_MM_dd")))
        _StrikePrice = Decimal.MinValue
        _SelectionData = New SelectionDetails

        If Me.TradableInstrument.InstrumentType = IInstrument.TypeOfInstrument.Options Then
            Dim lastDayOfTheMonth As Date = New Date(Me.TradableInstrument.Expiry.Value.Year, Me.TradableInstrument.Expiry.Value.Month,
                                                 Date.DaysInMonth(Me.TradableInstrument.Expiry.Value.Year, Me.TradableInstrument.Expiry.Value.Month))
            Dim lastThursDayOfTheMonth As Date = lastDayOfTheMonth
            While lastThursDayOfTheMonth.DayOfWeek <> DayOfWeek.Thursday
                lastThursDayOfTheMonth = lastThursDayOfTheMonth.AddDays(-1)
            End While
            If Me.TradableInstrument.Expiry.Value.Date = lastThursDayOfTheMonth.Date Then
                If IsNumeric(Me.TradableInstrument.TradingSymbol.Split(" ")(2).Trim) Then
                    _StrikePrice = Val(Me.TradableInstrument.TradingSymbol.Split(" ")(2).Trim)
                End If
            Else
                If IsNumeric(Me.TradableInstrument.TradingSymbol.Split(" ")(3).Trim) Then
                    _StrikePrice = Val(Me.TradableInstrument.TradingSymbol.Split(" ")(3).Trim)
                End If
            End If
        End If
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
                                candleCreator.IndicatorCreator.CalculateFractal(currentXMinute, consumer)
                            Next
                        End If
                    End If
                End If
            Next
        End If
    End Function

    Public Async Function CheckInstrumentAsync() As Task
        Try
            Dim todayDate As String = Now.ToString("yy_MM_dd")
            For Each runningFile In Directory.GetFiles(My.Application.Info.DirectoryPath, "*.DerivedInstrument.a2t")
                If Not runningFile.Contains(todayDate) Then File.Delete(runningFile)
            Next
        Catch ex As Exception
            logger.Error(ex)
        End Try

        Try
            _strategyInstrumentRunning = True
            Me.TradableInstrument.FetchHistorical = False
            If CType(Me.ParentStrategy, NFOStrategy).DerivedInstruments IsNot Nothing AndAlso
                CType(Me.ParentStrategy, NFOStrategy).DerivedInstruments.Count > 0 AndAlso
                CType(Me.ParentStrategy, NFOStrategy).DerivedInstruments.ContainsKey(Me.TradableInstrument.InstrumentIdentifier) Then
                If CType(Me.ParentStrategy, NFOStrategy).DerivedInstruments(Me.TradableInstrument.InstrumentIdentifier) IsNot Nothing AndAlso
                    CType(Me.ParentStrategy, NFOStrategy).DerivedInstruments(Me.TradableInstrument.InstrumentIdentifier).Count > 0 Then
                    Dim derivedStrategyInstruments As List(Of NFOStrategyInstrument) = Nothing
                    For Each runningInstrument In CType(Me.ParentStrategy, NFOStrategy).DerivedInstruments(Me.TradableInstrument.InstrumentIdentifier)
                        Dim runningStrategyInstrument As StrategyInstrument = Me.ParentStrategy.TradableStrategyInstruments.ToList.Find(Function(x)
                                                                                                                                            Return x.TradableInstrument.InstrumentIdentifier = runningInstrument.InstrumentIdentifier
                                                                                                                                        End Function)
                        If runningStrategyInstrument IsNot Nothing Then
                            If derivedStrategyInstruments Is Nothing Then derivedStrategyInstruments = New List(Of NFOStrategyInstrument)
                            derivedStrategyInstruments.Add(runningStrategyInstrument)
                        End If
                    Next
                    If derivedStrategyInstruments IsNot Nothing AndAlso derivedStrategyInstruments.Count > 0 Then
                        Dim instrumentsToRun As NFOStrategyInstrument = Nothing
                        If File.Exists(_runningInstrumentFilename) Then
                            Dim instrument As IInstrument = Utilities.Strings.DeserializeToCollection(Of IInstrument)(_runningInstrumentFilename)
                            If instrument IsNot Nothing Then
                                instrumentsToRun = derivedStrategyInstruments.Find(Function(x)
                                                                                       Return x.TradableInstrument.InstrumentIdentifier = instrument.InstrumentIdentifier
                                                                                   End Function)
                            End If
                        Else
                            Dim allHistoricalComplete As Boolean = False
                            While Not allHistoricalComplete
                                Dim notDone As List(Of NFOStrategyInstrument) = derivedStrategyInstruments.FindAll(Function(x)
                                                                                                                       Return Not x.TradableInstrument.IsHistoricalCompleted
                                                                                                                   End Function)
                                If notDone Is Nothing OrElse notDone.Count = 0 Then allHistoricalComplete = True

                                _cts.Token.ThrowIfCancellationRequested()
                                Await Task.Delay(5000, _cts.Token).ConfigureAwait(False)
                            End While

                            'allHistoricalComplete = True
                            If allHistoricalComplete Then
                                Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
                                While True
                                    If Me.ParentStrategy.ParentController.OrphanException IsNot Nothing Then
                                        Throw Me.ParentStrategy.ParentController.OrphanException
                                    End If
                                    _cts.Token.ThrowIfCancellationRequested()
                                    Dim tasks As New List(Of Task)()
                                    For Each tradableStrategyInstrument As NFOStrategyInstrument In derivedStrategyInstruments
                                        _cts.Token.ThrowIfCancellationRequested()
                                        tasks.Add(Task.Run(AddressOf tradableStrategyInstrument.CheckStockSelectionSignalAsync, _cts.Token))
                                    Next
                                    Await Task.WhenAll(tasks).ConfigureAwait(False)

                                    Dim spotPrice As Decimal = Me.TradableInstrument.LastTick.LastPrice
                                    Dim satisfiedStrategyInstruments As List(Of NFOStrategyInstrument) = derivedStrategyInstruments.FindAll(Function(x)
                                                                                                                                                Return x.SelectionData.Turnover <> Decimal.MinValue AndAlso
                                                                                                                                                x.SelectionData.VolumePercentage <> Decimal.MinValue AndAlso
                                                                                                                                                x.SelectionData.VolumePercentage >= userSettings.MinVolumePercentage AndAlso
                                                                                                                                                x.StrikePrice <> Decimal.MinValue AndAlso
                                                                                                                                                Math.Abs(x.StrikePrice - spotPrice) <= spotPrice * userSettings.MaxStrikeRangePercentage / 100
                                                                                                                                            End Function)



                                    If satisfiedStrategyInstruments IsNot Nothing AndAlso satisfiedStrategyInstruments.Count > 0 Then
                                        For Each runningStrategyInstrument In satisfiedStrategyInstruments.OrderBy(Function(x)
                                                                                                                       Return x.SelectionData.Turnover
                                                                                                                   End Function)
                                            _cts.Token.ThrowIfCancellationRequested()
                                            instrumentsToRun = runningStrategyInstrument
                                            Exit For
                                        Next

                                        Exit While
                                    End If
                                    _cts.Token.ThrowIfCancellationRequested()
                                    Await Task.Delay(1000, _cts.Token).ConfigureAwait(False)
                                End While

                            End If
                        End If
                        If instrumentsToRun IsNot Nothing Then
                            Utilities.Strings.SerializeFromCollection(Of IInstrument)(_runningInstrumentFilename, instrumentsToRun.TradableInstrument)

                            For Each runningStrategyInstrument In derivedStrategyInstruments
                                If runningStrategyInstrument.TradableInstrument.InstrumentIdentifier <> instrumentsToRun.TradableInstrument.InstrumentIdentifier Then
                                    runningStrategyInstrument.TradableInstrument.FetchHistorical = False
                                End If
                            Next

                            logger.Debug("********************************** Instrument Found ********************************** {0}", instrumentsToRun.TradableInstrument.TradingSymbol)
                            Await instrumentsToRun.MonitorAsync().ConfigureAwait(False)
                        End If

                    End If
                End If
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

    Public Async Function CheckStockSelectionSignalAsync() As Task
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
        Dim runningCandle As OHLCPayload = GetXMinuteCurrentCandle(userSettings.SignalTimeFrame)
        Dim fractalData As FractalConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummyFractalConsumer)
        Dim xMinutePayloadConsumer As PayloadToChartConsumer = RawPayloadDependentConsumers.FirstOrDefault
        If runningCandle IsNot Nothing AndAlso runningCandle.PreviousPayload IsNot Nothing AndAlso
            runningCandle.SnapshotDateTime >= userSettings.TradeStartTime AndAlso runningCandle.SnapshotDateTime <= userSettings.LastEntryTime AndAlso
            Me.TradableInstrument.IsHistoricalCompleted AndAlso fractalData.ConsumerPayloads IsNot Nothing AndAlso fractalData.ConsumerPayloads.Count > 0 Then
            If runningCandle.PreviousPayload.SnapshotDateTime.Date = Now.Date AndAlso fractalData.ConsumerPayloads.ContainsKey(runningCandle.PreviousPayload.SnapshotDateTime) Then
                Dim fractalTurnoverSatisfied As Tuple(Of Boolean, String) = IsFratalAndTurnoverSatisfied(fractalData, runningCandle)
                If fractalTurnoverSatisfied IsNot Nothing Then
                    Dim remark As String = fractalTurnoverSatisfied.Item2
                    If fractalTurnoverSatisfied.Item1 Then
                        Dim firstCandleOfTheDay As OHLCPayload = GetXMinuteFirstCandleOfTheDay(Me.ParentStrategy.UserSettings.SignalTimeFrame)
                        If firstCandleOfTheDay IsNot Nothing AndAlso firstCandleOfTheDay.PreviousPayload IsNot Nothing Then
                            Dim currentDaySignalCandleTime As Date = runningCandle.PreviousPayload.SnapshotDateTime
                            Dim previousDaySignalCandleTime As Date = New Date(firstCandleOfTheDay.PreviousPayload.SnapshotDateTime.Year,
                                                                           firstCandleOfTheDay.PreviousPayload.SnapshotDateTime.Month,
                                                                           firstCandleOfTheDay.PreviousPayload.SnapshotDateTime.Day,
                                                                           currentDaySignalCandleTime.Hour,
                                                                           currentDaySignalCandleTime.Minute,
                                                                           currentDaySignalCandleTime.Second)

                            If xMinutePayloadConsumer IsNot Nothing AndAlso xMinutePayloadConsumer.ConsumerPayloads IsNot Nothing AndAlso
                                xMinutePayloadConsumer.ConsumerPayloads.ContainsKey(currentDaySignalCandleTime) AndAlso
                                xMinutePayloadConsumer.ConsumerPayloads.ContainsKey(previousDaySignalCandleTime) Then
                                Dim currentDayTotalVolume As Long = CType(xMinutePayloadConsumer.ConsumerPayloads(currentDaySignalCandleTime), OHLCPayload).DailyVolume
                                Dim previousDayTotalVolume As Long = CType(xMinutePayloadConsumer.ConsumerPayloads(previousDaySignalCandleTime), OHLCPayload).DailyVolume

                                _SelectionData.CurrentDayTotalVolume = currentDayTotalVolume
                                _SelectionData.PreviousDayTotalVolume = previousDayTotalVolume
                                _SelectionData.VolumePercentage = Math.Round(currentDayTotalVolume * 100 / previousDayTotalVolume, 2)

                                remark = String.Format("{0} Volume %={1}.", remark, SelectionData.VolumePercentage)
                            End If
                        End If
                    End If
                    If Not runningCandle.PreviousPayload.ToString = _lastPrevPayloadAllDisplayLog Then
                        _lastPrevPayloadAllDisplayLog = runningCandle.PreviousPayload.ToString
                        OnHeartbeat(remark)
                    End If
                End If
            End If
        End If
    End Function

    Public Overrides Async Function MonitorAsync() As Task
        Try
            _strategyInstrumentRunning = True
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
                    Dim orderResponse = Await ExecuteCommandAsync(ExecuteCommands.PlaceRegularMarketCNCOrder, Nothing).ConfigureAwait(False)
                    If orderResponse IsNot Nothing AndAlso orderResponse.Count > 0 Then
                        Dim placeOrderResponse = CType(orderResponse, Concurrent.ConcurrentBag(Of Object)).FirstOrDefault
                        If placeOrderResponse.ContainsKey("data") AndAlso
                            placeOrderResponse("data").ContainsKey("order_id") Then
                            If placeOrderTriggers.FirstOrDefault.Item2.EntryDirection = IOrder.TypeOfTransaction.Sell Then
                                _targetReached = True
                            End If

                            If placeOrderTriggers.FirstOrDefault.Item2.Supporting IsNot Nothing AndAlso placeOrderTriggers.FirstOrDefault.Item2.Supporting.Count > 0 Then
                                _targetPrice = placeOrderTriggers.FirstOrDefault.Item2.Supporting.FirstOrDefault
                            End If

                            OnHeartbeat(String.Format("Trade Placed. Signal Candle: {0}, Direction:{1}, Quantity:{2}, Potential Target:{3}",
                                                      placeOrderTriggers.FirstOrDefault.Item2.SignalCandle.SnapshotDateTime.ToString("HH:mm:ss"),
                                                      placeOrderTriggers.FirstOrDefault.Item2.EntryDirection.ToString,
                                                      placeOrderTriggers.FirstOrDefault.Item2.Quantity,
                                                      If(_targetPrice <> Decimal.MinValue, _targetPrice, "Not Set")))
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
        Dim runningCandle As OHLCPayload = GetXMinuteCurrentCandle(userSettings.SignalTimeFrame)
        Dim fractalData As FractalConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummyFractalConsumer)
        Dim currentTick As ITick = Me.TradableInstrument.LastTick
        Dim currentTime As Date = Now()
        Dim parameters As PlaceOrderParameters = Nothing

        If _targetPrice = Decimal.MinValue AndAlso Me.TradableInstrument.IsHistoricalCompleted Then
            Dim lastExecutedOrder As IBusinessOrder = GetLastExecutedOrder()
            If lastExecutedOrder IsNot Nothing Then
                If runningCandle IsNot Nothing AndAlso runningCandle.PreviousPayload IsNot Nothing Then
                    Dim lastTradeSignalCandle As OHLCPayload = GetSignalCandleOfAnOrder(lastExecutedOrder.ParentOrderIdentifier, userSettings.SignalTimeFrame)
                    If lastTradeSignalCandle IsNot Nothing Then
                        If fractalData.ConsumerPayloads IsNot Nothing AndAlso fractalData.ConsumerPayloads.ContainsKey(lastTradeSignalCandle.SnapshotDateTime) Then
                            Dim fractal As FractalConsumer.FractalPayload = fractalData.ConsumerPayloads(lastTradeSignalCandle.SnapshotDateTime)
                            If fractal IsNot Nothing Then
                                _targetPrice = fractal.FractalHigh.Value
                            End If
                        End If

                        If _targetPrice <> Decimal.MinValue Then _candleClosedAboveTarget = IsCandleClosedAboveTarget(_targetPrice, lastTradeSignalCandle.SnapshotDateTime, runningCandle.SnapshotDateTime)
                        If GetTotalQuantityTraded() = 0 Then _targetReached = True
                    End If
                End If
            End If
        End If

        Try
            If runningCandle IsNot Nothing AndAlso runningCandle.PreviousPayload IsNot Nothing AndAlso
                Me.TradableInstrument.IsHistoricalCompleted Then
                If Not runningCandle.PreviousPayload.ToString = _lastPrevPayloadPlaceOrder Then
                    _lastPrevPayloadPlaceOrder = runningCandle.PreviousPayload.ToString
                    logger.Debug("PlaceOrder-> Potential Signal Candle is:{0}. Will check rest parameters.", runningCandle.PreviousPayload.ToString)
                    logger.Debug("PlaceOrder-> Rest all parameters: Running Candle:{0}, PayloadGeneratedBy:{1}, IsHistoricalCompleted:{2}, IsFirstTimeInformationCollected:{3}, Fractal High:{4}, Fractal Low:{5}, Target Price:{6}, Candle Closed Above Target:{7}, Target Reached:{8}, Total PL:{9}, Stock PL:{10}, Last Day Fractal Changed:{11}, Current Time:{12}, Current Tick:{13}, TradingSymbol:{14}",
                                runningCandle.SnapshotDateTime.ToString("dd-MM-yyyy HH:mm:ss"),
                                runningCandle.PayloadGeneratedBy.ToString,
                                Me.TradableInstrument.IsHistoricalCompleted,
                                Me.ParentStrategy.IsFirstTimeInformationCollected,
                                CType(fractalData.ConsumerPayloads(runningCandle.PreviousPayload.SnapshotDateTime), FractalConsumer.FractalPayload).FractalHigh.Value,
                                CType(fractalData.ConsumerPayloads(runningCandle.PreviousPayload.SnapshotDateTime), FractalConsumer.FractalPayload).FractalLow.Value,
                                If(_targetPrice = Decimal.MinValue, "Not Set", _targetPrice),
                                _candleClosedAboveTarget,
                                _targetReached,
                                Me.ParentStrategy.GetTotalPLAfterBrokerage(),
                                Me.GetOverallPLAfterBrokerage(),
                                IsLastDayFractalLowChanged(fractalData, runningCandle),
                                currentTime.ToString,
                                currentTick.LastPrice,
                                Me.TradableInstrument.TradingSymbol)
                End If
            End If
        Catch ex As Exception
            logger.Warn(ex)
        End Try

        If _targetReached AndAlso Not _targetMessageSend AndAlso GetTotalQuantityTraded() = 0 Then
            Dim lastExecutedOrder As IBusinessOrder = GetLastExecutedOrder()
            If lastExecutedOrder IsNot Nothing AndAlso lastExecutedOrder.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Sell AndAlso
                lastExecutedOrder.ParentOrder.Status = IOrder.TypeOfStatus.Complete Then
                Dim sellPrice As Decimal = lastExecutedOrder.ParentOrder.AveragePrice
                Dim pl As Decimal = GetUnrealizedPL(sellPrice)

                Dim message As String = String.Format("Target reached. PL: {0}", pl)

                Await DisplayAndSendSignalAlertAsync(message, runningCandle, True).ConfigureAwait(False)
                _targetMessageSend = True
            End If
        End If

            Dim checkOfEntryOrder As Boolean = False
        If runningCandle IsNot Nothing AndAlso runningCandle.PreviousPayload IsNot Nothing AndAlso _targetPrice <> Decimal.MinValue Then
            If Not _targetReached Then
                If Not _candleClosedAboveTarget AndAlso runningCandle.PreviousPayload.ClosePrice.Value >= _targetPrice Then
                    _candleClosedAboveTarget = True
                End If
                If _candleClosedAboveTarget AndAlso currentTick.LastPrice >= _targetPrice Then
                    Dim totalQuantity As Long = GetTotalQuantityTraded()
                    If totalQuantity > 0 Then
                        parameters = New PlaceOrderParameters(runningCandle) With
                                               {.EntryDirection = IOrder.TypeOfTransaction.Sell,
                                                .OrderType = IOrder.TypeOfOrder.Market,
                                                .Quantity = totalQuantity}
                    Else
                        checkOfEntryOrder = True
                    End If
                Else
                    checkOfEntryOrder = True
                End If
            Else
                checkOfEntryOrder = True
            End If
        Else
            checkOfEntryOrder = True
        End If
        If Not _targetReached AndAlso checkOfEntryOrder AndAlso currentTime > userSettings.EODExitTime Then
            checkOfEntryOrder = False
            Dim totalQuantity As Long = GetTotalQuantityTraded()
            If totalQuantity > 0 Then
                parameters = New PlaceOrderParameters(runningCandle) With
                                       {.EntryDirection = IOrder.TypeOfTransaction.Sell,
                                        .OrderType = IOrder.TypeOfOrder.Market,
                                        .Quantity = totalQuantity}
            End If
        End If

        If checkOfEntryOrder Then
            Dim parentStrategy As NFOStrategy = Me.ParentStrategy
            If currentTime >= userSettings.TradeStartTime AndAlso currentTime <= userSettings.LastTradeEntryTime AndAlso currentTime <= userSettings.EODExitTime AndAlso
                runningCandle IsNot Nothing AndAlso runningCandle.SnapshotDateTime >= userSettings.TradeStartTime AndAlso Not _targetReached AndAlso
                runningCandle.PayloadGeneratedBy = OHLCPayload.PayloadSource.CalculatedTick AndAlso runningCandle.PreviousPayload IsNot Nothing AndAlso
                Me.TradableInstrument.IsHistoricalCompleted AndAlso fractalData.ConsumerPayloads IsNot Nothing AndAlso fractalData.ConsumerPayloads.Count > 0 Then
                Dim signal As Tuple(Of Boolean, OHLCPayload, Integer, Decimal) = Await GetEntrySignalAsync(runningCandle, currentTick, forcePrint).ConfigureAwait(False)
                If signal IsNot Nothing AndAlso signal.Item1 Then
                    Dim signalCandle As OHLCPayload = signal.Item2
                    Dim quantity As Integer = signal.Item3

                    If signalCandle IsNot Nothing AndAlso quantity > 0 Then
                        parameters = New PlaceOrderParameters(signalCandle) With
                                        {.EntryDirection = IOrder.TypeOfTransaction.Buy,
                                         .OrderType = IOrder.TypeOfOrder.Market,
                                         .Quantity = quantity,
                                         .Supporting = New List(Of Object) From {signal.Item4}}
                    End If
                End If
            End If
        End If

        ''Below portion have to be done in every place order trigger
        If parameters IsNot Nothing Then
            Try
                If forcePrint Then
                    logger.Debug("PlaceOrder-> ************************************************ {0}", Me.TradableInstrument.TradingSymbol)
                    logger.Debug("PlaceOrder Parameters-> {0}, {1}", parameters.ToString, Me.TradableInstrument.TradingSymbol)
                End If
            Catch ex As Exception
                logger.Warn(ex)
            End Try

            Dim parametersList As List(Of PlaceOrderParameters) = New List(Of PlaceOrderParameters)
            Dim freezeQuantity As Long = Long.MaxValue
            Dim rawInstrumentName As String = Me.TradableInstrument.TradingSymbol.Split(" ")(0).Trim.ToUpper
            If CType(Me.ParentStrategy, NFOStrategy).FreezeQuantityData.ContainsKey(rawInstrumentName) Then
                freezeQuantity = CType(Me.ParentStrategy, NFOStrategy).FreezeQuantityData(rawInstrumentName)
            End If
            If freezeQuantity <> Long.MaxValue AndAlso parameters.Quantity > freezeQuantity Then
                Dim split As Integer = Math.Ceiling(parameters.Quantity / freezeQuantity)
                Dim quantityOfEachSplit As Integer = Math.Ceiling(parameters.Quantity / split)
                If quantityOfEachSplit Mod Me.TradableInstrument.LotSize <> 0 Then
                    quantityOfEachSplit = Math.Min(Math.Ceiling(quantityOfEachSplit / Me.TradableInstrument.LotSize) * Me.TradableInstrument.LotSize, freezeQuantity)
                End If
                For iteration As Integer = 1 To split
                    If iteration = split Then
                        Dim parameter As PlaceOrderParameters = New PlaceOrderParameters(parameters.SignalCandle) With
                                        {.EntryDirection = parameters.EntryDirection,
                                         .OrderType = parameters.OrderType,
                                         .Supporting = parameters.Supporting,
                                         .Quantity = parameters.Quantity - (quantityOfEachSplit * (split - 1))}
                        parametersList.Add(parameter)
                    Else
                        Dim parameter As PlaceOrderParameters = New PlaceOrderParameters(parameters.SignalCandle) With
                                        {.EntryDirection = parameters.EntryDirection,
                                         .OrderType = parameters.OrderType,
                                         .Supporting = parameters.Supporting,
                                         .Quantity = quantityOfEachSplit}
                        parametersList.Add(parameter)
                    End If
                Next
            Else
                parametersList.Add(parameters)
            End If

            If parametersList IsNot Nothing AndAlso parametersList.Count > 0 Then
                For Each runningParameter In parametersList
                    Dim currentSignalActivities As IEnumerable(Of ActivityDashboard) = Me.ParentStrategy.SignalManager.GetSignalActivities(runningParameter.SignalCandle.SnapshotDateTime, Me.TradableInstrument.InstrumentIdentifier)
                    If currentSignalActivities IsNot Nothing AndAlso currentSignalActivities.Count > 0 Then
                        Dim lastPlacedActivity As ActivityDashboard = currentSignalActivities.OrderBy(Function(x)
                                                                                                          Return x.EntryActivity.RequestTime
                                                                                                      End Function).LastOrDefault
                        If lastPlacedActivity.EntryActivity.RequestStatus = ActivityDashboard.SignalStatusType.Discarded AndAlso
                            lastPlacedActivity.EntryActivity.LastException IsNot Nothing AndAlso
                            lastPlacedActivity.EntryActivity.LastException.Message.ToUpper.Contains("TIME") Then
                            Await Task.Delay(Me.ParentStrategy.ParentController.UserInputs.BackToBackOrderCoolOffDelay * 1000, _cts.Token).ConfigureAwait(False)

                            If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                            ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.WaitAndTake, runningParameter, runningParameter.ToString))
                        End If
                    Else
                        If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                        ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.Take, runningParameter, runningParameter.ToString))
                    End If
                Next
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

    Protected Overrides Async Function ForceExitSpecificTradeAsync(order As IOrder, ByVal reason As String) As Task
        If order IsNot Nothing AndAlso Not order.Status = IOrder.TypeOfStatus.Complete AndAlso
            Not order.Status = IOrder.TypeOfStatus.Cancelled AndAlso
            Not order.Status = IOrder.TypeOfStatus.Rejected Then
            Dim cancellableOrder As New List(Of Tuple(Of ExecuteCommandAction, IOrder, String)) From
            {
                New Tuple(Of ExecuteCommandAction, IOrder, String)(ExecuteCommandAction.Take, order, reason)
            }

            Await ExecuteCommandAsync(ExecuteCommands.ForceCancelRegularOrder, cancellableOrder).ConfigureAwait(False)
        End If
    End Function

    Private Async Function GetEntrySignalAsync(ByVal runningCandle As OHLCPayload, ByVal currentTick As ITick, ByVal forcePrint As Boolean) As Task(Of Tuple(Of Boolean, OHLCPayload, Integer, Decimal))
        Dim ret As Tuple(Of Boolean, OHLCPayload, Integer, Decimal) = Nothing
        If runningCandle IsNot Nothing AndAlso runningCandle.PreviousPayload IsNot Nothing Then
            Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
            Dim fractalData As FractalConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummyFractalConsumer)

            Dim fractalTurnoverSatisfied As Tuple(Of Boolean, String) = IsFratalAndTurnoverSatisfied(fractalData, runningCandle)
            If fractalTurnoverSatisfied IsNot Nothing AndAlso fractalTurnoverSatisfied.Item1 Then
                Dim remark As String = fractalTurnoverSatisfied.Item2

                Dim signalCandle As OHLCPayload = Nothing
                Dim lastExecutedOrder As IBusinessOrder = GetLastExecutedOrder()
                If lastExecutedOrder IsNot Nothing Then
                    Dim lastOrderSignalCandle As OHLCPayload = GetSignalCandleOfAnOrder(lastExecutedOrder.ParentOrderIdentifier, userSettings.SignalTimeFrame)
                    If lastOrderSignalCandle IsNot Nothing Then
                        If lastOrderSignalCandle.SnapshotDateTime <> runningCandle.PreviousPayload.SnapshotDateTime Then
                            remark = String.Format("{0} Last Order Signal Candle({1})<>Signal Candle({2})[True] ~",
                                                   remark, lastOrderSignalCandle.SnapshotDateTime.ToString("HH:mm:ss"),
                                                   runningCandle.PreviousPayload.SnapshotDateTime.ToString("HH:mm:ss"))
                            If fractalData.ConsumerPayloads.ContainsKey(lastOrderSignalCandle.SnapshotDateTime) Then
                                Dim lastTradedFractal As FractalConsumer.FractalPayload = fractalData.ConsumerPayloads(lastOrderSignalCandle.SnapshotDateTime)
                                If lastTradedFractal.FractalLow.Value <> SelectionData.FractalLow AndAlso
                                    lastTradedFractal.FractalHigh.Value <> SelectionData.FractalHigh Then
                                    signalCandle = runningCandle.PreviousPayload
                                End If
                                remark = String.Format("{0} Last Order Fractal High({1})<>Fractal High({2})[{3}]. Last Order Fractal Low({4})<>Fractal Low({5})[{6}] ~",
                                                       remark, lastTradedFractal.FractalHigh.Value, SelectionData.FractalHigh,
                                                       lastTradedFractal.FractalHigh.Value <> SelectionData.FractalHigh,
                                                       lastTradedFractal.FractalLow.Value, SelectionData.FractalLow,
                                                       lastTradedFractal.FractalLow.Value <> SelectionData.FractalLow)
                            Else
                                remark = String.Format("{0} Last Order Fractal Not Found.", remark)
                            End If
                        Else
                            remark = String.Format("{0} Last Order Signal Candle({1})<>Signal Candle({2})[False] ~",
                                                   remark, lastOrderSignalCandle.SnapshotDateTime.ToString("HH:mm:ss"),
                                                   runningCandle.PreviousPayload.SnapshotDateTime.ToString("HH:mm:ss"))
                        End If
                    Else
                        remark = String.Format("{0} Last Order Signal Candle not found.", remark)
                    End If
                Else
                    signalCandle = SelectionData.SignalCandle
                End If

                If signalCandle IsNot Nothing Then
                    Dim entryPrice As Decimal = SelectionData.FractalLow
                    Dim targetPrice As Decimal = SelectionData.FractalHigh
                    Dim averageEntryPrice As Decimal = GetAverageEntryPrice()
                    Dim unrealizedPL As Decimal = GetUnrealizedPL(targetPrice)
                    Dim plToAchive As Decimal = userSettings.MaxProfitPerStock - unrealizedPL
                    If plToAchive > 0 Then
                        Dim quantity As Integer = CalculateQuantityFromTarget(entryPrice, targetPrice, plToAchive)

                        remark = String.Format("{0} Average Entry Price={1}, Potential Target={2}, PL To Achieve=Max Stock Profit({3}) - Unrealized PL({4})={5}>0[True] ~",
                                                remark, Math.Round(averageEntryPrice, 4), targetPrice, userSettings.MaxProfitPerStock, unrealizedPL, plToAchive)

                        remark = String.Format("{0} Potential Entry={1}, Target={2}, Quantity={3}.",
                                               remark, entryPrice, targetPrice, quantity)

                        ret = New Tuple(Of Boolean, OHLCPayload, Integer, Decimal)(True, signalCandle, quantity, targetPrice)

                        If forcePrint Then logger.Debug(remark)
                    Else
                        remark = String.Format("{0} Average Entry Price={1}, Potential Target={2}, PL To Achieve=Max Stock Profit({3}) - Unrealized PL({4})={5}>0[False].",
                                           remark, Math.Round(averageEntryPrice, 4), targetPrice, userSettings.MaxProfitPerStock, unrealizedPL, plToAchive)
                    End If
                End If

                Await DisplayAndSendSignalAlertAsync(remark, runningCandle).ConfigureAwait(False)
            End If
        End If
        Return ret
    End Function

    Private Function IsFratalAndTurnoverSatisfied(ByVal fractalData As FractalConsumer, ByVal runningCandle As OHLCPayload) As Tuple(Of Boolean, String)
        Dim ret As Tuple(Of Boolean, String) = Nothing
        Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
        Dim conditionSatisfied As Boolean = False
        Dim comment As String = Nothing

        _SelectionData.LastDayFractalLowChanged = IsLastDayFractalLowChanged(fractalData, runningCandle)
        conditionSatisfied = SelectionData.LastDayFractalLowChanged
        comment = String.Format("Last Day Fractal Low Changed[{0}] ~", SelectionData.LastDayFractalLowChanged)

        _SelectionData.SignalCandle = runningCandle.PreviousPayload
        If fractalData.ConsumerPayloads IsNot Nothing AndAlso fractalData.ConsumerPayloads.ContainsKey(SelectionData.SignalCandle.SnapshotDateTime) Then
            Dim fractal As FractalConsumer.FractalPayload = fractalData.ConsumerPayloads(SelectionData.SignalCandle.SnapshotDateTime)
            _SelectionData.FractalHigh = fractal.FractalHigh.Value
            _SelectionData.FractalLow = fractal.FractalLow.Value

            _SelectionData.CandleCloseBelowFractalLow = SelectionData.SignalCandle.ClosePrice.Value < SelectionData.FractalLow
            conditionSatisfied = conditionSatisfied AndAlso SelectionData.CandleCloseBelowFractalLow
            comment = String.Format("{0} Candle Close({1})<Fractal Low({2})[{3}] ~",
                                    comment, SelectionData.SignalCandle.ClosePrice.Value, SelectionData.FractalLow, SelectionData.CandleCloseBelowFractalLow)

            _SelectionData.FractalHighGreaterThanFractalLow = SelectionData.FractalHigh > SelectionData.FractalLow
            conditionSatisfied = conditionSatisfied AndAlso SelectionData.FractalHighGreaterThanFractalLow
            comment = String.Format("{0} Fractal High({1})>Fractal Low({2})[{3}] ~",
                                    comment, SelectionData.FractalHigh, SelectionData.FractalLow, SelectionData.FractalHighGreaterThanFractalLow)

            _SelectionData.FractalDiffernce = SelectionData.FractalHigh - SelectionData.FractalLow
            _SelectionData.MaxFractalDiffernce = Math.Round(SelectionData.FractalLow * userSettings.MaxFractalDifferencePercentage / 100, 4)
            _SelectionData.FractalDiffLessThanMaxFractalDiff = SelectionData.FractalDiffernce < SelectionData.MaxFractalDiffernce
            conditionSatisfied = conditionSatisfied AndAlso SelectionData.FractalDiffLessThanMaxFractalDiff
            comment = String.Format("{0} Fractal Differnce({1})<{2}% of Lower Fractal({3})[{4}] ~",
                                    comment, SelectionData.FractalDiffernce,
                                    userSettings.MaxFractalDifferencePercentage,
                                    SelectionData.MaxFractalDiffernce,
                                    SelectionData.FractalDiffLessThanMaxFractalDiff)

            _SelectionData.PotentialQuantityForMinimumTurnover = Math.Ceiling((userSettings.MinTurnoverPerTrade / SelectionData.FractalLow) / Me.TradableInstrument.LotSize) * Me.TradableInstrument.LotSize
            _SelectionData.PLForPotentialQuantityForMinimumTurnover = _APIAdapter.CalculatePLWithBrokerage(Me.TradableInstrument, SelectionData.FractalLow, SelectionData.FractalHigh, SelectionData.PotentialQuantityForMinimumTurnover)
            _SelectionData.PLForPotentialQuantityForMinimumTurnoverGratenThanZero = SelectionData.PLForPotentialQuantityForMinimumTurnover > 0
            conditionSatisfied = conditionSatisfied AndAlso SelectionData.PLForPotentialQuantityForMinimumTurnoverGratenThanZero
            comment = String.Format("{0} Potential Quantity for min turnover=Min Turnover({1})/Fractal Low({2})={3}, Using that PL({4}) > 0[{5}] ~",
                                    comment, userSettings.MinTurnoverPerTrade, SelectionData.FractalLow,
                                    SelectionData.PotentialQuantityForMinimumTurnover,
                                    SelectionData.PLForPotentialQuantityForMinimumTurnover,
                                    SelectionData.PLForPotentialQuantityForMinimumTurnoverGratenThanZero)


            If conditionSatisfied Then
                _SelectionData.QuantityToGetRequiedTargetWithoutPreviousLoss = CalculateQuantityFromTarget(SelectionData.FractalLow, SelectionData.FractalHigh, userSettings.MaxProfitPerStock)
                _SelectionData.Turnover = SelectionData.FractalLow * SelectionData.QuantityToGetRequiedTargetWithoutPreviousLoss
                _SelectionData.TurnoverGreaterThanMinimumTurnover = SelectionData.Turnover >= userSettings.MinTurnoverPerTrade
                _SelectionData.TurnoverLessThanMaximumTurnover = SelectionData.Turnover <= userSettings.MaxTurnoverPerTrade
                conditionSatisfied = conditionSatisfied AndAlso SelectionData.TurnoverGreaterThanMinimumTurnover AndAlso SelectionData.TurnoverLessThanMaximumTurnover
                comment = String.Format("{0} Qunatity to get required target without previous loss={1}, Turnover=Quantity({2})*Fractal Low({3})={4}, Turnover({5})>={6} And Turnover({7})<={8}[{9}].",
                                          comment, SelectionData.QuantityToGetRequiedTargetWithoutPreviousLoss,
                                          SelectionData.QuantityToGetRequiedTargetWithoutPreviousLoss, SelectionData.FractalLow,
                                          SelectionData.Turnover, SelectionData.Turnover, userSettings.MinTurnoverPerTrade,
                                          SelectionData.Turnover, userSettings.MaxTurnoverPerTrade,
                                          (SelectionData.TurnoverGreaterThanMinimumTurnover AndAlso SelectionData.TurnoverLessThanMaximumTurnover))
            End If
        Else
            conditionSatisfied = False
            comment = String.Format("{0} Signal Candle Fractal not found", comment)
        End If

        ret = New Tuple(Of Boolean, String)(conditionSatisfied, comment)

        Return ret
    End Function

    Private Function GetUnrealizedPL(ByVal targetPrice As Decimal) As Decimal
        Dim ret As Decimal = 0
        If Me.OrderDetails IsNot Nothing AndAlso Me.OrderDetails.Count > 0 Then
            For Each runningOrder In Me.OrderDetails.Values
                If runningOrder.ParentOrder IsNot Nothing AndAlso runningOrder.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Buy AndAlso
                    runningOrder.ParentOrder.Status = IOrder.TypeOfStatus.Complete Then
                    ret += _APIAdapter.CalculatePLWithBrokerage(Me.TradableInstrument, runningOrder.ParentOrder.AveragePrice, targetPrice, runningOrder.ParentOrder.Quantity)
                End If
            Next
        End If
        Return ret
    End Function

    Private Function GetAverageEntryPrice() As Decimal
        Dim ret As Decimal = 0
        If Me.OrderDetails IsNot Nothing AndAlso Me.OrderDetails.Count > 0 Then
            Dim counter As Integer = 0
            For Each runningOrder In Me.OrderDetails.Values
                If runningOrder.ParentOrder IsNot Nothing AndAlso runningOrder.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Buy AndAlso
                    runningOrder.ParentOrder.Status = IOrder.TypeOfStatus.Complete Then
                    ret += runningOrder.ParentOrder.AveragePrice
                    counter += 1
                End If
            Next
            If counter > 0 Then ret = ret / counter
        End If
        Return ret
    End Function

    Private Function GetTotalQuantityTraded() As Long
        Dim ret As Long = 0
        If Me.OrderDetails IsNot Nothing AndAlso Me.OrderDetails.Count > 0 Then
            For Each runningOrder In Me.OrderDetails.Values
                If runningOrder.ParentOrder IsNot Nothing AndAlso runningOrder.ParentOrder.Status <> IOrder.TypeOfStatus.Cancelled AndAlso
                    runningOrder.ParentOrder.Status <> IOrder.TypeOfStatus.Rejected AndAlso runningOrder.ParentOrder.Status <> IOrder.TypeOfStatus.None Then
                    If runningOrder.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Buy Then
                        ret += runningOrder.ParentOrder.Quantity
                    ElseIf runningOrder.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Sell Then
                        ret -= runningOrder.ParentOrder.Quantity
                    End If
                End If
            Next
        End If
        Return ret
    End Function

    Private _lastDayFractalLow As Decimal = Decimal.MinValue
    Private _lastDayFractalLowChanged As Boolean = False
    Private Function IsLastDayFractalLowChanged(ByVal fractaldata As FractalConsumer, ByVal runningCandle As OHLCPayload) As Boolean
        If Not _lastDayFractalLowChanged AndAlso runningCandle.PreviousPayload IsNot Nothing Then
            If _lastDayFractalLow = Decimal.MinValue Then
                Dim firstCandleOfTheDay As OHLCPayload = GetXMinuteFirstCandleOfTheDay(Me.ParentStrategy.UserSettings.SignalTimeFrame)
                If firstCandleOfTheDay IsNot Nothing AndAlso firstCandleOfTheDay.PreviousPayload IsNot Nothing Then
                    If fractaldata.ConsumerPayloads IsNot Nothing AndAlso fractaldata.ConsumerPayloads.ContainsKey(firstCandleOfTheDay.PreviousPayload.SnapshotDateTime) Then
                        Dim fractal As FractalConsumer.FractalPayload = fractaldata.ConsumerPayloads(firstCandleOfTheDay.PreviousPayload.SnapshotDateTime)
                        _lastDayFractalLow = fractal.FractalLow.Value
                    End If
                End If
            End If
            If _lastDayFractalLow <> Decimal.MinValue Then
                If Me.RawPayloadDependentConsumers IsNot Nothing AndAlso Me.RawPayloadDependentConsumers.Count > 0 Then
                    Dim XMinutePayloadConsumer As PayloadToChartConsumer = RawPayloadDependentConsumers.Find(Function(x)
                                                                                                                 If x.GetType Is GetType(PayloadToChartConsumer) Then
                                                                                                                     Return CType(x, PayloadToChartConsumer).Timeframe = Me.ParentStrategy.UserSettings.SignalTimeFrame
                                                                                                                 Else
                                                                                                                     Return Nothing
                                                                                                                 End If
                                                                                                             End Function)

                    If XMinutePayloadConsumer IsNot Nothing AndAlso XMinutePayloadConsumer.ConsumerPayloads IsNot Nothing AndAlso XMinutePayloadConsumer.ConsumerPayloads.Count > 0 Then
                        Dim currentDayPayloads As IEnumerable(Of KeyValuePair(Of Date, IPayload)) = XMinutePayloadConsumer.ConsumerPayloads.Where(Function(y)
                                                                                                                                                      Return y.Key.Date = Now.Date
                                                                                                                                                  End Function)

                        If currentDayPayloads IsNot Nothing AndAlso currentDayPayloads.Count > 0 Then
                            For Each runningPayload In currentDayPayloads.OrderBy(Function(x)
                                                                                      Return x.Key
                                                                                  End Function)
                                If runningPayload.Key <= runningCandle.PreviousPayload.SnapshotDateTime Then
                                    If fractaldata.ConsumerPayloads.ContainsKey(runningPayload.Key) Then
                                        Dim fractal As FractalConsumer.FractalPayload = fractaldata.ConsumerPayloads(runningPayload.Key)
                                        If fractal.FractalLow.Value <> _lastDayFractalLow Then
                                            _lastDayFractalLowChanged = True
                                            Exit For
                                        End If
                                    End If
                                End If
                            Next
                        End If
                    End If
                End If
            End If
        End If
        Return _lastDayFractalLowChanged
    End Function

    Private Function IsCandleClosedAboveTarget(ByVal targetPrice As Decimal, ByVal startTime As Date, ByVal endTime As Date) As Boolean
        Dim ret As Boolean = False
        If Me.RawPayloadDependentConsumers IsNot Nothing AndAlso Me.RawPayloadDependentConsumers.Count > 0 Then
            Dim XMinutePayloadConsumer As PayloadToChartConsumer = RawPayloadDependentConsumers.Find(Function(x)
                                                                                                         If x.GetType Is GetType(PayloadToChartConsumer) Then
                                                                                                             Return CType(x, PayloadToChartConsumer).Timeframe = Me.ParentStrategy.UserSettings.SignalTimeFrame
                                                                                                         Else
                                                                                                             Return Nothing
                                                                                                         End If
                                                                                                     End Function)

            If XMinutePayloadConsumer IsNot Nothing AndAlso XMinutePayloadConsumer.ConsumerPayloads IsNot Nothing AndAlso XMinutePayloadConsumer.ConsumerPayloads.Count > 0 Then
                Dim currentDayPayloads As IEnumerable(Of KeyValuePair(Of Date, IPayload)) = XMinutePayloadConsumer.ConsumerPayloads.Where(Function(y)
                                                                                                                                              Return y.Key.Date = Now.Date
                                                                                                                                          End Function)

                If currentDayPayloads IsNot Nothing AndAlso currentDayPayloads.Count > 0 Then
                    For Each runningPayload In currentDayPayloads.OrderBy(Function(x)
                                                                              Return x.Key
                                                                          End Function)
                        If runningPayload.Key > startTime AndAlso runningPayload.Key < endTime Then
                            If CType(runningPayload.Value, OHLCPayload).ClosePrice.Value >= targetPrice Then
                                ret = True
                                Exit For
                            End If
                        End If
                    Next
                End If
            End If
        End If
        Return ret
    End Function

    Private Async Function DisplayAndSendSignalAlertAsync(ByVal message As String, ByVal runningCandle As OHLCPayload, Optional ByVal forceDisplay As Boolean = False) As Task
        Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
        _cts.Token.ThrowIfCancellationRequested()
        If message IsNot Nothing AndAlso message.Trim <> "" AndAlso runningCandle IsNot Nothing AndAlso runningCandle.PreviousPayload IsNot Nothing Then
            If Not runningCandle.PreviousPayload.ToString = _lastPrevPayloadDisplayLog OrElse forceDisplay Then
                _lastPrevPayloadDisplayLog = runningCandle.PreviousPayload.ToString
                OnHeartbeat(message)
                message = String.Format("{0}: {1}", Me.TradableInstrument.TradingSymbol, message)
                SendTelegramMessageAsync(message)
            End If
        End If
    End Function

    Private Async Function SendTelegramMessageAsync(ByVal message As String) As Task
        Try
            Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
            _cts.Token.ThrowIfCancellationRequested()
            Dim userInputs As NFOUserInputs = Me.ParentStrategy.UserSettings
            If Me.ParentStrategy.ParentController.UserInputs.TelegramAPIKey IsNot Nothing AndAlso
                Not Me.ParentStrategy.ParentController.UserInputs.TelegramAPIKey.Trim = "" AndAlso
                Me.ParentStrategy.ParentController.UserInputs.TelegramChatID IsNot Nothing AndAlso
                Not Me.ParentStrategy.ParentController.UserInputs.TelegramChatID.Trim = "" Then
                Using tSender As New Utilities.Notification.Telegram(Me.ParentStrategy.ParentController.UserInputs.TelegramAPIKey.Trim, Me.ParentStrategy.ParentController.UserInputs.TelegramChatID.Trim, _cts)
                    Dim encodedString As String = Utilities.Strings.UrlEncodeString(message)
                    Await tSender.SendMessageGetAsync(encodedString).ConfigureAwait(False)
                End Using
            End If
        Catch ex As Exception
            logger.Warn(ex.ToString)
        End Try
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