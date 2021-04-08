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
            dt.Columns.Add("Expiry")
            dt.Columns.Add("Lot Size")
            dt.Columns.Add("Running")
            For Each instrument In _TradableStrategyInstruments
                If instrument.StrategyInstrumentRunning Then
                    Dim row As DataRow = dt.NewRow
                    row("Instrument Name") = instrument.TradableInstrument.TradingSymbol
                    row("Exchange") = instrument.TradableInstrument.RawExchange
                    row("Instrument Type") = instrument.TradableInstrument.RawInstrumentType
                    row("Expiry") = instrument.TradableInstrument.Expiry
                    row("Lot Size") = instrument.TradableInstrument.LotSize
                    row("Running") = instrument.StrategyInstrumentRunning
                    dt.Rows.Add(row)
                End If
            Next
            dgvTradableInstruments.DataSource = dt
            dgvTradableInstruments.Refresh()
        End If
    End Sub
End Class