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
            Calculator = New ZerodhaBrokerageCalculator(Me.ParentController, canceller)
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

        Public Overrides Function GetAllInstrumentsAsync() As Task(Of IEnumerable(Of IInstrument))
            Throw New NotImplementedException()
        End Function

        Public Overrides Function GetAllTradesAsync() As Task(Of IEnumerable(Of ITrade))
            Throw New NotImplementedException()
        End Function

        Public Overrides Function GetAllOrdersAsync() As Task(Of IEnumerable(Of IOrder))
            Throw New NotImplementedException()
        End Function

        Public Overrides Function GetAllHoldingsAsync() As Task(Of IEnumerable(Of IHolding))
            Throw New NotImplementedException()
        End Function

        Public Overrides Function GetAllPositionsAsync() As Task(Of IPositionResponse)
            Throw New NotImplementedException()
        End Function

        Public Overrides Function GetUserMarginsAsync() As Task(Of Dictionary(Of TypeOfExchage, IUserMargin))
            Throw New NotImplementedException()
        End Function

        Public Overrides Function GetAllQuotesAsync(instruments As IEnumerable(Of IInstrument)) As Task(Of IEnumerable(Of IQuote))
            Throw New NotImplementedException()
        End Function

        Public Overrides Function CreateSingleInstrument(supportedTradingSymbol As String, instrumentToken As UInteger, sampleInstrument As IInstrument) As IInstrument
            Throw New NotImplementedException()
        End Function

        Public Overrides Function CreateSimilarOrderWithTag(tag As String, orderData As IOrder) As IOrder
            Throw New NotImplementedException()
        End Function

        Public Overrides Function ModifyStoplossOrderAsync(orderId As String, triggerPrice As Decimal) As Task(Of Dictionary(Of String, Object))
            Throw New NotImplementedException()
        End Function

        Public Overrides Function ModifyTargetOrderAsync(orderId As String, price As Decimal) As Task(Of Dictionary(Of String, Object))
            Throw New NotImplementedException()
        End Function

        Public Overrides Function CancelBOOrderAsync(orderId As String, parentOrderID As String) As Task(Of Dictionary(Of String, Object))
            Throw New NotImplementedException()
        End Function

        Public Overrides Function CancelCOOrderAsync(orderId As String, parentOrderID As String) As Task(Of Dictionary(Of String, Object))
            Throw New NotImplementedException()
        End Function

        Public Overrides Function CancelRegularOrderAsync(orderId As String, parentOrderID As String) As Task(Of Dictionary(Of String, Object))
            Throw New NotImplementedException()
        End Function

        Public Overrides Function PlaceBOLimitMISOrderAsync(tradeExchange As String, tradingSymbol As String, transaction As IOrder.TypeOfTransaction, quantity As Integer, price As Decimal, squareOffValue As Decimal, stopLossValue As Decimal, tag As String) As Task(Of Dictionary(Of String, Object))
            Throw New NotImplementedException()
        End Function

        Public Overrides Function PlaceBOSLMISOrderAsync(tradeExchange As String, tradingSymbol As String, transaction As IOrder.TypeOfTransaction, quantity As Integer, price As Decimal, triggerPrice As Decimal, squareOffValue As Decimal, stopLossValue As Decimal, tag As String) As Task(Of Dictionary(Of String, Object))
            Throw New NotImplementedException()
        End Function

        Public Overrides Function PlaceCOMarketMISOrderAsync(tradeExchange As String, tradingSymbol As String, transaction As IOrder.TypeOfTransaction, quantity As Integer, triggerPrice As Decimal, tag As String) As Task(Of Dictionary(Of String, Object))
            Throw New NotImplementedException()
        End Function

        Public Overrides Function PlaceRegularMarketMISOrderAsync(tradeExchange As String, tradingSymbol As String, transaction As IOrder.TypeOfTransaction, quantity As Integer, tag As String) As Task(Of Dictionary(Of String, Object))
            Throw New NotImplementedException()
        End Function

        Public Overrides Function PlaceRegularLimitMISOrderAsync(tradeExchange As String, tradingSymbol As String, transaction As IOrder.TypeOfTransaction, quantity As Integer, price As Decimal, tag As String) As Task(Of Dictionary(Of String, Object))
            Throw New NotImplementedException()
        End Function

        Public Overrides Function PlaceRegularSLMMISOrderAsync(tradeExchange As String, tradingSymbol As String, transaction As IOrder.TypeOfTransaction, quantity As Integer, triggerPrice As Decimal, tag As String) As Task(Of Dictionary(Of String, Object))
            Throw New NotImplementedException()
        End Function

        Public Overrides Function PlaceRegularMarketCNCOrderAsync(tradeExchange As String, tradingSymbol As String, transaction As IOrder.TypeOfTransaction, quantity As Integer, tag As String) As Task(Of Dictionary(Of String, Object))
            Throw New NotImplementedException()
        End Function

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
                                                                           Return _Alice.PlaceOrder(Exchange:=CType(stockData("Exchange"), String),
                                                                                                    InstrumentToken:=CType(stockData("InstrumentToken"), String),
                                                                                                    TransactionType:=CType(stockData("TransactionType"), String),
                                                                                                    Quantity:=CType(stockData("Quantity"), Integer),
                                                                                                    Price:=CType(stockData("Price"), Decimal),
                                                                                                    Product:=CType(stockData("Product"), String),
                                                                                                    OrderType:=CType(stockData("OrderType"), String),
                                                                                                    Validity:=CType(stockData("Validity"), String),
                                                                                                    TriggerPrice:=CType(stockData("TriggerPrice"), String),
                                                                                                    SquareOffValue:=CType(stockData("SquareOffValue"), Decimal),
                                                                                                    StoplossValue:=CType(stockData("StoplossValue"), Decimal),
                                                                                                    Variety:=CType(stockData("Variety"), String),
                                                                                                    Tag:=CType(stockData("Tag"), String))
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