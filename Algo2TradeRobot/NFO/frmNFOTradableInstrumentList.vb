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

    Private Async Sub frmNFOTradableInstrumentList_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim dt As New DataTable
        dt.Columns.Add("Instrument", GetType(NFOStrategyInstrument))
        dt.Columns.Add("Instrument Name")
        dt.Columns.Add("Tick Size")
        dt.Columns.Add("Pre Process")
        dt.Columns.Add("Running")
        dt.Columns.Add("Active Signal")
        dt.Columns.Add("Current PL")
        dt.Columns.Add("Trade Number")

        If _TradableStrategyInstruments IsNot Nothing AndAlso _TradableStrategyInstruments.Count > 0 Then
            For Each instrument In _TradableStrategyInstruments
                Dim row As DataRow = dt.NewRow
                row("Instrument") = instrument
                row("Instrument Name") = instrument.TradableInstrument.TradingSymbol
                row("Tick Size") = instrument.TradableInstrument.TickSize
                row("Pre Process") = instrument.PreProcessingDone
                row("Running") = instrument.StrategyInstrumentRunning

                Dim lastRunningTrade As Trade = instrument.SignalData.GetLastTrade()
                If lastRunningTrade IsNot Nothing AndAlso lastRunningTrade.CurrentStatus = TradeStatus.InProgress Then
                    Dim optnStrgInstrmnt As NFOStrategyInstrument = Await instrument.GetStrategyInstrumentFromTradingSymbol(lastRunningTrade.TradingSymbol).ConfigureAwait(False)
                    If optnStrgInstrmnt IsNot Nothing Then
                        Dim pl As Decimal = 0
                        If lastRunningTrade.TypeOfEntry = EntryType.Fresh Then
                            pl = instrument.GetFreshTradePL(lastRunningTrade, optnStrgInstrmnt)
                        Else
                            pl = instrument.GetLossMakeupTradePL(lastRunningTrade, optnStrgInstrmnt)
                        End If

                        row("Active Signal") = True
                        row("Current PL") = pl
                        row("Trade Number") = lastRunningTrade.TradeNumber
                    End If
                Else
                    row("Active Signal") = False
                    row("Current PL") = 0
                    row("Trade Number") = 0
                End If

                dt.Rows.Add(row)
            Next
        End If

        dgvTradableInstruments.DataSource = dt
        dgvTradableInstruments.Columns.Item("Instrument").Visible = False

        Dim detailsColumn As DataGridViewButtonColumn = New DataGridViewButtonColumn
        detailsColumn.HeaderText = ""
        detailsColumn.Name = "details_column"
        detailsColumn.Text = "Check Trades"
        detailsColumn.UseColumnTextForButtonValue = True
        Dim detailsColumnIndex As Integer = 8
        If dgvTradableInstruments.Columns("details_column") Is Nothing Then
            dgvTradableInstruments.Columns.Insert(detailsColumnIndex, detailsColumn)
        End If

        dgvTradableInstruments.Refresh()
    End Sub

    Private Sub dgvTradableInstruments_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvTradableInstruments.CellContentClick
        If e.ColumnIndex = 8 Then
            Dim frm As Form = New frmTradeDetails(CType(dgvTradableInstruments.Rows(e.RowIndex).Cells(0).Value, NFOStrategyInstrument))
            frm.ShowDialog()
        End If
    End Sub
End Class