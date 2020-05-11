﻿Imports System.Net
Imports System.Threading
Imports Algo2TradeCore.Controller
Imports NLog
Imports Algo2TradeCore.Entities
Imports System.IO
Imports Utilities.Strings
Imports Utilities.Time

Namespace Adapter
    Public Class AliceHistoricalDataFetcher
        Inherits APIHistoricalDataFetcher
        Implements IDisposable

#Region "Logging and Status Progress"
        Public Shared Shadows logger As Logger = LogManager.GetCurrentClassLogger
#End Region

#Region "Events/Event handlers specific to the derived class"
        Public Event FetcherCandles(ByVal instrumentIdentifier As String, ByVal historicalCandlesJSONDict As Dictionary(Of String, Object))
        Public Event FetcherError(ByVal instrumentIdentifier As String, ByVal msg As String)
        'The below functions are needed to allow the derived classes to raise the above two events
        Protected Overridable Async Function OnFetcherCandlesAsync(ByVal instrumentIdentifier As String, ByVal historicalCandlesJSONDict As Dictionary(Of String, Object)) As Task
            Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
            RaiseEvent FetcherCandles(instrumentIdentifier, historicalCandlesJSONDict)
        End Function
        Protected Overridable Sub OnFetcherError(ByVal instrumentIdentifier As String, ByVal msg As String)
            RaiseEvent FetcherError(instrumentIdentifier, msg)
        End Sub
#End Region

        Private ALICE_HISTORICAL_URL = "https://ant.aliceblueonline.com/api/v1/charts?exchange={0}&token={1}&candletype=1&starttime={2}&endtime={3}&type=historical"
        Private ALICE_LIVE_URL = "https://ant.aliceblueonline.com/api/v1/charts?exchange={0}&token={1}&candletype=1&starttime={2}&endtime={3}&type=live"
        Public Sub New(ByVal associatedParentController As APIStrategyController,
                       ByVal daysToGoBack As Integer,
                       ByVal canceller As CancellationTokenSource)
            MyBase.New(associatedParentController, daysToGoBack, canceller)
            StartPollingAsync()
        End Sub
        Public Sub New(ByVal associatedParentController As APIStrategyController,
                       ByVal daysToGoBack As Integer,
                       ByVal instrumentIdentifier As String,
                       ByVal canceller As CancellationTokenSource)
            MyBase.New(associatedParentController, daysToGoBack, instrumentIdentifier, canceller)
            Dim currentAliceStrategyController As AliceStrategyController = CType(ParentController, AliceStrategyController)
            AddHandler Me.FetcherCandles, AddressOf currentAliceStrategyController.OnFetcherCandlesAsync
            AddHandler Me.FetcherError, AddressOf currentAliceStrategyController.OnFetcherError
        End Sub
        Public Sub New(ByVal associatedParentController As APIStrategyController,
                       ByVal daysToGoBack As Integer,
                       ByVal instrumentIdentifier As String,
                       ByVal instrumentExchange As String,
                       ByVal canceller As CancellationTokenSource)
            MyBase.New(associatedParentController, daysToGoBack, instrumentIdentifier, instrumentExchange, canceller)
            Dim currentAliceStrategyController As AliceStrategyController = CType(ParentController, AliceStrategyController)
            AddHandler Me.FetcherCandles, AddressOf currentAliceStrategyController.OnFetcherCandlesAsync
            AddHandler Me.FetcherError, AddressOf currentAliceStrategyController.OnFetcherError
        End Sub
        Public Overrides Async Function ConnectFetcherAsync() As Task
            'logger.Debug("{0}->ConnectTickerAsync, parameters:Nothing", Me.ToString)
            _cts.Token.ThrowIfCancellationRequested()
            Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
            'Dim currentZerodhaStrategyController As ZerodhaStrategyController = CType(ParentController, ZerodhaStrategyController)

            'RemoveHandler Me.FetcherCandlesAsync, AddressOf currentZerodhaStrategyController.OnFetcherCandlesAsync
            'RemoveHandler Me.FetcherError, AddressOf currentZerodhaStrategyController.OnFetcherError
            '_cts.Token.ThrowIfCancellationRequested()
            'AddHandler Me.FetcherCandlesAsync, AddressOf currentZerodhaStrategyController.OnFetcherCandlesAsync
            'AddHandler Me.FetcherError, AddressOf currentZerodhaStrategyController.OnFetcherError
        End Function
        Protected Overrides Async Function GetHistoricalCandleStickAsync() As Task(Of Dictionary(Of String, Object))
            Try
                Dim ret As Dictionary(Of String, Object) = Nothing
                _cts.Token.ThrowIfCancellationRequested()
                Dim historicalDataURL As String = String.Format(ALICE_HISTORICAL_URL,
                                                                    _instrumentExchange,
                                                                    _instrumentIdentifer,
                                                                    DateTimeToUnix(Now.AddDays(-1 * _daysToGoBack)),
                                                                    DateTimeToUnix(Now))

                ServicePointManager.Expect100Continue = False
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
                ServicePointManager.ServerCertificateValidationCallback = Function(s, Ca, CaC, sslPE)
                                                                              Return True
                                                                          End Function

                Console.WriteLine(historicalDataURL)
                Dim request As HttpWebRequest = HttpWebRequest.Create(historicalDataURL)
                request.Headers.Add("X-Authorization-Token", Me.ParentController.APIConnection.ENCToken)

                Using sr = New StreamReader(request.GetResponseAsync().Result.GetResponseStream)
                    Dim jsonString = Await sr.ReadToEndAsync.ConfigureAwait(False)
                    Dim dataDictionary As Dictionary(Of String, Object) = StringManipulation.JsonDeserialize(jsonString)

                    If dataDictionary IsNot Nothing AndAlso dataDictionary.ContainsKey("data") Then
                        Dim candles As ArrayList = dataDictionary("data")
                        Dim candlesDict As Dictionary(Of String, Object) = New Dictionary(Of String, Object) From {{"candles", candles}}
                        dataDictionary = New Dictionary(Of String, Object) From {{"data", candlesDict}}
                        ret = dataDictionary
                    End If
                    Return ret
                End Using
            Catch ex As Exception
                Throw ex
            End Try
        End Function
        Private Async Function GetLiveCandleStickAsync() As Task(Of Dictionary(Of String, Object))
            Try
                Dim ret As Dictionary(Of String, Object) = Nothing
                _cts.Token.ThrowIfCancellationRequested()
                Dim liveDataURL As String = String.Format(ALICE_LIVE_URL,
                                                                _instrumentExchange,
                                                                _instrumentIdentifer,
                                                                DateTimeToUnix(Now.Date),
                                                                DateTimeToUnix(Now))

                ServicePointManager.Expect100Continue = False
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
                ServicePointManager.ServerCertificateValidationCallback = Function(s, Ca, CaC, sslPE)
                                                                              Return True
                                                                          End Function

                Console.WriteLine(liveDataURL)
                Dim request As HttpWebRequest = HttpWebRequest.Create(liveDataURL)
                request.Headers.Add("X-Authorization-Token", Me.ParentController.APIConnection.ENCToken)

                Using sr = New StreamReader(request.GetResponseAsync().Result.GetResponseStream)
                    Dim jsonString = Await sr.ReadToEndAsync.ConfigureAwait(False)
                    Dim dataDictionary As Dictionary(Of String, Object) = StringManipulation.JsonDeserialize(jsonString)

                    If dataDictionary IsNot Nothing AndAlso dataDictionary.ContainsKey("data") Then
                        Dim candles As ArrayList = dataDictionary("data")
                        Dim candlesDict As Dictionary(Of String, Object) = New Dictionary(Of String, Object) From {{"candles", candles}}
                        dataDictionary = New Dictionary(Of String, Object) From {{"data", candlesDict}}
                        ret = dataDictionary
                    End If
                    Return ret
                End Using
            Catch ex As Exception
                Throw ex
            End Try
        End Function

        Protected Overrides Async Function StartPollingAsync() As Task
            'logger.Debug("{0}->StartPollingAsync, parameters:Nothing", Me.ToString)
            Try
                ServicePointManager.DefaultConnectionLimit = 10
                _stopPollRunning = False
                _isPollRunning = False
                ServicePointManager.DefaultConnectionLimit = 10000
                Dim lastTimeWhenDone As Date = Date.MinValue
                Dim nextTimeToDo As Date = Date.MinValue
                Dim apiConnectionBeingUsed As AliceConnection = Me.ParentController.APIConnection
                While True
                    If _stopPollRunning Then
                        Exit While
                    End If
                    Dim sw As New Stopwatch
                    _isPollRunning = True
                    _cts.Token.ThrowIfCancellationRequested()
                    lastTimeWhenDone = Now
                    If _subscribedInstruments IsNot Nothing AndAlso _subscribedInstruments.Count > 0 Then

                        Dim tasks = _subscribedInstruments.Select(Async Function(x)
                                                                      Try
                                                                          If x.FetchHistorical Then
                                                                              _cts.Token.ThrowIfCancellationRequested()
                                                                              Dim individualFetcher As New AliceHistoricalDataFetcher(Me.ParentController,
                                                                                                              If(x.IsHistoricalCompleted, 1, _daysToGoBack),
                                                                                                              x.InstrumentIdentifier, x.RawExchange,
                                                                                                              Me._cts)
                                                                              Dim ret As Dictionary(Of String, Object) = Await individualFetcher.GetLiveCandleStickAsync.ConfigureAwait(False)
                                                                              If Not x.IsHistoricalCompleted Then
                                                                                  Dim tempret As Dictionary(Of String, Object) = Await individualFetcher.GetHistoricalCandleStickAsync.ConfigureAwait(False)
                                                                                  If tempret IsNot Nothing AndAlso tempret.Count > 0 Then
                                                                                      If ret Is Nothing Then
                                                                                          ret = tempret
                                                                                      Else
                                                                                          If tempret IsNot Nothing AndAlso tempret.ContainsKey("data") AndAlso tempret("data").ContainsKey("candles") Then
                                                                                              If ret IsNot Nothing AndAlso ret.ContainsKey("data") AndAlso ret("data").ContainsKey("candles") Then
                                                                                                  Dim livedata As ArrayList = ret("data")("candles")
                                                                                                  Dim historicaldata As ArrayList = tempret("data")("candles")
                                                                                                  historicaldata.AddRange(livedata)
                                                                                                  ret("data")("candles") = historicaldata
                                                                                              End If
                                                                                          End If
                                                                                      End If
                                                                                  End If
                                                                              End If
                                                                              If ret IsNot Nothing AndAlso ret.GetType Is GetType(Dictionary(Of String, Object)) Then
                                                                                  Dim errorMessage As String = ParentController.GetErrorResponse(ret)
                                                                                  If errorMessage IsNot Nothing Then
                                                                                      individualFetcher.OnFetcherError(x.InstrumentIdentifier, errorMessage)
                                                                                  Else
                                                                                      Await individualFetcher.OnFetcherCandlesAsync(x.InstrumentIdentifier, ret).ConfigureAwait(False)
                                                                                  End If
                                                                              Else
                                                                                  'TO DO: Uncomment this
                                                                                  Throw New ApplicationException("Fetching of historical data failed as no return detected")
                                                                              End If
                                                                          End If
                                                                      Catch ex As Exception
                                                                          'Neglect error as in the next minute, it will be run again,
                                                                          'till that time tick based candles will be used
                                                                          logger.Warn(ex)
                                                                          If ex.GetType Is GetType(AggregateException) Then
                                                                              For Each e In CType(ex, AggregateException).Flatten.InnerExceptions
                                                                                  If e.GetType Is GetType(WebException) Then
                                                                                      If e.Message.Contains("401") Then
                                                                                          OnFetcherError(Me.ToString, e.Message)
                                                                                          CType(ParentController, AliceStrategyController).OnSessionExpireAsync()
                                                                                      End If
                                                                                  End If
                                                                              Next
                                                                          End If
                                                                          If Not ex.GetType Is GetType(OperationCanceledException) Then
                                                                              OnFetcherError(Me.ToString, ex.Message)
                                                                          End If
                                                                      End Try
                                                                      Return True
                                                                  End Function)
                        'OnHeartbeat("Polling historical candles")
                        logger.Debug("Polling historical candles")
                        sw.Start()
                        Await Task.WhenAll(tasks).ConfigureAwait(False)
                        sw.Stop()
                        Console.WriteLine(String.Format("Get Historical and Calling candle processor time:{0}", sw.ElapsedMilliseconds))
                        If Me.ParentController.APIConnection Is Nothing OrElse apiConnectionBeingUsed Is Nothing OrElse
                        (Me.ParentController.APIConnection IsNot Nothing AndAlso apiConnectionBeingUsed IsNot Nothing AndAlso
                        Not Me.ParentController.APIConnection.Equals(apiConnectionBeingUsed)) Then
                            Debug.WriteLine("Exiting start polling")
                            Exit While
                        End If
                    End If
                    _cts.Token.ThrowIfCancellationRequested()

                    If Utilities.Time.IsDateTimeEqualTillMinutes(lastTimeWhenDone, nextTimeToDo) Then
                        'Already done for this minute
                        lastTimeWhenDone = lastTimeWhenDone.AddMinutes(1)
                        nextTimeToDo = New Date(lastTimeWhenDone.Year, lastTimeWhenDone.Month, lastTimeWhenDone.Day, lastTimeWhenDone.Hour, lastTimeWhenDone.Minute, 5)
                    Else
                        nextTimeToDo = New Date(lastTimeWhenDone.Year, lastTimeWhenDone.Month, lastTimeWhenDone.Day, lastTimeWhenDone.Hour, lastTimeWhenDone.Minute, 5)
                    End If
                    Console.WriteLine(nextTimeToDo.ToLongTimeString)

                    While Now < nextTimeToDo
                        _cts.Token.ThrowIfCancellationRequested()
                        If _stopPollRunning Then
                            Exit While
                        End If
                        Await Task.Delay(1000, _cts.Token).ConfigureAwait(False)
                    End While
                End While
            Catch ex As Exception
                logger.Error("Instrument Identifier:{0}, error:{1}", _instrumentIdentifer, ex.ToString)
                Me.ParentController.OrphanException = ex
            Finally
                _isPollRunning = False
            End Try
        End Function

        Public Overrides Async Function SubscribeAsync(ByVal tradableInstruments As IEnumerable(Of IInstrument), ByVal maxNumberOfDays As Integer) As Task
            'logger.Debug("{0}->SubscribeAsync, instrumentIdentifiers:{1}", Me.ToString, Utils.JsonSerialize(instrumentIdentifiers))
            _cts.Token.ThrowIfCancellationRequested()
            Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
            If _subscribedInstruments Is Nothing Then
                _subscribedInstruments = New Concurrent.ConcurrentBag(Of IInstrument)
            End If
            For Each runningInstrument In tradableInstruments
                _cts.Token.ThrowIfCancellationRequested()
                Dim existingSubscribeInstruments As IEnumerable(Of IInstrument) = _subscribedInstruments.Where(Function(x)
                                                                                                                   Return x.InstrumentIdentifier = runningInstrument.InstrumentIdentifier
                                                                                                               End Function)
                If existingSubscribeInstruments IsNot Nothing AndAlso existingSubscribeInstruments.Count > 0 Then
                    If maxNumberOfDays > _daysToGoBack Then existingSubscribeInstruments.FirstOrDefault.IsHistoricalCompleted = False
                    Continue For
                End If
                _subscribedInstruments.Add(runningInstrument)
            Next
            _daysToGoBack = Math.Max(_daysToGoBack, maxNumberOfDays)
            If _subscribedInstruments Is Nothing OrElse _subscribedInstruments.Count = 0 Then
                OnHeartbeat("No instruments were subscribed for historical as they may be already subscribed")
                logger.Error("No tokens to subscribe for historical")
            Else
                OnHeartbeat(String.Format("Subscribed:{0} instruments for historical", _subscribedInstruments.Count))
            End If
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetType.ToString
        End Function

        Public Overrides Sub ClearLocalUniqueSubscriptionList()
            _subscribedInstruments = Nothing
        End Sub

        Public Overrides Function IsConnected() As Boolean
            Return _isPollRunning
        End Function

        Public Overrides Async Function CloseFetcherIfConnectedAsync(ByVal forceClose As Boolean) As Task
            'Intentionally no _cts.Token.ThrowIfCancellationRequested() since we need to close the fetcher when cancellation is done
            While IsConnected()
                _stopPollRunning = True
                If forceClose Then Exit While
                Await Task.Delay(100, _cts.Token).ConfigureAwait(False)
            End While
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    Dim currentAliceStrategyController As AliceStrategyController = CType(ParentController, AliceStrategyController)

                    RemoveHandler Me.FetcherCandles, AddressOf currentAliceStrategyController.OnFetcherCandlesAsync
                    RemoveHandler Me.FetcherError, AddressOf currentAliceStrategyController.OnFetcherError
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
End Namespace