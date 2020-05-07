Namespace Entities
    Public Class AliceConnection
        Implements IConnection

        Public Property RequestToken As String Implements IConnection.RequestToken
        Public ReadOnly Property AccessToken As String Implements IConnection.AccessToken
            Get
                If AliceUser IsNot Nothing AndAlso CType(AliceUser, AliceUser).WrappedUser.AccessToken IsNot Nothing Then
                    Return CType(AliceUser, AliceUser).WrappedUser.AccessToken
                Else
                    Return Nothing
                End If
            End Get
        End Property
        Public ReadOnly Property PublicToken As String Implements IConnection.PublicToken
            Get
                'If AliceUser IsNot Nothing AndAlso CType(AliceUser, AliceUser).WrappedUser.PublicToken IsNot Nothing Then
                '    Return CType(AliceUser, AliceUser).WrappedUser.PublicToken
                'Else
                Return Nothing
                'End If
            End Get
        End Property
        Public Property AliceUser As IUser Implements IConnection.APIUser

        Public ReadOnly Property Broker As APISource Implements IConnection.Broker
            Get
                Return APISource.AliceBlue
            End Get
        End Property

        Public Property ENCToken As String Implements IConnection.ENCToken
    End Class
End Namespace