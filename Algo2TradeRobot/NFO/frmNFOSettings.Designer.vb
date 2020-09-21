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
        Me.grpStockSelection = New System.Windows.Forms.GroupBox()
        Me.txtMaxBlankCandlePer = New System.Windows.Forms.TextBox()
        Me.lblMaxBlankCandlePer = New System.Windows.Forms.Label()
        Me.txtMinVolumePer = New System.Windows.Forms.TextBox()
        Me.lblMinVolumePer = New System.Windows.Forms.Label()
        Me.dtpckrLastEntryTime = New System.Windows.Forms.DateTimePicker()
        Me.lblLastEntryTime = New System.Windows.Forms.Label()
        Me.txtStockList = New System.Windows.Forms.TextBox()
        Me.lblStockList = New System.Windows.Forms.Label()
        Me.lblSignalTimeFrame = New System.Windows.Forms.Label()
        Me.txtSignalTimeFrame = New System.Windows.Forms.TextBox()
        Me.lblTradeStartTime = New System.Windows.Forms.Label()
        Me.lblLastTradeEntryTime = New System.Windows.Forms.Label()
        Me.lblEODExitTime = New System.Windows.Forms.Label()
        Me.dtpckrTradeStartTime = New System.Windows.Forms.DateTimePicker()
        Me.dtpckrLastTradeEntryTime = New System.Windows.Forms.DateTimePicker()
        Me.dtpckrEODExitTime = New System.Windows.Forms.DateTimePicker()
        Me.lblMaxProfitPerStock = New System.Windows.Forms.Label()
        Me.txtMaxProfitPerStock = New System.Windows.Forms.TextBox()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.txtMaxStrikeRangePer = New System.Windows.Forms.TextBox()
        Me.lblMaxStrikeRangePer = New System.Windows.Forms.Label()
        Me.txtMaxFractalDiffPer = New System.Windows.Forms.TextBox()
        Me.lblMaxFractalDiffPer = New System.Windows.Forms.Label()
        Me.txtMinTurnoverPerTrade = New System.Windows.Forms.TextBox()
        Me.lblMinTurnoverPerTrade = New System.Windows.Forms.Label()
        Me.txtMaxTurnoverPerTrade = New System.Windows.Forms.TextBox()
        Me.lblMaxTurnoverPerTrade = New System.Windows.Forms.Label()
        Me.txtNumberOfStockToTrade = New System.Windows.Forms.TextBox()
        Me.lblNumberOfStockToTrade = New System.Windows.Forms.Label()
        Me.toolTipStockList = New System.Windows.Forms.ToolTip(Me.components)
        Me.txtMinTotalCandlePer = New System.Windows.Forms.TextBox()
        Me.lblMinTotalCandlePer = New System.Windows.Forms.Label()
        Me.grpStockSelection.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnSave
        '
        Me.btnSave.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnSave.ImageKey = "save-icon-36533.png"
        Me.btnSave.ImageList = Me.ImageList1
        Me.btnSave.Location = New System.Drawing.Point(797, 302)
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
        'grpStockSelection
        '
        Me.grpStockSelection.Controls.Add(Me.txtMinTotalCandlePer)
        Me.grpStockSelection.Controls.Add(Me.lblMinTotalCandlePer)
        Me.grpStockSelection.Controls.Add(Me.txtMaxBlankCandlePer)
        Me.grpStockSelection.Controls.Add(Me.lblMaxBlankCandlePer)
        Me.grpStockSelection.Controls.Add(Me.txtMinVolumePer)
        Me.grpStockSelection.Controls.Add(Me.lblMinVolumePer)
        Me.grpStockSelection.Controls.Add(Me.dtpckrLastEntryTime)
        Me.grpStockSelection.Controls.Add(Me.lblLastEntryTime)
        Me.grpStockSelection.Controls.Add(Me.txtStockList)
        Me.grpStockSelection.Controls.Add(Me.lblStockList)
        Me.grpStockSelection.Location = New System.Drawing.Point(551, -2)
        Me.grpStockSelection.Name = "grpStockSelection"
        Me.grpStockSelection.Size = New System.Drawing.Size(358, 248)
        Me.grpStockSelection.TabIndex = 46
        Me.grpStockSelection.TabStop = False
        Me.grpStockSelection.Text = "Stock Selection Settings"
        '
        'txtMaxBlankCandlePer
        '
        Me.txtMaxBlankCandlePer.Location = New System.Drawing.Point(147, 169)
        Me.txtMaxBlankCandlePer.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMaxBlankCandlePer.Name = "txtMaxBlankCandlePer"
        Me.txtMaxBlankCandlePer.Size = New System.Drawing.Size(201, 22)
        Me.txtMaxBlankCandlePer.TabIndex = 14
        Me.txtMaxBlankCandlePer.Tag = "Max Blank Candle %"
        '
        'lblMaxBlankCandlePer
        '
        Me.lblMaxBlankCandlePer.AutoSize = True
        Me.lblMaxBlankCandlePer.Location = New System.Drawing.Point(8, 172)
        Me.lblMaxBlankCandlePer.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMaxBlankCandlePer.Name = "lblMaxBlankCandlePer"
        Me.lblMaxBlankCandlePer.Size = New System.Drawing.Size(136, 17)
        Me.lblMaxBlankCandlePer.TabIndex = 63
        Me.lblMaxBlankCandlePer.Text = "Max Blank Candle %"
        '
        'txtMinVolumePer
        '
        Me.txtMinVolumePer.Location = New System.Drawing.Point(147, 134)
        Me.txtMinVolumePer.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMinVolumePer.Name = "txtMinVolumePer"
        Me.txtMinVolumePer.Size = New System.Drawing.Size(201, 22)
        Me.txtMinVolumePer.TabIndex = 13
        Me.txtMinVolumePer.Tag = "Min Volume %"
        '
        'lblMinVolumePer
        '
        Me.lblMinVolumePer.AutoSize = True
        Me.lblMinVolumePer.Location = New System.Drawing.Point(8, 137)
        Me.lblMinVolumePer.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMinVolumePer.Name = "lblMinVolumePer"
        Me.lblMinVolumePer.Size = New System.Drawing.Size(97, 17)
        Me.lblMinVolumePer.TabIndex = 61
        Me.lblMinVolumePer.Text = "Min Volume %"
        '
        'dtpckrLastEntryTime
        '
        Me.dtpckrLastEntryTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpckrLastEntryTime.Location = New System.Drawing.Point(147, 100)
        Me.dtpckrLastEntryTime.Name = "dtpckrLastEntryTime"
        Me.dtpckrLastEntryTime.ShowUpDown = True
        Me.dtpckrLastEntryTime.Size = New System.Drawing.Size(201, 22)
        Me.dtpckrLastEntryTime.TabIndex = 12
        Me.dtpckrLastEntryTime.Value = New Date(2019, 3, 12, 0, 0, 0, 0)
        '
        'lblLastEntryTime
        '
        Me.lblLastEntryTime.AutoSize = True
        Me.lblLastEntryTime.Location = New System.Drawing.Point(8, 103)
        Me.lblLastEntryTime.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblLastEntryTime.Name = "lblLastEntryTime"
        Me.lblLastEntryTime.Size = New System.Drawing.Size(107, 17)
        Me.lblLastEntryTime.TabIndex = 41
        Me.lblLastEntryTime.Text = "Last Entry Time"
        '
        'txtStockList
        '
        Me.txtStockList.Location = New System.Drawing.Point(147, 24)
        Me.txtStockList.Margin = New System.Windows.Forms.Padding(4)
        Me.txtStockList.Multiline = True
        Me.txtStockList.Name = "txtStockList"
        Me.txtStockList.Size = New System.Drawing.Size(201, 63)
        Me.txtStockList.TabIndex = 11
        Me.txtStockList.Tag = "Stock List"
        Me.toolTipStockList.SetToolTip(Me.txtStockList, "Add core stock name seperated by comma. Options stocks will be selected automatic" &
        "ally from core stock name.")
        '
        'lblStockList
        '
        Me.lblStockList.AutoSize = True
        Me.lblStockList.Location = New System.Drawing.Point(8, 27)
        Me.lblStockList.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblStockList.Name = "lblStockList"
        Me.lblStockList.Size = New System.Drawing.Size(69, 17)
        Me.lblStockList.TabIndex = 39
        Me.lblStockList.Text = "Stock List"
        Me.toolTipStockList.SetToolTip(Me.lblStockList, "Add core stock name seperated by comma. Options stocks will be selected automatic" &
        "ally from core stock name.")
        '
        'lblSignalTimeFrame
        '
        Me.lblSignalTimeFrame.AutoSize = True
        Me.lblSignalTimeFrame.Location = New System.Drawing.Point(9, 20)
        Me.lblSignalTimeFrame.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblSignalTimeFrame.Name = "lblSignalTimeFrame"
        Me.lblSignalTimeFrame.Size = New System.Drawing.Size(158, 17)
        Me.lblSignalTimeFrame.TabIndex = 3
        Me.lblSignalTimeFrame.Text = "Signal Time Frame(min)"
        '
        'txtSignalTimeFrame
        '
        Me.txtSignalTimeFrame.Location = New System.Drawing.Point(285, 17)
        Me.txtSignalTimeFrame.Margin = New System.Windows.Forms.Padding(4)
        Me.txtSignalTimeFrame.Name = "txtSignalTimeFrame"
        Me.txtSignalTimeFrame.Size = New System.Drawing.Size(242, 22)
        Me.txtSignalTimeFrame.TabIndex = 1
        Me.txtSignalTimeFrame.Tag = "Signal Time Frame"
        '
        'lblTradeStartTime
        '
        Me.lblTradeStartTime.AutoSize = True
        Me.lblTradeStartTime.Location = New System.Drawing.Point(9, 56)
        Me.lblTradeStartTime.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblTradeStartTime.Name = "lblTradeStartTime"
        Me.lblTradeStartTime.Size = New System.Drawing.Size(115, 17)
        Me.lblTradeStartTime.TabIndex = 19
        Me.lblTradeStartTime.Text = "Trade Start Time"
        '
        'lblLastTradeEntryTime
        '
        Me.lblLastTradeEntryTime.AutoSize = True
        Me.lblLastTradeEntryTime.Location = New System.Drawing.Point(9, 92)
        Me.lblLastTradeEntryTime.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblLastTradeEntryTime.Name = "lblLastTradeEntryTime"
        Me.lblLastTradeEntryTime.Size = New System.Drawing.Size(149, 17)
        Me.lblLastTradeEntryTime.TabIndex = 21
        Me.lblLastTradeEntryTime.Text = "Last Trade Entry Time"
        '
        'lblEODExitTime
        '
        Me.lblEODExitTime.AutoSize = True
        Me.lblEODExitTime.Location = New System.Drawing.Point(9, 127)
        Me.lblEODExitTime.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblEODExitTime.Name = "lblEODExitTime"
        Me.lblEODExitTime.Size = New System.Drawing.Size(99, 17)
        Me.lblEODExitTime.TabIndex = 23
        Me.lblEODExitTime.Text = "EOD Exit Time"
        '
        'dtpckrTradeStartTime
        '
        Me.dtpckrTradeStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpckrTradeStartTime.Location = New System.Drawing.Point(285, 54)
        Me.dtpckrTradeStartTime.Name = "dtpckrTradeStartTime"
        Me.dtpckrTradeStartTime.ShowUpDown = True
        Me.dtpckrTradeStartTime.Size = New System.Drawing.Size(242, 22)
        Me.dtpckrTradeStartTime.TabIndex = 2
        Me.dtpckrTradeStartTime.Value = New Date(2019, 3, 12, 0, 0, 0, 0)
        '
        'dtpckrLastTradeEntryTime
        '
        Me.dtpckrLastTradeEntryTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpckrLastTradeEntryTime.Location = New System.Drawing.Point(285, 91)
        Me.dtpckrLastTradeEntryTime.Name = "dtpckrLastTradeEntryTime"
        Me.dtpckrLastTradeEntryTime.ShowUpDown = True
        Me.dtpckrLastTradeEntryTime.Size = New System.Drawing.Size(242, 22)
        Me.dtpckrLastTradeEntryTime.TabIndex = 3
        Me.dtpckrLastTradeEntryTime.Value = New Date(2019, 3, 12, 0, 0, 0, 0)
        '
        'dtpckrEODExitTime
        '
        Me.dtpckrEODExitTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpckrEODExitTime.Location = New System.Drawing.Point(285, 126)
        Me.dtpckrEODExitTime.Name = "dtpckrEODExitTime"
        Me.dtpckrEODExitTime.ShowUpDown = True
        Me.dtpckrEODExitTime.Size = New System.Drawing.Size(242, 22)
        Me.dtpckrEODExitTime.TabIndex = 4
        Me.dtpckrEODExitTime.Value = New Date(2019, 3, 12, 0, 0, 0, 0)
        '
        'lblMaxProfitPerStock
        '
        Me.lblMaxProfitPerStock.AutoSize = True
        Me.lblMaxProfitPerStock.Location = New System.Drawing.Point(9, 265)
        Me.lblMaxProfitPerStock.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMaxProfitPerStock.Name = "lblMaxProfitPerStock"
        Me.lblMaxProfitPerStock.Size = New System.Drawing.Size(135, 17)
        Me.lblMaxProfitPerStock.TabIndex = 53
        Me.lblMaxProfitPerStock.Text = "Max Profit Per Stock"
        '
        'txtMaxProfitPerStock
        '
        Me.txtMaxProfitPerStock.Location = New System.Drawing.Point(285, 262)
        Me.txtMaxProfitPerStock.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMaxProfitPerStock.Name = "txtMaxProfitPerStock"
        Me.txtMaxProfitPerStock.Size = New System.Drawing.Size(243, 22)
        Me.txtMaxProfitPerStock.TabIndex = 8
        Me.txtMaxProfitPerStock.Tag = "Max Profit Per Stock"
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.txtMaxStrikeRangePer)
        Me.GroupBox1.Controls.Add(Me.lblMaxStrikeRangePer)
        Me.GroupBox1.Controls.Add(Me.txtMaxFractalDiffPer)
        Me.GroupBox1.Controls.Add(Me.lblMaxFractalDiffPer)
        Me.GroupBox1.Controls.Add(Me.txtMinTurnoverPerTrade)
        Me.GroupBox1.Controls.Add(Me.lblMinTurnoverPerTrade)
        Me.GroupBox1.Controls.Add(Me.txtMaxTurnoverPerTrade)
        Me.GroupBox1.Controls.Add(Me.lblMaxTurnoverPerTrade)
        Me.GroupBox1.Controls.Add(Me.txtNumberOfStockToTrade)
        Me.GroupBox1.Controls.Add(Me.lblNumberOfStockToTrade)
        Me.GroupBox1.Controls.Add(Me.txtMaxProfitPerStock)
        Me.GroupBox1.Controls.Add(Me.lblMaxProfitPerStock)
        Me.GroupBox1.Controls.Add(Me.dtpckrEODExitTime)
        Me.GroupBox1.Controls.Add(Me.dtpckrLastTradeEntryTime)
        Me.GroupBox1.Controls.Add(Me.dtpckrTradeStartTime)
        Me.GroupBox1.Controls.Add(Me.lblEODExitTime)
        Me.GroupBox1.Controls.Add(Me.lblLastTradeEntryTime)
        Me.GroupBox1.Controls.Add(Me.lblTradeStartTime)
        Me.GroupBox1.Controls.Add(Me.txtSignalTimeFrame)
        Me.GroupBox1.Controls.Add(Me.lblSignalTimeFrame)
        Me.GroupBox1.Location = New System.Drawing.Point(5, -2)
        Me.GroupBox1.Margin = New System.Windows.Forms.Padding(4)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Padding = New System.Windows.Forms.Padding(4)
        Me.GroupBox1.Size = New System.Drawing.Size(539, 362)
        Me.GroupBox1.TabIndex = 44
        Me.GroupBox1.TabStop = False
        '
        'txtMaxStrikeRangePer
        '
        Me.txtMaxStrikeRangePer.Location = New System.Drawing.Point(285, 330)
        Me.txtMaxStrikeRangePer.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMaxStrikeRangePer.Name = "txtMaxStrikeRangePer"
        Me.txtMaxStrikeRangePer.Size = New System.Drawing.Size(243, 22)
        Me.txtMaxStrikeRangePer.TabIndex = 10
        Me.txtMaxStrikeRangePer.Tag = "Max Strike Range %"
        '
        'lblMaxStrikeRangePer
        '
        Me.lblMaxStrikeRangePer.AutoSize = True
        Me.lblMaxStrikeRangePer.Location = New System.Drawing.Point(9, 333)
        Me.lblMaxStrikeRangePer.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMaxStrikeRangePer.Name = "lblMaxStrikeRangePer"
        Me.lblMaxStrikeRangePer.Size = New System.Drawing.Size(135, 17)
        Me.lblMaxStrikeRangePer.TabIndex = 63
        Me.lblMaxStrikeRangePer.Text = "Max Strike Range %"
        '
        'txtMaxFractalDiffPer
        '
        Me.txtMaxFractalDiffPer.Location = New System.Drawing.Point(285, 295)
        Me.txtMaxFractalDiffPer.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMaxFractalDiffPer.Name = "txtMaxFractalDiffPer"
        Me.txtMaxFractalDiffPer.Size = New System.Drawing.Size(243, 22)
        Me.txtMaxFractalDiffPer.TabIndex = 9
        Me.txtMaxFractalDiffPer.Tag = "Max Fractal Difference %"
        '
        'lblMaxFractalDiffPer
        '
        Me.lblMaxFractalDiffPer.AutoSize = True
        Me.lblMaxFractalDiffPer.Location = New System.Drawing.Point(9, 298)
        Me.lblMaxFractalDiffPer.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMaxFractalDiffPer.Name = "lblMaxFractalDiffPer"
        Me.lblMaxFractalDiffPer.Size = New System.Drawing.Size(165, 17)
        Me.lblMaxFractalDiffPer.TabIndex = 61
        Me.lblMaxFractalDiffPer.Text = "Max Fractal Difference %"
        '
        'txtMinTurnoverPerTrade
        '
        Me.txtMinTurnoverPerTrade.Location = New System.Drawing.Point(285, 194)
        Me.txtMinTurnoverPerTrade.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMinTurnoverPerTrade.Name = "txtMinTurnoverPerTrade"
        Me.txtMinTurnoverPerTrade.Size = New System.Drawing.Size(242, 22)
        Me.txtMinTurnoverPerTrade.TabIndex = 6
        Me.txtMinTurnoverPerTrade.Tag = "Min Turnover Per Trade"
        '
        'lblMinTurnoverPerTrade
        '
        Me.lblMinTurnoverPerTrade.AutoSize = True
        Me.lblMinTurnoverPerTrade.Location = New System.Drawing.Point(8, 197)
        Me.lblMinTurnoverPerTrade.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMinTurnoverPerTrade.Name = "lblMinTurnoverPerTrade"
        Me.lblMinTurnoverPerTrade.Size = New System.Drawing.Size(160, 17)
        Me.lblMinTurnoverPerTrade.TabIndex = 59
        Me.lblMinTurnoverPerTrade.Text = "Min Turnover Per Trade"
        '
        'txtMaxTurnoverPerTrade
        '
        Me.txtMaxTurnoverPerTrade.Location = New System.Drawing.Point(285, 228)
        Me.txtMaxTurnoverPerTrade.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMaxTurnoverPerTrade.Name = "txtMaxTurnoverPerTrade"
        Me.txtMaxTurnoverPerTrade.Size = New System.Drawing.Size(242, 22)
        Me.txtMaxTurnoverPerTrade.TabIndex = 7
        Me.txtMaxTurnoverPerTrade.Tag = "Max Turnover Per Trade"
        '
        'lblMaxTurnoverPerTrade
        '
        Me.lblMaxTurnoverPerTrade.AutoSize = True
        Me.lblMaxTurnoverPerTrade.Location = New System.Drawing.Point(8, 231)
        Me.lblMaxTurnoverPerTrade.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMaxTurnoverPerTrade.Name = "lblMaxTurnoverPerTrade"
        Me.lblMaxTurnoverPerTrade.Size = New System.Drawing.Size(163, 17)
        Me.lblMaxTurnoverPerTrade.TabIndex = 57
        Me.lblMaxTurnoverPerTrade.Text = "Max Turnover Per Trade"
        '
        'txtNumberOfStockToTrade
        '
        Me.txtNumberOfStockToTrade.Location = New System.Drawing.Point(285, 160)
        Me.txtNumberOfStockToTrade.Margin = New System.Windows.Forms.Padding(4)
        Me.txtNumberOfStockToTrade.Name = "txtNumberOfStockToTrade"
        Me.txtNumberOfStockToTrade.Size = New System.Drawing.Size(242, 22)
        Me.txtNumberOfStockToTrade.TabIndex = 5
        Me.txtNumberOfStockToTrade.Tag = "Number Of Stock To Trade"
        '
        'lblNumberOfStockToTrade
        '
        Me.lblNumberOfStockToTrade.AutoSize = True
        Me.lblNumberOfStockToTrade.Location = New System.Drawing.Point(8, 163)
        Me.lblNumberOfStockToTrade.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblNumberOfStockToTrade.Name = "lblNumberOfStockToTrade"
        Me.lblNumberOfStockToTrade.Size = New System.Drawing.Size(179, 17)
        Me.lblNumberOfStockToTrade.TabIndex = 48
        Me.lblNumberOfStockToTrade.Text = "Number Of Stock To Trade"
        '
        'txtMinTotalCandlePer
        '
        Me.txtMinTotalCandlePer.Location = New System.Drawing.Point(147, 204)
        Me.txtMinTotalCandlePer.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMinTotalCandlePer.Name = "txtMinTotalCandlePer"
        Me.txtMinTotalCandlePer.Size = New System.Drawing.Size(201, 22)
        Me.txtMinTotalCandlePer.TabIndex = 15
        Me.txtMinTotalCandlePer.Tag = "Min Total Candle %"
        '
        'lblMinTotalCandlePer
        '
        Me.lblMinTotalCandlePer.AutoSize = True
        Me.lblMinTotalCandlePer.Location = New System.Drawing.Point(8, 207)
        Me.lblMinTotalCandlePer.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMinTotalCandlePer.Name = "lblMinTotalCandlePer"
        Me.lblMinTotalCandlePer.Size = New System.Drawing.Size(130, 17)
        Me.lblMinTotalCandlePer.TabIndex = 65
        Me.lblMinTotalCandlePer.Text = "Min Total Candle %"
        '
        'frmNFOSettings
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(913, 367)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.grpStockSelection)
        Me.Controls.Add(Me.btnSave)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmNFOSettings"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "NFO Settings"
        Me.grpStockSelection.ResumeLayout(False)
        Me.grpStockSelection.PerformLayout()
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents btnSave As Button
    Friend WithEvents ImageList1 As ImageList
    Friend WithEvents grpStockSelection As GroupBox
    Friend WithEvents lblSignalTimeFrame As Label
    Friend WithEvents txtSignalTimeFrame As TextBox
    Friend WithEvents lblTradeStartTime As Label
    Friend WithEvents lblLastTradeEntryTime As Label
    Friend WithEvents lblEODExitTime As Label
    Friend WithEvents dtpckrTradeStartTime As DateTimePicker
    Friend WithEvents dtpckrLastTradeEntryTime As DateTimePicker
    Friend WithEvents dtpckrEODExitTime As DateTimePicker
    Friend WithEvents lblMaxProfitPerStock As Label
    Friend WithEvents txtMaxProfitPerStock As TextBox
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents txtNumberOfStockToTrade As TextBox
    Friend WithEvents lblNumberOfStockToTrade As Label
    Friend WithEvents txtMaxTurnoverPerTrade As TextBox
    Friend WithEvents lblMaxTurnoverPerTrade As Label
    Friend WithEvents txtStockList As TextBox
    Friend WithEvents lblStockList As Label
    Friend WithEvents toolTipStockList As ToolTip
    Friend WithEvents dtpckrLastEntryTime As DateTimePicker
    Friend WithEvents lblLastEntryTime As Label
    Friend WithEvents txtMinVolumePer As TextBox
    Friend WithEvents lblMinVolumePer As Label
    Friend WithEvents txtMaxFractalDiffPer As TextBox
    Friend WithEvents lblMaxFractalDiffPer As Label
    Friend WithEvents txtMinTurnoverPerTrade As TextBox
    Friend WithEvents lblMinTurnoverPerTrade As Label
    Friend WithEvents txtMaxStrikeRangePer As TextBox
    Friend WithEvents lblMaxStrikeRangePer As Label
    Friend WithEvents txtMaxBlankCandlePer As TextBox
    Friend WithEvents lblMaxBlankCandlePer As Label
    Friend WithEvents txtMinTotalCandlePer As TextBox
    Friend WithEvents lblMinTotalCandlePer As Label
End Class
