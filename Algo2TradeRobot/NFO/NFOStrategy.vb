﻿Imports System.Text.RegularExpressions
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
            Dim allfnoInstruments As List(Of IInstrument) = allInstruments.ToList.FindAll(Function(x)
                                                                                              Return x.InstrumentType = IInstrument.TypeOfInstrument.Futures AndAlso x.RawExchange = "NFO"
                                                                                          End Function)
            If allfnoInstruments IsNot Nothing AndAlso allfnoInstruments.Count > 0 Then
                Dim uniquefnoInstruments As List(Of String) = New List(Of String)
                For Each runningInstrument In allfnoInstruments
                    If Not uniquefnoInstruments.Contains(runningInstrument.RawInstrumentName.ToUpper) Then
                        uniquefnoInstruments.Add(runningInstrument.RawInstrumentName.ToUpper)
                    End If
                Next
                If uniquefnoInstruments IsNot Nothing AndAlso uniquefnoInstruments.Count > 0 Then
                    Dim allCashInstrument As IEnumerable(Of IInstrument) = allInstruments.Where(Function(x)
                                                                                                    Return x.InstrumentType = IInstrument.TypeOfInstrument.Cash AndAlso
                                                                                                    x.Segment = "NSE"
                                                                                                End Function)

                    For Each instrument As IInstrument In allCashInstrument
                        _cts.Token.ThrowIfCancellationRequested()
                        If uniquefnoInstruments.Contains(instrument.TradingSymbol.ToUpper) Then
                            'Dim runningTradableInstrument As IInstrument = GetCurrentContract(instrument.TradingSymbol, allInstruments.ToList)
                            '_cts.Token.ThrowIfCancellationRequested()
                            'If runningTradableInstrument IsNot Nothing Then
                            '    If retTradableInstrumentsAsPerStrategy Is Nothing Then retTradableInstrumentsAsPerStrategy = New List(Of IInstrument)
                            '    retTradableInstrumentsAsPerStrategy.Add(instrument)
                            '    retTradableInstrumentsAsPerStrategy.Add(runningTradableInstrument)
                            'End If
                        End If
                    Next
                    If retTradableInstrumentsAsPerStrategy IsNot Nothing AndAlso retTradableInstrumentsAsPerStrategy.Count > 0 Then
                        'Dim niftyInstrument As IInstrument = GetCurrentContract("NIFTY", allInstruments.ToList)
                        'If niftyInstrument IsNot Nothing Then
                        '    retTradableInstrumentsAsPerStrategy.Add(niftyInstrument)
                        '    ret = True
                        'End If
                    End If
                End If
            End If
            TradableInstrumentsAsPerStrategy = retTradableInstrumentsAsPerStrategy
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
                tasks.Add(Task.Run(AddressOf tradableStrategyInstrument.MonitorAsync, _cts.Token))
            Next
            tasks.Add(Task.Run(AddressOf ForceExitAllTradesAsync, _cts.Token))
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
        Dim userSettings As NFOUserInputs = Me.UserSettings
        Dim overallPL As Decimal = Me.GetTotalPLAfterBrokerage

        Dim currentTime As Date = Now
        If currentTime >= Me.UserSettings.EODExitTime Then
            ret = New Tuple(Of Boolean, String)(True, "EOD Exit")
        ElseIf overallPL <= userSettings.MTMLoss Then
            logger.Debug("MTM Loss reached. Overall PL: {0}", overallPL)
            ret = New Tuple(Of Boolean, String)(True, "MTM Loss Per Day Reached")
        ElseIf overallPL >= userSettings.MTMProfit Then
            logger.Debug("MTM Profit reached. Overall PL: {0}", overallPL)
            ret = New Tuple(Of Boolean, String)(True, "MTM Profit Per Day Reached")
        End If
        Return ret
    End Function
End Class
