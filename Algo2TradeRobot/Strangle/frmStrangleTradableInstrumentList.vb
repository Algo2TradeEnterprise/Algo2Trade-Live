Public Class frmStrangleTradableInstrumentList

    Private _TradableStrategyInstruments As IEnumerable(Of StrangleStrategyInstrument)
    Public Sub New(ByVal associatedTradableInstruments As IEnumerable(Of StrangleStrategyInstrument))
        InitializeComponent()
        Me._TradableStrategyInstruments = associatedTradableInstruments
    End Sub

    Private Sub frmStrangleTradableInstrumentList_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If _TradableStrategyInstruments IsNot Nothing AndAlso _TradableStrategyInstruments.Count > 0 Then
            Dim dt As New DataTable
            dt.Columns.Add("Instrument Name")
            dt.Columns.Add("Exchange")
            dt.Columns.Add("Instrument Type")
            dt.Columns.Add("Expiry")
            dt.Columns.Add("Lot Size")
            dt.Columns.Add("Running")
            dt.Columns.Add("Historical")
            For Each instrument In _TradableStrategyInstruments
                If instrument.StrategyInstrumentRunning Then
                    Dim row As DataRow = dt.NewRow
                    row("Instrument Name") = instrument.TradableInstrument.TradingSymbol
                    row("Exchange") = instrument.TradableInstrument.RawExchange
                    row("Instrument Type") = instrument.TradableInstrument.RawInstrumentType
                    row("Expiry") = instrument.TradableInstrument.Expiry
                    row("Lot Size") = instrument.TradableInstrument.LotSize
                    row("Running") = instrument.StrategyInstrumentRunning
                    row("Historical") = instrument.TradableInstrument.IsHistoricalCompleted
                    dt.Rows.Add(row)
                End If
            Next
            dgvTradableInstruments.DataSource = dt
            dgvTradableInstruments.Refresh()
        End If
    End Sub
End Class