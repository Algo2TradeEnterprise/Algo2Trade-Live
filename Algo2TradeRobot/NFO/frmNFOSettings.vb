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

        Dim itemValues As Array = System.Enum.GetValues(GetType(NFOUserInputs.TypeOfRanges))
        Dim itemNames As Array = System.Enum.GetNames(GetType(NFOUserInputs.TypeOfRanges))

        For i As Integer = 0 To itemNames.Length - 1
            Dim name As String = itemNames(i)
            If name.StartsWith("C_") Then
                name = name.Substring(2)
            End If
            name = name.Replace("_", " ")
            Dim item As KeyValuePair(Of String, NFOUserInputs.TypeOfRanges) = New KeyValuePair(Of String, NFOUserInputs.TypeOfRanges)(name, itemValues(i))
            cmbRangeBreakout.Items.Add(item)
        Next
        cmbRangeBreakout.DisplayMember = "Key"
        cmbRangeBreakout.ValueMember = "Value"

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

            cmbRangeBreakout.SelectedIndex = GetIndex(cmbRangeBreakout, _settings.RangeType)
            dtpckrEODExitTime.Value = _settings.EODExitTime
            cmbNumberOfTradePerStock.SelectedIndex = GetIndex(cmbNumberOfTradePerStock, _settings.NumberOfTradePerStock)
            txtRangeStoplossPercentage.Text = _settings.RangeStoplossPercentage
            txtMTMProfit.Text = _settings.MTMProfit
            txtMTMLoss.Text = _settings.MTMLoss
            txtInstrumentDetalis.Text = _settings.InstrumentDetailsFilePath
        End If
    End Sub

    Private Function GetIndex(ByVal cmbBox As ComboBox, ByVal value As Object) As Integer
        Dim ret As Integer = -1
        If cmbBox IsNot Nothing AndAlso cmbBox.Items IsNot Nothing AndAlso cmbBox.Items.Count > 0 Then
            For index As Integer = 0 To cmbBox.Items.Count - 1
                If value.GetType = GetType(Integer) Then
                    If cmbBox.Items(index) = value Then
                        ret = index
                        Exit For
                    End If
                Else
                    If cmbBox.Items(index).Value = value Then
                        ret = index
                        Exit For
                    End If
                End If
            Next
        End If
        Return ret
    End Function

    Private Sub SaveSettings()
        _settings.RangeType = cmbRangeBreakout.SelectedItem.Value
        _settings.EODExitTime = dtpckrEODExitTime.Value
        _settings.NumberOfTradePerStock = cmbNumberOfTradePerStock.SelectedItem
        _settings.RangeStoplossPercentage = txtRangeStoplossPercentage.Text
        _settings.MTMProfit = Math.Abs(Val(txtMTMProfit.Text))
        _settings.MTMLoss = Math.Abs(Val(txtMTMLoss.Text)) * -1
        _settings.InstrumentDetailsFilePath = txtInstrumentDetalis.Text

        Select Case _settings.RangeType
            Case NFOUserInputs.TypeOfRanges.C_1_Minute
                _settings.SignalTimeFrame = 1
            Case NFOUserInputs.TypeOfRanges.C_2_Minute
                _settings.SignalTimeFrame = 2
            Case NFOUserInputs.TypeOfRanges.C_3_Minute
                _settings.SignalTimeFrame = 3
            Case NFOUserInputs.TypeOfRanges.C_4_Minute
                _settings.SignalTimeFrame = 4
            Case NFOUserInputs.TypeOfRanges.C_5_Minute
                _settings.SignalTimeFrame = 5
            Case NFOUserInputs.TypeOfRanges.C_10_Minute
                _settings.SignalTimeFrame = 10
            Case NFOUserInputs.TypeOfRanges.C_15_Minute
                _settings.SignalTimeFrame = 15
            Case NFOUserInputs.TypeOfRanges.C_30_Minute
                _settings.SignalTimeFrame = 30
            Case NFOUserInputs.TypeOfRanges.C_60_Minute
                _settings.SignalTimeFrame = 60
            Case NFOUserInputs.TypeOfRanges.Previous_Day
                _settings.SignalTimeFrame = 1
            Case Else
                Throw New NotImplementedException
        End Select

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
        ValidateNumbers(0, 100, txtRangeStoplossPercentage)
        ValidateNumbers(0, Decimal.MaxValue, txtMTMProfit)
        ValidateNumbers(Decimal.MinValue, Decimal.MaxValue, txtMTMLoss)

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