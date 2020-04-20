﻿Imports System.IO
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

            Dim folderPath As String = Path.Combine(My.Application.Info.DirectoryPath, Now.ToString("yyyyMMdd"))
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
        OnHeartbeat("Trying to export data to excel")
        Await Task.Delay(1000).ConfigureAwait(False)
        If Me.TradableStrategyInstruments IsNot Nothing AndAlso Me.TradableStrategyInstruments.Count > 0 Then
            Dim userInputs As NFOUserInputs = Me.UserSettings
            OnHeartbeat("Opening excel")
            Using xlHlpr As New Utilities.DAL.ExcelHelper(userInputs.InstrumentDetailsFilePath, Utilities.DAL.ExcelHelper.ExcelOpenStatus.OpenExistingForReadWrite, Utilities.DAL.ExcelHelper.ExcelSaveType.XLS_XLSX, _cts)
                'AddHandler xlHlpr.Heartbeat, AddressOf OnHeartbeat

                Dim allSheets As List(Of String) = xlHlpr.GetExcelSheetsName()
                If allSheets IsNot Nothing AndAlso allSheets.Count > 0 Then
                    Dim counter As Integer = 0
                    For Each runningSheet In allSheets
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
                                    Dim mainRawData(mainInstrument.BidAskCollection.Count - 1, 3) As Object
                                    Dim rowCtr As Integer = 0
                                    For Each runningData In mainInstrument.BidAskCollection
                                        Dim colCtr As Integer = 0
                                        mainRawData(rowCtr, colCtr) = mainInstrument.TradableInstrument.Expiry.Value.ToString("dd-MMM-yyyy")
                                        colCtr += 1
                                        mainRawData(rowCtr, colCtr) = runningData.Value.SnapshotDateTime.ToString("dd-MM-yyyy HH:mm:ss")
                                        colCtr += 1
                                        mainRawData(rowCtr, colCtr) = runningData.Value.Bid
                                        colCtr += 1
                                        mainRawData(rowCtr, colCtr) = runningData.Value.Ask
                                        rowCtr += 1
                                    Next
                                    Dim range As String = xlHlpr.GetNamedRange(4, mainRawData.GetLength(0) - 1, 1, mainRawData.GetLength(1) - 1)
                                    xlHlpr.WriteArrayToExcel(mainRawData, range)
                                End If
                            End If

                            Dim copyRng As String = String.Format("{0}:{1}", xlHlpr.GetColumnName(5), xlHlpr.GetColumnName(11))

                        End If

                        'Dim insrtRng As String = String.Format("{0}:{1}", xlHlpr.GetColumnName(12), xlHlpr.GetColumnName(18))
                        'xlHlpr.CopyPasteData(copyRng, insrtRng)
                    Next
                End If
            End Using
        End If
    End Function

End Class
