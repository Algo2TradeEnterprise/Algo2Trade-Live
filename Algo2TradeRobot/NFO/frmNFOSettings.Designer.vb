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
        Me.grpTelegram = New System.Windows.Forms.GroupBox()
        Me.txtTelegramChatID = New System.Windows.Forms.TextBox()
        Me.lblTelegramChatID = New System.Windows.Forms.Label()
        Me.txtTelegramAPIKey = New System.Windows.Forms.TextBox()
        Me.lblTelegramAPIKey = New System.Windows.Forms.Label()
        Me.grpIndicator = New System.Windows.Forms.GroupBox()
        Me.txtRSIValue = New System.Windows.Forms.TextBox()
        Me.lblRSIValue = New System.Windows.Forms.Label()
        Me.txtCloseRSIPeriod = New System.Windows.Forms.TextBox()
        Me.lblCloseRSIPeriod = New System.Windows.Forms.Label()
        Me.txtDayCloseSMAPeriod = New System.Windows.Forms.TextBox()
        Me.lblDayCloseSMAPeriod = New System.Windows.Forms.Label()
        Me.txtVWAPEMAPeriod = New System.Windows.Forms.TextBox()
        Me.lblVWAPEMAPeriod = New System.Windows.Forms.Label()
        Me.txtDayCloseATRPeriod = New System.Windows.Forms.TextBox()
        Me.lblDayCloseATRPeriod = New System.Windows.Forms.Label()
        Me.grpOther = New System.Windows.Forms.GroupBox()
        Me.dtpckrEODExitTime = New System.Windows.Forms.DateTimePicker()
        Me.lblEODExitTime = New System.Windows.Forms.Label()
        Me.txtOverallMaxLoss = New System.Windows.Forms.TextBox()
        Me.lblOverallMaxLoss = New System.Windows.Forms.Label()
        Me.txtOverallMaxProfit = New System.Windows.Forms.TextBox()
        Me.lblOverallMaxProfit = New System.Windows.Forms.Label()
        Me.txtMaxLossPerTrade = New System.Windows.Forms.TextBox()
        Me.lblMaxLossPerTrade = New System.Windows.Forms.Label()
        Me.txtTargetMultiplier = New System.Windows.Forms.TextBox()
        Me.lblTargetMultiplier = New System.Windows.Forms.Label()
        Me.txtTargetToLeftMovementPercentage = New System.Windows.Forms.TextBox()
        Me.lblTargetToLeftMovementPercentage = New System.Windows.Forms.Label()
        Me.txtSignalTimeFrame = New System.Windows.Forms.TextBox()
        Me.lblSignalTimeFrame = New System.Windows.Forms.Label()
        Me.grpStockSelection = New System.Windows.Forms.GroupBox()
        Me.txtNumberOfStocks = New System.Windows.Forms.TextBox()
        Me.lblNumberOfStocks = New System.Windows.Forms.Label()
        Me.txtMinATRPer = New System.Windows.Forms.TextBox()
        Me.lblMinATRPer = New System.Windows.Forms.Label()
        Me.txtMinVolume = New System.Windows.Forms.TextBox()
        Me.lblMinVolume = New System.Windows.Forms.Label()
        Me.txtMaxPrice = New System.Windows.Forms.TextBox()
        Me.lblMaxPrice = New System.Windows.Forms.Label()
        Me.txtMinPrice = New System.Windows.Forms.TextBox()
        Me.lblMinPrice = New System.Windows.Forms.Label()
        Me.chkbAutoSelectStock = New System.Windows.Forms.CheckBox()
        Me.btnBrowse = New System.Windows.Forms.Button()
        Me.txtInstrumentDetalis = New System.Windows.Forms.TextBox()
        Me.lblInstrumentDetails = New System.Windows.Forms.Label()
        Me.opnFileSettings = New System.Windows.Forms.OpenFileDialog()
        Me.grpTelegram.SuspendLayout()
        Me.grpIndicator.SuspendLayout()
        Me.grpOther.SuspendLayout()
        Me.grpStockSelection.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnSave
        '
        Me.btnSave.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnSave.ImageKey = "save-icon-36533.png"
        Me.btnSave.ImageList = Me.ImageList1
        Me.btnSave.Location = New System.Drawing.Point(1032, 8)
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
        'grpTelegram
        '
        Me.grpTelegram.Controls.Add(Me.txtTelegramChatID)
        Me.grpTelegram.Controls.Add(Me.lblTelegramChatID)
        Me.grpTelegram.Controls.Add(Me.txtTelegramAPIKey)
        Me.grpTelegram.Controls.Add(Me.lblTelegramAPIKey)
        Me.grpTelegram.Location = New System.Drawing.Point(487, 3)
        Me.grpTelegram.Name = "grpTelegram"
        Me.grpTelegram.Size = New System.Drawing.Size(538, 93)
        Me.grpTelegram.TabIndex = 2
        Me.grpTelegram.TabStop = False
        Me.grpTelegram.Text = "Telegram Details"
        '
        'txtTelegramChatID
        '
        Me.txtTelegramChatID.Location = New System.Drawing.Point(178, 57)
        Me.txtTelegramChatID.Margin = New System.Windows.Forms.Padding(4)
        Me.txtTelegramChatID.Name = "txtTelegramChatID"
        Me.txtTelegramChatID.Size = New System.Drawing.Size(348, 22)
        Me.txtTelegramChatID.TabIndex = 1
        Me.txtTelegramChatID.Tag = "Telegram Chat ID"
        '
        'lblTelegramChatID
        '
        Me.lblTelegramChatID.AutoSize = True
        Me.lblTelegramChatID.Location = New System.Drawing.Point(9, 60)
        Me.lblTelegramChatID.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblTelegramChatID.Name = "lblTelegramChatID"
        Me.lblTelegramChatID.Size = New System.Drawing.Size(54, 17)
        Me.lblTelegramChatID.TabIndex = 61
        Me.lblTelegramChatID.Text = "Chat ID"
        '
        'txtTelegramAPIKey
        '
        Me.txtTelegramAPIKey.Location = New System.Drawing.Point(178, 26)
        Me.txtTelegramAPIKey.Margin = New System.Windows.Forms.Padding(4)
        Me.txtTelegramAPIKey.Name = "txtTelegramAPIKey"
        Me.txtTelegramAPIKey.Size = New System.Drawing.Size(348, 22)
        Me.txtTelegramAPIKey.TabIndex = 0
        Me.txtTelegramAPIKey.Tag = "Telegram API Key"
        '
        'lblTelegramAPIKey
        '
        Me.lblTelegramAPIKey.AutoSize = True
        Me.lblTelegramAPIKey.Location = New System.Drawing.Point(10, 29)
        Me.lblTelegramAPIKey.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblTelegramAPIKey.Name = "lblTelegramAPIKey"
        Me.lblTelegramAPIKey.Size = New System.Drawing.Size(57, 17)
        Me.lblTelegramAPIKey.TabIndex = 60
        Me.lblTelegramAPIKey.Text = "API Key"
        '
        'grpIndicator
        '
        Me.grpIndicator.Controls.Add(Me.txtRSIValue)
        Me.grpIndicator.Controls.Add(Me.lblRSIValue)
        Me.grpIndicator.Controls.Add(Me.txtCloseRSIPeriod)
        Me.grpIndicator.Controls.Add(Me.lblCloseRSIPeriod)
        Me.grpIndicator.Controls.Add(Me.txtDayCloseSMAPeriod)
        Me.grpIndicator.Controls.Add(Me.lblDayCloseSMAPeriod)
        Me.grpIndicator.Controls.Add(Me.txtVWAPEMAPeriod)
        Me.grpIndicator.Controls.Add(Me.lblVWAPEMAPeriod)
        Me.grpIndicator.Controls.Add(Me.txtDayCloseATRPeriod)
        Me.grpIndicator.Controls.Add(Me.lblDayCloseATRPeriod)
        Me.grpIndicator.Location = New System.Drawing.Point(487, 102)
        Me.grpIndicator.Name = "grpIndicator"
        Me.grpIndicator.Size = New System.Drawing.Size(272, 200)
        Me.grpIndicator.TabIndex = 3
        Me.grpIndicator.TabStop = False
        Me.grpIndicator.Text = "Indicator Details"
        '
        'txtRSIValue
        '
        Me.txtRSIValue.Location = New System.Drawing.Point(178, 169)
        Me.txtRSIValue.Margin = New System.Windows.Forms.Padding(4)
        Me.txtRSIValue.Name = "txtRSIValue"
        Me.txtRSIValue.Size = New System.Drawing.Size(79, 22)
        Me.txtRSIValue.TabIndex = 4
        Me.txtRSIValue.Tag = "RSI Value"
        '
        'lblRSIValue
        '
        Me.lblRSIValue.AutoSize = True
        Me.lblRSIValue.Location = New System.Drawing.Point(7, 172)
        Me.lblRSIValue.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblRSIValue.Name = "lblRSIValue"
        Me.lblRSIValue.Size = New System.Drawing.Size(68, 17)
        Me.lblRSIValue.TabIndex = 63
        Me.lblRSIValue.Text = "RSI Level"
        '
        'txtCloseRSIPeriod
        '
        Me.txtCloseRSIPeriod.Location = New System.Drawing.Point(178, 134)
        Me.txtCloseRSIPeriod.Margin = New System.Windows.Forms.Padding(4)
        Me.txtCloseRSIPeriod.Name = "txtCloseRSIPeriod"
        Me.txtCloseRSIPeriod.Size = New System.Drawing.Size(79, 22)
        Me.txtCloseRSIPeriod.TabIndex = 3
        Me.txtCloseRSIPeriod.Tag = "RSI Period(Close)"
        '
        'lblCloseRSIPeriod
        '
        Me.lblCloseRSIPeriod.AutoSize = True
        Me.lblCloseRSIPeriod.Location = New System.Drawing.Point(7, 137)
        Me.lblCloseRSIPeriod.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblCloseRSIPeriod.Name = "lblCloseRSIPeriod"
        Me.lblCloseRSIPeriod.Size = New System.Drawing.Size(120, 17)
        Me.lblCloseRSIPeriod.TabIndex = 61
        Me.lblCloseRSIPeriod.Text = "RSI Period(Close)"
        '
        'txtDayCloseSMAPeriod
        '
        Me.txtDayCloseSMAPeriod.Location = New System.Drawing.Point(178, 97)
        Me.txtDayCloseSMAPeriod.Margin = New System.Windows.Forms.Padding(4)
        Me.txtDayCloseSMAPeriod.Name = "txtDayCloseSMAPeriod"
        Me.txtDayCloseSMAPeriod.Size = New System.Drawing.Size(79, 22)
        Me.txtDayCloseSMAPeriod.TabIndex = 2
        Me.txtDayCloseSMAPeriod.Tag = "SMA Period(Day Close)"
        '
        'lblDayCloseSMAPeriod
        '
        Me.lblDayCloseSMAPeriod.AutoSize = True
        Me.lblDayCloseSMAPeriod.Location = New System.Drawing.Point(7, 100)
        Me.lblDayCloseSMAPeriod.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblDayCloseSMAPeriod.Name = "lblDayCloseSMAPeriod"
        Me.lblDayCloseSMAPeriod.Size = New System.Drawing.Size(156, 17)
        Me.lblDayCloseSMAPeriod.TabIndex = 59
        Me.lblDayCloseSMAPeriod.Text = "SMA Period(Day Close)"
        '
        'txtVWAPEMAPeriod
        '
        Me.txtVWAPEMAPeriod.Location = New System.Drawing.Point(178, 25)
        Me.txtVWAPEMAPeriod.Margin = New System.Windows.Forms.Padding(4)
        Me.txtVWAPEMAPeriod.Name = "txtVWAPEMAPeriod"
        Me.txtVWAPEMAPeriod.Size = New System.Drawing.Size(79, 22)
        Me.txtVWAPEMAPeriod.TabIndex = 0
        Me.txtVWAPEMAPeriod.Tag = "EMA Period(VWAP)"
        '
        'lblVWAPEMAPeriod
        '
        Me.lblVWAPEMAPeriod.AutoSize = True
        Me.lblVWAPEMAPeriod.Location = New System.Drawing.Point(7, 28)
        Me.lblVWAPEMAPeriod.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblVWAPEMAPeriod.Name = "lblVWAPEMAPeriod"
        Me.lblVWAPEMAPeriod.Size = New System.Drawing.Size(132, 17)
        Me.lblVWAPEMAPeriod.TabIndex = 57
        Me.lblVWAPEMAPeriod.Text = "EMA Period(VWAP)"
        '
        'txtDayCloseATRPeriod
        '
        Me.txtDayCloseATRPeriod.Location = New System.Drawing.Point(178, 62)
        Me.txtDayCloseATRPeriod.Margin = New System.Windows.Forms.Padding(4)
        Me.txtDayCloseATRPeriod.Name = "txtDayCloseATRPeriod"
        Me.txtDayCloseATRPeriod.Size = New System.Drawing.Size(79, 22)
        Me.txtDayCloseATRPeriod.TabIndex = 1
        Me.txtDayCloseATRPeriod.Tag = "ATR Period(Day Close)"
        '
        'lblDayCloseATRPeriod
        '
        Me.lblDayCloseATRPeriod.AutoSize = True
        Me.lblDayCloseATRPeriod.Location = New System.Drawing.Point(7, 65)
        Me.lblDayCloseATRPeriod.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblDayCloseATRPeriod.Name = "lblDayCloseATRPeriod"
        Me.lblDayCloseATRPeriod.Size = New System.Drawing.Size(155, 17)
        Me.lblDayCloseATRPeriod.TabIndex = 55
        Me.lblDayCloseATRPeriod.Text = "ATR Period(Day Close)"
        '
        'grpOther
        '
        Me.grpOther.Controls.Add(Me.btnBrowse)
        Me.grpOther.Controls.Add(Me.txtInstrumentDetalis)
        Me.grpOther.Controls.Add(Me.lblInstrumentDetails)
        Me.grpOther.Controls.Add(Me.dtpckrEODExitTime)
        Me.grpOther.Controls.Add(Me.lblEODExitTime)
        Me.grpOther.Controls.Add(Me.txtOverallMaxLoss)
        Me.grpOther.Controls.Add(Me.lblOverallMaxLoss)
        Me.grpOther.Controls.Add(Me.txtOverallMaxProfit)
        Me.grpOther.Controls.Add(Me.lblOverallMaxProfit)
        Me.grpOther.Controls.Add(Me.txtMaxLossPerTrade)
        Me.grpOther.Controls.Add(Me.lblMaxLossPerTrade)
        Me.grpOther.Controls.Add(Me.txtTargetMultiplier)
        Me.grpOther.Controls.Add(Me.lblTargetMultiplier)
        Me.grpOther.Controls.Add(Me.txtTargetToLeftMovementPercentage)
        Me.grpOther.Controls.Add(Me.lblTargetToLeftMovementPercentage)
        Me.grpOther.Controls.Add(Me.txtSignalTimeFrame)
        Me.grpOther.Controls.Add(Me.lblSignalTimeFrame)
        Me.grpOther.Location = New System.Drawing.Point(8, 2)
        Me.grpOther.Name = "grpOther"
        Me.grpOther.Size = New System.Drawing.Size(471, 300)
        Me.grpOther.TabIndex = 1
        Me.grpOther.TabStop = False
        Me.grpOther.Text = "Other Details"
        '
        'dtpckrEODExitTime
        '
        Me.dtpckrEODExitTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpckrEODExitTime.Location = New System.Drawing.Point(237, 218)
        Me.dtpckrEODExitTime.Name = "dtpckrEODExitTime"
        Me.dtpckrEODExitTime.ShowUpDown = True
        Me.dtpckrEODExitTime.Size = New System.Drawing.Size(220, 22)
        Me.dtpckrEODExitTime.TabIndex = 6
        '
        'lblEODExitTime
        '
        Me.lblEODExitTime.AutoSize = True
        Me.lblEODExitTime.Location = New System.Drawing.Point(7, 222)
        Me.lblEODExitTime.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblEODExitTime.Name = "lblEODExitTime"
        Me.lblEODExitTime.Size = New System.Drawing.Size(99, 17)
        Me.lblEODExitTime.TabIndex = 75
        Me.lblEODExitTime.Text = "EOD Exit Time"
        '
        'txtOverallMaxLoss
        '
        Me.txtOverallMaxLoss.Location = New System.Drawing.Point(237, 186)
        Me.txtOverallMaxLoss.Margin = New System.Windows.Forms.Padding(4)
        Me.txtOverallMaxLoss.Name = "txtOverallMaxLoss"
        Me.txtOverallMaxLoss.Size = New System.Drawing.Size(220, 22)
        Me.txtOverallMaxLoss.TabIndex = 5
        Me.txtOverallMaxLoss.Tag = "Overall Max Loss"
        '
        'lblOverallMaxLoss
        '
        Me.lblOverallMaxLoss.AutoSize = True
        Me.lblOverallMaxLoss.Location = New System.Drawing.Point(7, 189)
        Me.lblOverallMaxLoss.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblOverallMaxLoss.Name = "lblOverallMaxLoss"
        Me.lblOverallMaxLoss.Size = New System.Drawing.Size(116, 17)
        Me.lblOverallMaxLoss.TabIndex = 73
        Me.lblOverallMaxLoss.Text = "Overall Max Loss"
        '
        'txtOverallMaxProfit
        '
        Me.txtOverallMaxProfit.Location = New System.Drawing.Point(237, 154)
        Me.txtOverallMaxProfit.Margin = New System.Windows.Forms.Padding(4)
        Me.txtOverallMaxProfit.Name = "txtOverallMaxProfit"
        Me.txtOverallMaxProfit.Size = New System.Drawing.Size(220, 22)
        Me.txtOverallMaxProfit.TabIndex = 4
        Me.txtOverallMaxProfit.Tag = "Overall Max Profit"
        '
        'lblOverallMaxProfit
        '
        Me.lblOverallMaxProfit.AutoSize = True
        Me.lblOverallMaxProfit.Location = New System.Drawing.Point(7, 157)
        Me.lblOverallMaxProfit.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblOverallMaxProfit.Name = "lblOverallMaxProfit"
        Me.lblOverallMaxProfit.Size = New System.Drawing.Size(119, 17)
        Me.lblOverallMaxProfit.TabIndex = 71
        Me.lblOverallMaxProfit.Text = "Overall Max Profit"
        '
        'txtMaxLossPerTrade
        '
        Me.txtMaxLossPerTrade.Location = New System.Drawing.Point(237, 122)
        Me.txtMaxLossPerTrade.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMaxLossPerTrade.Name = "txtMaxLossPerTrade"
        Me.txtMaxLossPerTrade.Size = New System.Drawing.Size(220, 22)
        Me.txtMaxLossPerTrade.TabIndex = 3
        Me.txtMaxLossPerTrade.Tag = "Max Loss Per Trade"
        '
        'lblMaxLossPerTrade
        '
        Me.lblMaxLossPerTrade.AutoSize = True
        Me.lblMaxLossPerTrade.Location = New System.Drawing.Point(7, 125)
        Me.lblMaxLossPerTrade.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMaxLossPerTrade.Name = "lblMaxLossPerTrade"
        Me.lblMaxLossPerTrade.Size = New System.Drawing.Size(135, 17)
        Me.lblMaxLossPerTrade.TabIndex = 69
        Me.lblMaxLossPerTrade.Text = "Max Loss Per Trade"
        '
        'txtTargetMultiplier
        '
        Me.txtTargetMultiplier.Location = New System.Drawing.Point(237, 58)
        Me.txtTargetMultiplier.Margin = New System.Windows.Forms.Padding(4)
        Me.txtTargetMultiplier.Name = "txtTargetMultiplier"
        Me.txtTargetMultiplier.Size = New System.Drawing.Size(220, 22)
        Me.txtTargetMultiplier.TabIndex = 1
        Me.txtTargetMultiplier.Tag = "Target Multiplier"
        '
        'lblTargetMultiplier
        '
        Me.lblTargetMultiplier.AutoSize = True
        Me.lblTargetMultiplier.Location = New System.Drawing.Point(6, 61)
        Me.lblTargetMultiplier.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblTargetMultiplier.Name = "lblTargetMultiplier"
        Me.lblTargetMultiplier.Size = New System.Drawing.Size(110, 17)
        Me.lblTargetMultiplier.TabIndex = 67
        Me.lblTargetMultiplier.Text = "Target Multiplier"
        '
        'txtTargetToLeftMovementPercentage
        '
        Me.txtTargetToLeftMovementPercentage.Location = New System.Drawing.Point(237, 91)
        Me.txtTargetToLeftMovementPercentage.Margin = New System.Windows.Forms.Padding(4)
        Me.txtTargetToLeftMovementPercentage.Name = "txtTargetToLeftMovementPercentage"
        Me.txtTargetToLeftMovementPercentage.Size = New System.Drawing.Size(220, 22)
        Me.txtTargetToLeftMovementPercentage.TabIndex = 2
        Me.txtTargetToLeftMovementPercentage.Tag = "Target To Left Movement %"
        '
        'lblTargetToLeftMovementPercentage
        '
        Me.lblTargetToLeftMovementPercentage.AutoSize = True
        Me.lblTargetToLeftMovementPercentage.Location = New System.Drawing.Point(7, 94)
        Me.lblTargetToLeftMovementPercentage.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblTargetToLeftMovementPercentage.Name = "lblTargetToLeftMovementPercentage"
        Me.lblTargetToLeftMovementPercentage.Size = New System.Drawing.Size(184, 17)
        Me.lblTargetToLeftMovementPercentage.TabIndex = 66
        Me.lblTargetToLeftMovementPercentage.Text = "Target To Left Movement %"
        '
        'txtSignalTimeFrame
        '
        Me.txtSignalTimeFrame.Location = New System.Drawing.Point(237, 26)
        Me.txtSignalTimeFrame.Margin = New System.Windows.Forms.Padding(4)
        Me.txtSignalTimeFrame.Name = "txtSignalTimeFrame"
        Me.txtSignalTimeFrame.Size = New System.Drawing.Size(220, 22)
        Me.txtSignalTimeFrame.TabIndex = 0
        Me.txtSignalTimeFrame.Tag = "Signal Time Frame"
        '
        'lblSignalTimeFrame
        '
        Me.lblSignalTimeFrame.AutoSize = True
        Me.lblSignalTimeFrame.Location = New System.Drawing.Point(7, 29)
        Me.lblSignalTimeFrame.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblSignalTimeFrame.Name = "lblSignalTimeFrame"
        Me.lblSignalTimeFrame.Size = New System.Drawing.Size(158, 17)
        Me.lblSignalTimeFrame.TabIndex = 61
        Me.lblSignalTimeFrame.Text = "Signal Time Frame(min)"
        '
        'grpStockSelection
        '
        Me.grpStockSelection.Controls.Add(Me.txtNumberOfStocks)
        Me.grpStockSelection.Controls.Add(Me.lblNumberOfStocks)
        Me.grpStockSelection.Controls.Add(Me.txtMinATRPer)
        Me.grpStockSelection.Controls.Add(Me.lblMinATRPer)
        Me.grpStockSelection.Controls.Add(Me.txtMinVolume)
        Me.grpStockSelection.Controls.Add(Me.lblMinVolume)
        Me.grpStockSelection.Controls.Add(Me.txtMaxPrice)
        Me.grpStockSelection.Controls.Add(Me.lblMaxPrice)
        Me.grpStockSelection.Controls.Add(Me.txtMinPrice)
        Me.grpStockSelection.Controls.Add(Me.lblMinPrice)
        Me.grpStockSelection.Location = New System.Drawing.Point(765, 120)
        Me.grpStockSelection.Name = "grpStockSelection"
        Me.grpStockSelection.Size = New System.Drawing.Size(260, 181)
        Me.grpStockSelection.TabIndex = 5
        Me.grpStockSelection.TabStop = False
        Me.grpStockSelection.Text = "Stock Selection Details"
        '
        'txtNumberOfStocks
        '
        Me.txtNumberOfStocks.Location = New System.Drawing.Point(151, 151)
        Me.txtNumberOfStocks.Margin = New System.Windows.Forms.Padding(4)
        Me.txtNumberOfStocks.Name = "txtNumberOfStocks"
        Me.txtNumberOfStocks.Size = New System.Drawing.Size(97, 22)
        Me.txtNumberOfStocks.TabIndex = 4
        Me.txtNumberOfStocks.Tag = "Number Of Stocks"
        '
        'lblNumberOfStocks
        '
        Me.lblNumberOfStocks.AutoSize = True
        Me.lblNumberOfStocks.Location = New System.Drawing.Point(6, 154)
        Me.lblNumberOfStocks.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblNumberOfStocks.Name = "lblNumberOfStocks"
        Me.lblNumberOfStocks.Size = New System.Drawing.Size(123, 17)
        Me.lblNumberOfStocks.TabIndex = 88
        Me.lblNumberOfStocks.Text = "Number Of Stocks"
        '
        'txtMinATRPer
        '
        Me.txtMinATRPer.Location = New System.Drawing.Point(151, 119)
        Me.txtMinATRPer.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMinATRPer.Name = "txtMinATRPer"
        Me.txtMinATRPer.Size = New System.Drawing.Size(97, 22)
        Me.txtMinATRPer.TabIndex = 3
        Me.txtMinATRPer.Tag = "Minimum ATR %"
        '
        'lblMinATRPer
        '
        Me.lblMinATRPer.AutoSize = True
        Me.lblMinATRPer.Location = New System.Drawing.Point(6, 122)
        Me.lblMinATRPer.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMinATRPer.Name = "lblMinATRPer"
        Me.lblMinATRPer.Size = New System.Drawing.Size(111, 17)
        Me.lblMinATRPer.TabIndex = 86
        Me.lblMinATRPer.Text = "Minimum ATR %"
        '
        'txtMinVolume
        '
        Me.txtMinVolume.Location = New System.Drawing.Point(151, 87)
        Me.txtMinVolume.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMinVolume.Name = "txtMinVolume"
        Me.txtMinVolume.Size = New System.Drawing.Size(97, 22)
        Me.txtMinVolume.TabIndex = 2
        Me.txtMinVolume.Tag = "Minimum Volume"
        '
        'lblMinVolume
        '
        Me.lblMinVolume.AutoSize = True
        Me.lblMinVolume.Location = New System.Drawing.Point(6, 90)
        Me.lblMinVolume.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMinVolume.Name = "lblMinVolume"
        Me.lblMinVolume.Size = New System.Drawing.Size(114, 17)
        Me.lblMinVolume.TabIndex = 84
        Me.lblMinVolume.Text = "Minimum Volume"
        '
        'txtMaxPrice
        '
        Me.txtMaxPrice.Location = New System.Drawing.Point(151, 54)
        Me.txtMaxPrice.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMaxPrice.Name = "txtMaxPrice"
        Me.txtMaxPrice.Size = New System.Drawing.Size(97, 22)
        Me.txtMaxPrice.TabIndex = 1
        Me.txtMaxPrice.Tag = "Maximum Price"
        '
        'lblMaxPrice
        '
        Me.lblMaxPrice.AutoSize = True
        Me.lblMaxPrice.Location = New System.Drawing.Point(6, 57)
        Me.lblMaxPrice.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMaxPrice.Name = "lblMaxPrice"
        Me.lblMaxPrice.Size = New System.Drawing.Size(102, 17)
        Me.lblMaxPrice.TabIndex = 82
        Me.lblMaxPrice.Text = "Maximum Price"
        '
        'txtMinPrice
        '
        Me.txtMinPrice.Location = New System.Drawing.Point(151, 22)
        Me.txtMinPrice.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMinPrice.Name = "txtMinPrice"
        Me.txtMinPrice.Size = New System.Drawing.Size(97, 22)
        Me.txtMinPrice.TabIndex = 0
        Me.txtMinPrice.Tag = "Minimum Price"
        '
        'lblMinPrice
        '
        Me.lblMinPrice.AutoSize = True
        Me.lblMinPrice.Location = New System.Drawing.Point(6, 25)
        Me.lblMinPrice.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMinPrice.Name = "lblMinPrice"
        Me.lblMinPrice.Size = New System.Drawing.Size(99, 17)
        Me.lblMinPrice.TabIndex = 80
        Me.lblMinPrice.Text = "Minimum Price"
        '
        'chkbAutoSelectStock
        '
        Me.chkbAutoSelectStock.AutoSize = True
        Me.chkbAutoSelectStock.Location = New System.Drawing.Point(774, 99)
        Me.chkbAutoSelectStock.Name = "chkbAutoSelectStock"
        Me.chkbAutoSelectStock.Size = New System.Drawing.Size(141, 21)
        Me.chkbAutoSelectStock.TabIndex = 4
        Me.chkbAutoSelectStock.Text = "Auto Select Stock"
        Me.chkbAutoSelectStock.UseVisualStyleBackColor = True
        '
        'btnBrowse
        '
        Me.btnBrowse.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnBrowse.Location = New System.Drawing.Point(425, 251)
        Me.btnBrowse.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnBrowse.Name = "btnBrowse"
        Me.btnBrowse.Size = New System.Drawing.Size(32, 23)
        Me.btnBrowse.TabIndex = 7
        Me.btnBrowse.Text = "..."
        Me.btnBrowse.UseVisualStyleBackColor = True
        '
        'txtInstrumentDetalis
        '
        Me.txtInstrumentDetalis.Location = New System.Drawing.Point(237, 252)
        Me.txtInstrumentDetalis.Margin = New System.Windows.Forms.Padding(4)
        Me.txtInstrumentDetalis.Name = "txtInstrumentDetalis"
        Me.txtInstrumentDetalis.ReadOnly = True
        Me.txtInstrumentDetalis.Size = New System.Drawing.Size(181, 22)
        Me.txtInstrumentDetalis.TabIndex = 7
        '
        'lblInstrumentDetails
        '
        Me.lblInstrumentDetails.AutoSize = True
        Me.lblInstrumentDetails.Location = New System.Drawing.Point(7, 255)
        Me.lblInstrumentDetails.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblInstrumentDetails.Name = "lblInstrumentDetails"
        Me.lblInstrumentDetails.Size = New System.Drawing.Size(121, 17)
        Me.lblInstrumentDetails.TabIndex = 77
        Me.lblInstrumentDetails.Text = "Instrument Details"
        '
        'opnFileSettings
        '
        '
        'frmNFOSettings
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1148, 307)
        Me.Controls.Add(Me.chkbAutoSelectStock)
        Me.Controls.Add(Me.grpStockSelection)
        Me.Controls.Add(Me.grpOther)
        Me.Controls.Add(Me.grpIndicator)
        Me.Controls.Add(Me.grpTelegram)
        Me.Controls.Add(Me.btnSave)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmNFOSettings"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Settings"
        Me.grpTelegram.ResumeLayout(False)
        Me.grpTelegram.PerformLayout()
        Me.grpIndicator.ResumeLayout(False)
        Me.grpIndicator.PerformLayout()
        Me.grpOther.ResumeLayout(False)
        Me.grpOther.PerformLayout()
        Me.grpStockSelection.ResumeLayout(False)
        Me.grpStockSelection.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents btnSave As Button
    Friend WithEvents ImageList1 As ImageList
    Friend WithEvents grpTelegram As GroupBox
    Friend WithEvents txtTelegramChatID As TextBox
    Friend WithEvents lblTelegramChatID As Label
    Friend WithEvents txtTelegramAPIKey As TextBox
    Friend WithEvents lblTelegramAPIKey As Label
    Friend WithEvents grpIndicator As GroupBox
    Friend WithEvents txtVWAPEMAPeriod As TextBox
    Friend WithEvents lblVWAPEMAPeriod As Label
    Friend WithEvents txtDayCloseATRPeriod As TextBox
    Friend WithEvents lblDayCloseATRPeriod As Label
    Friend WithEvents txtDayCloseSMAPeriod As TextBox
    Friend WithEvents lblDayCloseSMAPeriod As Label
    Friend WithEvents grpOther As GroupBox
    Friend WithEvents txtTargetMultiplier As TextBox
    Friend WithEvents lblTargetMultiplier As Label
    Friend WithEvents txtTargetToLeftMovementPercentage As TextBox
    Friend WithEvents lblTargetToLeftMovementPercentage As Label
    Friend WithEvents txtSignalTimeFrame As TextBox
    Friend WithEvents lblSignalTimeFrame As Label
    Friend WithEvents txtCloseRSIPeriod As TextBox
    Friend WithEvents lblCloseRSIPeriod As Label
    Friend WithEvents txtRSIValue As TextBox
    Friend WithEvents lblRSIValue As Label
    Friend WithEvents grpStockSelection As GroupBox
    Friend WithEvents txtMinPrice As TextBox
    Friend WithEvents lblMinPrice As Label
    Friend WithEvents txtMaxPrice As TextBox
    Friend WithEvents lblMaxPrice As Label
    Friend WithEvents txtMinVolume As TextBox
    Friend WithEvents lblMinVolume As Label
    Friend WithEvents txtMinATRPer As TextBox
    Friend WithEvents lblMinATRPer As Label
    Friend WithEvents txtMaxLossPerTrade As TextBox
    Friend WithEvents lblMaxLossPerTrade As Label
    Friend WithEvents txtOverallMaxProfit As TextBox
    Friend WithEvents lblOverallMaxProfit As Label
    Friend WithEvents txtOverallMaxLoss As TextBox
    Friend WithEvents lblOverallMaxLoss As Label
    Friend WithEvents dtpckrEODExitTime As DateTimePicker
    Friend WithEvents lblEODExitTime As Label
    Friend WithEvents txtNumberOfStocks As TextBox
    Friend WithEvents lblNumberOfStocks As Label
    Friend WithEvents chkbAutoSelectStock As CheckBox
    Friend WithEvents btnBrowse As Button
    Friend WithEvents txtInstrumentDetalis As TextBox
    Friend WithEvents lblInstrumentDetails As Label
    Friend WithEvents opnFileSettings As OpenFileDialog
End Class
