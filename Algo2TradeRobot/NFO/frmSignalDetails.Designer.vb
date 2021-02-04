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
        Dim ChartArea1 As System.Windows.Forms.DataVisualization.Charting.ChartArea = New System.Windows.Forms.DataVisualization.Charting.ChartArea()
        Dim Legend1 As System.Windows.Forms.DataVisualization.Charting.Legend = New System.Windows.Forms.DataVisualization.Charting.Legend()
        Dim Series1 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Dim Series2 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Dim ChartArea2 As System.Windows.Forms.DataVisualization.Charting.ChartArea = New System.Windows.Forms.DataVisualization.Charting.ChartArea()
        Dim Legend2 As System.Windows.Forms.DataVisualization.Charting.Legend = New System.Windows.Forms.DataVisualization.Charting.Legend()
        Dim Series3 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmSignalDetails))
        Me.dgvSignalDetails = New System.Windows.Forms.DataGridView()
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.chrtValueLine = New System.Windows.Forms.DataVisualization.Charting.Chart()
        Me.chrtInvestmentReturn = New System.Windows.Forms.DataVisualization.Charting.Chart()
        CType(Me.dgvSignalDetails, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TableLayoutPanel1.SuspendLayout()
        CType(Me.chrtValueLine, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.chrtInvestmentReturn, System.ComponentModel.ISupportInitialize).BeginInit()
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
        Me.dgvSignalDetails.Size = New System.Drawing.Size(1459, 301)
        Me.dgvSignalDetails.TabIndex = 0
        '
        'TableLayoutPanel1
        '
        Me.TableLayoutPanel1.ColumnCount = 1
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel1.Controls.Add(Me.dgvSignalDetails, 0, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.chrtValueLine, 0, 1)
        Me.TableLayoutPanel1.Controls.Add(Me.chrtInvestmentReturn, 0, 2)
        Me.TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutPanel1.Location = New System.Drawing.Point(0, 0)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        Me.TableLayoutPanel1.RowCount = 4
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 32.0!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 32.0!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 32.0!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 4.0!))
        Me.TableLayoutPanel1.Size = New System.Drawing.Size(1465, 960)
        Me.TableLayoutPanel1.TabIndex = 1
        '
        'chrtValueLine
        '
        ChartArea1.Name = "ChartArea1"
        Me.chrtValueLine.ChartAreas.Add(ChartArea1)
        Me.chrtValueLine.Dock = System.Windows.Forms.DockStyle.Fill
        Legend1.Name = "Legend1"
        Me.chrtValueLine.Legends.Add(Legend1)
        Me.chrtValueLine.Location = New System.Drawing.Point(3, 310)
        Me.chrtValueLine.Name = "chrtValueLine"
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
        Me.chrtValueLine.Series.Add(Series1)
        Me.chrtValueLine.Series.Add(Series2)
        Me.chrtValueLine.Size = New System.Drawing.Size(1459, 301)
        Me.chrtValueLine.TabIndex = 1
        Me.chrtValueLine.Text = "Value Line"
        '
        'chrtInvestmentReturn
        '
        ChartArea2.Name = "ChartArea1"
        Me.chrtInvestmentReturn.ChartAreas.Add(ChartArea2)
        Me.chrtInvestmentReturn.Dock = System.Windows.Forms.DockStyle.Fill
        Legend2.Name = "Legend1"
        Me.chrtInvestmentReturn.Legends.Add(Legend2)
        Me.chrtInvestmentReturn.Location = New System.Drawing.Point(3, 617)
        Me.chrtInvestmentReturn.Name = "chrtInvestmentReturn"
        Series3.ChartArea = "ChartArea1"
        Series3.Legend = "Legend1"
        Series3.Name = "Investment/Return"
        Me.chrtInvestmentReturn.Series.Add(Series3)
        Me.chrtInvestmentReturn.Size = New System.Drawing.Size(1459, 301)
        Me.chrtInvestmentReturn.TabIndex = 2
        Me.chrtInvestmentReturn.Text = "Chart1"
        '
        'frmSignalDetails
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1465, 960)
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MinimizeBox = False
        Me.Name = "frmSignalDetails"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Signal Details"
        CType(Me.dgvSignalDetails, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TableLayoutPanel1.ResumeLayout(False)
        CType(Me.chrtValueLine, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.chrtInvestmentReturn, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents dgvSignalDetails As DataGridView
    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents chrtValueLine As DataVisualization.Charting.Chart
    Friend WithEvents chrtInvestmentReturn As DataVisualization.Charting.Chart
End Class
