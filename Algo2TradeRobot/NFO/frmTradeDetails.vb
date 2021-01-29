Public Class frmTradeDetails
    Private ReadOnly _strategyInstrument As NFOStrategyInstrument
    Public Sub New(ByVal runningInstrument As NFOStrategyInstrument)
        InitializeComponent()
        _strategyInstrument = runningInstrument
    End Sub

    Private Sub frmSignalDetails_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If _strategyInstrument IsNot Nothing Then
            Me.Text = String.Format("Trade Details - {0}", _strategyInstrument.TradableInstrument.TradingSymbol.ToUpper)

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

            'If _strategyInstrument.SignalData.AllTrades IsNot Nothing AndAlso _strategyInstrument.SignalData.AllTrades.Count > 0 Then
            '    For Each runningTrade In _strategyInstrument.SignalData.AllTrades
            '        Dim row As DataRow = dt.NewRow
            '        'row("Trading Symbol") = runningSignal.TradingSymbol
            '        row("Snapshot Date") = runningSignal.SnapshotDate.ToString("dd-MMM-yyyy")
            '        row("Close Price") = runningSignal.ClosePrice
            '        row("Entry Price") = runningSignal.EntryPrice
            '        row("Desire Value") = runningSignal.DesireValue
            '        row("No. Of Shares Owned Before Rebalancing") = runningSignal.NoOfSharesOwnedBeforeRebalancing
            '        row("Total Value Before Rebalancing") = runningSignal.TotalValueBeforeRebalancing
            '        row("Amount To Invest") = runningSignal.AmountToInvest
            '        row("No. Of Shares To Buy") = runningSignal.NoOfSharesToBuy
            '        row("Shares Owned After Rebalancing") = runningSignal.SharesOwnedAfterRebalancing
            '        row("Total Invested") = runningSignal.TotalInvested
            '        row("Periodic Investment") = runningSignal.PeriodicInvestment

            '        dt.Rows.Add(row)
            '    Next

            '    dgvSignalDetails.DataSource = dt
            '    dgvSignalDetails.Refresh()
            'End If
        End If
    End Sub
End Class