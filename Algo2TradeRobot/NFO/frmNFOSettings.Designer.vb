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
        Me.txtLTEMA2Period = New System.Windows.Forms.TextBox()
        Me.lblLTEMA2Period = New System.Windows.Forms.Label()
        Me.txtLTEMA1Period = New System.Windows.Forms.TextBox()
        Me.lblLTEMA1Period = New System.Windows.Forms.Label()
        Me.grpSettings = New System.Windows.Forms.GroupBox()
        Me.txtStrikeRangePer = New System.Windows.Forms.TextBox()
        Me.lblStrikeRangePer = New System.Windows.Forms.Label()
        Me.btnBrowse = New System.Windows.Forms.Button()
        Me.txtInstrumentDetalis = New System.Windows.Forms.TextBox()
        Me.lblInstrumentDetails = New System.Windows.Forms.Label()
        Me.txtLowerTimeFrame = New System.Windows.Forms.TextBox()
        Me.lblLowerTimeFrame = New System.Windows.Forms.Label()
        Me.txtHigherTimeFrame = New System.Windows.Forms.TextBox()
        Me.lblHigherTimeFrame = New System.Windows.Forms.Label()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.txtHTEMA2Period = New System.Windows.Forms.TextBox()
        Me.lblHTEMA2Period = New System.Windows.Forms.Label()
        Me.txtHTEMA1Period = New System.Windows.Forms.TextBox()
        Me.lblHTEMA1Period = New System.Windows.Forms.Label()
        Me.GroupBox2.SuspendLayout()
        Me.grpSettings.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
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
        Me.GroupBox2.Controls.Add(Me.txtLTEMA2Period)
        Me.GroupBox2.Controls.Add(Me.lblLTEMA2Period)
        Me.GroupBox2.Controls.Add(Me.txtLTEMA1Period)
        Me.GroupBox2.Controls.Add(Me.lblLTEMA1Period)
        Me.GroupBox2.Location = New System.Drawing.Point(2, 142)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(451, 92)
        Me.GroupBox2.TabIndex = 24
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "Lower Timeframe Indicator Settings"
        '
        'txtLTEMA2Period
        '
        Me.txtLTEMA2Period.Location = New System.Drawing.Point(175, 56)
        Me.txtLTEMA2Period.Margin = New System.Windows.Forms.Padding(4)
        Me.txtLTEMA2Period.Name = "txtLTEMA2Period"
        Me.txtLTEMA2Period.Size = New System.Drawing.Size(255, 22)
        Me.txtLTEMA2Period.TabIndex = 1
        Me.txtLTEMA2Period.Tag = "LT EMA2 Period"
        '
        'lblLTEMA2Period
        '
        Me.lblLTEMA2Period.AutoSize = True
        Me.lblLTEMA2Period.Location = New System.Drawing.Point(9, 60)
        Me.lblLTEMA2Period.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblLTEMA2Period.Name = "lblLTEMA2Period"
        Me.lblLTEMA2Period.Size = New System.Drawing.Size(90, 17)
        Me.lblLTEMA2Period.TabIndex = 35
        Me.lblLTEMA2Period.Text = "EMA2 Period"
        '
        'txtLTEMA1Period
        '
        Me.txtLTEMA1Period.Location = New System.Drawing.Point(174, 25)
        Me.txtLTEMA1Period.Margin = New System.Windows.Forms.Padding(4)
        Me.txtLTEMA1Period.Name = "txtLTEMA1Period"
        Me.txtLTEMA1Period.Size = New System.Drawing.Size(256, 22)
        Me.txtLTEMA1Period.TabIndex = 0
        Me.txtLTEMA1Period.Tag = "LT EMA1 Period"
        '
        'lblLTEMA1Period
        '
        Me.lblLTEMA1Period.AutoSize = True
        Me.lblLTEMA1Period.Location = New System.Drawing.Point(10, 28)
        Me.lblLTEMA1Period.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblLTEMA1Period.Name = "lblLTEMA1Period"
        Me.lblLTEMA1Period.Size = New System.Drawing.Size(90, 17)
        Me.lblLTEMA1Period.TabIndex = 31
        Me.lblLTEMA1Period.Text = "EMA1 Period"
        '
        'grpSettings
        '
        Me.grpSettings.Controls.Add(Me.txtHigherTimeFrame)
        Me.grpSettings.Controls.Add(Me.lblHigherTimeFrame)
        Me.grpSettings.Controls.Add(Me.txtStrikeRangePer)
        Me.grpSettings.Controls.Add(Me.lblStrikeRangePer)
        Me.grpSettings.Controls.Add(Me.btnBrowse)
        Me.grpSettings.Controls.Add(Me.txtInstrumentDetalis)
        Me.grpSettings.Controls.Add(Me.lblInstrumentDetails)
        Me.grpSettings.Controls.Add(Me.txtLowerTimeFrame)
        Me.grpSettings.Controls.Add(Me.lblLowerTimeFrame)
        Me.grpSettings.Location = New System.Drawing.Point(2, -1)
        Me.grpSettings.Margin = New System.Windows.Forms.Padding(4)
        Me.grpSettings.Name = "grpSettings"
        Me.grpSettings.Padding = New System.Windows.Forms.Padding(4)
        Me.grpSettings.Size = New System.Drawing.Size(451, 142)
        Me.grpSettings.TabIndex = 23
        Me.grpSettings.TabStop = False
        '
        'txtStrikeRangePer
        '
        Me.txtStrikeRangePer.Location = New System.Drawing.Point(175, 77)
        Me.txtStrikeRangePer.Margin = New System.Windows.Forms.Padding(4)
        Me.txtStrikeRangePer.Name = "txtStrikeRangePer"
        Me.txtStrikeRangePer.Size = New System.Drawing.Size(255, 22)
        Me.txtStrikeRangePer.TabIndex = 2
        Me.txtStrikeRangePer.Tag = "Strike Price Range %"
        '
        'lblStrikeRangePer
        '
        Me.lblStrikeRangePer.AutoSize = True
        Me.lblStrikeRangePer.Location = New System.Drawing.Point(9, 80)
        Me.lblStrikeRangePer.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblStrikeRangePer.Name = "lblStrikeRangePer"
        Me.lblStrikeRangePer.Size = New System.Drawing.Size(142, 17)
        Me.lblStrikeRangePer.TabIndex = 10
        Me.lblStrikeRangePer.Text = "Strike Price Range %"
        '
        'btnBrowse
        '
        Me.btnBrowse.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnBrowse.Location = New System.Drawing.Point(404, 108)
        Me.btnBrowse.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnBrowse.Name = "btnBrowse"
        Me.btnBrowse.Size = New System.Drawing.Size(40, 23)
        Me.btnBrowse.TabIndex = 3
        Me.btnBrowse.Text = "..."
        Me.btnBrowse.UseVisualStyleBackColor = True
        '
        'txtInstrumentDetalis
        '
        Me.txtInstrumentDetalis.Location = New System.Drawing.Point(174, 109)
        Me.txtInstrumentDetalis.Margin = New System.Windows.Forms.Padding(4)
        Me.txtInstrumentDetalis.Name = "txtInstrumentDetalis"
        Me.txtInstrumentDetalis.ReadOnly = True
        Me.txtInstrumentDetalis.Size = New System.Drawing.Size(223, 22)
        Me.txtInstrumentDetalis.TabIndex = 3
        '
        'lblInstrumentDetails
        '
        Me.lblInstrumentDetails.AutoSize = True
        Me.lblInstrumentDetails.Location = New System.Drawing.Point(9, 112)
        Me.lblInstrumentDetails.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblInstrumentDetails.Name = "lblInstrumentDetails"
        Me.lblInstrumentDetails.Size = New System.Drawing.Size(121, 17)
        Me.lblInstrumentDetails.TabIndex = 8
        Me.lblInstrumentDetails.Text = "Instrument Details"
        '
        'txtLowerTimeFrame
        '
        Me.txtLowerTimeFrame.Location = New System.Drawing.Point(175, 15)
        Me.txtLowerTimeFrame.Margin = New System.Windows.Forms.Padding(4)
        Me.txtLowerTimeFrame.Name = "txtLowerTimeFrame"
        Me.txtLowerTimeFrame.Size = New System.Drawing.Size(255, 22)
        Me.txtLowerTimeFrame.TabIndex = 0
        Me.txtLowerTimeFrame.Tag = "Lower Time Frame"
        '
        'lblLowerTimeFrame
        '
        Me.lblLowerTimeFrame.AutoSize = True
        Me.lblLowerTimeFrame.Location = New System.Drawing.Point(9, 18)
        Me.lblLowerTimeFrame.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblLowerTimeFrame.Name = "lblLowerTimeFrame"
        Me.lblLowerTimeFrame.Size = New System.Drawing.Size(157, 17)
        Me.lblLowerTimeFrame.TabIndex = 3
        Me.lblLowerTimeFrame.Text = "Lower Time Frame(min)"
        '
        'txtHigherTimeFrame
        '
        Me.txtHigherTimeFrame.Location = New System.Drawing.Point(175, 46)
        Me.txtHigherTimeFrame.Margin = New System.Windows.Forms.Padding(4)
        Me.txtHigherTimeFrame.Name = "txtHigherTimeFrame"
        Me.txtHigherTimeFrame.Size = New System.Drawing.Size(255, 22)
        Me.txtHigherTimeFrame.TabIndex = 1
        Me.txtHigherTimeFrame.Tag = "Higher Time Frame"
        '
        'lblHigherTimeFrame
        '
        Me.lblHigherTimeFrame.AutoSize = True
        Me.lblHigherTimeFrame.Location = New System.Drawing.Point(9, 49)
        Me.lblHigherTimeFrame.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblHigherTimeFrame.Name = "lblHigherTimeFrame"
        Me.lblHigherTimeFrame.Size = New System.Drawing.Size(161, 17)
        Me.lblHigherTimeFrame.TabIndex = 12
        Me.lblHigherTimeFrame.Text = "Higher Time Frame(min)"
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.txtHTEMA2Period)
        Me.GroupBox1.Controls.Add(Me.lblHTEMA2Period)
        Me.GroupBox1.Controls.Add(Me.txtHTEMA1Period)
        Me.GroupBox1.Controls.Add(Me.lblHTEMA1Period)
        Me.GroupBox1.Location = New System.Drawing.Point(2, 235)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(451, 92)
        Me.GroupBox1.TabIndex = 25
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Higher Timeframe Indicator Settings"
        '
        'txtHTEMA2Period
        '
        Me.txtHTEMA2Period.Location = New System.Drawing.Point(175, 56)
        Me.txtHTEMA2Period.Margin = New System.Windows.Forms.Padding(4)
        Me.txtHTEMA2Period.Name = "txtHTEMA2Period"
        Me.txtHTEMA2Period.Size = New System.Drawing.Size(255, 22)
        Me.txtHTEMA2Period.TabIndex = 1
        Me.txtHTEMA2Period.Tag = "HT EMA2 Period"
        '
        'lblHTEMA2Period
        '
        Me.lblHTEMA2Period.AutoSize = True
        Me.lblHTEMA2Period.Location = New System.Drawing.Point(9, 60)
        Me.lblHTEMA2Period.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblHTEMA2Period.Name = "lblHTEMA2Period"
        Me.lblHTEMA2Period.Size = New System.Drawing.Size(90, 17)
        Me.lblHTEMA2Period.TabIndex = 35
        Me.lblHTEMA2Period.Text = "EMA2 Period"
        '
        'txtHTEMA1Period
        '
        Me.txtHTEMA1Period.Location = New System.Drawing.Point(174, 25)
        Me.txtHTEMA1Period.Margin = New System.Windows.Forms.Padding(4)
        Me.txtHTEMA1Period.Name = "txtHTEMA1Period"
        Me.txtHTEMA1Period.Size = New System.Drawing.Size(256, 22)
        Me.txtHTEMA1Period.TabIndex = 0
        Me.txtHTEMA1Period.Tag = "HT EMA1 Period"
        '
        'lblHTEMA1Period
        '
        Me.lblHTEMA1Period.AutoSize = True
        Me.lblHTEMA1Period.Location = New System.Drawing.Point(10, 28)
        Me.lblHTEMA1Period.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblHTEMA1Period.Name = "lblHTEMA1Period"
        Me.lblHTEMA1Period.Size = New System.Drawing.Size(90, 17)
        Me.lblHTEMA1Period.TabIndex = 31
        Me.lblHTEMA1Period.Text = "EMA1 Period"
        '
        'frmNFOSettings
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(580, 329)
        Me.Controls.Add(Me.GroupBox1)
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
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents btnSave As Button
    Friend WithEvents ImageList1 As ImageList
    Friend WithEvents opnFileSettings As OpenFileDialog
    Friend WithEvents GroupBox2 As GroupBox
    Friend WithEvents txtLTEMA2Period As TextBox
    Friend WithEvents lblLTEMA2Period As Label
    Friend WithEvents txtLTEMA1Period As TextBox
    Friend WithEvents lblLTEMA1Period As Label
    Friend WithEvents grpSettings As GroupBox
    Friend WithEvents btnBrowse As Button
    Friend WithEvents txtInstrumentDetalis As TextBox
    Friend WithEvents lblInstrumentDetails As Label
    Friend WithEvents txtLowerTimeFrame As TextBox
    Friend WithEvents lblLowerTimeFrame As Label
    Friend WithEvents txtStrikeRangePer As TextBox
    Friend WithEvents lblStrikeRangePer As Label
    Friend WithEvents txtHigherTimeFrame As TextBox
    Friend WithEvents lblHigherTimeFrame As Label
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents txtHTEMA2Period As TextBox
    Friend WithEvents lblHTEMA2Period As Label
    Friend WithEvents txtHTEMA1Period As TextBox
    Friend WithEvents lblHTEMA1Period As Label
End Class
