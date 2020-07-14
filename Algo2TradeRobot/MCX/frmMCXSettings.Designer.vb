<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmMCXSettings
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmMCXSettings))
        Me.opnFileSettings = New System.Windows.Forms.OpenFileDialog()
        Me.ImageList1 = New System.Windows.Forms.ImageList(Me.components)
        Me.btnSave = New System.Windows.Forms.Button()
        Me.grpSupertrend = New System.Windows.Forms.GroupBox()
        Me.txtSupertrendMultiplier = New System.Windows.Forms.TextBox()
        Me.lblSupertrendMultiplier = New System.Windows.Forms.Label()
        Me.txtSupertrendPeriod = New System.Windows.Forms.TextBox()
        Me.lblSupertrendPeriod = New System.Windows.Forms.Label()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.txtMiddleTimeframe = New System.Windows.Forms.TextBox()
        Me.lblMiddleTimeFrame = New System.Windows.Forms.Label()
        Me.txtHigherTimeframe = New System.Windows.Forms.TextBox()
        Me.lblHigherTF = New System.Windows.Forms.Label()
        Me.dtpckrEODExitTime = New System.Windows.Forms.DateTimePicker()
        Me.dtpckrLastTradeEntryTime = New System.Windows.Forms.DateTimePicker()
        Me.dtpckrTradeStartTime = New System.Windows.Forms.DateTimePicker()
        Me.lblEODExitTime = New System.Windows.Forms.Label()
        Me.lblLastTradeEntryTime = New System.Windows.Forms.Label()
        Me.lblTradeStartTime = New System.Windows.Forms.Label()
        Me.btnBrowse = New System.Windows.Forms.Button()
        Me.txtInstrumentDetalis = New System.Windows.Forms.TextBox()
        Me.lblInstrumentDetails = New System.Windows.Forms.Label()
        Me.txtLowerTimeframe = New System.Windows.Forms.TextBox()
        Me.lblLowerTimeFrame = New System.Windows.Forms.Label()
        Me.grpSupertrend.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        Me.SuspendLayout()
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
        Me.btnSave.Location = New System.Drawing.Point(461, 12)
        Me.btnSave.Margin = New System.Windows.Forms.Padding(4)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(112, 58)
        Me.btnSave.TabIndex = 24
        Me.btnSave.Text = "&Save"
        Me.btnSave.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.btnSave.UseVisualStyleBackColor = True
        '
        'grpSupertrend
        '
        Me.grpSupertrend.Controls.Add(Me.txtSupertrendMultiplier)
        Me.grpSupertrend.Controls.Add(Me.lblSupertrendMultiplier)
        Me.grpSupertrend.Controls.Add(Me.txtSupertrendPeriod)
        Me.grpSupertrend.Controls.Add(Me.lblSupertrendPeriod)
        Me.grpSupertrend.Location = New System.Drawing.Point(5, 254)
        Me.grpSupertrend.Name = "grpSupertrend"
        Me.grpSupertrend.Size = New System.Drawing.Size(451, 90)
        Me.grpSupertrend.TabIndex = 26
        Me.grpSupertrend.TabStop = False
        Me.grpSupertrend.Text = "Supertrend Settings"
        '
        'txtSupertrendMultiplier
        '
        Me.txtSupertrendMultiplier.Location = New System.Drawing.Point(189, 55)
        Me.txtSupertrendMultiplier.Margin = New System.Windows.Forms.Padding(4)
        Me.txtSupertrendMultiplier.Name = "txtSupertrendMultiplier"
        Me.txtSupertrendMultiplier.Size = New System.Drawing.Size(241, 22)
        Me.txtSupertrendMultiplier.TabIndex = 9
        Me.txtSupertrendMultiplier.Tag = "Multiplier"
        '
        'lblSupertrendMultiplier
        '
        Me.lblSupertrendMultiplier.AutoSize = True
        Me.lblSupertrendMultiplier.Location = New System.Drawing.Point(9, 60)
        Me.lblSupertrendMultiplier.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblSupertrendMultiplier.Name = "lblSupertrendMultiplier"
        Me.lblSupertrendMultiplier.Size = New System.Drawing.Size(64, 17)
        Me.lblSupertrendMultiplier.TabIndex = 37
        Me.lblSupertrendMultiplier.Text = "Multiplier"
        '
        'txtSupertrendPeriod
        '
        Me.txtSupertrendPeriod.Location = New System.Drawing.Point(189, 21)
        Me.txtSupertrendPeriod.Margin = New System.Windows.Forms.Padding(4)
        Me.txtSupertrendPeriod.Name = "txtSupertrendPeriod"
        Me.txtSupertrendPeriod.Size = New System.Drawing.Size(241, 22)
        Me.txtSupertrendPeriod.TabIndex = 8
        Me.txtSupertrendPeriod.Tag = "Period"
        '
        'lblSupertrendPeriod
        '
        Me.lblSupertrendPeriod.AutoSize = True
        Me.lblSupertrendPeriod.Location = New System.Drawing.Point(9, 26)
        Me.lblSupertrendPeriod.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblSupertrendPeriod.Name = "lblSupertrendPeriod"
        Me.lblSupertrendPeriod.Size = New System.Drawing.Size(49, 17)
        Me.lblSupertrendPeriod.TabIndex = 35
        Me.lblSupertrendPeriod.Text = "Period"
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.txtMiddleTimeframe)
        Me.GroupBox1.Controls.Add(Me.lblMiddleTimeFrame)
        Me.GroupBox1.Controls.Add(Me.txtHigherTimeframe)
        Me.GroupBox1.Controls.Add(Me.lblHigherTF)
        Me.GroupBox1.Controls.Add(Me.dtpckrEODExitTime)
        Me.GroupBox1.Controls.Add(Me.dtpckrLastTradeEntryTime)
        Me.GroupBox1.Controls.Add(Me.dtpckrTradeStartTime)
        Me.GroupBox1.Controls.Add(Me.lblEODExitTime)
        Me.GroupBox1.Controls.Add(Me.lblLastTradeEntryTime)
        Me.GroupBox1.Controls.Add(Me.lblTradeStartTime)
        Me.GroupBox1.Controls.Add(Me.btnBrowse)
        Me.GroupBox1.Controls.Add(Me.txtInstrumentDetalis)
        Me.GroupBox1.Controls.Add(Me.lblInstrumentDetails)
        Me.GroupBox1.Controls.Add(Me.txtLowerTimeframe)
        Me.GroupBox1.Controls.Add(Me.lblLowerTimeFrame)
        Me.GroupBox1.Location = New System.Drawing.Point(5, 3)
        Me.GroupBox1.Margin = New System.Windows.Forms.Padding(4)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Padding = New System.Windows.Forms.Padding(4)
        Me.GroupBox1.Size = New System.Drawing.Size(451, 249)
        Me.GroupBox1.TabIndex = 25
        Me.GroupBox1.TabStop = False
        '
        'txtMiddleTimeframe
        '
        Me.txtMiddleTimeframe.Location = New System.Drawing.Point(189, 47)
        Me.txtMiddleTimeframe.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMiddleTimeframe.Name = "txtMiddleTimeframe"
        Me.txtMiddleTimeframe.Size = New System.Drawing.Size(241, 22)
        Me.txtMiddleTimeframe.TabIndex = 2
        Me.txtMiddleTimeframe.Tag = "Middle Timeframe"
        '
        'lblMiddleTimeFrame
        '
        Me.lblMiddleTimeFrame.AutoSize = True
        Me.lblMiddleTimeFrame.Location = New System.Drawing.Point(9, 50)
        Me.lblMiddleTimeFrame.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMiddleTimeFrame.Name = "lblMiddleTimeFrame"
        Me.lblMiddleTimeFrame.Size = New System.Drawing.Size(160, 17)
        Me.lblMiddleTimeFrame.TabIndex = 27
        Me.lblMiddleTimeFrame.Tag = ""
        Me.lblMiddleTimeFrame.Text = "Middle Time Frame(min)"
        '
        'txtHigherTimeframe
        '
        Me.txtHigherTimeframe.Location = New System.Drawing.Point(189, 79)
        Me.txtHigherTimeframe.Margin = New System.Windows.Forms.Padding(4)
        Me.txtHigherTimeframe.Name = "txtHigherTimeframe"
        Me.txtHigherTimeframe.Size = New System.Drawing.Size(241, 22)
        Me.txtHigherTimeframe.TabIndex = 3
        Me.txtHigherTimeframe.Tag = "Higher Timeframe"
        '
        'lblHigherTF
        '
        Me.lblHigherTF.AutoSize = True
        Me.lblHigherTF.Location = New System.Drawing.Point(9, 82)
        Me.lblHigherTF.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblHigherTF.Name = "lblHigherTF"
        Me.lblHigherTF.Size = New System.Drawing.Size(161, 17)
        Me.lblHigherTF.TabIndex = 25
        Me.lblHigherTF.Tag = ""
        Me.lblHigherTF.Text = "Higher Time Frame(min)"
        '
        'dtpckrEODExitTime
        '
        Me.dtpckrEODExitTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpckrEODExitTime.Location = New System.Drawing.Point(189, 183)
        Me.dtpckrEODExitTime.Name = "dtpckrEODExitTime"
        Me.dtpckrEODExitTime.ShowUpDown = True
        Me.dtpckrEODExitTime.Size = New System.Drawing.Size(241, 22)
        Me.dtpckrEODExitTime.TabIndex = 6
        Me.dtpckrEODExitTime.Value = New Date(2019, 3, 12, 0, 0, 0, 0)
        '
        'dtpckrLastTradeEntryTime
        '
        Me.dtpckrLastTradeEntryTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpckrLastTradeEntryTime.Location = New System.Drawing.Point(189, 148)
        Me.dtpckrLastTradeEntryTime.Name = "dtpckrLastTradeEntryTime"
        Me.dtpckrLastTradeEntryTime.ShowUpDown = True
        Me.dtpckrLastTradeEntryTime.Size = New System.Drawing.Size(242, 22)
        Me.dtpckrLastTradeEntryTime.TabIndex = 5
        Me.dtpckrLastTradeEntryTime.Value = New Date(2019, 3, 12, 0, 0, 0, 0)
        '
        'dtpckrTradeStartTime
        '
        Me.dtpckrTradeStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpckrTradeStartTime.Location = New System.Drawing.Point(189, 113)
        Me.dtpckrTradeStartTime.Name = "dtpckrTradeStartTime"
        Me.dtpckrTradeStartTime.ShowUpDown = True
        Me.dtpckrTradeStartTime.Size = New System.Drawing.Size(241, 22)
        Me.dtpckrTradeStartTime.TabIndex = 4
        Me.dtpckrTradeStartTime.Value = New Date(2019, 3, 12, 0, 0, 0, 0)
        '
        'lblEODExitTime
        '
        Me.lblEODExitTime.AutoSize = True
        Me.lblEODExitTime.Location = New System.Drawing.Point(9, 184)
        Me.lblEODExitTime.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblEODExitTime.Name = "lblEODExitTime"
        Me.lblEODExitTime.Size = New System.Drawing.Size(99, 17)
        Me.lblEODExitTime.TabIndex = 23
        Me.lblEODExitTime.Text = "EOD Exit Time"
        '
        'lblLastTradeEntryTime
        '
        Me.lblLastTradeEntryTime.AutoSize = True
        Me.lblLastTradeEntryTime.Location = New System.Drawing.Point(9, 149)
        Me.lblLastTradeEntryTime.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblLastTradeEntryTime.Name = "lblLastTradeEntryTime"
        Me.lblLastTradeEntryTime.Size = New System.Drawing.Size(149, 17)
        Me.lblLastTradeEntryTime.TabIndex = 21
        Me.lblLastTradeEntryTime.Text = "Last Trade Entry Time"
        '
        'lblTradeStartTime
        '
        Me.lblTradeStartTime.AutoSize = True
        Me.lblTradeStartTime.Location = New System.Drawing.Point(9, 115)
        Me.lblTradeStartTime.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblTradeStartTime.Name = "lblTradeStartTime"
        Me.lblTradeStartTime.Size = New System.Drawing.Size(115, 17)
        Me.lblTradeStartTime.TabIndex = 19
        Me.lblTradeStartTime.Text = "Trade Start Time"
        '
        'btnBrowse
        '
        Me.btnBrowse.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnBrowse.Location = New System.Drawing.Point(404, 220)
        Me.btnBrowse.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnBrowse.Name = "btnBrowse"
        Me.btnBrowse.Size = New System.Drawing.Size(40, 23)
        Me.btnBrowse.TabIndex = 7
        Me.btnBrowse.Text = "..."
        Me.btnBrowse.UseVisualStyleBackColor = True
        '
        'txtInstrumentDetalis
        '
        Me.txtInstrumentDetalis.Location = New System.Drawing.Point(189, 221)
        Me.txtInstrumentDetalis.Margin = New System.Windows.Forms.Padding(4)
        Me.txtInstrumentDetalis.Name = "txtInstrumentDetalis"
        Me.txtInstrumentDetalis.ReadOnly = True
        Me.txtInstrumentDetalis.Size = New System.Drawing.Size(208, 22)
        Me.txtInstrumentDetalis.TabIndex = 7
        '
        'lblInstrumentDetails
        '
        Me.lblInstrumentDetails.AutoSize = True
        Me.lblInstrumentDetails.Location = New System.Drawing.Point(8, 224)
        Me.lblInstrumentDetails.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblInstrumentDetails.Name = "lblInstrumentDetails"
        Me.lblInstrumentDetails.Size = New System.Drawing.Size(121, 17)
        Me.lblInstrumentDetails.TabIndex = 8
        Me.lblInstrumentDetails.Text = "Instrument Details"
        '
        'txtLowerTimeframe
        '
        Me.txtLowerTimeframe.Location = New System.Drawing.Point(189, 15)
        Me.txtLowerTimeframe.Margin = New System.Windows.Forms.Padding(4)
        Me.txtLowerTimeframe.Name = "txtLowerTimeframe"
        Me.txtLowerTimeframe.Size = New System.Drawing.Size(241, 22)
        Me.txtLowerTimeframe.TabIndex = 1
        Me.txtLowerTimeframe.Tag = "Lower Timeframe"
        '
        'lblLowerTimeFrame
        '
        Me.lblLowerTimeFrame.AutoSize = True
        Me.lblLowerTimeFrame.Location = New System.Drawing.Point(9, 18)
        Me.lblLowerTimeFrame.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblLowerTimeFrame.Name = "lblLowerTimeFrame"
        Me.lblLowerTimeFrame.Size = New System.Drawing.Size(157, 17)
        Me.lblLowerTimeFrame.TabIndex = 3
        Me.lblLowerTimeFrame.Tag = ""
        Me.lblLowerTimeFrame.Text = "Lower Time Frame(min)"
        '
        'frmMCXSettings
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(576, 346)
        Me.Controls.Add(Me.grpSupertrend)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.btnSave)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmMCXSettings"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "MCX Settings"
        Me.grpSupertrend.ResumeLayout(False)
        Me.grpSupertrend.PerformLayout()
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents opnFileSettings As OpenFileDialog
    Friend WithEvents ImageList1 As ImageList
    Friend WithEvents btnSave As Button
    Friend WithEvents grpSupertrend As GroupBox
    Friend WithEvents txtSupertrendMultiplier As TextBox
    Friend WithEvents lblSupertrendMultiplier As Label
    Friend WithEvents txtSupertrendPeriod As TextBox
    Friend WithEvents lblSupertrendPeriod As Label
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents txtMiddleTimeframe As TextBox
    Friend WithEvents lblMiddleTimeFrame As Label
    Friend WithEvents txtHigherTimeframe As TextBox
    Friend WithEvents lblHigherTF As Label
    Friend WithEvents dtpckrEODExitTime As DateTimePicker
    Friend WithEvents dtpckrLastTradeEntryTime As DateTimePicker
    Friend WithEvents dtpckrTradeStartTime As DateTimePicker
    Friend WithEvents lblEODExitTime As Label
    Friend WithEvents lblLastTradeEntryTime As Label
    Friend WithEvents lblTradeStartTime As Label
    Friend WithEvents btnBrowse As Button
    Friend WithEvents txtInstrumentDetalis As TextBox
    Friend WithEvents lblInstrumentDetails As Label
    Friend WithEvents txtLowerTimeframe As TextBox
    Friend WithEvents lblLowerTimeFrame As Label
End Class
