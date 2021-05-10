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
        Me.grpSettings = New System.Windows.Forms.GroupBox()
        Me.txtMaxLoss = New System.Windows.Forms.TextBox()
        Me.lblMaxLoss = New System.Windows.Forms.Label()
        Me.txtMaxProfit = New System.Windows.Forms.TextBox()
        Me.lblMaxProfit = New System.Windows.Forms.Label()
        Me.dtpckrEODExitTime = New System.Windows.Forms.DateTimePicker()
        Me.lblEODExitTime = New System.Windows.Forms.Label()
        Me.dtpckrLastTradeEntryTime = New System.Windows.Forms.DateTimePicker()
        Me.lblLastTradeEntryTime = New System.Windows.Forms.Label()
        Me.dtpckrTradeStartTime = New System.Windows.Forms.DateTimePicker()
        Me.lblTradeStartTime = New System.Windows.Forms.Label()
        Me.btnBrowse = New System.Windows.Forms.Button()
        Me.txtInstrumentDetalis = New System.Windows.Forms.TextBox()
        Me.lblInstrumentDetails = New System.Windows.Forms.Label()
        Me.grpSettings.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnSave
        '
        Me.btnSave.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnSave.ImageKey = "save-icon-36533.png"
        Me.btnSave.ImageList = Me.ImageList1
        Me.btnSave.Location = New System.Drawing.Point(461, 10)
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
        'grpSettings
        '
        Me.grpSettings.Controls.Add(Me.txtMaxLoss)
        Me.grpSettings.Controls.Add(Me.lblMaxLoss)
        Me.grpSettings.Controls.Add(Me.txtMaxProfit)
        Me.grpSettings.Controls.Add(Me.lblMaxProfit)
        Me.grpSettings.Controls.Add(Me.dtpckrEODExitTime)
        Me.grpSettings.Controls.Add(Me.lblEODExitTime)
        Me.grpSettings.Controls.Add(Me.dtpckrLastTradeEntryTime)
        Me.grpSettings.Controls.Add(Me.lblLastTradeEntryTime)
        Me.grpSettings.Controls.Add(Me.dtpckrTradeStartTime)
        Me.grpSettings.Controls.Add(Me.lblTradeStartTime)
        Me.grpSettings.Controls.Add(Me.btnBrowse)
        Me.grpSettings.Controls.Add(Me.txtInstrumentDetalis)
        Me.grpSettings.Controls.Add(Me.lblInstrumentDetails)
        Me.grpSettings.Location = New System.Drawing.Point(2, -1)
        Me.grpSettings.Margin = New System.Windows.Forms.Padding(4)
        Me.grpSettings.Name = "grpSettings"
        Me.grpSettings.Padding = New System.Windows.Forms.Padding(4)
        Me.grpSettings.Size = New System.Drawing.Size(451, 215)
        Me.grpSettings.TabIndex = 23
        Me.grpSettings.TabStop = False
        '
        'txtMaxLoss
        '
        Me.txtMaxLoss.Location = New System.Drawing.Point(174, 147)
        Me.txtMaxLoss.Name = "txtMaxLoss"
        Me.txtMaxLoss.Size = New System.Drawing.Size(255, 22)
        Me.txtMaxLoss.TabIndex = 5
        Me.txtMaxLoss.Tag = "Max Loss Per Day"
        '
        'lblMaxLoss
        '
        Me.lblMaxLoss.AutoSize = True
        Me.lblMaxLoss.Location = New System.Drawing.Point(7, 150)
        Me.lblMaxLoss.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMaxLoss.Name = "lblMaxLoss"
        Me.lblMaxLoss.Size = New System.Drawing.Size(122, 17)
        Me.lblMaxLoss.TabIndex = 26
        Me.lblMaxLoss.Text = "Max Loss Per Day"
        '
        'txtMaxProfit
        '
        Me.txtMaxProfit.Location = New System.Drawing.Point(174, 114)
        Me.txtMaxProfit.Name = "txtMaxProfit"
        Me.txtMaxProfit.Size = New System.Drawing.Size(255, 22)
        Me.txtMaxProfit.TabIndex = 4
        Me.txtMaxProfit.Tag = "Max Profit Per Day"
        '
        'lblMaxProfit
        '
        Me.lblMaxProfit.AutoSize = True
        Me.lblMaxProfit.Location = New System.Drawing.Point(7, 117)
        Me.lblMaxProfit.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMaxProfit.Name = "lblMaxProfit"
        Me.lblMaxProfit.Size = New System.Drawing.Size(125, 17)
        Me.lblMaxProfit.TabIndex = 24
        Me.lblMaxProfit.Text = "Max Profit Per Day"
        '
        'dtpckrEODExitTime
        '
        Me.dtpckrEODExitTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpckrEODExitTime.Location = New System.Drawing.Point(174, 80)
        Me.dtpckrEODExitTime.Name = "dtpckrEODExitTime"
        Me.dtpckrEODExitTime.ShowUpDown = True
        Me.dtpckrEODExitTime.Size = New System.Drawing.Size(255, 22)
        Me.dtpckrEODExitTime.TabIndex = 3
        Me.dtpckrEODExitTime.Value = New Date(2021, 5, 10, 15, 0, 0, 0)
        '
        'lblEODExitTime
        '
        Me.lblEODExitTime.AutoSize = True
        Me.lblEODExitTime.Location = New System.Drawing.Point(7, 83)
        Me.lblEODExitTime.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblEODExitTime.Name = "lblEODExitTime"
        Me.lblEODExitTime.Size = New System.Drawing.Size(99, 17)
        Me.lblEODExitTime.TabIndex = 22
        Me.lblEODExitTime.Text = "EOD Exit Time"
        '
        'dtpckrLastTradeEntryTime
        '
        Me.dtpckrLastTradeEntryTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpckrLastTradeEntryTime.Location = New System.Drawing.Point(174, 47)
        Me.dtpckrLastTradeEntryTime.Name = "dtpckrLastTradeEntryTime"
        Me.dtpckrLastTradeEntryTime.ShowUpDown = True
        Me.dtpckrLastTradeEntryTime.Size = New System.Drawing.Size(255, 22)
        Me.dtpckrLastTradeEntryTime.TabIndex = 2
        Me.dtpckrLastTradeEntryTime.Value = New Date(2021, 5, 10, 14, 45, 0, 0)
        '
        'lblLastTradeEntryTime
        '
        Me.lblLastTradeEntryTime.AutoSize = True
        Me.lblLastTradeEntryTime.Location = New System.Drawing.Point(7, 50)
        Me.lblLastTradeEntryTime.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblLastTradeEntryTime.Name = "lblLastTradeEntryTime"
        Me.lblLastTradeEntryTime.Size = New System.Drawing.Size(149, 17)
        Me.lblLastTradeEntryTime.TabIndex = 21
        Me.lblLastTradeEntryTime.Text = "Last Trade Entry Time"
        '
        'dtpckrTradeStartTime
        '
        Me.dtpckrTradeStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpckrTradeStartTime.Location = New System.Drawing.Point(174, 14)
        Me.dtpckrTradeStartTime.Name = "dtpckrTradeStartTime"
        Me.dtpckrTradeStartTime.ShowUpDown = True
        Me.dtpckrTradeStartTime.Size = New System.Drawing.Size(255, 22)
        Me.dtpckrTradeStartTime.TabIndex = 1
        Me.dtpckrTradeStartTime.Value = New Date(2021, 5, 10, 9, 15, 0, 0)
        '
        'lblTradeStartTime
        '
        Me.lblTradeStartTime.AutoSize = True
        Me.lblTradeStartTime.Location = New System.Drawing.Point(7, 17)
        Me.lblTradeStartTime.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblTradeStartTime.Name = "lblTradeStartTime"
        Me.lblTradeStartTime.Size = New System.Drawing.Size(115, 17)
        Me.lblTradeStartTime.TabIndex = 20
        Me.lblTradeStartTime.Text = "Trade Start Time"
        '
        'btnBrowse
        '
        Me.btnBrowse.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnBrowse.Location = New System.Drawing.Point(404, 179)
        Me.btnBrowse.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnBrowse.Name = "btnBrowse"
        Me.btnBrowse.Size = New System.Drawing.Size(40, 23)
        Me.btnBrowse.TabIndex = 6
        Me.btnBrowse.Text = "..."
        Me.btnBrowse.UseVisualStyleBackColor = True
        '
        'txtInstrumentDetalis
        '
        Me.txtInstrumentDetalis.Location = New System.Drawing.Point(174, 180)
        Me.txtInstrumentDetalis.Margin = New System.Windows.Forms.Padding(4)
        Me.txtInstrumentDetalis.Name = "txtInstrumentDetalis"
        Me.txtInstrumentDetalis.ReadOnly = True
        Me.txtInstrumentDetalis.Size = New System.Drawing.Size(223, 22)
        Me.txtInstrumentDetalis.TabIndex = 6
        '
        'lblInstrumentDetails
        '
        Me.lblInstrumentDetails.AutoSize = True
        Me.lblInstrumentDetails.Location = New System.Drawing.Point(7, 183)
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
        Me.ClientSize = New System.Drawing.Size(580, 215)
        Me.Controls.Add(Me.grpSettings)
        Me.Controls.Add(Me.btnSave)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmNFOSettings"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Settings"
        Me.grpSettings.ResumeLayout(False)
        Me.grpSettings.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents btnSave As Button
    Friend WithEvents ImageList1 As ImageList
    Friend WithEvents opnFileSettings As OpenFileDialog
    Friend WithEvents grpSettings As GroupBox
    Friend WithEvents btnBrowse As Button
    Friend WithEvents txtInstrumentDetalis As TextBox
    Friend WithEvents lblInstrumentDetails As Label
    Friend WithEvents dtpckrEODExitTime As DateTimePicker
    Friend WithEvents lblEODExitTime As Label
    Friend WithEvents dtpckrLastTradeEntryTime As DateTimePicker
    Friend WithEvents lblLastTradeEntryTime As Label
    Friend WithEvents dtpckrTradeStartTime As DateTimePicker
    Friend WithEvents lblTradeStartTime As Label
    Friend WithEvents txtMaxLoss As TextBox
    Friend WithEvents lblMaxLoss As Label
    Friend WithEvents txtMaxProfit As TextBox
    Friend WithEvents lblMaxProfit As Label
End Class
