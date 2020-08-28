Imports System.IO
Imports System.Threading
Imports Algo2TradeCore.Entities.UserSettings
Imports Utilities.DAL
Imports Algo2TradeCore.Entities

<Serializable>
Public Class NFOUserInputs
    Inherits StrategyUserInputs

#Region "Enum"
    Enum TypeOfDisplayLog
        All = 1
        Negative
        Positive
    End Enum
#End Region

    Public Shared Property SettingsFileName As String = Path.Combine(My.Application.Info.DirectoryPath, "Screener.Strategy.a2t")

    Public Property InstrumentDetailsFilePath As String
    Public Property InstrumentsData As List(Of InstrumentDetails)

    Public Property RunNSE As Boolean
    Public Property RunNFO As Boolean
    Public Property RunMCX As Boolean

    Public Property TargetToLeftMovementPercentage As Decimal
    Public Property DisplayLogType As TypeOfDisplayLog
    Public Property RepeatSignalOnHistoricalRefresh As Boolean

    'Indicator
    Public Property VWAP_EMAPeriod As Integer
    Public Property DayClose_SMAPeriod As Integer
    Public Property DayClose_ATRPeriod As Integer

    'Telegram
    Public Property TelegramAPIKey As String
    Public Property TelegramChatID As String

    <Serializable>
    Public Class InstrumentDetails
        Public Property TradingSymbol As String
        Public Property InstrumentType As String
        Public Property Range As Decimal
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
                        Dim excelColumnList As New List(Of String) From {"INSTRUMENT NAME", "INSTRUMENT TYPE", "RANGE(INR FOR EQUITY;PL POINT FOR FUTURES)"}

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
                            Dim instmntTyp As String = Nothing
                            Dim range As Decimal = Decimal.MinValue

                            For columnCtr = 0 To instrumentDetails.GetLength(1)
                                If columnCtr = 0 Then
                                    If instrumentDetails(rowCtr, columnCtr) IsNot Nothing AndAlso
                                        Not Trim(instrumentDetails(rowCtr, columnCtr).ToString) = "" Then
                                        trdngSymbl = instrumentDetails(rowCtr, columnCtr)
                                    Else
                                        If Not rowCtr = instrumentDetails.GetLength(0) Then
                                            Throw New ApplicationException(String.Format("Instrument Name Missing or Blank Row. RowNumber: {0}", rowCtr))
                                        End If
                                    End If
                                ElseIf columnCtr = 1 Then
                                    If instrumentDetails(rowCtr, columnCtr) IsNot Nothing AndAlso
                                        Not Trim(instrumentDetails(rowCtr, columnCtr).ToString) = "" Then
                                        instmntTyp = instrumentDetails(rowCtr, columnCtr)
                                    Else
                                        Throw New ApplicationException(String.Format("Instrument Type cannot be null for {0}", instrumentDetails(rowCtr, columnCtr).GetType, trdngSymbl))
                                    End If
                                ElseIf columnCtr = 2 Then
                                    If instrumentDetails(rowCtr, columnCtr) IsNot Nothing AndAlso
                                        Not Trim(instrumentDetails(rowCtr, columnCtr).ToString) = "" Then
                                        If IsNumeric(instrumentDetails(rowCtr, columnCtr)) Then
                                            range = instrumentDetails(rowCtr, columnCtr)
                                        Else
                                            Throw New ApplicationException(String.Format("Range cannot be of type {0} for {1}", instrumentDetails(rowCtr, columnCtr).GetType, trdngSymbl))
                                        End If
                                    Else
                                        Throw New ApplicationException(String.Format("Range cannot be null for {0}", instrumentDetails(rowCtr, columnCtr).GetType, trdngSymbl))
                                    End If
                                End If
                            Next
                            If trdngSymbl IsNot Nothing Then
                                Dim instrumentData As New InstrumentDetails
                                instrumentData.TradingSymbol = trdngSymbl.ToUpper
                                instrumentData.InstrumentType = instmntTyp
                                instrumentData.Range = range

                                If Me.InstrumentsData Is Nothing Then Me.InstrumentsData = New List(Of InstrumentDetails)

                                Dim availableInstruments As List(Of InstrumentDetails) = Me.InstrumentsData.FindAll(Function(x)
                                                                                                                        Return x.TradingSymbol.ToUpper = instrumentData.TradingSymbol.ToUpper AndAlso
                                                                                                                        x.InstrumentType.ToUpper = instrumentData.InstrumentType.ToUpper
                                                                                                                    End Function)
                                If availableInstruments IsNot Nothing AndAlso availableInstruments.Count > 0 Then
                                    Throw New ApplicationException(String.Format("Duplicate Instrument Details {0}", instrumentData.TradingSymbol))
                                End If
                                Me.InstrumentsData.Add(instrumentData)
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