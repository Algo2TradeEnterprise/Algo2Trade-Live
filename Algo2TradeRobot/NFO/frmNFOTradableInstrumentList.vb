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
                row("Expiry") = instrument.TradableInstrument.Expiry
                row("Lot Size") = instrument.TradableInstrument.LotSize
                row("Running") = instrument.StrategyInstrumentRunning
                row("Total Quantity") = instrument.SignalData.TotalQuantity
                row("Average Price") = instrument.SignalData.AveragePrice
                row("Downward Drop %") = instrument.SignalData.DownwardDropPercentage
                row("Downward Rise %") = instrument.SignalData.DownwardNetRisePercentage
                row("Upward Rise %") = instrument.SignalData.UpwardRisePercentage
                row("Upward Drop %") = instrument.SignalData.UpwardNetDropPercentage
                dt.Rows.Add(row)
            Next
            dgvTradableInstruments.DataSource = dt
            dgvTradableInstruments.Refresh()
        End If
    End Sub
End Class