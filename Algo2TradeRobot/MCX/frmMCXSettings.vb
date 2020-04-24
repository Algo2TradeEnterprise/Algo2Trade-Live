Imports System.IO
Imports System.Threading

Public Class frmMCXSettings

    Private _cts As CancellationTokenSource = Nothing
    Private _settings As MCXUserInputs = Nothing
    Private _settingsFilename As String = MCXUserInputs.SettingsFileName

    Public Sub New(ByRef userInputs As MCXUserInputs)
        InitializeComponent()
        _settings = userInputs
    End Sub

    Private Sub frmMCXSettings_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadSettings()
    End Sub
    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Try
            _cts = New CancellationTokenSource
            If _settings Is Nothing Then _settings = New MCXUserInputs
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
            _settings = Utilities.Strings.DeserializeToCollection(Of MCXUserInputs)(_settingsFilename)
            txtSignalTimeFrame.Text = _settings.SignalTimeFrame
            txtHigherTimeframe.Text = _settings.HigherTimeframe
            dtpckrTradeStartTime.Value = _settings.TradeStartTime
            dtpckrLastTradeEntryTime.Value = _settings.LastTradeEntryTime
            dtpckrEODExitTime.Value = _settings.EODExitTime
            txtInstrumentDetalis.Text = _settings.InstrumentDetailsFilePath

            txtSupertrendPeriod.Text = _settings.Period
            txtSupertrendMultiplier.Text = _settings.Multiplier
        End If
    End Sub
    Private Sub SaveSettings()
        _settings.SignalTimeFrame = txtSignalTimeFrame.Text
        _settings.HigherTimeframe = txtHigherTimeframe.Text
        _settings.TradeStartTime = dtpckrTradeStartTime.Value
        _settings.LastTradeEntryTime = dtpckrLastTradeEntryTime.Value
        _settings.EODExitTime = dtpckrEODExitTime.Value
        _settings.InstrumentDetailsFilePath = txtInstrumentDetalis.Text

        _settings.Period = txtSupertrendPeriod.Text
        _settings.Multiplier = txtSupertrendMultiplier.Text

        Utilities.Strings.SerializeFromCollection(Of MCXUserInputs)(_settingsFilename, _settings)
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
    Private Sub ValidateFile()
        _settings.FillInstrumentDetails(txtInstrumentDetalis.Text, _cts)
    End Sub
    Private Sub ValidateInputs()
        ValidateNumbers(1, 60, txtSignalTimeFrame, True)
        ValidateNumbers(1, 180, txtHigherTimeframe, True)
        ValidateNumbers(0, Integer.MaxValue, txtSupertrendPeriod, True)
        ValidateNumbers(0, Decimal.MaxValue, txtSupertrendMultiplier, False)

        If Val(txtHigherTimeframe.Text) <= Val(txtSignalTimeFrame.Text) Then
            Throw New ApplicationException("Higher timeframe can not be lower than or equal to Signal timeframe")
        End If

        ValidateFile()
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