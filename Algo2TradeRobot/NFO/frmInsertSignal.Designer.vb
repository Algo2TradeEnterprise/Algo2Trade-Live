<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmInsertSignal
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmInsertSignal))
        Me.ImageList1 = New System.Windows.Forms.ImageList(Me.components)
        Me.btnInsert = New System.Windows.Forms.Button()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.txtEntryPrice = New System.Windows.Forms.TextBox()
        Me.lblEntryPrice = New System.Windows.Forms.Label()
        Me.dtpckrTradingDate = New System.Windows.Forms.DateTimePicker()
        Me.lblTradingDate = New System.Windows.Forms.Label()
        Me.GroupBox1.SuspendLayout()
        Me.SuspendLayout()
        '
        'ImageList1
        '
        Me.ImageList1.ImageStream = CType(resources.GetObject("ImageList1.ImageStream"), System.Windows.Forms.ImageListStreamer)
        Me.ImageList1.TransparentColor = System.Drawing.Color.Transparent
        Me.ImageList1.Images.SetKeyName(0, "save-icon-36533.png")
        '
        'btnInsert
        '
        Me.btnInsert.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnInsert.ImageKey = "save-icon-36533.png"
        Me.btnInsert.ImageList = Me.ImageList1
        Me.btnInsert.Location = New System.Drawing.Point(400, 8)
        Me.btnInsert.Margin = New System.Windows.Forms.Padding(4)
        Me.btnInsert.Name = "btnInsert"
        Me.btnInsert.Size = New System.Drawing.Size(102, 58)
        Me.btnInsert.TabIndex = 0
        Me.btnInsert.Text = "&Insert"
        Me.btnInsert.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.btnInsert.UseVisualStyleBackColor = True
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.txtEntryPrice)
        Me.GroupBox1.Controls.Add(Me.lblEntryPrice)
        Me.GroupBox1.Controls.Add(Me.dtpckrTradingDate)
        Me.GroupBox1.Controls.Add(Me.lblTradingDate)
        Me.GroupBox1.Location = New System.Drawing.Point(4, 1)
        Me.GroupBox1.Margin = New System.Windows.Forms.Padding(4)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Padding = New System.Windows.Forms.Padding(4)
        Me.GroupBox1.Size = New System.Drawing.Size(388, 84)
        Me.GroupBox1.TabIndex = 53
        Me.GroupBox1.TabStop = False
        '
        'txtEntryPrice
        '
        Me.txtEntryPrice.Location = New System.Drawing.Point(132, 50)
        Me.txtEntryPrice.Margin = New System.Windows.Forms.Padding(4)
        Me.txtEntryPrice.Name = "txtEntryPrice"
        Me.txtEntryPrice.Size = New System.Drawing.Size(243, 22)
        Me.txtEntryPrice.TabIndex = 2
        Me.txtEntryPrice.Tag = "Entry Price"
        '
        'lblEntryPrice
        '
        Me.lblEntryPrice.AutoSize = True
        Me.lblEntryPrice.Location = New System.Drawing.Point(9, 52)
        Me.lblEntryPrice.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblEntryPrice.Name = "lblEntryPrice"
        Me.lblEntryPrice.Size = New System.Drawing.Size(77, 17)
        Me.lblEntryPrice.TabIndex = 53
        Me.lblEntryPrice.Text = "Entry Price"
        '
        'dtpckrTradingDate
        '
        Me.dtpckrTradingDate.Format = System.Windows.Forms.DateTimePickerFormat.[Short]
        Me.dtpckrTradingDate.Location = New System.Drawing.Point(132, 17)
        Me.dtpckrTradingDate.Name = "dtpckrTradingDate"
        Me.dtpckrTradingDate.Size = New System.Drawing.Size(243, 22)
        Me.dtpckrTradingDate.TabIndex = 1
        Me.dtpckrTradingDate.Value = New Date(2020, 11, 10, 0, 0, 0, 0)
        '
        'lblTradingDate
        '
        Me.lblTradingDate.AutoSize = True
        Me.lblTradingDate.Location = New System.Drawing.Point(9, 18)
        Me.lblTradingDate.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblTradingDate.Name = "lblTradingDate"
        Me.lblTradingDate.Size = New System.Drawing.Size(91, 17)
        Me.lblTradingDate.TabIndex = 19
        Me.lblTradingDate.Text = "Trading Date"
        '
        'frmInsertSignal
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(504, 90)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.btnInsert)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmInsertSignal"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Add Signal"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents ImageList1 As ImageList
    Friend WithEvents btnInsert As Button
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents txtEntryPrice As TextBox
    Friend WithEvents lblEntryPrice As Label
    Friend WithEvents dtpckrTradingDate As DateTimePicker
    Friend WithEvents lblTradingDate As Label
End Class
