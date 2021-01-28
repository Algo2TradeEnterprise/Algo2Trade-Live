﻿Imports System.IO
Imports System.Threading

Public Class frmNFOSettings

    Private _cts As CancellationTokenSource = Nothing
    Private _settings As NFOUserInputs = Nothing
    Private _settingsFilename As String = NFOUserInputs.SettingsFileName
    Private _strategyRunning As Boolean = False

    Public Sub New(ByRef userInputs As NFOUserInputs, ByVal strategyRunning As Boolean)
        InitializeComponent()
        _settings = userInputs
        _strategyRunning = strategyRunning
    End Sub

    Private Sub frmNFOSettings_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If _strategyRunning Then
            btnSave.Enabled = False
        End If
        LoadSettings()
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Try
            _cts = New CancellationTokenSource
            If _settings Is Nothing Then _settings = New NFOUserInputs
            ValidateInputs()
            SaveSettings()
            Me.Close()
        Catch ex As Exception
            MsgBox(String.Format("The following error occurred: {0}", ex.Message), MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub LoadSettings()
        If File.Exists(_settingsFilename) Then
            _settings = Utilities.Strings.DeserializeToCollection(Of NFOUserInputs)(_settingsFilename)
            txtSignalTimeFrame.Text = _settings.SignalTimeFrame
            txtTargetMultiplier.Text = _settings.TargetMultiplier
            txtTargetToLeftMovementPercentage.Text = _settings.TargetToLeftMovementPercentage
            txtMaxLossPerTrade.Text = _settings.NSEMaxLossPerTrade
            txtOverallMaxProfit.Text = _settings.OverallMaxProfit
            txtOverallMaxLoss.Text = _settings.OverallMaxLoss
            dtpckrEODExitTime.Value = _settings.EODExitTime
            txtInstrumentDetalis.Text = _settings.InstrumentDetailsFilepath

            chkbAutoSelectStock.Checked = _settings.AutoSelectStock
            txtMinPrice.Text = _settings.MinimumStockPrice
            txtMaxPrice.Text = _settings.MaximumStockPrice
            txtMinVolume.Text = _settings.MinimumVolume
            txtMinATRPer.Text = _settings.MinimumATRPercentage
            txtNumberOfStocks.Text = _settings.NumberOfStocks

            txtTelegramAPIKey.Text = _settings.TelegramAPIKey
            txtTelegramChatID.Text = _settings.TelegramChatID

            txtDayCloseATRPeriod.Text = _settings.DayClose_ATRPeriod
            txtVWAPEMAPeriod.Text = _settings.VWAP_EMAPeriod
            txtDayCloseSMAPeriod.Text = _settings.DayClose_SMAPeriod
            txtCloseRSIPeriod.Text = _settings.Close_RSIPeriod
            txtRSIValue.Text = _settings.RSILevel

            chkbAutoSelectStock_CheckedChanged(Nothing, Nothing)
        End If
    End Sub

    Private Sub SaveSettings()
        _settings.SignalTimeFrame = txtSignalTimeFrame.Text
        _settings.TargetMultiplier = txtTargetMultiplier.Text
        _settings.TargetToLeftMovementPercentage = txtTargetToLeftMovementPercentage.Text
        _settings.NSEMaxLossPerTrade = Math.Abs(CDec(txtMaxLossPerTrade.Text)) * -1
        _settings.OverallMaxProfit = Math.Abs(CDec(txtOverallMaxProfit.Text))
        _settings.OverallMaxLoss = Math.Abs(CDec(txtOverallMaxLoss.Text)) * -1
        _settings.EODExitTime = dtpckrEODExitTime.Value
        _settings.InstrumentDetailsFilepath = txtInstrumentDetalis.Text

        _settings.AutoSelectStock = chkbAutoSelectStock.Checked
        _settings.MinimumStockPrice = txtMinPrice.Text
        _settings.MaximumStockPrice = txtMaxPrice.Text
        _settings.MinimumVolume = txtMinVolume.Text
        _settings.MinimumATRPercentage = txtMinATRPer.Text
        _settings.NumberOfStocks = txtNumberOfStocks.Text

        _settings.TelegramAPIKey = txtTelegramAPIKey.Text
        _settings.TelegramChatID = txtTelegramChatID.Text

        _settings.DayClose_ATRPeriod = txtDayCloseATRPeriod.Text
        _settings.VWAP_EMAPeriod = txtVWAPEMAPeriod.Text
        _settings.DayClose_SMAPeriod = txtDayCloseSMAPeriod.Text
        _settings.Close_RSIPeriod = txtCloseRSIPeriod.Text
        _settings.RSILevel = txtRSIValue.Text

        Utilities.Strings.SerializeFromCollection(Of NFOUserInputs)(_settingsFilename, _settings)
    End Sub

    Private Function ValidateNumbers(ByVal startNumber As Decimal, ByVal endNumber As Decimal, ByVal inputTB As TextBox, Optional ByVal validateInteger As Boolean = False) As Boolean
        Dim ret As Boolean = False
        If IsNumeric(inputTB.Text) Then
            If validateInteger Then
                If Val(inputTB.Text) <> Math.Round(Val(inputTB.Text), 0) Then
                    Throw New ApplicationException(String.Format("{0} should be of type Integer", inputTB.Tag))
                End If
            End If
            If Val(inputTB.Text) >= startNumber And Val(inputTB.Text) <= endNumber Then
                ret = True
            End If
        End If
        If Not ret Then Throw New ApplicationException(String.Format("{0} cannot have a value < {1} or > {2}", inputTB.Tag, startNumber, endNumber))
        Return ret
    End Function

    Private Sub ValidateInputs()
        ValidateNumbers(1, 60, txtSignalTimeFrame, True)
        ValidateNumbers(0, Decimal.MaxValue, txtTargetToLeftMovementPercentage)
        ValidateNumbers(0, Decimal.MaxValue, txtTargetMultiplier)

        ValidateNumbers(0, Decimal.MaxValue, txtMinPrice)
        ValidateNumbers(CDec(txtMinPrice.Text), Decimal.MaxValue, txtMaxPrice)
        ValidateNumbers(0, Long.MaxValue, txtMinVolume)
        ValidateNumbers(0, Decimal.MaxValue, txtMinATRPer)

        ValidateNumbers(1, Integer.MaxValue, txtDayCloseATRPeriod, True)
        ValidateNumbers(1, Integer.MaxValue, txtVWAPEMAPeriod, True)
        ValidateNumbers(1, Integer.MaxValue, txtDayCloseSMAPeriod, True)
        ValidateNumbers(1, Integer.MaxValue, txtCloseRSIPeriod, True)
        ValidateNumbers(Decimal.MinValue, Decimal.MaxValue, txtRSIValue)
    End Sub

    Private Sub chkbAutoSelectStock_CheckedChanged(sender As Object, e As EventArgs) Handles chkbAutoSelectStock.CheckedChanged
        grpStockSelection.Enabled = chkbAutoSelectStock.Checked
    End Sub

    Private Sub btnBrowse_Click(sender As Object, e As EventArgs) Handles btnBrowse.Click
        opnFileSettings.Filter = "|*.csv"
        opnFileSettings.ShowDialog()
    End Sub

    Private Sub opnFileSettings_FileOk(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles opnFileSettings.FileOk
        Dim extension As String = Path.GetExtension(opnFileSettings.FileName)
        If extension = ".csv" Then
            txtInstrumentDetalis.Text = opnFileSettings.FileName
        Else
            MsgBox("File Type not supported. Please Try again.", MsgBoxStyle.Critical)
        End If
    End Sub
End Class