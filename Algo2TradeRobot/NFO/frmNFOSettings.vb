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
        chkbAutoSelectStock_CheckedChanged(sender, e)
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
            dtpckrTradeStartTime.Value = _settings.TradeStartTime
            dtpckrLastTradeEntryTime.Value = _settings.LastTradeEntryTime
            dtpckrEODExitTime.Value = _settings.EODExitTime
            txtMaxProfitPerTrade.Text = _settings.MaxLossPercentagePerDay
            txtNumberOfTradePerStock.Text = _settings.NumberOfTradePerStock
            txtOverallMaxLossPerDay.Text = _settings.OverallMaxLossPerDay
            txtOverallMaxProfitPerDay.Text = _settings.OverallMaxProfitPerDay
            txtInstrumentDetalis.Text = _settings.InstrumentDetailsFilePath

            chkbAutoSelectStock.Checked = _settings.AutoSelectStock
            txtMinPrice.Text = _settings.MinStockPrice
            txtMaxPrice.Text = _settings.MaxStockPrice
            txtATRPercentage.Text = _settings.MinATRPercentage
            txtMaxBlankCandlePer.Text = _settings.MaxBlankCandlePercentage
            txtNumberOfStock.Text = _settings.NumberOfStock
        End If
    End Sub
    Private Sub SaveSettings()
        _settings.SignalTimeFrame = txtSignalTimeFrame.Text
        _settings.TradeStartTime = dtpckrTradeStartTime.Value
        _settings.LastTradeEntryTime = dtpckrLastTradeEntryTime.Value
        _settings.EODExitTime = dtpckrEODExitTime.Value
        _settings.MaxLossPercentagePerDay = Math.Abs(CDec(txtMaxProfitPerTrade.Text)) * -1
        _settings.NumberOfTradePerStock = txtNumberOfTradePerStock.Text
        _settings.OverallMaxLossPerDay = Math.Abs(CDec(txtOverallMaxLossPerDay.Text)) * -1
        _settings.OverallMaxProfitPerDay = Math.Abs(CDec(txtOverallMaxProfitPerDay.Text))
        _settings.InstrumentDetailsFilePath = txtInstrumentDetalis.Text

        _settings.AutoSelectStock = chkbAutoSelectStock.Checked
        _settings.MinStockPrice = txtMinPrice.Text
        _settings.MaxStockPrice = txtMaxPrice.Text
        _settings.MinATRPercentage = txtATRPercentage.Text
        _settings.MaxBlankCandlePercentage = txtMaxBlankCandlePer.Text
        _settings.NumberOfStock = txtNumberOfStock.Text

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
        ValidateNumbers(1, 60, txtSignalTimeFrame, True)

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
        If chkbAutoSelectStock.Checked Then
            grpStockSelection.Enabled = True
        Else
            grpStockSelection.Enabled = False
        End If
    End Sub
End Class