<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmLowSLSettings
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmLowSLSettings))
        Me.txtMinPrice = New System.Windows.Forms.TextBox()
        Me.rdbFuture = New System.Windows.Forms.RadioButton()
        Me.rdbCash = New System.Windows.Forms.RadioButton()
        Me.chbAutoSelectStock = New System.Windows.Forms.CheckBox()
        Me.ImageList1 = New System.Windows.Forms.ImageList(Me.components)
        Me.grbInstrumentType = New System.Windows.Forms.GroupBox()
        Me.opnFileSettings = New System.Windows.Forms.OpenFileDialog()
        Me.txtNumberOfStock = New System.Windows.Forms.TextBox()
        Me.lblNumberOfStock = New System.Windows.Forms.Label()
        Me.txtATRPercentage = New System.Windows.Forms.TextBox()
        Me.lblATR = New System.Windows.Forms.Label()
        Me.txtMaxPrice = New System.Windows.Forms.TextBox()
        Me.lblMaxPrice = New System.Windows.Forms.Label()
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.txtMaxBlankCandlePer = New System.Windows.Forms.TextBox()
        Me.lblMaxBlankCandlePer = New System.Windows.Forms.Label()
        Me.lblMinPrice = New System.Windows.Forms.Label()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.txtStockMaxProfitPerDay = New System.Windows.Forms.TextBox()
        Me.lblStockMaxProfitPerDay = New System.Windows.Forms.Label()
        Me.txtStockMaxLossPerDay = New System.Windows.Forms.TextBox()
        Me.lblStockMaxLossPerDay = New System.Windows.Forms.Label()
        Me.txtOverallMaxProfitPerDay = New System.Windows.Forms.TextBox()
        Me.lblOverallMaxProfitPerDay = New System.Windows.Forms.Label()
        Me.txtOverallMaxLossPerDay = New System.Windows.Forms.TextBox()
        Me.lblOverallMaxLossPerDay = New System.Windows.Forms.Label()
        Me.txtNumberOfTradePerStock = New System.Windows.Forms.TextBox()
        Me.lblNumberOfTradePerStock = New System.Windows.Forms.Label()
        Me.txtTargetMultiplier = New System.Windows.Forms.TextBox()
        Me.lblTargetMultiplier = New System.Windows.Forms.Label()
        Me.dtpckrEODExitTime = New System.Windows.Forms.DateTimePicker()
        Me.dtpckrLastTradeEntryTime = New System.Windows.Forms.DateTimePicker()
        Me.dtpckrTradeStartTime = New System.Windows.Forms.DateTimePicker()
        Me.lblEODExitTime = New System.Windows.Forms.Label()
        Me.lblLastTradeEntryTime = New System.Windows.Forms.Label()
        Me.lblTradeStartTime = New System.Windows.Forms.Label()
        Me.btnBrowse = New System.Windows.Forms.Button()
        Me.txtInstrumentDetalis = New System.Windows.Forms.TextBox()
        Me.lblInstrumentDetails = New System.Windows.Forms.Label()
        Me.txtSignalTimeFrame = New System.Windows.Forms.TextBox()
        Me.lblSignalTimeFrame = New System.Windows.Forms.Label()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.btnLowSLStrategySettings = New System.Windows.Forms.Button()
        Me.txtMaxLossPerTrade = New System.Windows.Forms.TextBox()
        Me.lblMaxLossPerTrade = New System.Windows.Forms.Label()
        Me.txtMinLossPerTrade = New System.Windows.Forms.TextBox()
        Me.lblMinLossPerTrade = New System.Windows.Forms.Label()
        Me.txtMinInvestmentPerStock = New System.Windows.Forms.TextBox()
        Me.lblMinInvestmentPerStock = New System.Windows.Forms.Label()
        Me.grbInstrumentType.SuspendLayout()
        Me.GroupBox3.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.SuspendLayout()
        '
        'txtMinPrice
        '
        Me.txtMinPrice.Location = New System.Drawing.Point(146, 25)
        Me.txtMinPrice.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMinPrice.Name = "txtMinPrice"
        Me.txtMinPrice.Size = New System.Drawing.Size(201, 22)
        Me.txtMinPrice.TabIndex = 17
        Me.txtMinPrice.Tag = "Min Price"
        '
        'rdbFuture
        '
        Me.rdbFuture.AutoSize = True
        Me.rdbFuture.Checked = True
        Me.rdbFuture.Location = New System.Drawing.Point(85, 26)
        Me.rdbFuture.Name = "rdbFuture"
        Me.rdbFuture.Size = New System.Drawing.Size(70, 21)
        Me.rdbFuture.TabIndex = 1
        Me.rdbFuture.TabStop = True
        Me.rdbFuture.Text = "Future"
        Me.rdbFuture.UseVisualStyleBackColor = True
        '
        'rdbCash
        '
        Me.rdbCash.AutoSize = True
        Me.rdbCash.Location = New System.Drawing.Point(14, 26)
        Me.rdbCash.Name = "rdbCash"
        Me.rdbCash.Size = New System.Drawing.Size(61, 21)
        Me.rdbCash.TabIndex = 0
        Me.rdbCash.Text = "Cash"
        Me.rdbCash.UseVisualStyleBackColor = True
        '
        'chbAutoSelectStock
        '
        Me.chbAutoSelectStock.AutoSize = True
        Me.chbAutoSelectStock.Location = New System.Drawing.Point(12, 34)
        Me.chbAutoSelectStock.Name = "chbAutoSelectStock"
        Me.chbAutoSelectStock.Size = New System.Drawing.Size(141, 21)
        Me.chbAutoSelectStock.TabIndex = 13
        Me.chbAutoSelectStock.Text = "Auto Select Stock"
        Me.chbAutoSelectStock.UseVisualStyleBackColor = True
        '
        'ImageList1
        '
        Me.ImageList1.ImageStream = CType(resources.GetObject("ImageList1.ImageStream"), System.Windows.Forms.ImageListStreamer)
        Me.ImageList1.TransparentColor = System.Drawing.Color.Transparent
        Me.ImageList1.Images.SetKeyName(0, "save-icon-36533.png")
        '
        'grbInstrumentType
        '
        Me.grbInstrumentType.Controls.Add(Me.rdbFuture)
        Me.grbInstrumentType.Controls.Add(Me.rdbCash)
        Me.grbInstrumentType.Location = New System.Drawing.Point(185, 14)
        Me.grbInstrumentType.Name = "grbInstrumentType"
        Me.grbInstrumentType.Size = New System.Drawing.Size(162, 62)
        Me.grbInstrumentType.TabIndex = 14
        Me.grbInstrumentType.TabStop = False
        Me.grbInstrumentType.Text = "Instrument Type"
        '
        'opnFileSettings
        '
        '
        'txtNumberOfStock
        '
        Me.txtNumberOfStock.Location = New System.Drawing.Point(146, 165)
        Me.txtNumberOfStock.Margin = New System.Windows.Forms.Padding(4)
        Me.txtNumberOfStock.Name = "txtNumberOfStock"
        Me.txtNumberOfStock.Size = New System.Drawing.Size(201, 22)
        Me.txtNumberOfStock.TabIndex = 21
        Me.txtNumberOfStock.Tag = "Number Of Stock"
        '
        'lblNumberOfStock
        '
        Me.lblNumberOfStock.AutoSize = True
        Me.lblNumberOfStock.Location = New System.Drawing.Point(9, 168)
        Me.lblNumberOfStock.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblNumberOfStock.Name = "lblNumberOfStock"
        Me.lblNumberOfStock.Size = New System.Drawing.Size(116, 17)
        Me.lblNumberOfStock.TabIndex = 43
        Me.lblNumberOfStock.Text = "Number Of Stock"
        '
        'txtATRPercentage
        '
        Me.txtATRPercentage.Location = New System.Drawing.Point(146, 95)
        Me.txtATRPercentage.Margin = New System.Windows.Forms.Padding(4)
        Me.txtATRPercentage.Name = "txtATRPercentage"
        Me.txtATRPercentage.Size = New System.Drawing.Size(201, 22)
        Me.txtATRPercentage.TabIndex = 19
        Me.txtATRPercentage.Tag = "ATR %"
        '
        'lblATR
        '
        Me.lblATR.AutoSize = True
        Me.lblATR.Location = New System.Drawing.Point(9, 98)
        Me.lblATR.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblATR.Name = "lblATR"
        Me.lblATR.Size = New System.Drawing.Size(52, 17)
        Me.lblATR.TabIndex = 39
        Me.lblATR.Text = "ATR %"
        '
        'txtMaxPrice
        '
        Me.txtMaxPrice.Location = New System.Drawing.Point(146, 59)
        Me.txtMaxPrice.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMaxPrice.Name = "txtMaxPrice"
        Me.txtMaxPrice.Size = New System.Drawing.Size(201, 22)
        Me.txtMaxPrice.TabIndex = 18
        Me.txtMaxPrice.Tag = "Max Price"
        '
        'lblMaxPrice
        '
        Me.lblMaxPrice.AutoSize = True
        Me.lblMaxPrice.Location = New System.Drawing.Point(9, 62)
        Me.lblMaxPrice.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMaxPrice.Name = "lblMaxPrice"
        Me.lblMaxPrice.Size = New System.Drawing.Size(69, 17)
        Me.lblMaxPrice.TabIndex = 37
        Me.lblMaxPrice.Text = "Max Price"
        '
        'GroupBox3
        '
        Me.GroupBox3.Controls.Add(Me.txtMaxBlankCandlePer)
        Me.GroupBox3.Controls.Add(Me.lblMaxBlankCandlePer)
        Me.GroupBox3.Controls.Add(Me.txtNumberOfStock)
        Me.GroupBox3.Controls.Add(Me.lblNumberOfStock)
        Me.GroupBox3.Controls.Add(Me.txtATRPercentage)
        Me.GroupBox3.Controls.Add(Me.lblATR)
        Me.GroupBox3.Controls.Add(Me.txtMaxPrice)
        Me.GroupBox3.Controls.Add(Me.lblMaxPrice)
        Me.GroupBox3.Controls.Add(Me.txtMinPrice)
        Me.GroupBox3.Controls.Add(Me.lblMinPrice)
        Me.GroupBox3.Location = New System.Drawing.Point(487, 88)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.Size = New System.Drawing.Size(358, 195)
        Me.GroupBox3.TabIndex = 39
        Me.GroupBox3.TabStop = False
        Me.GroupBox3.Text = "Stock Selection Settings"
        '
        'txtMaxBlankCandlePer
        '
        Me.txtMaxBlankCandlePer.Location = New System.Drawing.Point(146, 130)
        Me.txtMaxBlankCandlePer.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMaxBlankCandlePer.Name = "txtMaxBlankCandlePer"
        Me.txtMaxBlankCandlePer.Size = New System.Drawing.Size(201, 22)
        Me.txtMaxBlankCandlePer.TabIndex = 22
        Me.txtMaxBlankCandlePer.Tag = "Min Price"
        '
        'lblMaxBlankCandlePer
        '
        Me.lblMaxBlankCandlePer.AutoSize = True
        Me.lblMaxBlankCandlePer.Location = New System.Drawing.Point(9, 133)
        Me.lblMaxBlankCandlePer.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMaxBlankCandlePer.Name = "lblMaxBlankCandlePer"
        Me.lblMaxBlankCandlePer.Size = New System.Drawing.Size(136, 17)
        Me.lblMaxBlankCandlePer.TabIndex = 46
        Me.lblMaxBlankCandlePer.Text = "Max Blank Candle %"
        '
        'lblMinPrice
        '
        Me.lblMinPrice.AutoSize = True
        Me.lblMinPrice.Location = New System.Drawing.Point(9, 28)
        Me.lblMinPrice.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMinPrice.Name = "lblMinPrice"
        Me.lblMinPrice.Size = New System.Drawing.Size(66, 17)
        Me.lblMinPrice.TabIndex = 35
        Me.lblMinPrice.Text = "Min Price"
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.txtMaxLossPerTrade)
        Me.GroupBox1.Controls.Add(Me.lblMaxLossPerTrade)
        Me.GroupBox1.Controls.Add(Me.txtMinLossPerTrade)
        Me.GroupBox1.Controls.Add(Me.lblMinLossPerTrade)
        Me.GroupBox1.Controls.Add(Me.txtMinInvestmentPerStock)
        Me.GroupBox1.Controls.Add(Me.lblMinInvestmentPerStock)
        Me.GroupBox1.Controls.Add(Me.txtStockMaxProfitPerDay)
        Me.GroupBox1.Controls.Add(Me.lblStockMaxProfitPerDay)
        Me.GroupBox1.Controls.Add(Me.txtStockMaxLossPerDay)
        Me.GroupBox1.Controls.Add(Me.lblStockMaxLossPerDay)
        Me.GroupBox1.Controls.Add(Me.txtOverallMaxProfitPerDay)
        Me.GroupBox1.Controls.Add(Me.lblOverallMaxProfitPerDay)
        Me.GroupBox1.Controls.Add(Me.txtOverallMaxLossPerDay)
        Me.GroupBox1.Controls.Add(Me.lblOverallMaxLossPerDay)
        Me.GroupBox1.Controls.Add(Me.txtNumberOfTradePerStock)
        Me.GroupBox1.Controls.Add(Me.lblNumberOfTradePerStock)
        Me.GroupBox1.Controls.Add(Me.txtTargetMultiplier)
        Me.GroupBox1.Controls.Add(Me.lblTargetMultiplier)
        Me.GroupBox1.Controls.Add(Me.dtpckrEODExitTime)
        Me.GroupBox1.Controls.Add(Me.dtpckrLastTradeEntryTime)
        Me.GroupBox1.Controls.Add(Me.dtpckrTradeStartTime)
        Me.GroupBox1.Controls.Add(Me.lblEODExitTime)
        Me.GroupBox1.Controls.Add(Me.lblLastTradeEntryTime)
        Me.GroupBox1.Controls.Add(Me.lblTradeStartTime)
        Me.GroupBox1.Controls.Add(Me.btnBrowse)
        Me.GroupBox1.Controls.Add(Me.txtInstrumentDetalis)
        Me.GroupBox1.Controls.Add(Me.lblInstrumentDetails)
        Me.GroupBox1.Controls.Add(Me.txtSignalTimeFrame)
        Me.GroupBox1.Controls.Add(Me.lblSignalTimeFrame)
        Me.GroupBox1.Location = New System.Drawing.Point(2, -6)
        Me.GroupBox1.Margin = New System.Windows.Forms.Padding(4)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Padding = New System.Windows.Forms.Padding(4)
        Me.GroupBox1.Size = New System.Drawing.Size(477, 514)
        Me.GroupBox1.TabIndex = 37
        Me.GroupBox1.TabStop = False
        '
        'txtStockMaxProfitPerDay
        '
        Me.txtStockMaxProfitPerDay.Location = New System.Drawing.Point(198, 443)
        Me.txtStockMaxProfitPerDay.Margin = New System.Windows.Forms.Padding(4)
        Me.txtStockMaxProfitPerDay.Name = "txtStockMaxProfitPerDay"
        Me.txtStockMaxProfitPerDay.Size = New System.Drawing.Size(272, 22)
        Me.txtStockMaxProfitPerDay.TabIndex = 11
        '
        'lblStockMaxProfitPerDay
        '
        Me.lblStockMaxProfitPerDay.AutoSize = True
        Me.lblStockMaxProfitPerDay.Location = New System.Drawing.Point(9, 446)
        Me.lblStockMaxProfitPerDay.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblStockMaxProfitPerDay.Name = "lblStockMaxProfitPerDay"
        Me.lblStockMaxProfitPerDay.Size = New System.Drawing.Size(164, 17)
        Me.lblStockMaxProfitPerDay.TabIndex = 47
        Me.lblStockMaxProfitPerDay.Text = "Stock Max Profit Per Day"
        '
        'txtStockMaxLossPerDay
        '
        Me.txtStockMaxLossPerDay.Location = New System.Drawing.Point(198, 408)
        Me.txtStockMaxLossPerDay.Margin = New System.Windows.Forms.Padding(4)
        Me.txtStockMaxLossPerDay.Name = "txtStockMaxLossPerDay"
        Me.txtStockMaxLossPerDay.Size = New System.Drawing.Size(271, 22)
        Me.txtStockMaxLossPerDay.TabIndex = 10
        '
        'lblStockMaxLossPerDay
        '
        Me.lblStockMaxLossPerDay.AutoSize = True
        Me.lblStockMaxLossPerDay.Location = New System.Drawing.Point(9, 412)
        Me.lblStockMaxLossPerDay.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblStockMaxLossPerDay.Name = "lblStockMaxLossPerDay"
        Me.lblStockMaxLossPerDay.Size = New System.Drawing.Size(161, 17)
        Me.lblStockMaxLossPerDay.TabIndex = 46
        Me.lblStockMaxLossPerDay.Text = "Stock Max Loss Per Day"
        '
        'txtOverallMaxProfitPerDay
        '
        Me.txtOverallMaxProfitPerDay.Location = New System.Drawing.Point(198, 372)
        Me.txtOverallMaxProfitPerDay.Margin = New System.Windows.Forms.Padding(4)
        Me.txtOverallMaxProfitPerDay.Name = "txtOverallMaxProfitPerDay"
        Me.txtOverallMaxProfitPerDay.Size = New System.Drawing.Size(272, 22)
        Me.txtOverallMaxProfitPerDay.TabIndex = 9
        '
        'lblOverallMaxProfitPerDay
        '
        Me.lblOverallMaxProfitPerDay.AutoSize = True
        Me.lblOverallMaxProfitPerDay.Location = New System.Drawing.Point(9, 375)
        Me.lblOverallMaxProfitPerDay.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblOverallMaxProfitPerDay.Name = "lblOverallMaxProfitPerDay"
        Me.lblOverallMaxProfitPerDay.Size = New System.Drawing.Size(174, 17)
        Me.lblOverallMaxProfitPerDay.TabIndex = 43
        Me.lblOverallMaxProfitPerDay.Text = "Overall Max Profit Per Day"
        '
        'txtOverallMaxLossPerDay
        '
        Me.txtOverallMaxLossPerDay.Location = New System.Drawing.Point(198, 337)
        Me.txtOverallMaxLossPerDay.Margin = New System.Windows.Forms.Padding(4)
        Me.txtOverallMaxLossPerDay.Name = "txtOverallMaxLossPerDay"
        Me.txtOverallMaxLossPerDay.Size = New System.Drawing.Size(272, 22)
        Me.txtOverallMaxLossPerDay.TabIndex = 8
        '
        'lblOverallMaxLossPerDay
        '
        Me.lblOverallMaxLossPerDay.AutoSize = True
        Me.lblOverallMaxLossPerDay.Location = New System.Drawing.Point(9, 341)
        Me.lblOverallMaxLossPerDay.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblOverallMaxLossPerDay.Name = "lblOverallMaxLossPerDay"
        Me.lblOverallMaxLossPerDay.Size = New System.Drawing.Size(171, 17)
        Me.lblOverallMaxLossPerDay.TabIndex = 42
        Me.lblOverallMaxLossPerDay.Text = "Overall Max Loss Per Day"
        '
        'txtNumberOfTradePerStock
        '
        Me.txtNumberOfTradePerStock.Location = New System.Drawing.Point(198, 301)
        Me.txtNumberOfTradePerStock.Margin = New System.Windows.Forms.Padding(4)
        Me.txtNumberOfTradePerStock.Name = "txtNumberOfTradePerStock"
        Me.txtNumberOfTradePerStock.Size = New System.Drawing.Size(272, 22)
        Me.txtNumberOfTradePerStock.TabIndex = 7
        Me.txtNumberOfTradePerStock.Tag = "Max Loss Per Day"
        '
        'lblNumberOfTradePerStock
        '
        Me.lblNumberOfTradePerStock.AutoSize = True
        Me.lblNumberOfTradePerStock.Location = New System.Drawing.Point(8, 305)
        Me.lblNumberOfTradePerStock.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblNumberOfTradePerStock.Name = "lblNumberOfTradePerStock"
        Me.lblNumberOfTradePerStock.Size = New System.Drawing.Size(184, 17)
        Me.lblNumberOfTradePerStock.TabIndex = 39
        Me.lblNumberOfTradePerStock.Text = "Number Of Trade Per Stock"
        '
        'txtTargetMultiplier
        '
        Me.txtTargetMultiplier.Location = New System.Drawing.Point(199, 162)
        Me.txtTargetMultiplier.Margin = New System.Windows.Forms.Padding(4)
        Me.txtTargetMultiplier.Name = "txtTargetMultiplier"
        Me.txtTargetMultiplier.Size = New System.Drawing.Size(270, 22)
        Me.txtTargetMultiplier.TabIndex = 6
        Me.txtTargetMultiplier.Tag = "Target Multiplier"
        '
        'lblTargetMultiplier
        '
        Me.lblTargetMultiplier.AutoSize = True
        Me.lblTargetMultiplier.Location = New System.Drawing.Point(9, 166)
        Me.lblTargetMultiplier.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblTargetMultiplier.Name = "lblTargetMultiplier"
        Me.lblTargetMultiplier.Size = New System.Drawing.Size(110, 17)
        Me.lblTargetMultiplier.TabIndex = 31
        Me.lblTargetMultiplier.Text = "Target Multiplier"
        '
        'dtpckrEODExitTime
        '
        Me.dtpckrEODExitTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpckrEODExitTime.Location = New System.Drawing.Point(199, 127)
        Me.dtpckrEODExitTime.Name = "dtpckrEODExitTime"
        Me.dtpckrEODExitTime.ShowUpDown = True
        Me.dtpckrEODExitTime.Size = New System.Drawing.Size(270, 22)
        Me.dtpckrEODExitTime.TabIndex = 4
        Me.dtpckrEODExitTime.Value = New Date(2019, 3, 12, 0, 0, 0, 0)
        '
        'dtpckrLastTradeEntryTime
        '
        Me.dtpckrLastTradeEntryTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpckrLastTradeEntryTime.Location = New System.Drawing.Point(200, 92)
        Me.dtpckrLastTradeEntryTime.Name = "dtpckrLastTradeEntryTime"
        Me.dtpckrLastTradeEntryTime.ShowUpDown = True
        Me.dtpckrLastTradeEntryTime.Size = New System.Drawing.Size(269, 22)
        Me.dtpckrLastTradeEntryTime.TabIndex = 3
        Me.dtpckrLastTradeEntryTime.Value = New Date(2019, 3, 12, 0, 0, 0, 0)
        '
        'dtpckrTradeStartTime
        '
        Me.dtpckrTradeStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpckrTradeStartTime.Location = New System.Drawing.Point(199, 55)
        Me.dtpckrTradeStartTime.Name = "dtpckrTradeStartTime"
        Me.dtpckrTradeStartTime.ShowUpDown = True
        Me.dtpckrTradeStartTime.Size = New System.Drawing.Size(270, 22)
        Me.dtpckrTradeStartTime.TabIndex = 2
        Me.dtpckrTradeStartTime.Value = New Date(2019, 3, 12, 0, 0, 0, 0)
        '
        'lblEODExitTime
        '
        Me.lblEODExitTime.AutoSize = True
        Me.lblEODExitTime.Location = New System.Drawing.Point(9, 128)
        Me.lblEODExitTime.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblEODExitTime.Name = "lblEODExitTime"
        Me.lblEODExitTime.Size = New System.Drawing.Size(99, 17)
        Me.lblEODExitTime.TabIndex = 23
        Me.lblEODExitTime.Text = "EOD Exit Time"
        '
        'lblLastTradeEntryTime
        '
        Me.lblLastTradeEntryTime.AutoSize = True
        Me.lblLastTradeEntryTime.Location = New System.Drawing.Point(9, 93)
        Me.lblLastTradeEntryTime.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblLastTradeEntryTime.Name = "lblLastTradeEntryTime"
        Me.lblLastTradeEntryTime.Size = New System.Drawing.Size(149, 17)
        Me.lblLastTradeEntryTime.TabIndex = 21
        Me.lblLastTradeEntryTime.Text = "Last Trade Entry Time"
        '
        'lblTradeStartTime
        '
        Me.lblTradeStartTime.AutoSize = True
        Me.lblTradeStartTime.Location = New System.Drawing.Point(9, 57)
        Me.lblTradeStartTime.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblTradeStartTime.Name = "lblTradeStartTime"
        Me.lblTradeStartTime.Size = New System.Drawing.Size(115, 17)
        Me.lblTradeStartTime.TabIndex = 19
        Me.lblTradeStartTime.Text = "Trade Start Time"
        '
        'btnBrowse
        '
        Me.btnBrowse.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnBrowse.Location = New System.Drawing.Point(428, 477)
        Me.btnBrowse.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnBrowse.Name = "btnBrowse"
        Me.btnBrowse.Size = New System.Drawing.Size(42, 23)
        Me.btnBrowse.TabIndex = 12
        Me.btnBrowse.Text = "..."
        Me.btnBrowse.UseVisualStyleBackColor = True
        '
        'txtInstrumentDetalis
        '
        Me.txtInstrumentDetalis.Location = New System.Drawing.Point(198, 478)
        Me.txtInstrumentDetalis.Margin = New System.Windows.Forms.Padding(4)
        Me.txtInstrumentDetalis.Name = "txtInstrumentDetalis"
        Me.txtInstrumentDetalis.ReadOnly = True
        Me.txtInstrumentDetalis.Size = New System.Drawing.Size(223, 22)
        Me.txtInstrumentDetalis.TabIndex = 15
        '
        'lblInstrumentDetails
        '
        Me.lblInstrumentDetails.AutoSize = True
        Me.lblInstrumentDetails.Location = New System.Drawing.Point(8, 481)
        Me.lblInstrumentDetails.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblInstrumentDetails.Name = "lblInstrumentDetails"
        Me.lblInstrumentDetails.Size = New System.Drawing.Size(121, 17)
        Me.lblInstrumentDetails.TabIndex = 8
        Me.lblInstrumentDetails.Text = "Instrument Details"
        '
        'txtSignalTimeFrame
        '
        Me.txtSignalTimeFrame.Location = New System.Drawing.Point(199, 18)
        Me.txtSignalTimeFrame.Margin = New System.Windows.Forms.Padding(4)
        Me.txtSignalTimeFrame.Name = "txtSignalTimeFrame"
        Me.txtSignalTimeFrame.Size = New System.Drawing.Size(270, 22)
        Me.txtSignalTimeFrame.TabIndex = 1
        Me.txtSignalTimeFrame.Tag = "Signal Time Frame"
        '
        'lblSignalTimeFrame
        '
        Me.lblSignalTimeFrame.AutoSize = True
        Me.lblSignalTimeFrame.Location = New System.Drawing.Point(9, 21)
        Me.lblSignalTimeFrame.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblSignalTimeFrame.Name = "lblSignalTimeFrame"
        Me.lblSignalTimeFrame.Size = New System.Drawing.Size(158, 17)
        Me.lblSignalTimeFrame.TabIndex = 3
        Me.lblSignalTimeFrame.Text = "Signal Time Frame(min)"
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.grbInstrumentType)
        Me.GroupBox2.Controls.Add(Me.chbAutoSelectStock)
        Me.GroupBox2.Location = New System.Drawing.Point(487, -5)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(358, 87)
        Me.GroupBox2.TabIndex = 38
        Me.GroupBox2.TabStop = False
        '
        'btnLowSLStrategySettings
        '
        Me.btnLowSLStrategySettings.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnLowSLStrategySettings.ImageKey = "save-icon-36533.png"
        Me.btnLowSLStrategySettings.ImageList = Me.ImageList1
        Me.btnLowSLStrategySettings.Location = New System.Drawing.Point(733, 450)
        Me.btnLowSLStrategySettings.Margin = New System.Windows.Forms.Padding(4)
        Me.btnLowSLStrategySettings.Name = "btnLowSLStrategySettings"
        Me.btnLowSLStrategySettings.Size = New System.Drawing.Size(112, 58)
        Me.btnLowSLStrategySettings.TabIndex = 36
        Me.btnLowSLStrategySettings.Text = "&Save"
        Me.btnLowSLStrategySettings.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.btnLowSLStrategySettings.UseVisualStyleBackColor = True
        '
        'txtMaxLossPerTrade
        '
        Me.txtMaxLossPerTrade.Location = New System.Drawing.Point(198, 267)
        Me.txtMaxLossPerTrade.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMaxLossPerTrade.Name = "txtMaxLossPerTrade"
        Me.txtMaxLossPerTrade.Size = New System.Drawing.Size(272, 22)
        Me.txtMaxLossPerTrade.TabIndex = 50
        '
        'lblMaxLossPerTrade
        '
        Me.lblMaxLossPerTrade.AutoSize = True
        Me.lblMaxLossPerTrade.Location = New System.Drawing.Point(9, 270)
        Me.lblMaxLossPerTrade.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMaxLossPerTrade.Name = "lblMaxLossPerTrade"
        Me.lblMaxLossPerTrade.Size = New System.Drawing.Size(135, 17)
        Me.lblMaxLossPerTrade.TabIndex = 53
        Me.lblMaxLossPerTrade.Text = "Max Loss Per Trade"
        '
        'txtMinLossPerTrade
        '
        Me.txtMinLossPerTrade.Location = New System.Drawing.Point(198, 232)
        Me.txtMinLossPerTrade.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMinLossPerTrade.Name = "txtMinLossPerTrade"
        Me.txtMinLossPerTrade.Size = New System.Drawing.Size(272, 22)
        Me.txtMinLossPerTrade.TabIndex = 49
        '
        'lblMinLossPerTrade
        '
        Me.lblMinLossPerTrade.AutoSize = True
        Me.lblMinLossPerTrade.Location = New System.Drawing.Point(9, 236)
        Me.lblMinLossPerTrade.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMinLossPerTrade.Name = "lblMinLossPerTrade"
        Me.lblMinLossPerTrade.Size = New System.Drawing.Size(132, 17)
        Me.lblMinLossPerTrade.TabIndex = 52
        Me.lblMinLossPerTrade.Text = "Min Loss Per Trade"
        '
        'txtMinInvestmentPerStock
        '
        Me.txtMinInvestmentPerStock.Location = New System.Drawing.Point(198, 196)
        Me.txtMinInvestmentPerStock.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMinInvestmentPerStock.Name = "txtMinInvestmentPerStock"
        Me.txtMinInvestmentPerStock.Size = New System.Drawing.Size(272, 22)
        Me.txtMinInvestmentPerStock.TabIndex = 48
        Me.txtMinInvestmentPerStock.Tag = "Max Loss Per Day"
        '
        'lblMinInvestmentPerStock
        '
        Me.lblMinInvestmentPerStock.AutoSize = True
        Me.lblMinInvestmentPerStock.Location = New System.Drawing.Point(8, 200)
        Me.lblMinInvestmentPerStock.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMinInvestmentPerStock.Name = "lblMinInvestmentPerStock"
        Me.lblMinInvestmentPerStock.Size = New System.Drawing.Size(167, 17)
        Me.lblMinInvestmentPerStock.TabIndex = 51
        Me.lblMinInvestmentPerStock.Text = "Min Investment Per Stock"
        '
        'frmLowSLSettings
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(852, 510)
        Me.Controls.Add(Me.GroupBox3)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.btnLowSLStrategySettings)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmLowSLSettings"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Low SL Strategy - Settings"
        Me.grbInstrumentType.ResumeLayout(False)
        Me.grbInstrumentType.PerformLayout()
        Me.GroupBox3.ResumeLayout(False)
        Me.GroupBox3.PerformLayout()
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents txtMinPrice As TextBox
    Friend WithEvents rdbFuture As RadioButton
    Friend WithEvents rdbCash As RadioButton
    Friend WithEvents chbAutoSelectStock As CheckBox
    Friend WithEvents ImageList1 As ImageList
    Friend WithEvents grbInstrumentType As GroupBox
    Friend WithEvents opnFileSettings As OpenFileDialog
    Friend WithEvents txtNumberOfStock As TextBox
    Friend WithEvents lblNumberOfStock As Label
    Friend WithEvents txtATRPercentage As TextBox
    Friend WithEvents lblATR As Label
    Friend WithEvents txtMaxPrice As TextBox
    Friend WithEvents lblMaxPrice As Label
    Friend WithEvents GroupBox3 As GroupBox
    Friend WithEvents lblMinPrice As Label
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents txtOverallMaxProfitPerDay As TextBox
    Friend WithEvents lblOverallMaxProfitPerDay As Label
    Friend WithEvents txtOverallMaxLossPerDay As TextBox
    Friend WithEvents lblOverallMaxLossPerDay As Label
    Friend WithEvents txtNumberOfTradePerStock As TextBox
    Friend WithEvents lblNumberOfTradePerStock As Label
    Friend WithEvents txtTargetMultiplier As TextBox
    Friend WithEvents lblTargetMultiplier As Label
    Friend WithEvents dtpckrEODExitTime As DateTimePicker
    Friend WithEvents dtpckrLastTradeEntryTime As DateTimePicker
    Friend WithEvents dtpckrTradeStartTime As DateTimePicker
    Friend WithEvents lblEODExitTime As Label
    Friend WithEvents lblLastTradeEntryTime As Label
    Friend WithEvents lblTradeStartTime As Label
    Friend WithEvents btnBrowse As Button
    Friend WithEvents txtInstrumentDetalis As TextBox
    Friend WithEvents lblInstrumentDetails As Label
    Friend WithEvents txtSignalTimeFrame As TextBox
    Friend WithEvents lblSignalTimeFrame As Label
    Friend WithEvents GroupBox2 As GroupBox
    Friend WithEvents btnLowSLStrategySettings As Button
    Friend WithEvents txtMaxBlankCandlePer As TextBox
    Friend WithEvents lblMaxBlankCandlePer As Label
    Friend WithEvents txtStockMaxProfitPerDay As TextBox
    Friend WithEvents lblStockMaxProfitPerDay As Label
    Friend WithEvents txtStockMaxLossPerDay As TextBox
    Friend WithEvents lblStockMaxLossPerDay As Label
    Friend WithEvents txtMaxLossPerTrade As TextBox
    Friend WithEvents lblMaxLossPerTrade As Label
    Friend WithEvents txtMinLossPerTrade As TextBox
    Friend WithEvents lblMinLossPerTrade As Label
    Friend WithEvents txtMinInvestmentPerStock As TextBox
    Friend WithEvents lblMinInvestmentPerStock As Label
End Class
