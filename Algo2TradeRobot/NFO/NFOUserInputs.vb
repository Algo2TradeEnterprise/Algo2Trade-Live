Imports System.IO
Imports Algo2TradeCore.Entities
Imports Algo2TradeCore.Entities.UserSettings

<Serializable>
Public Class NFOUserInputs
    Inherits StrategyUserInputs

    Public Shared Property SettingsFileName As String = Path.Combine(My.Application.Info.DirectoryPath, "AdaptiveMartingale.Strategy.a2t")

    Public Property MinTurnoverPerTrade As Decimal
    Public Property MaxTurnoverPerTrade As Decimal
    Public Property MaxProfitPerStock As Decimal
    Public Property MaxFractalDifferencePercentage As Decimal


    Public Property TelegramBotAPIKey As String
    Public Property TelegramDebugChatID As String
    Public Property TelegramInfoChatID As String


    Public Property StockList As List(Of String)
    Public Property SpotDirection As IOrder.TypeOfTransaction
    Public Property MinNonBlankCandlePercentage As Decimal
    Public Property MinTotalCandlePercentage As Decimal
    Public Property MinEODTurnoverPercentage As Decimal


    Private _LastOptionCheckTime As Date
    Public Property LastOptionCheckTime As Date
        Get
            Return New Date(Now.Year, Now.Month, Now.Day, _LastOptionCheckTime.Hour, _LastOptionCheckTime.Minute, _LastOptionCheckTime.Second)
        End Get
        Set(value As Date)
            _LastOptionCheckTime = value
        End Set
    End Property
    Public Property MaxStrikeRangePercentage As Decimal
    Public Property MinVolumePercentageTillSignalTime As Decimal
End Class