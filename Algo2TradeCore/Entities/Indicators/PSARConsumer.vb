Imports System.Drawing
Namespace Entities.Indicators
    Public Class PSARConsumer
        Inherits PayloadToIndicatorConsumer
        Public ReadOnly Property MinimumAF As Decimal
        Public ReadOnly Property MaximumAF As Decimal

        Public Sub New(ByVal associatedParentConsumer As IPayloadConsumer, ByVal minimumAF As Decimal, ByVal maximumAF As Decimal)
            MyBase.New(associatedParentConsumer)
            Me.MinimumAF = minimumAF
            Me.MaximumAF = maximumAF
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format("{0}_{1}({2},{3})", Me.ParentConsumer.ToString, Me.GetType.Name, Me.MinimumAF, Me.MaximumAF)
        End Function

        Class PSARPayload
            Implements IPayload
            Public Sub New()
                Me.PSAR = New Field(TypeOfField.PSAR)
            End Sub

            Public Property TradingSymbol As String Implements IPayload.TradingSymbol
            Public Property PSAR As Field
            Public Property Trend As Color

            Public Property EP As Decimal
            Public Property AF As Decimal
            Public Property NextBarSAR As Decimal

            Public Overrides Function ToString() As String
                Return String.Format("PSAR:{0}, Trend:{1}", Math.Round(Me.PSAR.Value, 4), Me.Trend.Name)
            End Function
        End Class
    End Class
End Namespace