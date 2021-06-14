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
        Me.grpTradeDetails = New System.Windows.Forms.GroupBox()
        Me.txtEntrySDMultiplier = New System.Windows.Forms.TextBox()
        Me.lblEntrySDMultiplier = New System.Windows.Forms.Label()
        Me.txtSignalTimeframe = New System.Windows.Forms.TextBox()
        Me.lblSignalTimeframe = New System.Windows.Forms.Label()
        Me.txtLoopBackPeriod = New System.Windows.Forms.TextBox()
        Me.lblLoopBackPeriod = New System.Windows.Forms.Label()
        Me.btnBrowse = New System.Windows.Forms.Button()
        Me.txtInstrumentDetalis = New System.Windows.Forms.TextBox()
        Me.lblInstrumentDetails = New System.Windows.Forms.Label()
        Me.txtMaxSpreadPercentage = New System.Windows.Forms.TextBox()
        Me.lblMaxSpreadPercentage = New System.Windows.Forms.Label()
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
        'grpTradeDetails
        '
        Me.grpTradeDetails.Controls.Add(Me.txtMaxSpreadPercentage)
        Me.grpTradeDetails.Controls.Add(Me.lblMaxSpreadPercentage)
        Me.grpTradeDetails.Controls.Add(Me.txtEntrySDMultiplier)
        Me.grpTradeDetails.Controls.Add(Me.lblEntrySDMultiplier)
        Me.grpTradeDetails.Controls.Add(Me.txtSignalTimeframe)
        Me.grpTradeDetails.Controls.Add(Me.lblSignalTimeframe)
        Me.grpTradeDetails.Controls.Add(Me.txtLoopBackPeriod)
        Me.grpTradeDetails.Controls.Add(Me.lblLoopBackPeriod)
        Me.grpTradeDetails.Controls.Add(Me.btnBrowse)
        Me.grpTradeDetails.Controls.Add(Me.txtInstrumentDetalis)
        Me.grpTradeDetails.Controls.Add(Me.lblInstrumentDetails)
        Me.grpTradeDetails.Location = New System.Drawing.Point(4, 0)
        Me.grpTradeDetails.Margin = New System.Windows.Forms.Padding(4)
        Me.grpTradeDetails.Name = "grpTradeDetails"
        Me.grpTradeDetails.Padding = New System.Windows.Forms.Padding(4)
        Me.grpTradeDetails.Size = New System.Drawing.Size(477, 182)
        Me.grpTradeDetails.TabIndex = 1
        Me.grpTradeDetails.TabStop = False
        Me.grpTradeDetails.Text = "Trade Details"
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
        'txtLoopBackPeriod
        '
        Me.txtLoopBackPeriod.Location = New System.Drawing.Point(227, 52)
        Me.txtLoopBackPeriod.Margin = New System.Windows.Forms.Padding(4)
        Me.txtLoopBackPeriod.Name = "txtLoopBackPeriod"
        Me.txtLoopBackPeriod.Size = New System.Drawing.Size(243, 22)
        Me.txtLoopBackPeriod.TabIndex = 1
        Me.txtLoopBackPeriod.Tag = "Days Back"
        '
        'lblLoopBackPeriod
        '
        Me.lblLoopBackPeriod.AutoSize = True
        Me.lblLoopBackPeriod.Location = New System.Drawing.Point(8, 54)
        Me.lblLoopBackPeriod.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblLoopBackPeriod.Name = "lblLoopBackPeriod"
        Me.lblLoopBackPeriod.Size = New System.Drawing.Size(120, 17)
        Me.lblLoopBackPeriod.TabIndex = 39
        Me.lblLoopBackPeriod.Text = "Loop Back Period"
        '
        'btnBrowse
        '
        Me.btnBrowse.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnBrowse.Location = New System.Drawing.Point(428, 149)
        Me.btnBrowse.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnBrowse.Name = "btnBrowse"
        Me.btnBrowse.Size = New System.Drawing.Size(42, 23)
        Me.btnBrowse.TabIndex = 4
        Me.btnBrowse.Text = "..."
        Me.btnBrowse.UseVisualStyleBackColor = True
        '
        'txtInstrumentDetalis
        '
        Me.txtInstrumentDetalis.Location = New System.Drawing.Point(227, 150)
        Me.txtInstrumentDetalis.Margin = New System.Windows.Forms.Padding(4)
        Me.txtInstrumentDetalis.Name = "txtInstrumentDetalis"
        Me.txtInstrumentDetalis.ReadOnly = True
        Me.txtInstrumentDetalis.Size = New System.Drawing.Size(194, 22)
        Me.txtInstrumentDetalis.TabIndex = 4
        '
        'lblInstrumentDetails
        '
        Me.lblInstrumentDetails.AutoSize = True
        Me.lblInstrumentDetails.Location = New System.Drawing.Point(8, 153)
        Me.lblInstrumentDetails.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblInstrumentDetails.Name = "lblInstrumentDetails"
        Me.lblInstrumentDetails.Size = New System.Drawing.Size(121, 17)
        Me.lblInstrumentDetails.TabIndex = 8
        Me.lblInstrumentDetails.Text = "Instrument Details"
        '
        'txtMaxSpreadPercentage
        '
        Me.txtMaxSpreadPercentage.Location = New System.Drawing.Point(227, 118)
        Me.txtMaxSpreadPercentage.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMaxSpreadPercentage.Name = "txtMaxSpreadPercentage"
        Me.txtMaxSpreadPercentage.Size = New System.Drawing.Size(243, 22)
        Me.txtMaxSpreadPercentage.TabIndex = 3
        Me.txtMaxSpreadPercentage.Tag = "Max Spread %"
        '
        'lblMaxSpreadPercentage
        '
        Me.lblMaxSpreadPercentage.AutoSize = True
        Me.lblMaxSpreadPercentage.Location = New System.Drawing.Point(8, 120)
        Me.lblMaxSpreadPercentage.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMaxSpreadPercentage.Name = "lblMaxSpreadPercentage"
        Me.lblMaxSpreadPercentage.Size = New System.Drawing.Size(99, 17)
        Me.lblMaxSpreadPercentage.TabIndex = 57
        Me.lblMaxSpreadPercentage.Text = "Max Spread %"
        '
        'frmNFOSettings
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(602, 185)
        Me.Controls.Add(Me.grpTradeDetails)
        Me.Controls.Add(Me.btnSave)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmNFOSettings"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Settings"
        Me.grpTradeDetails.ResumeLayout(False)
        Me.grpTradeDetails.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents btnSave As Button
    Friend WithEvents ImageList1 As ImageList
    Friend WithEvents opnFileSettings As OpenFileDialog
    Friend WithEvents grpTradeDetails As GroupBox
    Friend WithEvents txtSignalTimeframe As TextBox
    Friend WithEvents lblSignalTimeframe As Label
    Friend WithEvents txtLoopBackPeriod As TextBox
    Friend WithEvents lblLoopBackPeriod As Label
    Friend WithEvents btnBrowse As Button
    Friend WithEvents txtInstrumentDetalis As TextBox
    Friend WithEvents lblInstrumentDetails As Label
    Friend WithEvents txtEntrySDMultiplier As TextBox
    Friend WithEvents lblEntrySDMultiplier As Label
    Friend WithEvents txtMaxSpreadPercentage As TextBox
    Friend WithEvents lblMaxSpreadPercentage As Label
End Class
