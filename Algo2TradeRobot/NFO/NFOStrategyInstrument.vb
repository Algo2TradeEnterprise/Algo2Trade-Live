Imports NLog
Imports System.Threading
Imports Algo2TradeCore.Adapter
Imports Algo2TradeCore.Entities
Imports Algo2TradeCore.Strategies
Imports Algo2TradeCore.ChartHandler.ChartStyle
Imports System.IO
Imports Algo2TradeCore.Calculator

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

    Public ReadOnly Property TradingDay As DayOfWeek
    Public ReadOnly Property TakeTradeToday As Boolean

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

        Select Case CType(Me.ParentStrategy.UserSettings, NFOUserInputs).InstrumentsData(Me.TradableInstrument.TradingSymbol).TradingDay.ToUpper.Trim
            Case DayOfWeek.Monday.ToString.ToUpper
                Me.TradingDay = DayOfWeek.Monday
            Case DayOfWeek.Tuesday.ToString.ToUpper
                Me.TradingDay = DayOfWeek.Tuesday
            Case DayOfWeek.Wednesday.ToString.ToUpper
                Me.TradingDay = DayOfWeek.Wednesday
            Case DayOfWeek.Thursday.ToString.ToUpper
                Me.TradingDay = DayOfWeek.Thursday
            Case DayOfWeek.Friday.ToString.ToUpper
                Me.TradingDay = DayOfWeek.Friday
            Case Else
                Throw New NotImplementedException
        End Select
    End Sub

    Public Overrides Function PopulateChartAndIndicatorsAsync(candleCreator As Chart, currentCandle As OHLCPayload) As Task
        Throw New NotImplementedException
    End Function

    Public Overrides Async Function MonitorAsync() As Task
        Try
            Me.TradableInstrument.FetchHistorical = False
            GetLastSignalDetails(Now.Date)

            If Now.DayOfWeek = Me.TradingDay Then
                If Not (CType(Me.ParentStrategy, NFOStrategy).NSEHolidays IsNot Nothing AndAlso
                    CType(Me.ParentStrategy, NFOStrategy).NSEHolidays.Contains(Now.Date)) Then
                    _TakeTradeToday = True
                End If
            Else
                Dim daysUntilTuesday As Integer = (CInt(Me.TradingDay) - CInt(Now.DayOfWeek) + 7) Mod 7
                Dim nextTradingDate As Date = Now.Date.AddDays(daysUntilTuesday).Date
                If CType(Me.ParentStrategy, NFOStrategy).NSEHolidays IsNot Nothing AndAlso
                    CType(Me.ParentStrategy, NFOStrategy).NSEHolidays.Contains(nextTradingDate.Date) Then
                    While nextTradingDate.Date >= Now.Date
                        If CType(Me.ParentStrategy, NFOStrategy).NSEHolidays.Contains(nextTradingDate.Date) OrElse
                            nextTradingDate.DayOfWeek = DayOfWeek.Saturday OrElse nextTradingDate.DayOfWeek = DayOfWeek.Sunday Then
                            nextTradingDate = nextTradingDate.AddDays(-1)
                        Else
                            If nextTradingDate.Date = Now.Date Then
                                _TakeTradeToday = True
                            End If
                            Exit While
                        End If
                    End While
                End If
            End If

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
            If Me.TakeTradeToday Then
                If currentTime >= userSettings.TradeEntryTime AndAlso _lastTick IsNot Nothing AndAlso Not _entryDoneForTheDay Then
                    Dim signalCandle As OHLCPayload = New OHLCPayload(OHLCPayload.PayloadSource.CalculatedTick)
                    signalCandle.SnapshotDateTime = _lastTick.Timestamp.Value.Date
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
                        _tempSignal = New SignalDetails(Me, lastSignal, Me.TradableInstrument.TradingSymbol, signalCandle.SnapshotDateTime, signalCandle.ClosePrice.Value, 0, desireValue)
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
                                            .Quantity = Math.Abs(quantity)
                                         }
                        End If
                        If log AndAlso Not forcePrint Then
                            Dim remarks As String = String.Format("Date={0}, Desire Value={1}, Total Value Before Rebalancing=(No. of Shares Owned Before Rebalancing[{2}]*Close Price[{3}])={4}, So Amount to Invest=(Desire Value[{5}]-Total Value Before Rebalancing[{6}])={7}, So No. of Shares To Buy/Sell={8}",
                                                                  _tempSignal.SnapshotDate.ToString("dd-MMM-yyyy"),
                                                                  _tempSignal.DesireValue,
                                                                  _tempSignal.NoOfSharesOwnedBeforeRebalancing,
                                                                  _tempSignal.ClosePrice,
                                                                  _tempSignal.TotalValueBeforeRebalancing,
                                                                  _tempSignal.DesireValue,
                                                                  _tempSignal.TotalValueBeforeRebalancing,
                                                                  _tempSignal.AmountToInvest,
                                                                  _tempSignal.NoOfSharesToBuy)

                            logger.Fatal(remarks)
                            SendTradeAlertMessageAsync(remarks)
                        End If
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
            For Each runningSignal In AllSignalDetails.Values
                runningSignal.ParentStrategyInstrument = Me
            Next
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
            Dim signal As SignalDetails = New SignalDetails(Me, lastSignal, Me.TradableInstrument.TradingSymbol, snapshotDate, closePrice, entryPrice, desireValue)
            _allSignalDetails.Add(signal.SnapshotDate, signal)

            Dim remarks As String = String.Format("Date={0}, {1}, No. of Shares Owned After Rebalancing={2}, Total Invested=(Previous Investment[{3}]+Entry Price[{4}]*No. of Shares To Buy[{5}]={6})",
                                                  signal.SnapshotDate.ToString("dd-MMM-yyyy"),
                                                  If(signal.NoOfSharesToBuy = 0, "Trades not taken", "Trades taken"),
                                                  signal.SharesOwnedAfterRebalancing,
                                                  If(signal.PreviousSignal IsNot Nothing, signal.PreviousSignal.TotalInvested, 0),
                                                  signal.EntryPrice,
                                                  signal.NoOfSharesToBuy,
                                                  signal.TotalInvested)
            logger.Fatal(remarks)
            SendTradeAlertMessageAsync(remarks)

            Utilities.Strings.SerializeFromCollection(Of Dictionary(Of Date, SignalDetails))(_signalDetailsFilename, AllSignalDetails)
            ret = True
        End If
        Return ret
    End Function

    Private Async Function SendTradeAlertMessageAsync(ByVal message As String) As Task
        Try
            Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
            _cts.Token.ThrowIfCancellationRequested()
            'If message.Contains("&") Then
            '    message = message.Replace("&", "_")
            'End If
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

    Private Function GetTotalTaxAndCharges(ByVal buy As Double, ByVal sell As Double, ByVal quantity As Integer) As Decimal
        Dim ret As Decimal = Decimal.MinValue
        Dim calculator As ZerodhaBrokerageCalculator = New ZerodhaBrokerageCalculator(Me.ParentStrategy.ParentController, _cts)
        Dim brokerageAttributes As IBrokerageAttributes = calculator.GetDeliveryEquityBrokerage(buy, sell, quantity)
        ret = CType(brokerageAttributes, ZerodhaBrokerageAttributes).TotalTax
        Return ret
    End Function

    <Serializable>
    Public Class SignalDetails
        Public Sub New(ByVal parentStrategyInstrument As NFOStrategyInstrument,
                       ByVal previousSignal As SignalDetails,
                       ByVal tradingSymbol As String,
                       ByVal snapshotDate As Date,
                       ByVal closePrice As Decimal,
                       ByVal entryPrice As Decimal,
                       ByVal desireValue As Double)
            Me.ParentStrategyInstrument = parentStrategyInstrument
            Me.PreviousSignal = previousSignal
            Me.TradingSymbol = tradingSymbol
            Me.SnapshotDate = snapshotDate
            Me.ClosePrice = closePrice
            Me.EntryPrice = Math.Round(entryPrice, 2)
            Me.DesireValue = Math.Round(desireValue, 2)
        End Sub

        <NonSerialized>
        Public ParentStrategyInstrument As NFOStrategyInstrument

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

        Public ReadOnly Property TotalValueBeforeRebalancing As Double  'Brokerage to add
            Get
                Dim totalTax As Decimal = 0
                If Me.NoOfSharesOwnedBeforeRebalancing <> 0 Then
                    totalTax = ParentStrategyInstrument.GetTotalTaxAndCharges(0, Me.ClosePrice, Me.NoOfSharesOwnedBeforeRebalancing)
                End If
                Return Math.Round(Me.ClosePrice * Me.NoOfSharesOwnedBeforeRebalancing - totalTax, 2)
            End Get
        End Property

        Public ReadOnly Property AmountToInvest As Double
            Get
                Return Math.Round(Me.DesireValue - Me.TotalValueBeforeRebalancing, 2)
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

        Public ReadOnly Property TotalInvested As Double    'Brokerage to add
            Get
                Dim totalTax As Decimal = ParentStrategyInstrument.GetTotalTaxAndCharges(Me.EntryPrice, 0, Me.NoOfSharesToBuy)
                If Me.PreviousSignal IsNot Nothing Then
                    Return Math.Round(Me.PreviousSignal.TotalInvested + Me.NoOfSharesToBuy * Me.EntryPrice + totalTax, 2)
                Else
                    Return Math.Round(Me.NoOfSharesToBuy * Me.EntryPrice + totalTax, 2)
                End If
            End Get
        End Property

        Public ReadOnly Property PeriodicInvestment As Double
            Get
                If Me.PreviousSignal IsNot Nothing Then
                    Return Math.Round(Me.PreviousSignal.TotalInvested - Me.TotalInvested, 2)
                Else
                    Return Math.Round(0 - Me.TotalInvested, 2)
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