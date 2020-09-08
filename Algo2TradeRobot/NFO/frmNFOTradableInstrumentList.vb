Public Class frmNFOTradableInstrumentList

    Private _TradableInstruments As IEnumerable(Of NFOStrategyInstrument)
    Public Sub New(ByVal associatedTradableInstruments As IEnumerable(Of NFOStrategyInstrument))
        InitializeComponent()
        Me._TradableInstruments = associatedTradableInstruments
    End Sub

    Private Sub frmNFOTradableInstrumentList_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim dt As New DataTable
        dt.Columns.Add("Instrument Name")
        dt.Columns.Add("Exchange")
        dt.Columns.Add("Instrument Type")
        dt.Columns.Add("Expiry")
        dt.Columns.Add("Lot Size")
        dt.Columns.Add("Tick Size")
        dt.Columns.Add("Historical")
        dt.Columns.Add("Pre Processing")
        dt.Columns.Add("Running")

        If _TradableInstruments IsNot Nothing AndAlso _TradableInstruments.Count > 0 Then
            For Each instrument In _TradableInstruments
                Dim row As DataRow = dt.NewRow
                row("Instrument Name") = instrument.TradableInstrument.TradingSymbol
                row("Exchange") = instrument.TradableInstrument.RawExchange
                row("Instrument Type") = instrument.TradableInstrument.RawInstrumentType
                row("Expiry") = instrument.TradableInstrument.Expiry
                row("Lot Size") = instrument.TradableInstrument.LotSize
                row("Tick Size") = instrument.TradableInstrument.TickSize
                row("Historical") = instrument.TradableInstrument.IsHistoricalCompleted
                row("Pre Processing") = instrument.PreProcessingDone
                row("Running") = instrument.StrategyInstrumentRunning
                dt.Rows.Add(row)
            Next
        End If

        dgvTradableInstruments.DataSource = dt
        dgvTradableInstruments.Refresh()
    End Sub
End Class