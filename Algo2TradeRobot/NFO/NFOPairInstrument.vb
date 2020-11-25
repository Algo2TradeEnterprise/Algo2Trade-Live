Imports NLog
Imports System.IO
Imports System.Threading
Imports Algo2TradeCore.Entities
Imports Algo2TradeCore.Strategies
Imports System.Text.RegularExpressions

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
        Public TradedDate As Date
        Public Property Mean As Decimal
        Public Property SD As Decimal
        Public Property SignalType As String
        Public Property InstrumentsData As Dictionary(Of String, Integer)
    End Class
#End Region

    Private _lastPrevPayload As String = ""

    Public ReadOnly PairName As String

    Private ReadOnly _filename As String
    Private ReadOnly _DependentInstruments As List(Of NFOStrategyInstrument)
    Private ReadOnly _ParentStrategy As NFOStrategy
    Private ReadOnly _cts As CancellationTokenSource

    Public Sub New(ByVal dependentInstruments As List(Of NFOStrategyInstrument),
                   ByVal associatedParentStrategy As NFOStrategy,
                   ByVal pairName As String,
                   ByVal canceller As CancellationTokenSource)
        _DependentInstruments = dependentInstruments
        _ParentStrategy = associatedParentStrategy
        Me.PairName = pairName
        _cts = canceller
        _filename = Path.Combine(My.Application.Info.DirectoryPath, String.Format("{0} Trades.a2t", Me.PairName))
    End Sub

    Public Overrides Function ToString() As String
        Return Me.PairName
    End Function

    Public Async Function MonitorPairAsync() As Task
        Try
            If _DependentInstruments IsNot Nothing AndAlso _DependentInstruments.Count > 0 Then
                Dim userSettings As NFOUserInputs = Me._ParentStrategy.UserSettings
                Dim signalCheckingInstruments As List(Of NFOStrategyInstrument) = _DependentInstruments.FindAll(Function(x)
                                                                                                                    Return x.TradableInstrument.InstrumentType = IInstrument.TypeOfInstrument.Cash
                                                                                                                End Function)
                If signalCheckingInstruments IsNot Nothing AndAlso signalCheckingInstruments.Count = 2 Then
                    Dim signalCheckingInstrument1 As NFOStrategyInstrument = signalCheckingInstruments.FirstOrDefault
                    Dim signalCheckingInstrument2 As NFOStrategyInstrument = signalCheckingInstruments.LastOrDefault

                    Dim tradedSignal As SignalDetails = Nothing
                    Dim rolloverTime As Date = New Date(Now.Year, Now.Month, Now.Day, 15, 28, 0)
                    Dim meadSd As Tuple(Of Boolean, Decimal, Decimal) = Nothing
                    While True
                        If _ParentStrategy.ParentController.OrphanException IsNot Nothing Then
                            Throw _ParentStrategy.ParentController.OrphanException
                        End If
                        _cts.Token.ThrowIfCancellationRequested()
                        If tradedSignal Is Nothing AndAlso File.Exists(_filename) Then
                            tradedSignal = Utilities.Strings.DeserializeToCollection(Of SignalDetails)(_filename)
                            'meadSd = New Tuple(Of Boolean, Decimal, Decimal)(True, tradedSignal.Mean, tradedSignal.SD)
                        End If
                        If meadSd IsNot Nothing AndAlso meadSd.Item1 Then
                            Dim ins1Candle As OHLCPayload = signalCheckingInstrument1.GetXMinuteCurrentCandle(userSettings.SignalTimeFrame)
                            Dim ins2Candle As OHLCPayload = signalCheckingInstrument2.GetXMinuteCurrentCandle(userSettings.SignalTimeFrame)
                            If ins1Candle IsNot Nothing AndAlso ins2Candle IsNot Nothing AndAlso
                                ins1Candle.SnapshotDateTime = ins2Candle.SnapshotDateTime AndAlso
                                ins1Candle.PreviousPayload IsNot Nothing AndAlso ins2Candle.PreviousPayload IsNot Nothing AndAlso
                                ins1Candle.PreviousPayload.SnapshotDateTime.Date = Now.Date Then
                                Dim ratio As Decimal = ins1Candle.PreviousPayload.ClosePrice.Value / ins2Candle.PreviousPayload.ClosePrice.Value
                                Dim log As Boolean = False
                                Dim message As String = Nothing
                                Try
                                    Dim candleString As String = String.Format("{0}, {1}", ins1Candle.PreviousPayload.ToString, ins2Candle.PreviousPayload.ToString)
                                    If Not candleString = _lastPrevPayload Then
                                        _lastPrevPayload = candleString
                                        log = True
                                    End If
                                Catch ex As Exception
                                    logger.Warn(ex)
                                End Try

                                If tradedSignal IsNot Nothing Then
                                    If tradedSignal.SignalType = "+" Then
                                        If userSettings.SameSideExit Then
                                            message = String.Format("{0} -> [{8}]Exit Signal, Traded Signal: Positive, Signal Time:{1}, Exit Side:{2}, Ratio({3})<={4}[Mean({5})+{6}*SD({7})][{8}]",
                                                                    Me.PairName,
                                                                    ins1Candle.PreviousPayload.SnapshotDateTime.ToString("HH:mm:ss"),
                                                                    If(userSettings.SameSideExit, "Same Side", "Opposite Side"),
                                                                    Math.Round(ratio, 6),
                                                                    Math.Round(tradedSignal.Mean + tradedSignal.SD * userSettings.ExitSDMultiplier, 6),
                                                                    Math.Round(tradedSignal.Mean, 6),
                                                                    userSettings.ExitSDMultiplier,
                                                                    Math.Round(tradedSignal.SD, 6),
                                                                    ratio <= tradedSignal.Mean + tradedSignal.SD * userSettings.ExitSDMultiplier)
                                        Else
                                            message = String.Format("{0} -> [{8}]Exit Signal, Traded Signal: Positive, Signal Time:{1}, Exit Side:{2}, Ratio({3})<={4}[Mean({5})-{6}*SD({7})][{8}]",
                                                                    Me.PairName,
                                                                    ins1Candle.PreviousPayload.SnapshotDateTime.ToString("HH:mm:ss"),
                                                                    If(userSettings.SameSideExit, "Same Side", "Opposite Side"),
                                                                    Math.Round(ratio, 6),
                                                                    Math.Round(tradedSignal.Mean - tradedSignal.SD * userSettings.ExitSDMultiplier, 6),
                                                                    Math.Round(tradedSignal.Mean, 6),
                                                                    userSettings.ExitSDMultiplier,
                                                                    Math.Round(tradedSignal.SD, 6),
                                                                    ratio <= tradedSignal.Mean - tradedSignal.SD * userSettings.ExitSDMultiplier)
                                        End If
                                    ElseIf tradedSignal.SignalType = "-" Then
                                        If userSettings.SameSideExit Then
                                            message = String.Format("{0} -> [{8}]Exit Signal, Traded Signal: Negative, Signal Time:{1}, Exit Side:{2}, Ratio({3})>={4}[Mean({5})-{6}*SD({7})][{8}]",
                                                                    Me.PairName,
                                                                    ins1Candle.PreviousPayload.SnapshotDateTime.ToString("HH:mm:ss"),
                                                                    If(userSettings.SameSideExit, "Same Side", "Opposite Side"),
                                                                    Math.Round(ratio, 6),
                                                                    Math.Round(tradedSignal.Mean - tradedSignal.SD * userSettings.ExitSDMultiplier, 6),
                                                                    Math.Round(tradedSignal.Mean, 6),
                                                                    userSettings.ExitSDMultiplier,
                                                                    Math.Round(tradedSignal.SD, 6),
                                                                    ratio >= tradedSignal.Mean - tradedSignal.SD * userSettings.ExitSDMultiplier)
                                        Else
                                            message = String.Format("{0} -> [{8}]Exit Signal, Traded Signal: Negative, Signal Time:{1}, Exit Side:{2}, Ratio({3})>={4}[Mean({5})+{6}*SD({7})][{8}]",
                                                                    Me.PairName,
                                                                    ins1Candle.PreviousPayload.SnapshotDateTime.ToString("HH:mm:ss"),
                                                                    If(userSettings.SameSideExit, "Same Side", "Opposite Side"),
                                                                    Math.Round(ratio, 6),
                                                                    Math.Round(tradedSignal.Mean + tradedSignal.SD * userSettings.ExitSDMultiplier, 6),
                                                                    Math.Round(tradedSignal.Mean, 6),
                                                                    userSettings.ExitSDMultiplier,
                                                                    Math.Round(tradedSignal.SD, 6),
                                                                    ratio >= tradedSignal.Mean + tradedSignal.SD * userSettings.ExitSDMultiplier)
                                        End If
                                    End If

                                    Dim exitTrade As Boolean = False
                                    Dim rollover As Boolean = False
                                    If Now >= rolloverTime Then
                                        Dim ins1Future As NFOStrategyInstrument = GetFutureInstrumentToTrade(signalCheckingInstrument1.TradableInstrument.TradingSymbol)
                                        Dim ins2Future As NFOStrategyInstrument = GetFutureInstrumentToTrade(signalCheckingInstrument2.TradableInstrument.TradingSymbol)
                                        If Not tradedSignal.InstrumentsData.ContainsKey(ins1Future.TradableInstrument.TradingSymbol) OrElse
                                            Not tradedSignal.InstrumentsData.ContainsKey(ins2Future.TradableInstrument.TradingSymbol) Then
                                            exitTrade = True
                                            rollover = True
                                        End If
                                    End If
                                    If tradedSignal.SignalType = "+" Then
                                        If userSettings.SameSideExit Then
                                            If ratio <= tradedSignal.Mean + tradedSignal.SD * userSettings.ExitSDMultiplier Then
                                                exitTrade = True
                                            End If
                                        Else
                                            If ratio <= tradedSignal.Mean - tradedSignal.SD * userSettings.ExitSDMultiplier Then
                                                exitTrade = True
                                            End If
                                        End If
                                    ElseIf tradedSignal.SignalType = "-" Then
                                        If userSettings.SameSideExit Then
                                            If ratio >= tradedSignal.Mean - tradedSignal.SD * userSettings.ExitSDMultiplier Then
                                                exitTrade = True
                                            End If
                                        Else
                                            If ratio >= tradedSignal.Mean + tradedSignal.SD * userSettings.ExitSDMultiplier Then
                                                exitTrade = True
                                            End If
                                        End If
                                    End If
                                    If exitTrade Then
                                        Dim tradeToExit As Dictionary(Of String, Tuple(Of NFOStrategyInstrument, Integer, String)) = New Dictionary(Of String, Tuple(Of NFOStrategyInstrument, Integer, String))
                                        For Each ruuningInstrument In tradedSignal.InstrumentsData
                                            Dim ins As NFOStrategyInstrument =
                                                    _DependentInstruments.Find(Function(x)
                                                                                   Return x.TradableInstrument.TradingSymbol.ToUpper = ruuningInstrument.Key.ToUpper
                                                                               End Function)
                                            If ins IsNot Nothing Then
                                                Dim reFut As Regex = New Regex("(JAN|FEB|MAR|APR|MAY|JUN|JUL|AUG|SEP|OCT|NOV|DEC)FUT$")
                                                If reFut.IsMatch(ruuningInstrument.Key.ToUpper) Then
                                                    tradeToExit.Add(ins.TradableInstrument.InstrumentIdentifier, New Tuple(Of NFOStrategyInstrument, Integer, String)(ins, ruuningInstrument.Value * -1, tradedSignal.SignalType))
                                                Else
                                                    If Not rollover Then
                                                        tradeToExit.Add(ins.TradableInstrument.InstrumentIdentifier, New Tuple(Of NFOStrategyInstrument, Integer, String)(ins, ruuningInstrument.Value * -1, tradedSignal.SignalType))
                                                    End If
                                                End If
                                            End If
                                        Next
                                        If tradeToExit IsNot Nothing AndAlso tradeToExit.Count > 0 Then
                                            Dim tradedData As Concurrent.ConcurrentBag(Of Tuple(Of NFOStrategyInstrument, Integer, String)) = New Concurrent.ConcurrentBag(Of Tuple(Of NFOStrategyInstrument, Integer, String))
                                            Dim tasks = tradeToExit.Select(Async Function(x)
                                                                               Try
                                                                                   Await x.Value.Item1.MonitorAsync(StrategyInstrument.ExecuteCommands.PlaceRegularLimitCNCOrder, x.Value.Item2).ConfigureAwait(False)
                                                                                   tradedData.Add(x.Value)
                                                                               Catch nex As Exception
                                                                                   logger.Error(nex.ToString)
                                                                                   Throw nex
                                                                               End Try
                                                                               Return True
                                                                           End Function)
                                            Await Task.WhenAll(tasks).ConfigureAwait(False)
                                            If tradedData IsNot Nothing AndAlso tradedData.Count > 0 Then
                                                If Not rollover Then
                                                    tradedData = Nothing
                                                    If File.Exists(_filename) Then File.Delete(_filename)
                                                Else
                                                    While True
                                                        Dim takeTrade As Boolean = True
                                                        For Each runningData In tradedData
                                                            takeTrade = takeTrade And Not runningData.Item1.IsActiveInstrument()
                                                        Next
                                                        If takeTrade Then Exit While
                                                        Await Task.Delay(500).ConfigureAwait(False)
                                                    End While
                                                    Dim tradeToTake As Dictionary(Of String, Tuple(Of NFOStrategyInstrument, Integer, String)) = New Dictionary(Of String, Tuple(Of NFOStrategyInstrument, Integer, String))
                                                    For Each runningData In tradedData
                                                        If tradedSignal.InstrumentsData.ContainsKey(runningData.Item1.TradableInstrument.TradingSymbol) Then
                                                            tradedSignal.InstrumentsData.Remove(runningData.Item1.TradableInstrument.TradingSymbol)
                                                            Dim insFuture As NFOStrategyInstrument = GetFutureInstrumentToTrade(runningData.Item1.TradableInstrument.RawInstrumentName)
                                                            tradeToTake.Add(insFuture.TradableInstrument.InstrumentIdentifier, New Tuple(Of NFOStrategyInstrument, Integer, String)(insFuture, runningData.Item2 * -1, tradedSignal.SignalType))
                                                        End If
                                                    Next
                                                    Dim rollData As Concurrent.ConcurrentBag(Of Tuple(Of NFOStrategyInstrument, Integer, String)) = New Concurrent.ConcurrentBag(Of Tuple(Of NFOStrategyInstrument, Integer, String))
                                                    Dim rolltasks = tradeToTake.Select(Async Function(x)
                                                                                           Try
                                                                                               Await x.Value.Item1.MonitorAsync(StrategyInstrument.ExecuteCommands.PlaceRegularLimitCNCOrder, x.Value.Item2).ConfigureAwait(False)
                                                                                               rollData.Add(x.Value)
                                                                                           Catch nex As Exception
                                                                                               logger.Error(nex.ToString)
                                                                                               Throw nex
                                                                                           End Try
                                                                                           Return True
                                                                                       End Function)
                                                    Await Task.WhenAll(rolltasks).ConfigureAwait(False)
                                                    If rollData IsNot Nothing AndAlso rollData.Count > 0 Then
                                                        For Each runningSignal In rollData
                                                            tradedSignal.InstrumentsData.Add(runningSignal.Item1.TradableInstrument.TradingSymbol, runningSignal.Item2)
                                                        Next
                                                        Utilities.Strings.SerializeFromCollection(Of SignalDetails)(_filename, tradedSignal)
                                                    End If
                                                End If
                                            End If
                                        End If
                                    End If
                                Else
                                    'Entry
                                    Dim tradeToTake As Dictionary(Of String, Tuple(Of NFOStrategyInstrument, Integer, String)) = Nothing
                                    message = String.Format("{0} -> Entry Signal, Signal Time:{1}, Ratio({2})>={3}[Mean({4})+{5}*SD({6})][{7}], Ratio({2})<={8}[Mean({4})-{5}*SD({6})[{9}]",
                                                            Me.PairName,
                                                            ins1Candle.PreviousPayload.SnapshotDateTime.ToString("HH:mm:ss"),
                                                            Math.Round(ratio, 6),
                                                            Math.Round(meadSd.Item2 + meadSd.Item3 * userSettings.EntrySDMultiplier, 6),
                                                            Math.Round(meadSd.Item2, 6),
                                                            userSettings.EntrySDMultiplier,
                                                            Math.Round(meadSd.Item3, 6),
                                                            ratio >= meadSd.Item2 + meadSd.Item3 * userSettings.EntrySDMultiplier,
                                                            Math.Round(meadSd.Item2 - meadSd.Item3 * userSettings.EntrySDMultiplier, 6),
                                                            ratio <= meadSd.Item2 - meadSd.Item3 * userSettings.EntrySDMultiplier)

                                    If ratio >= meadSd.Item2 + meadSd.Item3 * userSettings.EntrySDMultiplier Then
                                        'Stock 2 BUY, Stock 1 SELL
                                        tradeToTake = New Dictionary(Of String, Tuple(Of NFOStrategyInstrument, Integer, String))
                                        Dim ins1Future As NFOStrategyInstrument = GetFutureInstrumentToTrade(signalCheckingInstrument1.TradableInstrument.TradingSymbol)
                                        Dim ins2Future As NFOStrategyInstrument = GetFutureInstrumentToTrade(signalCheckingInstrument2.TradableInstrument.TradingSymbol)
                                        tradeToTake.Add(ins1Future.TradableInstrument.InstrumentIdentifier, New Tuple(Of NFOStrategyInstrument, Integer, String)(ins1Future, ins1Future.TradableInstrument.LotSize * userSettings.InstrumentsData(Me.PairName).NumberOfLots * -1, "+"))
                                        tradeToTake.Add(ins2Future.TradableInstrument.InstrumentIdentifier, New Tuple(Of NFOStrategyInstrument, Integer, String)(ins2Future, ins2Future.TradableInstrument.LotSize * userSettings.InstrumentsData(Me.PairName).NumberOfLots, "+"))

                                        Dim ins1Turnover As Decimal = ins1Future.TradableInstrument.LotSize * userSettings.InstrumentsData(Me.PairName).NumberOfLots * ins1Future.TradableInstrument.LastTick.LastPrice
                                        Dim ins2Turnover As Decimal = ins2Future.TradableInstrument.LotSize * userSettings.InstrumentsData(Me.PairName).NumberOfLots * ins2Future.TradableInstrument.LastTick.LastPrice
                                        If ins2Turnover < ins1Turnover Then
                                            Dim quantity As Integer = Math.Ceiling((ins1Turnover - ins2Turnover) / ins2Candle.ClosePrice.Value)
                                            tradeToTake.Add(signalCheckingInstrument2.TradableInstrument.InstrumentIdentifier, New Tuple(Of NFOStrategyInstrument, Integer, String)(signalCheckingInstrument2, quantity, "+"))
                                        End If
                                    ElseIf ratio <= meadSd.Item2 - meadSd.Item3 * userSettings.EntrySDMultiplier Then
                                        'Stock 1 BUY, Stock 2 SELL
                                        tradeToTake = New Dictionary(Of String, Tuple(Of NFOStrategyInstrument, Integer, String))
                                        Dim ins1Future As NFOStrategyInstrument = GetFutureInstrumentToTrade(signalCheckingInstrument1.TradableInstrument.TradingSymbol)
                                        Dim ins2Future As NFOStrategyInstrument = GetFutureInstrumentToTrade(signalCheckingInstrument2.TradableInstrument.TradingSymbol)
                                        tradeToTake.Add(ins1Future.TradableInstrument.InstrumentIdentifier, New Tuple(Of NFOStrategyInstrument, Integer, String)(ins1Future, ins1Future.TradableInstrument.LotSize * userSettings.InstrumentsData(Me.PairName).NumberOfLots, "-"))
                                        tradeToTake.Add(ins2Future.TradableInstrument.InstrumentIdentifier, New Tuple(Of NFOStrategyInstrument, Integer, String)(ins2Future, ins2Future.TradableInstrument.LotSize * userSettings.InstrumentsData(Me.PairName).NumberOfLots * -1, "-"))

                                        Dim ins1Turnover As Decimal = ins1Future.TradableInstrument.LotSize * userSettings.InstrumentsData(Me.PairName).NumberOfLots * ins1Future.TradableInstrument.LastTick.LastPrice
                                        Dim ins2Turnover As Decimal = ins2Future.TradableInstrument.LotSize * userSettings.InstrumentsData(Me.PairName).NumberOfLots * ins2Future.TradableInstrument.LastTick.LastPrice
                                        If ins1Turnover < ins2Turnover Then
                                            Dim quantity As Integer = Math.Ceiling((ins2Turnover - ins1Turnover) / ins1Candle.ClosePrice.Value)
                                            tradeToTake.Add(signalCheckingInstrument1.TradableInstrument.InstrumentIdentifier, New Tuple(Of NFOStrategyInstrument, Integer, String)(signalCheckingInstrument1, quantity, "-"))
                                        End If
                                    End If
                                    If tradeToTake IsNot Nothing AndAlso tradeToTake.Count > 0 Then
                                        Dim tradedData As Concurrent.ConcurrentBag(Of Tuple(Of NFOStrategyInstrument, Integer, String)) = New Concurrent.ConcurrentBag(Of Tuple(Of NFOStrategyInstrument, Integer, String))
                                        Dim tasks = tradeToTake.Select(Async Function(x)
                                                                           Try
                                                                               Await x.Value.Item1.MonitorAsync(StrategyInstrument.ExecuteCommands.PlaceRegularLimitCNCOrder, x.Value.Item2).ConfigureAwait(False)
                                                                               tradedData.Add(x.Value)
                                                                           Catch nex As Exception
                                                                               logger.Error(nex.ToString)
                                                                               Throw nex
                                                                           End Try
                                                                           Return True
                                                                       End Function)
                                        Await Task.WhenAll(tasks).ConfigureAwait(False)
                                        If tradedData IsNot Nothing AndAlso tradedData.Count > 0 Then
                                            Dim signal As SignalDetails = New SignalDetails
                                            signal.TradedDate = Now.Date
                                            signal.Mean = meadSd.Item2
                                            signal.SD = meadSd.Item3
                                            signal.SignalType = tradedData.FirstOrDefault.Item3
                                            signal.InstrumentsData = New Dictionary(Of String, Integer)
                                            For Each runningSignal In tradedData
                                                signal.InstrumentsData.Add(runningSignal.Item1.TradableInstrument.TradingSymbol, runningSignal.Item2)
                                            Next

                                            Utilities.Strings.SerializeFromCollection(Of SignalDetails)(_filename, signal)
                                        End If
                                    End If
                                End If
                                If log AndAlso message IsNot Nothing Then
                                    OnHeartbeat(message)
                                End If
                            End If
                        Else
                            meadSd = CalculateMeanSD(signalCheckingInstrument1, signalCheckingInstrument2)
                        End If

                        Await Task.Delay(1000, _cts.Token).ConfigureAwait(False)
                    End While
                End If
            End If
        Catch ex As Exception
            logger.Error("Pair Instrument:{0}, error:{1}", Me.ToString, ex.ToString)
            Throw ex
        End Try
    End Function

    Private Function GetFutureInstrumentToTrade(ByVal cashInstrumentName As String) As NFOStrategyInstrument
        Dim ret As NFOStrategyInstrument = Nothing
        If _DependentInstruments IsNot Nothing AndAlso _DependentInstruments.Count > 0 Then
            Dim futInstruments As List(Of NFOStrategyInstrument) = _DependentInstruments.FindAll(Function(x)
                                                                                                     Return x.TradableInstrument.RawInstrumentName = cashInstrumentName AndAlso
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

    Private Function CalculateMeanSD(ByVal ins1 As NFOStrategyInstrument, ByVal ins2 As NFOStrategyInstrument) As Tuple(Of Boolean, Decimal, Decimal)
        Dim ret As Tuple(Of Boolean, Decimal, Decimal) = Nothing
        If ins1.TradableInstrument.IsHistoricalCompleted AndAlso ins2.TradableInstrument.IsHistoricalCompleted Then
            ins1.TradableInstrument.FetchHistorical = False
            ins2.TradableInstrument.FetchHistorical = False
            Dim ins1Payload As Concurrent.ConcurrentDictionary(Of Date, IPayload) = ins1.GetXMinutePayload(_ParentStrategy.UserSettings.SignalTimeFrame)
            Dim ins2Payload As Concurrent.ConcurrentDictionary(Of Date, IPayload) = ins2.GetXMinutePayload(_ParentStrategy.UserSettings.SignalTimeFrame)
            If ins1Payload IsNot Nothing AndAlso ins2Payload IsNot Nothing AndAlso ins1Payload.Count = ins2Payload.Count Then
                Dim ratioPayload As Dictionary(Of Date, Decimal) = Nothing
                Dim noOfDays As Integer = 0
                Dim lastDay As Date = Date.MinValue
                For Each runningPayload In ins1Payload.OrderByDescending(Function(x)
                                                                             Return x.Key
                                                                         End Function)
                    If runningPayload.Key.Date <> Now.Date Then
                        If ins2Payload.ContainsKey(runningPayload.Key) Then
                            If lastDay <> runningPayload.Key.Date Then
                                lastDay = runningPayload.Key.Date
                                noOfDays += 1
                                If noOfDays > CType(_ParentStrategy.UserSettings, NFOUserInputs).DaysBack Then Exit For
                            End If
                            Dim ins1Candle As OHLCPayload = ins1Payload(runningPayload.Key)
                            Dim ins2Candle As OHLCPayload = ins2Payload(runningPayload.Key)
                            If ratioPayload Is Nothing Then ratioPayload = New Dictionary(Of Date, Decimal)
                            ratioPayload.Add(runningPayload.Key, ins1Candle.ClosePrice.Value / ins2Candle.ClosePrice.Value)
                        Else
                            Throw New ApplicationException("Data mismatch")
                        End If
                    End If
                Next
                If ratioPayload IsNot Nothing AndAlso ratioPayload.Count > 0 Then
                    Dim mean As Decimal = ratioPayload.Average(Function(x) x.Value)
                    Dim sd As Decimal = CalculateStandardDeviationPA(ratioPayload)

                    ret = New Tuple(Of Boolean, Decimal, Decimal)(True, mean, sd)
                End If
            End If
        End If
        Return ret
    End Function

    Private Function CalculateStandardDeviationPA(ByVal inputPayload As Dictionary(Of Date, Decimal)) As Double
        Dim ret As Double = Nothing
        If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
            Dim sum As Double = 0
            For Each runningPayload In inputPayload.Keys
                sum = sum + inputPayload(runningPayload)
            Next
            Dim mean As Double = sum / inputPayload.Count
            Dim sumVariance As Double = 0
            For Each runningPayload In inputPayload.Keys
                sumVariance = sumVariance + Math.Pow((inputPayload(runningPayload) - mean), 2)
            Next
            Dim sampleVariance As Double = sumVariance / (inputPayload.Count)
            Dim standardDeviation As Double = Math.Sqrt(sampleVariance)
            ret = standardDeviation
        End If
        Return ret
    End Function

    Private Async Function SendTradeAlertMessageAsync(ByVal message As String) As Task
        Try
            Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
            _cts.Token.ThrowIfCancellationRequested()
            message = String.Format("{0} -> {1}", Me.PairName, message)
            Dim userInputs As NFOUserInputs = _ParentStrategy.UserSettings
            If userInputs.TelegramBotAPIKey IsNot Nothing AndAlso Not userInputs.TelegramBotAPIKey.Trim = "" AndAlso
                userInputs.TelegramTradeChatID IsNot Nothing AndAlso Not userInputs.TelegramTradeChatID.Trim = "" Then
                Using tSender As New Utilities.Notification.Telegram(userInputs.TelegramBotAPIKey.Trim, userInputs.TelegramTradeChatID.Trim, _cts)
                    Dim encodedString As String = Utilities.Strings.UrlEncodeString(message)
                    Await tSender.SendMessageGetAsync(encodedString).ConfigureAwait(False)
                End Using
            End If
        Catch ex As Exception
            logger.Warn(ex.ToString)
        End Try
    End Function
End Class