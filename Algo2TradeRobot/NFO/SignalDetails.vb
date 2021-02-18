Imports System.IO

<Serializable>
Public Class SignalDetails
    Public Sub New(ByVal rawInstrumentName As String)
        Me.InstrumentName = rawInstrumentName
        Me.SignalDetailsFilename = Path.Combine(My.Application.Info.DirectoryPath, "Signals", String.Format("{0}.SignalDetails.a2t", Me.InstrumentName))
    End Sub

    Public ReadOnly Property InstrumentName As String
    Public ReadOnly Property SignalDetailsFilename As String

    Public ReadOnly Property AllTrades As List(Of Trade)

    Public Sub SerializeCollection()
        Utilities.Strings.SerializeFromCollection(Of SignalDetails)(Me.SignalDetailsFilename, Me)
    End Sub

    Public Sub InsertOrder(ByVal dummyTrade As Trade, ByVal entryOrderID As Decimal)
        If dummyTrade IsNot Nothing Then
            Dim tradeToInsert As Trade = New Trade(Me)
            tradeToInsert.UpdateTrade(TradingSymbol:=dummyTrade.TradingSymbol,
                                        ChildTag:=dummyTrade.ChildTag,
                                        ParentTag:=dummyTrade.ParentTag,
                                        CurrentStatus:=TradeStatus.Open,
                                        Direction:=dummyTrade.Direction,
                                        LossToRecover:=dummyTrade.LossToRecover,
                                        PotentialTarget:=dummyTrade.PotentialTarget,
                                        Quantity:=dummyTrade.Quantity,
                                        EntrySignalDate:=dummyTrade.EntrySignalDate,
                                        SpotATR:=dummyTrade.SpotATR,
                                        SpotPrice:=dummyTrade.SpotPrice,
                                        IterationNumber:=dummyTrade.IterationNumber,
                                        TypeOfEntry:=dummyTrade.TypeOfEntry,
                                        EntryOrderID:=dummyTrade.EntryOrderID,
                                        EntryTime:=dummyTrade.EntryTime,
                                        TypeOfEntryDetails:=dummyTrade.TypeOfEntryDetails,
                                        ATRConsumed:=dummyTrade.ATRConsumed,
                                        AttemptedEntryPrice:=dummyTrade.AttemptedEntryPrice)

            If _AllTrades Is Nothing Then _AllTrades = New List(Of Trade)
            _AllTrades.Add(tradeToInsert)
        End If
    End Sub

    Public Function IsActiveSignal() As Boolean
        Dim ret As Boolean = False
        If AllTrades IsNot Nothing AndAlso AllTrades.Count > 0 Then
            For Each runningTrade In AllTrades.OrderByDescending(Function(x)
                                                                     Return x.EntryTime
                                                                 End Function)
                If runningTrade.CurrentStatus = TradeStatus.InProgress OrElse
                    runningTrade.CurrentStatus = TradeStatus.Open Then
                    ret = True
                    Exit For
                End If
            Next
        End If
        Return ret
    End Function

    Public Function GetLastTrade() As Trade
        Dim ret As Trade = Nothing
        If AllTrades IsNot Nothing AndAlso AllTrades.Count > 0 Then
            For Each runningTrade In AllTrades.OrderByDescending(Function(x)
                                                                     Return x.EntryTime
                                                                 End Function)
                ret = runningTrade
                Exit For
            Next
        End If
        Return ret
    End Function

    Public Function GetLastCompleteTrade() As Trade
        Dim ret As Trade = Nothing
        If AllTrades IsNot Nothing AndAlso AllTrades.Count > 0 Then
            For Each runningTrade In AllTrades.OrderByDescending(Function(x)
                                                                     Return x.EntryTime
                                                                 End Function)
                If runningTrade.CurrentStatus = TradeStatus.Complete Then
                    ret = runningTrade
                    Exit For
                End If
            Next
        End If
        Return ret
    End Function

    Public Function GetEntryTrade(ByVal orderID As String) As Trade
        Dim ret As Trade = Nothing
        If AllTrades IsNot Nothing AndAlso AllTrades.Count > 0 Then
            For Each runningTrade In AllTrades.OrderByDescending(Function(x)
                                                                     Return x.EntryTime
                                                                 End Function)
                If runningTrade.EntryOrderID IsNot Nothing AndAlso runningTrade.EntryOrderID = orderID Then
                    ret = runningTrade
                    Exit For
                End If
            Next
        End If
        Return ret
    End Function

    Public Function GetAllTradesByChildTag(ByVal tag As String) As List(Of Trade)
        Dim ret As List(Of Trade) = Nothing
        If AllTrades IsNot Nothing AndAlso AllTrades.Count > 0 Then
            For Each runningTrade In AllTrades.OrderByDescending(Function(x)
                                                                     Return x.EntryTime
                                                                 End Function)
                If runningTrade.ChildTag = tag Then
                    If ret Is Nothing Then ret = New List(Of Trade)
                    ret.Add(runningTrade)
                End If
            Next
        End If
        Return ret
    End Function

    Public Function GetAllTradesByParentTag(ByVal tag As String) As List(Of Trade)
        Dim ret As List(Of Trade) = Nothing
        If AllTrades IsNot Nothing AndAlso AllTrades.Count > 0 Then
            For Each runningTrade In AllTrades.OrderByDescending(Function(x)
                                                                     Return x.EntryTime
                                                                 End Function)
                If runningTrade.ParentTag = tag Then
                    If ret Is Nothing Then ret = New List(Of Trade)
                    ret.Add(runningTrade)
                End If
            Next
        End If
        Return ret
    End Function
End Class

<Serializable>
Public Enum TradeStatus
    Open = 1
    InProgress
    Complete
    Cancel
    None
End Enum

<Serializable>
Public Enum ExitType
    Target = 1
    ContractRollover
    Reverse
    None
End Enum

<Serializable>
Public Enum EntryType
    Fresh
    LossMakeup
    None
End Enum

<Serializable>
Public Enum TradeDirection
    Buy = 1
    Sell
    None
End Enum

<Serializable>
Public Class Trade
    Public Sub New(ByVal parentSignal As SignalDetails)
        _ParentSignal = parentSignal
    End Sub

    <NonSerialized>
    Private _ParentSignal As SignalDetails

    Public ReadOnly Property TradingSymbol As String
    Public ReadOnly Property TypeOfEntry As EntryType
    Public ReadOnly Property TypeOfEntryDetails As ExitType
    Public ReadOnly Property CurrentStatus As TradeStatus
    Public ReadOnly Property ChildTag As String
    Public ReadOnly Property ParentTag As String
    Public ReadOnly Property IterationNumber As Integer
    Public ReadOnly Property SpotPrice As Decimal
    Public ReadOnly Property SpotATR As Decimal
    Public ReadOnly Property ATRConsumed As Decimal
    Public ReadOnly Property PotentialTarget As Decimal
    Public ReadOnly Property LossToRecover As Decimal
    Public ReadOnly Property EntrySignalDate As Date
    Public ReadOnly Property EntryTime As Date
    Public ReadOnly Property EntryOrderID As String
    Public ReadOnly Property AttemptedEntryPrice As Decimal
    Public ReadOnly Property EntryPrice As Decimal
    Public ReadOnly Property Quantity As Integer
    Public ReadOnly Property Direction As TradeDirection
    Public ReadOnly Property ExitSignalDate As Date
    Public ReadOnly Property ExitTime As Date
    Public ReadOnly Property ExitOrderID As String
    Public ReadOnly Property ExitPrice As Decimal
    Public ReadOnly Property AttemptedExitPrice As Decimal
    Public ReadOnly Property TypeOfExit As ExitType
    Public ReadOnly Property TargetOrderID As String
    Public ReadOnly Property TargetOrderTime As Date

    Public ReadOnly Property RequiredCapital As Decimal
        Get
            Return Me.EntryPrice * Me.Quantity
        End Get
    End Property

    Public Sub UpdateTrade(Optional ByVal TradingSymbol As String = Nothing,
                            Optional ByVal TypeOfEntry As EntryType = EntryType.None,
                            Optional ByVal TypeOfEntryDetails As ExitType = ExitType.None,
                            Optional ByVal CurrentStatus As TradeStatus = TradeStatus.None,
                            Optional ByVal ChildTag As String = Nothing,
                            Optional ByVal ParentTag As String = Nothing,
                            Optional ByVal IterationNumber As Integer = Integer.MinValue,
                            Optional ByVal SpotPrice As Decimal = Decimal.MinValue,
                            Optional ByVal SpotATR As Decimal = Decimal.MinValue,
                            Optional ByVal ATRConsumed As Decimal = Decimal.MinValue,
                            Optional ByVal PotentialTarget As Decimal = Decimal.MinValue,
                            Optional ByVal LossToRecover As Decimal = Decimal.MinValue,
                            Optional ByVal EntrySignalDate As Date = Nothing,
                            Optional ByVal EntryTime As Date = Nothing,
                            Optional ByVal EntryOrderID As String = Nothing,
                            Optional ByVal AttemptedEntryPrice As Decimal = Decimal.MinValue,
                            Optional ByVal EntryPrice As Decimal = Decimal.MinValue,
                            Optional ByVal Quantity As Integer = Integer.MinValue,
                            Optional ByVal Direction As TradeDirection = TradeDirection.None,
                            Optional ByVal ExitSignalDate As Date = Nothing,
                            Optional ByVal ExitTime As Date = Nothing,
                            Optional ByVal ExitOrderID As String = Nothing,
                            Optional ByVal ExitPrice As Decimal = Decimal.MinValue,
                            Optional ByVal AttemptedExitPrice As Decimal = Decimal.MinValue,
                            Optional ByVal TypeOfExit As ExitType = ExitType.None,
                            Optional ByVal TargetOrderID As String = Nothing,
                            Optional ByVal TargetOrderTime As Date = Nothing)
        If TradingSymbol IsNot Nothing Then _TradingSymbol = TradingSymbol
        If TypeOfEntry <> EntryType.None Then _TypeOfEntry = TypeOfEntry
        If TypeOfEntryDetails <> ExitType.None Then _TypeOfEntryDetails = TypeOfEntryDetails
        If CurrentStatus <> TradeStatus.None Then _CurrentStatus = CurrentStatus
        If ChildTag IsNot Nothing Then _ChildTag = ChildTag
        If ParentTag IsNot Nothing Then _ParentTag = ParentTag
        If IterationNumber <> Integer.MinValue Then _IterationNumber = IterationNumber
        If SpotPrice <> Decimal.MinValue Then _SpotPrice = SpotPrice
        If SpotATR <> Decimal.MinValue Then _SpotATR = SpotATR
        If ATRConsumed <> Decimal.MinValue Then _ATRConsumed = ATRConsumed
        If PotentialTarget <> Decimal.MinValue Then _PotentialTarget = PotentialTarget
        If LossToRecover <> Decimal.MinValue Then _LossToRecover = LossToRecover
        If EntrySignalDate <> Nothing AndAlso EntrySignalDate <> Date.MinValue Then _EntrySignalDate = EntrySignalDate
        If EntryTime <> Nothing AndAlso EntryTime <> Date.MinValue Then _EntryTime = EntryTime
        If EntryOrderID IsNot Nothing Then _EntryOrderID = EntryOrderID
        If AttemptedEntryPrice <> Decimal.MinValue Then _AttemptedEntryPrice = AttemptedEntryPrice
        If EntryPrice <> Decimal.MinValue Then _EntryPrice = EntryPrice
        If Quantity <> Integer.MinValue Then _Quantity = Quantity
        If Direction <> TradeDirection.None Then _Direction = Direction
        If ExitSignalDate <> Nothing AndAlso ExitSignalDate <> Date.MinValue Then _ExitSignalDate = ExitSignalDate
        If ExitTime <> Nothing AndAlso ExitTime <> Date.MinValue Then _ExitTime = ExitTime
        If ExitOrderID IsNot Nothing Then _ExitOrderID = ExitOrderID
        If ExitPrice <> Decimal.MinValue Then _ExitPrice = ExitPrice
        If AttemptedExitPrice <> Decimal.MinValue Then _AttemptedExitPrice = AttemptedExitPrice
        If TypeOfExit <> ExitType.None Then _TypeOfExit = TypeOfExit
        If TargetOrderID IsNot Nothing Then _TargetOrderID = TargetOrderID
        If TargetOrderTime <> Nothing AndAlso TargetOrderTime <> Date.MinValue Then _TargetOrderTime = TargetOrderTime

        If _ParentSignal IsNot Nothing Then _ParentSignal.SerializeCollection()
    End Sub

    Public Sub UpdateParentSignal(ByVal parentSignal As SignalDetails)
        Me._ParentSignal = parentSignal
    End Sub
End Class