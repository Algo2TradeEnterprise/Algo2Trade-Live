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
        Me.txtDayCloseSMAPeriod = New System.Windows.Forms.TextBox()
        Me.lblDayCloseSMAPeriod = New System.Windows.Forms.Label()
        Me.txtVWAPEMAPeriod = New System.Windows.Forms.TextBox()
        Me.lblVWAPEMAPeriod = New System.Windows.Forms.Label()
        Me.txtDayCloseATRPeriod = New System.Windows.Forms.TextBox()
        Me.lblDayCloseATRPeriod = New System.Windows.Forms.Label()
        Me.grpOther = New System.Windows.Forms.GroupBox()
        Me.cmbRepeatSignalOnHistoricalRefresh = New System.Windows.Forms.ComboBox()
        Me.lblRepeatSignalOnHistoricalRefresh = New System.Windows.Forms.Label()
        Me.cmbDisplayLogType = New System.Windows.Forms.ComboBox()
        Me.lblDisplayLogType = New System.Windows.Forms.Label()
        Me.chkbMCX = New System.Windows.Forms.CheckBox()
        Me.chkbNFO = New System.Windows.Forms.CheckBox()
        Me.chkbNSE = New System.Windows.Forms.CheckBox()
        Me.lblRunInstruments = New System.Windows.Forms.Label()
        Me.btnBrowse = New System.Windows.Forms.Button()
        Me.txtInstrumentDetalis = New System.Windows.Forms.TextBox()
        Me.lblInstrumentDetails = New System.Windows.Forms.Label()
        Me.txtTargetMultiplier = New System.Windows.Forms.TextBox()
        Me.lblTargetMultiplier = New System.Windows.Forms.Label()
        Me.txtTargetToLeftMovementPercentage = New System.Windows.Forms.TextBox()
        Me.lblTargetToLeftMovementPercentage = New System.Windows.Forms.Label()
        Me.txtSignalTimeFrame = New System.Windows.Forms.TextBox()
        Me.lblSignalTimeFrame = New System.Windows.Forms.Label()
        Me.opnFileSettings = New System.Windows.Forms.OpenFileDialog()
        Me.txtCloseRSIPeriod = New System.Windows.Forms.TextBox()
        Me.lblCloseRSIPeriod = New System.Windows.Forms.Label()
        Me.txtRSIValue = New System.Windows.Forms.TextBox()
        Me.lblRSIValue = New System.Windows.Forms.Label()
        Me.grpTelegram.SuspendLayout()
        Me.grpIndicator.SuspendLayout()
        Me.grpOther.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnSave
        '
        Me.btnSave.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnSave.ImageKey = "save-icon-36533.png"
        Me.btnSave.ImageList = Me.ImageList1
        Me.btnSave.Location = New System.Drawing.Point(1099, 8)
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
        Me.grpTelegram.Location = New System.Drawing.Point(554, 3)
        Me.grpTelegram.Name = "grpTelegram"
        Me.grpTelegram.Size = New System.Drawing.Size(538, 97)
        Me.grpTelegram.TabIndex = 2
        Me.grpTelegram.TabStop = False
        Me.grpTelegram.Text = "Telegram Details"
        '
        'txtTelegramChatID
        '
        Me.txtTelegramChatID.Location = New System.Drawing.Point(178, 62)
        Me.txtTelegramChatID.Margin = New System.Windows.Forms.Padding(4)
        Me.txtTelegramChatID.Name = "txtTelegramChatID"
        Me.txtTelegramChatID.Size = New System.Drawing.Size(348, 22)
        Me.txtTelegramChatID.TabIndex = 1
        Me.txtTelegramChatID.Tag = "Telegram Chat ID"
        '
        'lblTelegramChatID
        '
        Me.lblTelegramChatID.AutoSize = True
        Me.lblTelegramChatID.Location = New System.Drawing.Point(9, 65)
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
        Me.grpIndicator.Location = New System.Drawing.Point(554, 107)
        Me.grpIndicator.Name = "grpIndicator"
        Me.grpIndicator.Size = New System.Drawing.Size(538, 200)
        Me.grpIndicator.TabIndex = 3
        Me.grpIndicator.TabStop = False
        Me.grpIndicator.Text = "Indicator Details"
        '
        'txtDayCloseSMAPeriod
        '
        Me.txtDayCloseSMAPeriod.Location = New System.Drawing.Point(178, 97)
        Me.txtDayCloseSMAPeriod.Margin = New System.Windows.Forms.Padding(4)
        Me.txtDayCloseSMAPeriod.Name = "txtDayCloseSMAPeriod"
        Me.txtDayCloseSMAPeriod.Size = New System.Drawing.Size(348, 22)
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
        Me.txtVWAPEMAPeriod.Size = New System.Drawing.Size(348, 22)
        Me.txtVWAPEMAPeriod.TabIndex = 1
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
        Me.txtDayCloseATRPeriod.Size = New System.Drawing.Size(348, 22)
        Me.txtDayCloseATRPeriod.TabIndex = 0
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
        Me.grpOther.Controls.Add(Me.cmbRepeatSignalOnHistoricalRefresh)
        Me.grpOther.Controls.Add(Me.lblRepeatSignalOnHistoricalRefresh)
        Me.grpOther.Controls.Add(Me.cmbDisplayLogType)
        Me.grpOther.Controls.Add(Me.lblDisplayLogType)
        Me.grpOther.Controls.Add(Me.chkbMCX)
        Me.grpOther.Controls.Add(Me.chkbNFO)
        Me.grpOther.Controls.Add(Me.chkbNSE)
        Me.grpOther.Controls.Add(Me.lblRunInstruments)
        Me.grpOther.Controls.Add(Me.btnBrowse)
        Me.grpOther.Controls.Add(Me.txtInstrumentDetalis)
        Me.grpOther.Controls.Add(Me.lblInstrumentDetails)
        Me.grpOther.Controls.Add(Me.txtTargetMultiplier)
        Me.grpOther.Controls.Add(Me.lblTargetMultiplier)
        Me.grpOther.Controls.Add(Me.txtTargetToLeftMovementPercentage)
        Me.grpOther.Controls.Add(Me.lblTargetToLeftMovementPercentage)
        Me.grpOther.Controls.Add(Me.txtSignalTimeFrame)
        Me.grpOther.Controls.Add(Me.lblSignalTimeFrame)
        Me.grpOther.Location = New System.Drawing.Point(8, 2)
        Me.grpOther.Name = "grpOther"
        Me.grpOther.Size = New System.Drawing.Size(540, 305)
        Me.grpOther.TabIndex = 1
        Me.grpOther.TabStop = False
        Me.grpOther.Text = "Other Details"
        '
        'cmbRepeatSignalOnHistoricalRefresh
        '
        Me.cmbRepeatSignalOnHistoricalRefresh.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbRepeatSignalOnHistoricalRefresh.FormattingEnabled = True
        Me.cmbRepeatSignalOnHistoricalRefresh.Items.AddRange(New Object() {"True", "False"})
        Me.cmbRepeatSignalOnHistoricalRefresh.Location = New System.Drawing.Point(269, 243)
        Me.cmbRepeatSignalOnHistoricalRefresh.Name = "cmbRepeatSignalOnHistoricalRefresh"
        Me.cmbRepeatSignalOnHistoricalRefresh.Size = New System.Drawing.Size(255, 24)
        Me.cmbRepeatSignalOnHistoricalRefresh.TabIndex = 75
        '
        'lblRepeatSignalOnHistoricalRefresh
        '
        Me.lblRepeatSignalOnHistoricalRefresh.AutoSize = True
        Me.lblRepeatSignalOnHistoricalRefresh.Location = New System.Drawing.Point(7, 245)
        Me.lblRepeatSignalOnHistoricalRefresh.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblRepeatSignalOnHistoricalRefresh.Name = "lblRepeatSignalOnHistoricalRefresh"
        Me.lblRepeatSignalOnHistoricalRefresh.Size = New System.Drawing.Size(236, 17)
        Me.lblRepeatSignalOnHistoricalRefresh.TabIndex = 76
        Me.lblRepeatSignalOnHistoricalRefresh.Text = "Repeat Signal On Historical Refresh"
        '
        'cmbDisplayLogType
        '
        Me.cmbDisplayLogType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbDisplayLogType.FormattingEnabled = True
        Me.cmbDisplayLogType.Items.AddRange(New Object() {"All", "Negative", "Positive"})
        Me.cmbDisplayLogType.Location = New System.Drawing.Point(269, 205)
        Me.cmbDisplayLogType.Name = "cmbDisplayLogType"
        Me.cmbDisplayLogType.Size = New System.Drawing.Size(255, 24)
        Me.cmbDisplayLogType.TabIndex = 7
        '
        'lblDisplayLogType
        '
        Me.lblDisplayLogType.AutoSize = True
        Me.lblDisplayLogType.Location = New System.Drawing.Point(7, 207)
        Me.lblDisplayLogType.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblDisplayLogType.Name = "lblDisplayLogType"
        Me.lblDisplayLogType.Size = New System.Drawing.Size(118, 17)
        Me.lblDisplayLogType.TabIndex = 74
        Me.lblDisplayLogType.Text = "Display Log Type"
        '
        'chkbMCX
        '
        Me.chkbMCX.AutoSize = True
        Me.chkbMCX.Location = New System.Drawing.Point(399, 97)
        Me.chkbMCX.Name = "chkbMCX"
        Me.chkbMCX.Size = New System.Drawing.Size(59, 21)
        Me.chkbMCX.TabIndex = 4
        Me.chkbMCX.Text = "MCX"
        Me.chkbMCX.UseVisualStyleBackColor = True
        '
        'chkbNFO
        '
        Me.chkbNFO.AutoSize = True
        Me.chkbNFO.Location = New System.Drawing.Point(335, 97)
        Me.chkbNFO.Name = "chkbNFO"
        Me.chkbNFO.Size = New System.Drawing.Size(59, 21)
        Me.chkbNFO.TabIndex = 3
        Me.chkbNFO.Text = "NFO"
        Me.chkbNFO.UseVisualStyleBackColor = True
        '
        'chkbNSE
        '
        Me.chkbNSE.AutoSize = True
        Me.chkbNSE.Location = New System.Drawing.Point(271, 97)
        Me.chkbNSE.Name = "chkbNSE"
        Me.chkbNSE.Size = New System.Drawing.Size(58, 21)
        Me.chkbNSE.TabIndex = 2
        Me.chkbNSE.Text = "NSE"
        Me.chkbNSE.UseVisualStyleBackColor = True
        '
        'lblRunInstruments
        '
        Me.lblRunInstruments.AutoSize = True
        Me.lblRunInstruments.Location = New System.Drawing.Point(7, 99)
        Me.lblRunInstruments.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblRunInstruments.Name = "lblRunInstruments"
        Me.lblRunInstruments.Size = New System.Drawing.Size(111, 17)
        Me.lblRunInstruments.TabIndex = 72
        Me.lblRunInstruments.Text = "Run Instruments"
        '
        'btnBrowse
        '
        Me.btnBrowse.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnBrowse.Location = New System.Drawing.Point(484, 60)
        Me.btnBrowse.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnBrowse.Name = "btnBrowse"
        Me.btnBrowse.Size = New System.Drawing.Size(40, 23)
        Me.btnBrowse.TabIndex = 1
        Me.btnBrowse.Text = "..."
        Me.btnBrowse.UseVisualStyleBackColor = True
        '
        'txtInstrumentDetalis
        '
        Me.txtInstrumentDetalis.Location = New System.Drawing.Point(269, 61)
        Me.txtInstrumentDetalis.Margin = New System.Windows.Forms.Padding(4)
        Me.txtInstrumentDetalis.Name = "txtInstrumentDetalis"
        Me.txtInstrumentDetalis.ReadOnly = True
        Me.txtInstrumentDetalis.Size = New System.Drawing.Size(208, 22)
        Me.txtInstrumentDetalis.TabIndex = 1
        '
        'lblInstrumentDetails
        '
        Me.lblInstrumentDetails.AutoSize = True
        Me.lblInstrumentDetails.Location = New System.Drawing.Point(7, 64)
        Me.lblInstrumentDetails.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblInstrumentDetails.Name = "lblInstrumentDetails"
        Me.lblInstrumentDetails.Size = New System.Drawing.Size(121, 17)
        Me.lblInstrumentDetails.TabIndex = 70
        Me.lblInstrumentDetails.Text = "Instrument Details"
        '
        'txtTargetMultiplier
        '
        Me.txtTargetMultiplier.Location = New System.Drawing.Point(269, 131)
        Me.txtTargetMultiplier.Margin = New System.Windows.Forms.Padding(4)
        Me.txtTargetMultiplier.Name = "txtTargetMultiplier"
        Me.txtTargetMultiplier.Size = New System.Drawing.Size(255, 22)
        Me.txtTargetMultiplier.TabIndex = 5
        Me.txtTargetMultiplier.Tag = "Target Multiplier"
        '
        'lblTargetMultiplier
        '
        Me.lblTargetMultiplier.AutoSize = True
        Me.lblTargetMultiplier.Location = New System.Drawing.Point(6, 134)
        Me.lblTargetMultiplier.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblTargetMultiplier.Name = "lblTargetMultiplier"
        Me.lblTargetMultiplier.Size = New System.Drawing.Size(110, 17)
        Me.lblTargetMultiplier.TabIndex = 67
        Me.lblTargetMultiplier.Text = "Target Multiplier"
        '
        'txtTargetToLeftMovementPercentage
        '
        Me.txtTargetToLeftMovementPercentage.Location = New System.Drawing.Point(269, 168)
        Me.txtTargetToLeftMovementPercentage.Margin = New System.Windows.Forms.Padding(4)
        Me.txtTargetToLeftMovementPercentage.Name = "txtTargetToLeftMovementPercentage"
        Me.txtTargetToLeftMovementPercentage.Size = New System.Drawing.Size(255, 22)
        Me.txtTargetToLeftMovementPercentage.TabIndex = 6
        Me.txtTargetToLeftMovementPercentage.Tag = "Target To Left Movement %"
        '
        'lblTargetToLeftMovementPercentage
        '
        Me.lblTargetToLeftMovementPercentage.AutoSize = True
        Me.lblTargetToLeftMovementPercentage.Location = New System.Drawing.Point(7, 171)
        Me.lblTargetToLeftMovementPercentage.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblTargetToLeftMovementPercentage.Name = "lblTargetToLeftMovementPercentage"
        Me.lblTargetToLeftMovementPercentage.Size = New System.Drawing.Size(184, 17)
        Me.lblTargetToLeftMovementPercentage.TabIndex = 66
        Me.lblTargetToLeftMovementPercentage.Text = "Target To Left Movement %"
        '
        'txtSignalTimeFrame
        '
        Me.txtSignalTimeFrame.Location = New System.Drawing.Point(269, 26)
        Me.txtSignalTimeFrame.Margin = New System.Windows.Forms.Padding(4)
        Me.txtSignalTimeFrame.Name = "txtSignalTimeFrame"
        Me.txtSignalTimeFrame.Size = New System.Drawing.Size(255, 22)
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
        'opnFileSettings
        '
        '
        'txtCloseRSIPeriod
        '
        Me.txtCloseRSIPeriod.Location = New System.Drawing.Point(178, 134)
        Me.txtCloseRSIPeriod.Margin = New System.Windows.Forms.Padding(4)
        Me.txtCloseRSIPeriod.Name = "txtCloseRSIPeriod"
        Me.txtCloseRSIPeriod.Size = New System.Drawing.Size(348, 22)
        Me.txtCloseRSIPeriod.TabIndex = 60
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
        'txtRSIValue
        '
        Me.txtRSIValue.Location = New System.Drawing.Point(178, 169)
        Me.txtRSIValue.Margin = New System.Windows.Forms.Padding(4)
        Me.txtRSIValue.Name = "txtRSIValue"
        Me.txtRSIValue.Size = New System.Drawing.Size(348, 22)
        Me.txtRSIValue.TabIndex = 62
        Me.txtRSIValue.Tag = "RSI Value"
        '
        'lblRSIValue
        '
        Me.lblRSIValue.AutoSize = True
        Me.lblRSIValue.Location = New System.Drawing.Point(7, 172)
        Me.lblRSIValue.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblRSIValue.Name = "lblRSIValue"
        Me.lblRSIValue.Size = New System.Drawing.Size(70, 17)
        Me.lblRSIValue.TabIndex = 63
        Me.lblRSIValue.Text = "RSI Value"
        '
        'frmNFOSettings
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1217, 312)
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
        Me.ResumeLayout(False)

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
    Friend WithEvents btnBrowse As Button
    Friend WithEvents txtInstrumentDetalis As TextBox
    Friend WithEvents lblInstrumentDetails As Label
    Friend WithEvents opnFileSettings As OpenFileDialog
    Friend WithEvents chkbMCX As CheckBox
    Friend WithEvents chkbNFO As CheckBox
    Friend WithEvents chkbNSE As CheckBox
    Friend WithEvents lblRunInstruments As Label
    Friend WithEvents cmbDisplayLogType As ComboBox
    Friend WithEvents lblDisplayLogType As Label
    Friend WithEvents cmbRepeatSignalOnHistoricalRefresh As ComboBox
    Friend WithEvents lblRepeatSignalOnHistoricalRefresh As Label
    Friend WithEvents txtCloseRSIPeriod As TextBox
    Friend WithEvents lblCloseRSIPeriod As Label
    Friend WithEvents txtRSIValue As TextBox
    Friend WithEvents lblRSIValue As Label
End Class
