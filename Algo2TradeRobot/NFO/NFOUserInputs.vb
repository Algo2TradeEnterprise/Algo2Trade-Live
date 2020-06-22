Imports System.IO
Imports System.Threading
Imports Algo2TradeCore.Entities.UserSettings
Imports Utilities.DAL
Imports Algo2TradeCore.Entities

<Serializable>
Public Class NFOUserInputs
    Inherits StrategyUserInputs

    Public Shared Property SettingsFileName As String = Path.Combine(My.Application.Info.DirectoryPath, "Algo2Trade.Strategy.a2t")

    Public Property StoplossTrailingPercentage As Decimal
    Public Property BidAskRatio As Decimal
    Public Property CalculateQuantityFromCapital As Boolean
    Public Property Capital As Decimal
    Public Property MarginMultiplier As Decimal
    Public Property Quantity As Long

End Class
