Imports Algo2TradeCore.Entities.UserSettings
Imports Algo2TradeCore.Entities

Public Class frmAdvancedOptions

    Private _UserInputs As ControllerUserInputs
    Private _toolRunning As Boolean = False

    Public Sub New(ByVal userInputs As ControllerUserInputs, ByVal toolRunning As Boolean)
        InitializeComponent()
        Me._UserInputs = userInputs
        _toolRunning = toolRunning
    End Sub

    Private Sub frmAdvancedOptions_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If _toolRunning Then btnSaveSettings.Enabled = False
        LoadSettings()
    End Sub

    Private Sub btnSaveSettings_Click(sender As Object, e As EventArgs) Handles btnSaveSettings.Click
        Try
            ValidateInputs()
            SaveSettings()
            Me.Close()
        Catch ex As Exception
            MsgBox(String.Format("The following error occurred: {0}", ex.Message), MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub LoadSettings()
        If _UserInputs IsNot Nothing Then
            txtGetInformationDelay.Text = _UserInputs.GetInformationDelay
            txtBackToBackOrderCoolOffDelay.Text = _UserInputs.BackToBackOrderCoolOffDelay
            dtpckrForceRestartTime.Value = _UserInputs.ForceRestartTime
            dtpckrDeadStateStartTime.Value = _UserInputs.IdleStateStartTime
            dtpckrDeadStateEndTime.Value = _UserInputs.IdleStateEndTime
            If _UserInputs.TradingDays IsNot Nothing AndAlso _UserInputs.TradingDays.Count > 0 Then
                For Each runningTradingday In _UserInputs.TradingDays
                    Dim index As Integer = 0
                    For Each runningCheckBox In chkbLstTradingDays.Items
                        If runningCheckBox.ToString.ToUpper = runningTradingday.ToString.ToUpper Then
                            chkbLstTradingDays.SetItemChecked(index, True)
                            Exit For
                        End If
                        index += 1
                    Next
                Next
            End If
            If _UserInputs.ExchangeDetails IsNot Nothing Then
                dtpckrNSEExchangeStartTime.Value = _UserInputs.ExchangeDetails("NSE").ExchangeStartTime
                dtpckrNSEExchangeEndTime.Value = _UserInputs.ExchangeDetails("NSE").ExchangeEndTime
                dtpckrNSEContractRolloverTime.Value = _UserInputs.ExchangeDetails("NSE").ContractRolloverTime

                dtpckrMCXExchangeStartTime.Value = _UserInputs.ExchangeDetails("MCX").ExchangeStartTime
                dtpckrMCXExchangeEndTime.Value = _UserInputs.ExchangeDetails("MCX").ExchangeEndTime
                dtpckrMCXContractRolloverTime.Value = _UserInputs.ExchangeDetails("MCX").ContractRolloverTime

                dtpckrCDSExchangeStartTime.Value = _UserInputs.ExchangeDetails("CDS").ExchangeStartTime
                dtpckrCDSExchangeEndTime.Value = _UserInputs.ExchangeDetails("CDS").ExchangeEndTime
                dtpckrCDSContractRolloverTime.Value = _UserInputs.ExchangeDetails("CDS").ContractRolloverTime
            End If
            txtRemarks.Text = _UserInputs.FormRemarks
            txtTelegramAPI.Text = _UserInputs.TelegramAPIKey
            txtTelegramChatID.Text = _UserInputs.TelegramChatID
        End If
    End Sub

    Private Sub SaveSettings()
        If _UserInputs Is Nothing Then _UserInputs = New ControllerUserInputs
        _UserInputs.GetInformationDelay = txtGetInformationDelay.Text
        _UserInputs.BackToBackOrderCoolOffDelay = txtBackToBackOrderCoolOffDelay.Text
        _UserInputs.ForceRestartTime = dtpckrForceRestartTime.Value
        _UserInputs.IdleStateStartTime = dtpckrDeadStateStartTime.Value
        _UserInputs.IdleStateEndTime = dtpckrDeadStateEndTime.Value
        _UserInputs.TradingDays = New List(Of DayOfWeek)
        For Each runningTradingday In chkbLstTradingDays.CheckedItems
            Dim day As DayOfWeek = DayOfWeek.Sunday
            If runningTradingday.ToString.ToUpper = "MONDAY" Then
                day = DayOfWeek.Monday
            ElseIf runningTradingday.ToString.ToUpper = "TUESDAY" Then
                day = DayOfWeek.Tuesday
            ElseIf runningTradingday.ToString.ToUpper = "WEDNESDAY" Then
                day = DayOfWeek.Wednesday
            ElseIf runningTradingday.ToString.ToUpper = "THURSDAY" Then
                day = DayOfWeek.Thursday
            ElseIf runningTradingday.ToString.ToUpper = "FRIDAY" Then
                day = DayOfWeek.Friday
            ElseIf runningTradingday.ToString.ToUpper = "SATURDAY" Then
                day = DayOfWeek.Saturday
            ElseIf runningTradingday.ToString.ToUpper = "SUNDAY" Then
                day = DayOfWeek.Sunday
            End If
            _UserInputs.TradingDays.Add(day)
        Next

        _UserInputs.ExchangeDetails = New Dictionary(Of String, Exchange) From {
            {"NSE", New Exchange(Enums.TypeOfExchage.NSE) With
            {.ExchangeStartTime = dtpckrNSEExchangeStartTime.Value, .ExchangeEndTime = dtpckrNSEExchangeEndTime.Value, .ContractRolloverTime = dtpckrNSEContractRolloverTime.Value}},
            {"NFO", New Exchange(Enums.TypeOfExchage.NSE) With
            {.ExchangeStartTime = dtpckrNSEExchangeStartTime.Value, .ExchangeEndTime = dtpckrNSEExchangeEndTime.Value, .ContractRolloverTime = dtpckrNSEContractRolloverTime.Value}},
            {"MCX", New Exchange(Enums.TypeOfExchage.MCX) With
            {.ExchangeStartTime = dtpckrMCXExchangeStartTime.Value, .ExchangeEndTime = dtpckrMCXExchangeEndTime.Value, .ContractRolloverTime = dtpckrMCXContractRolloverTime.Value}},
            {"CDS", New Exchange(Enums.TypeOfExchage.CDS) With
            {.ExchangeStartTime = dtpckrCDSExchangeStartTime.Value, .ExchangeEndTime = dtpckrCDSExchangeEndTime.Value, .ContractRolloverTime = dtpckrCDSContractRolloverTime.Value}}
        }
        _UserInputs.FormRemarks = txtRemarks.Text
        _UserInputs.TelegramAPIKey = txtTelegramAPI.Text
        _UserInputs.TelegramChatID = txtTelegramChatID.Text
        Utilities.Strings.SerializeFromCollection(Of ControllerUserInputs)(ControllerUserInputs.Filename, _UserInputs)
    End Sub

    Private Sub ValidateInputs()
        ValidateNumbers(1, 1000, txtGetInformationDelay)
        ValidateNumbers(1, 1000, txtBackToBackOrderCoolOffDelay)
        If dtpckrForceRestartTime.Value.Hour = 0 AndAlso dtpckrForceRestartTime.Value.Minute = 0 Then
            Throw New ApplicationException("Force Restart Time can not be blank")
        End If
    End Sub

    Private Function ValidateNumbers(ByVal startNumber As Integer, ByVal endNumber As Integer, ByVal inputTB As TextBox) As Boolean
        Dim ret As Boolean = False
        If IsNumeric(inputTB.Text) Then
            If Val(inputTB.Text) >= startNumber And Val(inputTB.Text) <= endNumber Then
                ret = True
            End If
        End If
        If Not ret Then Throw New ApplicationException(String.Format("{0} cannot have a value < {1} or > {2}", inputTB.Tag, startNumber, endNumber))
        Return ret
    End Function

End Class