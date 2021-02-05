<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmSignalDetails
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
        Dim ChartArea1 As System.Windows.Forms.DataVisualization.Charting.ChartArea = New System.Windows.Forms.DataVisualization.Charting.ChartArea()
        Dim ChartArea2 As System.Windows.Forms.DataVisualization.Charting.ChartArea = New System.Windows.Forms.DataVisualization.Charting.ChartArea()
        Dim Legend1 As System.Windows.Forms.DataVisualization.Charting.Legend = New System.Windows.Forms.DataVisualization.Charting.Legend()
        Dim Series1 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Dim Series2 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Dim Series3 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmSignalDetails))
        Me.dgvSignalDetails = New System.Windows.Forms.DataGridView()
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.chrtDetails = New System.Windows.Forms.DataVisualization.Charting.Chart()
        Me.tlpExport = New System.Windows.Forms.TableLayoutPanel()
        Me.btnExport = New System.Windows.Forms.Button()
        Me.saveFile = New System.Windows.Forms.SaveFileDialog()
        CType(Me.dgvSignalDetails, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TableLayoutPanel1.SuspendLayout()
        CType(Me.chrtDetails, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.tlpExport.SuspendLayout()
        Me.SuspendLayout()
        '
        'dgvSignalDetails
        '
        Me.dgvSignalDetails.AllowUserToAddRows = False
        Me.dgvSignalDetails.AllowUserToDeleteRows = False
        Me.dgvSignalDetails.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvSignalDetails.Dock = System.Windows.Forms.DockStyle.Fill
        Me.dgvSignalDetails.Location = New System.Drawing.Point(3, 3)
        Me.dgvSignalDetails.Name = "dgvSignalDetails"
        Me.dgvSignalDetails.ReadOnly = True
        Me.dgvSignalDetails.RowHeadersVisible = False
        Me.dgvSignalDetails.RowTemplate.Height = 24
        Me.dgvSignalDetails.Size = New System.Drawing.Size(1459, 308)
        Me.dgvSignalDetails.TabIndex = 0
        '
        'TableLayoutPanel1
        '
        Me.TableLayoutPanel1.ColumnCount = 1
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel1.Controls.Add(Me.dgvSignalDetails, 0, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.chrtDetails, 0, 1)
        Me.TableLayoutPanel1.Controls.Add(Me.tlpExport, 0, 2)
        Me.TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutPanel1.Location = New System.Drawing.Point(0, 0)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        Me.TableLayoutPanel1.RowCount = 3
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 32.0!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 64.0!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 4.0!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20.0!))
        Me.TableLayoutPanel1.Size = New System.Drawing.Size(1465, 983)
        Me.TableLayoutPanel1.TabIndex = 1
        '
        'chrtDetails
        '
        ChartArea1.Name = "ChartArea1"
        ChartArea2.Name = "ChartArea2"
        Me.chrtDetails.ChartAreas.Add(ChartArea1)
        Me.chrtDetails.ChartAreas.Add(ChartArea2)
        Me.chrtDetails.Dock = System.Windows.Forms.DockStyle.Fill
        Legend1.Name = "Legend1"
        Me.chrtDetails.Legends.Add(Legend1)
        Me.chrtDetails.Location = New System.Drawing.Point(3, 317)
        Me.chrtDetails.Name = "chrtDetails"
        Series1.BorderWidth = 5
        Series1.ChartArea = "ChartArea1"
        Series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line
        Series1.Legend = "Legend1"
        Series1.Name = "Desire Value Line"
        Series2.BorderWidth = 5
        Series2.ChartArea = "ChartArea1"
        Series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line
        Series2.Legend = "Legend1"
        Series2.Name = "Current Value Line"
        Series3.ChartArea = "ChartArea2"
        Series3.Color = System.Drawing.Color.ForestGreen
        Series3.Legend = "Legend1"
        Series3.Name = "Investment/Return"
        Me.chrtDetails.Series.Add(Series1)
        Me.chrtDetails.Series.Add(Series2)
        Me.chrtDetails.Series.Add(Series3)
        Me.chrtDetails.Size = New System.Drawing.Size(1459, 623)
        Me.chrtDetails.TabIndex = 1
        Me.chrtDetails.Text = "Details"
        '
        'tlpExport
        '
        Me.tlpExport.ColumnCount = 2
        Me.tlpExport.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.tlpExport.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.tlpExport.Controls.Add(Me.btnExport, 1, 0)
        Me.tlpExport.Dock = System.Windows.Forms.DockStyle.Fill
        Me.tlpExport.Location = New System.Drawing.Point(3, 946)
        Me.tlpExport.Name = "tlpExport"
        Me.tlpExport.RowCount = 1
        Me.tlpExport.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.tlpExport.Size = New System.Drawing.Size(1459, 34)
        Me.tlpExport.TabIndex = 3
        '
        'btnExport
        '
        Me.btnExport.Dock = System.Windows.Forms.DockStyle.Right
        Me.btnExport.Location = New System.Drawing.Point(1332, 3)
        Me.btnExport.Name = "btnExport"
        Me.btnExport.Size = New System.Drawing.Size(124, 28)
        Me.btnExport.TabIndex = 0
        Me.btnExport.Text = "Export"
        Me.btnExport.UseVisualStyleBackColor = True
        '
        'saveFile
        '
        '
        'frmSignalDetails
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1465, 983)
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MinimizeBox = False
        Me.Name = "frmSignalDetails"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Signal Details"
        CType(Me.dgvSignalDetails, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TableLayoutPanel1.ResumeLayout(False)
        CType(Me.chrtDetails, System.ComponentModel.ISupportInitialize).EndInit()
        Me.tlpExport.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents dgvSignalDetails As DataGridView
    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents chrtDetails As DataVisualization.Charting.Chart
    Friend WithEvents tlpExport As TableLayoutPanel
    Friend WithEvents btnExport As Button
    Friend WithEvents saveFile As SaveFileDialog
End Class
