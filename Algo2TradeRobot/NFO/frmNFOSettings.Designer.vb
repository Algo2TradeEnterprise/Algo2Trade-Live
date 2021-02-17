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
        Me.grpTradeDetails = New System.Windows.Forms.GroupBox()
        Me.txtTargetPer = New System.Windows.Forms.TextBox()
        Me.lblTargetPer = New System.Windows.Forms.Label()
        Me.nmrcActiveInstrumentCount = New System.Windows.Forms.NumericUpDown()
        Me.lblActiveInstrumentCount = New System.Windows.Forms.Label()
        Me.dtpckrTradeEntryTime = New System.Windows.Forms.DateTimePicker()
        Me.lblTradeEntryTime = New System.Windows.Forms.Label()
        Me.grpStockSelectionDetails = New System.Windows.Forms.GroupBox()
        Me.pnlStockSelectionDetails = New System.Windows.Forms.Panel()
        Me.txtMinimumATR = New System.Windows.Forms.TextBox()
        Me.lblMinimumATR = New System.Windows.Forms.Label()
        Me.txtMinimumVolume = New System.Windows.Forms.TextBox()
        Me.lblMinimumVolume = New System.Windows.Forms.Label()
        Me.txtMaximumStockPrice = New System.Windows.Forms.TextBox()
        Me.lblMaximumStockPrice = New System.Windows.Forms.Label()
        Me.txtMinimumStockPrice = New System.Windows.Forms.TextBox()
        Me.lblMinimumStockPrice = New System.Windows.Forms.Label()
        Me.chkbAutoSelectStock = New System.Windows.Forms.CheckBox()
        Me.btnBrowse = New System.Windows.Forms.Button()
        Me.txtInstrumentDetalis = New System.Windows.Forms.TextBox()
        Me.lblInstrumentDetails = New System.Windows.Forms.Label()
        Me.grpIndicatorDetails = New System.Windows.Forms.GroupBox()
        Me.nmrcSMAPeriod = New System.Windows.Forms.NumericUpDown()
        Me.lblSMAPeriod = New System.Windows.Forms.Label()
        Me.nmrcATRPeriod = New System.Windows.Forms.NumericUpDown()
        Me.lblATRPeriod = New System.Windows.Forms.Label()
        Me.txtDistance = New System.Windows.Forms.TextBox()
        Me.lblDistance = New System.Windows.Forms.Label()
        Me.grpTradeDetails.SuspendLayout()
        CType(Me.nmrcActiveInstrumentCount, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpStockSelectionDetails.SuspendLayout()
        Me.pnlStockSelectionDetails.SuspendLayout()
        Me.grpIndicatorDetails.SuspendLayout()
        CType(Me.nmrcSMAPeriod, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.nmrcATRPeriod, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'btnSave
        '
        Me.btnSave.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnSave.ImageKey = "save-icon-36533.png"
        Me.btnSave.ImageList = Me.ImageList1
        Me.btnSave.Location = New System.Drawing.Point(767, 206)
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
        'grpTradeDetails
        '
        Me.grpTradeDetails.Controls.Add(Me.txtDistance)
        Me.grpTradeDetails.Controls.Add(Me.lblDistance)
        Me.grpTradeDetails.Controls.Add(Me.txtTargetPer)
        Me.grpTradeDetails.Controls.Add(Me.lblTargetPer)
        Me.grpTradeDetails.Controls.Add(Me.nmrcActiveInstrumentCount)
        Me.grpTradeDetails.Controls.Add(Me.lblActiveInstrumentCount)
        Me.grpTradeDetails.Controls.Add(Me.dtpckrTradeEntryTime)
        Me.grpTradeDetails.Controls.Add(Me.lblTradeEntryTime)
        Me.grpTradeDetails.Location = New System.Drawing.Point(4, 0)
        Me.grpTradeDetails.Margin = New System.Windows.Forms.Padding(4)
        Me.grpTradeDetails.Name = "grpTradeDetails"
        Me.grpTradeDetails.Padding = New System.Windows.Forms.Padding(4)
        Me.grpTradeDetails.Size = New System.Drawing.Size(325, 177)
        Me.grpTradeDetails.TabIndex = 53
        Me.grpTradeDetails.TabStop = False
        Me.grpTradeDetails.Text = "Trade Details"
        '
        'txtTargetPer
        '
        Me.txtTargetPer.Location = New System.Drawing.Point(210, 83)
        Me.txtTargetPer.Name = "txtTargetPer"
        Me.txtTargetPer.Size = New System.Drawing.Size(107, 22)
        Me.txtTargetPer.TabIndex = 67
        Me.txtTargetPer.Tag = "Target %"
        '
        'lblTargetPer
        '
        Me.lblTargetPer.AutoSize = True
        Me.lblTargetPer.Location = New System.Drawing.Point(9, 84)
        Me.lblTargetPer.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblTargetPer.Name = "lblTargetPer"
        Me.lblTargetPer.Size = New System.Drawing.Size(66, 17)
        Me.lblTargetPer.TabIndex = 66
        Me.lblTargetPer.Text = "Target %"
        '
        'nmrcActiveInstrumentCount
        '
        Me.nmrcActiveInstrumentCount.Location = New System.Drawing.Point(210, 52)
        Me.nmrcActiveInstrumentCount.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nmrcActiveInstrumentCount.Name = "nmrcActiveInstrumentCount"
        Me.nmrcActiveInstrumentCount.Size = New System.Drawing.Size(107, 22)
        Me.nmrcActiveInstrumentCount.TabIndex = 56
        Me.nmrcActiveInstrumentCount.Tag = "Active Instrument Count"
        Me.nmrcActiveInstrumentCount.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'lblActiveInstrumentCount
        '
        Me.lblActiveInstrumentCount.AutoSize = True
        Me.lblActiveInstrumentCount.Location = New System.Drawing.Point(8, 54)
        Me.lblActiveInstrumentCount.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblActiveInstrumentCount.Name = "lblActiveInstrumentCount"
        Me.lblActiveInstrumentCount.Size = New System.Drawing.Size(157, 17)
        Me.lblActiveInstrumentCount.TabIndex = 55
        Me.lblActiveInstrumentCount.Text = "Active Instrument Count"
        '
        'dtpckrTradeEntryTime
        '
        Me.dtpckrTradeEntryTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpckrTradeEntryTime.Location = New System.Drawing.Point(210, 21)
        Me.dtpckrTradeEntryTime.Name = "dtpckrTradeEntryTime"
        Me.dtpckrTradeEntryTime.ShowUpDown = True
        Me.dtpckrTradeEntryTime.Size = New System.Drawing.Size(107, 22)
        Me.dtpckrTradeEntryTime.TabIndex = 0
        Me.dtpckrTradeEntryTime.Value = New Date(2019, 3, 12, 0, 0, 0, 0)
        '
        'lblTradeEntryTime
        '
        Me.lblTradeEntryTime.AutoSize = True
        Me.lblTradeEntryTime.Location = New System.Drawing.Point(9, 23)
        Me.lblTradeEntryTime.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblTradeEntryTime.Name = "lblTradeEntryTime"
        Me.lblTradeEntryTime.Size = New System.Drawing.Size(118, 17)
        Me.lblTradeEntryTime.TabIndex = 19
        Me.lblTradeEntryTime.Text = "Trade Entry Time"
        '
        'grpStockSelectionDetails
        '
        Me.grpStockSelectionDetails.Controls.Add(Me.pnlStockSelectionDetails)
        Me.grpStockSelectionDetails.Controls.Add(Me.chkbAutoSelectStock)
        Me.grpStockSelectionDetails.Controls.Add(Me.btnBrowse)
        Me.grpStockSelectionDetails.Controls.Add(Me.txtInstrumentDetalis)
        Me.grpStockSelectionDetails.Controls.Add(Me.lblInstrumentDetails)
        Me.grpStockSelectionDetails.Location = New System.Drawing.Point(336, 0)
        Me.grpStockSelectionDetails.Name = "grpStockSelectionDetails"
        Me.grpStockSelectionDetails.Size = New System.Drawing.Size(543, 199)
        Me.grpStockSelectionDetails.TabIndex = 54
        Me.grpStockSelectionDetails.TabStop = False
        Me.grpStockSelectionDetails.Text = "Stock Selection Details"
        '
        'pnlStockSelectionDetails
        '
        Me.pnlStockSelectionDetails.Controls.Add(Me.txtMinimumATR)
        Me.pnlStockSelectionDetails.Controls.Add(Me.lblMinimumATR)
        Me.pnlStockSelectionDetails.Controls.Add(Me.txtMinimumVolume)
        Me.pnlStockSelectionDetails.Controls.Add(Me.lblMinimumVolume)
        Me.pnlStockSelectionDetails.Controls.Add(Me.txtMaximumStockPrice)
        Me.pnlStockSelectionDetails.Controls.Add(Me.lblMaximumStockPrice)
        Me.pnlStockSelectionDetails.Controls.Add(Me.txtMinimumStockPrice)
        Me.pnlStockSelectionDetails.Controls.Add(Me.lblMinimumStockPrice)
        Me.pnlStockSelectionDetails.Location = New System.Drawing.Point(0, 95)
        Me.pnlStockSelectionDetails.Name = "pnlStockSelectionDetails"
        Me.pnlStockSelectionDetails.Size = New System.Drawing.Size(543, 92)
        Me.pnlStockSelectionDetails.TabIndex = 13
        '
        'txtMinimumATR
        '
        Me.txtMinimumATR.Location = New System.Drawing.Point(419, 55)
        Me.txtMinimumATR.Name = "txtMinimumATR"
        Me.txtMinimumATR.Size = New System.Drawing.Size(100, 22)
        Me.txtMinimumATR.TabIndex = 65
        Me.txtMinimumATR.Tag = "Minimum ATR %"
        '
        'lblMinimumATR
        '
        Me.lblMinimumATR.AutoSize = True
        Me.lblMinimumATR.Location = New System.Drawing.Point(270, 56)
        Me.lblMinimumATR.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMinimumATR.Name = "lblMinimumATR"
        Me.lblMinimumATR.Size = New System.Drawing.Size(111, 17)
        Me.lblMinimumATR.TabIndex = 64
        Me.lblMinimumATR.Text = "Minimum ATR %"
        '
        'txtMinimumVolume
        '
        Me.txtMinimumVolume.Location = New System.Drawing.Point(156, 57)
        Me.txtMinimumVolume.Name = "txtMinimumVolume"
        Me.txtMinimumVolume.Size = New System.Drawing.Size(100, 22)
        Me.txtMinimumVolume.TabIndex = 63
        Me.txtMinimumVolume.Tag = "Minimum Volume"
        '
        'lblMinimumVolume
        '
        Me.lblMinimumVolume.AutoSize = True
        Me.lblMinimumVolume.Location = New System.Drawing.Point(7, 58)
        Me.lblMinimumVolume.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMinimumVolume.Name = "lblMinimumVolume"
        Me.lblMinimumVolume.Size = New System.Drawing.Size(114, 17)
        Me.lblMinimumVolume.TabIndex = 62
        Me.lblMinimumVolume.Text = "Minimum Volume"
        '
        'txtMaximumStockPrice
        '
        Me.txtMaximumStockPrice.Location = New System.Drawing.Point(419, 17)
        Me.txtMaximumStockPrice.Name = "txtMaximumStockPrice"
        Me.txtMaximumStockPrice.Size = New System.Drawing.Size(100, 22)
        Me.txtMaximumStockPrice.TabIndex = 61
        Me.txtMaximumStockPrice.Tag = "Maximum Stock Price"
        '
        'lblMaximumStockPrice
        '
        Me.lblMaximumStockPrice.AutoSize = True
        Me.lblMaximumStockPrice.Location = New System.Drawing.Point(270, 18)
        Me.lblMaximumStockPrice.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMaximumStockPrice.Name = "lblMaximumStockPrice"
        Me.lblMaximumStockPrice.Size = New System.Drawing.Size(141, 17)
        Me.lblMaximumStockPrice.TabIndex = 60
        Me.lblMaximumStockPrice.Text = "Maximum Stock Price"
        '
        'txtMinimumStockPrice
        '
        Me.txtMinimumStockPrice.Location = New System.Drawing.Point(156, 19)
        Me.txtMinimumStockPrice.Name = "txtMinimumStockPrice"
        Me.txtMinimumStockPrice.Size = New System.Drawing.Size(100, 22)
        Me.txtMinimumStockPrice.TabIndex = 59
        Me.txtMinimumStockPrice.Tag = "Minimum Stock Price"
        '
        'lblMinimumStockPrice
        '
        Me.lblMinimumStockPrice.AutoSize = True
        Me.lblMinimumStockPrice.Location = New System.Drawing.Point(7, 20)
        Me.lblMinimumStockPrice.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMinimumStockPrice.Name = "lblMinimumStockPrice"
        Me.lblMinimumStockPrice.Size = New System.Drawing.Size(138, 17)
        Me.lblMinimumStockPrice.TabIndex = 58
        Me.lblMinimumStockPrice.Text = "Minimum Stock Price"
        '
        'chkbAutoSelectStock
        '
        Me.chkbAutoSelectStock.AutoSize = True
        Me.chkbAutoSelectStock.Location = New System.Drawing.Point(11, 64)
        Me.chkbAutoSelectStock.Name = "chkbAutoSelectStock"
        Me.chkbAutoSelectStock.Size = New System.Drawing.Size(141, 21)
        Me.chkbAutoSelectStock.TabIndex = 12
        Me.chkbAutoSelectStock.Text = "Auto Select Stock"
        Me.chkbAutoSelectStock.UseVisualStyleBackColor = True
        '
        'btnBrowse
        '
        Me.btnBrowse.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnBrowse.Location = New System.Drawing.Point(485, 24)
        Me.btnBrowse.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnBrowse.Name = "btnBrowse"
        Me.btnBrowse.Size = New System.Drawing.Size(42, 23)
        Me.btnBrowse.TabIndex = 9
        Me.btnBrowse.Text = "..."
        Me.btnBrowse.UseVisualStyleBackColor = True
        '
        'txtInstrumentDetalis
        '
        Me.txtInstrumentDetalis.Location = New System.Drawing.Point(151, 25)
        Me.txtInstrumentDetalis.Margin = New System.Windows.Forms.Padding(4)
        Me.txtInstrumentDetalis.Name = "txtInstrumentDetalis"
        Me.txtInstrumentDetalis.ReadOnly = True
        Me.txtInstrumentDetalis.Size = New System.Drawing.Size(327, 22)
        Me.txtInstrumentDetalis.TabIndex = 10
        '
        'lblInstrumentDetails
        '
        Me.lblInstrumentDetails.AutoSize = True
        Me.lblInstrumentDetails.Location = New System.Drawing.Point(8, 28)
        Me.lblInstrumentDetails.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblInstrumentDetails.Name = "lblInstrumentDetails"
        Me.lblInstrumentDetails.Size = New System.Drawing.Size(121, 17)
        Me.lblInstrumentDetails.TabIndex = 11
        Me.lblInstrumentDetails.Text = "Instrument Details"
        '
        'grpIndicatorDetails
        '
        Me.grpIndicatorDetails.Controls.Add(Me.nmrcSMAPeriod)
        Me.grpIndicatorDetails.Controls.Add(Me.lblSMAPeriod)
        Me.grpIndicatorDetails.Controls.Add(Me.nmrcATRPeriod)
        Me.grpIndicatorDetails.Controls.Add(Me.lblATRPeriod)
        Me.grpIndicatorDetails.Location = New System.Drawing.Point(4, 184)
        Me.grpIndicatorDetails.Name = "grpIndicatorDetails"
        Me.grpIndicatorDetails.Size = New System.Drawing.Size(325, 84)
        Me.grpIndicatorDetails.TabIndex = 55
        Me.grpIndicatorDetails.TabStop = False
        Me.grpIndicatorDetails.Text = "Indicator Details"
        '
        'nmrcSMAPeriod
        '
        Me.nmrcSMAPeriod.Location = New System.Drawing.Point(183, 53)
        Me.nmrcSMAPeriod.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nmrcSMAPeriod.Name = "nmrcSMAPeriod"
        Me.nmrcSMAPeriod.Size = New System.Drawing.Size(107, 22)
        Me.nmrcSMAPeriod.TabIndex = 60
        Me.nmrcSMAPeriod.Tag = "Pivot Period"
        Me.nmrcSMAPeriod.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'lblSMAPeriod
        '
        Me.lblSMAPeriod.AutoSize = True
        Me.lblSMAPeriod.Location = New System.Drawing.Point(8, 55)
        Me.lblSMAPeriod.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblSMAPeriod.Name = "lblSMAPeriod"
        Me.lblSMAPeriod.Size = New System.Drawing.Size(82, 17)
        Me.lblSMAPeriod.TabIndex = 59
        Me.lblSMAPeriod.Text = "SMA Period"
        '
        'nmrcATRPeriod
        '
        Me.nmrcATRPeriod.Location = New System.Drawing.Point(183, 24)
        Me.nmrcATRPeriod.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.nmrcATRPeriod.Name = "nmrcATRPeriod"
        Me.nmrcATRPeriod.Size = New System.Drawing.Size(107, 22)
        Me.nmrcATRPeriod.TabIndex = 58
        Me.nmrcATRPeriod.Tag = "ATR Period"
        Me.nmrcATRPeriod.Value = New Decimal(New Integer() {1, 0, 0, 0})
        '
        'lblATRPeriod
        '
        Me.lblATRPeriod.AutoSize = True
        Me.lblATRPeriod.Location = New System.Drawing.Point(9, 26)
        Me.lblATRPeriod.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblATRPeriod.Name = "lblATRPeriod"
        Me.lblATRPeriod.Size = New System.Drawing.Size(81, 17)
        Me.lblATRPeriod.TabIndex = 57
        Me.lblATRPeriod.Text = "ATR Period"
        '
        'txtDistance
        '
        Me.txtDistance.Location = New System.Drawing.Point(210, 114)
        Me.txtDistance.Name = "txtDistance"
        Me.txtDistance.Size = New System.Drawing.Size(107, 22)
        Me.txtDistance.TabIndex = 69
        Me.txtDistance.Tag = "Distance from MA (ATR Mul)"
        '
        'lblDistance
        '
        Me.lblDistance.AutoSize = True
        Me.lblDistance.Location = New System.Drawing.Point(9, 115)
        Me.lblDistance.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblDistance.Name = "lblDistance"
        Me.lblDistance.Size = New System.Drawing.Size(187, 17)
        Me.lblDistance.TabIndex = 68
        Me.lblDistance.Text = "Distance from MA (ATR Mul)"
        '
        'frmNFOSettings
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(883, 272)
        Me.Controls.Add(Me.grpIndicatorDetails)
        Me.Controls.Add(Me.grpStockSelectionDetails)
        Me.Controls.Add(Me.grpTradeDetails)
        Me.Controls.Add(Me.btnSave)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmNFOSettings"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Settings"
        Me.grpTradeDetails.ResumeLayout(False)
        Me.grpTradeDetails.PerformLayout()
        CType(Me.nmrcActiveInstrumentCount, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpStockSelectionDetails.ResumeLayout(False)
        Me.grpStockSelectionDetails.PerformLayout()
        Me.pnlStockSelectionDetails.ResumeLayout(False)
        Me.pnlStockSelectionDetails.PerformLayout()
        Me.grpIndicatorDetails.ResumeLayout(False)
        Me.grpIndicatorDetails.PerformLayout()
        CType(Me.nmrcSMAPeriod, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.nmrcATRPeriod, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents btnSave As Button
    Friend WithEvents ImageList1 As ImageList
    Friend WithEvents opnFileSettings As OpenFileDialog
    Friend WithEvents grpTradeDetails As GroupBox
    Friend WithEvents dtpckrTradeEntryTime As DateTimePicker
    Friend WithEvents lblTradeEntryTime As Label
    Friend WithEvents lblActiveInstrumentCount As Label
    Friend WithEvents nmrcActiveInstrumentCount As NumericUpDown
    Friend WithEvents grpStockSelectionDetails As GroupBox
    Friend WithEvents btnBrowse As Button
    Friend WithEvents txtInstrumentDetalis As TextBox
    Friend WithEvents lblInstrumentDetails As Label
    Friend WithEvents grpIndicatorDetails As GroupBox
    Friend WithEvents nmrcSMAPeriod As NumericUpDown
    Friend WithEvents lblSMAPeriod As Label
    Friend WithEvents nmrcATRPeriod As NumericUpDown
    Friend WithEvents lblATRPeriod As Label
    Friend WithEvents pnlStockSelectionDetails As Panel
    Friend WithEvents chkbAutoSelectStock As CheckBox
    Friend WithEvents lblMinimumStockPrice As Label
    Friend WithEvents txtMinimumStockPrice As TextBox
    Friend WithEvents txtMaximumStockPrice As TextBox
    Friend WithEvents lblMaximumStockPrice As Label
    Friend WithEvents txtMinimumVolume As TextBox
    Friend WithEvents lblMinimumVolume As Label
    Friend WithEvents txtMinimumATR As TextBox
    Friend WithEvents lblMinimumATR As Label
    Friend WithEvents txtTargetPer As TextBox
    Friend WithEvents lblTargetPer As Label
    Friend WithEvents txtDistance As TextBox
    Friend WithEvents lblDistance As Label
End Class
