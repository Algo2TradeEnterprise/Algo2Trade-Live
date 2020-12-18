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
        Public Property ZScore As Decimal
        Public Property YStock As String
        Public Property SignalType As String
        Public Property InstrumentsData As Dictionary(Of String, Tuple(Of Integer, Decimal))
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
        If File.Exists(_filename) Then
            _ParentStrategy.ActiveTradeCount += 1
        End If
    End Sub

    Public Overrides Function ToString() As String
        Return Me.PairName
    End Function

    Public Async Function MonitorPairAsync() As Task
        Try
            If _DependentInstruments IsNot Nothing AndAlso _DependentInstruments.Count > 0 Then
                Dim userSettings As NFOUserInputs = Me._ParentStrategy.UserSettings
                Dim pairDetails As NFOUserInputs.SectorDetails = Nothing
                For Each runningSector In userSettings.SectorData
                    If runningSector.Value.InstrumentsData IsNot Nothing Then
                        If runningSector.Value.InstrumentsData.ContainsKey(Me.PairName) Then
                            pairDetails = runningSector.Value
                            Exit For
                        End If
                    End If
                Next

                Dim signalCheckingInstruments As List(Of NFOStrategyInstrument) = _DependentInstruments.FindAll(Function(x)
                                                                                                                    Return x.TradableInstrument.InstrumentType = IInstrument.TypeOfInstrument.Cash
                                                                                                                End Function)
                If signalCheckingInstruments IsNot Nothing AndAlso signalCheckingInstruments.Count = 2 Then
                    Dim signalCheckingInstrument1 As NFOStrategyInstrument = signalCheckingInstruments.FirstOrDefault
                    Dim signalCheckingInstrument2 As NFOStrategyInstrument = signalCheckingInstruments.LastOrDefault

                    Dim tradedSignal As SignalDetails = Nothing
                    Dim rolloverTime As Date = New Date(Now.Year, Now.Month, Now.Day, 15, 28, 0)
                    Dim rgsn As Tuple(Of Boolean, Boolean, Date, Decimal, String, Decimal, Decimal) = Nothing
                    While True
                        If _ParentStrategy.ParentController.OrphanException IsNot Nothing Then
                            Throw _ParentStrategy.ParentController.OrphanException
                        End If
                        _cts.Token.ThrowIfCancellationRequested()
                        If tradedSignal Is Nothing AndAlso File.Exists(_filename) Then
                            tradedSignal = Utilities.Strings.DeserializeToCollection(Of SignalDetails)(_filename)
                        End If
                        If rgsn IsNot Nothing AndAlso rgsn.Item1 Then
                            Dim ins1Candle As OHLCPayload = signalCheckingInstrument1.GetXMinuteCurrentCandle(userSettings.SignalTimeFrame)
                            Dim ins2Candle As OHLCPayload = signalCheckingInstrument2.GetXMinuteCurrentCandle(userSettings.SignalTimeFrame)
                            If ins1Candle IsNot Nothing AndAlso ins2Candle IsNot Nothing AndAlso
                                ins1Candle.SnapshotDateTime = ins2Candle.SnapshotDateTime AndAlso
                                ins1Candle.PreviousPayload IsNot Nothing AndAlso ins2Candle.PreviousPayload IsNot Nothing Then

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
                                If log Then
                                    rgsn = CalculateRegression(signalCheckingInstrument1, signalCheckingInstrument2)
                                End If

                                If tradedSignal IsNot Nothing Then
                                    'Exit
                                    Dim priceChange As Dictionary(Of String, Decimal) = Nothing
                                    For Each runningInstrument In tradedSignal.InstrumentsData
                                        If runningInstrument.Value.Item2 = 0 Then
                                            Dim ins As NFOStrategyInstrument =
                                                        _DependentInstruments.Find(Function(x)
                                                                                       Return x.TradableInstrument.TradingSymbol.ToUpper = runningInstrument.Key.ToUpper
                                                                                   End Function)
                                            If ins IsNot Nothing Then
                                                Dim lastOrder As IBusinessOrder = ins.GetLastExecutedOrder()
                                                If lastOrder.ParentOrder.Status = IOrder.TypeOfStatus.Complete Then
                                                    If priceChange Is Nothing Then priceChange = New Dictionary(Of String, Decimal)
                                                    priceChange.Add(runningInstrument.Key, lastOrder.ParentOrder.AveragePrice)
                                                End If
                                            End If
                                        End If
                                    Next
                                    If priceChange IsNot Nothing AndAlso priceChange.Count > 0 Then
                                        Dim tempInstrumentsData As Dictionary(Of String, Tuple(Of Integer, Decimal)) = tradedSignal.InstrumentsData
                                        tradedSignal.InstrumentsData = New Dictionary(Of String, Tuple(Of Integer, Decimal))
                                        For Each runningIns In tempInstrumentsData
                                            If runningIns.Value.Item2 = 0 Then
                                                If priceChange.ContainsKey(runningIns.Key) Then
                                                    tradedSignal.InstrumentsData.Add(runningIns.Key, New Tuple(Of Integer, Decimal)(runningIns.Value.Item1, priceChange(runningIns.Key)))
                                                Else
                                                    tradedSignal.InstrumentsData.Add(runningIns.Key, New Tuple(Of Integer, Decimal)(runningIns.Value.Item1, runningIns.Value.Item2))
                                                End If
                                            Else
                                                tradedSignal.InstrumentsData.Add(runningIns.Key, New Tuple(Of Integer, Decimal)(runningIns.Value.Item1, runningIns.Value.Item2))
                                            End If
                                        Next
                                        Utilities.Strings.SerializeFromCollection(Of SignalDetails)(_filename, tradedSignal)
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

                                    Dim totalPl As Decimal = 0
                                    For Each runningInstrument In tradedSignal.InstrumentsData
                                        If runningInstrument.Value.Item2 <> 0 Then
                                            Dim ins As NFOStrategyInstrument =
                                                        _DependentInstruments.Find(Function(x)
                                                                                       Return x.TradableInstrument.TradingSymbol.ToUpper = runningInstrument.Key.ToUpper
                                                                                   End Function)
                                            Dim entryPrice As Decimal = runningInstrument.Value.Item2
                                            Dim entryQty As Decimal = runningInstrument.Value.Item1
                                            Dim price As Decimal = ins.TradableInstrument.LastTick.LastPrice

                                            If entryQty > 0 Then
                                                totalPl += (price - entryPrice) * Math.Abs(entryQty)
                                            Else
                                                totalPl += (entryPrice - price) * Math.Abs(entryQty)
                                            End If
                                        Else
                                            totalPl = 0
                                            Exit For
                                        End If
                                    Next

                                    message = String.Format("{0} -> Exit Signal, PL:{1}", Me.PairName, totalPl)
                                    OnHeartbeat(message)

                                    If totalPl >= pairDetails.Target Then
                                        exitTrade = True
                                    End If
                                    If exitTrade Then
                                        Dim tradeToExit As Dictionary(Of String, Tuple(Of NFOStrategyInstrument, Integer, String)) = New Dictionary(Of String, Tuple(Of NFOStrategyInstrument, Integer, String))
                                        For Each runningInstrument In tradedSignal.InstrumentsData
                                            Dim ins As NFOStrategyInstrument =
                                                    _DependentInstruments.Find(Function(x)
                                                                                   Return x.TradableInstrument.TradingSymbol.ToUpper = runningInstrument.Key.ToUpper
                                                                               End Function)
                                            If ins IsNot Nothing Then
                                                Dim reFut As Regex = New Regex("(JAN|FEB|MAR|APR|MAY|JUN|JUL|AUG|SEP|OCT|NOV|DEC)FUT$")
                                                If reFut.IsMatch(runningInstrument.Key.ToUpper) Then
                                                    tradeToExit.Add(ins.TradableInstrument.InstrumentIdentifier, New Tuple(Of NFOStrategyInstrument, Integer, String)(ins, runningInstrument.Value.Item1 * -1, tradedSignal.SignalType))
                                                Else
                                                    If Not rollover Then
                                                        tradeToExit.Add(ins.TradableInstrument.InstrumentIdentifier, New Tuple(Of NFOStrategyInstrument, Integer, String)(ins, runningInstrument.Value.Item1 * -1, tradedSignal.SignalType))
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
                                                    tradedSignal = Nothing
                                                    If File.Exists(_filename) Then File.Delete(_filename)
                                                    _ParentStrategy.ActiveTradeCount -= 1
                                                Else
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
                                                            tradedSignal.InstrumentsData.Add(runningSignal.Item1.TradableInstrument.TradingSymbol, New Tuple(Of Integer, Decimal)(runningSignal.Item2, 0))
                                                        Next
                                                        Utilities.Strings.SerializeFromCollection(Of SignalDetails)(_filename, tradedSignal)
                                                    End If
                                                End If
                                            End If
                                        End If
                                    End If
                                Else
                                    'Entry
                                    If rgsn IsNot Nothing AndAlso rgsn.Item1 Then
                                        If rgsn.Item2 Then
                                            If rgsn.Item3.AddMinutes(5) <= signalCheckingInstrument1.TradableInstrument.LastTick.Timestamp.Value Then
                                                message = String.Format("{0} -> Entry Signal, Signal Time:{1}, Z-Score:{2}, Y-Stock:{3}, Corel:{4}, Intercept%:{5}",
                                                                            Me.PairName,
                                                                            ins1Candle.PreviousPayload.SnapshotDateTime.ToString("HH:mm:ss"),
                                                                            rgsn.Item4,
                                                                            rgsn.Item5,
                                                                            rgsn.Item6,
                                                                            rgsn.Item7)

                                                Dim ins1Future As NFOStrategyInstrument = GetFutureInstrumentToTrade(signalCheckingInstrument1.TradableInstrument.TradingSymbol)
                                                Dim ins2Future As NFOStrategyInstrument = GetFutureInstrumentToTrade(signalCheckingInstrument2.TradableInstrument.TradingSymbol)
                                                If Not File.Exists(signalCheckingInstrument1.TradeFileName) AndAlso Not File.Exists(signalCheckingInstrument2.TradeFileName) AndAlso
                                                    Not File.Exists(ins1Future.TradeFileName) AndAlso Not File.Exists(ins2Future.TradeFileName) Then
                                                    Dim tradeToTake As Dictionary(Of String, Tuple(Of NFOStrategyInstrument, Integer, String)) = Nothing
                                                    If rgsn.Item4 > 0 Then
                                                        tradeToTake = New Dictionary(Of String, Tuple(Of NFOStrategyInstrument, Integer, String))

                                                        Dim stk1Mul As Integer = 0
                                                        Dim stk2Mul As Integer = 0
                                                        If rgsn.Item5.ToUpper = signalCheckingInstrument1.TradableInstrument.TradingSymbol Then
                                                            stk1Mul = -1
                                                            stk2Mul = 1
                                                        Else
                                                            stk1Mul = 1
                                                            stk2Mul = -1
                                                        End If

                                                        tradeToTake.Add(ins1Future.TradableInstrument.InstrumentIdentifier, New Tuple(Of NFOStrategyInstrument, Integer, String)(ins1Future, ins1Future.TradableInstrument.LotSize * stk1Mul, "+"))
                                                        tradeToTake.Add(ins2Future.TradableInstrument.InstrumentIdentifier, New Tuple(Of NFOStrategyInstrument, Integer, String)(ins2Future, ins2Future.TradableInstrument.LotSize * stk2Mul, "+"))

                                                        Dim ins1Turnover As Decimal = ins1Future.TradableInstrument.LotSize * ins1Future.TradableInstrument.LastTick.LastPrice
                                                        Dim ins2Turnover As Decimal = ins2Future.TradableInstrument.LotSize * ins2Future.TradableInstrument.LastTick.LastPrice
                                                        If stk2Mul = 1 AndAlso ins2Turnover < ins1Turnover Then
                                                            Dim quantity As Integer = Math.Ceiling((ins1Turnover - ins2Turnover) / ins2Candle.ClosePrice.Value)
                                                            tradeToTake.Add(signalCheckingInstrument2.TradableInstrument.InstrumentIdentifier, New Tuple(Of NFOStrategyInstrument, Integer, String)(signalCheckingInstrument2, quantity, "+"))
                                                        ElseIf stk2Mul = 1 AndAlso ins2Turnover > ins1Turnover Then
                                                            Dim beta = (ins2Turnover / (ins1Turnover + ins2Turnover)) - (ins1Turnover / (ins1Turnover + ins2Turnover))
                                                            If beta * 100 > 5 Then
                                                                tradeToTake = Nothing
                                                                message = String.Format("{0} -> Trade Neglect: Unable to cash neutral({6}). Entry Signal, Signal Time:{1}, Z-Score:{2}, Y-Stock:{3}, Corel:{4}, Intercept%:{5}",
                                                                                        Me.PairName,
                                                                                        ins1Candle.PreviousPayload.SnapshotDateTime.ToString("HH:mm:ss"),
                                                                                        rgsn.Item4,
                                                                                        rgsn.Item5,
                                                                                        rgsn.Item6,
                                                                                        rgsn.Item7,
                                                                                        Math.Round(beta * 100, 2))
                                                            Else
                                                                message = String.Format("{0} -> Trade Taken: Cash neutral({6}). Entry Signal, Signal Time:{1}, Z-Score:{2}, Y-Stock:{3}, Corel:{4}, Intercept%:{5}",
                                                                                        Me.PairName,
                                                                                        ins1Candle.PreviousPayload.SnapshotDateTime.ToString("HH:mm:ss"),
                                                                                        rgsn.Item4,
                                                                                        rgsn.Item5,
                                                                                        rgsn.Item6,
                                                                                        rgsn.Item7,
                                                                                        Math.Round(beta * 100, 2))
                                                            End If
                                                        ElseIf stk1Mul = 1 AndAlso ins1Turnover < ins2Turnover Then
                                                            Dim quantity As Integer = Math.Ceiling((ins2Turnover - ins1Turnover) / ins1Candle.ClosePrice.Value)
                                                            tradeToTake.Add(signalCheckingInstrument1.TradableInstrument.InstrumentIdentifier, New Tuple(Of NFOStrategyInstrument, Integer, String)(signalCheckingInstrument1, quantity, "+"))
                                                        ElseIf stk1Mul = 1 AndAlso ins1Turnover > ins2Turnover Then
                                                            Dim beta = (ins1Turnover / (ins1Turnover + ins2Turnover)) - (ins2Turnover / (ins1Turnover + ins2Turnover))
                                                            If beta * 100 > 5 Then
                                                                tradeToTake = Nothing
                                                                message = String.Format("{0} -> Trade Neglect: Unable to cash neutral({6}). Entry Signal, Signal Time:{1}, Z-Score:{2}, Y-Stock:{3}, Corel:{4}, Intercept%:{5}",
                                                                                        Me.PairName,
                                                                                        ins1Candle.PreviousPayload.SnapshotDateTime.ToString("HH:mm:ss"),
                                                                                        rgsn.Item4,
                                                                                        rgsn.Item5,
                                                                                        rgsn.Item6,
                                                                                        rgsn.Item7,
                                                                                        Math.Round(beta * 100, 2))
                                                            Else
                                                                message = String.Format("{0} -> Trade Taken: Cash neutral({6}). Entry Signal, Signal Time:{1}, Z-Score:{2}, Y-Stock:{3}, Corel:{4}, Intercept%:{5}",
                                                                                        Me.PairName,
                                                                                        ins1Candle.PreviousPayload.SnapshotDateTime.ToString("HH:mm:ss"),
                                                                                        rgsn.Item4,
                                                                                        rgsn.Item5,
                                                                                        rgsn.Item6,
                                                                                        rgsn.Item7,
                                                                                        Math.Round(beta * 100, 2))
                                                            End If
                                                        End If
                                                    ElseIf rgsn.Item4 < 0 Then
                                                        tradeToTake = New Dictionary(Of String, Tuple(Of NFOStrategyInstrument, Integer, String))

                                                        Dim stk1Mul As Integer = 0
                                                        Dim stk2Mul As Integer = 0
                                                        If rgsn.Item5.ToUpper = signalCheckingInstrument1.TradableInstrument.TradingSymbol Then
                                                            stk1Mul = 1
                                                            stk2Mul = -1
                                                        Else
                                                            stk1Mul = -1
                                                            stk2Mul = 1
                                                        End If

                                                        tradeToTake.Add(ins1Future.TradableInstrument.InstrumentIdentifier, New Tuple(Of NFOStrategyInstrument, Integer, String)(ins1Future, ins1Future.TradableInstrument.LotSize * stk1Mul, "-"))
                                                        tradeToTake.Add(ins2Future.TradableInstrument.InstrumentIdentifier, New Tuple(Of NFOStrategyInstrument, Integer, String)(ins2Future, ins2Future.TradableInstrument.LotSize * stk2Mul, "-"))

                                                        Dim ins1Turnover As Decimal = ins1Future.TradableInstrument.LotSize * ins1Future.TradableInstrument.LastTick.LastPrice
                                                        Dim ins2Turnover As Decimal = ins2Future.TradableInstrument.LotSize * ins2Future.TradableInstrument.LastTick.LastPrice
                                                        If stk2Mul = 1 AndAlso ins2Turnover < ins1Turnover Then
                                                            Dim quantity As Integer = Math.Ceiling((ins1Turnover - ins2Turnover) / ins2Candle.ClosePrice.Value)
                                                            tradeToTake.Add(signalCheckingInstrument2.TradableInstrument.InstrumentIdentifier, New Tuple(Of NFOStrategyInstrument, Integer, String)(signalCheckingInstrument2, quantity, "-"))
                                                        ElseIf stk2Mul = 1 AndAlso ins2Turnover > ins1Turnover Then
                                                            Dim beta = (ins2Turnover / (ins1Turnover + ins2Turnover)) - (ins1Turnover / (ins1Turnover + ins2Turnover))
                                                            If beta * 100 > 5 Then
                                                                tradeToTake = Nothing
                                                                message = String.Format("{0} -> Trade Neglect: Unable to cash neutral({6}). Entry Signal, Signal Time:{1}, Z-Score:{2}, Y-Stock:{3}, Corel:{4}, Intercept%:{5}",
                                                                                        Me.PairName,
                                                                                        ins1Candle.PreviousPayload.SnapshotDateTime.ToString("HH:mm:ss"),
                                                                                        rgsn.Item4,
                                                                                        rgsn.Item5,
                                                                                        rgsn.Item6,
                                                                                        rgsn.Item7,
                                                                                        Math.Round(beta * 100, 2))
                                                            Else
                                                                message = String.Format("{0} -> Trade Taken: Cash neutral({6}). Entry Signal, Signal Time:{1}, Z-Score:{2}, Y-Stock:{3}, Corel:{4}, Intercept%:{5}",
                                                                                        Me.PairName,
                                                                                        ins1Candle.PreviousPayload.SnapshotDateTime.ToString("HH:mm:ss"),
                                                                                        rgsn.Item4,
                                                                                        rgsn.Item5,
                                                                                        rgsn.Item6,
                                                                                        rgsn.Item7,
                                                                                        Math.Round(beta * 100, 2))
                                                            End If
                                                        ElseIf stk1Mul = 1 AndAlso ins1Turnover < ins2Turnover Then
                                                            Dim quantity As Integer = Math.Ceiling((ins2Turnover - ins1Turnover) / ins1Candle.ClosePrice.Value)
                                                            tradeToTake.Add(signalCheckingInstrument1.TradableInstrument.InstrumentIdentifier, New Tuple(Of NFOStrategyInstrument, Integer, String)(signalCheckingInstrument1, quantity, "-"))
                                                        ElseIf stk1Mul = 1 AndAlso ins1Turnover > ins2Turnover Then
                                                            Dim beta = (ins1Turnover / (ins1Turnover + ins2Turnover)) - (ins2Turnover / (ins1Turnover + ins2Turnover))
                                                            If beta * 100 > 5 Then
                                                                tradeToTake = Nothing
                                                                message = String.Format("{0} -> Trade Neglect: Unable to cash neutral({6}). Entry Signal, Signal Time:{1}, Z-Score:{2}, Y-Stock:{3}, Corel:{4}, Intercept%:{5}",
                                                                                        Me.PairName,
                                                                                        ins1Candle.PreviousPayload.SnapshotDateTime.ToString("HH:mm:ss"),
                                                                                        rgsn.Item4,
                                                                                        rgsn.Item5,
                                                                                        rgsn.Item6,
                                                                                        rgsn.Item7,
                                                                                        Math.Round(beta * 100, 2))
                                                            Else
                                                                message = String.Format("{0} -> Trade Taken: Cash neutral({6}). Entry Signal, Signal Time:{1}, Z-Score:{2}, Y-Stock:{3}, Corel:{4}, Intercept%:{5}",
                                                                                        Me.PairName,
                                                                                        ins1Candle.PreviousPayload.SnapshotDateTime.ToString("HH:mm:ss"),
                                                                                        rgsn.Item4,
                                                                                        rgsn.Item5,
                                                                                        rgsn.Item6,
                                                                                        rgsn.Item7,
                                                                                        Math.Round(beta * 100, 2))
                                                            End If
                                                        End If
                                                    End If
                                                    If tradeToTake IsNot Nothing AndAlso tradeToTake.Count > 0 Then
                                                        Try
                                                            While 1 = Interlocked.Exchange(_ParentStrategy.ActiveTradeCountLock, 1)
                                                                Await Task.Delay(10, _cts.Token).ConfigureAwait(False)
                                                            End While
                                                            Await Task.Delay(1, _cts.Token).ConfigureAwait(False)

                                                            If _ParentStrategy.ActiveTradeCount < userSettings.OverallTradeCount Then
                                                                _ParentStrategy.ActiveTradeCount += 1

                                                                SendTradeAlertMessageAsync(message)

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
                                                                    signal.TradedDate = ins1Candle.SnapshotDateTime
                                                                    signal.ZScore = rgsn.Item4
                                                                    signal.YStock = rgsn.Item5
                                                                    signal.SignalType = tradedData.FirstOrDefault.Item3
                                                                    signal.InstrumentsData = New Dictionary(Of String, Tuple(Of Integer, Decimal))
                                                                    For Each runningSignal In tradedData
                                                                        signal.InstrumentsData.Add(runningSignal.Item1.TradableInstrument.TradingSymbol, New Tuple(Of Integer, Decimal)(runningSignal.Item2, 0))
                                                                    Next

                                                                    Utilities.Strings.SerializeFromCollection(Of SignalDetails)(_filename, signal)
                                                                End If
                                                            Else
                                                                message = String.Format("{0} -> Trade Neglect: Maximum trade count({6}) exceed. Entry Signal, Signal Time:{1}, Z-Score:{2}, Y-Stock:{3}, Corel:{4}, Intercept%:{5}",
                                                                                        Me.PairName,
                                                                                        ins1Candle.PreviousPayload.SnapshotDateTime.ToString("HH:mm:ss"),
                                                                                        rgsn.Item4,
                                                                                        rgsn.Item5,
                                                                                        rgsn.Item6,
                                                                                        rgsn.Item7,
                                                                                        userSettings.OverallTradeCount)
                                                            End If
                                                        Catch ex As Exception
                                                            logger.Error(ex.ToString)
                                                        Finally
                                                            Interlocked.Exchange(_ParentStrategy.ActiveTradeCountLock, 0)
                                                        End Try
                                                    End If
                                                Else
                                                    message = String.Format("{0} -> Trade Neglect: Any of the pair instruments is active. Entry Signal, Signal Time:{1}, Z-Score:{2}, Y-Stock:{3}, Corel:{4}, Intercept%:{5}",
                                                                            Me.PairName,
                                                                            ins1Candle.PreviousPayload.SnapshotDateTime.ToString("HH:mm:ss"),
                                                                            rgsn.Item4,
                                                                            rgsn.Item5,
                                                                            rgsn.Item6,
                                                                            rgsn.Item7)
                                                End If
                                            Else
                                                If log Then logger.Info(String.Format("{0} -> Trade Neglect: Not fresh signal. Entry Signal, Signal Time:{1}, Z-Score:{2}, Y-Stock:{3}, Corel:{4}, Intercept%:{5}",
                                                                            Me.PairName,
                                                                            ins1Candle.PreviousPayload.SnapshotDateTime.ToString("HH:mm:ss"),
                                                                            rgsn.Item4,
                                                                            rgsn.Item5,
                                                                            rgsn.Item6,
                                                                            rgsn.Item7))
                                            End If
                                        Else
                                            If rgsn.Item5 Is Nothing OrElse rgsn.Item5.Trim = "" Then
                                                If log Then logger.Info(String.Format("{0} -> Trade Neglect: Price of any pair <100 or >5000. Entry Signal, Signal Time:{1}, Z-Score:{2}, Y-Stock:{3}, Corel:{4}, Intercept%:{5}",
                                                                        Me.PairName,
                                                                        ins1Candle.PreviousPayload.SnapshotDateTime.ToString("HH:mm:ss"),
                                                                        rgsn.Item4,
                                                                        rgsn.Item5,
                                                                        rgsn.Item6,
                                                                        rgsn.Item7))
                                            Else
                                                If log Then logger.Info(String.Format("{0} -> Trade Neglect: SD/Corel/Intercept%OnY mismatch. Entry Signal, Signal Time:{1}, Z-Score:{2}, Y-Stock:{3}, Corel:{4}, Intercept%:{5}",
                                                                        Me.PairName,
                                                                        ins1Candle.PreviousPayload.SnapshotDateTime.ToString("HH:mm:ss"),
                                                                        rgsn.Item4,
                                                                        rgsn.Item5,
                                                                        rgsn.Item6,
                                                                        rgsn.Item7))
                                            End If
                                        End If
                                    End If
                                End If
                                If log AndAlso message IsNot Nothing Then
                                    OnHeartbeat(message)
                                End If
                            End If
                        Else
                            Await signalCheckingInstrument1.ProcessHistoricalAsync(Now.AddDays(Me._ParentStrategy.MaxNumberOfDaysForHistoricalFetch * -1), Now).ConfigureAwait(False)
                            Await signalCheckingInstrument2.ProcessHistoricalAsync(Now.AddDays(Me._ParentStrategy.MaxNumberOfDaysForHistoricalFetch * -1), Now).ConfigureAwait(False)
                            rgsn = CalculateRegression(signalCheckingInstrument1, signalCheckingInstrument2)
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

    Private Function CalculateRegression(ByVal ins1 As NFOStrategyInstrument, ByVal ins2 As NFOStrategyInstrument) As Tuple(Of Boolean, Boolean, Date, Decimal, String, Decimal, Decimal)
        Dim ret As Tuple(Of Boolean, Boolean, Date, Decimal, String, Decimal, Decimal) = Nothing
        If ins1.TradableInstrument.IsHistoricalCompleted AndAlso ins2.TradableInstrument.IsHistoricalCompleted Then
            ins1.TradableInstrument.FetchHistorical = False
            ins2.TradableInstrument.FetchHistorical = False
            Dim userInput As NFOUserInputs = _ParentStrategy.UserSettings
            Dim pairDetails As NFOUserInputs.SectorDetails = Nothing
            For Each runningSector In userInput.SectorData
                If runningSector.Value.InstrumentsData IsNot Nothing Then
                    If runningSector.Value.InstrumentsData.ContainsKey(Me.PairName) Then
                        pairDetails = runningSector.Value
                        Exit For
                    End If
                End If
            Next
            Dim ins1Payload As Concurrent.ConcurrentDictionary(Of Date, IPayload) = ins1.GetXMinutePayload(_ParentStrategy.UserSettings.SignalTimeFrame)
            Dim ins2Payload As Concurrent.ConcurrentDictionary(Of Date, IPayload) = ins2.GetXMinutePayload(_ParentStrategy.UserSettings.SignalTimeFrame)
            If ins1Payload IsNot Nothing AndAlso ins2Payload IsNot Nothing AndAlso ins1Payload.Count = ins2Payload.Count AndAlso ins2Payload.Count > userInput.LoopBackPeriod Then
                Dim ins1CurrentXMinPayload As OHLCPayload = ins1.GetXMinuteCurrentCandle(userInput.SignalTimeFrame)
                Dim ins2CurrentXMinPayload As OHLCPayload = ins2.GetXMinuteCurrentCandle(userInput.SignalTimeFrame)
                If ins1CurrentXMinPayload IsNot Nothing AndAlso ins2CurrentXMinPayload IsNot Nothing AndAlso
                    ins1CurrentXMinPayload.SnapshotDateTime = ins2CurrentXMinPayload.SnapshotDateTime Then
                    Dim tempnSubPayloadsStk1 As List(Of KeyValuePair(Of Date, OHLCPayload)) = Nothing
                    Dim tempnSubPayloadsStk2 As List(Of KeyValuePair(Of Date, OHLCPayload)) = Nothing

                    Dim ctr1 As Integer = 0
                    For Each runningPayload In ins1Payload.OrderByDescending(Function(x)
                                                                                 Return x.Key
                                                                             End Function)
                        If runningPayload.Key < ins1CurrentXMinPayload.SnapshotDateTime Then
                            If tempnSubPayloadsStk1 Is Nothing Then tempnSubPayloadsStk1 = New List(Of KeyValuePair(Of Date, OHLCPayload))
                            tempnSubPayloadsStk1.Add(New KeyValuePair(Of Date, OHLCPayload)(runningPayload.Key, runningPayload.Value))
                            ctr1 += 1
                            If ctr1 >= userInput.LoopBackPeriod Then Exit For
                        End If
                    Next

                    Dim ctr2 As Integer = 0
                    For Each runningPayload In ins2Payload.OrderByDescending(Function(x)
                                                                                 Return x.Key
                                                                             End Function)
                        If runningPayload.Key < ins2CurrentXMinPayload.SnapshotDateTime Then
                            If tempnSubPayloadsStk2 Is Nothing Then tempnSubPayloadsStk2 = New List(Of KeyValuePair(Of Date, OHLCPayload))
                            tempnSubPayloadsStk2.Add(New KeyValuePair(Of Date, OHLCPayload)(runningPayload.Key, runningPayload.Value))
                            ctr2 += 1
                            If ctr2 >= userInput.LoopBackPeriod Then Exit For
                        End If
                    Next

                    If tempnSubPayloadsStk1 IsNot Nothing AndAlso tempnSubPayloadsStk1.Count = userInput.LoopBackPeriod AndAlso
                        tempnSubPayloadsStk2 IsNot Nothing AndAlso tempnSubPayloadsStk2.Count = userInput.LoopBackPeriod AndAlso
                        tempnSubPayloadsStk1.FirstOrDefault.Key = tempnSubPayloadsStk2.FirstOrDefault.Key AndAlso
                        tempnSubPayloadsStk1.LastOrDefault.Key = tempnSubPayloadsStk2.LastOrDefault.Key Then
                        Dim nSubPayloadsStk1 As List(Of KeyValuePair(Of Date, OHLCPayload)) = tempnSubPayloadsStk1.OrderBy(Function(x) x.Key).ToList
                        Dim nSubPayloadsStk2 As List(Of KeyValuePair(Of Date, OHLCPayload)) = tempnSubPayloadsStk2.OrderBy(Function(x) x.Key).ToList

                        Dim forwardRgsn As Regression = LinearRegression(nSubPayloadsStk1, nSubPayloadsStk2)
                        Dim reverseRgsn As Regression = LinearRegression(nSubPayloadsStk2, nSubPayloadsStk1)

                        Dim xRawValues As List(Of KeyValuePair(Of Date, OHLCPayload)) = Nothing
                        Dim yRawValues As List(Of KeyValuePair(Of Date, OHLCPayload)) = Nothing
                        Dim rgrsn As Regression = Nothing
                        If forwardRgsn.ErrorRatio < reverseRgsn.ErrorRatio Then
                            xRawValues = nSubPayloadsStk1
                            yRawValues = nSubPayloadsStk2
                            rgrsn = forwardRgsn
                        Else
                            xRawValues = nSubPayloadsStk2
                            yRawValues = nSubPayloadsStk1
                            rgrsn = reverseRgsn
                        End If
                        If xRawValues IsNot Nothing AndAlso yRawValues IsNot Nothing AndAlso rgrsn IsNot Nothing Then
                            If xRawValues.LastOrDefault.Value.ClosePrice.Value >= 100 AndAlso xRawValues.LastOrDefault.Value.ClosePrice.Value <= 5000 AndAlso
                                yRawValues.LastOrDefault.Value.ClosePrice.Value >= 100 AndAlso yRawValues.LastOrDefault.Value.ClosePrice.Value <= 5000 Then
                                Dim predictedY As Double = xRawValues.LastOrDefault.Value.ClosePrice.Value * rgrsn.Slope + rgrsn.Intercept
                                Dim originalY As Double = yRawValues.LastOrDefault.Value.ClosePrice.Value
                                Dim residual As Double = originalY - predictedY
                                Dim stdErrorOrZScore As Double = residual / rgrsn.StandardErrorOfResiduals
                                Dim interceptOnPriceOfY As Double = (rgrsn.Intercept / originalY) * 100
                                Dim pvalue As Double = 0

                                Dim take As Boolean = Math.Round(rgrsn.Correl * 100, 4) >= userInput.Correlation AndAlso Math.Abs(Math.Round(stdErrorOrZScore, 4)) >= pairDetails.EntrySD AndAlso Math.Round(interceptOnPriceOfY, 4) < userInput.InterpectPercentage

                                ret = New Tuple(Of Boolean, Boolean, Date, Decimal, String, Decimal, Decimal)(True, take, ins1CurrentXMinPayload.SnapshotDateTime, Math.Round(stdErrorOrZScore, 4), yRawValues.LastOrDefault.Value.TradingSymbol, Math.Round(rgrsn.Correl * 100, 4), Math.Round(interceptOnPriceOfY, 4))
                            Else
                                ret = New Tuple(Of Boolean, Boolean, Date, Decimal, String, Decimal, Decimal)(True, False, ins1CurrentXMinPayload.SnapshotDateTime, 0, "", 0, 0)
                            End If
                        End If
                    End If
                End If
            End If
        End If
        Return ret
    End Function

#Region "Regression"
    <Serializable>
    Private Class Regression
        Public Property Correl As Double
        Public Property RSquared As Double
        Public Property StandardErrorOfResiduals As Double
        Public Property Intercept As Double
        Public Property Slope As Double
        Public Property InterceptError As Double
        Public ReadOnly Property ErrorRatio As Double
            Get
                If Me.StandardErrorOfResiduals <> 0 Then
                    Return Me.InterceptError / Me.StandardErrorOfResiduals
                Else
                    Return Double.MinValue
                End If
            End Get
        End Property
    End Class

    Private Function LinearRegression(ByVal xRawVals As List(Of KeyValuePair(Of Date, OHLCPayload)),
                                     ByVal yRawVals As List(Of KeyValuePair(Of Date, OHLCPayload))) As Regression
        Dim ret As Regression = Nothing
        If xRawVals IsNot Nothing AndAlso yRawVals IsNot Nothing AndAlso xRawVals.Count = yRawVals.Count Then
            Dim xValues As Dictionary(Of Date, Double) = Nothing
            For Each runningPayload In xRawVals
                If xValues Is Nothing Then xValues = New Dictionary(Of Date, Double)
                xValues.Add(runningPayload.Key, runningPayload.Value.ClosePrice.Value)
            Next

            Dim yValues As Dictionary(Of Date, Double) = Nothing
            For Each runningPayload In yRawVals
                If yValues Is Nothing Then yValues = New Dictionary(Of Date, Double)
                yValues.Add(runningPayload.Key, runningPayload.Value.ClosePrice.Value)
            Next

            Dim xVals As Double() = xValues.Values.ToArray
            Dim yVals As Double() = yValues.Values.ToArray
            Dim sumOfX As Double = 0
            Dim sumOfY As Double = 0
            Dim sumOfXSq As Double = 0
            Dim sumOfYSq As Double = 0
            Dim sumCodeviates As Double = 0

            For i = 0 To xVals.Length - 1
                Dim x = xVals(i)
                Dim y = yVals(i)
                sumCodeviates += x * y
                sumOfX += x
                sumOfY += y
                sumOfXSq += x * x
                sumOfYSq += y * y
            Next

            ret = New Regression
            Dim count = xVals.Length
            Dim ssX = sumOfXSq - ((sumOfX * sumOfX) / count)
            Dim ssY = sumOfYSq - ((sumOfY * sumOfY) / count)
            Dim rNumerator = (count * sumCodeviates) - (sumOfX * sumOfY)
            Dim rDenom = (count * sumOfXSq - (sumOfX * sumOfX)) * (count * sumOfYSq - (sumOfY * sumOfY))
            Dim sCo = sumCodeviates - ((sumOfX * sumOfY) / count)
            Dim meanX = sumOfX / count
            Dim meanY = sumOfY / count
            Dim dblR = rNumerator / Math.Sqrt(rDenom)
            ret.Correl = dblR
            ret.RSquared = dblR * dblR
            ret.Intercept = meanY - ((sCo / ssX) * meanX)
            ret.Slope = sCo / ssX
            Dim standardDeviationOfY As Double = CalculateStandardDeviationS(yValues)
            Dim adjustedRSquare As Double = 1 - ((count - 1) / (count - 2)) * (1 - ret.RSquared)
            ret.StandardErrorOfResiduals = Math.Sqrt(1 - adjustedRSquare) * standardDeviationOfY

            Dim meanOfX As Double = xValues.Values.Average()
            Dim varpOfX As Double = CalculateVARP(xValues)
            If varpOfX <> 0 Then
                ret.InterceptError = (ret.StandardErrorOfResiduals / Math.Sqrt(count)) * Math.Sqrt(1 + meanOfX ^ 2 / varpOfX)
            Else
                ret.InterceptError = 0
            End If
        Else
            Throw New Exception("Input values should be with the same length.")
        End If
        Return ret
    End Function

    Private Function CalculateStandardDeviationS(ByVal inputPayload As Dictionary(Of Date, Double)) As Double
        Dim ret As Double = Nothing
        If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
            Dim mean As Double = inputPayload.Values.Average()
            Dim sumVariance As Double = 0
            For Each runningPayload In inputPayload.Keys
                sumVariance = sumVariance + Math.Pow((inputPayload(runningPayload) - mean), 2)
            Next
            Dim sampleVariance As Double = sumVariance / (inputPayload.Count - 1)
            Dim standardDeviation As Double = Math.Sqrt(sampleVariance)
            ret = standardDeviation
        End If
        Return ret
    End Function

    Private Function CalculateVARP(ByVal inputPayload As Dictionary(Of Date, Double)) As Double
        Dim ret As Double = Nothing
        If inputPayload IsNot Nothing AndAlso inputPayload.Count > 0 Then
            Dim mean As Double = inputPayload.Values.Average()
            Dim var As Dictionary(Of Date, Double) = Nothing
            For Each runningPayload In inputPayload.Keys
                If var Is Nothing Then var = New Dictionary(Of Date, Double)
                var.Add(runningPayload, Math.Pow(inputPayload(runningPayload) - mean, 2))
            Next
            ret = var.Values.Average
        End If
        Return ret
    End Function
#End Region

#Region "Telegram"
    Private Async Function SendTradeAlertMessageAsync(ByVal message As String) As Task
        Try
            Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
            _cts.Token.ThrowIfCancellationRequested()
            'message = String.Format("{0} -> {1}", Me.PairName, message)
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
#End Region

End Class