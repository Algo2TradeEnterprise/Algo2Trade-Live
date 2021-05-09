Imports System.IO
Imports Utilities.DAL
Imports System.Threading
Imports Algo2TradeCore.Entities.UserSettings

<Serializable>
Public Class NFOUserInputs
    Inherits StrategyUserInputs

    Public Shared Property SettingsFileName As String = Path.Combine(My.Application.Info.DirectoryPath, "NFOSettings.Strategy.a2t")

    Public Property InstrumentDetailsFilePath As String
    Public Property InstrumentsData As Dictionary(Of String, InstrumentDetails)

    <Serializable>
    Public Class InstrumentDetails
        Public Property InstrumentName As String
        Public Property ExpiryDate As Date
        Public Property NumberOfLots As Integer
        Public Property EntryBuffer As Integer
        Public Property InitialStoploss As Integer
        Public Property TrailingStoploss As Integer
        Public Property MinimumMovementForModification As Integer
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
                        Dim excelColumnList As New List(Of String) From {"CORE INSTRUMENT NAME", "EXPIRY DATE", "NUMBER OF LOTS", "ENTRY BUFFER", "INITIAL STOPLOSS", "TRAILING STOPLOSS", "MINIMUM MOVEMENT FOR MODIFICATION"}

                        For colCtr = 0 To 6
                            If instrumentDetails(0, colCtr) Is Nothing OrElse Trim(instrumentDetails(0, colCtr).ToString) = "" Then
                                Throw New ApplicationException(String.Format("Invalid format or invalid column at ColumnNumber: {0}", colCtr))
                            Else
                                If Not excelColumnList.Contains(Trim(instrumentDetails(0, colCtr).ToString.ToUpper)) Then
                                    Throw New ApplicationException(String.Format("Invalid format or invalid column at ColumnNumber: {0}", colCtr))
                                End If
                            End If
                        Next
                        For rowCtr = 1 To instrumentDetails.GetLength(0) - 1
                            Dim instrument As InstrumentDetails = New InstrumentDetails

                            For columnCtr = 0 To instrumentDetails.GetLength(1)
                                If columnCtr = 0 Then
                                    If instrumentDetails(rowCtr, columnCtr) IsNot Nothing AndAlso
                                        Not Trim(instrumentDetails(rowCtr, columnCtr).ToString) = "" Then
                                        instrument.InstrumentName = Trim(instrumentDetails(rowCtr, columnCtr).ToString).ToUpper
                                    Else
                                        If rowCtr <> instrumentDetails.GetLength(0) Then
                                            Throw New ApplicationException(String.Format("Instrument Name Missing or Blank Row. RowNumber: {0}", rowCtr))
                                        End If
                                    End If
                                ElseIf columnCtr = 1 Then
                                    If instrumentDetails(rowCtr, columnCtr) IsNot Nothing AndAlso
                                        Not Trim(instrumentDetails(rowCtr, columnCtr).ToString) = "" Then
                                        Try
                                            instrument.ExpiryDate = Date.ParseExact(instrumentDetails(rowCtr, columnCtr), "dd-MM-yyyy", Nothing)
                                        Catch ex As Exception
                                            Throw New ApplicationException(String.Format("Expiry should be in 'DD-MM-YYYY' format for {0}", instrument.InstrumentName))
                                        End Try
                                    Else
                                        Throw New ApplicationException(String.Format("Expiry cannot be blank for {0}", instrument.InstrumentName))
                                    End If
                                ElseIf columnCtr = 2 Then
                                    If instrumentDetails(rowCtr, columnCtr) IsNot Nothing AndAlso
                                        Not Trim(instrumentDetails(rowCtr, columnCtr).ToString) = "" Then
                                        If IsNumeric(Trim(instrumentDetails(rowCtr, columnCtr).ToString)) AndAlso
                                            Math.Round(Val(Trim(instrumentDetails(rowCtr, columnCtr).ToString)), 0) = Val(Trim(instrumentDetails(rowCtr, columnCtr).ToString)) Then
                                            instrument.NumberOfLots = Val(Trim(instrumentDetails(rowCtr, columnCtr)))
                                        Else
                                            Throw New ApplicationException(String.Format("Number Of Lots should be 'Integer' for {0}", instrument.InstrumentName))
                                        End If
                                    Else
                                        Throw New ApplicationException(String.Format("Number Of Lots cannot be blank for {0}", instrument.InstrumentName))
                                    End If
                                ElseIf columnCtr = 3 Then
                                    If instrumentDetails(rowCtr, columnCtr) IsNot Nothing AndAlso
                                        Not Trim(instrumentDetails(rowCtr, columnCtr).ToString) = "" Then
                                        If IsNumeric(Trim(instrumentDetails(rowCtr, columnCtr).ToString)) Then
                                            instrument.EntryBuffer = Val(Trim(instrumentDetails(rowCtr, columnCtr)))
                                        Else
                                            Throw New ApplicationException(String.Format("Entry Buffer should be 'Numeric' for {0}", instrument.InstrumentName))
                                        End If
                                    Else
                                        Throw New ApplicationException(String.Format("Entry Buffer cannot be blank for {0}", instrument.InstrumentName))
                                    End If
                                ElseIf columnCtr = 4 Then
                                    If instrumentDetails(rowCtr, columnCtr) IsNot Nothing AndAlso
                                        Not Trim(instrumentDetails(rowCtr, columnCtr).ToString) = "" Then
                                        If IsNumeric(Trim(instrumentDetails(rowCtr, columnCtr).ToString)) Then
                                            instrument.InitialStoploss = Val(Trim(instrumentDetails(rowCtr, columnCtr)))
                                        Else
                                            Throw New ApplicationException(String.Format("Initial Stoploss should be 'Numeric' for {0}", instrument.InstrumentName))
                                        End If
                                    Else
                                        Throw New ApplicationException(String.Format("Initial Stoploss cannot be blank for {0}", instrument.InstrumentName))
                                    End If
                                ElseIf columnCtr = 5 Then
                                    If instrumentDetails(rowCtr, columnCtr) IsNot Nothing AndAlso
                                        Not Trim(instrumentDetails(rowCtr, columnCtr).ToString) = "" Then
                                        If IsNumeric(Trim(instrumentDetails(rowCtr, columnCtr).ToString)) Then
                                            instrument.TrailingStoploss = Val(Trim(instrumentDetails(rowCtr, columnCtr)))
                                        Else
                                            Throw New ApplicationException(String.Format("Trailing Stoploss should be 'Numeric' for {0}", instrument.InstrumentName))
                                        End If
                                    Else
                                        Throw New ApplicationException(String.Format("Trailing Stoploss cannot be blank for {0}", instrument.InstrumentName))
                                    End If
                                ElseIf columnCtr = 6 Then
                                    If instrumentDetails(rowCtr, columnCtr) IsNot Nothing AndAlso
                                        Not Trim(instrumentDetails(rowCtr, columnCtr).ToString) = "" Then
                                        If IsNumeric(Trim(instrumentDetails(rowCtr, columnCtr).ToString)) Then
                                            instrument.MinimumMovementForModification = Val(Trim(instrumentDetails(rowCtr, columnCtr)))
                                        Else
                                            Throw New ApplicationException(String.Format("Minimum Movement For Modification should be 'Numeric' for {0}", instrument.InstrumentName))
                                        End If
                                    Else
                                        Throw New ApplicationException(String.Format("Minimum Movement For Modification cannot be blank for {0}", instrument.InstrumentName))
                                    End If
                                End If
                            Next
                            If instrument IsNot Nothing Then
                                If instrument.NumberOfLots < 1 Then
                                    Throw New ApplicationException("Number of Lots cannot be < 1")
                                End If
                                If instrument.EntryBuffer < 0.05 Then
                                    Throw New ApplicationException("Entry Buffer cannot be < 0.05")
                                End If
                                If instrument.InitialStoploss < 0.05 Then
                                    Throw New ApplicationException("Initial Stoploss cannot be < 0.05")
                                End If
                                If instrument.TrailingStoploss < 0.05 Then
                                    Throw New ApplicationException("Trailing Stoploss cannot be < 0.05")
                                End If
                                If instrument.MinimumMovementForModification < 1 Then
                                    Throw New ApplicationException("Minimum Movement For Modification cannot be < 1")
                                End If

                                If Me.InstrumentsData Is Nothing Then Me.InstrumentsData = New Dictionary(Of String, InstrumentDetails)
                                If Me.InstrumentsData.ContainsKey(instrument.InstrumentName) Then
                                    Throw New ApplicationException(String.Format("Duplicate Instrument Name {0}", instrument.InstrumentName))
                                End If
                                Me.InstrumentsData.Add(instrument.InstrumentName, instrument)
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
            Throw New ApplicationException("Input file path isnot valid")
        End If
    End Sub
End Class
