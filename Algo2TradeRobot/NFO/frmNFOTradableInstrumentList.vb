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
            'dt.Columns.Add("Expiry")
            'dt.Columns.Add("Lot Size")
            dt.Columns.Add("Running")
            dt.Columns.Add("Total Quantity")
            dt.Columns.Add("Average Price")
            dt.Columns.Add("Downward Drop %")
            dt.Columns.Add("Downward Rise %")
            dt.Columns.Add("Upward Rise %")
            dt.Columns.Add("Upward Drop %")
            For Each instrument In _TradableStrategyInstruments
                Dim row As DataRow = dt.NewRow
                row("Instrument Name") = instrument.TradableInstrument.TradingSymbol
                row("Exchange") = instrument.TradableInstrument.RawExchange
                row("Instrument Type") = instrument.TradableInstrument.RawInstrumentType
                'row("Expiry") = instrument.TradableInstrument.Expiry
                'row("Lot Size") = instrument.TradableInstrument.LotSize
                row("Running") = instrument.StrategyInstrumentRunning
                row("Total Quantity") = instrument.SignalData.TotalQuantity
                row("Average Price") = Math.Round(instrument.SignalData.AveragePrice, 2)
                row("Downward Drop %") = Math.Round(instrument.SignalData.DownwardDropPercentage, 2)
                row("Downward Rise %") = Math.Round(instrument.SignalData.DownwardNetRisePercentage, 2)
                row("Upward Rise %") = Math.Round(instrument.SignalData.UpwardRisePercentage, 2)
                row("Upward Drop %") = Math.Round(instrument.SignalData.UpwardNetDropPercentage, 2)
                dt.Rows.Add(row)
            Next
            dgvTradableInstruments.DataSource = dt
            dgvTradableInstruments.Refresh()
        End If
    End Sub
End Class