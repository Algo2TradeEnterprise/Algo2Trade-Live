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
            txtLowerTimeFrame.Text = _settings.SignalTimeFrame
            txtHigherTimeFrame.Text = _settings.HigherTimeframe
            txtStrikeRangePer.Text = _settings.StrikePriceRangePercentage

            txtInstrumentDetalis.Text = _settings.InstrumentDetailsFilePath

            txtLTEMA1Period.Text = _settings.LTEMA1Period
            txtLTEMA2Period.Text = _settings.LTEMA2Period
            txtHTEMA1Period.Text = _settings.HTEMA1Period
            txtHTEMA2Period.Text = _settings.HTEMA2Period
        End If
    End Sub

    Private Sub SaveSettings()
        _settings.SignalTimeFrame = txtLowerTimeFrame.Text
        _settings.HigherTimeframe = txtHigherTimeFrame.Text
        _settings.StrikePriceRangePercentage = txtStrikeRangePer.Text

        _settings.InstrumentDetailsFilePath = txtInstrumentDetalis.Text

        _settings.LTEMA1Period = txtLTEMA1Period.Text
        _settings.LTEMA2Period = txtLTEMA2Period.Text
        _settings.HTEMA1Period = txtHTEMA1Period.Text
        _settings.HTEMA2Period = txtHTEMA2Period.Text


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
    Private Sub ValidateFile()
        _settings.FillInstrumentDetails(txtInstrumentDetalis.Text, _cts)
    End Sub
    Private Sub ValidateInputs()
        ValidateNumbers(1, 240, txtLowerTimeFrame, True)
        ValidateNumbers(0, Decimal.MaxValue, txtStrikeRangePer)

        ValidateNumbers(1, Integer.MaxValue, txtLTEMA1Period, True)
        ValidateNumbers(1, Integer.MaxValue, txtLTEMA2Period, True)

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