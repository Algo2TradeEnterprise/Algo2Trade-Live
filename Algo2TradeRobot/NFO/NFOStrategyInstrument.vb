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

        Public Property Iteration As Integer
        Public Property FinalMessage As String
    End Class

    Enum MessageType
        INFO = 1
        DEBUG
        ALL
    End Enum
#End Region

    Public ReadOnly Property StrikePrice As Decimal
    Public ReadOnly Property SelectionData As SelectionDetails

    Private _lastPrevPayloadPlaceOrder As String = ""
    Private _displayedLogData As Dictionary(Of Date, List(Of String)) = Nothing

    Private _targetMessageSend As Boolean = False
    Private _targetReached As Boolean = False
    Private _candleClosedAboveTarget As Boolean = False
    Private _targetPrice As Decimal = Decimal.MinValue

    Private ReadOnly _dummyFractalConsumer As FractalConsumer
    Private ReadOnly _runningInstrumentFilename As String
    Private ReadOnly _runningInstrumentLogFilename As String

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

        _SelectionData = New SelectionDetails

        _StrikePrice = Decimal.MinValue
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

        _runningInstrumentLogFilename = Path.Combine(My.Application.Info.DirectoryPath, String.Format("{0} {1}.Log.a2t", Me.TradableInstrument.TradingSymbol, Now.ToString("yy_MM_dd")))
        _displayedLogData = New Dictionary(Of Date, List(Of String))
        If File.Exists(_runningInstrumentLogFilename) Then
            _displayedLogData = Utilities.Strings.DeserializeToCollection(Of Dictionary(Of Date, List(Of String)))(_runningInstrumentLogFilename)
        End If

        _runningInstrumentFilename = Nothing
        If CType(Me.ParentStrategy, NFOStrategy).DerivedInstruments IsNot Nothing AndAlso CType(Me.ParentStrategy, NFOStrategy).DerivedInstruments.Count > 0 Then
            For Each runningController In CType(Me.ParentStrategy, NFOStrategy).DerivedInstruments
                If runningController.Key.ToUpper = Me.TradableInstrument.InstrumentIdentifier.ToUpper Then
                    _runningInstrumentFilename = Path.Combine(My.Application.Info.DirectoryPath, String.Format("{0} {1}.DerivedInstrument.txt", runningController.Key, Now.ToString("yy_MM_dd")))
                Else
                    If runningController.Value IsNot Nothing AndAlso runningController.Value.Count > 0 Then
                        For Each runningInstrument In runningController.Value
                            If runningInstrument.InstrumentIdentifier.ToUpper = Me.TradableInstrument.InstrumentIdentifier Then
                                _runningInstrumentFilename = Path.Combine(My.Application.Info.DirectoryPath, String.Format("{0} {1}.DerivedInstrument.txt", runningController.Key, Now.ToString("yy_MM_dd")))
                                Exit For
                            End If
                        Next
                    End If
                End If
                If _runningInstrumentFilename IsNot Nothing Then Exit For
            Next
        End If
        If _runningInstrumentFilename Is Nothing Then
            Throw New ApplicationException(String.Format("{0}: Unable to find my controller.", Me.TradableInstrument.TradingSymbol))
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
            _strategyInstrumentRunning = True
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
                        While True
                            _cts.Token.ThrowIfCancellationRequested()
                            Dim instrumentsToRun As NFOStrategyInstrument = Nothing
                            If File.Exists(_runningInstrumentFilename) Then
                                Dim instrument As String = File.ReadAllText(_runningInstrumentFilename)
                                If instrument IsNot Nothing AndAlso instrument.Trim <> "" Then
                                    instrumentsToRun = derivedStrategyInstruments.Find(Function(x)
                                                                                           Return x.TradableInstrument.InstrumentIdentifier = instrument.Trim
                                                                                       End Function)
                                End If
                            Else
                                Dim allHistoricalComplete As Boolean = False
                                While Not allHistoricalComplete
                                    Dim notDone As List(Of NFOStrategyInstrument) = derivedStrategyInstruments.FindAll(Function(x)
                                                                                                                           Return Not x.TradableInstrument.IsHistoricalCompleted
                                                                                                                       End Function)
                                    If notDone Is Nothing OrElse notDone.Count = 0 Then
                                        allHistoricalComplete = Me.TradableInstrument.IsHistoricalCompleted
                                    End If

                                    _cts.Token.ThrowIfCancellationRequested()
                                    Await Task.Delay(5000, _cts.Token).ConfigureAwait(False)
                                End While

                                Await DisplayAndSendSignalAlertAsync(Now, "All Historical complete", MessageType.INFO, False, False).ConfigureAwait(False)
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
                                        Dim runningCandle As OHLCPayload = GetXMinuteCurrentCandle(userSettings.SignalTimeFrame)
                                        If runningCandle IsNot Nothing AndAlso runningCandle.PreviousPayload IsNot Nothing Then
                                            spotPrice = runningCandle.PreviousPayload.ClosePrice.Value
                                        End If
                                        Dim satisfiedStrategyInstruments As List(Of NFOStrategyInstrument) = derivedStrategyInstruments.FindAll(Function(x)
                                                                                                                                                    Return x.SelectionData.Turnover <> Decimal.MinValue AndAlso
                                                                                                                                                    x.SelectionData.VolumePercentage <> Decimal.MinValue AndAlso
                                                                                                                                                    x.SelectionData.Turnover > 0 AndAlso
                                                                                                                                                    x.SelectionData.VolumePercentage >= userSettings.MinVolumePercentageTillSignalTime AndAlso
                                                                                                                                                    x.StrikePrice <> Decimal.MinValue AndAlso
                                                                                                                                                    (Math.Abs(x.StrikePrice - spotPrice) / spotPrice) * 100 <= userSettings.MaxStrikeRangePercentage
                                                                                                                                                End Function)

                                        If satisfiedStrategyInstruments IsNot Nothing AndAlso satisfiedStrategyInstruments.Count > 0 Then
                                            For Each runningStrategyInstrument In satisfiedStrategyInstruments.OrderBy(Function(x)
                                                                                                                           Return x.SelectionData.Turnover
                                                                                                                       End Function).ThenByDescending(Function(y)
                                                                                                                                                          Return y.SelectionData.VolumePercentage
                                                                                                                                                      End Function)
                                                _cts.Token.ThrowIfCancellationRequested()
                                                instrumentsToRun = runningStrategyInstrument
                                                Exit For
                                            Next
                                        End If

                                        Dim maxSignalCandleTime As Date = derivedStrategyInstruments.Max(Function(x)
                                                                                                             If x.SelectionData.SignalCandle IsNot Nothing Then
                                                                                                                 Return x.SelectionData.SignalCandle.SnapshotDateTime
                                                                                                             Else
                                                                                                                 Return Date.MinValue
                                                                                                             End If
                                                                                                         End Function)
                                        If maxSignalCandleTime <> Date.MinValue AndAlso maxSignalCandleTime.Date = Now.Date Then
                                            Dim htmlString As String = String.Format("<html>{0}<head>{0}<style>", vbNewLine)

                                            Dim styleString As String = "table, th, td {  border: 1px solid black;  border-collapse: collapse;}"
                                            htmlString = String.Format("{0}{1}{2}{1}</head>", htmlString, vbNewLine, styleString)

                                            htmlString = String.Format("{0}{1}</style>{1}</head>", htmlString, vbNewLine)
                                            htmlString = String.Format("{0}{1}{1}<body>{1}<table>", htmlString, vbNewLine)

                                            'Header
                                            htmlString = String.Format("{0}{1}<tr>", htmlString, vbNewLine)
                                            htmlString = String.Format("{0}{1}<th>Trading Symbol</th>", htmlString, vbNewLine)
                                            htmlString = String.Format("{0}{1}<th>Reason</th>", htmlString, vbNewLine)
                                            htmlString = String.Format("{0}{1}<th>Strike%</th>", htmlString, vbNewLine)
                                            htmlString = String.Format("{0}{1}<th>Iteration</th>", htmlString, vbNewLine)
                                            htmlString = String.Format("{0}{1}<th>Turnover</th>", htmlString, vbNewLine)
                                            htmlString = String.Format("{0}{1}<th>Volume%</th>", htmlString, vbNewLine)
                                            htmlString = String.Format("{0}{1}</tr>", htmlString, vbNewLine)

                                            Dim addedInstrument As List(Of String) = New List(Of String)
                                            If instrumentsToRun IsNot Nothing Then
                                                addedInstrument.Add(instrumentsToRun.TradableInstrument.InstrumentIdentifier)

                                                Dim tradingSymbol As String = instrumentsToRun.TradableInstrument.TradingSymbol
                                                Dim strikePriceRangePer As Decimal = Math.Round((Math.Abs(instrumentsToRun.StrikePrice - spotPrice) / spotPrice) * 100, 2)
                                                Dim iteration As Integer = instrumentsToRun.SelectionData.Iteration
                                                Dim turnover As String = If(instrumentsToRun.SelectionData.Turnover <> Decimal.MinValue, instrumentsToRun.SelectionData.Turnover, "N/A")
                                                Dim volumePer As String = If(instrumentsToRun.SelectionData.Turnover <> Decimal.MinValue, instrumentsToRun.SelectionData.VolumePercentage, "N/A")
                                                Dim finalReason As String = "Shortlisted and Selected"

                                                htmlString = String.Format("{0}{1}<tr>", htmlString, vbNewLine)
                                                htmlString = String.Format("{0}{1}<td>{2}</td>", htmlString, vbNewLine, tradingSymbol)
                                                htmlString = String.Format("{0}{1}<td><b><i>{2}</i></b></td>", htmlString, vbNewLine, finalReason)
                                                htmlString = String.Format("{0}{1}<td>{2}</td>", htmlString, vbNewLine, strikePriceRangePer)
                                                htmlString = String.Format("{0}{1}<td>{2}</td>", htmlString, vbNewLine, iteration)
                                                htmlString = String.Format("{0}{1}<td>{2}</td>", htmlString, vbNewLine, turnover)
                                                htmlString = String.Format("{0}{1}<td>{2}</td>", htmlString, vbNewLine, volumePer)
                                                htmlString = String.Format("{0}{1}</tr>", htmlString, vbNewLine)
                                            End If
                                            If satisfiedStrategyInstruments IsNot Nothing AndAlso satisfiedStrategyInstruments.Count > 0 Then
                                                For Each runningInstrument In satisfiedStrategyInstruments.OrderBy(Function(x)
                                                                                                                       Return x.SelectionData.Turnover
                                                                                                                   End Function).ThenByDescending(Function(y)
                                                                                                                                                      Return y.SelectionData.VolumePercentage
                                                                                                                                                  End Function)
                                                    If Not addedInstrument.Contains(runningInstrument.TradableInstrument.InstrumentIdentifier) Then
                                                        addedInstrument.Add(runningInstrument.TradableInstrument.InstrumentIdentifier)

                                                        Dim tradingSymbol As String = runningInstrument.TradableInstrument.TradingSymbol
                                                        Dim strikePriceRangePer As Decimal = Math.Round((Math.Abs(runningInstrument.StrikePrice - spotPrice) / spotPrice) * 100, 2)
                                                        Dim iteration As Integer = runningInstrument.SelectionData.Iteration
                                                        Dim turnover As String = If(runningInstrument.SelectionData.Turnover <> Decimal.MinValue, runningInstrument.SelectionData.Turnover, "N/A")
                                                        Dim volumePer As String = If(runningInstrument.SelectionData.Turnover <> Decimal.MinValue, runningInstrument.SelectionData.VolumePercentage, "N/A")
                                                        Dim finalReason As String = "Shortlisted"

                                                        htmlString = String.Format("{0}{1}<tr>", htmlString, vbNewLine)
                                                        htmlString = String.Format("{0}{1}<td>{2}</td>", htmlString, vbNewLine, tradingSymbol)
                                                        htmlString = String.Format("{0}{1}<td><i>{2}</i></td>", htmlString, vbNewLine, finalReason)
                                                        htmlString = String.Format("{0}{1}<td>{2}</td>", htmlString, vbNewLine, strikePriceRangePer)
                                                        htmlString = String.Format("{0}{1}<td>{2}</td>", htmlString, vbNewLine, iteration)
                                                        htmlString = String.Format("{0}{1}<td>{2}</td>", htmlString, vbNewLine, turnover)
                                                        htmlString = String.Format("{0}{1}<td>{2}</td>", htmlString, vbNewLine, volumePer)
                                                        htmlString = String.Format("{0}{1}</tr>", htmlString, vbNewLine)
                                                    End If
                                                Next
                                            End If
                                            For Each runningInstrument In derivedStrategyInstruments
                                                If Not addedInstrument.Contains(runningInstrument.TradableInstrument.InstrumentIdentifier) Then
                                                    addedInstrument.Add(runningInstrument.TradableInstrument.InstrumentIdentifier)

                                                    Dim tradingSymbol As String = runningInstrument.TradableInstrument.TradingSymbol
                                                    Dim strikePriceRangePer As Decimal = Math.Round((Math.Abs(runningInstrument.StrikePrice - spotPrice) / spotPrice) * 100, 2)
                                                    Dim iteration As Integer = runningInstrument.SelectionData.Iteration
                                                    Dim turnover As String = If(runningInstrument.SelectionData.Turnover <> Decimal.MinValue, runningInstrument.SelectionData.Turnover, "N/A")
                                                    Dim volumePer As String = If(runningInstrument.SelectionData.Turnover <> Decimal.MinValue, runningInstrument.SelectionData.VolumePercentage, "N/A")
                                                    Dim finalReason As String = runningInstrument.SelectionData.FinalMessage
                                                    If runningInstrument.SelectionData.SignalCandle IsNot Nothing Then
                                                        If runningInstrument.SelectionData.SignalCandle.SnapshotDateTime < maxSignalCandleTime Then
                                                            finalReason = "Tick not received"
                                                            iteration = 0
                                                            turnover = 0
                                                            volumePer = 0
                                                        End If
                                                    Else
                                                        finalReason = "Tick not received"
                                                        iteration = 0
                                                        turnover = 0
                                                        volumePer = 0
                                                    End If
                                                    If strikePriceRangePer > userSettings.MaxStrikeRangePercentage Then
                                                        finalReason = "Outside max strike range"
                                                    End If

                                                    htmlString = String.Format("{0}{1}<tr>", htmlString, vbNewLine)
                                                    htmlString = String.Format("{0}{1}<td>{2}</td>", htmlString, vbNewLine, tradingSymbol)
                                                    htmlString = String.Format("{0}{1}<td>{2}</td>", htmlString, vbNewLine, finalReason)
                                                    htmlString = String.Format("{0}{1}<td>{2}</td>", htmlString, vbNewLine, strikePriceRangePer)
                                                    htmlString = String.Format("{0}{1}<td>{2}</td>", htmlString, vbNewLine, iteration)
                                                    htmlString = String.Format("{0}{1}<td>{2}</td>", htmlString, vbNewLine, turnover)
                                                    htmlString = String.Format("{0}{1}<td>{2}</td>", htmlString, vbNewLine, volumePer)
                                                    htmlString = String.Format("{0}{1}</tr>", htmlString, vbNewLine)
                                                End If
                                            Next
                                            htmlString = String.Format("{0}{1}</table>{1}{1}</body>{1}</html>", htmlString, vbNewLine)

                                            Await DisplayAndSendSignalAlertAsync(maxSignalCandleTime, htmlString, MessageType.INFO, True, True)
                                        End If

                                        If instrumentsToRun IsNot Nothing Then Exit While

                                        _cts.Token.ThrowIfCancellationRequested()
                                        Await Task.Delay(1000, _cts.Token).ConfigureAwait(False)
                                    End While
                                End If
                            End If
                            If instrumentsToRun IsNot Nothing Then
                                Dim infoMessage As String = String.Format("#Potential_Instrument_Found {0}. Will Check rest of the conditions.",
                                                                           instrumentsToRun.TradableInstrument.TradingSymbol)
                                Await DisplayAndSendSignalAlertAsync(Now, infoMessage, MessageType.INFO, True, False).ConfigureAwait(False)

                                For Each runningStrategyInstrument In derivedStrategyInstruments
                                    If runningStrategyInstrument.TradableInstrument.InstrumentIdentifier <> instrumentsToRun.TradableInstrument.InstrumentIdentifier Then
                                        runningStrategyInstrument.TradableInstrument.FetchHistorical = False
                                    End If
                                Next

                                Await instrumentsToRun.MonitorAsync().ConfigureAwait(False)

                                If Not File.Exists(_runningInstrumentFilename) Then
                                    For Each runningStrategyInstrument In derivedStrategyInstruments
                                        runningStrategyInstrument.TradableInstrument.FetchHistorical = True
                                    Next
                                Else
                                    Throw New ApplicationException("Serilized file available but it is out of monitor async loop")
                                End If
                            Else
                                Throw New ApplicationException("No instrument found")
                            End If
                        End While
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
            runningCandle.SnapshotDateTime >= userSettings.TradeStartTime AndAlso runningCandle.SnapshotDateTime <= userSettings.LastOptionCheckTime AndAlso
            Me.TradableInstrument.IsHistoricalCompleted AndAlso fractalData.ConsumerPayloads IsNot Nothing AndAlso fractalData.ConsumerPayloads.Count > 0 Then
            If runningCandle.PreviousPayload.SnapshotDateTime.Date = Now.Date AndAlso fractalData.ConsumerPayloads.ContainsKey(runningCandle.PreviousPayload.SnapshotDateTime) Then
                Dim fractalTurnoverSatisfied As Tuple(Of Boolean, String) = IsFratalAndTurnoverSatisfied(fractalData, runningCandle, "From 'Controller'")
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

                                If SelectionData.VolumePercentage >= userSettings.MinVolumePercentageTillSignalTime Then
                                    remark = String.Format("{0} Volume %({1}) >= {2}[True]{3}{3}",
                                                           remark, SelectionData.VolumePercentage,
                                                           userSettings.MinVolumePercentageTillSignalTime, vbNewLine)
                                Else
                                    remark = String.Format("{0} Volume %({1}) >= {2}[False]{3}{3}",
                                                           remark, SelectionData.VolumePercentage,
                                                           userSettings.MinVolumePercentageTillSignalTime, vbNewLine)

                                    _SelectionData.FinalMessage = String.Format("Volume % till signal time not satisfied")
                                End If
                            End If
                        End If
                    End If

                    Await DisplayAndSendSignalAlertAsync(_SelectionData.SignalCandle.SnapshotDateTime, remark, MessageType.DEBUG, False, False).ConfigureAwait(False)
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
                            Dim message As String = Nothing

                            If placeOrderTriggers.FirstOrDefault.Item2.EntryDirection = IOrder.TypeOfTransaction.Sell Then
                                _targetReached = True
                                message = String.Format("Exit order placed. Signal Candle: {0}, Direction:{1}, Quantity:{2}",
                                                        placeOrderTriggers.FirstOrDefault.Item2.SignalCandle.SnapshotDateTime.ToString("HH:mm:ss"),
                                                        placeOrderTriggers.FirstOrDefault.Item2.EntryDirection.ToString,
                                                        placeOrderTriggers.FirstOrDefault.Item2.Quantity)
                            End If

                            If placeOrderTriggers.FirstOrDefault.Item2.Supporting IsNot Nothing AndAlso placeOrderTriggers.FirstOrDefault.Item2.Supporting.Count > 0 Then
                                _targetPrice = placeOrderTriggers.FirstOrDefault.Item2.Supporting.FirstOrDefault

                                message = String.Format("Entry Order Placed. Signal Candle: {0}, Direction:{1}, Potential Entry:{2}, Potential Target:{3}, Quantity:{4}",
                                                        placeOrderTriggers.FirstOrDefault.Item2.SignalCandle.SnapshotDateTime.ToString("HH:mm:ss"),
                                                        placeOrderTriggers.FirstOrDefault.Item2.EntryDirection.ToString,
                                                        placeOrderTriggers.FirstOrDefault.Item2.Supporting.LastOrDefault,
                                                        placeOrderTriggers.FirstOrDefault.Item2.Supporting.FirstOrDefault,
                                                        placeOrderTriggers.FirstOrDefault.Item2.Quantity)
                            End If

                            Await DisplayAndSendSignalAlertAsync(placeOrderTriggers.FirstOrDefault.Item2.SignalCandle.SnapshotDateTime, message, MessageType.INFO, False, False).ConfigureAwait(False)

                            If Not File.Exists(_runningInstrumentFilename) Then
                                File.WriteAllText(_runningInstrumentFilename, Me.TradableInstrument.InstrumentIdentifier)
                            End If
                        End If
                    Else
                        If Not File.Exists(_runningInstrumentFilename) Then
                            Dim message As String = String.Format("'Is Trigger Recevied Execute command' returned false. So instrument will be released.")
                            Await DisplayAndSendSignalAlertAsync(Now, message, MessageType.INFO, False, False).ConfigureAwait(False)
                            Exit While
                        End If
                    End If
                Else
                    If Not File.Exists(_runningInstrumentFilename) Then
                        Dim message As String = String.Format("'Is Trigger Recevied' returned false. So instrument will be released.")
                        Await DisplayAndSendSignalAlertAsync(Now, message, MessageType.INFO, False, False).ConfigureAwait(False)
                        Exit While
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
                Dim pl As Decimal = GetProjectedPL(sellPrice)

                Dim lastTradeSignalCandle As OHLCPayload = GetSignalCandleOfAnOrder(lastExecutedOrder.ParentOrderIdentifier, userSettings.SignalTimeFrame)
                If lastTradeSignalCandle IsNot Nothing Then
                    Dim message As String = String.Format("Target reached. PL: {0}", pl)

                    Await DisplayAndSendSignalAlertAsync(lastTradeSignalCandle.SnapshotDateTime, message, MessageType.INFO, False, False).ConfigureAwait(False)
                    _targetMessageSend = True
                End If
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
                Dim signal As Tuple(Of Boolean, OHLCPayload, Integer, Decimal, Decimal) = Await GetEntrySignalAsync(runningCandle, currentTick, forcePrint).ConfigureAwait(False)
                If signal IsNot Nothing AndAlso signal.Item1 Then
                    Dim signalCandle As OHLCPayload = signal.Item2
                    Dim quantity As Integer = signal.Item3

                    If signalCandle IsNot Nothing AndAlso quantity > 0 Then
                        parameters = New PlaceOrderParameters(signalCandle) With
                                        {.EntryDirection = IOrder.TypeOfTransaction.Buy,
                                         .OrderType = IOrder.TypeOfOrder.Market,
                                         .Quantity = quantity,
                                         .Supporting = New List(Of Object) From {signal.Item4, signal.Item5}}
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

    Private Async Function GetEntrySignalAsync(ByVal runningCandle As OHLCPayload, ByVal currentTick As ITick, ByVal forcePrint As Boolean) As Task(Of Tuple(Of Boolean, OHLCPayload, Integer, Decimal, Decimal))
        Dim ret As Tuple(Of Boolean, OHLCPayload, Integer, Decimal, Decimal) = Nothing
        If runningCandle IsNot Nothing AndAlso runningCandle.PreviousPayload IsNot Nothing Then
            Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
            Dim fractalData As FractalConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummyFractalConsumer)

            Dim fractalTurnoverSatisfied As Tuple(Of Boolean, String) = IsFratalAndTurnoverSatisfied(fractalData, runningCandle, "From 'Is Trigger Receive'")
            If fractalTurnoverSatisfied IsNot Nothing Then
                Dim remark As String = fractalTurnoverSatisfied.Item2
                If fractalTurnoverSatisfied.Item1 Then
                    Dim signalCandle As OHLCPayload = Nothing
                    Dim lastExecutedOrder As IBusinessOrder = GetLastExecutedOrder()
                    If lastExecutedOrder IsNot Nothing Then
                        Dim lastOrderSignalCandle As OHLCPayload = GetSignalCandleOfAnOrder(lastExecutedOrder.ParentOrderIdentifier, userSettings.SignalTimeFrame)
                        If lastOrderSignalCandle IsNot Nothing Then
                            If lastOrderSignalCandle.SnapshotDateTime <> runningCandle.PreviousPayload.SnapshotDateTime Then
                                remark = String.Format("{0}Last Order Signal Candle({1})<>Signal Candle({2})[True]{3}{3}",
                                                       remark, lastOrderSignalCandle.SnapshotDateTime.ToString("HH:mm:ss"),
                                                       runningCandle.PreviousPayload.SnapshotDateTime.ToString("HH:mm:ss"),
                                                       vbNewLine)
                                If fractalData.ConsumerPayloads.ContainsKey(lastOrderSignalCandle.SnapshotDateTime) Then
                                    Dim lastTradedFractal As FractalConsumer.FractalPayload = fractalData.ConsumerPayloads(lastOrderSignalCandle.SnapshotDateTime)
                                    If lastTradedFractal.FractalLow.Value <> SelectionData.FractalLow AndAlso
                                        lastTradedFractal.FractalHigh.Value <> SelectionData.FractalHigh Then
                                        signalCandle = runningCandle.PreviousPayload
                                    Else
                                        _SelectionData.FinalMessage = String.Format("Last order signal candle fractal high/low same")
                                    End If
                                    remark = String.Format("{0}Last Order Fractal High({1})<>Fractal High({2})[{3}]. Last Order Fractal Low({4})<>Fractal Low({5})[{6}]{7}{7}",
                                                           remark, lastTradedFractal.FractalHigh.Value, SelectionData.FractalHigh,
                                                           lastTradedFractal.FractalHigh.Value <> SelectionData.FractalHigh,
                                                           lastTradedFractal.FractalLow.Value, SelectionData.FractalLow,
                                                           lastTradedFractal.FractalLow.Value <> SelectionData.FractalLow, vbNewLine)
                                Else
                                    remark = String.Format("{0}Last Order Fractal Not Found.", remark)
                                    _SelectionData.FinalMessage = String.Format("Last order signal candle fractal not found")
                                End If
                            Else
                                remark = String.Format("{0}Last Order Signal Candle({1})<>Signal Candle({2})[False]{3}{3}",
                                                       remark, lastOrderSignalCandle.SnapshotDateTime.ToString("HH:mm:ss"),
                                                       runningCandle.PreviousPayload.SnapshotDateTime.ToString("HH:mm:ss"),
                                                       vbNewLine)
                                _SelectionData.FinalMessage = String.Format("Last order signal candle is not different from current signal candle")
                            End If
                        Else
                            remark = String.Format("{0}Last Order Signal Candle not found.", remark)
                            _SelectionData.FinalMessage = String.Format("Last order signal candle not found")
                        End If
                    Else
                        signalCandle = SelectionData.SignalCandle
                    End If

                    If signalCandle IsNot Nothing Then
                        Dim entryPrice As Decimal = SelectionData.FractalLow
                        Dim targetPrice As Decimal = SelectionData.FractalHigh
                        Dim previousProjectedPL As Decimal = GetProjectedPL(targetPrice)
                        Dim previousQuantity As Long = GetTotalQuantityTraded()

                        Dim plToAchive As Decimal = userSettings.MaxProfitPerStock - previousProjectedPL
                        Dim quantity As Integer = CalculateQuantityFromTarget(entryPrice, targetPrice, plToAchive)

                        remark = String.Format("{0}Previous Projected PL INR={1} for Quantity={2}, So Current Effective INR Required={3}, So Quantity To Trade={4}.{5}{5}",
                                                remark, previousProjectedPL, previousQuantity, plToAchive, quantity, vbNewLine)

                        ret = New Tuple(Of Boolean, OHLCPayload, Integer, Decimal, Decimal)(True, signalCandle, quantity, targetPrice, entryPrice)

                        If forcePrint Then logger.Debug(remark)
                    End If
                End If
                Await DisplayAndSendSignalAlertAsync(SelectionData.SignalCandle.SnapshotDateTime, remark, MessageType.ALL, False, False).ConfigureAwait(False)
            End If
        End If
        Return ret
    End Function

    Private Function IsFratalAndTurnoverSatisfied(ByVal fractalData As FractalConsumer, ByVal runningCandle As OHLCPayload, ByVal checkFrom As String) As Tuple(Of Boolean, String)
        Dim ret As Tuple(Of Boolean, String) = Nothing
        Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
        Dim conditionSatisfied As Boolean = False
        Dim comment As String = String.Format("{0}{1}[FINAL MESSAGE]{1}{1}", checkFrom, vbNewLine)

        If SelectionData.SignalCandle IsNot Nothing Then
            If SelectionData.SignalCandle.SnapshotDateTime <> runningCandle.PreviousPayload.SnapshotDateTime Then
                _SelectionData.SignalCandle = runningCandle.PreviousPayload
                _SelectionData.Iteration = 0
                _SelectionData.FinalMessage = Nothing
            Else
                'Nothing to do as signal candle and iteration already assigned
            End If
        Else
            _SelectionData.SignalCandle = runningCandle.PreviousPayload
            _SelectionData.Iteration = 0
            _SelectionData.FinalMessage = Nothing
        End If
        comment = String.Format("{0}Signal Candle: {1}{2}{2}", comment, SelectionData.SignalCandle.SnapshotDateTime.ToString("HH:mm:ss"), vbNewLine)
        If fractalData.ConsumerPayloads IsNot Nothing AndAlso fractalData.ConsumerPayloads.ContainsKey(SelectionData.SignalCandle.SnapshotDateTime) Then
            Dim fractal As FractalConsumer.FractalPayload = fractalData.ConsumerPayloads(SelectionData.SignalCandle.SnapshotDateTime)
            _SelectionData.FractalHigh = fractal.FractalHigh.Value
            _SelectionData.FractalLow = fractal.FractalLow.Value

            If SelectionData.FractalHigh > 0 AndAlso SelectionData.FractalLow > 0 Then
                _SelectionData.LastDayFractalLowChanged = IsLastDayFractalLowChanged(fractalData, runningCandle)
                conditionSatisfied = SelectionData.LastDayFractalLowChanged
                comment = String.Format("{0}Last Day Fractal Low Changed[{1}]{2}{2}", comment, SelectionData.LastDayFractalLowChanged, vbNewLine)

                If conditionSatisfied Then
                    _SelectionData.CandleCloseBelowFractalLow = SelectionData.SignalCandle.ClosePrice.Value < SelectionData.FractalLow
                    conditionSatisfied = conditionSatisfied AndAlso SelectionData.CandleCloseBelowFractalLow
                    comment = String.Format("{0}Candle Close({1})<Fractal Low({2})[{3}]{4}{4}",
                                            comment, SelectionData.SignalCandle.ClosePrice.Value, SelectionData.FractalLow,
                                            SelectionData.CandleCloseBelowFractalLow, vbNewLine)

                    If conditionSatisfied Then
                        _SelectionData.FractalHighGreaterThanFractalLow = SelectionData.FractalHigh > SelectionData.FractalLow
                        conditionSatisfied = conditionSatisfied AndAlso SelectionData.FractalHighGreaterThanFractalLow
                        comment = String.Format("{0}Fractal High({1})>Fractal Low({2})[{3}]{4}{4}",
                                                comment, SelectionData.FractalHigh, SelectionData.FractalLow,
                                                SelectionData.FractalHighGreaterThanFractalLow, vbNewLine)

                        If conditionSatisfied Then
                            _SelectionData.FractalDiffernce = SelectionData.FractalHigh - SelectionData.FractalLow
                            _SelectionData.MaxFractalDiffernce = Math.Round(SelectionData.FractalLow * userSettings.MaxFractalDifferencePercentage / 100, 4)
                            _SelectionData.FractalDiffLessThanMaxFractalDiff = SelectionData.FractalDiffernce < SelectionData.MaxFractalDiffernce
                            conditionSatisfied = conditionSatisfied AndAlso SelectionData.FractalDiffLessThanMaxFractalDiff
                            comment = String.Format("{0}Fractal Differnce({1})<{2}% ({3}) of Lower Fractal({4})[{5}]{6}{6}",
                                                    comment, SelectionData.FractalDiffernce,
                                                    userSettings.MaxFractalDifferencePercentage,
                                                    SelectionData.MaxFractalDiffernce,
                                                    SelectionData.FractalLow,
                                                    SelectionData.FractalDiffLessThanMaxFractalDiff,
                                                    vbNewLine)

                            If conditionSatisfied Then
                                _SelectionData.PotentialQuantityForMinimumTurnover = Math.Ceiling((userSettings.MinTurnoverPerTrade / SelectionData.FractalLow) / Me.TradableInstrument.LotSize) * Me.TradableInstrument.LotSize
                                _SelectionData.PLForPotentialQuantityForMinimumTurnover = _APIAdapter.CalculatePLWithBrokerage(Me.TradableInstrument, SelectionData.FractalLow, SelectionData.FractalHigh, SelectionData.PotentialQuantityForMinimumTurnover)
                                _SelectionData.PLForPotentialQuantityForMinimumTurnoverGratenThanZero = SelectionData.PLForPotentialQuantityForMinimumTurnover > 0
                                conditionSatisfied = conditionSatisfied AndAlso SelectionData.PLForPotentialQuantityForMinimumTurnoverGratenThanZero
                                comment = String.Format("{0}Potential Quantity for min turnover=Min Turnover({1}) / Fractal Low({2})={3}, Using that PL({4}) > 0[{5}]{6}{6}",
                                                        comment, userSettings.MinTurnoverPerTrade, SelectionData.FractalLow,
                                                        SelectionData.PotentialQuantityForMinimumTurnover,
                                                        SelectionData.PLForPotentialQuantityForMinimumTurnover,
                                                        SelectionData.PLForPotentialQuantityForMinimumTurnoverGratenThanZero, vbNewLine)

                                If conditionSatisfied Then
                                    _SelectionData.QuantityToGetRequiedTargetWithoutPreviousLoss = CalculateQuantityFromTarget(SelectionData.FractalLow, SelectionData.FractalHigh, userSettings.MaxProfitPerStock)
                                    _SelectionData.Turnover = SelectionData.FractalLow * SelectionData.QuantityToGetRequiedTargetWithoutPreviousLoss
                                    _SelectionData.TurnoverGreaterThanMinimumTurnover = SelectionData.Turnover >= userSettings.MinTurnoverPerTrade
                                    _SelectionData.TurnoverLessThanMaximumTurnover = SelectionData.Turnover <= userSettings.MaxTurnoverPerTrade
                                    conditionSatisfied = conditionSatisfied AndAlso SelectionData.TurnoverGreaterThanMinimumTurnover AndAlso SelectionData.TurnoverLessThanMaximumTurnover
                                    comment = String.Format("{0}Quantity to get required target without previous loss={1}, Turnover=Quantity({2})*Fractal Low({3})={4}, Turnover({5})>={6} And Turnover({7})<={8}[{9}]{10}{10}",
                                                              comment, SelectionData.QuantityToGetRequiedTargetWithoutPreviousLoss,
                                                              SelectionData.QuantityToGetRequiedTargetWithoutPreviousLoss, SelectionData.FractalLow,
                                                              SelectionData.Turnover, SelectionData.Turnover, userSettings.MinTurnoverPerTrade,
                                                              SelectionData.Turnover, userSettings.MaxTurnoverPerTrade,
                                                              (SelectionData.TurnoverGreaterThanMinimumTurnover AndAlso SelectionData.TurnoverLessThanMaximumTurnover), vbNewLine)
                                    If Not conditionSatisfied Then
                                        _SelectionData.FinalMessage = String.Format("Maximum/minimum turnover not satisfied")
                                    End If
                                Else
                                    _SelectionData.FinalMessage = String.Format("Minimum turnover pl not greater than zero")
                                End If
                            Else
                                _SelectionData.FinalMessage = String.Format("Fractal difference not less that max allowed fractal difference")
                            End If
                        Else
                            _SelectionData.FinalMessage = String.Format("Fractal high not greater than fractal low")
                        End If
                    Else
                        _SelectionData.FinalMessage = String.Format("Candle close not below fractal")
                    End If
                Else
                    _SelectionData.FinalMessage = String.Format("Last day fractal low not changed")
                End If
            Else
                conditionSatisfied = False
                comment = String.Format("Signal Candle Fractal High:{0}, Fractal Low:{1}", SelectionData.FractalHigh, SelectionData.FractalLow)
                _SelectionData.FinalMessage = String.Format("Signal Candle Fractal High/Low = 0")
            End If
        Else
            conditionSatisfied = False
            comment = String.Format("Signal Candle Fractal not found")
            _SelectionData.FinalMessage = String.Format("Signal Candle Fractal not found")
        End If

        ret = New Tuple(Of Boolean, String)(conditionSatisfied, comment)

        Return ret
    End Function

    Private Function GetProjectedPL(ByVal targetPrice As Decimal) As Decimal
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

    Private Async Function DisplayAndSendSignalAlertAsync(ByVal signalMinuteTime As Date, ByVal message As String, ByVal typeOfMessage As MessageType,
                                                          ByVal doNotIncludeTradingSymbol As Boolean,
                                                          ByVal convertToImage As Boolean) As Task
        Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
        Try
            _cts.Token.ThrowIfCancellationRequested()
            If message IsNot Nothing AndAlso message.Trim <> "" Then
                If Not _displayedLogData.ContainsKey(signalMinuteTime) Then _displayedLogData.Add(signalMinuteTime, New List(Of String))
                If Not _displayedLogData(signalMinuteTime).Contains(message, StringComparer.OrdinalIgnoreCase) Then
                    If typeOfMessage <> MessageType.INFO Then _SelectionData.Iteration = SelectionData.Iteration + 1

                    _displayedLogData(signalMinuteTime).Add(message)
                    Utilities.Strings.SerializeFromCollection(Of Dictionary(Of Date, List(Of String)))(_runningInstrumentLogFilename, _displayedLogData)

                    Dim summaryMessage As String = Nothing
                    If typeOfMessage <> MessageType.INFO Then
                        Dim selectionMessage As String = "#Condition_Satisfied"
                        Dim finalMessage As String = "N/A"
                        If SelectionData.FinalMessage IsNot Nothing AndAlso SelectionData.FinalMessage.Trim <> "" Then
                            selectionMessage = "#Not_Condition_Satisfied"
                            finalMessage = SelectionData.FinalMessage
                        End If
                        summaryMessage = String.Format("Iteration:{0}, Reason:{1} ({2})",
                                                       SelectionData.Iteration, finalMessage, selectionMessage)
                        message = message.Replace("[FINAL MESSAGE]", summaryMessage)
                    End If

                    If Not convertToImage Then OnHeartbeat(message)

                    SendTelegramMessageAsync(message, summaryMessage, typeOfMessage, doNotIncludeTradingSymbol, convertToImage)
                End If
            End If
        Catch ex As Exception
            logger.Warn(ex.ToString)
        End Try
    End Function

    Private Async Function SendTelegramMessageAsync(ByVal message As String, ByVal summaryMessage As String, ByVal typeOfMessage As MessageType,
                                                    ByVal doNotIncludeTradingSymbol As Boolean, ByVal convertToImage As Boolean) As Task
        Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
        Try
            _cts.Token.ThrowIfCancellationRequested()
            Dim userInputs As NFOUserInputs = Me.ParentStrategy.UserSettings
            If userInputs.TelegramBotAPIKey IsNot Nothing AndAlso userInputs.TelegramBotAPIKey.Trim <> "" AndAlso
                userInputs.TelegramDebugChatID IsNot Nothing AndAlso userInputs.TelegramDebugChatID.Trim <> "" AndAlso
                userInputs.TelegramInfoChatID IsNot Nothing AndAlso userInputs.TelegramInfoChatID.Trim <> "" Then
                If convertToImage Then
                    Dim messageImage As Image = HtmlToImage.HtmlToImage.HtmlToImage.ConvertHtmlToImage(message, 1000)

                    Await SendTelegramImageMessageAsync(userInputs.TelegramBotAPIKey, userInputs.TelegramDebugChatID, messageImage).ConfigureAwait(False)
                    If typeOfMessage = MessageType.INFO Then
                        Await SendTelegramImageMessageAsync(userInputs.TelegramBotAPIKey, userInputs.TelegramInfoChatID, messageImage).ConfigureAwait(False)
                    End If
                Else
                    If doNotIncludeTradingSymbol Then
                        message = String.Format("{0}{1}Timestamp: {2}", message, vbNewLine, Now.ToString("HH:mm:ss"))
                    Else
                        message = String.Format("{0}: {1}{2}Timestamp: {3}", Me.TradableInstrument.TradingSymbol, message, vbNewLine, Now.ToString("HH:mm:ss"))
                    End If

                    Await SendTelegramTextMessageAsync(userInputs.TelegramBotAPIKey, userInputs.TelegramDebugChatID, message).ConfigureAwait(False)
                    If typeOfMessage = MessageType.INFO Then
                        Await SendTelegramTextMessageAsync(userInputs.TelegramBotAPIKey, userInputs.TelegramInfoChatID, message).ConfigureAwait(False)
                    ElseIf typeOfMessage = MessageType.ALL Then
                        If doNotIncludeTradingSymbol Then
                            summaryMessage = String.Format("{0}{1}Timestamp: {2}", summaryMessage, vbNewLine, Now.ToString("HH:mm:ss"))
                        Else
                            summaryMessage = String.Format("{0}: {1}{2}Timestamp: {3}", Me.TradableInstrument.TradingSymbol, summaryMessage, vbNewLine, Now.ToString("HH:mm:ss"))
                        End If
                        Await SendTelegramTextMessageAsync(userInputs.TelegramBotAPIKey, userInputs.TelegramInfoChatID, summaryMessage).ConfigureAwait(False)
                    End If
                End If
            End If
        Catch ex As Exception
            logger.Warn(ex.ToString)
        End Try
    End Function

    Private Async Function SendTelegramTextMessageAsync(ByVal apiKey As String, ByVal chatID As String, ByVal message As String) As Task
        Using tSender As New Utilities.Notification.Telegram(apiKey.Trim, chatID.Trim, _cts)
            Await tSender.SendMessageGetAsync(Utilities.Strings.UrlEncodeString(message)).ConfigureAwait(False)
        End Using
    End Function

    Private Async Function SendTelegramImageMessageAsync(ByVal apiKey As String, ByVal chatID As String, ByVal messageImage As Image) As Task
        Using tSender As New Utilities.Notification.Telegram(apiKey.Trim, chatID.Trim, _cts)
            Using stream As New System.IO.MemoryStream()
                messageImage.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg)
                stream.Position = 0

                Await tSender.SendDocumentGetAsync(stream, "Potential To Actual Strike Selection.jpeg", String.Format("Timestamp: {0}", Now.ToString("HH:mm:ss"))).ConfigureAwait(False)
            End Using
        End Using
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