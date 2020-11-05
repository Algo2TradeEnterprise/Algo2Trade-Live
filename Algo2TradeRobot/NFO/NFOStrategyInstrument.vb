Imports NLog
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


    Private ReadOnly _signalDetailsFilename As String = Path.Combine(My.Application.Info.DirectoryPath, String.Format("{0}.SignalDetails.a2t", Me.TradableInstrument.TradingSymbol))
    Private _allSignalDetails As Dictionary(Of Date, SignalDetails) = Nothing
    Public ReadOnly Property AllSignalDetails As Dictionary(Of Date, SignalDetails)
        Get
            Return _allSignalDetails
        End Get
    End Property


    Private _lastTick As ITick = Nothing
    Private _entryDoneForTheDay As Boolean = False
    Private _tempSignal As SignalDetails = Nothing

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

    Public Overrides Function PopulateChartAndIndicatorsAsync(candleCreator As Chart, currentCandle As OHLCPayload) As Task
        Throw New NotImplementedException
    End Function

    Public Overrides Async Function MonitorAsync() As Task
        Try
            Me.TradableInstrument.FetchHistorical = False
            GetLastSignalDetails(Now.Date)
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

                If _tempSignal IsNot Nothing Then
                    If _tempSignal.NoOfSharesToBuy = 0 Then
                        SetSignalDetails(_tempSignal.SnapshotDate, _tempSignal.ClosePrice, _tempSignal.ClosePrice, _tempSignal.DesireValue)
                        _tempSignal = Nothing
                        _entryDoneForTheDay = True
                    Else
                        Dim lastExecutedTrade As IBusinessOrder = GetLastExecutedOrder()
                        If lastExecutedTrade IsNot Nothing AndAlso lastExecutedTrade.ParentOrder.Status = IOrder.TypeOfStatus.Complete Then
                            SetSignalDetails(_tempSignal.SnapshotDate, _tempSignal.ClosePrice, lastExecutedTrade.ParentOrder.AveragePrice, _tempSignal.DesireValue)
                            _tempSignal = Nothing
                            _entryDoneForTheDay = True
                        End If
                    End If
                End If

                'Place Order block start
                Dim placeOrderTriggers As List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)) = Await IsTriggerReceivedForPlaceOrderAsync(False).ConfigureAwait(False)
                If placeOrderTriggers IsNot Nothing AndAlso placeOrderTriggers.Count > 0 AndAlso
                    placeOrderTriggers.FirstOrDefault.Item1 = ExecuteCommandAction.Take Then
                    Dim orderResponse = Await ExecuteCommandAsync(ExecuteCommands.PlaceRegularMarketCNCOrder, Nothing).ConfigureAwait(False)
                    If orderResponse IsNot Nothing AndAlso orderResponse.Count > 0 Then
                        Dim placeOrderResponse = CType(orderResponse, Concurrent.ConcurrentBag(Of Object)).FirstOrDefault
                        If placeOrderResponse.ContainsKey("data") AndAlso
                            placeOrderResponse("data").ContainsKey("order_id") Then
                            _entryDoneForTheDay = True
                        End If
                    End If
                End If
                'Place Order block end
                _cts.Token.ThrowIfCancellationRequested()

                If _lastTick IsNot Nothing AndAlso Now >= _lastTick.Timestamp.Value.AddMinutes(5) Then
                    _lastTick = Nothing
                    _entryDoneForTheDay = False
                End If

                Await Task.Delay(2000, _cts.Token).ConfigureAwait(False)
            End While
        Catch ex As Exception
            'To log exceptions getting created from this function as the bubble up of the exception
            'will anyways happen to Strategy.MonitorAsync but it will not be shown until all tasks exit
            Dim message As String = String.Format("Strategy Instrument:{0}, error:{1}", Me.ToString, ex.ToString)
            logger.Error(message)

            If Me.ParentStrategy.ParentController.OrphanException Is Nothing Then
                SendTradeAlertMessageAsync(message)
            End If

            Throw ex
        End Try
    End Function

    Protected Overrides Async Function IsTriggerReceivedForPlaceOrderAsync(ByVal forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)))
        Dim ret As List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
        Dim currentTick As ITick = Me.TradableInstrument.LastTick
        Dim currentTime As Date = Now()

        Dim log As Boolean = False
        If currentTime >= userSettings.TradeEntryTime Then
            If _lastTick Is Nothing Then
                log = True
                _lastTick = currentTick
            End If
        End If

        Dim parameters As PlaceOrderParameters = Nothing
        If currentTime >= Me.TradableInstrument.ExchangeDetails.ExchangeStartTime AndAlso currentTime <= Me.TradableInstrument.ExchangeDetails.ExchangeEndTime Then
            If CType(Me.ParentStrategy, NFOStrategy).TradingDates IsNot Nothing AndAlso CType(Me.ParentStrategy, NFOStrategy).TradingDates.Contains(Now.Date) Then
                If currentTime >= userSettings.TradeEntryTime AndAlso _lastTick IsNot Nothing AndAlso Not _entryDoneForTheDay Then
                    Dim signalCandle As OHLCPayload = New OHLCPayload(OHLCPayload.PayloadSource.CalculatedTick)
                    signalCandle.SnapshotDateTime = _lastTick.Timestamp.Value
                    signalCandle.OpenPrice.Value = _lastTick.LastPrice
                    signalCandle.LowPrice.Value = _lastTick.LastPrice
                    signalCandle.HighPrice.Value = _lastTick.LastPrice
                    signalCandle.ClosePrice.Value = _lastTick.LastPrice
                    signalCandle.Volume.Value = _lastTick.Volume
                    If signalCandle IsNot Nothing Then
                        Dim lastSignal As SignalDetails = GetLastSignalDetails(signalCandle.SnapshotDateTime)
                        Dim desireValue As Double = userSettings.InitialInvestment
                        If lastSignal IsNot Nothing Then
                            desireValue = lastSignal.DesireValue + userSettings.ExpectedIncreaseEachPeriod
                        End If
                        _tempSignal = New SignalDetails(lastSignal, Me.TradableInstrument.TradingSymbol, signalCandle.SnapshotDateTime, signalCandle.ClosePrice.Value, 0, desireValue)
                        Dim quantity As Decimal = _tempSignal.NoOfSharesToBuy
                        If quantity > 0 Then
                            parameters = New PlaceOrderParameters(signalCandle) With
                                         {
                                            .EntryDirection = IOrder.TypeOfTransaction.Buy,
                                            .OrderType = IOrder.TypeOfOrder.Market,
                                            .Quantity = quantity
                                         }
                        ElseIf quantity < 0 Then
                            parameters = New PlaceOrderParameters(signalCandle) With
                                         {
                                            .EntryDirection = IOrder.TypeOfTransaction.Sell,
                                            .OrderType = IOrder.TypeOfOrder.Market,
                                            .Quantity = quantity
                                         }
                        End If
                        'If log Then

                        'End If
                    End If
                End If
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
                            If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                            ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.DonotTake, parameters, parameters.ToString))
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
                            If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                            ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.DonotTake, parameters, parameters.ToString))
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
                        If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                        ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.DonotTake, parameters, parameters.ToString))
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

    Public Function GetLastSignalDetails(ByVal snapshotDate As Date) As SignalDetails
        Dim ret As SignalDetails = Nothing
        If File.Exists(_signalDetailsFilename) Then
            _allSignalDetails = Utilities.Strings.DeserializeToCollection(Of Dictionary(Of Date, SignalDetails))(_signalDetailsFilename)
        End If
        If AllSignalDetails IsNot Nothing AndAlso AllSignalDetails.Count > 0 Then
            ret = AllSignalDetails.Where(Function(y)
                                             Return y.Key < snapshotDate
                                         End Function).OrderBy(Function(x)
                                                                   Return x.Key
                                                               End Function).LastOrDefault.Value
        End If
        Return ret
    End Function

    Public Function SetSignalDetails(ByVal snapshotDate As Date, ByVal closePrice As Decimal, ByVal entryPrice As Decimal, ByVal desireValue As Double) As Boolean
        Dim ret As Boolean = False
        If _allSignalDetails Is Nothing Then _allSignalDetails = New Dictionary(Of Date, SignalDetails)
        If Not _allSignalDetails.ContainsKey(snapshotDate) Then
            Dim lastSignal As SignalDetails = GetLastSignalDetails(snapshotDate)
            Dim signal As SignalDetails = New SignalDetails(lastSignal, Me.TradableInstrument.TradingSymbol, snapshotDate, closePrice, entryPrice, desireValue)
            _allSignalDetails.Add(signal.SnapshotDate, signal)
            Utilities.Strings.SerializeFromCollection(Of Dictionary(Of Date, SignalDetails))(_signalDetailsFilename, AllSignalDetails)
        End If
        Return ret
    End Function

    Private Async Function SendTradeAlertMessageAsync(ByVal message As String) As Task
        Try
            Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
            _cts.Token.ThrowIfCancellationRequested()
            If message.Contains("&") Then
                message = message.Replace("&", "_")
            End If
            message = String.Format("{0} -> {1}", Me.TradableInstrument.TradingSymbol, message)
            Dim userInputs As NFOUserInputs = Me.ParentStrategy.UserSettings
            If userInputs.TelegramBotAPIKey IsNot Nothing AndAlso Not userInputs.TelegramBotAPIKey.Trim = "" AndAlso
                userInputs.TelegramTradeChatID IsNot Nothing AndAlso Not userInputs.TelegramTradeChatID.Trim = "" Then
                Using tSender As New Utilities.Notification.Telegram(userInputs.TelegramBotAPIKey.Trim, userInputs.TelegramTradeChatID.Trim, _cts)
                    Dim encodedString As String = Utilities.Strings.UrlEncodeString(message)
                    Await tSender.SendMessageGetAsync(encodedString).ConfigureAwait(False)
                End Using
            End If
        Catch ex As Exception
            logger.Warn(ex.ToString)
        End Try
    End Function

    <Serializable>
    Public Class SignalDetails
        Public Sub New(ByVal previousSignal As SignalDetails,
                       ByVal tradingSymbol As String,
                       ByVal snapshotDate As Date,
                       ByVal closePrice As Decimal,
                       ByVal entryPrice As Decimal,
                       ByVal desireValue As Double)
            Me.PreviousSignal = previousSignal
            Me.TradingSymbol = tradingSymbol
            Me.SnapshotDate = snapshotDate
            Me.ClosePrice = closePrice
            Me.EntryPrice = entryPrice
            Me.DesireValue = desireValue
        End Sub

        Public ReadOnly Property PreviousSignal As SignalDetails

        Public ReadOnly Property TradingSymbol As String

        Public ReadOnly Property SnapshotDate As Date

        Public ReadOnly Property ClosePrice As Decimal

        Public ReadOnly Property EntryPrice As Decimal

        Public ReadOnly Property DesireValue As Double

        Public ReadOnly Property NoOfSharesOwnedBeforeRebalancing As Long
            Get
                If Me.PreviousSignal IsNot Nothing Then
                    Return Me.PreviousSignal.SharesOwnedAfterRebalancing
                Else
                    Return 0
                End If
            End Get
        End Property

        Public ReadOnly Property ToatalValueBeforeRebalancing As Double
            Get
                Return Me.ClosePrice * Me.NoOfSharesOwnedBeforeRebalancing
            End Get
        End Property

        Public ReadOnly Property AmountToInvest As Double
            Get
                Return Me.DesireValue - Me.ToatalValueBeforeRebalancing
            End Get
        End Property

        Public ReadOnly Property NoOfSharesToBuy As Long
            Get
                Return Math.Ceiling(Me.AmountToInvest / Me.ClosePrice)
            End Get
        End Property

        Public ReadOnly Property SharesOwnedAfterRebalancing As Long
            Get
                Return Me.NoOfSharesOwnedBeforeRebalancing + Me.NoOfSharesToBuy
            End Get
        End Property

        Public ReadOnly Property TotalInvested As Double
            Get
                If Me.PreviousSignal IsNot Nothing Then
                    Return Me.PreviousSignal.TotalInvested + Me.NoOfSharesToBuy * Me.EntryPrice
                Else
                    Return Me.NoOfSharesToBuy * Me.EntryPrice
                End If
            End Get
        End Property

        Public ReadOnly Property PeriodicInvestment As Double
            Get
                If Me.PreviousSignal IsNot Nothing Then
                    Return (Me.PreviousSignal.TotalInvested - Me.TotalInvested)
                Else
                    Return (0 - Me.TotalInvested)
                End If
            End Get
        End Property
    End Class

#Region "Not Required For This Strategy"
    Public Overrides Function MonitorAsync(ByVal command As ExecuteCommands, ByVal data As Object) As Task
        Throw New NotImplementedException()
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

    Protected Overrides Function IsTriggerReceivedForPlaceOrderAsync(forcePrint As Boolean, data As Object) As Task(Of List(Of Tuple(Of ExecuteCommandAction, StrategyInstrument, PlaceOrderParameters, String)))
        Throw New NotImplementedException()
    End Function

    Protected Overrides Function IsTriggerReceivedForExitOrderAsync(forcePrint As Boolean, data As Object) As Task(Of List(Of Tuple(Of ExecuteCommandAction, StrategyInstrument, IOrder, String)))
        Throw New NotImplementedException()
    End Function

    Protected Overrides Function ForceExitSpecificTradeAsync(order As IOrder, ByVal reason As String) As Task
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