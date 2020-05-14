Imports System.IO
Imports Algo2TradeCore.Entities
Imports Algo2TradeCore.Entities.UserSettings
Imports Utilities.Strings.StringManipulation
Public Class frmAliceUserDetails

    Private _UserInputs As ControllerUserInputs

    Public Sub New(ByVal userInputs As ControllerUserInputs)
        InitializeComponent()
        Me._UserInputs = userInputs
    End Sub

    Private Sub frmAliceUserDetails_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If _UserInputs IsNot Nothing AndAlso _UserInputs.UserDetails IsNot Nothing Then
            txtAliceUserId.Text = _UserInputs.UserDetails.UserId
            txtAlicePassword.Text = Decrypt(_UserInputs.UserDetails.Password, Common.MASTER_KEY)
            txtAlice2FAAnswer.Text = Decrypt(_UserInputs.UserDetails.API2FAPin, Common.MASTER_KEY)
            txtAliceAPISecret.Text = Decrypt(_UserInputs.UserDetails.APISecret, Common.MASTER_KEY)
        End If
    End Sub

    Private Sub btnSaveAliceUserDetails_Click(sender As Object, e As EventArgs) Handles btnSaveAliceUserDetails.Click
        Try
            ValidateAll()
            SaveUserDetails()
            Me.Close()
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub SaveUserDetails()
        If _UserInputs Is Nothing Then _UserInputs = New ControllerUserInputs
        If _UserInputs.UserDetails Is Nothing Then _UserInputs.UserDetails = New AliceUser
        _UserInputs.UserDetails.UserId = txtAliceUserId.Text
        _UserInputs.UserDetails.Password = Encrypt(txtAlicePassword.Text, Common.MASTER_KEY)
        _UserInputs.UserDetails.API2FAPin = Encrypt(txtAlice2FAAnswer.Text, Common.MASTER_KEY)
        _UserInputs.UserDetails.APISecret = Encrypt(txtAliceAPISecret.Text, Common.MASTER_KEY)
        Utilities.Strings.SerializeFromCollection(Of ControllerUserInputs)(ControllerUserInputs.Filename, _UserInputs)
    End Sub

    Private Sub ValidateAll()
        ValidateTextLength(txtAliceUserId, 1, "User Id")
        ValidateTextLength(txtAlicePassword, 1, "Password")
        ValidateTextLength(txtAlice2FAAnswer, 1, "2FA Answer")
        ValidateTextLength(txtAliceAPISecret, 1, "API Secret")
    End Sub

    Private Sub ValidateTextLength(ByVal txtControl As TextBox, ByVal minLength As Integer, ByVal friendlyNameOfContents As String)
        If txtControl.Text Is Nothing OrElse txtControl.Text.Trim.Count = 0 Then
            Throw New ApplicationException(String.Format("{0} cannot be blank", friendlyNameOfContents))
        ElseIf txtControl.Text IsNot Nothing AndAlso txtControl.Text.Trim.Count < minLength Then
            Throw New ApplicationException(String.Format("{0} cannpot have less than {1} characters", friendlyNameOfContents, minLength))
        End If
    End Sub
End Class