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

    Public Sub InsertOrder(ByVal dummyTrade As Trade, ByVal entryOrderID As Decimal)
        If dummyTrade IsNot Nothing Then
            Dim tradeToInsert As Trade = New Trade With {
                                .ChildTag = dummyTrade.ChildTag,
                                .ContractRemark = dummyTrade.ContractRemark,
                                .CurrentStatus = TradeStatus.Open,
                                .Direction = dummyTrade.Direction,
                                .ParentTag = dummyTrade.ParentTag,
                                .PotentialTarget = dummyTrade.PotentialTarget,
                                .Quantity = dummyTrade.Quantity,
                                .SignalDate = dummyTrade.SignalDate,
                                .SpotATR = dummyTrade.SpotATR,
                                .SpotPrice = dummyTrade.SpotPrice,
                                .TradeNumber = dummyTrade.TradeNumber,
                                .TradingSymbol = dummyTrade.TradingSymbol,
                                .TypeOfEntry = dummyTrade.TypeOfEntry,
                                .EntryOrderID = entryOrderID,
                                .EntryTime = Now,
                                .TypeOfEntryDetails = dummyTrade.TypeOfEntryDetails,
                                .ATRConsumed = dummyTrade.ATRConsumed
                            }

            If _AllTrades Is Nothing Then _AllTrades = New List(Of Trade)
            _AllTrades.Add(tradeToInsert)

            Utilities.Strings.SerializeFromCollection(Of SignalDetails)(Me.SignalDetailsFilename, Me)
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
    ZeroPremium
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
    Public Property TradingSymbol As String
    Public Property TypeOfEntry As EntryType
    Public Property TypeOfEntryDetails As ExitType
    Public Property CurrentStatus As TradeStatus
    Public Property ChildTag As String
    Public Property ParentTag As String
    Public Property TradeNumber As Integer
    Public Property SpotPrice As Decimal
    Public Property SpotATR As Decimal
    Public Property ATRConsumed As Decimal
    Public Property PotentialTarget As Decimal
    Public Property ContractRemark As String
    Public Property SignalDate As Date
    Public Property EntryTime As Date
    Public Property EntryOrderID As String
    Public Property EntryPrice As Decimal
    Public Property Quantity As Integer
    Public Property Direction As TradeDirection
    Public Property ExitTime As Date
    Public Property ExitOrderID As String
    Public Property ExitPrice As Decimal
    Public Property TypeOfExit As ExitType
End Class