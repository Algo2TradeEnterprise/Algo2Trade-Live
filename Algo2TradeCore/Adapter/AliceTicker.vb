Imports System.Threading
Imports Algo2TradeCore.Controller
Imports AliceBlueClient
Imports NLog

Namespace Adapter
    Public Class AliceTicker
        Inherits APITicker
        Protected _ticker As Ticker

#Region "Logging and Status Progress"
        Public Shared Shadows logger As Logger = LogManager.GetCurrentClassLogger
#End Region
        Public Sub New(ByVal associatedParentController As APIStrategyController,
                       ByVal canceller As CancellationTokenSource)
            MyBase.New(associatedParentController, canceller)
        End Sub
        Public Overrides Async Function ConnectTickerAsync() As Task
            logger.Debug("{0}->ConnectTickerAsync, parameters:Nothing", Me.ToString)
            _cts.Token.ThrowIfCancellationRequested()
            Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
            Dim currentAliceStrategyController As AliceStrategyController = CType(ParentController, AliceStrategyController)

            If _ticker IsNot Nothing Then
                RemoveHandler _ticker.TickReceived, AddressOf currentAliceStrategyController.OnTickerTickAsync
                RemoveHandler _ticker.ReconnectTicker, AddressOf currentAliceStrategyController.OnTickerReconnect
                RemoveHandler _ticker.NoReconnectTicker, AddressOf currentAliceStrategyController.OnTickerNoReconnect
                RemoveHandler _ticker.ErrorMessage, AddressOf currentAliceStrategyController.OnTickerError
                RemoveHandler _ticker.CloseTicker, AddressOf currentAliceStrategyController.OnTickerClose
                RemoveHandler _ticker.ConnectTicker, AddressOf currentAliceStrategyController.OnTickerConnect
                RemoveHandler _ticker.OrderUpdate, AddressOf currentAliceStrategyController.OnTickerOrderUpdateAsync

                _cts.Token.ThrowIfCancellationRequested()
                If _ticker.IsConnected Then _ticker.Close()
                _cts.Token.ThrowIfCancellationRequested()
            End If
            _ticker = New Ticker(ParentController.APIConnection.APIUser.APISecret, ParentController.APIConnection.AccessToken)
            AddHandler _ticker.TickReceived, AddressOf currentAliceStrategyController.OnTickerTickAsync
            AddHandler _ticker.ReconnectTicker, AddressOf currentAliceStrategyController.OnTickerReconnect
            AddHandler _ticker.NoReconnectTicker, AddressOf currentAliceStrategyController.OnTickerNoReconnect
            AddHandler _ticker.ErrorMessage, AddressOf currentAliceStrategyController.OnTickerError
            AddHandler _ticker.CloseTicker, AddressOf currentAliceStrategyController.OnTickerClose
            AddHandler _ticker.ConnectTicker, AddressOf currentAliceStrategyController.OnTickerConnect
            AddHandler _ticker.OrderUpdate, AddressOf currentAliceStrategyController.OnTickerOrderUpdateAsync

            _cts.Token.ThrowIfCancellationRequested()
            _ticker.EnableReconnect(Interval:=5, Retries:=50)
            _ticker.Connect()
            _cts.Token.ThrowIfCancellationRequested()
        End Function

        Public Overrides Async Function SubscribeAsync(ByVal instrumentIdentifiers As List(Of String)) As Task
            logger.Debug("{0}->SubscribeAsync, instrumentIdentifiers:{1}", Me.ToString, Utils.JsonSerialize(instrumentIdentifiers))
            _cts.Token.ThrowIfCancellationRequested()
            Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
            If _subscribedInstruments Is Nothing Then _subscribedInstruments = New List(Of String)
            Dim subscriptionList() As UInt32 = Nothing

            Dim index As Integer = -1
            For Each runningInstrumentIdentifier In instrumentIdentifiers
                _cts.Token.ThrowIfCancellationRequested()
                If _subscribedInstruments.Contains(runningInstrumentIdentifier) Then Continue For
                index += 1
                If index = 0 Then
                    ReDim subscriptionList(0)
                Else
                    ReDim Preserve subscriptionList(UBound(subscriptionList) + 1)
                End If
                subscriptionList(index) = runningInstrumentIdentifier
                _subscribedInstruments.Add(runningInstrumentIdentifier)
            Next
            If subscriptionList Is Nothing OrElse subscriptionList.Length = 0 Then
                OnHeartbeat("No instruments were subscribed as they may be already subscribed")
                logger.Error("No tokens to subscribe")
            Else
                _cts.Token.ThrowIfCancellationRequested()
                _ticker.Subscribe(GetSubscriptionDictionary(instrumentIdentifiers), ModeOfTicker.marketdata)
                OnHeartbeat(String.Format("Subscribed:{0} instruments for ticker", subscriptionList.Count))
            End If
        End Function
        Public Overrides Function ToString() As String
            Return Me.GetType.ToString
        End Function
        Public Overrides Sub ClearLocalUniqueSubscriptionList()
            _subscribedInstruments = Nothing
        End Sub
        Public Overrides Function IsConnected() As Boolean
            If _ticker IsNot Nothing Then
                Return _ticker.IsConnected
            Else
                Return False
            End If
        End Function
        Public Overrides Async Function CloseTickerIfConnectedAsync() As Task
            'Intentionally no _cts.Token.ThrowIfCancellationRequested() since we need to close the ticker when cancellation is done
            Await Task.Delay(0).ConfigureAwait(False)
            If _ticker IsNot Nothing AndAlso _ticker.IsConnected Then
                _ticker.Close()
            End If
        End Function
        Public Overrides Async Function UnSubscribeAsync(instrumentToken As String) As Task
            Await Task.Delay(0).ConfigureAwait(False)
            If _ticker IsNot Nothing Then
                Dim subscriptions As Dictionary(Of UInteger, TypesOfExchange) = GetSubscriptionDictionary(_subscribedInstruments)
                If subscriptions IsNot Nothing AndAlso subscriptions.ContainsKey(CInt(instrumentToken)) Then
                    Dim dataToUnsubscibe As Dictionary(Of UInteger, TypesOfExchange) = New Dictionary(Of UInteger, TypesOfExchange) From
                        {{CInt(instrumentToken), subscriptions(CInt(instrumentToken))}}
                    _ticker.UnSubscribe(dataToUnsubscibe, ModeOfTicker.marketdata)
                End If
            End If
        End Function

        Public Function GetTickerSubscriptionString(ByVal instrumentDetails As Dictionary(Of String, TypesOfExchange)) As List(Of String)
            Dim ret As List(Of String) = Nothing
            If instrumentDetails IsNot Nothing AndAlso instrumentDetails.Count > 0 Then
                For Each runningInstrument In instrumentDetails
                    Dim token As UInteger = runningInstrument.Key
                    Dim exchange As Integer = runningInstrument.Value
                    Dim dataString As String = String.Format("{0},{1}", token, exchange)
                    If ret Is Nothing Then ret = New List(Of String)
                    ret.Add(dataString)
                Next
            End If
            Return ret
        End Function

        Private Function GetSubscriptionDictionary(ByVal instrumentDetails As List(Of String)) As Dictionary(Of UInteger, TypesOfExchange)
            Dim ret As Dictionary(Of UInteger, TypesOfExchange) = Nothing
            If instrumentDetails IsNot Nothing AndAlso instrumentDetails.Count > 0 Then
                For Each runningInstrument In instrumentDetails
                    Dim token As UInteger = runningInstrument.Split(",")(0)
                    Dim exchange As TypesOfExchange = CInt(runningInstrument.Split(",")(1))
                    If ret Is Nothing Then ret = New Dictionary(Of UInteger, TypesOfExchange)
                    ret.Add(token, exchange)
                Next
            End If
            Return ret
        End Function
    End Class
End Namespace