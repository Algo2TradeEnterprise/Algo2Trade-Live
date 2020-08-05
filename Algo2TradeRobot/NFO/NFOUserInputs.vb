﻿Imports System.IO
Imports System.Threading
Imports Algo2TradeCore.Entities.UserSettings
Imports Utilities.DAL
Imports Algo2TradeCore.Entities

<Serializable>
Public Class NFOUserInputs
    Inherits StrategyUserInputs

    Public Shared Property SettingsFileName As String = Path.Combine(My.Application.Info.DirectoryPath, "PreDayHKTrendEmaMartingale.Strategy.a2t")

    Public Property InstrumentDetailsFilePath As String
    Public Property InstrumentsData As Dictionary(Of String, InstrumentDetails)

    Public Property MaxProfitPerTrade As Decimal
    Public Property NumberOfTradePerStock As Integer
    Public Property OverallMaxProfitPerDay As Decimal
    Public Property OverallMaxLossPerDay As Decimal
    Public Property MaxTargetToStoplossMultiplier As Decimal
    Public Property MaxTurnoverOfATrade As Decimal

    Public Property AutoSelectStock As Boolean
    Public Property MinStockPrice As Decimal
    Public Property MaxStockPrice As Decimal
    Public Property MinATRPercentage As Decimal
    Public Property MaxBlankCandlePercentage As Decimal
    Public Property NumberOfStock As Integer

    <Serializable>
    Public Class InstrumentDetails
        Public Property TradingSymbol As String
        Public Property Multiplier As Decimal
        Public Property PreviousDayHighestATR As Decimal
        Public Property PreviousDayHKOpen As Decimal
        Public Property PreviousDayHKLow As Decimal
        Public Property PreviousDayHKHigh As Decimal
        Public Property PreviousDayHKClose As Decimal
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
                        Dim excelColumnList As New List(Of String) From {"TRADING SYMBOL", "MULTIPLIER", "PREVIOUS DAY HIGHEST ATR", "PREVIOUS DAY HK OPEN", "PREVIOUS DAY HK LOW", "PREVIOUS DAY HK HIGH", "PREVIOUS DAY HK CLOSE"}

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
                            Dim mul As Decimal = 0
                            Dim hgstATR As Decimal = 0
                            Dim hkOpen As Decimal = 0
                            Dim hkLow As Decimal = 0
                            Dim hkHigh As Decimal = 0
                            Dim hkClose As Decimal = 0
                            For columnCtr = 0 To instrumentDetails.GetLength(1)
                                If columnCtr = 0 Then
                                    If instrumentDetails(rowCtr, columnCtr) IsNot Nothing AndAlso
                                        Not Trim(instrumentDetails(rowCtr, columnCtr).ToString) = "" Then
                                        instrumentName = instrumentDetails(rowCtr, columnCtr)
                                    Else
                                        If Not rowCtr = instrumentDetails.GetLength(0) Then
                                            Throw New ApplicationException(String.Format("Trading Symbol Missing or Blank Row. RowNumber: {0}", rowCtr))
                                        End If
                                    End If
                                ElseIf columnCtr = 1 Then
                                    If instrumentDetails(rowCtr, columnCtr) IsNot Nothing AndAlso
                                        Not Trim(instrumentDetails(rowCtr, columnCtr).ToString) = "" Then
                                        If IsNumeric(instrumentDetails(rowCtr, columnCtr)) Then
                                            mul = instrumentDetails(rowCtr, columnCtr)
                                        Else
                                            Throw New ApplicationException(String.Format("Multiplier can not be of type {0}. RowNumber: {1}", instrumentDetails(rowCtr, columnCtr).GetType, rowCtr))
                                        End If
                                    Else
                                        Throw New ApplicationException(String.Format("Multiplier can not be null. RowNumber: {0}", rowCtr))
                                    End If
                                ElseIf columnCtr = 2 Then
                                    If instrumentDetails(rowCtr, columnCtr) IsNot Nothing AndAlso
                                        Not Trim(instrumentDetails(rowCtr, columnCtr).ToString) = "" Then
                                        If IsNumeric(instrumentDetails(rowCtr, columnCtr)) Then
                                            hgstATR = instrumentDetails(rowCtr, columnCtr)
                                        Else
                                            Throw New ApplicationException(String.Format("Previous Day Highest ATR can not be of type {0}. RowNumber: {1}", instrumentDetails(rowCtr, columnCtr).GetType, rowCtr))
                                        End If
                                    Else
                                        Throw New ApplicationException(String.Format("Previous Day Highest ATR can not be null. RowNumber: {0}", rowCtr))
                                    End If
                                ElseIf columnCtr = 3 Then
                                    If instrumentDetails(rowCtr, columnCtr) IsNot Nothing AndAlso
                                        Not Trim(instrumentDetails(rowCtr, columnCtr).ToString) = "" Then
                                        If IsNumeric(instrumentDetails(rowCtr, columnCtr)) Then
                                            hkOpen = instrumentDetails(rowCtr, columnCtr)
                                        Else
                                            Throw New ApplicationException(String.Format("Previous Day HK Open can not be of type {0}. RowNumber: {1}", instrumentDetails(rowCtr, columnCtr).GetType, rowCtr))
                                        End If
                                    Else
                                        Throw New ApplicationException(String.Format("Previous Day HK Open can not be null. RowNumber: {0}", rowCtr))
                                    End If
                                ElseIf columnCtr = 4 Then
                                    If instrumentDetails(rowCtr, columnCtr) IsNot Nothing AndAlso
                                        Not Trim(instrumentDetails(rowCtr, columnCtr).ToString) = "" Then
                                        If IsNumeric(instrumentDetails(rowCtr, columnCtr)) Then
                                            hkLow = instrumentDetails(rowCtr, columnCtr)
                                        Else
                                            Throw New ApplicationException(String.Format("Previous Day HK Low can not be of type {0}. RowNumber: {1}", instrumentDetails(rowCtr, columnCtr).GetType, rowCtr))
                                        End If
                                    Else
                                        Throw New ApplicationException(String.Format("Previous Day HK Low can not be null. RowNumber: {0}", rowCtr))
                                    End If
                                ElseIf columnCtr = 5 Then
                                    If instrumentDetails(rowCtr, columnCtr) IsNot Nothing AndAlso
                                        Not Trim(instrumentDetails(rowCtr, columnCtr).ToString) = "" Then
                                        If IsNumeric(instrumentDetails(rowCtr, columnCtr)) Then
                                            hkHigh = instrumentDetails(rowCtr, columnCtr)
                                        Else
                                            Throw New ApplicationException(String.Format("Previous Day HK High can not be of type {0}. RowNumber: {1}", instrumentDetails(rowCtr, columnCtr).GetType, rowCtr))
                                        End If
                                    Else
                                        Throw New ApplicationException(String.Format("Previous Day HK Close can not be null. RowNumber: {0}", rowCtr))
                                    End If
                                ElseIf columnCtr = 6 Then
                                    If instrumentDetails(rowCtr, columnCtr) IsNot Nothing AndAlso
                                        Not Trim(instrumentDetails(rowCtr, columnCtr).ToString) = "" Then
                                        If IsNumeric(instrumentDetails(rowCtr, columnCtr)) Then
                                            hkClose = instrumentDetails(rowCtr, columnCtr)
                                        Else
                                            Throw New ApplicationException(String.Format("Previous Day HK Close can not be of type {0}. RowNumber: {1}", instrumentDetails(rowCtr, columnCtr).GetType, rowCtr))
                                        End If
                                    Else
                                        Throw New ApplicationException(String.Format("Previous Day HK Close can not be null. RowNumber: {0}", rowCtr))
                                    End If
                                End If
                            Next
                            If instrumentName IsNot Nothing Then
                                Dim instrumentData As New InstrumentDetails
                                With instrumentData
                                    .TradingSymbol = instrumentName.ToUpper
                                    .Multiplier = mul
                                    .PreviousDayHighestATR = hgstATR
                                    .PreviousDayHKOpen = hkOpen
                                    .PreviousDayHKLow = hkLow
                                    .PreviousDayHKHigh = hkHigh
                                    .PreviousDayHKClose = hkClose
                                End With
                                If Me.InstrumentsData Is Nothing Then Me.InstrumentsData = New Dictionary(Of String, InstrumentDetails)
                                If Me.InstrumentsData.ContainsKey(instrumentData.TradingSymbol) Then
                                    Throw New ApplicationException(String.Format("Duplicate Trading Symbol {0}", instrumentData.TradingSymbol))
                                End If
                                Me.InstrumentsData.Add(instrumentData.TradingSymbol, instrumentData)
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
