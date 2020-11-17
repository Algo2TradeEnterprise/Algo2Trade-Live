<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmUpdateSignal
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
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
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmUpdateSignal))
        Me.ImageList1 = New System.Windows.Forms.ImageList(Me.components)
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.txtIterationNumber = New System.Windows.Forms.TextBox()
        Me.lblIterationNumber = New System.Windows.Forms.Label()
        Me.txtEntryPrice = New System.Windows.Forms.TextBox()
        Me.lblEntryPrice = New System.Windows.Forms.Label()
        Me.txtQuantity = New System.Windows.Forms.TextBox()
        Me.lblQuantity = New System.Windows.Forms.Label()
        Me.dtpckrSignalTime = New System.Windows.Forms.DateTimePicker()
        Me.lblSignalTime = New System.Windows.Forms.Label()
        Me.txtEntryRason = New System.Windows.Forms.TextBox()
        Me.lblEntryReason = New System.Windows.Forms.Label()
        Me.btnSave = New System.Windows.Forms.Button()
        Me.GroupBox1.SuspendLayout()
        Me.SuspendLayout()
        '
        'ImageList1
        '
        Me.ImageList1.ImageStream = CType(resources.GetObject("ImageList1.ImageStream"), System.Windows.Forms.ImageListStreamer)
        Me.ImageList1.TransparentColor = System.Drawing.Color.Transparent
        Me.ImageList1.Images.SetKeyName(0, "save-icon-36533.png")
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.txtIterationNumber)
        Me.GroupBox1.Controls.Add(Me.lblIterationNumber)
        Me.GroupBox1.Controls.Add(Me.txtEntryPrice)
        Me.GroupBox1.Controls.Add(Me.lblEntryPrice)
        Me.GroupBox1.Controls.Add(Me.txtQuantity)
        Me.GroupBox1.Controls.Add(Me.lblQuantity)
        Me.GroupBox1.Controls.Add(Me.dtpckrSignalTime)
        Me.GroupBox1.Controls.Add(Me.lblSignalTime)
        Me.GroupBox1.Controls.Add(Me.txtEntryRason)
        Me.GroupBox1.Controls.Add(Me.lblEntryReason)
        Me.GroupBox1.Location = New System.Drawing.Point(3, 2)
        Me.GroupBox1.Margin = New System.Windows.Forms.Padding(4)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Padding = New System.Windows.Forms.Padding(4)
        Me.GroupBox1.Size = New System.Drawing.Size(481, 253)
        Me.GroupBox1.TabIndex = 54
        Me.GroupBox1.TabStop = False
        '
        'txtIterationNumber
        '
        Me.txtIterationNumber.Location = New System.Drawing.Point(227, 118)
        Me.txtIterationNumber.Margin = New System.Windows.Forms.Padding(4)
        Me.txtIterationNumber.Name = "txtIterationNumber"
        Me.txtIterationNumber.Size = New System.Drawing.Size(243, 22)
        Me.txtIterationNumber.TabIndex = 4
        Me.txtIterationNumber.Tag = "Iteration Number"
        '
        'lblIterationNumber
        '
        Me.lblIterationNumber.AutoSize = True
        Me.lblIterationNumber.Location = New System.Drawing.Point(8, 122)
        Me.lblIterationNumber.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblIterationNumber.Name = "lblIterationNumber"
        Me.lblIterationNumber.Size = New System.Drawing.Size(113, 17)
        Me.lblIterationNumber.TabIndex = 59
        Me.lblIterationNumber.Text = "Iteration Number"
        '
        'txtEntryPrice
        '
        Me.txtEntryPrice.Location = New System.Drawing.Point(227, 49)
        Me.txtEntryPrice.Margin = New System.Windows.Forms.Padding(4)
        Me.txtEntryPrice.Name = "txtEntryPrice"
        Me.txtEntryPrice.Size = New System.Drawing.Size(243, 22)
        Me.txtEntryPrice.TabIndex = 2
        Me.txtEntryPrice.Tag = "Entry Price"
        '
        'lblEntryPrice
        '
        Me.lblEntryPrice.AutoSize = True
        Me.lblEntryPrice.Location = New System.Drawing.Point(9, 52)
        Me.lblEntryPrice.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblEntryPrice.Name = "lblEntryPrice"
        Me.lblEntryPrice.Size = New System.Drawing.Size(77, 17)
        Me.lblEntryPrice.TabIndex = 53
        Me.lblEntryPrice.Text = "Entry Price"
        '
        'txtQuantity
        '
        Me.txtQuantity.Location = New System.Drawing.Point(227, 83)
        Me.txtQuantity.Margin = New System.Windows.Forms.Padding(4)
        Me.txtQuantity.Name = "txtQuantity"
        Me.txtQuantity.Size = New System.Drawing.Size(243, 22)
        Me.txtQuantity.TabIndex = 3
        Me.txtQuantity.Tag = "Quantity"
        '
        'lblQuantity
        '
        Me.lblQuantity.AutoSize = True
        Me.lblQuantity.Location = New System.Drawing.Point(8, 87)
        Me.lblQuantity.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblQuantity.Name = "lblQuantity"
        Me.lblQuantity.Size = New System.Drawing.Size(61, 17)
        Me.lblQuantity.TabIndex = 39
        Me.lblQuantity.Text = "Quantity"
        '
        'dtpckrSignalTime
        '
        Me.dtpckrSignalTime.Format = System.Windows.Forms.DateTimePickerFormat.[Short]
        Me.dtpckrSignalTime.Location = New System.Drawing.Point(227, 16)
        Me.dtpckrSignalTime.Name = "dtpckrSignalTime"
        Me.dtpckrSignalTime.Size = New System.Drawing.Size(243, 22)
        Me.dtpckrSignalTime.TabIndex = 1
        Me.dtpckrSignalTime.Value = New Date(2020, 7, 6, 0, 0, 0, 0)
        '
        'lblSignalTime
        '
        Me.lblSignalTime.AutoSize = True
        Me.lblSignalTime.Location = New System.Drawing.Point(9, 18)
        Me.lblSignalTime.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblSignalTime.Name = "lblSignalTime"
        Me.lblSignalTime.Size = New System.Drawing.Size(82, 17)
        Me.lblSignalTime.TabIndex = 19
        Me.lblSignalTime.Text = "Signal Time"
        '
        'txtEntryRason
        '
        Me.txtEntryRason.Location = New System.Drawing.Point(227, 151)
        Me.txtEntryRason.Margin = New System.Windows.Forms.Padding(4)
        Me.txtEntryRason.Multiline = True
        Me.txtEntryRason.Name = "txtEntryRason"
        Me.txtEntryRason.Size = New System.Drawing.Size(242, 94)
        Me.txtEntryRason.TabIndex = 5
        Me.txtEntryRason.Tag = "Entry Reason"
        '
        'lblEntryReason
        '
        Me.lblEntryReason.AutoSize = True
        Me.lblEntryReason.Location = New System.Drawing.Point(9, 154)
        Me.lblEntryReason.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblEntryReason.Name = "lblEntryReason"
        Me.lblEntryReason.Size = New System.Drawing.Size(94, 17)
        Me.lblEntryReason.TabIndex = 3
        Me.lblEntryReason.Text = "Entry Reason"
        '
        'btnSave
        '
        Me.btnSave.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnSave.ImageKey = "save-icon-36533.png"
        Me.btnSave.ImageList = Me.ImageList1
        Me.btnSave.Location = New System.Drawing.Point(488, 10)
        Me.btnSave.Margin = New System.Windows.Forms.Padding(4)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(112, 58)
        Me.btnSave.TabIndex = 53
        Me.btnSave.Text = "&Save"
        Me.btnSave.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.btnSave.UseVisualStyleBackColor = True
        '
        'frmUpdateSignal
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(603, 256)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.btnSave)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "frmUpdateSignal"
        Me.Text = "Update Signal"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents ImageList1 As ImageList
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents txtIterationNumber As TextBox
    Friend WithEvents lblIterationNumber As Label
    Friend WithEvents txtEntryPrice As TextBox
    Friend WithEvents lblEntryPrice As Label
    Friend WithEvents txtQuantity As TextBox
    Friend WithEvents lblQuantity As Label
    Friend WithEvents dtpckrSignalTime As DateTimePicker
    Friend WithEvents lblSignalTime As Label
    Friend WithEvents txtEntryRason As TextBox
    Friend WithEvents lblEntryReason As Label
    Friend WithEvents btnSave As Button
End Class
