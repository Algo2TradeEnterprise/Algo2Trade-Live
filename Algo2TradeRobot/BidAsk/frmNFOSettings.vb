Imports System.IO
Imports System.Threading

Public Class frmNFOSettings

    Private _cts As CancellationTokenSource = Nothing
    Private _settings As NFOUserInputs = Nothing
    Private _settingsFilename As String = NFOUserInputs.SettingsFileName

    Public Sub New(ByRef userInputs As NFOUserInputs)
        InitializeComponent()
        _settings = userInputs
    End Sub

    Private Sub frmNFOSettings_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadSettings()
    End Sub
    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Try
            _cts = New CancellationTokenSource
            If _settings Is Nothing Then _settings = New NFOUserInputs
            _settings.InstrumentsData = Nothing
            ValidateFile()
            SaveSettings()
            Me.Close()
        Catch ex As Exception
            MsgBox(String.Format("The following error occurred: {0}", ex.Message), MsgBoxStyle.Critical)
        End Try
    End Sub
    Private Sub LoadSettings()
        If File.Exists(_settingsFilename) Then
            _settings = Utilities.Strings.DeserializeToCollection(Of NFOUserInputs)(_settingsFilename)
            txtInstrumentDetalis.Text = _settings.InstrumentDetailsFilePath
            dtpckrStartTime.Value = _settings.StartTime
            dtpckEndTime.Value = _settings.EndTime
            rdbTickBased.Checked = _settings.TickBased
            rdbMinuteBased.Checked = _settings.MinuteBased
        End If
    End Sub
    Private Sub SaveSettings()
        _settings.InstrumentDetailsFilePath = txtInstrumentDetalis.Text
        _settings.StartTime = dtpckrStartTime.Value
        _settings.EndTime = dtpckEndTime.Value
        _settings.TickBased = rdbTickBased.Checked
        _settings.MinuteBased = rdbMinuteBased.Checked

        Utilities.Strings.SerializeFromCollection(Of NFOUserInputs)(_settingsFilename, _settings)
    End Sub

    Private Sub ValidateFile()
        _settings.FillInstrumentDetails(txtInstrumentDetalis.Text, _cts)
    End Sub

    Private Sub btnBrowse_Click(sender As Object, e As EventArgs) Handles btnBrowse.Click
        opnFileSettings.Filter = "|*.xlsx"
        opnFileSettings.ShowDialog()
    End Sub

    Private Sub opnFileSettings_FileOk(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles opnFileSettings.FileOk
        Dim extension As String = Path.GetExtension(opnFileSettings.FileName)
        If extension = ".xlsx" Then
            txtInstrumentDetalis.Text = opnFileSettings.FileName
        Else
            MsgBox("File Type not supported. Please Try again.", MsgBoxStyle.Critical)
        End If
    End Sub
End Class