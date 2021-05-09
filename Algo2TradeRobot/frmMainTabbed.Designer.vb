<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmMainTabbed
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmMainTabbed))
        Me.msMainMenuStrip = New System.Windows.Forms.MenuStrip()
        Me.miOptions = New System.Windows.Forms.ToolStripMenuItem()
        Me.miUserDetails = New System.Windows.Forms.ToolStripMenuItem()
        Me.miAdvancedOptions = New System.Windows.Forms.ToolStripMenuItem()
        Me.miAbout = New System.Windows.Forms.ToolStripMenuItem()
        Me.tabMain = New System.Windows.Forms.TabControl()
        Me.tabNFO = New System.Windows.Forms.TabPage()
        Me.pnlNFOMainPanelHorizontalSplitter = New System.Windows.Forms.TableLayoutPanel()
        Me.pnlNFOTopHeaderVerticalSplitter = New System.Windows.Forms.TableLayoutPanel()
        Me.btnNFOStop = New System.Windows.Forms.Button()
        Me.btnNFOStart = New System.Windows.Forms.Button()
        Me.pnlNFOTicker = New System.Windows.Forms.Panel()
        Me.blbNFOTickerStatus = New Bulb.LedBulb()
        Me.lblNFOTickerStatus = New System.Windows.Forms.Label()
        Me.btnNFOSettings = New System.Windows.Forms.Button()
        Me.linklblNFOTradableInstruments = New System.Windows.Forms.LinkLabel()
        Me.pnlNFOBodyVerticalSplitter = New System.Windows.Forms.TableLayoutPanel()
        Me.pctrBxNFO = New System.Windows.Forms.PictureBox()
        Me.pnlNFOBodyHorizontalSplitter = New System.Windows.Forms.TableLayoutPanel()
        Me.lstNFOLog = New System.Windows.Forms.ListBox()
        Me.sfdgvNFOMainDashboard = New Syncfusion.WinForms.DataGrid.SfDataGrid()
        Me.tmrNFOTickerStatus = New System.Windows.Forms.Timer(Me.components)
        Me.msMainMenuStrip.SuspendLayout()
        Me.tabMain.SuspendLayout()
        Me.tabNFO.SuspendLayout()
        Me.pnlNFOMainPanelHorizontalSplitter.SuspendLayout()
        Me.pnlNFOTopHeaderVerticalSplitter.SuspendLayout()
        Me.pnlNFOTicker.SuspendLayout()
        Me.pnlNFOBodyVerticalSplitter.SuspendLayout()
        CType(Me.pctrBxNFO, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlNFOBodyHorizontalSplitter.SuspendLayout()
        CType(Me.sfdgvNFOMainDashboard, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'msMainMenuStrip
        '
        Me.msMainMenuStrip.ImageScalingSize = New System.Drawing.Size(20, 20)
        Me.msMainMenuStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.miOptions, Me.miAbout})
        Me.msMainMenuStrip.Location = New System.Drawing.Point(0, 0)
        Me.msMainMenuStrip.Name = "msMainMenuStrip"
        Me.msMainMenuStrip.Padding = New System.Windows.Forms.Padding(8, 2, 0, 2)
        Me.msMainMenuStrip.Size = New System.Drawing.Size(1371, 28)
        Me.msMainMenuStrip.TabIndex = 0
        Me.msMainMenuStrip.Text = "MenuStrip1"
        '
        'miOptions
        '
        Me.miOptions.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.miUserDetails, Me.miAdvancedOptions})
        Me.miOptions.Name = "miOptions"
        Me.miOptions.Size = New System.Drawing.Size(73, 24)
        Me.miOptions.Text = "&Options"
        '
        'miUserDetails
        '
        Me.miUserDetails.Name = "miUserDetails"
        Me.miUserDetails.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.F2), System.Windows.Forms.Keys)
        Me.miUserDetails.Size = New System.Drawing.Size(263, 26)
        Me.miUserDetails.Text = "&User Details"
        '
        'miAdvancedOptions
        '
        Me.miAdvancedOptions.Name = "miAdvancedOptions"
        Me.miAdvancedOptions.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.F7), System.Windows.Forms.Keys)
        Me.miAdvancedOptions.Size = New System.Drawing.Size(263, 26)
        Me.miAdvancedOptions.Text = "A&dvanced Options"
        '
        'miAbout
        '
        Me.miAbout.Name = "miAbout"
        Me.miAbout.Size = New System.Drawing.Size(62, 24)
        Me.miAbout.Text = "&About"
        '
        'tabMain
        '
        Me.tabMain.Controls.Add(Me.tabNFO)
        Me.tabMain.Dock = System.Windows.Forms.DockStyle.Fill
        Me.tabMain.Location = New System.Drawing.Point(0, 28)
        Me.tabMain.Margin = New System.Windows.Forms.Padding(4)
        Me.tabMain.Name = "tabMain"
        Me.tabMain.SelectedIndex = 0
        Me.tabMain.Size = New System.Drawing.Size(1371, 722)
        Me.tabMain.TabIndex = 1
        '
        'tabNFO
        '
        Me.tabNFO.Controls.Add(Me.pnlNFOMainPanelHorizontalSplitter)
        Me.tabNFO.Location = New System.Drawing.Point(4, 25)
        Me.tabNFO.Margin = New System.Windows.Forms.Padding(4)
        Me.tabNFO.Name = "tabNFO"
        Me.tabNFO.Padding = New System.Windows.Forms.Padding(4)
        Me.tabNFO.Size = New System.Drawing.Size(1363, 693)
        Me.tabNFO.TabIndex = 0
        Me.tabNFO.Text = "Algo2Trade"
        Me.tabNFO.UseVisualStyleBackColor = True
        '
        'pnlNFOMainPanelHorizontalSplitter
        '
        Me.pnlNFOMainPanelHorizontalSplitter.ColumnCount = 1
        Me.pnlNFOMainPanelHorizontalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.pnlNFOMainPanelHorizontalSplitter.Controls.Add(Me.pnlNFOTopHeaderVerticalSplitter, 0, 0)
        Me.pnlNFOMainPanelHorizontalSplitter.Controls.Add(Me.pnlNFOBodyVerticalSplitter, 0, 1)
        Me.pnlNFOMainPanelHorizontalSplitter.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlNFOMainPanelHorizontalSplitter.Location = New System.Drawing.Point(4, 4)
        Me.pnlNFOMainPanelHorizontalSplitter.Margin = New System.Windows.Forms.Padding(4)
        Me.pnlNFOMainPanelHorizontalSplitter.Name = "pnlNFOMainPanelHorizontalSplitter"
        Me.pnlNFOMainPanelHorizontalSplitter.RowCount = 2
        Me.pnlNFOMainPanelHorizontalSplitter.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7.0!))
        Me.pnlNFOMainPanelHorizontalSplitter.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 93.0!))
        Me.pnlNFOMainPanelHorizontalSplitter.Size = New System.Drawing.Size(1355, 685)
        Me.pnlNFOMainPanelHorizontalSplitter.TabIndex = 0
        '
        'pnlNFOTopHeaderVerticalSplitter
        '
        Me.pnlNFOTopHeaderVerticalSplitter.ColumnCount = 15
        Me.pnlNFOTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlNFOTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlNFOTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlNFOTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlNFOTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlNFOTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlNFOTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlNFOTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlNFOTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlNFOTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlNFOTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.70379!))
        Me.pnlNFOTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 0.8166295!))
        Me.pnlNFOTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 1.707498!))
        Me.pnlNFOTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 2.741935!))
        Me.pnlNFOTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 11.29032!))
        Me.pnlNFOTopHeaderVerticalSplitter.Controls.Add(Me.btnNFOStop, 0, 0)
        Me.pnlNFOTopHeaderVerticalSplitter.Controls.Add(Me.btnNFOStart, 0, 0)
        Me.pnlNFOTopHeaderVerticalSplitter.Controls.Add(Me.pnlNFOTicker, 14, 0)
        Me.pnlNFOTopHeaderVerticalSplitter.Controls.Add(Me.btnNFOSettings, 9, 0)
        Me.pnlNFOTopHeaderVerticalSplitter.Controls.Add(Me.linklblNFOTradableInstruments, 10, 0)
        Me.pnlNFOTopHeaderVerticalSplitter.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlNFOTopHeaderVerticalSplitter.Location = New System.Drawing.Point(4, 4)
        Me.pnlNFOTopHeaderVerticalSplitter.Margin = New System.Windows.Forms.Padding(4)
        Me.pnlNFOTopHeaderVerticalSplitter.Name = "pnlNFOTopHeaderVerticalSplitter"
        Me.pnlNFOTopHeaderVerticalSplitter.RowCount = 1
        Me.pnlNFOTopHeaderVerticalSplitter.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.pnlNFOTopHeaderVerticalSplitter.Size = New System.Drawing.Size(1347, 39)
        Me.pnlNFOTopHeaderVerticalSplitter.TabIndex = 0
        '
        'btnNFOStop
        '
        Me.btnNFOStop.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnNFOStop.Location = New System.Drawing.Point(93, 4)
        Me.btnNFOStop.Margin = New System.Windows.Forms.Padding(4)
        Me.btnNFOStop.Name = "btnNFOStop"
        Me.btnNFOStop.Size = New System.Drawing.Size(81, 31)
        Me.btnNFOStop.TabIndex = 10
        Me.btnNFOStop.Text = "Stop"
        Me.btnNFOStop.UseVisualStyleBackColor = True
        '
        'btnNFOStart
        '
        Me.btnNFOStart.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnNFOStart.Location = New System.Drawing.Point(4, 4)
        Me.btnNFOStart.Margin = New System.Windows.Forms.Padding(4)
        Me.btnNFOStart.Name = "btnNFOStart"
        Me.btnNFOStart.Size = New System.Drawing.Size(81, 31)
        Me.btnNFOStart.TabIndex = 2
        Me.btnNFOStart.Text = "Start"
        Me.btnNFOStart.UseVisualStyleBackColor = True
        '
        'pnlNFOTicker
        '
        Me.pnlNFOTicker.Controls.Add(Me.blbNFOTickerStatus)
        Me.pnlNFOTicker.Controls.Add(Me.lblNFOTickerStatus)
        Me.pnlNFOTicker.Location = New System.Drawing.Point(1189, 4)
        Me.pnlNFOTicker.Margin = New System.Windows.Forms.Padding(4)
        Me.pnlNFOTicker.Name = "pnlNFOTicker"
        Me.pnlNFOTicker.Size = New System.Drawing.Size(147, 31)
        Me.pnlNFOTicker.TabIndex = 9
        '
        'blbNFOTickerStatus
        '
        Me.blbNFOTickerStatus.Color = System.Drawing.Color.Pink
        Me.blbNFOTickerStatus.Dock = System.Windows.Forms.DockStyle.Right
        Me.blbNFOTickerStatus.Location = New System.Drawing.Point(100, 0)
        Me.blbNFOTickerStatus.Margin = New System.Windows.Forms.Padding(4)
        Me.blbNFOTickerStatus.Name = "blbNFOTickerStatus"
        Me.blbNFOTickerStatus.On = True
        Me.blbNFOTickerStatus.Size = New System.Drawing.Size(47, 31)
        Me.blbNFOTickerStatus.TabIndex = 7
        Me.blbNFOTickerStatus.Text = "LedBulb1"
        '
        'lblNFOTickerStatus
        '
        Me.lblNFOTickerStatus.AutoSize = True
        Me.lblNFOTickerStatus.Location = New System.Drawing.Point(9, 9)
        Me.lblNFOTickerStatus.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblNFOTickerStatus.Name = "lblNFOTickerStatus"
        Me.lblNFOTickerStatus.Size = New System.Drawing.Size(91, 17)
        Me.lblNFOTickerStatus.TabIndex = 9
        Me.lblNFOTickerStatus.Text = "Ticker Status"
        '
        'btnNFOSettings
        '
        Me.btnNFOSettings.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnNFOSettings.Location = New System.Drawing.Point(805, 4)
        Me.btnNFOSettings.Margin = New System.Windows.Forms.Padding(4)
        Me.btnNFOSettings.Name = "btnNFOSettings"
        Me.btnNFOSettings.Size = New System.Drawing.Size(81, 31)
        Me.btnNFOSettings.TabIndex = 11
        Me.btnNFOSettings.Text = "Settings"
        Me.btnNFOSettings.UseVisualStyleBackColor = True
        '
        'linklblNFOTradableInstruments
        '
        Me.linklblNFOTradableInstruments.AutoSize = True
        Me.linklblNFOTradableInstruments.Dock = System.Windows.Forms.DockStyle.Fill
        Me.linklblNFOTradableInstruments.Enabled = False
        Me.linklblNFOTradableInstruments.Location = New System.Drawing.Point(893, 0)
        Me.linklblNFOTradableInstruments.Name = "linklblNFOTradableInstruments"
        Me.linklblNFOTradableInstruments.Size = New System.Drawing.Size(219, 39)
        Me.linklblNFOTradableInstruments.TabIndex = 12
        Me.linklblNFOTradableInstruments.TabStop = True
        Me.linklblNFOTradableInstruments.Text = "Tradable Instruments: 0"
        Me.linklblNFOTradableInstruments.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'pnlNFOBodyVerticalSplitter
        '
        Me.pnlNFOBodyVerticalSplitter.ColumnCount = 2
        Me.pnlNFOBodyVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70.0!))
        Me.pnlNFOBodyVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30.0!))
        Me.pnlNFOBodyVerticalSplitter.Controls.Add(Me.pctrBxNFO, 0, 0)
        Me.pnlNFOBodyVerticalSplitter.Controls.Add(Me.pnlNFOBodyHorizontalSplitter, 0, 0)
        Me.pnlNFOBodyVerticalSplitter.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlNFOBodyVerticalSplitter.Location = New System.Drawing.Point(4, 51)
        Me.pnlNFOBodyVerticalSplitter.Margin = New System.Windows.Forms.Padding(4)
        Me.pnlNFOBodyVerticalSplitter.Name = "pnlNFOBodyVerticalSplitter"
        Me.pnlNFOBodyVerticalSplitter.RowCount = 1
        Me.pnlNFOBodyVerticalSplitter.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.pnlNFOBodyVerticalSplitter.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 630.0!))
        Me.pnlNFOBodyVerticalSplitter.Size = New System.Drawing.Size(1347, 630)
        Me.pnlNFOBodyVerticalSplitter.TabIndex = 1
        '
        'pctrBxNFO
        '
        Me.pctrBxNFO.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pctrBxNFO.Image = CType(resources.GetObject("pctrBxNFO.Image"), System.Drawing.Image)
        Me.pctrBxNFO.Location = New System.Drawing.Point(945, 2)
        Me.pctrBxNFO.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.pctrBxNFO.Name = "pctrBxNFO"
        Me.pctrBxNFO.Size = New System.Drawing.Size(399, 626)
        Me.pctrBxNFO.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.pctrBxNFO.TabIndex = 2
        Me.pctrBxNFO.TabStop = False
        '
        'pnlNFOBodyHorizontalSplitter
        '
        Me.pnlNFOBodyHorizontalSplitter.ColumnCount = 1
        Me.pnlNFOBodyHorizontalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.pnlNFOBodyHorizontalSplitter.Controls.Add(Me.lstNFOLog, 0, 1)
        Me.pnlNFOBodyHorizontalSplitter.Controls.Add(Me.sfdgvNFOMainDashboard, 0, 0)
        Me.pnlNFOBodyHorizontalSplitter.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlNFOBodyHorizontalSplitter.Location = New System.Drawing.Point(4, 4)
        Me.pnlNFOBodyHorizontalSplitter.Margin = New System.Windows.Forms.Padding(4)
        Me.pnlNFOBodyHorizontalSplitter.Name = "pnlNFOBodyHorizontalSplitter"
        Me.pnlNFOBodyHorizontalSplitter.RowCount = 2
        Me.pnlNFOBodyHorizontalSplitter.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70.0!))
        Me.pnlNFOBodyHorizontalSplitter.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30.0!))
        Me.pnlNFOBodyHorizontalSplitter.Size = New System.Drawing.Size(934, 622)
        Me.pnlNFOBodyHorizontalSplitter.TabIndex = 0
        '
        'lstNFOLog
        '
        Me.lstNFOLog.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lstNFOLog.ForeColor = System.Drawing.Color.FromArgb(CType(CType(29, Byte), Integer), CType(CType(29, Byte), Integer), CType(CType(29, Byte), Integer))
        Me.lstNFOLog.FormattingEnabled = True
        Me.lstNFOLog.HorizontalScrollbar = True
        Me.lstNFOLog.ItemHeight = 16
        Me.lstNFOLog.Location = New System.Drawing.Point(4, 439)
        Me.lstNFOLog.Margin = New System.Windows.Forms.Padding(4)
        Me.lstNFOLog.Name = "lstNFOLog"
        Me.lstNFOLog.Size = New System.Drawing.Size(926, 179)
        Me.lstNFOLog.TabIndex = 9
        '
        'sfdgvNFOMainDashboard
        '
        Me.sfdgvNFOMainDashboard.AccessibleName = "Table"
        Me.sfdgvNFOMainDashboard.AllowDraggingColumns = True
        Me.sfdgvNFOMainDashboard.AllowEditing = False
        Me.sfdgvNFOMainDashboard.AllowFiltering = True
        Me.sfdgvNFOMainDashboard.AllowResizingColumns = True
        Me.sfdgvNFOMainDashboard.AutoGenerateColumnsMode = Syncfusion.WinForms.DataGrid.Enums.AutoGenerateColumnsMode.SmartReset
        Me.sfdgvNFOMainDashboard.AutoSizeColumnsMode = Syncfusion.WinForms.DataGrid.Enums.AutoSizeColumnsMode.AllCells
        Me.sfdgvNFOMainDashboard.Dock = System.Windows.Forms.DockStyle.Fill
        Me.sfdgvNFOMainDashboard.Location = New System.Drawing.Point(4, 4)
        Me.sfdgvNFOMainDashboard.Margin = New System.Windows.Forms.Padding(4)
        Me.sfdgvNFOMainDashboard.Name = "sfdgvNFOMainDashboard"
        Me.sfdgvNFOMainDashboard.PasteOption = Syncfusion.WinForms.DataGrid.Enums.PasteOptions.None
        Me.sfdgvNFOMainDashboard.Size = New System.Drawing.Size(926, 427)
        Me.sfdgvNFOMainDashboard.TabIndex = 6
        Me.sfdgvNFOMainDashboard.Text = "SfDataGrid1"
        '
        'tmrNFOTickerStatus
        '
        Me.tmrNFOTickerStatus.Enabled = True
        '
        'frmMainTabbed
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1371, 750)
        Me.Controls.Add(Me.tabMain)
        Me.Controls.Add(Me.msMainMenuStrip)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MainMenuStrip = Me.msMainMenuStrip
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.Name = "frmMainTabbed"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Algo2Trade Robot"
        Me.msMainMenuStrip.ResumeLayout(False)
        Me.msMainMenuStrip.PerformLayout()
        Me.tabMain.ResumeLayout(False)
        Me.tabNFO.ResumeLayout(False)
        Me.pnlNFOMainPanelHorizontalSplitter.ResumeLayout(False)
        Me.pnlNFOTopHeaderVerticalSplitter.ResumeLayout(False)
        Me.pnlNFOTopHeaderVerticalSplitter.PerformLayout()
        Me.pnlNFOTicker.ResumeLayout(False)
        Me.pnlNFOTicker.PerformLayout()
        Me.pnlNFOBodyVerticalSplitter.ResumeLayout(False)
        CType(Me.pctrBxNFO, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlNFOBodyHorizontalSplitter.ResumeLayout(False)
        CType(Me.sfdgvNFOMainDashboard, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents msMainMenuStrip As MenuStrip
    Friend WithEvents miOptions As ToolStripMenuItem
    Friend WithEvents miUserDetails As ToolStripMenuItem
    Friend WithEvents miAbout As ToolStripMenuItem
    Friend WithEvents tabMain As TabControl
    Friend WithEvents tabNFO As TabPage
    Friend WithEvents pnlNFOMainPanelHorizontalSplitter As TableLayoutPanel
    Friend WithEvents pnlNFOTopHeaderVerticalSplitter As TableLayoutPanel
    Friend WithEvents btnNFOStart As Button
    Friend WithEvents pnlNFOTicker As Panel
    Friend WithEvents lblNFOTickerStatus As Label
    Friend WithEvents blbNFOTickerStatus As Bulb.LedBulb
    Friend WithEvents pnlNFOBodyVerticalSplitter As TableLayoutPanel
    Friend WithEvents pnlNFOBodyHorizontalSplitter As TableLayoutPanel
    Friend WithEvents sfdgvNFOMainDashboard As Syncfusion.WinForms.DataGrid.SfDataGrid
    Friend WithEvents lstNFOLog As ListBox
    Friend WithEvents tmrNFOTickerStatus As Timer
    Friend WithEvents btnNFOStop As Button
    Friend WithEvents btnNFOSettings As Button
    Friend WithEvents miAdvancedOptions As ToolStripMenuItem
    Friend WithEvents linklblNFOTradableInstruments As LinkLabel
    Friend WithEvents pctrBxNFO As PictureBox
End Class
