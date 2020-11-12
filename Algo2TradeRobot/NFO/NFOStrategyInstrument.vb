Imports System.Threading
Imports Algo2TradeCore.Adapter
Imports Algo2TradeCore.Entities
Imports Algo2TradeCore.Strategies
Imports NLog
Imports Algo2TradeCore.Entities.Indicators
Imports Utilities.Numbers
Imports Algo2TradeCore.ChartHandler.ChartStyle
Imports Algo2TradeCore

Public Class NFOStrategyInstrument
    Inherits StrategyInstrument
    Implements IDisposable

#Region "Logging and Status Progress"
    Public Shared Shadows logger As Logger = LogManager.GetCurrentClassLogger
#End Region

    Public Property ForceExitByUser As Boolean
    'Public Property ForceExitForExpiry As Boolean
    Public Property ForceExitForContractRolloverDone As Boolean
    Public Property ForceExitForContractRollover As Boolean
    Public Property ForceEntryForContractRolloverDone As Boolean
    Public Property ForceEntryForContractRollover As Boolean

    Private _lastPrevPayloadPlaceOrder As String = ""

    Private ReadOnly _dummyLTEMA1Consumer As EMAConsumer
    Private ReadOnly _dummyLTEMA2Consumer As EMAConsumer
    Private ReadOnly _dummyHTEMA1Consumer As EMAConsumer
    Private ReadOnly _dummyHTEMA2Consumer As EMAConsumer

    Public _Direction As IOrder.TypeOfTransaction = IOrder.TypeOfTransaction.None

    Public DependentOptionStrategyInstruments As IEnumerable(Of NFOStrategyInstrument)

    Private ReadOnly _ParentInstrument As Boolean

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

        _ParentInstrument = parentInstrument

        If _ParentInstrument Then
            RawPayloadDependentConsumers = New List(Of IPayloadConsumer)
            If Me.ParentStrategy.IsStrategyCandleStickBased Then
                Dim userInputs As NFOUserInputs = Me.ParentStrategy.UserSettings
                Dim ltchartConsumer As PayloadToChartConsumer = New PayloadToChartConsumer(userInputs.SignalTimeFrame)
                ltchartConsumer.OnwardLevelConsumers = New List(Of IPayloadConsumer)
                ltchartConsumer.OnwardLevelConsumers.Add(New EMAConsumer(ltchartConsumer, userInputs.LTEMA1Period, TypeOfField.Close))
                ltchartConsumer.OnwardLevelConsumers.Add(New EMAConsumer(ltchartConsumer, userInputs.LTEMA2Period, TypeOfField.Close))
                RawPayloadDependentConsumers.Add(ltchartConsumer)
                _dummyLTEMA1Consumer = New EMAConsumer(ltchartConsumer, userInputs.LTEMA1Period, TypeOfField.Close)
                _dummyLTEMA2Consumer = New EMAConsumer(ltchartConsumer, userInputs.LTEMA2Period, TypeOfField.Close)

                Dim htchartConsumer As PayloadToChartConsumer = New PayloadToChartConsumer(userInputs.HigherTimeframe)
                htchartConsumer.OnwardLevelConsumers = New List(Of IPayloadConsumer)
                htchartConsumer.OnwardLevelConsumers.Add(New EMAConsumer(htchartConsumer, userInputs.HTEMA1Period, TypeOfField.Close))
                htchartConsumer.OnwardLevelConsumers.Add(New EMAConsumer(htchartConsumer, userInputs.HTEMA2Period, TypeOfField.Close))
                RawPayloadDependentConsumers.Add(htchartConsumer)
                _dummyHTEMA1Consumer = New EMAConsumer(htchartConsumer, userInputs.HTEMA1Period, TypeOfField.Close)
                _dummyHTEMA2Consumer = New EMAConsumer(htchartConsumer, userInputs.HTEMA2Period, TypeOfField.Close)
            End If
        End If
        Me.ForceExitByUser = False
        Me.ForceExitForContractRollover = False
    End Sub

    Public Overrides Async Function PopulateChartAndIndicatorsAsync(candleCreator As Chart, currentCandle As OHLCPayload) As Task
        If RawPayloadDependentConsumers IsNot Nothing AndAlso RawPayloadDependentConsumers.Count > 0 Then
            For Each runningRawPayloadConsumer In RawPayloadDependentConsumers
                If runningRawPayloadConsumer.TypeOfConsumer = IPayloadConsumer.ConsumerType.Chart Then
                    Dim currentXMinute As Date = candleCreator.ConvertTimeframe(CType(runningRawPayloadConsumer, PayloadToChartConsumer).Timeframe,
                                                                currentCandle,
                                                                runningRawPayloadConsumer)
                    If candleCreator.IndicatorCreator Is Nothing Then candleCreator.IndicatorCreator = New ChartHandler.Indicator.IndicatorManeger(Me.ParentStrategy.ParentController, candleCreator, _cts)
                    If currentXMinute <> Date.MaxValue Then
                        If runningRawPayloadConsumer.OnwardLevelConsumers IsNot Nothing AndAlso runningRawPayloadConsumer.OnwardLevelConsumers.Count > 0 Then
                            For Each consumer In runningRawPayloadConsumer.OnwardLevelConsumers
                                'candleCreator.IndicatorCreator.CalculateEMA(currentXMinute, consumer)

                                Dim outputConsumer As Indicators.EMAConsumer = consumer
                                If outputConsumer IsNot Nothing AndAlso outputConsumer.ParentConsumer IsNot Nothing AndAlso
                                    outputConsumer.ParentConsumer.ConsumerPayloads IsNot Nothing AndAlso
                                    outputConsumer.ParentConsumer.ConsumerPayloads.Count > 0 Then
                                    Dim minimumNumberOfCandlesRequied As Integer = outputConsumer.EMAPeriod * 3
                                    Dim counter As Integer = 0
                                    Dim dateToCalculateFrom As Date = Date.MinValue
                                    For Each runningInputDate In outputConsumer.ParentConsumer.ConsumerPayloads.Keys.OrderByDescending(Function(x)
                                                                                                                                           Return x
                                                                                                                                       End Function)
                                        counter += 1
                                        If counter >= minimumNumberOfCandlesRequied Then
                                            dateToCalculateFrom = runningInputDate
                                            Exit For
                                        End If
                                    Next
                                    If currentXMinute > dateToCalculateFrom Then
                                        candleCreator.IndicatorCreator.CalculateEMA(currentXMinute, consumer)
                                    Else
                                        candleCreator.IndicatorCreator.CalculateEMA(dateToCalculateFrom, consumer)
                                    End If
                                End If
                            Next
                        End If
                    End If
                End If
            Next
        End If
    End Function

    Private Async Function SubscribeOptionInstrumentsFromPosition() As Task
        If _ParentInstrument Then
            Dim availablePositions As Concurrent.ConcurrentBag(Of IPosition) = Await Me.ParentStrategy.ParentController.GetPositionDetailsAsync().ConfigureAwait(False)
            If availablePositions IsNot Nothing AndAlso availablePositions.Count > 0 Then
                Dim allInstruments As IEnumerable(Of IInstrument) = CType(Me.ParentStrategy, NFOStrategy).DependentInstruments
                If allInstruments IsNot Nothing AndAlso allInstruments.Count > 0 Then
                    Dim tradableInstruments As List(Of IInstrument) = Nothing
                    For Each runningPosition In availablePositions
                        Dim instrument As IInstrument = allInstruments.ToList.Find(Function(x)
                                                                                       Return x.InstrumentIdentifier = runningPosition.InstrumentIdentifier
                                                                                   End Function)
                        If instrument IsNot Nothing Then
                            If instrument.RawInstrumentName = Me.TradableInstrument.RawInstrumentName Then
                                If tradableInstruments Is Nothing Then tradableInstruments = New List(Of IInstrument)
                                tradableInstruments.Add(instrument)
                            End If
                        End If
                    Next
                    If tradableInstruments IsNot Nothing AndAlso tradableInstruments.Count > 0 Then
                        Await CreateStrategyInstrumentAndPopulate(tradableInstruments).ConfigureAwait(False)
                    End If
                End If
            End If
        End If
    End Function

    Private Async Function CreateStrategyInstrumentAndPopulate(ByVal instrumentList As List(Of IInstrument)) As Task
        Await CType(Me.ParentStrategy, NFOStrategy).CreateDependentTradableStrategyInstrumentsAsync(instrumentList).ConfigureAwait(False)
        Dim tradableStrategyInstruments As List(Of NFOStrategyInstrument) = New List(Of NFOStrategyInstrument)
        For Each runningInstrument In instrumentList
            Dim subscribedStrategyInstrument As StrategyInstrument =
                Me.ParentStrategy.TradableStrategyInstruments.ToList.Find(Function(x)
                                                                              Return x.TradableInstrument.InstrumentIdentifier = runningInstrument.InstrumentIdentifier
                                                                          End Function)
            tradableStrategyInstruments.Add(subscribedStrategyInstrument)
        Next
        If DependentOptionStrategyInstruments IsNot Nothing AndAlso DependentOptionStrategyInstruments.Count > 0 Then
            DependentOptionStrategyInstruments = DependentOptionStrategyInstruments.Concat(tradableStrategyInstruments)
        Else
            DependentOptionStrategyInstruments = tradableStrategyInstruments
        End If
        Await Task.Delay(1000).ConfigureAwait(False)
    End Function

    Public Overrides Async Function MonitorAsync() As Task
        Try
            _strategyInstrumentRunning = True
            If _ParentInstrument Then
                Await SubscribeOptionInstrumentsFromPosition().ConfigureAwait(False)
                Dim userInputs As NFOUserInputs = Me.ParentStrategy.UserSettings
                While True
                    If Me.ParentStrategy.ParentController.OrphanException IsNot Nothing Then
                        Throw Me.ParentStrategy.ParentController.OrphanException
                    End If
                    _cts.Token.ThrowIfCancellationRequested()

                    Dim placeOrderTriggers As List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)) = Await IsTriggerReceivedForPlaceOrderAsync(False).ConfigureAwait(False)
                    If placeOrderTriggers IsNot Nothing AndAlso placeOrderTriggers.Count > 0 Then
                        Dim placeOrderTrigger As Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String) = placeOrderTriggers.FirstOrDefault
                        If placeOrderTrigger IsNot Nothing AndAlso placeOrderTrigger.Item1 = ExecuteCommandAction.Take Then
                            If placeOrderTrigger.Item2.Quantity <> 0 Then
                                If Me.TradableInstrument.RawInstrumentName = "BANKNIFTY" OrElse Me.TradableInstrument.RawInstrumentName = "NIFTY" Then
                                    If placeOrderTrigger.Item2.Supporting IsNot Nothing AndAlso placeOrderTrigger.Item2.Supporting.Count > 0 Then
                                        'Find strategy instrument and exit
                                        Dim processOption As Boolean = False
                                        Dim orderResponse = Await ExecuteCommandAsync(ExecuteCommands.PlaceRegularLimitCNCOrder, Nothing).ConfigureAwait(False)
                                        If orderResponse IsNot Nothing AndAlso orderResponse.Count > 0 Then
                                            Dim placeOrderResponse = CType(orderResponse, Concurrent.ConcurrentBag(Of Object)).FirstOrDefault
                                            If placeOrderResponse IsNot Nothing AndAlso placeOrderResponse.ContainsKey("data") AndAlso
                                                placeOrderResponse("data").ContainsKey("order_id") Then
                                                processOption = True
                                            ElseIf Me.GetQuantityToTrade() = 0 Then
                                                processOption = True
                                            End If
                                        ElseIf Me.GetQuantityToTrade() = 0 Then
                                            processOption = True
                                        End If
                                        If processOption Then
                                            Await Task.Delay(1000).ConfigureAwait(False)
                                            For Each runningStrategyInstrument In Me.DependentOptionStrategyInstruments
                                                If runningStrategyInstrument.GetQuantityToTrade() > 0 Then
                                                    Await runningStrategyInstrument.MonitorAsync(command:=ExecuteCommands.PlaceRegularLimitCNCOrder, data:="SELL").ConfigureAwait(False)
                                                End If
                                            Next
                                            Await Task.Delay(5000).ConfigureAwait(False)
                                            If ForceExitByUser Then ForceExitByUser = False
                                            If ForceExitForContractRollover Then ForceExitForContractRollover = False
                                        End If
                                        'ElseIf Not Me.ForceExitForExpiry Then
                                    Else
                                        'Create strategy instrument and entry
                                        Try
                                            logger.Debug("PlaceOrder-> ************************************************ {0}", Me.TradableInstrument.TradingSymbol)
                                            logger.Debug("PlaceOrder-> Parameters: Force Exit by user:{0}, Force Entry for contract rollover:{1}, TradingSymbol:{2}", Me.ForceExitByUser, Me.ForceEntryForContractRollover, Me.TradableInstrument.TradingSymbol)
                                        Catch ex As Exception
                                            logger.Error(ex)
                                        End Try
                                        Dim optionInstrument As IInstrument = Nothing
                                        Dim minExpiry As Date = CType(Me.ParentStrategy, NFOStrategy).DependentInstruments.Min(Function(x)
                                                                                                                                   If x.RawInstrumentName = Me.TradableInstrument.RawInstrumentName Then
                                                                                                                                       Dim expiry As Date = x.Expiry.Value
                                                                                                                                       If x.Expiry.Value = Me.TradableInstrument.Expiry.Value Then
                                                                                                                                           expiry = x.Expiry.Value.AddDays(-2)
                                                                                                                                       End If
                                                                                                                                       If Now.Date < expiry.Date Then
                                                                                                                                           Return x.Expiry.Value
                                                                                                                                       ElseIf Now.Date = expiry.Date Then
                                                                                                                                           If Now < Me.TradableInstrument.ExchangeDetails.ContractRolloverTime Then
                                                                                                                                               Return x.Expiry.Value
                                                                                                                                           Else
                                                                                                                                               Return Date.MaxValue
                                                                                                                                           End If
                                                                                                                                       Else
                                                                                                                                           Return Date.MaxValue
                                                                                                                                       End If
                                                                                                                                   Else
                                                                                                                                       Return Date.MaxValue
                                                                                                                                   End If
                                                                                                                               End Function)
                                        If placeOrderTrigger.Item2.EntryDirection = IOrder.TypeOfTransaction.Buy Then
                                            Dim stockPrice As Decimal = Me.TradableInstrument.LastTick.LastPrice
                                            Dim otmPutStrike As Decimal = stockPrice - (stockPrice * userInputs.StrikePriceRangePercentage / 100)

                                            optionInstrument = CType(Me.ParentStrategy, NFOStrategy).DependentInstruments.Where(Function(x)
                                                                                                                                    Return x.Strike <= otmPutStrike AndAlso x.RawInstrumentType = "PE" AndAlso
                                                                                                                                          x.Expiry.Value.Date = minExpiry.Date AndAlso
                                                                                                                                          x.RawInstrumentName = Me.TradableInstrument.RawInstrumentName
                                                                                                                                End Function).OrderBy(Function(y)
                                                                                                                                                          Return y.Strike
                                                                                                                                                      End Function).LastOrDefault
                                        ElseIf placeOrderTrigger.Item2.EntryDirection = IOrder.TypeOfTransaction.Sell Then
                                            Dim stockPrice As Decimal = Me.TradableInstrument.LastTick.LastPrice
                                            Dim otmCallStrike As Decimal = stockPrice + (stockPrice * userInputs.StrikePriceRangePercentage / 100)

                                            optionInstrument = CType(Me.ParentStrategy, NFOStrategy).DependentInstruments.Where(Function(x)
                                                                                                                                    Return x.Strike >= otmCallStrike AndAlso x.RawInstrumentType = "CE" AndAlso
                                                                                                                                          x.Expiry.Value.Date = minExpiry.Date AndAlso
                                                                                                                                          x.RawInstrumentName = Me.TradableInstrument.RawInstrumentName
                                                                                                                                End Function).OrderBy(Function(y)
                                                                                                                                                          Return y.Strike
                                                                                                                                                      End Function).FirstOrDefault
                                        End If
                                        If optionInstrument IsNot Nothing Then
                                            Dim otmStrategyInstrument As NFOStrategyInstrument = Nothing
                                            If Me.DependentOptionStrategyInstruments IsNot Nothing AndAlso Me.DependentOptionStrategyInstruments.Count > 0 Then
                                                otmStrategyInstrument = DependentOptionStrategyInstruments.ToList.Find(Function(x)
                                                                                                                           Return x.TradableInstrument.TradingSymbol = optionInstrument.TradingSymbol
                                                                                                                       End Function)
                                            End If
                                            If otmStrategyInstrument Is Nothing Then
                                                Await CreateStrategyInstrumentAndPopulate(New List(Of IInstrument) From {optionInstrument}).ConfigureAwait(False)
                                                otmStrategyInstrument = DependentOptionStrategyInstruments.ToList.Find(Function(x)
                                                                                                                           Return x.TradableInstrument.TradingSymbol = optionInstrument.TradingSymbol
                                                                                                                       End Function)
                                            End If
                                            If otmStrategyInstrument IsNot Nothing Then
                                                Await otmStrategyInstrument.MonitorAsync(command:=ExecuteCommands.PlaceRegularLimitCNCOrder, data:="BUY").ConfigureAwait(False)
                                                If otmStrategyInstrument._Direction = IOrder.TypeOfTransaction.None Then
                                                    Await Task.Delay(2000).ConfigureAwait(False)
                                                    Await ExecuteCommandAsync(ExecuteCommands.PlaceRegularLimitCNCOrder, Nothing).ConfigureAwait(False)
                                                    Await Task.Delay(5000).ConfigureAwait(False)
                                                    If Me.ForceEntryForContractRollover Then
                                                        Me.ForceEntryForContractRollover = False
                                                        Me.ForceEntryForContractRolloverDone = True
                                                    End If
                                                End If
                                            End If
                                        End If
                                    End If
                                Else    'Normal Stock
                                    If placeOrderTrigger.Item2.Supporting IsNot Nothing AndAlso placeOrderTrigger.Item2.Supporting.Count > 0 Then
                                        'Find strategy instrument and exit
                                        Try
                                            logger.Debug("PlaceOrder-> ************************************************ {0}", Me.TradableInstrument.TradingSymbol)
                                            logger.Debug("PlaceOrder-> Parameters: Force Exit by user:{0}, Force Exit for contract rollover:{1}, TradingSymbol:{2}", Me.ForceExitByUser, Me.ForceExitForContractRollover, Me.TradableInstrument.TradingSymbol)
                                        Catch ex As Exception
                                            logger.Error(ex)
                                        End Try
                                        Dim processOption As Boolean = False
                                        Dim orderResponse = Await ExecuteCommandAsync(ExecuteCommands.PlaceRegularLimitCNCOrder, Nothing).ConfigureAwait(False)
                                        If orderResponse IsNot Nothing AndAlso orderResponse.Count > 0 Then
                                            Dim placeOrderResponse = CType(orderResponse, Concurrent.ConcurrentBag(Of Object)).FirstOrDefault
                                            If placeOrderResponse IsNot Nothing AndAlso placeOrderResponse.ContainsKey("data") AndAlso
                                                placeOrderResponse("data").ContainsKey("order_id") Then
                                                processOption = True
                                            ElseIf Me.GetQuantityToTrade() = 0 Then
                                                processOption = True
                                            End If
                                        ElseIf Me.GetQuantityToTrade() = 0 Then
                                            processOption = True
                                        End If
                                        If processOption Then
                                            Await Task.Delay(1000).ConfigureAwait(False)
                                            For Each runningStrategyInstrument In Me.DependentOptionStrategyInstruments
                                                If runningStrategyInstrument.GetQuantityToTrade() > 0 Then
                                                    Await runningStrategyInstrument.MonitorAsync(command:=ExecuteCommands.PlaceRegularLimitCNCOrder, data:="SELL").ConfigureAwait(False)
                                                End If
                                            Next
                                            Await Task.Delay(5000).ConfigureAwait(False)
                                            If ForceExitByUser Then ForceExitByUser = False
                                            If ForceExitForContractRollover Then ForceExitForContractRollover = False
                                        End If
                                    Else
                                        'Create strategy instrument and entry
                                        Try
                                            logger.Debug("PlaceOrder-> ************************************************ {0}", Me.TradableInstrument.TradingSymbol)
                                            logger.Debug("PlaceOrder-> Parameters: Force Exit by user:{0}, Force Entry for contract rollover:{1}, TradingSymbol:{2}", Me.ForceExitByUser, Me.ForceEntryForContractRollover, Me.TradableInstrument.TradingSymbol)
                                        Catch ex As Exception
                                            logger.Error(ex)
                                        End Try
                                        Dim optionInstrument As IInstrument = Nothing
                                        If placeOrderTrigger.Item2.EntryDirection = IOrder.TypeOfTransaction.Buy Then
                                            Dim stockPrice As Decimal = Me.TradableInstrument.LastTick.LastPrice
                                            Dim putStrike As Decimal = stockPrice - (stockPrice * userInputs.StrikePriceRangePercentage / 100)
                                            optionInstrument = CType(Me.ParentStrategy, NFOStrategy).DependentInstruments.Where(Function(x)
                                                                                                                                    Return x.Strike <= putStrike AndAlso x.RawInstrumentType = "PE" AndAlso
                                                                                                                                          x.Expiry.Value.Date = Me.TradableInstrument.Expiry.Value.Date AndAlso
                                                                                                                                          x.RawInstrumentName = Me.TradableInstrument.RawInstrumentName
                                                                                                                                End Function).OrderBy(Function(y)
                                                                                                                                                          Return y.Strike
                                                                                                                                                      End Function).LastOrDefault
                                        ElseIf placeOrderTrigger.Item2.EntryDirection = IOrder.TypeOfTransaction.Sell Then
                                            Dim stockPrice As Decimal = Me.TradableInstrument.LastTick.LastPrice
                                            Dim callStrike As Decimal = stockPrice + (stockPrice * userInputs.StrikePriceRangePercentage / 100)

                                            optionInstrument = CType(Me.ParentStrategy, NFOStrategy).DependentInstruments.Where(Function(x)
                                                                                                                                    Return x.Strike >= callStrike AndAlso x.RawInstrumentType = "CE" AndAlso
                                                                                                                                          x.Expiry.Value.Date = Me.TradableInstrument.Expiry.Value.Date AndAlso
                                                                                                                                          x.RawInstrumentName = Me.TradableInstrument.RawInstrumentName
                                                                                                                                End Function).OrderBy(Function(y)
                                                                                                                                                          Return y.Strike
                                                                                                                                                      End Function).FirstOrDefault
                                        End If
                                        If optionInstrument IsNot Nothing Then
                                            Dim otmStrategyInstrument As NFOStrategyInstrument = Nothing
                                            If Me.DependentOptionStrategyInstruments IsNot Nothing AndAlso Me.DependentOptionStrategyInstruments.Count > 0 Then
                                                otmStrategyInstrument = DependentOptionStrategyInstruments.ToList.Find(Function(x)
                                                                                                                           Return x.TradableInstrument.TradingSymbol = optionInstrument.TradingSymbol
                                                                                                                       End Function)
                                            End If
                                            If otmStrategyInstrument Is Nothing Then
                                                Await CreateStrategyInstrumentAndPopulate(New List(Of IInstrument) From {optionInstrument}).ConfigureAwait(False)
                                                otmStrategyInstrument = DependentOptionStrategyInstruments.ToList.Find(Function(x)
                                                                                                                           Return x.TradableInstrument.TradingSymbol = optionInstrument.TradingSymbol
                                                                                                                       End Function)
                                            End If
                                            If otmStrategyInstrument IsNot Nothing Then
                                                Await otmStrategyInstrument.MonitorAsync(command:=ExecuteCommands.PlaceRegularLimitCNCOrder, data:="BUY").ConfigureAwait(False)
                                                If otmStrategyInstrument._Direction = IOrder.TypeOfTransaction.None Then
                                                    Await Task.Delay(2000).ConfigureAwait(False)
                                                    Await ExecuteCommandAsync(ExecuteCommands.PlaceRegularLimitCNCOrder, Nothing).ConfigureAwait(False)
                                                    Await Task.Delay(5000).ConfigureAwait(False)
                                                    If Me.ForceEntryForContractRollover Then
                                                        Me.ForceEntryForContractRollover = False
                                                        Me.ForceEntryForContractRolloverDone = True
                                                    End If
                                                End If
                                            End If
                                        End If
                                    End If
                                End If
                            Else
                                If ForceExitByUser Then
                                    ForceExitByUser = False
                                    OnHeartbeat(String.Format("No position available for force exit: {0}", Me.TradableInstrument.TradingSymbol))
                                End If
                                If ForceExitForContractRollover Then
                                    ForceExitForContractRollover = False
                                    If Me.TradableInstrument.RawInstrumentName = "BANKNIFTY" OrElse Me.TradableInstrument.RawInstrumentName = "NIFTY" Then
                                        If IsMyAnotherContractAvailable().Item1 Then
                                            CType(IsMyAnotherContractAvailable().Item2, NFOStrategyInstrument).DependentOptionStrategyInstruments = Me.DependentOptionStrategyInstruments
                                            Me.DependentOptionStrategyInstruments = Nothing
                                        End If
                                    Else
                                        OnHeartbeat(String.Format("No position available for contract rollover force exit: {0}", Me.TradableInstrument.TradingSymbol))
                                    End If
                                End If
                                If ForceEntryForContractRollover Then
                                    ForceEntryForContractRollover = False
                                    ForceEntryForContractRolloverDone = True
                                    OnHeartbeat(String.Format("No position available for contract rollover force entry: {0}", Me.TradableInstrument.TradingSymbol))
                                End If
                            End If
                        End If
                    End If

                    Await Task.Delay(1000, _cts.Token).ConfigureAwait(False)
                End While
            End If
        Catch ex As Exception
            'To log exceptions getting created from this function as the bubble up of the exception
            'will anyways happen to Strategy.MonitorAsync but it will not be shown until all tasks exit
            logger.Error("Strategy Instrument:{0}, error:{1}", Me.ToString, ex.ToString)
            Throw ex
        Finally
            _strategyInstrumentRunning = False
        End Try
    End Function

    Public Overrides Async Function MonitorAsync(command As ExecuteCommands, data As Object) As Task
        If Not _ParentInstrument Then
            If data = "BUY" Then
                _Direction = IOrder.TypeOfTransaction.Buy
            ElseIf data = "SELL" Then
                _Direction = IOrder.TypeOfTransaction.Sell
            End If
            If command = ExecuteCommands.PlaceRegularLimitCNCOrder Then
                Dim orderResponse = Await ExecuteCommandAsync(ExecuteCommands.PlaceRegularLimitCNCOrder, Nothing).ConfigureAwait(False)
                If orderResponse IsNot Nothing AndAlso orderResponse.Count > 0 Then
                    Dim placeOrderResponse = CType(orderResponse, Concurrent.ConcurrentBag(Of Object)).FirstOrDefault
                    If placeOrderResponse IsNot Nothing AndAlso placeOrderResponse.ContainsKey("data") AndAlso
                        placeOrderResponse("data").ContainsKey("order_id") Then
                        _Direction = IOrder.TypeOfTransaction.None
                    ElseIf Me.GetQuantityToTrade() = 0 Then
                        _Direction = IOrder.TypeOfTransaction.None
                    End If
                ElseIf Me.GetQuantityToTrade() = 0 Then
                    _Direction = IOrder.TypeOfTransaction.None
                End If
            End If
        End If
    End Function

    Protected Overrides Async Function IsTriggerReceivedForPlaceOrderAsync(forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)))
        Dim ret As List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        Dim parameters As PlaceOrderParameters = Nothing
        Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
        If _ParentInstrument Then
            Dim ltRunningCandlePayload As OHLCPayload = GetXMinuteCurrentCandle(userSettings.SignalTimeFrame)
            Dim htRunningCandlePayload As OHLCPayload = GetXMinuteCurrentCandle(userSettings.HigherTimeframe)
            Dim ltema1Consumer As EMAConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummyLTEMA1Consumer)
            Dim ltema2Consumer As EMAConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummyLTEMA2Consumer)
            Dim htema1Consumer As EMAConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummyHTEMA1Consumer)
            Dim htema2Consumer As EMAConsumer = GetConsumer(Me.RawPayloadDependentConsumers, _dummyHTEMA2Consumer)
            Dim currentTime As Date = Now()
            Dim currentTick As ITick = Me.TradableInstrument.LastTick

            Try
                If ltRunningCandlePayload IsNot Nothing AndAlso ltRunningCandlePayload.PreviousPayload IsNot Nothing AndAlso
                    htRunningCandlePayload IsNot Nothing AndAlso htRunningCandlePayload.PreviousPayload IsNot Nothing AndAlso
                    Me.TradableInstrument.IsHistoricalCompleted Then
                    If Not ltRunningCandlePayload.PreviousPayload.ToString = _lastPrevPayloadPlaceOrder Then
                        _lastPrevPayloadPlaceOrder = ltRunningCandlePayload.PreviousPayload.ToString
                        logger.Debug("PlaceOrder-> LT Potential Signal Candle is:{0}. Will check rest parameters.", ltRunningCandlePayload.PreviousPayload.ToString)
                        logger.Debug("PlaceOrder-> HT Potential Signal Candle is:{0}. Will check rest parameters.", htRunningCandlePayload.PreviousPayload.ToString)
                        logger.Debug("PlaceOrder-> Rest all parameters: LTRunningCandleTime:{0}, LTPayloadGeneratedBy:{1}, HTRunningCandleTime:{2}, HTPayloadGeneratedBy:{3}, IsHistoricalCompleted:{4}, IsFirstTimeInformationCollected:{5}, LT EMA({6}):{7}, LT EMA({8}):{9}, HT EMA({10}):{11}, HT EMA({12}):{13}, Exchange Start Time:{14}, Exchange End Time:{15}, Current Time:{16}, Is My Another Contract Available:{17}, Contract Rollover Time:{18}, TradingSymbol:{19}",
                                    ltRunningCandlePayload.SnapshotDateTime.ToString,
                                    ltRunningCandlePayload.PayloadGeneratedBy.ToString,
                                    htRunningCandlePayload.SnapshotDateTime.ToString,
                                    htRunningCandlePayload.PayloadGeneratedBy.ToString,
                                    Me.TradableInstrument.IsHistoricalCompleted,
                                    Me.ParentStrategy.IsFirstTimeInformationCollected,
                                    userSettings.LTEMA1Period,
                                    ltema1Consumer.ConsumerPayloads(ltRunningCandlePayload.PreviousPayload.SnapshotDateTime).ToString,
                                    userSettings.LTEMA2Period,
                                    ltema2Consumer.ConsumerPayloads(ltRunningCandlePayload.PreviousPayload.SnapshotDateTime).ToString,
                                    userSettings.HTEMA1Period,
                                    htema1Consumer.ConsumerPayloads(htRunningCandlePayload.PreviousPayload.SnapshotDateTime).ToString,
                                    userSettings.HTEMA2Period,
                                    htema2Consumer.ConsumerPayloads(htRunningCandlePayload.PreviousPayload.SnapshotDateTime).ToString,
                                    Me.TradableInstrument.ExchangeDetails.ExchangeStartTime.ToString,
                                    Me.TradableInstrument.ExchangeDetails.ExchangeEndTime.ToString,
                                    currentTime.ToString,
                                    IsMyAnotherContractAvailable(),
                                    Me.TradableInstrument.ExchangeDetails.ContractRolloverTime.ToString,
                                    Me.TradableInstrument.TradingSymbol)
                    End If
                End If
            Catch ex As Exception
                logger.Error(ex)
            End Try

            If (ForceExitByUser OrElse ForceExitForContractRollover OrElse ForceEntryForContractRollover) AndAlso
                currentTime >= Me.TradableInstrument.ExchangeDetails.ExchangeStartTime AndAlso
                currentTime <= Me.TradableInstrument.ExchangeDetails.ExchangeEndTime AndAlso
                Me.TradableInstrument.IsHistoricalCompleted AndAlso ltRunningCandlePayload IsNot Nothing Then

                Dim quantity As Integer = GetQuantityToTrade()

                'If Me.ForceExitByUser OrElse Me.ForceExitForExpiry Then
                '    If Me.TradableInstrument.RawInstrumentName = "BANKNIFTY" OrElse Me.TradableInstrument.RawInstrumentName = "NIFTY" Then
                '        If IsBuyActive() OrElse IsSellActive() Then
                '            quantity = 1
                '        End If
                '    End If
                'End If

                If ForceExitForContractRollover Then
                    userSettings.InstrumentsData(Me.TradableInstrument.RawInstrumentName).ModifiedQuantity = quantity
                End If

                If quantity > 0 Then
                    Dim price As Decimal = currentTick.LastPrice - ConvertFloorCeling(currentTick.LastPrice * 0.3 / 100, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                    parameters = New PlaceOrderParameters(ltRunningCandlePayload) With
                                   {.EntryDirection = IOrder.TypeOfTransaction.Sell,
                                    .Price = price,
                                    .Quantity = Math.Abs(quantity),
                                    .Supporting = New List(Of Object) From {"Exit"}}
                Else
                    Dim price As Decimal = currentTick.LastPrice + ConvertFloorCeling(currentTick.LastPrice * 0.3 / 100, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                    parameters = New PlaceOrderParameters(ltRunningCandlePayload) With
                                  {.EntryDirection = IOrder.TypeOfTransaction.Buy,
                                   .Price = price,
                                   .Quantity = Math.Abs(quantity),
                                   .Supporting = New List(Of Object) From {"Exit"}}
                End If

                If Me.ForceEntryForContractRollover Then
                    quantity = userSettings.InstrumentsData(Me.TradableInstrument.RawInstrumentName).ModifiedQuantity
                    If quantity > 0 Then
                        Dim price As Decimal = currentTick.LastPrice + ConvertFloorCeling(currentTick.LastPrice * 0.3 / 100, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                        parameters = New PlaceOrderParameters(ltRunningCandlePayload) With
                                       {.EntryDirection = IOrder.TypeOfTransaction.Buy,
                                        .Price = price,
                                        .Quantity = Math.Abs(quantity)}
                    Else
                        Dim price As Decimal = currentTick.LastPrice - ConvertFloorCeling(currentTick.LastPrice * 0.3 / 100, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                        parameters = New PlaceOrderParameters(ltRunningCandlePayload) With
                                      {.EntryDirection = IOrder.TypeOfTransaction.Sell,
                                       .Price = price,
                                       .Quantity = Math.Abs(quantity)}
                    End If
                End If

            ElseIf currentTime >= Me.TradableInstrument.ExchangeDetails.ExchangeStartTime AndAlso currentTime <= Me.TradableInstrument.ExchangeDetails.ExchangeEndTime AndAlso Me.TradableInstrument.IsHistoricalCompleted AndAlso
                ltRunningCandlePayload IsNot Nothing AndAlso ltRunningCandlePayload.PayloadGeneratedBy = OHLCPayload.PayloadSource.CalculatedTick AndAlso ltRunningCandlePayload.PreviousPayload IsNot Nothing AndAlso
                htRunningCandlePayload IsNot Nothing AndAlso htRunningCandlePayload.PayloadGeneratedBy = OHLCPayload.PayloadSource.CalculatedTick AndAlso htRunningCandlePayload.PreviousPayload IsNot Nothing Then
                If (Me.TradableInstrument.Expiry.Value.Date.AddDays(-2) <> Now.Date AndAlso Not IsMyAnotherContractAvailable.Item1) OrElse
                    (Me.TradableInstrument.Expiry.Value.Date.AddDays(-2) <> Now.Date AndAlso IsMyAnotherContractAvailable.Item1 AndAlso currentTime >= Me.TradableInstrument.ExchangeDetails.ContractRolloverTime AndAlso Me.ForceEntryForContractRolloverDone) OrElse
                    (Me.TradableInstrument.Expiry.Value.Date.AddDays(-2) = Now.Date AndAlso IsMyAnotherContractAvailable.Item1 AndAlso currentTime < Me.TradableInstrument.ExchangeDetails.ContractRolloverTime) Then
                    If ltema1Consumer.ConsumerPayloads.ContainsKey(ltRunningCandlePayload.PreviousPayload.SnapshotDateTime) AndAlso
                        ltema2Consumer.ConsumerPayloads.ContainsKey(ltRunningCandlePayload.PreviousPayload.SnapshotDateTime) AndAlso
                        htema1Consumer.ConsumerPayloads.ContainsKey(htRunningCandlePayload.PreviousPayload.SnapshotDateTime) AndAlso
                        htema2Consumer.ConsumerPayloads.ContainsKey(htRunningCandlePayload.PreviousPayload.SnapshotDateTime) Then
                        'Check condition for entry
                        Dim ltema1 As Decimal = CType(ltema1Consumer.ConsumerPayloads(ltRunningCandlePayload.PreviousPayload.SnapshotDateTime), EMAConsumer.EMAPayload).EMA.Value
                        Dim ltema2 As Decimal = CType(ltema2Consumer.ConsumerPayloads(ltRunningCandlePayload.PreviousPayload.SnapshotDateTime), EMAConsumer.EMAPayload).EMA.Value
                        Dim htema1 As Decimal = CType(htema1Consumer.ConsumerPayloads(htRunningCandlePayload.PreviousPayload.SnapshotDateTime), EMAConsumer.EMAPayload).EMA.Value
                        Dim htema2 As Decimal = CType(htema2Consumer.ConsumerPayloads(htRunningCandlePayload.PreviousPayload.SnapshotDateTime), EMAConsumer.EMAPayload).EMA.Value

                        'If ema1 > ema2 AndAlso ema2 > ltEma3 AndAlso runningCandlePayload.PreviousPayload.ClosePrice.Value > ltEma3 Then
                        If ltema1 > ltema2 AndAlso htema1 > htema2 Then
                            Dim quantity As Integer = GetQuantityToTrade()
                            If quantity = 0 Then
                                quantity = Me.TradableInstrument.LotSize * userSettings.InstrumentsData(Me.TradableInstrument.RawInstrumentName).InitialQuantity
                                Dim price As Decimal = currentTick.LastPrice + ConvertFloorCeling(currentTick.LastPrice * 0.3 / 100, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                                parameters = New PlaceOrderParameters(ltRunningCandlePayload) With
                                               {.EntryDirection = IOrder.TypeOfTransaction.Buy,
                                                .Price = price,
                                                .Quantity = Math.Abs(quantity)}
                            End If
                            'ElseIf ema1 < ema2 AndAlso ema2 < ltEma3 AndAlso runningCandlePayload.PreviousPayload.ClosePrice.Value < ltEma3 Then
                        ElseIf ltema1 < ltema2 AndAlso htema1 < htema2 Then
                            Dim quantity As Integer = GetQuantityToTrade()
                            If quantity = 0 Then
                                quantity = Me.TradableInstrument.LotSize * userSettings.InstrumentsData(Me.TradableInstrument.RawInstrumentName).InitialQuantity
                                Dim price As Decimal = currentTick.LastPrice - ConvertFloorCeling(currentTick.LastPrice * 0.3 / 100, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                                parameters = New PlaceOrderParameters(ltRunningCandlePayload) With
                                           {.EntryDirection = IOrder.TypeOfTransaction.Sell,
                                            .Price = price,
                                            .Quantity = Math.Abs(quantity)}
                            End If
                        End If

                        ''Check condition for exit
                        'Dim preltEma1 As Decimal = CType(ltema1Consumer.ConsumerPayloads(ltRunningCandlePayload.PreviousPayload.PreviousPayload.SnapshotDateTime), EMAConsumer.EMAPayload).EMA.Value
                        'Dim preltEma2 As Decimal = CType(ltema2Consumer.ConsumerPayloads(ltRunningCandlePayload.PreviousPayload.PreviousPayload.SnapshotDateTime), EMAConsumer.EMAPayload).EMA.Value
                        'Dim prehtEma1 As Decimal = CType(htema1Consumer.ConsumerPayloads(htRunningCandlePayload.PreviousPayload.PreviousPayload.SnapshotDateTime), EMAConsumer.EMAPayload).EMA.Value
                        'Dim prehtEma2 As Decimal = CType(htema2Consumer.ConsumerPayloads(htRunningCandlePayload.PreviousPayload.PreviousPayload.SnapshotDateTime), EMAConsumer.EMAPayload).EMA.Value

                        'If (preEma1 > preEma2 AndAlso ema1 < ema2) AndAlso
                        '    (parameters Is Nothing OrElse parameters.EntryDirection <> IOrder.TypeOfTransaction.Sell) Then
                        'If preltEma1 > preltEma2 AndAlso ltema1 < ltema2 Then
                        If ltema1 < ltema2 AndAlso htema1 < htema2 Then
                            Dim quantity As Integer = GetQuantityToTrade()
                            'If quantity = 0 AndAlso (Me.TradableInstrument.RawInstrumentName = "BANKNIFTY" OrElse Me.TradableInstrument.RawInstrumentName = "NIFTY") Then
                            '    quantity = 1
                            'End If
                            If quantity > 0 Then
                                Dim price As Decimal = currentTick.LastPrice - ConvertFloorCeling(currentTick.LastPrice * 0.3 / 100, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                                parameters = New PlaceOrderParameters(ltRunningCandlePayload) With
                                       {.EntryDirection = IOrder.TypeOfTransaction.Sell,
                                        .Price = price,
                                        .Quantity = Math.Abs(quantity),
                                        .Supporting = New List(Of Object) From {"Exit"}}
                            End If
                            'ElseIf (preEma1 < preEma2 AndAlso ema1 > ema2) AndAlso
                            '    (parameters Is Nothing OrElse parameters.EntryDirection <> IOrder.TypeOfTransaction.Buy) Then
                            'ElseIf preltEma1 < preltEma2 AndAlso ltema1 > ltema2 Then
                        ElseIf ltema1 > ltema2 AndAlso htema1 > htema2 Then
                            Dim quantity As Integer = GetQuantityToTrade()
                            'If quantity = 0 AndAlso (Me.TradableInstrument.RawInstrumentName = "BANKNIFTY" OrElse Me.TradableInstrument.RawInstrumentName = "NIFTY") Then
                            '    quantity = -1
                            'End If
                            If quantity < 0 Then
                                Dim price As Decimal = currentTick.LastPrice + ConvertFloorCeling(currentTick.LastPrice * 0.3 / 100, Me.TradableInstrument.TickSize, RoundOfType.Celing)
                                parameters = New PlaceOrderParameters(ltRunningCandlePayload) With
                                       {.EntryDirection = IOrder.TypeOfTransaction.Buy,
                                        .Price = price,
                                        .Quantity = Math.Abs(quantity),
                                        .Supporting = New List(Of Object) From {"Exit"}}
                            End If
                        End If
                    End If
                End If
            End If
        Else
            Dim currentTick As ITick = Me.TradableInstrument.LastTick
            While currentTick Is Nothing
                currentTick = Me.TradableInstrument.LastTick
                Await Task.Delay(1000).ConfigureAwait(False)
            End While
            Dim runningCandlePayload As OHLCPayload = New OHLCPayload(OHLCPayload.PayloadSource.CalculatedTick)
            runningCandlePayload.OpenPrice.Value = currentTick.LastPrice
            runningCandlePayload.LowPrice.Value = currentTick.LastPrice
            runningCandlePayload.HighPrice.Value = currentTick.LastPrice
            runningCandlePayload.ClosePrice.Value = currentTick.LastPrice
            runningCandlePayload.SnapshotDateTime = Now

            Dim quantity As Integer = GetQuantityToTrade()
            If quantity <> 0 Then
                Dim price As Decimal = Decimal.MinValue
                If _Direction = IOrder.TypeOfTransaction.Buy AndAlso quantity < 0 Then
                    price = currentTick.LastPrice + ConvertFloorCeling(Math.Max(currentTick.LastPrice * 0.9 / 100, 0.2), Me.TradableInstrument.TickSize, RoundOfType.Celing)
                    If price < Me.TradableInstrument.TickSize Then price = Me.TradableInstrument.TickSize
                    parameters = New PlaceOrderParameters(runningCandlePayload) With
                                       {.EntryDirection = _Direction,
                                        .Price = price,
                                        .Quantity = quantity}
                ElseIf _Direction = IOrder.TypeOfTransaction.Sell AndAlso quantity > 0 Then
                    price = currentTick.LastPrice - ConvertFloorCeling(Math.Max(currentTick.LastPrice * 0.9 / 100, 0.2), Me.TradableInstrument.TickSize, RoundOfType.Celing)
                    If price < Me.TradableInstrument.TickSize Then price = Me.TradableInstrument.TickSize
                    parameters = New PlaceOrderParameters(runningCandlePayload) With
                                       {.EntryDirection = _Direction,
                                        .Price = price,
                                        .Quantity = quantity}
                End If
            Else
                quantity = Me.TradableInstrument.LotSize * userSettings.InstrumentsData(Me.TradableInstrument.RawInstrumentName).InitialQuantity
                Dim price As Decimal = Decimal.MinValue
                If _Direction = IOrder.TypeOfTransaction.Buy Then
                    price = currentTick.LastPrice + ConvertFloorCeling(Math.Max(currentTick.LastPrice * 0.9 / 100, 0.2), Me.TradableInstrument.TickSize, RoundOfType.Celing)
                ElseIf _Direction = IOrder.TypeOfTransaction.Sell Then
                    price = currentTick.LastPrice - ConvertFloorCeling(Math.Max(currentTick.LastPrice * 0.9 / 100, 0.2), Me.TradableInstrument.TickSize, RoundOfType.Celing)
                End If
                parameters = New PlaceOrderParameters(runningCandlePayload) With
                                       {.EntryDirection = _Direction,
                                        .Price = price,
                                        .Quantity = quantity}
            End If
        End If

        'Below portion have to be done in every place order trigger
        If parameters IsNot Nothing Then
            Dim remarks As String = "Entry Signal"
            If parameters.Supporting IsNot Nothing AndAlso parameters.Supporting.Count > 0 Then
                remarks = "Exit Signal"
            End If
            Dim currentSignalActivities As IEnumerable(Of ActivityDashboard) = Me.ParentStrategy.SignalManager.GetSignalActivities(parameters.SignalCandle.SnapshotDateTime, Me.TradableInstrument.InstrumentIdentifier)
            If currentSignalActivities IsNot Nothing AndAlso currentSignalActivities.Count > 0 Then
                Dim placedActivities As IEnumerable(Of ActivityDashboard) = currentSignalActivities.Where(Function(x)
                                                                                                              Return x.EntryActivity.RequestRemarks.ToUpper = remarks.ToUpper
                                                                                                          End Function)
                If placedActivities IsNot Nothing AndAlso placedActivities.Count > 0 Then
                    Dim lastPlacedActivity As ActivityDashboard = placedActivities.OrderBy(Function(x)
                                                                                               Return x.EntryActivity.RequestTime
                                                                                           End Function).LastOrDefault

                    If lastPlacedActivity.EntryActivity.RequestStatus = ActivityDashboard.SignalStatusType.Discarded AndAlso
                        lastPlacedActivity.EntryActivity.LastException IsNot Nothing AndAlso
                        lastPlacedActivity.EntryActivity.LastException.Message.ToUpper.Contains("TIME") Then
                        Await Task.Delay(2000).ConfigureAwait(False)
                        If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                        ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.WaitAndTake, parameters, remarks))
                    ElseIf lastPlacedActivity.EntryActivity.RequestStatus = ActivityDashboard.SignalStatusType.Discarded Then
                        If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                        ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.Take, parameters, remarks))
                        'ElseIf currentSignalActivities.FirstOrDefault.EntryActivity.RequestStatus = ActivityDashboard.SignalStatusType.Rejected Then
                        '    ret = New Tuple(Of ExecuteCommandAction, PlaceOrderParameters)(ExecuteCommandAction.Take, parameters)
                    Else
                        If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                        ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.DonotTake, Nothing, remarks))
                    End If
                Else
                    If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                    ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.Take, parameters, remarks))
                End If
            Else
                If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.Take, parameters, remarks))
            End If
        End If
        Return ret
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

    Public Function GetQuantityToTrade() As Integer
        Dim ret As Integer = 0
        If PositionDetails IsNot Nothing Then
            ret = Me.PositionDetails.Quantity
        End If
        Return ret
    End Function

    Private Function IsMyAnotherContractAvailable() As Tuple(Of Boolean, NFOStrategyInstrument)
        Dim ret As Tuple(Of Boolean, NFOStrategyInstrument) = New Tuple(Of Boolean, NFOStrategyInstrument)(False, Nothing)
        For Each runningStrategyInstrument As NFOStrategyInstrument In Me.ParentStrategy.TradableStrategyInstruments
            If runningStrategyInstrument.TradableInstrument.InstrumentIdentifier <> Me.TradableInstrument.InstrumentIdentifier AndAlso
                runningStrategyInstrument.TradableInstrument.RawInstrumentName = Me.TradableInstrument.RawInstrumentName AndAlso
                runningStrategyInstrument.TradableInstrument.InstrumentType <> IInstrument.TypeOfInstrument.Options Then
                ret = New Tuple(Of Boolean, NFOStrategyInstrument)(True, runningStrategyInstrument)
                Exit For
            End If
        Next
        Return ret
    End Function

    Public Async Function ContractRolloverAsync() As Task
        If Me.TradableInstrument.Expiry.Value.Date.AddDays(-2) = Now.Date Then
            Try
                While True
                    If Me.ParentStrategy.ParentController.OrphanException IsNot Nothing Then
                        Throw Me.ParentStrategy.ParentController.OrphanException
                    End If
                    _cts.Token.ThrowIfCancellationRequested()

                    If Now >= Me.TradableInstrument.ExchangeDetails.ContractRolloverTime AndAlso
                        IsMyAnotherContractAvailable.Item1 Then
                        Me.ForceExitForContractRollover = True
                        While Me.ForceExitForContractRollover
                            Await Task.Delay(1000, _cts.Token).ConfigureAwait(False)
                        End While
                        Me.ForceExitForContractRolloverDone = True
                        If Me.TradableInstrument.RawInstrumentName = "BANKNIFTY" OrElse Me.TradableInstrument.RawInstrumentName = "NIFTY" Then
                            IsMyAnotherContractAvailable.Item2.ForceEntryForContractRollover = False
                            IsMyAnotherContractAvailable.Item2.ForceEntryForContractRolloverDone = True
                        Else
                            IsMyAnotherContractAvailable.Item2.ForceEntryForContractRollover = True
                            IsMyAnotherContractAvailable.Item2.ForceEntryForContractRolloverDone = False
                        End If
                        Exit While
                    End If

                    Await Task.Delay(60000, _cts.Token).ConfigureAwait(False)
                End While
            Catch ex As Exception
                logger.Error("Strategy Instrument:{0}, error:{1}", Me.ToString, ex.ToString)
                Throw ex
            End Try
        End If
    End Function

    Public Async Function ExpiryExitEntryAsync() As Task
        If Me.TradableInstrument.RawInstrumentName = "BANKNIFTY" OrElse Me.TradableInstrument.RawInstrumentName = "NIFTY" Then
            Try
                Dim userInputs As NFOUserInputs = Me.ParentStrategy.UserSettings
                While True
                    If Me.ParentStrategy.ParentController.OrphanException IsNot Nothing Then
                        Throw Me.ParentStrategy.ParentController.OrphanException
                    End If
                    _cts.Token.ThrowIfCancellationRequested()

                    If Now >= Me.TradableInstrument.ExchangeDetails.ContractRolloverTime Then
                        If Me.DependentOptionStrategyInstruments IsNot Nothing AndAlso
                            Me.DependentOptionStrategyInstruments.Count > 0 Then
                            Dim processOption As Boolean = False
                            For Each runningInstrument In Me.DependentOptionStrategyInstruments
                                If runningInstrument.TradableInstrument.Expiry.Value <> Me.TradableInstrument.Expiry.Value Then
                                    If runningInstrument.TradableInstrument.Expiry.Value.Date = Now.Date Then
                                        If runningInstrument.GetQuantityToTrade() > 0 Then
                                            Try
                                                logger.Debug("PlaceOrder-> ************************************************ {0}", runningInstrument.TradableInstrument.TradingSymbol)
                                                logger.Debug("PlaceOrder-> Parameters: Force Exit for weekly contract rollover:{0}, TradingSymbol:{1}", True, runningInstrument.TradableInstrument.TradingSymbol)
                                            Catch ex As Exception
                                                logger.Error(ex)
                                            End Try
                                            Await runningInstrument.MonitorAsync(command:=ExecuteCommands.PlaceRegularLimitCNCOrder, data:="SELL").ConfigureAwait(False)
                                            If runningInstrument._Direction = IOrder.TypeOfTransaction.None Then
                                                processOption = True
                                            End If
                                        End If
                                    End If
                                End If
                            Next
                            If processOption Then
                                Dim optionInstrument As IInstrument = Nothing
                                Dim minExpiry As Date = CType(Me.ParentStrategy, NFOStrategy).DependentInstruments.Min(Function(x)
                                                                                                                           If x.RawInstrumentName = Me.TradableInstrument.RawInstrumentName Then
                                                                                                                               Dim expiry As Date = x.Expiry.Value
                                                                                                                               If x.Expiry.Value = Me.TradableInstrument.Expiry.Value Then
                                                                                                                                   expiry = x.Expiry.Value.AddDays(-2)
                                                                                                                               End If
                                                                                                                               If Now.Date < expiry.Date Then
                                                                                                                                   Return x.Expiry.Value
                                                                                                                               ElseIf Now.Date = expiry.Date Then
                                                                                                                                   If Now < Me.TradableInstrument.ExchangeDetails.ContractRolloverTime Then
                                                                                                                                       Return x.Expiry.Value
                                                                                                                                   Else
                                                                                                                                       Return Date.MaxValue
                                                                                                                                   End If
                                                                                                                               Else
                                                                                                                                   Return Date.MaxValue
                                                                                                                               End If
                                                                                                                           Else
                                                                                                                               Return Date.MaxValue
                                                                                                                           End If
                                                                                                                       End Function)

                                If Me.GetQuantityToTrade() > 0 Then
                                    Dim stockPrice As Decimal = Me.TradableInstrument.LastTick.LastPrice
                                    Dim otmPutStrike As Decimal = stockPrice - (stockPrice * userInputs.StrikePriceRangePercentage / 100)

                                    optionInstrument = CType(Me.ParentStrategy, NFOStrategy).DependentInstruments.Where(Function(x)
                                                                                                                            Return x.Strike <= otmPutStrike AndAlso x.RawInstrumentType = "PE" AndAlso
                                                                                                                                  x.Expiry.Value.Date = minExpiry.Date AndAlso x.RawInstrumentName = Me.TradableInstrument.RawInstrumentName
                                                                                                                        End Function).OrderBy(Function(y)
                                                                                                                                                  Return y.Strike
                                                                                                                                              End Function).LastOrDefault
                                ElseIf Me.GetQuantityToTrade < 0 Then
                                    Dim stockPrice As Decimal = Me.TradableInstrument.LastTick.LastPrice
                                    Dim otmCallStrike As Decimal = stockPrice + (stockPrice * userInputs.StrikePriceRangePercentage / 100)

                                    optionInstrument = CType(Me.ParentStrategy, NFOStrategy).DependentInstruments.Where(Function(x)
                                                                                                                            Return x.Strike >= otmCallStrike AndAlso x.RawInstrumentType = "CE" AndAlso
                                                                                                                                  x.Expiry.Value.Date = minExpiry.Date AndAlso x.RawInstrumentName = Me.TradableInstrument.RawInstrumentName
                                                                                                                        End Function).OrderBy(Function(y)
                                                                                                                                                  Return y.Strike
                                                                                                                                              End Function).FirstOrDefault
                                End If
                                If optionInstrument IsNot Nothing Then
                                    Dim otmStrategyInstrument As NFOStrategyInstrument = Nothing
                                    If Me.DependentOptionStrategyInstruments IsNot Nothing AndAlso Me.DependentOptionStrategyInstruments.Count > 0 Then
                                        otmStrategyInstrument = DependentOptionStrategyInstruments.ToList.Find(Function(x)
                                                                                                                   Return x.TradableInstrument.TradingSymbol = optionInstrument.TradingSymbol
                                                                                                               End Function)
                                    End If
                                    If otmStrategyInstrument Is Nothing Then
                                        Await CreateStrategyInstrumentAndPopulate(New List(Of IInstrument) From {optionInstrument}).ConfigureAwait(False)
                                        otmStrategyInstrument = DependentOptionStrategyInstruments.ToList.Find(Function(x)
                                                                                                                   Return x.TradableInstrument.TradingSymbol = optionInstrument.TradingSymbol
                                                                                                               End Function)
                                    End If
                                    If otmStrategyInstrument IsNot Nothing Then
                                        Try
                                            logger.Debug("PlaceOrder-> ************************************************ {0}", otmStrategyInstrument.TradableInstrument.TradingSymbol)
                                            logger.Debug("PlaceOrder-> Parameters: Force Entry for weekly contract rollover:{0}, TradingSymbol:{1}", True, otmStrategyInstrument.TradableInstrument.TradingSymbol)
                                        Catch ex As Exception
                                            logger.Error(ex)
                                        End Try
                                        Await otmStrategyInstrument.MonitorAsync(command:=ExecuteCommands.PlaceRegularLimitCNCOrder, data:="BUY").ConfigureAwait(False)
                                    End If
                                End If
                            End If
                        End If
                        Exit While
                    End If

                    Await Task.Delay(60000, _cts.Token).ConfigureAwait(False)
                End While
            Catch ex As Exception
                logger.Error("Strategy Instrument:{0}, error:{1}", Me.ToString, ex.ToString)
                Throw ex
            End Try
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