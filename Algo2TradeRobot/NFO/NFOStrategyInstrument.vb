Imports NLog
Imports System.IO
Imports System.Threading
Imports Utilities.Numbers
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


    Private _DependentCEOptionStrategyInstruments As List(Of NFOStrategyInstrument) = Nothing
    Private _DependentPEOptionStrategyInstruments As List(Of NFOStrategyInstrument) = Nothing

    Private ReadOnly _ParentInstrument As Boolean
    Private ReadOnly _ITMOptionsFilename As String
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
        _ITMOptionsFilename = Path.Combine(My.Application.Info.DirectoryPath, String.Format("{0} {1}.Options.a2t", Me.TradableInstrument.TradingSymbol, Now.ToString("yy_MM_dd")))
    End Sub

    Public Overrides Function PopulateChartAndIndicatorsAsync(candleCreator As Chart, currentCandle As OHLCPayload) As Task
        Throw New NotImplementedException
    End Function

    Private Async Function CreateStrategyInstrumentAndPopulateAsync(ByVal instrumentList As List(Of IInstrument)) As Task
        Dim instrumentsToSubscribe As List(Of IInstrument) = Nothing
        If instrumentList IsNot Nothing AndAlso instrumentList.Count > 0 Then
            For Each runningInstrument In instrumentList
                Dim subscribedStrategyInstrument As StrategyInstrument =
                    Me.ParentStrategy.TradableStrategyInstruments.ToList.Find(Function(x)
                                                                                  Return x.TradableInstrument.InstrumentIdentifier = runningInstrument.InstrumentIdentifier
                                                                              End Function)
                If subscribedStrategyInstrument Is Nothing Then
                    If instrumentsToSubscribe Is Nothing Then instrumentsToSubscribe = New List(Of IInstrument)
                    instrumentsToSubscribe.Add(runningInstrument)
                End If
            Next
        End If
        If instrumentsToSubscribe IsNot Nothing AndAlso instrumentsToSubscribe.Count > 0 Then
            Await CType(Me.ParentStrategy, NFOStrategy).CreateDependentTradableStrategyInstrumentsAsync(instrumentsToSubscribe).ConfigureAwait(False)
            Dim allOptions As List(Of String) = Nothing
            If File.Exists(_ITMOptionsFilename) Then
                allOptions = Utilities.Strings.DeserializeToCollection(Of List(Of String))(_ITMOptionsFilename)
            End If
            For Each runningInstrument In instrumentsToSubscribe
                Dim subscribedStrategyInstrument As StrategyInstrument =
                    Me.ParentStrategy.TradableStrategyInstruments.ToList.Find(Function(x)
                                                                                  Return x.TradableInstrument.InstrumentIdentifier = runningInstrument.InstrumentIdentifier
                                                                              End Function)
                If subscribedStrategyInstrument IsNot Nothing Then
                    If subscribedStrategyInstrument.TradableInstrument.RawInstrumentType = "CE" Then
                        If _DependentCEOptionStrategyInstruments Is Nothing Then _DependentCEOptionStrategyInstruments = New List(Of NFOStrategyInstrument)
                        _DependentCEOptionStrategyInstruments.Add(subscribedStrategyInstrument)
                    ElseIf subscribedStrategyInstrument.TradableInstrument.RawInstrumentType = "PE" Then
                        If _DependentPEOptionStrategyInstruments Is Nothing Then _DependentPEOptionStrategyInstruments = New List(Of NFOStrategyInstrument)
                        _DependentPEOptionStrategyInstruments.Add(subscribedStrategyInstrument)
                    End If
                    If allOptions Is Nothing Then allOptions = New List(Of String)
                    allOptions.Add(subscribedStrategyInstrument.TradableInstrument.InstrumentIdentifier)
                End If
                Utilities.Strings.SerializeFromCollection(Of List(Of String))(_ITMOptionsFilename, allOptions)
            Next
        End If
        Await Task.Delay(1000).ConfigureAwait(False)
    End Function

    Private Async Function SubscribeOptionInstrumentsFromA2TAsync() As Task
        If File.Exists(_ITMOptionsFilename) Then
            Dim allOptions As List(Of String) = Utilities.Strings.DeserializeToCollection(Of List(Of String))(_ITMOptionsFilename)
            If allOptions IsNot Nothing AndAlso allOptions.Count > 0 Then
                Dim instrumentsToSubscribe As List(Of IInstrument) = Nothing
                Dim allInstruments As IEnumerable(Of IInstrument) = CType(Me.ParentStrategy, NFOStrategy).DependentInstruments
                For Each runningOption In allOptions
                    Dim instrument As IInstrument = allInstruments.ToList.Find(Function(x)
                                                                                   Return x.InstrumentIdentifier = runningOption
                                                                               End Function)
                    If instrument IsNot Nothing Then
                        If instrumentsToSubscribe Is Nothing Then instrumentsToSubscribe = New List(Of IInstrument)
                        instrumentsToSubscribe.Add(instrument)
                    End If
                Next

                Await CreateStrategyInstrumentAndPopulateAsync(instrumentsToSubscribe).ConfigureAwait(False)
                Await Task.Delay(5000).ConfigureAwait(False)
            End If
        End If
    End Function

    Public Overrides Async Function MonitorAsync() As Task
        Try
            _strategyInstrumentRunning = True
            If _ParentInstrument Then
                Await SubscribeOptionInstrumentsFromA2TAsync().ConfigureAwait(False)
                Dim instrumentName As String = Me.TradableInstrument.TradingSymbol
                If Me.TradableInstrument.TradingSymbol = "NIFTY BANK" Then
                    instrumentName = "BANKNIFTY"
                ElseIf Me.TradableInstrument.TradingSymbol = "NIFTY 50" Then
                    instrumentName = "NIFTY"
                End If
                If instrumentName IsNot Nothing AndAlso
                    CType(Me.ParentStrategy.UserSettings, NFOUserInputs).InstrumentsData.ContainsKey(instrumentName) Then
                    Dim instrumentDetails As NFOUserInputs.InstrumentDetails = CType(Me.ParentStrategy.UserSettings, NFOUserInputs).InstrumentsData(instrumentName)
                    While True
                        If Me.ParentStrategy.ParentController.OrphanException IsNot Nothing Then
                            Throw Me.ParentStrategy.ParentController.OrphanException
                        End If
                        _cts.Token.ThrowIfCancellationRequested()
                        If Me.TradableInstrument.LastTick IsNot Nothing AndAlso Me.TradableInstrument.LastTick.Timestamp IsNot Nothing AndAlso
                            Me.TradableInstrument.LastTick.Timestamp.Value >= Me.ParentStrategy.UserSettings.TradeStartTime Then
                            Dim ltp As Decimal = Me.TradableInstrument.LastTick.LastPrice
                            Dim itmCall As IInstrument = GetITMCall(ltp)
                            Dim itmPut As IInstrument = GetITMPut(ltp)
                            If itmCall IsNot Nothing AndAlso itmPut IsNot Nothing Then
                                Dim ceStrategyInstrument As NFOStrategyInstrument = Nothing
                                If _DependentCEOptionStrategyInstruments IsNot Nothing AndAlso _DependentCEOptionStrategyInstruments.Count > 0 Then
                                    For Each runningStrategyInstrument As NFOStrategyInstrument In _DependentCEOptionStrategyInstruments
                                        If runningStrategyInstrument.IsRunningInstrument OrElse runningStrategyInstrument.IsOpenInstrument Then
                                            ceStrategyInstrument = runningStrategyInstrument
                                            Exit For
                                        End If
                                    Next
                                End If
                                If ceStrategyInstrument Is Nothing Then
                                    Await CreateStrategyInstrumentAndPopulateAsync(New List(Of IInstrument) From {itmCall}).ConfigureAwait(False)
                                    ceStrategyInstrument = _DependentCEOptionStrategyInstruments.Find(Function(x)
                                                                                                          Return x.TradableInstrument.InstrumentIdentifier = itmCall.InstrumentIdentifier
                                                                                                      End Function)
                                    OnHeartbeat(String.Format("{0} subscribed. LTP:{0}", ceStrategyInstrument.TradableInstrument.TradingSymbol, ltp))
                                End If
                                If ceStrategyInstrument IsNot Nothing Then
                                    Await ceStrategyInstrument.MonitorAsync(command:=ExecuteCommands.PlaceRegularMarketMISOrder, data:=Nothing).ConfigureAwait(False)
                                    Await ceStrategyInstrument.MonitorAsync(command:=ExecuteCommands.ModifyStoplossOrder, data:=Nothing).ConfigureAwait(False)
                                    If Me.StrategyExitAllTriggerd Then
                                        Await ceStrategyInstrument.MonitorAsync(command:=ExecuteCommands.CancelRegularOrder, data:=Nothing).ConfigureAwait(False)
                                    ElseIf ceStrategyInstrument.TradableInstrument.Strike <> itmCall.Strike AndAlso
                                        ceStrategyInstrument.IsRunningInstrument AndAlso ceStrategyInstrument.IsOpenInstrument Then
                                        OnHeartbeat("ITM Call Strike changed. So it will cancel existing CE order.")
                                        Await ceStrategyInstrument.MonitorAsync(command:=ExecuteCommands.CancelRegularOrder, data:=Nothing).ConfigureAwait(False)
                                    End If
                                End If

                                Dim peStrategyInstrument As NFOStrategyInstrument = Nothing
                                If _DependentPEOptionStrategyInstruments IsNot Nothing AndAlso _DependentPEOptionStrategyInstruments.Count > 0 Then
                                    For Each runningStrategyInstrument As NFOStrategyInstrument In _DependentPEOptionStrategyInstruments
                                        If runningStrategyInstrument.IsRunningInstrument OrElse runningStrategyInstrument.IsOpenInstrument Then
                                            peStrategyInstrument = runningStrategyInstrument
                                            Exit For
                                        End If
                                    Next
                                End If
                                If peStrategyInstrument Is Nothing Then
                                    Await CreateStrategyInstrumentAndPopulateAsync(New List(Of IInstrument) From {itmPut}).ConfigureAwait(False)
                                    peStrategyInstrument = _DependentPEOptionStrategyInstruments.Find(Function(x)
                                                                                                          Return x.TradableInstrument.InstrumentIdentifier = itmPut.InstrumentIdentifier
                                                                                                      End Function)
                                    OnHeartbeat(String.Format("{0} subscribed. LTP:{0}", ceStrategyInstrument.TradableInstrument.TradingSymbol, ltp))
                                End If
                                If peStrategyInstrument IsNot Nothing Then
                                    Await peStrategyInstrument.MonitorAsync(command:=ExecuteCommands.PlaceRegularMarketMISOrder, data:=Nothing).ConfigureAwait(False)
                                    Await peStrategyInstrument.MonitorAsync(command:=ExecuteCommands.ModifyStoplossOrder, data:=Nothing).ConfigureAwait(False)
                                    If Me.StrategyExitAllTriggerd Then
                                        Await peStrategyInstrument.MonitorAsync(command:=ExecuteCommands.CancelRegularOrder, data:=Nothing).ConfigureAwait(False)
                                    ElseIf peStrategyInstrument.TradableInstrument.Strike <> itmPut.Strike AndAlso
                                        peStrategyInstrument.IsRunningInstrument AndAlso peStrategyInstrument.IsOpenInstrument Then
                                        OnHeartbeat("ITM Put Strike changed. So it will cancel existing PE order.")
                                        Await peStrategyInstrument.MonitorAsync(command:=ExecuteCommands.CancelRegularOrder, data:=Nothing).ConfigureAwait(False)
                                    End If
                                End If
                            End If
                        End If
                        Await Task.Delay(1000, _cts.Token).ConfigureAwait(False)
                    End While
                End If
            End If
        Catch ex As Exception
            'To log exceptions getting created from this function as the bubble up of the exception
            'will anyways happen to Strategy.MonitorAsync but it will not be shown until all tasks exit
            logger.Error("Strategy Instrument:{0}, error:{1}", Me.ToString, ex.ToString)
            Throw ex
        Finally
            If _ParentInstrument Then OnHeartbeat(String.Format("Strategy Instrument Stopped: {0}", Me.ToString))
            _strategyInstrumentRunning = False
        End Try
    End Function

    Public Overrides Async Function MonitorAsync(command As ExecuteCommands, data As Object) As Task
        If Not _ParentInstrument Then
            If command = ExecuteCommands.PlaceRegularMarketMISOrder Then
                Dim placeOrderTriggers As List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)) = Await IsTriggerReceivedForPlaceOrderAsync(False).ConfigureAwait(False)
                If placeOrderTriggers IsNot Nothing AndAlso placeOrderTriggers.Count > 0 AndAlso
                    placeOrderTriggers.FirstOrDefault.Item1 = ExecuteCommandAction.Take Then
                    Dim orderResponse = Nothing
                    If placeOrderTriggers.FirstOrDefault.Item2.OrderType = IOrder.TypeOfOrder.SL_M Then
                        orderResponse = Await ExecuteCommandAsync(ExecuteCommands.PlaceRegularSLMMISOrder, Nothing).ConfigureAwait(False)
                    ElseIf placeOrderTriggers.FirstOrDefault.Item2.OrderType = IOrder.TypeOfOrder.Market Then
                        orderResponse = Await ExecuteCommandAsync(ExecuteCommands.PlaceRegularMarketMISOrder, Nothing).ConfigureAwait(False)
                    End If
                End If
            ElseIf command = ExecuteCommands.ModifyStoplossOrder Then
                Dim modifyStoplossOrderTrigger As List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)) = Await IsTriggerReceivedForModifyStoplossOrderAsync(False).ConfigureAwait(False)
                If modifyStoplossOrderTrigger IsNot Nothing AndAlso modifyStoplossOrderTrigger.Count > 0 Then
                    Await ExecuteCommandAsync(ExecuteCommands.ModifyStoplossOrder, Nothing).ConfigureAwait(False)
                End If
            ElseIf command = ExecuteCommands.CancelRegularOrder Then
                Dim cancelOrderTrigger As List(Of Tuple(Of ExecuteCommandAction, IOrder, String)) = Await IsTriggerReceivedForExitOrderAsync(False).ConfigureAwait(False)
                If cancelOrderTrigger IsNot Nothing AndAlso cancelOrderTrigger.Count > 0 Then
                    Dim orderResponse = Await ExecuteCommandAsync(ExecuteCommands.CancelRegularOrder, Nothing).ConfigureAwait(False)
                End If
            End If
        End If
    End Function

    Protected Overrides Async Function IsTriggerReceivedForPlaceOrderAsync(forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)))
        Dim ret As List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
        Dim instrumentDetails As NFOUserInputs.InstrumentDetails = userSettings.InstrumentsData(Me.TradableInstrument.Name)
        Dim currentTick As ITick = Me.TradableInstrument.LastTick
        Dim currentTime As Date = Now()

        Dim parameters As PlaceOrderParameters = Nothing
        If currentTick IsNot Nothing AndAlso currentTime >= userSettings.TradeStartTime AndAlso Me.ParentStrategy.IsFirstTimeInformationCollected AndAlso currentTime <= Me.TradableInstrument.ExchangeDetails.ExchangeEndTime Then
            Dim runningCandle As OHLCPayload = New OHLCPayload(OHLCPayload.PayloadSource.Tick)
            runningCandle.OpenPrice.Value = currentTick.LastPrice
            runningCandle.LowPrice.Value = currentTick.LastPrice
            runningCandle.HighPrice.Value = currentTick.LastPrice
            runningCandle.ClosePrice.Value = currentTick.LastPrice
            runningCandle.SnapshotDateTime = currentTick.Timestamp.Value
            runningCandle.TradingSymbol = Me.TradableInstrument.TradingSymbol

            Dim quantity As Integer = Me.TradableInstrument.LotSize * instrumentDetails.NumberOfLots

            If Not Me.StrategyExitAllTriggerd Then
                If Not IsRunningInstrument() AndAlso Not IsOpenInstrument() Then
                    If currentTime <= userSettings.LastTradeEntryTime Then
                        Dim triggerPrice As Double = ConvertFloorCeling(currentTick.LastPrice + instrumentDetails.EntryBuffer, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                        If forcePrint Then OnHeartbeat(String.Format("Place Entry Order. Trigger Price[LTP({0}+Buffer({1}))]={2}", currentTick.LastPrice, instrumentDetails.EntryBuffer, triggerPrice))
                        parameters = New PlaceOrderParameters(runningCandle) With
                                            {.EntryDirection = IOrder.TypeOfTransaction.Buy,
                                             .Quantity = quantity,
                                             .TriggerPrice = triggerPrice,
                                             .OrderType = IOrder.TypeOfOrder.SL_M}
                    End If
                ElseIf IsRunningInstrument() AndAlso Not IsOpenInstrument() Then
                    If Me.OrderDetails IsNot Nothing AndAlso Me.OrderDetails.Count > 0 Then
                        Dim entryOrder As IOrder = Me.OrderDetails.Where(Function(x)
                                                                             Return x.Value.ParentOrder.Status = IOrder.TypeOfStatus.Complete AndAlso
                                                                             x.Value.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Buy
                                                                         End Function).OrderByDescending(Function(y)
                                                                                                             Return y.Value.ParentOrder.TimeStamp
                                                                                                         End Function).FirstOrDefault.Value.ParentOrder

                        If entryOrder IsNot Nothing Then
                            Dim triggerPrice As Double = ConvertFloorCeling(entryOrder.AveragePrice - instrumentDetails.InitialStoploss, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                            If forcePrint Then OnHeartbeat(String.Format("Place Stoploss Order. Trigger Price[EntryPrice({0}-InitialStoploss({1}))]={2}", entryOrder.AveragePrice, instrumentDetails.InitialStoploss, triggerPrice))
                            If currentTick.LastPrice > triggerPrice Then
                                parameters = New PlaceOrderParameters(runningCandle) With
                                                {.EntryDirection = IOrder.TypeOfTransaction.Sell,
                                                 .Quantity = quantity,
                                                 .TriggerPrice = triggerPrice,
                                                 .OrderType = IOrder.TypeOfOrder.SL_M}
                            Else
                                parameters = New PlaceOrderParameters(runningCandle) With
                                                {.EntryDirection = IOrder.TypeOfTransaction.Sell,
                                                 .Quantity = quantity,
                                                 .OrderType = IOrder.TypeOfOrder.Market}
                            End If
                        End If
                    End If
                End If
            Else
                If IsRunningInstrument() AndAlso Not IsOpenInstrument() Then
                    parameters = New PlaceOrderParameters(runningCandle) With
                                {.EntryDirection = IOrder.TypeOfTransaction.Sell,
                                 .Quantity = quantity,
                                 .OrderType = IOrder.TypeOfOrder.Market,
                                 .Supporting = New List(Of Object) From {"Strategy Force Exit"}}
                End If
            End If
        End If

        'Below portion have to be done in every place order trigger
        If parameters IsNot Nothing Then
            Try
                If forcePrint Then
                    logger.Debug("PlaceOrder-> ************************************************ {0}", Me.TradableInstrument.TradingSymbol)
                    logger.Debug("PlaceOrder Parameters-> {0}, {1}, {2}",
                                 parameters.ToString,
                                 If(parameters.Supporting IsNot Nothing AndAlso parameters.Supporting.Count > 0, parameters.Supporting.FirstOrDefault.ToString, ""),
                                 Me.TradableInstrument.TradingSymbol)
                End If
            Catch ex As Exception
                logger.Warn(ex)
            End Try

            Dim currentSignalActivities As IEnumerable(Of ActivityDashboard) = Me.ParentStrategy.SignalManager.GetActiveSignalActivities(Me.TradableInstrument.InstrumentIdentifier)
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
                            If lastPlacedActivity.CancelActivity.RequestStatus = ActivityDashboard.SignalStatusType.Activated Then
                                If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                                ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.Take, parameters, parameters.ToString))
                            Else
                                If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String))
                                ret.Add(New Tuple(Of ExecuteCommandAction, PlaceOrderParameters, String)(ExecuteCommandAction.DonotTake, parameters, parameters.ToString))
                                Try
                                    logger.Debug("Will not take this trade as similar previous trade detected. Current Trade->{0}, Last Activity Direction:{1}, Last Activity Status:{2}, Trading Symbol:{3}",
                                         parameters.ToString, lastPlacedActivity.SignalDirection, lastPlacedActivity.EntryActivity.RequestStatus, Me.TradableInstrument.TradingSymbol)
                                Catch ex As Exception
                                    logger.Warn(ex.ToString)
                                End Try
                            End If
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

    Protected Overrides Async Function IsTriggerReceivedForModifyStoplossOrderAsync(forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)))
        Dim ret As List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        Dim userSettings As NFOUserInputs = Me.ParentStrategy.UserSettings
        Dim instrumentDetails As NFOUserInputs.InstrumentDetails = userSettings.InstrumentsData(Me.TradableInstrument.Name)
        Dim currentTick As ITick = Me.TradableInstrument.LastTick
        If currentTick IsNot Nothing AndAlso Me.OrderDetails IsNot Nothing AndAlso Me.OrderDetails.Count > 0 Then
            If IsOpenInstrument() AndAlso Not IsRunningInstrument() Then
                Dim entryOrder As IOrder = Me.OrderDetails.Where(Function(x)
                                                                     Return x.Value.ParentOrder.Status = IOrder.TypeOfStatus.Complete AndAlso
                                                                     x.Value.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Buy
                                                                 End Function).OrderByDescending(Function(y)
                                                                                                     Return y.Value.ParentOrder.TimeStamp
                                                                                                 End Function).FirstOrDefault.Value.ParentOrder
                If entryOrder IsNot Nothing Then
                    Dim movememt As Decimal = currentTick.LastPrice - entryOrder.AveragePrice
                    Dim multiplier As Decimal = Math.Floor(movememt / instrumentDetails.TrailingStoploss)
                    If multiplier > 0 Then
                        Dim stoploss As Decimal = entryOrder.AveragePrice - instrumentDetails.InitialStoploss + multiplier * instrumentDetails.TrailingStoploss
                        For Each runningOrder In Me.OrderDetails.Values
                            If (runningOrder.ParentOrder.Status = IOrder.TypeOfStatus.TriggerPending OrElse
                                runningOrder.ParentOrder.Status = IOrder.TypeOfStatus.Open) AndAlso
                                runningOrder.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Sell Then
                                If runningOrder.ParentOrder.TriggerPrice < stoploss Then
                                    'Below portion have to be done in every modify stoploss order trigger
                                    Dim currentSignalActivities As ActivityDashboard = Me.ParentStrategy.SignalManager.GetSignalActivities(runningOrder.ParentOrder.Tag)
                                    If currentSignalActivities IsNot Nothing Then
                                        If currentSignalActivities.StoplossModifyActivity.RequestStatus = ActivityDashboard.SignalStatusType.Handled OrElse
                                        currentSignalActivities.StoplossModifyActivity.RequestStatus = ActivityDashboard.SignalStatusType.Activated OrElse
                                        currentSignalActivities.StoplossModifyActivity.RequestStatus = ActivityDashboard.SignalStatusType.Completed Then
                                            If Val(currentSignalActivities.StoplossModifyActivity.Supporting) = stoploss Then
                                                Continue For
                                            End If
                                        End If
                                    End If
                                    If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String))
                                    ret.Add(New Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)(ExecuteCommandAction.Take, runningOrder.ParentOrder, stoploss, String.Format("Trailing {0}", multiplier)))
                                End If
                            End If
                        Next
                    End If
                End If
            ElseIf IsOpenInstrument() AndAlso IsRunningInstrument() Then
                For Each runningOrder In Me.OrderDetails.Values
                    If runningOrder.ParentOrder.Status = IOrder.TypeOfStatus.TriggerPending OrElse
                        runningOrder.ParentOrder.Status = IOrder.TypeOfStatus.Open Then
                        Dim triggerPrice As Double = ConvertFloorCeling(currentTick.LastPrice + instrumentDetails.EntryBuffer, Me.TradableInstrument.TickSize, RoundOfType.Floor)
                        If runningOrder.ParentOrder.TriggerPrice - triggerPrice >= instrumentDetails.MinimumMovementForModification Then
                            'Below portion have to be done in every modify stoploss order trigger
                            Dim currentSignalActivities As ActivityDashboard = Me.ParentStrategy.SignalManager.GetSignalActivities(runningOrder.ParentOrder.Tag)
                            If currentSignalActivities IsNot Nothing Then
                                If currentSignalActivities.StoplossModifyActivity.RequestStatus = ActivityDashboard.SignalStatusType.Handled OrElse
                                currentSignalActivities.StoplossModifyActivity.RequestStatus = ActivityDashboard.SignalStatusType.Activated OrElse
                                currentSignalActivities.StoplossModifyActivity.RequestStatus = ActivityDashboard.SignalStatusType.Completed Then
                                    If Val(currentSignalActivities.StoplossModifyActivity.Supporting) = triggerPrice Then
                                        Continue For
                                    End If
                                End If
                            End If
                            If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String))
                            ret.Add(New Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)(ExecuteCommandAction.Take, runningOrder.ParentOrder, triggerPrice, "LTP Movement"))
                        End If
                    End If
                Next
            End If
        End If
        If forcePrint AndAlso ret IsNot Nothing AndAlso ret.Count > 0 Then
            Try
                For Each runningOrder In ret
                    OnHeartbeat(String.Format("***** Modify Order ***** Order ID:{0}, Reason:{1}, {2}", runningOrder.Item2.OrderIdentifier, runningOrder.Item3, Me.TradableInstrument.TradingSymbol))
                Next
            Catch ex As Exception
                logger.Warn(ex)
            End Try
        End If
        Return ret
    End Function

    Protected Overrides Async Function IsTriggerReceivedForExitOrderAsync(forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, String)))
        Dim ret As List(Of Tuple(Of ExecuteCommandAction, IOrder, String)) = Nothing
        Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
        If Me.OrderDetails IsNot Nothing AndAlso Me.OrderDetails.Count > 0 Then
            For Each runningOrder In Me.OrderDetails.Values
                If runningOrder.ParentOrder.Status = IOrder.TypeOfStatus.TriggerPending OrElse
                    runningOrder.ParentOrder.Status = IOrder.TypeOfStatus.Open Then
                    'Below portion have to be done in every cancel order trigger
                    Dim currentSignalActivities As ActivityDashboard = Me.ParentStrategy.SignalManager.GetSignalActivities(runningOrder.ParentOrder.Tag)
                    If currentSignalActivities IsNot Nothing Then
                        If currentSignalActivities.CancelActivity.RequestStatus = ActivityDashboard.SignalStatusType.Handled OrElse
                            currentSignalActivities.CancelActivity.RequestStatus = ActivityDashboard.SignalStatusType.Activated OrElse
                            currentSignalActivities.CancelActivity.RequestStatus = ActivityDashboard.SignalStatusType.Completed Then
                            Continue For
                        End If
                    End If
                    If ret Is Nothing Then ret = New List(Of Tuple(Of ExecuteCommandAction, IOrder, String))
                    ret.Add(New Tuple(Of ExecuteCommandAction, IOrder, String)(ExecuteCommandAction.Take, runningOrder.ParentOrder, ""))
                End If
            Next
        End If
        If forcePrint AndAlso ret IsNot Nothing AndAlso ret.Count > 0 Then
            Try
                For Each runningOrder In ret
                    OnHeartbeat(String.Format("***** Exit Order ***** Order ID:{0}, Reason:{1}, {2}", runningOrder.Item2.OrderIdentifier, runningOrder.Item3, Me.TradableInstrument.TradingSymbol))
                Next
            Catch ex As Exception
                logger.Warn(ex)
            End Try
        End If
        Return ret
    End Function

    Private Function GetITMCall(ByVal price As Decimal) As IInstrument
        Dim ret As IInstrument = Nothing
        Dim allInstruments As IEnumerable(Of IInstrument) = CType(Me.ParentStrategy, NFOStrategy).DependentInstruments
        If allInstruments IsNot Nothing AndAlso allInstruments.Count > 0 Then
            ret = allInstruments.Where(Function(x)
                                           Return x.Strike <= price AndAlso x.RawInstrumentType = "CE"
                                       End Function).OrderBy(Function(y)
                                                                 Return y.Strike
                                                             End Function).LastOrDefault
        End If
        Return ret
    End Function

    Private Function GetITMPut(ByVal price As Decimal) As IInstrument
        Dim ret As IInstrument = Nothing
        Dim allInstruments As IEnumerable(Of IInstrument) = CType(Me.ParentStrategy, NFOStrategy).DependentInstruments
        If allInstruments IsNot Nothing AndAlso allInstruments.Count > 0 Then
            ret = allInstruments.Where(Function(x)
                                           Return x.Strike > price AndAlso x.RawInstrumentType = "PE"
                                       End Function).OrderBy(Function(y)
                                                                 Return y.Strike
                                                             End Function).FirstOrDefault
        End If
        Return ret
    End Function

    Public Function GetTotalPL() As Decimal
        Dim ret As Decimal = 0
        If Me.OrderDetails IsNot Nothing AndAlso Me.OrderDetails.Count > 0 Then
            Dim currentPrice As Decimal = Me.TradableInstrument.LastTick.LastPrice
            For Each runningOrder In Me.OrderDetails.Values
                If runningOrder.ParentOrder IsNot Nothing AndAlso runningOrder.ParentOrder.Status = IOrder.TypeOfStatus.Complete Then
                    If runningOrder.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Buy Then
                        ret += (currentPrice - runningOrder.ParentOrder.AveragePrice) * runningOrder.ParentOrder.Quantity
                    ElseIf runningOrder.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Sell Then
                        ret += (runningOrder.ParentOrder.AveragePrice - currentPrice) * runningOrder.ParentOrder.Quantity
                    End If
                End If
            Next
        End If
        Return ret
    End Function

    Public Function IsRunningInstrument() As Boolean
        Dim ret As Boolean = False
        If Me.OrderDetails IsNot Nothing AndAlso Me.OrderDetails.Count > 0 Then
            Dim totalTraded As Integer = 0
            For Each runningOrder In Me.OrderDetails
                If runningOrder.Value.ParentOrder IsNot Nothing Then
                    If runningOrder.Value.ParentOrder.Status <> IOrder.TypeOfStatus.Rejected AndAlso
                        runningOrder.Value.ParentOrder.Status <> IOrder.TypeOfStatus.Cancelled Then
                        If runningOrder.Value.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Buy Then
                            totalTraded += Math.Abs(runningOrder.Value.ParentOrder.Quantity)
                        ElseIf runningOrder.Value.ParentOrder.TransactionType = IOrder.TypeOfTransaction.Sell Then
                            totalTraded -= Math.Abs(runningOrder.Value.ParentOrder.Quantity)
                        End If
                    End If
                End If
            Next

            ret = totalTraded <> 0
        End If
        Return ret
    End Function

    Public Function IsOpenInstrument() As Boolean
        Dim ret As Boolean = False
        If Me.OrderDetails IsNot Nothing AndAlso Me.OrderDetails.Count > 0 Then
            For Each runningOrder In Me.OrderDetails
                If runningOrder.Value.ParentOrder IsNot Nothing Then
                    If runningOrder.Value.ParentOrder.Status = IOrder.TypeOfStatus.Open OrElse
                        runningOrder.Value.ParentOrder.Status = IOrder.TypeOfStatus.TriggerPending Then
                        ret = True
                        Exit For
                    End If
                End If
            Next
        End If
        Return ret
    End Function

#Region "Not required functions"
    Protected Overrides Function IsTriggerReceivedForPlaceOrderAsync(forcePrint As Boolean, data As Object) As Task(Of List(Of Tuple(Of ExecuteCommandAction, StrategyInstrument, PlaceOrderParameters, String)))
        Throw New NotImplementedException()
    End Function

    Protected Overrides Function IsTriggerReceivedForModifyTargetOrderAsync(forcePrint As Boolean) As Task(Of List(Of Tuple(Of ExecuteCommandAction, IOrder, Decimal, String)))
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