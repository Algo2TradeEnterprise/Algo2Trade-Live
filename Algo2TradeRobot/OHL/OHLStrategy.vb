﻿Imports System.Text.RegularExpressions
Imports System.Threading
Imports Algo2TradeCore.Controller
Imports Algo2TradeCore.Entities
Imports Algo2TradeCore.Strategies
Imports NLog

Public Class OHLStrategy
    Inherits Strategy
#Region "Logging and Status Progress"
    Public Shared Shadows logger As Logger = LogManager.GetCurrentClassLogger
#End Region

    Public Sub New(ByVal associatedParentController As APIStrategyController,
                   ByVal strategyIdentifier As String,
                   ByVal userSettings As OHLUserInputs,
                   ByVal maxNumberOfDaysForHistoricalFetch As Integer,
                   ByVal canceller As CancellationTokenSource)
        'MyBase.New(associatedParentController, strategyIdentifier, False, userSettings, maxNumberOfDaysForHistoricalFetch, canceller, True)
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
            'Get OHL Strategy Instruments
            Using fillInstrumentDetails As New OHLFillInstrumentDetails(_cts, Me)
                Await fillInstrumentDetails.GetInstrumentData(allInstruments, bannedInstruments).ConfigureAwait(False)
            End Using
            logger.Debug(Utilities.Strings.JsonSerialize(Me.UserSettings))

            Dim ohlUserInputs As OHLUserInputs = CType(Me.UserSettings, OHLUserInputs)
            If ohlUserInputs.InstrumentsData IsNot Nothing AndAlso ohlUserInputs.InstrumentsData.Count > 0 Then
                Dim dummyAllInstruments As List(Of IInstrument) = allInstruments.ToList
                For Each instrument In ohlUserInputs.InstrumentsData
                    _cts.Token.ThrowIfCancellationRequested()
                    Dim runningTradableInstrument As IInstrument = dummyAllInstruments.Find(Function(x)
                                                                                                Return x.TradingSymbol = instrument.Key
                                                                                            End Function)
                    _cts.Token.ThrowIfCancellationRequested()
                    ret = True
                    If retTradableInstrumentsAsPerStrategy Is Nothing Then retTradableInstrumentsAsPerStrategy = New List(Of IInstrument)
                    If runningTradableInstrument IsNot Nothing Then retTradableInstrumentsAsPerStrategy.Add(runningTradableInstrument)
                Next
                TradableInstrumentsAsPerStrategy = retTradableInstrumentsAsPerStrategy
            End If
        End If
        If retTradableInstrumentsAsPerStrategy IsNot Nothing AndAlso retTradableInstrumentsAsPerStrategy.Count > 0 Then
            'tradableInstrumentsAsPerStrategy = tradableInstrumentsAsPerStrategy.Take(5).ToList
            'Now create the strategy tradable instruments
            Dim retTradableStrategyInstruments As List(Of OHLStrategyInstrument) = Nothing
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
                If retTradableStrategyInstruments Is Nothing Then retTradableStrategyInstruments = New List(Of OHLStrategyInstrument)
                Dim runningTradableStrategyInstrument As New OHLStrategyInstrument(runningTradableInstrument, Me, False, _cts)
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
            For Each tradableStrategyInstrument As OHLStrategyInstrument In TradableStrategyInstruments
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
        Dim capitalAtDayStart As Decimal = Me.ParentController.GetUserMargin(Me.TradableInstrumentsAsPerStrategy.FirstOrDefault.ExchangeDetails.ExchangeType)
        Dim currentTime As Date = Now
        If currentTime >= Me.UserSettings.EODExitTime Then
            ret = New Tuple(Of Boolean, String)(True, "EOD Exit")
        ElseIf Me.GetTotalPL <= capitalAtDayStart * Math.Abs(Me.UserSettings.MaxLossPercentagePerDay) * -1 / 100 Then
            logger.Warn("MTM Reached")
            ret = New Tuple(Of Boolean, String)(True, "Max Loss % Per Day Reached Exit")
        ElseIf Me.GetTotalPL >= capitalAtDayStart * Math.Abs(Me.UserSettings.MaxProfitPercentagePerDay) / 100 Then
            logger.Warn("MTM Reached")
            ret = New Tuple(Of Boolean, String)(True, "Max Profit % Per Day Reached Exit")
        End If
        Return ret
    End Function
End Class
