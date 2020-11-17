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
            dt.Columns.Add("Instrument", GetType(NFOStrategyInstrument))
            For Each instrument In _TradableInstruments
                Dim row As DataRow = dt.NewRow
                row("Instrument Name") = instrument.TradableInstrument.TradingSymbol
                row("Exchange") = instrument.TradableInstrument.RawExchange
                row("Instrument Type") = instrument.TradableInstrument.RawInstrumentType
                row("Expiry") = instrument.TradableInstrument.Expiry
                row("Lot Size") = instrument.TradableInstrument.LotSize
                row("Tick Size") = instrument.TradableInstrument.TickSize
                row("Historical") = instrument.TradableInstrument.IsHistoricalCompleted
                row("Instrument") = instrument

                dt.Rows.Add(row)
            Next
            dgvTradableInstruments.DataSource = dt
            dgvTradableInstruments.Columns.Item("Instrument").Visible = False

            Dim updateColumn As DataGridViewButtonColumn = New DataGridViewButtonColumn
            updateColumn.HeaderText = ""
            updateColumn.Name = "update_column"
            updateColumn.Text = "Update"
            updateColumn.UseColumnTextForButtonValue = True
            Dim updateColumnIndex As Integer = 8
            If dgvTradableInstruments.Columns("update_column") Is Nothing Then
                dgvTradableInstruments.Columns.Insert(updateColumnIndex, updateColumn)
            End If

            Dim resetColumn As DataGridViewButtonColumn = New DataGridViewButtonColumn
            resetColumn.HeaderText = ""
            resetColumn.Name = "reset_column"
            resetColumn.Text = "Reset"
            resetColumn.UseColumnTextForButtonValue = True
            Dim resetColumnIndex As Integer = 9
            If dgvTradableInstruments.Columns("reset_column") Is Nothing Then
                dgvTradableInstruments.Columns.Insert(resetColumnIndex, resetColumn)
            End If

            dgvTradableInstruments.Refresh()
        End If
    End Sub

    Private Sub dgvTradableInstruments_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvTradableInstruments.CellContentClick
        If e.ColumnIndex = 8 Then
            Dim frm As Form = New frmUpdateSignal(CType(dgvTradableInstruments.Rows(e.RowIndex).Cells(7).Value, NFOStrategyInstrument))
            frm.ShowDialog()
        ElseIf e.ColumnIndex = 9 Then
            Dim strgyInstrmnt As NFOStrategyInstrument = CType(dgvTradableInstruments.Rows(e.RowIndex).Cells(7).Value, NFOStrategyInstrument)
            If strgyInstrmnt IsNot Nothing Then
                Dim lastSignal As NFOStrategyInstrument.SignalDetails = strgyInstrmnt.GetSignalDetails()
                If lastSignal IsNot Nothing Then
                    Dim userSetting As NFOUserInputs = strgyInstrmnt.ParentStrategy.UserSettings
                    strgyInstrmnt.SetSignalDetails(lastSignal.SignalTime, lastSignal.EntryPrice, String.Format("(Manual Reset) {0}", lastSignal.EntryReason), lastSignal.Quantity, userSetting.MaxMartingaleIteration)
                    MsgBox(String.Format("Reset successful for {0}", strgyInstrmnt.TradableInstrument.TradingSymbol))
                Else
                    MsgBox(String.Format("No previous signal to reset for {0}", strgyInstrmnt.TradableInstrument.TradingSymbol))
                End If
            End If
        End If
    End Sub
End Class