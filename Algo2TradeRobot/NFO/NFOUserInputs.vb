Imports System.IO
Imports Algo2TradeCore.Entities.UserSettings

<Serializable>
Public Class NFOUserInputs
    Inherits StrategyUserInputs

    Public Shared Property SettingsFileName As String = Path.Combine(My.Application.Info.DirectoryPath, "Screener Live.Strategy.a2t")

    Public Property TargetToLeftMovementPercentage As Decimal

    Public Property MinimumPrice As Decimal
    Public Property MaximumPrice As Decimal
    Public Property MinimumVolume As Long
    Public Property MinimumATRPercentage As Decimal

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