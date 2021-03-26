﻿Imports NLog
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
            Dim userInputs As NFOUserInputs = Me.UserSettings
            If userInputs.InstrumentsData IsNot Nothing AndAlso userInputs.InstrumentsData.Count > 0 Then
                For Each instrument In userInputs.InstrumentsData
                    _cts.Token.ThrowIfCancellationRequested()
                    Dim runningTradableInstrument As IInstrument = allInstruments.ToList.Find(Function(x)
                                                                                                  Return x.TradingSymbol = instrument.Value.TradingSymbol
                                                                                              End Function)

                    _cts.Token.ThrowIfCancellationRequested()
                    If runningTradableInstrument IsNot Nothing Then
                        If retTradableInstrumentsAsPerStrategy Is Nothing Then retTradableInstrumentsAsPerStrategy = New List(Of IInstrument)
                        retTradableInstrumentsAsPerStrategy.Add(runningTradableInstrument)
                        ret = True
                    Else
                        OnHeartbeat(String.Format("Unable to map with any instrument: {0}", instrument.Key))
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
            tasks.Add(Task.Run(AddressOf GetBidAskRatioAsync, _cts.Token))
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

    Private Async Function GetBidAskRatioAsync() As Task
        Try
            Dim userInput As NFOUserInputs = Me.UserSettings
            While True
                If Me.ParentController.OrphanException IsNot Nothing Then
                    Throw Me.ParentController.OrphanException
                End If

                If Now >= userInput.CheckTime Then
                    Dim bidAskCollection As List(Of BidAskData) = New List(Of BidAskData)

                    For Each runningStrategyInstrument In Me.TradableStrategyInstruments
                        If runningStrategyInstrument.TradableInstrument IsNot Nothing AndAlso
                            runningStrategyInstrument.TradableInstrument.LastTick IsNot Nothing Then
                            Dim runningBidAsk As BidAskData = New BidAskData
                            runningBidAsk.TradingSymbol = runningStrategyInstrument.TradableInstrument.TradingSymbol
                            runningBidAsk.TotalBidQuantity = runningStrategyInstrument.TradableInstrument.LastTick.BuyQuantity
                            runningBidAsk.TotalAskQuantity = runningStrategyInstrument.TradableInstrument.LastTick.SellQuantity

                            bidAskCollection.Add(runningBidAsk)
                        End If
                    Next

                    If bidAskCollection IsNot Nothing AndAlso bidAskCollection.Count > 0 Then
                        Dim dt As DataTable = New DataTable
                        dt.Columns.Add("Trading Symbol")
                        dt.Columns.Add("Total Bid Quantity")
                        dt.Columns.Add("Total Ask Quantity")
                        dt.Columns.Add("Bid Ask Ratio")
                        For Each runningBidAsk In bidAskCollection.OrderByDescending(Function(x)
                                                                                         Return x.BidAskRatio
                                                                                     End Function)
                            Dim row As DataRow = dt.NewRow
                            row("Trading Symbol") = runningBidAsk.TradingSymbol
                            row("Total Bid Quantity") = runningBidAsk.TotalBidQuantity
                            row("Total Ask Quantity") = runningBidAsk.TotalAskQuantity
                            row("Bid Ask Ratio") = runningBidAsk.BidAskRatio
                            dt.Rows.Add(row)
                        Next

                        Dim folderPath As String = Path.Combine(My.Application.Info.DirectoryPath, "Bid Ask Output")
                        If Not Directory.Exists(folderPath) Then
                            Directory.CreateDirectory(folderPath)
                        End If
                        Dim filename As String = Path.Combine(folderPath, String.Format("Bid Ask Data {0}.csv", Now.ToString("dd_MM_yyyy")))
                        Using csv As New Utilities.DAL.CSVHelper(filename, ",", _cts)
                            csv.GetCSVFromDataTable(dt)
                        End Using
                    End If

                    Exit While
                End If

                Await Task.Delay(1000).ConfigureAwait(False)
            End While
        Catch ex As Exception
            logger.Error(ex.ToString)
            Throw ex
        End Try
    End Function

    Private Class BidAskData
        Public Property TradingSymbol As String
        Public Property TotalBidQuantity As Long
        Public Property TotalAskQuantity As Long
        Public ReadOnly Property BidAskRatio As Decimal
            Get
                If Me.TotalBidQuantity <> Long.MinValue AndAlso Me.TotalAskQuantity <> Long.MinValue AndAlso Me.TotalAskQuantity <> 0 Then
                    Return Math.Round(Me.TotalBidQuantity / Me.TotalAskQuantity, 4)
                Else
                    Return Decimal.MinValue
                End If
            End Get
        End Property
    End Class
End Class
