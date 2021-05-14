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
        Me.tabStrangle = New System.Windows.Forms.TabPage()
        Me.pnlStrangleMainPanelHorizontalSplitter = New System.Windows.Forms.TableLayoutPanel()
        Me.pnlStrangleTopHeaderVerticalSplitter = New System.Windows.Forms.TableLayoutPanel()
        Me.btnStrangleStop = New System.Windows.Forms.Button()
        Me.btnStrangleStart = New System.Windows.Forms.Button()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.blbStrangleTickerStatus = New Bulb.LedBulb()
        Me.lblStrangleTickerStatus = New System.Windows.Forms.Label()
        Me.btnStrangleSettings = New System.Windows.Forms.Button()
        Me.linklblStrangleTradableInstruments = New System.Windows.Forms.LinkLabel()
        Me.TableLayoutPanel3 = New System.Windows.Forms.TableLayoutPanel()
        Me.pctrBxStrangle = New System.Windows.Forms.PictureBox()
        Me.pnlStrangleBodyHorizontalSplitter = New System.Windows.Forms.TableLayoutPanel()
        Me.lstStrangleLog = New System.Windows.Forms.ListBox()
        Me.sfdgvStrangleMainDashboard = New Syncfusion.WinForms.DataGrid.SfDataGrid()
        Me.tabStraddle = New System.Windows.Forms.TabPage()
        Me.pnlStraddleMainPanelHorizontalSplitter = New System.Windows.Forms.TableLayoutPanel()
        Me.pnlStraddleTopHeaderVerticalSplitter = New System.Windows.Forms.TableLayoutPanel()
        Me.btnStraddleStop = New System.Windows.Forms.Button()
        Me.btnStraddleStart = New System.Windows.Forms.Button()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.blbStraddleTickerStatus = New Bulb.LedBulb()
        Me.lblStraddleTickerStatus = New System.Windows.Forms.Label()
        Me.btnStraddleSettings = New System.Windows.Forms.Button()
        Me.linklblStraddleTradableInstruments = New System.Windows.Forms.LinkLabel()
        Me.TableLayoutPanel4 = New System.Windows.Forms.TableLayoutPanel()
        Me.pctrBxStraddle = New System.Windows.Forms.PictureBox()
        Me.pnlStraddleBodyHorizontalSplitter = New System.Windows.Forms.TableLayoutPanel()
        Me.lstStraddleLog = New System.Windows.Forms.ListBox()
        Me.sfdgvStraddleMainDashboard = New Syncfusion.WinForms.DataGrid.SfDataGrid()
        Me.tmrNFOTickerStatus = New System.Windows.Forms.Timer(Me.components)
        Me.tmrStrangleTickerStatus = New System.Windows.Forms.Timer(Me.components)
        Me.tmrStraddleTickerStatus = New System.Windows.Forms.Timer(Me.components)
        Me.tabORB = New System.Windows.Forms.TabPage()
        Me.pnlORBMainPanelHorizontalSplitter = New System.Windows.Forms.TableLayoutPanel()
        Me.TableLayoutPanel2 = New System.Windows.Forms.TableLayoutPanel()
        Me.btnORBStop = New System.Windows.Forms.Button()
        Me.btnORBStart = New System.Windows.Forms.Button()
        Me.Panel3 = New System.Windows.Forms.Panel()
        Me.blbORBTickerStatus = New Bulb.LedBulb()
        Me.lblORBTickerStatus = New System.Windows.Forms.Label()
        Me.btnORBSettings = New System.Windows.Forms.Button()
        Me.linklblORBTradableInstruments = New System.Windows.Forms.LinkLabel()
        Me.TableLayoutPanel5 = New System.Windows.Forms.TableLayoutPanel()
        Me.pctrBxORB = New System.Windows.Forms.PictureBox()
        Me.pnlORBBodyHorizontalSplitter = New System.Windows.Forms.TableLayoutPanel()
        Me.lstORBLog = New System.Windows.Forms.ListBox()
        Me.sfdgvORBMainDashboard = New Syncfusion.WinForms.DataGrid.SfDataGrid()
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
        Me.tabStrangle.SuspendLayout()
        Me.pnlStrangleMainPanelHorizontalSplitter.SuspendLayout()
        Me.pnlStrangleTopHeaderVerticalSplitter.SuspendLayout()
        Me.Panel1.SuspendLayout()
        Me.TableLayoutPanel3.SuspendLayout()
        CType(Me.pctrBxStrangle, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlStrangleBodyHorizontalSplitter.SuspendLayout()
        CType(Me.sfdgvStrangleMainDashboard, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.tabStraddle.SuspendLayout()
        Me.pnlStraddleMainPanelHorizontalSplitter.SuspendLayout()
        Me.pnlStraddleTopHeaderVerticalSplitter.SuspendLayout()
        Me.Panel2.SuspendLayout()
        Me.TableLayoutPanel4.SuspendLayout()
        CType(Me.pctrBxStraddle, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlStraddleBodyHorizontalSplitter.SuspendLayout()
        CType(Me.sfdgvStraddleMainDashboard, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.tabORB.SuspendLayout()
        Me.pnlORBMainPanelHorizontalSplitter.SuspendLayout()
        Me.TableLayoutPanel2.SuspendLayout()
        Me.Panel3.SuspendLayout()
        Me.TableLayoutPanel5.SuspendLayout()
        CType(Me.pctrBxORB, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlORBBodyHorizontalSplitter.SuspendLayout()
        CType(Me.sfdgvORBMainDashboard, System.ComponentModel.ISupportInitialize).BeginInit()
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
        Me.tabMain.Controls.Add(Me.tabStrangle)
        Me.tabMain.Controls.Add(Me.tabStraddle)
        Me.tabMain.Controls.Add(Me.tabORB)
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
        'tabStrangle
        '
        Me.tabStrangle.Controls.Add(Me.pnlStrangleMainPanelHorizontalSplitter)
        Me.tabStrangle.Location = New System.Drawing.Point(4, 25)
        Me.tabStrangle.Name = "tabStrangle"
        Me.tabStrangle.Padding = New System.Windows.Forms.Padding(3)
        Me.tabStrangle.Size = New System.Drawing.Size(1363, 693)
        Me.tabStrangle.TabIndex = 1
        Me.tabStrangle.Text = "Strangle"
        Me.tabStrangle.UseVisualStyleBackColor = True
        '
        'pnlStrangleMainPanelHorizontalSplitter
        '
        Me.pnlStrangleMainPanelHorizontalSplitter.ColumnCount = 1
        Me.pnlStrangleMainPanelHorizontalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.pnlStrangleMainPanelHorizontalSplitter.Controls.Add(Me.pnlStrangleTopHeaderVerticalSplitter, 0, 0)
        Me.pnlStrangleMainPanelHorizontalSplitter.Controls.Add(Me.TableLayoutPanel3, 0, 1)
        Me.pnlStrangleMainPanelHorizontalSplitter.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlStrangleMainPanelHorizontalSplitter.Location = New System.Drawing.Point(3, 3)
        Me.pnlStrangleMainPanelHorizontalSplitter.Margin = New System.Windows.Forms.Padding(4)
        Me.pnlStrangleMainPanelHorizontalSplitter.Name = "pnlStrangleMainPanelHorizontalSplitter"
        Me.pnlStrangleMainPanelHorizontalSplitter.RowCount = 2
        Me.pnlStrangleMainPanelHorizontalSplitter.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7.0!))
        Me.pnlStrangleMainPanelHorizontalSplitter.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 93.0!))
        Me.pnlStrangleMainPanelHorizontalSplitter.Size = New System.Drawing.Size(1357, 687)
        Me.pnlStrangleMainPanelHorizontalSplitter.TabIndex = 1
        '
        'pnlStrangleTopHeaderVerticalSplitter
        '
        Me.pnlStrangleTopHeaderVerticalSplitter.ColumnCount = 15
        Me.pnlStrangleTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlStrangleTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlStrangleTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlStrangleTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlStrangleTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlStrangleTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlStrangleTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlStrangleTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlStrangleTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlStrangleTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlStrangleTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.70379!))
        Me.pnlStrangleTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 0.8166295!))
        Me.pnlStrangleTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 1.707498!))
        Me.pnlStrangleTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 2.741935!))
        Me.pnlStrangleTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 11.29032!))
        Me.pnlStrangleTopHeaderVerticalSplitter.Controls.Add(Me.btnStrangleStop, 0, 0)
        Me.pnlStrangleTopHeaderVerticalSplitter.Controls.Add(Me.btnStrangleStart, 0, 0)
        Me.pnlStrangleTopHeaderVerticalSplitter.Controls.Add(Me.Panel1, 14, 0)
        Me.pnlStrangleTopHeaderVerticalSplitter.Controls.Add(Me.btnStrangleSettings, 9, 0)
        Me.pnlStrangleTopHeaderVerticalSplitter.Controls.Add(Me.linklblStrangleTradableInstruments, 10, 0)
        Me.pnlStrangleTopHeaderVerticalSplitter.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlStrangleTopHeaderVerticalSplitter.Location = New System.Drawing.Point(4, 4)
        Me.pnlStrangleTopHeaderVerticalSplitter.Margin = New System.Windows.Forms.Padding(4)
        Me.pnlStrangleTopHeaderVerticalSplitter.Name = "pnlStrangleTopHeaderVerticalSplitter"
        Me.pnlStrangleTopHeaderVerticalSplitter.RowCount = 1
        Me.pnlStrangleTopHeaderVerticalSplitter.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.pnlStrangleTopHeaderVerticalSplitter.Size = New System.Drawing.Size(1349, 40)
        Me.pnlStrangleTopHeaderVerticalSplitter.TabIndex = 0
        '
        'btnStrangleStop
        '
        Me.btnStrangleStop.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnStrangleStop.Location = New System.Drawing.Point(93, 4)
        Me.btnStrangleStop.Margin = New System.Windows.Forms.Padding(4)
        Me.btnStrangleStop.Name = "btnStrangleStop"
        Me.btnStrangleStop.Size = New System.Drawing.Size(81, 32)
        Me.btnStrangleStop.TabIndex = 10
        Me.btnStrangleStop.Text = "Stop"
        Me.btnStrangleStop.UseVisualStyleBackColor = True
        '
        'btnStrangleStart
        '
        Me.btnStrangleStart.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnStrangleStart.Location = New System.Drawing.Point(4, 4)
        Me.btnStrangleStart.Margin = New System.Windows.Forms.Padding(4)
        Me.btnStrangleStart.Name = "btnStrangleStart"
        Me.btnStrangleStart.Size = New System.Drawing.Size(81, 32)
        Me.btnStrangleStart.TabIndex = 2
        Me.btnStrangleStart.Text = "Start"
        Me.btnStrangleStart.UseVisualStyleBackColor = True
        '
        'Panel1
        '
        Me.Panel1.Controls.Add(Me.blbStrangleTickerStatus)
        Me.Panel1.Controls.Add(Me.lblStrangleTickerStatus)
        Me.Panel1.Location = New System.Drawing.Point(1190, 4)
        Me.Panel1.Margin = New System.Windows.Forms.Padding(4)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(147, 31)
        Me.Panel1.TabIndex = 9
        '
        'blbStrangleTickerStatus
        '
        Me.blbStrangleTickerStatus.Color = System.Drawing.Color.Pink
        Me.blbStrangleTickerStatus.Dock = System.Windows.Forms.DockStyle.Right
        Me.blbStrangleTickerStatus.Location = New System.Drawing.Point(100, 0)
        Me.blbStrangleTickerStatus.Margin = New System.Windows.Forms.Padding(4)
        Me.blbStrangleTickerStatus.Name = "blbStrangleTickerStatus"
        Me.blbStrangleTickerStatus.On = True
        Me.blbStrangleTickerStatus.Size = New System.Drawing.Size(47, 31)
        Me.blbStrangleTickerStatus.TabIndex = 7
        Me.blbStrangleTickerStatus.Text = "LedBulb1"
        '
        'lblStrangleTickerStatus
        '
        Me.lblStrangleTickerStatus.AutoSize = True
        Me.lblStrangleTickerStatus.Location = New System.Drawing.Point(9, 9)
        Me.lblStrangleTickerStatus.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblStrangleTickerStatus.Name = "lblStrangleTickerStatus"
        Me.lblStrangleTickerStatus.Size = New System.Drawing.Size(91, 17)
        Me.lblStrangleTickerStatus.TabIndex = 9
        Me.lblStrangleTickerStatus.Text = "Ticker Status"
        '
        'btnStrangleSettings
        '
        Me.btnStrangleSettings.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnStrangleSettings.Location = New System.Drawing.Point(805, 4)
        Me.btnStrangleSettings.Margin = New System.Windows.Forms.Padding(4)
        Me.btnStrangleSettings.Name = "btnStrangleSettings"
        Me.btnStrangleSettings.Size = New System.Drawing.Size(81, 32)
        Me.btnStrangleSettings.TabIndex = 11
        Me.btnStrangleSettings.Text = "Settings"
        Me.btnStrangleSettings.UseVisualStyleBackColor = True
        '
        'linklblStrangleTradableInstruments
        '
        Me.linklblStrangleTradableInstruments.AutoSize = True
        Me.linklblStrangleTradableInstruments.Dock = System.Windows.Forms.DockStyle.Fill
        Me.linklblStrangleTradableInstruments.Enabled = False
        Me.linklblStrangleTradableInstruments.Location = New System.Drawing.Point(893, 0)
        Me.linklblStrangleTradableInstruments.Name = "linklblStrangleTradableInstruments"
        Me.linklblStrangleTradableInstruments.Size = New System.Drawing.Size(219, 40)
        Me.linklblStrangleTradableInstruments.TabIndex = 12
        Me.linklblStrangleTradableInstruments.TabStop = True
        Me.linklblStrangleTradableInstruments.Text = "Tradable Instruments: 0"
        Me.linklblStrangleTradableInstruments.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'TableLayoutPanel3
        '
        Me.TableLayoutPanel3.ColumnCount = 2
        Me.TableLayoutPanel3.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70.0!))
        Me.TableLayoutPanel3.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30.0!))
        Me.TableLayoutPanel3.Controls.Add(Me.pctrBxStrangle, 0, 0)
        Me.TableLayoutPanel3.Controls.Add(Me.pnlStrangleBodyHorizontalSplitter, 0, 0)
        Me.TableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutPanel3.Location = New System.Drawing.Point(4, 52)
        Me.TableLayoutPanel3.Margin = New System.Windows.Forms.Padding(4)
        Me.TableLayoutPanel3.Name = "TableLayoutPanel3"
        Me.TableLayoutPanel3.RowCount = 1
        Me.TableLayoutPanel3.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel3.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 631.0!))
        Me.TableLayoutPanel3.Size = New System.Drawing.Size(1349, 631)
        Me.TableLayoutPanel3.TabIndex = 1
        '
        'pctrBxStrangle
        '
        Me.pctrBxStrangle.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pctrBxStrangle.Image = CType(resources.GetObject("pctrBxStrangle.Image"), System.Drawing.Image)
        Me.pctrBxStrangle.Location = New System.Drawing.Point(947, 2)
        Me.pctrBxStrangle.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.pctrBxStrangle.Name = "pctrBxStrangle"
        Me.pctrBxStrangle.Size = New System.Drawing.Size(399, 627)
        Me.pctrBxStrangle.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.pctrBxStrangle.TabIndex = 2
        Me.pctrBxStrangle.TabStop = False
        '
        'pnlStrangleBodyHorizontalSplitter
        '
        Me.pnlStrangleBodyHorizontalSplitter.ColumnCount = 1
        Me.pnlStrangleBodyHorizontalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.pnlStrangleBodyHorizontalSplitter.Controls.Add(Me.lstStrangleLog, 0, 1)
        Me.pnlStrangleBodyHorizontalSplitter.Controls.Add(Me.sfdgvStrangleMainDashboard, 0, 0)
        Me.pnlStrangleBodyHorizontalSplitter.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlStrangleBodyHorizontalSplitter.Location = New System.Drawing.Point(4, 4)
        Me.pnlStrangleBodyHorizontalSplitter.Margin = New System.Windows.Forms.Padding(4)
        Me.pnlStrangleBodyHorizontalSplitter.Name = "pnlStrangleBodyHorizontalSplitter"
        Me.pnlStrangleBodyHorizontalSplitter.RowCount = 2
        Me.pnlStrangleBodyHorizontalSplitter.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70.0!))
        Me.pnlStrangleBodyHorizontalSplitter.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30.0!))
        Me.pnlStrangleBodyHorizontalSplitter.Size = New System.Drawing.Size(936, 623)
        Me.pnlStrangleBodyHorizontalSplitter.TabIndex = 0
        '
        'lstStrangleLog
        '
        Me.lstStrangleLog.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lstStrangleLog.ForeColor = System.Drawing.Color.FromArgb(CType(CType(29, Byte), Integer), CType(CType(29, Byte), Integer), CType(CType(29, Byte), Integer))
        Me.lstStrangleLog.FormattingEnabled = True
        Me.lstStrangleLog.HorizontalScrollbar = True
        Me.lstStrangleLog.ItemHeight = 16
        Me.lstStrangleLog.Location = New System.Drawing.Point(4, 440)
        Me.lstStrangleLog.Margin = New System.Windows.Forms.Padding(4)
        Me.lstStrangleLog.Name = "lstStrangleLog"
        Me.lstStrangleLog.Size = New System.Drawing.Size(928, 179)
        Me.lstStrangleLog.TabIndex = 9
        '
        'sfdgvStrangleMainDashboard
        '
        Me.sfdgvStrangleMainDashboard.AccessibleName = "Table"
        Me.sfdgvStrangleMainDashboard.AllowDraggingColumns = True
        Me.sfdgvStrangleMainDashboard.AllowEditing = False
        Me.sfdgvStrangleMainDashboard.AllowFiltering = True
        Me.sfdgvStrangleMainDashboard.AllowResizingColumns = True
        Me.sfdgvStrangleMainDashboard.AutoGenerateColumnsMode = Syncfusion.WinForms.DataGrid.Enums.AutoGenerateColumnsMode.SmartReset
        Me.sfdgvStrangleMainDashboard.AutoSizeColumnsMode = Syncfusion.WinForms.DataGrid.Enums.AutoSizeColumnsMode.AllCells
        Me.sfdgvStrangleMainDashboard.Dock = System.Windows.Forms.DockStyle.Fill
        Me.sfdgvStrangleMainDashboard.Location = New System.Drawing.Point(4, 4)
        Me.sfdgvStrangleMainDashboard.Margin = New System.Windows.Forms.Padding(4)
        Me.sfdgvStrangleMainDashboard.Name = "sfdgvStrangleMainDashboard"
        Me.sfdgvStrangleMainDashboard.PasteOption = Syncfusion.WinForms.DataGrid.Enums.PasteOptions.None
        Me.sfdgvStrangleMainDashboard.Size = New System.Drawing.Size(928, 428)
        Me.sfdgvStrangleMainDashboard.TabIndex = 6
        Me.sfdgvStrangleMainDashboard.Text = "SfDataGrid1"
        '
        'tabStraddle
        '
        Me.tabStraddle.Controls.Add(Me.pnlStraddleMainPanelHorizontalSplitter)
        Me.tabStraddle.Location = New System.Drawing.Point(4, 25)
        Me.tabStraddle.Name = "tabStraddle"
        Me.tabStraddle.Size = New System.Drawing.Size(1363, 693)
        Me.tabStraddle.TabIndex = 2
        Me.tabStraddle.Text = "Straddle"
        Me.tabStraddle.UseVisualStyleBackColor = True
        '
        'pnlStraddleMainPanelHorizontalSplitter
        '
        Me.pnlStraddleMainPanelHorizontalSplitter.ColumnCount = 1
        Me.pnlStraddleMainPanelHorizontalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.pnlStraddleMainPanelHorizontalSplitter.Controls.Add(Me.pnlStraddleTopHeaderVerticalSplitter, 0, 0)
        Me.pnlStraddleMainPanelHorizontalSplitter.Controls.Add(Me.TableLayoutPanel4, 0, 1)
        Me.pnlStraddleMainPanelHorizontalSplitter.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlStraddleMainPanelHorizontalSplitter.Location = New System.Drawing.Point(0, 0)
        Me.pnlStraddleMainPanelHorizontalSplitter.Margin = New System.Windows.Forms.Padding(4)
        Me.pnlStraddleMainPanelHorizontalSplitter.Name = "pnlStraddleMainPanelHorizontalSplitter"
        Me.pnlStraddleMainPanelHorizontalSplitter.RowCount = 2
        Me.pnlStraddleMainPanelHorizontalSplitter.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7.0!))
        Me.pnlStraddleMainPanelHorizontalSplitter.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 93.0!))
        Me.pnlStraddleMainPanelHorizontalSplitter.Size = New System.Drawing.Size(1363, 693)
        Me.pnlStraddleMainPanelHorizontalSplitter.TabIndex = 2
        '
        'pnlStraddleTopHeaderVerticalSplitter
        '
        Me.pnlStraddleTopHeaderVerticalSplitter.ColumnCount = 15
        Me.pnlStraddleTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlStraddleTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlStraddleTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlStraddleTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlStraddleTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlStraddleTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlStraddleTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlStraddleTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlStraddleTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlStraddleTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 6.666668!))
        Me.pnlStraddleTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.70379!))
        Me.pnlStraddleTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 0.8166295!))
        Me.pnlStraddleTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 1.707498!))
        Me.pnlStraddleTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 2.741935!))
        Me.pnlStraddleTopHeaderVerticalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 11.29032!))
        Me.pnlStraddleTopHeaderVerticalSplitter.Controls.Add(Me.btnStraddleStop, 0, 0)
        Me.pnlStraddleTopHeaderVerticalSplitter.Controls.Add(Me.btnStraddleStart, 0, 0)
        Me.pnlStraddleTopHeaderVerticalSplitter.Controls.Add(Me.Panel2, 14, 0)
        Me.pnlStraddleTopHeaderVerticalSplitter.Controls.Add(Me.btnStraddleSettings, 9, 0)
        Me.pnlStraddleTopHeaderVerticalSplitter.Controls.Add(Me.linklblStraddleTradableInstruments, 10, 0)
        Me.pnlStraddleTopHeaderVerticalSplitter.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlStraddleTopHeaderVerticalSplitter.Location = New System.Drawing.Point(4, 4)
        Me.pnlStraddleTopHeaderVerticalSplitter.Margin = New System.Windows.Forms.Padding(4)
        Me.pnlStraddleTopHeaderVerticalSplitter.Name = "pnlStraddleTopHeaderVerticalSplitter"
        Me.pnlStraddleTopHeaderVerticalSplitter.RowCount = 1
        Me.pnlStraddleTopHeaderVerticalSplitter.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.pnlStraddleTopHeaderVerticalSplitter.Size = New System.Drawing.Size(1355, 40)
        Me.pnlStraddleTopHeaderVerticalSplitter.TabIndex = 0
        '
        'btnStraddleStop
        '
        Me.btnStraddleStop.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnStraddleStop.Location = New System.Drawing.Point(94, 4)
        Me.btnStraddleStop.Margin = New System.Windows.Forms.Padding(4)
        Me.btnStraddleStop.Name = "btnStraddleStop"
        Me.btnStraddleStop.Size = New System.Drawing.Size(82, 32)
        Me.btnStraddleStop.TabIndex = 10
        Me.btnStraddleStop.Text = "Stop"
        Me.btnStraddleStop.UseVisualStyleBackColor = True
        '
        'btnStraddleStart
        '
        Me.btnStraddleStart.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnStraddleStart.Location = New System.Drawing.Point(4, 4)
        Me.btnStraddleStart.Margin = New System.Windows.Forms.Padding(4)
        Me.btnStraddleStart.Name = "btnStraddleStart"
        Me.btnStraddleStart.Size = New System.Drawing.Size(82, 32)
        Me.btnStraddleStart.TabIndex = 2
        Me.btnStraddleStart.Text = "Start"
        Me.btnStraddleStart.UseVisualStyleBackColor = True
        '
        'Panel2
        '
        Me.Panel2.Controls.Add(Me.blbStraddleTickerStatus)
        Me.Panel2.Controls.Add(Me.lblStraddleTickerStatus)
        Me.Panel2.Location = New System.Drawing.Point(1201, 4)
        Me.Panel2.Margin = New System.Windows.Forms.Padding(4)
        Me.Panel2.Name = "Panel2"
        Me.Panel2.Size = New System.Drawing.Size(147, 31)
        Me.Panel2.TabIndex = 9
        '
        'blbStraddleTickerStatus
        '
        Me.blbStraddleTickerStatus.Color = System.Drawing.Color.Pink
        Me.blbStraddleTickerStatus.Dock = System.Windows.Forms.DockStyle.Right
        Me.blbStraddleTickerStatus.Location = New System.Drawing.Point(100, 0)
        Me.blbStraddleTickerStatus.Margin = New System.Windows.Forms.Padding(4)
        Me.blbStraddleTickerStatus.Name = "blbStraddleTickerStatus"
        Me.blbStraddleTickerStatus.On = True
        Me.blbStraddleTickerStatus.Size = New System.Drawing.Size(47, 31)
        Me.blbStraddleTickerStatus.TabIndex = 7
        Me.blbStraddleTickerStatus.Text = "LedBulb1"
        '
        'lblStraddleTickerStatus
        '
        Me.lblStraddleTickerStatus.AutoSize = True
        Me.lblStraddleTickerStatus.Location = New System.Drawing.Point(9, 9)
        Me.lblStraddleTickerStatus.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblStraddleTickerStatus.Name = "lblStraddleTickerStatus"
        Me.lblStraddleTickerStatus.Size = New System.Drawing.Size(91, 17)
        Me.lblStraddleTickerStatus.TabIndex = 9
        Me.lblStraddleTickerStatus.Text = "Ticker Status"
        '
        'btnStraddleSettings
        '
        Me.btnStraddleSettings.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnStraddleSettings.Location = New System.Drawing.Point(814, 4)
        Me.btnStraddleSettings.Margin = New System.Windows.Forms.Padding(4)
        Me.btnStraddleSettings.Name = "btnStraddleSettings"
        Me.btnStraddleSettings.Size = New System.Drawing.Size(82, 32)
        Me.btnStraddleSettings.TabIndex = 11
        Me.btnStraddleSettings.Text = "Settings"
        Me.btnStraddleSettings.UseVisualStyleBackColor = True
        '
        'linklblStraddleTradableInstruments
        '
        Me.linklblStraddleTradableInstruments.AutoSize = True
        Me.linklblStraddleTradableInstruments.Dock = System.Windows.Forms.DockStyle.Fill
        Me.linklblStraddleTradableInstruments.Enabled = False
        Me.linklblStraddleTradableInstruments.Location = New System.Drawing.Point(903, 0)
        Me.linklblStraddleTradableInstruments.Name = "linklblStraddleTradableInstruments"
        Me.linklblStraddleTradableInstruments.Size = New System.Drawing.Size(220, 40)
        Me.linklblStraddleTradableInstruments.TabIndex = 12
        Me.linklblStraddleTradableInstruments.TabStop = True
        Me.linklblStraddleTradableInstruments.Text = "Tradable Instruments: 0"
        Me.linklblStraddleTradableInstruments.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'TableLayoutPanel4
        '
        Me.TableLayoutPanel4.ColumnCount = 2
        Me.TableLayoutPanel4.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70.0!))
        Me.TableLayoutPanel4.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30.0!))
        Me.TableLayoutPanel4.Controls.Add(Me.pctrBxStraddle, 0, 0)
        Me.TableLayoutPanel4.Controls.Add(Me.pnlStraddleBodyHorizontalSplitter, 0, 0)
        Me.TableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutPanel4.Location = New System.Drawing.Point(4, 52)
        Me.TableLayoutPanel4.Margin = New System.Windows.Forms.Padding(4)
        Me.TableLayoutPanel4.Name = "TableLayoutPanel4"
        Me.TableLayoutPanel4.RowCount = 1
        Me.TableLayoutPanel4.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel4.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 630.0!))
        Me.TableLayoutPanel4.Size = New System.Drawing.Size(1355, 637)
        Me.TableLayoutPanel4.TabIndex = 1
        '
        'pctrBxStraddle
        '
        Me.pctrBxStraddle.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pctrBxStraddle.Image = CType(resources.GetObject("pctrBxStraddle.Image"), System.Drawing.Image)
        Me.pctrBxStraddle.Location = New System.Drawing.Point(951, 2)
        Me.pctrBxStraddle.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.pctrBxStraddle.Name = "pctrBxStraddle"
        Me.pctrBxStraddle.Size = New System.Drawing.Size(401, 633)
        Me.pctrBxStraddle.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.pctrBxStraddle.TabIndex = 2
        Me.pctrBxStraddle.TabStop = False
        '
        'pnlStraddleBodyHorizontalSplitter
        '
        Me.pnlStraddleBodyHorizontalSplitter.ColumnCount = 1
        Me.pnlStraddleBodyHorizontalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.pnlStraddleBodyHorizontalSplitter.Controls.Add(Me.lstStraddleLog, 0, 1)
        Me.pnlStraddleBodyHorizontalSplitter.Controls.Add(Me.sfdgvStraddleMainDashboard, 0, 0)
        Me.pnlStraddleBodyHorizontalSplitter.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlStraddleBodyHorizontalSplitter.Location = New System.Drawing.Point(4, 4)
        Me.pnlStraddleBodyHorizontalSplitter.Margin = New System.Windows.Forms.Padding(4)
        Me.pnlStraddleBodyHorizontalSplitter.Name = "pnlStraddleBodyHorizontalSplitter"
        Me.pnlStraddleBodyHorizontalSplitter.RowCount = 2
        Me.pnlStraddleBodyHorizontalSplitter.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70.0!))
        Me.pnlStraddleBodyHorizontalSplitter.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30.0!))
        Me.pnlStraddleBodyHorizontalSplitter.Size = New System.Drawing.Size(940, 629)
        Me.pnlStraddleBodyHorizontalSplitter.TabIndex = 0
        '
        'lstStraddleLog
        '
        Me.lstStraddleLog.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lstStraddleLog.ForeColor = System.Drawing.Color.FromArgb(CType(CType(29, Byte), Integer), CType(CType(29, Byte), Integer), CType(CType(29, Byte), Integer))
        Me.lstStraddleLog.FormattingEnabled = True
        Me.lstStraddleLog.HorizontalScrollbar = True
        Me.lstStraddleLog.ItemHeight = 16
        Me.lstStraddleLog.Location = New System.Drawing.Point(4, 444)
        Me.lstStraddleLog.Margin = New System.Windows.Forms.Padding(4)
        Me.lstStraddleLog.Name = "lstStraddleLog"
        Me.lstStraddleLog.Size = New System.Drawing.Size(932, 181)
        Me.lstStraddleLog.TabIndex = 9
        '
        'sfdgvStraddleMainDashboard
        '
        Me.sfdgvStraddleMainDashboard.AccessibleName = "Table"
        Me.sfdgvStraddleMainDashboard.AllowDraggingColumns = True
        Me.sfdgvStraddleMainDashboard.AllowEditing = False
        Me.sfdgvStraddleMainDashboard.AllowFiltering = True
        Me.sfdgvStraddleMainDashboard.AllowResizingColumns = True
        Me.sfdgvStraddleMainDashboard.AutoGenerateColumnsMode = Syncfusion.WinForms.DataGrid.Enums.AutoGenerateColumnsMode.SmartReset
        Me.sfdgvStraddleMainDashboard.AutoSizeColumnsMode = Syncfusion.WinForms.DataGrid.Enums.AutoSizeColumnsMode.AllCells
        Me.sfdgvStraddleMainDashboard.Dock = System.Windows.Forms.DockStyle.Fill
        Me.sfdgvStraddleMainDashboard.Location = New System.Drawing.Point(4, 4)
        Me.sfdgvStraddleMainDashboard.Margin = New System.Windows.Forms.Padding(4)
        Me.sfdgvStraddleMainDashboard.Name = "sfdgvStraddleMainDashboard"
        Me.sfdgvStraddleMainDashboard.PasteOption = Syncfusion.WinForms.DataGrid.Enums.PasteOptions.None
        Me.sfdgvStraddleMainDashboard.Size = New System.Drawing.Size(932, 432)
        Me.sfdgvStraddleMainDashboard.TabIndex = 6
        Me.sfdgvStraddleMainDashboard.Text = "SfDataGrid1"
        '
        'tmrNFOTickerStatus
        '
        Me.tmrNFOTickerStatus.Enabled = True
        '
        'tmrStrangleTickerStatus
        '
        Me.tmrStrangleTickerStatus.Enabled = True
        '
        'tmrStraddleTickerStatus
        '
        Me.tmrStraddleTickerStatus.Enabled = True
        '
        'tabORB
        '
        Me.tabORB.Controls.Add(Me.pnlORBMainPanelHorizontalSplitter)
        Me.tabORB.Location = New System.Drawing.Point(4, 25)
        Me.tabORB.Name = "tabORB"
        Me.tabORB.Size = New System.Drawing.Size(1363, 693)
        Me.tabORB.TabIndex = 3
        Me.tabORB.Text = "ORB"
        Me.tabORB.UseVisualStyleBackColor = True
        '
        'pnlORBMainPanelHorizontalSplitter
        '
        Me.pnlORBMainPanelHorizontalSplitter.ColumnCount = 1
        Me.pnlORBMainPanelHorizontalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.pnlORBMainPanelHorizontalSplitter.Controls.Add(Me.TableLayoutPanel2, 0, 0)
        Me.pnlORBMainPanelHorizontalSplitter.Controls.Add(Me.TableLayoutPanel5, 0, 1)
        Me.pnlORBMainPanelHorizontalSplitter.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlORBMainPanelHorizontalSplitter.Location = New System.Drawing.Point(0, 0)
        Me.pnlORBMainPanelHorizontalSplitter.Margin = New System.Windows.Forms.Padding(4)
        Me.pnlORBMainPanelHorizontalSplitter.Name = "pnlORBMainPanelHorizontalSplitter"
        Me.pnlORBMainPanelHorizontalSplitter.RowCount = 2
        Me.pnlORBMainPanelHorizontalSplitter.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7.0!))
        Me.pnlORBMainPanelHorizontalSplitter.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 93.0!))
        Me.pnlORBMainPanelHorizontalSplitter.Size = New System.Drawing.Size(1363, 693)
        Me.pnlORBMainPanelHorizontalSplitter.TabIndex = 3
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
        Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.70379!))
        Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 0.8166295!))
        Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 1.707498!))
        Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 2.741935!))
        Me.TableLayoutPanel2.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 11.29032!))
        Me.TableLayoutPanel2.Controls.Add(Me.btnORBStop, 0, 0)
        Me.TableLayoutPanel2.Controls.Add(Me.btnORBStart, 0, 0)
        Me.TableLayoutPanel2.Controls.Add(Me.Panel3, 14, 0)
        Me.TableLayoutPanel2.Controls.Add(Me.btnORBSettings, 9, 0)
        Me.TableLayoutPanel2.Controls.Add(Me.linklblORBTradableInstruments, 10, 0)
        Me.TableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutPanel2.Location = New System.Drawing.Point(4, 4)
        Me.TableLayoutPanel2.Margin = New System.Windows.Forms.Padding(4)
        Me.TableLayoutPanel2.Name = "TableLayoutPanel2"
        Me.TableLayoutPanel2.RowCount = 1
        Me.TableLayoutPanel2.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel2.Size = New System.Drawing.Size(1355, 40)
        Me.TableLayoutPanel2.TabIndex = 0
        '
        'btnORBStop
        '
        Me.btnORBStop.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnORBStop.Location = New System.Drawing.Point(94, 4)
        Me.btnORBStop.Margin = New System.Windows.Forms.Padding(4)
        Me.btnORBStop.Name = "btnORBStop"
        Me.btnORBStop.Size = New System.Drawing.Size(82, 32)
        Me.btnORBStop.TabIndex = 10
        Me.btnORBStop.Text = "Stop"
        Me.btnORBStop.UseVisualStyleBackColor = True
        '
        'btnORBStart
        '
        Me.btnORBStart.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnORBStart.Location = New System.Drawing.Point(4, 4)
        Me.btnORBStart.Margin = New System.Windows.Forms.Padding(4)
        Me.btnORBStart.Name = "btnORBStart"
        Me.btnORBStart.Size = New System.Drawing.Size(82, 32)
        Me.btnORBStart.TabIndex = 2
        Me.btnORBStart.Text = "Start"
        Me.btnORBStart.UseVisualStyleBackColor = True
        '
        'Panel3
        '
        Me.Panel3.Controls.Add(Me.blbORBTickerStatus)
        Me.Panel3.Controls.Add(Me.lblORBTickerStatus)
        Me.Panel3.Location = New System.Drawing.Point(1201, 4)
        Me.Panel3.Margin = New System.Windows.Forms.Padding(4)
        Me.Panel3.Name = "Panel3"
        Me.Panel3.Size = New System.Drawing.Size(147, 31)
        Me.Panel3.TabIndex = 9
        '
        'blbORBTickerStatus
        '
        Me.blbORBTickerStatus.Color = System.Drawing.Color.Pink
        Me.blbORBTickerStatus.Dock = System.Windows.Forms.DockStyle.Right
        Me.blbORBTickerStatus.Location = New System.Drawing.Point(100, 0)
        Me.blbORBTickerStatus.Margin = New System.Windows.Forms.Padding(4)
        Me.blbORBTickerStatus.Name = "blbORBTickerStatus"
        Me.blbORBTickerStatus.On = True
        Me.blbORBTickerStatus.Size = New System.Drawing.Size(47, 31)
        Me.blbORBTickerStatus.TabIndex = 7
        Me.blbORBTickerStatus.Text = "LedBulb1"
        '
        'lblORBTickerStatus
        '
        Me.lblORBTickerStatus.AutoSize = True
        Me.lblORBTickerStatus.Location = New System.Drawing.Point(9, 9)
        Me.lblORBTickerStatus.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblORBTickerStatus.Name = "lblORBTickerStatus"
        Me.lblORBTickerStatus.Size = New System.Drawing.Size(91, 17)
        Me.lblORBTickerStatus.TabIndex = 9
        Me.lblORBTickerStatus.Text = "Ticker Status"
        '
        'btnORBSettings
        '
        Me.btnORBSettings.Dock = System.Windows.Forms.DockStyle.Fill
        Me.btnORBSettings.Location = New System.Drawing.Point(814, 4)
        Me.btnORBSettings.Margin = New System.Windows.Forms.Padding(4)
        Me.btnORBSettings.Name = "btnORBSettings"
        Me.btnORBSettings.Size = New System.Drawing.Size(82, 32)
        Me.btnORBSettings.TabIndex = 11
        Me.btnORBSettings.Text = "Settings"
        Me.btnORBSettings.UseVisualStyleBackColor = True
        '
        'linklblORBTradableInstruments
        '
        Me.linklblORBTradableInstruments.AutoSize = True
        Me.linklblORBTradableInstruments.Dock = System.Windows.Forms.DockStyle.Fill
        Me.linklblORBTradableInstruments.Enabled = False
        Me.linklblORBTradableInstruments.Location = New System.Drawing.Point(903, 0)
        Me.linklblORBTradableInstruments.Name = "linklblORBTradableInstruments"
        Me.linklblORBTradableInstruments.Size = New System.Drawing.Size(220, 40)
        Me.linklblORBTradableInstruments.TabIndex = 12
        Me.linklblORBTradableInstruments.TabStop = True
        Me.linklblORBTradableInstruments.Text = "Tradable Instruments: 0"
        Me.linklblORBTradableInstruments.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'TableLayoutPanel5
        '
        Me.TableLayoutPanel5.ColumnCount = 2
        Me.TableLayoutPanel5.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70.0!))
        Me.TableLayoutPanel5.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30.0!))
        Me.TableLayoutPanel5.Controls.Add(Me.pctrBxORB, 0, 0)
        Me.TableLayoutPanel5.Controls.Add(Me.pnlORBBodyHorizontalSplitter, 0, 0)
        Me.TableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutPanel5.Location = New System.Drawing.Point(4, 52)
        Me.TableLayoutPanel5.Margin = New System.Windows.Forms.Padding(4)
        Me.TableLayoutPanel5.Name = "TableLayoutPanel5"
        Me.TableLayoutPanel5.RowCount = 1
        Me.TableLayoutPanel5.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanel5.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 630.0!))
        Me.TableLayoutPanel5.Size = New System.Drawing.Size(1355, 637)
        Me.TableLayoutPanel5.TabIndex = 1
        '
        'pctrBxORB
        '
        Me.pctrBxORB.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pctrBxORB.Image = CType(resources.GetObject("pctrBxORB.Image"), System.Drawing.Image)
        Me.pctrBxORB.Location = New System.Drawing.Point(951, 2)
        Me.pctrBxORB.Margin = New System.Windows.Forms.Padding(3, 2, 3, 2)
        Me.pctrBxORB.Name = "pctrBxORB"
        Me.pctrBxORB.Size = New System.Drawing.Size(401, 633)
        Me.pctrBxORB.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.pctrBxORB.TabIndex = 2
        Me.pctrBxORB.TabStop = False
        '
        'pnlORBBodyHorizontalSplitter
        '
        Me.pnlORBBodyHorizontalSplitter.ColumnCount = 1
        Me.pnlORBBodyHorizontalSplitter.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.pnlORBBodyHorizontalSplitter.Controls.Add(Me.lstORBLog, 0, 1)
        Me.pnlORBBodyHorizontalSplitter.Controls.Add(Me.sfdgvORBMainDashboard, 0, 0)
        Me.pnlORBBodyHorizontalSplitter.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlORBBodyHorizontalSplitter.Location = New System.Drawing.Point(4, 4)
        Me.pnlORBBodyHorizontalSplitter.Margin = New System.Windows.Forms.Padding(4)
        Me.pnlORBBodyHorizontalSplitter.Name = "pnlORBBodyHorizontalSplitter"
        Me.pnlORBBodyHorizontalSplitter.RowCount = 2
        Me.pnlORBBodyHorizontalSplitter.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70.0!))
        Me.pnlORBBodyHorizontalSplitter.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30.0!))
        Me.pnlORBBodyHorizontalSplitter.Size = New System.Drawing.Size(940, 629)
        Me.pnlORBBodyHorizontalSplitter.TabIndex = 0
        '
        'lstORBLog
        '
        Me.lstORBLog.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lstORBLog.ForeColor = System.Drawing.Color.FromArgb(CType(CType(29, Byte), Integer), CType(CType(29, Byte), Integer), CType(CType(29, Byte), Integer))
        Me.lstORBLog.FormattingEnabled = True
        Me.lstORBLog.HorizontalScrollbar = True
        Me.lstORBLog.ItemHeight = 16
        Me.lstORBLog.Location = New System.Drawing.Point(4, 444)
        Me.lstORBLog.Margin = New System.Windows.Forms.Padding(4)
        Me.lstORBLog.Name = "lstORBLog"
        Me.lstORBLog.Size = New System.Drawing.Size(932, 181)
        Me.lstORBLog.TabIndex = 9
        '
        'sfdgvORBMainDashboard
        '
        Me.sfdgvORBMainDashboard.AccessibleName = "Table"
        Me.sfdgvORBMainDashboard.AllowDraggingColumns = True
        Me.sfdgvORBMainDashboard.AllowEditing = False
        Me.sfdgvORBMainDashboard.AllowFiltering = True
        Me.sfdgvORBMainDashboard.AllowResizingColumns = True
        Me.sfdgvORBMainDashboard.AutoGenerateColumnsMode = Syncfusion.WinForms.DataGrid.Enums.AutoGenerateColumnsMode.SmartReset
        Me.sfdgvORBMainDashboard.AutoSizeColumnsMode = Syncfusion.WinForms.DataGrid.Enums.AutoSizeColumnsMode.AllCells
        Me.sfdgvORBMainDashboard.Dock = System.Windows.Forms.DockStyle.Fill
        Me.sfdgvORBMainDashboard.Location = New System.Drawing.Point(4, 4)
        Me.sfdgvORBMainDashboard.Margin = New System.Windows.Forms.Padding(4)
        Me.sfdgvORBMainDashboard.Name = "sfdgvORBMainDashboard"
        Me.sfdgvORBMainDashboard.PasteOption = Syncfusion.WinForms.DataGrid.Enums.PasteOptions.None
        Me.sfdgvORBMainDashboard.Size = New System.Drawing.Size(932, 432)
        Me.sfdgvORBMainDashboard.TabIndex = 6
        Me.sfdgvORBMainDashboard.Text = "SfDataGrid1"
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
        Me.tabStrangle.ResumeLayout(False)
        Me.pnlStrangleMainPanelHorizontalSplitter.ResumeLayout(False)
        Me.pnlStrangleTopHeaderVerticalSplitter.ResumeLayout(False)
        Me.pnlStrangleTopHeaderVerticalSplitter.PerformLayout()
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        Me.TableLayoutPanel3.ResumeLayout(False)
        CType(Me.pctrBxStrangle, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlStrangleBodyHorizontalSplitter.ResumeLayout(False)
        CType(Me.sfdgvStrangleMainDashboard, System.ComponentModel.ISupportInitialize).EndInit()
        Me.tabStraddle.ResumeLayout(False)
        Me.pnlStraddleMainPanelHorizontalSplitter.ResumeLayout(False)
        Me.pnlStraddleTopHeaderVerticalSplitter.ResumeLayout(False)
        Me.pnlStraddleTopHeaderVerticalSplitter.PerformLayout()
        Me.Panel2.ResumeLayout(False)
        Me.Panel2.PerformLayout()
        Me.TableLayoutPanel4.ResumeLayout(False)
        CType(Me.pctrBxStraddle, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlStraddleBodyHorizontalSplitter.ResumeLayout(False)
        CType(Me.sfdgvStraddleMainDashboard, System.ComponentModel.ISupportInitialize).EndInit()
        Me.tabORB.ResumeLayout(False)
        Me.pnlORBMainPanelHorizontalSplitter.ResumeLayout(False)
        Me.TableLayoutPanel2.ResumeLayout(False)
        Me.TableLayoutPanel2.PerformLayout()
        Me.Panel3.ResumeLayout(False)
        Me.Panel3.PerformLayout()
        Me.TableLayoutPanel5.ResumeLayout(False)
        CType(Me.pctrBxORB, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlORBBodyHorizontalSplitter.ResumeLayout(False)
        CType(Me.sfdgvORBMainDashboard, System.ComponentModel.ISupportInitialize).EndInit()
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
    Friend WithEvents tabStrangle As TabPage
    Friend WithEvents pnlStrangleMainPanelHorizontalSplitter As TableLayoutPanel
    Friend WithEvents pnlStrangleTopHeaderVerticalSplitter As TableLayoutPanel
    Friend WithEvents btnStrangleStop As Button
    Friend WithEvents btnStrangleStart As Button
    Friend WithEvents Panel1 As Panel
    Friend WithEvents blbStrangleTickerStatus As Bulb.LedBulb
    Friend WithEvents lblStrangleTickerStatus As Label
    Friend WithEvents btnStrangleSettings As Button
    Friend WithEvents linklblStrangleTradableInstruments As LinkLabel
    Friend WithEvents TableLayoutPanel3 As TableLayoutPanel
    Friend WithEvents pctrBxStrangle As PictureBox
    Friend WithEvents pnlStrangleBodyHorizontalSplitter As TableLayoutPanel
    Friend WithEvents lstStrangleLog As ListBox
    Friend WithEvents sfdgvStrangleMainDashboard As Syncfusion.WinForms.DataGrid.SfDataGrid
    Friend WithEvents tmrStrangleTickerStatus As Timer
    Friend WithEvents tabStraddle As TabPage
    Friend WithEvents pnlStraddleMainPanelHorizontalSplitter As TableLayoutPanel
    Friend WithEvents pnlStraddleTopHeaderVerticalSplitter As TableLayoutPanel
    Friend WithEvents btnStraddleStop As Button
    Friend WithEvents btnStraddleStart As Button
    Friend WithEvents Panel2 As Panel
    Friend WithEvents blbStraddleTickerStatus As Bulb.LedBulb
    Friend WithEvents lblStraddleTickerStatus As Label
    Friend WithEvents btnStraddleSettings As Button
    Friend WithEvents linklblStraddleTradableInstruments As LinkLabel
    Friend WithEvents TableLayoutPanel4 As TableLayoutPanel
    Friend WithEvents pctrBxStraddle As PictureBox
    Friend WithEvents pnlStraddleBodyHorizontalSplitter As TableLayoutPanel
    Friend WithEvents lstStraddleLog As ListBox
    Friend WithEvents sfdgvStraddleMainDashboard As Syncfusion.WinForms.DataGrid.SfDataGrid
    Friend WithEvents tmrStraddleTickerStatus As Timer
    Friend WithEvents tabORB As TabPage
    Friend WithEvents pnlORBMainPanelHorizontalSplitter As TableLayoutPanel
    Friend WithEvents TableLayoutPanel2 As TableLayoutPanel
    Friend WithEvents btnORBStop As Button
    Friend WithEvents btnORBStart As Button
    Friend WithEvents Panel3 As Panel
    Friend WithEvents blbORBTickerStatus As Bulb.LedBulb
    Friend WithEvents lblORBTickerStatus As Label
    Friend WithEvents btnORBSettings As Button
    Friend WithEvents linklblORBTradableInstruments As LinkLabel
    Friend WithEvents TableLayoutPanel5 As TableLayoutPanel
    Friend WithEvents pctrBxORB As PictureBox
    Friend WithEvents pnlORBBodyHorizontalSplitter As TableLayoutPanel
    Friend WithEvents lstORBLog As ListBox
    Friend WithEvents sfdgvORBMainDashboard As Syncfusion.WinForms.DataGrid.SfDataGrid
End Class
