Imports NLog
Imports System.IO
Imports System.Net.Http
Imports System.Threading
Imports Utilities.Numbers
Imports Utilities.Network
Imports Algo2TradeCore.Adapter
Imports Algo2TradeCore.Entities
Imports Algo2TradeCore.Strategies
Imports Algo2TradeCore.ChartHandler.ChartStyle

Public Class NFOStrategyInstrument
    Inherits StrategyInstrument
    Implements IDisposable

#Region "Logging and Status Progress"
    Public Shared Shadows logger As Logger = LogManager.GetCurrentClassLogger
#End Region

    Private _lastPlacedOrder As String
    Private _quantityToTrade As Integer
    Public ReadOnly TradeFileName As String

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
        RawPayloadDependentConsumers = New List(Of IPayloadConsumer)
        If Me.ParentStrategy.IsStrategyCandleStickBased Then
            If Me.ParentStrategy.UserSettings.SignalTimeFrame > 0 Then
                Dim chartConsumer As PayloadToChartConsumer = New PayloadToChartConsumer(Me.ParentStrategy.UserSettings.SignalTimeFrame)
                RawPayloadDependentConsumers.Add(chartConsumer)
            Else
                Throw New ApplicationException(String.Format("Signal Timeframe is 0 or Nothing, does not adhere to the strategy:{0}", Me.ParentStrategy.ToString))
            End If
        End If

        Me.TradableInstrument.FetchHistorical = False
        TradeFileName = Path.Combine(My.Application.Info.DirectoryPath, String.Format("{0}.Trade.a2t", Me.TradableInstrument.TradingSymbol))
    End Sub

    Public Overrides Async Function PopulateChartAndIndicatorsAsync(candleCreator As Chart, currentCandle As OHLCPayload) As Task
        If RawPayloadDependentConsumers IsNot Nothing AndAlso RawPayloadDependentConsumers.Count > 0 Then
            For Each runningRawPayloadConsumer In RawPayloadDependentConsumers
                If runningRawPayloadConsumer.TypeOfConsumer = IPayloadConsumer.ConsumerType.Chart Then
                    candleCreator.ConvertTimeframe(CType(runningRawPayloadConsumer, PayloadToChartConsumer).Timeframe, currentCandle, runningRawPayloadConsumer)
                End If
            Next
        End If
    End Function

    Public Overrides Async Function MonitorAsync() As Task
        Await Task.Delay(0).ConfigureAwait(False)
        Try
            While True
                _cts.Token.ThrowIfCancellationRequested()
                If Me.ParentStrategy.ParentController.OrphanException IsNot Nothing Then
                    Throw Me.ParentStrategy.ParentController.OrphanException
                End If
                'If Me._RMSException IsNot Nothing AndAlso
                '    _RMSException.ExceptionType = Algo2TradeCore.Exceptions.AdapterBusinessException.TypeOfException.RMSError Then
                '    OnHeartbeat(String.Format("{0}:Will not take no more action in this instrument as RMS Error occured. Error-{1}", Me.TradableInstrument.TradingSymbol, _RMSException.Message))
                '    Throw Me._RMSException
                'End If

                Dim modifyTargetOrderTrigger As List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)) = Await IsTriggerReceivedForModifyTargetOrderAsync(False).ConfigureAwait(False)
                If modifyTargetOrderTrigger IsNot Nothing AndAlso modifyTargetOrderTrigger.Count > 0 Then
                    Dim orderResponse = Await ExecuteCommandAsync(ExecuteCommands.ModifyTargetOrder, Nothing).ConfigureAwait(False)
                    If orderResponse IsNot Nothing AndAlso orderResponse.Count > 0 Then
                        Dim modifyOrderResponse = CType(orderResponse, Concurrent.ConcurrentBag(Of Object)).FirstOrDefault
                        If modifyTargetOrderTrigger.FirstOrDefault.Item4.ToUpper.Contains("MARKET") Then
                            Exit While
                        End If
                    End If
                End If

                If _lastPlacedOrder IsNot Nothing AndAlso Me.OrderDetails IsNot Nothing AndAlso Me.OrderDetails.ContainsKey(_lastPlacedOrder) Then
                    If Me.OrderDetails(_lastPlacedOrder).ParentOrder IsNot Nothing Then
                        If Me.OrderDetails(_lastPlacedOrder).ParentOrder.Status = IOrder.TypeOfStatus.Complete Then
                            Exit While
                        ElseIf Me.OrderDetails(_lastPlacedOrder).ParentOrder.Status = IOrder.TypeOfStatus.Rejected Then
                            Exit While
                        End If
                    End If
                End If

                Await Task.Delay(1000).ConfigureAwait(False)
            End While
        Catch ex As Exception
            logger.Error("Strategy Instrument:{0}, error:{1}", Me.ToString, ex.ToString)
            Throw ex
        End Try
    End Function

    Public Overrides Async Function MonitorAsync(ByVal command As ExecuteCommands, ByVal data As Object) As Task
        Await Task.Delay(0).ConfigureAwait(False)
        Try
            If Me.ParentStrategy.ParentController.OrphanException IsNot Nothing Then
                Throw Me.ParentStrategy.ParentController.OrphanException
            End If
            'If Me._RMSException IsNot Nothing AndAlso
            '    _RMSException.ExceptionType = Algo2TradeCore.Exceptions.AdapterBusinessException.TypeOfException.RMSError Then
            '    OnHeartbeat(String.Format("{0}:Will not take no more action in this instrument as RMS Error occured. Error-{1}", Me.TradableInstrument.TradingSymbol, _RMSException.Message))
            '    Throw Me._RMSException
            'End If

            If command = ExecuteCommands.PlaceRegularLimitCNCOrder Then
                _quantityToTrade = data
                Dim orderResponse = Await ExecuteCommandAsync(ExecuteCommands.PlaceRegularLimitCNCOrder, Nothing).ConfigureAwait(False)
                If orderResponse IsNot Nothing AndAlso orderResponse.Count > 0 Then
                    Dim placeOrderResponse = CType(orderResponse, Concurrent.ConcurrentBag(Of Object)).FirstOrDefault
                    If placeOrderResponse.ContainsKey("data") AndAlso
                        placeOrderResponse("data").ContainsKey("order_id") Then
                        _lastPlacedOrder = placeOrderResponse("data")("order_id")
                        Await MonitorAsync().ConfigureAwait(False)
                        _lastPlacedOrder = Nothing
                        If File.Exists(Me.TradeFileName) Then
                            Dim tradedQuantity As Integer = Utilities.Strings.DeserializeToCollection(Of Integer)(Me.TradeFileName)
                            If tradedQuantity + _quantityToTrade = 0 Then
                                File.Delete(Me.TradeFileName)
                            End If
                        Else
                            Utilities.Strings.SerializeFromCollection(Of Integer)(Me.TradeFileName, _quantityToTrade)
                        End If
                        _quantityToTrade = 0
                    End If
                End If
            Else
                Throw New NotImplementedException
            End If
        Catch ex As Exception
            logger.Error("Strategy Instrument:{0}, error:{1}", Me.ToString, ex.ToString)
            Throw ex
        End Try
    End Function

    Protected Overrides Async Function IsTriggerReceivedForPlaceOrderAsync(ByVal forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)))
        Dim ret As List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        Dim currentTick As ITick = Me.TradableInstrument.LastTick
        Dim currentTime As Date = Now()

        Dim parameters As PlaceOrderParameters = Nothing
        If currentTime >= Me.TradableInstrument.ExchangeDetails.ExchangeStartTime AndAlso currentTime <= Me.TradableInstrument.ExchangeDetails.ExchangeEndTime Then
            Dim signalCandle As OHLCPayload = New OHLCPayload(OHLCPayload.PayloadSource.CalculatedTick)
            signalCandle.SnapshotDateTime = currentTick.Timestamp.Value
            signalCandle.OpenPrice.Value = currentTick.LastPrice
            signalCandle.LowPrice.Value = currentTick.LastPrice
            signalCandle.HighPrice.Value = currentTick.LastPrice
            signalCandle.ClosePrice.Value = currentTick.LastPrice
            signalCandle.Volume.Value = currentTick.Volume
            If signalCandle IsNot Nothing Then
                If _quantityToTrade > 0 Then
                    parameters = New PlaceOrderParameters(signalCandle) With
                                 {
                                    .EntryDirection = IOrder.TypeOfTransaction.Buy,
                                    .OrderType = IOrder.TypeOfOrder.Limit,
                                    .Price = currentTick.FirstOfferPrice,
                                    .Quantity = Math.Abs(_quantityToTrade)
                                 }
                ElseIf _quantityToTrade < 0 Then
                    parameters = New PlaceOrderParameters(signalCandle) With
                                 {
                                    .EntryDirection = IOrder.TypeOfTransaction.Sell,
                                    .OrderType = IOrder.TypeOfOrder.Limit,
                                    .Price = currentTick.FirstBidPrice,
                                    .Quantity = Math.Abs(_quantityToTrade)
                                 }
                End If
            End If
        End If

        'Below portion have to be done in every place order trigger
        If parameters IsNot Nothing Then
            Try
                If forcePrint Then
                    logger.Debug("PlaceOrder-> ************************************************ {0}", Me.TradableInstrument.TradingSymbol)
                    logger.Debug("PlaceOrder Parameters-> {0},{1}", parameters.ToString, Me.TradableInstrument.TradingSymbol)
                    'logger.Fatal(Utilities.Strings.JsonSerialize(currentTick))
                End If
            Catch ex As Exception
                logger.Error(ex)
            End Try

            Dim currentSignalActivities As IEnumerable(Of ActivityDashboard) = Me.ParentStrategy.SignalManager.GetSignalActivities(parameters.SignalCandle.SnapshotDateTime, Me.TradableInstrument.InstrumentIdentifier)
            If currentSignalActivities IsNot Nothing AndAlso currentSignalActivities.Count > 0 Then
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

    Protected Overrides Async Function IsTriggerReceivedForModifyTargetOrderAsync(forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)))
        Dim ret As List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        Dim currentTick As ITick = Me.TradableInstrument.LastTick
        Dim currentTime As Date = Now()
        If OrderDetails IsNot Nothing AndAlso OrderDetails.Count > 0 Then
            For Each runningParentOrder In OrderDetails.Keys
                Dim parentBussinessOrder As IBusinessOrder = OrderDetails(runningParentOrder)
                If parentBussinessOrder.ParentOrder.Status = IOrder.TypeOfStatus.Open Then
                    Dim price As Decimal = Decimal.MinValue
                    Dim reason As String = ""
                    If parentBussinessOrder.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Buy Then
                        If currentTime >= parentBussinessOrder.ParentOrder.TimeStamp.AddSeconds(2) Then
                            price = currentTick.LastPrice + ConvertFloorCeling(currentTick.LastPrice * 0.3 / 100, TradableInstrument.TickSize, RoundOfType.Celing)
                            reason = "Market Order"
                        ElseIf currentTime >= parentBussinessOrder.ParentOrder.TimeStamp.AddSeconds(1) Then
                            price = currentTick.FirstOfferPrice
                            reason = "Second Try"
                        End If
                    ElseIf parentBussinessOrder.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Sell Then
                        If currentTime >= parentBussinessOrder.ParentOrder.TimeStamp.AddSeconds(2) Then
                            price = currentTick.LastPrice - ConvertFloorCeling(currentTick.LastPrice * 0.3 / 100, TradableInstrument.TickSize, RoundOfType.Celing)
                            reason = "Market Order"
                        ElseIf currentTime >= parentBussinessOrder.ParentOrder.TimeStamp.AddSeconds(1) Then
                            price = currentTick.FirstBidPrice
                            reason = "Second Try"
                        End If
                    End If
                    If price <> Decimal.MinValue AndAlso parentBussinessOrder.ParentOrder.Price <> price Then
                        'Below portion have to be done in every modify target order trigger
                        Dim currentSignalActivities As ActivityDashboard = Me.ParentStrategy.SignalManager.GetSignalActivities(parentBussinessOrder.ParentOrder.Tag)
                        If currentSignalActivities IsNot Nothing Then
                            If currentSignalActivities.TargetModifyActivity.RequestStatus = ActivityDashboard.SignalStatusType.Handled OrElse
                                        currentSignalActivities.TargetModifyActivity.RequestStatus = ActivityDashboard.SignalStatusType.Activated OrElse
                                        currentSignalActivities.TargetModifyActivity.RequestStatus = ActivityDashboard.SignalStatusType.Completed Then
                                If Val(currentSignalActivities.TargetModifyActivity.Supporting) = price Then
                                    Continue For
                                End If
                            End If
                        End If
                        If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String))
                        ret.Add(New Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)(ExecuteCommandAction.Take, parentBussinessOrder.ParentOrder, price, reason))
                    End If
                End If
            Next
        End If
        If forcePrint AndAlso ret IsNot Nothing AndAlso ret.Count > 0 Then
            For Each runningOrder In ret
                logger.Debug("***** Modify Target ***** Order ID:{0}, Reason:{1}, {2}", runningOrder.Item2.OrderIdentifier, runningOrder.Item4, Me.TradableInstrument.TradingSymbol)
            Next
        End If
        Return ret
    End Function

    Public Function GetXMinutePayload(ByVal timeFrame As Integer) As Concurrent.ConcurrentDictionary(Of Date, IPayload)
        Dim ret As Concurrent.ConcurrentDictionary(Of Date, IPayload) = Nothing
        If Me.RawPayloadDependentConsumers IsNot Nothing AndAlso Me.RawPayloadDependentConsumers.Count > 0 Then
            Dim XMinutePayloadConsumer As PayloadToChartConsumer =
                RawPayloadDependentConsumers.Find(Function(x)
                                                      If x.GetType Is GetType(PayloadToChartConsumer) Then
                                                          Return CType(x, PayloadToChartConsumer).Timeframe = timeFrame
                                                      Else
                                                          Return Nothing
                                                      End If
                                                  End Function)

            If XMinutePayloadConsumer IsNot Nothing AndAlso
                XMinutePayloadConsumer.ConsumerPayloads IsNot Nothing AndAlso
                XMinutePayloadConsumer.ConsumerPayloads.Count > 0 Then
                ret = XMinutePayloadConsumer.ConsumerPayloads
            End If
        End If
        Return ret
    End Function

#Region "Historical Data Fetcher"
    Private _historicalLock As Integer = 0
    Public Async Function ProcessHistoricalAsync(ByVal fromDate As Date, ByVal toDate As Date) As Task
        Try
            While 1 = Interlocked.Exchange(_historicalLock, 1)
                Await Task.Delay(10, _cts.Token).ConfigureAwait(False)
            End While
            Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
            If Not Me.TradableInstrument.IsHistoricalCompleted Then
                Dim histoticalJsonDic As Dictionary(Of String, Object) = Await GetEODHistoricalDataAsync(Me.TradableInstrument, fromDate, toDate).ConfigureAwait(False)
                Await Me.ParentStrategy.ParentController.PopulateRawPayloadManuallyAsync(Me.TradableInstrument.InstrumentIdentifier, histoticalJsonDic).ConfigureAwait(False)
            End If
        Catch ex As Exception
            logger.Error(ex.ToString)
            Throw ex
        Finally
            Interlocked.Exchange(_historicalLock, 0)
        End Try
    End Function

    Private Async Function GetEODHistoricalDataAsync(ByVal instrument As IInstrument, ByVal fromDate As Date, ByVal toDate As Date) As Task(Of Dictionary(Of String, Object))
        Dim ret As Dictionary(Of String, Object) = Nothing
        _cts.Token.ThrowIfCancellationRequested()
        Dim zerodhaHistoricalURL As String = "https://kite.zerodha.com/oms/instruments/historical/{0}/day?oi=1&from={1}&to={2}"
        If Me.ParentStrategy.UserSettings.SignalTimeFrame >= 375 Then
            zerodhaHistoricalURL = "https://kite.zerodha.com/oms/instruments/historical/{0}/day?oi=1&from={1}&to={2}"
        ElseIf Me.ParentStrategy.UserSettings.SignalTimeFrame > 60 Then
            zerodhaHistoricalURL = zerodhaHistoricalURL.Replace("day", String.Format("{0}hour", CInt(Me.ParentStrategy.UserSettings.SignalTimeFrame / 60)))
        Else
            zerodhaHistoricalURL = zerodhaHistoricalURL.Replace("day", String.Format("{0}minute", Me.ParentStrategy.UserSettings.SignalTimeFrame))
        End If

        Dim historicalDataURL As String = String.Format(zerodhaHistoricalURL, instrument.InstrumentIdentifier, fromDate.ToString("yyyy-MM-dd"), toDate.ToString("yyyy-MM-dd"))
        Dim proxyToBeUsed As HttpProxy = Nothing
        Using browser As New HttpBrowser(proxyToBeUsed, Net.DecompressionMethods.GZip, New TimeSpan(0, 1, 0), _cts)
            'AddHandler browser.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete
            'AddHandler browser.Heartbeat, AddressOf OnHeartbeat
            'AddHandler browser.WaitingFor, AddressOf OnWaitingFor
            'AddHandler browser.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
            browser.KeepAlive = True
            Dim headers As Dictionary(Of String, String) = New Dictionary(Of String, String)
            headers.Add("Host", "kite.zerodha.com")
            headers.Add("Accept", "*/*")
            headers.Add("Accept-Language", "en-US,en;q=0.9,hi;q=0.8,ko;q=0.7")
            headers.Add("Authorization", String.Format("enctoken {0}", Me.ParentStrategy.ParentController.APIConnection.ENCToken))
            headers.Add("Referer", "https://kite.zerodha.com/static/build/chart.html?v=2.4.0")
            headers.Add("sec-fetch-mode", "cors")
            headers.Add("sec-fetch-site", "same-origin")

            Dim l As Tuple(Of Uri, Object) = Await browser.NonPOSTRequestAsync(historicalDataURL, HttpMethod.Get, Nothing, False, headers, True, "application/json").ConfigureAwait(False)
            If l Is Nothing OrElse l.Item2 Is Nothing Then
                Throw New ApplicationException(String.Format("No response while getting eod historical data for: {0}", historicalDataURL))
            End If
            If l IsNot Nothing AndAlso l.Item2 IsNot Nothing Then
                ret = l.Item2
            End If

            'RemoveHandler browser.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete
            'RemoveHandler browser.Heartbeat, AddressOf OnHeartbeat
            'RemoveHandler browser.WaitingFor, AddressOf OnWaitingFor
            'RemoveHandler browser.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
        End Using
        Return ret
    End Function
#End Region

#Region "Not Required For This Strategy"
    Protected Overrides Function IsTriggerReceivedForModifyStoplossOrderAsync(ByVal forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)))
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