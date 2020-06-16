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
    Public Property CalculateQuantityFromCapital As Boolean
    Public Property Capital As Decimal
    Public Property Margin As Decimal
    Public Property Quantity As Long

    Private _FirstEntryTime As Date
    Public Property FirstEntryTime As Date
        Get
            Return New Date(Now.Year, Now.Month, Now.Day, _FirstEntryTime.Hour, _FirstEntryTime.Minute, _FirstEntryTime.Second)
        End Get
        Set(value As Date)
            _FirstEntryTime = value
        End Set
    End Property

    Private _SecondEntryTime As Date
    Public Property SecondEntryTime As Date
        Get
            Return New Date(Now.Year, Now.Month, Now.Day, _SecondEntryTime.Hour, _SecondEntryTime.Minute, _SecondEntryTime.Second)
        End Get
        Set(value As Date)
            _SecondEntryTime = value
        End Set
    End Property

End Class
