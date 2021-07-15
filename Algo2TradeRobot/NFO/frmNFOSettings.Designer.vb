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
        Me.btnBrowse = New System.Windows.Forms.Button()
        Me.txtInstrumentDetalis = New System.Windows.Forms.TextBox()
        Me.lblInstrumentDetails = New System.Windows.Forms.Label()
        Me.lblNoOfParallelPair = New System.Windows.Forms.Label()
        Me.lblMinimumStockPrice = New System.Windows.Forms.Label()
        Me.lblMaximumStockPrice = New System.Windows.Forms.Label()
        Me.lblMaximumBlankCandlePercentage = New System.Windows.Forms.Label()
        Me.txtNoOfParallelPair = New System.Windows.Forms.TextBox()
        Me.txtMinimumStockPrice = New System.Windows.Forms.TextBox()
        Me.txtMaximumStockPrice = New System.Windows.Forms.TextBox()
        Me.txtMaximumBlankCandlePercentage = New System.Windows.Forms.TextBox()
        Me.SuspendLayout()
        '
        'btnSave
        '
        Me.btnSave.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnSave.ImageKey = "save-icon-36533.png"
        Me.btnSave.ImageList = Me.ImageList1
        Me.btnSave.Location = New System.Drawing.Point(437, 8)
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
        'btnBrowse
        '
        Me.btnBrowse.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.8!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnBrowse.Location = New System.Drawing.Point(511, 175)
        Me.btnBrowse.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.btnBrowse.Name = "btnBrowse"
        Me.btnBrowse.Size = New System.Drawing.Size(42, 23)
        Me.btnBrowse.TabIndex = 5
        Me.btnBrowse.Text = "..."
        Me.btnBrowse.UseVisualStyleBackColor = True
        '
        'txtInstrumentDetalis
        '
        Me.txtInstrumentDetalis.Location = New System.Drawing.Point(210, 176)
        Me.txtInstrumentDetalis.Margin = New System.Windows.Forms.Padding(4)
        Me.txtInstrumentDetalis.Name = "txtInstrumentDetalis"
        Me.txtInstrumentDetalis.ReadOnly = True
        Me.txtInstrumentDetalis.Size = New System.Drawing.Size(294, 22)
        Me.txtInstrumentDetalis.TabIndex = 5
        '
        'lblInstrumentDetails
        '
        Me.lblInstrumentDetails.AutoSize = True
        Me.lblInstrumentDetails.Location = New System.Drawing.Point(9, 179)
        Me.lblInstrumentDetails.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblInstrumentDetails.Name = "lblInstrumentDetails"
        Me.lblInstrumentDetails.Size = New System.Drawing.Size(125, 17)
        Me.lblInstrumentDetails.TabIndex = 8
        Me.lblInstrumentDetails.Text = "Instrument Details:"
        '
        'lblNoOfParallelPair
        '
        Me.lblNoOfParallelPair.AutoSize = True
        Me.lblNoOfParallelPair.Location = New System.Drawing.Point(9, 13)
        Me.lblNoOfParallelPair.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblNoOfParallelPair.Name = "lblNoOfParallelPair"
        Me.lblNoOfParallelPair.Size = New System.Drawing.Size(130, 17)
        Me.lblNoOfParallelPair.TabIndex = 9
        Me.lblNoOfParallelPair.Text = "No. of Parallel Pair:"
        '
        'lblMinimumStockPrice
        '
        Me.lblMinimumStockPrice.AutoSize = True
        Me.lblMinimumStockPrice.Location = New System.Drawing.Point(9, 49)
        Me.lblMinimumStockPrice.Name = "lblMinimumStockPrice"
        Me.lblMinimumStockPrice.Size = New System.Drawing.Size(142, 17)
        Me.lblMinimumStockPrice.TabIndex = 11
        Me.lblMinimumStockPrice.Text = "Minimum Stock Price:"
        '
        'lblMaximumStockPrice
        '
        Me.lblMaximumStockPrice.AutoSize = True
        Me.lblMaximumStockPrice.Location = New System.Drawing.Point(9, 87)
        Me.lblMaximumStockPrice.Name = "lblMaximumStockPrice"
        Me.lblMaximumStockPrice.Size = New System.Drawing.Size(145, 17)
        Me.lblMaximumStockPrice.TabIndex = 12
        Me.lblMaximumStockPrice.Text = "Maximum Stock Price:"
        '
        'lblMaximumBlankCandlePercentage
        '
        Me.lblMaximumBlankCandlePercentage.AutoSize = True
        Me.lblMaximumBlankCandlePercentage.Location = New System.Drawing.Point(9, 126)
        Me.lblMaximumBlankCandlePercentage.Name = "lblMaximumBlankCandlePercentage"
        Me.lblMaximumBlankCandlePercentage.Size = New System.Drawing.Size(169, 34)
        Me.lblMaximumBlankCandlePercentage.TabIndex = 13
        Me.lblMaximumBlankCandlePercentage.Text = "Maximum Blank Candle%:" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "(Previous Day)"
        '
        'txtNoOfParallelPair
        '
        Me.txtNoOfParallelPair.Location = New System.Drawing.Point(210, 10)
        Me.txtNoOfParallelPair.Name = "txtNoOfParallelPair"
        Me.txtNoOfParallelPair.Size = New System.Drawing.Size(100, 22)
        Me.txtNoOfParallelPair.TabIndex = 1
        Me.txtNoOfParallelPair.Tag = "No. of Parallel Pair"
        '
        'txtMinimumStockPrice
        '
        Me.txtMinimumStockPrice.Location = New System.Drawing.Point(210, 46)
        Me.txtMinimumStockPrice.Name = "txtMinimumStockPrice"
        Me.txtMinimumStockPrice.Size = New System.Drawing.Size(100, 22)
        Me.txtMinimumStockPrice.TabIndex = 2
        Me.txtMinimumStockPrice.Tag = "Minimum Stock Price"
        '
        'txtMaximumStockPrice
        '
        Me.txtMaximumStockPrice.Location = New System.Drawing.Point(210, 84)
        Me.txtMaximumStockPrice.Name = "txtMaximumStockPrice"
        Me.txtMaximumStockPrice.Size = New System.Drawing.Size(100, 22)
        Me.txtMaximumStockPrice.TabIndex = 3
        Me.txtMaximumStockPrice.Tag = "Maximum Stock Price"
        '
        'txtMaximumBlankCandlePercentage
        '
        Me.txtMaximumBlankCandlePercentage.Location = New System.Drawing.Point(210, 123)
        Me.txtMaximumBlankCandlePercentage.Name = "txtMaximumBlankCandlePercentage"
        Me.txtMaximumBlankCandlePercentage.Size = New System.Drawing.Size(100, 22)
        Me.txtMaximumBlankCandlePercentage.TabIndex = 4
        Me.txtMaximumBlankCandlePercentage.Tag = "Maximum Blank Candle%"
        '
        'frmNFOSettings
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(560, 210)
        Me.Controls.Add(Me.txtMaximumBlankCandlePercentage)
        Me.Controls.Add(Me.txtMaximumStockPrice)
        Me.Controls.Add(Me.txtMinimumStockPrice)
        Me.Controls.Add(Me.txtNoOfParallelPair)
        Me.Controls.Add(Me.lblMaximumBlankCandlePercentage)
        Me.Controls.Add(Me.lblMaximumStockPrice)
        Me.Controls.Add(Me.lblMinimumStockPrice)
        Me.Controls.Add(Me.lblNoOfParallelPair)
        Me.Controls.Add(Me.btnBrowse)
        Me.Controls.Add(Me.txtInstrumentDetalis)
        Me.Controls.Add(Me.btnSave)
        Me.Controls.Add(Me.lblInstrumentDetails)
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
    Friend WithEvents btnBrowse As Button
    Friend WithEvents txtInstrumentDetalis As TextBox
    Friend WithEvents lblInstrumentDetails As Label
    Friend WithEvents lblNoOfParallelPair As Label
    Friend WithEvents lblMinimumStockPrice As Label
    Friend WithEvents lblMaximumStockPrice As Label
    Friend WithEvents lblMaximumBlankCandlePercentage As Label
    Friend WithEvents txtNoOfParallelPair As TextBox
    Friend WithEvents txtMinimumStockPrice As TextBox
    Friend WithEvents txtMaximumStockPrice As TextBox
    Friend WithEvents txtMaximumBlankCandlePercentage As TextBox
End Class
