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
        Me.grpSupertrend = New System.Windows.Forms.GroupBox()
        Me.txtSupertrendMultiplier = New System.Windows.Forms.TextBox()
        Me.lblLTEMA2Period = New System.Windows.Forms.Label()
        Me.txtSupertrendPeriod = New System.Windows.Forms.TextBox()
        Me.lblLTEMA1Period = New System.Windows.Forms.Label()
        Me.grpSettings = New System.Windows.Forms.GroupBox()
        Me.dtpckrEODExitTime = New System.Windows.Forms.DateTimePicker()
        Me.lblEODExitTime = New System.Windows.Forms.Label()
        Me.dtpckrLastTradeEntryTime = New System.Windows.Forms.DateTimePicker()
        Me.lblLastTradeEntryTime = New System.Windows.Forms.Label()
        Me.dtpckrTradeStartTime = New System.Windows.Forms.DateTimePicker()
        Me.lblTradeStartTime = New System.Windows.Forms.Label()
        Me.txtStrikeRangePer = New System.Windows.Forms.TextBox()
        Me.lblStrikeRangePer = New System.Windows.Forms.Label()
        Me.btnBrowse = New System.Windows.Forms.Button()
        Me.txtInstrumentDetalis = New System.Windows.Forms.TextBox()
        Me.lblInstrumentDetails = New System.Windows.Forms.Label()
        Me.txtSignalTimeframe = New System.Windows.Forms.TextBox()
        Me.lblSignalTimeframe = New System.Windows.Forms.Label()
        Me.grpSupertrend.SuspendLayout()
        Me.grpSettings.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnSave
        '
        Me.btnSave.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnSave.ImageKey = "save-icon-36533.png"
        Me.btnSave.ImageList = Me.ImageList1
        Me.btnSave.Location = New System.Drawing.Point(461, 5)
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
        'grpSupertrend
        '
        Me.grpSupertrend.Controls.Add(Me.txtSupertrendMultiplier)
        Me.grpSupertrend.Controls.Add(Me.lblLTEMA2Period)
        Me.grpSupertrend.Controls.Add(Me.txtSupertrendPeriod)
        Me.grpSupertrend.Controls.Add(Me.lblLTEMA1Period)
        Me.grpSupertrend.Location = New System.Drawing.Point(2, 213)
        Me.grpSupertrend.Name = "grpSupertrend"
        Me.grpSupertrend.Size = New System.Drawing.Size(451, 92)
        Me.grpSupertrend.TabIndex = 2
        Me.grpSupertrend.TabStop = False
        Me.grpSupertrend.Text = "Supertrend Settings"
        '
        'txtSupertrendMultiplier
        '
        Me.txtSupertrendMultiplier.Location = New System.Drawing.Point(175, 56)
        Me.txtSupertrendMultiplier.Margin = New System.Windows.Forms.Padding(4)
        Me.txtSupertrendMultiplier.Name = "txtSupertrendMultiplier"
        Me.txtSupertrendMultiplier.Size = New System.Drawing.Size(255, 22)
        Me.txtSupertrendMultiplier.TabIndex = 1
        Me.txtSupertrendMultiplier.Tag = "Supertrend Multiplier"
        '
        'lblLTEMA2Period
        '
        Me.lblLTEMA2Period.AutoSize = True
        Me.lblLTEMA2Period.Location = New System.Drawing.Point(9, 60)
        Me.lblLTEMA2Period.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblLTEMA2Period.Name = "lblLTEMA2Period"
        Me.lblLTEMA2Period.Size = New System.Drawing.Size(139, 17)
        Me.lblLTEMA2Period.TabIndex = 35
        Me.lblLTEMA2Period.Text = "Supertrend Multiplier"
        '
        'txtSupertrendPeriod
        '
        Me.txtSupertrendPeriod.Location = New System.Drawing.Point(174, 25)
        Me.txtSupertrendPeriod.Margin = New System.Windows.Forms.Padding(4)
        Me.txtSupertrendPeriod.Name = "txtSupertrendPeriod"
        Me.txtSupertrendPeriod.Size = New System.Drawing.Size(256, 22)
        Me.txtSupertrendPeriod.TabIndex = 0
        Me.txtSupertrendPeriod.Tag = "Supertrend Period"
        '
        'lblLTEMA1Period
        '
        Me.lblLTEMA1Period.AutoSize = True
        Me.lblLTEMA1Period.Location = New System.Drawing.Point(10, 28)
        Me.lblLTEMA1Period.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblLTEMA1Period.Name = "lblLTEMA1Period"
        Me.lblLTEMA1Period.Size = New System.Drawing.Size(124, 17)
        Me.lblLTEMA1Period.TabIndex = 31
        Me.lblLTEMA1Period.Text = "Supertrend Period"
        '
        'grpSettings
        '
        Me.grpSettings.Controls.Add(Me.dtpckrEODExitTime)
        Me.grpSettings.Controls.Add(Me.lblEODExitTime)
        Me.grpSettings.Controls.Add(Me.dtpckrLastTradeEntryTime)
        Me.grpSettings.Controls.Add(Me.lblLastTradeEntryTime)
        Me.grpSettings.Controls.Add(Me.dtpckrTradeStartTime)
        Me.grpSettings.Controls.Add(Me.lblTradeStartTime)
        Me.grpSettings.Controls.Add(Me.txtStrikeRangePer)
        Me.grpSettings.Controls.Add(Me.lblStrikeRangePer)
        Me.grpSettings.Controls.Add(Me.btnBrowse)
        Me.grpSettings.Controls.Add(Me.txtInstrumentDetalis)
        Me.grpSettings.Controls.Add(Me.lblInstrumentDetails)
        Me.grpSettings.Controls.Add(Me.txtSignalTimeframe)
        Me.grpSettings.Controls.Add(Me.lblSignalTimeframe)
        Me.grpSettings.Location = New System.Drawing.Point(2, 0)
        Me.grpSettings.Margin = New System.Windows.Forms.Padding(4)
        Me.grpSettings.Name = "grpSettings"
        Me.grpSettings.Padding = New System.Windows.Forms.Padding(4)
        Me.grpSettings.Size = New System.Drawing.Size(451, 212)
        Me.grpSettings.TabIndex = 1
        Me.grpSettings.TabStop = False
        '
        'dtpckrEODExitTime
        '
        Me.dtpckrEODExitTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpckrEODExitTime.Location = New System.Drawing.Point(175, 114)
        Me.dtpckrEODExitTime.Name = "dtpckrEODExitTime"
        Me.dtpckrEODExitTime.ShowUpDown = True
        Me.dtpckrEODExitTime.Size = New System.Drawing.Size(255, 22)
        Me.dtpckrEODExitTime.TabIndex = 3
        '
        'lblEODExitTime
        '
        Me.lblEODExitTime.AutoSize = True
        Me.lblEODExitTime.Location = New System.Drawing.Point(8, 117)
        Me.lblEODExitTime.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblEODExitTime.Name = "lblEODExitTime"
        Me.lblEODExitTime.Size = New System.Drawing.Size(99, 17)
        Me.lblEODExitTime.TabIndex = 16
        Me.lblEODExitTime.Text = "EOD Exit Time"
        '
        'dtpckrLastTradeEntryTime
        '
        Me.dtpckrLastTradeEntryTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpckrLastTradeEntryTime.Location = New System.Drawing.Point(175, 81)
        Me.dtpckrLastTradeEntryTime.Name = "dtpckrLastTradeEntryTime"
        Me.dtpckrLastTradeEntryTime.ShowUpDown = True
        Me.dtpckrLastTradeEntryTime.Size = New System.Drawing.Size(255, 22)
        Me.dtpckrLastTradeEntryTime.TabIndex = 2
        '
        'lblLastTradeEntryTime
        '
        Me.lblLastTradeEntryTime.AutoSize = True
        Me.lblLastTradeEntryTime.Location = New System.Drawing.Point(8, 84)
        Me.lblLastTradeEntryTime.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblLastTradeEntryTime.Name = "lblLastTradeEntryTime"
        Me.lblLastTradeEntryTime.Size = New System.Drawing.Size(149, 17)
        Me.lblLastTradeEntryTime.TabIndex = 14
        Me.lblLastTradeEntryTime.Text = "Last Trade Entry Time"
        '
        'dtpckrTradeStartTime
        '
        Me.dtpckrTradeStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpckrTradeStartTime.Location = New System.Drawing.Point(175, 48)
        Me.dtpckrTradeStartTime.Name = "dtpckrTradeStartTime"
        Me.dtpckrTradeStartTime.ShowUpDown = True
        Me.dtpckrTradeStartTime.Size = New System.Drawing.Size(255, 22)
        Me.dtpckrTradeStartTime.TabIndex = 1
        '
        'lblTradeStartTime
        '
        Me.lblTradeStartTime.AutoSize = True
        Me.lblTradeStartTime.Location = New System.Drawing.Point(8, 51)
        Me.lblTradeStartTime.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblTradeStartTime.Name = "lblTradeStartTime"
        Me.lblTradeStartTime.Size = New System.Drawing.Size(115, 17)
        Me.lblTradeStartTime.TabIndex = 12
        Me.lblTradeStartTime.Text = "Trade Start Time"
        '
        'txtStrikeRangePer
        '
        Me.txtStrikeRangePer.Location = New System.Drawing.Point(175, 146)
        Me.txtStrikeRangePer.Margin = New System.Windows.Forms.Padding(4)
        Me.txtStrikeRangePer.Name = "txtStrikeRangePer"
        Me.txtStrikeRangePer.Size = New System.Drawing.Size(255, 22)
        Me.txtStrikeRangePer.TabIndex = 4
        Me.txtStrikeRangePer.Tag = "Strike Price Range %"
        '
        'lblStrikeRangePer
        '
        Me.lblStrikeRangePer.AutoSize = True
        Me.lblStrikeRangePer.Location = New System.Drawing.Point(9, 149)
        Me.lblStrikeRangePer.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblStrikeRangePer.Name = "lblStrikeRangePer"
        Me.lblStrikeRangePer.Size = New System.Drawing.Size(142, 17)
        Me.lblStrikeRangePer.TabIndex = 10
        Me.lblStrikeRangePer.Text = "Strike Price Range %"
        '
        'btnBrowse
        '
        Me.btnBrowse.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnBrowse.Location = New System.Drawing.Point(404, 177)
        Me.btnBrowse.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnBrowse.Name = "btnBrowse"
        Me.btnBrowse.Size = New System.Drawing.Size(40, 23)
        Me.btnBrowse.TabIndex = 5
        Me.btnBrowse.Text = "..."
        Me.btnBrowse.UseVisualStyleBackColor = True
        '
        'txtInstrumentDetalis
        '
        Me.txtInstrumentDetalis.Location = New System.Drawing.Point(174, 178)
        Me.txtInstrumentDetalis.Margin = New System.Windows.Forms.Padding(4)
        Me.txtInstrumentDetalis.Name = "txtInstrumentDetalis"
        Me.txtInstrumentDetalis.ReadOnly = True
        Me.txtInstrumentDetalis.Size = New System.Drawing.Size(223, 22)
        Me.txtInstrumentDetalis.TabIndex = 6
        '
        'lblInstrumentDetails
        '
        Me.lblInstrumentDetails.AutoSize = True
        Me.lblInstrumentDetails.Location = New System.Drawing.Point(9, 181)
        Me.lblInstrumentDetails.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblInstrumentDetails.Name = "lblInstrumentDetails"
        Me.lblInstrumentDetails.Size = New System.Drawing.Size(121, 17)
        Me.lblInstrumentDetails.TabIndex = 8
        Me.lblInstrumentDetails.Text = "Instrument Details"
        '
        'txtSignalTimeframe
        '
        Me.txtSignalTimeframe.Location = New System.Drawing.Point(175, 15)
        Me.txtSignalTimeframe.Margin = New System.Windows.Forms.Padding(4)
        Me.txtSignalTimeframe.Name = "txtSignalTimeframe"
        Me.txtSignalTimeframe.Size = New System.Drawing.Size(255, 22)
        Me.txtSignalTimeframe.TabIndex = 0
        Me.txtSignalTimeframe.Tag = "Signal Timeframe"
        '
        'lblSignalTimeframe
        '
        Me.lblSignalTimeframe.AutoSize = True
        Me.lblSignalTimeframe.Location = New System.Drawing.Point(9, 18)
        Me.lblSignalTimeframe.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblSignalTimeframe.Name = "lblSignalTimeframe"
        Me.lblSignalTimeframe.Size = New System.Drawing.Size(150, 17)
        Me.lblSignalTimeframe.TabIndex = 3
        Me.lblSignalTimeframe.Text = "Signal Timeframe(min)"
        '
        'frmNFOSettings
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(580, 308)
        Me.Controls.Add(Me.grpSupertrend)
        Me.Controls.Add(Me.grpSettings)
        Me.Controls.Add(Me.btnSave)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmNFOSettings"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Settings"
        Me.grpSupertrend.ResumeLayout(False)
        Me.grpSupertrend.PerformLayout()
        Me.grpSettings.ResumeLayout(False)
        Me.grpSettings.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents btnSave As Button
    Friend WithEvents ImageList1 As ImageList
    Friend WithEvents opnFileSettings As OpenFileDialog
    Friend WithEvents grpSupertrend As GroupBox
    Friend WithEvents txtSupertrendMultiplier As TextBox
    Friend WithEvents lblLTEMA2Period As Label
    Friend WithEvents txtSupertrendPeriod As TextBox
    Friend WithEvents lblLTEMA1Period As Label
    Friend WithEvents grpSettings As GroupBox
    Friend WithEvents btnBrowse As Button
    Friend WithEvents txtInstrumentDetalis As TextBox
    Friend WithEvents lblInstrumentDetails As Label
    Friend WithEvents txtSignalTimeframe As TextBox
    Friend WithEvents lblSignalTimeframe As Label
    Friend WithEvents txtStrikeRangePer As TextBox
    Friend WithEvents lblStrikeRangePer As Label
    Friend WithEvents dtpckrTradeStartTime As DateTimePicker
    Friend WithEvents lblTradeStartTime As Label
    Friend WithEvents dtpckrEODExitTime As DateTimePicker
    Friend WithEvents lblEODExitTime As Label
    Friend WithEvents dtpckrLastTradeEntryTime As DateTimePicker
    Friend WithEvents lblLastTradeEntryTime As Label
End Class
