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
        Me.grpStockFilter = New System.Windows.Forms.GroupBox()
        Me.txtMinEODTurnoverPercentage = New System.Windows.Forms.TextBox()
        Me.lblEODTurnoverPercentage = New System.Windows.Forms.Label()
        Me.txtMinTotalCandlePer = New System.Windows.Forms.TextBox()
        Me.lblMinTotalCandlePer = New System.Windows.Forms.Label()
        Me.txtMinNonBlankCandlePer = New System.Windows.Forms.TextBox()
        Me.lblMinNonBlankCandlePer = New System.Windows.Forms.Label()
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
        Me.grpSignal = New System.Windows.Forms.GroupBox()
        Me.txtMaxFractalDiffPer = New System.Windows.Forms.TextBox()
        Me.lblMaxFractalDiffPer = New System.Windows.Forms.Label()
        Me.txtMinTurnoverPerTrade = New System.Windows.Forms.TextBox()
        Me.lblMinTurnoverPerTrade = New System.Windows.Forms.Label()
        Me.txtMaxTurnoverPerTrade = New System.Windows.Forms.TextBox()
        Me.lblMaxTurnoverPerTrade = New System.Windows.Forms.Label()
        Me.toolTipStockList = New System.Windows.Forms.ToolTip(Me.components)
        Me.grpStockSelection = New System.Windows.Forms.GroupBox()
        Me.txtMinVolumePerTillSignalTime = New System.Windows.Forms.TextBox()
        Me.lblMinVolumePerTillSignalTime = New System.Windows.Forms.Label()
        Me.txtMaxStrikeRangePer = New System.Windows.Forms.TextBox()
        Me.lblMaxStrikeRangePer = New System.Windows.Forms.Label()
        Me.dtpckrLastOptionCheckTime = New System.Windows.Forms.DateTimePicker()
        Me.lblLastEntryTime = New System.Windows.Forms.Label()
        Me.grpTelegram = New System.Windows.Forms.GroupBox()
        Me.txtTelegramInfoChatID = New System.Windows.Forms.TextBox()
        Me.lblTelegramInfoChatID = New System.Windows.Forms.Label()
        Me.txtTelegramBotAPIKey = New System.Windows.Forms.TextBox()
        Me.lblTelegramBotAPIKey = New System.Windows.Forms.Label()
        Me.txtTelegramDebugChatID = New System.Windows.Forms.TextBox()
        Me.lblTelegramDebugChatID = New System.Windows.Forms.Label()
        Me.lblSpotDirection = New System.Windows.Forms.Label()
        Me.cmbSpotDirection = New System.Windows.Forms.ComboBox()
        Me.grpStockFilter.SuspendLayout()
        Me.grpSignal.SuspendLayout()
        Me.grpStockSelection.SuspendLayout()
        Me.grpTelegram.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnSave
        '
        Me.btnSave.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnSave.ImageKey = "save-icon-36533.png"
        Me.btnSave.ImageList = Me.ImageList1
        Me.btnSave.Location = New System.Drawing.Point(864, 365)
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
        'grpStockFilter
        '
        Me.grpStockFilter.Controls.Add(Me.cmbSpotDirection)
        Me.grpStockFilter.Controls.Add(Me.lblSpotDirection)
        Me.grpStockFilter.Controls.Add(Me.txtMinEODTurnoverPercentage)
        Me.grpStockFilter.Controls.Add(Me.lblEODTurnoverPercentage)
        Me.grpStockFilter.Controls.Add(Me.txtMinTotalCandlePer)
        Me.grpStockFilter.Controls.Add(Me.lblMinTotalCandlePer)
        Me.grpStockFilter.Controls.Add(Me.txtMinNonBlankCandlePer)
        Me.grpStockFilter.Controls.Add(Me.lblMinNonBlankCandlePer)
        Me.grpStockFilter.Controls.Add(Me.txtStockList)
        Me.grpStockFilter.Controls.Add(Me.lblStockList)
        Me.grpStockFilter.Location = New System.Drawing.Point(551, -2)
        Me.grpStockFilter.Name = "grpStockFilter"
        Me.grpStockFilter.Size = New System.Drawing.Size(425, 225)
        Me.grpStockFilter.TabIndex = 2
        Me.grpStockFilter.TabStop = False
        Me.grpStockFilter.Text = "Stock and Option Filter Settings"
        '
        'txtMinEODTurnoverPercentage
        '
        Me.txtMinEODTurnoverPercentage.Location = New System.Drawing.Point(216, 186)
        Me.txtMinEODTurnoverPercentage.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMinEODTurnoverPercentage.Name = "txtMinEODTurnoverPercentage"
        Me.txtMinEODTurnoverPercentage.Size = New System.Drawing.Size(201, 22)
        Me.txtMinEODTurnoverPercentage.TabIndex = 4
        Me.txtMinEODTurnoverPercentage.Tag = "Min EOD Turnover %"
        '
        'lblEODTurnoverPercentage
        '
        Me.lblEODTurnoverPercentage.AutoSize = True
        Me.lblEODTurnoverPercentage.Location = New System.Drawing.Point(8, 189)
        Me.lblEODTurnoverPercentage.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblEODTurnoverPercentage.Name = "lblEODTurnoverPercentage"
        Me.lblEODTurnoverPercentage.Size = New System.Drawing.Size(142, 17)
        Me.lblEODTurnoverPercentage.TabIndex = 67
        Me.lblEODTurnoverPercentage.Text = "Min EOD Turnover %"
        '
        'txtMinTotalCandlePer
        '
        Me.txtMinTotalCandlePer.Location = New System.Drawing.Point(216, 152)
        Me.txtMinTotalCandlePer.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMinTotalCandlePer.Name = "txtMinTotalCandlePer"
        Me.txtMinTotalCandlePer.Size = New System.Drawing.Size(201, 22)
        Me.txtMinTotalCandlePer.TabIndex = 3
        Me.txtMinTotalCandlePer.Tag = "Min Total Candle %"
        '
        'lblMinTotalCandlePer
        '
        Me.lblMinTotalCandlePer.AutoSize = True
        Me.lblMinTotalCandlePer.Location = New System.Drawing.Point(8, 155)
        Me.lblMinTotalCandlePer.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMinTotalCandlePer.Name = "lblMinTotalCandlePer"
        Me.lblMinTotalCandlePer.Size = New System.Drawing.Size(130, 17)
        Me.lblMinTotalCandlePer.TabIndex = 65
        Me.lblMinTotalCandlePer.Text = "Min Total Candle %"
        '
        'txtMinNonBlankCandlePer
        '
        Me.txtMinNonBlankCandlePer.Location = New System.Drawing.Point(216, 117)
        Me.txtMinNonBlankCandlePer.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMinNonBlankCandlePer.Name = "txtMinNonBlankCandlePer"
        Me.txtMinNonBlankCandlePer.Size = New System.Drawing.Size(201, 22)
        Me.txtMinNonBlankCandlePer.TabIndex = 2
        Me.txtMinNonBlankCandlePer.Tag = "Min Non-Blank Candle %"
        '
        'lblMinNonBlankCandlePer
        '
        Me.lblMinNonBlankCandlePer.AutoSize = True
        Me.lblMinNonBlankCandlePer.Location = New System.Drawing.Point(8, 120)
        Me.lblMinNonBlankCandlePer.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMinNonBlankCandlePer.Name = "lblMinNonBlankCandlePer"
        Me.lblMinNonBlankCandlePer.Size = New System.Drawing.Size(164, 17)
        Me.lblMinNonBlankCandlePer.TabIndex = 63
        Me.lblMinNonBlankCandlePer.Text = "Min Non-Blank Candle %"
        '
        'txtStockList
        '
        Me.txtStockList.Location = New System.Drawing.Point(216, 24)
        Me.txtStockList.Margin = New System.Windows.Forms.Padding(4)
        Me.txtStockList.Multiline = True
        Me.txtStockList.Name = "txtStockList"
        Me.txtStockList.Size = New System.Drawing.Size(201, 49)
        Me.txtStockList.TabIndex = 0
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
        Me.txtSignalTimeFrame.TabIndex = 0
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
        Me.dtpckrTradeStartTime.TabIndex = 1
        Me.dtpckrTradeStartTime.Value = New Date(2019, 3, 12, 0, 0, 0, 0)
        '
        'dtpckrLastTradeEntryTime
        '
        Me.dtpckrLastTradeEntryTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpckrLastTradeEntryTime.Location = New System.Drawing.Point(285, 91)
        Me.dtpckrLastTradeEntryTime.Name = "dtpckrLastTradeEntryTime"
        Me.dtpckrLastTradeEntryTime.ShowUpDown = True
        Me.dtpckrLastTradeEntryTime.Size = New System.Drawing.Size(242, 22)
        Me.dtpckrLastTradeEntryTime.TabIndex = 2
        Me.dtpckrLastTradeEntryTime.Value = New Date(2019, 3, 12, 0, 0, 0, 0)
        '
        'dtpckrEODExitTime
        '
        Me.dtpckrEODExitTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpckrEODExitTime.Location = New System.Drawing.Point(285, 126)
        Me.dtpckrEODExitTime.Name = "dtpckrEODExitTime"
        Me.dtpckrEODExitTime.ShowUpDown = True
        Me.dtpckrEODExitTime.Size = New System.Drawing.Size(242, 22)
        Me.dtpckrEODExitTime.TabIndex = 3
        Me.dtpckrEODExitTime.Value = New Date(2019, 3, 12, 0, 0, 0, 0)
        '
        'lblMaxProfitPerStock
        '
        Me.lblMaxProfitPerStock.AutoSize = True
        Me.lblMaxProfitPerStock.Location = New System.Drawing.Point(9, 232)
        Me.lblMaxProfitPerStock.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMaxProfitPerStock.Name = "lblMaxProfitPerStock"
        Me.lblMaxProfitPerStock.Size = New System.Drawing.Size(135, 17)
        Me.lblMaxProfitPerStock.TabIndex = 53
        Me.lblMaxProfitPerStock.Text = "Max Profit Per Stock"
        '
        'txtMaxProfitPerStock
        '
        Me.txtMaxProfitPerStock.Location = New System.Drawing.Point(285, 229)
        Me.txtMaxProfitPerStock.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMaxProfitPerStock.Name = "txtMaxProfitPerStock"
        Me.txtMaxProfitPerStock.Size = New System.Drawing.Size(243, 22)
        Me.txtMaxProfitPerStock.TabIndex = 6
        Me.txtMaxProfitPerStock.Tag = "Max Profit Per Stock"
        '
        'grpSignal
        '
        Me.grpSignal.Controls.Add(Me.txtMaxFractalDiffPer)
        Me.grpSignal.Controls.Add(Me.lblMaxFractalDiffPer)
        Me.grpSignal.Controls.Add(Me.txtMinTurnoverPerTrade)
        Me.grpSignal.Controls.Add(Me.lblMinTurnoverPerTrade)
        Me.grpSignal.Controls.Add(Me.txtMaxTurnoverPerTrade)
        Me.grpSignal.Controls.Add(Me.lblMaxTurnoverPerTrade)
        Me.grpSignal.Controls.Add(Me.txtMaxProfitPerStock)
        Me.grpSignal.Controls.Add(Me.lblMaxProfitPerStock)
        Me.grpSignal.Controls.Add(Me.dtpckrEODExitTime)
        Me.grpSignal.Controls.Add(Me.dtpckrLastTradeEntryTime)
        Me.grpSignal.Controls.Add(Me.dtpckrTradeStartTime)
        Me.grpSignal.Controls.Add(Me.lblEODExitTime)
        Me.grpSignal.Controls.Add(Me.lblLastTradeEntryTime)
        Me.grpSignal.Controls.Add(Me.lblTradeStartTime)
        Me.grpSignal.Controls.Add(Me.txtSignalTimeFrame)
        Me.grpSignal.Controls.Add(Me.lblSignalTimeFrame)
        Me.grpSignal.Location = New System.Drawing.Point(5, -2)
        Me.grpSignal.Margin = New System.Windows.Forms.Padding(4)
        Me.grpSignal.Name = "grpSignal"
        Me.grpSignal.Padding = New System.Windows.Forms.Padding(4)
        Me.grpSignal.Size = New System.Drawing.Size(539, 298)
        Me.grpSignal.TabIndex = 1
        Me.grpSignal.TabStop = False
        Me.grpSignal.Text = "Signal Settings"
        '
        'txtMaxFractalDiffPer
        '
        Me.txtMaxFractalDiffPer.Location = New System.Drawing.Point(285, 263)
        Me.txtMaxFractalDiffPer.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMaxFractalDiffPer.Name = "txtMaxFractalDiffPer"
        Me.txtMaxFractalDiffPer.Size = New System.Drawing.Size(243, 22)
        Me.txtMaxFractalDiffPer.TabIndex = 7
        Me.txtMaxFractalDiffPer.Tag = "Max Fractal Difference %"
        '
        'lblMaxFractalDiffPer
        '
        Me.lblMaxFractalDiffPer.AutoSize = True
        Me.lblMaxFractalDiffPer.Location = New System.Drawing.Point(9, 266)
        Me.lblMaxFractalDiffPer.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMaxFractalDiffPer.Name = "lblMaxFractalDiffPer"
        Me.lblMaxFractalDiffPer.Size = New System.Drawing.Size(165, 17)
        Me.lblMaxFractalDiffPer.TabIndex = 61
        Me.lblMaxFractalDiffPer.Text = "Max Fractal Difference %"
        '
        'txtMinTurnoverPerTrade
        '
        Me.txtMinTurnoverPerTrade.Location = New System.Drawing.Point(285, 159)
        Me.txtMinTurnoverPerTrade.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMinTurnoverPerTrade.Name = "txtMinTurnoverPerTrade"
        Me.txtMinTurnoverPerTrade.Size = New System.Drawing.Size(242, 22)
        Me.txtMinTurnoverPerTrade.TabIndex = 4
        Me.txtMinTurnoverPerTrade.Tag = "Min Turnover Per Trade"
        '
        'lblMinTurnoverPerTrade
        '
        Me.lblMinTurnoverPerTrade.AutoSize = True
        Me.lblMinTurnoverPerTrade.Location = New System.Drawing.Point(8, 162)
        Me.lblMinTurnoverPerTrade.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMinTurnoverPerTrade.Name = "lblMinTurnoverPerTrade"
        Me.lblMinTurnoverPerTrade.Size = New System.Drawing.Size(160, 17)
        Me.lblMinTurnoverPerTrade.TabIndex = 59
        Me.lblMinTurnoverPerTrade.Text = "Min Turnover Per Trade"
        '
        'txtMaxTurnoverPerTrade
        '
        Me.txtMaxTurnoverPerTrade.Location = New System.Drawing.Point(285, 193)
        Me.txtMaxTurnoverPerTrade.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMaxTurnoverPerTrade.Name = "txtMaxTurnoverPerTrade"
        Me.txtMaxTurnoverPerTrade.Size = New System.Drawing.Size(242, 22)
        Me.txtMaxTurnoverPerTrade.TabIndex = 5
        Me.txtMaxTurnoverPerTrade.Tag = "Max Turnover Per Trade"
        '
        'lblMaxTurnoverPerTrade
        '
        Me.lblMaxTurnoverPerTrade.AutoSize = True
        Me.lblMaxTurnoverPerTrade.Location = New System.Drawing.Point(8, 196)
        Me.lblMaxTurnoverPerTrade.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMaxTurnoverPerTrade.Name = "lblMaxTurnoverPerTrade"
        Me.lblMaxTurnoverPerTrade.Size = New System.Drawing.Size(163, 17)
        Me.lblMaxTurnoverPerTrade.TabIndex = 57
        Me.lblMaxTurnoverPerTrade.Text = "Max Turnover Per Trade"
        '
        'grpStockSelection
        '
        Me.grpStockSelection.Controls.Add(Me.txtMinVolumePerTillSignalTime)
        Me.grpStockSelection.Controls.Add(Me.lblMinVolumePerTillSignalTime)
        Me.grpStockSelection.Controls.Add(Me.txtMaxStrikeRangePer)
        Me.grpStockSelection.Controls.Add(Me.lblMaxStrikeRangePer)
        Me.grpStockSelection.Controls.Add(Me.dtpckrLastOptionCheckTime)
        Me.grpStockSelection.Controls.Add(Me.lblLastEntryTime)
        Me.grpStockSelection.Location = New System.Drawing.Point(551, 229)
        Me.grpStockSelection.Name = "grpStockSelection"
        Me.grpStockSelection.Size = New System.Drawing.Size(425, 128)
        Me.grpStockSelection.TabIndex = 3
        Me.grpStockSelection.TabStop = False
        Me.grpStockSelection.Text = "Option Selection Settings"
        '
        'txtMinVolumePerTillSignalTime
        '
        Me.txtMinVolumePerTillSignalTime.Location = New System.Drawing.Point(216, 89)
        Me.txtMinVolumePerTillSignalTime.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMinVolumePerTillSignalTime.Name = "txtMinVolumePerTillSignalTime"
        Me.txtMinVolumePerTillSignalTime.Size = New System.Drawing.Size(201, 22)
        Me.txtMinVolumePerTillSignalTime.TabIndex = 2
        Me.txtMinVolumePerTillSignalTime.Tag = "Min Volume % Till Signal Time"
        '
        'lblMinVolumePerTillSignalTime
        '
        Me.lblMinVolumePerTillSignalTime.AutoSize = True
        Me.lblMinVolumePerTillSignalTime.Location = New System.Drawing.Point(8, 94)
        Me.lblMinVolumePerTillSignalTime.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMinVolumePerTillSignalTime.Name = "lblMinVolumePerTillSignalTime"
        Me.lblMinVolumePerTillSignalTime.Size = New System.Drawing.Size(197, 17)
        Me.lblMinVolumePerTillSignalTime.TabIndex = 71
        Me.lblMinVolumePerTillSignalTime.Text = "Min Volume % Till Signal Time"
        '
        'txtMaxStrikeRangePer
        '
        Me.txtMaxStrikeRangePer.Location = New System.Drawing.Point(216, 56)
        Me.txtMaxStrikeRangePer.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMaxStrikeRangePer.Name = "txtMaxStrikeRangePer"
        Me.txtMaxStrikeRangePer.Size = New System.Drawing.Size(201, 22)
        Me.txtMaxStrikeRangePer.TabIndex = 1
        Me.txtMaxStrikeRangePer.Tag = "Max Strike Range %"
        '
        'lblMaxStrikeRangePer
        '
        Me.lblMaxStrikeRangePer.AutoSize = True
        Me.lblMaxStrikeRangePer.Location = New System.Drawing.Point(8, 61)
        Me.lblMaxStrikeRangePer.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMaxStrikeRangePer.Name = "lblMaxStrikeRangePer"
        Me.lblMaxStrikeRangePer.Size = New System.Drawing.Size(135, 17)
        Me.lblMaxStrikeRangePer.TabIndex = 70
        Me.lblMaxStrikeRangePer.Text = "Max Strike Range %"
        '
        'dtpckrLastOptionCheckTime
        '
        Me.dtpckrLastOptionCheckTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpckrLastOptionCheckTime.Location = New System.Drawing.Point(216, 21)
        Me.dtpckrLastOptionCheckTime.Name = "dtpckrLastOptionCheckTime"
        Me.dtpckrLastOptionCheckTime.ShowUpDown = True
        Me.dtpckrLastOptionCheckTime.Size = New System.Drawing.Size(201, 22)
        Me.dtpckrLastOptionCheckTime.TabIndex = 0
        Me.dtpckrLastOptionCheckTime.Value = New Date(2019, 3, 12, 0, 0, 0, 0)
        '
        'lblLastEntryTime
        '
        Me.lblLastEntryTime.AutoSize = True
        Me.lblLastEntryTime.Location = New System.Drawing.Point(8, 26)
        Me.lblLastEntryTime.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblLastEntryTime.Name = "lblLastEntryTime"
        Me.lblLastEntryTime.Size = New System.Drawing.Size(159, 17)
        Me.lblLastEntryTime.TabIndex = 69
        Me.lblLastEntryTime.Text = "Last Option Check Time"
        '
        'grpTelegram
        '
        Me.grpTelegram.Controls.Add(Me.txtTelegramInfoChatID)
        Me.grpTelegram.Controls.Add(Me.lblTelegramInfoChatID)
        Me.grpTelegram.Controls.Add(Me.txtTelegramBotAPIKey)
        Me.grpTelegram.Controls.Add(Me.lblTelegramBotAPIKey)
        Me.grpTelegram.Controls.Add(Me.txtTelegramDebugChatID)
        Me.grpTelegram.Controls.Add(Me.lblTelegramDebugChatID)
        Me.grpTelegram.Location = New System.Drawing.Point(5, 297)
        Me.grpTelegram.Name = "grpTelegram"
        Me.grpTelegram.Size = New System.Drawing.Size(539, 124)
        Me.grpTelegram.TabIndex = 4
        Me.grpTelegram.TabStop = False
        Me.grpTelegram.Text = "Telegram Settings"
        '
        'txtTelegramInfoChatID
        '
        Me.txtTelegramInfoChatID.Location = New System.Drawing.Point(286, 92)
        Me.txtTelegramInfoChatID.Margin = New System.Windows.Forms.Padding(4)
        Me.txtTelegramInfoChatID.Name = "txtTelegramInfoChatID"
        Me.txtTelegramInfoChatID.Size = New System.Drawing.Size(243, 22)
        Me.txtTelegramInfoChatID.TabIndex = 2
        Me.txtTelegramInfoChatID.Tag = "Telegram Info Chat ID"
        '
        'lblTelegramInfoChatID
        '
        Me.lblTelegramInfoChatID.AutoSize = True
        Me.lblTelegramInfoChatID.Location = New System.Drawing.Point(10, 95)
        Me.lblTelegramInfoChatID.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblTelegramInfoChatID.Name = "lblTelegramInfoChatID"
        Me.lblTelegramInfoChatID.Size = New System.Drawing.Size(145, 17)
        Me.lblTelegramInfoChatID.TabIndex = 67
        Me.lblTelegramInfoChatID.Text = "Telegram Info Chat ID"
        '
        'txtTelegramBotAPIKey
        '
        Me.txtTelegramBotAPIKey.Location = New System.Drawing.Point(286, 25)
        Me.txtTelegramBotAPIKey.Margin = New System.Windows.Forms.Padding(4)
        Me.txtTelegramBotAPIKey.Name = "txtTelegramBotAPIKey"
        Me.txtTelegramBotAPIKey.Size = New System.Drawing.Size(242, 22)
        Me.txtTelegramBotAPIKey.TabIndex = 0
        Me.txtTelegramBotAPIKey.Tag = "Telegram Bot API Key"
        '
        'lblTelegramBotAPIKey
        '
        Me.lblTelegramBotAPIKey.AutoSize = True
        Me.lblTelegramBotAPIKey.Location = New System.Drawing.Point(9, 28)
        Me.lblTelegramBotAPIKey.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblTelegramBotAPIKey.Name = "lblTelegramBotAPIKey"
        Me.lblTelegramBotAPIKey.Size = New System.Drawing.Size(146, 17)
        Me.lblTelegramBotAPIKey.TabIndex = 66
        Me.lblTelegramBotAPIKey.Text = "Telegram Bot API Key"
        '
        'txtTelegramDebugChatID
        '
        Me.txtTelegramDebugChatID.Location = New System.Drawing.Point(286, 59)
        Me.txtTelegramDebugChatID.Margin = New System.Windows.Forms.Padding(4)
        Me.txtTelegramDebugChatID.Name = "txtTelegramDebugChatID"
        Me.txtTelegramDebugChatID.Size = New System.Drawing.Size(243, 22)
        Me.txtTelegramDebugChatID.TabIndex = 1
        Me.txtTelegramDebugChatID.Tag = "Telegram Debug Chat ID"
        '
        'lblTelegramDebugChatID
        '
        Me.lblTelegramDebugChatID.AutoSize = True
        Me.lblTelegramDebugChatID.Location = New System.Drawing.Point(10, 62)
        Me.lblTelegramDebugChatID.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblTelegramDebugChatID.Name = "lblTelegramDebugChatID"
        Me.lblTelegramDebugChatID.Size = New System.Drawing.Size(164, 17)
        Me.lblTelegramDebugChatID.TabIndex = 65
        Me.lblTelegramDebugChatID.Text = "Telegram Debug Chat ID"
        '
        'lblSpotDirection
        '
        Me.lblSpotDirection.AutoSize = True
        Me.lblSpotDirection.Location = New System.Drawing.Point(8, 86)
        Me.lblSpotDirection.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblSpotDirection.Name = "lblSpotDirection"
        Me.lblSpotDirection.Size = New System.Drawing.Size(97, 17)
        Me.lblSpotDirection.TabIndex = 69
        Me.lblSpotDirection.Text = "Spot Direction"
        '
        'cmbSpotDirection
        '
        Me.cmbSpotDirection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbSpotDirection.FormattingEnabled = True
        Me.cmbSpotDirection.Items.AddRange(New Object() {"AUTO", "BUY", "SELL"})
        Me.cmbSpotDirection.Location = New System.Drawing.Point(216, 83)
        Me.cmbSpotDirection.Name = "cmbSpotDirection"
        Me.cmbSpotDirection.Size = New System.Drawing.Size(201, 24)
        Me.cmbSpotDirection.TabIndex = 1
        '
        'frmNFOSettings
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(978, 426)
        Me.Controls.Add(Me.grpTelegram)
        Me.Controls.Add(Me.grpStockSelection)
        Me.Controls.Add(Me.grpSignal)
        Me.Controls.Add(Me.grpStockFilter)
        Me.Controls.Add(Me.btnSave)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmNFOSettings"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Adaptive Martingale Strategy - Settings"
        Me.grpStockFilter.ResumeLayout(False)
        Me.grpStockFilter.PerformLayout()
        Me.grpSignal.ResumeLayout(False)
        Me.grpSignal.PerformLayout()
        Me.grpStockSelection.ResumeLayout(False)
        Me.grpStockSelection.PerformLayout()
        Me.grpTelegram.ResumeLayout(False)
        Me.grpTelegram.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents btnSave As Button
    Friend WithEvents ImageList1 As ImageList
    Friend WithEvents grpStockFilter As GroupBox
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
    Friend WithEvents grpSignal As GroupBox
    Friend WithEvents txtMaxTurnoverPerTrade As TextBox
    Friend WithEvents lblMaxTurnoverPerTrade As Label
    Friend WithEvents txtStockList As TextBox
    Friend WithEvents lblStockList As Label
    Friend WithEvents toolTipStockList As ToolTip
    Friend WithEvents txtMaxFractalDiffPer As TextBox
    Friend WithEvents lblMaxFractalDiffPer As Label
    Friend WithEvents txtMinTurnoverPerTrade As TextBox
    Friend WithEvents lblMinTurnoverPerTrade As Label
    Friend WithEvents txtMinNonBlankCandlePer As TextBox
    Friend WithEvents lblMinNonBlankCandlePer As Label
    Friend WithEvents txtMinTotalCandlePer As TextBox
    Friend WithEvents lblMinTotalCandlePer As Label
    Friend WithEvents txtMinEODTurnoverPercentage As TextBox
    Friend WithEvents lblEODTurnoverPercentage As Label
    Friend WithEvents grpStockSelection As GroupBox
    Friend WithEvents txtMinVolumePerTillSignalTime As TextBox
    Friend WithEvents lblMinVolumePerTillSignalTime As Label
    Friend WithEvents txtMaxStrikeRangePer As TextBox
    Friend WithEvents lblMaxStrikeRangePer As Label
    Friend WithEvents dtpckrLastOptionCheckTime As DateTimePicker
    Friend WithEvents lblLastEntryTime As Label
    Friend WithEvents grpTelegram As GroupBox
    Friend WithEvents txtTelegramInfoChatID As TextBox
    Friend WithEvents lblTelegramInfoChatID As Label
    Friend WithEvents txtTelegramBotAPIKey As TextBox
    Friend WithEvents lblTelegramBotAPIKey As Label
    Friend WithEvents txtTelegramDebugChatID As TextBox
    Friend WithEvents lblTelegramDebugChatID As Label
    Friend WithEvents cmbSpotDirection As ComboBox
    Friend WithEvents lblSpotDirection As Label
End Class
