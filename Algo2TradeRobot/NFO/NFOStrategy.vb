Imports NLog
Imports System.Threading
Imports Algo2TradeCore.Controller
Imports Algo2TradeCore.Entities
Imports Algo2TradeCore.Strategies

Public Class NFOStrategy
    Inherits Strategy

#Region "Logging and Status Progress"
    Public Shared Shadows logger As Logger = LogManager.GetCurrentClassLogger
#End Region

    Public Property TradablePairInstruments As IEnumerable(Of NFOPairInstrument)

    Public Sub New(ByVal associatedParentController As APIStrategyController,
                   ByVal strategyIdentifier As String,
                   ByVal userSettings As NFOUserInputs,
                   ByVal maxNumberOfDaysForHistoricalFetch As Integer,
                   ByVal canceller As CancellationTokenSource)
        MyBase.New(associatedParentController, strategyIdentifier, True, userSettings, maxNumberOfDaysForHistoricalFetch, canceller)
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
        Dim retTradablePairInstrumentsAsPerStrategy As Dictionary(Of String, List(Of IInstrument)) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        logger.Debug("Starting to fill strategy specific instruments, strategy:{0}", Me.ToString)
        If allInstruments IsNot Nothing AndAlso allInstruments.Count > 0 Then
            Dim userInputs As NFOUserInputs = Me.UserSettings
            If userInputs.InstrumentsData IsNot Nothing AndAlso userInputs.InstrumentsData.Count > 0 Then
                Dim dummyAllInstruments As List(Of IInstrument) = allInstruments.ToList
                For Each instrument In userInputs.InstrumentsData
                    _cts.Token.ThrowIfCancellationRequested()
                    Dim pairStocks As List(Of IInstrument) = New List(Of IInstrument)
                    _cts.Token.ThrowIfCancellationRequested()
                    Dim stock1Instrument As IInstrument = dummyAllInstruments.Find(Function(x)
                                                                                       Return x.TradingSymbol = instrument.Value.Stock1
                                                                                   End Function)
                    pairStocks.Add(stock1Instrument)
                    _cts.Token.ThrowIfCancellationRequested()
                    Dim stock2Instrument As IInstrument = dummyAllInstruments.Find(Function(x)
                                                                                       Return x.TradingSymbol = instrument.Value.Stock2
                                                                                   End Function)
                    pairStocks.Add(stock2Instrument)
                    _cts.Token.ThrowIfCancellationRequested()
                    Dim stock1FutInstrument As List(Of IInstrument) = dummyAllInstruments.FindAll(Function(x)
                                                                                                      Return x.RawInstrumentName = instrument.Value.Stock1 AndAlso
                                                                                                      x.InstrumentType = IInstrument.TypeOfInstrument.Futures
                                                                                                  End Function)
                    Dim stk1futContracts As List(Of IInstrument) = GetCurrentFutureContracts(stock1FutInstrument)
                    If stk1futContracts IsNot Nothing AndAlso stk1futContracts.Count > 0 Then
                        For Each runningContract In stk1futContracts
                            _cts.Token.ThrowIfCancellationRequested()
                            pairStocks.Add(runningContract)
                        Next
                    End If
                    _cts.Token.ThrowIfCancellationRequested()
                    Dim stock2FutInstrument As List(Of IInstrument) = dummyAllInstruments.FindAll(Function(x)
                                                                                                      Return x.RawInstrumentName = instrument.Value.Stock2 AndAlso
                                                                                                      x.InstrumentType = IInstrument.TypeOfInstrument.Futures
                                                                                                  End Function)
                    Dim stk2futContracts As List(Of IInstrument) = GetCurrentFutureContracts(stock2FutInstrument)
                    If stk2futContracts IsNot Nothing AndAlso stk2futContracts.Count > 0 Then
                        For Each runningContract In stk2futContracts
                            _cts.Token.ThrowIfCancellationRequested()
                            pairStocks.Add(runningContract)
                        Next
                    End If
                    _cts.Token.ThrowIfCancellationRequested()

                    If retTradableInstrumentsAsPerStrategy Is Nothing Then retTradableInstrumentsAsPerStrategy = New List(Of IInstrument)
                    retTradableInstrumentsAsPerStrategy.AddRange(pairStocks)

                    If retTradablePairInstrumentsAsPerStrategy Is Nothing Then retTradablePairInstrumentsAsPerStrategy = New Dictionary(Of String, List(Of IInstrument))
                    retTradablePairInstrumentsAsPerStrategy.Add(instrument.Value.PairName, pairStocks)

                    ret = True
                Next
                TradableInstrumentsAsPerStrategy = retTradableInstrumentsAsPerStrategy
            End If
        End If

        If retTradableInstrumentsAsPerStrategy IsNot Nothing AndAlso retTradableInstrumentsAsPerStrategy.Count > 0 AndAlso
            retTradablePairInstrumentsAsPerStrategy IsNot Nothing AndAlso retTradablePairInstrumentsAsPerStrategy.Count > 0 Then
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

            Dim retTradablePairInstruments As List(Of NFOPairInstrument) = Nothing
            For Each runningTradablePairInstrument In retTradablePairInstrumentsAsPerStrategy
                _cts.Token.ThrowIfCancellationRequested()
                Dim dependentStrategyInstruments As List(Of NFOStrategyInstrument) = New List(Of NFOStrategyInstrument)
                For Each runningInstrument In runningTradablePairInstrument.Value
                    Dim dpndStrategyIns As StrategyInstrument = Me.TradableStrategyInstruments.ToList.Find(Function(x)
                                                                                                               Return x.TradableInstrument.InstrumentIdentifier = runningInstrument.InstrumentIdentifier
                                                                                                           End Function)
                    dependentStrategyInstruments.Add(dpndStrategyIns)
                Next

                If retTradablePairInstruments Is Nothing Then retTradablePairInstruments = New List(Of NFOPairInstrument)
                Dim runningPairInstrument As New NFOPairInstrument(dependentStrategyInstruments, Me, runningTradablePairInstrument.Key, _cts)
                AddHandler runningPairInstrument.Heartbeat, AddressOf OnHeartbeat
                AddHandler runningPairInstrument.WaitingFor, AddressOf OnWaitingFor
                AddHandler runningPairInstrument.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
                AddHandler runningPairInstrument.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete

                retTradablePairInstruments.Add(runningPairInstrument)
            Next
            TradablePairInstruments = retTradablePairInstruments
        Else
            Throw New ApplicationException(String.Format("Cannot run this strategy as no strategy instruments could be created from the tradable instruments, stratgey:{0}", Me.ToString))
        End If

        Return ret
    End Function

    Private Function GetCurrentFutureContracts(ByVal allFutureContracts As List(Of IInstrument)) As List(Of IInstrument)
        Dim ret As List(Of IInstrument) = Nothing
        If allFutureContracts IsNot Nothing AndAlso allFutureContracts.Count > 0 Then
            Dim minExpiry As Date = allFutureContracts.Min(Function(x)
                                                               If x.Expiry.Value.AddDays(-1).Date >= Now.Date Then
                                                                   Return x.Expiry.Value.Date
                                                               Else
                                                                   Return Date.MaxValue
                                                               End If
                                                           End Function)
            Dim minExpryFutInstrmt As IInstrument = allFutureContracts.Find(Function(x)
                                                                                Return x.Expiry.Value.Date = minExpiry.Date
                                                                            End Function)
            If ret Is Nothing Then ret = New List(Of IInstrument)
            ret.Add(minExpryFutInstrmt)
            If minExpiry.Date.AddDays(-1) = Now.Date Then
                Dim nextMinExpiry As Date = allFutureContracts.Min(Function(x)
                                                                       If x.Expiry.Value.Date > minExpiry.Date Then
                                                                           Return x.Expiry.Value.Date
                                                                       Else
                                                                           Return Date.MaxValue
                                                                       End If
                                                                   End Function)
                Dim nxtMinExpryFutInstrmt As IInstrument = allFutureContracts.Find(Function(x)
                                                                                       Return x.Expiry.Value.Date = nextMinExpiry.Date
                                                                                   End Function)
                ret.Add(nxtMinExpryFutInstrmt)
            End If
        End If
        Return ret
    End Function

    Public Overrides Async Function MonitorAsync() As Task
        Dim lastException As Exception = Nothing

        Try
            _cts.Token.ThrowIfCancellationRequested()
            Dim tasks As New List(Of Task)()
            For Each tradablePairInstrument In TradablePairInstruments
                _cts.Token.ThrowIfCancellationRequested()
                tasks.Add(Task.Run(AddressOf tradablePairInstrument.MonitorPairAsync, _cts.Token))
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
End Class
