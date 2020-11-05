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
        Me.txtInitialInvestment = New System.Windows.Forms.TextBox()
        Me.lblInitialInvestment = New System.Windows.Forms.Label()
        Me.txtExpectedIncreaseEachPeriod = New System.Windows.Forms.TextBox()
        Me.lblExpectedIncreaseEachPeriod = New System.Windows.Forms.Label()
        Me.dtpckrTradeEntryTime = New System.Windows.Forms.DateTimePicker()
        Me.lblTradeEntryTime = New System.Windows.Forms.Label()
        Me.btnBrowse = New System.Windows.Forms.Button()
        Me.txtInstrumentDetalis = New System.Windows.Forms.TextBox()
        Me.lblInstrumentDetails = New System.Windows.Forms.Label()
        Me.grpTelegram.SuspendLayout()
        Me.grpTradeDetails.SuspendLayout()
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
        Me.grpTelegram.Location = New System.Drawing.Point(4, 170)
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
        Me.grpTradeDetails.Controls.Add(Me.txtInitialInvestment)
        Me.grpTradeDetails.Controls.Add(Me.lblInitialInvestment)
        Me.grpTradeDetails.Controls.Add(Me.txtExpectedIncreaseEachPeriod)
        Me.grpTradeDetails.Controls.Add(Me.lblExpectedIncreaseEachPeriod)
        Me.grpTradeDetails.Controls.Add(Me.dtpckrTradeEntryTime)
        Me.grpTradeDetails.Controls.Add(Me.lblTradeEntryTime)
        Me.grpTradeDetails.Controls.Add(Me.btnBrowse)
        Me.grpTradeDetails.Controls.Add(Me.txtInstrumentDetalis)
        Me.grpTradeDetails.Controls.Add(Me.lblInstrumentDetails)
        Me.grpTradeDetails.Location = New System.Drawing.Point(4, 0)
        Me.grpTradeDetails.Margin = New System.Windows.Forms.Padding(4)
        Me.grpTradeDetails.Name = "grpTradeDetails"
        Me.grpTradeDetails.Padding = New System.Windows.Forms.Padding(4)
        Me.grpTradeDetails.Size = New System.Drawing.Size(477, 169)
        Me.grpTradeDetails.TabIndex = 53
        Me.grpTradeDetails.TabStop = False
        Me.grpTradeDetails.Text = "Trade Details"
        '
        'txtInitialInvestment
        '
        Me.txtInitialInvestment.Location = New System.Drawing.Point(227, 56)
        Me.txtInitialInvestment.Margin = New System.Windows.Forms.Padding(4)
        Me.txtInitialInvestment.Name = "txtInitialInvestment"
        Me.txtInitialInvestment.Size = New System.Drawing.Size(243, 22)
        Me.txtInitialInvestment.TabIndex = 1
        Me.txtInitialInvestment.Tag = "Initial Investment"
        '
        'lblInitialInvestment
        '
        Me.lblInitialInvestment.AutoSize = True
        Me.lblInitialInvestment.Location = New System.Drawing.Point(9, 57)
        Me.lblInitialInvestment.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblInitialInvestment.Name = "lblInitialInvestment"
        Me.lblInitialInvestment.Size = New System.Drawing.Size(112, 17)
        Me.lblInitialInvestment.TabIndex = 53
        Me.lblInitialInvestment.Text = "Initial Investment"
        '
        'txtExpectedIncreaseEachPeriod
        '
        Me.txtExpectedIncreaseEachPeriod.Location = New System.Drawing.Point(227, 91)
        Me.txtExpectedIncreaseEachPeriod.Margin = New System.Windows.Forms.Padding(4)
        Me.txtExpectedIncreaseEachPeriod.Name = "txtExpectedIncreaseEachPeriod"
        Me.txtExpectedIncreaseEachPeriod.Size = New System.Drawing.Size(243, 22)
        Me.txtExpectedIncreaseEachPeriod.TabIndex = 2
        Me.txtExpectedIncreaseEachPeriod.Tag = "Expected Increase Each Period"
        '
        'lblExpectedIncreaseEachPeriod
        '
        Me.lblExpectedIncreaseEachPeriod.AutoSize = True
        Me.lblExpectedIncreaseEachPeriod.Location = New System.Drawing.Point(8, 93)
        Me.lblExpectedIncreaseEachPeriod.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblExpectedIncreaseEachPeriod.Name = "lblExpectedIncreaseEachPeriod"
        Me.lblExpectedIncreaseEachPeriod.Size = New System.Drawing.Size(205, 17)
        Me.lblExpectedIncreaseEachPeriod.TabIndex = 39
        Me.lblExpectedIncreaseEachPeriod.Text = "Expected Increase Each Period"
        '
        'dtpckrTradeEntryTime
        '
        Me.dtpckrTradeEntryTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpckrTradeEntryTime.Location = New System.Drawing.Point(227, 21)
        Me.dtpckrTradeEntryTime.Name = "dtpckrTradeEntryTime"
        Me.dtpckrTradeEntryTime.ShowUpDown = True
        Me.dtpckrTradeEntryTime.Size = New System.Drawing.Size(242, 22)
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
        'btnBrowse
        '
        Me.btnBrowse.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnBrowse.Location = New System.Drawing.Point(428, 128)
        Me.btnBrowse.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnBrowse.Name = "btnBrowse"
        Me.btnBrowse.Size = New System.Drawing.Size(42, 23)
        Me.btnBrowse.TabIndex = 3
        Me.btnBrowse.Text = "..."
        Me.btnBrowse.UseVisualStyleBackColor = True
        '
        'txtInstrumentDetalis
        '
        Me.txtInstrumentDetalis.Location = New System.Drawing.Point(227, 129)
        Me.txtInstrumentDetalis.Margin = New System.Windows.Forms.Padding(4)
        Me.txtInstrumentDetalis.Name = "txtInstrumentDetalis"
        Me.txtInstrumentDetalis.ReadOnly = True
        Me.txtInstrumentDetalis.Size = New System.Drawing.Size(194, 22)
        Me.txtInstrumentDetalis.TabIndex = 3
        '
        'lblInstrumentDetails
        '
        Me.lblInstrumentDetails.AutoSize = True
        Me.lblInstrumentDetails.Location = New System.Drawing.Point(8, 132)
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
        Me.ClientSize = New System.Drawing.Size(602, 266)
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
    Friend WithEvents txtInitialInvestment As TextBox
    Friend WithEvents lblInitialInvestment As Label
    Friend WithEvents txtExpectedIncreaseEachPeriod As TextBox
    Friend WithEvents lblExpectedIncreaseEachPeriod As Label
    Friend WithEvents dtpckrTradeEntryTime As DateTimePicker
    Friend WithEvents lblTradeEntryTime As Label
    Friend WithEvents btnBrowse As Button
    Friend WithEvents txtInstrumentDetalis As TextBox
    Friend WithEvents lblInstrumentDetails As Label
End Class
