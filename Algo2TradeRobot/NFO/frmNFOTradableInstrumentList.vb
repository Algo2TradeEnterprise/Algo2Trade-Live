Public Class frmNFOTradableInstrumentList

    Private _TradableStrategyInstruments As IEnumerable(Of NFOStrategyInstrument)
    Public Sub New(ByVal associatedTradableInstruments As IEnumerable(Of NFOStrategyInstrument))
        InitializeComponent()
        Me._TradableStrategyInstruments = associatedTradableInstruments
    End Sub

    Private Sub frmNFOTradableInstrumentList_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If _TradableStrategyInstruments IsNot Nothing AndAlso _TradableStrategyInstruments.Count > 0 Then
            Dim dt As New DataTable
            dt.Columns.Add("Instrument Name")
            dt.Columns.Add("Exchange")
            dt.Columns.Add("Instrument Type")
            dt.Columns.Add("Tick Size")
            dt.Columns.Add("Historical")
            For Each instrument In _TradableStrategyInstruments
                Dim row As DataRow = dt.NewRow
                row("Instrument Name") = instrument.TradableInstrument.TradingSymbol
                row("Exchange") = instrument.TradableInstrument.RawExchange
                row("Instrument Type") = instrument.TradableInstrument.RawInstrumentType
                row("Tick Size") = instrument.TradableInstrument.TickSize
                row("Historical") = instrument.TradableInstrument.IsHistoricalCompleted

                dt.Rows.Add(row)
            Next
            dgvTradableInstruments.DataSource = dt
            dgvTradableInstruments.Refresh()
        End If
    End Sub
End Class