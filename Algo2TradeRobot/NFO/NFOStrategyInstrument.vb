Imports NLog
Imports System.IO
Imports System.Threading
Imports Algo2TradeCore.Adapter
Imports Algo2TradeCore.Entities
Imports Algo2TradeCore.Strategies
Imports Algo2TradeCore.ChartHandler.ChartStyle
Imports System.Text

Public Class NFOStrategyInstrument
    Inherits StrategyInstrument
    Implements IDisposable

#Region "Logging and Status Progress"
    Public Shared Shadows logger As Logger = LogManager.GetCurrentClassLogger
#End Region

    Public Sub New(ByVal associatedInstrument As IInstrument,
                   ByVal associatedParentStrategy As Strategy,
                   ByVal isPairInstrumnet As Boolean,
                   ByVal canceller As CancellationTokenSource)
        MyBase.New(associatedInstrument, associatedParentStrategy, isPairInstrumnet, canceller)
        Select Case Me.ParentStrategy.ParentController.BrokerSource
            Case APISource.Zerodha
                _APIAdapter = New ZerodhaAdapter(ParentStrategy.ParentController, _cts)
            Case APISource.AliceBlue
                _APIAdapter = New AliceAdapter(ParentStrategy.ParentController, Me.TradableInstrument, _cts)
            Case APISource.Upstox
                Throw New NotImplementedException
            Case APISource.None
                Throw New NotImplementedException
        End Select
        AddHandler _APIAdapter.Heartbeat, AddressOf OnHeartbeat
        AddHandler _APIAdapter.WaitingFor, AddressOf OnWaitingFor
        AddHandler _APIAdapter.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
        AddHandler _APIAdapter.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete
    End Sub

#Region "Signal Check"
    Public Async Function CheckSignalAsync() As Task
        Try
            Dim folderPath As String = Path.Combine(My.Application.Info.DirectoryPath, "Bid Ask Log")
            If Not Directory.Exists(folderPath) Then Directory.CreateDirectory(folderPath)
            Dim filename As String = Path.Combine(folderPath, String.Format("Bid Ask Log {0}.csv", Now.ToString("ddMMMyyyy")))
            WriteCSVColumn(filename)
            While True
                If Me.ParentStrategy.ParentController.OrphanException IsNot Nothing Then
                    Throw Me.ParentStrategy.ParentController.OrphanException
                End If
                If Me._RMSException IsNot Nothing AndAlso
                    _RMSException.ExceptionType = Algo2TradeCore.Exceptions.AdapterBusinessException.TypeOfException.RMSError Then
                    OnHeartbeat(String.Format("{0}:Will not take no more action in this instrument as RMS Error occured. Error-{1}", Me.TradableInstrument.TradingSymbol, _RMSException.Message))
                    Throw Me._RMSException
                End If
                _cts.Token.ThrowIfCancellationRequested()

                Dim allStrategyInstruments As IEnumerable(Of StrategyInstrument) = Me.ParentStrategy.TradableStrategyInstruments
                If allStrategyInstruments IsNot Nothing AndAlso allStrategyInstruments.Count > 0 Then
                    For Each runningStrategyInstrument In allStrategyInstruments
                        _cts.Token.ThrowIfCancellationRequested()
                        Dim currentTick As ITick = runningStrategyInstrument.TradableInstrument.LastTick
                        If currentTick IsNot Nothing Then
                            If currentTick.Timestamp >= runningStrategyInstrument.TradableInstrument.ExchangeDetails.ExchangeStartTime AndAlso
                                currentTick.Timestamp <= runningStrategyInstrument.TradableInstrument.ExchangeDetails.ExchangeEndTime Then
                                WriteToCSV(filename, runningStrategyInstrument.TradableInstrument.TradingSymbol, currentTick)
                            End If
                        End If
                    Next
                End If

                _cts.Token.ThrowIfCancellationRequested()
                Await Task.Delay(1000 * 60, _cts.Token).ConfigureAwait(False)
            End While
        Catch ex As Exception
            'To log exceptions getting created from this function as the bubble up of the exception
            'will anyways happen to Strategy.MonitorAsync but it will not be shown until all tasks exit
            logger.Error("Strategy Instrument:{0}, error:{1}", Me.ToString, ex.ToString)
            Throw ex
        End Try
    End Function

    Private Sub WriteToCSV(ByVal filename As String, ByVal instrumentName As String, ByVal currentTick As ITick)
        Dim sb As StringBuilder = New StringBuilder
        sb.AppendLine()
        sb.Append(instrumentName)
        sb.Append(",")
        sb.Append(currentTick.Timestamp.Value.ToString("dd-MMM-yyyy HH:mm:ss"))
        sb.Append(",")
        sb.Append(currentTick.Open)
        sb.Append(",")
        sb.Append(currentTick.Low)
        sb.Append(",")
        sb.Append(currentTick.High)
        sb.Append(",")
        sb.Append(currentTick.Close)
        sb.Append(",")
        sb.Append(currentTick.LastPrice)
        sb.Append(",")
        sb.Append(currentTick.Volume)
        sb.Append(",")
        sb.Append(currentTick.BuyQuantity)
        sb.Append(",")
        sb.Append(currentTick.SellQuantity)
        sb.Append(",")
        If currentTick.SellQuantity <> UInteger.MaxValue AndAlso currentTick.SellQuantity <> UInteger.MinValue AndAlso currentTick.SellQuantity <> 0 Then
            sb.Append(currentTick.BuyQuantity / currentTick.SellQuantity)
        End If
        sb.Append(",")
        If currentTick.BuyQuantity <> UInteger.MaxValue AndAlso currentTick.BuyQuantity <> UInteger.MinValue AndAlso currentTick.BuyQuantity <> 0 Then
            sb.Append(currentTick.SellQuantity / currentTick.BuyQuantity)
        End If

        File.AppendAllText(filename, sb.ToString())
    End Sub

    Private Sub WriteCSVColumn(ByVal filename As String)
        If Not File.Exists(filename) Then
            Dim sb As StringBuilder = New StringBuilder
            sb.Append("Instrument Name")
            sb.Append(",")
            sb.Append("Date Time")
            sb.Append(",")
            sb.Append("Open")
            sb.Append(",")
            sb.Append("Low")
            sb.Append(",")
            sb.Append("High")
            sb.Append(",")
            sb.Append("Close")
            sb.Append(",")
            sb.Append("LTP")
            sb.Append(",")
            sb.Append("Volume")
            sb.Append(",")
            sb.Append("Bid Quantity")
            sb.Append(",")
            sb.Append("Ask Quantity")
            sb.Append(",")
            sb.Append("Bid To Ask Ratio")
            sb.Append(",")
            sb.Append("Ask To Bid Ratio")

            File.AppendAllText(filename, sb.ToString())
        End If
    End Sub
#End Region

#Region "Not Required For This Strategy"

    Public Overrides Function MonitorAsync() As Task
        Throw New NotImplementedException
    End Function

    Protected Overrides Function IsTriggerReceivedForPlaceOrderAsync(ByVal forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)))
        Throw New NotImplementedException
    End Function

    Protected Overrides Function IsTriggerReceivedForModifyStoplossOrderAsync(ByVal forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)))
        Throw New NotImplementedException
    End Function

    Protected Overrides Function IsTriggerReceivedForExitOrderAsync(ByVal forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, String)))
        Throw New NotImplementedException
    End Function

    Protected Overrides Function ForceExitSpecificTradeAsync(order As IOrder, ByVal reason As String) As Task
        Throw New NotImplementedException
    End Function

    Public Overrides Function PopulateChartAndIndicatorsAsync(candleCreator As Chart, currentCandle As OHLCPayload) As Task
        Throw New NotImplementedException()
    End Function

    Public Overrides Function MonitorAsync(ByVal command As ExecuteCommands, ByVal data As Object) As Task
        Throw New NotImplementedException()
    End Function

    Protected Overrides Function IsTriggerReceivedForModifyTargetOrderAsync(forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)))
        Throw New NotImplementedException
    End Function

    Protected Overrides Function IsTriggerReceivedForPlaceOrderAsync(forcePrint As Boolean, data As Object) As Task(Of List(Of Tuple(Of ExecuteCommandAction, StrategyInstrument, PlaceOrderParameters, String)))
        Throw New NotImplementedException()
    End Function

    Protected Overrides Function IsTriggerReceivedForExitOrderAsync(forcePrint As Boolean, data As Object) As Task(Of List(Of Tuple(Of ExecuteCommandAction, StrategyInstrument, IOrder, String)))
        Throw New NotImplementedException()
    End Function
#End Region

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
                If _APIAdapter IsNot Nothing Then
                    RemoveHandler _APIAdapter.Heartbeat, AddressOf OnHeartbeat
                    RemoveHandler _APIAdapter.WaitingFor, AddressOf OnWaitingFor
                    RemoveHandler _APIAdapter.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
                    RemoveHandler _APIAdapter.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete
                End If
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