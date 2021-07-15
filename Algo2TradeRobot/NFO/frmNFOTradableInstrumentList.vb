Public Class frmNFOTradableInstrumentList

    Private ReadOnly _TradablePairInstruments As IEnumerable(Of NFOPairInstrument)
    Public Sub New(ByVal associatedTradableInstruments As IEnumerable(Of NFOPairInstrument))
        InitializeComponent()
        Me._TradablePairInstruments = associatedTradableInstruments
    End Sub

    Private Sub frmNFOTradableInstrumentList_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If _TradablePairInstruments IsNot Nothing AndAlso _TradablePairInstruments.Count > 0 Then
            Dim dt As New DataTable
            dt.Columns.Add("Pair Name")
            dt.Columns.Add("Running")
            dt.Columns.Add("Instrument 1")
            dt.Columns.Add("Historical 1")
            dt.Columns.Add("Instrument 2")
            dt.Columns.Add("Historical 2")
            For Each runningPair In _TradablePairInstruments
                Dim row As DataRow = dt.NewRow
                row("Pair Name") = runningPair.PairName
                row("Running") = runningPair.PairRunning
                row("Instrument 1") = runningPair.DependentInstruments.FirstOrDefault.TradableInstrument.TradingSymbol
                row("Historical 1") = runningPair.DependentInstruments.FirstOrDefault.TradableInstrument.IsHistoricalCompleted
                row("Instrument 2") = runningPair.DependentInstruments.LastOrDefault.TradableInstrument.TradingSymbol
                row("Historical 2") = runningPair.DependentInstruments.LastOrDefault.TradableInstrument.IsHistoricalCompleted

                dt.Rows.Add(row)
            Next
            dgvTradableInstruments.DataSource = dt
            dgvTradableInstruments.Refresh()
        End If
    End Sub
End Class