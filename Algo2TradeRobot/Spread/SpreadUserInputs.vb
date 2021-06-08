﻿Imports System.IO
Imports Utilities.DAL
Imports System.Threading
Imports Algo2TradeCore.Entities.UserSettings

<Serializable>
Public Class SpreadUserInputs
    Inherits StrategyUserInputs

    Public Shared Property SettingsFileName As String = Path.Combine(My.Application.Info.DirectoryPath, "SpreadSettings.Strategy.a2t")

    Public Property InstrumentDetailsFilePath As String
    Public Property InstrumentsData As Dictionary(Of String, InstrumentDetails)

    <Serializable>
    Public Class InstrumentDetails
        Public Property InstrumentName As String
        Public Property NumberOfLots As Integer

        Public Property Timeframe As Integer
        Public Property SupertrendPeriod As Integer
        Public Property SupertrendMultiplier As Decimal
        Public Property EntryGap As Decimal
        Public Property Distance As Decimal
    End Class

    Public Sub FillInstrumentDetails(ByVal filePath As String, ByVal canceller As CancellationTokenSource)
        If filePath IsNot Nothing Then
            If File.Exists(filePath) Then
                Dim extension As String = Path.GetExtension(filePath)
                If extension = ".csv" Then
                    Dim instrumentDetails(,) As Object = Nothing
                    Using csvReader As New CSVHelper(filePath, ",", canceller)
                        instrumentDetails = csvReader.Get2DArrayFromCSV(0)
                    End Using
                    If instrumentDetails IsNot Nothing AndAlso instrumentDetails.Length > 0 Then
                        Dim excelColumnList As New List(Of String) From {"RAW INSTRUMENT NAME", "NUMBER OF LOTS", "TIMEFRAME", "SUPERTREND PERIOD", "SUPERTREND MULTIPLIER", "ENTRY GAP", "ATM TO OTM DISTANCE"}

                        For colCtr = 0 To 6
                            If instrumentDetails(0, colCtr) Is Nothing OrElse Trim(instrumentDetails(0, colCtr).ToString) = "" Then
                                Throw New ApplicationException(String.Format("Invalid format."))
                            Else
                                If Not excelColumnList.Contains(Trim(instrumentDetails(0, colCtr).ToString.ToUpper)) Then
                                    Throw New ApplicationException(String.Format("Invalid format or invalid column at ColumnNumber: {0}", colCtr))
                                End If
                            End If
                        Next
                        For rowCtr = 1 To instrumentDetails.GetLength(0) - 1
                            Dim instrumentName As String = Nothing
                            Dim quantity As Integer = Integer.MinValue
                            Dim timeframe As Integer = Integer.MinValue
                            Dim stPeriod As Integer = Integer.MinValue
                            Dim stMultiplier As Decimal = Decimal.MinValue
                            Dim entryGap As Decimal = Decimal.MinValue
                            Dim distance As Decimal = Decimal.MinValue

                            For columnCtr = 0 To instrumentDetails.GetLength(1)
                                If columnCtr = 0 Then
                                    If instrumentDetails(rowCtr, columnCtr) IsNot Nothing AndAlso
                                        Not Trim(instrumentDetails(rowCtr, columnCtr).ToString) = "" Then
                                        instrumentName = instrumentDetails(rowCtr, columnCtr)
                                    Else
                                        If Not rowCtr = instrumentDetails.GetLength(0) Then
                                            Throw New ApplicationException(String.Format("Instrument Name Missing or Blank Row. RowNumber: {0}", rowCtr))
                                        End If
                                    End If
                                ElseIf columnCtr = 1 Then
                                    If instrumentDetails(rowCtr, columnCtr) IsNot Nothing AndAlso
                                        Not Trim(instrumentDetails(rowCtr, columnCtr).ToString) = "" Then
                                        If IsNumeric(instrumentDetails(rowCtr, columnCtr)) AndAlso
                                            Math.Round(Val(instrumentDetails(rowCtr, columnCtr)), 0) = Val(instrumentDetails(rowCtr, columnCtr)) Then
                                            quantity = Val(instrumentDetails(rowCtr, columnCtr))
                                        Else
                                            Throw New ApplicationException(String.Format("Number Of Lots cannot be of type {0} for {1}", instrumentDetails(rowCtr, columnCtr).GetType, instrumentName))
                                        End If
                                    Else
                                        Throw New ApplicationException(String.Format("Number Of Lots cannot be null for {0}", instrumentDetails(rowCtr, columnCtr).GetType, instrumentName))
                                    End If
                                ElseIf columnCtr = 2 Then
                                    If instrumentDetails(rowCtr, columnCtr) IsNot Nothing AndAlso
                                        Not Trim(instrumentDetails(rowCtr, columnCtr).ToString) = "" Then
                                        If IsNumeric(instrumentDetails(rowCtr, columnCtr)) AndAlso
                                            Math.Round(Val(instrumentDetails(rowCtr, columnCtr)), 0) = Val(instrumentDetails(rowCtr, columnCtr)) Then
                                            If Val(instrumentDetails(rowCtr, columnCtr)) > 0 AndAlso Val(instrumentDetails(rowCtr, columnCtr)) <= 180 Then
                                                timeframe = Val(instrumentDetails(rowCtr, columnCtr))
                                            Else
                                                Throw New ApplicationException(String.Format("Timeframe cannot be < 1 & > 180 for {0}", instrumentName))
                                            End If
                                        Else
                                            Throw New ApplicationException(String.Format("Timeframe cannot be of type {0} for {1}", instrumentDetails(rowCtr, columnCtr).GetType, instrumentName))
                                        End If
                                    Else
                                        Throw New ApplicationException(String.Format("Timeframe cannot be null for {0}", instrumentDetails(rowCtr, columnCtr).GetType, instrumentName))
                                    End If
                                ElseIf columnCtr = 3 Then
                                    If instrumentDetails(rowCtr, columnCtr) IsNot Nothing AndAlso
                                        Not Trim(instrumentDetails(rowCtr, columnCtr).ToString) = "" Then
                                        If IsNumeric(instrumentDetails(rowCtr, columnCtr)) AndAlso
                                            Math.Round(Val(instrumentDetails(rowCtr, columnCtr)), 0) = Val(instrumentDetails(rowCtr, columnCtr)) Then
                                            stPeriod = Val(instrumentDetails(rowCtr, columnCtr))
                                        Else
                                            Throw New ApplicationException(String.Format("Supertrend Period cannot be of type {0} for {1}", instrumentDetails(rowCtr, columnCtr).GetType, instrumentName))
                                        End If
                                    Else
                                        Throw New ApplicationException(String.Format("Supertrend Period cannot be null for {0}", instrumentDetails(rowCtr, columnCtr).GetType, instrumentName))
                                    End If
                                ElseIf columnCtr = 4 Then
                                    If instrumentDetails(rowCtr, columnCtr) IsNot Nothing AndAlso
                                        Not Trim(instrumentDetails(rowCtr, columnCtr).ToString) = "" Then
                                        If IsNumeric(instrumentDetails(rowCtr, columnCtr)) Then
                                            stMultiplier = Val(instrumentDetails(rowCtr, columnCtr))
                                        Else
                                            Throw New ApplicationException(String.Format("Supertrend Multiplier cannot be of type {0} for {1}", instrumentDetails(rowCtr, columnCtr).GetType, instrumentName))
                                        End If
                                    Else
                                        Throw New ApplicationException(String.Format("Supertrend Multiplier cannot be null for {0}", instrumentDetails(rowCtr, columnCtr).GetType, instrumentName))
                                    End If
                                ElseIf columnCtr = 5 Then
                                    If instrumentDetails(rowCtr, columnCtr) IsNot Nothing AndAlso
                                        Not Trim(instrumentDetails(rowCtr, columnCtr).ToString) = "" Then
                                        If IsNumeric(instrumentDetails(rowCtr, columnCtr)) Then
                                            entryGap = Val(instrumentDetails(rowCtr, columnCtr))
                                        Else
                                            Throw New ApplicationException(String.Format("Entry Gap cannot be of type {0} for {1}", instrumentDetails(rowCtr, columnCtr).GetType, instrumentName))
                                        End If
                                    Else
                                        Throw New ApplicationException(String.Format("Entry Gap cannot be null for {0}", instrumentDetails(rowCtr, columnCtr).GetType, instrumentName))
                                    End If
                                ElseIf columnCtr = 6 Then
                                    If instrumentDetails(rowCtr, columnCtr) IsNot Nothing AndAlso
                                        Not Trim(instrumentDetails(rowCtr, columnCtr).ToString) = "" Then
                                        If IsNumeric(instrumentDetails(rowCtr, columnCtr)) Then
                                            distance = Val(instrumentDetails(rowCtr, columnCtr))
                                        Else
                                            Throw New ApplicationException(String.Format("ATM to OTM Distance cannot be of type {0} for {1}", instrumentDetails(rowCtr, columnCtr).GetType, instrumentName))
                                        End If
                                    Else
                                        Throw New ApplicationException(String.Format("ATM to OTM Distance cannot be null for {0}", instrumentDetails(rowCtr, columnCtr).GetType, instrumentName))
                                    End If
                                End If
                            Next
                            If instrumentName IsNot Nothing AndAlso quantity > 0 Then
                                Dim instrumentData As New InstrumentDetails With {
                                    .InstrumentName = instrumentName.ToUpper.Trim,
                                    .NumberOfLots = quantity,
                                    .Timeframe = timeframe,
                                    .SupertrendPeriod = stPeriod,
                                    .SupertrendMultiplier = stMultiplier,
                                    .EntryGap = entryGap,
                                    .Distance = distance
                                }

                                If Me.InstrumentsData Is Nothing Then Me.InstrumentsData = New Dictionary(Of String, InstrumentDetails)
                                If Me.InstrumentsData.ContainsKey(instrumentData.InstrumentName) Then
                                    Throw New ApplicationException(String.Format("Duplicate Instrument Name {0}", instrumentData.InstrumentName))
                                End If
                                Me.InstrumentsData.Add(instrumentData.InstrumentName, instrumentData)
                            End If
                        Next
                    Else
                        Throw New ApplicationException("No valid input in the file")
                    End If
                Else
                    Throw New ApplicationException("Input file Type not supported. Application only support .csv file.")
                End If
            Else
                Throw New ApplicationException("Input file does not exists. Please select valid file")
            End If
        Else
            Throw New ApplicationException("No valid input file path exists")
        End If
    End Sub
End Class