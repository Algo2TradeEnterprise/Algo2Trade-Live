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
        Me.txtSupertrendMultiplier = New System.Windows.Forms.TextBox()
        Me.lblLTEMA2Period = New System.Windows.Forms.Label()
        Me.txtSupertrendPeriod = New System.Windows.Forms.TextBox()
        Me.lblLTEMA1Period = New System.Windows.Forms.Label()
        Me.btnBrowse = New System.Windows.Forms.Button()
        Me.txtInstrumentDetalis = New System.Windows.Forms.TextBox()
        Me.lblInstrumentDetails = New System.Windows.Forms.Label()
        Me.txtSignalTimeframe = New System.Windows.Forms.TextBox()
        Me.lblSignalTimeframe = New System.Windows.Forms.Label()
        Me.txtExpireBefore = New System.Windows.Forms.TextBox()
        Me.lblExpireBefore = New System.Windows.Forms.Label()
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
        'txtSupertrendMultiplier
        '
        Me.txtSupertrendMultiplier.Location = New System.Drawing.Point(178, 116)
        Me.txtSupertrendMultiplier.Margin = New System.Windows.Forms.Padding(4)
        Me.txtSupertrendMultiplier.Name = "txtSupertrendMultiplier"
        Me.txtSupertrendMultiplier.Size = New System.Drawing.Size(76, 22)
        Me.txtSupertrendMultiplier.TabIndex = 4
        Me.txtSupertrendMultiplier.Tag = "Supertrend Multiplier"
        '
        'lblLTEMA2Period
        '
        Me.lblLTEMA2Period.AutoSize = True
        Me.lblLTEMA2Period.Location = New System.Drawing.Point(14, 120)
        Me.lblLTEMA2Period.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblLTEMA2Period.Name = "lblLTEMA2Period"
        Me.lblLTEMA2Period.Size = New System.Drawing.Size(139, 17)
        Me.lblLTEMA2Period.TabIndex = 35
        Me.lblLTEMA2Period.Text = "Supertrend Multiplier"
        '
        'txtSupertrendPeriod
        '
        Me.txtSupertrendPeriod.Location = New System.Drawing.Point(178, 83)
        Me.txtSupertrendPeriod.Margin = New System.Windows.Forms.Padding(4)
        Me.txtSupertrendPeriod.Name = "txtSupertrendPeriod"
        Me.txtSupertrendPeriod.Size = New System.Drawing.Size(76, 22)
        Me.txtSupertrendPeriod.TabIndex = 3
        Me.txtSupertrendPeriod.Tag = "Supertrend Period"
        '
        'lblLTEMA1Period
        '
        Me.lblLTEMA1Period.AutoSize = True
        Me.lblLTEMA1Period.Location = New System.Drawing.Point(15, 86)
        Me.lblLTEMA1Period.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblLTEMA1Period.Name = "lblLTEMA1Period"
        Me.lblLTEMA1Period.Size = New System.Drawing.Size(124, 17)
        Me.lblLTEMA1Period.TabIndex = 31
        Me.lblLTEMA1Period.Text = "Supertrend Period"
        '
        'btnBrowse
        '
        Me.btnBrowse.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnBrowse.Location = New System.Drawing.Point(532, 149)
        Me.btnBrowse.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnBrowse.Name = "btnBrowse"
        Me.btnBrowse.Size = New System.Drawing.Size(40, 23)
        Me.btnBrowse.TabIndex = 5
        Me.btnBrowse.Text = "..."
        Me.btnBrowse.UseVisualStyleBackColor = True
        '
        'txtInstrumentDetalis
        '
        Me.txtInstrumentDetalis.Location = New System.Drawing.Point(178, 150)
        Me.txtInstrumentDetalis.Margin = New System.Windows.Forms.Padding(4)
        Me.txtInstrumentDetalis.Name = "txtInstrumentDetalis"
        Me.txtInstrumentDetalis.ReadOnly = True
        Me.txtInstrumentDetalis.Size = New System.Drawing.Size(347, 22)
        Me.txtInstrumentDetalis.TabIndex = 6
        '
        'lblInstrumentDetails
        '
        Me.lblInstrumentDetails.AutoSize = True
        Me.lblInstrumentDetails.Location = New System.Drawing.Point(13, 153)
        Me.lblInstrumentDetails.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblInstrumentDetails.Name = "lblInstrumentDetails"
        Me.lblInstrumentDetails.Size = New System.Drawing.Size(121, 17)
        Me.lblInstrumentDetails.TabIndex = 8
        Me.lblInstrumentDetails.Text = "Instrument Details"
        '
        'txtSignalTimeframe
        '
        Me.txtSignalTimeframe.Location = New System.Drawing.Point(178, 16)
        Me.txtSignalTimeframe.Margin = New System.Windows.Forms.Padding(4)
        Me.txtSignalTimeframe.Name = "txtSignalTimeframe"
        Me.txtSignalTimeframe.Size = New System.Drawing.Size(75, 22)
        Me.txtSignalTimeframe.TabIndex = 1
        Me.txtSignalTimeframe.Tag = "Signal Timeframe"
        '
        'lblSignalTimeframe
        '
        Me.lblSignalTimeframe.AutoSize = True
        Me.lblSignalTimeframe.Location = New System.Drawing.Point(13, 19)
        Me.lblSignalTimeframe.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblSignalTimeframe.Name = "lblSignalTimeframe"
        Me.lblSignalTimeframe.Size = New System.Drawing.Size(150, 17)
        Me.lblSignalTimeframe.TabIndex = 3
        Me.lblSignalTimeframe.Text = "Signal Timeframe(min)"
        '
        'txtExpireBefore
        '
        Me.txtExpireBefore.Location = New System.Drawing.Point(178, 50)
        Me.txtExpireBefore.Margin = New System.Windows.Forms.Padding(4)
        Me.txtExpireBefore.Name = "txtExpireBefore"
        Me.txtExpireBefore.Size = New System.Drawing.Size(75, 22)
        Me.txtExpireBefore.TabIndex = 2
        Me.txtExpireBefore.Tag = "Expire Before (Days)"
        '
        'lblExpireBefore
        '
        Me.lblExpireBefore.AutoSize = True
        Me.lblExpireBefore.Location = New System.Drawing.Point(13, 53)
        Me.lblExpireBefore.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblExpireBefore.Name = "lblExpireBefore"
        Me.lblExpireBefore.Size = New System.Drawing.Size(139, 17)
        Me.lblExpireBefore.TabIndex = 37
        Me.lblExpireBefore.Text = "Expire Before (Days)"
        '
        'frmNFOSettings
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(580, 185)
        Me.Controls.Add(Me.txtExpireBefore)
        Me.Controls.Add(Me.lblExpireBefore)
        Me.Controls.Add(Me.txtSupertrendMultiplier)
        Me.Controls.Add(Me.btnBrowse)
        Me.Controls.Add(Me.lblLTEMA2Period)
        Me.Controls.Add(Me.txtSupertrendPeriod)
        Me.Controls.Add(Me.txtInstrumentDetalis)
        Me.Controls.Add(Me.lblLTEMA1Period)
        Me.Controls.Add(Me.lblInstrumentDetails)
        Me.Controls.Add(Me.btnSave)
        Me.Controls.Add(Me.txtSignalTimeframe)
        Me.Controls.Add(Me.lblSignalTimeframe)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmNFOSettings"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Settings"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents btnSave As Button
    Friend WithEvents ImageList1 As ImageList
    Friend WithEvents opnFileSettings As OpenFileDialog
    Friend WithEvents txtSupertrendMultiplier As TextBox
    Friend WithEvents lblLTEMA2Period As Label
    Friend WithEvents txtSupertrendPeriod As TextBox
    Friend WithEvents lblLTEMA1Period As Label
    Friend WithEvents btnBrowse As Button
    Friend WithEvents txtInstrumentDetalis As TextBox
    Friend WithEvents lblInstrumentDetails As Label
    Friend WithEvents txtSignalTimeframe As TextBox
    Friend WithEvents lblSignalTimeframe As Label
    Friend WithEvents txtExpireBefore As TextBox
    Friend WithEvents lblExpireBefore As Label
End Class
