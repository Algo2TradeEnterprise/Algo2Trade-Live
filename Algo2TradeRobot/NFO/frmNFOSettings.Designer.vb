﻿<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
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
        Me.chkbAutoSelectStock = New System.Windows.Forms.CheckBox()
        Me.grpStockSelection = New System.Windows.Forms.GroupBox()
        Me.txtMaxBlankCandlePer = New System.Windows.Forms.TextBox()
        Me.lblMaxBlankCandlePer = New System.Windows.Forms.Label()
        Me.txtATRPercentage = New System.Windows.Forms.TextBox()
        Me.lblATR = New System.Windows.Forms.Label()
        Me.txtMaxPrice = New System.Windows.Forms.TextBox()
        Me.lblMaxPrice = New System.Windows.Forms.Label()
        Me.txtMinPrice = New System.Windows.Forms.TextBox()
        Me.lblMinPrice = New System.Windows.Forms.Label()
        Me.lblSignalTimeFrame = New System.Windows.Forms.Label()
        Me.txtSignalTimeFrame = New System.Windows.Forms.TextBox()
        Me.lblInstrumentDetails = New System.Windows.Forms.Label()
        Me.txtInstrumentDetalis = New System.Windows.Forms.TextBox()
        Me.btnBrowse = New System.Windows.Forms.Button()
        Me.lblTradeStartTime = New System.Windows.Forms.Label()
        Me.lblLastTradeEntryTime = New System.Windows.Forms.Label()
        Me.lblEODExitTime = New System.Windows.Forms.Label()
        Me.dtpckrTradeStartTime = New System.Windows.Forms.DateTimePicker()
        Me.dtpckrLastTradeEntryTime = New System.Windows.Forms.DateTimePicker()
        Me.dtpckrEODExitTime = New System.Windows.Forms.DateTimePicker()
        Me.lblNumberOfTradePerStock = New System.Windows.Forms.Label()
        Me.txtNumberOfTradePerStock = New System.Windows.Forms.TextBox()
        Me.lblOverallMaxLossPerDay = New System.Windows.Forms.Label()
        Me.txtOverallMaxLossPerDay = New System.Windows.Forms.TextBox()
        Me.lblOverallMaxProfitPerDay = New System.Windows.Forms.Label()
        Me.txtOverallMaxProfitPerDay = New System.Windows.Forms.TextBox()
        Me.lblMaxProfitPerTrade = New System.Windows.Forms.Label()
        Me.txtMaxProfitPerTrade = New System.Windows.Forms.TextBox()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.txtMinDistancePercentageForCancellation = New System.Windows.Forms.TextBox()
        Me.lblMinDistancePercentageForCancellation = New System.Windows.Forms.Label()
        Me.txtMaxTurnoverOfATrade = New System.Windows.Forms.TextBox()
        Me.lblMaxTurnoverOfATrade = New System.Windows.Forms.Label()
        Me.txtNumberOfStock = New System.Windows.Forms.TextBox()
        Me.lblNumberOfStock = New System.Windows.Forms.Label()
        Me.txtMaxTargetToStoplossMultiplier = New System.Windows.Forms.TextBox()
        Me.lblMaxTargetToStoplossMultiplier = New System.Windows.Forms.Label()
        Me.grpStockSelection.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnSave
        '
        Me.btnSave.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnSave.ImageKey = "save-icon-36533.png"
        Me.btnSave.ImageList = Me.ImageList1
        Me.btnSave.Location = New System.Drawing.Point(797, 407)
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
        'chkbAutoSelectStock
        '
        Me.chkbAutoSelectStock.AutoSize = True
        Me.chkbAutoSelectStock.Location = New System.Drawing.Point(551, 16)
        Me.chkbAutoSelectStock.Name = "chkbAutoSelectStock"
        Me.chkbAutoSelectStock.Size = New System.Drawing.Size(141, 21)
        Me.chkbAutoSelectStock.TabIndex = 12
        Me.chkbAutoSelectStock.Text = "Auto Select Stock"
        Me.chkbAutoSelectStock.UseVisualStyleBackColor = True
        '
        'grpStockSelection
        '
        Me.grpStockSelection.Controls.Add(Me.txtMaxBlankCandlePer)
        Me.grpStockSelection.Controls.Add(Me.lblMaxBlankCandlePer)
        Me.grpStockSelection.Controls.Add(Me.txtATRPercentage)
        Me.grpStockSelection.Controls.Add(Me.lblATR)
        Me.grpStockSelection.Controls.Add(Me.txtMaxPrice)
        Me.grpStockSelection.Controls.Add(Me.lblMaxPrice)
        Me.grpStockSelection.Controls.Add(Me.txtMinPrice)
        Me.grpStockSelection.Controls.Add(Me.lblMinPrice)
        Me.grpStockSelection.Location = New System.Drawing.Point(551, 46)
        Me.grpStockSelection.Name = "grpStockSelection"
        Me.grpStockSelection.Size = New System.Drawing.Size(358, 167)
        Me.grpStockSelection.TabIndex = 46
        Me.grpStockSelection.TabStop = False
        Me.grpStockSelection.Text = "Stock Selection Settings"
        '
        'txtMaxBlankCandlePer
        '
        Me.txtMaxBlankCandlePer.Location = New System.Drawing.Point(146, 130)
        Me.txtMaxBlankCandlePer.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMaxBlankCandlePer.Name = "txtMaxBlankCandlePer"
        Me.txtMaxBlankCandlePer.Size = New System.Drawing.Size(201, 22)
        Me.txtMaxBlankCandlePer.TabIndex = 16
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
        'txtATRPercentage
        '
        Me.txtATRPercentage.Location = New System.Drawing.Point(146, 95)
        Me.txtATRPercentage.Margin = New System.Windows.Forms.Padding(4)
        Me.txtATRPercentage.Name = "txtATRPercentage"
        Me.txtATRPercentage.Size = New System.Drawing.Size(201, 22)
        Me.txtATRPercentage.TabIndex = 15
        Me.txtATRPercentage.Tag = "ATR %"
        '
        'lblATR
        '
        Me.lblATR.AutoSize = True
        Me.lblATR.Location = New System.Drawing.Point(9, 98)
        Me.lblATR.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblATR.Name = "lblATR"
        Me.lblATR.Size = New System.Drawing.Size(78, 17)
        Me.lblATR.TabIndex = 39
        Me.lblATR.Text = "Min ATR %"
        '
        'txtMaxPrice
        '
        Me.txtMaxPrice.Location = New System.Drawing.Point(146, 59)
        Me.txtMaxPrice.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMaxPrice.Name = "txtMaxPrice"
        Me.txtMaxPrice.Size = New System.Drawing.Size(201, 22)
        Me.txtMaxPrice.TabIndex = 14
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
        'txtMinPrice
        '
        Me.txtMinPrice.Location = New System.Drawing.Point(146, 25)
        Me.txtMinPrice.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMinPrice.Name = "txtMinPrice"
        Me.txtMinPrice.Size = New System.Drawing.Size(201, 22)
        Me.txtMinPrice.TabIndex = 13
        Me.txtMinPrice.Tag = "Min Price"
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
        'lblInstrumentDetails
        '
        Me.lblInstrumentDetails.AutoSize = True
        Me.lblInstrumentDetails.Location = New System.Drawing.Point(8, 338)
        Me.lblInstrumentDetails.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblInstrumentDetails.Name = "lblInstrumentDetails"
        Me.lblInstrumentDetails.Size = New System.Drawing.Size(121, 17)
        Me.lblInstrumentDetails.TabIndex = 8
        Me.lblInstrumentDetails.Text = "Instrument Details"
        '
        'txtInstrumentDetalis
        '
        Me.txtInstrumentDetalis.Location = New System.Drawing.Point(285, 335)
        Me.txtInstrumentDetalis.Margin = New System.Windows.Forms.Padding(4)
        Me.txtInstrumentDetalis.Name = "txtInstrumentDetalis"
        Me.txtInstrumentDetalis.ReadOnly = True
        Me.txtInstrumentDetalis.Size = New System.Drawing.Size(194, 22)
        Me.txtInstrumentDetalis.TabIndex = 10
        '
        'btnBrowse
        '
        Me.btnBrowse.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnBrowse.Location = New System.Drawing.Point(486, 334)
        Me.btnBrowse.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnBrowse.Name = "btnBrowse"
        Me.btnBrowse.Size = New System.Drawing.Size(42, 23)
        Me.btnBrowse.TabIndex = 10
        Me.btnBrowse.Text = "..."
        Me.btnBrowse.UseVisualStyleBackColor = True
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
        'lblNumberOfTradePerStock
        '
        Me.lblNumberOfTradePerStock.AutoSize = True
        Me.lblNumberOfTradePerStock.Location = New System.Drawing.Point(8, 233)
        Me.lblNumberOfTradePerStock.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblNumberOfTradePerStock.Name = "lblNumberOfTradePerStock"
        Me.lblNumberOfTradePerStock.Size = New System.Drawing.Size(184, 17)
        Me.lblNumberOfTradePerStock.TabIndex = 39
        Me.lblNumberOfTradePerStock.Text = "Number Of Trade Per Stock"
        '
        'txtNumberOfTradePerStock
        '
        Me.txtNumberOfTradePerStock.Location = New System.Drawing.Point(285, 229)
        Me.txtNumberOfTradePerStock.Margin = New System.Windows.Forms.Padding(4)
        Me.txtNumberOfTradePerStock.Name = "txtNumberOfTradePerStock"
        Me.txtNumberOfTradePerStock.Size = New System.Drawing.Size(243, 22)
        Me.txtNumberOfTradePerStock.TabIndex = 7
        Me.txtNumberOfTradePerStock.Tag = "Max Loss Per Day"
        '
        'lblOverallMaxLossPerDay
        '
        Me.lblOverallMaxLossPerDay.AutoSize = True
        Me.lblOverallMaxLossPerDay.Location = New System.Drawing.Point(9, 269)
        Me.lblOverallMaxLossPerDay.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblOverallMaxLossPerDay.Name = "lblOverallMaxLossPerDay"
        Me.lblOverallMaxLossPerDay.Size = New System.Drawing.Size(171, 17)
        Me.lblOverallMaxLossPerDay.TabIndex = 42
        Me.lblOverallMaxLossPerDay.Text = "Overall Max Loss Per Day"
        '
        'txtOverallMaxLossPerDay
        '
        Me.txtOverallMaxLossPerDay.Location = New System.Drawing.Point(285, 265)
        Me.txtOverallMaxLossPerDay.Margin = New System.Windows.Forms.Padding(4)
        Me.txtOverallMaxLossPerDay.Name = "txtOverallMaxLossPerDay"
        Me.txtOverallMaxLossPerDay.Size = New System.Drawing.Size(243, 22)
        Me.txtOverallMaxLossPerDay.TabIndex = 8
        '
        'lblOverallMaxProfitPerDay
        '
        Me.lblOverallMaxProfitPerDay.AutoSize = True
        Me.lblOverallMaxProfitPerDay.Location = New System.Drawing.Point(9, 303)
        Me.lblOverallMaxProfitPerDay.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblOverallMaxProfitPerDay.Name = "lblOverallMaxProfitPerDay"
        Me.lblOverallMaxProfitPerDay.Size = New System.Drawing.Size(174, 17)
        Me.lblOverallMaxProfitPerDay.TabIndex = 43
        Me.lblOverallMaxProfitPerDay.Text = "Overall Max Profit Per Day"
        '
        'txtOverallMaxProfitPerDay
        '
        Me.txtOverallMaxProfitPerDay.Location = New System.Drawing.Point(285, 300)
        Me.txtOverallMaxProfitPerDay.Margin = New System.Windows.Forms.Padding(4)
        Me.txtOverallMaxProfitPerDay.Name = "txtOverallMaxProfitPerDay"
        Me.txtOverallMaxProfitPerDay.Size = New System.Drawing.Size(243, 22)
        Me.txtOverallMaxProfitPerDay.TabIndex = 9
        '
        'lblMaxProfitPerTrade
        '
        Me.lblMaxProfitPerTrade.AutoSize = True
        Me.lblMaxProfitPerTrade.Location = New System.Drawing.Point(9, 198)
        Me.lblMaxProfitPerTrade.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMaxProfitPerTrade.Name = "lblMaxProfitPerTrade"
        Me.lblMaxProfitPerTrade.Size = New System.Drawing.Size(138, 17)
        Me.lblMaxProfitPerTrade.TabIndex = 53
        Me.lblMaxProfitPerTrade.Text = "Max Profit Per Trade"
        '
        'txtMaxProfitPerTrade
        '
        Me.txtMaxProfitPerTrade.Location = New System.Drawing.Point(285, 195)
        Me.txtMaxProfitPerTrade.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMaxProfitPerTrade.Name = "txtMaxProfitPerTrade"
        Me.txtMaxProfitPerTrade.Size = New System.Drawing.Size(243, 22)
        Me.txtMaxProfitPerTrade.TabIndex = 6
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.txtMinDistancePercentageForCancellation)
        Me.GroupBox1.Controls.Add(Me.lblMinDistancePercentageForCancellation)
        Me.GroupBox1.Controls.Add(Me.txtMaxTurnoverOfATrade)
        Me.GroupBox1.Controls.Add(Me.lblMaxTurnoverOfATrade)
        Me.GroupBox1.Controls.Add(Me.txtNumberOfStock)
        Me.GroupBox1.Controls.Add(Me.lblNumberOfStock)
        Me.GroupBox1.Controls.Add(Me.txtMaxTargetToStoplossMultiplier)
        Me.GroupBox1.Controls.Add(Me.lblMaxTargetToStoplossMultiplier)
        Me.GroupBox1.Controls.Add(Me.txtMaxProfitPerTrade)
        Me.GroupBox1.Controls.Add(Me.lblMaxProfitPerTrade)
        Me.GroupBox1.Controls.Add(Me.txtOverallMaxProfitPerDay)
        Me.GroupBox1.Controls.Add(Me.lblOverallMaxProfitPerDay)
        Me.GroupBox1.Controls.Add(Me.txtOverallMaxLossPerDay)
        Me.GroupBox1.Controls.Add(Me.lblOverallMaxLossPerDay)
        Me.GroupBox1.Controls.Add(Me.txtNumberOfTradePerStock)
        Me.GroupBox1.Controls.Add(Me.lblNumberOfTradePerStock)
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
        Me.GroupBox1.Location = New System.Drawing.Point(5, -2)
        Me.GroupBox1.Margin = New System.Windows.Forms.Padding(4)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Padding = New System.Windows.Forms.Padding(4)
        Me.GroupBox1.Size = New System.Drawing.Size(539, 467)
        Me.GroupBox1.TabIndex = 44
        Me.GroupBox1.TabStop = False
        '
        'txtMinDistancePercentageForCancellation
        '
        Me.txtMinDistancePercentageForCancellation.Location = New System.Drawing.Point(285, 436)
        Me.txtMinDistancePercentageForCancellation.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMinDistancePercentageForCancellation.Name = "txtMinDistancePercentageForCancellation"
        Me.txtMinDistancePercentageForCancellation.Size = New System.Drawing.Size(242, 22)
        Me.txtMinDistancePercentageForCancellation.TabIndex = 58
        Me.txtMinDistancePercentageForCancellation.Tag = "Min Distance % For Cancellation"
        '
        'lblMinDistancePercentageForCancellation
        '
        Me.lblMinDistancePercentageForCancellation.AutoSize = True
        Me.lblMinDistancePercentageForCancellation.Location = New System.Drawing.Point(8, 439)
        Me.lblMinDistancePercentageForCancellation.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMinDistancePercentageForCancellation.Name = "lblMinDistancePercentageForCancellation"
        Me.lblMinDistancePercentageForCancellation.Size = New System.Drawing.Size(211, 17)
        Me.lblMinDistancePercentageForCancellation.TabIndex = 59
        Me.lblMinDistancePercentageForCancellation.Text = "Min Distance % For Cancellation"
        '
        'txtMaxTurnoverOfATrade
        '
        Me.txtMaxTurnoverOfATrade.Location = New System.Drawing.Point(285, 402)
        Me.txtMaxTurnoverOfATrade.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMaxTurnoverOfATrade.Name = "txtMaxTurnoverOfATrade"
        Me.txtMaxTurnoverOfATrade.Size = New System.Drawing.Size(242, 22)
        Me.txtMaxTurnoverOfATrade.TabIndex = 56
        Me.txtMaxTurnoverOfATrade.Tag = "Max Turnover Of A Trade"
        '
        'lblMaxTurnoverOfATrade
        '
        Me.lblMaxTurnoverOfATrade.AutoSize = True
        Me.lblMaxTurnoverOfATrade.Location = New System.Drawing.Point(8, 405)
        Me.lblMaxTurnoverOfATrade.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMaxTurnoverOfATrade.Name = "lblMaxTurnoverOfATrade"
        Me.lblMaxTurnoverOfATrade.Size = New System.Drawing.Size(169, 17)
        Me.lblMaxTurnoverOfATrade.TabIndex = 57
        Me.lblMaxTurnoverOfATrade.Text = "Max Turnover Of A Trade"
        '
        'txtNumberOfStock
        '
        Me.txtNumberOfStock.Location = New System.Drawing.Point(285, 368)
        Me.txtNumberOfStock.Margin = New System.Windows.Forms.Padding(4)
        Me.txtNumberOfStock.Name = "txtNumberOfStock"
        Me.txtNumberOfStock.Size = New System.Drawing.Size(242, 22)
        Me.txtNumberOfStock.TabIndex = 11
        Me.txtNumberOfStock.Tag = "Number Of Stock"
        '
        'lblNumberOfStock
        '
        Me.lblNumberOfStock.AutoSize = True
        Me.lblNumberOfStock.Location = New System.Drawing.Point(8, 371)
        Me.lblNumberOfStock.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblNumberOfStock.Name = "lblNumberOfStock"
        Me.lblNumberOfStock.Size = New System.Drawing.Size(116, 17)
        Me.lblNumberOfStock.TabIndex = 48
        Me.lblNumberOfStock.Text = "Number Of Stock"
        '
        'txtMaxTargetToStoplossMultiplier
        '
        Me.txtMaxTargetToStoplossMultiplier.Location = New System.Drawing.Point(285, 161)
        Me.txtMaxTargetToStoplossMultiplier.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMaxTargetToStoplossMultiplier.Name = "txtMaxTargetToStoplossMultiplier"
        Me.txtMaxTargetToStoplossMultiplier.Size = New System.Drawing.Size(244, 22)
        Me.txtMaxTargetToStoplossMultiplier.TabIndex = 5
        '
        'lblMaxTargetToStoplossMultiplier
        '
        Me.lblMaxTargetToStoplossMultiplier.AutoSize = True
        Me.lblMaxTargetToStoplossMultiplier.Location = New System.Drawing.Point(10, 164)
        Me.lblMaxTargetToStoplossMultiplier.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMaxTargetToStoplossMultiplier.Name = "lblMaxTargetToStoplossMultiplier"
        Me.lblMaxTargetToStoplossMultiplier.Size = New System.Drawing.Size(218, 17)
        Me.lblMaxTargetToStoplossMultiplier.TabIndex = 55
        Me.lblMaxTargetToStoplossMultiplier.Text = "Max Target To Stoploss Multiplier"
        '
        'frmNFOSettings
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(913, 469)
        Me.Controls.Add(Me.chkbAutoSelectStock)
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
        Me.PerformLayout()

    End Sub

    Friend WithEvents btnSave As Button
    Friend WithEvents ImageList1 As ImageList
    Friend WithEvents opnFileSettings As OpenFileDialog
    Friend WithEvents chkbAutoSelectStock As CheckBox
    Friend WithEvents grpStockSelection As GroupBox
    Friend WithEvents txtMaxBlankCandlePer As TextBox
    Friend WithEvents lblMaxBlankCandlePer As Label
    Friend WithEvents txtATRPercentage As TextBox
    Friend WithEvents lblATR As Label
    Friend WithEvents txtMaxPrice As TextBox
    Friend WithEvents lblMaxPrice As Label
    Friend WithEvents txtMinPrice As TextBox
    Friend WithEvents lblMinPrice As Label
    Friend WithEvents lblSignalTimeFrame As Label
    Friend WithEvents txtSignalTimeFrame As TextBox
    Friend WithEvents lblInstrumentDetails As Label
    Friend WithEvents txtInstrumentDetalis As TextBox
    Friend WithEvents btnBrowse As Button
    Friend WithEvents lblTradeStartTime As Label
    Friend WithEvents lblLastTradeEntryTime As Label
    Friend WithEvents lblEODExitTime As Label
    Friend WithEvents dtpckrTradeStartTime As DateTimePicker
    Friend WithEvents dtpckrLastTradeEntryTime As DateTimePicker
    Friend WithEvents dtpckrEODExitTime As DateTimePicker
    Friend WithEvents lblNumberOfTradePerStock As Label
    Friend WithEvents txtNumberOfTradePerStock As TextBox
    Friend WithEvents lblOverallMaxLossPerDay As Label
    Friend WithEvents txtOverallMaxLossPerDay As TextBox
    Friend WithEvents lblOverallMaxProfitPerDay As Label
    Friend WithEvents txtOverallMaxProfitPerDay As TextBox
    Friend WithEvents lblMaxProfitPerTrade As Label
    Friend WithEvents txtMaxProfitPerTrade As TextBox
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents txtMaxTargetToStoplossMultiplier As TextBox
    Friend WithEvents lblMaxTargetToStoplossMultiplier As Label
    Friend WithEvents txtNumberOfStock As TextBox
    Friend WithEvents lblNumberOfStock As Label
    Friend WithEvents txtMinDistancePercentageForCancellation As TextBox
    Friend WithEvents lblMinDistancePercentageForCancellation As Label
    Friend WithEvents txtMaxTurnoverOfATrade As TextBox
    Friend WithEvents lblMaxTurnoverOfATrade As Label
End Class
