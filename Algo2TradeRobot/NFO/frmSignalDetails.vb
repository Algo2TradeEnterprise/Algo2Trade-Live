Public Class frmSignalDetails
    Private ReadOnly _strategyInstrument As NFOStrategyInstrument
    Public Sub New(ByVal runningInstrument As NFOStrategyInstrument)
        InitializeComponent()
        _strategyInstrument = runningInstrument
    End Sub

    Private Sub frmSignalDetails_Load(sender As Object, e As EventArgs) Handles MyBase.Load
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
                Dim dt As New DataTable
                'dt.Columns.Add("Trading Symbol")
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
                    'row("Trading Symbol") = runningSignal.TradingSymbol
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

                    payments.Add(runningSignal.PeriodicInvestment)
                    days.Add(runningSignal.SnapshotDate.DayOfYear)
                Next
                payments.Add(allSignalDetails.LastOrDefault.Value.SharesOwnedAfterRebalancing * price)
                days.Add(Now.DayOfYear)

                dgvSignalDetails.DataSource = dt
                dgvSignalDetails.Refresh()

                lblXIRR.Text = String.Format("XIRR: {0}", (Newtons_method(0.1, total_f_xirr(payments.ToArray, days.ToArray), total_df_xirr(payments.ToArray, days.ToArray))*100).ToString("F"))
            End If
        End If
    End Sub

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
End Class