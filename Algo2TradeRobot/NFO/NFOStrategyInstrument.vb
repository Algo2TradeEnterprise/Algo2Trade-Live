Imports NLog
Imports Algo2TradeCore
Imports System.Threading
Imports Algo2TradeCore.Adapter
Imports Algo2TradeCore.Entities
Imports Algo2TradeCore.Strategies
Imports Algo2TradeCore.ChartHandler.ChartStyle
Imports System.IO

Public Class NFOStrategyInstrument
    Inherits StrategyInstrument
    Implements IDisposable

#Region "Logging and Status Progress"
    Public Shared Shadows logger As Logger = LogManager.GetCurrentClassLogger
#End Region

    Private _lastSerializeTime As Date = Date.MinValue
    Private _entryDirection As IOrder.TypeOfTransaction = IOrder.TypeOfTransaction.None
    Private _placedOrderID As String = Nothing
    Private ReadOnly _instrumentDetails As NFOUserInputs.InstrumentDetails
    Private ReadOnly _signalDetailsFilename As String
    Public ReadOnly Property SignalData As SignalDetails

    Public Sub New(ByVal associatedInstrument As IInstrument,
                   ByVal associatedParentStrategy As Strategy,
                   ByVal isPairInstrumnet As Boolean,
                   ByVal parentInstrument As Boolean,
                   ByVal canceller As CancellationTokenSource)
        MyBase.New(associatedInstrument, associatedParentStrategy, isPairInstrumnet, canceller)
        Select Case Me.ParentStrategy.ParentController.BrokerSource
            Case APISource.Zerodha
                _APIAdapter = New ZerodhaAdapter(ParentStrategy.ParentController, Me.TradableInstrument, _cts)
            Case APISource.AliceBlue
                _APIAdapter = New AliceAdapter(ParentStrategy.ParentController, Me.TradableInstrument, _cts)
            Case APISource.Upstox
                Throw New NotImplementedException
            Case Else
                Throw New NotImplementedException
        End Select
        AddHandler _APIAdapter.Heartbeat, AddressOf OnHeartbeat
        AddHandler _APIAdapter.WaitingFor, AddressOf OnWaitingFor
        AddHandler _APIAdapter.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
        AddHandler _APIAdapter.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete

        _instrumentDetails = CType(Me.ParentStrategy.UserSettings, NFOUserInputs).InstrumentsData(Me.TradableInstrument.TradingSymbol)
        _signalDetailsFilename = Path.Combine(My.Application.Info.DirectoryPath, String.Format("{0}.SignalDetails.a2t", Me.TradableInstrument.TradingSymbol))
    End Sub

    Public Overrides Function PopulateChartAndIndicatorsAsync(candleCreator As Chart, currentCandle As OHLCPayload) As Task
        Throw New NotImplementedException
    End Function

    Public Overrides Async Function MonitorAsync() As Task
        Try
            Me.TradableInstrument.FetchHistorical = False

            If File.Exists(_signalDetailsFilename) Then
                _SignalData = Utilities.Strings.DeserializeToCollection(Of SignalDetails)(_signalDetailsFilename)
            Else
                _SignalData = New SignalDetails
            End If
            _SignalData.TradingSymbol = Me.TradableInstrument.TradingSymbol
            _SignalData.HoldingsAveragePrice = Me.HoldingDetails.AveragePrice
            _SignalData.HoldingsQuantity = Me.HoldingDetails.Quantity + Me.HoldingDetails.T1Quantity
            If Me.PositionDetails IsNot Nothing Then
                If Me.PositionDetails.Quantity < 0 Then
                    _SignalData.HoldingsQuantity += Me.PositionDetails.Quantity
                End If
                _SignalData.PositionsAveragePrice = Me.PositionDetails.AveragePrice
                _SignalData.PositionsQuantity = Me.PositionDetails.Quantity
            Else
                _SignalData.PositionsAveragePrice = 0
                _SignalData.PositionsQuantity = 0
            End If
            Await SerializeSignalDetailsAsync().ConfigureAwait(False)

            _strategyInstrumentRunning = True

            While True
                If Me.ParentStrategy.ParentController.OrphanException IsNot Nothing Then
                    Throw Me.ParentStrategy.ParentController.OrphanException
                End If
                'If Me._RMSException IsNot Nothing AndAlso _RMSException.ExceptionType = Algo2TradeCore.Exceptions.AdapterBusinessException.TypeOfException.RMSError Then
                '    OnHeartbeat(String.Format("{0}:Will not take no more action in this instrument as RMS Error occured. Error-{1}", Me.TradableInstrument.TradingSymbol, _RMSException.Message))
                '    Throw Me._RMSException
                'End If
                _cts.Token.ThrowIfCancellationRequested()

                Await Task.Delay(5000, _cts.Token).ConfigureAwait(False)
            End While
        Catch ex As Exception
            'To log exceptions getting created from this function as the bubble up of the exception
            'will anyways happen to Strategy.MonitorAsync but it will not be shown until all tasks exit
            logger.Error("Strategy Instrument:{0}, error:{1}", Me.ToString, ex.ToString)
            Throw ex
        Finally
            OnHeartbeat(String.Format("{0} Strategy Instrument Stopped", Me.ToString))
            _strategyInstrumentRunning = False
        End Try
    End Function

    Public Overrides Async Function HandleTickTriggerToUIETCAsync() As Task
        MyBase.HandleTickTriggerToUIETCAsync()
        If Me.StrategyInstrumentRunning AndAlso Me.SignalData IsNot Nothing AndAlso Me.TradableInstrument.LastTick IsNot Nothing Then
            If Me.TradableInstrument.LastTick.Timestamp.Value >= Me.TradableInstrument.ExchangeDetails.ExchangeStartTime Then
                _cts.Token.ThrowIfCancellationRequested()
                If _placedOrderID IsNot Nothing AndAlso Me.OrderDetails.ContainsKey(_placedOrderID) Then
                    If Me.OrderDetails(_placedOrderID).ParentOrder.Status = IOrder.TypeOfStatus.Complete Then
                        _SignalData.ResetHighestLowestPoint()
                        _placedOrderID = Nothing
                    ElseIf Me.OrderDetails(_placedOrderID).ParentOrder.Status = IOrder.TypeOfStatus.Rejected Then
                        If Me._RMSException IsNot Nothing AndAlso _RMSException.ExceptionType = Algo2TradeCore.Exceptions.AdapterBusinessException.TypeOfException.RMSError Then
                            OnHeartbeat(String.Format("{0}:Will not take no more action in this instrument as RMS Error occured. Error-{1}", Me.TradableInstrument.TradingSymbol, _RMSException.Message))
                            Me.ParentStrategy.ParentController.OrphanException = Me._RMSException
                        End If
                    End If
                End If
                If Me.PositionDetails IsNot Nothing AndAlso Me.PositionDetails.Quantity <> _SignalData.PositionsQuantity Then
                    If Me.PositionDetails.Quantity < 0 AndAlso _SignalData.HoldingsQuantity = 1 Then
                        _SignalData.HoldingsQuantity += Me.PositionDetails.Quantity
                    End If

                    _SignalData.PositionsAveragePrice = Me.PositionDetails.AveragePrice
                    _SignalData.PositionsQuantity = Me.PositionDetails.Quantity

                    _SignalData.ResetHighestLowestPoint()

                    _SignalData.CurrentLTPTime = Me.TradableInstrument.LastTick.Timestamp.Value
                    _SignalData.CurrentLTP = Me.TradableInstrument.LastTick.LastPrice

                    _cts.Token.ThrowIfCancellationRequested()
                    Await SerializeSignalDetailsAsync().ConfigureAwait(False)

                    OnHeartbeat(Me.SignalData.ToString)
                ElseIf Me.TradableInstrument.LastTick.Timestamp.Value >= _lastSerializeTime.AddMinutes(1) Then
                    _SignalData.CurrentLTPTime = Me.TradableInstrument.LastTick.Timestamp.Value
                    _SignalData.CurrentLTP = Me.TradableInstrument.LastTick.LastPrice

                    _cts.Token.ThrowIfCancellationRequested()
                    Await SerializeSignalDetailsAsync().ConfigureAwait(False)

                    OnHeartbeat(Me.SignalData.ToString)
                End If

                _cts.Token.ThrowIfCancellationRequested()
                If _SignalData.TotalQuantity = 1 Then
                    If _SignalData.DownwardDropPercentage <= Math.Abs(_instrumentDetails.DownwardDropPercentage) * -1 Then
                        If _SignalData.DownwardNetRisePercentage >= Math.Abs(_instrumentDetails.DownwardRisePercentage) Then
                            OnHeartbeat(String.Format("********** {0}. Place Order ID:{1}", Me.SignalData.ToString, If(_placedOrderID, "Nothing")))
                            If _placedOrderID Is Nothing Then
                                _entryDirection = IOrder.TypeOfTransaction.Buy
                                Dim orderResponse = Await ExecuteCommandAsync(ExecuteCommands.PlaceRegularMarketCNCOrder, Nothing).ConfigureAwait(False)
                                If orderResponse IsNot Nothing AndAlso orderResponse.Count > 0 Then
                                    Dim placeOrderResponse = CType(orderResponse, Concurrent.ConcurrentBag(Of Object)).FirstOrDefault
                                    If placeOrderResponse.ContainsKey("data") AndAlso
                                    placeOrderResponse("data").ContainsKey("order_id") Then
                                        _entryDirection = IOrder.TypeOfTransaction.None
                                        _placedOrderID = placeOrderResponse("data")("order_id")
                                        '_SignalData.ResetHighestLowestPoint()
                                    End If
                                End If
                            End If
                        End If
                    End If
                ElseIf _SignalData.TotalQuantity > 1 Then
                    If _SignalData.UpwardRisePercentage >= Math.Abs(_instrumentDetails.UpwardRisePercentage) Then
                        If _SignalData.UpwardNetDropPercentage <= Math.Abs(_instrumentDetails.UpwardDropPercentage) * -1 Then
                            OnHeartbeat(String.Format("********** {0}. Place Order ID:{1}", Me.SignalData.ToString, If(_placedOrderID, "Nothing")))
                            If _placedOrderID Is Nothing Then
                                _entryDirection = IOrder.TypeOfTransaction.Sell
                                Dim orderResponse = Await ExecuteCommandAsync(ExecuteCommands.PlaceRegularMarketCNCOrder, Nothing).ConfigureAwait(False)
                                If orderResponse IsNot Nothing AndAlso orderResponse.Count > 0 Then
                                    Dim placeOrderResponse = CType(orderResponse, Concurrent.ConcurrentBag(Of Object)).FirstOrDefault
                                    If placeOrderResponse.ContainsKey("data") AndAlso
                                    placeOrderResponse("data").ContainsKey("order_id") Then
                                        _entryDirection = IOrder.TypeOfTransaction.None
                                        _placedOrderID = placeOrderResponse("data")("order_id")
                                        '_SignalData.ResetHighestLowestPoint()
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
            End If
        End If
    End Function

    Protected Overrides Async Function IsTriggerReceivedForPlaceOrderAsync(forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)))
        Dim ret As List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        Dim currentTime As Date = Now()
        Dim parameters As PlaceOrderParameters = Nothing
        If currentTime >= Me.TradableInstrument.ExchangeDetails.ExchangeStartTime AndAlso currentTime <= Me.TradableInstrument.ExchangeDetails.ExchangeEndTime Then
            Dim signalCandle As OHLCPayload = New OHLCPayload(OHLCPayload.PayloadSource.CalculatedTick)
            signalCandle.SnapshotDateTime = Me.SignalData.CurrentLTPTime
            signalCandle.OpenPrice.Value = Me.SignalData.CurrentLTP
            signalCandle.LowPrice.Value = Me.SignalData.CurrentLTP
            signalCandle.HighPrice.Value = Me.SignalData.CurrentLTP
            signalCandle.ClosePrice.Value = Me.SignalData.CurrentLTP
            signalCandle.Volume.Value = 1

            If signalCandle IsNot Nothing Then
                If _entryDirection = IOrder.TypeOfTransaction.Buy Then
                    parameters = New PlaceOrderParameters(signalCandle) With
                                 {
                                    .EntryDirection = IOrder.TypeOfTransaction.Buy,
                                    .OrderType = IOrder.TypeOfOrder.Market,
                                    .Quantity = Math.Ceiling(_instrumentDetails.Capital / Me.SignalData.CurrentLTP)
                                 }
                ElseIf _entryDirection = IOrder.TypeOfTransaction.Sell Then
                    parameters = New PlaceOrderParameters(signalCandle) With
                                 {
                                    .EntryDirection = IOrder.TypeOfTransaction.Sell,
                                    .OrderType = IOrder.TypeOfOrder.Market,
                                    .Quantity = Me.SignalData.TotalQuantity - 1
                                 }
                End If
                If forcePrint Then OnHeartbeat(Me.SignalData.ToString)
            End If
        End If

        'Below portion have to be done in every place order trigger
        If parameters IsNot Nothing Then
            Try
                If forcePrint Then
                    logger.Debug("PlaceOrder-> ************************************************ {0}", Me.TradableInstrument.TradingSymbol)
                    logger.Debug("PlaceOrder Parameters-> {0},{1}", parameters.ToString, Me.TradableInstrument.TradingSymbol)
                End If
            Catch ex As Exception
                logger.Error(ex)
            End Try

            Dim currentSignalActivities As IEnumerable(Of ActivityDashboard) = Me.ParentStrategy.SignalManager.GetActiveSignalActivities(Me.TradableInstrument.InstrumentIdentifier)
            If currentSignalActivities IsNot Nothing AndAlso currentSignalActivities.Count > 0 Then
                'Dim placedActivities As IEnumerable(Of ActivityDashboard) = currentSignalActivities.Where(Function(x)
                '                                                                                              Return x.EntryActivity.RequestRemarks = parameters.ToString
                '                                                                                          End Function)
                Dim placedActivities As IEnumerable(Of ActivityDashboard) = currentSignalActivities
                If placedActivities IsNot Nothing AndAlso placedActivities.Count > 0 Then
                    Dim lastPlacedActivity As ActivityDashboard = placedActivities.OrderBy(Function(x)
                                                                                               Return x.EntryActivity.RequestTime
                                                                                           End Function).LastOrDefault
                    If lastPlacedActivity.EntryActivity.RequestStatus = ActivityDashboard.SignalStatusType.Discarded AndAlso
                            lastPlacedActivity.EntryActivity.LastException IsNot Nothing AndAlso
                            lastPlacedActivity.EntryActivity.LastException.Message.ToUpper.Contains("TIME") Then
                        Await Task.Delay(Me.ParentStrategy.ParentController.UserInputs.BackToBackOrderCoolOffDelay * 1000, _cts.Token).ConfigureAwait(False)

                        If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                        ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.WaitAndTake, parameters, parameters.ToString))
                    ElseIf lastPlacedActivity.EntryActivity.RequestStatus = ActivityDashboard.SignalStatusType.Handled Then
                        If lastPlacedActivity.SignalDirection = parameters.EntryDirection Then
                            'If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                            'ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.DonotTake, parameters, parameters.ToString))
                            Try
                                logger.Debug("Will not take this trade as similar previous trade detected. Current Trade->{0}, Last Activity Direction:{1}, Last Activity Status:{2}, Trading Symbol:{3}",
                                             parameters.ToString, lastPlacedActivity.SignalDirection, lastPlacedActivity.EntryActivity.RequestStatus, Me.TradableInstrument.TradingSymbol)
                            Catch ex As Exception
                                logger.Warn(ex.ToString)
                            End Try
                        Else
                            If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                            ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.Take, parameters, parameters.ToString))
                        End If
                    ElseIf lastPlacedActivity.EntryActivity.RequestStatus = ActivityDashboard.SignalStatusType.Activated Then
                        If lastPlacedActivity.SignalDirection = parameters.EntryDirection Then
                            'If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                            'ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.DonotTake, parameters, parameters.ToString))
                            Try
                                logger.Debug("Will not take this trade as similar previous trade detected. Current Trade->{0}, Last Activity Direction:{1}, Last Activity Status:{2}, Trading Symbol:{3}",
                                             parameters.ToString, lastPlacedActivity.SignalDirection, lastPlacedActivity.EntryActivity.RequestStatus, Me.TradableInstrument.TradingSymbol)
                            Catch ex As Exception
                                logger.Warn(ex.ToString)
                            End Try
                        Else
                            If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                            ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.Take, parameters, parameters.ToString))
                        End If
                    ElseIf lastPlacedActivity.EntryActivity.RequestStatus = ActivityDashboard.SignalStatusType.Rejected Then
                        'If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                        'ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.DonotTake, parameters, parameters.ToString))
                    Else
                        If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                        ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.Take, parameters, parameters.ToString))
                    End If
                Else
                    If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                    ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.Take, parameters, parameters.ToString))
                End If
            Else
                If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.Take, parameters, parameters.ToString))
            End If
        End If
        Return ret
    End Function

#Region "Signal Details"
    Public Async Function SerializeSignalDetailsAsync() As Task(Of Boolean)
        Await Task.Delay(0).ConfigureAwait(False)
        Dim ret As Boolean = False
        If Me.SignalData IsNot Nothing Then
            Utilities.Strings.SerializeFromCollection(Of SignalDetails)(_signalDetailsFilename, Me.SignalData)
            _lastSerializeTime = Me.SignalData.CurrentLTPTime
            ret = True
        End If
        Return ret
    End Function

    <Serializable>
    Public Class SignalDetails
        Public Property TradingSymbol As String
        Public Property HoldingsAveragePrice As Decimal
        Public Property HoldingsQuantity As Long
        Public Property PositionsAveragePrice As Decimal
        Public Property PositionsQuantity As Long
        Public Property CurrentLTPTime As Date

        Private _CurrentLTP As Decimal
        Public Property CurrentLTP As Decimal
            Get
                Return _CurrentLTP
            End Get
            Set(value As Decimal)
                _CurrentLTP = value
                _LowestPoint = Math.Min(_LowestPoint, _CurrentLTP)
                _HighestPoint = Math.Max(_HighestPoint, _CurrentLTP)
            End Set
        End Property

        Private _LowestPoint As Decimal = Decimal.MaxValue
        Public ReadOnly Property LowestPoint As Decimal
            Get
                Return _LowestPoint
            End Get
        End Property

        Private _HighestPoint As Decimal = Decimal.MinValue
        Public ReadOnly Property HighestPoint As Decimal
            Get
                Return _HighestPoint
            End Get
        End Property

        Public ReadOnly Property TotalQuantity As Decimal
            Get
                Return Me.HoldingsQuantity + Me.PositionsQuantity
            End Get
        End Property

        Public ReadOnly Property AveragePrice As Decimal
            Get
                Return (Me.HoldingsAveragePrice * Me.HoldingsQuantity + Me.PositionsAveragePrice * Me.PositionsQuantity) / Me.TotalQuantity
            End Get
        End Property

        Public ReadOnly Property DownwardDropPercentage As Decimal
            Get
                Return ((Me.LowestPoint / Me.AveragePrice) - 1) * 100
            End Get
        End Property

        Public ReadOnly Property DownwardNetRisePercentage As Decimal
            Get
                Return ((Me.CurrentLTP / Me.LowestPoint) - 1) * 100
            End Get
        End Property

        Public ReadOnly Property UpwardRisePercentage As Decimal
            Get
                Return ((Me.HighestPoint / Me.AveragePrice) - 1) * 100
            End Get
        End Property

        Public ReadOnly Property UpwardNetDropPercentage As Decimal
            Get
                Return ((Me.CurrentLTP / Me.HighestPoint) - 1) * 100
            End Get
        End Property

        Public Sub ResetHighestLowestPoint()
            _HighestPoint = Decimal.MinValue
            _LowestPoint = Decimal.MaxValue
        End Sub

        Public Overrides Function ToString() As String
            If Me.TotalQuantity > 1 Then
                Return String.Format("{0}: Total Quantity={1}, Average Price={2}, Highest Point={3}, LTP={4}, Upward Rise %={5}%, Upward Drop %={6}%. Holdings Avg Price={7}, Holdings Quantity={8}, Positions Avg Price={9}, Positions Quantity={10}",
                                     Me.TradingSymbol, Me.TotalQuantity, Math.Round(Me.AveragePrice, 2), Math.Round(Me.HighestPoint, 2), Math.Round(Me.CurrentLTP, 2), Math.Round(Me.UpwardRisePercentage, 2), Math.Round(Me.UpwardNetDropPercentage, 2),
                                     Math.Round(Me.HoldingsAveragePrice, 2), Math.Round(Me.HoldingsQuantity, 2), Math.Round(Me.PositionsAveragePrice, 2), Math.Round(Me.PositionsQuantity, 2))
            Else
                Return String.Format("{0}: Total Quantity={1}, Average Price={2}, Lowest Point={3}, LTP={4}, Downward Drop %={5}%, Downward Rise %={6}%. Holdings Avg Price={7}, Holdings Quantity={8}, Positions Avg Price={9}, Positions Quantity={10}",
                                     Me.TradingSymbol, Me.TotalQuantity, Math.Round(Me.AveragePrice, 2), Math.Round(Me.LowestPoint, 2), Math.Round(Me.CurrentLTP, 2), Math.Round(Me.DownwardDropPercentage, 2), Math.Round(Me.DownwardNetRisePercentage, 2),
                                     Math.Round(Me.HoldingsAveragePrice, 2), Math.Round(Me.HoldingsQuantity, 2), Math.Round(Me.PositionsAveragePrice, 2), Math.Round(Me.PositionsQuantity, 2))
            End If
        End Function
    End Class
#End Region

#Region "Not required functions"
    Public Overrides Function MonitorAsync(command As ExecuteCommands, data As Object) As Task
        Throw New NotImplementedException
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