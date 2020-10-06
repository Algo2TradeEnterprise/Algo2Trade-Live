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
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.txtEMA2Period = New System.Windows.Forms.TextBox()
        Me.lblEMA2Period = New System.Windows.Forms.Label()
        Me.txtEMA1Period = New System.Windows.Forms.TextBox()
        Me.lblEMA1Period = New System.Windows.Forms.Label()
        Me.grpSettings = New System.Windows.Forms.GroupBox()
        Me.btnBrowse = New System.Windows.Forms.Button()
        Me.txtInstrumentDetalis = New System.Windows.Forms.TextBox()
        Me.lblInstrumentDetails = New System.Windows.Forms.Label()
        Me.txtTimeFrame = New System.Windows.Forms.TextBox()
        Me.lblTimeFrame = New System.Windows.Forms.Label()
        Me.txtStrikeRangePer = New System.Windows.Forms.TextBox()
        Me.lblStrikeRangePer = New System.Windows.Forms.Label()
        Me.GroupBox2.SuspendLayout()
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
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.txtEMA2Period)
        Me.GroupBox2.Controls.Add(Me.lblEMA2Period)
        Me.GroupBox2.Controls.Add(Me.txtEMA1Period)
        Me.GroupBox2.Controls.Add(Me.lblEMA1Period)
        Me.GroupBox2.Location = New System.Drawing.Point(2, 110)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(451, 92)
        Me.GroupBox2.TabIndex = 24
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "Indicator Settings"
        '
        'txtEMA2Period
        '
        Me.txtEMA2Period.Location = New System.Drawing.Point(175, 56)
        Me.txtEMA2Period.Margin = New System.Windows.Forms.Padding(4)
        Me.txtEMA2Period.Name = "txtEMA2Period"
        Me.txtEMA2Period.Size = New System.Drawing.Size(255, 22)
        Me.txtEMA2Period.TabIndex = 1
        Me.txtEMA2Period.Tag = "EMA2 Period"
        '
        'lblEMA2Period
        '
        Me.lblEMA2Period.AutoSize = True
        Me.lblEMA2Period.Location = New System.Drawing.Point(9, 60)
        Me.lblEMA2Period.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblEMA2Period.Name = "lblEMA2Period"
        Me.lblEMA2Period.Size = New System.Drawing.Size(90, 17)
        Me.lblEMA2Period.TabIndex = 35
        Me.lblEMA2Period.Text = "EMA2 Period"
        '
        'txtEMA1Period
        '
        Me.txtEMA1Period.Location = New System.Drawing.Point(174, 25)
        Me.txtEMA1Period.Margin = New System.Windows.Forms.Padding(4)
        Me.txtEMA1Period.Name = "txtEMA1Period"
        Me.txtEMA1Period.Size = New System.Drawing.Size(256, 22)
        Me.txtEMA1Period.TabIndex = 0
        Me.txtEMA1Period.Tag = "EMA1 Period"
        '
        'lblEMA1Period
        '
        Me.lblEMA1Period.AutoSize = True
        Me.lblEMA1Period.Location = New System.Drawing.Point(10, 28)
        Me.lblEMA1Period.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblEMA1Period.Name = "lblEMA1Period"
        Me.lblEMA1Period.Size = New System.Drawing.Size(90, 17)
        Me.lblEMA1Period.TabIndex = 31
        Me.lblEMA1Period.Text = "EMA1 Period"
        '
        'grpSettings
        '
        Me.grpSettings.Controls.Add(Me.txtStrikeRangePer)
        Me.grpSettings.Controls.Add(Me.lblStrikeRangePer)
        Me.grpSettings.Controls.Add(Me.btnBrowse)
        Me.grpSettings.Controls.Add(Me.txtInstrumentDetalis)
        Me.grpSettings.Controls.Add(Me.lblInstrumentDetails)
        Me.grpSettings.Controls.Add(Me.txtTimeFrame)
        Me.grpSettings.Controls.Add(Me.lblTimeFrame)
        Me.grpSettings.Location = New System.Drawing.Point(2, -1)
        Me.grpSettings.Margin = New System.Windows.Forms.Padding(4)
        Me.grpSettings.Name = "grpSettings"
        Me.grpSettings.Padding = New System.Windows.Forms.Padding(4)
        Me.grpSettings.Size = New System.Drawing.Size(451, 110)
        Me.grpSettings.TabIndex = 23
        Me.grpSettings.TabStop = False
        '
        'btnBrowse
        '
        Me.btnBrowse.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnBrowse.Location = New System.Drawing.Point(404, 77)
        Me.btnBrowse.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnBrowse.Name = "btnBrowse"
        Me.btnBrowse.Size = New System.Drawing.Size(40, 23)
        Me.btnBrowse.TabIndex = 2
        Me.btnBrowse.Text = "..."
        Me.btnBrowse.UseVisualStyleBackColor = True
        '
        'txtInstrumentDetalis
        '
        Me.txtInstrumentDetalis.Location = New System.Drawing.Point(174, 78)
        Me.txtInstrumentDetalis.Margin = New System.Windows.Forms.Padding(4)
        Me.txtInstrumentDetalis.Name = "txtInstrumentDetalis"
        Me.txtInstrumentDetalis.ReadOnly = True
        Me.txtInstrumentDetalis.Size = New System.Drawing.Size(223, 22)
        Me.txtInstrumentDetalis.TabIndex = 2
        '
        'lblInstrumentDetails
        '
        Me.lblInstrumentDetails.AutoSize = True
        Me.lblInstrumentDetails.Location = New System.Drawing.Point(9, 81)
        Me.lblInstrumentDetails.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblInstrumentDetails.Name = "lblInstrumentDetails"
        Me.lblInstrumentDetails.Size = New System.Drawing.Size(121, 17)
        Me.lblInstrumentDetails.TabIndex = 8
        Me.lblInstrumentDetails.Text = "Instrument Details"
        '
        'txtTimeFrame
        '
        Me.txtTimeFrame.Location = New System.Drawing.Point(175, 15)
        Me.txtTimeFrame.Margin = New System.Windows.Forms.Padding(4)
        Me.txtTimeFrame.Name = "txtTimeFrame"
        Me.txtTimeFrame.Size = New System.Drawing.Size(255, 22)
        Me.txtTimeFrame.TabIndex = 0
        Me.txtTimeFrame.Tag = "Time Frame"
        '
        'lblTimeFrame
        '
        Me.lblTimeFrame.AutoSize = True
        Me.lblTimeFrame.Location = New System.Drawing.Point(9, 18)
        Me.lblTimeFrame.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblTimeFrame.Name = "lblTimeFrame"
        Me.lblTimeFrame.Size = New System.Drawing.Size(115, 17)
        Me.lblTimeFrame.TabIndex = 3
        Me.lblTimeFrame.Text = "Time Frame(min)"
        '
        'txtStrikeRangePer
        '
        Me.txtStrikeRangePer.Location = New System.Drawing.Point(175, 46)
        Me.txtStrikeRangePer.Margin = New System.Windows.Forms.Padding(4)
        Me.txtStrikeRangePer.Name = "txtStrikeRangePer"
        Me.txtStrikeRangePer.Size = New System.Drawing.Size(255, 22)
        Me.txtStrikeRangePer.TabIndex = 1
        Me.txtStrikeRangePer.Tag = "Strike Price Range %"
        '
        'lblStrikeRangePer
        '
        Me.lblStrikeRangePer.AutoSize = True
        Me.lblStrikeRangePer.Location = New System.Drawing.Point(9, 49)
        Me.lblStrikeRangePer.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblStrikeRangePer.Name = "lblStrikeRangePer"
        Me.lblStrikeRangePer.Size = New System.Drawing.Size(142, 17)
        Me.lblStrikeRangePer.TabIndex = 10
        Me.lblStrikeRangePer.Text = "Strike Price Range %"
        '
        'frmNFOSettings
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(580, 209)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.grpSettings)
        Me.Controls.Add(Me.btnSave)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmNFOSettings"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "EMA Crossover Settings"
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        Me.grpSettings.ResumeLayout(False)
        Me.grpSettings.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents btnSave As Button
    Friend WithEvents ImageList1 As ImageList
    Friend WithEvents opnFileSettings As OpenFileDialog
    Friend WithEvents GroupBox2 As GroupBox
    Friend WithEvents txtEMA2Period As TextBox
    Friend WithEvents lblEMA2Period As Label
    Friend WithEvents txtEMA1Period As TextBox
    Friend WithEvents lblEMA1Period As Label
    Friend WithEvents grpSettings As GroupBox
    Friend WithEvents btnBrowse As Button
    Friend WithEvents txtInstrumentDetalis As TextBox
    Friend WithEvents lblInstrumentDetails As Label
    Friend WithEvents txtTimeFrame As TextBox
    Friend WithEvents lblTimeFrame As Label
    Friend WithEvents txtStrikeRangePer As TextBox
    Friend WithEvents lblStrikeRangePer As Label
End Class
