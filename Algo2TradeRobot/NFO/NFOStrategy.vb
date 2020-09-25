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

    Public ReadOnly Property DerivedInstruments As Dictionary(Of String, List(Of IInstrument))
    Public ReadOnly Property FreezeQuantityData As Dictionary(Of String, Long)

    Public Sub New(ByVal associatedParentController As APIStrategyController,
                   ByVal strategyIdentifier As String,
                   ByVal userSettings As NFOUserInputs,
                   ByVal maxNumberOfDaysForHistoricalFetch As Integer,
                   ByVal canceller As CancellationTokenSource)
        MyBase.New(associatedParentController, strategyIdentifier, True, userSettings, maxNumberOfDaysForHistoricalFetch, canceller, True)
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
            Dim userInputs As NFOUserInputs = Me.UserSettings
            If userInputs.StockList IsNot Nothing AndAlso userInputs.StockList.Count > 0 Then
                Dim fillInstrument As NFOFillInstrumentDetails = New NFOFillInstrumentDetails(_cts, Me)
                AddHandler fillInstrument.HeartbeatSpecial, AddressOf OnHeartbeat

                For Each runningStock In userInputs.StockList
                    _cts.Token.ThrowIfCancellationRequested()
                    Dim spotTradingSymbol As String = runningStock
                    If runningStock = "BANKNIFTY" Then
                        spotTradingSymbol = "NIFTY BANK"
                    ElseIf runningStock = "NIFTY" Then
                        spotTradingSymbol = "NIFTY 50"
                    End If

                    Dim spotInstrument As IInstrument = allInstruments.ToList.Find(Function(x)
                                                                                       Return x.InstrumentType = IInstrument.TypeOfInstrument.Cash AndAlso
                                                                                       x.TradingSymbol.ToUpper = spotTradingSymbol
                                                                                   End Function)
                    If spotInstrument IsNot Nothing Then
                        Dim optionInstruments As IEnumerable(Of IInstrument) = allInstruments.Where(Function(x)
                                                                                                        Return x.InstrumentType = IInstrument.TypeOfInstrument.Options AndAlso
                                                                                                    x.TradingSymbol.Split(" ")(0).Trim.ToUpper = runningStock
                                                                                                    End Function)
                        If optionInstruments IsNot Nothing AndAlso optionInstruments.Count > 0 Then
                            Dim intrumentToCheck As List(Of IInstrument) = Await fillInstrument.GetInstrumentData(optionInstruments, spotInstrument).ConfigureAwait(False)
                            _cts.Token.ThrowIfCancellationRequested()
                            If intrumentToCheck IsNot Nothing AndAlso intrumentToCheck.Count > 0 Then
                                If _DerivedInstruments Is Nothing Then _DerivedInstruments = New Dictionary(Of String, List(Of IInstrument))
                                _DerivedInstruments.Add(spotInstrument.InstrumentIdentifier, intrumentToCheck)

                                If retTradableInstrumentsAsPerStrategy Is Nothing Then retTradableInstrumentsAsPerStrategy = New List(Of IInstrument)
                                retTradableInstrumentsAsPerStrategy.Add(spotInstrument)
                                For Each runningInstrument In intrumentToCheck
                                    _cts.Token.ThrowIfCancellationRequested()
                                    retTradableInstrumentsAsPerStrategy.Add(runningInstrument)
                                    ret = True
                                Next
                            End If
                        End If
                    End If
                Next
                TradableInstrumentsAsPerStrategy = retTradableInstrumentsAsPerStrategy
            End If

            _FreezeQuantityData = Await GetFreezeQuantityData().ConfigureAwait(False)
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
            _cts.Token.ThrowIfCancellationRequested()
            Dim tasks As New List(Of Task)()
            For Each tradableStrategyInstrument As NFOStrategyInstrument In TradableStrategyInstruments
                _cts.Token.ThrowIfCancellationRequested()
                If tradableStrategyInstrument.TradableInstrument.InstrumentType = IInstrument.TypeOfInstrument.Cash Then
                    tasks.Add(Task.Run(AddressOf tradableStrategyInstrument.CheckInstrumentAsync, _cts.Token))
                End If
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
        Throw New NotImplementedException()
    End Function

    Private Async Function GetFreezeQuantityData() As Task(Of Dictionary(Of String, Long))
        Dim ret As Dictionary(Of String, Long) = Nothing
        Dim freezeQuantityFile As String = Path.Combine(My.Application.Info.DirectoryPath, "Freeze Quantity.xls")
        If File.Exists(freezeQuantityFile) Then File.Delete(freezeQuantityFile)
        Using browser As New Utilities.Network.HttpBrowser(Nothing, Net.DecompressionMethods.GZip, TimeSpan.FromSeconds(30), _cts)
            browser.KeepAlive = True
            Dim headersToBeSent As New Dictionary(Of String, String)
            headersToBeSent.Add("Host", "www1.nseindia.com")
            headersToBeSent.Add("Upgrade-Insecure-Requests", "1")
            headersToBeSent.Add("Sec-Fetch-Mode", "navigate")
            headersToBeSent.Add("Sec-Fetch-Site", "none")

            Dim targetURL As String = "https://www1.nseindia.com/content/fo/qtyfreeze.xls"
            If targetURL IsNot Nothing Then
                Await browser.GetFileAsync(targetURL, freezeQuantityFile, False, headersToBeSent).ConfigureAwait(False)
            End If
        End Using
        If freezeQuantityFile IsNot Nothing AndAlso File.Exists(freezeQuantityFile) Then
            Using xlHlpr As New Utilities.DAL.ExcelHelper(freezeQuantityFile, Utilities.DAL.ExcelHelper.ExcelOpenStatus.OpenExistingForReadWrite, Utilities.DAL.ExcelHelper.ExcelSaveType.XLS_XLSX, _cts)
                Dim freezeQuantityData As Object(,) = xlHlpr.GetExcelInMemory()
                If freezeQuantityData IsNot Nothing Then
                    For row As Integer = 2 To freezeQuantityData.GetLength(0) - 1
                        _cts.Token.ThrowIfCancellationRequested()
                        If ret Is Nothing Then ret = New Dictionary(Of String, Long)
                        ret.Add(freezeQuantityData(row, 2).ToString.Trim.ToUpper, freezeQuantityData(row, 3))
                    Next
                End If
            End Using
            File.Delete(freezeQuantityFile)
        End If
        If ret IsNot Nothing AndAlso ret.Count > 0 Then
            OnHeartbeat(String.Format("Freeze quanity data returned {0} stocks", ret.Count))
        Else
            OnHeartbeat("Freeze quanity data did not returned anything")
        End If
        Return ret
    End Function
End Class
