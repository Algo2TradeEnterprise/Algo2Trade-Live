Imports System.IO
Imports Algo2TradeCore.Entities.UserSettings

<Serializable>
Public Class NFOUserInputs
    Inherits StrategyUserInputs

    Public Shared Property SettingsFileName As String = Path.Combine(My.Application.Info.DirectoryPath, "Screener Live.Strategy.a2t")

    Public Property TargetToLeftMovementPercentage As Decimal
    Public Property NSEMaxLossPerTrade As Decimal
    Public Property OverallMaxProfit As Decimal
    Public Property OverallMaxLoss As Decimal

    'Stock Selection
    Public Property MinimumStockPrice As Decimal
    Public Property MaximumStockPrice As Decimal
    Public Property MinimumVolume As Long
    Public Property MinimumATRPercentage As Decimal
    Public Property NumberOfStocks As Integer
    Public Property InstrumentDetailsFilepath As String
    Public Property AutoSelectStock As Boolean

    'Indicator
    Public Property VWAP_EMAPeriod As Integer
    Public Property DayClose_SMAPeriod As Integer
    Public Property DayClose_ATRPeriod As Integer
    Public Property Close_RSIPeriod As Integer
    Public Property RSILevel As Decimal

    'Telegram
    Public Property TelegramAPIKey As String
    Public Property TelegramChatID As String
End Class