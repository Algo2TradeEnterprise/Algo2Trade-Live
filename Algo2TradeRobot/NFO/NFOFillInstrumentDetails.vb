Imports System.IO
Imports System.Threading
Imports Utilities.DAL
Imports Algo2TradeCore.Entities
Imports Algo2TradeCore.Adapter
Imports NLog

Public Class NFOFillInstrumentDetails
    Implements IDisposable

#Region "Logging and Status Progress"
    Public Shared Shadows logger As Logger = LogManager.GetCurrentClassLogger
#End Region

#Region "Events/Event handlers"
    Public Event DocumentDownloadComplete()
    Public Event DocumentRetryStatus(ByVal currentTry As Integer, ByVal totalTries As Integer)
    Public Event Heartbeat(ByVal msg As String)
    Public Event WaitingFor(ByVal elapsedSecs As Integer, ByVal totalSecs As Integer, ByVal msg As String)
    'The below functions are needed to allow the derived classes to raise the above two events
    Protected Overridable Sub OnDocumentDownloadComplete()
        RaiseEvent DocumentDownloadComplete()
    End Sub
    Protected Overridable Sub OnDocumentRetryStatus(ByVal currentTry As Integer, ByVal totalTries As Integer)
        RaiseEvent DocumentRetryStatus(currentTry, totalTries)
    End Sub
    Protected Overridable Sub OnHeartbeat(ByVal msg As String)
        RaiseEvent Heartbeat(msg)
    End Sub
    Protected Overridable Sub OnWaitingFor(ByVal elapsedSecs As Integer, ByVal totalSecs As Integer, ByVal msg As String)
        RaiseEvent WaitingFor(elapsedSecs, totalSecs, msg)
    End Sub
#End Region

    Private _cts As CancellationTokenSource
    Private ReadOnly _parentStrategy As NFOStrategy
    Private ReadOnly _userInputs As NFOUserInputs
    Private ReadOnly tradingDay As Date = Date.MinValue
    Private ReadOnly _APIAdapter As APIAdapter

    Public Sub New(ByVal canceller As CancellationTokenSource, ByVal parentStrategy As NFOStrategy)
        _cts = canceller
        _parentStrategy = parentStrategy
        _userInputs = _parentStrategy.UserSettings
        tradingDay = Now

        Select Case _parentStrategy.ParentController.BrokerSource
            Case APISource.Zerodha
                _APIAdapter = New ZerodhaAdapter(parentStrategy.ParentController, _cts)
            Case APISource.AliceBlue
                _APIAdapter = New AliceAdapter(parentStrategy.ParentController, _cts)
            Case APISource.Upstox
                Throw New NotImplementedException
            Case APISource.None
                Throw New NotImplementedException
        End Select
    End Sub

    Public Async Function GetInstrumentData(ByVal allInstruments As IEnumerable(Of IInstrument), ByVal bannedStock As List(Of String)) As Task
        If allInstruments IsNot Nothing AndAlso allInstruments.Count > 0 Then
            Dim nfoInstruments As IEnumerable(Of IInstrument) = allInstruments.Where(Function(x)
                                                                                         Return x.Segment = "NFO-FUT"
                                                                                     End Function)
            If nfoInstruments IsNot Nothing AndAlso nfoInstruments.Count > 0 Then
                Dim currentNFOInstruments As List(Of IInstrument) = Nothing
                For Each runningInstrument In nfoInstruments
                    If currentNFOInstruments IsNot Nothing AndAlso currentNFOInstruments.Count > 0 Then
                        Dim availableInstrument As IEnumerable(Of IInstrument) = currentNFOInstruments.FindAll(Function(z)
                                                                                                                   Return z.RawInstrumentName = runningInstrument.RawInstrumentName
                                                                                                               End Function)
                        If availableInstrument IsNot Nothing AndAlso availableInstrument.Count > 0 Then
                            Continue For
                        End If
                    End If
                    Dim runningIntruments As IEnumerable(Of IInstrument) = nfoInstruments.Where(Function(x)
                                                                                                    Return x.RawInstrumentName = runningInstrument.RawInstrumentName
                                                                                                End Function)
                    Dim minExpiry As Date = runningIntruments.Min(Function(x)
                                                                      If x.Expiry.Value.Date <= Now.Date Then
                                                                          Return Date.MaxValue
                                                                      Else
                                                                          Return x.Expiry
                                                                      End If
                                                                  End Function)
                    Dim currentIntrument As IInstrument = runningIntruments.ToList.Find(Function(y)
                                                                                            Return y.Expiry.Value.Date = minExpiry.Date
                                                                                        End Function)
                    If currentIntrument IsNot Nothing Then
                        If currentNFOInstruments Is Nothing Then currentNFOInstruments = New List(Of IInstrument)
                        currentNFOInstruments.Add(currentIntrument)
                    End If
                Next
                If currentNFOInstruments IsNot Nothing AndAlso currentNFOInstruments.Count > 0 Then
                    File.Delete(_userInputs.InstrumentDetailsFilePath)
                    Using csv As New CSVHelper(_userInputs.InstrumentDetailsFilePath, ",", _cts)
                        _cts.Token.ThrowIfCancellationRequested()
                        Dim allStockData As DataTable = New DataTable
                        allStockData.Columns.Add("TRADING SYMBOL")

                        For Each runningStock In currentNFOInstruments
                            Dim row As DataRow = allStockData.NewRow
                            row("TRADING SYMBOL") = runningStock.TradingSymbol
                            allStockData.Rows.Add(row)
                        Next

                        csv.GetCSVFromDataTable(allStockData)
                    End Using
                    If _userInputs.InstrumentsData IsNot Nothing Then
                        _userInputs.InstrumentsData.Clear()
                        _userInputs.InstrumentsData = Nothing
                        _userInputs.FillInstrumentDetails(_userInputs.InstrumentDetailsFilePath, _cts)
                    End If
                End If
            End If
        End If
    End Function

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' TODO: set large fields to null.
        End If
        disposedValue = True
    End Sub

    ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        ' TODO: uncomment the following line if Finalize() is overridden above.
        ' GC.SuppressFinalize(Me)
    End Sub
#End Region
End Class