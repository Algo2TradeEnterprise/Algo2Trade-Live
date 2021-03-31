<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmPortfolio
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
        Dim LegendItem1 As System.Windows.Forms.DataVisualization.Charting.LegendItem = New System.Windows.Forms.DataVisualization.Charting.LegendItem()
        Dim Series1 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Dim Title1 As System.Windows.Forms.DataVisualization.Charting.Title = New System.Windows.Forms.DataVisualization.Charting.Title()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmPortfolio))
        Me.chrtDetails = New System.Windows.Forms.DataVisualization.Charting.Chart()
        CType(Me.chrtDetails, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'chrtDetails
        '
        ChartArea1.Name = "ChartArea1"
        ChartArea1.Position.Auto = False
        ChartArea1.Position.Height = 94.0!
        ChartArea1.Position.Width = 75.0!
        ChartArea1.Position.X = 3.0!
        ChartArea1.Position.Y = 3.0!
        Me.chrtDetails.ChartAreas.Add(ChartArea1)
        Me.chrtDetails.Dock = System.Windows.Forms.DockStyle.Fill
        LegendItem1.Color = System.Drawing.Color.Red
        LegendItem1.Name = "Investment"
        Legend1.CustomItems.Add(LegendItem1)
        Legend1.Name = "Legend1"
        Me.chrtDetails.Legends.Add(Legend1)
        Me.chrtDetails.Location = New System.Drawing.Point(0, 0)
        Me.chrtDetails.Name = "chrtDetails"
        Series1.ChartArea = "ChartArea1"
        Series1.Color = System.Drawing.Color.Green
        Series1.Legend = "Legend1"
        Series1.LegendText = "Return"
        Series1.Name = "Investment/Return"
        Me.chrtDetails.Series.Add(Series1)
        Me.chrtDetails.Size = New System.Drawing.Size(1469, 581)
        Me.chrtDetails.TabIndex = 0
        Me.chrtDetails.Text = "Portfolio"
        Title1.DockedToChartArea = "ChartArea1"
        Title1.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, CType((System.Drawing.FontStyle.Bold Or System.Drawing.FontStyle.Italic), System.Drawing.FontStyle), System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Title1.Name = "Title1"
        Title1.Text = "Portfolio"
        Me.chrtDetails.Titles.Add(Title1)
        '
        'frmPortfolio
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1469, 581)
        Me.Controls.Add(Me.chrtDetails)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "frmPortfolio"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Overall Portfolio"
        CType(Me.chrtDetails, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents chrtDetails As DataVisualization.Charting.Chart
End Class
