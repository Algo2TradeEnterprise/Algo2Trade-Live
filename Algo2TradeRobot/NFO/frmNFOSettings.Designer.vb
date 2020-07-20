<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmNFOSettings
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmNFOSettings))
        Me.btnSave = New System.Windows.Forms.Button()
        Me.ImageList1 = New System.Windows.Forms.ImageList(Me.components)
        Me.opnFileSettings = New System.Windows.Forms.OpenFileDialog()
        Me.lblEODExitTime = New System.Windows.Forms.Label()
        Me.dtpckrEODExitTime = New System.Windows.Forms.DateTimePicker()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.lblRangeBreakout = New System.Windows.Forms.Label()
        Me.cmbRangeBreakout = New System.Windows.Forms.ComboBox()
        Me.cmbNumberOfTradePerStock = New System.Windows.Forms.ComboBox()
        Me.lblNumberOfTradePerStock = New System.Windows.Forms.Label()
        Me.lblMTMProfit = New System.Windows.Forms.Label()
        Me.txtMTMProfit = New System.Windows.Forms.TextBox()
        Me.txtMTMLoss = New System.Windows.Forms.TextBox()
        Me.lblMTMLoss = New System.Windows.Forms.Label()
        Me.btnBrowse = New System.Windows.Forms.Button()
        Me.txtInstrumentDetalis = New System.Windows.Forms.TextBox()
        Me.lblInstrumentDetails = New System.Windows.Forms.Label()
        Me.GroupBox1.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnSave
        '
        Me.btnSave.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnSave.ImageKey = "save-icon-36533.png"
        Me.btnSave.ImageList = Me.ImageList1
        Me.btnSave.Location = New System.Drawing.Point(490, 9)
        Me.btnSave.Margin = New System.Windows.Forms.Padding(4)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(112, 58)
        Me.btnSave.TabIndex = 0
        Me.btnSave.Text = "&Save"
        Me.btnSave.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.btnSave.UseVisualStyleBackColor = True
        '
        'ImageList1
        '
        Me.ImageList1.ImageStream = CType(resources.GetObject("ImageList1.ImageStream"), System.Windows.Forms.ImageListStreamer)
        Me.ImageList1.TransparentColor = System.Drawing.Color.Transparent
        Me.ImageList1.Images.SetKeyName(0, "save-icon-36533.png")
        '
        'opnFileSettings
        '
        '
        'lblEODExitTime
        '
        Me.lblEODExitTime.AutoSize = True
        Me.lblEODExitTime.Location = New System.Drawing.Point(9, 52)
        Me.lblEODExitTime.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblEODExitTime.Name = "lblEODExitTime"
        Me.lblEODExitTime.Size = New System.Drawing.Size(99, 17)
        Me.lblEODExitTime.TabIndex = 23
        Me.lblEODExitTime.Text = "EOD Exit Time"
        '
        'dtpckrEODExitTime
        '
        Me.dtpckrEODExitTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpckrEODExitTime.Location = New System.Drawing.Point(227, 51)
        Me.dtpckrEODExitTime.Name = "dtpckrEODExitTime"
        Me.dtpckrEODExitTime.ShowUpDown = True
        Me.dtpckrEODExitTime.Size = New System.Drawing.Size(242, 22)
        Me.dtpckrEODExitTime.TabIndex = 2
        Me.dtpckrEODExitTime.Value = New Date(2019, 3, 12, 0, 0, 0, 0)
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.btnBrowse)
        Me.GroupBox1.Controls.Add(Me.txtInstrumentDetalis)
        Me.GroupBox1.Controls.Add(Me.lblInstrumentDetails)
        Me.GroupBox1.Controls.Add(Me.txtMTMLoss)
        Me.GroupBox1.Controls.Add(Me.lblMTMLoss)
        Me.GroupBox1.Controls.Add(Me.txtMTMProfit)
        Me.GroupBox1.Controls.Add(Me.lblMTMProfit)
        Me.GroupBox1.Controls.Add(Me.cmbNumberOfTradePerStock)
        Me.GroupBox1.Controls.Add(Me.lblNumberOfTradePerStock)
        Me.GroupBox1.Controls.Add(Me.cmbRangeBreakout)
        Me.GroupBox1.Controls.Add(Me.lblRangeBreakout)
        Me.GroupBox1.Controls.Add(Me.dtpckrEODExitTime)
        Me.GroupBox1.Controls.Add(Me.lblEODExitTime)
        Me.GroupBox1.Location = New System.Drawing.Point(5, -2)
        Me.GroupBox1.Margin = New System.Windows.Forms.Padding(4)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Padding = New System.Windows.Forms.Padding(4)
        Me.GroupBox1.Size = New System.Drawing.Size(477, 221)
        Me.GroupBox1.TabIndex = 44
        Me.GroupBox1.TabStop = False
        '
        'lblRangeBreakout
        '
        Me.lblRangeBreakout.AutoSize = True
        Me.lblRangeBreakout.Location = New System.Drawing.Point(9, 19)
        Me.lblRangeBreakout.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblRangeBreakout.Name = "lblRangeBreakout"
        Me.lblRangeBreakout.Size = New System.Drawing.Size(111, 17)
        Me.lblRangeBreakout.TabIndex = 24
        Me.lblRangeBreakout.Text = "Range Breakout"
        '
        'cmbRangeBreakout
        '
        Me.cmbRangeBreakout.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbRangeBreakout.FormattingEnabled = True
        Me.cmbRangeBreakout.Location = New System.Drawing.Point(227, 16)
        Me.cmbRangeBreakout.Name = "cmbRangeBreakout"
        Me.cmbRangeBreakout.Size = New System.Drawing.Size(242, 24)
        Me.cmbRangeBreakout.TabIndex = 1
        '
        'cmbNumberOfTradePerStock
        '
        Me.cmbNumberOfTradePerStock.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbNumberOfTradePerStock.FormattingEnabled = True
        Me.cmbNumberOfTradePerStock.Items.AddRange(New Object() {"1", "2"})
        Me.cmbNumberOfTradePerStock.Location = New System.Drawing.Point(227, 85)
        Me.cmbNumberOfTradePerStock.Name = "cmbNumberOfTradePerStock"
        Me.cmbNumberOfTradePerStock.Size = New System.Drawing.Size(242, 24)
        Me.cmbNumberOfTradePerStock.TabIndex = 3
        '
        'lblNumberOfTradePerStock
        '
        Me.lblNumberOfTradePerStock.AutoSize = True
        Me.lblNumberOfTradePerStock.Location = New System.Drawing.Point(9, 88)
        Me.lblNumberOfTradePerStock.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblNumberOfTradePerStock.Name = "lblNumberOfTradePerStock"
        Me.lblNumberOfTradePerStock.Size = New System.Drawing.Size(184, 17)
        Me.lblNumberOfTradePerStock.TabIndex = 26
        Me.lblNumberOfTradePerStock.Text = "Number Of Trade Per Stock"
        '
        'lblMTMProfit
        '
        Me.lblMTMProfit.AutoSize = True
        Me.lblMTMProfit.Location = New System.Drawing.Point(9, 124)
        Me.lblMTMProfit.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMTMProfit.Name = "lblMTMProfit"
        Me.lblMTMProfit.Size = New System.Drawing.Size(76, 17)
        Me.lblMTMProfit.TabIndex = 28
        Me.lblMTMProfit.Text = "MTM Profit"
        '
        'txtMTMProfit
        '
        Me.txtMTMProfit.Location = New System.Drawing.Point(227, 121)
        Me.txtMTMProfit.Name = "txtMTMProfit"
        Me.txtMTMProfit.Size = New System.Drawing.Size(242, 22)
        Me.txtMTMProfit.TabIndex = 4
        Me.txtMTMProfit.Tag = "MTM Profit"
        '
        'txtMTMLoss
        '
        Me.txtMTMLoss.Location = New System.Drawing.Point(227, 154)
        Me.txtMTMLoss.Name = "txtMTMLoss"
        Me.txtMTMLoss.Size = New System.Drawing.Size(242, 22)
        Me.txtMTMLoss.TabIndex = 5
        Me.txtMTMLoss.Tag = "MTM Loss"
        '
        'lblMTMLoss
        '
        Me.lblMTMLoss.AutoSize = True
        Me.lblMTMLoss.Location = New System.Drawing.Point(9, 157)
        Me.lblMTMLoss.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMTMLoss.Name = "lblMTMLoss"
        Me.lblMTMLoss.Size = New System.Drawing.Size(73, 17)
        Me.lblMTMLoss.TabIndex = 30
        Me.lblMTMLoss.Text = "MTM Loss"
        '
        'btnBrowse
        '
        Me.btnBrowse.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnBrowse.Location = New System.Drawing.Point(438, 189)
        Me.btnBrowse.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnBrowse.Name = "btnBrowse"
        Me.btnBrowse.Size = New System.Drawing.Size(31, 23)
        Me.btnBrowse.TabIndex = 6
        Me.btnBrowse.Text = "..."
        Me.btnBrowse.UseVisualStyleBackColor = True
        '
        'txtInstrumentDetalis
        '
        Me.txtInstrumentDetalis.Location = New System.Drawing.Point(227, 189)
        Me.txtInstrumentDetalis.Margin = New System.Windows.Forms.Padding(4)
        Me.txtInstrumentDetalis.Name = "txtInstrumentDetalis"
        Me.txtInstrumentDetalis.ReadOnly = True
        Me.txtInstrumentDetalis.Size = New System.Drawing.Size(204, 22)
        Me.txtInstrumentDetalis.TabIndex = 6
        '
        'lblInstrumentDetails
        '
        Me.lblInstrumentDetails.AutoSize = True
        Me.lblInstrumentDetails.Location = New System.Drawing.Point(9, 191)
        Me.lblInstrumentDetails.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblInstrumentDetails.Name = "lblInstrumentDetails"
        Me.lblInstrumentDetails.Size = New System.Drawing.Size(121, 17)
        Me.lblInstrumentDetails.TabIndex = 34
        Me.lblInstrumentDetails.Text = "Instrument Details"
        '
        'frmNFOSettings
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(608, 224)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.btnSave)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmNFOSettings"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Settings"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents btnSave As Button
    Friend WithEvents ImageList1 As ImageList
    Friend WithEvents opnFileSettings As OpenFileDialog
    Friend WithEvents lblEODExitTime As Label
    Friend WithEvents dtpckrEODExitTime As DateTimePicker
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents cmbRangeBreakout As ComboBox
    Friend WithEvents lblRangeBreakout As Label
    Friend WithEvents cmbNumberOfTradePerStock As ComboBox
    Friend WithEvents lblNumberOfTradePerStock As Label
    Friend WithEvents txtMTMProfit As TextBox
    Friend WithEvents lblMTMProfit As Label
    Friend WithEvents txtMTMLoss As TextBox
    Friend WithEvents lblMTMLoss As Label
    Friend WithEvents btnBrowse As Button
    Friend WithEvents txtInstrumentDetalis As TextBox
    Friend WithEvents lblInstrumentDetails As Label
End Class
