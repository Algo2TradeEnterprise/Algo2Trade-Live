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

    Public TakeTradeLock As Integer = 0

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
            If userInputs.InstrumentDetailsFilepath IsNot Nothing AndAlso File.Exists(userInputs.InstrumentDetailsFilepath) Then
                If userInputs.AutoSelectStock Then
                    Dim atrInstruments As List(Of IInstrument) = Nothing
                    Using fillInstruments As New NFOFillInstrumentDetails(_cts, Me)
                        atrInstruments = Await fillInstruments.GetInstrumentDataAsync(allInstruments, bannedInstruments).ConfigureAwait(False)
                    End Using
                    If atrInstruments IsNot Nothing AndAlso atrInstruments.Count > 0 Then
                        Dim dt As DataTable = New DataTable
                        dt.Columns.Add("Trading Symbol")
                        For Each runningStock In atrInstruments
                            Dim row As DataRow = dt.NewRow
                            row("Trading Symbol") = runningStock.TradingSymbol.ToUpper
                            dt.Rows.Add(row)
                        Next
                        File.Delete(userInputs.InstrumentDetailsFilepath)
                        Using csv As New Utilities.DAL.CSVHelper(userInputs.InstrumentDetailsFilepath, ",", _cts)
                            csv.GetCSVFromDataTable(dt)
                        End Using
                    End If
                End If

                Dim stockDT As DataTable = Nothing
                Using csv As New Utilities.DAL.CSVHelper(userInputs.InstrumentDetailsFilepath, ",", _cts)
                    stockDT = csv.GetDataTableFromCSV(1)
                End Using

                If stockDT IsNot Nothing AndAlso stockDT.Rows.Count > 0 Then
                    Dim stkCtr As Integer = 0
                    For Each runningRow As DataRow In stockDT.Rows
                        _cts.Token.ThrowIfCancellationRequested()
                        Dim runningStock As String = runningRow.Item("Trading Symbol")

                        Dim runningInstrument As IInstrument = allInstruments.ToList.Find(Function(x)
                                                                                              Return x.TradingSymbol.ToUpper = runningStock.Trim.ToUpper AndAlso
                                                                                              x.InstrumentType = IInstrument.TypeOfInstrument.Cash
                                                                                          End Function)

                        If retTradableInstrumentsAsPerStrategy Is Nothing Then retTradableInstrumentsAsPerStrategy = New List(Of IInstrument)
                        retTradableInstrumentsAsPerStrategy.Add(runningInstrument)
                        ret = True

                        stkCtr += 1
                        If stkCtr >= userInputs.NumberOfStocks Then Exit For
                    Next
                End If

                'Dim cdsStocks As List(Of String) = New List(Of String) From {"USDINR", "EURINR", "GBPINR", "JPYINR"}
                'For Each runningStock In cdsStocks
                '    _cts.Token.ThrowIfCancellationRequested()
                '    Dim runningInstruments As List(Of IInstrument) = allInstruments.ToList.FindAll(Function(x)
                '                                                                                       Return x.RawInstrumentName.ToUpper = runningStock.ToUpper AndAlso
                '                                                                                       x.InstrumentType = IInstrument.TypeOfInstrument.Futures
                '                                                                                   End Function)
                '    If runningInstruments IsNot Nothing AndAlso runningInstruments.Count > 0 Then
                '        Dim minExpiry As Date = runningInstruments.Min(Function(x)
                '                                                           If x.Expiry.Value > Now.Date Then
                '                                                               Return x.Expiry.Value
                '                                                           Else
                '                                                               Return Date.MaxValue
                '                                                           End If
                '                                                       End Function)
                '        If minExpiry <> Date.MinValue Then
                '            Dim runningInstrument As IInstrument = runningInstruments.Find(Function(x)
                '                                                                               Return x.Expiry.Value = minExpiry
                '                                                                           End Function)
                '            If runningInstrument IsNot Nothing Then
                '                If retTradableInstrumentsAsPerStrategy Is Nothing Then retTradableInstrumentsAsPerStrategy = New List(Of IInstrument)
                '                retTradableInstrumentsAsPerStrategy.Add(runningInstrument)
                '                ret = True
                '            End If
                '        End If
                '    End If
                'Next
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
        ElseIf overallPL <= userSettings.OverallMaxLoss Then
            logger.Debug("Max loss reached. Overall PL: {0}", overallPL)
            ret = New Tuple(Of Boolean, String)(True, "Max Loss Per Day Reached")
        ElseIf overallPL >= userSettings.OverallMaxProfit Then
            logger.Debug("Max Profit reached. Overall PL: {0}", overallPL)
            ret = New Tuple(Of Boolean, String)(True, "Max Profit Per Day Reached")
        End If
        Return ret
    End Function
End Class
