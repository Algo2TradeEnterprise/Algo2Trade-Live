Imports System.IO
Imports Utilities.DAL
Imports System.Threading
Imports System.Windows.Forms.DataVisualization.Charting

Public Class frmSignalDetails
    Private _telegramCtr As Long = 0
    Private _cts As CancellationTokenSource
    Private ReadOnly _strategyInstrument As NFOStrategyInstrument
    Public Sub New(ByVal runningInstrument As NFOStrategyInstrument, ByVal canceller As CancellationTokenSource)
        InitializeComponent()
        _strategyInstrument = runningInstrument
        AddHandler _strategyInstrument.EndOfTheDay, AddressOf OnEndOfDay
        _cts = canceller
    End Sub

    Private Sub frmSignalDetails_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadData()
    End Sub

    Private Sub btnExport_Click(sender As Object, e As EventArgs) Handles btnExport.Click
        If dgvSignalDetails IsNot Nothing AndAlso dgvSignalDetails.Rows.Count > 0 Then
            saveFile.AddExtension = True
            saveFile.FileName = String.Format("{0}-Signal Details {1}.csv",
                                              _strategyInstrument.TradableInstrument.TradingSymbol,
                                              Now.ToString("HHmmss"))
            saveFile.Filter = "CSV (*.csv)|*.csv"
            saveFile.ShowDialog()
        Else
            MessageBox.Show("Empty DataGrid. Nothing to export.", "Future Stock CSV File", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If
    End Sub

    Private Sub saveFile_FileOk(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles saveFile.FileOk
        Using export As New CSVHelper(saveFile.FileName, ",", _cts)
            export.GetCSVFromDataGrid(dgvSignalDetails)
        End Using
        Dim exportFolderName As String = Path.GetDirectoryName(saveFile.FileName)
        Me.chrtDetails.SaveImage(Path.Combine(exportFolderName, String.Format("{0}-Details Chart {1}.png", _strategyInstrument.TradableInstrument.TradingSymbol, Now.ToString("HHmmss"))), ChartImageFormat.Png)
        'Me.chrtInvestmentReturn.SaveImage(Path.Combine(exportFolderName, String.Format("{0}-Investment Return Chart {1}.png", _strategyInstrument.TradableInstrument.TradingSymbol, Now.ToString("HHmmss"))), ChartImageFormat.Png)

        MessageBox.Show("Export successful .....", "Signal Details", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub LoadData()
        Try
            If _strategyInstrument IsNot Nothing Then
                Me.Text = String.Format("Signal Details - {0}", _strategyInstrument.TradableInstrument.TradingSymbol.ToUpper)

                Dim allSignalDetails As Dictionary(Of Date, NFOStrategyInstrument.SignalDetails) = _strategyInstrument.AllSignalDetails
                Dim lastSignal As NFOStrategyInstrument.SignalDetails = _strategyInstrument.GetLastSignalDetails(Now.Date)
                Dim desireValue As Double = CType(_strategyInstrument.ParentStrategy.UserSettings, NFOUserInputs).InitialInvestment
                If lastSignal IsNot Nothing Then
                    desireValue = lastSignal.DesireValue + CType(_strategyInstrument.ParentStrategy.UserSettings, NFOUserInputs).ExpectedIncreaseEachPeriod
                End If
                Dim price As Decimal = _strategyInstrument.TradableInstrument.LastTick.LastPrice
                Dim signal As NFOStrategyInstrument.SignalDetails = New NFOStrategyInstrument.SignalDetails(_strategyInstrument, lastSignal, _strategyInstrument.TradableInstrument.TradingSymbol, Now.Date, price, price, desireValue)
                If allSignalDetails Is Nothing Then
                    allSignalDetails = New Dictionary(Of Date, NFOStrategyInstrument.SignalDetails)
                    allSignalDetails.Add(signal.SnapshotDate, signal)
                Else
                    If Not allSignalDetails.ContainsKey(signal.SnapshotDate) Then
                        allSignalDetails.Add(signal.SnapshotDate, signal)
                    End If
                End If
                If allSignalDetails IsNot Nothing AndAlso allSignalDetails.Count > 0 Then
                    Me.chrtDetails.ChartAreas(0).AxisX.IsStartedFromZero = False
                    Me.chrtDetails.ChartAreas(0).AxisY.IsStartedFromZero = False

                    Dim dt As New DataTable
                    dt.Columns.Add("Snapshot Date")
                    dt.Columns.Add("Close Price")
                    dt.Columns.Add("Entry Price")
                    dt.Columns.Add("Desire Value")
                    dt.Columns.Add("No. Of Shares Owned Before Rebalancing")
                    dt.Columns.Add("Total Value Before Rebalancing")
                    dt.Columns.Add("Amount To Invest")
                    dt.Columns.Add("No. Of Shares To Buy")
                    dt.Columns.Add("Shares Owned After Rebalancing")
                    dt.Columns.Add("Total Invested")
                    dt.Columns.Add("Periodic Investment")

                    Dim payments As List(Of Double) = New List(Of Double)
                    Dim days As List(Of Double) = New List(Of Double)
                    For Each runningSignal In allSignalDetails.Values
                        Dim row As DataRow = dt.NewRow
                        row("Snapshot Date") = runningSignal.SnapshotDate.ToString("dd-MMM-yyyy")
                        row("Close Price") = runningSignal.ClosePrice
                        row("Entry Price") = runningSignal.EntryPrice
                        row("Desire Value") = runningSignal.DesireValue
                        row("No. Of Shares Owned Before Rebalancing") = runningSignal.NoOfSharesOwnedBeforeRebalancing
                        row("Total Value Before Rebalancing") = runningSignal.TotalValueBeforeRebalancing
                        row("Amount To Invest") = runningSignal.AmountToInvest
                        row("No. Of Shares To Buy") = runningSignal.NoOfSharesToBuy
                        row("Shares Owned After Rebalancing") = runningSignal.SharesOwnedAfterRebalancing
                        row("Total Invested") = runningSignal.TotalInvested
                        row("Periodic Investment") = runningSignal.PeriodicInvestment

                        dt.Rows.Add(row)

                        Me.chrtDetails.Series("Desire Value Line").Points.AddXY(runningSignal.SnapshotDate.ToString("dd-MMM-yyyy"), runningSignal.DesireValue)
                        If runningSignal.SnapshotDate = allSignalDetails.FirstOrDefault.Value.SnapshotDate Then
                            Me.chrtDetails.Series("Current Value Line").Points.AddXY(runningSignal.SnapshotDate.ToString("dd-MMM-yyyy"), runningSignal.DesireValue)
                        Else
                            Me.chrtDetails.Series("Current Value Line").Points.AddXY(runningSignal.SnapshotDate.ToString("dd-MMM-yyyy"), runningSignal.TotalValueBeforeRebalancing)
                        End If

                        If runningSignal.SnapshotDate = allSignalDetails.FirstOrDefault.Value.SnapshotDate Then
                            Me.chrtDetails.Series("Investment/Return").Points.AddXY(runningSignal.SnapshotDate.ToString("dd-MMM-yyyy"), 0)
                        Else
                            Me.chrtDetails.Series("Investment/Return").Points.AddXY(runningSignal.SnapshotDate.ToString("dd-MMM-yyyy"), Math.Round(runningSignal.PeriodicInvestment, 0))
                        End If

                        payments.Add(runningSignal.PeriodicInvestment)
                        If runningSignal.SnapshotDate = allSignalDetails.FirstOrDefault.Value.SnapshotDate Then
                            days.Add(runningSignal.SnapshotDate.DayOfYear)
                        Else
                            days.Add(allSignalDetails.FirstOrDefault.Value.SnapshotDate.DayOfYear + runningSignal.SnapshotDate.Subtract(allSignalDetails.FirstOrDefault.Value.SnapshotDate).Days)
                        End If
                    Next
                    payments.Add(allSignalDetails.LastOrDefault.Value.SharesOwnedAfterRebalancing * price)
                    days.Add(allSignalDetails.FirstOrDefault.Value.SnapshotDate.DayOfYear + Now.Date.Subtract(allSignalDetails.FirstOrDefault.Value.SnapshotDate).Days)

                    dgvSignalDetails.DataSource = dt
                    dgvSignalDetails.Refresh()

                    Dim xirr As Double = (Newtons_method(0.1, total_f_xirr(payments.ToArray, days.ToArray), total_df_xirr(payments.ToArray, days.ToArray)) * 100)
                    Dim totalInvested As Double = allSignalDetails.Values.Sum(Function(x)
                                                                                  If x.PeriodicInvestment < 0 Then
                                                                                      Return Math.Abs(x.PeriodicInvestment)
                                                                                  Else
                                                                                      Return 0
                                                                                  End If
                                                                              End Function)
                    Dim totalReturned As Double = allSignalDetails.Values.Sum(Function(x)
                                                                                  If x.PeriodicInvestment > 0 Then
                                                                                      Return Math.Abs(x.PeriodicInvestment)
                                                                                  Else
                                                                                      Return 0
                                                                                  End If
                                                                              End Function)
                    Dim wealthBuild As Double = allSignalDetails.LastOrDefault.Value.CurrentValue
                    Dim absoluteReturn As Double = allSignalDetails.Last.Value.AbsoluteReturns
                    Dim annualizedAbsoluteReturn As Double = (absoluteReturn / allSignalDetails.LastOrDefault.Value.SnapshotDate.Subtract(allSignalDetails.FirstOrDefault.Value.SnapshotDate).Days) * 365
                    Dim maxOutflowNeeded As Double = allSignalDetails.Values.Max(Function(x)
                                                                                     Return Math.Abs(x.ContinuousInvestmentNeeded)
                                                                                 End Function)
                    Dim maxAccumulatedCorpus As Double = allSignalDetails.Values.Max(Function(x)
                                                                                         Return Math.Abs(x.AccumulatedCorpus)
                                                                                     End Function)
                    Dim initialInvestment As Double = Math.Abs(allSignalDetails.FirstOrDefault.Value.PeriodicInvestment)

                    Dim a As New DataVisualization.Charting.TextAnnotation With {
                        .Alignment = ContentAlignment.TopLeft,
                        .X = 81,
                        .Y = 20,
                        .Text = String.Format("Current Investment/Return: {13}{0}{12}{0}Total Invested: {1}{0}{0}Total Returned: {2}{0}{0}Absolute Return: {3} %{0}{0}Annualized Absolute Return: {4} %{0}{0}XIRR: {5} %{0}{6}{0}Total Outflow: {9}{0}{0}Total Corpus: {10}{0}{0}Leftover Corpus: {11}{0}{0}Max Outflow Needed: {7}{0}{0}Max Corpus Accumulated: {8}",
                                              vbNewLine, Math.Round(totalInvested, 0), Math.Round(wealthBuild + totalReturned, 0), absoluteReturn.ToString("F"), annualizedAbsoluteReturn.ToString("F"), xirr.ToString("F"),
                                              "------------------------------------------",
                                              Math.Round(maxOutflowNeeded, 0), Math.Round(maxAccumulatedCorpus, 0), Math.Round(totalInvested - initialInvestment, 0), Math.Round(totalReturned, 0), Math.Round(totalReturned - (totalInvested - initialInvestment), 0),
                                              "------------------------------------------",
                                              Math.Round(allSignalDetails.LastOrDefault.Value.PeriodicInvestment, 0))
                    }
                    Me.chrtDetails.Annotations.Add(a)

                    For Each dp As DataPoint In Me.chrtDetails.Series("Investment/Return").Points
                        If dp.YValues(0) > 0 Then
                            dp.Color = Color.Green
                        Else
                            dp.Color = Color.Red
                        End If
                    Next
                End If
            End If
        Catch ex As Exception
            Console.WriteLine(ex.ToString)
        End Try
    End Sub

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

    Public Sub OnEndOfDay()
        LoadData()
        SendTelegramInfoMessageAsync()
    End Sub

    Private Async Function SendTelegramInfoMessageAsync() As Task
        Try
            Await Task.Delay(10000, _cts.Token).ConfigureAwait(False)
            _cts.Token.ThrowIfCancellationRequested()
            Dim userInputs As NFOUserInputs = _strategyInstrument.ParentStrategy.UserSettings
            If userInputs.TelegramBotAPIKey IsNot Nothing AndAlso Not userInputs.TelegramBotAPIKey.Trim = "" AndAlso
                userInputs.TelegramTradeChatID IsNot Nothing AndAlso Not userInputs.TelegramTradeChatID.Trim = "" Then
                _telegramCtr += 1
                Using tSender As New Utilities.Notification.Telegram(userInputs.TelegramBotAPIKey.Trim, userInputs.TelegramTradeChatID.Trim, _cts)
                    Using stream As New System.IO.MemoryStream()
                        Me.chrtDetails.SaveImage(stream, ChartImageFormat.Jpeg)
                        stream.Position = 0

                        Await tSender.SendDocumentGetAsync(stream, String.Format("{0}-Details Chart({1}) {2}.jpeg", _strategyInstrument.TradableInstrument.TradingSymbol, _telegramCtr, Now.ToString("HHmmss"))).ConfigureAwait(False)
                    End Using
                End Using
            End If
        Catch ex As Exception
            'logger.Warn(ex.ToString)
        End Try
    End Function
End Class