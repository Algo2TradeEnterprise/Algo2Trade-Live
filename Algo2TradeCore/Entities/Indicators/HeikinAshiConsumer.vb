Namespace Entities.Indicators
    Public Class HeikinAshiConsumer
        Inherits PayloadToIndicatorConsumer
        Public Sub New(ByVal associatedParentConsumer As IPayloadConsumer)
            MyBase.New(associatedParentConsumer)
        End Sub
        Public Overrides Function ToString() As String
            Return String.Format("{0}_{1}", Me.ParentConsumer.ToString, Me.GetType.Name)
        End Function
        'Class HeikinAshiPayload
        '    Implements IPayload
        '    Public Sub New()
        '        Me.Open = New Field(TypeOfField.HeikinAshi)
        '        Me.High = New Field(TypeOfField.HeikinAshi)
        '        Me.Low = New Field(TypeOfField.HeikinAshi)
        '        Me.Close = New Field(TypeOfField.HeikinAshi)
        '        Me.Volume = New Field(TypeOfField.HeikinAshi)
        '    End Sub
        '    Public Property TradingSymbol As String Implements IPayload.TradingSymbol
        '    Public Property SnapshotDateTime As Date
        '    Public Property Open As Field
        '    Public Property High As Field
        '    Public Property Low As Field
        '    Public Property Close As Field
        '    Public Property Volume As Field
        '    Public Property PreviousPayload As HeikinAshiPayload

        '    Public Overrides Function ToString() As String
        '        Return String.Format("HK Open:{0}, HK High:{1}, HK Low:{2}, HK Close:{3}, DateTime:{4}", Math.Round(Me.Open.Value, 4), Math.Round(Me.High.Value, 4), Math.Round(Me.Low.Value, 4), Math.Round(Me.Close.Value, 4), Me.SnapshotDateTime.ToString("dd-MM-yyyy HH:mm:ss"))
        '    End Function
        'End Class
    End Class
End Namespace