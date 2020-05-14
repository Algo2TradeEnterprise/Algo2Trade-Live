Imports System.Threading
Imports Algo2TradeCore.Entities
Imports Algo2TradeCore.Calculator
Imports AliceBlueClient
Imports NLog
Imports Algo2TradeCore.Controller
Imports Algo2TradeCore.Exceptions
Imports System.Text.RegularExpressions

Namespace Adapter
    Public Class AliceAdapter
        Inherits APIAdapter

#Region "Logging and Status Progress"
        Public Shared Shadows logger As Logger = LogManager.GetCurrentClassLogger
#End Region

        Protected _Alice As AliceBlue
        Public Sub New(ByVal associatedParentController As AliceStrategyController,
                        ByVal canceller As CancellationTokenSource)
            MyBase.New(associatedParentController, canceller)
            _Alice = New AliceBlue(UserID:=CType(associatedParentController.APIConnection, AliceConnection).AliceUser.UserId,
                                    APISecret:=CType(associatedParentController.APIConnection, AliceConnection).AliceUser.APISecret,
                                    AccessToken:=CType(associatedParentController.APIConnection, AliceConnection).AccessToken)
            _Alice.SetSessionExpiryHook(AddressOf associatedParentController.OnSessionExpireAsync)
            Calculator = New AliceBrokerageCalculator(Me.ParentController, canceller)
        End Sub

        Public Sub New(ByVal associatedParentController As AliceStrategyController,
                       ByVal associatedInstrument As IInstrument,
                       ByVal canceller As CancellationTokenSource)
            MyBase.New(associatedParentController, associatedInstrument, canceller)
            _Alice = New AliceBlue(UserID:=CType(associatedParentController.APIConnection, AliceConnection).AliceUser.UserId,
                                    APISecret:=CType(associatedParentController.APIConnection, AliceConnection).AliceUser.APISecret,
                                    AccessToken:=CType(associatedParentController.APIConnection, AliceConnection).AccessToken)
            _Alice.SetSessionExpiryHook(AddressOf associatedParentController.OnSessionExpireAsync)
            Calculator = New AliceBrokerageCalculator(Me.ParentController, canceller)
        End Sub

#Region "Access Token"
        Public Overrides Sub SetAPIAccessToken(apiAccessToken As String)
            _Alice.SetAccessToken(apiAccessToken)
        End Sub
#End Region

#Region "Brokerage Calculator"
        Public Overrides Function CalculatePLWithBrokerage(instrument As IInstrument, buy As Double, sell As Double, quantity As Integer) As Decimal
            Dim ret As Decimal = Nothing
            Dim brokerageAttributes As IBrokerageAttributes = Nothing
            Select Case instrument.RawExchange
                Case "NSE"
                    brokerageAttributes = Calculator.GetIntradayEquityBrokerage(buy, sell, quantity)
                Case "NFO"
                    brokerageAttributes = Calculator.GetIntradayEquityFuturesBrokerage(buy, sell, quantity)
                Case "MCX"
                    brokerageAttributes = Calculator.GetIntradayCommodityFuturesBrokerage(instrument, buy, sell, quantity)
                Case "CDS"
                    brokerageAttributes = Calculator.GetIntradayCurrencyFuturesBrokerage(buy, sell, quantity)
                Case Else
                    Throw New NotImplementedException("Calculator not implemented")
            End Select
            ret = brokerageAttributes.NetProfitLoss
            Return ret
        End Function
#End Region

#Region "All Instruments"
        Public Overrides Async Function GetAllInstrumentsAsync() As Task(Of IEnumerable(Of IInstrument))
            logger.Debug("GetAllInstrumentsAsync, parameters:Nothing")
            Dim ret As List(Of AliceInstrument) = Nothing
            Dim execCommand As ExecutionCommands = ExecutionCommands.GetInstruments

            _cts.Token.ThrowIfCancellationRequested()
            Dim tempAllRet As Dictionary(Of String, Object) = Nothing
            Try
                tempAllRet = Await ExecuteCommandAsync(execCommand, Nothing).ConfigureAwait(False)
            Catch tex As TokenException
                Throw New AliceBusinessException(tex.Message, tex, AdapterBusinessException.TypeOfException.TokenException)
            Catch oex As OrderException
                Throw New AliceBusinessException(oex.Message, oex, AdapterBusinessException.TypeOfException.OrderException)
            Catch uex As UnknownException
                Throw New AliceBusinessException(uex.Message, uex, AdapterBusinessException.TypeOfException.UnknownException)
            Catch ex As Exception
                Throw ex
            End Try
            _cts.Token.ThrowIfCancellationRequested()

            Dim tempRet As Object = Nothing
            If tempAllRet IsNot Nothing AndAlso tempAllRet.ContainsKey(execCommand.ToString) Then
                tempRet = tempAllRet(execCommand.ToString)
                If tempRet IsNot Nothing Then
                    Dim errorMessage As String = ParentController.GetErrorResponse(tempRet)
                    If errorMessage IsNot Nothing Then
                        Throw New ApplicationException(errorMessage)
                    End If
                Else
                    Throw New ApplicationException(String.Format("Alice command execution did not return anything, command:{0}", execCommand.ToString))
                End If
            Else
                Throw New ApplicationException(String.Format("Relevant command was fired but not detected in the response, command:{0}", execCommand.ToString))
            End If

            If tempRet.GetType = GetType(List(Of Instrument)) Then
                OnHeartbeat(String.Format("Creating Alice instrument collection from API instruments, count:{0}", tempRet.count))
                Dim aliceReturedInstruments As List(Of Instrument) = CType(tempRet, List(Of Instrument))
                For Each runningInstrument As Instrument In aliceReturedInstruments
                    _cts.Token.ThrowIfCancellationRequested()
                    If ret Is Nothing Then ret = New List(Of AliceInstrument)
                    ret.Add(New AliceInstrument(Me.ParentController, runningInstrument.InstrumentToken) With {.WrappedInstrument = runningInstrument})
                Next
            Else
                Throw New ApplicationException(String.Format("Alice command execution did not return any list of instrument, command:{0}", execCommand.ToString))
            End If
            Return ret
        End Function

        Public Overrides Function CreateSingleInstrument(supportedTradingSymbol As String, instrumentToken As UInteger, sampleInstrument As IInstrument) As IInstrument
            Dim ret As AliceInstrument = Nothing
            If supportedTradingSymbol IsNot Nothing Then
                Dim dummyInstrument As Instrument = New Instrument With
                    {
                    .TradingSymbol = supportedTradingSymbol,
                    .Exchange = sampleInstrument.RawExchange,
                    .Expiry = Nothing,
                    .ExchangeToken = instrumentToken,
                    .InstrumentToken = instrumentToken,
                    .InstrumentType = Nothing,
                    .LotSize = 0,
                    .Name = supportedTradingSymbol,
                    .Segment = Nothing,
                    .TickSize = sampleInstrument.TickSize
                    }

                ret = New AliceInstrument(Me.ParentController, dummyInstrument.InstrumentToken) With {.WrappedInstrument = dummyInstrument}
            End If
            Return ret
        End Function
#End Region

#Region "Quotes"
        Public Overrides Async Function GetAllQuotesAsync(instruments As IEnumerable(Of IInstrument)) As Task(Of IEnumerable(Of IQuote))
            'logger.Debug("GetAllQuotes, parameters:{0}", Utils.JsonSerialize(instruments))
            Dim ret As List(Of AliceQuote) = Nothing
            Dim execCommand As ExecutionCommands = ExecutionCommands.GetQuotes

            _cts.Token.ThrowIfCancellationRequested()
            Dim tempAllRet As Dictionary(Of String, Object) = Nothing
            Try
                tempAllRet = Await ExecuteCommandAsync(execCommand, New Dictionary(Of String, Object) From {{"instruments", instruments}}).ConfigureAwait(False)
            Catch tex As TokenException
                Throw New AliceBusinessException(tex.Message, tex, AdapterBusinessException.TypeOfException.TokenException)
            Catch oex As OrderException
                Throw New AliceBusinessException(oex.Message, oex, AdapterBusinessException.TypeOfException.OrderException)
            Catch uex As UnknownException
                Throw New AliceBusinessException(uex.Message, uex, AdapterBusinessException.TypeOfException.UnknownException)
            Catch ex As Exception
                Throw ex
            End Try
            _cts.Token.ThrowIfCancellationRequested()

            Dim tempRet As Object = Nothing
            If tempAllRet IsNot Nothing AndAlso tempAllRet.ContainsKey(execCommand.ToString) Then
                tempRet = tempAllRet(execCommand.ToString)
                If tempRet IsNot Nothing Then
                    Dim errorMessage As String = ParentController.GetErrorResponse(tempRet)
                    If errorMessage IsNot Nothing Then
                        Throw New ApplicationException(errorMessage)
                    End If
                Else
                    Throw New ApplicationException(String.Format("Alice command execution did not return anything, command:{0}", execCommand.ToString))
                End If
            Else
                Throw New ApplicationException(String.Format("Relevant command was fired but not detected in the response, command:{0}", execCommand.ToString))
            End If

            If tempRet.GetType = GetType(Dictionary(Of String, Quote)) Then
                OnHeartbeat(String.Format("Creating Alice quote collection from API quotes, count:{0}", tempRet.count))
                Dim AliceReturedQuotes As Dictionary(Of String, Quote) = CType(tempRet, Dictionary(Of String, Quote))
                For Each runningQuote In AliceReturedQuotes
                    _cts.Token.ThrowIfCancellationRequested()
                    If ret Is Nothing Then ret = New List(Of AliceQuote)
                    ret.Add(New AliceQuote() With {.WrappedQuote = runningQuote.Value})
                Next
            Else
                Throw New ApplicationException(String.Format("Alice command execution did not return any list of quotes, command:{0}", execCommand.ToString))
            End If
            Return ret
        End Function
#End Region

#Region "Trades"
        Public Overrides Async Function GetAllTradesAsync() As Task(Of IEnumerable(Of ITrade))
            'logger.Debug("GetAllTradesAsync, parameters:Nothing")
            Dim ret As List(Of AliceTrade) = Nothing
            Dim execCommand As ExecutionCommands = ExecutionCommands.GetOrderTrades
            _cts.Token.ThrowIfCancellationRequested()
            Dim tempAllRet As Dictionary(Of String, Object) = Nothing
            Try
                tempAllRet = Await ExecuteCommandAsync(execCommand, Nothing).ConfigureAwait(False)
            Catch tex As TokenException
                Throw New AliceBusinessException(tex.Message, tex, AdapterBusinessException.TypeOfException.TokenException)
            Catch oex As OrderException
                Throw New AliceBusinessException(oex.Message, oex, AdapterBusinessException.TypeOfException.OrderException)
            Catch uex As UnknownException
                Throw New AliceBusinessException(uex.Message, uex, AdapterBusinessException.TypeOfException.UnknownException)
            Catch ex As Exception
                Throw ex
            End Try
            _cts.Token.ThrowIfCancellationRequested()

            Dim tempRet As Object = Nothing
            If tempAllRet IsNot Nothing AndAlso tempAllRet.ContainsKey(execCommand.ToString) Then
                tempRet = tempAllRet(execCommand.ToString)
                If tempRet IsNot Nothing Then
                    Dim errorMessage As String = ParentController.GetErrorResponse(tempRet)
                    If errorMessage IsNot Nothing Then
                        Throw New ApplicationException(errorMessage)
                    End If
                Else
                    Throw New ApplicationException(String.Format("Alice command execution did not return anything, command:{0}", execCommand.ToString))
                End If
            Else
                Throw New ApplicationException(String.Format("Relevant command was fired but not detected in the response, command:{0}", execCommand.ToString))
            End If

            If tempRet.GetType = GetType(List(Of Trade)) Then
                If tempRet.count > 0 Then
                    OnHeartbeat(String.Format("Creating Alice trade collection from API trades, count:{0}", tempRet.count))
                    Dim AliceReturedTrades As List(Of Trade) = CType(tempRet, List(Of Trade))
                    For Each runningTrade As Trade In AliceReturedTrades
                        _cts.Token.ThrowIfCancellationRequested()
                        If ret Is Nothing Then ret = New List(Of AliceTrade)
                        ret.Add(New AliceTrade With {.WrappedTrade = runningTrade})
                    Next
                Else
                    OnHeartbeat(String.Format("Alice command execution did not return any list of trade, command:{0}", execCommand.ToString))
                    If ret Is Nothing Then ret = New List(Of AliceTrade)
                End If
            Else
                Throw New ApplicationException(String.Format("Alice command execution did not return any list of trade, command:{0}", execCommand.ToString))
            End If
            Return ret
        End Function
#End Region

#Region "Orders"
        Public Overrides Async Function GetAllOrdersAsync() As Task(Of IEnumerable(Of IOrder))
            'logger.Debug("GetAllOrdersAsync, parameters:Nothing")
            Dim ret As List(Of AliceOrder) = Nothing
            Dim execCommand As ExecutionCommands = ExecutionCommands.GetOrders
            _cts.Token.ThrowIfCancellationRequested()
            Dim tempAllRet As Dictionary(Of String, Object) = Nothing
            Try
                tempAllRet = Await ExecuteCommandAsync(execCommand, Nothing).ConfigureAwait(False)
            Catch tex As TokenException
                Throw New AliceBusinessException(tex.Message, tex, AdapterBusinessException.TypeOfException.TokenException)
            Catch oex As OrderException
                Throw New AliceBusinessException(oex.Message, oex, AdapterBusinessException.TypeOfException.OrderException)
            Catch uex As UnknownException
                Throw New AliceBusinessException(uex.Message, uex, AdapterBusinessException.TypeOfException.UnknownException)
            Catch ex As Exception
                Throw ex
            End Try
            _cts.Token.ThrowIfCancellationRequested()

            Dim tempRet As Object = Nothing
            If tempAllRet IsNot Nothing AndAlso tempAllRet.ContainsKey(execCommand.ToString) Then
                tempRet = tempAllRet(execCommand.ToString)
                If tempRet IsNot Nothing Then
                    Dim errorMessage As String = ParentController.GetErrorResponse(tempRet)
                    If errorMessage IsNot Nothing Then
                        Throw New ApplicationException(errorMessage)
                    End If
                Else
                    Throw New ApplicationException(String.Format("Alice command execution did not return anything, command:{0}", execCommand.ToString))
                End If
            Else
                Throw New ApplicationException(String.Format("Relevant command was fired but not detected in the response, command:{0}", execCommand.ToString))
            End If

            If tempRet.GetType = GetType(List(Of Order)) Then
                If tempRet.count > 0 Then
                    'OnHeartbeat(String.Format("Creating Alice order collection from API orders, count:{0}", tempRet.count))
                    'logger.Debug(String.Format("Creating Alice order collection from API orders, count:{0}", tempRet.count))
                    Dim AliceReturedOrders As List(Of Order) = CType(tempRet, List(Of Order))
                    For Each runningOrder As Order In AliceReturedOrders
                        _cts.Token.ThrowIfCancellationRequested()
                        Dim tag As String = runningOrder.Tag
                        If tag Is Nothing OrElse tag.Trim = "" Then
                            Dim dummyOrder As AliceOrder = New AliceOrder With {.WrappedOrder = runningOrder}
                            tag = Await Me.ParentController.GetOrderTagAsync(runningOrder.InstrumentToken, dummyOrder.OrderIdentifier, dummyOrder.ParentOrderIdentifier, runningOrder.OrderTimestamp.Value, dummyOrder.TransactionType, dummyOrder.OrderType, dummyOrder.Quantity).ConfigureAwait(False)
                        End If
                        If tag IsNot Nothing AndAlso tag.Trim <> "" Then
                            runningOrder.Tag = tag
                            If ret Is Nothing Then ret = New List(Of AliceOrder)
                            ret.Add(New AliceOrder With {.WrappedOrder = runningOrder})
                        End If
                    Next
                    'Else
                    'OnHeartbeat(String.Format("Alice command execution did not return any list of order, command:{0}", execCommand.ToString))
                    'logger.Debug(String.Format("Alice command execution did not return any list of order, command:{0}", execCommand.ToString))
                End If
            Else
                Throw New ApplicationException(String.Format("Alice command execution did not return any list of order, command:{0}", execCommand.ToString))
            End If
            Return ret
        End Function

        Public Overrides Function CreateSimilarOrderWithTag(tag As String, orderData As IOrder) As IOrder
            Dim ret As AliceOrder = Nothing
            If orderData IsNot Nothing Then
                Dim similarOrder As Order = CType(orderData, AliceOrder).WrappedOrder
                similarOrder.Tag = tag
                ret = New AliceOrder With {.WrappedOrder = similarOrder}
            End If
            Return ret
        End Function
#End Region

#Region "Holdings"
        Public Overrides Async Function GetAllHoldingsAsync() As Task(Of IEnumerable(Of IHolding))
            'logger.Debug("GetAllOrdersAsync, parameters:Nothing")
            Dim ret As List(Of AliceHolding) = Nothing
            Dim execCommand As ExecutionCommands = ExecutionCommands.GetHoldings
            _cts.Token.ThrowIfCancellationRequested()
            Dim tempAllRet As Dictionary(Of String, Object) = Nothing
            Try
                tempAllRet = Await ExecuteCommandAsync(execCommand, Nothing).ConfigureAwait(False)
            Catch tex As TokenException
                Throw New AliceBusinessException(tex.Message, tex, AdapterBusinessException.TypeOfException.TokenException)
            Catch oex As OrderException
                Throw New AliceBusinessException(oex.Message, oex, AdapterBusinessException.TypeOfException.OrderException)
            Catch uex As UnknownException
                Throw New AliceBusinessException(uex.Message, uex, AdapterBusinessException.TypeOfException.UnknownException)
            Catch ex As Exception
                Throw ex
            End Try
            _cts.Token.ThrowIfCancellationRequested()

            Dim tempRet As Object = Nothing
            If tempAllRet IsNot Nothing AndAlso tempAllRet.ContainsKey(execCommand.ToString) Then
                tempRet = tempAllRet(execCommand.ToString)
                If tempRet IsNot Nothing Then
                    Dim errorMessage As String = ParentController.GetErrorResponse(tempRet)
                    If errorMessage IsNot Nothing Then
                        Throw New ApplicationException(errorMessage)
                    End If
                Else
                    Throw New ApplicationException(String.Format("Alice command execution did not return anything, command:{0}", execCommand.ToString))
                End If
            Else
                Throw New ApplicationException(String.Format("Relevant command was fired but not detected in the response, command:{0}", execCommand.ToString))
            End If

            If tempRet.GetType = GetType(List(Of Holding)) Then
                If tempRet.count > 0 Then
                    'OnHeartbeat(String.Format("Creating Alice order collection from API orders, count:{0}", tempRet.count))
                    'logger.Debug(String.Format("Creating Alice order collection from API orders, count:{0}", tempRet.count))
                    Dim AliceReturedHoldings As List(Of Holding) = CType(tempRet, List(Of Holding))
                    For Each runningOrder As Holding In AliceReturedHoldings
                        _cts.Token.ThrowIfCancellationRequested()
                        If ret Is Nothing Then ret = New List(Of AliceHolding)
                        ret.Add(New AliceHolding With {.WrappedHolding = runningOrder})
                    Next
                    'Else
                    'OnHeartbeat(String.Format("Alice command execution did not return any list of order, command:{0}", execCommand.ToString))
                    'logger.Debug(String.Format("Alice command execution did not return any list of order, command:{0}", execCommand.ToString))
                End If
            Else
                Throw New ApplicationException(String.Format("Alice command execution did not return any list of holding, command:{0}", execCommand.ToString))
            End If
            Return ret
        End Function
#End Region

#Region "Positions"
        Public Overrides Async Function GetAllPositionsAsync() As Task(Of IPositionResponse)
            'logger.Debug("GetAllPositionsAsync, parameters:Nothing")
            Dim ret As BusinessPositionResponse = Nothing
            Dim execCommand As ExecutionCommands = ExecutionCommands.GetPositions
            _cts.Token.ThrowIfCancellationRequested()
            Dim tempAllRet As Dictionary(Of String, Object) = Nothing
            Try
                tempAllRet = Await ExecuteCommandAsync(execCommand, Nothing).ConfigureAwait(False)
            Catch tex As TokenException
                Throw New AliceBusinessException(tex.Message, tex, AdapterBusinessException.TypeOfException.TokenException)
            Catch oex As OrderException
                Throw New AliceBusinessException(oex.Message, oex, AdapterBusinessException.TypeOfException.OrderException)
            Catch uex As UnknownException
                Throw New AliceBusinessException(uex.Message, uex, AdapterBusinessException.TypeOfException.UnknownException)
            Catch ex As Exception
                Throw ex
            End Try
            _cts.Token.ThrowIfCancellationRequested()

            Dim tempRet As Object = Nothing
            If tempAllRet IsNot Nothing AndAlso tempAllRet.ContainsKey(execCommand.ToString) Then
                tempRet = tempAllRet(execCommand.ToString)
                If tempRet IsNot Nothing Then
                    Dim errorMessage As String = ParentController.GetErrorResponse(tempRet)
                    If errorMessage IsNot Nothing Then
                        Throw New ApplicationException(errorMessage)
                    End If
                Else
                    Throw New ApplicationException(String.Format("Alice command execution did not return anything, command:{0}", execCommand.ToString))
                End If
            Else
                Throw New ApplicationException(String.Format("Relevant command was fired but not detected in the response, command:{0}", execCommand.ToString))
            End If

            If tempRet.GetType = GetType(PositionResponse) Then
                'OnHeartbeat(String.Format("Creating Alice position collection from API position, count:{0}", tempRet.count))
                'logger.Debug(String.Format("Creating Alice position collection from API position, count:{0}", tempRet.count))
                Dim AliceReturedPositions As PositionResponse = CType(tempRet, PositionResponse)

                If ret Is Nothing Then ret = New BusinessPositionResponse

                If AliceReturedPositions.Day IsNot Nothing AndAlso AliceReturedPositions.Day.Count > 0 Then
                    For Each runningPosition As Position In AliceReturedPositions.Day
                        _cts.Token.ThrowIfCancellationRequested()
                        If ret.Day Is Nothing Then ret.Day = New List(Of IPosition)
                        ret.Day.Add(New AlicePosition With {.WrappedPosition = runningPosition})
                    Next
                End If

                If AliceReturedPositions.Net IsNot Nothing AndAlso AliceReturedPositions.Net.Count > 0 Then
                    For Each runningPosition As Position In AliceReturedPositions.Net
                        _cts.Token.ThrowIfCancellationRequested()
                        If ret.Net Is Nothing Then ret.Net = New List(Of IPosition)
                        ret.Net.Add(New AlicePosition With {.WrappedPosition = runningPosition})
                    Next
                End If
                'Else
                'OnHeartbeat(String.Format("Alice command execution did not return any list of order, command:{0}", execCommand.ToString))
                'logger.Debug(String.Format("Alice command execution did not return any list of order, command:{0}", execCommand.ToString))
            Else
                Throw New ApplicationException(String.Format("Alice command execution did not return any position response, command:{0}", execCommand.ToString))
            End If
            Return ret
        End Function
#End Region

#Region "Margin"
        Public Overrides Async Function GetUserMarginsAsync() As Task(Of Dictionary(Of TypeOfExchage, IUserMargin))
            'logger.Debug("GetAllOrdersAsync, parameters:Nothing")
            Dim ret As Dictionary(Of Enums.TypeOfExchage, IUserMargin) = Nothing
            Dim execCommand As ExecutionCommands = ExecutionCommands.GetUserMargins
            _cts.Token.ThrowIfCancellationRequested()
            Dim tempAllRet As Dictionary(Of String, Object) = Nothing
            Try
                tempAllRet = Await ExecuteCommandAsync(execCommand, Nothing).ConfigureAwait(False)
            Catch tex As TokenException
                Throw New AliceBusinessException(tex.Message, tex, AdapterBusinessException.TypeOfException.TokenException)
            Catch oex As OrderException
                Throw New AliceBusinessException(oex.Message, oex, AdapterBusinessException.TypeOfException.OrderException)
            Catch uex As UnknownException
                Throw New AliceBusinessException(uex.Message, uex, AdapterBusinessException.TypeOfException.UnknownException)
            Catch ex As Exception
                Throw ex
            End Try
            _cts.Token.ThrowIfCancellationRequested()

            Dim tempRet As Object = Nothing
            If tempAllRet IsNot Nothing AndAlso tempAllRet.ContainsKey(execCommand.ToString) Then
                tempRet = tempAllRet(execCommand.ToString)
                If tempRet IsNot Nothing Then
                    Dim errorMessage As String = ParentController.GetErrorResponse(tempRet)
                    If errorMessage IsNot Nothing Then
                        Throw New ApplicationException(errorMessage)
                    End If
                Else
                    Throw New ApplicationException(String.Format("Alice command execution did not return anything, command:{0}", execCommand.ToString))
                End If
            Else
                Throw New ApplicationException(String.Format("Relevant command was fired but not detected in the response, command:{0}", execCommand.ToString))
            End If

            If tempRet.GetType = GetType(UserMarginsResponse) Then
                'OnHeartbeat(String.Format("Creating Alice order collection from API orders, count:{0}", tempRet.count))
                logger.Debug(String.Format("Creating IBussinessUserMargin from API User Margin", Utils.JsonSerialize(tempRet)))
                Dim AliceReturedUserMarginResponse As UserMarginsResponse = CType(tempRet, UserMarginsResponse)
                logger.Debug(Utilities.Strings.JsonSerialize(AliceReturedUserMarginResponse))
                Dim equityMargin As New AliceUserMargin With
                    {.WrappedUserMargin = AliceReturedUserMarginResponse.Equity}
                Dim commodityMargin As New AliceUserMargin With
                    {.WrappedUserMargin = AliceReturedUserMarginResponse.Commodity}
                If ret Is Nothing Then ret = New Dictionary(Of Enums.TypeOfExchage, IUserMargin)
                ret.Add(Enums.TypeOfExchage.NSE, equityMargin)
                ret.Add(Enums.TypeOfExchage.MCX, commodityMargin)
            Else
                Throw New ApplicationException(String.Format("Alice command execution did not return any User margin, command:{0}", execCommand.ToString))
            End If
            Return ret
        End Function
#End Region

#Region "Modify Order"
        Public Overrides Async Function ModifyStoplossOrderAsync(orderId As String, triggerPrice As Decimal) As Task(Of Dictionary(Of String, Object))
            Dim ret As Dictionary(Of String, Object) = Nothing
            Dim execCommand As ExecutionCommands = ExecutionCommands.ModifySLOrderPrice
            _cts.Token.ThrowIfCancellationRequested()
            Dim tradeParameters As New Dictionary(Of String, Object) From {{"OrderId", orderId}, {"TriggerPrice", triggerPrice}}
            Dim tempAllRet As Dictionary(Of String, Object) = Nothing
            Try
                tempAllRet = Await ExecuteCommandAsync(execCommand, tradeParameters).ConfigureAwait(False)
            Catch tex As TokenException
                Throw New AliceBusinessException(tex.Message, tex, AdapterBusinessException.TypeOfException.TokenException)
            Catch oex As OrderException
                Throw New AliceBusinessException(oex.Message, oex, AdapterBusinessException.TypeOfException.OrderException)
            Catch uex As UnknownException
                Throw New AliceBusinessException(uex.Message, uex, AdapterBusinessException.TypeOfException.UnknownException)
            Catch ex As Exception
                Throw ex
            End Try
            _cts.Token.ThrowIfCancellationRequested()

            Dim tempRet As Object = Nothing
            If tempAllRet IsNot Nothing AndAlso tempAllRet.ContainsKey(execCommand.ToString) Then
                tempRet = tempAllRet(execCommand.ToString)
                If tempRet IsNot Nothing Then
                    Dim errorMessage As String = ParentController.GetErrorResponse(tempRet)
                    If errorMessage IsNot Nothing Then
                        Throw New ApplicationException(errorMessage)
                    End If
                Else
                    Throw New ApplicationException(String.Format("Alice command execution did not return anything, command:{0}", execCommand.ToString))
                End If
            Else
                Throw New ApplicationException(String.Format("Relevant command was fired but not detected in the response, command:{0}", execCommand.ToString))
            End If

            If tempRet.GetType = GetType(Dictionary(Of String, Object)) Then
                OnHeartbeat(String.Format("Modify Order successful, details:{0}", Utils.JsonSerialize(tempRet)))
                ret = CType(tempRet, Dictionary(Of String, Object))
            Else
                Throw New ApplicationException(String.Format("Alice command execution did not return anything, command:{0}", execCommand.ToString))
            End If
            Return ret
        End Function

        Public Overrides Async Function ModifyTargetOrderAsync(orderId As String, price As Decimal) As Task(Of Dictionary(Of String, Object))
            Dim ret As Dictionary(Of String, Object) = Nothing
            Dim execCommand As ExecutionCommands = ExecutionCommands.ModifyTargetOrderPrice
            _cts.Token.ThrowIfCancellationRequested()
            Dim tradeParameters As New Dictionary(Of String, Object) From {{"OrderId", orderId}, {"Price", price}}
            Dim tempAllRet As Dictionary(Of String, Object) = Nothing
            Try
                tempAllRet = Await ExecuteCommandAsync(execCommand, tradeParameters).ConfigureAwait(False)
            Catch tex As TokenException
                Throw New AliceBusinessException(tex.Message, tex, AdapterBusinessException.TypeOfException.TokenException)
            Catch oex As OrderException
                Throw New AliceBusinessException(oex.Message, oex, AdapterBusinessException.TypeOfException.OrderException)
            Catch uex As UnknownException
                Throw New AliceBusinessException(uex.Message, uex, AdapterBusinessException.TypeOfException.UnknownException)
            Catch ex As Exception
                Throw ex
            End Try
            _cts.Token.ThrowIfCancellationRequested()

            Dim tempRet As Object = Nothing
            If tempAllRet IsNot Nothing AndAlso tempAllRet.ContainsKey(execCommand.ToString) Then
                tempRet = tempAllRet(execCommand.ToString)
                If tempRet IsNot Nothing Then
                    Dim errorMessage As String = ParentController.GetErrorResponse(tempRet)
                    If errorMessage IsNot Nothing Then
                        Throw New ApplicationException(errorMessage)
                    End If
                Else
                    Throw New ApplicationException(String.Format("Alice command execution did not return anything, command:{0}", execCommand.ToString))
                End If
            Else
                Throw New ApplicationException(String.Format("Relevant command was fired but not detected in the response, command:{0}", execCommand.ToString))
            End If

            If tempRet.GetType = GetType(Dictionary(Of String, Object)) Then
                OnHeartbeat(String.Format("Modify Order successful, details:{0}", Utils.JsonSerialize(tempRet)))
                ret = CType(tempRet, Dictionary(Of String, Object))
            Else
                Throw New ApplicationException(String.Format("Alice command execution did not return anything, command:{0}", execCommand.ToString))
            End If
            Return ret
        End Function
#End Region

#Region "Cancel Order"
        Public Overrides Async Function CancelBOOrderAsync(ByVal orderId As String, ByVal parentOrderID As String) As Task(Of Dictionary(Of String, Object))
            'logger.Debug("ModifyStoplossOrderAsync, parameters:{0},{1}", orderId, parentOrderID)
            Dim ret As Dictionary(Of String, Object) = Nothing
            Dim execCommand As ExecutionCommands = ExecutionCommands.CancelOrder
            _cts.Token.ThrowIfCancellationRequested()
            Dim cancelOrderID As String = orderId
            If parentOrderID IsNot Nothing AndAlso parentOrderID.Trim <> "" Then
                cancelOrderID = parentOrderID
            End If

            Dim tradeParameters As New Dictionary(Of String, Object) From {
                {"OrderId", cancelOrderID},
                {"ParentOrderId", cancelOrderID},
                {"Variety", TypesOfVariety.BO}
            }
            Dim tempAllRet As Dictionary(Of String, Object) = Nothing
            Try
                tempAllRet = Await ExecuteCommandAsync(execCommand, tradeParameters).ConfigureAwait(False)
            Catch tex As TokenException
                Throw New AliceBusinessException(tex.Message, tex, AdapterBusinessException.TypeOfException.TokenException)
            Catch oex As OrderException
                Throw New AliceBusinessException(oex.Message, oex, AdapterBusinessException.TypeOfException.OrderException)
            Catch uex As UnknownException
                Throw New AliceBusinessException(uex.Message, uex, AdapterBusinessException.TypeOfException.UnknownException)
            Catch ex As Exception
                Throw ex
            End Try
            _cts.Token.ThrowIfCancellationRequested()

            Dim tempRet As Object = Nothing
            If tempAllRet IsNot Nothing AndAlso tempAllRet.ContainsKey(execCommand.ToString) Then
                tempRet = tempAllRet(execCommand.ToString)
                If tempRet IsNot Nothing Then
                    Dim errorMessage As String = ParentController.GetErrorResponse(tempRet)
                    If errorMessage IsNot Nothing Then
                        Throw New ApplicationException(errorMessage)
                    End If
                Else
                    Throw New ApplicationException(String.Format("Alice command execution did not return anything, command:{0}", execCommand.ToString))
                End If
            Else
                Throw New ApplicationException(String.Format("Relevant command was fired but not detected in the response, command:{0}", execCommand.ToString))
            End If

            If tempRet.GetType = GetType(Dictionary(Of String, Object)) Then
                OnHeartbeat(String.Format("Cancel Order successful, details:{0}", Utils.JsonSerialize(tempRet)))
                ret = CType(tempRet, Dictionary(Of String, Object))
            Else
                Throw New ApplicationException(String.Format("Alice command execution did not return anything, command:{0}", execCommand.ToString))
            End If
            Return ret
        End Function
        Public Overrides Async Function CancelCOOrderAsync(ByVal orderId As String, ByVal parentOrderID As String) As Task(Of Dictionary(Of String, Object))
            'logger.Debug("ModifyStoplossOrderAsync, parameters:{0},{1}", orderId, parentOrderID)
            Dim ret As Dictionary(Of String, Object) = Nothing
            Dim execCommand As ExecutionCommands = ExecutionCommands.CancelOrder
            _cts.Token.ThrowIfCancellationRequested()
            Dim tradeParameters As New Dictionary(Of String, Object) From {
                {"OrderId", orderId},
                {"ParentOrderId", parentOrderID},
                {"Variety", TypesOfVariety.CO}
            }
            Dim tempAllRet As Dictionary(Of String, Object) = Nothing
            Try
                tempAllRet = Await ExecuteCommandAsync(execCommand, tradeParameters).ConfigureAwait(False)
            Catch tex As TokenException
                Throw New AliceBusinessException(tex.Message, tex, AdapterBusinessException.TypeOfException.TokenException)
            Catch oex As OrderException
                Throw New AliceBusinessException(oex.Message, oex, AdapterBusinessException.TypeOfException.OrderException)
            Catch uex As UnknownException
                Throw New AliceBusinessException(uex.Message, uex, AdapterBusinessException.TypeOfException.UnknownException)
            Catch ex As Exception
                Throw ex
            End Try
            _cts.Token.ThrowIfCancellationRequested()

            Dim tempRet As Object = Nothing
            If tempAllRet IsNot Nothing AndAlso tempAllRet.ContainsKey(execCommand.ToString) Then
                tempRet = tempAllRet(execCommand.ToString)
                If tempRet IsNot Nothing Then
                    Dim errorMessage As String = ParentController.GetErrorResponse(tempRet)
                    If errorMessage IsNot Nothing Then
                        Throw New ApplicationException(errorMessage)
                    End If
                Else
                    Throw New ApplicationException(String.Format("Alice command execution did not return anything, command:{0}", execCommand.ToString))
                End If
            Else
                Throw New ApplicationException(String.Format("Relevant command was fired but not detected in the response, command:{0}", execCommand.ToString))
            End If

            If tempRet.GetType = GetType(Dictionary(Of String, Object)) Then
                OnHeartbeat(String.Format("Cancel Order successful, details:{0}", Utils.JsonSerialize(tempRet)))
                ret = CType(tempRet, Dictionary(Of String, Object))
            Else
                Throw New ApplicationException(String.Format("Alice command execution did not return anything, command:{0}", execCommand.ToString))
            End If
            Return ret
        End Function
        Public Overrides Async Function CancelRegularOrderAsync(ByVal orderId As String, ByVal parentOrderID As String) As Task(Of Dictionary(Of String, Object))
            'logger.Debug("ModifyStoplossOrderAsync, parameters:{0},{1}", orderId, parentOrderID)
            Dim ret As Dictionary(Of String, Object) = Nothing
            Dim execCommand As ExecutionCommands = ExecutionCommands.CancelOrder
            _cts.Token.ThrowIfCancellationRequested()
            Dim tradeParameters As New Dictionary(Of String, Object) From {
                {"OrderId", orderId},
                {"ParentOrderId", parentOrderID},
                {"Variety", TypesOfVariety.REGULAR}
            }
            Dim tempAllRet As Dictionary(Of String, Object) = Nothing
            Try
                tempAllRet = Await ExecuteCommandAsync(execCommand, tradeParameters).ConfigureAwait(False)
            Catch tex As TokenException
                Throw New AliceBusinessException(tex.Message, tex, AdapterBusinessException.TypeOfException.TokenException)
            Catch oex As OrderException
                Throw New AliceBusinessException(oex.Message, oex, AdapterBusinessException.TypeOfException.OrderException)
            Catch uex As UnknownException
                Throw New AliceBusinessException(uex.Message, uex, AdapterBusinessException.TypeOfException.UnknownException)
            Catch ex As Exception
                Throw ex
            End Try
            _cts.Token.ThrowIfCancellationRequested()

            Dim tempRet As Object = Nothing
            If tempAllRet IsNot Nothing AndAlso tempAllRet.ContainsKey(execCommand.ToString) Then
                tempRet = tempAllRet(execCommand.ToString)
                If tempRet IsNot Nothing Then
                    Dim errorMessage As String = ParentController.GetErrorResponse(tempRet)
                    If errorMessage IsNot Nothing Then
                        Throw New ApplicationException(errorMessage)
                    End If
                Else
                    Throw New ApplicationException(String.Format("Alice command execution did not return anything, command:{0}", execCommand.ToString))
                End If
            Else
                Throw New ApplicationException(String.Format("Relevant command was fired but not detected in the response, command:{0}", execCommand.ToString))
            End If

            If tempRet.GetType = GetType(Dictionary(Of String, Object)) Then
                OnHeartbeat(String.Format("Cancel Order successful, details:{0}", Utils.JsonSerialize(tempRet)))
                ret = CType(tempRet, Dictionary(Of String, Object))
            Else
                Throw New ApplicationException(String.Format("Alice command execution did not return anything, command:{0}", execCommand.ToString))
            End If
            Return ret
        End Function
#End Region

#Region "Place BO"
        Public Overrides Async Function PlaceBOLimitMISOrderAsync(ByVal tradeExchange As String,
                                                                   ByVal tradingSymbol As String,
                                                                   ByVal transaction As IOrder.TypeOfTransaction,
                                                                   ByVal quantity As Integer,
                                                                   ByVal price As Decimal,
                                                                   ByVal squareOffValue As Decimal,
                                                                   ByVal stopLossValue As Decimal,
                                                                   ByVal tag As String) As Task(Of Dictionary(Of String, Object))
            Dim ret As Dictionary(Of String, Object) = Nothing
            If Me.ParentInstrument IsNot Nothing Then
                Dim execCommand As ExecutionCommands = ExecutionCommands.PlaceOrder
                _cts.Token.ThrowIfCancellationRequested()

                Dim transactionDirection As TypesOfTransaction = TypesOfTransaction.None
                Select Case transaction
                    Case IOrder.TypeOfTransaction.Buy
                        transactionDirection = TypesOfTransaction.BUY
                    Case IOrder.TypeOfTransaction.Sell
                        transactionDirection = TypesOfTransaction.SELL
                End Select
                Dim tradeParameters As New Dictionary(Of String, Object) From {
                {"Exchange", CType(Me.ParentInstrument, AliceInstrument).WrappedInstrument.ExchangeType},
                {"InstrumentToken", Me.ParentInstrument.InstrumentIdentifier},
                {"TransactionType", transactionDirection},
                {"Quantity", quantity},
                {"Price", price},
                {"Product", TypesOfProduct.MIS},
                {"OrderType", TypesOfOrder.LIMIT},
                {"Validity", ValidityOfOrder.DAY},
                {"TriggerPrice", Nothing},
                {"SquareOffValue", squareOffValue},
                {"StoplossValue", stopLossValue},
                {"Variety", TypesOfVariety.BO},
                {"Tag", tag}
            }
                Dim tempAllRet As Dictionary(Of String, Object) = Nothing
                Try
                    tempAllRet = Await ExecuteCommandAsync(execCommand, tradeParameters).ConfigureAwait(False)
                Catch tex As TokenException
                    Throw New AliceBusinessException(tex.Message, tex, AdapterBusinessException.TypeOfException.TokenException)
                Catch oex As OrderException
                    Throw New AliceBusinessException(oex.Message, oex, AdapterBusinessException.TypeOfException.OrderException)
                Catch uex As UnknownException
                    Throw New AliceBusinessException(uex.Message, uex, AdapterBusinessException.TypeOfException.UnknownException)
                Catch ex As Exception
                    Throw ex
                End Try
                _cts.Token.ThrowIfCancellationRequested()

                Dim tempRet As Object = Nothing
                If tempAllRet IsNot Nothing AndAlso tempAllRet.ContainsKey(execCommand.ToString) Then
                    tempRet = tempAllRet(execCommand.ToString)
                    If tempRet IsNot Nothing Then
                        Dim errorMessage As String = ParentController.GetErrorResponse(tempRet)
                        If errorMessage IsNot Nothing Then
                            Throw New ApplicationException(errorMessage)
                        End If
                    Else
                        Throw New ApplicationException(String.Format("Alice command execution did not return anything, command:{0}", execCommand.ToString))
                    End If
                Else
                    Throw New ApplicationException(String.Format("Relevant command was fired but not detected in the response, command:{0}", execCommand.ToString))
                End If

                If tempRet.GetType = GetType(Dictionary(Of String, Object)) Then
                    OnHeartbeat(String.Format("PlaceOrder successful, details:{0}", Utils.JsonSerialize(tempRet)))
                    If tempRet.ContainsKey("data") AndAlso tempRet("data").ContainsKey("oms_order_id") Then
                        Dim order As Dictionary(Of String, Object) = New Dictionary(Of String, Object) From {{"order_id", tempRet("data")("oms_order_id")}}
                        ret = New Dictionary(Of String, Object) From {
                            {"status", "success"},
                            {"data", order}
                        }
                    End If
                Else
                    Throw New ApplicationException(String.Format("Alice command execution did not return anything, command:{0}", execCommand.ToString))
                End If
            Else
                Throw New ApplicationException("Associated Instrument is not available")
            End If
            Return ret
        End Function
        Public Overrides Async Function PlaceBOSLMISOrderAsync(ByVal tradeExchange As String,
                                                                ByVal tradingSymbol As String,
                                                                ByVal transaction As IOrder.TypeOfTransaction,
                                                                ByVal quantity As Integer,
                                                                ByVal price As Decimal,
                                                                ByVal triggerPrice As Decimal,
                                                                ByVal squareOffValue As Decimal,
                                                                ByVal stopLossValue As Decimal,
                                                                ByVal tag As String) As Task(Of Dictionary(Of String, Object))
            Dim ret As Dictionary(Of String, Object) = Nothing
            If Me.ParentInstrument IsNot Nothing Then
                Dim execCommand As ExecutionCommands = ExecutionCommands.PlaceOrder
                _cts.Token.ThrowIfCancellationRequested()

                Dim transactionDirection As TypesOfTransaction = TypesOfTransaction.None
                Select Case transaction
                    Case IOrder.TypeOfTransaction.Buy
                        transactionDirection = TypesOfTransaction.BUY
                    Case IOrder.TypeOfTransaction.Sell
                        transactionDirection = TypesOfTransaction.SELL
                End Select
                Dim tradeParameters As New Dictionary(Of String, Object) From {
                    {"Exchange", CType(Me.ParentInstrument, AliceInstrument).WrappedInstrument.ExchangeType},
                    {"InstrumentToken", Me.ParentInstrument.InstrumentIdentifier},
                    {"TransactionType", transactionDirection},
                    {"Quantity", quantity},
                    {"Price", price},
                    {"Product", TypesOfProduct.MIS},
                    {"OrderType", TypesOfOrder.SL},
                    {"Validity", ValidityOfOrder.DAY},
                    {"TriggerPrice", triggerPrice},
                    {"SquareOffValue", squareOffValue},
                    {"StoplossValue", stopLossValue},
                    {"Variety", TypesOfVariety.BO},
                    {"Tag", tag}
                }
                Dim tempAllRet As Dictionary(Of String, Object) = Nothing
                Try
                    tempAllRet = Await ExecuteCommandAsync(execCommand, tradeParameters).ConfigureAwait(False)
                Catch tex As TokenException
                    Throw New AliceBusinessException(tex.Message, tex, AdapterBusinessException.TypeOfException.TokenException)
                Catch oex As OrderException
                    Throw New AliceBusinessException(oex.Message, oex, AdapterBusinessException.TypeOfException.OrderException)
                Catch uex As UnknownException
                    Throw New AliceBusinessException(uex.Message, uex, AdapterBusinessException.TypeOfException.UnknownException)
                Catch ex As Exception
                    Throw ex
                End Try
                _cts.Token.ThrowIfCancellationRequested()

                Dim tempRet As Object = Nothing
                If tempAllRet IsNot Nothing AndAlso tempAllRet.ContainsKey(execCommand.ToString) Then
                    tempRet = tempAllRet(execCommand.ToString)
                    If tempRet IsNot Nothing Then
                        Dim errorMessage As String = ParentController.GetErrorResponse(tempRet)
                        If errorMessage IsNot Nothing Then
                            Throw New ApplicationException(errorMessage)
                        End If
                    Else
                        Throw New ApplicationException(String.Format("Alice command execution did not return anything, command:{0}", execCommand.ToString))
                    End If
                Else
                    Throw New ApplicationException(String.Format("Relevant command was fired but not detected in the response, command:{0}", execCommand.ToString))
                End If

                If tempRet.GetType = GetType(Dictionary(Of String, Object)) Then
                    OnHeartbeat(String.Format("PlaceOrder successful, details:{0}", Utils.JsonSerialize(tempRet)))
                    If tempRet.ContainsKey("data") AndAlso tempRet("data").ContainsKey("oms_order_id") Then
                        Dim order As Dictionary(Of String, Object) = New Dictionary(Of String, Object) From {{"order_id", tempRet("data")("oms_order_id")}}
                        ret = New Dictionary(Of String, Object) From {
                            {"status", "success"},
                            {"data", order}
                        }
                    End If
                Else
                    Throw New ApplicationException(String.Format("Alice command execution did not return anything, command:{0}", execCommand.ToString))
                End If
            Else
                Throw New ApplicationException("Associated Instrument is not available")
            End If
            Return ret
        End Function
#End Region

#Region "Place CO"
        Public Overrides Async Function PlaceCOMarketMISOrderAsync(ByVal tradeExchange As String,
                                                                   ByVal tradingSymbol As String,
                                                                   ByVal transaction As IOrder.TypeOfTransaction,
                                                                   ByVal quantity As Integer,
                                                                   ByVal triggerPrice As Decimal,
                                                                   ByVal tag As String) As Task(Of Dictionary(Of String, Object))
            Dim ret As Dictionary(Of String, Object) = Nothing
            If Me.ParentInstrument IsNot Nothing Then
                Dim execCommand As ExecutionCommands = ExecutionCommands.PlaceOrder
                _cts.Token.ThrowIfCancellationRequested()

                Dim transactionDirection As TypesOfTransaction = TypesOfTransaction.None
                Select Case transaction
                    Case IOrder.TypeOfTransaction.Buy
                        transactionDirection = TypesOfTransaction.BUY
                    Case IOrder.TypeOfTransaction.Sell
                        transactionDirection = TypesOfTransaction.SELL
                End Select
                Dim tradeParameters As New Dictionary(Of String, Object) From {
                    {"Exchange", CType(Me.ParentInstrument, AliceInstrument).WrappedInstrument.ExchangeType},
                    {"InstrumentToken", Me.ParentInstrument.InstrumentIdentifier},
                    {"TransactionType", transactionDirection},
                    {"Quantity", quantity},
                    {"Price", Nothing},
                    {"Product", TypesOfProduct.MIS},
                    {"OrderType", TypesOfOrder.MARKET},
                    {"Validity", ValidityOfOrder.DAY},
                    {"TriggerPrice", triggerPrice},
                    {"SquareOffValue", Nothing},
                    {"StoplossValue", Nothing},
                    {"Variety", TypesOfVariety.CO},
                    {"Tag", tag}
                }
                Dim tempAllRet As Dictionary(Of String, Object) = Nothing
                Try
                    tempAllRet = Await ExecuteCommandAsync(execCommand, tradeParameters).ConfigureAwait(False)
                Catch tex As TokenException
                    Throw New AliceBusinessException(tex.Message, tex, AdapterBusinessException.TypeOfException.TokenException)
                Catch oex As OrderException
                    Throw New AliceBusinessException(oex.Message, oex, AdapterBusinessException.TypeOfException.OrderException)
                Catch uex As UnknownException
                    Throw New AliceBusinessException(uex.Message, uex, AdapterBusinessException.TypeOfException.UnknownException)
                Catch ex As Exception
                    Throw ex
                End Try
                _cts.Token.ThrowIfCancellationRequested()

                Dim tempRet As Object = Nothing
                If tempAllRet IsNot Nothing AndAlso tempAllRet.ContainsKey(execCommand.ToString) Then
                    tempRet = tempAllRet(execCommand.ToString)
                    If tempRet IsNot Nothing Then
                        Dim errorMessage As String = ParentController.GetErrorResponse(tempRet)
                        If errorMessage IsNot Nothing Then
                            Throw New ApplicationException(errorMessage)
                        End If
                    Else
                        Throw New ApplicationException(String.Format("Alice command execution did not return anything, command:{0}", execCommand.ToString))
                    End If
                Else
                    Throw New ApplicationException(String.Format("Relevant command was fired but not detected in the response, command:{0}", execCommand.ToString))
                End If

                If tempRet.GetType = GetType(Dictionary(Of String, Object)) Then
                    OnHeartbeat(String.Format("PlaceOrder successful, details:{0}", Utils.JsonSerialize(tempRet)))
                    If tempRet.ContainsKey("data") AndAlso tempRet("data").ContainsKey("oms_order_id") Then
                        Dim order As Dictionary(Of String, Object) = New Dictionary(Of String, Object) From {{"order_id", tempRet("data")("oms_order_id")}}
                        ret = New Dictionary(Of String, Object) From {
                            {"status", "success"},
                            {"data", order}
                        }
                    End If
                Else
                    Throw New ApplicationException(String.Format("Alice command execution did not return anything, command:{0}", execCommand.ToString))
                End If
            Else
                Throw New ApplicationException("Associated Instrument is not available")
            End If
            Return ret
        End Function
#End Region

#Region "Place Regular MIS"
        Public Overrides Async Function PlaceRegularMarketMISOrderAsync(ByVal tradeExchange As String,
                                                                         ByVal tradingSymbol As String,
                                                                         ByVal transaction As IOrder.TypeOfTransaction,
                                                                         ByVal quantity As Integer,
                                                                         ByVal tag As String) As Task(Of Dictionary(Of String, Object))
            Dim ret As Dictionary(Of String, Object) = Nothing
            If Me.ParentInstrument IsNot Nothing Then
                Dim execCommand As ExecutionCommands = ExecutionCommands.PlaceOrder
                _cts.Token.ThrowIfCancellationRequested()

                Dim transactionDirection As TypesOfTransaction = TypesOfTransaction.None
                Select Case transaction
                    Case IOrder.TypeOfTransaction.Buy
                        transactionDirection = TypesOfTransaction.BUY
                    Case IOrder.TypeOfTransaction.Sell
                        transactionDirection = TypesOfTransaction.SELL
                End Select
                Dim tradeParameters As New Dictionary(Of String, Object) From {
                    {"Exchange", CType(Me.ParentInstrument, AliceInstrument).WrappedInstrument.ExchangeType},
                    {"InstrumentToken", Me.ParentInstrument.InstrumentIdentifier},
                    {"TransactionType", transactionDirection},
                    {"Quantity", quantity},
                    {"Price", Nothing},
                    {"Product", TypesOfProduct.MIS},
                    {"OrderType", TypesOfOrder.MARKET},
                    {"Validity", ValidityOfOrder.DAY},
                    {"TriggerPrice", Nothing},
                    {"SquareOffValue", Nothing},
                    {"StoplossValue", Nothing},
                    {"Variety", TypesOfVariety.REGULAR},
                    {"Tag", tag}
                }
                Dim tempAllRet As Dictionary(Of String, Object) = Nothing
                Try
                    tempAllRet = Await ExecuteCommandAsync(execCommand, tradeParameters).ConfigureAwait(False)
                Catch tex As TokenException
                    Throw New AliceBusinessException(tex.Message, tex, AdapterBusinessException.TypeOfException.TokenException)
                Catch oex As OrderException
                    Throw New AliceBusinessException(oex.Message, oex, AdapterBusinessException.TypeOfException.OrderException)
                Catch uex As UnknownException
                    Throw New AliceBusinessException(uex.Message, uex, AdapterBusinessException.TypeOfException.UnknownException)
                Catch ex As Exception
                    Throw ex
                End Try
                _cts.Token.ThrowIfCancellationRequested()

                Dim tempRet As Object = Nothing
                If tempAllRet IsNot Nothing AndAlso tempAllRet.ContainsKey(execCommand.ToString) Then
                    tempRet = tempAllRet(execCommand.ToString)
                    If tempRet IsNot Nothing Then
                        Dim errorMessage As String = ParentController.GetErrorResponse(tempRet)
                        If errorMessage IsNot Nothing Then
                            Throw New ApplicationException(errorMessage)
                        End If
                    Else
                        Throw New ApplicationException(String.Format("Alice command execution did not return anything, command:{0}", execCommand.ToString))
                    End If
                Else
                    Throw New ApplicationException(String.Format("Relevant command was fired but not detected in the response, command:{0}", execCommand.ToString))
                End If

                If tempRet.GetType = GetType(Dictionary(Of String, Object)) Then
                    OnHeartbeat(String.Format("PlaceOrder successful, details:{0}", Utils.JsonSerialize(tempRet)))
                    If tempRet.ContainsKey("data") AndAlso tempRet("data").ContainsKey("oms_order_id") Then
                        Dim order As Dictionary(Of String, Object) = New Dictionary(Of String, Object) From {{"order_id", tempRet("data")("oms_order_id")}}
                        ret = New Dictionary(Of String, Object) From {
                            {"status", "success"},
                            {"data", order}
                        }
                    End If
                Else
                    Throw New ApplicationException(String.Format("Alice command execution did not return anything, command:{0}", execCommand.ToString))
                End If
            Else
                Throw New ApplicationException("Associated Instrument is not available")
            End If
            Return ret
        End Function
        Public Overrides Async Function PlaceRegularLimitMISOrderAsync(ByVal tradeExchange As String,
                                                                    ByVal tradingSymbol As String,
                                                                    ByVal transaction As IOrder.TypeOfTransaction,
                                                                    ByVal quantity As Integer,
                                                                    ByVal price As Decimal,
                                                                    ByVal tag As String) As Task(Of Dictionary(Of String, Object))
            Dim ret As Dictionary(Of String, Object) = Nothing
            If Me.ParentInstrument IsNot Nothing Then
                Dim execCommand As ExecutionCommands = ExecutionCommands.PlaceOrder
                _cts.Token.ThrowIfCancellationRequested()

                Dim transactionDirection As TypesOfTransaction = TypesOfTransaction.None
                Select Case transaction
                    Case IOrder.TypeOfTransaction.Buy
                        transactionDirection = TypesOfTransaction.BUY
                    Case IOrder.TypeOfTransaction.Sell
                        transactionDirection = TypesOfTransaction.SELL
                End Select
                Dim tradeParameters As New Dictionary(Of String, Object) From {
                    {"Exchange", CType(Me.ParentInstrument, AliceInstrument).WrappedInstrument.ExchangeType},
                    {"InstrumentToken", Me.ParentInstrument.InstrumentIdentifier},
                    {"TransactionType", transactionDirection},
                    {"Quantity", quantity},
                    {"Price", price},
                    {"Product", TypesOfProduct.MIS},
                    {"OrderType", TypesOfOrder.LIMIT},
                    {"Validity", ValidityOfOrder.DAY},
                    {"TriggerPrice", Nothing},
                    {"SquareOffValue", Nothing},
                    {"StoplossValue", Nothing},
                    {"Variety", TypesOfVariety.REGULAR},
                    {"Tag", tag}
                }
                Dim tempAllRet As Dictionary(Of String, Object) = Nothing
                Try
                    tempAllRet = Await ExecuteCommandAsync(execCommand, tradeParameters).ConfigureAwait(False)
                Catch tex As TokenException
                    Throw New AliceBusinessException(tex.Message, tex, AdapterBusinessException.TypeOfException.TokenException)
                Catch oex As OrderException
                    Throw New AliceBusinessException(oex.Message, oex, AdapterBusinessException.TypeOfException.OrderException)
                Catch uex As UnknownException
                    Throw New AliceBusinessException(uex.Message, uex, AdapterBusinessException.TypeOfException.UnknownException)
                Catch ex As Exception
                    Throw ex
                End Try
                _cts.Token.ThrowIfCancellationRequested()

                Dim tempRet As Object = Nothing
                If tempAllRet IsNot Nothing AndAlso tempAllRet.ContainsKey(execCommand.ToString) Then
                    tempRet = tempAllRet(execCommand.ToString)
                    If tempRet IsNot Nothing Then
                        Dim errorMessage As String = ParentController.GetErrorResponse(tempRet)
                        If errorMessage IsNot Nothing Then
                            Throw New ApplicationException(errorMessage)
                        End If
                    Else
                        Throw New ApplicationException(String.Format("Alice command execution did not return anything, command:{0}", execCommand.ToString))
                    End If
                Else
                    Throw New ApplicationException(String.Format("Relevant command was fired but not detected in the response, command:{0}", execCommand.ToString))
                End If

                If tempRet.GetType = GetType(Dictionary(Of String, Object)) Then
                    OnHeartbeat(String.Format("PlaceOrder successful, details:{0}", Utils.JsonSerialize(tempRet)))
                    If tempRet.ContainsKey("data") AndAlso tempRet("data").ContainsKey("oms_order_id") Then
                        Dim order As Dictionary(Of String, Object) = New Dictionary(Of String, Object) From {{"order_id", tempRet("data")("oms_order_id")}}
                        ret = New Dictionary(Of String, Object) From {
                            {"status", "success"},
                            {"data", order}
                        }
                    End If
                Else
                    Throw New ApplicationException(String.Format("Alice command execution did not return anything, command:{0}", execCommand.ToString))
                End If
            Else
                Throw New ApplicationException("Associated Instrument is not available")
            End If
            Return ret
        End Function
        Public Overrides Async Function PlaceRegularSLMMISOrderAsync(ByVal tradeExchange As String,
                                                                  ByVal tradingSymbol As String,
                                                                  ByVal transaction As IOrder.TypeOfTransaction,
                                                                  ByVal quantity As Integer,
                                                                  ByVal triggerPrice As Decimal,
                                                                  ByVal tag As String) As Task(Of Dictionary(Of String, Object))
            Dim ret As Dictionary(Of String, Object) = Nothing
            If Me.ParentInstrument IsNot Nothing Then
                Dim execCommand As ExecutionCommands = ExecutionCommands.PlaceOrder
                _cts.Token.ThrowIfCancellationRequested()

                Dim transactionDirection As TypesOfTransaction = TypesOfTransaction.None
                Select Case transaction
                    Case IOrder.TypeOfTransaction.Buy
                        transactionDirection = TypesOfTransaction.BUY
                    Case IOrder.TypeOfTransaction.Sell
                        transactionDirection = TypesOfTransaction.SELL
                End Select
                Dim tradeParameters As New Dictionary(Of String, Object) From {
                    {"Exchange", CType(Me.ParentInstrument, AliceInstrument).WrappedInstrument.ExchangeType},
                    {"InstrumentToken", Me.ParentInstrument.InstrumentIdentifier},
                    {"TransactionType", transactionDirection},
                    {"Quantity", quantity},
                    {"Price", Nothing},
                    {"Product", TypesOfProduct.MIS},
                    {"OrderType", TypesOfOrder.SLM},
                    {"Validity", ValidityOfOrder.DAY},
                    {"TriggerPrice", triggerPrice},
                    {"SquareOffValue", Nothing},
                    {"StoplossValue", Nothing},
                    {"Variety", TypesOfVariety.REGULAR},
                    {"Tag", tag}
                }
                Dim tempAllRet As Dictionary(Of String, Object) = Nothing
                Try
                    tempAllRet = Await ExecuteCommandAsync(execCommand, tradeParameters).ConfigureAwait(False)
                Catch tex As TokenException
                    Throw New AliceBusinessException(tex.Message, tex, AdapterBusinessException.TypeOfException.TokenException)
                Catch oex As OrderException
                    Throw New AliceBusinessException(oex.Message, oex, AdapterBusinessException.TypeOfException.OrderException)
                Catch uex As UnknownException
                    Throw New AliceBusinessException(uex.Message, uex, AdapterBusinessException.TypeOfException.UnknownException)
                Catch ex As Exception
                    Throw ex
                End Try
                _cts.Token.ThrowIfCancellationRequested()

                Dim tempRet As Object = Nothing
                If tempAllRet IsNot Nothing AndAlso tempAllRet.ContainsKey(execCommand.ToString) Then
                    tempRet = tempAllRet(execCommand.ToString)
                    If tempRet IsNot Nothing Then
                        Dim errorMessage As String = ParentController.GetErrorResponse(tempRet)
                        If errorMessage IsNot Nothing Then
                            Throw New ApplicationException(errorMessage)
                        End If
                    Else
                        Throw New ApplicationException(String.Format("Alice command execution did not return anything, command:{0}", execCommand.ToString))
                    End If
                Else
                    Throw New ApplicationException(String.Format("Relevant command was fired but not detected in the response, command:{0}", execCommand.ToString))
                End If

                If tempRet.GetType = GetType(Dictionary(Of String, Object)) Then
                    OnHeartbeat(String.Format("PlaceOrder successful, details:{0}", Utils.JsonSerialize(tempRet)))
                    If tempRet.ContainsKey("data") AndAlso tempRet("data").ContainsKey("oms_order_id") Then
                        Dim order As Dictionary(Of String, Object) = New Dictionary(Of String, Object) From {{"order_id", tempRet("data")("oms_order_id")}}
                        ret = New Dictionary(Of String, Object) From {
                            {"status", "success"},
                            {"data", order}
                        }
                    End If
                Else
                    Throw New ApplicationException(String.Format("Alice command execution did not return anything, command:{0}", execCommand.ToString))
                End If
            Else
                Throw New ApplicationException("Associated Instrument is not available")
            End If
            Return ret
        End Function
#End Region

#Region "Place Regular CNC"
        Public Overrides Async Function PlaceRegularMarketCNCOrderAsync(ByVal tradeExchange As String,
                                                                         ByVal tradingSymbol As String,
                                                                         ByVal transaction As IOrder.TypeOfTransaction,
                                                                         ByVal quantity As Integer,
                                                                         ByVal tag As String) As Task(Of Dictionary(Of String, Object))
            Dim ret As Dictionary(Of String, Object) = Nothing
            If Me.ParentInstrument IsNot Nothing Then
                Dim execCommand As ExecutionCommands = ExecutionCommands.PlaceOrder
                _cts.Token.ThrowIfCancellationRequested()

                Dim transactionDirection As TypesOfTransaction = TypesOfTransaction.None
                Select Case transaction
                    Case IOrder.TypeOfTransaction.Buy
                        transactionDirection = TypesOfTransaction.BUY
                    Case IOrder.TypeOfTransaction.Sell
                        transactionDirection = TypesOfTransaction.SELL
                End Select
                Dim tradeParameters As New Dictionary(Of String, Object) From {
                    {"Exchange", CType(Me.ParentInstrument, AliceInstrument).WrappedInstrument.ExchangeType},
                    {"InstrumentToken", Me.ParentInstrument.InstrumentIdentifier},
                    {"TransactionType", transactionDirection},
                    {"Quantity", quantity},
                    {"Price", Nothing},
                    {"Product", TypesOfProduct.CNC},
                    {"OrderType", TypesOfOrder.MARKET},
                    {"Validity", ValidityOfOrder.DAY},
                    {"TriggerPrice", Nothing},
                    {"SquareOffValue", Nothing},
                    {"StoplossValue", Nothing},
                    {"Variety", TypesOfVariety.REGULAR},
                    {"Tag", tag}
                }
                Dim tempAllRet As Dictionary(Of String, Object) = Nothing
                Try
                    tempAllRet = Await ExecuteCommandAsync(execCommand, tradeParameters).ConfigureAwait(False)
                Catch tex As TokenException
                    Throw New AliceBusinessException(tex.Message, tex, AdapterBusinessException.TypeOfException.TokenException)
                Catch oex As OrderException
                    Throw New AliceBusinessException(oex.Message, oex, AdapterBusinessException.TypeOfException.OrderException)
                Catch uex As UnknownException
                    Throw New AliceBusinessException(uex.Message, uex, AdapterBusinessException.TypeOfException.UnknownException)
                Catch ex As Exception
                    Throw ex
                End Try
                _cts.Token.ThrowIfCancellationRequested()

                Dim tempRet As Object = Nothing
                If tempAllRet IsNot Nothing AndAlso tempAllRet.ContainsKey(execCommand.ToString) Then
                    tempRet = tempAllRet(execCommand.ToString)
                    If tempRet IsNot Nothing Then
                        Dim errorMessage As String = ParentController.GetErrorResponse(tempRet)
                        If errorMessage IsNot Nothing Then
                            Throw New ApplicationException(errorMessage)
                        End If
                    Else
                        Throw New ApplicationException(String.Format("Alice command execution did not return anything, command:{0}", execCommand.ToString))
                    End If
                Else
                    Throw New ApplicationException(String.Format("Relevant command was fired but not detected in the response, command:{0}", execCommand.ToString))
                End If

                If tempRet.GetType = GetType(Dictionary(Of String, Object)) Then
                    OnHeartbeat(String.Format("PlaceOrder successful, details:{0}", Utils.JsonSerialize(tempRet)))
                    If tempRet.ContainsKey("data") AndAlso tempRet("data").ContainsKey("oms_order_id") Then
                        Dim order As Dictionary(Of String, Object) = New Dictionary(Of String, Object) From {{"order_id", tempRet("data")("oms_order_id")}}
                        ret = New Dictionary(Of String, Object) From {
                            {"status", "success"},
                            {"data", order}
                        }
                    End If
                Else
                    Throw New ApplicationException(String.Format("Alice command execution did not return anything, command:{0}", execCommand.ToString))
                End If
            Else
                Throw New ApplicationException("Associated Instrument is not available")
            End If
            Return ret
        End Function
#End Region


#Region "Alice Commands"
        Private Async Function ExecuteCommandAsync(ByVal command As ExecutionCommands, ByVal stockData As Dictionary(Of String, Object)) As Task(Of Dictionary(Of String, Object))
            If command <> ExecutionCommands.GetOrders AndAlso command <> ExecutionCommands.GetQuotes AndAlso command <> ExecutionCommands.GetPositions Then
                logger.Debug("ExecuteCommandAsync, command:{0}, stockData:{1}", command.ToString, Utils.JsonSerialize(stockData))
            End If
            _cts.Token.ThrowIfCancellationRequested()
            Dim ret As Dictionary(Of String, Object) = Nothing

            Dim lastException As Exception = Nothing
            If command <> ExecutionCommands.GetOrders AndAlso command <> ExecutionCommands.GetPositions Then
                logger.Debug(String.Format("Firing Alice command to complete desired action, command:{0}", command.ToString))
            End If
            Select Case command
                Case ExecutionCommands.GetQuotes
                    'Dim getQuotesResponse As Dictionary(Of String, Quote) = Nothing
                    'If stockData IsNot Nothing AndAlso stockData.ContainsKey("instruments") Then
                    '    Dim index As Integer = -1
                    '    Dim subscriptionList() As String = Nothing
                    '    For Each runningInstruments As IInstrument In stockData("instruments")
                    '        _cts.Token.ThrowIfCancellationRequested()
                    '        index += 1
                    '        If index = 0 Then
                    '            ReDim subscriptionList(0)
                    '        Else
                    '            ReDim Preserve subscriptionList(UBound(subscriptionList) + 1)
                    '        End If
                    '        subscriptionList(index) = runningInstruments.InstrumentIdentifier
                    '    Next
                    '    If subscriptionList IsNot Nothing AndAlso subscriptionList.Length > 0 Then

                    '        getQuotesResponse = Await Task.Factory.StartNew(Function()
                    '                                                            Try
                    '                                                                Return _Alice.GetQuote(subscriptionList)
                    '                                                            Catch ex As Exception
                    '                                                                logger.Error(ex)
                    '                                                                lastException = ex
                    '                                                                Return Nothing
                    '                                                            End Try
                    '                                                        End Function).ConfigureAwait(False)
                    '        _cts.Token.ThrowIfCancellationRequested()
                    '        ret = New Dictionary(Of String, Object) From {{command.ToString, getQuotesResponse}}
                    '    End If
                    'End If
                    Throw New NotImplementedException()
                    lastException = Nothing
                Case ExecutionCommands.GetPositions
                    Dim positions As PositionResponse = Nothing
                    positions = Await Task.Factory.StartNew(Function()
                                                                Try
                                                                    Return _Alice.GetPositions()
                                                                Catch ex As Exception
                                                                    logger.Error(ex)
                                                                    lastException = ex
                                                                    Return Nothing
                                                                End Try
                                                            End Function).ConfigureAwait(False)
                    _cts.Token.ThrowIfCancellationRequested()
                    ret = New Dictionary(Of String, Object) From {{command.ToString, positions}}
                Case ExecutionCommands.GetHoldings
                    Dim holdings As List(Of Holding) = Nothing
                    holdings = Await Task.Factory.StartNew(Function()
                                                               Try
                                                                   Return _Alice.GetHoldings()
                                                               Catch ex As Exception
                                                                   logger.Error(ex)
                                                                   lastException = ex
                                                                   Return Nothing
                                                               End Try
                                                           End Function).ConfigureAwait(False)
                    _cts.Token.ThrowIfCancellationRequested()
                    ret = New Dictionary(Of String, Object) From {{command.ToString, holdings}}
                Case ExecutionCommands.PlaceOrder
                    Dim placedOrders As Dictionary(Of String, Object) = Nothing
                    If stockData IsNot Nothing AndAlso stockData.Count > 0 Then
                        placedOrders = Await Task.Factory.StartNew(Function()
                                                                       Try
                                                                           Return _Alice.PlaceOrder(Exchange:=stockData("Exchange"),
                                                                                                    InstrumentToken:=stockData("InstrumentToken"),
                                                                                                    TransactionType:=stockData("TransactionType"),
                                                                                                    Quantity:=stockData("Quantity"),
                                                                                                    Price:=stockData("Price"),
                                                                                                    Product:=stockData("Product"),
                                                                                                    OrderType:=stockData("OrderType"),
                                                                                                    Validity:=stockData("Validity"),
                                                                                                    TriggerPrice:=stockData("TriggerPrice"),
                                                                                                    SquareOffValue:=stockData("SquareOffValue"),
                                                                                                    StoplossValue:=stockData("StoplossValue"),
                                                                                                    Variety:=stockData("Variety"),
                                                                                                    Tag:=stockData("Tag"))
                                                                       Catch ex As Exception
                                                                           logger.Error(ex)
                                                                           lastException = ex
                                                                           Return Nothing
                                                                       End Try
                                                                   End Function).ConfigureAwait(False)
                        _cts.Token.ThrowIfCancellationRequested()
                    End If
                    ret = New Dictionary(Of String, Object) From {{command.ToString, placedOrders}}
                Case ExecutionCommands.ModifyOrderQuantity
                    Dim modifiedOrdersQuantity As Dictionary(Of String, Object) = Nothing
                    If stockData IsNot Nothing AndAlso stockData.Count > 0 Then
                        modifiedOrdersQuantity = Await Task.Factory.StartNew(Function()
                                                                                 Try
                                                                                     Return _Alice.ModifyOrder(OrderId:=CType(stockData("OrderId"), String),
                                                                                                              Quantity:=CType(stockData("Quantity"), String))
                                                                                 Catch oex As OrderException
                                                                                     logger.Error(oex)
                                                                                     Return New Dictionary(Of String, Object) From {{"status", "success"}, {"message", oex.Message}}
                                                                                 Catch ex As Exception
                                                                                     logger.Error(ex)
                                                                                     lastException = ex
                                                                                     Return Nothing
                                                                                 End Try
                                                                             End Function).ConfigureAwait(False)
                        _cts.Token.ThrowIfCancellationRequested()
                    End If
                    ret = New Dictionary(Of String, Object) From {{command.ToString, modifiedOrdersQuantity}}
                Case ExecutionCommands.ModifyTargetOrderPrice, ExecutionCommands.ModifyOrderPrice
                    Dim modifiedOrdersPrice As Dictionary(Of String, Object) = Nothing
                    If stockData IsNot Nothing AndAlso stockData.Count > 0 Then
                        modifiedOrdersPrice = Await Task.Factory.StartNew(Function()
                                                                              Try
                                                                                  Return _Alice.ModifyOrder(OrderId:=CType(stockData("OrderId"), String),
                                                                                                           Price:=CType(stockData("Price"), Decimal))
                                                                              Catch oex As OrderException
                                                                                  logger.Error(oex)
                                                                                  Return New Dictionary(Of String, Object) From {{"status", "success"}, {"message", oex.Message}}
                                                                              Catch ex As Exception
                                                                                  logger.Error(ex)
                                                                                  lastException = ex
                                                                                  Return Nothing
                                                                              End Try
                                                                          End Function).ConfigureAwait(False)
                        _cts.Token.ThrowIfCancellationRequested()
                    End If
                    ret = New Dictionary(Of String, Object) From {{command.ToString, modifiedOrdersPrice}}
                Case ExecutionCommands.ModifySLOrderPrice
                    Dim modifiedOrdersPrice As Dictionary(Of String, Object) = Nothing
                    If stockData IsNot Nothing AndAlso stockData.Count > 0 Then
                        modifiedOrdersPrice = Await Task.Factory.StartNew(Function()
                                                                              Try
                                                                                  Return _Alice.ModifyOrder(OrderId:=CType(stockData("OrderId"), String),
                                                                                                           TriggerPrice:=CType(stockData("TriggerPrice"), Decimal))
                                                                              Catch oex As OrderException
                                                                                  logger.Error(oex)
                                                                                  Return New Dictionary(Of String, Object) From {{"status", "success"}, {"message", oex.Message}}
                                                                              Catch ex As Exception
                                                                                  logger.Error(ex)
                                                                                  lastException = ex
                                                                                  Return Nothing
                                                                              End Try
                                                                          End Function).ConfigureAwait(False)
                        _cts.Token.ThrowIfCancellationRequested()
                    End If
                    ret = New Dictionary(Of String, Object) From {{command.ToString, modifiedOrdersPrice}}
                Case ExecutionCommands.CancelOrder
                    Dim cancelledOrder As Dictionary(Of String, Object) = Nothing
                    If stockData IsNot Nothing AndAlso stockData.Count > 0 Then
                        cancelledOrder = Await Task.Factory.StartNew(Function()
                                                                         Try
                                                                             Return _Alice.CancelOrder(OrderId:=CType(stockData("OrderId"), String),
                                                                                                       ParentOrderId:=CType(stockData("ParentOrderId"), String),
                                                                                                       Variety:=CType(stockData("Variety"), String))
                                                                         Catch oex As OrderException
                                                                             logger.Error(oex)
                                                                             Return New Dictionary(Of String, Object) From {{"status", "success"}, {"message", oex.Message}}
                                                                         Catch ex As Exception
                                                                             logger.Error(ex)
                                                                             lastException = ex
                                                                             Return Nothing
                                                                         End Try
                                                                     End Function).ConfigureAwait(False)
                        _cts.Token.ThrowIfCancellationRequested()
                    End If
                    ret = New Dictionary(Of String, Object) From {{command.ToString, cancelledOrder}}
                Case ExecutionCommands.GetOrderHistory
                    Dim orderList As List(Of Order) = Nothing
                    If stockData IsNot Nothing AndAlso stockData.Count > 0 Then
                        orderList = Await Task.Factory.StartNew(Function()
                                                                    Try
                                                                        Return _Alice.GetOrderHistory(OrderId:=CType(stockData("OrderId"), String))
                                                                    Catch ex As Exception
                                                                        logger.Error(ex)
                                                                        lastException = ex
                                                                        Return Nothing
                                                                    End Try
                                                                End Function).ConfigureAwait(False)
                        _cts.Token.ThrowIfCancellationRequested()
                    End If
                    ret = New Dictionary(Of String, Object) From {{command.ToString, orderList}}
                Case ExecutionCommands.GetOrders
                    Dim orderList As List(Of Order) = Nothing
                    orderList = Await Task.Factory.StartNew(Function()
                                                                Try
                                                                    Return _Alice.GetOrders()
                                                                Catch ex As Exception
                                                                    logger.Error(ex)
                                                                    lastException = ex
                                                                    Return Nothing
                                                                End Try
                                                            End Function).ConfigureAwait(False)
                    _cts.Token.ThrowIfCancellationRequested()
                    ret = New Dictionary(Of String, Object) From {{command.ToString, orderList}}
                Case ExecutionCommands.GetOrderTrades
                    Dim tradeList As List(Of Trade) = Nothing
                    If stockData IsNot Nothing AndAlso stockData.Count > 0 Then
                        Dim orderId As String = stockData("OrderId")
                        tradeList = Await Task.Factory.StartNew(Function()
                                                                    Try
                                                                        Return _Alice.GetOrderTrades()
                                                                    Catch ex As Exception
                                                                        logger.Error(ex)
                                                                        lastException = ex
                                                                        Return Nothing
                                                                    End Try
                                                                End Function).ConfigureAwait(False)
                        If tradeList IsNot Nothing Then
                            Dim modifiedTradeList As List(Of Trade) = tradeList.FindAll(Function(x)
                                                                                            Return x.OrderId = orderId
                                                                                        End Function)
                            tradeList = modifiedTradeList
                        End If
                        _cts.Token.ThrowIfCancellationRequested()
                    Else
                        tradeList = Await Task.Factory.StartNew(Function()
                                                                    Try
                                                                        Return _Alice.GetOrderTrades()
                                                                    Catch ex As Exception
                                                                        logger.Error(ex)
                                                                        lastException = ex
                                                                        Return Nothing
                                                                    End Try
                                                                End Function).ConfigureAwait(False)
                        _cts.Token.ThrowIfCancellationRequested()
                    End If
                    ret = New Dictionary(Of String, Object) From {{command.ToString, tradeList}}
                Case ExecutionCommands.GetInstruments
                    Dim instruments As List(Of Instrument) = Nothing
                    instruments = Await Task.Factory.StartNew(Function()
                                                                  Try
                                                                      Return _Alice.GetInstruments()
                                                                  Catch ex As Exception
                                                                      logger.Error(ex)
                                                                      lastException = ex
                                                                      Return Nothing
                                                                  End Try
                                                              End Function).ConfigureAwait(False)
                    _cts.Token.ThrowIfCancellationRequested()
                    Dim count As Integer = If(instruments Is Nothing, 0, instruments.Count)
                    logger.Debug(String.Format("Fetched {0} instruments from Alice", count))
                    If instruments IsNot Nothing AndAlso instruments.Count > 0 Then
                        instruments.RemoveAll(Function(x)
                                                  Return x.Exchange = "BFO" Or x.Exchange = "BSE" Or x.Exchange = "BCD"
                                              End Function)
                        instruments.RemoveAll(Function(x)
                                                  Dim pattern As String = "([0-9][0-9]JAN)|([0-9][0-9]FEB)|([0-9][0-9]MAR)|([0-9][0-9]APR)|([0-9][0-9]MAY)|([0-9][0-9]JUN)|([0-9][0-9]JUL)|([0-9][0-9]AUG)|([0-9][0-9]SEP)|([0-9][0-9]OCT)|([0-9][0-9]NOV)|([0-9][0-9]DEC)"
                                                  If Regex.Matches(x.TradingSymbol, pattern).Count >= 2 Then
                                                      Console.WriteLine(x.TradingSymbol)
                                                  End If
                                                  Return Regex.Matches(x.TradingSymbol, pattern).Count >= 2
                                              End Function)
                        'instruments.RemoveAll(Function(x)
                        '                          Return x.Segment.EndsWith("OPT")
                        '                      End Function)
                        instruments.RemoveAll(Function(x)
                                                  Return x.TradingSymbol.Length > 3 AndAlso x.TradingSymbol.Substring(x.TradingSymbol.Length - 3).StartsWith("-")
                                              End Function)
                        count = If(instruments Is Nothing, 0, instruments.Count)
                        logger.Debug(String.Format("After cleanup, fetched {0} instruments from Alice", count))
                    End If
                    _cts.Token.ThrowIfCancellationRequested()
                    ret = New Dictionary(Of String, Object) From {{command.ToString, instruments}}
                Case ExecutionCommands.GetUserMargins
                    Dim userMargins As UserMarginsResponse = Nothing
                    userMargins = Await Task.Factory.StartNew(Function()
                                                                  Try
                                                                      Return _Alice.GetMargins()
                                                                  Catch ex As Exception
                                                                      logger.Error(ex)
                                                                      lastException = ex
                                                                      Return Nothing
                                                                  End Try
                                                              End Function).ConfigureAwait(False)
                    _cts.Token.ThrowIfCancellationRequested()
                    ret = New Dictionary(Of String, Object) From {{command.ToString, userMargins}}
                Case ExecutionCommands.InvalidateAccessToken
                    'Dim invalidateToken = _Alice.InvalidateAccessToken(CType(ParentController.APIConnection, AliceConnection).AccessToken)
                    'lastException = Nothing
                    Throw New NotImplementedException()
                    _cts.Token.ThrowIfCancellationRequested()
                Case Else
                    Throw New ApplicationException("No Command Triggered")
            End Select
            If lastException IsNot Nothing Then
                Throw lastException
            End If
            Return ret
        End Function
#End Region

    End Class
End Namespace