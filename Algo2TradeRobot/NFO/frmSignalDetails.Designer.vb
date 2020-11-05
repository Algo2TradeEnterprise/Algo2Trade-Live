<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmSignalDetails
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmSignalDetails))
        Me.dgvSignalDetails = New System.Windows.Forms.DataGridView()
        CType(Me.dgvSignalDetails, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'dgvSignalDetails
        '
        Me.dgvSignalDetails.AllowUserToAddRows = False
        Me.dgvSignalDetails.AllowUserToDeleteRows = False
        Me.dgvSignalDetails.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvSignalDetails.Dock = System.Windows.Forms.DockStyle.Fill
        Me.dgvSignalDetails.Location = New System.Drawing.Point(0, 0)
        Me.dgvSignalDetails.Name = "dgvSignalDetails"
        Me.dgvSignalDetails.ReadOnly = True
        Me.dgvSignalDetails.RowHeadersVisible = False
        Me.dgvSignalDetails.RowTemplate.Height = 24
        Me.dgvSignalDetails.Size = New System.Drawing.Size(1782, 553)
        Me.dgvSignalDetails.TabIndex = 0
        '
        'frmSignalDetails
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1782, 553)
        Me.Controls.Add(Me.dgvSignalDetails)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmSignalDetails"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Signal Details"
        CType(Me.dgvSignalDetails, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents dgvSignalDetails As DataGridView
End Class
