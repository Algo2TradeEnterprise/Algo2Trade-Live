Imports System.IO
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
            _settings.InstrumentsData = Nothing
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
            txtInstrumentDetalis.Text = _settings.InstrumentDetailsFilePath
            txtMaxLossPerTrade.Text = _settings.MaxLossPerTrade
            txtTargetMultiplier.Text = _settings.TargetMultiplier
            chkbNSE.Checked = _settings.RunNSE
            chkbNFO.Checked = _settings.RunNFO
            chkbMCX.Checked = _settings.RunMCX

            txtTelegramAPIKey.Text = _settings.TelegramAPIKey
            txtTelegramChatID.Text = _settings.TelegramChatID

            txtATRPeriod.Text = _settings.ATRPeriod
            txtVWAPEMAPeriod.Text = _settings.VWAP_EMAPeriod
            txtDayCloseSMAPeriod.Text = _settings.DayClose_SMAPeriod
        End If
    End Sub

    Private Sub SaveSettings()
        _settings.SignalTimeFrame = txtSignalTimeFrame.Text
        _settings.InstrumentDetailsFilePath = txtInstrumentDetalis.Text
        _settings.MaxLossPerTrade = Math.Abs(Val(txtMaxLossPerTrade.Text)) * -1
        _settings.TargetMultiplier = txtTargetMultiplier.Text
        _settings.RunNSE = chkbNSE.Checked
        _settings.RunNFO = chkbNFO.Checked
        _settings.RunMCX = chkbMCX.Checked

        _settings.TelegramAPIKey = txtTelegramAPIKey.Text
        _settings.TelegramChatID = txtTelegramChatID.Text

        _settings.ATRPeriod = txtATRPeriod.Text
        _settings.VWAP_EMAPeriod = txtVWAPEMAPeriod.Text
        _settings.DayClose_SMAPeriod = txtDayCloseSMAPeriod.Text

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
        ValidateNumbers(Decimal.MinValue, Decimal.MaxValue, txtMaxLossPerTrade)
        ValidateNumbers(0, Decimal.MaxValue, txtTargetMultiplier)
        ValidateNumbers(1, Integer.MaxValue, txtATRPeriod, True)
        ValidateNumbers(1, Integer.MaxValue, txtVWAPEMAPeriod, True)
        ValidateNumbers(1, Integer.MaxValue, txtDayCloseSMAPeriod, True)

        ValidateFile()
    End Sub

    Private Sub ValidateFile()
        _settings.FillInstrumentDetails(txtInstrumentDetalis.Text, _cts)
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