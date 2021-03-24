Public Class frmInsertSignal
    Private ReadOnly _strategyInstrument As NFOStrategyInstrument
    Public Sub New(ByVal runningInstrument As NFOStrategyInstrument)
        InitializeComponent()
        _strategyInstrument = runningInstrument
    End Sub

    Private Sub frmInsertSignal_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = String.Format("Insert Signal - {0}", _strategyInstrument.TradableInstrument.TradingSymbol.ToUpper)

        dtpckrTradingDate.Value = Now.Date
        txtClosePrice.Text = _strategyInstrument.TradableInstrument.LastTick.LastPrice
        txtEntryPrice.Text = _strategyInstrument.TradableInstrument.LastTick.LastPrice
        chkMainTradingDay.Checked = _strategyInstrument.TakeTradeToday
    End Sub

    Private Sub btnInsert_Click(sender As Object, e As EventArgs) Handles btnInsert.Click
        Try
            Dim tradingDate As Date = dtpckrTradingDate.Value.Date
            Dim closePrice As String = txtClosePrice.Text
            Dim entryPrice As String = txtEntryPrice.Text
            If tradingDate.Date > Now.Date Then
                Throw New ApplicationException(String.Format("Cannot enter signal for > {0}", Now.Date.ToString("dd-MMM-yyyy")))
            Else
                If entryPrice IsNot Nothing AndAlso entryPrice <> "" AndAlso IsNumeric(entryPrice) AndAlso
                    closePrice IsNot Nothing AndAlso closePrice <> "" AndAlso IsNumeric(closePrice) Then
                    Dim lastSignal As NFOStrategyInstrument.SignalDetails = _strategyInstrument.GetLastSignalDetails(Now.Date)
                    If lastSignal IsNot Nothing AndAlso lastSignal.SnapshotDate >= tradingDate.Date Then
                        Throw New ApplicationException(String.Format("Cannot enter signal for < {0}", lastSignal.SnapshotDate.AddDays(1).ToString("dd-MMM-yyyy")))
                    Else
                        Dim desireValue As Double = CType(_strategyInstrument.ParentStrategy.UserSettings, NFOUserInputs).InitialInvestment
                        If lastSignal IsNot Nothing Then
                            If lastSignal.MainTradingDay Then
                                desireValue = lastSignal.DesireValue + CType(_strategyInstrument.ParentStrategy.UserSettings, NFOUserInputs).ExpectedIncreaseEachPeriod
                            Else
                                desireValue = lastSignal.DesireValue
                            End If
                        End If
                        If _strategyInstrument.SetSignalDetails(tradingDate, closePrice, entryPrice, desireValue, chkMainTradingDay.Checked, CType(_strategyInstrument.ParentStrategy.UserSettings, NFOUserInputs).InstrumentsData(_strategyInstrument.TradableInstrument.TradingSymbol).RunDaily) Then
                            MsgBox(String.Format("Signal Insertion Successful"), MsgBoxStyle.Information)
                            Me.Close()
                        Else
                            Throw New ApplicationException(String.Format("Cannot enter signal for {0} as the signal is already available", tradingDate.ToString("dd-MMM-yyyy")))
                        End If
                    End If
                Else
                    Throw New ApplicationException(String.Format("Invalid Entry/Close Price"))
                End If
            End If
        Catch ex As Exception
            MsgBox(String.Format("The following error occurred: {0}", ex.Message), MsgBoxStyle.Critical)
        End Try
    End Sub
End Class