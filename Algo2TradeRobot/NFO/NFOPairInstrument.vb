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
        Public Property InstrumentsData As Dictionary(Of String, Tuple(Of Integer, IOrder.TypeOfOrder))
    End Class
#End Region

    Private _lastPrevPayloadTime As Date = Now.Date

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
                Dim signalCheckingInstruments As List(Of NFOStrategyInstrument) = _DependentInstruments
                If signalCheckingInstruments IsNot Nothing AndAlso signalCheckingInstruments.Count = 2 Then
                    Dim signalCheckingInstrument1 As NFOStrategyInstrument = signalCheckingInstruments.FirstOrDefault
                    Dim signalCheckingInstrument2 As NFOStrategyInstrument = signalCheckingInstruments.LastOrDefault

                    Dim tradedSignal As SignalDetails = Nothing
                    'Dim rolloverTime As Date = New Date(Now.Year, Now.Month, Now.Day, 15, 28, 0)
                    Dim lastCandle As Date = Now.Date
                    logger.Fatal("Stock 1,LTP 1,Bid 1,Ask 1,Stock 2,LTP 2,Bid 2,Ask 2")
                    While True
                        If _ParentStrategy.ParentController.OrphanException IsNot Nothing Then
                            Throw _ParentStrategy.ParentController.OrphanException
                        End If
                        _cts.Token.ThrowIfCancellationRequested()

                        If signalCheckingInstrument1.TradableInstrument.IsHistoricalCompleted AndAlso signalCheckingInstrument1.TradableInstrument.LastTick IsNot Nothing AndAlso
                            signalCheckingInstrument2.TradableInstrument.IsHistoricalCompleted AndAlso signalCheckingInstrument2.TradableInstrument.LastTick IsNot Nothing Then
                            Dim candle As OHLCPayload = signalCheckingInstrument1.GetXMinuteCurrentCandle(userSettings.SignalTimeFrame)
                            If candle IsNot Nothing AndAlso candle.PreviousPayload.SnapshotDateTime <> lastCandle Then
                                lastCandle = candle.PreviousPayload.SnapshotDateTime
                                logger.Fatal("{0},{1},{2},{3},{4},{5},{6},{7}",
                                             signalCheckingInstrument1.TradableInstrument.TradingSymbol,
                                             signalCheckingInstrument1.TradableInstrument.LastTick.LastPrice,
                                             signalCheckingInstrument1.TradableInstrument.LastTick.FirstBidPrice,
                                             signalCheckingInstrument1.TradableInstrument.LastTick.FirstOfferPrice,
                                             signalCheckingInstrument2.TradableInstrument.TradingSymbol,
                                             signalCheckingInstrument2.TradableInstrument.LastTick.LastPrice,
                                             signalCheckingInstrument2.TradableInstrument.LastTick.FirstBidPrice,
                                             signalCheckingInstrument2.TradableInstrument.LastTick.FirstOfferPrice)
                            End If
                        End If

                        'If tradedSignal Is Nothing AndAlso File.Exists(_filename) Then
                        '    tradedSignal = Utilities.Strings.DeserializeToCollection(Of SignalDetails)(_filename)
                        'End If
                        'Dim meadSd As Tuple(Of Boolean, Decimal, Decimal) = CalculateMeanSD(signalCheckingInstrument1, signalCheckingInstrument2)
                        'If meadSd IsNot Nothing AndAlso meadSd.Item1 Then
                        '    Dim ins1Candle As OHLCPayload = signalCheckingInstrument1.GetXMinuteCurrentCandle(userSettings.SignalTimeFrame)
                        '    Dim ins2Candle As OHLCPayload = signalCheckingInstrument2.GetXMinuteCurrentCandle(userSettings.SignalTimeFrame)
                        '    If ins1Candle IsNot Nothing AndAlso ins2Candle IsNot Nothing AndAlso
                        '        ins1Candle.SnapshotDateTime = ins2Candle.SnapshotDateTime AndAlso
                        '        ins1Candle.PreviousPayload IsNot Nothing AndAlso ins2Candle.PreviousPayload IsNot Nothing Then
                        '        Dim ratio As Decimal = ins1Candle.PreviousPayload.ClosePrice.Value / ins2Candle.PreviousPayload.ClosePrice.Value
                        '        Dim log As Boolean = False
                        '        Dim message As String = Nothing
                        '        Try
                        '            If ins1Candle.PreviousPayload.SnapshotDateTime <> _lastPrevPayloadTime Then
                        '                _lastPrevPayloadTime = ins1Candle.PreviousPayload.SnapshotDateTime
                        '                log = True
                        '            End If
                        '        Catch ex As Exception
                        '            logger.Warn(ex)
                        '        End Try

                        '        If tradedSignal IsNot Nothing Then
                        '            Dim exitTrade As Boolean = False
                        '            If tradedSignal.SignalType = "+" Then
                        '                exitTrade = (ratio <= meadSd.Item2)
                        '                message = String.Format("{0} -> [{4}]Exit Signal, Traded Signal: SELL, Signal Time:{1}, Ratio({2})<=Mean({3})[{4}], Close1:{5}, Close2:{6}",
                        '                                        Me.PairName,
                        '                                        ins1Candle.PreviousPayload.SnapshotDateTime.ToString("HH:mm:ss"),
                        '                                        Math.Round(ratio, 6),
                        '                                        Math.Round(meadSd.Item2, 6),
                        '                                        exitTrade,
                        '                                        ins1Candle.ClosePrice.Value,
                        '                                        ins2Candle.ClosePrice.Value)
                        '            ElseIf tradedSignal.SignalType = "-" Then
                        '                exitTrade = (ratio >= meadSd.Item2)
                        '                message = String.Format("{0} -> [{4}]Exit Signal, Traded Signal: BUY, Signal Time:{1}, Ratio({2})>=Mean({3})[{4}], Close1:{5}, Close2:{6}",
                        '                                        Me.PairName,
                        '                                        ins1Candle.PreviousPayload.SnapshotDateTime.ToString("HH:mm:ss"),
                        '                                        Math.Round(ratio, 6),
                        '                                        Math.Round(meadSd.Item2, 6),
                        '                                        exitTrade,
                        '                                        ins1Candle.ClosePrice.Value,
                        '                                        ins2Candle.ClosePrice.Value)
                        '            End If

                        '            'Dim rollover As Boolean = False
                        '            'If Now >= rolloverTime Then
                        '            '    Dim ins1Future As NFOStrategyInstrument = GetFutureInstrumentToTrade(signalCheckingInstrument1.TradableInstrument.TradingSymbol)
                        '            '    Dim ins2Future As NFOStrategyInstrument = GetFutureInstrumentToTrade(signalCheckingInstrument2.TradableInstrument.TradingSymbol)
                        '            '    If Not tradedSignal.InstrumentsData.ContainsKey(ins1Future.TradableInstrument.TradingSymbol) OrElse
                        '            '        Not tradedSignal.InstrumentsData.ContainsKey(ins2Future.TradableInstrument.TradingSymbol) Then
                        '            '        exitTrade = True
                        '            '        rollover = True
                        '            '    End If
                        '            'End If
                        '            If exitTrade Then
                        '                OnHeartbeat(message)
                        '                log = False

                        '                Dim tradeToExit As New Dictionary(Of String, Tuple(Of NFOStrategyInstrument, IOrder.TypeOfOrder, Integer, Date, Decimal, String))
                        '                For Each ruuningInstrument In tradedSignal.InstrumentsData
                        '                    Dim ins As NFOStrategyInstrument =
                        '                            _DependentInstruments.Find(Function(x)
                        '                                                           Return x.TradableInstrument.TradingSymbol.ToUpper = ruuningInstrument.Key.ToUpper
                        '                                                       End Function)
                        '                    tradeToExit.Add(ins.TradableInstrument.InstrumentIdentifier, New Tuple(Of NFOStrategyInstrument, IOrder.TypeOfOrder, Integer, Date, Decimal, String)(ins, ruuningInstrument.Value.Item2, ruuningInstrument.Value.Item1 * -1, ins1Candle.PreviousPayload.SnapshotDateTime, userSettings.MaxSpreadPercentage, tradedSignal.SignalType))
                        '                Next
                        '                If tradeToExit IsNot Nothing AndAlso tradeToExit.Count > 0 Then
                        '                    Dim tradedData As New Concurrent.ConcurrentBag(Of Tuple(Of NFOStrategyInstrument, IOrder.TypeOfOrder, Integer, Date, Decimal, String))
                        '                    For Each runningInstrument In tradeToExit.Values
                        '                        If Await runningInstrument.Item1.TakeTradeAsync(runningInstrument.Item2, runningInstrument.Item3, runningInstrument.Item4, runningInstrument.Item5).ConfigureAwait(False) Then
                        '                            tradedData.Add(runningInstrument)
                        '                        Else
                        '                            Exit For
                        '                        End If
                        '                    Next
                        '                    If tradedData IsNot Nothing AndAlso tradedData.Count > 0 Then
                        '                        tradedSignal = Nothing
                        '                        If File.Exists(_filename) Then File.Delete(_filename)
                        '                    End If
                        '                End If
                        '            End If
                        '        Else
                        '            'Entry
                        '            Dim tradeToTake As Dictionary(Of String, Tuple(Of NFOStrategyInstrument, IOrder.TypeOfOrder, Integer, Date, Decimal, String)) = Nothing
                        '            message = String.Format("{0} -> Entry Signal, Signal Time:{1}, Ratio({2})>={3}[Mean({4})+{5}*SD({6})][{7}], Ratio({2})<={8}[Mean({4})-{5}*SD({6})[{9}], Close1:{10}, Close2:{11}",
                        '                                    Me.PairName,
                        '                                    ins1Candle.PreviousPayload.SnapshotDateTime.ToString("HH:mm:ss"),
                        '                                    Math.Round(ratio, 6),
                        '                                    Math.Round(meadSd.Item2 + meadSd.Item3 * userSettings.EntrySDMultiplier, 6),
                        '                                    Math.Round(meadSd.Item2, 6),
                        '                                    userSettings.EntrySDMultiplier,
                        '                                    Math.Round(meadSd.Item3, 6),
                        '                                    ratio >= meadSd.Item2 + meadSd.Item3 * userSettings.EntrySDMultiplier,
                        '                                    Math.Round(meadSd.Item2 - meadSd.Item3 * userSettings.EntrySDMultiplier, 6),
                        '                                    ratio <= meadSd.Item2 - meadSd.Item3 * userSettings.EntrySDMultiplier,
                        '                                    ins1Candle.ClosePrice.Value,
                        '                                    ins2Candle.ClosePrice.Value)

                        '            If ratio >= meadSd.Item2 + meadSd.Item3 * userSettings.EntrySDMultiplier Then
                        '                'Stock 2 BUY, Stock 1 SELL
                        '                tradeToTake = New Dictionary(Of String, Tuple(Of NFOStrategyInstrument, IOrder.TypeOfOrder, Integer, Date, Decimal, String))
                        '                Dim ins1Future As NFOStrategyInstrument = GetFutureInstrumentToTrade(signalCheckingInstrument1.TradableInstrument.RawInstrumentName)
                        '                Dim ins2Future As NFOStrategyInstrument = GetFutureInstrumentToTrade(signalCheckingInstrument2.TradableInstrument.RawInstrumentName)
                        '                tradeToTake.Add(ins2Future.TradableInstrument.InstrumentIdentifier, New Tuple(Of NFOStrategyInstrument, IOrder.TypeOfOrder, Integer, Date, Decimal, String)(ins2Future, IOrder.TypeOfOrder.Limit, ins2Future.TradableInstrument.LotSize * userSettings.InstrumentsData(Me.PairName).NumberOfLots, ins2Candle.PreviousPayload.SnapshotDateTime, userSettings.MaxSpreadPercentage, "+"))
                        '                tradeToTake.Add(ins1Future.TradableInstrument.InstrumentIdentifier, New Tuple(Of NFOStrategyInstrument, IOrder.TypeOfOrder, Integer, Date, Decimal, String)(ins1Future, IOrder.TypeOfOrder.Market, ins1Future.TradableInstrument.LotSize * userSettings.InstrumentsData(Me.PairName).NumberOfLots * -1, ins1Candle.PreviousPayload.SnapshotDateTime, userSettings.MaxSpreadPercentage, "+"))
                        '            ElseIf ratio <= meadSd.Item2 - meadSd.Item3 * userSettings.EntrySDMultiplier Then
                        '                'Stock 1 BUY, Stock 2 SELL
                        '                tradeToTake = New Dictionary(Of String, Tuple(Of NFOStrategyInstrument, IOrder.TypeOfOrder, Integer, Date, Decimal, String))
                        '                Dim ins1Future As NFOStrategyInstrument = GetFutureInstrumentToTrade(signalCheckingInstrument1.TradableInstrument.RawInstrumentName)
                        '                Dim ins2Future As NFOStrategyInstrument = GetFutureInstrumentToTrade(signalCheckingInstrument2.TradableInstrument.RawInstrumentName)
                        '                tradeToTake.Add(ins2Future.TradableInstrument.InstrumentIdentifier, New Tuple(Of NFOStrategyInstrument, IOrder.TypeOfOrder, Integer, Date, Decimal, String)(ins2Future, IOrder.TypeOfOrder.Limit, ins2Future.TradableInstrument.LotSize * userSettings.InstrumentsData(Me.PairName).NumberOfLots * -1, ins2Candle.PreviousPayload.SnapshotDateTime, userSettings.MaxSpreadPercentage, "-"))
                        '                tradeToTake.Add(ins1Future.TradableInstrument.InstrumentIdentifier, New Tuple(Of NFOStrategyInstrument, IOrder.TypeOfOrder, Integer, Date, Decimal, String)(ins1Future, IOrder.TypeOfOrder.Market, ins1Future.TradableInstrument.LotSize * userSettings.InstrumentsData(Me.PairName).NumberOfLots, ins1Candle.PreviousPayload.SnapshotDateTime, userSettings.MaxSpreadPercentage, "-"))
                        '            End If
                        '            If tradeToTake IsNot Nothing AndAlso tradeToTake.Count > 0 Then
                        '                OnHeartbeat(message)
                        '                log = False

                        '                Dim tradedData As New Concurrent.ConcurrentBag(Of Tuple(Of NFOStrategyInstrument, IOrder.TypeOfOrder, Integer, Date, Decimal, String))
                        '                For Each runningInstrument In tradeToTake.Values
                        '                    If Await runningInstrument.Item1.TakeTradeAsync(runningInstrument.Item2, runningInstrument.Item3, runningInstrument.Item4, runningInstrument.Item5).ConfigureAwait(False) Then
                        '                        tradedData.Add(runningInstrument)
                        '                    Else
                        '                        Exit For
                        '                    End If
                        '                Next
                        '                If tradedData IsNot Nothing AndAlso tradedData.Count > 0 Then
                        '                    Dim signal As SignalDetails = New SignalDetails
                        '                    signal.TradedDate = Now.Date
                        '                    signal.Mean = meadSd.Item2
                        '                    signal.SD = meadSd.Item3
                        '                    signal.SignalType = tradedData.FirstOrDefault.Item6
                        '                    signal.InstrumentsData = New Dictionary(Of String, Tuple(Of Integer, IOrder.TypeOfOrder))
                        '                    For Each runningSignal In tradedData
                        '                        signal.InstrumentsData.Add(runningSignal.Item1.TradableInstrument.TradingSymbol, New Tuple(Of Integer, IOrder.TypeOfOrder)(runningSignal.Item3, runningSignal.Item2))
                        '                    Next

                        '                    Utilities.Strings.SerializeFromCollection(Of SignalDetails)(_filename, signal)
                        '                End If
                        '            End If
                        '        End If
                        '        If log AndAlso message IsNot Nothing Then
                        '            OnHeartbeat(message)
                        '        End If
                        '    End If
                        'End If

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
            'ins1.TradableInstrument.FetchHistorical = False
            'ins2.TradableInstrument.FetchHistorical = False
            Dim ins1Payload As Concurrent.ConcurrentDictionary(Of Date, IPayload) = ins1.GetXMinutePayload(_ParentStrategy.UserSettings.SignalTimeFrame)
            Dim ins2Payload As Concurrent.ConcurrentDictionary(Of Date, IPayload) = ins2.GetXMinutePayload(_ParentStrategy.UserSettings.SignalTimeFrame)
            If ins1Payload IsNot Nothing AndAlso ins2Payload IsNot Nothing AndAlso ins1Payload.Count = ins2Payload.Count Then
                Dim ratioPayload As Dictionary(Of Date, Decimal) = Nothing
                Dim noOfDays As Integer = 0
                Dim ctr As Integer = 0
                For Each runningPayload In ins1Payload.OrderByDescending(Function(x)
                                                                             Return x.Key
                                                                         End Function)
                    If ctr >= 1 Then
                        If ins2Payload.ContainsKey(runningPayload.Key) Then
                            noOfDays += 1
                            If noOfDays > CType(_ParentStrategy.UserSettings, NFOUserInputs).LoopBackPeriod Then Exit For

                            Dim ins1Cndl As OHLCPayload = ins1Payload(runningPayload.Key)
                            Dim ins2Cndl As OHLCPayload = ins2Payload(runningPayload.Key)
                            If ratioPayload Is Nothing Then ratioPayload = New Dictionary(Of Date, Decimal)
                            ratioPayload.Add(runningPayload.Key, ins1Cndl.ClosePrice.Value / ins2Cndl.ClosePrice.Value)
                        Else
                            Throw New ApplicationException("Data mismatch")
                        End If
                    End If
                    ctr += 1
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
                sum += inputPayload(runningPayload)
            Next
            Dim mean As Double = sum / inputPayload.Count
            Dim sumVariance As Double = 0
            For Each runningPayload In inputPayload.Keys
                sumVariance += Math.Pow((inputPayload(runningPayload) - mean), 2)
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
            'Dim userInputs As NFOUserInputs = _ParentStrategy.UserSettings
            'If userInputs.TelegramBotAPIKey IsNot Nothing AndAlso Not userInputs.TelegramBotAPIKey.Trim = "" AndAlso
            '    userInputs.TelegramTradeChatID IsNot Nothing AndAlso Not userInputs.TelegramTradeChatID.Trim = "" Then
            '    Using tSender As New Utilities.Notification.Telegram(userInputs.TelegramBotAPIKey.Trim, userInputs.TelegramTradeChatID.Trim, _cts)
            '        Dim encodedString As String = Utilities.Strings.UrlEncodeString(message)
            '        Await tSender.SendMessageGetAsync(encodedString).ConfigureAwait(False)
            '    End Using
            'End If
            logger.Fatal(message)
        Catch ex As Exception
            logger.Warn(ex.ToString)
        End Try
    End Function
End Class