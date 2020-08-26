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
        Me.linklblNFOTradableInstruments = New System.Windows.Forms.LinkLabel()
        Me.pnlMomentumReversalBodyVerticalSplitter = New System.Windows.Forms.TableLayoutPanel()
        Me.PictureBox2 = New System.Windows.Forms.PictureBox()
        Me.pnlMomentumReversalBodyHorizontalSplitter = New System.Windows.Forms.TableLayoutPanel()
        Me.lstNFOLog = New System.Windows.Forms.ListBox()
        Me.sfdgvNFOMainDashboard = New Syncfusion.WinForms.DataGrid.SfDataGrid()
        Me.tabMCX = New System.Windows.Forms.TabPage()
        Me.pnlOHLMainPanelHorizontalSplitter = New System.Windows.Forms.TableLayoutPanel()
        Me.pnlOHLTopHeaderVerticalSplitter = New System.Windows.Forms.TableLayoutPanel()
        Me.btnMCXStop = New System.Windows.Forms.Button()
        Me.btnMCXStart = New System.Windows.Forms.Button()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.blbMCXTickerStatus = New Bulb.LedBulb()
        Me.lblMCXTickerStatus = New System.Windows.Forms.Label()
        Me.btnMCXSettings = New System.Windows.Forms.Button()
        Me.linklblMCXTradableInstruments = New System.Windows.Forms.LinkLabel()
        Me.pnlOHLBodyVerticalSplitter = New System.Windows.Forms.TableLayoutPanel()
        Me.PictureBox3 = New System.Windows.Forms.PictureBox()
        Me.pnlOHLBodyHorizontalSplitter = New System.Windows.Forms.TableLayoutPanel()
        Me.lstMCXLog = New System.Windows.Forms.ListBox()
        Me.sfdgvMCXMainDashboard = New Syncfusion.WinForms.DataGrid.SfDataGrid()
        Me.tabCDS = New System.Windows.Forms.TabPage()
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.TableLayoutPanel2 = New System.Windows.Forms.TableLayoutPanel()
        Me.btnCDSStop = New System.Windows.Forms.Button()
        Me.btnCDSStart = New System.Windows.Forms.Button()
        Me.Panel3 = New System.Windows.Forms.Panel()
        Me.blbCDSTickerStatus = New Bulb.LedBulb()
        Me.lblCDSTickerStatus = New System.Windows.Forms.Label()
        Me.btnCDSSettings = New System.Windows.Forms.Button()
        Me.linklblCDSTradableInstruments = New System.Windows.Forms.LinkLabel()
        Me.TableLayoutPanel3 = New System.Windows.Forms.TableLayoutPanel()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.TableLayoutPanel4 = New System.Windows.Forms.TableLayoutPanel()
        Me.lstCDSLog = New System.Windows.Forms.ListBox()
        Me.sfdgvCDSMainDashboard = New Syncfusion.WinForms.DataGrid.SfDataGrid()
        Me.tmrNFOTickerStatus = New System.Windows.Forms.Timer(Me.components)
        Me.tmrMCXTickerStatus = New System.Windows.Forms.Timer(Me.components)
        Me.tmrCDSTickerStatus = New System.Windows.Forms.Timer(Me.components)
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
        Me.tabMCX.SuspendLayout()
        Me.pnlOHLMainPanelHorizontalSplitter.SuspendLayout()
        Me.pnlOHLTopHeaderVerticalSplitter.SuspendLayout()
        Me.Panel2.SuspendLayout()
        Me.pnlOHLBodyVerticalSplitter.SuspendLayout()
        CType(Me.PictureBox3, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlOHLBodyHorizontalSplitter.SuspendLayout()
        CType(Me.sfdgvMCXMainDashboard, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.tabCDS.SuspendLayout()
        Me.TableLayoutPanel1.SuspendLayout()
        Me.TableLayoutPanel2.SuspendLayout()
        Me.Panel3.SuspendLayout()
        Me.TableLayoutPanel3.SuspendLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TableLayoutPanel4.SuspendLayout()
        CType(Me.sfdgvCDSMainDashboard, System.ComponentModel.ISupportInitialize).BeginInit()
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
        Me.tabMain.Controls.Add(Me.tabMCX)
        Me.tabMain.Controls.Add(Me.tabCDS)
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
        Me.tabNFO.Text = "Screener"
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
        Me.pnlMomentumReversalTopHeaderVerticalSplitter.Controls.Add(Me.linklblNFOTradableInstruments, 10, 0)
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
        Me.pnlMomentumReversalBodyHorizontalSplitter.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70.0!))
        Me.pnlMomentumReversalBodyHorizontalSplitter.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30.0!))
        Me.pnlMomentumReversalBodyHorizontalSplitter.Size = New System.Drawing.Size(934, 622)
        Me.pnlMomentumReversalBodyHorizontalSplitter.TabIndex = 0
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
        'tabMCX
        '
        Me.tabMCX.Controls.Add(Me.pnlOHLMainPanelHorizontalSplitter)
        Me.tabMCX.Location = New System.Drawing.Point(4, 25)
        Me.tabMCX.Margin = New System.Windows.Forms.Padding(4)
        Me.tabMCX.Name = "tabMCX"
        Me.tabMCX.Padding = New System.Windows.Forms.Padding(4)
        Me.tabMCX.Size = New System.Drawing.Size(1363, 693)
        Me.tabMCX.TabIndex = 1
        Me.tabMCX.Text = "MCX"
        Me.tabMCX.UseVisualStyleBackColor = True
        '
        'pnlOHLMainPanelHorizontalSplitter
        '
        Me.pnlOHLMainPanelHorizontalSplitter.ColumnCount = 1
        Me.pnlOHLMainPanelHorizontalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.pnlOHLMainPanelHorizontalSplitter.Controls.Add(Me.pnlOHLTopHeaderVerticalSplitter, 0, 0)
        Me.pnlOHLMainPanelHorizontalSplitter.Controls.Add(Me.pnlOHLBodyVerticalSplitter, 0, 1)
        Me.pnlOHLMainPanelHorizontalSplitter.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlOHLMainPanelHorizontalSplitter.Location = New System.Drawing.Point(4, 4)
        Me.pnlOHLMainPanelHorizontalSplitter.Margin = New System.Windows.Forms.Padding(4)
        Me.pnlOHLMainPanelHorizontalSplitter.Name = "pnlOHLMainPanelHorizontalSplitter"
        Me.pnlOHLMainPanelHorizontalSplitter.RowCount = 2
        Me.pnlOHLMainPanelHorizontalSplitter.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7.0!))
        Me.pnlOHLMainPanelHorizontalSplitter.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 93.0!))
        Me.pnlOHLMainPanelHorizontalSplitter.Size = New System.Drawing.Size(1355, 685)
        Me.pnlOHLMainPanelHorizontalSplitter.TabIndex = 1
        '
        'pnlOHLTopHeaderVerticalSplitter
        '
        Me.pnlOHLTopHeaderVerticalSplitter.ColumnCount = 15
        Me.pnlOHLTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlOHLTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlOHLTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlOHLTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlOHLTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlOHLTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlOHLTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlOHLTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlOHLTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlOHLTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlOHLTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.62955!))
        Me.pnlOHLTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 0.8166295!))
        Me.pnlOHLTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 1.781737!))
        Me.pnlOHLTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 2.741935!))
        Me.pnlOHLTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 11.29032!))
        Me.pnlOHLTopHeaderVerticalSplitter.Controls.Add(Me.btnMCXStop, 0, 0)
        Me.pnlOHLTopHeaderVerticalSplitter.Controls.Add(Me.btnMCXStart, 0, 0)
        Me.pnlOHLTopHeaderVerticalSplitter.Controls.Add(Me.Panel2, 14, 0)
        Me.pnlOHLTopHeaderVerticalSplitter.Controls.Add(Me.btnMCXSettings, 9, 0)
        Me.pnlOHLTopHeaderVerticalSplitter.Controls.Add(Me.linklblMCXTradableInstruments, 10, 0)
        Me.pnlOHLTopHeaderVerticalSplitter.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlOHLTopHeaderVerticalSplitter.Location = New System.Drawing.Point(4, 4)
        Me.pnlOHLTopHeaderVerticalSplitter.Margin = New System.Windows.Forms.Padding(4)
        Me.pnlOHLTopHeaderVerticalSplitter.Name = "pnlOHLTopHeaderVerticalSplitter"
        Me.pnlOHLTopHeaderVerticalSplitter.RowCount = 1
        Me.pnlOHLTopHeaderVerticalSplitter.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.pnlOHLTopHeaderVerticalSplitter.Size = New System.Drawing.Size(1347, 39)
        Me.pnlOHLTopHeaderVerticalSplitter.TabIndex = 0
        '
        'btnMCXStop
        '
        Me.btnMCXStop.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnMCXStop.Location = New System.Drawing.Point(93, 4)
        Me.btnMCXStop.Margin = New System.Windows.Forms.Padding(4)
        Me.btnMCXStop.Name = "btnMCXStop"
        Me.btnMCXStop.Size = New System.Drawing.Size(81, 31)
        Me.btnMCXStop.TabIndex = 11
        Me.btnMCXStop.Text = "Stop"
        Me.btnMCXStop.UseVisualStyleBackColor = True
        '
        'btnMCXStart
        '
        Me.btnMCXStart.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnMCXStart.Location = New System.Drawing.Point(4, 4)
        Me.btnMCXStart.Margin = New System.Windows.Forms.Padding(4)
        Me.btnMCXStart.Name = "btnMCXStart"
        Me.btnMCXStart.Size = New System.Drawing.Size(81, 31)
        Me.btnMCXStart.TabIndex = 2
        Me.btnMCXStart.Text = "Start"
        Me.btnMCXStart.UseVisualStyleBackColor = True
        '
        'Panel2
        '
        Me.Panel2.Controls.Add(Me.blbMCXTickerStatus)
        Me.Panel2.Controls.Add(Me.lblMCXTickerStatus)
        Me.Panel2.Location = New System.Drawing.Point(1189, 4)
        Me.Panel2.Margin = New System.Windows.Forms.Padding(4)
        Me.Panel2.Name = "Panel2"
        Me.Panel2.Size = New System.Drawing.Size(147, 31)
        Me.Panel2.TabIndex = 9
        '
        'blbMCXTickerStatus
        '
        Me.blbMCXTickerStatus.Color = System.Drawing.Color.Pink
        Me.blbMCXTickerStatus.Dock = System.Windows.Forms.DockStyle.Right
        Me.blbMCXTickerStatus.Location = New System.Drawing.Point(100, 0)
        Me.blbMCXTickerStatus.Margin = New System.Windows.Forms.Padding(4)
        Me.blbMCXTickerStatus.Name = "blbMCXTickerStatus"
        Me.blbMCXTickerStatus.On = True
        Me.blbMCXTickerStatus.Size = New System.Drawing.Size(47, 31)
        Me.blbMCXTickerStatus.TabIndex = 7
        Me.blbMCXTickerStatus.Text = "LedBulb1"
        '
        'lblMCXTickerStatus
        '
        Me.lblMCXTickerStatus.AutoSize = True
        Me.lblMCXTickerStatus.Location = New System.Drawing.Point(11, 9)
        Me.lblMCXTickerStatus.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMCXTickerStatus.Name = "lblMCXTickerStatus"
        Me.lblMCXTickerStatus.Size = New System.Drawing.Size(91, 17)
        Me.lblMCXTickerStatus.TabIndex = 9
        Me.lblMCXTickerStatus.Text = "Ticker Status"
        '
        'btnMCXSettings
        '
        Me.btnMCXSettings.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnMCXSettings.Location = New System.Drawing.Point(805, 4)
        Me.btnMCXSettings.Margin = New System.Windows.Forms.Padding(4)
        Me.btnMCXSettings.Name = "btnMCXSettings"
        Me.btnMCXSettings.Size = New System.Drawing.Size(81, 31)
        Me.btnMCXSettings.TabIndex = 12
        Me.btnMCXSettings.Text = "Settings"
        Me.btnMCXSettings.UseVisualStyleBackColor = True
        '
        'linklblMCXTradableInstruments
        '
        Me.linklblMCXTradableInstruments.AutoSize = True
        Me.linklblMCXTradableInstruments.Dock = System.Windows.Forms.DockStyle.Fill
        Me.linklblMCXTradableInstruments.Enabled = False
        Me.linklblMCXTradableInstruments.Location = New System.Drawing.Point(893, 0)
        Me.linklblMCXTradableInstruments.Name = "linklblMCXTradableInstruments"
        Me.linklblMCXTradableInstruments.Size = New System.Drawing.Size(218, 39)
        Me.linklblMCXTradableInstruments.TabIndex = 13
        Me.linklblMCXTradableInstruments.TabStop = True
        Me.linklblMCXTradableInstruments.Text = "Tradable Instruments: 0"
        Me.linklblMCXTradableInstruments.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'pnlOHLBodyVerticalSplitter
        '
        Me.pnlOHLBodyVerticalSplitter.ColumnCount = 2
        Me.pnlOHLBodyVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70.0!))
        Me.pnlOHLBodyVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30.0!))
        Me.pnlOHLBodyVerticalSplitter.Controls.Add(Me.PictureBox3, 0, 0)
        Me.pnlOHLBodyVerticalSplitter.Controls.Add(Me.pnlOHLBodyHorizontalSplitter, 0, 0)
        Me.pnlOHLBodyVerticalSplitter.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlOHLBodyVerticalSplitter.Location = New System.Drawing.Point(4, 51)
        Me.pnlOHLBodyVerticalSplitter.Margin = New System.Windows.Forms.Padding(4)
        Me.pnlOHLBodyVerticalSplitter.Name = "pnlOHLBodyVerticalSplitter"
        Me.pnlOHLBodyVerticalSplitter.RowCount = 1
        Me.pnlOHLBodyVerticalSplitter.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.pnlOHLBodyVerticalSplitter.Size = New System.Drawing.Size(1347, 630)
        Me.pnlOHLBodyVerticalSplitter.TabIndex = 1
        '
        'PictureBox3
        '
        Me.PictureBox3.Dock = System.Windows.Forms.DockStyle.Fill
        Me.PictureBox3.Image = CType(resources.GetObject("PictureBox3.Image"), System.Drawing.Image)
        Me.PictureBox3.Location = New System.Drawing.Point(945, 2)
        Me.PictureBox3.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.PictureBox3.Name = "PictureBox3"
        Me.PictureBox3.Size = New System.Drawing.Size(399, 626)
        Me.PictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.PictureBox3.TabIndex = 2
        Me.PictureBox3.TabStop = False
        '
        'pnlOHLBodyHorizontalSplitter
        '
        Me.pnlOHLBodyHorizontalSplitter.ColumnCount = 1
        Me.pnlOHLBodyHorizontalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.pnlOHLBodyHorizontalSplitter.Controls.Add(Me.lstMCXLog, 0, 1)
        Me.pnlOHLBodyHorizontalSplitter.Controls.Add(Me.sfdgvMCXMainDashboard, 0, 0)
        Me.pnlOHLBodyHorizontalSplitter.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlOHLBodyHorizontalSplitter.Location = New System.Drawing.Point(4, 4)
        Me.pnlOHLBodyHorizontalSplitter.Margin = New System.Windows.Forms.Padding(4)
        Me.pnlOHLBodyHorizontalSplitter.Name = "pnlOHLBodyHorizontalSplitter"
        Me.pnlOHLBodyHorizontalSplitter.RowCount = 2
        Me.pnlOHLBodyHorizontalSplitter.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70.0!))
        Me.pnlOHLBodyHorizontalSplitter.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30.0!))
        Me.pnlOHLBodyHorizontalSplitter.Size = New System.Drawing.Size(934, 622)
        Me.pnlOHLBodyHorizontalSplitter.TabIndex = 0
        '
        'lstMCXLog
        '
        Me.lstMCXLog.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lstMCXLog.ForeColor = System.Drawing.Color.FromArgb(CType(CType(29, Byte), Integer), CType(CType(29, Byte), Integer), CType(CType(29, Byte), Integer))
        Me.lstMCXLog.FormattingEnabled = True
        Me.lstMCXLog.ItemHeight = 16
        Me.lstMCXLog.Location = New System.Drawing.Point(4, 439)
        Me.lstMCXLog.Margin = New System.Windows.Forms.Padding(4)
        Me.lstMCXLog.Name = "lstMCXLog"
        Me.lstMCXLog.Size = New System.Drawing.Size(926, 179)
        Me.lstMCXLog.TabIndex = 9
        '
        'sfdgvMCXMainDashboard
        '
        Me.sfdgvMCXMainDashboard.AccessibleName = "Table"
        Me.sfdgvMCXMainDashboard.AllowDraggingColumns = True
        Me.sfdgvMCXMainDashboard.AllowEditing = False
        Me.sfdgvMCXMainDashboard.AllowFiltering = True
        Me.sfdgvMCXMainDashboard.AllowResizingColumns = True
        Me.sfdgvMCXMainDashboard.AutoGenerateColumnsMode = Syncfusion.WinForms.DataGrid.Enums.AutoGenerateColumnsMode.SmartReset
        Me.sfdgvMCXMainDashboard.AutoSizeColumnsMode = Syncfusion.WinForms.DataGrid.Enums.AutoSizeColumnsMode.AllCells
        Me.sfdgvMCXMainDashboard.Dock = System.Windows.Forms.DockStyle.Fill
        Me.sfdgvMCXMainDashboard.Location = New System.Drawing.Point(4, 4)
        Me.sfdgvMCXMainDashboard.Margin = New System.Windows.Forms.Padding(4)
        Me.sfdgvMCXMainDashboard.Name = "sfdgvMCXMainDashboard"
        Me.sfdgvMCXMainDashboard.PasteOption = Syncfusion.WinForms.DataGrid.Enums.PasteOptions.None
        Me.sfdgvMCXMainDashboard.Size = New System.Drawing.Size(926, 427)
        Me.sfdgvMCXMainDashboard.TabIndex = 6
        Me.sfdgvMCXMainDashboard.Text = "SfDataGrid1"
        '
        'tabCDS
        '
        Me.tabCDS.Controls.Add(Me.TableLayoutPanel1)
        Me.tabCDS.Location = New System.Drawing.Point(4, 25)
        Me.tabCDS.Name = "tabCDS"
        Me.tabCDS.Size = New System.Drawing.Size(1363, 693)
        Me.tabCDS.TabIndex = 2
        Me.tabCDS.Text = "CDS"
        Me.tabCDS.UseVisualStyleBackColor = True
        '
        'TableLayoutPanel1
        '
        Me.TableLayoutPanel1.ColumnCount = 1
        Me.TableLayoutPanel1.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel1.Controls.Add(Me.TableLayoutPanel2, 0, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.TableLayoutPanel3, 0, 1)
        Me.TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutPanel1.Location = New System.Drawing.Point(0, 0)
        Me.TableLayoutPanel1.Margin = New System.Windows.Forms.Padding(4)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        Me.TableLayoutPanel1.RowCount = 2
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7.0!))
        Me.TableLayoutPanel1.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 93.0!))
        Me.TableLayoutPanel1.Size = New System.Drawing.Size(1363, 693)
        Me.TableLayoutPanel1.TabIndex = 2
        '
        'TableLayoutPanel2
        '
        Me.TableLayoutPanel2.ColumnCount = 15
        Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.62955!))
        Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 0.8166295!))
        Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 1.781737!))
        Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 2.741935!))
        Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 11.29032!))
        Me.TableLayoutPanel2.Controls.Add(Me.btnCDSStop, 0, 0)
        Me.TableLayoutPanel2.Controls.Add(Me.btnCDSStart, 0, 0)
        Me.TableLayoutPanel2.Controls.Add(Me.Panel3, 14, 0)
        Me.TableLayoutPanel2.Controls.Add(Me.btnCDSSettings, 9, 0)
        Me.TableLayoutPanel2.Controls.Add(Me.linklblCDSTradableInstruments, 10, 0)
        Me.TableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutPanel2.Location = New System.Drawing.Point(4, 4)
        Me.TableLayoutPanel2.Margin = New System.Windows.Forms.Padding(4)
        Me.TableLayoutPanel2.Name = "TableLayoutPanel2"
        Me.TableLayoutPanel2.RowCount = 1
        Me.TableLayoutPanel2.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel2.Size = New System.Drawing.Size(1355, 40)
        Me.TableLayoutPanel2.TabIndex = 0
        '
        'btnCDSStop
        '
        Me.btnCDSStop.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnCDSStop.Location = New System.Drawing.Point(94, 4)
        Me.btnCDSStop.Margin = New System.Windows.Forms.Padding(4)
        Me.btnCDSStop.Name = "btnCDSStop"
        Me.btnCDSStop.Size = New System.Drawing.Size(82, 32)
        Me.btnCDSStop.TabIndex = 11
        Me.btnCDSStop.Text = "Stop"
        Me.btnCDSStop.UseVisualStyleBackColor = True
        '
        'btnCDSStart
        '
        Me.btnCDSStart.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnCDSStart.Location = New System.Drawing.Point(4, 4)
        Me.btnCDSStart.Margin = New System.Windows.Forms.Padding(4)
        Me.btnCDSStart.Name = "btnCDSStart"
        Me.btnCDSStart.Size = New System.Drawing.Size(82, 32)
        Me.btnCDSStart.TabIndex = 2
        Me.btnCDSStart.Text = "Start"
        Me.btnCDSStart.UseVisualStyleBackColor = True
        '
        'Panel3
        '
        Me.Panel3.Controls.Add(Me.blbCDSTickerStatus)
        Me.Panel3.Controls.Add(Me.lblCDSTickerStatus)
        Me.Panel3.Location = New System.Drawing.Point(1201, 4)
        Me.Panel3.Margin = New System.Windows.Forms.Padding(4)
        Me.Panel3.Name = "Panel3"
        Me.Panel3.Size = New System.Drawing.Size(147, 31)
        Me.Panel3.TabIndex = 9
        '
        'blbCDSTickerStatus
        '
        Me.blbCDSTickerStatus.Color = System.Drawing.Color.Pink
        Me.blbCDSTickerStatus.Dock = System.Windows.Forms.DockStyle.Right
        Me.blbCDSTickerStatus.Location = New System.Drawing.Point(100, 0)
        Me.blbCDSTickerStatus.Margin = New System.Windows.Forms.Padding(4)
        Me.blbCDSTickerStatus.Name = "blbCDSTickerStatus"
        Me.blbCDSTickerStatus.On = True
        Me.blbCDSTickerStatus.Size = New System.Drawing.Size(47, 31)
        Me.blbCDSTickerStatus.TabIndex = 7
        Me.blbCDSTickerStatus.Text = "LedBulb1"
        '
        'lblCDSTickerStatus
        '
        Me.lblCDSTickerStatus.AutoSize = True
        Me.lblCDSTickerStatus.Location = New System.Drawing.Point(11, 9)
        Me.lblCDSTickerStatus.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblCDSTickerStatus.Name = "lblCDSTickerStatus"
        Me.lblCDSTickerStatus.Size = New System.Drawing.Size(91, 17)
        Me.lblCDSTickerStatus.TabIndex = 9
        Me.lblCDSTickerStatus.Text = "Ticker Status"
        '
        'btnCDSSettings
        '
        Me.btnCDSSettings.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnCDSSettings.Location = New System.Drawing.Point(814, 4)
        Me.btnCDSSettings.Margin = New System.Windows.Forms.Padding(4)
        Me.btnCDSSettings.Name = "btnCDSSettings"
        Me.btnCDSSettings.Size = New System.Drawing.Size(82, 32)
        Me.btnCDSSettings.TabIndex = 12
        Me.btnCDSSettings.Text = "Settings"
        Me.btnCDSSettings.UseVisualStyleBackColor = True
        '
        'linklblCDSTradableInstruments
        '
        Me.linklblCDSTradableInstruments.AutoSize = True
        Me.linklblCDSTradableInstruments.Dock = System.Windows.Forms.DockStyle.Fill
        Me.linklblCDSTradableInstruments.Enabled = False
        Me.linklblCDSTradableInstruments.Location = New System.Drawing.Point(903, 0)
        Me.linklblCDSTradableInstruments.Name = "linklblCDSTradableInstruments"
        Me.linklblCDSTradableInstruments.Size = New System.Drawing.Size(219, 40)
        Me.linklblCDSTradableInstruments.TabIndex = 13
        Me.linklblCDSTradableInstruments.TabStop = True
        Me.linklblCDSTradableInstruments.Text = "Tradable Instruments: 0"
        Me.linklblCDSTradableInstruments.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'TableLayoutPanel3
        '
        Me.TableLayoutPanel3.ColumnCount = 2
        Me.TableLayoutPanel3.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70.0!))
        Me.TableLayoutPanel3.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30.0!))
        Me.TableLayoutPanel3.Controls.Add(Me.PictureBox1, 0, 0)
        Me.TableLayoutPanel3.Controls.Add(Me.TableLayoutPanel4, 0, 0)
        Me.TableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutPanel3.Location = New System.Drawing.Point(4, 52)
        Me.TableLayoutPanel3.Margin = New System.Windows.Forms.Padding(4)
        Me.TableLayoutPanel3.Name = "TableLayoutPanel3"
        Me.TableLayoutPanel3.RowCount = 1
        Me.TableLayoutPanel3.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel3.Size = New System.Drawing.Size(1355, 637)
        Me.TableLayoutPanel3.TabIndex = 1
        '
        'PictureBox1
        '
        Me.PictureBox1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.PictureBox1.Image = CType(resources.GetObject("PictureBox1.Image"), System.Drawing.Image)
        Me.PictureBox1.Location = New System.Drawing.Point(951, 2)
        Me.PictureBox1.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(401, 633)
        Me.PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.PictureBox1.TabIndex = 2
        Me.PictureBox1.TabStop = False
        '
        'TableLayoutPanel4
        '
        Me.TableLayoutPanel4.ColumnCount = 1
        Me.TableLayoutPanel4.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel4.Controls.Add(Me.lstCDSLog, 0, 1)
        Me.TableLayoutPanel4.Controls.Add(Me.sfdgvCDSMainDashboard, 0, 0)
        Me.TableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutPanel4.Location = New System.Drawing.Point(4, 4)
        Me.TableLayoutPanel4.Margin = New System.Windows.Forms.Padding(4)
        Me.TableLayoutPanel4.Name = "TableLayoutPanel4"
        Me.TableLayoutPanel4.RowCount = 2
        Me.TableLayoutPanel4.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70.0!))
        Me.TableLayoutPanel4.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30.0!))
        Me.TableLayoutPanel4.Size = New System.Drawing.Size(940, 629)
        Me.TableLayoutPanel4.TabIndex = 0
        '
        'lstCDSLog
        '
        Me.lstCDSLog.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lstCDSLog.ForeColor = System.Drawing.Color.FromArgb(CType(CType(29, Byte), Integer), CType(CType(29, Byte), Integer), CType(CType(29, Byte), Integer))
        Me.lstCDSLog.FormattingEnabled = True
        Me.lstCDSLog.ItemHeight = 16
        Me.lstCDSLog.Location = New System.Drawing.Point(4, 444)
        Me.lstCDSLog.Margin = New System.Windows.Forms.Padding(4)
        Me.lstCDSLog.Name = "lstCDSLog"
        Me.lstCDSLog.Size = New System.Drawing.Size(932, 181)
        Me.lstCDSLog.TabIndex = 9
        '
        'sfdgvCDSMainDashboard
        '
        Me.sfdgvCDSMainDashboard.AccessibleName = "Table"
        Me.sfdgvCDSMainDashboard.AllowDraggingColumns = True
        Me.sfdgvCDSMainDashboard.AllowEditing = False
        Me.sfdgvCDSMainDashboard.AllowFiltering = True
        Me.sfdgvCDSMainDashboard.AllowResizingColumns = True
        Me.sfdgvCDSMainDashboard.AutoGenerateColumnsMode = Syncfusion.WinForms.DataGrid.Enums.AutoGenerateColumnsMode.SmartReset
        Me.sfdgvCDSMainDashboard.AutoSizeColumnsMode = Syncfusion.WinForms.DataGrid.Enums.AutoSizeColumnsMode.AllCells
        Me.sfdgvCDSMainDashboard.Dock = System.Windows.Forms.DockStyle.Fill
        Me.sfdgvCDSMainDashboard.Location = New System.Drawing.Point(4, 4)
        Me.sfdgvCDSMainDashboard.Margin = New System.Windows.Forms.Padding(4)
        Me.sfdgvCDSMainDashboard.Name = "sfdgvCDSMainDashboard"
        Me.sfdgvCDSMainDashboard.PasteOption = Syncfusion.WinForms.DataGrid.Enums.PasteOptions.None
        Me.sfdgvCDSMainDashboard.Size = New System.Drawing.Size(932, 432)
        Me.sfdgvCDSMainDashboard.TabIndex = 6
        Me.sfdgvCDSMainDashboard.Text = "SfDataGrid1"
        '
        'tmrNFOTickerStatus
        '
        Me.tmrNFOTickerStatus.Enabled = True
        '
        'tmrMCXTickerStatus
        '
        Me.tmrMCXTickerStatus.Enabled = True
        '
        'tmrCDSTickerStatus
        '
        Me.tmrCDSTickerStatus.Enabled = True
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
        Me.tabMCX.ResumeLayout(False)
        Me.pnlOHLMainPanelHorizontalSplitter.ResumeLayout(False)
        Me.pnlOHLTopHeaderVerticalSplitter.ResumeLayout(False)
        Me.pnlOHLTopHeaderVerticalSplitter.PerformLayout()
        Me.Panel2.ResumeLayout(False)
        Me.Panel2.PerformLayout()
        Me.pnlOHLBodyVerticalSplitter.ResumeLayout(False)
        CType(Me.PictureBox3, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlOHLBodyHorizontalSplitter.ResumeLayout(False)
        CType(Me.sfdgvMCXMainDashboard, System.ComponentModel.ISupportInitialize).EndInit()
        Me.tabCDS.ResumeLayout(False)
        Me.TableLayoutPanel1.ResumeLayout(False)
        Me.TableLayoutPanel2.ResumeLayout(False)
        Me.TableLayoutPanel2.PerformLayout()
        Me.Panel3.ResumeLayout(False)
        Me.Panel3.PerformLayout()
        Me.TableLayoutPanel3.ResumeLayout(False)
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TableLayoutPanel4.ResumeLayout(False)
        CType(Me.sfdgvCDSMainDashboard, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents msMainMenuStrip As MenuStrip
    Friend WithEvents miOptions As ToolStripMenuItem
    Friend WithEvents miUserDetails As ToolStripMenuItem
    Friend WithEvents miAbout As ToolStripMenuItem
    Friend WithEvents tabMain As TabControl
    Friend WithEvents tabNFO As TabPage
    Friend WithEvents tabMCX As TabPage
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
    Friend WithEvents pnlOHLMainPanelHorizontalSplitter As TableLayoutPanel
    Friend WithEvents pnlOHLTopHeaderVerticalSplitter As TableLayoutPanel
    Friend WithEvents btnMCXStart As Button
    Friend WithEvents Panel2 As Panel
    Friend WithEvents lblMCXTickerStatus As Label
    Friend WithEvents blbMCXTickerStatus As Bulb.LedBulb
    Friend WithEvents pnlOHLBodyVerticalSplitter As TableLayoutPanel
    Friend WithEvents pnlOHLBodyHorizontalSplitter As TableLayoutPanel
    Friend WithEvents lstMCXLog As ListBox
    Friend WithEvents sfdgvMCXMainDashboard As Syncfusion.WinForms.DataGrid.SfDataGrid
    Friend WithEvents tmrMCXTickerStatus As Timer
    Friend WithEvents btnNFOStop As Button
    Friend WithEvents btnMCXStop As Button
    Friend WithEvents btnNFOSettings As Button
    Friend WithEvents PictureBox3 As PictureBox
    Friend WithEvents miAdvancedOptions As ToolStripMenuItem
    Friend WithEvents linklblNFOTradableInstruments As LinkLabel
    Friend WithEvents btnMCXSettings As Button
    Friend WithEvents linklblMCXTradableInstruments As LinkLabel
    Friend WithEvents PictureBox2 As PictureBox
    Friend WithEvents tabCDS As TabPage
    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents TableLayoutPanel2 As TableLayoutPanel
    Friend WithEvents btnCDSStop As Button
    Friend WithEvents btnCDSStart As Button
    Friend WithEvents Panel3 As Panel
    Friend WithEvents blbCDSTickerStatus As Bulb.LedBulb
    Friend WithEvents lblCDSTickerStatus As Label
    Friend WithEvents btnCDSSettings As Button
    Friend WithEvents linklblCDSTradableInstruments As LinkLabel
    Friend WithEvents TableLayoutPanel3 As TableLayoutPanel
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents TableLayoutPanel4 As TableLayoutPanel
    Friend WithEvents lstCDSLog As ListBox
    Friend WithEvents sfdgvCDSMainDashboard As Syncfusion.WinForms.DataGrid.SfDataGrid
    Friend WithEvents tmrCDSTickerStatus As Timer
End Class
