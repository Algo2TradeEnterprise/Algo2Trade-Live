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
        Me.lblFirstEntryTime = New System.Windows.Forms.Label()
        Me.lblSecondEntryTime = New System.Windows.Forms.Label()
        Me.lblEODExitTime = New System.Windows.Forms.Label()
        Me.dtpckrFirstEntryTime = New System.Windows.Forms.DateTimePicker()
        Me.dtpckrSecondEntryTime = New System.Windows.Forms.DateTimePicker()
        Me.dtpckrEODExitTime = New System.Windows.Forms.DateTimePicker()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.grpQuantity = New System.Windows.Forms.GroupBox()
        Me.txtQuantity = New System.Windows.Forms.TextBox()
        Me.lblQuantity = New System.Windows.Forms.Label()
        Me.txtMargin = New System.Windows.Forms.TextBox()
        Me.lblMargin = New System.Windows.Forms.Label()
        Me.txtCapital = New System.Windows.Forms.TextBox()
        Me.lblCapital = New System.Windows.Forms.Label()
        Me.chbCalculateQuantityFromCapital = New System.Windows.Forms.CheckBox()
        Me.txtStoplossTrailingPercentage = New System.Windows.Forms.TextBox()
        Me.lblStoplossTrailingPercentage = New System.Windows.Forms.Label()
        Me.GroupBox1.SuspendLayout()
        Me.grpQuantity.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnSave
        '
        Me.btnSave.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnSave.ImageKey = "save-icon-36533.png"
        Me.btnSave.ImageList = Me.ImageList1
        Me.btnSave.Location = New System.Drawing.Point(490, 9)
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
        'lblFirstEntryTime
        '
        Me.lblFirstEntryTime.AutoSize = True
        Me.lblFirstEntryTime.Location = New System.Drawing.Point(9, 20)
        Me.lblFirstEntryTime.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblFirstEntryTime.Name = "lblFirstEntryTime"
        Me.lblFirstEntryTime.Size = New System.Drawing.Size(107, 17)
        Me.lblFirstEntryTime.TabIndex = 19
        Me.lblFirstEntryTime.Text = "First Entry Time"
        '
        'lblSecondEntryTime
        '
        Me.lblSecondEntryTime.AutoSize = True
        Me.lblSecondEntryTime.Location = New System.Drawing.Point(9, 56)
        Me.lblSecondEntryTime.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblSecondEntryTime.Name = "lblSecondEntryTime"
        Me.lblSecondEntryTime.Size = New System.Drawing.Size(128, 17)
        Me.lblSecondEntryTime.TabIndex = 21
        Me.lblSecondEntryTime.Text = "Second Entry Time"
        '
        'lblEODExitTime
        '
        Me.lblEODExitTime.AutoSize = True
        Me.lblEODExitTime.Location = New System.Drawing.Point(9, 91)
        Me.lblEODExitTime.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblEODExitTime.Name = "lblEODExitTime"
        Me.lblEODExitTime.Size = New System.Drawing.Size(99, 17)
        Me.lblEODExitTime.TabIndex = 23
        Me.lblEODExitTime.Text = "EOD Exit Time"
        '
        'dtpckrFirstEntryTime
        '
        Me.dtpckrFirstEntryTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpckrFirstEntryTime.Location = New System.Drawing.Point(227, 18)
        Me.dtpckrFirstEntryTime.Name = "dtpckrFirstEntryTime"
        Me.dtpckrFirstEntryTime.ShowUpDown = True
        Me.dtpckrFirstEntryTime.Size = New System.Drawing.Size(242, 22)
        Me.dtpckrFirstEntryTime.TabIndex = 2
        Me.dtpckrFirstEntryTime.Value = New Date(2019, 3, 12, 0, 0, 0, 0)
        '
        'dtpckrSecondEntryTime
        '
        Me.dtpckrSecondEntryTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpckrSecondEntryTime.Location = New System.Drawing.Point(227, 55)
        Me.dtpckrSecondEntryTime.Name = "dtpckrSecondEntryTime"
        Me.dtpckrSecondEntryTime.ShowUpDown = True
        Me.dtpckrSecondEntryTime.Size = New System.Drawing.Size(242, 22)
        Me.dtpckrSecondEntryTime.TabIndex = 3
        Me.dtpckrSecondEntryTime.Value = New Date(2019, 3, 12, 0, 0, 0, 0)
        '
        'dtpckrEODExitTime
        '
        Me.dtpckrEODExitTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpckrEODExitTime.Location = New System.Drawing.Point(227, 90)
        Me.dtpckrEODExitTime.Name = "dtpckrEODExitTime"
        Me.dtpckrEODExitTime.ShowUpDown = True
        Me.dtpckrEODExitTime.Size = New System.Drawing.Size(242, 22)
        Me.dtpckrEODExitTime.TabIndex = 4
        Me.dtpckrEODExitTime.Value = New Date(2019, 3, 12, 0, 0, 0, 0)
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.grpQuantity)
        Me.GroupBox1.Controls.Add(Me.txtStoplossTrailingPercentage)
        Me.GroupBox1.Controls.Add(Me.lblStoplossTrailingPercentage)
        Me.GroupBox1.Controls.Add(Me.dtpckrEODExitTime)
        Me.GroupBox1.Controls.Add(Me.dtpckrSecondEntryTime)
        Me.GroupBox1.Controls.Add(Me.dtpckrFirstEntryTime)
        Me.GroupBox1.Controls.Add(Me.lblEODExitTime)
        Me.GroupBox1.Controls.Add(Me.lblSecondEntryTime)
        Me.GroupBox1.Controls.Add(Me.lblFirstEntryTime)
        Me.GroupBox1.Location = New System.Drawing.Point(5, -2)
        Me.GroupBox1.Margin = New System.Windows.Forms.Padding(4)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Padding = New System.Windows.Forms.Padding(4)
        Me.GroupBox1.Size = New System.Drawing.Size(477, 272)
        Me.GroupBox1.TabIndex = 44
        Me.GroupBox1.TabStop = False
        '
        'grpQuantity
        '
        Me.grpQuantity.Controls.Add(Me.txtQuantity)
        Me.grpQuantity.Controls.Add(Me.lblQuantity)
        Me.grpQuantity.Controls.Add(Me.txtMargin)
        Me.grpQuantity.Controls.Add(Me.lblMargin)
        Me.grpQuantity.Controls.Add(Me.txtCapital)
        Me.grpQuantity.Controls.Add(Me.lblCapital)
        Me.grpQuantity.Controls.Add(Me.chbCalculateQuantityFromCapital)
        Me.grpQuantity.Location = New System.Drawing.Point(0, 158)
        Me.grpQuantity.Name = "grpQuantity"
        Me.grpQuantity.Size = New System.Drawing.Size(477, 112)
        Me.grpQuantity.TabIndex = 56
        Me.grpQuantity.TabStop = False
        '
        'txtQuantity
        '
        Me.txtQuantity.Location = New System.Drawing.Point(227, 49)
        Me.txtQuantity.Margin = New System.Windows.Forms.Padding(4)
        Me.txtQuantity.Name = "txtQuantity"
        Me.txtQuantity.Size = New System.Drawing.Size(242, 22)
        Me.txtQuantity.TabIndex = 60
        Me.txtQuantity.Tag = "Quantity"
        '
        'lblQuantity
        '
        Me.lblQuantity.AutoSize = True
        Me.lblQuantity.Location = New System.Drawing.Point(9, 52)
        Me.lblQuantity.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblQuantity.Name = "lblQuantity"
        Me.lblQuantity.Size = New System.Drawing.Size(61, 17)
        Me.lblQuantity.TabIndex = 61
        Me.lblQuantity.Text = "Quantity"
        '
        'txtMargin
        '
        Me.txtMargin.Location = New System.Drawing.Point(227, 79)
        Me.txtMargin.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMargin.Name = "txtMargin"
        Me.txtMargin.Size = New System.Drawing.Size(242, 22)
        Me.txtMargin.TabIndex = 58
        Me.txtMargin.Tag = "Margin"
        '
        'lblMargin
        '
        Me.lblMargin.AutoSize = True
        Me.lblMargin.Location = New System.Drawing.Point(9, 82)
        Me.lblMargin.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMargin.Name = "lblMargin"
        Me.lblMargin.Size = New System.Drawing.Size(111, 17)
        Me.lblMargin.TabIndex = 59
        Me.lblMargin.Text = "Margin Multiplier"
        '
        'txtCapital
        '
        Me.txtCapital.Location = New System.Drawing.Point(227, 49)
        Me.txtCapital.Margin = New System.Windows.Forms.Padding(4)
        Me.txtCapital.Name = "txtCapital"
        Me.txtCapital.Size = New System.Drawing.Size(242, 22)
        Me.txtCapital.TabIndex = 56
        Me.txtCapital.Tag = "Capital"
        '
        'lblCapital
        '
        Me.lblCapital.AutoSize = True
        Me.lblCapital.Location = New System.Drawing.Point(9, 52)
        Me.lblCapital.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblCapital.Name = "lblCapital"
        Me.lblCapital.Size = New System.Drawing.Size(51, 17)
        Me.lblCapital.TabIndex = 57
        Me.lblCapital.Text = "Capital"
        '
        'chbCalculateQuantityFromCapital
        '
        Me.chbCalculateQuantityFromCapital.AutoSize = True
        Me.chbCalculateQuantityFromCapital.Location = New System.Drawing.Point(12, 19)
        Me.chbCalculateQuantityFromCapital.Name = "chbCalculateQuantityFromCapital"
        Me.chbCalculateQuantityFromCapital.Size = New System.Drawing.Size(228, 21)
        Me.chbCalculateQuantityFromCapital.TabIndex = 0
        Me.chbCalculateQuantityFromCapital.Text = "Calculate Quantity From Capital"
        Me.chbCalculateQuantityFromCapital.UseVisualStyleBackColor = True
        '
        'txtStoplossTrailingPercentage
        '
        Me.txtStoplossTrailingPercentage.Location = New System.Drawing.Point(227, 125)
        Me.txtStoplossTrailingPercentage.Margin = New System.Windows.Forms.Padding(4)
        Me.txtStoplossTrailingPercentage.Name = "txtStoplossTrailingPercentage"
        Me.txtStoplossTrailingPercentage.Size = New System.Drawing.Size(242, 22)
        Me.txtStoplossTrailingPercentage.TabIndex = 5
        Me.txtStoplossTrailingPercentage.Tag = "Stoploss Trailing %"
        '
        'lblStoplossTrailingPercentage
        '
        Me.lblStoplossTrailingPercentage.AutoSize = True
        Me.lblStoplossTrailingPercentage.Location = New System.Drawing.Point(9, 128)
        Me.lblStoplossTrailingPercentage.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblStoplossTrailingPercentage.Name = "lblStoplossTrailingPercentage"
        Me.lblStoplossTrailingPercentage.Size = New System.Drawing.Size(129, 17)
        Me.lblStoplossTrailingPercentage.TabIndex = 55
        Me.lblStoplossTrailingPercentage.Text = "Stoploss Trailing %"
        '
        'frmNFOSettings
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(608, 275)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.btnSave)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmNFOSettings"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "NFO Settings"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.grpQuantity.ResumeLayout(False)
        Me.grpQuantity.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents btnSave As Button
    Friend WithEvents ImageList1 As ImageList
    Friend WithEvents opnFileSettings As OpenFileDialog
    Friend WithEvents lblFirstEntryTime As Label
    Friend WithEvents lblSecondEntryTime As Label
    Friend WithEvents lblEODExitTime As Label
    Friend WithEvents dtpckrFirstEntryTime As DateTimePicker
    Friend WithEvents dtpckrSecondEntryTime As DateTimePicker
    Friend WithEvents dtpckrEODExitTime As DateTimePicker
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents txtStoplossTrailingPercentage As TextBox
    Friend WithEvents lblStoplossTrailingPercentage As Label
    Friend WithEvents grpQuantity As GroupBox
    Friend WithEvents chbCalculateQuantityFromCapital As CheckBox
    Friend WithEvents txtQuantity As TextBox
    Friend WithEvents lblQuantity As Label
    Friend WithEvents txtMargin As TextBox
    Friend WithEvents lblMargin As Label
    Friend WithEvents txtCapital As TextBox
    Friend WithEvents lblCapital As Label
End Class
