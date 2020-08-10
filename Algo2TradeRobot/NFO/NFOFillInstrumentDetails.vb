Imports System.IO
Imports System.Threading
Imports Utilities.DAL
Imports Algo2TradeCore.Entities
Imports Algo2TradeCore.Adapter
Imports Utilities.Network
Imports Utilities.Time
Imports Utilities.Numbers
Imports System.Net.Http
Imports NLog

Public Class NFOFillInstrumentDetails
    Implements IDisposable

#Region "Logging and Status Progress"
    Public Shared Shadows logger As Logger = LogManager.GetCurrentClassLogger
#End Region

#Region "Events/Event handlers"
    Public Event DocumentDownloadComplete()
    Public Event DocumentRetryStatus(ByVal currentTry As Integer, ByVal totalTries As Integer)
    Public Event Heartbeat(ByVal msg As String)
    Public Event WaitingFor(ByVal elapsedSecs As Integer, ByVal totalSecs As Integer, ByVal msg As String)
    'The below functions are needed to allow the derived classes to raise the above two events
    Protected Overridable Sub OnDocumentDownloadComplete()
        RaiseEvent DocumentDownloadComplete()
    End Sub
    Protected Overridable Sub OnDocumentRetryStatus(ByVal currentTry As Integer, ByVal totalTries As Integer)
        RaiseEvent DocumentRetryStatus(currentTry, totalTries)
    End Sub
    Protected Overridable Sub OnHeartbeat(ByVal msg As String)
        RaiseEvent Heartbeat(msg)
    End Sub
    Protected Overridable Sub OnWaitingFor(ByVal elapsedSecs As Integer, ByVal totalSecs As Integer, ByVal msg As String)
        RaiseEvent WaitingFor(elapsedSecs, totalSecs, msg)
    End Sub
#End Region

    Private _cts As CancellationTokenSource
    Private ReadOnly _parentStrategy As NFOStrategy
    Private ReadOnly _userInputs As NFOUserInputs
    Private ReadOnly AliceEODHistoricalURL = "https://ant.aliceblueonline.com/api/v1/charts?exchange={0}&token={1}&candletype=3&starttime={2}&endtime={3}&type=historical"
    Private ReadOnly ALiceIntradayHistoricalURL = "https://ant.aliceblueonline.com/api/v1/charts?exchange={0}&token={1}&candletype=1&starttime={2}&endtime={3}&type=historical"
    Private ReadOnly _tradingDay As Date = Date.MinValue
    Private ReadOnly _APIAdapter As APIAdapter
    Public Sub New(ByVal canceller As CancellationTokenSource, ByVal parentStrategy As NFOStrategy)
        _cts = canceller
        _parentStrategy = parentStrategy
        _userInputs = _parentStrategy.UserSettings
        _tradingDay = Now

        Select Case _parentStrategy.ParentController.BrokerSource
            Case APISource.Zerodha
                _APIAdapter = New ZerodhaAdapter(parentStrategy.ParentController, _cts)
            Case APISource.AliceBlue
                _APIAdapter = New AliceAdapter(parentStrategy.ParentController, _cts)
            Case APISource.Upstox
                Throw New NotImplementedException
            Case APISource.None
                Throw New NotImplementedException
        End Select
    End Sub

    Private Async Function GetHistoricalCandleStickAsync(ByVal instrument As IInstrument, ByVal fromDate As Date, ByVal toDate As Date, ByVal historicalDataType As TypeOfData) As Task(Of Dictionary(Of String, Object))
        Dim ret As Dictionary(Of String, Object) = Nothing
        _cts.Token.ThrowIfCancellationRequested()
        Dim historicalDataURL As String = Nothing
        Select Case historicalDataType
            Case TypeOfData.Intraday
                historicalDataURL = String.Format(ALiceIntradayHistoricalURL, instrument.RawExchange, instrument.InstrumentIdentifier, DateTimeToUnix(fromDate), DateTimeToUnix(toDate))
            Case TypeOfData.EOD
                historicalDataURL = String.Format(AliceEODHistoricalURL, instrument.RawExchange, instrument.InstrumentIdentifier, DateTimeToUnix(fromDate), DateTimeToUnix(toDate))
        End Select
        OnHeartbeat(String.Format("Fetching historical Data: {0}", historicalDataURL))
        Dim proxyToBeUsed As HttpProxy = Nothing
        Using browser As New HttpBrowser(proxyToBeUsed, Net.DecompressionMethods.GZip, New TimeSpan(0, 1, 0), _cts)
            AddHandler browser.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete
            AddHandler browser.Heartbeat, AddressOf OnHeartbeat
            AddHandler browser.WaitingFor, AddressOf OnWaitingFor
            AddHandler browser.DocumentRetryStatus, AddressOf OnDocumentRetryStatus

            Dim headers As Dictionary(Of String, String) = New Dictionary(Of String, String)
            headers.Add("X-Authorization-Token", _parentStrategy.ParentController.APIConnection.ENCToken)

            Dim l As Tuple(Of Uri, Object) = Await browser.NonPOSTRequestAsync(historicalDataURL,
                                                                                HttpMethod.Get,
                                                                                Nothing,
                                                                                False,
                                                                                headers,
                                                                                True,
                                                                                "application/json").ConfigureAwait(False)
            If l Is Nothing OrElse l.Item2 Is Nothing Then
                Throw New ApplicationException(String.Format("No response while getting historical data for: {0}", historicalDataURL))
            End If
            If l IsNot Nothing AndAlso l.Item2 IsNot Nothing Then
                ret = l.Item2
            End If
            RemoveHandler browser.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete
            RemoveHandler browser.Heartbeat, AddressOf OnHeartbeat
            RemoveHandler browser.WaitingFor, AddressOf OnWaitingFor
            RemoveHandler browser.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
        End Using
        Return ret
    End Function
    Private Async Function GetChartFromHistoricalAsync(ByVal instrument As IInstrument,
                                                       ByVal fromDate As Date,
                                                       ByVal toDate As Date,
                                                       ByVal historicalDataType As TypeOfData) As Task(Of Dictionary(Of Date, OHLCPayload))
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        Dim ret As Dictionary(Of Date, OHLCPayload) = Nothing
        Dim historicalCandlesJSONDict As Dictionary(Of String, Object) = Await GetHistoricalCandleStickAsync(instrument, fromDate, toDate, historicalDataType).ConfigureAwait(False)
        If historicalCandlesJSONDict.ContainsKey("data") Then
            Dim historicalCandles As ArrayList = historicalCandlesJSONDict("data")
            OnHeartbeat(String.Format("Generating Payload for {0}", instrument.TradingSymbol))
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

    Public Async Function GetInstrumentData(ByVal allInstruments As IEnumerable(Of IInstrument), ByVal bannedStock As List(Of String)) As Task
        If allInstruments IsNot Nothing AndAlso allInstruments.Count > 0 Then
            Dim nfoInstruments As IEnumerable(Of IInstrument) = allInstruments.Where(Function(x)
                                                                                         Return x.Segment = "NFO-FUT"
                                                                                     End Function)
            If nfoInstruments IsNot Nothing AndAlso nfoInstruments.Count > 0 Then
                Dim currentNFOInstruments As List(Of IInstrument) = Nothing
                For Each runningInstrument In nfoInstruments
                    If currentNFOInstruments IsNot Nothing AndAlso currentNFOInstruments.Count > 0 Then
                        Dim availableInstrument As IEnumerable(Of IInstrument) = currentNFOInstruments.FindAll(Function(z)
                                                                                                                   Return z.RawInstrumentName = runningInstrument.RawInstrumentName
                                                                                                               End Function)
                        If availableInstrument IsNot Nothing AndAlso availableInstrument.Count > 0 Then
                            Continue For
                        End If
                    End If
                    Dim runningIntruments As IEnumerable(Of IInstrument) = nfoInstruments.Where(Function(x)
                                                                                                    Return x.RawInstrumentName = runningInstrument.RawInstrumentName
                                                                                                End Function)
                    Dim minExpiry As Date = runningIntruments.Min(Function(x)
                                                                      If x.Expiry.Value.Date <= Now.Date Then
                                                                          Return Date.MaxValue
                                                                      Else
                                                                          Return x.Expiry
                                                                      End If
                                                                  End Function)
                    Dim currentIntrument As IInstrument = runningIntruments.ToList.Find(Function(y)
                                                                                            Return y.Expiry.Value.Date = minExpiry.Date
                                                                                        End Function)
                    If currentIntrument IsNot Nothing Then
                        If currentNFOInstruments Is Nothing Then currentNFOInstruments = New List(Of IInstrument)
                        currentNFOInstruments.Add(currentIntrument)
                    End If
                Next
                Dim lastTradingDay As Date = Date.MinValue
                Dim highATRStocks As Concurrent.ConcurrentDictionary(Of String, Decimal()) = Nothing
                Try
                    If currentNFOInstruments IsNot Nothing AndAlso currentNFOInstruments.Count > 0 Then
                        For i As Integer = 0 To currentNFOInstruments.Count - 1 Step 20
                            Dim numberOfData As Integer = If(currentNFOInstruments.Count - i > 20, 20, currentNFOInstruments.Count - i)
                            Dim tasks As IEnumerable(Of Task(Of Boolean)) = Nothing
                            tasks = currentNFOInstruments.GetRange(i, numberOfData).Select(Async Function(y)
                                                                                               Try
                                                                                                   _cts.Token.ThrowIfCancellationRequested()
                                                                                                   If y.RawExchange.ToUpper = "NFO" AndAlso (bannedStock Is Nothing OrElse
                                                                                                                   bannedStock IsNot Nothing AndAlso Not bannedStock.Contains(y.RawInstrumentName)) Then
                                                                                                       ''Dim futureEODPayload As Dictionary(Of Date, OHLCPayload) = Await GetChartFromHistoricalAsync(y, tradingDay.AddDays(-10), tradingDay.AddDays(-1), TypeOfData.EOD).ConfigureAwait(False)
                                                                                                       'Dim futureEODPayload As Dictionary(Of Date, OHLCPayload) = Await GetChartFromHistoricalAsync(y, tradingDay.AddDays(-10), tradingDay, TypeOfData.EOD).ConfigureAwait(False)
                                                                                                       'If futureEODPayload IsNot Nothing AndAlso futureEODPayload.Count > 0 Then
                                                                                                       '    Dim lastDayPayload As OHLCPayload = futureEODPayload.LastOrDefault.Value
                                                                                                       '    If lastDayPayload.ClosePrice.Value >= _userInputs.MinStockPrice AndAlso lastDayPayload.ClosePrice.Value <= _userInputs.MaxStockPrice Then
                                                                                                       Dim rawCashInstrument As IInstrument = allInstruments.ToList.Find(Function(x)
                                                                                                                                                                             Return x.TradingSymbol = y.RawInstrumentName
                                                                                                                                                                         End Function)
                                                                                                       If rawCashInstrument IsNot Nothing Then
                                                                                                           _cts.Token.ThrowIfCancellationRequested()
                                                                                                           'Dim eodHistoricalData As Dictionary(Of Date, OHLCPayload) = Await GetChartFromHistoricalAsync(rawCashInstrument, tradingDay.AddDays(-300), tradingDay.AddDays(-1), TypeOfData.EOD).ConfigureAwait(False)
                                                                                                           Dim eodHistoricalData As Dictionary(Of Date, OHLCPayload) = Await GetChartFromHistoricalAsync(rawCashInstrument, _tradingDay.AddDays(-300), _tradingDay, TypeOfData.EOD).ConfigureAwait(False)
                                                                                                           _cts.Token.ThrowIfCancellationRequested()
                                                                                                           If eodHistoricalData IsNot Nothing AndAlso eodHistoricalData.Count > 100 Then
                                                                                                               _cts.Token.ThrowIfCancellationRequested()
                                                                                                               If eodHistoricalData.LastOrDefault.Value.ClosePrice.Value >= _userInputs.MinStockPrice AndAlso eodHistoricalData.LastOrDefault.Value.ClosePrice.Value <= _userInputs.MaxStockPrice Then
                                                                                                                   Dim ATRPayload As Dictionary(Of Date, Decimal) = Nothing
                                                                                                                   CalculateATR(14, eodHistoricalData, ATRPayload)
                                                                                                                   _cts.Token.ThrowIfCancellationRequested()
                                                                                                                   Dim lastDayClosePrice As Decimal = eodHistoricalData.LastOrDefault.Value.ClosePrice.Value
                                                                                                                   lastTradingDay = eodHistoricalData.LastOrDefault.Key
                                                                                                                   Dim atrPercentage As Decimal = (ATRPayload(eodHistoricalData.LastOrDefault.Key) / lastDayClosePrice) * 100
                                                                                                                   If atrPercentage >= _userInputs.MinATRPercentage Then
                                                                                                                       Dim eodHKPayload As Dictionary(Of Date, OHLCPayload) = Nothing
                                                                                                                       ConvertToHeikenAshi(eodHistoricalData, eodHKPayload)
                                                                                                                       If eodHKPayload IsNot Nothing AndAlso eodHKPayload.Count > 0 Then
                                                                                                                           If highATRStocks Is Nothing Then highATRStocks = New Concurrent.ConcurrentDictionary(Of String, Decimal())
                                                                                                                           highATRStocks.TryAdd(rawCashInstrument.TradingSymbol, {atrPercentage, lastDayClosePrice, eodHKPayload.LastOrDefault.Value.OpenPrice.Value, eodHKPayload.LastOrDefault.Value.LowPrice.Value, eodHKPayload.LastOrDefault.Value.HighPrice.Value, eodHKPayload.LastOrDefault.Value.ClosePrice.Value})
                                                                                                                       End If
                                                                                                                   End If
                                                                                                               End If
                                                                                                           End If
                                                                                                       End If
                                                                                                       '    End If
                                                                                                       'End If
                                                                                                   End If
                                                                                               Catch ex As Exception
                                                                                                   logger.Error(ex)
                                                                                                   Throw ex
                                                                                               End Try
                                                                                               Return True
                                                                                           End Function)

                            Dim mainTask As Task = Task.WhenAll(tasks)
                            Await mainTask.ConfigureAwait(False)
                            If mainTask.Exception IsNot Nothing Then
                                logger.Error(mainTask.Exception)
                                Throw mainTask.Exception
                            End If
                        Next
                    End If
                Catch cex As TaskCanceledException
                    logger.Error(cex)
                    Throw cex
                Catch aex As AggregateException
                    logger.Error(aex)
                    Throw aex
                Catch ex As Exception
                    logger.Error(ex)
                    Throw ex
                End Try

                If highATRStocks IsNot Nothing AndAlso highATRStocks.Count > 0 Then
                    Dim capableStocks As Dictionary(Of String, InstrumentDetails) = Nothing
                    For Each stock In highATRStocks.OrderByDescending(Function(x)
                                                                          Return x.Value(0)
                                                                      End Function)
                        _cts.Token.ThrowIfCancellationRequested()
                        'Dim futureStocks As List(Of IInstrument) = nfoInstruments.ToList.FindAll(Function(x)
                        '                                                                             Return x.RawInstrumentName = stock.Key
                        '                                                                         End Function)
                        'If futureStocks IsNot Nothing AndAlso futureStocks.Count > 0 Then
                        '    Dim minexpiry As Date = futureStocks.Min(Function(y)
                        '                                                 Return y.Expiry
                        '                                             End Function)
                        'Dim tradingStock As IInstrument = Nothing
                        'Dim volumeCheckingStock As IInstrument = Nothing
                        Dim tradingStock As IInstrument = allInstruments.ToList.Find(Function(y)
                                                                                         Return y.TradingSymbol = stock.Key
                                                                                     End Function)
                        Dim volumeCheckingStock As IInstrument = tradingStock
                        '_cts.Token.ThrowIfCancellationRequested()
                        'If minexpiry.Date = Now.Date Then
                        '    volumeCheckingStock = futureStocks.Find(Function(x)
                        '                                                Return x.Expiry = minexpiry
                        '                                            End Function)
                        '    Dim nextMinExpiry As Date = futureStocks.Min(Function(y)
                        '                                                     If Not y.Expiry.Value.Date = Now.Date Then
                        '                                                         Return y.Expiry.Value
                        '                                                     Else
                        '                                                         Return Date.MaxValue
                        '                                                     End If
                        '                                                 End Function)
                        '    tradingStock = futureStocks.Find(Function(z)
                        '                                         Return z.Expiry = nextMinExpiry
                        '                                     End Function)
                        'ElseIf minexpiry.Date < Now.Date Then
                        '    Dim nextMinExpiry As Date = futureStocks.Min(Function(y)
                        '                                                     If Not y.Expiry.Value.Date <= Now.Date Then
                        '                                                         Return y.Expiry.Value
                        '                                                     Else
                        '                                                         Return Date.MaxValue
                        '                                                     End If
                        '                                                 End Function)
                        '    volumeCheckingStock = futureStocks.Find(Function(x)
                        '                                                Return x.Expiry = nextMinExpiry
                        '                                            End Function)
                        '    tradingStock = futureStocks.Find(Function(z)
                        '                                         Return z.Expiry = nextMinExpiry
                        '                                     End Function)
                        'Else
                        '    tradingStock = futureStocks.Find(Function(x)
                        '                                         Return x.Expiry = minexpiry
                        '                                     End Function)
                        '    volumeCheckingStock = tradingStock
                        'End If
                        _cts.Token.ThrowIfCancellationRequested()
                        If tradingStock IsNot Nothing AndAlso volumeCheckingStock IsNot Nothing Then
                            _cts.Token.ThrowIfCancellationRequested()
                            'Dim intradayHistoricalData As Dictionary(Of Date, OHLCPayload) = Await GetChartFromHistoricalAsync(volumeCheckingStock, lastTradingDay.AddDays(-5), lastTradingDay, TypeOfData.Intraday).ConfigureAwait(False)
                            Dim intradayHistoricalData As Dictionary(Of Date, OHLCPayload) = Await GetChartFromHistoricalAsync(volumeCheckingStock, lastTradingDay.AddDays(-5), _tradingDay, TypeOfData.Intraday).ConfigureAwait(False)
                            If intradayHistoricalData IsNot Nothing AndAlso intradayHistoricalData.Count > 100 Then
                                Dim intradayHKPayload As Dictionary(Of Date, OHLCPayload) = Nothing
                                ConvertToHeikenAshi(intradayHistoricalData, intradayHKPayload)
                                Dim intradayATRPayload As Dictionary(Of Date, Decimal) = Nothing
                                CalculateATR(14, intradayHKPayload, intradayATRPayload)
                                Dim blankCandlePercentage As Decimal = CalculateBlankVolumePercentage(intradayHistoricalData, lastTradingDay)
                                Dim instrumentData As New InstrumentDetails With
                                    {.TradingSymbol = tradingStock.TradingSymbol,
                                     .ATRPercentage = stock.Value(0),
                                     .Price = stock.Value(1),
                                     .Slab = CalculateSlab(stock.Value(1), stock.Value(0)),
                                     .BlankCandlePercentage = blankCandlePercentage,
                                     .PreviousDayHighestATR = GetHighestATR(intradayATRPayload, lastTradingDay),
                                     .PreviousDayHKOpen = stock.Value(2),
                                     .PreviousDayHKLow = stock.Value(3),
                                     .PreviousDayHKHigh = stock.Value(4),
                                     .PreviousDayHKClose = stock.Value(5),
                                     .Instrument = tradingStock}
                                If capableStocks Is Nothing Then capableStocks = New Dictionary(Of String, InstrumentDetails)
                                capableStocks.Add(tradingStock.TradingSymbol, instrumentData)
                            End If
                        End If
                        'End If
                    Next
                    If capableStocks IsNot Nothing AndAlso capableStocks.Count > 0 Then
                        Dim todayStockList As List(Of String) = Nothing
                        Dim stocksLessThanMaxBlankCandlePercentage As IEnumerable(Of KeyValuePair(Of String, InstrumentDetails)) =
                                    capableStocks.Where(Function(x)
                                                            Return x.Value.BlankCandlePercentage <> Decimal.MinValue AndAlso
                                                                  x.Value.BlankCandlePercentage <= _userInputs.MaxBlankCandlePercentage
                                                        End Function)
                        If stocksLessThanMaxBlankCandlePercentage IsNot Nothing AndAlso stocksLessThanMaxBlankCandlePercentage.Count > 0 Then
                            Dim stockCounter As Integer = 0
                            For Each stockData In stocksLessThanMaxBlankCandlePercentage.OrderByDescending(Function(x)
                                                                                                               Return x.Value.ATRPercentage
                                                                                                           End Function)
                                _cts.Token.ThrowIfCancellationRequested()
                                If todayStockList Is Nothing Then todayStockList = New List(Of String)
                                todayStockList.Add(stockData.Key)
                                stockCounter += 1
                            Next
                        End If
                        _cts.Token.ThrowIfCancellationRequested()
                        If todayStockList IsNot Nothing AndAlso todayStockList.Count > 0 Then
                            Dim allStockData As DataTable = Nothing
                            If _userInputs.InstrumentDetailsFilePath IsNot Nothing AndAlso
                                File.Exists(_userInputs.InstrumentDetailsFilePath) Then
                                Dim eligibleStocks As Dictionary(Of String, Decimal) = Nothing
                                For Each runningStock In todayStockList
                                    Dim hkOpen As Decimal = Math.Round(capableStocks(runningStock).PreviousDayHKOpen, 2)
                                    Dim hkLow As Decimal = Math.Round(capableStocks(runningStock).PreviousDayHKLow, 2)
                                    Dim hkHigh As Decimal = Math.Round(capableStocks(runningStock).PreviousDayHKHigh, 2)
                                    Dim hkClose As Decimal = Math.Round(capableStocks(runningStock).PreviousDayHKClose, 2)
                                    If hkOpen = hkHigh OrElse hkOpen = hkLow Then
                                        Dim highestATR As Decimal = capableStocks(runningStock).PreviousDayHighestATR
                                        Dim price As Decimal = capableStocks(runningStock).Price
                                        Dim instrument As IInstrument = capableStocks(runningStock).Instrument
                                        Dim buffer As Decimal = CalculateBuffer(price, RoundOfType.Floor)
                                        Dim slPoint As Decimal = ConvertFloorCeling(highestATR, instrument.TickSize, RoundOfType.Celing)
                                        Dim quantity As Integer = CalculateQuantityFromStoploss(price, price - slPoint, _userInputs.MaxProfitPerTrade, instrument)
                                        Dim target As Decimal = CalculateTargetFromPL(price, quantity, Math.Abs(_userInputs.MaxProfitPerTrade), instrument)
                                        Dim multiplier As Decimal = Math.Round((target - price) / slPoint, 4)
                                        If multiplier <= _userInputs.MaxTargetToStoplossMultiplier Then
                                            If eligibleStocks Is Nothing Then eligibleStocks = New Dictionary(Of String, Decimal)
                                            eligibleStocks.Add(runningStock, multiplier)
                                        Else
                                            Console.WriteLine(String.Format("Neglect for multiplier,{0},{1}", runningStock, multiplier))
                                        End If
                                    Else
                                        Console.WriteLine(String.Format("Neglect for hk storng candle,{0},{1},{2},{3}", runningStock, hkOpen, hkLow, hkHigh, hkClose))
                                    End If
                                Next


                                File.Delete(_userInputs.InstrumentDetailsFilePath)
                                Using csv As New CSVHelper(_userInputs.InstrumentDetailsFilePath, ",", _cts)
                                    _cts.Token.ThrowIfCancellationRequested()
                                    allStockData = New DataTable
                                    allStockData.Columns.Add("Trading Symbol")
                                    allStockData.Columns.Add("Multiplier")
                                    allStockData.Columns.Add("Previous Day Highest ATR")
                                    allStockData.Columns.Add("Previous Day HK Open")
                                    allStockData.Columns.Add("Previous Day HK Low")
                                    allStockData.Columns.Add("Previous Day HK High")
                                    allStockData.Columns.Add("Previous Day HK Close")

                                    If eligibleStocks IsNot Nothing AndAlso eligibleStocks.Count > 0 Then
                                        For Each stock In eligibleStocks
                                            Dim row As DataRow = allStockData.NewRow
                                            row("Trading Symbol") = stock.Key
                                            row("Multiplier") = stock.Value
                                            row("Previous Day Highest ATR") = Math.Round(capableStocks(stock.Key).PreviousDayHighestATR, 6)
                                            row("Previous Day HK Open") = Math.Round(capableStocks(stock.Key).PreviousDayHKOpen, 2)
                                            row("Previous Day HK Low") = Math.Round(capableStocks(stock.Key).PreviousDayHKLow, 2)
                                            row("Previous Day HK High") = Math.Round(capableStocks(stock.Key).PreviousDayHKHigh, 2)
                                            row("Previous Day HK Close") = Math.Round(capableStocks(stock.Key).PreviousDayHKClose, 2)

                                            allStockData.Rows.Add(row)
                                        Next
                                    End If

                                    csv.GetCSVFromDataTable(allStockData)
                                End Using
                                If _userInputs.InstrumentsData IsNot Nothing Then
                                    _userInputs.InstrumentsData.Clear()
                                    _userInputs.InstrumentsData = Nothing
                                    _userInputs.FillInstrumentDetails(_userInputs.InstrumentDetailsFilePath, _cts)
                                End If
                            End If
                        End If
                    End If
                End If
            End If
        End If
    End Function
    Private Function CalculateSlab(ByVal price As Decimal, ByVal atrPer As Decimal) As Decimal
        Dim ret As Decimal = 0.25
        Dim slabList As List(Of Decimal) = New List(Of Decimal) From {0.25, 0.5, 1, 2.5, 5, 10, 25}
        Dim atr As Decimal = (atrPer / 100) * price
        Dim supportedSlabList As List(Of Decimal) = slabList.FindAll(Function(x)
                                                                         Return x <= atr / 8
                                                                     End Function)
        If supportedSlabList IsNot Nothing AndAlso supportedSlabList.Count > 0 Then
            ret = supportedSlabList.Max
            If price * 1 / 100 < ret Then
                Dim newSupportedSlabList As List(Of Decimal) = supportedSlabList.FindAll(Function(x)
                                                                                             Return x <= price * 1 / 100
                                                                                         End Function)
                If newSupportedSlabList IsNot Nothing AndAlso newSupportedSlabList.Count > 0 Then
                    ret = newSupportedSlabList.Max
                End If
            End If
        End If
        Return ret
    End Function
    Private Sub CalculateATR(ByVal ATRPeriod As Integer, ByVal inputPayload As Dictionary(Of Date, OHLCPayload), ByRef outputPayload As Dictionary(Of Date, Decimal))
        'Using WILDER Formula
        If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
            If inputPayload.Count < 100 Then
                Throw New ApplicationException("Can't Calculate ATR")
            End If
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
    Private Sub ConvertToHeikenAshi(ByVal inputPayload As Dictionary(Of Date, OHLCPayload), ByRef outputPayload As Dictionary(Of Date, OHLCPayload))
        If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
            If inputPayload.Count < 30 Then
                Throw New ApplicationException("Can't Calculate Heikenshi Properly")
            End If

            Dim tempHAPayload As OHLCPayload = Nothing
            Dim tempPreHAPayload As OHLCPayload = Nothing

            For Each runningInputPayload In inputPayload

                tempHAPayload = New OHLCPayload(OHLCPayload.PayloadSource.Historical)
                tempHAPayload.PreviousPayload = tempPreHAPayload
                If tempPreHAPayload Is Nothing Then
                    tempHAPayload.OpenPrice.Value = (runningInputPayload.Value.OpenPrice.Value + runningInputPayload.Value.ClosePrice.Value) / 2
                Else
                    tempHAPayload.OpenPrice.Value = (tempPreHAPayload.OpenPrice.Value + tempPreHAPayload.ClosePrice.Value) / 2
                End If
                tempHAPayload.ClosePrice.Value = (runningInputPayload.Value.OpenPrice.Value + runningInputPayload.Value.ClosePrice.Value + runningInputPayload.Value.HighPrice.Value + runningInputPayload.Value.LowPrice.Value) / 4
                tempHAPayload.HighPrice.Value = Math.Max(runningInputPayload.Value.HighPrice.Value, Math.Max(tempHAPayload.OpenPrice.Value, tempHAPayload.ClosePrice.Value))
                tempHAPayload.LowPrice.Value = Math.Min(runningInputPayload.Value.LowPrice.Value, Math.Min(tempHAPayload.OpenPrice.Value, tempHAPayload.ClosePrice.Value))
                tempHAPayload.Volume.Value = runningInputPayload.Value.Volume
                tempHAPayload.DailyVolume = runningInputPayload.Value.DailyVolume
                tempHAPayload.SnapshotDateTime = runningInputPayload.Value.SnapshotDateTime
                tempHAPayload.TradingSymbol = runningInputPayload.Value.TradingSymbol
                tempPreHAPayload = tempHAPayload
                If outputPayload Is Nothing Then outputPayload = New Dictionary(Of Date, OHLCPayload)
                outputPayload.Add(runningInputPayload.Key, tempHAPayload)
            Next
        End If
    End Sub
    Private Function CalculateBlankVolumePercentage(ByVal inputPayload As Dictionary(Of Date, OHLCPayload), ByVal lastTradingDay As Date) As Decimal
        Dim ret As Decimal = Decimal.MinValue
        If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
            Dim blankCandlePayload As IEnumerable(Of KeyValuePair(Of Date, OHLCPayload)) = inputPayload.Where(Function(x)
                                                                                                                  Return x.Value.OpenPrice.Value = x.Value.LowPrice.Value AndAlso
                                                                                                                  x.Value.LowPrice.Value = x.Value.HighPrice.Value AndAlso
                                                                                                                  x.Value.HighPrice.Value = x.Value.ClosePrice.Value AndAlso
                                                                                                                  x.Value.SnapshotDateTime.Date = lastTradingDay.Date
                                                                                                              End Function)
            If blankCandlePayload IsNot Nothing AndAlso blankCandlePayload.Count > 0 Then
                ret = Math.Round((blankCandlePayload.Count / inputPayload.Count) * 100, 2)
            Else
                ret = 0
            End If
        End If
        Return ret
    End Function
    Private Function GetHighestATR(ByVal inputPayload As Dictionary(Of Date, Decimal), ByVal lastTradingDay As Date) As Decimal
        Dim ret As Decimal = Decimal.MinValue
        If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
            ret = inputPayload.Max(Function(x)
                                       If x.Key.Date = lastTradingDay.Date Then
                                           Return x.Value
                                       Else
                                           Return Decimal.MinValue
                                       End If
                                   End Function)
        End If
        Return ret
    End Function
    Protected Function CalculateBuffer(ByVal price As Double, ByVal floorOrCeiling As RoundOfType) As Double
        Dim bufferPrice As Double = Nothing
        bufferPrice = ConvertFloorCeling(price * 0.01 * 0.025, 0.05, floorOrCeiling)
        Return bufferPrice
    End Function
    Public Function CalculateQuantityFromStoploss(ByVal buyPrice As Double, ByVal sellPrice As Double, ByVal NetProfitLossOfTrade As Double, ByVal instrument As IInstrument) As Integer
        Dim lotSize As Integer = instrument.LotSize
        Dim quantityMultiplier As Integer = 1
        Dim previousQuantity As Integer = lotSize
        For quantityMultiplier = 1 To Integer.MaxValue
            Dim plAfterBrokerage As Decimal = _APIAdapter.CalculatePLWithBrokerage(instrument, buyPrice, sellPrice, lotSize * quantityMultiplier)
            If plAfterBrokerage <= Math.Abs(NetProfitLossOfTrade) * -1 Then
                previousQuantity = lotSize * If(quantityMultiplier - 1 = 0, 1, quantityMultiplier - 1)
                Exit For
            Else
                previousQuantity = lotSize * quantityMultiplier
            End If
        Next
        Return previousQuantity
    End Function
    Public Function CalculateTargetFromPL(ByVal buyPrice As Decimal, ByVal quantity As Integer, ByVal NetProfitOfTrade As Decimal, ByVal instrument As IInstrument) As Decimal
        Dim ret As Decimal = buyPrice
        For ret = buyPrice To Decimal.MaxValue Step instrument.TickSize
            Dim plAfterBrokerage As Decimal = _APIAdapter.CalculatePLWithBrokerage(instrument, buyPrice, ret, quantity)
            If plAfterBrokerage >= NetProfitOfTrade Then
                Exit For
            End If
        Next
        Return ret
    End Function

    Private Class InstrumentDetails
        Public TradingSymbol As String
        Public ATRPercentage As Decimal
        Public Price As Decimal
        Public Slab As Decimal
        Public BlankCandlePercentage As Decimal
        Public Instrument As IInstrument
        Public PreviousDayHighestATR As Decimal
        Public PreviousDayHKOpen As Decimal
        Public PreviousDayHKLow As Decimal
        Public PreviousDayHKHigh As Decimal
        Public PreviousDayHKClose As Decimal
    End Class

    Enum TypeOfData
        Intraday = 1
        EOD
    End Enum

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