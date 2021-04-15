<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmAliceUserDetails
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmAliceUserDetails))
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.txtAliceAppID = New System.Windows.Forms.TextBox()
        Me.lblAliceAppID = New System.Windows.Forms.Label()
        Me.txtAlice2FAAnswer = New System.Windows.Forms.TextBox()
        Me.lblAlice2FAAnswer = New System.Windows.Forms.Label()
        Me.txtAliceAPISecret = New System.Windows.Forms.TextBox()
        Me.txtAlicePassword = New System.Windows.Forms.TextBox()
        Me.txtAliceUserId = New System.Windows.Forms.TextBox()
        Me.lblAliceAPISecret = New System.Windows.Forms.Label()
        Me.lblAlicePassword = New System.Windows.Forms.Label()
        Me.lblAliceUserId = New System.Windows.Forms.Label()
        Me.btnSaveAliceUserDetails = New System.Windows.Forms.Button()
        Me.ImageList1 = New System.Windows.Forms.ImageList(Me.components)
        Me.GroupBox1.SuspendLayout()
        Me.SuspendLayout()
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.txtAliceAppID)
        Me.GroupBox1.Controls.Add(Me.lblAliceAppID)
        Me.GroupBox1.Controls.Add(Me.txtAlice2FAAnswer)
        Me.GroupBox1.Controls.Add(Me.lblAlice2FAAnswer)
        Me.GroupBox1.Controls.Add(Me.txtAliceAPISecret)
        Me.GroupBox1.Controls.Add(Me.txtAlicePassword)
        Me.GroupBox1.Controls.Add(Me.txtAliceUserId)
        Me.GroupBox1.Controls.Add(Me.lblAliceAPISecret)
        Me.GroupBox1.Controls.Add(Me.lblAlicePassword)
        Me.GroupBox1.Controls.Add(Me.lblAliceUserId)
        Me.GroupBox1.Location = New System.Drawing.Point(10, 10)
        Me.GroupBox1.Margin = New System.Windows.Forms.Padding(4)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Padding = New System.Windows.Forms.Padding(4)
        Me.GroupBox1.Size = New System.Drawing.Size(460, 203)
        Me.GroupBox1.TabIndex = 2
        Me.GroupBox1.TabStop = False
        '
        'txtAliceAppID
        '
        Me.txtAliceAppID.Location = New System.Drawing.Point(103, 131)
        Me.txtAliceAppID.Margin = New System.Windows.Forms.Padding(4)
        Me.txtAliceAppID.Name = "txtAliceAppID"
        Me.txtAliceAppID.Size = New System.Drawing.Size(348, 22)
        Me.txtAliceAppID.TabIndex = 8
        '
        'lblAliceAppID
        '
        Me.lblAliceAppID.AutoSize = True
        Me.lblAliceAppID.Location = New System.Drawing.Point(9, 134)
        Me.lblAliceAppID.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblAliceAppID.Name = "lblAliceAppID"
        Me.lblAliceAppID.Size = New System.Drawing.Size(50, 17)
        Me.lblAliceAppID.TabIndex = 10
        Me.lblAliceAppID.Text = "App ID"
        '
        'txtAlice2FAAnswer
        '
        Me.txtAlice2FAAnswer.Location = New System.Drawing.Point(103, 96)
        Me.txtAlice2FAAnswer.Margin = New System.Windows.Forms.Padding(4)
        Me.txtAlice2FAAnswer.Name = "txtAlice2FAAnswer"
        Me.txtAlice2FAAnswer.PasswordChar = Global.Microsoft.VisualBasic.ChrW(36)
        Me.txtAlice2FAAnswer.Size = New System.Drawing.Size(185, 22)
        Me.txtAlice2FAAnswer.TabIndex = 7
        '
        'lblAlice2FAAnswer
        '
        Me.lblAlice2FAAnswer.AutoSize = True
        Me.lblAlice2FAAnswer.Location = New System.Drawing.Point(9, 100)
        Me.lblAlice2FAAnswer.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblAlice2FAAnswer.Name = "lblAlice2FAAnswer"
        Me.lblAlice2FAAnswer.Size = New System.Drawing.Size(83, 17)
        Me.lblAlice2FAAnswer.TabIndex = 2
        Me.lblAlice2FAAnswer.Text = "2FA Answer"
        '
        'txtAliceAPISecret
        '
        Me.txtAliceAPISecret.Location = New System.Drawing.Point(103, 165)
        Me.txtAliceAPISecret.Margin = New System.Windows.Forms.Padding(4)
        Me.txtAliceAPISecret.Name = "txtAliceAPISecret"
        Me.txtAliceAPISecret.Size = New System.Drawing.Size(348, 22)
        Me.txtAliceAPISecret.TabIndex = 9
        '
        'txtAlicePassword
        '
        Me.txtAlicePassword.Location = New System.Drawing.Point(103, 60)
        Me.txtAlicePassword.Margin = New System.Windows.Forms.Padding(4)
        Me.txtAlicePassword.Name = "txtAlicePassword"
        Me.txtAlicePassword.PasswordChar = Global.Microsoft.VisualBasic.ChrW(36)
        Me.txtAlicePassword.Size = New System.Drawing.Size(185, 22)
        Me.txtAlicePassword.TabIndex = 6
        '
        'txtAliceUserId
        '
        Me.txtAliceUserId.Location = New System.Drawing.Point(103, 21)
        Me.txtAliceUserId.Margin = New System.Windows.Forms.Padding(4)
        Me.txtAliceUserId.Name = "txtAliceUserId"
        Me.txtAliceUserId.Size = New System.Drawing.Size(185, 22)
        Me.txtAliceUserId.TabIndex = 5
        '
        'lblAliceAPISecret
        '
        Me.lblAliceAPISecret.AutoSize = True
        Me.lblAliceAPISecret.Location = New System.Drawing.Point(9, 169)
        Me.lblAliceAPISecret.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblAliceAPISecret.Name = "lblAliceAPISecret"
        Me.lblAliceAPISecret.Size = New System.Drawing.Size(74, 17)
        Me.lblAliceAPISecret.TabIndex = 4
        Me.lblAliceAPISecret.Text = "API Secret"
        '
        'lblAlicePassword
        '
        Me.lblAlicePassword.AutoSize = True
        Me.lblAlicePassword.Location = New System.Drawing.Point(9, 64)
        Me.lblAlicePassword.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblAlicePassword.Name = "lblAlicePassword"
        Me.lblAlicePassword.Size = New System.Drawing.Size(69, 17)
        Me.lblAlicePassword.TabIndex = 1
        Me.lblAlicePassword.Text = "Password"
        '
        'lblAliceUserId
        '
        Me.lblAliceUserId.AutoSize = True
        Me.lblAliceUserId.Location = New System.Drawing.Point(9, 25)
        Me.lblAliceUserId.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblAliceUserId.Name = "lblAliceUserId"
        Me.lblAliceUserId.Size = New System.Drawing.Size(49, 17)
        Me.lblAliceUserId.TabIndex = 0
        Me.lblAliceUserId.Text = "UserId"
        '
        'btnSaveAliceUserDetails
        '
        Me.btnSaveAliceUserDetails.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnSaveAliceUserDetails.ImageKey = "save-icon-36533.png"
        Me.btnSaveAliceUserDetails.ImageList = Me.ImageList1
        Me.btnSaveAliceUserDetails.Location = New System.Drawing.Point(481, 19)
        Me.btnSaveAliceUserDetails.Margin = New System.Windows.Forms.Padding(4)
        Me.btnSaveAliceUserDetails.Name = "btnSaveAliceUserDetails"
        Me.btnSaveAliceUserDetails.Size = New System.Drawing.Size(112, 58)
        Me.btnSaveAliceUserDetails.TabIndex = 3
        Me.btnSaveAliceUserDetails.Text = "&Save"
        Me.btnSaveAliceUserDetails.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.btnSaveAliceUserDetails.UseVisualStyleBackColor = True
        '
        'ImageList1
        '
        Me.ImageList1.ImageStream = CType(resources.GetObject("ImageList1.ImageStream"), System.Windows.Forms.ImageListStreamer)
        Me.ImageList1.TransparentColor = System.Drawing.Color.Transparent
        Me.ImageList1.Images.SetKeyName(0, "save-icon-36533.png")
        '
        'frmAliceUserDetails
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(603, 226)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.btnSaveAliceUserDetails)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmAliceUserDetails"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Alice-Blue User Details"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents txtAlice2FAAnswer As TextBox
    Friend WithEvents lblAlice2FAAnswer As Label
    Friend WithEvents txtAliceAPISecret As TextBox
    Friend WithEvents txtAlicePassword As TextBox
    Friend WithEvents txtAliceUserId As TextBox
    Friend WithEvents lblAliceAPISecret As Label
    Friend WithEvents lblAlicePassword As Label
    Friend WithEvents lblAliceUserId As Label
    Friend WithEvents btnSaveAliceUserDetails As Button
    Friend WithEvents ImageList1 As ImageList
    Friend WithEvents txtAliceAppID As TextBox
    Friend WithEvents lblAliceAppID As Label
End Class
