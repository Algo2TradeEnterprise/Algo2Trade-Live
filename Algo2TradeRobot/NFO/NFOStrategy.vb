Imports NLog
Imports System.Threading
Imports Algo2TradeCore.Controller
Imports Algo2TradeCore.Entities
Imports Algo2TradeCore.Strategies
Imports System.IO

Public Class NFOStrategy
    Inherits Strategy
#Region "Logging and Status Progress"
    Public Shared Shadows logger As Logger = LogManager.GetCurrentClassLogger
#End Region

    Public Property OptionInstruments As List(Of IInstrument)

    Public Property TotalActiveInstrumentCount As Integer
    Public TradePlacementLock As Integer = 0

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
            If userInputs.InstrumentDetailsFilepath IsNot Nothing AndAlso File.Exists(userInputs.InstrumentDetailsFilepath) Then
                If userInputs.AutoSelectStock Then
                    Dim atrInstruments As List(Of String) = Nothing
                    Using fillInstruments As New NFOFillInstrumentDetails(_cts, Me)
                        atrInstruments = Await fillInstruments.GetInstrumentDataAsync(allInstruments, bannedInstruments).ConfigureAwait(False)
                    End Using
                    If atrInstruments IsNot Nothing AndAlso atrInstruments.Count > 0 Then
                        Dim dt As DataTable = New DataTable
                        dt.Columns.Add("Trading Symbol")
                        For Each runningStock In atrInstruments
                            Dim row As DataRow = dt.NewRow
                            row("Trading Symbol") = runningStock
                            dt.Rows.Add(row)
                        Next
                        File.Delete(userInputs.InstrumentDetailsFilepath)
                        Using csv As New Utilities.DAL.CSVHelper(userInputs.InstrumentDetailsFilepath, ",", _cts)
                            csv.GetCSVFromDataTable(dt)
                        End Using

                        userInputs.InstrumentsData = Nothing
                        userInputs.FillInstrumentDetails(userInputs.InstrumentDetailsFilepath, _cts)
                    End If
                End If
            End If
            If userInputs.InstrumentsData IsNot Nothing AndAlso userInputs.InstrumentsData.Count > 0 Then
                Dim dummyAllInstruments As List(Of IInstrument) = allInstruments.ToList

                Dim runningInstruments As List(Of String) = GetRunningInstrumentList()
                If runningInstruments IsNot Nothing AndAlso runningInstruments.Count > 0 Then
                    For Each runinstrument In runningInstruments
                        _cts.Token.ThrowIfCancellationRequested()
                        Dim runningTradableInstrument As IInstrument = dummyAllInstruments.Find(Function(x)
                                                                                                    Return x.TradingSymbol.ToUpper = runinstrument.ToUpper
                                                                                                End Function)

                        _cts.Token.ThrowIfCancellationRequested()
                        If retTradableInstrumentsAsPerStrategy Is Nothing Then retTradableInstrumentsAsPerStrategy = New List(Of IInstrument)
                        If runningTradableInstrument IsNot Nothing Then
                            retTradableInstrumentsAsPerStrategy.Add(runningTradableInstrument)
                            Dim myOptionContracts As List(Of IInstrument) = GetCurrentOptionContracts(dummyAllInstruments, runningTradableInstrument)
                            If myOptionContracts IsNot Nothing Then
                                If Me.OptionInstruments Is Nothing Then Me.OptionInstruments = New List(Of IInstrument)
                                Me.OptionInstruments.AddRange(myOptionContracts)
                            End If
                            ret = True
                        End If
                    Next
                End If

                For Each instrument In userInputs.InstrumentsData
                    _cts.Token.ThrowIfCancellationRequested()
                    If runningInstruments Is Nothing OrElse Not runningInstruments.Contains(instrument.Value.TradingSymbol, StringComparer.OrdinalIgnoreCase) Then
                        Dim runningTradableInstrument As IInstrument = dummyAllInstruments.Find(Function(x)
                                                                                                    Return x.TradingSymbol.ToUpper = instrument.Value.TradingSymbol.ToUpper
                                                                                                End Function)

                        _cts.Token.ThrowIfCancellationRequested()
                        If retTradableInstrumentsAsPerStrategy Is Nothing Then retTradableInstrumentsAsPerStrategy = New List(Of IInstrument)
                        If runningTradableInstrument IsNot Nothing Then
                            retTradableInstrumentsAsPerStrategy.Add(runningTradableInstrument)
                            Dim myOptionContracts As List(Of IInstrument) = GetCurrentOptionContracts(dummyAllInstruments, runningTradableInstrument)
                            If myOptionContracts IsNot Nothing Then
                                If Me.OptionInstruments Is Nothing Then Me.OptionInstruments = New List(Of IInstrument)
                                Me.OptionInstruments.AddRange(myOptionContracts)
                            End If
                            ret = True
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
                Dim runningTradableStrategyInstrument As New NFOStrategyInstrument(runningTradableInstrument, Me, False, _cts, Nothing)
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

    Private Function GetCurrentOptionContracts(ByVal allInstruments As List(Of IInstrument), ByVal cashInstrument As IInstrument) As List(Of IInstrument)
        Dim ret As List(Of IInstrument) = Nothing
        If allInstruments IsNot Nothing AndAlso allInstruments.Count > 0 Then
            Dim myOptionContracts As List(Of IInstrument) = allInstruments.FindAll(Function(x)
                                                                                       Return x.InstrumentType = IInstrument.TypeOfInstrument.Options AndAlso
                                                                                       x.RawInstrumentName.ToUpper = cashInstrument.RawInstrumentName.ToUpper
                                                                                   End Function)

            Dim minExpiry As Date = myOptionContracts.Min(Function(x)
                                                              If Now.Date <= x.Expiry.Value.AddDays(-2).Date Then
                                                                  Return x.Expiry.Value.Date
                                                              Else
                                                                  Return Date.MaxValue
                                                              End If
                                                          End Function)

            Dim minExpryInstrmts As List(Of IInstrument) = myOptionContracts.FindAll(Function(x)
                                                                                         Return x.Expiry.Value.Date = minExpiry.Date
                                                                                     End Function)
            If ret Is Nothing Then ret = New List(Of IInstrument)
            ret.AddRange(minExpryInstrmts)

            If minExpiry.Date.AddDays(-2) = Now.Date Then
                Dim nextMinExpiry As Date = myOptionContracts.Min(Function(x)
                                                                      If x.Expiry.Value.Date > minExpiry.Date Then
                                                                          Return x.Expiry.Value.Date
                                                                      Else
                                                                          Return Date.MaxValue
                                                                      End If
                                                                  End Function)
                Dim nxtMinExpryInstrmt As List(Of IInstrument) = myOptionContracts.FindAll(Function(x)
                                                                                               Return x.Expiry.Value.Date = nextMinExpiry.Date
                                                                                           End Function)
                ret.AddRange(nxtMinExpryInstrmt)
            End If
        End If
        Return ret
    End Function

    Public Async Function CreateDependentTradableStrategyInstrumentsAsync(ByVal instrumentToBeSubscrided As IInstrument, ByVal myParentStrategyInstrument As NFOStrategyInstrument) As Task(Of NFOStrategyInstrument)
        Dim ret As NFOStrategyInstrument = Nothing
        Dim retTradableStrategyInstruments As List(Of NFOStrategyInstrument) = Nothing
        _cts.Token.ThrowIfCancellationRequested()
        If retTradableStrategyInstruments Is Nothing Then retTradableStrategyInstruments = New List(Of NFOStrategyInstrument)
        Dim runningTradableStrategyInstrument As New NFOStrategyInstrument(instrumentToBeSubscrided, Me, False, _cts, myParentStrategyInstrument)
        AddHandler runningTradableStrategyInstrument.HeartbeatEx, AddressOf OnHeartbeatEx
        AddHandler runningTradableStrategyInstrument.WaitingForEx, AddressOf OnWaitingForEx
        AddHandler runningTradableStrategyInstrument.DocumentRetryStatusEx, AddressOf OnDocumentRetryStatusEx
        AddHandler runningTradableStrategyInstrument.DocumentDownloadCompleteEx, AddressOf OnDocumentDownloadCompleteEx

        retTradableStrategyInstruments.Add(runningTradableStrategyInstrument)
        ret = runningTradableStrategyInstrument
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

    Private Function GetRunningInstrumentList() As List(Of String)
        Dim ret As List(Of String) = Nothing
        If Directory.Exists(Path.Combine(My.Application.Info.DirectoryPath, "Signals")) Then
            For Each runningFile In Directory.GetFiles(Path.Combine(My.Application.Info.DirectoryPath, "Signals"), "*.SignalDetails.a2t")
                Dim runningSignal As SignalDetails = Utilities.Strings.DeserializeToCollection(Of SignalDetails)(runningFile)
                If runningSignal IsNot Nothing AndAlso runningSignal.IsActiveSignal Then
                    If ret Is Nothing Then ret = New List(Of String)
                    ret.Add(runningSignal.InstrumentName)
                End If
            Next
        End If
        Return ret
    End Function
End Class
