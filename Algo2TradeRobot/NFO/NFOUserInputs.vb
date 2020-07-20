Imports System.IO
Imports System.Threading
Imports Algo2TradeCore.Entities.UserSettings
Imports Utilities.DAL
Imports Algo2TradeCore.Entities

<Serializable>
Public Class NFOUserInputs
    Inherits StrategyUserInputs

    Public Shared Property SettingsFileName As String = Path.Combine(My.Application.Info.DirectoryPath, "OpeningRangeBreakout.Strategy.a2t")

    Enum TypeOfRanges
        C_1_Minute = 1
        C_2_Minute
        C_3_Minute
        C_4_Minute
        C_5_Minute
        C_10_Minute
        C_15_Minute
        C_30_Minute
        C_60_Minute
        Previous_Day
    End Enum

    Public Property RangeType As TypeOfRanges
    Public Property NumberOfTradePerStock As Integer
    Public Property MTMProfit As Decimal
    Public Property MTMLoss As Decimal
    Public Property InstrumentsData As Dictionary(Of String, InstrumentDetails)
    Public Property InstrumentDetailsFilePath As String

    <Serializable>
    Public Class InstrumentDetails
        Public Property TradingSymbol As String
        Public Property MaxLossPerStock As Decimal
        Public Property TargetMultiplier As Decimal
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
                        Dim excelColumnList As New List(Of String) From {"TRADING SYMBOL", "MAX LOSS", "TARGET MULTIPLIER"}

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
                            Dim trdngSymbl As String = Nothing
                            Dim maxLoss As Decimal = Decimal.MinValue
                            Dim trgtMul As Decimal = Decimal.MinValue

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
                                        If IsNumeric(instrumentDetails(rowCtr, columnCtr)) Then
                                            maxLoss = instrumentDetails(rowCtr, columnCtr)
                                        Else
                                            Throw New ApplicationException(String.Format("Max Loss cannot be of type {0} for {1}", instrumentDetails(rowCtr, columnCtr).GetType, trdngSymbl))
                                        End If
                                    Else
                                        Throw New ApplicationException(String.Format("Max Loss cannot be null for {0}", instrumentDetails(rowCtr, columnCtr).GetType, trdngSymbl))
                                    End If
                                ElseIf columnCtr = 2 Then
                                    If instrumentDetails(rowCtr, columnCtr) IsNot Nothing AndAlso
                                        Not Trim(instrumentDetails(rowCtr, columnCtr).ToString) = "" Then
                                        If IsNumeric(instrumentDetails(rowCtr, columnCtr)) Then
                                            trgtMul = instrumentDetails(rowCtr, columnCtr)
                                        Else
                                            Throw New ApplicationException(String.Format("Target Multiplier cannot be of type {0} for {1}", instrumentDetails(rowCtr, columnCtr).GetType, trdngSymbl))
                                        End If
                                    Else
                                        Throw New ApplicationException(String.Format("Target Multiplier cannot be null for {0}", instrumentDetails(rowCtr, columnCtr).GetType, trdngSymbl))
                                    End If
                                End If
                            Next
                            If trdngSymbl IsNot Nothing Then
                                Dim instrumentData As New InstrumentDetails With {
                                    .TradingSymbol = trdngSymbl.ToUpper,
                                    .MaxLossPerStock = maxLoss,
                                    .TargetMultiplier = trgtMul
                                }

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