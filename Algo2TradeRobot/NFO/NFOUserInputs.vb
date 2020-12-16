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

    Public Property TelegramBotAPIKey As String
    Public Property TelegramTradeChatID As String

    Public Property InstrumentDetailsFilePath As String
    Public Property InstrumentsData As Dictionary(Of String, InstrumentDetails)

    <Serializable>
    Public Class InstrumentDetails
        Public Property Stock1 As String
        Public Property Stock2 As String

        Public ReadOnly Property PairName As String
            Get
                Return String.Format("{0} ~ {1}", Me.Stock1, Me.Stock2)
            End Get
        End Property
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
                        Dim excelColumnList As New List(Of String) From {"STOCK 1", "STOCK 2"}

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
                            Dim stock1 As String = Nothing
                            Dim stock2 As String = Nothing
                            For columnCtr = 0 To instrumentDetails.GetLength(1)
                                If columnCtr = 0 Then
                                    If instrumentDetails(rowCtr, columnCtr) IsNot Nothing AndAlso
                                        Not Trim(instrumentDetails(rowCtr, columnCtr).ToString) = "" Then
                                        stock1 = instrumentDetails(rowCtr, columnCtr)
                                    Else
                                        If Not rowCtr = instrumentDetails.GetLength(0) Then
                                            Throw New ApplicationException(String.Format("Stock 1 Missing or Blank Row. RowNumber: {0}", rowCtr))
                                        End If
                                    End If
                                ElseIf columnCtr = 1 Then
                                    If instrumentDetails(rowCtr, columnCtr) IsNot Nothing AndAlso
                                        Not Trim(instrumentDetails(rowCtr, columnCtr).ToString) = "" Then
                                        stock2 = instrumentDetails(rowCtr, columnCtr)
                                    Else
                                        If Not rowCtr = instrumentDetails.GetLength(0) Then
                                            Throw New ApplicationException(String.Format("Stock 3 Missing or Blank Row. RowNumber: {0}", rowCtr))
                                        End If
                                    End If
                                End If
                            Next
                            If stock1 IsNot Nothing AndAlso stock2 IsNot Nothing Then
                                Dim instrumentData As New InstrumentDetails
                                With instrumentData
                                    .Stock1 = stock1.ToUpper.Trim
                                    .Stock2 = stock2.ToUpper.Trim
                                End With
                                If Me.InstrumentsData Is Nothing Then Me.InstrumentsData = New Dictionary(Of String, InstrumentDetails)
                                If Me.InstrumentsData.ContainsKey(instrumentData.PairName) Then
                                    Throw New ApplicationException(String.Format("Duplicate Trading Symbol {0}", instrumentData.PairName))
                                End If
                                Me.InstrumentsData.Add(instrumentData.PairName, instrumentData)
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