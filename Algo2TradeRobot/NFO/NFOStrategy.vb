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


    Public Property DependentInstruments As IEnumerable(Of IInstrument)

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
                Dim dummyAllInstruments As List(Of IInstrument) = allInstruments.ToList
                For Each instrument In userInputs.InstrumentsData
                    _cts.Token.ThrowIfCancellationRequested()

                    Dim spotInstrumentName As String = instrument.Key
                    If instrument.Key = "BANKNIFTY" Then
                        spotInstrumentName = "NIFTY BANK"
                    ElseIf instrument.Key = "NIFTY" Then
                        spotInstrumentName = "NIFTY 50"
                    End If
                    If spotInstrumentName IsNot Nothing Then
                        Dim runningTradableInstrument As IInstrument = dummyAllInstruments.Find(Function(x)
                                                                                                    Return x.TradingSymbol = spotInstrumentName
                                                                                                End Function)
                        If runningTradableInstrument IsNot Nothing Then
                            If retTradableInstrumentsAsPerStrategy Is Nothing Then retTradableInstrumentsAsPerStrategy = New List(Of IInstrument)
                            retTradableInstrumentsAsPerStrategy.Add(runningTradableInstrument)

                            Dim dependentTradableInstruments As IEnumerable(Of IInstrument) = allInstruments.Where(Function(x)
                                                                                                                       Return x.Name = instrument.Key AndAlso
                                                                                                                       x.InstrumentType = IInstrument.TypeOfInstrument.Options AndAlso
                                                                                                                       x.Expiry.Value.Date = instrument.Value.ExpiryDate.Date
                                                                                                                   End Function)
                            If dependentTradableInstruments IsNot Nothing AndAlso dependentTradableInstruments.Count > 0 Then
                                If Me.DependentInstruments IsNot Nothing Then
                                    Me.DependentInstruments = Me.DependentInstruments.Concat(dependentTradableInstruments)
                                Else
                                    Me.DependentInstruments = dependentTradableInstruments
                                End If
                                ret = True
                            End If
                        End If
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
                Dim runningTradableStrategyInstrument As New NFOStrategyInstrument(runningTradableInstrument, Me, False, True, _cts)
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

    Public Async Function CreateDependentTradableStrategyInstrumentsAsync(ByVal instrumentsToBeSubscrided As List(Of IInstrument)) As Task(Of Boolean)
        Dim ret As Boolean = False
        Dim retTradableStrategyInstruments As List(Of NFOStrategyInstrument) = Nothing
        For Each runningTradableInstrument In instrumentsToBeSubscrided
            _cts.Token.ThrowIfCancellationRequested()
            If retTradableStrategyInstruments Is Nothing Then retTradableStrategyInstruments = New List(Of NFOStrategyInstrument)
            Dim runningTradableStrategyInstrument As New NFOStrategyInstrument(runningTradableInstrument, Me, False, False, _cts)
            AddHandler runningTradableStrategyInstrument.HeartbeatEx, AddressOf OnHeartbeatEx
            AddHandler runningTradableStrategyInstrument.WaitingForEx, AddressOf OnWaitingForEx
            AddHandler runningTradableStrategyInstrument.DocumentRetryStatusEx, AddressOf OnDocumentRetryStatusEx
            AddHandler runningTradableStrategyInstrument.DocumentDownloadCompleteEx, AddressOf OnDocumentDownloadCompleteEx

            retTradableStrategyInstruments.Add(runningTradableStrategyInstrument)
            ret = True
        Next
        TradableStrategyInstruments = TradableStrategyInstruments.Concat(retTradableStrategyInstruments)
        Await Me.ParentController.ProcessDependentStrategyInstrumentSubscriptionAsync(Me).ConfigureAwait(False)
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
            tasks.Add(Task.Run(AddressOf ExitAllTradesAsync, _cts.Token))
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
        Throw New NotImplementedException
    End Function

    Private Async Function ExitAllTradesAsync() As Task
        Await Task.Delay(1).ConfigureAwait(False)
        Try
            While True
                If Me.ParentController.OrphanException IsNot Nothing Then
                    Throw Me.ParentController.OrphanException
                End If
                _cts.Token.ThrowIfCancellationRequested()
                Dim exitAll As Boolean = False
                Dim pl As Decimal = GetOverallPL()
                If pl >= CType(Me.UserSettings, NFOUserInputs).MaxProfitPerDay Then
                    OnHeartbeat(String.Format("Strategy Max Profit reached. PL:{0}", pl))
                    exitAll = True
                ElseIf pl <= CType(Me.UserSettings, NFOUserInputs).MaxLossPerDay Then
                    OnHeartbeat(String.Format("Strategy Max Loss reached. PL:{0}", pl))
                    exitAll = True
                ElseIf Now >= CType(Me.UserSettings, NFOUserInputs).EODExitTime Then
                    OnHeartbeat(String.Format("Strategy EOD Exit Time reached. Time:{0}", Now.ToString("HH:mm:ss")))
                    exitAll = True
                End If
                If exitAll Then
                    For Each runningStrategyInstrument In TradableStrategyInstruments
                        runningStrategyInstrument.StrategyExitAllTriggerd = True
                    Next
                    Exit While
                End If
                Await Task.Delay(1000, _cts.Token).ConfigureAwait(False)
            End While
        Catch ex As Exception
            'To log exceptions getting created from this function as the bubble up of the exception
            'will anyways happen to Strategy.MonitorAsync but it will not be shown until all tasks exit
            logger.Error("Strategy:{0}, error:{1}", Me.ToString, ex.ToString)
            Throw ex
        End Try
    End Function

    Public Function GetOverallPL() As Decimal
        Dim ret As Decimal = 0
        If Me.TradableStrategyInstruments IsNot Nothing AndAlso Me.TradableStrategyInstruments.Count > 0 Then
            For Each runningStrategyInstrument As NFOStrategyInstrument In Me.TradableStrategyInstruments
                ret += runningStrategyInstrument.GetTotalPL()
            Next
        End If
        Return ret
    End Function
End Class
