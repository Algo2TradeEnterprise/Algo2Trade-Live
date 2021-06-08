﻿Imports System.Threading
Imports Algo2TradeCore.Controller
Imports Algo2TradeCore.Entities
Imports Algo2TradeCore.Strategies
Imports NLog

Public Class SpreadStrategy
    Inherits Strategy
#Region "Logging and Status Progress"
    Public Shared Shadows logger As Logger = LogManager.GetCurrentClassLogger
#End Region

    Public DependentInstruments As IEnumerable(Of IInstrument) = Nothing
    Public ExpireToday As Boolean = False

    Public Sub New(ByVal associatedParentController As APIStrategyController,
                   ByVal strategyIdentifier As String,
                   ByVal userSettings As SpreadUserInputs,
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
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        logger.Debug("Starting to fill strategy specific instruments, strategy:{0}", Me.ToString)
        If allInstruments IsNot Nothing AndAlso allInstruments.Count > 0 Then
            Dim userInputs As SpreadUserInputs = Me.UserSettings
            If userInputs.InstrumentsData IsNot Nothing AndAlso userInputs.InstrumentsData.Count > 0 Then
                Dim dummyAllInstruments As List(Of IInstrument) = allInstruments.ToList
                For Each instrument In userInputs.InstrumentsData
                    _cts.Token.ThrowIfCancellationRequested()
                    Dim spotInstrumentName As String = instrument.Value.InstrumentName
                    If instrument.Value.InstrumentName.Trim.ToUpper = "NIFTY" Then
                        spotInstrumentName = "NIFTY 50"
                    ElseIf instrument.Value.InstrumentName.Trim.ToUpper = "BANKNIFTY" Then
                        spotInstrumentName = "NIFTY BANK"
                    End If

                    Dim runningTradableInstrument As IInstrument = dummyAllInstruments.Find(Function(x)
                                                                                                Return x.TradingSymbol.ToUpper = spotInstrumentName
                                                                                            End Function)

                    If runningTradableInstrument IsNot Nothing Then
                        If retTradableInstrumentsAsPerStrategy Is Nothing Then retTradableInstrumentsAsPerStrategy = New List(Of IInstrument)
                        retTradableInstrumentsAsPerStrategy.Add(runningTradableInstrument)

                        Dim dependentTradableInstruments As IEnumerable(Of IInstrument) = allInstruments.Where(Function(x)
                                                                                                                   Return x.RawInstrumentName.ToUpper = instrument.Value.InstrumentName.ToUpper AndAlso
                                                                                                                    x.InstrumentType = IInstrument.TypeOfInstrument.Options AndAlso x.Expiry.HasValue
                                                                                                               End Function)
                        If dependentTradableInstruments IsNot Nothing AndAlso dependentTradableInstruments.Count > 0 Then
                            Dim minExpiry As Date = dependentTradableInstruments.Min(Function(x)
                                                                                         Return x.Expiry.Value
                                                                                     End Function)

                            Dim optionTradableInstruments As IEnumerable(Of IInstrument) = dependentTradableInstruments.Where(Function(x)
                                                                                                                                  Return x.Expiry.Value.Date = minExpiry.Date
                                                                                                                              End Function)
                            If optionTradableInstruments IsNot Nothing AndAlso optionTradableInstruments.Count > 0 Then
                                ret = True
                                If Me.DependentInstruments IsNot Nothing Then
                                    Me.DependentInstruments = Me.DependentInstruments.Concat(optionTradableInstruments)
                                Else
                                    Me.DependentInstruments = optionTradableInstruments
                                End If

                                If minExpiry.Date = Now.Date Then
                                    Me.ExpireToday = True
                                    Dim nextMinExpiry As Date = dependentTradableInstruments.Min(Function(x)
                                                                                                     If x.Expiry.Value.Date > minExpiry.Date Then
                                                                                                         Return x.Expiry.Value
                                                                                                     Else
                                                                                                         Return Date.MaxValue
                                                                                                     End If
                                                                                                 End Function)

                                    Dim nextOptionTradableInstruments As IEnumerable(Of IInstrument) = dependentTradableInstruments.Where(Function(x)
                                                                                                                                              Return x.Expiry.Value.Date = nextMinExpiry.Date
                                                                                                                                          End Function)
                                    If nextOptionTradableInstruments IsNot Nothing AndAlso nextOptionTradableInstruments.Count > 0 Then
                                        If Me.DependentInstruments IsNot Nothing Then
                                            Me.DependentInstruments = Me.DependentInstruments.Concat(nextOptionTradableInstruments)
                                        Else
                                            Me.DependentInstruments = nextOptionTradableInstruments
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    Else
                        OnHeartbeat(String.Format("Unable to find instrument for: {0}", instrument.Key))
                    End If
                Next
                TradableInstrumentsAsPerStrategy = retTradableInstrumentsAsPerStrategy
            End If
        End If

        If retTradableInstrumentsAsPerStrategy IsNot Nothing AndAlso retTradableInstrumentsAsPerStrategy.Count > 0 Then
            'tradableInstrumentsAsPerStrategy = tradableInstrumentsAsPerStrategy.Take(5).ToList
            'Now create the strategy tradable instruments
            Dim retTradableStrategyInstruments As List(Of SpreadStrategyInstrument) = Nothing
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
                If retTradableStrategyInstruments Is Nothing Then retTradableStrategyInstruments = New List(Of SpreadStrategyInstrument)
                Dim runningTradableStrategyInstrument As New SpreadStrategyInstrument(runningTradableInstrument, Me, False, _cts)
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
        Dim retTradableStrategyInstruments As List(Of SpreadStrategyInstrument) = Nothing
        For Each runningTradableInstrument In instrumentsToBeSubscrided
            _cts.Token.ThrowIfCancellationRequested()
            If retTradableStrategyInstruments Is Nothing Then retTradableStrategyInstruments = New List(Of SpreadStrategyInstrument)
            Dim runningTradableStrategyInstrument As New SpreadStrategyInstrument(runningTradableInstrument, Me, False, _cts)
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
            Await Me.GetPositionsDataAsync().ConfigureAwait(False)
            _cts.Token.ThrowIfCancellationRequested()
            Dim tasks As New List(Of Task)()
            For Each tradableStrategyInstrument As SpreadStrategyInstrument In TradableStrategyInstruments
                _cts.Token.ThrowIfCancellationRequested()
                tasks.Add(Task.Run(AddressOf tradableStrategyInstrument.MonitorAsync, _cts.Token))
                If Me.ExpireToday Then tasks.Add(Task.Run(AddressOf tradableStrategyInstrument.ContractRolloverAsync, _cts.Token))
            Next
            'tasks.Add(Task.Run(AddressOf ForceExitAllTradesAsync, _cts.Token))
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
End Class