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
                                        Dim optionContracts As List(Of IInstrument) = New List(Of IInstrument)
                                        For Each runningContract In volumeCheckContracts
                                            _cts.Token.ThrowIfCancellationRequested()
                                            Dim message As String = Nothing
                                            Dim optionPayload As Dictionary(Of Date, OHLCPayload) = Await GetChartFromHistoricalAsync(runningContract, lastTradingDay.Date, Now.Date, TypeOfData.Intraday)
                                            If optionPayload IsNot Nothing AndAlso optionPayload.Count > 0 Then
                                                Dim numberOfBlankCandle As Integer = optionPayload.Where(Function(x)
                                                                                                             Return x.Key.Date = lastTradingDay.Date AndAlso
                                                                                                             (x.Value.Volume.Value = 0 OrElse
                                                                                                             x.Value.HighPrice.Value = x.Value.LowPrice.Value)
                                                                                                         End Function).Count
                                                Dim totalCount As Integer = optionPayload.Where(Function(x)
                                                                                                    Return x.Key.Date = lastTradingDay.Date
                                                                                                End Function).Count

                                                Dim blankCandlePer As Decimal = (numberOfBlankCandle / 375) * 100
                                                Dim totalCandlePer As Decimal = (totalCount / 375) * 100
                                                message = String.Format("{0}: Total Candle = {1}, Blank Candle = {2}, Total Candle% = {3}, Blank Candle% = {4}. [INFO2]. Selected #[INFO3]",
                                                                        "[INFO1]",
                                                                        totalCount,
                                                                        numberOfBlankCandle,
                                                                        Math.Round(totalCandlePer, 4),
                                                                        Math.Round(blankCandlePer, 4))

                                                If totalCandlePer >= _userInputs.MinTotalCandlePercentage AndAlso
                                                    blankCandlePer <= _userInputs.MaxBlankCandlePercentage Then
                                                    If Now.DayOfWeek = DayOfWeek.Thursday Then
                                                        Dim currentOptionContract As IInstrument = GetCurrentOptionContract(currentContracts, runningContract)
                                                        If currentOptionContract IsNot Nothing Then
                                                            optionContracts.Add(currentOptionContract)
                                                            message = message.Replace("[INFO1]", currentOptionContract.TradingSymbol)
                                                        Else
                                                            Throw New ApplicationException(String.Format("Unable to find current option contract for {0}", runningContract.TradingSymbol))
                                                        End If
                                                    Else
                                                        optionContracts.Add(runningContract)
                                                        message = message.Replace("[INFO1]", runningContract.TradingSymbol)
                                                    End If
                                                    message = message.Replace("[INFO2]", "SELECTED")
                                                Else
                                                    message = message.Replace("[INFO1]", runningContract.TradingSymbol)
                                                    message = message.Replace("[INFO2]", "NOT SELECTED")
                                                End If

                                                message = message.Replace("[INFO3]", String.Format("{0}/{1}", optionContracts.Count, volumeCheckContracts.Count))
                                            Else
                                                message = String.Format("{0}: No historical candle found. NOT SELECTED. Selected #{1}/{2}",
                                                                        runningContract.TradingSymbol, optionContracts.Count, volumeCheckContracts.Count)
                                            End If

                                            OnHeartbeatSpecial(message)
                                            Await SendTelegramMessageAsync(message).ConfigureAwait(False)
                                        Next


                                        _cts.Token.ThrowIfCancellationRequested()
                                        If optionContracts IsNot Nothing AndAlso optionContracts.Count > 0 Then
                                            For Each runningIntrument In optionContracts
                                                _cts.Token.ThrowIfCancellationRequested()
                                                If ret Is Nothing Then ret = New List(Of IInstrument)
                                                ret.Add(runningIntrument)
                                            Next
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
            End If
        End If
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

    Private Async Function SendTelegramMessageAsync(ByVal message As String) As Task
        Try
            Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
            _cts.Token.ThrowIfCancellationRequested()
            If _parentStrategy.ParentController.UserInputs.TelegramAPIKey IsNot Nothing AndAlso
                Not _parentStrategy.ParentController.UserInputs.TelegramAPIKey.Trim = "" AndAlso
                _parentStrategy.ParentController.UserInputs.TelegramChatID IsNot Nothing AndAlso
                Not _parentStrategy.ParentController.UserInputs.TelegramChatID.Trim = "" Then
                Using tSender As New Utilities.Notification.Telegram(_parentStrategy.ParentController.UserInputs.TelegramAPIKey.Trim, _parentStrategy.ParentController.UserInputs.TelegramChatID.Trim, _cts)
                    Dim encodedString As String = Utilities.Strings.UrlEncodeString(message)
                    Await tSender.SendMessageGetAsync(encodedString).ConfigureAwait(False)
                End Using
            End If
        Catch ex As Exception
            logger.Warn(ex.ToString)
        End Try
    End Function
End Class