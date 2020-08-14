Imports System.IO
Imports System.Threading
Imports Algo2TradeCore.Entities.UserSettings
Imports Utilities.DAL
Imports Algo2TradeCore.Entities

<Serializable>
Public Class NFOUserInputs
    Inherits StrategyUserInputs

    Public Shared Property SettingsFileName As String = Path.Combine(My.Application.Info.DirectoryPath, "AdaptiveMartingale.Strategy.a2t")

    Public Property MaxProfitPerStock As Decimal
    Public Property MaxTurnoverPerTrade As Decimal
    Public Property NumberOfStockToTrade As Integer

    Public Property MaxStockPrice As Decimal
    Public Property StockList As List(Of String)

End Class