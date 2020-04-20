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
        Me.pnlMomentumReversalMainPanelHorizontalSplitter = New System.Windows.Forms.TableLayoutPanel()
        Me.pnlMomentumReversalTopHeaderVerticalSplitter = New System.Windows.Forms.TableLayoutPanel()
        Me.btnNFOStop = New System.Windows.Forms.Button()
        Me.btnNFOStart = New System.Windows.Forms.Button()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.blbNFOTickerStatus = New Bulb.LedBulb()
        Me.lblNFOTickerStatus = New System.Windows.Forms.Label()
        Me.btnNFOSettings = New System.Windows.Forms.Button()
        Me.linklblNFOTradableInstrument = New System.Windows.Forms.LinkLabel()
        Me.pnlMomentumReversalBodyVerticalSplitter = New System.Windows.Forms.TableLayoutPanel()
        Me.PictureBox2 = New System.Windows.Forms.PictureBox()
        Me.pnlMomentumReversalBodyHorizontalSplitter = New System.Windows.Forms.TableLayoutPanel()
        Me.lstNFOLog = New System.Windows.Forms.ListBox()
        Me.sfdgvNFOMainDashboard = New Syncfusion.WinForms.DataGrid.SfDataGrid()
        Me.tmrNFOTickerStatus = New System.Windows.Forms.Timer(Me.components)
        Me.msMainMenuStrip.SuspendLayout()
        Me.tabMain.SuspendLayout()
        Me.tabNFO.SuspendLayout()
        Me.pnlMomentumReversalMainPanelHorizontalSplitter.SuspendLayout()
        Me.pnlMomentumReversalTopHeaderVerticalSplitter.SuspendLayout()
        Me.Panel1.SuspendLayout()
        Me.pnlMomentumReversalBodyVerticalSplitter.SuspendLayout()
        CType(Me.PictureBox2, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlMomentumReversalBodyHorizontalSplitter.SuspendLayout()
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
        Me.tabNFO.Controls.Add(Me.pnlMomentumReversalMainPanelHorizontalSplitter)
        Me.tabNFO.Location = New System.Drawing.Point(4, 25)
        Me.tabNFO.Margin = New System.Windows.Forms.Padding(4)
        Me.tabNFO.Name = "tabNFO"
        Me.tabNFO.Padding = New System.Windows.Forms.Padding(4)
        Me.tabNFO.Size = New System.Drawing.Size(1363, 693)
        Me.tabNFO.TabIndex = 0
        Me.tabNFO.Text = "Algo2Trade"
        Me.tabNFO.UseVisualStyleBackColor = True
        '
        'pnlMomentumReversalMainPanelHorizontalSplitter
        '
        Me.pnlMomentumReversalMainPanelHorizontalSplitter.ColumnCount = 1
        Me.pnlMomentumReversalMainPanelHorizontalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.pnlMomentumReversalMainPanelHorizontalSplitter.Controls.Add(Me.pnlMomentumReversalTopHeaderVerticalSplitter, 0, 0)
        Me.pnlMomentumReversalMainPanelHorizontalSplitter.Controls.Add(Me.pnlMomentumReversalBodyVerticalSplitter, 0, 1)
        Me.pnlMomentumReversalMainPanelHorizontalSplitter.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlMomentumReversalMainPanelHorizontalSplitter.Location = New System.Drawing.Point(4, 4)
        Me.pnlMomentumReversalMainPanelHorizontalSplitter.Margin = New System.Windows.Forms.Padding(4)
        Me.pnlMomentumReversalMainPanelHorizontalSplitter.Name = "pnlMomentumReversalMainPanelHorizontalSplitter"
        Me.pnlMomentumReversalMainPanelHorizontalSplitter.RowCount = 2
        Me.pnlMomentumReversalMainPanelHorizontalSplitter.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7.0!))
        Me.pnlMomentumReversalMainPanelHorizontalSplitter.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 93.0!))
        Me.pnlMomentumReversalMainPanelHorizontalSplitter.Size = New System.Drawing.Size(1355, 685)
        Me.pnlMomentumReversalMainPanelHorizontalSplitter.TabIndex = 0
        '
        'pnlMomentumReversalTopHeaderVerticalSplitter
        '
        Me.pnlMomentumReversalTopHeaderVerticalSplitter.ColumnCount = 15
        Me.pnlMomentumReversalTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlMomentumReversalTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlMomentumReversalTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlMomentumReversalTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlMomentumReversalTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlMomentumReversalTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlMomentumReversalTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlMomentumReversalTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlMomentumReversalTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlMomentumReversalTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlMomentumReversalTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.70379!))
        Me.pnlMomentumReversalTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 0.8166295!))
        Me.pnlMomentumReversalTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 1.707498!))
        Me.pnlMomentumReversalTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 2.741935!))
        Me.pnlMomentumReversalTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 11.29032!))
        Me.pnlMomentumReversalTopHeaderVerticalSplitter.Controls.Add(Me.btnNFOStop, 0, 0)
        Me.pnlMomentumReversalTopHeaderVerticalSplitter.Controls.Add(Me.btnNFOStart, 0, 0)
        Me.pnlMomentumReversalTopHeaderVerticalSplitter.Controls.Add(Me.Panel1, 14, 0)
        Me.pnlMomentumReversalTopHeaderVerticalSplitter.Controls.Add(Me.btnNFOSettings, 9, 0)
        Me.pnlMomentumReversalTopHeaderVerticalSplitter.Controls.Add(Me.linklblNFOTradableInstrument, 10, 0)
        Me.pnlMomentumReversalTopHeaderVerticalSplitter.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlMomentumReversalTopHeaderVerticalSplitter.Location = New System.Drawing.Point(4, 4)
        Me.pnlMomentumReversalTopHeaderVerticalSplitter.Margin = New System.Windows.Forms.Padding(4)
        Me.pnlMomentumReversalTopHeaderVerticalSplitter.Name = "pnlMomentumReversalTopHeaderVerticalSplitter"
        Me.pnlMomentumReversalTopHeaderVerticalSplitter.RowCount = 1
        Me.pnlMomentumReversalTopHeaderVerticalSplitter.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.pnlMomentumReversalTopHeaderVerticalSplitter.Size = New System.Drawing.Size(1347, 39)
        Me.pnlMomentumReversalTopHeaderVerticalSplitter.TabIndex = 0
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
        'Panel1
        '
        Me.Panel1.Controls.Add(Me.blbNFOTickerStatus)
        Me.Panel1.Controls.Add(Me.lblNFOTickerStatus)
        Me.Panel1.Location = New System.Drawing.Point(1189, 4)
        Me.Panel1.Margin = New System.Windows.Forms.Padding(4)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(147, 31)
        Me.Panel1.TabIndex = 9
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
        'linklblNFOTradableInstrument
        '
        Me.linklblNFOTradableInstrument.AutoSize = True
        Me.linklblNFOTradableInstrument.Dock = System.Windows.Forms.DockStyle.Fill
        Me.linklblNFOTradableInstrument.Enabled = False
        Me.linklblNFOTradableInstrument.Location = New System.Drawing.Point(893, 0)
        Me.linklblNFOTradableInstrument.Name = "linklblNFOTradableInstrument"
        Me.linklblNFOTradableInstrument.Size = New System.Drawing.Size(219, 39)
        Me.linklblNFOTradableInstrument.TabIndex = 12
        Me.linklblNFOTradableInstrument.TabStop = True
        Me.linklblNFOTradableInstrument.Text = "Tradable Instruments: 0"
        Me.linklblNFOTradableInstrument.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'pnlMomentumReversalBodyVerticalSplitter
        '
        Me.pnlMomentumReversalBodyVerticalSplitter.ColumnCount = 2
        Me.pnlMomentumReversalBodyVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70.0!))
        Me.pnlMomentumReversalBodyVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30.0!))
        Me.pnlMomentumReversalBodyVerticalSplitter.Controls.Add(Me.PictureBox2, 0, 0)
        Me.pnlMomentumReversalBodyVerticalSplitter.Controls.Add(Me.pnlMomentumReversalBodyHorizontalSplitter, 0, 0)
        Me.pnlMomentumReversalBodyVerticalSplitter.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlMomentumReversalBodyVerticalSplitter.Location = New System.Drawing.Point(4, 51)
        Me.pnlMomentumReversalBodyVerticalSplitter.Margin = New System.Windows.Forms.Padding(4)
        Me.pnlMomentumReversalBodyVerticalSplitter.Name = "pnlMomentumReversalBodyVerticalSplitter"
        Me.pnlMomentumReversalBodyVerticalSplitter.RowCount = 1
        Me.pnlMomentumReversalBodyVerticalSplitter.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.pnlMomentumReversalBodyVerticalSplitter.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 630.0!))
        Me.pnlMomentumReversalBodyVerticalSplitter.Size = New System.Drawing.Size(1347, 630)
        Me.pnlMomentumReversalBodyVerticalSplitter.TabIndex = 1
        '
        'PictureBox2
        '
        Me.PictureBox2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.PictureBox2.Image = CType(resources.GetObject("PictureBox2.Image"), System.Drawing.Image)
        Me.PictureBox2.Location = New System.Drawing.Point(945, 2)
        Me.PictureBox2.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.PictureBox2.Name = "PictureBox2"
        Me.PictureBox2.Size = New System.Drawing.Size(399, 626)
        Me.PictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.PictureBox2.TabIndex = 2
        Me.PictureBox2.TabStop = False
        '
        'pnlMomentumReversalBodyHorizontalSplitter
        '
        Me.pnlMomentumReversalBodyHorizontalSplitter.ColumnCount = 1
        Me.pnlMomentumReversalBodyHorizontalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.pnlMomentumReversalBodyHorizontalSplitter.Controls.Add(Me.lstNFOLog, 0, 1)
        Me.pnlMomentumReversalBodyHorizontalSplitter.Controls.Add(Me.sfdgvNFOMainDashboard, 0, 0)
        Me.pnlMomentumReversalBodyHorizontalSplitter.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlMomentumReversalBodyHorizontalSplitter.Location = New System.Drawing.Point(4, 4)
        Me.pnlMomentumReversalBodyHorizontalSplitter.Margin = New System.Windows.Forms.Padding(4)
        Me.pnlMomentumReversalBodyHorizontalSplitter.Name = "pnlMomentumReversalBodyHorizontalSplitter"
        Me.pnlMomentumReversalBodyHorizontalSplitter.RowCount = 2
        Me.pnlMomentumReversalBodyHorizontalSplitter.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 1.286174!))
        Me.pnlMomentumReversalBodyHorizontalSplitter.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 98.71383!))
        Me.pnlMomentumReversalBodyHorizontalSplitter.Size = New System.Drawing.Size(934, 622)
        Me.pnlMomentumReversalBodyHorizontalSplitter.TabIndex = 0
        '
        'lstNFOLog
        '
        Me.lstNFOLog.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lstNFOLog.ForeColor = System.Drawing.Color.FromArgb(CType(CType(29, Byte), Integer), CType(CType(29, Byte), Integer), CType(CType(29, Byte), Integer))
        Me.lstNFOLog.FormattingEnabled = True
        Me.lstNFOLog.ItemHeight = 16
        Me.lstNFOLog.Location = New System.Drawing.Point(4, 11)
        Me.lstNFOLog.Margin = New System.Windows.Forms.Padding(4)
        Me.lstNFOLog.Name = "lstNFOLog"
        Me.lstNFOLog.Size = New System.Drawing.Size(926, 607)
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
        Me.sfdgvNFOMainDashboard.Location = New System.Drawing.Point(4, 4)
        Me.sfdgvNFOMainDashboard.Margin = New System.Windows.Forms.Padding(4)
        Me.sfdgvNFOMainDashboard.Name = "sfdgvNFOMainDashboard"
        Me.sfdgvNFOMainDashboard.PasteOption = Syncfusion.WinForms.DataGrid.Enums.PasteOptions.None
        Me.sfdgvNFOMainDashboard.Size = New System.Drawing.Size(37, 1)
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
        Me.pnlMomentumReversalMainPanelHorizontalSplitter.ResumeLayout(False)
        Me.pnlMomentumReversalTopHeaderVerticalSplitter.ResumeLayout(False)
        Me.pnlMomentumReversalTopHeaderVerticalSplitter.PerformLayout()
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        Me.pnlMomentumReversalBodyVerticalSplitter.ResumeLayout(False)
        CType(Me.PictureBox2, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlMomentumReversalBodyHorizontalSplitter.ResumeLayout(False)
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
    Friend WithEvents pnlMomentumReversalMainPanelHorizontalSplitter As TableLayoutPanel
    Friend WithEvents pnlMomentumReversalTopHeaderVerticalSplitter As TableLayoutPanel
    Friend WithEvents btnNFOStart As Button
    Friend WithEvents Panel1 As Panel
    Friend WithEvents lblNFOTickerStatus As Label
    Friend WithEvents blbNFOTickerStatus As Bulb.LedBulb
    Friend WithEvents pnlMomentumReversalBodyVerticalSplitter As TableLayoutPanel
    Friend WithEvents pnlMomentumReversalBodyHorizontalSplitter As TableLayoutPanel
    Friend WithEvents sfdgvNFOMainDashboard As Syncfusion.WinForms.DataGrid.SfDataGrid
    Friend WithEvents lstNFOLog As ListBox
    Friend WithEvents tmrNFOTickerStatus As Timer
    Friend WithEvents btnNFOStop As Button
    Friend WithEvents btnNFOSettings As Button
    Friend WithEvents miAdvancedOptions As ToolStripMenuItem
    Friend WithEvents linklblNFOTradableInstrument As LinkLabel
    Friend WithEvents PictureBox2 As PictureBox
End Class
