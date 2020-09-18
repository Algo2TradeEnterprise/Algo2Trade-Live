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
        txtNumberOfStockToTrade.Text = 1
        txtNumberOfStockToTrade.Enabled = False
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
            dtpckrTradeStartTime.Value = _settings.TradeStartTime
            dtpckrLastTradeEntryTime.Value = _settings.LastTradeEntryTime
            dtpckrEODExitTime.Value = _settings.EODExitTime
            'txtNumberOfStockToTrade.Text = _settings.NumberOfStockToTrade
            txtMinTurnoverPerTrade.Text = _settings.MinTurnoverPerTrade
            txtMaxTurnoverPerTrade.Text = _settings.MaxTurnoverPerTrade
            txtMaxProfitPerStock.Text = _settings.MaxProfitPerStock
            txtMaxFractalDiffPer.Text = _settings.MaxFractalDifferencePercentage
            txtMaxStrikeRangePer.Text = _settings.MaxStrikeRangePercentage

            If _settings.StockList IsNot Nothing And _settings.StockList.Count > 0 Then
                Dim stocks As String = Nothing
                For Each runningStock In _settings.StockList
                    stocks = String.Format("{0},{1}", stocks, runningStock.Trim.ToUpper)
                Next

                txtStockList.Text = stocks.Substring(1)
            End If
            dtpckrLastEntryTime.Value = _settings.LastEntryTime
            txtMinVolumePer.Text = _settings.MinVolumePercentage
            txtMaxBlankCandlePer.Text = _settings.MaxBlankCandlePercentage
        End If
    End Sub

    Private Sub SaveSettings()
        _settings.SignalTimeFrame = txtSignalTimeFrame.Text
        _settings.TradeStartTime = dtpckrTradeStartTime.Value
        _settings.LastTradeEntryTime = dtpckrLastTradeEntryTime.Value
        _settings.EODExitTime = dtpckrEODExitTime.Value
        _settings.NumberOfStockToTrade = txtNumberOfStockToTrade.Text
        _settings.MinTurnoverPerTrade = txtMinTurnoverPerTrade.Text
        _settings.MaxTurnoverPerTrade = txtMaxTurnoverPerTrade.Text
        _settings.MaxProfitPerStock = Math.Abs(CDec(txtMaxProfitPerStock.Text))
        _settings.MaxFractalDifferencePercentage = txtMaxFractalDiffPer.Text
        _settings.MaxStrikeRangePercentage = txtMaxStrikeRangePer.Text

        If txtStockList.Text IsNot Nothing And txtStockList.Text.Count > 0 Then
            Dim stocks() As String = txtStockList.Text.Trim.Split(",")
            If stocks IsNot Nothing AndAlso stocks.Count > 0 Then
                _settings.StockList = New List(Of String)
                For i = 0 To stocks.Count - 1
                    _settings.StockList.Add(stocks(i).Trim.ToUpper)
                Next
            End If
        End If
        _settings.LastEntryTime = dtpckrLastEntryTime.Value
        _settings.MinVolumePercentage = txtMinVolumePer.Text
        _settings.MaxBlankCandlePercentage = txtMaxBlankCandlePer.Text

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
        ValidateNumbers(1, Integer.MaxValue, txtNumberOfStockToTrade, True)
        ValidateNumbers(1, Decimal.MaxValue, txtMinTurnoverPerTrade)
        ValidateNumbers(1, Decimal.MaxValue, txtMaxTurnoverPerTrade)
        ValidateNumbers(1, Decimal.MaxValue, txtMaxProfitPerStock)
        ValidateNumbers(1, Decimal.MaxValue, txtMaxFractalDiffPer)
        ValidateNumbers(1, Decimal.MaxValue, txtMaxStrikeRangePer)

        ValidateNumbers(1, Decimal.MaxValue, txtMinVolumePer)
        ValidateNumbers(1, Decimal.MaxValue, txtMaxBlankCandlePer)
    End Sub

End Class