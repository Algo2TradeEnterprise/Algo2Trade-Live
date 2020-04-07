﻿Imports System.IO
Imports System.Threading
Imports Algo2TradeCore.Entities.UserSettings
Imports Utilities.DAL
Imports Algo2TradeCore.Entities

<Serializable>
Public Class NFOUserInputs
    Inherits StrategyUserInputs

    Public Shared Property SettingsFileName As String = Path.Combine(My.Application.Info.DirectoryPath, "PSAR_NFO.Strategy.a2t")

    Public Property InstrumentDetailsFilePath As String
    Public Property InstrumentsData As Dictionary(Of String, InstrumentDetails)

    Public Property MinimumAF As Decimal
    Public Property MaximumAF As Decimal

    <Serializable>
    Public Class InstrumentDetails
        Public Property TradingSymbol As String
        Public Property NumberOfLots As Integer
        Public Property MaxStoplossPercentage As Decimal
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
                        Dim excelColumnList As New List(Of String) From {"INSTRUMENT NAME", "NUMBER OF LOTS", "MAX STOPLOSS %"}

                        For colCtr = 0 To 1
                            If instrumentDetails(0, colCtr) Is Nothing OrElse Trim(instrumentDetails(0, colCtr).ToString) = "" Then
                                Throw New ApplicationException(String.Format("Invalid format."))
                            Else
                                If Not excelColumnList.Contains(Trim(instrumentDetails(0, colCtr).ToString.ToUpper)) Then
                                    Throw New ApplicationException(String.Format("Invalid format or invalid column at ColumnNumber: {0}", colCtr))
                                End If
                            End If
                        Next
                        For rowCtr = 1 To instrumentDetails.GetLength(0) - 1
                            Dim trdngSymbl As String = Nothing
                            Dim nmbrOfLots As Integer = Integer.MinValue
                            Dim maxSlPer As Decimal = Decimal.MinValue

                            For columnCtr = 0 To instrumentDetails.GetLength(1)
                                If columnCtr = 0 Then
                                    If instrumentDetails(rowCtr, columnCtr) IsNot Nothing AndAlso
                                        Not Trim(instrumentDetails(rowCtr, columnCtr).ToString) = "" Then
                                        trdngSymbl = instrumentDetails(rowCtr, columnCtr)
                                    Else
                                        If Not rowCtr = instrumentDetails.GetLength(0) Then
                                            Throw New ApplicationException(String.Format("Trading Symbol Missing or Blank Row. RowNumber: {0}", rowCtr))
                                        End If
                                    End If
                                ElseIf columnCtr = 1 Then
                                    If instrumentDetails(rowCtr, columnCtr) IsNot Nothing AndAlso
                                        Not Trim(instrumentDetails(rowCtr, columnCtr).ToString) = "" Then
                                        If IsNumeric(instrumentDetails(rowCtr, columnCtr)) AndAlso
                                            Math.Round(Val(instrumentDetails(rowCtr, columnCtr)), 0) = Val(instrumentDetails(rowCtr, columnCtr)) Then
                                            nmbrOfLots = instrumentDetails(rowCtr, columnCtr)
                                        Else
                                            Throw New ApplicationException(String.Format("Number Of Lots cannot be of type {0} for {1}", instrumentDetails(rowCtr, columnCtr).GetType, trdngSymbl))
                                        End If
                                    Else
                                        Throw New ApplicationException(String.Format("Number Of Lots cannot be null for {0}", instrumentDetails(rowCtr, columnCtr).GetType, trdngSymbl))
                                    End If
                                ElseIf columnCtr = 2 Then
                                    If instrumentDetails(rowCtr, columnCtr) IsNot Nothing AndAlso
                                        Not Trim(instrumentDetails(rowCtr, columnCtr).ToString) = "" Then
                                        If IsNumeric(instrumentDetails(rowCtr, columnCtr)) Then
                                            maxSlPer = instrumentDetails(rowCtr, columnCtr)
                                        Else
                                            Throw New ApplicationException(String.Format("Max Stoploss % cannot be of type {0} for {1}", instrumentDetails(rowCtr, columnCtr).GetType, trdngSymbl))
                                        End If
                                    Else
                                        Throw New ApplicationException(String.Format("Max Stoploss % cannot be null for {0}", instrumentDetails(rowCtr, columnCtr).GetType, trdngSymbl))
                                    End If
                                End If
                            Next
                            If trdngSymbl IsNot Nothing Then
                                Dim instrumentData As New InstrumentDetails
                                instrumentData.TradingSymbol = trdngSymbl.ToUpper
                                instrumentData.NumberOfLots = nmbrOfLots
                                instrumentData.MaxStoplossPercentage = maxSlPer
                                If Me.InstrumentsData Is Nothing Then Me.InstrumentsData = New Dictionary(Of String, InstrumentDetails)
                                If Me.InstrumentsData.ContainsKey(instrumentData.TradingSymbol) Then
                                    Throw New ApplicationException(String.Format("Duplicate Trading Symbol {0}", instrumentData.TradingSymbol))
                                End If
                                Me.InstrumentsData.Add(instrumentData.TradingSymbol, instrumentData)
                            End If
                        Next
                    Else
                        Throw New ApplicationException("No valid input in the 'Instruments' file")
                    End If
                Else
                    Throw New ApplicationException("'Instruments' File Type not supported. Application only support .csv file.")
                End If
            Else
                Throw New ApplicationException("'Instruments' File does not exists. Please select valid file")
            End If
        Else
            Throw New ApplicationException("No valid 'Instruments' file path exists")
        End If
    End Sub

End Class