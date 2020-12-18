Imports NLog
Imports System.Net.Http
Imports System.Threading
Imports Algo2TradeCore.Controller
Imports Algo2TradeCore.Entities
Imports Algo2TradeCore.Strategies
Imports Utilities.Network
Imports HtmlAgilityPack
Imports System.IO

Public Class NFOStrategy
    Inherits Strategy
#Region "Logging and Status Progress"
    Public Shared Shadows logger As Logger = LogManager.GetCurrentClassLogger
#End Region

    Public Property EligibleInstruments As Concurrent.ConcurrentBag(Of NFOStrategyInstrument)
    Public Property TotalActiveInstrumentCount As Integer
    Public ReadOnly Property NSEHolidays As List(Of Date)

    Public Sub New(ByVal associatedParentController As APIStrategyController,
                   ByVal strategyIdentifier As String,
                   ByVal userSettings As NFOUserInputs,
                   ByVal maxNumberOfDaysForHistoricalFetch As Integer,
                   ByVal canceller As CancellationTokenSource)
        MyBase.New(associatedParentController, strategyIdentifier, False, userSettings, maxNumberOfDaysForHistoricalFetch, canceller)
        'Though the TradableStrategyInstruments is being populated from inside by newing it,
        'lets also initiatilize here so that after creation of the strategy and before populating strategy instruments,
        'the fron end grid can bind to this created TradableStrategyInstruments which will be empty
        'TradableStrategyInstruments = New List(Of StrategyInstrument)
    End Sub

    ''' <summary>
    ''' This function will fill the instruments based on the stratgey used and also create the workers
    ''' </summary>
    ''' <param name="allInstruments"></param>
    ''' <returns></returns>
    Public Overrides Async Function CreateTradableStrategyInstrumentsAsync(ByVal allInstruments As IEnumerable(Of IInstrument), ByVal bannedInstruments As List(Of String)) As Task(Of Boolean)
        If allInstruments IsNot Nothing AndAlso allInstruments.Count > 0 Then
            logger.Debug("CreateTradableStrategyInstrumentsAsync, allInstruments.Count:{0}", allInstruments.Count)
        Else
            logger.Debug("CreateTradableStrategyInstrumentsAsync, allInstruments.Count:Nothing or 0")
        End If
        _cts.Token.ThrowIfCancellationRequested()
        Dim ret As Boolean = False
        Dim retTradableInstrumentsAsPerStrategy As List(Of IInstrument) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        logger.Debug("Starting to fill strategy specific instruments, strategy:{0}", Me.ToString)
        If allInstruments IsNot Nothing AndAlso allInstruments.Count > 0 Then
            _NSEHolidays = Await GetNSEEquityHolidaysAsync().ConfigureAwait(False)

            Dim userInputs As NFOUserInputs = Me.UserSettings
            If userInputs.InstrumentsData IsNot Nothing AndAlso userInputs.InstrumentsData.Count > 0 Then
                Dim dummyAllInstruments As List(Of IInstrument) = allInstruments.ToList
                For Each instrument In userInputs.InstrumentsData
                    _cts.Token.ThrowIfCancellationRequested()
                    Dim runningTradableInstrument As IInstrument = dummyAllInstruments.Find(Function(x)
                                                                                                Return x.TradingSymbol = instrument.Value.TradingSymbol
                                                                                            End Function)

                    _cts.Token.ThrowIfCancellationRequested()
                    If retTradableInstrumentsAsPerStrategy Is Nothing Then retTradableInstrumentsAsPerStrategy = New List(Of IInstrument)
                    If runningTradableInstrument IsNot Nothing Then
                        retTradableInstrumentsAsPerStrategy.Add(runningTradableInstrument)
                        ret = True
                    End If
                Next
                TradableInstrumentsAsPerStrategy = retTradableInstrumentsAsPerStrategy
            End If
        End If

        If retTradableInstrumentsAsPerStrategy IsNot Nothing AndAlso retTradableInstrumentsAsPerStrategy.Count > 0 Then
            'tradableInstrumentsAsPerStrategy = tradableInstrumentsAsPerStrategy.Take(5).ToList
            'Now create the strategy tradable instruments
            Dim retTradableStrategyInstruments As List(Of NFOStrategyInstrument) = Nothing
            logger.Debug("Creating strategy tradable instruments, _tradableInstruments.count:{0}", retTradableInstrumentsAsPerStrategy.Count)
            'Remove the old handlers from the previous strategyinstruments collection
            If TradableStrategyInstruments IsNot Nothing AndAlso TradableStrategyInstruments.Count > 0 Then
                For Each runningTradableStrategyInstruments In TradableStrategyInstruments
                    RemoveHandler runningTradableStrategyInstruments.HeartbeatEx, AddressOf OnHeartbeatEx
                    RemoveHandler runningTradableStrategyInstruments.WaitingForEx, AddressOf OnWaitingForEx
                    RemoveHandler runningTradableStrategyInstruments.DocumentRetryStatusEx, AddressOf OnDocumentRetryStatusEx
                    RemoveHandler runningTradableStrategyInstruments.DocumentDownloadCompleteEx, AddressOf OnDocumentDownloadCompleteEx
                Next
                TradableStrategyInstruments = Nothing
            End If

            'Now create the fresh handlers
            For Each runningTradableInstrument In retTradableInstrumentsAsPerStrategy
                _cts.Token.ThrowIfCancellationRequested()
                If retTradableStrategyInstruments Is Nothing Then retTradableStrategyInstruments = New List(Of NFOStrategyInstrument)
                Dim runningTradableStrategyInstrument As New NFOStrategyInstrument(runningTradableInstrument, Me, False, _cts)
                AddHandler runningTradableStrategyInstrument.HeartbeatEx, AddressOf OnHeartbeatEx
                AddHandler runningTradableStrategyInstrument.WaitingForEx, AddressOf OnWaitingForEx
                AddHandler runningTradableStrategyInstrument.DocumentRetryStatusEx, AddressOf OnDocumentRetryStatusEx
                AddHandler runningTradableStrategyInstrument.DocumentDownloadCompleteEx, AddressOf OnDocumentDownloadCompleteEx

                retTradableStrategyInstruments.Add(runningTradableStrategyInstrument)
                'If runningTradableInstrument.FirstLevelConsumers Is Nothing Then runningTradableInstrument.FirstLevelConsumers = New List(Of StrategyInstrument)
                'runningTradableInstrument.FirstLevelConsumers.Add(runningTradableStrategyInstrument)
            Next
            TradableStrategyInstruments = retTradableStrategyInstruments
        Else
            Throw New ApplicationException(String.Format("Cannot run this strategy as no strategy instruments could be created from the tradable instruments, stratgey:{0}", Me.ToString))
        End If

        Return ret
    End Function

    Public Overrides Async Function MonitorAsync() As Task
        Dim lastException As Exception = Nothing

        Try
            EligibleInstruments = New Concurrent.ConcurrentBag(Of NFOStrategyInstrument)
            _cts.Token.ThrowIfCancellationRequested()
            Dim tasks As New List(Of Task)()
            For Each tradableStrategyInstrument As NFOStrategyInstrument In TradableStrategyInstruments
                _cts.Token.ThrowIfCancellationRequested()
                tasks.Add(Task.Run(AddressOf tradableStrategyInstrument.MonitorAsync, _cts.Token))
            Next
            tasks.Add(Task.Run(AddressOf CheckEligibilityAsync, _cts.Token))
            Await Task.WhenAll(tasks).ConfigureAwait(False)
        Catch ex As Exception
            lastException = ex
            logger.Error(ex)
        End Try
        If lastException IsNot Nothing Then
            Await ParentController.CloseTickerIfConnectedAsync().ConfigureAwait(False)
            Await ParentController.CloseFetcherIfConnectedAsync(False).ConfigureAwait(False)
            Await ParentController.CloseCollectorIfConnectedAsync(False).ConfigureAwait(False)
            Throw lastException
        End If
    End Function

    Public Overrides Function ToString() As String
        Return Me.GetType().Name
    End Function

    Protected Overrides Function IsTriggerReceivedForExitAllOrders() As Tuple(Of Boolean, String)
        Dim ret As Tuple(Of Boolean, String) = Nothing
        Return ret
    End Function

    Private Async Function GetNSEEquityHolidaysAsync() As Task(Of List(Of Date))
        Dim ret As List(Of Date) = Nothing
        Dim holidayURL As String = "https://www1.nseindia.com/global/content/market_timings_holidays/market_timings_holidays.jsp?pageName=0&dateRange=&fromDate={0}&toDate={1}&tabActive=trading&load=false"
        Dim futureHolidayURL As String = String.Format(holidayURL, Now.ToString("dd-MM-yyyy"), Now.AddDays(15).ToString("dd-MM-yyyy"))
        Dim outputResponse As HtmlDocument = Nothing
        HttpBrowser.KillCookies()
        Using browser As New HttpBrowser(Nothing, Net.DecompressionMethods.GZip, New TimeSpan(0, 1, 0), _cts)
            AddHandler browser.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete
            AddHandler browser.Heartbeat, AddressOf OnHeartbeat
            AddHandler browser.WaitingFor, AddressOf OnWaitingFor
            AddHandler browser.DocumentRetryStatus, AddressOf OnDocumentRetryStatus

            browser.KeepAlive = True
            Dim headersToBeSent As New Dictionary(Of String, String)
            headersToBeSent.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9")
            headersToBeSent.Add("Accept-Encoding", "gzip, deflate, br")
            headersToBeSent.Add("Accept-Language", "en-US,en;q=0.9")
            headersToBeSent.Add("Host", "www1.nseindia.com")
            headersToBeSent.Add("Sec-Fetch-Dest", "document")
            headersToBeSent.Add("Upgrade-Insecure-Requests", "1")
            headersToBeSent.Add("Sec-Fetch-Mode", "navigate")
            headersToBeSent.Add("Sec-Fetch-Site", "none")
            headersToBeSent.Add("Sec-Fetch-User", "?1")

            Dim l As Tuple(Of Uri, Object) = Await browser.NonPOSTRequestAsync(futureHolidayURL, HttpMethod.Get, Nothing, False, headersToBeSent, True, "text/html").ConfigureAwait(False)
            If l IsNot Nothing AndAlso l.Item2 IsNot Nothing Then
                outputResponse = l.Item2
            End If
            RemoveHandler browser.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete
            RemoveHandler browser.Heartbeat, AddressOf OnHeartbeat
            RemoveHandler browser.WaitingFor, AddressOf OnWaitingFor
            RemoveHandler browser.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
        End Using
        If outputResponse IsNot Nothing AndAlso outputResponse.DocumentNode IsNot Nothing Then
            If outputResponse.DocumentNode.SelectNodes("//table") IsNot Nothing AndAlso outputResponse.DocumentNode.SelectNodes("//table").Count = 1 Then
                Dim table As HtmlNode = outputResponse.DocumentNode.SelectNodes("//table")(0)
                If table IsNot Nothing And table.SelectNodes("tr") IsNot Nothing AndAlso table.SelectNodes("tr").Count > 1 Then
                    _cts.Token.ThrowIfCancellationRequested()
                    If table.SelectNodes("//td[@class='number']") IsNot Nothing AndAlso table.SelectNodes("//td[@class='number']").Count Then
                        For Each runningData As HtmlNode In table.SelectNodes("//td[@class='number']")
                            Dim holiday As Date = Date.ParseExact(runningData.InnerText, "dd-MMM-yyyy", Nothing)
                            If ret Is Nothing Then ret = New List(Of Date)
                            ret.Add(holiday)
                        Next
                    End If
                End If
            End If
        End If
        Return ret
    End Function

    Private Async Function CheckEligibilityAsync() As Task
        Try
            Dim userInput As NFOUserInputs = Me.UserSettings
            While True
                If Me.ParentController.OrphanException IsNot Nothing Then
                    Throw Me.ParentController.OrphanException
                End If

                If Now >= userInput.TradeEntryTime Then
                    If Me.EligibleInstruments IsNot Nothing AndAlso Me.EligibleInstruments.Count > 0 Then
                        Await Task.Delay(1000).ConfigureAwait(False)
                        For Each runningStock In userInput.InstrumentsData
                            Dim instrument As NFOStrategyInstrument = Me.EligibleInstruments.Where(Function(x)
                                                                                                       Return x.TradableInstrument.TradingSymbol = runningStock.Key.ToUpper.Trim
                                                                                                   End Function).FirstOrDefault
                            If instrument IsNot Nothing Then
                                instrument.TakeTradeToday = True
                                Me.TotalActiveInstrumentCount += 1
                                runningStock.Value.TradingDay = Now.DayOfWeek.ToString
                                Exit For
                            End If
                        Next

                        If File.Exists(userInput.InstrumentDetailsFilePath) Then
                            File.Delete(userInput.InstrumentDetailsFilePath)
                            Dim dt As DataTable = New DataTable
                            dt.Columns.Add("Trading Symbol")
                            dt.Columns.Add("Trading Day")
                            For Each runningStock In userInput.InstrumentsData
                                Dim row As DataRow = dt.NewRow
                                row("Trading Symbol") = runningStock.Value.TradingSymbol.Trim
                                row("Trading Day") = runningStock.Value.TradingDay.Trim
                                dt.Rows.Add(row)
                            Next

                            Using csv As New Utilities.DAL.CSVHelper(userInput.InstrumentDetailsFilePath, ",", _cts)
                                csv.GetCSVFromDataTable(dt)
                            End Using
                        End If

                        Exit While
                    End If
                End If
                Await Task.Delay(1000).ConfigureAwait(False)
            End While
        Catch ex As Exception
            logger.Error(ex.ToString)
            Throw ex
        End Try
    End Function
End Class
