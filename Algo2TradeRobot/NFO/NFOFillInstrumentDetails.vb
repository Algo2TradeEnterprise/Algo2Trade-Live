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

#Region "Logging and Status Progress"
    Public Shared Shadows logger As Logger = LogManager.GetCurrentClassLogger
#End Region

#Region "Events/Event handlers"
    Public Event DocumentDownloadComplete()
    Public Event DocumentRetryStatus(ByVal currentTry As Integer, ByVal totalTries As Integer)
    Public Event Heartbeat(ByVal msg As String)
    Public Event HeartbeatSpecial(ByVal msg As String)
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
    Protected Overridable Sub OnHeartbeatSpecial(ByVal msg As String)
        RaiseEvent HeartbeatSpecial(msg)
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
    Private ReadOnly AliceEODHistoricalURL As String = "https://ant.aliceblueonline.com/api/v1/charts?exchange={0}&token={1}&candletype=3&starttime={2}&endtime={3}&type=historical"
    Private ReadOnly ALiceIntradayHistoricalURL As String = "https://ant.aliceblueonline.com/api/v1/charts?exchange={0}&token={1}&candletype=1&starttime={2}&endtime={3}&type=historical"
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

    Public Async Function GetInstrumentData(ByVal optionsInstruments As IEnumerable(Of IInstrument), ByVal spotInstrument As IInstrument) As Task(Of List(Of IInstrument))
        Dim ret As List(Of IInstrument) = Nothing
        If spotInstrument IsNot Nothing Then
            Dim eodPayload As Dictionary(Of Date, OHLCPayload) = Await GetChartFromHistoricalAsync(spotInstrument, Now.Date.AddMonths(-3), Now.Date, TypeOfData.EOD)
            If eodPayload IsNot Nothing AndAlso eodPayload.Count > 0 Then
                Dim hkPayload As Dictionary(Of Date, OHLCPayload) = Nothing
                ConvertToHeikenAshi(eodPayload, hkPayload)
                If hkPayload IsNot Nothing AndAlso hkPayload.Count > 0 Then
                    Dim lastTradingDay As Date = hkPayload.Keys.Where(Function(x)
                                                                          Return x.Date < Now.Date
                                                                      End Function).Max
                    If lastTradingDay <> Date.MinValue AndAlso lastTradingDay <> Date.MaxValue AndAlso lastTradingDay.Date <> Now.Date AndAlso
                        hkPayload.ContainsKey(lastTradingDay) Then
                        Dim instrumentType As String = Nothing
                        If hkPayload(lastTradingDay).CandleColor = Color.Green Then
                            instrumentType = "CE"
                        ElseIf hkPayload(lastTradingDay).CandleColor = Color.Red Then
                            instrumentType = "PE"
                        End If
                        If instrumentType IsNot Nothing AndAlso instrumentType.Trim <> "" Then
                            If optionsInstruments IsNot Nothing AndAlso optionsInstruments.Count > 0 Then
                                Dim minExpiry As Date = optionsInstruments.Min(Function(x)
                                                                                   If x.Expiry.Value.Date > Now.Date Then
                                                                                       Return x.Expiry.Value.Date
                                                                                   Else
                                                                                       Return Date.MaxValue
                                                                                   End If
                                                                               End Function)
                                _cts.Token.ThrowIfCancellationRequested()
                                If minExpiry <> Date.MaxValue AndAlso minExpiry <> Date.MinValue AndAlso minExpiry.Date > Now.Date Then
                                    Dim currentContracts As IEnumerable(Of IInstrument) = optionsInstruments.Where(Function(x)
                                                                                                                       Return x.Expiry.Value.Date = minExpiry.Date AndAlso
                                                                                                                       x.RawInstrumentType.ToUpper = instrumentType.ToUpper
                                                                                                                   End Function)

                                    _cts.Token.ThrowIfCancellationRequested()
                                    Dim volumeCheckContracts As IEnumerable(Of IInstrument) = Nothing
                                    If Now.DayOfWeek = DayOfWeek.Thursday Then
                                        volumeCheckContracts = optionsInstruments.Where(Function(x)
                                                                                            Return x.Expiry.Value.Date = Now.Date AndAlso
                                                                                            x.RawInstrumentType.ToUpper = instrumentType.ToUpper
                                                                                        End Function)
                                    Else
                                        volumeCheckContracts = currentContracts
                                    End If

                                    _cts.Token.ThrowIfCancellationRequested()
                                    If volumeCheckContracts IsNot Nothing AndAlso volumeCheckContracts.Count > 0 Then
                                        Dim messageLog As Dictionary(Of String, Tuple(Of String, Decimal, Decimal, Decimal, Boolean)) = New Dictionary(Of String, Tuple(Of String, Decimal, Decimal, Decimal, Boolean))
                                        Dim primarySelectedOptionData As Dictionary(Of String, Double) = New Dictionary(Of String, Double)
                                        For Each runningContract In volumeCheckContracts
                                            _cts.Token.ThrowIfCancellationRequested()
                                            Dim currentOptionContract As IInstrument = GetCurrentOptionContract(currentContracts, runningContract)
                                            If currentOptionContract IsNot Nothing Then
                                                Dim message As Tuple(Of String, Decimal, Decimal, Decimal, Boolean) = Nothing
                                                Dim optionIntradayPayload As Dictionary(Of Date, OHLCPayload) = Await GetChartFromHistoricalAsync(runningContract, lastTradingDay.Date, Now.Date, TypeOfData.Intraday)
                                                If optionIntradayPayload IsNot Nothing AndAlso optionIntradayPayload.Count > 0 Then
                                                    Dim numberOfBlankCandle As Integer = optionIntradayPayload.Where(Function(x)
                                                                                                                         Return x.Key.Date = lastTradingDay.Date AndAlso
                                                                                                                 (x.Value.Volume.Value = 0 OrElse
                                                                                                                 x.Value.HighPrice.Value = x.Value.LowPrice.Value)
                                                                                                                     End Function).Count
                                                    Dim totalCount As Integer = optionIntradayPayload.Where(Function(x)
                                                                                                                Return x.Key.Date = lastTradingDay.Date
                                                                                                            End Function).Count

                                                    Dim blankCandlePer As Decimal = (numberOfBlankCandle / 375) * 100
                                                    Dim nonBlankCandlePer As Decimal = Math.Round(100 - blankCandlePer, 4)
                                                    Dim totalCandlePer As Decimal = Math.Round((totalCount / 375) * 100, 4)
                                                    If totalCandlePer >= _userInputs.MinTotalCandlePercentage AndAlso
                                                        nonBlankCandlePer >= _userInputs.MinNonBlankCandlePercentage Then
                                                        Dim optionEODPayload As Dictionary(Of Date, OHLCPayload) = Await GetChartFromHistoricalAsync(runningContract, lastTradingDay.Date, Now.Date, TypeOfData.EOD)
                                                        If optionEODPayload IsNot Nothing AndAlso optionEODPayload.ContainsKey(lastTradingDay) Then
                                                            primarySelectedOptionData.Add(currentOptionContract.InstrumentIdentifier, optionEODPayload(lastTradingDay).Volume.Value * optionEODPayload(lastTradingDay).ClosePrice.Value)
                                                        End If
                                                        message = New Tuple(Of String, Decimal, Decimal, Decimal, Boolean)(Nothing, totalCandlePer, nonBlankCandlePer, 0, False)
                                                    Else
                                                        message = New Tuple(Of String, Decimal, Decimal, Decimal, Boolean)("Total candle/Non-blank candle less than min value", totalCandlePer, nonBlankCandlePer, 0, False)
                                                    End If
                                                Else
                                                    message = New Tuple(Of String, Decimal, Decimal, Decimal, Boolean)("No historical candle found", 0, 0, 0, False)
                                                End If

                                                messageLog.Add(currentOptionContract.InstrumentIdentifier, message)
                                            Else
                                                If Not MessageBox.Show(String.Format("Unable to find current option contract for {0}{1}Do you want to procced ?", runningContract.TradingSymbol, vbNewLine), "Adaptive Martingale", MessageBoxButtons.YesNo, MessageBoxIcon.Error) = DialogResult.Yes Then
                                                    Throw New ApplicationException(String.Format("Unable to find current option contract for {0}", runningContract.TradingSymbol))
                                                End If
                                            End If
                                        Next
                                        If primarySelectedOptionData IsNot Nothing AndAlso primarySelectedOptionData.Count > 0 Then
                                            Dim avgTurnover As Double = primarySelectedOptionData.Values.Average
                                            For Each runningContract In primarySelectedOptionData
                                                _cts.Token.ThrowIfCancellationRequested()
                                                Dim message As Tuple(Of String, Decimal, Decimal, Decimal, Boolean) = messageLog(runningContract.Key)
                                                Dim turnoverPer As Double = Math.Round(runningContract.Value * 100 / avgTurnover, 4)
                                                If turnoverPer >= _userInputs.MinEODTurnoverPercentage Then
                                                    Dim currentInstrument As IInstrument = currentContracts.ToList.Find(Function(x)
                                                                                                                            Return x.InstrumentIdentifier = runningContract.Key
                                                                                                                        End Function)
                                                    If currentInstrument IsNot Nothing Then
                                                        If ret Is Nothing Then ret = New List(Of IInstrument)
                                                        ret.Add(currentInstrument)
                                                    End If
                                                    messageLog(runningContract.Key) = New Tuple(Of String, Decimal, Decimal, Decimal, Boolean)("SELECTED", message.Item2, message.Item3, turnoverPer, True)
                                                Else
                                                    messageLog(runningContract.Key) = New Tuple(Of String, Decimal, Decimal, Decimal, Boolean)("EOD Turnover less than min value", message.Item2, message.Item3, turnoverPer, False)
                                                End If
                                            Next
                                            If ret IsNot Nothing AndAlso ret.Count > 0 Then
                                                Dim htmlString As String = String.Format("<html>{0}<head>{0}<style>", vbNewLine)

                                                Dim styleString As String = "table, th, td {  border: 1px solid black;  border-collapse: collapse;}"
                                                htmlString = String.Format("{0}{1}{2}{1}</head>", htmlString, vbNewLine, styleString)

                                                htmlString = String.Format("{0}{1}</style>{1}</head>", htmlString, vbNewLine)
                                                htmlString = String.Format("{0}{1}{1}<body>{1}<table>", htmlString, vbNewLine)

                                                'Header
                                                htmlString = String.Format("{0}{1}<tr>", htmlString, vbNewLine)
                                                htmlString = String.Format("{0}{1}<th>Trading Symbol</th>", htmlString, vbNewLine)
                                                htmlString = String.Format("{0}{1}<th>Remarks</th>", htmlString, vbNewLine)
                                                htmlString = String.Format("{0}{1}<th>Total Candle%</th>", htmlString, vbNewLine)
                                                htmlString = String.Format("{0}{1}<th>Non-Blank Candle%</th>", htmlString, vbNewLine)
                                                htmlString = String.Format("{0}{1}<th>EOD Turnover%</th>", htmlString, vbNewLine)
                                                htmlString = String.Format("{0}{1}</tr>", htmlString, vbNewLine)

                                                For Each runningMessage In messageLog.OrderByDescending(Function(x)
                                                                                                            Return x.Value.Item4
                                                                                                        End Function).ThenByDescending(Function(y)
                                                                                                                                           Return y.Value.Item3
                                                                                                                                       End Function).ThenByDescending(Function(z)
                                                                                                                                                                          Return z.Value.Item2
                                                                                                                                                                      End Function)
                                                    Dim contractAvailable As IInstrument = currentContracts.ToList.Find(Function(x)
                                                                                                                            Return x.InstrumentIdentifier = runningMessage.Key
                                                                                                                        End Function)
                                                    If contractAvailable IsNot Nothing Then
                                                        htmlString = String.Format("{0}{1}<tr>", htmlString, vbNewLine)
                                                        htmlString = String.Format("{0}{1}<td>{2}</td>", htmlString, vbNewLine, contractAvailable.TradingSymbol)
                                                        htmlString = String.Format("{0}{1}<td>{2}</td>", htmlString, vbNewLine, runningMessage.Value.Item1)
                                                        htmlString = String.Format("{0}{1}<td>{2}</td>", htmlString, vbNewLine, runningMessage.Value.Item2)
                                                        htmlString = String.Format("{0}{1}<td>{2}</td>", htmlString, vbNewLine, runningMessage.Value.Item3)
                                                        htmlString = String.Format("{0}{1}<td>{2}</td>", htmlString, vbNewLine, runningMessage.Value.Item4)
                                                        htmlString = String.Format("{0}{1}</tr>", htmlString, vbNewLine)
                                                    End If
                                                Next
                                                htmlString = String.Format("{0}{1}</table>{1}{1}</body>{1}</html>", htmlString, vbNewLine)
                                                Await SendTelegramDebugMessageAsync(htmlString, True).ConfigureAwait(False)
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
            End If
        End If
        Dim infoMessage As String = Nothing
        If ret IsNot Nothing AndAlso ret.Count > 0 Then
            Dim selectedInstruments As String = Nothing
            For Each runningInstrument In ret
                selectedInstruments = String.Format("{0} {1},", selectedInstruments, runningInstrument.TradingSymbol)
            Next
            selectedInstruments = selectedInstruments.Trim
            selectedInstruments = selectedInstruments.Substring(0, selectedInstruments.Count - 1)
            infoMessage = String.Format("{0}: Number of option selected = {1}.{2}{3}", spotInstrument.TradingSymbol, ret.Count, vbNewLine, selectedInstruments)
        Else
            infoMessage = String.Format("{0}: No option selected", spotInstrument.TradingSymbol)
        End If
        OnHeartbeatSpecial(infoMessage)
        Await SendTelegramDebugMessageAsync(infoMessage, False).ConfigureAwait(False)
        Await SendTelegramInfoMessageAsync(infoMessage, False).ConfigureAwait(False)

        Return ret
    End Function

    Private Function GetCurrentOptionContract(ByVal currentContracts As IEnumerable(Of IInstrument), ByVal previousExiryInstrument As IInstrument) As IInstrument
        Dim ret As IInstrument = Nothing
        Dim previousExpiryStrike As Decimal = Decimal.MinValue
        If previousExiryInstrument IsNot Nothing Then
            Dim lastDayOfTheMonth As Date = New Date(previousExiryInstrument.Expiry.Value.Year, previousExiryInstrument.Expiry.Value.Month,
                                                 Date.DaysInMonth(previousExiryInstrument.Expiry.Value.Year, previousExiryInstrument.Expiry.Value.Month))
            Dim lastThursDayOfTheMonth As Date = lastDayOfTheMonth
            While lastThursDayOfTheMonth.DayOfWeek <> DayOfWeek.Thursday
                lastThursDayOfTheMonth = lastThursDayOfTheMonth.AddDays(-1)
            End While

            If previousExiryInstrument.Expiry.Value.Date = lastThursDayOfTheMonth.Date Then
                If IsNumeric(previousExiryInstrument.TradingSymbol.Split(" ")(2).Trim) Then
                    previousExpiryStrike = Val(previousExiryInstrument.TradingSymbol.Split(" ")(2).Trim)
                End If
            Else
                If IsNumeric(previousExiryInstrument.TradingSymbol.Split(" ")(3).Trim) Then
                    previousExpiryStrike = Val(previousExiryInstrument.TradingSymbol.Split(" ")(3).Trim)
                End If
            End If
        End If
        If previousExpiryStrike <> Decimal.MinValue Then
            For Each runningContact In currentContracts
                Dim lastDayOfTheMonth As Date = New Date(runningContact.Expiry.Value.Year, runningContact.Expiry.Value.Month,
                                                 Date.DaysInMonth(runningContact.Expiry.Value.Year, runningContact.Expiry.Value.Month))
                Dim lastThursDayOfTheMonth As Date = lastDayOfTheMonth
                While lastThursDayOfTheMonth.DayOfWeek <> DayOfWeek.Thursday
                    lastThursDayOfTheMonth = lastThursDayOfTheMonth.AddDays(-1)
                End While

                Dim currentStrike As Decimal = Decimal.MinValue
                If runningContact.Expiry.Value.Date = lastThursDayOfTheMonth.Date Then
                    If IsNumeric(runningContact.TradingSymbol.Split(" ")(2).Trim) Then
                        currentStrike = Val(runningContact.TradingSymbol.Split(" ")(2).Trim)
                    End If
                Else
                    If IsNumeric(runningContact.TradingSymbol.Split(" ")(3).Trim) Then
                        currentStrike = Val(runningContact.TradingSymbol.Split(" ")(3).Trim)
                    End If
                End If
                If currentStrike <> Decimal.MinValue AndAlso currentStrike = previousExpiryStrike Then
                    ret = runningContact
                    Exit For
                End If
            Next
        End If
        Return ret
    End Function

    Private Async Function GetHistoricalCandleStickAsync(ByVal instrument As IInstrument, ByVal fromDate As Date, ByVal toDate As Date, ByVal historicalDataType As TypeOfData) As Task(Of Dictionary(Of String, Object))
        Dim ret As Dictionary(Of String, Object) = Nothing
        _cts.Token.ThrowIfCancellationRequested()
        Dim historicalDataURL As String = Nothing
        If instrument.Segment.ToUpper = "INDICES" Then
            Select Case historicalDataType
                Case TypeOfData.Intraday
                    historicalDataURL = String.Format(ALiceIntradayHistoricalURL.Replace("token", "name"), String.Format("{0}_{1}", instrument.RawExchange, instrument.Segment), instrument.TradingSymbol, DateTimeToUnix(fromDate), DateTimeToUnix(toDate))
                Case TypeOfData.EOD
                    historicalDataURL = String.Format(AliceEODHistoricalURL.Replace("token", "name"), String.Format("{0}_{1}", instrument.RawExchange, instrument.Segment), instrument.TradingSymbol, DateTimeToUnix(fromDate), DateTimeToUnix(toDate))
            End Select
        Else
            Select Case historicalDataType
                Case TypeOfData.Intraday
                    historicalDataURL = String.Format(ALiceIntradayHistoricalURL, instrument.RawExchange, instrument.InstrumentIdentifier, DateTimeToUnix(fromDate), DateTimeToUnix(toDate))
                Case TypeOfData.EOD
                    historicalDataURL = String.Format(AliceEODHistoricalURL, instrument.RawExchange, instrument.InstrumentIdentifier, DateTimeToUnix(fromDate), DateTimeToUnix(toDate))
            End Select
        End If
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

    Private Async Function SendTelegramDebugMessageAsync(ByVal message As String, ByVal convertToImage As Boolean) As Task
        Try
            Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
            _cts.Token.ThrowIfCancellationRequested()
            If _userInputs.TelegramBotAPIKey IsNot Nothing AndAlso Not _userInputs.TelegramBotAPIKey.Trim = "" AndAlso
                _userInputs.TelegramDebugChatID IsNot Nothing AndAlso Not _userInputs.TelegramDebugChatID.Trim = "" Then
                Using tSender As New Utilities.Notification.Telegram(_userInputs.TelegramBotAPIKey.Trim, _userInputs.TelegramDebugChatID.Trim, _cts)
                    If convertToImage Then
                        Dim render As Utilities.HTMLRender.Render = New Utilities.HTMLRender.Render(_cts)
                        Dim messageImage As Image = Await render.ConvertHTMLStringToImage(message).ConfigureAwait(False)
                        Dim stream = New System.IO.MemoryStream()
                        messageImage.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg)
                        stream.Position = 0

                        Await tSender.SendDocumentGetAsync(stream, "Day Beginning Strike Selection.jpeg", String.Format("Timestamp: {0}", Now.ToString("HH:mm:ss"))).ConfigureAwait(False)
                    Else
                        Dim encodedString As String = Utilities.Strings.UrlEncodeString(message)
                        Await tSender.SendMessageGetAsync(encodedString).ConfigureAwait(False)
                    End If
                End Using
            End If
        Catch ex As Exception
            logger.Warn(ex.ToString)
        End Try
    End Function

    Private Async Function SendTelegramInfoMessageAsync(ByVal message As String, ByVal convertToImage As Boolean) As Task
        Try
            Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
            _cts.Token.ThrowIfCancellationRequested()
            If _userInputs.TelegramBotAPIKey IsNot Nothing AndAlso Not _userInputs.TelegramBotAPIKey.Trim = "" AndAlso
                _userInputs.TelegramInfoChatID IsNot Nothing AndAlso Not _userInputs.TelegramInfoChatID.Trim = "" Then
                Using tSender As New Utilities.Notification.Telegram(_userInputs.TelegramBotAPIKey.Trim, _userInputs.TelegramInfoChatID.Trim, _cts)
                    If convertToImage Then
                        Dim render As Utilities.HTMLRender.Render = New Utilities.HTMLRender.Render(_cts)
                        Dim messageImage As Image = Await render.ConvertHTMLStringToImage(message).ConfigureAwait(False)
                        Dim stream = New System.IO.MemoryStream()
                        messageImage.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg)
                        stream.Position = 0

                        Await tSender.SendDocumentGetAsync(stream, "Day Beginning Strike Selection.jpeg", String.Format("Timestamp: {0}", Now.ToString("HH:mm:ss"))).ConfigureAwait(False)
                    Else
                        Dim encodedString As String = Utilities.Strings.UrlEncodeString(message)
                        Await tSender.SendMessageGetAsync(encodedString).ConfigureAwait(False)
                    End If
                End Using
            End If
        Catch ex As Exception
            logger.Warn(ex.ToString)
        End Try
    End Function
End Class