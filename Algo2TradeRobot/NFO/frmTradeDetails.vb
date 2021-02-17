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
            dt.Columns.Add("Trading Symbol")
            dt.Columns.Add("Direction")
            dt.Columns.Add("Entry Type")
            dt.Columns.Add("Detailed Entry Type")
            dt.Columns.Add("Exit Type")
            dt.Columns.Add("Entry Time")
            dt.Columns.Add("Exit Time")
            dt.Columns.Add("Entry Price")
            dt.Columns.Add("Exit Price")
            dt.Columns.Add("Quantity")
            dt.Columns.Add("Entry Order ID")
            dt.Columns.Add("Exit Order ID")
            dt.Columns.Add("Status")
            dt.Columns.Add("Potential Target")
            dt.Columns.Add("Signal Date")
            'dt.Columns.Add("Contract Remark")
            dt.Columns.Add("Trade Number")
            dt.Columns.Add("Spot Price")
            dt.Columns.Add("Spot ATR")
            dt.Columns.Add("ATR Consumed")
            dt.Columns.Add("Child Tag")
            dt.Columns.Add("Parent Tag")
            dt.Columns.Add("Attempted Entry Price")
            dt.Columns.Add("Attempted Exit Price")

            If _strategyInstrument.SignalData.AllTrades IsNot Nothing AndAlso _strategyInstrument.SignalData.AllTrades.Count > 0 Then
                For Each runningTrade In _strategyInstrument.SignalData.AllTrades.OrderBy(Function(x)
                                                                                              Return x.EntryTime
                                                                                          End Function)
                    Dim row As DataRow = dt.NewRow
                    row("Trading Symbol") = runningTrade.TradingSymbol
                    row("Direction") = runningTrade.Direction.ToString
                    row("Entry Type") = runningTrade.TypeOfEntry.ToString
                    row("Detailed Entry Type") = runningTrade.TypeOfEntryDetails.ToString
                    row("Exit Type") = If(runningTrade.TypeOfExit <> 0, runningTrade.TypeOfExit.ToString, "")
                    row("Entry Time") = runningTrade.EntryTime.ToString("dd-MMM-yyyy HH:mm:ss")
                    row("Exit Time") = runningTrade.ExitTime.ToString("dd-MMM-yyyy HH:mm:ss")
                    row("Entry Price") = runningTrade.EntryPrice
                    row("Exit Price") = runningTrade.ExitPrice
                    row("Quantity") = runningTrade.Quantity
                    row("Entry Order ID") = runningTrade.EntryOrderID
                    row("Exit Order ID") = runningTrade.ExitOrderID
                    row("Status") = runningTrade.CurrentStatus
                    row("Potential Target") = runningTrade.PotentialTarget
                    row("Signal Date") = runningTrade.EntrySignalDate.ToString("dd-MMM-yyyy HH:mm:ss")
                    'row("Contract Remark") = runningTrade.ContractRemark
                    row("Trade Number") = runningTrade.IterationNumber
                    row("Spot Price") = runningTrade.SpotPrice
                    row("Spot ATR") = Math.Round(runningTrade.SpotATR, 2)
                    row("ATR Consumed") = String.Format("{0}%", Math.Round(runningTrade.ATRConsumed, 2))
                    row("Child Tag") = runningTrade.ChildTag
                    row("Parent Tag") = runningTrade.ParentTag
                    row("Attempted Entry Price") = runningTrade.AttemptedEntryPrice
                    row("Attempted Exit Price") = runningTrade.AttemptedExitPrice

                    dt.Rows.Add(row)
                Next
            End If

            dgvSignalDetails.DataSource = dt
            dgvSignalDetails.Refresh()
        End If
    End Sub
End Class