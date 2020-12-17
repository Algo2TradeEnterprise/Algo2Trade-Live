Imports System.IO
Imports System.Threading
Imports Algo2TradeCore.Entities.UserSettings
Imports Utilities.DAL
Imports Algo2TradeCore.Entities

<Serializable>
Public Class NFOUserInputs
    Inherits StrategyUserInputs

    Public Shared Property SettingsFileName As String = Path.Combine(My.Application.Info.DirectoryPath, "Coinciding Pair.Strategy.a2t")

    Public Property LoopBackPeriod As Integer
    Public Property RolloverBeforeExpiry As Integer
    Public Property OverallTradeCount As Integer
    Public Property Correlation As Decimal
    Public Property InterpectPercentage As Decimal

    Public Property TelegramBotAPIKey As String
    Public Property TelegramTradeChatID As String

    Public Property SecotrDetailsFilePath As String
    Public Property SectorData As Dictionary(Of String, SectorDetails)

    <Serializable>
    Public Class InstrumentDetails
        Public Property Stock1 As String
        Public Property Stock2 As String
        Public Property PairName As String
    End Class

    <Serializable>
    Public Class SectorDetails
        Public Property SectorName As String
        Public Property EntrySD As Decimal
        Public Property Target As Decimal
        Public Property InstrumentsData As Dictionary(Of String, InstrumentDetails)
    End Class

    Public Sub FillSectorDetails(ByVal filePath As String, ByVal canceller As CancellationTokenSource)
        If filePath IsNot Nothing Then
            If File.Exists(filePath) Then
                Dim extension As String = Path.GetExtension(filePath)
                If extension = ".csv" Then
                    Dim instrumentDetails(,) As Object = Nothing
                    Using csvReader As New CSVHelper(filePath, ",", canceller)
                        instrumentDetails = csvReader.Get2DArrayFromCSV(0)
                    End Using
                    If instrumentDetails IsNot Nothing AndAlso instrumentDetails.Length > 0 Then
                        Dim excelColumnList As New List(Of String) From {"SECTOR", "ENTRY SD", "TARGET"}

                        For colCtr = 0 To 2
                            If instrumentDetails(0, colCtr) Is Nothing OrElse Trim(instrumentDetails(0, colCtr).ToString) = "" Then
                                Throw New ApplicationException(String.Format("Invalid format."))
                            Else
                                If Not excelColumnList.Contains(Trim(instrumentDetails(0, colCtr).ToString.ToUpper)) Then
                                    Throw New ApplicationException(String.Format("Invalid format or invalid column at ColumnNumber: {0}", colCtr))
                                End If
                            End If
                        Next
                        For rowCtr = 1 To instrumentDetails.GetLength(0) - 1
                            Dim sector As String = Nothing
                            Dim entrySD As Decimal = Decimal.MinValue
                            Dim target As Decimal = Decimal.MinValue
                            For columnCtr = 0 To instrumentDetails.GetLength(1)
                                If columnCtr = 0 Then
                                    If instrumentDetails(rowCtr, columnCtr) IsNot Nothing AndAlso
                                        Not Trim(instrumentDetails(rowCtr, columnCtr).ToString) = "" Then
                                        sector = instrumentDetails(rowCtr, columnCtr)
                                    Else
                                        If Not rowCtr = instrumentDetails.GetLength(0) Then
                                            Throw New ApplicationException(String.Format("Sector Missing or Blank Row. RowNumber: {0}", rowCtr))
                                        End If
                                    End If
                                ElseIf columnCtr = 1 Then
                                    If instrumentDetails(rowCtr, columnCtr) IsNot Nothing AndAlso
                                        Not Trim(instrumentDetails(rowCtr, columnCtr).ToString) = "" Then
                                        entrySD = instrumentDetails(rowCtr, columnCtr)
                                    Else
                                        If Not rowCtr = instrumentDetails.GetLength(0) Then
                                            Throw New ApplicationException(String.Format("Entry SD Missing or Blank Row. RowNumber: {0}", rowCtr))
                                        End If
                                    End If
                                ElseIf columnCtr = 2 Then
                                    If instrumentDetails(rowCtr, columnCtr) IsNot Nothing AndAlso
                                        Not Trim(instrumentDetails(rowCtr, columnCtr).ToString) = "" Then
                                        target = instrumentDetails(rowCtr, columnCtr)
                                    Else
                                        If Not rowCtr = instrumentDetails.GetLength(0) Then
                                            Throw New ApplicationException(String.Format("Target Missing or Blank Row. RowNumber: {0}", rowCtr))
                                        End If
                                    End If
                                End If
                            Next
                            If sector IsNot Nothing AndAlso entrySD <> Decimal.MinValue AndAlso target <> Decimal.MinValue Then
                                Dim sectorData As New SectorDetails
                                With sectorData
                                    .SectorName = sector
                                    .EntrySD = entrySD
                                    .Target = target
                                End With
                                If Me.SectorData Is Nothing Then Me.SectorData = New Dictionary(Of String, SectorDetails)
                                If Me.SectorData.ContainsKey(sectorData.SectorName) Then
                                    Throw New ApplicationException(String.Format("Duplicate Trading Symbol {0}", sectorData.SectorName))
                                End If
                                Me.SectorData.Add(sectorData.SectorName, sectorData)
                            End If
                        Next
                    Else
                        Throw New ApplicationException("No valid input in the file")
                    End If
                Else
                    Throw New ApplicationException("File Type not supported. Application only support .csv file.")
                End If
            Else
                Throw New ApplicationException("File does not exists. Please select valid file")
            End If
        Else
            Throw New ApplicationException("No valid file path exists")
        End If
    End Sub
End Class