Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Threading
Imports Algo2TradeCore.Controller
Imports Algo2TradeCore.Entities
Imports Algo2TradeCore.Strategies
Imports NLog

Public Class NFOStrategy
    Inherits Strategy
#Region "Logging and Status Progress"
    Public Shared Shadows logger As Logger = LogManager.GetCurrentClassLogger
#End Region

    Public Sub New(ByVal associatedParentController As APIStrategyController,
                   ByVal strategyIdentifier As String,
                   ByVal userSettings As NFOUserInputs,
                   ByVal maxNumberOfDaysForHistoricalFetch As Integer,
                   ByVal canceller As CancellationTokenSource)
        MyBase.New(associatedParentController, strategyIdentifier, False, userSettings, maxNumberOfDaysForHistoricalFetch, canceller, False)
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
        Dim sheetForToken As Dictionary(Of String, String) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        logger.Debug("Starting to fill strategy specific instruments, strategy:{0}", Me.ToString)
        If allInstruments IsNot Nothing AndAlso allInstruments.Count > 0 Then
            Dim userInputs As NFOUserInputs = Me.UserSettings
            If userInputs.InstrumentsData IsNot Nothing AndAlso userInputs.InstrumentsData.Count > 0 Then
                Dim dummyAllInstruments As List(Of IInstrument) = allInstruments.ToList
                For Each instrument In userInputs.InstrumentsData
                    _cts.Token.ThrowIfCancellationRequested()
                    Dim allTradableInstruments As List(Of IInstrument) = dummyAllInstruments.FindAll(Function(x)
                                                                                                         Return x.RawInstrumentName = instrument.Value.RawInstrumentName AndAlso
                                                                                                         x.InstrumentType = IInstrument.TypeOfInstrument.Futures
                                                                                                     End Function)
                    If allTradableInstruments IsNot Nothing AndAlso allTradableInstruments.Count > 0 Then
                        Dim counter As Integer = 0
                        For Each runningExpiryInstrument In allTradableInstruments.OrderBy(Function(x)
                                                                                               Return x.Expiry.Value
                                                                                           End Function)
                            counter += 1
                            If (counter = 1 AndAlso instrument.Value.ContractType = NFOUserInputs.InstrumentDetails.TypeOfContract.Current) OrElse
                                (counter = 2 AndAlso instrument.Value.ContractType = NFOUserInputs.InstrumentDetails.TypeOfContract.Near) OrElse
                                (counter = 3 AndAlso instrument.Value.ContractType = NFOUserInputs.InstrumentDetails.TypeOfContract.Future) Then
                                _cts.Token.ThrowIfCancellationRequested()
                                If retTradableInstrumentsAsPerStrategy Is Nothing Then retTradableInstrumentsAsPerStrategy = New List(Of IInstrument)
                                retTradableInstrumentsAsPerStrategy.Add(runningExpiryInstrument)
                                If sheetForToken Is Nothing Then sheetForToken = New Dictionary(Of String, String)
                                sheetForToken.Add(runningExpiryInstrument.InstrumentIdentifier, instrument.Value.SheetName)
                                ret = True

                                Dim optionName As String = String.Format("{0}{1}*", runningExpiryInstrument.RawInstrumentName, runningExpiryInstrument.Expiry.Value.ToString("yyMMM")).ToUpper
                                Dim allOptionTradableInstruments As List(Of IInstrument) = dummyAllInstruments.FindAll(Function(x)
                                                                                                                           Return Regex.Match(x.TradingSymbol, optionName).Success AndAlso
                                                                                                                           Regex.Match(x.TradingSymbol, optionName).Index = 0 AndAlso
                                                                                                                           x.Expiry IsNot Nothing AndAlso x.Expiry.Value = runningExpiryInstrument.Expiry.Value AndAlso
                                                                                                                           x.InstrumentType = IInstrument.TypeOfInstrument.Options
                                                                                                                       End Function)
                                If allOptionTradableInstruments IsNot Nothing AndAlso allOptionTradableInstruments.Count > 0 Then
                                    For Each runningOptionInstrument In allOptionTradableInstruments
                                        If runningOptionInstrument.TradingSymbol <> runningExpiryInstrument.TradingSymbol Then
                                            retTradableInstrumentsAsPerStrategy.Add(runningOptionInstrument)
                                            sheetForToken.Add(runningOptionInstrument.InstrumentIdentifier, instrument.Value.SheetName)
                                        End If
                                    Next
                                End If
                            End If
                        Next
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

            Dim folderPath As String = Path.Combine(My.Application.Info.DirectoryPath, String.Format("Bid Ask Data {0}", Now.ToString("yyyyMMdd")))
            If Not Directory.Exists(folderPath) Then Directory.CreateDirectory(folderPath)

            'Now create the fresh handlers
            For Each runningTradableInstrument In retTradableInstrumentsAsPerStrategy
                _cts.Token.ThrowIfCancellationRequested()
                If retTradableStrategyInstruments Is Nothing Then retTradableStrategyInstruments = New List(Of NFOStrategyInstrument)
                Dim runningTradableStrategyInstrument As New NFOStrategyInstrument(runningTradableInstrument, Me, False, _cts, folderPath, sheetForToken(runningTradableInstrument.InstrumentIdentifier))
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
            _cts.Token.ThrowIfCancellationRequested()
            Dim tasks As New List(Of Task)()
            For Each tradableStrategyInstrument As NFOStrategyInstrument In TradableStrategyInstruments
                _cts.Token.ThrowIfCancellationRequested()
                tasks.Add(Task.Run(AddressOf tradableStrategyInstrument.MonitorAsync, _cts.Token))
            Next
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

    Public Async Function ExportDataAsync() As Task
        OnHeartbeat("Trying to export data to excel. Loading data ....")
        If Me.TradableStrategyInstruments IsNot Nothing AndAlso Me.TradableStrategyInstruments.Count > 0 Then
            logger.Debug("Waiting for data load from a2t if required")
            While True
                _cts.Token.ThrowIfCancellationRequested()
                Dim allInstrumentsReadyToExport As Boolean = True
                For Each runningInstrument As NFOStrategyInstrument In Me.TradableStrategyInstruments
                    allInstrumentsReadyToExport = allInstrumentsReadyToExport AndAlso runningInstrument.ReadyToExport
                Next
                If allInstrumentsReadyToExport Then Exit While
                Await Task.Delay(1000).ConfigureAwait(False)
            End While

            logger.Debug("Checking if data is available to write or not")
            Dim dataAvailableToExport As Boolean = False
            For Each runningInstrument As NFOStrategyInstrument In Me.TradableStrategyInstruments
                _cts.Token.ThrowIfCancellationRequested()
                dataAvailableToExport = dataAvailableToExport OrElse (runningInstrument.BidAskCollection IsNot Nothing AndAlso runningInstrument.BidAskCollection.Count > 0)
            Next

            If dataAvailableToExport Then
                logger.Debug("Data available to write excel")
                Dim minTime As Date = Date.MaxValue
                Dim maxTime As Date = Date.MinValue
                For Each runningInstrument As NFOStrategyInstrument In Me.TradableStrategyInstruments
                    _cts.Token.ThrowIfCancellationRequested()
                    If runningInstrument.BidAskCollection IsNot Nothing AndAlso runningInstrument.BidAskCollection.Count > 0 Then
                        Dim lowestTime As Date = runningInstrument.BidAskCollection.Min(Function(x)
                                                                                            Return x.Value.SnapshotDateTime
                                                                                        End Function)
                        Dim highestTime As Date = runningInstrument.BidAskCollection.Max(Function(x)
                                                                                             Return x.Value.SnapshotDateTime
                                                                                         End Function)

                        If lowestTime <> Date.MinValue AndAlso lowestTime < minTime Then minTime = lowestTime
                        If highestTime <> Date.MaxValue AndAlso highestTime > maxTime Then maxTime = highestTime
                    End If
                Next
                Dim timeList As List(Of Date) = Nothing
                If minTime <> Date.MaxValue AndAlso maxTime <> Date.MinValue Then
                    timeList = New List(Of Date)
                    Dim startTime As Date = minTime
                    While startTime <= maxTime
                        _cts.Token.ThrowIfCancellationRequested()
                        timeList.Add(startTime)
                        startTime = startTime.AddSeconds(1)
                    End While
                End If

                If timeList IsNot Nothing AndAlso timeList.Count > 0 Then
                    Dim userInputs As NFOUserInputs = Me.UserSettings
                    OnHeartbeat("Opening excel")
                    Using xlHlpr As New Utilities.DAL.ExcelHelper(userInputs.InstrumentDetailsFilePath, Utilities.DAL.ExcelHelper.ExcelOpenStatus.OpenExistingForReadWrite, Utilities.DAL.ExcelHelper.ExcelSaveType.XLS_XLSX, _cts)
                        'AddHandler xlHlpr.Heartbeat, AddressOf OnHeartbeat

                        Dim allSheets As List(Of String) = xlHlpr.GetExcelSheetsName()
                        If allSheets IsNot Nothing AndAlso allSheets.Count > 0 Then
                            Dim counter As Integer = 0
                            For Each runningSheet In allSheets
                                _cts.Token.ThrowIfCancellationRequested()
                                counter += 1
                                OnHeartbeat(String.Format("Trying to export data for {0} #{1}/{2}", runningSheet, counter, allSheets.Count))
                                xlHlpr.SetActiveSheet(runningSheet)

                                Dim instrumentsOfThisSheet As IEnumerable(Of StrategyInstrument) = Me.TradableStrategyInstruments.Where(Function(x)
                                                                                                                                            Return CType(x, NFOStrategyInstrument).SheetName = runningSheet
                                                                                                                                        End Function)
                                If instrumentsOfThisSheet IsNot Nothing AndAlso instrumentsOfThisSheet.Count > 0 Then
                                    Dim mainInstruments As IEnumerable(Of StrategyInstrument) = instrumentsOfThisSheet.Where(Function(x)
                                                                                                                                 Return x.TradableInstrument.InstrumentType = IInstrument.TypeOfInstrument.Futures AndAlso
                                                                                                                         x.TradableInstrument.Strike = 0
                                                                                                                             End Function)
                                    If mainInstruments IsNot Nothing AndAlso mainInstruments.Count > 0 Then
                                        Dim mainInstrument As NFOStrategyInstrument = mainInstruments.FirstOrDefault
                                        If mainInstrument.BidAskCollection IsNot Nothing AndAlso mainInstrument.BidAskCollection.Count > 0 Then
                                            Dim instrumentsExpiry As Date = mainInstrument.TradableInstrument.Expiry.Value
                                            Dim mainRawData(timeList.Count - 1, 3) As Object
                                            Dim rowCtr As Integer = 0
                                            For Each runningData In timeList
                                                Dim colCtr As Integer = 0
                                                mainRawData(rowCtr, colCtr) = runningData.ToString("dd-MMM-yyyy HH:mm:ss")
                                                colCtr += 1
                                                mainRawData(rowCtr, colCtr) = instrumentsExpiry.ToString("dd-MMM-yyyy")
                                                colCtr += 1
                                                If mainInstrument.BidAskCollection.ContainsKey(runningData) Then
                                                    mainRawData(rowCtr, colCtr) = mainInstrument.BidAskCollection(runningData).Bid
                                                    colCtr += 1
                                                    mainRawData(rowCtr, colCtr) = mainInstrument.BidAskCollection(runningData).Ask
                                                Else
                                                    mainRawData(rowCtr, colCtr) = 0
                                                    colCtr += 1
                                                    mainRawData(rowCtr, colCtr) = 0
                                                End If
                                                rowCtr += 1
                                            Next
                                            Dim range As String = xlHlpr.GetNamedRange(4, mainRawData.GetLength(0) - 1, 1, mainRawData.GetLength(1) - 1)
                                            xlHlpr.WriteArrayToExcel(mainRawData, range)
                                        End If
                                    End If

                                    Dim strikePriceList As List(Of Decimal) = Nothing
                                    For Each runningStrategyInstrument In instrumentsOfThisSheet.OrderBy(Function(x)
                                                                                                             Return x.TradableInstrument.Strike
                                                                                                         End Function)
                                        If runningStrategyInstrument.TradableInstrument.InstrumentType = IInstrument.TypeOfInstrument.Options AndAlso
                                            runningStrategyInstrument.TradableInstrument.Strike <> 0 Then
                                            If strikePriceList Is Nothing Then strikePriceList = New List(Of Decimal)
                                            If Not strikePriceList.Contains(runningStrategyInstrument.TradableInstrument.Strike) Then
                                                strikePriceList.Add(runningStrategyInstrument.TradableInstrument.Strike)
                                            End If
                                        End If
                                    Next
                                    If strikePriceList IsNot Nothing AndAlso strikePriceList.Count > 0 Then
                                        Dim initialStartColumn As Integer = 5
                                        Dim initialEndColumn As Integer = 9
                                        Dim startColumn As Integer = initialStartColumn
                                        Dim endColumn As Integer = initialEndColumn
                                        Dim copyRng As String = String.Format("{0}:{1}", xlHlpr.GetColumnName(5), xlHlpr.GetColumnName(endColumn))
                                        Dim strikeCounter As Integer = 0
                                        For Each runningStrikePrice In strikePriceList
                                            strikeCounter += 1
                                            OnHeartbeat(String.Format("Trying to export data for {0} #{1}/{2} #{3}/{4}",
                                                              runningSheet, counter, allSheets.Count, strikeCounter, strikePriceList.Count))
                                            'If strikeCounter > 1 Then
                                            startColumn = endColumn + 1
                                            endColumn = startColumn + (initialEndColumn - initialStartColumn)
                                            Dim insrtRng As String = String.Format("{0}:{1}", xlHlpr.GetColumnName(startColumn), xlHlpr.GetColumnName(endColumn))
                                            xlHlpr.CopyPasteData(copyRng, insrtRng)
                                            'End If
                                            xlHlpr.SetData(1, startColumn, String.Format("{0}", runningStrikePrice), Utilities.DAL.ExcelHelper.XLAlign.Center)

                                            Dim strikeInstruments As IEnumerable(Of StrategyInstrument) = instrumentsOfThisSheet.Where(Function(x)
                                                                                                                                           Return x.TradableInstrument.Strike = runningStrikePrice
                                                                                                                                       End Function)
                                            For Each runningStrategyInstruments As NFOStrategyInstrument In strikeInstruments
                                                If runningStrategyInstruments.BidAskCollection IsNot Nothing AndAlso runningStrategyInstruments.BidAskCollection.Count > 0 Then
                                                    Dim rawData(timeList.Count - 1, 1) As Object
                                                    Dim rowCtr As Integer = 0
                                                    For Each runningData In timeList
                                                        Dim colCtr As Integer = 0
                                                        If runningStrategyInstruments.BidAskCollection.ContainsKey(runningData) Then
                                                            rawData(rowCtr, colCtr) = runningStrategyInstruments.BidAskCollection(runningData).Bid
                                                            colCtr += 1
                                                            rawData(rowCtr, colCtr) = runningStrategyInstruments.BidAskCollection(runningData).Ask
                                                        Else
                                                            rawData(rowCtr, colCtr) = 0
                                                            colCtr += 1
                                                            rawData(rowCtr, colCtr) = 0
                                                        End If
                                                        rowCtr += 1
                                                    Next

                                                    If runningStrategyInstruments.TradableInstrument.RawInstrumentType = "CE" Then
                                                        Dim range As String = xlHlpr.GetNamedRange(4, rawData.GetLength(0) - 1, startColumn, rawData.GetLength(1) - 1)
                                                        xlHlpr.WriteArrayToExcel(rawData, range)
                                                    ElseIf runningStrategyInstruments.TradableInstrument.RawInstrumentType = "PE" Then
                                                        Dim range As String = xlHlpr.GetNamedRange(4, rawData.GetLength(0) - 1, startColumn + (initialEndColumn - initialStartColumn) / 2, rawData.GetLength(1) - 1)
                                                        xlHlpr.WriteArrayToExcel(rawData, range)
                                                    End If
                                                End If
                                            Next
                                        Next
                                        xlHlpr.DeleteColumn(copyRng)
                                    End If
                                End If
                            Next
                            OnHeartbeat("Export successful")
                        End If
                    End Using
                Else
                    logger.Debug("Unable to create time collection. Min Time:{0}, Max Time:{0}", minTime, maxTime)
                End If
            Else
                OnHeartbeat("No data available to export")
            End If
        End If
    End Function

End Class
