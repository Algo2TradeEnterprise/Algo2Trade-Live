Imports System.Threading
Imports Algo2TradeCore.Adapter
Imports Algo2TradeCore.Entities
Imports Algo2TradeCore.Strategies
Imports Utilities.Numbers
Imports NLog
Imports Algo2TradeCore.Entities.Indicators
Imports Algo2TradeCore.ChartHandler.ChartStyle
Imports Algo2TradeCore

Public Class NFOStrategyInstrument
    Inherits StrategyInstrument
    Implements IDisposable

#Region "Logging and Status Progress"
    Public Shared Shadows logger As Logger = LogManager.GetCurrentClassLogger
#End Region

    Private _changePer As Decimal = 3

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

    Public Overrides Async Function PopulateChartAndIndicatorsAsync(candleCreator As Chart, currentCandle As OHLCPayload) As Task
        If RawPayloadDependentConsumers IsNot Nothing AndAlso RawPayloadDependentConsumers.Count > 0 Then
        End If
    End Function

    Public Overrides Async Function MonitorAsync() As Task
        Try
            _strategyInstrumentRunning = True
            If Me.TradableInstrument.InstrumentType = IInstrument.TypeOfInstrument.Futures Then
                Dim myCashInstrument As StrategyInstrument = Me.ParentStrategy.TradableStrategyInstruments.Where(Function(x)
                                                                                                                     Return x.TradableInstrument.InstrumentType = IInstrument.TypeOfInstrument.Cash AndAlso
                                                                                                                        x.TradableInstrument.TradingSymbol = Me.TradableInstrument.RawInstrumentName
                                                                                                                 End Function).FirstOrDefault
                If myCashInstrument IsNot Nothing Then
                    While True
                        If Me.ParentStrategy.ParentController.OrphanException IsNot Nothing Then
                            Throw Me.ParentStrategy.ParentController.OrphanException
                        End If
                        _cts.Token.ThrowIfCancellationRequested()

                        If Now >= Me.ParentStrategy.UserSettings.TradeStartTime AndAlso Now <= Me.ParentStrategy.UserSettings.LastTradeEntryTime Then
                            Dim futureTick As ITick = Me.TradableInstrument.LastTick
                            Dim cashTick As ITick = myCashInstrument.TradableInstrument.LastTick
                            If cashTick.LastPrice > futureTick.LastPrice Then
                                Dim cashBid As Decimal = CType(cashTick, ZerodhaTick).WrappedTick.Bids.FirstOrDefault.Price
                                Dim futureAsk As Decimal = CType(futureTick, ZerodhaTick).WrappedTick.Offers.FirstOrDefault.Price
                                Dim changePer As Decimal = Math.Round(((cashBid - futureAsk) / futureAsk) * 100, 2)
                                If changePer >= _changePer Then
                                    logger.Fatal("{0}:SELL Cash LTP={1}, Future LTP={2}, Cash Bid={3}, Future Ask={4}, Change%={5}, Tick Time={6}",
                                                  myCashInstrument.TradableInstrument.TradingSymbol,
                                                  cashTick.LastPrice,
                                                  futureTick.LastPrice,
                                                  cashBid,
                                                  futureAsk,
                                                  changePer,
                                                  cashTick.Timestamp.Value.ToString("dd-MMM-yyyy HH:mm:ss"))
                                End If
                            ElseIf cashTick.LastPrice < futureTick.LastPrice Then
                                Dim cashAsk As Decimal = CType(cashTick, ZerodhaTick).WrappedTick.Offers.FirstOrDefault.Price
                                Dim futureBid As Decimal = CType(futureTick, ZerodhaTick).WrappedTick.Bids.FirstOrDefault.Price
                                Dim changePer As Decimal = Math.Round(((futureBid - cashAsk) / cashAsk) * 100, 2)
                                If changePer >= _changePer Then
                                    logger.Fatal("{0}:BUY Cash LTP={1}, Future LTP={2}, Cash Ask={3}, Future Bid={4}, Change%={5}, Tick Time={6}",
                                                  myCashInstrument.TradableInstrument.TradingSymbol,
                                                  cashTick.LastPrice,
                                                  futureTick.LastPrice,
                                                  cashAsk,
                                                  futureBid,
                                                  changePer,
                                                  cashTick.Timestamp.Value.ToString("dd-MMM-yyyy HH:mm:ss"))
                                End If
                            End If
                        End If

                        _cts.Token.ThrowIfCancellationRequested()
                        Await Task.Delay(1000, _cts.Token).ConfigureAwait(False)
                    End While
                End If
            End If
        Catch ex As Exception
            'To log exceptions getting created from this function as the bubble up of the exception
            'will anyways happen to Strategy.MonitorAsync but it will not be shown until all tasks exit
            logger.Error("Strategy Instrument:{0} stopped, error:{1}", Me.ToString, ex.ToString)
            Throw ex
        Finally
            _strategyInstrumentRunning = False
        End Try
    End Function

    Protected Overrides Function IsTriggerReceivedForPlaceOrderAsync(ByVal forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)))
        Throw New NotImplementedException
    End Function

    Protected Overrides Function IsTriggerReceivedForModifyStoplossOrderAsync(ByVal forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)))
        Throw New NotImplementedException
    End Function

    Protected Overrides Function IsTriggerReceivedForModifyTargetOrderAsync(forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)))
        Throw New NotImplementedException
    End Function

    Protected Overrides Function IsTriggerReceivedForExitOrderAsync(ByVal forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, String)))
        Throw New NotImplementedException
    End Function

    Public Overrides Function MonitorAsync(ByVal command As ExecuteCommands, ByVal data As Object) As Task
        Throw New NotImplementedException()
    End Function

    Protected Overrides Function IsTriggerReceivedForPlaceOrderAsync(forcePrint As Boolean, data As Object) As Task(Of List(Of Tuple(Of ExecuteCommandAction, StrategyInstrument, PlaceOrderParameters, String)))
        Throw New NotImplementedException()
    End Function

    Protected Overrides Function IsTriggerReceivedForExitOrderAsync(forcePrint As Boolean, data As Object) As Task(Of List(Of Tuple(Of ExecuteCommandAction, StrategyInstrument, IOrder, String)))
        Throw New NotImplementedException()
    End Function

    Protected Overrides Function ForceExitSpecificTradeAsync(order As IOrder, ByVal reason As String) As Task
        Throw New NotImplementedException
    End Function

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