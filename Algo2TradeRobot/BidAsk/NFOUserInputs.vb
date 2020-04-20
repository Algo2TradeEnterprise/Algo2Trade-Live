Imports System.IO
Imports System.Threading
Imports Algo2TradeCore.Entities.UserSettings
Imports Utilities.DAL

<Serializable>
Public Class NFOUserInputs
    Inherits StrategyUserInputs

    Public Shared Property SettingsFileName As String = Path.Combine(My.Application.Info.DirectoryPath, "OptionBidAsk.Strategy.a2t")

    Public Property InstrumentDetailsFilePath As String
    Public Property InstrumentsData As Dictionary(Of String, InstrumentDetails)

    Private _StartTime As Date
    Public Property StartTime As Date
        Get
            Return New Date(Now.Year, Now.Month, Now.Day, _StartTime.Hour, _StartTime.Minute, _StartTime.Second)
        End Get
        Set(value As Date)
            _StartTime = value
        End Set
    End Property

    Private _EndTime As Date
    Public Property EndTime As Date
        Get
            Return New Date(Now.Year, Now.Month, Now.Day, _EndTime.Hour, _EndTime.Minute, _EndTime.Second)
        End Get
        Set(value As Date)
            _EndTime = value
        End Set
    End Property

    Public Property TickBased As Boolean
    Public Property MinuteBased As Boolean

    <Serializable>
    Public Class InstrumentDetails
        Enum TypeOfContract
            None = 1
            Current
            Near
            Future
        End Enum

        Public Property SheetName As String
        Public Property RawInstrumentName As String
        Public Property ContractType As TypeOfContract
    End Class
    Public Async Function FillInstrumentDetailsAsync(ByVal filePath As String, ByVal canceller As CancellationTokenSource) As Task
        Await Task.Delay(1).ConfigureAwait(False)
        If filePath IsNot Nothing Then
            If File.Exists(filePath) Then
                Dim extension As String = Path.GetExtension(filePath)
                If extension = ".xlsx" Then
                    Using xlHlpr As New ExcelHelper(filePath, ExcelHelper.ExcelOpenStatus.OpenExistingForReadWrite, ExcelHelper.ExcelSaveType.XLS_XLSX, canceller)
                        Dim allSheets As List(Of String) = xlHlpr.GetExcelSheetsName()
                        If allSheets IsNot Nothing AndAlso allSheets.Count > 0 Then
                            For Each runningSheet In allSheets
                                Dim sheetName As String = runningSheet
                                Dim instrumentName As String = sheetName.Split(".")(0).Trim.ToUpper
                                Dim contractName As String = sheetName.Split(".")(1).Trim.ToUpper
                                Dim contractType As InstrumentDetails.TypeOfContract = InstrumentDetails.TypeOfContract.None
                                If contractName = "CURRENT" Then
                                    contractType = InstrumentDetails.TypeOfContract.Current
                                ElseIf contractName = "NEAR" Then
                                    contractType = InstrumentDetails.TypeOfContract.Near
                                ElseIf contractName = "FUTURE" Then
                                    contractType = InstrumentDetails.TypeOfContract.Future
                                End If

                                Dim instrumentData As InstrumentDetails = New InstrumentDetails With {
                                    .SheetName = sheetName,
                                    .RawInstrumentName = instrumentName,
                                    .ContractType = contractType
                                }
                                If Me.InstrumentsData Is Nothing Then Me.InstrumentsData = New Dictionary(Of String, InstrumentDetails)
                                If Not Me.InstrumentsData.ContainsKey(instrumentData.SheetName) Then
                                    Me.InstrumentsData.Add(instrumentData.SheetName, instrumentData)
                                End If
                            Next
                        End If
                    End Using
                Else
                    Throw New ApplicationException("'Instruments' File Type not supported. Application only support .csv file.")
                End If
            Else
                Throw New ApplicationException("'Instruments' File does not exists. Please select valid file")
            End If
        Else
            Throw New ApplicationException("No valid 'Instruments' file path exists")
        End If
    End Function

End Class
