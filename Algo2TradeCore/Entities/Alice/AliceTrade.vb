Imports AliceBlueClient

Namespace Entities
    Public Class AliceTrade
        Implements ITrade
        Public Property TradeIdentifier As String Implements ITrade.TradeIdentifier
        Public Property WrappedTrade As Trade
        Public ReadOnly Property Broker As APISource Implements ITrade.Broker
            Get
                Return APISource.AliceBlue
            End Get
        End Property
    End Class
End Namespace
