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
        chbCalculateQuantityFromCapital_CheckedChanged(sender, e)
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
            dtpckrTradeStartTime.Value = _settings.TradeStartTime
            dtpckrEODExitTime.Value = _settings.EODExitTime
            txtStoplossTrailingPercentage.Text = _settings.StoplossTrailingPercentage
            txtBidAskRatio.Text = _settings.BidAskRatio
            txtHardClosePercentage.Text = _settings.HardClosePercentage
            chbCalculateQuantityFromCapital.Checked = _settings.CalculateQuantityFromCapital
            txtCapital.Text = _settings.Capital
            txtMargin.Text = _settings.MarginMultiplier
            txtQuantity.Text = _settings.Quantity
        End If
    End Sub

    Private Sub SaveSettings()
        _settings.TradeStartTime = dtpckrTradeStartTime.Value
        _settings.EODExitTime = dtpckrEODExitTime.Value
        _settings.StoplossTrailingPercentage = txtStoplossTrailingPercentage.Text
        _settings.BidAskRatio = txtBidAskRatio.Text
        _settings.HardClosePercentage = txtHardClosePercentage.Text
        _settings.CalculateQuantityFromCapital = chbCalculateQuantityFromCapital.Checked
        _settings.Capital = txtCapital.Text
        _settings.MarginMultiplier = txtMargin.Text
        _settings.Quantity = txtQuantity.Text

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
        ValidateNumbers(0.00000001, 100, txtStoplossTrailingPercentage)
        ValidateNumbers(0.00000001, Decimal.MaxValue, txtBidAskRatio)
        ValidateNumbers(0.00000001, 100, txtHardClosePercentage)
        ValidateNumbers(1, Decimal.MaxValue, txtCapital)
        ValidateNumbers(1, Decimal.MaxValue, txtMargin)
        ValidateNumbers(1, Integer.MaxValue, txtQuantity, True)
    End Sub

    Private Sub chbCalculateQuantityFromCapital_CheckedChanged(sender As Object, e As EventArgs) Handles chbCalculateQuantityFromCapital.CheckedChanged
        If chbCalculateQuantityFromCapital.Checked Then
            lblCapital.Visible = True
            txtCapital.Visible = True
            lblMargin.Visible = True
            txtMargin.Visible = True
            lblQuantity.Visible = False
            txtQuantity.Visible = False
        Else
            lblCapital.Visible = False
            txtCapital.Visible = False
            lblMargin.Visible = False
            txtMargin.Visible = False
            lblQuantity.Visible = True
            txtQuantity.Visible = True
        End If
    End Sub
End Class