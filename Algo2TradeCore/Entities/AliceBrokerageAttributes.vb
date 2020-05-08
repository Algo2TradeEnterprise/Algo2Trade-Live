Namespace Calculator
    Public Class AliceBrokerageAttributes
        Implements IBrokerageAttributes

        Public Property Buy As Decimal Implements IBrokerageAttributes.Buy
        Public Property Sell As Decimal Implements IBrokerageAttributes.Sell
        Public Property Quantity As Integer Implements IBrokerageAttributes.Quantity
        Public Property Turnover As Decimal
        Public Property Brokerage As Decimal
        Public Property STT As Integer
        Public Property CTT As Decimal
        Public Property ExchangeFees As Decimal
        Public Property Clearing As Decimal
        Public Property GST As Decimal
        Public Property SEBI As Decimal
        Public Property TotalTax As Decimal
        Public Property BreakevenPoints As Decimal
        Public Property NetProfitLoss As Decimal Implements IBrokerageAttributes.NetProfitLoss
    End Class
End Namespace
