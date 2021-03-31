Imports System.Threading
Imports System.Windows.Forms.DataVisualization.Charting

Public Class frmPortfolio

#Region "Common Delegates"
    Delegate Sub SetObjectEnableDisable_Delegate(ByVal [obj] As Object, ByVal [value] As Boolean)
    Public Sub SetObjectEnableDisable_ThreadSafe(ByVal [obj] As Object, ByVal [value] As Boolean)
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [obj].InvokeRequired Then
            Dim MyDelegate As New SetObjectEnableDisable_Delegate(AddressOf SetObjectEnableDisable_ThreadSafe)
            Me.Invoke(MyDelegate, New Object() {[obj], [value]})
        Else
            [obj].Enabled = [value]
        End If
    End Sub

    Delegate Sub SetChartImageToStream_Delegate(ByVal [chart] As Chart, ByRef [stream] As IO.Stream)
    Public Sub SetChartImageToStream_ThreadSafe(ByVal [chart] As Chart, ByRef [stream] As IO.Stream)
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [chart].InvokeRequired Then
            Dim MyDelegate As New SetChartImageToStream_Delegate(AddressOf SetChartImageToStream_ThreadSafe)
            Me.Invoke(MyDelegate, New Object() {[chart], [stream]})
        Else
            [chart].SaveImage([stream], ChartImageFormat.Jpeg)
        End If
    End Sub
#End Region

    Private _cts As CancellationTokenSource
    Private _TradableStrategyInstruments As IEnumerable(Of NFOStrategyInstrument)
    Public Sub New(ByVal associatedTradableInstruments As IEnumerable(Of NFOStrategyInstrument), ByVal canceller As CancellationTokenSource)
        InitializeComponent()
        Me._TradableStrategyInstruments = associatedTradableInstruments
        _cts = canceller
    End Sub

    Private Sub frmPortfolio_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If _TradableStrategyInstruments IsNot Nothing AndAlso _TradableStrategyInstruments.Count > 0 Then
            Dim portfolioSignalDetails As List(Of NFOStrategyInstrument.SignalDetails) = Nothing
            Dim portfolioPeriodicInvestment As Dictionary(Of Date, Double) = Nothing
            Dim portfolioPeriodicInvestmentWithoutFirstEntry As Dictionary(Of Date, Double) = Nothing
            Dim totalCurrentValue As Double = 0
            Dim totalInitialInvestment As Double = 0
            For Each runningInstrument In _TradableStrategyInstruments
                Dim allSignalDetails As Dictionary(Of Date, NFOStrategyInstrument.SignalDetails) = runningInstrument.AllSignalDetails
                Dim lastSignal As NFOStrategyInstrument.SignalDetails = runningInstrument.GetLastSignalDetails(Now.Date)
                Dim initialInvestment As Double = CType(runningInstrument.ParentStrategy.UserSettings, NFOUserInputs).InitialInvestment
                Dim desireValue As Double = initialInvestment
                If lastSignal IsNot Nothing Then
                    If lastSignal.MainTradingDay Then
                        desireValue = lastSignal.DesireValue + CType(runningInstrument.ParentStrategy.UserSettings, NFOUserInputs).ExpectedIncreaseEachPeriod
                    Else
                        desireValue = lastSignal.DesireValue
                    End If
                End If
                Dim price As Decimal = runningInstrument.TradableInstrument.LastTick.LastPrice
                Dim signal As NFOStrategyInstrument.SignalDetails = New NFOStrategyInstrument.SignalDetails(runningInstrument, lastSignal, runningInstrument.TradableInstrument.TradingSymbol, Now.Date, price, price, desireValue, runningInstrument.TakeTradeToday, CType(runningInstrument.ParentStrategy.UserSettings, NFOUserInputs).InstrumentsData(runningInstrument.TradableInstrument.TradingSymbol).RunDaily)
                If allSignalDetails Is Nothing Then
                    allSignalDetails = New Dictionary(Of Date, NFOStrategyInstrument.SignalDetails)
                    allSignalDetails.Add(signal.SnapshotDate, signal)
                Else
                    If Not allSignalDetails.ContainsKey(signal.SnapshotDate) Then
                        allSignalDetails.Add(signal.SnapshotDate, signal)
                    End If
                End If
                If allSignalDetails IsNot Nothing AndAlso allSignalDetails.Count > 0 Then
                    If portfolioSignalDetails Is Nothing Then portfolioSignalDetails = New List(Of NFOStrategyInstrument.SignalDetails)
                    If portfolioPeriodicInvestment Is Nothing Then portfolioPeriodicInvestment = New Dictionary(Of Date, Double)
                    If portfolioPeriodicInvestmentWithoutFirstEntry Is Nothing Then portfolioPeriodicInvestmentWithoutFirstEntry = New Dictionary(Of Date, Double)
                    For Each runningSignal In allSignalDetails
                        portfolioSignalDetails.Add(runningSignal.Value)
                        If portfolioPeriodicInvestment.ContainsKey(runningSignal.Key) Then
                            portfolioPeriodicInvestment(runningSignal.Key) = portfolioPeriodicInvestment(runningSignal.Key) + runningSignal.Value.PeriodicInvestment
                        Else
                            portfolioPeriodicInvestment.Add(runningSignal.Key, runningSignal.Value.PeriodicInvestment)
                        End If

                        Dim periodicInvestment As Double = runningSignal.Value.PeriodicInvestment
                        If runningSignal.Value.SnapshotDate = allSignalDetails.FirstOrDefault.Value.SnapshotDate Then
                            periodicInvestment = 0
                        End If
                        If portfolioPeriodicInvestmentWithoutFirstEntry.ContainsKey(runningSignal.Key) Then
                            portfolioPeriodicInvestmentWithoutFirstEntry(runningSignal.Key) = portfolioPeriodicInvestmentWithoutFirstEntry(runningSignal.Key) + periodicInvestment
                        Else
                            portfolioPeriodicInvestmentWithoutFirstEntry.Add(runningSignal.Key, periodicInvestment)
                        End If
                    Next
                    totalCurrentValue += allSignalDetails.LastOrDefault.Value.CurrentValue
                    totalInitialInvestment += Math.Abs(allSignalDetails.FirstOrDefault.Value.PeriodicInvestment)
                End If
            Next

            If portfolioSignalDetails IsNot Nothing AndAlso portfolioSignalDetails.Count > 0 AndAlso
                portfolioPeriodicInvestment IsNot Nothing AndAlso portfolioPeriodicInvestment.Count > 0 AndAlso
                portfolioPeriodicInvestmentWithoutFirstEntry IsNot Nothing AndAlso portfolioPeriodicInvestmentWithoutFirstEntry.Count > 0 Then
                Dim continuousInvestment As Double = 0
                Dim maxOutflowNeeded As Double = 0
                For Each runningDate In portfolioPeriodicInvestmentWithoutFirstEntry
                    Me.chrtDetails.Series("Investment/Return").Points.AddXY(runningDate.Key.ToString("dd-MMM-yyyy"), Math.Round(runningDate.Value, 0))

                    If runningDate.Value <= 0 Then
                        If continuousInvestment < 0 Then
                            continuousInvestment = continuousInvestment + runningDate.Value
                        Else
                            continuousInvestment = runningDate.Value
                        End If
                    Else
                        continuousInvestment = 0
                    End If
                    maxOutflowNeeded = Math.Max(maxOutflowNeeded, Math.Abs(continuousInvestment))
                Next
                For Each dp As DataPoint In Me.chrtDetails.Series("Investment/Return").Points
                    If dp.YValues(0) > 0 Then
                        dp.Color = Color.Green
                    Else
                        dp.Color = Color.Red
                    End If
                Next

                Dim accumulatedCorpus As Double = 0
                Dim maxAccumulatedCorpus As Double = 0
                Dim netGoing As Double = 0
                Dim totalNetGoing As Double = 0
                Dim payments As List(Of Double) = New List(Of Double)
                Dim days As List(Of Double) = New List(Of Double)
                For Each runningDate In portfolioPeriodicInvestment
                    payments.Add(runningDate.Value)
                    If runningDate.Key = portfolioPeriodicInvestment.FirstOrDefault.Key Then
                        days.Add(runningDate.Key.DayOfYear)
                    Else
                        days.Add(portfolioPeriodicInvestment.FirstOrDefault.Key.DayOfYear + runningDate.Key.Subtract(portfolioPeriodicInvestment.FirstOrDefault.Key).Days)
                    End If

                    If accumulatedCorpus + runningDate.Value < 0 Then
                        If runningDate.Value < 0 Then
                            netGoing = Math.Abs(runningDate.Value + accumulatedCorpus)
                        Else
                            netGoing = 0
                        End If
                        accumulatedCorpus = 0
                    Else
                        netGoing = 0
                        accumulatedCorpus = accumulatedCorpus + runningDate.Value
                    End If
                    maxAccumulatedCorpus = Math.Max(maxAccumulatedCorpus, accumulatedCorpus)
                    totalNetGoing += netGoing
                Next
                payments.Add(totalCurrentValue)
                days.Add(portfolioPeriodicInvestment.FirstOrDefault.Key.DayOfYear + Now.Date.Subtract(portfolioPeriodicInvestment.FirstOrDefault.Key).Days)

                Dim xirr As Double = (Newtons_method(0.1, total_f_xirr(payments.ToArray, days.ToArray), total_df_xirr(payments.ToArray, days.ToArray)) * 100)
                Dim totalInvested As Double = portfolioPeriodicInvestment.Values.Sum(Function(x)
                                                                                         If x < 0 Then
                                                                                             Return Math.Abs(x)
                                                                                         Else
                                                                                             Return 0
                                                                                         End If
                                                                                     End Function)
                Dim totalReturned As Double = portfolioPeriodicInvestment.Values.Sum(Function(x)
                                                                                         If x > 0 Then
                                                                                             Return Math.Abs(x)
                                                                                         Else
                                                                                             Return 0
                                                                                         End If
                                                                                     End Function)
                Dim wealthBuild As Double = totalCurrentValue


                Dim absoluteReturn As Double = Math.Round(((accumulatedCorpus + totalCurrentValue) / totalNetGoing - 1) * 100, 2)
                Dim annualizedAbsoluteReturn As Double = (absoluteReturn / portfolioPeriodicInvestment.LastOrDefault.Key.Subtract(portfolioPeriodicInvestment.FirstOrDefault.Key).Days) * 365

                Dim a As New DataVisualization.Charting.TextAnnotation With {
                        .Alignment = ContentAlignment.TopLeft,
                        .X = 80,
                        .Y = 25,
                        .Text = String.Format("Current Return/Investment: {13}{0}{12}{0}Total Invested: {1}{0}{0}Total Returned: {2}{0}{0}Absolute Return: {3} %{0}{0}Annualized Absolute Return: {4} %{0}{0}XIRR: {5} %{0}{6}{0}Total Outflow: {9}{0}{0}Total Corpus: {10}{0}{0}Leftover Corpus: {11}{0}{0}Max Outflow Needed: {7}{0}{0}Max Corpus Accumulated: {8}",
                                              vbNewLine, Math.Round(totalInvested, 0), Math.Round(wealthBuild + totalReturned, 0), absoluteReturn.ToString("F"), annualizedAbsoluteReturn.ToString("F"), xirr.ToString("F"),
                                              "------------------------------------------",
                                              Math.Round(maxOutflowNeeded, 0), Math.Round(maxAccumulatedCorpus, 0), Math.Round(totalInvested - totalInitialInvestment, 0), Math.Round(totalReturned, 0), Math.Round(totalReturned - (totalInvested - totalInitialInvestment), 0),
                                              "------------------------------------------",
                                              Math.Round(portfolioPeriodicInvestment.LastOrDefault.Value, 0))
                    }
                Me.chrtDetails.Annotations.Add(a)
            End If
        End If
    End Sub

    Public Async Function SendGraphAsync() As Task
        frmPortfolio_Load(Nothing, Nothing)
        Await SendTelegramInfoMessageAsync().ConfigureAwait(False)
    End Function

    Private Async Function SendTelegramInfoMessageAsync() As Task
        Try
            Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
            _cts.Token.ThrowIfCancellationRequested()
            Dim userInputs As NFOUserInputs = _TradableStrategyInstruments.FirstOrDefault.ParentStrategy.UserSettings
            If userInputs.TelegramBotAPIKey IsNot Nothing AndAlso Not userInputs.TelegramBotAPIKey.Trim = "" AndAlso
                userInputs.TelegramTradeChatID IsNot Nothing AndAlso Not userInputs.TelegramTradeChatID.Trim = "" Then
                Using tSender As New Utilities.Notification.Telegram(userInputs.TelegramBotAPIKey.Trim, userInputs.TelegramTradeChatID.Trim, _cts)
                    Using stream As New System.IO.MemoryStream()
                        'Await Task.Delay(5000, _cts.Token).ConfigureAwait(False)
                        'Me.chrtDetails.SaveImage(stream, ChartImageFormat.Jpeg)
                        SetChartImageToStream_ThreadSafe(chrtDetails, stream)
                        'Await Task.Delay(5000, _cts.Token).ConfigureAwait(False)
                        stream.Position = 0

                        Await tSender.SendDocumentGetAsync(stream, String.Format("{0}-Details Chart {1}.jpeg", "Portfolio", Now.ToString("HHmmss"))).ConfigureAwait(False)
                    End Using
                End Using
            End If
        Catch ex As Exception
            'logger.Warn(ex.ToString)
        End Try
    End Function

#Region "XIRR Calculation"
    Public Const tol As Double = 0.001
    Public Delegate Function fx(ByVal x As Double) As Double

    Public Shared Function composeFunctions(ByVal f1 As fx, ByVal f2 As fx) As fx
        Return Function(ByVal x As Double) f1(x) + f2(x)
    End Function

    Public Shared Function f_xirr(ByVal p As Double, ByVal dt As Double, ByVal dt0 As Double) As fx
        Return Function(ByVal x As Double) p * Math.Pow((1.0 + x), ((dt0 - dt) / 365.0))
    End Function

    Public Shared Function df_xirr(ByVal p As Double, ByVal dt As Double, ByVal dt0 As Double) As fx
        Return Function(ByVal x As Double) (1.0 / 365.0) * (dt0 - dt) * p * Math.Pow((x + 1.0), (((dt0 - dt) / 365.0) - 1.0))
    End Function

    Public Shared Function total_f_xirr(ByVal payments As Double(), ByVal days As Double()) As fx
        Dim resf As fx = Function(ByVal x As Double) 0.0

        For i As Integer = 0 To payments.Length - 1
            resf = composeFunctions(resf, f_xirr(payments(i), days(i), days(0)))
        Next

        Return resf
    End Function

    Public Shared Function total_df_xirr(ByVal payments As Double(), ByVal days As Double()) As fx
        Dim resf As fx = Function(ByVal x As Double) 0.0

        For i As Integer = 0 To payments.Length - 1
            resf = composeFunctions(resf, df_xirr(payments(i), days(i), days(0)))
        Next

        Return resf
    End Function

    Public Shared Function Newtons_method(ByVal guess As Double, ByVal f As fx, ByVal df As fx) As Double
        Dim x0 As Double = guess
        Dim x1 As Double = 0.0
        Dim err As Double = 1.0E+100

        While err > tol
            x1 = x0 - f(x0) / df(x0)
            err = Math.Abs(x1 - x0)
            x0 = x1
        End While

        Return x0
    End Function
#End Region
End Class