﻿Imports System.IO
Imports System.Threading
Imports Algo2TradeCore.Entities.UserSettings
Imports Utilities.DAL

<Serializable>
Public Class LowSLUserInputs
    Inherits StrategyUserInputs
    Public Property ATRPeriod As Integer
    Public Property NumberOfTradePerStock As Integer
    Public Property StockMaxProfitPerDay As Decimal
    Public Property StockMaxLossPerDay As Decimal
    Public Property MaxProfitPerDay As Decimal
    Public Property MaxLossPerDay As Decimal
    Public Property AutoSelectStock As Boolean
    Public Property CashInstrument As Boolean
    Public Property FutureInstrument As Boolean
    Public Property MaxStoploss As Decimal
    Public Property MinCapital As Decimal
    Public Property InstrumentDetailsFilePath As String
    Public Property InstrumentsData As Dictionary(Of String, InstrumentDetails)

    Public Property MinPrice As Decimal
    Public Property MaxPrice As Decimal
    Public Property ATRPercentage As Decimal
    Public Property MinVolume As Decimal
    Public Property NumberOfStock As Integer
    Public Property MaxCapital As Decimal
    Public Property MinVolumeSpikePercentage As Decimal


    <Serializable>
    Public Class InstrumentDetails
        Public Property TradingSymbol As String
        Public Property MarginMultiplier As Decimal
        Public Property DayATR As Decimal
        Public Property Quantity As Integer
        Public Property SLPoint As Decimal
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
                        Dim excelColumnList As New List(Of String) From {"TRADING SYMBOL", "MARGIN MULTIPLIER", "DAY ATR", "QUANTITY", "SL POINT"}

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
                            Dim instrumentName As String = Nothing
                            Dim margin As Decimal = Decimal.MinValue
                            Dim dayATR As Decimal = Decimal.MinValue
                            Dim quantity As Integer = Integer.MinValue
                            Dim slPoint As Decimal = Decimal.MinValue
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
                                        If IsNumeric(instrumentDetails(rowCtr, columnCtr)) Then
                                            margin = instrumentDetails(rowCtr, columnCtr)
                                        Else
                                            Throw New ApplicationException(String.Format("Margin Multiplier cannot be of type {0} for {1}", instrumentDetails(rowCtr, columnCtr).GetType, instrumentName))
                                        End If
                                    Else
                                        Throw New ApplicationException(String.Format("Margin Multiplier cannot be null for {0}", instrumentDetails(rowCtr, columnCtr).GetType, instrumentName))
                                    End If
                                ElseIf columnCtr = 2 Then
                                    If instrumentDetails(rowCtr, columnCtr) IsNot Nothing AndAlso
                                        Not Trim(instrumentDetails(rowCtr, columnCtr).ToString) = "" Then
                                        If IsNumeric(instrumentDetails(rowCtr, columnCtr)) Then
                                            dayATR = instrumentDetails(rowCtr, columnCtr)
                                        Else
                                            Throw New ApplicationException(String.Format("Day ATR cannot be of type {0} for {1}", instrumentDetails(rowCtr, columnCtr).GetType, instrumentName))
                                        End If
                                    Else
                                        Throw New ApplicationException(String.Format("Day ATR cannot be null for {0}", instrumentDetails(rowCtr, columnCtr).GetType, instrumentName))
                                    End If
                                ElseIf columnCtr = 3 Then
                                    If instrumentDetails(rowCtr, columnCtr) IsNot Nothing AndAlso
                                        Not Trim(instrumentDetails(rowCtr, columnCtr).ToString) = "" Then
                                        If IsNumeric(instrumentDetails(rowCtr, columnCtr)) Then
                                            quantity = instrumentDetails(rowCtr, columnCtr)
                                        Else
                                            Throw New ApplicationException(String.Format("Quantity cannot be of type {0} for {1}", instrumentDetails(rowCtr, columnCtr).GetType, instrumentName))
                                        End If
                                    Else
                                        Throw New ApplicationException(String.Format("Quantity cannot be null for {0}", instrumentDetails(rowCtr, columnCtr).GetType, instrumentName))
                                    End If
                                ElseIf columnCtr = 4 Then
                                    If instrumentDetails(rowCtr, columnCtr) IsNot Nothing AndAlso
                                        Not Trim(instrumentDetails(rowCtr, columnCtr).ToString) = "" Then
                                        If IsNumeric(instrumentDetails(rowCtr, columnCtr)) Then
                                            slPoint = instrumentDetails(rowCtr, columnCtr)
                                        Else
                                            Throw New ApplicationException(String.Format("SL Point cannot be of type {0} for {1}", instrumentDetails(rowCtr, columnCtr).GetType, instrumentName))
                                        End If
                                    Else
                                        Throw New ApplicationException(String.Format("SL Point cannot be null for {0}", instrumentDetails(rowCtr, columnCtr).GetType, instrumentName))
                                    End If
                                End If
                            Next
                            If instrumentName IsNot Nothing Then
                                Dim instrumentData As New InstrumentDetails
                                With instrumentData
                                    .TradingSymbol = instrumentName.ToUpper
                                    .MarginMultiplier = margin
                                    .DayATR = dayATR
                                    .Quantity = quantity
                                    .SLPoint = slPoint
                                End With
                                If Me.InstrumentsData Is Nothing Then Me.InstrumentsData = New Dictionary(Of String, InstrumentDetails)
                                If Me.InstrumentsData.ContainsKey(instrumentData.TradingSymbol) Then
                                    Throw New ApplicationException(String.Format("Duplicate Instrument Name {0}", instrumentData.TradingSymbol))
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
