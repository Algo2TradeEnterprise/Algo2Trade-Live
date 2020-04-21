Imports System.IO
Imports System.Threading
Imports Algo2TradeCore.Adapter
Imports Algo2TradeCore.Entities
Imports Algo2TradeCore.Strategies
Imports NLog

Public Class NFOStrategyInstrument
    Inherits StrategyInstrument

#Region "Logging and Status Progress"
    Public Shared Shadows logger As Logger = LogManager.GetCurrentClassLogger
#End Region

    Private ReadOnly _folderPath As String
    Private ReadOnly _userSettings As NFOUserInputs
    Public BidAskCollection As Concurrent.ConcurrentDictionary(Of Date, BidAsk)
    Public ReadOnly SheetName As String
    Public ReadyToExport As Boolean = False

    Public Sub New(ByVal associatedInstrument As IInstrument,
                   ByVal associatedParentStrategy As Strategy,
                   ByVal isPairInstrumnet As Boolean,
                   ByVal canceller As CancellationTokenSource,
                   ByVal folderpath As String,
                   ByVal sheetName As String)
        MyBase.New(associatedInstrument, associatedParentStrategy, isPairInstrumnet, canceller)
        Select Case Me.ParentStrategy.ParentController.BrokerSource
            Case APISource.Zerodha
                _APIAdapter = New ZerodhaAdapter(ParentStrategy.ParentController, _cts)
            Case APISource.Upstox
                Throw New NotImplementedException
            Case APISource.None
                Throw New NotImplementedException
        End Select
        AddHandler _APIAdapter.Heartbeat, AddressOf OnHeartbeat
        AddHandler _APIAdapter.WaitingFor, AddressOf OnWaitingFor
        AddHandler _APIAdapter.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
        AddHandler _APIAdapter.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete
        RawPayloadDependentConsumers = New List(Of IPayloadConsumer)
        If Me.ParentStrategy.IsStrategyCandleStickBased Then
            If Me.ParentStrategy.UserSettings.SignalTimeFrame > 0 Then
                Dim chartConsumer As PayloadToChartConsumer = New PayloadToChartConsumer(Me.ParentStrategy.UserSettings.SignalTimeFrame)
                RawPayloadDependentConsumers.Add(chartConsumer)
            Else
                Throw New ApplicationException(String.Format("Signal Timeframe is 0 or Nothing, does not adhere to the strategy:{0}", Me.ParentStrategy.ToString))
            End If
        End If
        _userSettings = Me.ParentStrategy.UserSettings
        _folderPath = folderpath
        Me.SheetName = sheetName
        Me.BidAskCollection = New Concurrent.ConcurrentDictionary(Of Date, BidAsk)
    End Sub

    Public Overrides Async Function MonitorAsync() As Task
        Try
            Dim filename As String = Path.Combine(_folderPath, String.Format("{0}.a2t", Me.ToString))
            If File.Exists(filename) Then
                'DeSerialization
                BidAskCollection = Utilities.Strings.DeserializeToCollection(Of Concurrent.ConcurrentDictionary(Of Date, BidAsk))(filename)
            End If

            While True
                Me.ReadyToExport = True
                If Me.ParentStrategy.ParentController.OrphanException IsNot Nothing Then
                    Throw Me.ParentStrategy.ParentController.OrphanException
                End If
                _cts.Token.ThrowIfCancellationRequested()
                'Serialization
                If Me.TradableInstrument.LastTick.Timestamp >= Me.TradableInstrument.ExchangeDetails.ExchangeStartTime AndAlso
                    Me.TradableInstrument.LastTick.Timestamp <= Me.TradableInstrument.ExchangeDetails.ExchangeEndTime Then
                    Utilities.Strings.SerializeFromCollection(Of Concurrent.ConcurrentDictionary(Of Date, BidAsk))(filename, BidAskCollection)
                End If

                _cts.Token.ThrowIfCancellationRequested()
                Await Task.Delay(60000, _cts.Token).ConfigureAwait(False)
            End While
        Catch ex As Exception
            'To log exceptions getting created from this function as the bubble up of the exception
            'will anyways happen to Strategy.MonitorAsync but it will not be shown until all tasks exit
            logger.Error("Strategy Instrument:{0}, error:{1}", Me.ToString, ex.ToString)
            Throw ex
        End Try
    End Function

    Public Overrides Async Function HandleTickTriggerToUIETCAsync() As Task
        _cts.Token.ThrowIfCancellationRequested()
        StoreBidAskAsync()
    End Function

    Private Async Function StoreBidAskAsync() As Task
        _cts.Token.ThrowIfCancellationRequested()
        Dim lastTick As ITick = Me.TradableInstrument.LastTick
        If lastTick Is Nothing OrElse lastTick.Timestamp Is Nothing OrElse lastTick.Timestamp.Value = Date.MinValue OrElse lastTick.Timestamp.Value = New Date(1970, 1, 1, 5, 30, 0) Then
            Exit Function
        End If
        _cts.Token.ThrowIfCancellationRequested()
        If lastTick.Timestamp >= Me.TradableInstrument.ExchangeDetails.ExchangeStartTime AndAlso
            lastTick.Timestamp <= Me.TradableInstrument.ExchangeDetails.ExchangeEndTime Then
            Dim bidAskData As BidAsk = New BidAsk With {
                .SnapshotDateTime = lastTick.Timestamp.Value,
                .Bid = lastTick.FirstBidPrice,
                .Ask = lastTick.FirstOfferPrice
            }
            _cts.Token.ThrowIfCancellationRequested()
            BidAskCollection.AddOrUpdate(bidAskData.SnapshotDateTime, bidAskData, Function(key, value) bidAskData)
            _cts.Token.ThrowIfCancellationRequested()
        End If
    End Function

#Region "Class"
    <Serializable>
    Public Class BidAsk
        Public SnapshotDateTime As Date
        Public Bid As Decimal
        Public Ask As Decimal
    End Class
#End Region

#Region "Not Needed"
    Public Overrides Function MonitorAsync(command As ExecuteCommands, data As Object) As Task
        Throw New NotImplementedException()
    End Function

    Protected Overrides Function IsTriggerReceivedForPlaceOrderAsync(forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)))
        Throw New NotImplementedException()
    End Function

    Protected Overrides Function IsTriggerReceivedForPlaceOrderAsync(forcePrint As Boolean, data As Object) As Task(Of List(Of Tuple(Of ExecuteCommandAction, StrategyInstrument, PlaceOrderParameters, String)))
        Throw New NotImplementedException()
    End Function

    Protected Overrides Function IsTriggerReceivedForModifyStoplossOrderAsync(forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)))
        Throw New NotImplementedException()
    End Function

    Protected Overrides Function IsTriggerReceivedForModifyTargetOrderAsync(forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)))
        Throw New NotImplementedException()
    End Function

    Protected Overrides Function IsTriggerReceivedForExitOrderAsync(forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, String)))
        Throw New NotImplementedException()
    End Function

    Protected Overrides Function IsTriggerReceivedForExitOrderAsync(forcePrint As Boolean, data As Object) As Task(Of List(Of Tuple(Of ExecuteCommandAction, StrategyInstrument, IOrder, String)))
        Throw New NotImplementedException()
    End Function

    Protected Overrides Function ForceExitSpecificTradeAsync(order As IOrder, reason As String) As Task
        Throw New NotImplementedException()
    End Function
#End Region
End Class