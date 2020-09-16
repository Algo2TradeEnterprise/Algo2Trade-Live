Imports System.IO
Imports System.Threading
Imports Algo2TradeCore.Entities.UserSettings
Imports Utilities.DAL
Imports Algo2TradeCore.Entities

<Serializable>
Public Class NFOUserInputs
    Inherits StrategyUserInputs

    Public Shared Property SettingsFileName As String = Path.Combine(My.Application.Info.DirectoryPath, "AdaptiveMartingaleWithController.Strategy.a2t")

    Public Property MaxProfitPerStock As Decimal
    Public Property MaxTurnoverPerTrade As Decimal
    Public Property NumberOfStockToTrade As Integer

    Public Property StockList As List(Of String)
    Public Property MaxTargetPL As Decimal
    Public Property MinVolumePercentage As Decimal

    Private _LastEntryTime As Date
    Public Property LastEntryTime As Date
        Get
            Return New Date(Now.Year, Now.Month, Now.Day, _LastEntryTime.Hour, _LastEntryTime.Minute, _LastEntryTime.Second)
        End Get
        Set(value As Date)
            _LastEntryTime = value
        End Set
    End Property
End Class