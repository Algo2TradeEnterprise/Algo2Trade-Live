Public Class frmNFOTradableInstrumentList

#Region "Common Delegate"
    Delegate Sub SetObjectEnableDisable_Delegate(ByVal [obj] As Object, ByVal [value] As Boolean)
    Public Sub SetObjectEnableDisable_ThreadSafe(ByVal [obj] As Object, ByVal [value] As Boolean)
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [obj].InvokeRequired Then
            Dim MyDelegate As New SetObjectEnableDisable_Delegate(AddressOf SetObjectEnableDisable_ThreadSafe)
            Me.Invoke(MyDelegate, New Object() {[obj], [value]})
        Else
            [obj].Enabled = [value]
        End If
    End Sub

    Delegate Sub SetObjectText_Delegate(ByVal [Object] As Object, ByVal [text] As String)
    Public Sub SetObjectText_ThreadSafe(ByVal [Object] As Object, ByVal [text] As String)
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [Object].InvokeRequired Then
            Dim MyDelegate As New SetObjectText_Delegate(AddressOf SetObjectText_ThreadSafe)
            Me.Invoke(MyDelegate, New Object() {[Object], [text]})
        Else
            [Object].Text = [text]
        End If
    End Sub
#End Region

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
            dt.Columns.Add("Tick Size")
            dt.Columns.Add("Historical")
            dt.Columns.Add("Instrument", GetType(NFOStrategyInstrument))
            For Each instrument In _TradableStrategyInstruments
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

            Dim detailsColumn As DataGridViewButtonColumn = New DataGridViewButtonColumn
            detailsColumn.HeaderText = ""
            detailsColumn.Name = "details_column"
            detailsColumn.Text = "Check Detials"
            detailsColumn.UseColumnTextForButtonValue = True
            Dim detailsColumnIndex As Integer = 8
            If dgvTradableInstruments.Columns("details_column") Is Nothing Then
                dgvTradableInstruments.Columns.Insert(detailsColumnIndex, detailsColumn)
            End If

            dgvTradableInstruments.Refresh()
        End If
    End Sub

    Private Sub dgvTradableInstruments_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvTradableInstruments.CellContentClick
        If e.ColumnIndex = 8 Then
            Dim frm As Form = New frmSignalDetails(CType(dgvTradableInstruments.Rows(e.RowIndex).Cells(7).Value, NFOStrategyInstrument))
            frm.ShowDialog()
        End If
    End Sub
End Class