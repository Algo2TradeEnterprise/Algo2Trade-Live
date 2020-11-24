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
        Me.grpTelegram = New System.Windows.Forms.GroupBox()
        Me.txtTelegramTradeChatID = New System.Windows.Forms.TextBox()
        Me.lblTelegramChatID = New System.Windows.Forms.Label()
        Me.txtTelegramBotAPIKey = New System.Windows.Forms.TextBox()
        Me.lblTelegramAPI = New System.Windows.Forms.Label()
        Me.grpTradeDetails = New System.Windows.Forms.GroupBox()
        Me.lblExitType = New System.Windows.Forms.Label()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.rbOppositeSide = New System.Windows.Forms.RadioButton()
        Me.rbSameSideExit = New System.Windows.Forms.RadioButton()
        Me.txtExitSDMultiplier = New System.Windows.Forms.TextBox()
        Me.lblExitSDMultiplier = New System.Windows.Forms.Label()
        Me.txtEntrySDMultiplier = New System.Windows.Forms.TextBox()
        Me.lblEntrySDMultiplier = New System.Windows.Forms.Label()
        Me.txtSignalTimeframe = New System.Windows.Forms.TextBox()
        Me.lblSignalTimeframe = New System.Windows.Forms.Label()
        Me.txtDaysBack = New System.Windows.Forms.TextBox()
        Me.lblDaysBack = New System.Windows.Forms.Label()
        Me.btnBrowse = New System.Windows.Forms.Button()
        Me.txtInstrumentDetalis = New System.Windows.Forms.TextBox()
        Me.lblInstrumentDetails = New System.Windows.Forms.Label()
        Me.grpTelegram.SuspendLayout()
        Me.grpTradeDetails.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnSave
        '
        Me.btnSave.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnSave.ImageKey = "save-icon-36533.png"
        Me.btnSave.ImageList = Me.ImageList1
        Me.btnSave.Location = New System.Drawing.Point(485, 5)
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
        'grpTelegram
        '
        Me.grpTelegram.Controls.Add(Me.txtTelegramTradeChatID)
        Me.grpTelegram.Controls.Add(Me.lblTelegramChatID)
        Me.grpTelegram.Controls.Add(Me.txtTelegramBotAPIKey)
        Me.grpTelegram.Controls.Add(Me.lblTelegramAPI)
        Me.grpTelegram.Location = New System.Drawing.Point(4, 220)
        Me.grpTelegram.Name = "grpTelegram"
        Me.grpTelegram.Size = New System.Drawing.Size(477, 91)
        Me.grpTelegram.TabIndex = 54
        Me.grpTelegram.TabStop = False
        Me.grpTelegram.Text = "Telegram Details"
        '
        'txtTelegramTradeChatID
        '
        Me.txtTelegramTradeChatID.Location = New System.Drawing.Point(227, 58)
        Me.txtTelegramTradeChatID.Margin = New System.Windows.Forms.Padding(4)
        Me.txtTelegramTradeChatID.Name = "txtTelegramTradeChatID"
        Me.txtTelegramTradeChatID.Size = New System.Drawing.Size(242, 22)
        Me.txtTelegramTradeChatID.TabIndex = 1
        Me.txtTelegramTradeChatID.Tag = "Trade Chat ID"
        '
        'lblTelegramChatID
        '
        Me.lblTelegramChatID.AutoSize = True
        Me.lblTelegramChatID.Location = New System.Drawing.Point(9, 61)
        Me.lblTelegramChatID.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblTelegramChatID.Name = "lblTelegramChatID"
        Me.lblTelegramChatID.Size = New System.Drawing.Size(129, 17)
        Me.lblTelegramChatID.TabIndex = 39
        Me.lblTelegramChatID.Text = "Trade Alert Chat ID"
        '
        'txtTelegramBotAPIKey
        '
        Me.txtTelegramBotAPIKey.Location = New System.Drawing.Point(227, 25)
        Me.txtTelegramBotAPIKey.Margin = New System.Windows.Forms.Padding(4)
        Me.txtTelegramBotAPIKey.Name = "txtTelegramBotAPIKey"
        Me.txtTelegramBotAPIKey.Size = New System.Drawing.Size(243, 22)
        Me.txtTelegramBotAPIKey.TabIndex = 0
        Me.txtTelegramBotAPIKey.Tag = "Bot API Key"
        '
        'lblTelegramAPI
        '
        Me.lblTelegramAPI.AutoSize = True
        Me.lblTelegramAPI.Location = New System.Drawing.Point(9, 28)
        Me.lblTelegramAPI.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblTelegramAPI.Name = "lblTelegramAPI"
        Me.lblTelegramAPI.Size = New System.Drawing.Size(82, 17)
        Me.lblTelegramAPI.TabIndex = 37
        Me.lblTelegramAPI.Text = "Bot API Key"
        '
        'grpTradeDetails
        '
        Me.grpTradeDetails.Controls.Add(Me.txtExitSDMultiplier)
        Me.grpTradeDetails.Controls.Add(Me.lblExitType)
        Me.grpTradeDetails.Controls.Add(Me.GroupBox1)
        Me.grpTradeDetails.Controls.Add(Me.lblExitSDMultiplier)
        Me.grpTradeDetails.Controls.Add(Me.txtEntrySDMultiplier)
        Me.grpTradeDetails.Controls.Add(Me.lblEntrySDMultiplier)
        Me.grpTradeDetails.Controls.Add(Me.txtSignalTimeframe)
        Me.grpTradeDetails.Controls.Add(Me.lblSignalTimeframe)
        Me.grpTradeDetails.Controls.Add(Me.txtDaysBack)
        Me.grpTradeDetails.Controls.Add(Me.lblDaysBack)
        Me.grpTradeDetails.Controls.Add(Me.btnBrowse)
        Me.grpTradeDetails.Controls.Add(Me.txtInstrumentDetalis)
        Me.grpTradeDetails.Controls.Add(Me.lblInstrumentDetails)
        Me.grpTradeDetails.Location = New System.Drawing.Point(4, 0)
        Me.grpTradeDetails.Margin = New System.Windows.Forms.Padding(4)
        Me.grpTradeDetails.Name = "grpTradeDetails"
        Me.grpTradeDetails.Padding = New System.Windows.Forms.Padding(4)
        Me.grpTradeDetails.Size = New System.Drawing.Size(477, 219)
        Me.grpTradeDetails.TabIndex = 1
        Me.grpTradeDetails.TabStop = False
        Me.grpTradeDetails.Text = "Trade Details"
        '
        'lblExitType
        '
        Me.lblExitType.AutoSize = True
        Me.lblExitType.Location = New System.Drawing.Point(8, 157)
        Me.lblExitType.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblExitType.Name = "lblExitType"
        Me.lblExitType.Size = New System.Drawing.Size(66, 17)
        Me.lblExitType.TabIndex = 59
        Me.lblExitType.Text = "Exit Type"
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.rbOppositeSide)
        Me.GroupBox1.Controls.Add(Me.rbSameSideExit)
        Me.GroupBox1.Location = New System.Drawing.Point(227, 141)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(242, 44)
        Me.GroupBox1.TabIndex = 4
        Me.GroupBox1.TabStop = False
        '
        'rbOppositeSide
        '
        Me.rbOppositeSide.AutoSize = True
        Me.rbOppositeSide.Location = New System.Drawing.Point(110, 12)
        Me.rbOppositeSide.Name = "rbOppositeSide"
        Me.rbOppositeSide.Size = New System.Drawing.Size(118, 21)
        Me.rbOppositeSide.TabIndex = 1
        Me.rbOppositeSide.TabStop = True
        Me.rbOppositeSide.Text = "Opposite Side"
        Me.rbOppositeSide.UseVisualStyleBackColor = True
        '
        'rbSameSideExit
        '
        Me.rbSameSideExit.AutoSize = True
        Me.rbSameSideExit.Location = New System.Drawing.Point(7, 12)
        Me.rbSameSideExit.Name = "rbSameSideExit"
        Me.rbSameSideExit.Size = New System.Drawing.Size(97, 21)
        Me.rbSameSideExit.TabIndex = 0
        Me.rbSameSideExit.TabStop = True
        Me.rbSameSideExit.Text = "Same Side"
        Me.rbSameSideExit.UseVisualStyleBackColor = True
        '
        'txtExitSDMultiplier
        '
        Me.txtExitSDMultiplier.Location = New System.Drawing.Point(227, 118)
        Me.txtExitSDMultiplier.Margin = New System.Windows.Forms.Padding(4)
        Me.txtExitSDMultiplier.Name = "txtExitSDMultiplier"
        Me.txtExitSDMultiplier.Size = New System.Drawing.Size(243, 22)
        Me.txtExitSDMultiplier.TabIndex = 3
        Me.txtExitSDMultiplier.Tag = "Exit SD Multiplier"
        '
        'lblExitSDMultiplier
        '
        Me.lblExitSDMultiplier.AutoSize = True
        Me.lblExitSDMultiplier.Location = New System.Drawing.Point(8, 120)
        Me.lblExitSDMultiplier.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblExitSDMultiplier.Name = "lblExitSDMultiplier"
        Me.lblExitSDMultiplier.Size = New System.Drawing.Size(113, 17)
        Me.lblExitSDMultiplier.TabIndex = 57
        Me.lblExitSDMultiplier.Text = "Exit SD Multiplier"
        '
        'txtEntrySDMultiplier
        '
        Me.txtEntrySDMultiplier.Location = New System.Drawing.Point(227, 85)
        Me.txtEntrySDMultiplier.Margin = New System.Windows.Forms.Padding(4)
        Me.txtEntrySDMultiplier.Name = "txtEntrySDMultiplier"
        Me.txtEntrySDMultiplier.Size = New System.Drawing.Size(243, 22)
        Me.txtEntrySDMultiplier.TabIndex = 2
        Me.txtEntrySDMultiplier.Tag = "Entry SD Multiplier"
        '
        'lblEntrySDMultiplier
        '
        Me.lblEntrySDMultiplier.AutoSize = True
        Me.lblEntrySDMultiplier.Location = New System.Drawing.Point(8, 87)
        Me.lblEntrySDMultiplier.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblEntrySDMultiplier.Name = "lblEntrySDMultiplier"
        Me.lblEntrySDMultiplier.Size = New System.Drawing.Size(124, 17)
        Me.lblEntrySDMultiplier.TabIndex = 55
        Me.lblEntrySDMultiplier.Text = "Entry SD Multiplier"
        '
        'txtSignalTimeframe
        '
        Me.txtSignalTimeframe.Location = New System.Drawing.Point(227, 20)
        Me.txtSignalTimeframe.Margin = New System.Windows.Forms.Padding(4)
        Me.txtSignalTimeframe.Name = "txtSignalTimeframe"
        Me.txtSignalTimeframe.Size = New System.Drawing.Size(243, 22)
        Me.txtSignalTimeframe.TabIndex = 0
        Me.txtSignalTimeframe.Tag = "Signal Timeframe"
        '
        'lblSignalTimeframe
        '
        Me.lblSignalTimeframe.AutoSize = True
        Me.lblSignalTimeframe.Location = New System.Drawing.Point(9, 21)
        Me.lblSignalTimeframe.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblSignalTimeframe.Name = "lblSignalTimeframe"
        Me.lblSignalTimeframe.Size = New System.Drawing.Size(154, 17)
        Me.lblSignalTimeframe.TabIndex = 53
        Me.lblSignalTimeframe.Text = "Signal Timeframe (min)"
        '
        'txtDaysBack
        '
        Me.txtDaysBack.Location = New System.Drawing.Point(227, 52)
        Me.txtDaysBack.Margin = New System.Windows.Forms.Padding(4)
        Me.txtDaysBack.Name = "txtDaysBack"
        Me.txtDaysBack.Size = New System.Drawing.Size(243, 22)
        Me.txtDaysBack.TabIndex = 1
        Me.txtDaysBack.Tag = "Days Back"
        '
        'lblDaysBack
        '
        Me.lblDaysBack.AutoSize = True
        Me.lblDaysBack.Location = New System.Drawing.Point(8, 54)
        Me.lblDaysBack.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblDaysBack.Name = "lblDaysBack"
        Me.lblDaysBack.Size = New System.Drawing.Size(75, 17)
        Me.lblDaysBack.TabIndex = 39
        Me.lblDaysBack.Text = "Days Back"
        '
        'btnBrowse
        '
        Me.btnBrowse.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnBrowse.Location = New System.Drawing.Point(428, 186)
        Me.btnBrowse.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnBrowse.Name = "btnBrowse"
        Me.btnBrowse.Size = New System.Drawing.Size(42, 23)
        Me.btnBrowse.TabIndex = 5
        Me.btnBrowse.Text = "..."
        Me.btnBrowse.UseVisualStyleBackColor = True
        '
        'txtInstrumentDetalis
        '
        Me.txtInstrumentDetalis.Location = New System.Drawing.Point(227, 187)
        Me.txtInstrumentDetalis.Margin = New System.Windows.Forms.Padding(4)
        Me.txtInstrumentDetalis.Name = "txtInstrumentDetalis"
        Me.txtInstrumentDetalis.ReadOnly = True
        Me.txtInstrumentDetalis.Size = New System.Drawing.Size(194, 22)
        Me.txtInstrumentDetalis.TabIndex = 5
        '
        'lblInstrumentDetails
        '
        Me.lblInstrumentDetails.AutoSize = True
        Me.lblInstrumentDetails.Location = New System.Drawing.Point(8, 190)
        Me.lblInstrumentDetails.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblInstrumentDetails.Name = "lblInstrumentDetails"
        Me.lblInstrumentDetails.Size = New System.Drawing.Size(121, 17)
        Me.lblInstrumentDetails.TabIndex = 8
        Me.lblInstrumentDetails.Text = "Instrument Details"
        '
        'frmNFOSettings
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(602, 314)
        Me.Controls.Add(Me.grpTelegram)
        Me.Controls.Add(Me.grpTradeDetails)
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
        Me.grpTradeDetails.ResumeLayout(False)
        Me.grpTradeDetails.PerformLayout()
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents btnSave As Button
    Friend WithEvents ImageList1 As ImageList
    Friend WithEvents opnFileSettings As OpenFileDialog
    Friend WithEvents grpTelegram As GroupBox
    Friend WithEvents txtTelegramTradeChatID As TextBox
    Friend WithEvents lblTelegramChatID As Label
    Friend WithEvents txtTelegramBotAPIKey As TextBox
    Friend WithEvents lblTelegramAPI As Label
    Friend WithEvents grpTradeDetails As GroupBox
    Friend WithEvents txtSignalTimeframe As TextBox
    Friend WithEvents lblSignalTimeframe As Label
    Friend WithEvents txtDaysBack As TextBox
    Friend WithEvents lblDaysBack As Label
    Friend WithEvents btnBrowse As Button
    Friend WithEvents txtInstrumentDetalis As TextBox
    Friend WithEvents lblInstrumentDetails As Label
    Friend WithEvents txtEntrySDMultiplier As TextBox
    Friend WithEvents lblEntrySDMultiplier As Label
    Friend WithEvents txtExitSDMultiplier As TextBox
    Friend WithEvents lblExitSDMultiplier As Label
    Friend WithEvents lblExitType As Label
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents rbOppositeSide As RadioButton
    Friend WithEvents rbSameSideExit As RadioButton
End Class
