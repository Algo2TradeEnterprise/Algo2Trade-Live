﻿<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmAdvancedOptions
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmAdvancedOptions))
        Me.tabMain = New System.Windows.Forms.TabControl()
        Me.tabExchangeDetails = New System.Windows.Forms.TabPage()
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.dtpckrCDSContractRolloverTime = New System.Windows.Forms.DateTimePicker()
        Me.lblCDSContractRolloverTime = New System.Windows.Forms.Label()
        Me.dtpckrCDSExchangeEndTime = New System.Windows.Forms.DateTimePicker()
        Me.lblCDSExchangeEndTime = New System.Windows.Forms.Label()
        Me.dtpckrCDSExchangeStartTime = New System.Windows.Forms.DateTimePicker()
        Me.lblCDSExchangeStartTime = New System.Windows.Forms.Label()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.dtpckrMCXContractRolloverTime = New System.Windows.Forms.DateTimePicker()
        Me.lblMCXContractRolloverTime = New System.Windows.Forms.Label()
        Me.dtpckrMCXExchangeEndTime = New System.Windows.Forms.DateTimePicker()
        Me.lblMCXExchangeEndTime = New System.Windows.Forms.Label()
        Me.dtpckrMCXExchangeStartTime = New System.Windows.Forms.DateTimePicker()
        Me.lblMCXExchangeStartTime = New System.Windows.Forms.Label()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.dtpckrNSEContractRolloverTime = New System.Windows.Forms.DateTimePicker()
        Me.lblNSEContractRolloverTime = New System.Windows.Forms.Label()
        Me.dtpckrNSEExchangeEndTime = New System.Windows.Forms.DateTimePicker()
        Me.lblNSEExcahngeEndTime = New System.Windows.Forms.Label()
        Me.dtpckrNSEExchangeStartTime = New System.Windows.Forms.DateTimePicker()
        Me.lblNSEExchangeStartTime = New System.Windows.Forms.Label()
        Me.tabSettings = New System.Windows.Forms.TabPage()
        Me.txtTickerStatusUpdateDelay = New System.Windows.Forms.TextBox()
        Me.lblTickerStatusUpdateDelay = New System.Windows.Forms.Label()
        Me.grpTradingDays = New System.Windows.Forms.GroupBox()
        Me.chkbLstTradingDays = New System.Windows.Forms.CheckedListBox()
        Me.dtpckrDeadStateEndTime = New System.Windows.Forms.DateTimePicker()
        Me.lblDeadStateEndTime = New System.Windows.Forms.Label()
        Me.dtpckrDeadStateStartTime = New System.Windows.Forms.DateTimePicker()
        Me.lblDeadStateStartTime = New System.Windows.Forms.Label()
        Me.dtpckrForceRestartTime = New System.Windows.Forms.DateTimePicker()
        Me.lblForceRestartTime = New System.Windows.Forms.Label()
        Me.txtBackToBackOrderCoolOffDelay = New System.Windows.Forms.TextBox()
        Me.lblBackToBackOrderCoolOffDelay = New System.Windows.Forms.Label()
        Me.txtGetInformationDelay = New System.Windows.Forms.TextBox()
        Me.lblGetInformationDelay = New System.Windows.Forms.Label()
        Me.tabRemarks = New System.Windows.Forms.TabPage()
        Me.txtRemarks = New System.Windows.Forms.TextBox()
        Me.lblRemarks = New System.Windows.Forms.Label()
        Me.tabSender = New System.Windows.Forms.TabPage()
        Me.grpTelegram = New System.Windows.Forms.GroupBox()
        Me.txtTelegramChatID = New System.Windows.Forms.TextBox()
        Me.lblTelegramChatID = New System.Windows.Forms.Label()
        Me.txtTelegramAPI = New System.Windows.Forms.TextBox()
        Me.lblTelegramAPI = New System.Windows.Forms.Label()
        Me.ImageList1 = New System.Windows.Forms.ImageList(Me.components)
        Me.btnSaveSettings = New System.Windows.Forms.Button()
        Me.tabMain.SuspendLayout()
        Me.tabExchangeDetails.SuspendLayout()
        Me.GroupBox3.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        Me.tabSettings.SuspendLayout()
        Me.grpTradingDays.SuspendLayout()
        Me.tabRemarks.SuspendLayout()
        Me.tabSender.SuspendLayout()
        Me.grpTelegram.SuspendLayout()
        Me.SuspendLayout()
        '
        'tabMain
        '
        Me.tabMain.Controls.Add(Me.tabExchangeDetails)
        Me.tabMain.Controls.Add(Me.tabSettings)
        Me.tabMain.Controls.Add(Me.tabRemarks)
        Me.tabMain.Controls.Add(Me.tabSender)
        Me.tabMain.Location = New System.Drawing.Point(0, 0)
        Me.tabMain.Margin = New System.Windows.Forms.Padding(2)
        Me.tabMain.Name = "tabMain"
        Me.tabMain.SelectedIndex = 0
        Me.tabMain.Size = New System.Drawing.Size(318, 301)
        Me.tabMain.TabIndex = 0
        '
        'tabExchangeDetails
        '
        Me.tabExchangeDetails.Controls.Add(Me.GroupBox3)
        Me.tabExchangeDetails.Controls.Add(Me.GroupBox2)
        Me.tabExchangeDetails.Controls.Add(Me.GroupBox1)
        Me.tabExchangeDetails.Location = New System.Drawing.Point(4, 22)
        Me.tabExchangeDetails.Margin = New System.Windows.Forms.Padding(2)
        Me.tabExchangeDetails.Name = "tabExchangeDetails"
        Me.tabExchangeDetails.Size = New System.Drawing.Size(310, 275)
        Me.tabExchangeDetails.TabIndex = 1
        Me.tabExchangeDetails.Text = "Exchange Details"
        Me.tabExchangeDetails.UseVisualStyleBackColor = True
        '
        'GroupBox3
        '
        Me.GroupBox3.Controls.Add(Me.dtpckrCDSContractRolloverTime)
        Me.GroupBox3.Controls.Add(Me.lblCDSContractRolloverTime)
        Me.GroupBox3.Controls.Add(Me.dtpckrCDSExchangeEndTime)
        Me.GroupBox3.Controls.Add(Me.lblCDSExchangeEndTime)
        Me.GroupBox3.Controls.Add(Me.dtpckrCDSExchangeStartTime)
        Me.GroupBox3.Controls.Add(Me.lblCDSExchangeStartTime)
        Me.GroupBox3.Location = New System.Drawing.Point(7, 183)
        Me.GroupBox3.Margin = New System.Windows.Forms.Padding(2)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.Padding = New System.Windows.Forms.Padding(2)
        Me.GroupBox3.Size = New System.Drawing.Size(299, 89)
        Me.GroupBox3.TabIndex = 31
        Me.GroupBox3.TabStop = False
        Me.GroupBox3.Text = "CDS"
        '
        'dtpckrCDSContractRolloverTime
        '
        Me.dtpckrCDSContractRolloverTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpckrCDSContractRolloverTime.Location = New System.Drawing.Point(188, 63)
        Me.dtpckrCDSContractRolloverTime.Margin = New System.Windows.Forms.Padding(2)
        Me.dtpckrCDSContractRolloverTime.Name = "dtpckrCDSContractRolloverTime"
        Me.dtpckrCDSContractRolloverTime.ShowUpDown = True
        Me.dtpckrCDSContractRolloverTime.Size = New System.Drawing.Size(102, 20)
        Me.dtpckrCDSContractRolloverTime.TabIndex = 37
        Me.dtpckrCDSContractRolloverTime.Value = New Date(2019, 3, 12, 0, 0, 0, 0)
        '
        'lblCDSContractRolloverTime
        '
        Me.lblCDSContractRolloverTime.AutoSize = True
        Me.lblCDSContractRolloverTime.Location = New System.Drawing.Point(2, 65)
        Me.lblCDSContractRolloverTime.Name = "lblCDSContractRolloverTime"
        Me.lblCDSContractRolloverTime.Size = New System.Drawing.Size(115, 13)
        Me.lblCDSContractRolloverTime.TabIndex = 38
        Me.lblCDSContractRolloverTime.Text = "Contract Rollover Time"
        '
        'dtpckrCDSExchangeEndTime
        '
        Me.dtpckrCDSExchangeEndTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpckrCDSExchangeEndTime.Location = New System.Drawing.Point(188, 40)
        Me.dtpckrCDSExchangeEndTime.Margin = New System.Windows.Forms.Padding(2)
        Me.dtpckrCDSExchangeEndTime.Name = "dtpckrCDSExchangeEndTime"
        Me.dtpckrCDSExchangeEndTime.ShowUpDown = True
        Me.dtpckrCDSExchangeEndTime.Size = New System.Drawing.Size(102, 20)
        Me.dtpckrCDSExchangeEndTime.TabIndex = 9
        Me.dtpckrCDSExchangeEndTime.Value = New Date(2019, 3, 12, 0, 0, 0, 0)
        '
        'lblCDSExchangeEndTime
        '
        Me.lblCDSExchangeEndTime.AutoSize = True
        Me.lblCDSExchangeEndTime.Location = New System.Drawing.Point(2, 41)
        Me.lblCDSExchangeEndTime.Name = "lblCDSExchangeEndTime"
        Me.lblCDSExchangeEndTime.Size = New System.Drawing.Size(103, 13)
        Me.lblCDSExchangeEndTime.TabIndex = 32
        Me.lblCDSExchangeEndTime.Text = "Exchange End Time"
        '
        'dtpckrCDSExchangeStartTime
        '
        Me.dtpckrCDSExchangeStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpckrCDSExchangeStartTime.Location = New System.Drawing.Point(188, 16)
        Me.dtpckrCDSExchangeStartTime.Margin = New System.Windows.Forms.Padding(2)
        Me.dtpckrCDSExchangeStartTime.Name = "dtpckrCDSExchangeStartTime"
        Me.dtpckrCDSExchangeStartTime.ShowUpDown = True
        Me.dtpckrCDSExchangeStartTime.Size = New System.Drawing.Size(102, 20)
        Me.dtpckrCDSExchangeStartTime.TabIndex = 8
        Me.dtpckrCDSExchangeStartTime.Value = New Date(2019, 3, 12, 0, 0, 0, 0)
        '
        'lblCDSExchangeStartTime
        '
        Me.lblCDSExchangeStartTime.AutoSize = True
        Me.lblCDSExchangeStartTime.Location = New System.Drawing.Point(2, 18)
        Me.lblCDSExchangeStartTime.Name = "lblCDSExchangeStartTime"
        Me.lblCDSExchangeStartTime.Size = New System.Drawing.Size(106, 13)
        Me.lblCDSExchangeStartTime.TabIndex = 30
        Me.lblCDSExchangeStartTime.Text = "Exchange Start Time"
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.dtpckrMCXContractRolloverTime)
        Me.GroupBox2.Controls.Add(Me.lblMCXContractRolloverTime)
        Me.GroupBox2.Controls.Add(Me.dtpckrMCXExchangeEndTime)
        Me.GroupBox2.Controls.Add(Me.lblMCXExchangeEndTime)
        Me.GroupBox2.Controls.Add(Me.dtpckrMCXExchangeStartTime)
        Me.GroupBox2.Controls.Add(Me.lblMCXExchangeStartTime)
        Me.GroupBox2.Location = New System.Drawing.Point(7, 93)
        Me.GroupBox2.Margin = New System.Windows.Forms.Padding(2)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Padding = New System.Windows.Forms.Padding(2)
        Me.GroupBox2.Size = New System.Drawing.Size(299, 89)
        Me.GroupBox2.TabIndex = 30
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "MCX"
        '
        'dtpckrMCXContractRolloverTime
        '
        Me.dtpckrMCXContractRolloverTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpckrMCXContractRolloverTime.Location = New System.Drawing.Point(188, 63)
        Me.dtpckrMCXContractRolloverTime.Margin = New System.Windows.Forms.Padding(2)
        Me.dtpckrMCXContractRolloverTime.Name = "dtpckrMCXContractRolloverTime"
        Me.dtpckrMCXContractRolloverTime.ShowUpDown = True
        Me.dtpckrMCXContractRolloverTime.Size = New System.Drawing.Size(102, 20)
        Me.dtpckrMCXContractRolloverTime.TabIndex = 35
        Me.dtpckrMCXContractRolloverTime.Value = New Date(2019, 3, 12, 0, 0, 0, 0)
        '
        'lblMCXContractRolloverTime
        '
        Me.lblMCXContractRolloverTime.AutoSize = True
        Me.lblMCXContractRolloverTime.Location = New System.Drawing.Point(2, 64)
        Me.lblMCXContractRolloverTime.Name = "lblMCXContractRolloverTime"
        Me.lblMCXContractRolloverTime.Size = New System.Drawing.Size(115, 13)
        Me.lblMCXContractRolloverTime.TabIndex = 36
        Me.lblMCXContractRolloverTime.Text = "Contract Rollover Time"
        '
        'dtpckrMCXExchangeEndTime
        '
        Me.dtpckrMCXExchangeEndTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpckrMCXExchangeEndTime.Location = New System.Drawing.Point(188, 40)
        Me.dtpckrMCXExchangeEndTime.Margin = New System.Windows.Forms.Padding(2)
        Me.dtpckrMCXExchangeEndTime.Name = "dtpckrMCXExchangeEndTime"
        Me.dtpckrMCXExchangeEndTime.ShowUpDown = True
        Me.dtpckrMCXExchangeEndTime.Size = New System.Drawing.Size(102, 20)
        Me.dtpckrMCXExchangeEndTime.TabIndex = 7
        Me.dtpckrMCXExchangeEndTime.Value = New Date(2019, 3, 12, 0, 0, 0, 0)
        '
        'lblMCXExchangeEndTime
        '
        Me.lblMCXExchangeEndTime.AutoSize = True
        Me.lblMCXExchangeEndTime.Location = New System.Drawing.Point(2, 41)
        Me.lblMCXExchangeEndTime.Name = "lblMCXExchangeEndTime"
        Me.lblMCXExchangeEndTime.Size = New System.Drawing.Size(103, 13)
        Me.lblMCXExchangeEndTime.TabIndex = 32
        Me.lblMCXExchangeEndTime.Text = "Exchange End Time"
        '
        'dtpckrMCXExchangeStartTime
        '
        Me.dtpckrMCXExchangeStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpckrMCXExchangeStartTime.Location = New System.Drawing.Point(188, 16)
        Me.dtpckrMCXExchangeStartTime.Margin = New System.Windows.Forms.Padding(2)
        Me.dtpckrMCXExchangeStartTime.Name = "dtpckrMCXExchangeStartTime"
        Me.dtpckrMCXExchangeStartTime.ShowUpDown = True
        Me.dtpckrMCXExchangeStartTime.Size = New System.Drawing.Size(102, 20)
        Me.dtpckrMCXExchangeStartTime.TabIndex = 6
        Me.dtpckrMCXExchangeStartTime.Value = New Date(2019, 3, 12, 0, 0, 0, 0)
        '
        'lblMCXExchangeStartTime
        '
        Me.lblMCXExchangeStartTime.AutoSize = True
        Me.lblMCXExchangeStartTime.Location = New System.Drawing.Point(2, 18)
        Me.lblMCXExchangeStartTime.Name = "lblMCXExchangeStartTime"
        Me.lblMCXExchangeStartTime.Size = New System.Drawing.Size(106, 13)
        Me.lblMCXExchangeStartTime.TabIndex = 30
        Me.lblMCXExchangeStartTime.Text = "Exchange Start Time"
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.dtpckrNSEContractRolloverTime)
        Me.GroupBox1.Controls.Add(Me.lblNSEContractRolloverTime)
        Me.GroupBox1.Controls.Add(Me.dtpckrNSEExchangeEndTime)
        Me.GroupBox1.Controls.Add(Me.lblNSEExcahngeEndTime)
        Me.GroupBox1.Controls.Add(Me.dtpckrNSEExchangeStartTime)
        Me.GroupBox1.Controls.Add(Me.lblNSEExchangeStartTime)
        Me.GroupBox1.Location = New System.Drawing.Point(7, 2)
        Me.GroupBox1.Margin = New System.Windows.Forms.Padding(2)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Padding = New System.Windows.Forms.Padding(2)
        Me.GroupBox1.Size = New System.Drawing.Size(299, 89)
        Me.GroupBox1.TabIndex = 29
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "NSE / NFO"
        '
        'dtpckrNSEContractRolloverTime
        '
        Me.dtpckrNSEContractRolloverTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpckrNSEContractRolloverTime.Location = New System.Drawing.Point(188, 63)
        Me.dtpckrNSEContractRolloverTime.Margin = New System.Windows.Forms.Padding(2)
        Me.dtpckrNSEContractRolloverTime.Name = "dtpckrNSEContractRolloverTime"
        Me.dtpckrNSEContractRolloverTime.ShowUpDown = True
        Me.dtpckrNSEContractRolloverTime.Size = New System.Drawing.Size(102, 20)
        Me.dtpckrNSEContractRolloverTime.TabIndex = 33
        Me.dtpckrNSEContractRolloverTime.Value = New Date(2019, 3, 12, 0, 0, 0, 0)
        '
        'lblNSEContractRolloverTime
        '
        Me.lblNSEContractRolloverTime.AutoSize = True
        Me.lblNSEContractRolloverTime.Location = New System.Drawing.Point(2, 64)
        Me.lblNSEContractRolloverTime.Name = "lblNSEContractRolloverTime"
        Me.lblNSEContractRolloverTime.Size = New System.Drawing.Size(115, 13)
        Me.lblNSEContractRolloverTime.TabIndex = 34
        Me.lblNSEContractRolloverTime.Text = "Contract Rollover Time"
        '
        'dtpckrNSEExchangeEndTime
        '
        Me.dtpckrNSEExchangeEndTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpckrNSEExchangeEndTime.Location = New System.Drawing.Point(188, 40)
        Me.dtpckrNSEExchangeEndTime.Margin = New System.Windows.Forms.Padding(2)
        Me.dtpckrNSEExchangeEndTime.Name = "dtpckrNSEExchangeEndTime"
        Me.dtpckrNSEExchangeEndTime.ShowUpDown = True
        Me.dtpckrNSEExchangeEndTime.Size = New System.Drawing.Size(102, 20)
        Me.dtpckrNSEExchangeEndTime.TabIndex = 5
        Me.dtpckrNSEExchangeEndTime.Value = New Date(2019, 3, 12, 0, 0, 0, 0)
        '
        'lblNSEExcahngeEndTime
        '
        Me.lblNSEExcahngeEndTime.AutoSize = True
        Me.lblNSEExcahngeEndTime.Location = New System.Drawing.Point(2, 41)
        Me.lblNSEExcahngeEndTime.Name = "lblNSEExcahngeEndTime"
        Me.lblNSEExcahngeEndTime.Size = New System.Drawing.Size(103, 13)
        Me.lblNSEExcahngeEndTime.TabIndex = 32
        Me.lblNSEExcahngeEndTime.Text = "Exchange End Time"
        '
        'dtpckrNSEExchangeStartTime
        '
        Me.dtpckrNSEExchangeStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpckrNSEExchangeStartTime.Location = New System.Drawing.Point(188, 16)
        Me.dtpckrNSEExchangeStartTime.Margin = New System.Windows.Forms.Padding(2)
        Me.dtpckrNSEExchangeStartTime.Name = "dtpckrNSEExchangeStartTime"
        Me.dtpckrNSEExchangeStartTime.ShowUpDown = True
        Me.dtpckrNSEExchangeStartTime.Size = New System.Drawing.Size(102, 20)
        Me.dtpckrNSEExchangeStartTime.TabIndex = 4
        Me.dtpckrNSEExchangeStartTime.Value = New Date(2019, 3, 12, 0, 0, 0, 0)
        '
        'lblNSEExchangeStartTime
        '
        Me.lblNSEExchangeStartTime.AutoSize = True
        Me.lblNSEExchangeStartTime.Location = New System.Drawing.Point(2, 18)
        Me.lblNSEExchangeStartTime.Name = "lblNSEExchangeStartTime"
        Me.lblNSEExchangeStartTime.Size = New System.Drawing.Size(106, 13)
        Me.lblNSEExchangeStartTime.TabIndex = 30
        Me.lblNSEExchangeStartTime.Text = "Exchange Start Time"
        '
        'tabSettings
        '
        Me.tabSettings.Controls.Add(Me.txtTickerStatusUpdateDelay)
        Me.tabSettings.Controls.Add(Me.lblTickerStatusUpdateDelay)
        Me.tabSettings.Controls.Add(Me.grpTradingDays)
        Me.tabSettings.Controls.Add(Me.dtpckrDeadStateEndTime)
        Me.tabSettings.Controls.Add(Me.lblDeadStateEndTime)
        Me.tabSettings.Controls.Add(Me.dtpckrDeadStateStartTime)
        Me.tabSettings.Controls.Add(Me.lblDeadStateStartTime)
        Me.tabSettings.Controls.Add(Me.dtpckrForceRestartTime)
        Me.tabSettings.Controls.Add(Me.lblForceRestartTime)
        Me.tabSettings.Controls.Add(Me.txtBackToBackOrderCoolOffDelay)
        Me.tabSettings.Controls.Add(Me.lblBackToBackOrderCoolOffDelay)
        Me.tabSettings.Controls.Add(Me.txtGetInformationDelay)
        Me.tabSettings.Controls.Add(Me.lblGetInformationDelay)
        Me.tabSettings.Location = New System.Drawing.Point(4, 22)
        Me.tabSettings.Margin = New System.Windows.Forms.Padding(2)
        Me.tabSettings.Name = "tabSettings"
        Me.tabSettings.Padding = New System.Windows.Forms.Padding(2)
        Me.tabSettings.Size = New System.Drawing.Size(310, 275)
        Me.tabSettings.TabIndex = 0
        Me.tabSettings.Text = "Settings"
        Me.tabSettings.UseVisualStyleBackColor = True
        '
        'txtTickerStatusUpdateDelay
        '
        Me.txtTickerStatusUpdateDelay.Location = New System.Drawing.Point(201, 67)
        Me.txtTickerStatusUpdateDelay.Name = "txtTickerStatusUpdateDelay"
        Me.txtTickerStatusUpdateDelay.Size = New System.Drawing.Size(102, 20)
        Me.txtTickerStatusUpdateDelay.TabIndex = 3
        Me.txtTickerStatusUpdateDelay.Tag = "Ticker Status Update Delay"
        '
        'lblTickerStatusUpdateDelay
        '
        Me.lblTickerStatusUpdateDelay.AutoSize = True
        Me.lblTickerStatusUpdateDelay.Location = New System.Drawing.Point(4, 70)
        Me.lblTickerStatusUpdateDelay.Name = "lblTickerStatusUpdateDelay"
        Me.lblTickerStatusUpdateDelay.Size = New System.Drawing.Size(164, 13)
        Me.lblTickerStatusUpdateDelay.TabIndex = 27
        Me.lblTickerStatusUpdateDelay.Text = "Ticker Status Update Delay (sec)"
        '
        'grpTradingDays
        '
        Me.grpTradingDays.Controls.Add(Me.chkbLstTradingDays)
        Me.grpTradingDays.Location = New System.Drawing.Point(7, 180)
        Me.grpTradingDays.Margin = New System.Windows.Forms.Padding(2)
        Me.grpTradingDays.Name = "grpTradingDays"
        Me.grpTradingDays.Padding = New System.Windows.Forms.Padding(2)
        Me.grpTradingDays.Size = New System.Drawing.Size(296, 90)
        Me.grpTradingDays.TabIndex = 7
        Me.grpTradingDays.TabStop = False
        Me.grpTradingDays.Text = "Select Trading Days"
        '
        'chkbLstTradingDays
        '
        Me.chkbLstTradingDays.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkbLstTradingDays.FormattingEnabled = True
        Me.chkbLstTradingDays.Items.AddRange(New Object() {"Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"})
        Me.chkbLstTradingDays.Location = New System.Drawing.Point(4, 17)
        Me.chkbLstTradingDays.Margin = New System.Windows.Forms.Padding(2)
        Me.chkbLstTradingDays.MultiColumn = True
        Me.chkbLstTradingDays.Name = "chkbLstTradingDays"
        Me.chkbLstTradingDays.Size = New System.Drawing.Size(288, 68)
        Me.chkbLstTradingDays.TabIndex = 0
        '
        'dtpckrDeadStateEndTime
        '
        Me.dtpckrDeadStateEndTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpckrDeadStateEndTime.Location = New System.Drawing.Point(201, 158)
        Me.dtpckrDeadStateEndTime.Margin = New System.Windows.Forms.Padding(2)
        Me.dtpckrDeadStateEndTime.Name = "dtpckrDeadStateEndTime"
        Me.dtpckrDeadStateEndTime.ShowUpDown = True
        Me.dtpckrDeadStateEndTime.Size = New System.Drawing.Size(102, 20)
        Me.dtpckrDeadStateEndTime.TabIndex = 6
        Me.dtpckrDeadStateEndTime.Value = New Date(2019, 3, 12, 0, 0, 0, 0)
        '
        'lblDeadStateEndTime
        '
        Me.lblDeadStateEndTime.AutoSize = True
        Me.lblDeadStateEndTime.Location = New System.Drawing.Point(4, 160)
        Me.lblDeadStateEndTime.Name = "lblDeadStateEndTime"
        Me.lblDeadStateEndTime.Size = New System.Drawing.Size(100, 13)
        Me.lblDeadStateEndTime.TabIndex = 25
        Me.lblDeadStateEndTime.Text = "Idle State End Time"
        '
        'dtpckrDeadStateStartTime
        '
        Me.dtpckrDeadStateStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpckrDeadStateStartTime.Location = New System.Drawing.Point(201, 128)
        Me.dtpckrDeadStateStartTime.Margin = New System.Windows.Forms.Padding(2)
        Me.dtpckrDeadStateStartTime.Name = "dtpckrDeadStateStartTime"
        Me.dtpckrDeadStateStartTime.ShowUpDown = True
        Me.dtpckrDeadStateStartTime.Size = New System.Drawing.Size(102, 20)
        Me.dtpckrDeadStateStartTime.TabIndex = 5
        Me.dtpckrDeadStateStartTime.Value = New Date(2019, 3, 12, 0, 0, 0, 0)
        '
        'lblDeadStateStartTime
        '
        Me.lblDeadStateStartTime.AutoSize = True
        Me.lblDeadStateStartTime.Location = New System.Drawing.Point(4, 130)
        Me.lblDeadStateStartTime.Name = "lblDeadStateStartTime"
        Me.lblDeadStateStartTime.Size = New System.Drawing.Size(103, 13)
        Me.lblDeadStateStartTime.TabIndex = 23
        Me.lblDeadStateStartTime.Text = "Idle State Start Time"
        '
        'dtpckrForceRestartTime
        '
        Me.dtpckrForceRestartTime.Format = System.Windows.Forms.DateTimePickerFormat.Time
        Me.dtpckrForceRestartTime.Location = New System.Drawing.Point(201, 98)
        Me.dtpckrForceRestartTime.Margin = New System.Windows.Forms.Padding(2)
        Me.dtpckrForceRestartTime.Name = "dtpckrForceRestartTime"
        Me.dtpckrForceRestartTime.ShowUpDown = True
        Me.dtpckrForceRestartTime.Size = New System.Drawing.Size(102, 20)
        Me.dtpckrForceRestartTime.TabIndex = 4
        Me.dtpckrForceRestartTime.Value = New Date(2019, 3, 12, 0, 0, 0, 0)
        '
        'lblForceRestartTime
        '
        Me.lblForceRestartTime.AutoSize = True
        Me.lblForceRestartTime.Location = New System.Drawing.Point(4, 100)
        Me.lblForceRestartTime.Name = "lblForceRestartTime"
        Me.lblForceRestartTime.Size = New System.Drawing.Size(97, 13)
        Me.lblForceRestartTime.TabIndex = 21
        Me.lblForceRestartTime.Text = "Force Restart Time"
        '
        'txtBackToBackOrderCoolOffDelay
        '
        Me.txtBackToBackOrderCoolOffDelay.Location = New System.Drawing.Point(201, 37)
        Me.txtBackToBackOrderCoolOffDelay.Name = "txtBackToBackOrderCoolOffDelay"
        Me.txtBackToBackOrderCoolOffDelay.Size = New System.Drawing.Size(102, 20)
        Me.txtBackToBackOrderCoolOffDelay.TabIndex = 2
        Me.txtBackToBackOrderCoolOffDelay.Tag = "Back To Back Order Cool Off Delay"
        '
        'lblBackToBackOrderCoolOffDelay
        '
        Me.lblBackToBackOrderCoolOffDelay.AutoSize = True
        Me.lblBackToBackOrderCoolOffDelay.Location = New System.Drawing.Point(4, 40)
        Me.lblBackToBackOrderCoolOffDelay.Name = "lblBackToBackOrderCoolOffDelay"
        Me.lblBackToBackOrderCoolOffDelay.Size = New System.Drawing.Size(193, 13)
        Me.lblBackToBackOrderCoolOffDelay.TabIndex = 12
        Me.lblBackToBackOrderCoolOffDelay.Text = "BackToBack Order CoolOff Delay (sec)"
        '
        'txtGetInformationDelay
        '
        Me.txtGetInformationDelay.Location = New System.Drawing.Point(201, 8)
        Me.txtGetInformationDelay.Name = "txtGetInformationDelay"
        Me.txtGetInformationDelay.Size = New System.Drawing.Size(102, 20)
        Me.txtGetInformationDelay.TabIndex = 1
        Me.txtGetInformationDelay.Tag = "Get Information Delay"
        '
        'lblGetInformationDelay
        '
        Me.lblGetInformationDelay.AutoSize = True
        Me.lblGetInformationDelay.Location = New System.Drawing.Point(4, 10)
        Me.lblGetInformationDelay.Name = "lblGetInformationDelay"
        Me.lblGetInformationDelay.Size = New System.Drawing.Size(135, 13)
        Me.lblGetInformationDelay.TabIndex = 10
        Me.lblGetInformationDelay.Text = "Get Information Delay (sec)"
        '
        'tabRemarks
        '
        Me.tabRemarks.Controls.Add(Me.txtRemarks)
        Me.tabRemarks.Controls.Add(Me.lblRemarks)
        Me.tabRemarks.Location = New System.Drawing.Point(4, 22)
        Me.tabRemarks.Margin = New System.Windows.Forms.Padding(2)
        Me.tabRemarks.Name = "tabRemarks"
        Me.tabRemarks.Size = New System.Drawing.Size(310, 275)
        Me.tabRemarks.TabIndex = 2
        Me.tabRemarks.Text = "Remarks"
        Me.tabRemarks.UseVisualStyleBackColor = True
        '
        'txtRemarks
        '
        Me.txtRemarks.Location = New System.Drawing.Point(88, 16)
        Me.txtRemarks.Name = "txtRemarks"
        Me.txtRemarks.Size = New System.Drawing.Size(217, 20)
        Me.txtRemarks.TabIndex = 11
        Me.txtRemarks.Tag = "Form Remarks"
        '
        'lblRemarks
        '
        Me.lblRemarks.AutoSize = True
        Me.lblRemarks.Location = New System.Drawing.Point(7, 17)
        Me.lblRemarks.Name = "lblRemarks"
        Me.lblRemarks.Size = New System.Drawing.Size(75, 13)
        Me.lblRemarks.TabIndex = 12
        Me.lblRemarks.Text = "Form Remarks"
        '
        'tabSender
        '
        Me.tabSender.Controls.Add(Me.grpTelegram)
        Me.tabSender.Location = New System.Drawing.Point(4, 22)
        Me.tabSender.Margin = New System.Windows.Forms.Padding(2)
        Me.tabSender.Name = "tabSender"
        Me.tabSender.Size = New System.Drawing.Size(310, 275)
        Me.tabSender.TabIndex = 3
        Me.tabSender.Text = "Sender"
        Me.tabSender.UseVisualStyleBackColor = True
        '
        'grpTelegram
        '
        Me.grpTelegram.Controls.Add(Me.txtTelegramChatID)
        Me.grpTelegram.Controls.Add(Me.lblTelegramChatID)
        Me.grpTelegram.Controls.Add(Me.txtTelegramAPI)
        Me.grpTelegram.Controls.Add(Me.lblTelegramAPI)
        Me.grpTelegram.Location = New System.Drawing.Point(6, 4)
        Me.grpTelegram.Margin = New System.Windows.Forms.Padding(2)
        Me.grpTelegram.Name = "grpTelegram"
        Me.grpTelegram.Padding = New System.Windows.Forms.Padding(2)
        Me.grpTelegram.Size = New System.Drawing.Size(296, 76)
        Me.grpTelegram.TabIndex = 19
        Me.grpTelegram.TabStop = False
        Me.grpTelegram.Text = "Telegram Details"
        '
        'txtTelegramChatID
        '
        Me.txtTelegramChatID.Location = New System.Drawing.Point(69, 46)
        Me.txtTelegramChatID.Name = "txtTelegramChatID"
        Me.txtTelegramChatID.Size = New System.Drawing.Size(223, 20)
        Me.txtTelegramChatID.TabIndex = 32
        Me.txtTelegramChatID.Tag = "Telegram Chat ID"
        '
        'lblTelegramChatID
        '
        Me.lblTelegramChatID.AutoSize = True
        Me.lblTelegramChatID.Location = New System.Drawing.Point(7, 49)
        Me.lblTelegramChatID.Name = "lblTelegramChatID"
        Me.lblTelegramChatID.Size = New System.Drawing.Size(43, 13)
        Me.lblTelegramChatID.TabIndex = 35
        Me.lblTelegramChatID.Text = "Chat ID"
        '
        'txtTelegramAPI
        '
        Me.txtTelegramAPI.Location = New System.Drawing.Point(69, 20)
        Me.txtTelegramAPI.Name = "txtTelegramAPI"
        Me.txtTelegramAPI.Size = New System.Drawing.Size(223, 20)
        Me.txtTelegramAPI.TabIndex = 30
        Me.txtTelegramAPI.Tag = "Telegram API Key"
        '
        'lblTelegramAPI
        '
        Me.lblTelegramAPI.AutoSize = True
        Me.lblTelegramAPI.Location = New System.Drawing.Point(8, 23)
        Me.lblTelegramAPI.Name = "lblTelegramAPI"
        Me.lblTelegramAPI.Size = New System.Drawing.Size(45, 13)
        Me.lblTelegramAPI.TabIndex = 31
        Me.lblTelegramAPI.Text = "API Key"
        '
        'ImageList1
        '
        Me.ImageList1.ImageStream = CType(resources.GetObject("ImageList1.ImageStream"), System.Windows.Forms.ImageListStreamer)
        Me.ImageList1.TransparentColor = System.Drawing.Color.Transparent
        Me.ImageList1.Images.SetKeyName(0, "save-icon-36533.png")
        '
        'btnSaveSettings
        '
        Me.btnSaveSettings.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.btnSaveSettings.ImageKey = "save-icon-36533.png"
        Me.btnSaveSettings.ImageList = Me.ImageList1
        Me.btnSaveSettings.Location = New System.Drawing.Point(318, 20)
        Me.btnSaveSettings.Name = "btnSaveSettings"
        Me.btnSaveSettings.Size = New System.Drawing.Size(84, 47)
        Me.btnSaveSettings.TabIndex = 0
        Me.btnSaveSettings.Text = "&Save"
        Me.btnSaveSettings.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        Me.btnSaveSettings.UseVisualStyleBackColor = True
        '
        'frmAdvancedOptions
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(406, 303)
        Me.Controls.Add(Me.btnSaveSettings)
        Me.Controls.Add(Me.tabMain)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(2)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmAdvancedOptions"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Advanced Options"
        Me.tabMain.ResumeLayout(False)
        Me.tabExchangeDetails.ResumeLayout(False)
        Me.GroupBox3.ResumeLayout(False)
        Me.GroupBox3.PerformLayout()
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.tabSettings.ResumeLayout(False)
        Me.tabSettings.PerformLayout()
        Me.grpTradingDays.ResumeLayout(False)
        Me.tabRemarks.ResumeLayout(False)
        Me.tabRemarks.PerformLayout()
        Me.tabSender.ResumeLayout(False)
        Me.grpTelegram.ResumeLayout(False)
        Me.grpTelegram.PerformLayout()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents tabMain As TabControl
    Friend WithEvents tabSettings As TabPage
    Friend WithEvents ImageList1 As ImageList
    Friend WithEvents txtGetInformationDelay As TextBox
    Friend WithEvents lblGetInformationDelay As Label
    Friend WithEvents txtBackToBackOrderCoolOffDelay As TextBox
    Friend WithEvents lblBackToBackOrderCoolOffDelay As Label
    Friend WithEvents dtpckrForceRestartTime As DateTimePicker
    Friend WithEvents lblForceRestartTime As Label
    Friend WithEvents tabExchangeDetails As TabPage
    Friend WithEvents GroupBox1 As GroupBox
    Friend WithEvents dtpckrNSEExchangeStartTime As DateTimePicker
    Friend WithEvents lblNSEExchangeStartTime As Label
    Friend WithEvents dtpckrNSEExchangeEndTime As DateTimePicker
    Friend WithEvents lblNSEExcahngeEndTime As Label
    Friend WithEvents GroupBox2 As GroupBox
    Friend WithEvents dtpckrMCXExchangeEndTime As DateTimePicker
    Friend WithEvents lblMCXExchangeEndTime As Label
    Friend WithEvents dtpckrMCXExchangeStartTime As DateTimePicker
    Friend WithEvents lblMCXExchangeStartTime As Label
    Friend WithEvents GroupBox3 As GroupBox
    Friend WithEvents dtpckrCDSExchangeEndTime As DateTimePicker
    Friend WithEvents lblCDSExchangeEndTime As Label
    Friend WithEvents dtpckrCDSExchangeStartTime As DateTimePicker
    Friend WithEvents lblCDSExchangeStartTime As Label
    Friend WithEvents btnSaveSettings As Button
    Friend WithEvents tabRemarks As TabPage
    Friend WithEvents txtRemarks As TextBox
    Friend WithEvents lblRemarks As Label
    Friend WithEvents dtpckrCDSContractRolloverTime As DateTimePicker
    Friend WithEvents lblCDSContractRolloverTime As Label
    Friend WithEvents dtpckrMCXContractRolloverTime As DateTimePicker
    Friend WithEvents lblMCXContractRolloverTime As Label
    Friend WithEvents dtpckrNSEContractRolloverTime As DateTimePicker
    Friend WithEvents lblNSEContractRolloverTime As Label
    Friend WithEvents tabSender As TabPage
    Friend WithEvents grpTelegram As GroupBox
    Friend WithEvents txtTelegramChatID As TextBox
    Friend WithEvents lblTelegramChatID As Label
    Friend WithEvents txtTelegramAPI As TextBox
    Friend WithEvents lblTelegramAPI As Label
    Friend WithEvents dtpckrDeadStateEndTime As DateTimePicker
    Friend WithEvents lblDeadStateEndTime As Label
    Friend WithEvents dtpckrDeadStateStartTime As DateTimePicker
    Friend WithEvents lblDeadStateStartTime As Label
    Friend WithEvents grpTradingDays As GroupBox
    Friend WithEvents chkbLstTradingDays As CheckedListBox
    Friend WithEvents txtTickerStatusUpdateDelay As TextBox
    Friend WithEvents lblTickerStatusUpdateDelay As Label
End Class
