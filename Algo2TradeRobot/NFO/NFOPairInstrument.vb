Imports NLog
Imports System.IO
Imports System.Threading
Imports Algo2TradeCore.Entities

Public Class NFOPairInstrument

#Region "Logging and Status Progress"
    Public Shared logger As Logger = LogManager.GetCurrentClassLogger
#End Region

#Region "Events/Event handlers"
    Public Event DocumentDownloadComplete()
    Public Event DocumentRetryStatus(ByVal currentTry As Integer, ByVal totalTries As Integer)
    Public Event Heartbeat(ByVal msg As String)
    Public Event WaitingFor(ByVal elapsedSecs As Integer, ByVal totalSecs As Integer, ByVal msg As String)
    'The below functions are needed to allow the derived classes to raise the above two events
    Protected Overridable Sub OnDocumentDownloadComplete()
        RaiseEvent DocumentDownloadComplete()
    End Sub
    Protected Overridable Sub OnDocumentRetryStatus(ByVal currentTry As Integer, ByVal totalTries As Integer)
        RaiseEvent DocumentRetryStatus(currentTry, totalTries)
    End Sub
    Protected Overridable Sub OnHeartbeat(ByVal msg As String)
        RaiseEvent Heartbeat(msg)
    End Sub
    Protected Overridable Sub OnWaitingFor(ByVal elapsedSecs As Integer, ByVal totalSecs As Integer, ByVal msg As String)
        RaiseEvent WaitingFor(elapsedSecs, totalSecs, msg)
    End Sub
#End Region

#Region "Private Class"
    <Serializable>
    Private Class SignalDetails
        Public Property SignalTime As Date
        Public Property EntryZScore As Decimal
        Public Property InstrumentsData As Concurrent.ConcurrentDictionary(Of String, Integer)
    End Class

    Private Class RatioCorrelMeanSD
        Public Property SnapshotDateTime As Date
        Public Property Close1 As Decimal
        Public Property Close2 As Decimal
        Public Property Ratio As Decimal
        Public Property Correl As Decimal
        Public Property Mean As Decimal
        Public Property SD As Decimal
        Public ReadOnly Property ZScore As Decimal
            Get
                If Me.Mean <> 0 Then
                    Return (Me.Ratio - Me.Mean) / Me.SD
                Else
                    Return 0
                End If
            End Get
        End Property
    End Class
#End Region

    Private _lastPrevPayloadTime As Date = Now.Date

    Public ReadOnly Property PairName As String
    Public ReadOnly Property ParentStrategy As NFOStrategy
    Public ReadOnly Property DependentInstruments As List(Of NFOStrategyInstrument)
    Public ReadOnly Property PairRunning As Boolean

    Public ReadOnly Property MyOnePairExitDone As Boolean

    Private ReadOnly _tradesFilename As String
    Private ReadOnly _cts As CancellationTokenSource

    Public Sub New(ByVal pairName As String,
                   ByVal dependentInstruments As List(Of NFOStrategyInstrument),
                   ByVal associatedParentStrategy As NFOStrategy,
                   ByVal canceller As CancellationTokenSource)
        Me.PairName = pairName
        Me.DependentInstruments = dependentInstruments
        Me.ParentStrategy = associatedParentStrategy
        _cts = canceller
        _tradesFilename = Path.Combine(My.Application.Info.DirectoryPath, String.Format("{0} Trades.a2t", Me.PairName))
    End Sub

    Public Overrides Function ToString() As String
        Return Me.PairName
    End Function

    Public Async Function MonitorPairAsync() As Task
        Try
            _PairRunning = True
            If DependentInstruments IsNot Nothing AndAlso DependentInstruments.Count > 0 Then
                Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
                Dim pairSettings As NFOUserInputs.InstrumentDetails = userSettings.InstrumentsData(Me.PairName)
                If DependentInstruments IsNot Nothing AndAlso DependentInstruments.Count = 2 Then
                    Dim signalCheckingInstrument1 As NFOStrategyInstrument = DependentInstruments.FirstOrDefault
                    Dim signalCheckingInstrument2 As NFOStrategyInstrument = DependentInstruments.LastOrDefault

                    Dim preprocessPass As Boolean = False
                    Dim tradedSignal As SignalDetails = Nothing
                    If File.Exists(_tradesFilename) Then
                        tradedSignal = Utilities.Strings.DeserializeToCollection(Of SignalDetails)(_tradesFilename)
                        Interlocked.Increment(Me.ParentStrategy.ParallelPairCount)
                        preprocessPass = True
                        OnHeartbeat(String.Format("{0} : ***** Pair will monitor today as it is active", Me.PairName))
                        SendTelegramMessageAsync(String.Format("{0} : Pair will monitor today as it is active", Me.PairName))
                    Else
                        Dim blankCandlePercentage As Tuple(Of Decimal, Decimal) = Await GetPreviousDayBlankCandlePercentageAsync(signalCheckingInstrument1, signalCheckingInstrument2).ConfigureAwait(False)
                        If blankCandlePercentage IsNot Nothing Then
                            If blankCandlePercentage.Item1 <= userSettings.MaximumBlankCandlePercentage AndAlso
                                blankCandlePercentage.Item2 <= userSettings.MaximumBlankCandlePercentage Then
                                preprocessPass = True
                                OnHeartbeat(String.Format("{0} : ***** Pair will monitor today as blank candle % with in allowed range. {1} Blank Candle:{2}%, {3} Blank Candle:{4}%",
                                                          Me.PairName, signalCheckingInstrument1.TradableInstrument.TradingSymbol, Math.Round(blankCandlePercentage.Item1, 2),
                                                          signalCheckingInstrument2.TradableInstrument.TradingSymbol, Math.Round(blankCandlePercentage.Item2, 2)))
                                SendTelegramMessageAsync(String.Format("{0} : Pair will monitor today as blank candle % with in allowed range. {1} Blank Candle:{2}%, {3} Blank Candle:{4}%",
                                                          Me.PairName, signalCheckingInstrument1.TradableInstrument.TradingSymbol, Math.Round(blankCandlePercentage.Item1, 2),
                                                          signalCheckingInstrument2.TradableInstrument.TradingSymbol, Math.Round(blankCandlePercentage.Item2, 2)))
                            Else
                                OnHeartbeat(String.Format("{0} : Pair will not monitor today as blank candle % outside allowed range. {1} Blank Candle:{2}%, {3} Blank Candle:{4}%",
                                                          Me.PairName, signalCheckingInstrument1.TradableInstrument.TradingSymbol, Math.Round(blankCandlePercentage.Item1, 2),
                                                          signalCheckingInstrument2.TradableInstrument.TradingSymbol, Math.Round(blankCandlePercentage.Item2, 2)))
                                SendTelegramMessageAsync(String.Format("{0} : Pair will not monitor today as blank candle % outside allowed range. {1} Blank Candle:{2}%, {3} Blank Candle:{4}%",
                                                          Me.PairName, signalCheckingInstrument1.TradableInstrument.TradingSymbol, Math.Round(blankCandlePercentage.Item1, 2),
                                                          signalCheckingInstrument2.TradableInstrument.TradingSymbol, Math.Round(blankCandlePercentage.Item2, 2)))
                            End If
                        Else
                            OnHeartbeat(String.Format("{0} : Pair will not monitor today as unable to fetch blank candle information", Me.PairName))
                            SendTelegramMessageAsync(String.Format("{0} : Pair will not monitor today as unable to fetch blank candle information", Me.PairName))
                        End If
                    End If

                    If preprocessPass Then
                        While True
                            If ParentStrategy.ParentController.OrphanException IsNot Nothing Then
                                Throw ParentStrategy.ParentController.OrphanException
                            End If
                            _cts.Token.ThrowIfCancellationRequested()

                            Dim pairRatioCorrelMeanSD As RatioCorrelMeanSD = CalculateRatioMeanSD(signalCheckingInstrument1, signalCheckingInstrument2)
                            If pairRatioCorrelMeanSD IsNot Nothing Then
                                Dim log As Boolean = False
                                If pairRatioCorrelMeanSD.SnapshotDateTime <> _lastPrevPayloadTime Then
                                    _lastPrevPayloadTime = pairRatioCorrelMeanSD.SnapshotDateTime
                                    log = True
                                End If
                                Dim message As String = Nothing
                                If tradedSignal IsNot Nothing Then
                                    Dim exitTrade As Boolean = False
                                    If tradedSignal.EntryZScore > 0 Then
                                        exitTrade = pairRatioCorrelMeanSD.ZScore <= pairSettings.ExitSD
                                        message = String.Format("{0} -> [{4}]Exit Signal, Traded Signal: SELL, Signal Time:{1}, ZScore({2})<=Exit SD({3})[{4}], Close1:{5}, Close2:{6}, Ratio:{7}, Mean:{8}, SD:{9}",
                                                            Me.PairName,
                                                            pairRatioCorrelMeanSD.SnapshotDateTime.ToString("HH:mm:ss"),
                                                            Math.Round(pairRatioCorrelMeanSD.ZScore, 4),
                                                            Math.Round(pairSettings.ExitSD, 4),
                                                            exitTrade,
                                                            pairRatioCorrelMeanSD.Close1,
                                                            pairRatioCorrelMeanSD.Close2,
                                                            Math.Round(pairRatioCorrelMeanSD.Ratio, 4),
                                                            Math.Round(pairRatioCorrelMeanSD.Mean, 4),
                                                            Math.Round(pairRatioCorrelMeanSD.SD, 4))
                                    ElseIf tradedSignal.EntryZScore < 0 Then
                                        exitTrade = pairRatioCorrelMeanSD.ZScore >= pairSettings.ExitSD * -1
                                        message = String.Format("{0} -> [{4}]Exit Signal, Traded Signal: BUY, Signal Time:{1}, ZScore({2})>=Exit SD({3})[{4}], Close1:{5}, Close2:{6}, Ratio:{7}, Mean:{8}, SD:{9}",
                                                            Me.PairName,
                                                            pairRatioCorrelMeanSD.SnapshotDateTime.ToString("HH:mm:ss"),
                                                            Math.Round(pairRatioCorrelMeanSD.ZScore, 4),
                                                            Math.Round(pairSettings.ExitSD, 4),
                                                            exitTrade,
                                                            pairRatioCorrelMeanSD.Close1,
                                                            pairRatioCorrelMeanSD.Close2,
                                                            Math.Round(pairRatioCorrelMeanSD.Ratio, 4),
                                                            Math.Round(pairRatioCorrelMeanSD.Mean, 4),
                                                            Math.Round(pairRatioCorrelMeanSD.SD, 4))
                                    End If

                                    'Dim rollover As Boolean = False
                                    'If Now >= rolloverTime Then
                                    '    Dim ins1Future As NFOStrategyInstrument = GetFutureInstrumentToTrade(signalCheckingInstrument1.TradableInstrument.TradingSymbol)
                                    '    Dim ins2Future As NFOStrategyInstrument = GetFutureInstrumentToTrade(signalCheckingInstrument2.TradableInstrument.TradingSymbol)
                                    '    If Not tradedSignal.InstrumentsData.ContainsKey(ins1Future.TradableInstrument.TradingSymbol) OrElse
                                    '        Not tradedSignal.InstrumentsData.ContainsKey(ins2Future.TradableInstrument.TradingSymbol) Then
                                    '        exitTrade = True
                                    '        rollover = True
                                    '    End If
                                    'End If
                                    If exitTrade Then
                                        OnHeartbeat(message)
                                        SendTelegramMessageAsync(message)
                                        log = False

                                        Dim tradeToExit As New List(Of Tuple(Of NFOStrategyInstrument, IOrder.TypeOfOrder, Integer))
                                        For Each ruuningInstrument In tradedSignal.InstrumentsData
                                            Dim ins As NFOStrategyInstrument =
                                                DependentInstruments.Find(Function(x)
                                                                              Return x.TradableInstrument.TradingSymbol.ToUpper = ruuningInstrument.Key.ToUpper
                                                                          End Function)
                                            tradeToExit.Add(New Tuple(Of NFOStrategyInstrument, IOrder.TypeOfOrder, Integer)(ins, IOrder.TypeOfOrder.Limit, ruuningInstrument.Value * -1))
                                        Next
                                        If tradeToExit IsNot Nothing AndAlso tradeToExit.Count > 0 Then
                                            _MyOnePairExitDone = False
                                            Dim tasks As IEnumerable(Of Task(Of Boolean)) = Nothing
                                            tasks = tradeToExit.Select(Async Function(x)
                                                                           If Await x.Item1.TakeTradeAsync(x.Item2, x.Item3, Me).ConfigureAwait(False) Then
                                                                               _MyOnePairExitDone = True
                                                                               tradedSignal.InstrumentsData.TryRemove(x.Item1.TradableInstrument.TradingSymbol, Nothing)
                                                                           End If
                                                                           Return True
                                                                       End Function)

                                            Await Task.WhenAll(tasks).ConfigureAwait(False)
                                            If tradedSignal.InstrumentsData.Count = 0 Then
                                                tradedSignal = Nothing
                                                File.Delete(_tradesFilename)
                                                Interlocked.Decrement(Me.ParentStrategy.ParallelPairCount)
                                            Else
                                                For Each runningInstrument In tradeToExit
                                                    If tradedSignal.InstrumentsData.ContainsKey(runningInstrument.Item1.TradableInstrument.TradingSymbol) Then
                                                        Await runningInstrument.Item1.TakeTradeAsync(IOrder.TypeOfOrder.Market, runningInstrument.Item3, Me).ConfigureAwait(False)
                                                        tradedSignal = Nothing
                                                        File.Delete(_tradesFilename)
                                                        Interlocked.Decrement(Me.ParentStrategy.ParallelPairCount)
                                                    End If
                                                Next
                                            End If
                                        End If
                                    Else
                                        If tradedSignal.InstrumentsData.Count = 1 Then
                                            'Single Entry
                                            Dim ins1Future As NFOStrategyInstrument = GetFutureInstrumentToTrade(signalCheckingInstrument1.TradableInstrument.Name)
                                            Dim ins2Future As NFOStrategyInstrument = GetFutureInstrumentToTrade(signalCheckingInstrument2.TradableInstrument.Name)
                                            If ins1Future IsNot Nothing AndAlso ins2Future IsNot Nothing Then
                                                Dim tradeToTake As Tuple(Of NFOStrategyInstrument, IOrder.TypeOfOrder, Integer) = Nothing
                                                If tradedSignal.EntryZScore > 0 Then
                                                    'Stock 1 SELL, Stock 2 BUY
                                                    If Not tradedSignal.InstrumentsData.ContainsKey(ins1Future.TradableInstrument.TradingSymbol) Then
                                                        tradeToTake = New Tuple(Of NFOStrategyInstrument, IOrder.TypeOfOrder, Integer)(ins1Future, IOrder.TypeOfOrder.Limit, ins1Future.TradableInstrument.LotSize * -1)
                                                    ElseIf Not tradedSignal.InstrumentsData.ContainsKey(ins2Future.TradableInstrument.TradingSymbol) Then
                                                        tradeToTake = New Tuple(Of NFOStrategyInstrument, IOrder.TypeOfOrder, Integer)(ins2Future, IOrder.TypeOfOrder.Limit, ins2Future.TradableInstrument.LotSize)
                                                    End If
                                                ElseIf tradedSignal.EntryZScore < 0 Then
                                                    'Stock 1 BUY, Stock 2 SELL
                                                    If Not tradedSignal.InstrumentsData.ContainsKey(ins1Future.TradableInstrument.TradingSymbol) Then
                                                        tradeToTake = New Tuple(Of NFOStrategyInstrument, IOrder.TypeOfOrder, Integer)(ins1Future, IOrder.TypeOfOrder.Limit, ins1Future.TradableInstrument.LotSize)
                                                    ElseIf Not tradedSignal.InstrumentsData.ContainsKey(ins2Future.TradableInstrument.TradingSymbol) Then
                                                        tradeToTake = New Tuple(Of NFOStrategyInstrument, IOrder.TypeOfOrder, Integer)(ins2Future, IOrder.TypeOfOrder.Limit, ins2Future.TradableInstrument.LotSize * -1)
                                                    End If
                                                End If
                                                If tradeToTake IsNot Nothing AndAlso tradeToTake.Item1 IsNot Nothing Then
                                                    _MyOnePairExitDone = False
                                                    If Await tradeToTake.Item1.TakeTradeAsync(tradeToTake.Item2, tradeToTake.Item3, Me).ConfigureAwait(False) Then
                                                        tradedSignal.InstrumentsData.TryAdd(tradeToTake.Item1.TradableInstrument.TradingSymbol, tradeToTake.Item3)
                                                        Utilities.Strings.SerializeFromCollection(Of SignalDetails)(_tradesFilename, tradedSignal)
                                                    End If
                                                End If
                                            Else
                                                Throw New ApplicationException("Unable to get future instrument to trade")
                                            End If
                                        End If
                                    End If
                                Else
                                    'Entry
                                    message = String.Format("{0} -> [STATUS]Entry Signal, Signal Time({1})=Current Day({2})[{3}], Correl({4})>=Entry Correl({5})[{6}], ZScore({7})>=Entry SD({8})[{9}], ZScore({7})<=Entry SD({10})[{11}], Minimum Stock Price({12})<=Close1({14})<=Maximum Stock Price({13})[{15}], Minimum Stock Price({12})<=Close2({16})<=Maximum Stock Price({13})[{17}], No. Parallel Pair Running({18})<Allowed Parallel Running({19})[{20}], Ratio:{21}, Mean:{22}, SD:{23}",
                                                        Me.PairName,
                                                        pairRatioCorrelMeanSD.SnapshotDateTime.ToString("dd-MM-yyyy HH:mm:ss"),
                                                        Now.ToString("dd-MM-yyyy"),
                                                        pairRatioCorrelMeanSD.SnapshotDateTime.Date = Now.Date,
                                                        Math.Round(pairRatioCorrelMeanSD.Correl, 4),
                                                        Math.Round(pairSettings.Correl, 4),
                                                        pairRatioCorrelMeanSD.Correl >= pairSettings.Correl,
                                                        Math.Round(pairRatioCorrelMeanSD.ZScore, 4),
                                                        Math.Round(pairSettings.EntrySD, 4),
                                                        pairRatioCorrelMeanSD.ZScore >= pairSettings.EntrySD,
                                                        Math.Round(pairSettings.EntrySD * -1, 4),
                                                        pairRatioCorrelMeanSD.ZScore <= pairSettings.EntrySD * -1,
                                                        userSettings.MinimumStockPrice,
                                                        userSettings.MaximumStockPrice,
                                                        pairRatioCorrelMeanSD.Close1,
                                                        pairRatioCorrelMeanSD.Close1 >= userSettings.MinimumStockPrice AndAlso pairRatioCorrelMeanSD.Close1 <= userSettings.MaximumStockPrice,
                                                        pairRatioCorrelMeanSD.Close2,
                                                        pairRatioCorrelMeanSD.Close2 >= userSettings.MinimumStockPrice AndAlso pairRatioCorrelMeanSD.Close2 <= userSettings.MaximumStockPrice,
                                                        Me.ParentStrategy.ParallelPairCount,
                                                        userSettings.NoOfParallelPair,
                                                        Me.ParentStrategy.ParallelPairCount < userSettings.NoOfParallelPair,
                                                        Math.Round(pairRatioCorrelMeanSD.Ratio, 4),
                                                        Math.Round(pairRatioCorrelMeanSD.Mean, 4),
                                                        Math.Round(pairRatioCorrelMeanSD.SD, 4))

                                    If pairRatioCorrelMeanSD.Correl >= pairSettings.Correl AndAlso pairRatioCorrelMeanSD.SnapshotDateTime.Date = Now.Date AndAlso
                                    pairRatioCorrelMeanSD.Close1 >= userSettings.MinimumStockPrice AndAlso pairRatioCorrelMeanSD.Close1 <= userSettings.MaximumStockPrice AndAlso
                                    pairRatioCorrelMeanSD.Close2 >= userSettings.MinimumStockPrice AndAlso pairRatioCorrelMeanSD.Close2 <= userSettings.MaximumStockPrice Then
                                        Dim tradeToTake As List(Of Tuple(Of NFOStrategyInstrument, IOrder.TypeOfOrder, Integer)) = Nothing
                                        If pairRatioCorrelMeanSD.ZScore >= pairSettings.EntrySD Then
                                            'Stock 1 SELL, Stock 2 BUY
                                            Dim ins1Future As NFOStrategyInstrument = GetFutureInstrumentToTrade(signalCheckingInstrument1.TradableInstrument.Name)
                                            Dim ins2Future As NFOStrategyInstrument = GetFutureInstrumentToTrade(signalCheckingInstrument2.TradableInstrument.Name)
                                            If ins1Future IsNot Nothing AndAlso ins2Future IsNot Nothing Then
                                                tradeToTake = New List(Of Tuple(Of NFOStrategyInstrument, IOrder.TypeOfOrder, Integer))
                                                tradeToTake.Add(New Tuple(Of NFOStrategyInstrument, IOrder.TypeOfOrder, Integer)(ins1Future, IOrder.TypeOfOrder.Limit, ins1Future.TradableInstrument.LotSize * -1))
                                                tradeToTake.Add(New Tuple(Of NFOStrategyInstrument, IOrder.TypeOfOrder, Integer)(ins2Future, IOrder.TypeOfOrder.Limit, ins2Future.TradableInstrument.LotSize))
                                            Else
                                                Throw New ApplicationException("Unable to get future instrument to trade")
                                            End If
                                        ElseIf pairRatioCorrelMeanSD.ZScore <= pairSettings.EntrySD * -1 Then
                                            'Stock 1 BUY, Stock 2 SELL
                                            Dim ins1Future As NFOStrategyInstrument = GetFutureInstrumentToTrade(signalCheckingInstrument1.TradableInstrument.Name)
                                            Dim ins2Future As NFOStrategyInstrument = GetFutureInstrumentToTrade(signalCheckingInstrument2.TradableInstrument.Name)
                                            If ins1Future IsNot Nothing AndAlso ins2Future IsNot Nothing Then
                                                tradeToTake = New List(Of Tuple(Of NFOStrategyInstrument, IOrder.TypeOfOrder, Integer))
                                                tradeToTake.Add(New Tuple(Of NFOStrategyInstrument, IOrder.TypeOfOrder, Integer)(ins1Future, IOrder.TypeOfOrder.Limit, ins1Future.TradableInstrument.LotSize))
                                                tradeToTake.Add(New Tuple(Of NFOStrategyInstrument, IOrder.TypeOfOrder, Integer)(ins2Future, IOrder.TypeOfOrder.Limit, ins2Future.TradableInstrument.LotSize * -1))
                                            Else
                                                Throw New ApplicationException("Unable to get future instrument to trade")
                                            End If
                                        Else
                                            message = message.Replace("STATUS", "False")
                                        End If
                                        If tradeToTake IsNot Nothing AndAlso tradeToTake.Count > 0 Then
                                            If Me.ParentStrategy.ParallelPairCount < userSettings.NoOfParallelPair Then
                                                Interlocked.Increment(Me.ParentStrategy.ParallelPairCount)

                                                message = message.Replace("STATUS", "True")
                                                OnHeartbeat(message)
                                                SendTelegramMessageAsync(message)
                                                log = False

                                                Dim signal As New SignalDetails With {.SignalTime = pairRatioCorrelMeanSD.SnapshotDateTime, .EntryZScore = pairRatioCorrelMeanSD.ZScore, .InstrumentsData = New Concurrent.ConcurrentDictionary(Of String, Integer)}
                                                _MyOnePairExitDone = False
                                                Dim tasks As IEnumerable(Of Task(Of Boolean)) = Nothing
                                                tasks = tradeToTake.Select(Async Function(x)
                                                                               If Await x.Item1.TakeTradeAsync(x.Item2, x.Item3, Me).ConfigureAwait(False) Then
                                                                                   signal.InstrumentsData.TryAdd(x.Item1.TradableInstrument.TradingSymbol, x.Item3)
                                                                               End If
                                                                               Return True
                                                                           End Function)

                                                Await Task.WhenAll(tasks).ConfigureAwait(False)
                                                If signal.InstrumentsData.Count > 0 Then
                                                    Utilities.Strings.SerializeFromCollection(Of SignalDetails)(_tradesFilename, signal)
                                                Else
                                                    Interlocked.Decrement(Me.ParentStrategy.ParallelPairCount)
                                                End If
                                            End If
                                        End If
                                    Else
                                        message = message.Replace("STATUS", "False")
                                    End If
                                End If
                                If log AndAlso message IsNot Nothing Then
                                    OnHeartbeat(message)
                                    'SendTelegramMessageAsync(message)
                                End If
                            End If

                            Await Task.Delay(1000, _cts.Token).ConfigureAwait(False)
                        End While
                    End If
                End If
            End If
        Catch ex As Exception
            logger.Error("Pair Instrument:{0}, error:{1}", Me.ToString, ex.ToString)
            Throw ex
        Finally
            _PairRunning = False
        End Try
    End Function

    Private Function GetFutureInstrumentToTrade(ByVal cashInstrumentName As String) As NFOStrategyInstrument
        Dim ret As NFOStrategyInstrument = Nothing
        If DependentInstruments IsNot Nothing AndAlso DependentInstruments.Count > 0 Then
            Dim futInstruments As List(Of NFOStrategyInstrument) = DependentInstruments.FindAll(Function(x)
                                                                                                    Return x.TradableInstrument.Name = cashInstrumentName AndAlso
                                                                                                     x.TradableInstrument.InstrumentType = IInstrument.TypeOfInstrument.Futures
                                                                                                End Function)
            If futInstruments IsNot Nothing AndAlso futInstruments.Count > 0 Then
                Dim maxExpiry As Date = futInstruments.Max(Function(x)
                                                               Return x.TradableInstrument.Expiry.Value
                                                           End Function)
                ret = futInstruments.Find(Function(x) x.TradableInstrument.Expiry.Value = maxExpiry)
            End If
        End If
        Return ret
    End Function

    Private Async Function GetPreviousDayBlankCandlePercentageAsync(ByVal ins1 As NFOStrategyInstrument, ByVal ins2 As NFOStrategyInstrument) As Task(Of Tuple(Of Decimal, Decimal))
        Dim ret As Tuple(Of Decimal, Decimal) = Nothing
        While True
            If ins1.TradableInstrument.IsHistoricalCompleted AndAlso ins2.TradableInstrument.IsHistoricalCompleted Then
                Dim ins1Payload As Concurrent.ConcurrentDictionary(Of Date, IPayload) = ins1.GetXMinutePayload(ParentStrategy.UserSettings.SignalTimeFrame)
                Dim ins2Payload As Concurrent.ConcurrentDictionary(Of Date, IPayload) = ins2.GetXMinutePayload(ParentStrategy.UserSettings.SignalTimeFrame)
                If ins1Payload IsNot Nothing AndAlso ins2Payload IsNot Nothing Then
                    Dim maxAvailableDate As Date = ins1Payload.Keys.Max(Function(x)
                                                                            If x < Now.Date Then
                                                                                Return x
                                                                            Else
                                                                                Return Date.MinValue
                                                                            End If
                                                                        End Function)

                    Dim ins1BlankCandlesCount As Integer = 0
                    Dim ins2BlankCandlesCount As Integer = 0
                    If maxAvailableDate <> Date.MinValue Then
                        Dim ins1BlankCandles = ins1Payload.Where(Function(x)
                                                                     Return x.Key.Date = maxAvailableDate.Date AndAlso
                                                                        CType(x.Value, OHLCPayload).HighPrice.Value = CType(x.Value, OHLCPayload).LowPrice.Value
                                                                 End Function)
                        If ins1BlankCandles IsNot Nothing Then ins1BlankCandlesCount = ins1BlankCandles.Count

                        Dim ins2BlankCandles = ins2Payload.Where(Function(x)
                                                                     Return x.Key.Date = maxAvailableDate.Date AndAlso
                                                                        CType(x.Value, OHLCPayload).HighPrice.Value = CType(x.Value, OHLCPayload).LowPrice.Value
                                                                 End Function)
                        If ins2BlankCandles IsNot Nothing Then ins2BlankCandlesCount = ins2BlankCandles.Count
                    End If
                    ret = New Tuple(Of Decimal, Decimal)((ins1BlankCandlesCount / 375) * 100, (ins2BlankCandlesCount / 375) * 100)
                    Exit While
                End If
            End If

            Await Task.Delay(30000, _cts.Token).ConfigureAwait(False)
        End While
        Return ret
    End Function

    Private Function CalculateRatioMeanSD(ByVal ins1 As NFOStrategyInstrument, ByVal ins2 As NFOStrategyInstrument) As RatioCorrelMeanSD
        Dim ret As RatioCorrelMeanSD = Nothing
        If ins1.TradableInstrument.IsHistoricalCompleted AndAlso ins2.TradableInstrument.IsHistoricalCompleted Then
            'ins1.TradableInstrument.FetchHistorical = False
            'ins2.TradableInstrument.FetchHistorical = False
            Dim pairSettings As NFOUserInputs.InstrumentDetails = CType(ParentStrategy.UserSettings, NFOUserInputs).InstrumentsData(Me.PairName)

            Dim ins1Payload As Concurrent.ConcurrentDictionary(Of Date, IPayload) = ins1.GetXMinutePayload(ParentStrategy.UserSettings.SignalTimeFrame)
            Dim ins2Payload As Concurrent.ConcurrentDictionary(Of Date, IPayload) = ins2.GetXMinutePayload(ParentStrategy.UserSettings.SignalTimeFrame)
            If ins1Payload IsNot Nothing AndAlso ins1Payload.Count >= pairSettings.LoopBackPeriod AndAlso
                ins2Payload IsNot Nothing AndAlso ins2Payload.Count >= pairSettings.LoopBackPeriod Then
                Dim ins1CurrentCandle As OHLCPayload = ins1.GetXMinuteCurrentCandle(ParentStrategy.UserSettings.SignalTimeFrame)
                Dim ins2CurrentCandle As OHLCPayload = ins2.GetXMinuteCurrentCandle(ParentStrategy.UserSettings.SignalTimeFrame)
                If ins1CurrentCandle IsNot Nothing AndAlso ins2CurrentCandle IsNot Nothing Then
                    Dim currentCandleTime As Date = ins1CurrentCandle.SnapshotDateTime
                    If ins1CurrentCandle.SnapshotDateTime < ins2CurrentCandle.SnapshotDateTime Then
                        currentCandleTime = ins2CurrentCandle.SnapshotDateTime
                    End If

                    Dim timelist As List(Of Date) = Nothing
                    Dim ctr As Integer = 0
                    For Each runningPayload In ins1Payload.OrderByDescending(Function(x)
                                                                                 Return x.Key
                                                                             End Function)
                        If runningPayload.Key < currentCandleTime Then
                            ctr += 1
                            If timelist Is Nothing Then timelist = New List(Of Date)
                            If Not timelist.Contains(runningPayload.Key) Then timelist.Add(runningPayload.Key)

                            If ctr >= pairSettings.LoopBackPeriod Then Exit For
                        End If
                    Next
                    ctr = 0
                    For Each runningPayload In ins2Payload.OrderByDescending(Function(x)
                                                                                 Return x.Key
                                                                             End Function)
                        If runningPayload.Key < currentCandleTime Then
                            ctr += 1
                            If timelist Is Nothing Then timelist = New List(Of Date)
                            If Not timelist.Contains(runningPayload.Key) Then timelist.Add(runningPayload.Key)

                            If ctr >= pairSettings.LoopBackPeriod Then Exit For
                        End If
                    Next

                    If timelist IsNot Nothing AndAlso timelist.Count > 0 Then
                        Dim close1List As New List(Of Decimal)
                        Dim close2List As New List(Of Decimal)
                        Dim ratioList As New List(Of Decimal)
                        For Each runningTime In timelist.OrderByDescending(Function(x)
                                                                               Return x
                                                                           End Function)
                            Dim ins1Candle As OHLCPayload = Nothing
                            If ins1Payload.ContainsKey(runningTime) Then
                                ins1Candle = ins1Payload(runningTime)
                            Else
                                ins1Candle = ins1Payload.Where(Function(x)
                                                                   Return x.Key <= runningTime
                                                               End Function).OrderByDescending(Function(y)
                                                                                                   Return y.Key
                                                                                               End Function).FirstOrDefault.Value
                            End If

                            Dim ins2Candle As OHLCPayload = Nothing
                            If ins2Payload.ContainsKey(runningTime) Then
                                ins2Candle = ins2Payload(runningTime)
                            Else
                                ins2Candle = ins2Payload.Where(Function(x)
                                                                   Return x.Key <= runningTime
                                                               End Function).OrderByDescending(Function(y)
                                                                                                   Return y.Key
                                                                                               End Function).FirstOrDefault.Value
                            End If

                            close1List.Add(ins1Candle.ClosePrice.Value)
                            close2List.Add(ins2Candle.ClosePrice.Value)
                            ratioList.Add(ins1Candle.ClosePrice.Value / ins2Candle.ClosePrice.Value)

                            If ratioList.Count >= pairSettings.LoopBackPeriod Then Exit For
                        Next

                        ret = New RatioCorrelMeanSD With
                            {
                                .Correl = CalculateCorrelation(close1List.ToArray, close2List.ToArray) * 100,
                                .Close1 = close1List.FirstOrDefault,
                                .Close2 = close2List.FirstOrDefault,
                                .Ratio = ratioList.FirstOrDefault,
                                .Mean = ratioList.Average(),
                                .SD = CalculateStandardDeviationPA(ratioList.ToArray),
                                .SnapshotDateTime = timelist.OrderByDescending(Function(x) x).FirstOrDefault
                            }
                    End If
                End If
            End If
        End If
        Return ret
    End Function

    Public Function CalculateStandardDeviationPA(values As Decimal()) As Double
        Dim ret As Double = Double.MinValue
        If values IsNot Nothing AndAlso values.Count >= 2 Then
            Dim sum As Decimal = 0
            For i As Integer = 0 To values.Length - 1
                sum += values(i)
            Next
            Dim mean As Decimal = sum / values.Length
            Dim sumVariance As Double = 0
            For i As Integer = 0 To values.Length - 1
                sumVariance += Math.Pow(values(i) - mean, 2)
            Next
            Dim sampleVariance As Double = sumVariance / values.Length
            Dim standardDeviation As Double = Math.Sqrt(sampleVariance)
            ret = standardDeviation
        End If
        Return ret
    End Function

    Public Function CalculateCorrelation(values1 As Decimal(), values2 As Decimal()) As Double
        If values1.Length = values2.Length Then
            Dim avg1 As Decimal = values1.Average()
            Dim avg2 As Decimal = values2.Average()
            Dim sum As Decimal = 0
            For i As Integer = 0 To values1.Length - 1
                sum += (values1(i) - avg1) * (values2(i) - avg2)
            Next
            Dim sumSqr1 = values1.Sum(Function(x) Math.Pow(x - avg1, 2))
            Dim sumSqr2 = values2.Sum(Function(x) Math.Pow(x - avg2, 2))
            Dim result = sum / Math.Sqrt(sumSqr1 * sumSqr2)
            Return If(Double.IsNaN(result), 0, result)
        Else
            Return 0
        End If
    End Function

    Private Async Function SendTelegramMessageAsync(ByVal message As String) As Task
        Try
            Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
            _cts.Token.ThrowIfCancellationRequested()
            If Me.ParentStrategy.ParentController.UserInputs.TelegramAPIKey IsNot Nothing AndAlso
                Me.ParentStrategy.ParentController.UserInputs.TelegramAPIKey.Trim <> "" AndAlso
                Me.ParentStrategy.ParentController.UserInputs.TelegramChatID IsNot Nothing AndAlso
                Me.ParentStrategy.ParentController.UserInputs.TelegramChatID.Trim <> "" Then
                Using tSender As New Utilities.Notification.Telegram(Me.ParentStrategy.ParentController.UserInputs.TelegramAPIKey.Trim, Me.ParentStrategy.ParentController.UserInputs.TelegramChatID.Trim, _cts)
                    Dim encodedString As String = Utilities.Strings.UrlEncodeString(message)
                    Await tSender.SendMessageGetAsync(encodedString).ConfigureAwait(False)
                End Using
            End If
            logger.Fatal(message)
        Catch ex As Exception
            logger.Warn(ex.ToString)
        End Try
    End Function
End Class