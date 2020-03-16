Imports System.Threading
Imports System.IO

Public Class frmLowSLSettings
    Private _cts As CancellationTokenSource = Nothing
    Private _settings As LowSLUserInputs = Nothing
    Private _settingsFilename As String = Path.Combine(My.Application.Info.DirectoryPath, "LowSLSettings.Strategy.a2t")

    Public Sub New(ByRef userInputs As LowSLUserInputs)
        InitializeComponent()
        _settings = userInputs
    End Sub

    Private Sub frmLowSLSettings_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadSettings()
    End Sub

    Private Sub btnLowSLSettings_Click(sender As Object, e As EventArgs) Handles btnLowSLStrategySettings.Click
        Try
            _cts = New CancellationTokenSource
            If _settings Is Nothing Then _settings = New LowSLUserInputs
            _settings.InstrumentsData = Nothing
            ValidateInputs()
            SaveSettings()
            Me.Close()
        Catch ex As Exception
            MsgBox(String.Format("The following error occurred: {0}", ex.Message), MsgBoxStyle.Critical)
        End Try
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

    Private Sub LoadSettings()
        If File.Exists(_settingsFilename) Then
            _settings = Utilities.Strings.DeserializeToCollection(Of LowSLUserInputs)(_settingsFilename)

            txtSignalTimeFrame.Text = _settings.SignalTimeFrame
            dtpckrTradeStartTime.Value = _settings.TradeStartTime
            dtpckrLastTradeEntryTime.Value = _settings.LastTradeEntryTime
            dtpckrEODExitTime.Value = _settings.EODExitTime
            txtTargetMultiplier.Text = _settings.TargetMultiplier
            txtMinInvestmentPerStock.Text = _settings.MinInvestmentPerStock
            txtMinLossPerTrade.Text = _settings.MinStoplossPerTrade
            txtMaxLossPerTrade.Text = _settings.MaxStoplossPerTrade
            txtNumberOfTradePerStock.Text = _settings.NumberOfTradePerStock
            txtStockMaxLossPerDay.Text = _settings.StockMaxLossPerDay
            txtStockMaxProfitPerDay.Text = _settings.StockMaxProfitPerDay
            txtOverallMaxLossPerDay.Text = _settings.OverallMaxLossPerDay
            txtOverallMaxProfitPerDay.Text = _settings.OverallMaxProfitPerDay
            txtInstrumentDetalis.Text = _settings.InstrumentDetailsFilePath

            chbAutoSelectStock.Checked = _settings.AutoSelectStock
            rdbCash.Checked = _settings.CashInstrument
            rdbFuture.Checked = _settings.FutureInstrument

            txtMinPrice.Text = _settings.MinPrice
            txtMaxPrice.Text = _settings.MaxPrice
            txtATRPercentage.Text = _settings.MinATRPercentage
            txtNumberOfStock.Text = _settings.NumberOfStock
            txtMaxBlankCandlePer.Text = _settings.MaxBlankCandlePercentage
        End If
    End Sub

    Private Sub SaveSettings()
        _settings.SignalTimeFrame = txtSignalTimeFrame.Text
        _settings.TradeStartTime = dtpckrTradeStartTime.Value
        _settings.LastTradeEntryTime = dtpckrLastTradeEntryTime.Value
        _settings.EODExitTime = dtpckrEODExitTime.Value
        _settings.TargetMultiplier = txtTargetMultiplier.Text
        _settings.MinInvestmentPerStock = txtMinInvestmentPerStock.Text
        _settings.MinStoplossPerTrade = Math.Abs(CDec(txtMinLossPerTrade.Text)) * -1
        _settings.MaxStoplossPerTrade = Math.Abs(CDec(txtMaxLossPerTrade.Text)) * -1
        _settings.NumberOfTradePerStock = txtNumberOfTradePerStock.Text
        _settings.StockMaxLossPerDay = Math.Abs(CDec(txtStockMaxLossPerDay.Text)) * -1
        _settings.StockMaxProfitPerDay = txtStockMaxProfitPerDay.Text
        _settings.OverallMaxLossPerDay = Math.Abs(CDec(txtOverallMaxLossPerDay.Text)) * -1
        _settings.OverallMaxProfitPerDay = txtOverallMaxProfitPerDay.Text
        _settings.InstrumentDetailsFilePath = txtInstrumentDetalis.Text

        _settings.AutoSelectStock = chbAutoSelectStock.Checked
        _settings.CashInstrument = rdbCash.Checked
        _settings.FutureInstrument = rdbFuture.Checked

        _settings.MinPrice = txtMinPrice.Text
        _settings.MaxPrice = txtMaxPrice.Text
        _settings.MinATRPercentage = txtATRPercentage.Text
        _settings.NumberOfStock = txtNumberOfStock.Text
        _settings.MaxBlankCandlePercentage = txtMaxBlankCandlePer.Text

        Utilities.Strings.SerializeFromCollection(Of LowSLUserInputs)(_settingsFilename, _settings)
    End Sub

    Private Function ValidateNumbers(ByVal startNumber As Decimal, ByVal endNumber As Decimal, ByVal inputTB As TextBox) As Boolean
        Dim ret As Boolean = False
        If IsNumeric(inputTB.Text) Then
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
        ValidateNumbers(1, 60, txtSignalTimeFrame)
        ValidateFile()
    End Sub
End Class