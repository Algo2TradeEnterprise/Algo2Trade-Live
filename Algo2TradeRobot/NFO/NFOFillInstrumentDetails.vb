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

#Region "Enum"
    Enum TypeOfData
        Intraday = 1
        EOD
    End Enum
#End Region

    Private _cts As CancellationTokenSource
    Private ReadOnly _parentStrategy As NFOStrategy
    Private ReadOnly _userInputs As NFOUserInputs
    Private ReadOnly AliceEODHistoricalURL = "https://ant.aliceblueonline.com/api/v1/charts?exchange={0}&token={1}&candletype=3&starttime={2}&endtime={3}&type=historical"
    Private ReadOnly ALiceIntradayHistoricalURL = "https://ant.aliceblueonline.com/api/v1/charts?exchange={0}&token={1}&candletype=1&starttime={2}&endtime={3}&type=historical"
    Private ReadOnly tradingDay As Date = Date.MinValue
    Private ReadOnly _APIAdapter As APIAdapter
    Public Sub New(ByVal canceller As CancellationTokenSource, ByVal parentStrategy As NFOStrategy)
        _cts = canceller
        _parentStrategy = parentStrategy
        _userInputs = _parentStrategy.UserSettings
        tradingDay = Now

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

    Public Async Function GetInstrumentDataAsync(ByVal allInstruments As IEnumerable(Of IInstrument), ByVal bannedStock As List(Of String)) As Task(Of List(Of IInstrument))
        Dim ret As List(Of IInstrument) = Nothing
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
                Dim highATRStocks As Concurrent.ConcurrentBag(Of IInstrument) = Nothing
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
                                                                                                       Dim rawCashInstrument As IInstrument = allInstruments.ToList.Find(Function(x)
                                                                                                                                                                             Return x.TradingSymbol = y.RawInstrumentName
                                                                                                                                                                         End Function)
                                                                                                       If rawCashInstrument IsNot Nothing Then
                                                                                                           _cts.Token.ThrowIfCancellationRequested()
                                                                                                           'Dim eodHistoricalData As Dictionary(Of Date, OHLCPayload) = Await GetChartFromHistoricalAsync(rawCashInstrument, tradingDay.AddDays(-300), tradingDay.AddDays(-1), TypeOfData.EOD).ConfigureAwait(False)
                                                                                                           Dim eodHistoricalData As Dictionary(Of Date, OHLCPayload) = Await GetChartFromHistoricalAsync(rawCashInstrument, tradingDay.AddDays(-300), tradingDay, TypeOfData.EOD).ConfigureAwait(False)
                                                                                                           _cts.Token.ThrowIfCancellationRequested()
                                                                                                           If eodHistoricalData IsNot Nothing AndAlso eodHistoricalData.Count > 100 Then
                                                                                                               _cts.Token.ThrowIfCancellationRequested()
                                                                                                               If eodHistoricalData.LastOrDefault.Value.ClosePrice.Value >= _userInputs.MinimumStockPrice AndAlso
                                                                                                                    eodHistoricalData.LastOrDefault.Value.ClosePrice.Value <= _userInputs.MaximumStockPrice Then
                                                                                                                   Dim atrPayload As Dictionary(Of Date, Decimal) = Nothing
                                                                                                                   CalculateATR(14, eodHistoricalData, atrPayload)
                                                                                                                   _cts.Token.ThrowIfCancellationRequested()
                                                                                                                   Dim lastDayCandle As OHLCPayload = eodHistoricalData.LastOrDefault.Value
                                                                                                                   lastTradingDay = eodHistoricalData.LastOrDefault.Key
                                                                                                                   Dim atrPercentage As Decimal = (atrPayload(eodHistoricalData.LastOrDefault.Key) / lastDayCandle.ClosePrice.Value) * 100
                                                                                                                   If atrPercentage >= _userInputs.MinimumATRPercentage Then
                                                                                                                       If lastDayCandle.Volume.Value > lastDayCandle.PreviousPayload.Volume.Value Then
                                                                                                                           If lastDayCandle.Volume.Value > _userInputs.MinimumVolume Then
                                                                                                                               If highATRStocks Is Nothing Then highATRStocks = New Concurrent.ConcurrentBag(Of IInstrument)
                                                                                                                               highATRStocks.Add(rawCashInstrument)
                                                                                                                           End If
                                                                                                                       End If
                                                                                                                   End If
                                                                                                               End If
                                                                                                           End If
                                                                                                       End If
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
                    ret = highATRStocks.ToList
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