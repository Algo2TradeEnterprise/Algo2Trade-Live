Public Class frmUpdateSignal
    Private ReadOnly _strategyInstrument As NFOStrategyInstrument

    Public Sub New(ByVal runningInstrument As NFOStrategyInstrument)
        InitializeComponent()
        _strategyInstrument = runningInstrument
    End Sub

    Private Sub frmUpdateSignal_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If _strategyInstrument IsNot Nothing Then
            Me.Text = String.Format("Update Signal - {0}", _strategyInstrument.TradableInstrument.TradingSymbol.ToUpper)

            'dtpckrSignalTime.Format = DateTimePickerFormat.Custom
            'dtpckrSignalTime.CustomFormat = "dd-MMM-yyyy"
            dtpckrSignalTime.Value = Now

            Dim lastSignal As NFOStrategyInstrument.SignalDetails = _strategyInstrument.GetSignalDetails()
            If lastSignal IsNot Nothing Then
                dtpckrSignalTime.Value = lastSignal.SignalTime
                txtEntryPrice.Text = lastSignal.EntryPrice
                txtQuantity.Text = lastSignal.Quantity
                txtIterationNumber.Text = lastSignal.IterationNumber
                txtEntryRason.Text = lastSignal.EntryReason
            End If
        End If
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Try
            ValidateInputs()
            Dim entryReason As String = txtEntryRason.Text
            If entryReason Is Nothing OrElse entryReason.Trim = "" Then
                entryReason = "(Manual Update)"
            Else
                entryReason = String.Format("(Manual Update) {0}", entryReason)
            End If
            _strategyInstrument.SetSignalDetails(dtpckrSignalTime.Value, txtEntryPrice.Text, entryReason, txtQuantity.Text, txtIterationNumber.Text)
            Me.Close()
        Catch ex As Exception
            MsgBox(String.Format("The following error occurred: {0}", ex.Message), MsgBoxStyle.Critical)
        End Try
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

    Private Sub ValidateInputs()
        ValidateNumbers(0, Decimal.MaxValue, txtEntryPrice)
        ValidateNumbers(1, Integer.MaxValue, txtQuantity, True)
        ValidateNumbers(1, Integer.MaxValue, txtIterationNumber, True)
    End Sub
End Class