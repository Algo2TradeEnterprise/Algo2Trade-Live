Public Class frmNFOTradableInstrumentList

    Private _TradableInstruments As IEnumerable(Of NFOStrategyInstrument)
    Public Sub New(ByVal associatedTradableInstruments As IEnumerable(Of NFOStrategyInstrument))
        InitializeComponent()
        Me._TradableInstruments = associatedTradableInstruments
    End Sub

    Private Sub frmNFOTradableInstrumentList_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If _TradableInstruments IsNot Nothing AndAlso _TradableInstruments.Count > 0 Then
            Dim dt As New DataTable
            dt.Columns.Add("Instrument Name")
            dt.Columns.Add("Exchange")
            dt.Columns.Add("Instrument Type")
            dt.Columns.Add("Expiry")
            dt.Columns.Add("Lot Size")
            dt.Columns.Add("Tick Size")
            dt.Columns.Add("Historical")
            dt.Columns.Add("Multiplier")
            dt.Columns.Add("Direction")
            For Each instrument In _TradableInstruments
                Dim userSettings As NFOUserInputs = instrument.ParentStrategy.UserSettings
                Dim direction As String = "None"
                If userSettings.InstrumentsData(instrument.TradableInstrument.TradingSymbol).PreviousDayHKOpen = userSettings.InstrumentsData(instrument.TradableInstrument.TradingSymbol).PreviousDayHKLow Then
                    direction = "BUY"
                ElseIf userSettings.InstrumentsData(instrument.TradableInstrument.TradingSymbol).PreviousDayHKOpen = userSettings.InstrumentsData(instrument.TradableInstrument.TradingSymbol).PreviousDayHKHigh Then
                    direction = "SELL"
                End If

                Dim row As DataRow = dt.NewRow
                row("Instrument Name") = instrument.TradableInstrument.TradingSymbol
                row("Exchange") = instrument.TradableInstrument.RawExchange
                row("Instrument Type") = instrument.TradableInstrument.RawInstrumentType
                row("Expiry") = instrument.TradableInstrument.Expiry
                row("Lot Size") = instrument.TradableInstrument.LotSize
                row("Tick Size") = instrument.TradableInstrument.TickSize
                row("Historical") = instrument.TradableInstrument.IsHistoricalCompleted
                row("Multiplier") = userSettings.InstrumentsData(instrument.TradableInstrument.TradingSymbol).Multiplier
                row("Direction") = direction
                dt.Rows.Add(row)
            Next
            dgvTradableInstruments.DataSource = dt
            dgvTradableInstruments.Refresh()
        End If
    End Sub
End Class