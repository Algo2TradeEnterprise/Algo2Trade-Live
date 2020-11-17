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
        Me.lblTelegramChatID = New System.Windows.Forms.Label()
        Me.lblTelegramAPI = New System.Windows.Forms.Label()
        Me.txtMaxMartingaleIteration = New System.Windows.Forms.TextBox()
        Me.lblMaxMartingaleIteration = New System.Windows.Forms.Label()
        Me.txtSignalTimeframe = New System.Windows.Forms.TextBox()
        Me.lblSignalTimeframe = New System.Windows.Forms.Label()
        Me.txtMartingaleMultiplier = New System.Windows.Forms.TextBox()
        Me.lblMartingaleMultiplier = New System.Windows.Forms.Label()
        Me.dtpckrTradeEntryTime = New System.Windows.Forms.DateTimePicker()
        Me.lblTradeEntryTime = New System.Windows.Forms.Label()
        Me.btnBrowse = New System.Windows.Forms.Button()
        Me.txtInstrumentDetalis = New System.Windows.Forms.TextBox()
        Me.txtTelegramBotAPIKey = New System.Windows.Forms.TextBox()
        Me.lblInstrumentDetails = New System.Windows.Forms.Label()
        Me.txtTelegramCapitalChatID = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.grpTelegram = New System.Windows.Forms.GroupBox()
        Me.txtTelegramTradeChatID = New System.Windows.Forms.TextBox()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.opnFileSettings = New System.Windows.Forms.OpenFileDialog()
        Me.ImageList1 = New System.Windows.Forms.ImageList(Me.components)
        Me.btnSave = New System.Windows.Forms.Button()
        Me.txtInitialCapital = New System.Windows.Forms.TextBox()
        Me.lblInitialCapital = New System.Windows.Forms.Label()
        Me.grpTelegram.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        Me.SuspendLayout()
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
        'txtMaxMartingaleIteration
        '
        Me.txtMaxMartingaleIteration.Location = New System.Drawing.Point(227, 119)
        Me.txtMaxMartingaleIteration.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMaxMartingaleIteration.Name = "txtMaxMartingaleIteration"
        Me.txtMaxMartingaleIteration.Size = New System.Drawing.Size(243, 22)
        Me.txtMaxMartingaleIteration.TabIndex = 3
        Me.txtMaxMartingaleIteration.Tag = "Max Martingale Iteration"
        '
        'lblMaxMartingaleIteration
        '
        Me.lblMaxMartingaleIteration.AutoSize = True
        Me.lblMaxMartingaleIteration.Location = New System.Drawing.Point(8, 123)
        Me.lblMaxMartingaleIteration.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMaxMartingaleIteration.Name = "lblMaxMartingaleIteration"
        Me.lblMaxMartingaleIteration.Size = New System.Drawing.Size(158, 17)
        Me.lblMaxMartingaleIteration.TabIndex = 59
        Me.lblMaxMartingaleIteration.Text = "Max Martingale Iteration"
        '
        'txtSignalTimeframe
        '
        Me.txtSignalTimeframe.Location = New System.Drawing.Point(227, 50)
        Me.txtSignalTimeframe.Margin = New System.Windows.Forms.Padding(4)
        Me.txtSignalTimeframe.Name = "txtSignalTimeframe"
        Me.txtSignalTimeframe.Size = New System.Drawing.Size(243, 22)
        Me.txtSignalTimeframe.TabIndex = 1
        Me.txtSignalTimeframe.Tag = "Signal Timeframe"
        '
        'lblSignalTimeframe
        '
        Me.lblSignalTimeframe.AutoSize = True
        Me.lblSignalTimeframe.Location = New System.Drawing.Point(9, 53)
        Me.lblSignalTimeframe.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblSignalTimeframe.Name = "lblSignalTimeframe"
        Me.lblSignalTimeframe.Size = New System.Drawing.Size(154, 17)
        Me.lblSignalTimeframe.TabIndex = 53
        Me.lblSignalTimeframe.Text = "Signal Timeframe (min)"
        '
        'txtMartingaleMultiplier
        '
        Me.txtMartingaleMultiplier.Location = New System.Drawing.Point(227, 84)
        Me.txtMartingaleMultiplier.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMartingaleMultiplier.Name = "txtMartingaleMultiplier"
        Me.txtMartingaleMultiplier.Size = New System.Drawing.Size(243, 22)
        Me.txtMartingaleMultiplier.TabIndex = 2
        Me.txtMartingaleMultiplier.Tag = "Martingale Multiplier"
        '
        'lblMartingaleMultiplier
        '
        Me.lblMartingaleMultiplier.AutoSize = True
        Me.lblMartingaleMultiplier.Location = New System.Drawing.Point(8, 88)
        Me.lblMartingaleMultiplier.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMartingaleMultiplier.Name = "lblMartingaleMultiplier"
        Me.lblMartingaleMultiplier.Size = New System.Drawing.Size(134, 17)
        Me.lblMartingaleMultiplier.TabIndex = 39
        Me.lblMartingaleMultiplier.Text = "Martingale Multiplier"
        '
        'dtpckrTradeEntryTime
        '
        Me.dtpckrTradeEntryTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpckrTradeEntryTime.Location = New System.Drawing.Point(227, 16)
        Me.dtpckrTradeEntryTime.Name = "dtpckrTradeEntryTime"
        Me.dtpckrTradeEntryTime.ShowUpDown = True
        Me.dtpckrTradeEntryTime.Size = New System.Drawing.Size(242, 22)
        Me.dtpckrTradeEntryTime.TabIndex = 0
        Me.dtpckrTradeEntryTime.Value = New Date(2019, 3, 12, 0, 0, 0, 0)
        '
        'lblTradeEntryTime
        '
        Me.lblTradeEntryTime.AutoSize = True
        Me.lblTradeEntryTime.Location = New System.Drawing.Point(9, 18)
        Me.lblTradeEntryTime.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblTradeEntryTime.Name = "lblTradeEntryTime"
        Me.lblTradeEntryTime.Size = New System.Drawing.Size(118, 17)
        Me.lblTradeEntryTime.TabIndex = 19
        Me.lblTradeEntryTime.Text = "Trade Entry Time"
        '
        'btnBrowse
        '
        Me.btnBrowse.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnBrowse.Location = New System.Drawing.Point(428, 153)
        Me.btnBrowse.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnBrowse.Name = "btnBrowse"
        Me.btnBrowse.Size = New System.Drawing.Size(42, 23)
        Me.btnBrowse.TabIndex = 4
        Me.btnBrowse.Text = "..."
        Me.btnBrowse.UseVisualStyleBackColor = True
        '
        'txtInstrumentDetalis
        '
        Me.txtInstrumentDetalis.Location = New System.Drawing.Point(227, 154)
        Me.txtInstrumentDetalis.Margin = New System.Windows.Forms.Padding(4)
        Me.txtInstrumentDetalis.Name = "txtInstrumentDetalis"
        Me.txtInstrumentDetalis.ReadOnly = True
        Me.txtInstrumentDetalis.Size = New System.Drawing.Size(194, 22)
        Me.txtInstrumentDetalis.TabIndex = 4
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
        'lblInstrumentDetails
        '
        Me.lblInstrumentDetails.AutoSize = True
        Me.lblInstrumentDetails.Location = New System.Drawing.Point(8, 157)
        Me.lblInstrumentDetails.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblInstrumentDetails.Name = "lblInstrumentDetails"
        Me.lblInstrumentDetails.Size = New System.Drawing.Size(121, 17)
        Me.lblInstrumentDetails.TabIndex = 8
        Me.lblInstrumentDetails.Text = "Instrument Details"
        '
        'txtTelegramCapitalChatID
        '
        Me.txtTelegramCapitalChatID.Location = New System.Drawing.Point(227, 90)
        Me.txtTelegramCapitalChatID.Margin = New System.Windows.Forms.Padding(4)
        Me.txtTelegramCapitalChatID.Name = "txtTelegramCapitalChatID"
        Me.txtTelegramCapitalChatID.Size = New System.Drawing.Size(242, 22)
        Me.txtTelegramCapitalChatID.TabIndex = 2
        Me.txtTelegramCapitalChatID.Tag = "Capital Chat ID"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(9, 93)
        Me.Label1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(134, 17)
        Me.Label1.TabIndex = 41
        Me.Label1.Text = "Capital Alert Chat ID"
        '
        'grpTelegram
        '
        Me.grpTelegram.Controls.Add(Me.txtTelegramCapitalChatID)
        Me.grpTelegram.Controls.Add(Me.Label1)
        Me.grpTelegram.Controls.Add(Me.txtTelegramTradeChatID)
        Me.grpTelegram.Controls.Add(Me.lblTelegramChatID)
        Me.grpTelegram.Controls.Add(Me.txtTelegramBotAPIKey)
        Me.grpTelegram.Controls.Add(Me.lblTelegramAPI)
        Me.grpTelegram.Location = New System.Drawing.Point(3, 232)
        Me.grpTelegram.Name = "grpTelegram"
        Me.grpTelegram.Size = New System.Drawing.Size(477, 119)
        Me.grpTelegram.TabIndex = 55
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
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.txtInitialCapital)
        Me.GroupBox1.Controls.Add(Me.lblInitialCapital)
        Me.GroupBox1.Controls.Add(Me.txtMaxMartingaleIteration)
        Me.GroupBox1.Controls.Add(Me.lblMaxMartingaleIteration)
        Me.GroupBox1.Controls.Add(Me.txtSignalTimeframe)
        Me.GroupBox1.Controls.Add(Me.lblSignalTimeframe)
        Me.GroupBox1.Controls.Add(Me.txtMartingaleMultiplier)
        Me.GroupBox1.Controls.Add(Me.lblMartingaleMultiplier)
        Me.GroupBox1.Controls.Add(Me.dtpckrTradeEntryTime)
        Me.GroupBox1.Controls.Add(Me.lblTradeEntryTime)
        Me.GroupBox1.Controls.Add(Me.btnBrowse)
        Me.GroupBox1.Controls.Add(Me.txtInstrumentDetalis)
        Me.GroupBox1.Controls.Add(Me.lblInstrumentDetails)
        Me.GroupBox1.Location = New System.Drawing.Point(3, 2)
        Me.GroupBox1.Margin = New System.Windows.Forms.Padding(4)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Padding = New System.Windows.Forms.Padding(4)
        Me.GroupBox1.Size = New System.Drawing.Size(477, 223)
        Me.GroupBox1.TabIndex = 54
        Me.GroupBox1.TabStop = False
        '
        'opnFileSettings
        '
        '
        'ImageList1
        '
        Me.ImageList1.ImageStream = CType(resources.GetObject("ImageList1.ImageStream"), System.Windows.Forms.ImageListStreamer)
        Me.ImageList1.TransparentColor = System.Drawing.Color.Transparent
        Me.ImageList1.Images.SetKeyName(0, "save-icon-36533.png")
        '
        'btnSave
        '
        Me.btnSave.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnSave.ImageKey = "save-icon-36533.png"
        Me.btnSave.ImageList = Me.ImageList1
        Me.btnSave.Location = New System.Drawing.Point(487, 9)
        Me.btnSave.Margin = New System.Windows.Forms.Padding(4)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(112, 58)
        Me.btnSave.TabIndex = 0
        Me.btnSave.Text = "&Save"
        Me.btnSave.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.btnSave.UseVisualStyleBackColor = True
        '
        'txtInitialCapital
        '
        Me.txtInitialCapital.Location = New System.Drawing.Point(227, 188)
        Me.txtInitialCapital.Margin = New System.Windows.Forms.Padding(4)
        Me.txtInitialCapital.Name = "txtInitialCapital"
        Me.txtInitialCapital.Size = New System.Drawing.Size(243, 22)
        Me.txtInitialCapital.TabIndex = 5
        Me.txtInitialCapital.Tag = "Initial Capital"
        '
        'lblInitialCapital
        '
        Me.lblInitialCapital.AutoSize = True
        Me.lblInitialCapital.Location = New System.Drawing.Point(9, 191)
        Me.lblInitialCapital.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblInitialCapital.Name = "lblInitialCapital"
        Me.lblInitialCapital.Size = New System.Drawing.Size(87, 17)
        Me.lblInitialCapital.TabIndex = 61
        Me.lblInitialCapital.Text = "Initial Capital"
        '
        'frmNFOSettings
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(602, 355)
        Me.Controls.Add(Me.grpTelegram)
        Me.Controls.Add(Me.GroupBox1)
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
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents lblTelegramChatID As Label
    Friend WithEvents lblTelegramAPI As Label
    Friend WithEvents txtMaxMartingaleIteration As TextBox
    Friend WithEvents lblMaxMartingaleIteration As Label
    Friend WithEvents txtSignalTimeframe As TextBox
    Friend WithEvents lblSignalTimeframe As Label
    Friend WithEvents txtMartingaleMultiplier As TextBox
    Friend WithEvents lblMartingaleMultiplier As Label
    Friend WithEvents dtpckrTradeEntryTime As DateTimePicker
    Friend WithEvents lblTradeEntryTime As Label
    Friend WithEvents btnBrowse As Button
    Friend WithEvents txtInstrumentDetalis As TextBox
    Friend WithEvents txtTelegramBotAPIKey As TextBox
    Friend WithEvents lblInstrumentDetails As Label
    Friend WithEvents txtTelegramCapitalChatID As TextBox
    Friend WithEvents Label1 As Label
    Friend WithEvents grpTelegram As GroupBox
    Friend WithEvents txtTelegramTradeChatID As TextBox
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents opnFileSettings As OpenFileDialog
    Friend WithEvents ImageList1 As ImageList
    Friend WithEvents btnSave As Button
    Friend WithEvents txtInitialCapital As TextBox
    Friend WithEvents lblInitialCapital As Label
End Class
