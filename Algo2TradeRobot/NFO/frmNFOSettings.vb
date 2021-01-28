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
        chkbAutoSelectStock_CheckedChanged(Nothing, Nothing)
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
            dtpckrTradeEntryTime.Value = _settings.TradeEntryTime
            nmrcActiveInstrumentCount.Value = _settings.ActiveInstrumentCount

            nmrcATRPeriod.Value = _settings.ATRPeriod
            nmrcPivotPeriod.Value = _settings.PivotPeriod
            nmrcPivotTrendPeriod.Value = _settings.PivotTrendPeriod

            txtInstrumentDetalis.Text = _settings.InstrumentDetailsFilepath
            chkbAutoSelectStock.Checked = _settings.AutoSelectStock
            txtMinimumStockPrice.Text = _settings.MinimumStockPrice
            txtMaximumStockPrice.Text = _settings.MaximumStockPrice
            txtMinimumVolume.Text = _settings.MinimumVolume
            txtMinimumATR.Text = _settings.MinimumATRPercentage
        End If
    End Sub

    Private Sub SaveSettings()
        _settings.SignalTimeFrame = 1
        _settings.TradeEntryTime = dtpckrTradeEntryTime.Value
        _settings.ActiveInstrumentCount = nmrcActiveInstrumentCount.Value

        _settings.ATRPeriod = nmrcATRPeriod.Value
        _settings.PivotPeriod = nmrcPivotPeriod.Value
        _settings.PivotTrendPeriod = nmrcPivotTrendPeriod.Value

        _settings.InstrumentDetailsFilepath = txtInstrumentDetalis.Text
        _settings.AutoSelectStock = chkbAutoSelectStock.Checked
        _settings.MinimumStockPrice = txtMinimumStockPrice.Text
        _settings.MaximumStockPrice = txtMaximumStockPrice.Text
        _settings.MinimumVolume = txtMinimumVolume.Text
        _settings.MinimumATRPercentage = txtMinimumATR.Text

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
        If Not ret Then
            If endNumber = Decimal.MaxValue OrElse (endNumber = Integer.MaxValue AndAlso validateInteger) Then
                Throw New ApplicationException(String.Format("{0} cannot have a value < {1}", inputTB.Tag, startNumber))
            Else
                Throw New ApplicationException(String.Format("{0} cannot have a value < {1} or > {2}", inputTB.Tag, startNumber, endNumber))
            End If
        End If
        Return ret
    End Function

    Private Sub ValidateFile()
        _settings.FillInstrumentDetails(txtInstrumentDetalis.Text, _cts)
    End Sub

    Private Sub ValidateInputs()
        ValidateNumbers(1, Decimal.MaxValue, txtMinimumStockPrice)
        ValidateNumbers(1, Decimal.MaxValue, txtMaximumStockPrice)
        ValidateNumbers(1, Decimal.MaxValue, txtMinimumATR)
        ValidateNumbers(1, Long.MaxValue, txtMinimumVolume)

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

    Private Sub chkbAutoSelectStock_CheckedChanged(sender As Object, e As EventArgs) Handles chkbAutoSelectStock.CheckedChanged
        pnlStockSelectionDetails.Enabled = chkbAutoSelectStock.Checked
    End Sub
End Class