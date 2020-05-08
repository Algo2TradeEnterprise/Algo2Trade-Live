Imports System.Threading
Imports Algo2TradeCore.Controller
Imports Algo2TradeCore.Entities

Namespace Calculator
    Public Class AliceBrokerageCalculator
        Inherits APIBrokerageCalculator

        Private _jsonDictionary As Dictionary(Of String, Object)
        Public Sub New(ByVal associatedParentController As AliceStrategyController, canceller As CancellationTokenSource)
            MyBase.New(associatedParentController, canceller)
        End Sub
        Public Overrides Function GetIntradayEquityBrokerage(buy As Decimal, sell As Decimal, quantity As Integer) As IBrokerageAttributes
            'logger.Debug("{0}->GetIntradayEquityBrokerage, parameters:{1},{2},{3}", Me.ToString, buy, sell, quantity)
            Dim ret As AliceBrokerageAttributes = Nothing
            Dim bp As Decimal = buy
            Dim sp As Decimal = sell
            Dim qty As Integer = quantity

            Dim turnover As Decimal = Math.Round((bp + sp) * qty, 2)
            Dim brokerage_buy As Decimal = If((bp * qty * 0.0003) > 20, 20, Math.Round(bp * qty * 0.0003, 2))
            Dim brokerage_sell As Decimal = If((sp * qty * 0.0003) > 20, 20, Math.Round(sp * qty * 0.0003, 2))
            Dim brokerage As Decimal = Math.Round(brokerage_buy + brokerage_sell, 2)
            Dim stt_total As Decimal = Math.Round(sp * qty * 0.00025, 2)
            Dim etc As Decimal = Math.Round(0.0000325 * turnover, 2)
            Dim cc As Decimal = 0
            Dim stax As Decimal = Math.Round(0.18 * (brokerage + etc), 2)
            Dim sebi_charges As Decimal = Math.Round(turnover * 0.000001, 2)
            Dim total_tax As Decimal = Math.Round(brokerage + stt_total + etc + cc + stax + sebi_charges, 2)
            Dim breakeven As Decimal = Math.Round(total_tax / qty, 2)
            Dim net_profit As Decimal = Math.Round(((sp - bp) * qty) - total_tax, 2)

            ret = New AliceBrokerageAttributes With {
                .Buy = bp,
                .Sell = sp,
                .Quantity = qty,
                .Turnover = turnover,
                .Brokerage = brokerage,
                .STT = stt_total,
                .ExchangeFees = etc,
                .Clearing = cc,
                .GST = stax,
                .SEBI = sebi_charges,
                .TotalTax = total_tax,
                .BreakevenPoints = breakeven,
                .NetProfitLoss = net_profit
            }
            Return ret
        End Function

        Public Overrides Function GetDeliveryEquityBrokerage(buy As Decimal, sell As Decimal, quantity As Integer) As IBrokerageAttributes
            'logger.Debug("{0}->GetDeliveryEquityBrokerage, parameters:{1},{2},{3}", Me.ToString, buy, sell, quantity)
            Dim ret As AliceBrokerageAttributes = Nothing
            Dim bp As Decimal = buy
            Dim sp As Decimal = sell
            Dim qty As Integer = quantity

            Dim turnover As Decimal = Math.Round((bp + sp) * qty, 2)
            Dim brokerage As Decimal = 0
            Dim stt_total As Decimal = Math.Round(turnover * 0.001, 2)
            Dim etc As Decimal = Math.Round(0.0000325 * turnover, 2)
            Dim cc As Decimal = 0
            Dim stax As Decimal = Math.Round(0.18 * (brokerage + etc), 2)
            Dim sebi_charges As Decimal = Math.Round(turnover * 0.000001, 2)
            Dim total_tax As Decimal = Math.Round(brokerage + stt_total + etc + cc + stax + sebi_charges, 2)
            Dim breakeven As Decimal = Math.Round(total_tax / qty, 2)
            Dim net_profit As Decimal = Math.Round(((sp - bp) * qty) - total_tax, 2)

            ret = New AliceBrokerageAttributes With {
                .Buy = bp,
                .Sell = sp,
                .Quantity = qty,
                .Turnover = turnover,
                .Brokerage = brokerage,
                .STT = stt_total,
                .ExchangeFees = etc,
                .Clearing = 0,
                .GST = stax,
                .SEBI = sebi_charges,
                .TotalTax = total_tax,
                .BreakevenPoints = breakeven,
                .NetProfitLoss = net_profit
            }
            Return ret
        End Function

        Public Overrides Function GetIntradayEquityFuturesBrokerage(buy As Decimal, sell As Decimal, quantity As Integer) As IBrokerageAttributes
            'logger.Debug("{0}->GetIntradayEquityFuturesBrokerage, parameters:{1},{2},{3}", Me.ToString, buy, sell, quantity)
            Dim ret As AliceBrokerageAttributes = Nothing
            Dim bp As Decimal = buy
            Dim sp As Decimal = sell
            Dim qty As Integer = quantity

            Dim turnover As Decimal = Math.Round((bp + sp) * qty, 2)
            Dim brokerage_buy As Decimal = If((bp * qty * 0.0003) > 20, 20, Math.Round(bp * qty * 0.0003, 2))
            Dim brokerage_sell As Decimal = If((sp * qty * 0.0003) > 20, 20, Math.Round(sp * qty * 0.0003, 2))
            Dim brokerage As Decimal = Math.Round(brokerage_buy + brokerage_sell, 2)
            Dim stt_total As Decimal = Math.Round(sp * qty * 0.0001, 2)
            Dim etc As Decimal = Math.Round(0.000019 * turnover, 2)
            Dim stax As Decimal = Math.Round(0.18 * (brokerage + etc), 2)
            Dim sebi_charges As Decimal = Math.Round(turnover * 0.000001, 2)
            Dim total_tax As Decimal = Math.Round(brokerage + stt_total + etc + stax + sebi_charges, 2)
            Dim breakeven As Decimal = Math.Round(total_tax / qty, 2)
            Dim net_profit As Decimal = Math.Round(((sp - bp) * qty) - total_tax, 2)

            ret = New AliceBrokerageAttributes With {
                .Buy = bp,
                .Sell = sp,
                .Quantity = qty,
                .Turnover = turnover,
                .Brokerage = brokerage,
                .STT = stt_total,
                .ExchangeFees = etc,
                .GST = stax,
                .SEBI = sebi_charges,
                .TotalTax = total_tax,
                .BreakevenPoints = breakeven,
                .NetProfitLoss = net_profit
            }
            Return ret
        End Function

        Public Overrides Function GetIntradayCommodityFuturesBrokerage(instrument As IInstrument, buy As Decimal, sell As Decimal, quantity As Integer) As IBrokerageAttributes
            'logger.Debug("{0}->GetIntradayCommodityFuturesBrokerageAsync, parameters:{1},{2},{3},{4}", Me.ToString, instruemt.TradingSymbol, buy, sell, quantity)
            Dim ret As AliceBrokerageAttributes = Nothing
            Dim stockName As String = instrument.RawInstrumentName
            Dim bp As Decimal = buy
            Dim sp As Decimal = sell
            Dim qty As Integer = quantity

            Dim commodity_value As Long = instrument.QuantityMultiplier
            Dim commodity_cat As String = instrument.BrokerageCategory
            Dim commodity_group As String = instrument.BrokerageGroupCategory
            Dim turnover As Decimal = Math.Round((bp + sp) * commodity_value * qty, 2)
            Dim brokerage_buy As Decimal = 0
            If (bp * commodity_value * qty) > 200000 Then
                brokerage_buy = 20
            Else
                brokerage_buy = If((bp * commodity_value * qty * 0.0003) > 20, 20, Math.Round(bp * commodity_value * qty * 0.0003, 2))
            End If
            Dim brokerage_sell As Decimal = 0
            If (sp * commodity_value * qty) > 200000 Then
                brokerage_sell = 20
            Else
                brokerage_sell = If((sp * commodity_value * qty * 0.0003) > 20, 20, Math.Round(sp * commodity_value * qty * 0.0003, 2))
            End If
            Dim brokerage As Decimal = brokerage_buy + brokerage_sell
            Dim ctt As Decimal = 0
            If commodity_cat = "a" Then
                ctt = Math.Round(0.0001 * sp * qty * commodity_value, 2)
            End If
            Dim etc As Decimal = 0
            Dim cc As Decimal = 0
            etc = If(commodity_cat = "a", Math.Round(0.000026 * turnover, 2), Math.Round(0.0000005 * turnover, 2))
            If stockName = "RBDPMOLEIN" Then
                If turnover >= 100000 Then
                    Dim rbd_multiplier As Integer = CInt(turnover / 100000)
                    etc = Math.Round(rbd_multiplier, 2)
                End If
            End If
            If stockName = "CASTORSEED" Then
                etc = Math.Round(0.000005 * turnover, 2)
            ElseIf stockName = "RBDPMOLEIN" Then
                etc = Math.Round(0.00001 * turnover, 2)
            ElseIf stockName = "PEPPER" Then
                etc = Math.Round(0.0000005 * turnover, 2)
            ElseIf stockName = "KAPAS" Then
                etc = Math.Round(0.000005 * turnover, 2)
            End If
            Dim stax As Decimal = Math.Round(0.18 * (brokerage + etc), 2)
            Dim sebi_charges As Decimal = Math.Round(turnover * 0.000001, 2)
            If commodity_group = "a" Then
                sebi_charges = Math.Round(turnover * 0.0000001, 2)
            End If
            Dim total_tax As Decimal = Math.Round(brokerage + ctt + etc + stax + sebi_charges, 2)
            Dim breakeven As Decimal = Math.Round(total_tax / (qty * commodity_value), 2)
            Dim net_profit As Decimal = Math.Round(((sp - bp) * qty * commodity_value) - total_tax, 2)

            ret = New AliceBrokerageAttributes With {
                .Buy = bp,
                .Sell = sp,
                .Quantity = qty,
                .Turnover = turnover,
                .CTT = ctt,
                .Brokerage = brokerage,
                .ExchangeFees = etc,
                .Clearing = cc,
                .GST = stax,
                .SEBI = sebi_charges,
                .TotalTax = total_tax,
                .BreakevenPoints = breakeven,
                .NetProfitLoss = net_profit
            }
            Return ret
        End Function

        Public Overrides Function GetIntradayEquityOptionsBrokerage(buy As Decimal, sell As Decimal, quantity As Integer) As IBrokerageAttributes
            'logger.Debug("{0}->GetIntradayEquityOptionsBrokerage, parameters:{1},{2},{3}", Me.ToString, buy, sell, quantity)
            Throw New NotImplementedException()
        End Function

        Public Overrides Function GetIntradayCurrencyFuturesBrokerage(buy As Decimal, sell As Decimal, quantity As Integer) As IBrokerageAttributes
            'logger.Debug("{0}->GetIntradayCurrencyFuturesBrokerage, parameters:{1},{2},{3}", Me.ToString, buy, sell, quantity)
            Dim ret As AliceBrokerageAttributes = Nothing
            Dim bp As Decimal = buy
            Dim sp As Decimal = sell
            Dim qty As Integer = quantity

            Dim turnover As Decimal = Math.Round((bp + sp) * qty * 1000, 2)
            Dim brokerage_buy As Decimal = If((bp * qty * 1000 * 0.0003) > 20, 20, Math.Round(bp * qty * 1000 * 0.0003, 2))
            Dim brokerage_sell As Decimal = If((sp * qty * 1000 * 0.0003) > 20, 20, Math.Round(sp * qty * 1000 * 0.0003, 2))
            Dim brokerage As Decimal = Math.Round(brokerage_buy + brokerage_sell, 2)
            Dim etc As Decimal = Math.Round(0.000009 * turnover, 2)
            Dim cc As Decimal = 0
            Dim total_trans_charge As Decimal = etc + cc
            Dim stax As Decimal = Math.Round(0.18 * (brokerage + total_trans_charge), 2)
            Dim sebi_charges As Decimal = Math.Round(turnover * 0.000001, 2)
            Dim total_tax As Decimal = Math.Round(brokerage + total_trans_charge + stax + sebi_charges, 2)
            Dim breakeven As Decimal = Math.Round(total_tax / (qty * 1000), 4)
            Dim pips As Decimal = Math.Ceiling(breakeven / 0.0025)
            Dim net_profit As Decimal = Math.Round(((sp - bp) * qty * 1000) - total_tax, 2)

            ret = New AliceBrokerageAttributes With {
                .Buy = bp,
                .Sell = sp,
                .Quantity = qty,
                .Turnover = turnover,
                .Brokerage = brokerage,
                .ExchangeFees = etc,
                .Clearing = cc,
                .GST = stax,
                .SEBI = sebi_charges,
                .TotalTax = total_tax,
                .BreakevenPoints = breakeven,
                .NetProfitLoss = net_profit
            }
            Return ret
        End Function

        Public Overrides Function GetIntradayCurrencyOptionsBrokerage(strikePrice As Decimal, buyPremium As Decimal, sellPremium As Decimal, quantity As Integer) As IBrokerageAttributes
            'logger.Debug("{0}->GetIntradayCurrencyOptionsBrokerage, parameters:{1},{2},{3},{4}", Me.ToString, strikePrice, buyPremium, sellPremium, quantity)
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace