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
        Me.txtTargetMultiplier = New System.Windows.Forms.TextBox()
        Me.lblTargetMultiplier = New System.Windows.Forms.Label()
        Me.txtTargetToLeftMovementPercentage = New System.Windows.Forms.TextBox()
        Me.lblTargetToLeftMovementPercentage = New System.Windows.Forms.Label()
        Me.txtSignalTimeFrame = New System.Windows.Forms.TextBox()
        Me.lblSignalTimeFrame = New System.Windows.Forms.Label()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.txtMinPrice = New System.Windows.Forms.TextBox()
        Me.lblMinPrice = New System.Windows.Forms.Label()
        Me.txtMaxPrice = New System.Windows.Forms.TextBox()
        Me.lblMaxPrice = New System.Windows.Forms.Label()
        Me.txtMinVolume = New System.Windows.Forms.TextBox()
        Me.lblMinVolume = New System.Windows.Forms.Label()
        Me.txtMinATRPer = New System.Windows.Forms.TextBox()
        Me.lblMinATRPer = New System.Windows.Forms.Label()
        Me.grpTelegram.SuspendLayout()
        Me.grpIndicator.SuspendLayout()
        Me.grpOther.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
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
        Me.grpTelegram.TabIndex = 3
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
        Me.grpIndicator.Location = New System.Drawing.Point(554, 102)
        Me.grpIndicator.Name = "grpIndicator"
        Me.grpIndicator.Size = New System.Drawing.Size(538, 200)
        Me.grpIndicator.TabIndex = 4
        Me.grpIndicator.TabStop = False
        Me.grpIndicator.Text = "Indicator Details"
        '
        'txtRSIValue
        '
        Me.txtRSIValue.Location = New System.Drawing.Point(178, 169)
        Me.txtRSIValue.Margin = New System.Windows.Forms.Padding(4)
        Me.txtRSIValue.Name = "txtRSIValue"
        Me.txtRSIValue.Size = New System.Drawing.Size(348, 22)
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
        Me.txtCloseRSIPeriod.Size = New System.Drawing.Size(348, 22)
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
        Me.txtDayCloseATRPeriod.Size = New System.Drawing.Size(348, 22)
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
        Me.grpOther.Controls.Add(Me.txtTargetMultiplier)
        Me.grpOther.Controls.Add(Me.lblTargetMultiplier)
        Me.grpOther.Controls.Add(Me.txtTargetToLeftMovementPercentage)
        Me.grpOther.Controls.Add(Me.lblTargetToLeftMovementPercentage)
        Me.grpOther.Controls.Add(Me.txtSignalTimeFrame)
        Me.grpOther.Controls.Add(Me.lblSignalTimeFrame)
        Me.grpOther.Location = New System.Drawing.Point(8, 2)
        Me.grpOther.Name = "grpOther"
        Me.grpOther.Size = New System.Drawing.Size(540, 125)
        Me.grpOther.TabIndex = 1
        Me.grpOther.TabStop = False
        Me.grpOther.Text = "Other Details"
        '
        'txtTargetMultiplier
        '
        Me.txtTargetMultiplier.Location = New System.Drawing.Point(269, 58)
        Me.txtTargetMultiplier.Margin = New System.Windows.Forms.Padding(4)
        Me.txtTargetMultiplier.Name = "txtTargetMultiplier"
        Me.txtTargetMultiplier.Size = New System.Drawing.Size(255, 22)
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
        Me.txtTargetToLeftMovementPercentage.Location = New System.Drawing.Point(269, 91)
        Me.txtTargetToLeftMovementPercentage.Margin = New System.Windows.Forms.Padding(4)
        Me.txtTargetToLeftMovementPercentage.Name = "txtTargetToLeftMovementPercentage"
        Me.txtTargetToLeftMovementPercentage.Size = New System.Drawing.Size(255, 22)
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
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.txtMinATRPer)
        Me.GroupBox1.Controls.Add(Me.lblMinATRPer)
        Me.GroupBox1.Controls.Add(Me.txtMinVolume)
        Me.GroupBox1.Controls.Add(Me.lblMinVolume)
        Me.GroupBox1.Controls.Add(Me.txtMaxPrice)
        Me.GroupBox1.Controls.Add(Me.lblMaxPrice)
        Me.GroupBox1.Controls.Add(Me.txtMinPrice)
        Me.GroupBox1.Controls.Add(Me.lblMinPrice)
        Me.GroupBox1.Location = New System.Drawing.Point(8, 128)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(540, 174)
        Me.GroupBox1.TabIndex = 2
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Stock Selection Details"
        '
        'txtMinPrice
        '
        Me.txtMinPrice.Location = New System.Drawing.Point(269, 22)
        Me.txtMinPrice.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMinPrice.Name = "txtMinPrice"
        Me.txtMinPrice.Size = New System.Drawing.Size(255, 22)
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
        'txtMaxPrice
        '
        Me.txtMaxPrice.Location = New System.Drawing.Point(269, 54)
        Me.txtMaxPrice.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMaxPrice.Name = "txtMaxPrice"
        Me.txtMaxPrice.Size = New System.Drawing.Size(255, 22)
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
        'txtMinVolume
        '
        Me.txtMinVolume.Location = New System.Drawing.Point(269, 87)
        Me.txtMinVolume.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMinVolume.Name = "txtMinVolume"
        Me.txtMinVolume.Size = New System.Drawing.Size(255, 22)
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
        'txtMinATRPer
        '
        Me.txtMinATRPer.Location = New System.Drawing.Point(269, 119)
        Me.txtMinATRPer.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMinATRPer.Name = "txtMinATRPer"
        Me.txtMinATRPer.Size = New System.Drawing.Size(255, 22)
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
        'frmNFOSettings
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1217, 305)
        Me.Controls.Add(Me.GroupBox1)
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
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
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
    Friend WithEvents txtCloseRSIPeriod As TextBox
    Friend WithEvents lblCloseRSIPeriod As Label
    Friend WithEvents txtRSIValue As TextBox
    Friend WithEvents lblRSIValue As Label
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents txtMinPrice As TextBox
    Friend WithEvents lblMinPrice As Label
    Friend WithEvents txtMaxPrice As TextBox
    Friend WithEvents lblMaxPrice As Label
    Friend WithEvents txtMinVolume As TextBox
    Friend WithEvents lblMinVolume As Label
    Friend WithEvents txtMinATRPer As TextBox
    Friend WithEvents lblMinATRPer As Label
End Class
