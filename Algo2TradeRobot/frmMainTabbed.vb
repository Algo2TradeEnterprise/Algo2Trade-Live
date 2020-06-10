Imports System.Threading
Imports NLog
Imports Algo2TradeCore.Entities
Imports Algo2TradeCore.Controller
Imports Algo2TradeCore.Exceptions
Imports System.ComponentModel
Imports Syncfusion.WinForms.DataGrid
Imports Syncfusion.WinForms.DataGrid.Events
Imports Syncfusion.WinForms.Input.Enums
Imports Utilities.Time
Imports System.IO
Imports System.Runtime.Serialization.Formatters.Binary
Imports Algo2TradeCore.Entities.UserSettings
Imports Algo2TradeCore.Strategies
Imports System.Reflection
Imports System.Runtime.InteropServices

Public Class frmMainTabbed

#Region "Logging and Status Progress"
    Public Shared logger As Logger = LogManager.GetCurrentClassLogger
#End Region

#Region "Common Delegates"

    Delegate Sub SetSFGridDataBind_Delegate(ByVal [grd] As SfDataGrid, ByVal [value] As Object)
    Public Async Sub SetSFGridDataBind_ThreadSafe(ByVal [grd] As Syncfusion.WinForms.DataGrid.SfDataGrid, ByVal [value] As Object)
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [grd].InvokeRequired Then
            Dim MyDelegate As New SetSFGridDataBind_Delegate(AddressOf SetSFGridDataBind_ThreadSafe)
            Me.Invoke(MyDelegate, New Object() {[grd], [value]})
        Else
            While True
                Try
                    [grd].DataSource = [value]
                    Exit While
                Catch sop As System.InvalidOperationException
                    logger.Error(sop)
                End Try
                Await Task.Delay(500, _cts.Token).ConfigureAwait(False)
            End While
        End If
    End Sub

    Delegate Sub BindingListAdd_Delegate(ByVal [src] As BindingList(Of ActivityDashboard), ByVal [value] As ActivityDashboard)
    Public Async Sub BindingListAdd_ThreadSafe(ByVal [src] As BindingList(Of ActivityDashboard), ByVal [value] As ActivityDashboard)
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If Me.InvokeRequired Then
            Dim MyDelegate As New BindingListAdd_Delegate(AddressOf BindingListAdd_ThreadSafe)
            Me.Invoke(MyDelegate, New Object() {[src], [value]})
        Else
            'While True
            Try
                [src].Add([value])
                '[src].Insert(0, [value])
                'Exit While
            Catch aex As ArgumentOutOfRangeException
                'Nothing to do
            Catch ex As Exception
                logger.Error(ex)
            End Try
            Await Task.Delay(0, _cts.Token).ConfigureAwait(False)
            'End While
        End If
    End Sub

    Delegate Sub SetSFGridFreezFirstColumn_Delegate(ByVal [grd] As SfDataGrid)
    Public Sub SetSFGridFreezFirstColumn_ThreadSafe(ByVal [grd] As Syncfusion.WinForms.DataGrid.SfDataGrid)
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [grd].InvokeRequired Then
            Dim MyDelegate As New SetSFGridFreezFirstColumn_Delegate(AddressOf SetSFGridFreezFirstColumn_ThreadSafe)
            Me.Invoke(MyDelegate, New Object() {[grd]})
        Else
            [grd].FrozenColumnCount = 1
            'Await Task.Delay(500).ConfigureAwait(False)
        End If
    End Sub

    Delegate Sub SetGridDisplayIndex_Delegate(ByVal [grd] As DataGridView, ByVal [colName] As String, ByVal [value] As Integer)
    Public Sub SetGridDisplayIndex_ThreadSafe(ByVal [grd] As DataGridView, ByVal [colName] As String, ByVal [value] As Integer)
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [grd].InvokeRequired Then
            Dim MyDelegate As New SetGridDisplayIndex_Delegate(AddressOf SetGridDisplayIndex_ThreadSafe)
            Me.Invoke(MyDelegate, New Object() {[grd], [colName], [value]})
        Else
            [grd].Columns([colName]).DisplayIndex = [value]
        End If
    End Sub
    Delegate Function GetGridColumnCount_Delegate(ByVal [grd] As DataGridView) As String
    Public Function GetGridColumnCount_ThreadSafe(ByVal [grd] As DataGridView) As String
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [grd].InvokeRequired Then
            Dim MyDelegate As New GetGridColumnCount_Delegate(AddressOf GetGridColumnCount_ThreadSafe)
            Return Me.Invoke(MyDelegate, New Object() {[grd]})
        Else
            Return [grd].Columns.Count
        End If
    End Function

    Delegate Sub SetGridDataBind_Delegate(ByVal [grd] As DataGridView, ByVal [value] As Object)
    Public Sub SetGridDataBind_ThreadSafe(ByVal [grd] As DataGridView, ByVal [value] As Object)
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [grd].InvokeRequired Then
            Dim MyDelegate As New SetGridDataBind_Delegate(AddressOf SetGridDataBind_ThreadSafe)
            Me.Invoke(MyDelegate, New Object() {[grd], [value]})
        Else
            [grd].DataSource = [value]
        End If
    End Sub

    Delegate Sub SetListAddItem_Delegate(ByVal [lst] As ListBox, ByVal [value] As Object)
    Public Sub SetListAddItem_ThreadSafe(ByVal [lst] As ListBox, ByVal [value] As Object)
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [lst].InvokeRequired Then
            Dim MyDelegate As New SetListAddItem_Delegate(AddressOf SetListAddItem_ThreadSafe)
            Me.Invoke(MyDelegate, New Object() {lst, [value]})
        Else
            [lst].Items.Insert(0, [value])
        End If
    End Sub
    Delegate Sub SetObjectEnableDisable_Delegate(ByVal [obj] As Object, ByVal [value] As Boolean)
    Public Sub SetObjectEnableDisable_ThreadSafe(ByVal [obj] As Object, ByVal [value] As Boolean)
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [obj].InvokeRequired Then
            Dim MyDelegate As New SetObjectEnableDisable_Delegate(AddressOf SetObjectEnableDisable_ThreadSafe)
            Me.Invoke(MyDelegate, New Object() {[obj], [value]})
        Else
            [obj].Enabled = [value]
        End If
    End Sub

    Delegate Function GetObjectText_Delegate(ByVal [Object] As Object) As String
    Public Function GetObjectText_ThreadSafe(ByVal [Object] As Object) As String
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [Object].InvokeRequired Then
            Dim MyDelegate As New GetObjectText_Delegate(AddressOf GetObjectText_ThreadSafe)
            Return Me.Invoke(MyDelegate, New Object() {[Object]})
        Else
            Return [Object].Text
        End If
    End Function

    Delegate Sub SetObjectText_Delegate(ByVal [Object] As Object, ByVal [text] As String)
    Public Sub SetObjectText_ThreadSafe(ByVal [Object] As Object, ByVal [text] As String)
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [Object].InvokeRequired Then
            Dim MyDelegate As New SetObjectText_Delegate(AddressOf SetObjectText_ThreadSafe)
            Me.Invoke(MyDelegate, New Object() {[Object], [text]})
        Else
            [Object].Text = [text]
        End If
    End Sub

    Delegate Sub SetLabelText_Delegate(ByVal [label] As Label, ByVal [text] As String)
    Public Sub SetLabelText_ThreadSafe(ByVal [label] As Label, ByVal [text] As String)
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [label].InvokeRequired Then
            Dim MyDelegate As New SetLabelText_Delegate(AddressOf SetLabelText_ThreadSafe)
            Me.Invoke(MyDelegate, New Object() {[label], [text]})
        Else
            [label].Text = [text]
        End If
    End Sub
    Delegate Function GetLabelText_Delegate(ByVal [label] As Label) As String

    Public Function GetLabelText_ThreadSafe(ByVal [label] As Label) As String
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [label].InvokeRequired Then
            Dim MyDelegate As New GetLabelText_Delegate(AddressOf GetLabelText_ThreadSafe)
            Return Me.Invoke(MyDelegate, New Object() {[label]})
        Else
            Return [label].Text
        End If
    End Function

    Delegate Sub SetLabelTag_Delegate(ByVal [label] As Label, ByVal [tag] As String)
    Public Sub SetLabelTag_ThreadSafe(ByVal [label] As Label, ByVal [tag] As String)
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [label].InvokeRequired Then
            Dim MyDelegate As New SetLabelTag_Delegate(AddressOf SetLabelTag_ThreadSafe)
            Me.Invoke(MyDelegate, New Object() {[label], [tag]})
        Else
            [label].Tag = [tag]
        End If
    End Sub
    Delegate Function GetLabelTag_Delegate(ByVal [label] As Label) As String

    Public Function GetLabelTag_ThreadSafe(ByVal [label] As Label) As String
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [label].InvokeRequired Then
            Dim MyDelegate As New GetLabelTag_Delegate(AddressOf GetLabelTag_ThreadSafe)
            Return Me.Invoke(MyDelegate, New Object() {[label]})
        Else
            Return [label].Tag
        End If
    End Function
    Delegate Sub SetToolStripLabel_Delegate(ByVal [toolStrip] As StatusStrip, ByVal [label] As ToolStripStatusLabel, ByVal [text] As String)
    Public Sub SetToolStripLabel_ThreadSafe(ByVal [toolStrip] As StatusStrip, ByVal [label] As ToolStripStatusLabel, ByVal [text] As String)
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [toolStrip].InvokeRequired Then
            Dim MyDelegate As New SetToolStripLabel_Delegate(AddressOf SetToolStripLabel_ThreadSafe)
            Me.Invoke(MyDelegate, New Object() {[toolStrip], [label], [text]})
        Else
            [label].Text = [text]
        End If
    End Sub

    Delegate Function GetToolStripLabel_Delegate(ByVal [toolStrip] As StatusStrip, ByVal [label] As ToolStripLabel) As String
    Public Function GetToolStripLabel_ThreadSafe(ByVal [toolStrip] As StatusStrip, ByVal [label] As ToolStripLabel) As String
        ' InvokeRequired required compares the thread ID of the calling thread to the thread ID of the creating thread.  
        ' If these threads are different, it returns true.  
        If [toolStrip].InvokeRequired Then
            Dim MyDelegate As New GetToolStripLabel_Delegate(AddressOf GetToolStripLabel_ThreadSafe)
            Return Me.Invoke(MyDelegate, New Object() {[toolStrip], [label]})
        Else
            Return [label].Text
        End If
    End Function
#End Region

#Region "Standard Event Handlers"
    Private Sub OnHeartbeat(msg As String)
        'Update detailed status on the first part, dont append if the text starts with <
        If msg.Contains("<<<") Then
            msg = Replace(msg, "<<<", Nothing)
            ProgressStatus(msg)
        Else
            ProgressStatus(msg)
        End If
        msg = Nothing
    End Sub
    Private Sub OnWaitingFor(elapsedSecs As Integer, totalSecs As Integer, msg As String)
        If msg.Contains("...") Then msg = msg.Replace("...", "")
        ProgressStatus(String.Format("{0}, waiting {1}/{2} secs", msg, elapsedSecs, totalSecs))
    End Sub
    Private Sub OnDocumentRetryStatus(currentTry As Integer, totalTries As Integer)
        'ProgressStatus(String.Format("Try #{0}/{1}: Connecting", currentTry, totalTries))
    End Sub
    Private Sub OnDocumentDownloadComplete()
    End Sub
    Private Sub OnErrorOcurred(ByVal msg As String, ByVal ex As Exception)
        MsgBox(ex.Message)
        End
    End Sub
#End Region

#Region "Private Attributes"
    Private _cts As CancellationTokenSource
    Private _lastLoggedMessage As String = Nothing
    Private _commonController As APIStrategyController = Nothing
    Private _connection As IConnection = Nothing
    Private _commonControllerUserInput As ControllerUserInputs = Nothing
    Private _lastException As Exception = Nothing
#End Region

    Private _toolRunning As Boolean = False

    Private Sub miUserDetails_Click(sender As Object, e As EventArgs) Handles miUserDetails.Click
        Dim newForm As New frmAliceUserDetails(_commonControllerUserInput, _toolRunning)
        newForm.ShowDialog()
        If File.Exists(ControllerUserInputs.Filename) Then
            _commonControllerUserInput = Utilities.Strings.DeserializeToCollection(Of ControllerUserInputs)(ControllerUserInputs.Filename)
        End If
    End Sub

    Private Sub miAbout_Click(sender As Object, e As EventArgs) Handles miAbout.Click
        Dim newForm As New frmAbout
        newForm.ShowDialog()
    End Sub

    Private Sub miAdvanceOptions_Click(sender As Object, e As EventArgs) Handles miAdvancedOptions.Click
        Dim newForm As New frmAdvancedOptions(_commonControllerUserInput, _toolRunning)
        newForm.ShowDialog()
        If File.Exists(ControllerUserInputs.Filename) Then
            _commonControllerUserInput = Utilities.Strings.DeserializeToCollection(Of ControllerUserInputs)(ControllerUserInputs.Filename)
        End If
        Dim formRemarks As String = Nothing
        If _commonControllerUserInput IsNot Nothing AndAlso _commonControllerUserInput.FormRemarks IsNot Nothing AndAlso
            _commonControllerUserInput.FormRemarks.Trim <> "" Then
            formRemarks = _commonControllerUserInput.FormRemarks.Trim
        End If
        Me.Text = String.Format("Algo2Trade Robot v{0}{1}", My.Application.Info.Version, If(formRemarks IsNot Nothing, String.Format(" - {0}", formRemarks), ""))
    End Sub

#Region "NFO"
    Private _nfoStrategyRunning As Boolean = False
    Private _nfoUserInputs As NFOUserInputs = Nothing
    Private _nfoDashboadList As BindingList(Of ActivityDashboard) = Nothing
    Private _nfoTradableInstruments As IEnumerable(Of NFOStrategyInstrument) = Nothing
    Private _nfoStrategyToExecute As NFOStrategy = Nothing
    Private Sub sfdgvNFOMainDashboard_FilterPopupShowing(sender As Object, e As FilterPopupShowingEventArgs) Handles sfdgvNFOMainDashboard.FilterPopupShowing
        ManipulateGridEx(GridMode.TouchupPopupFilter, e, GetType(NFOStrategy))
    End Sub
    Private Sub sfdgvNFOMainDashboard_AutoGeneratingColumn(sender As Object, e As AutoGeneratingColumnArgs) Handles sfdgvNFOMainDashboard.AutoGeneratingColumn
        ManipulateGridEx(GridMode.TouchupAutogeneratingColumn, e, GetType(NFOStrategy))
    End Sub
    Private Async Function NFOWorkerAsync() As Task
        If GetObjectText_ThreadSafe(btnNFOStart) = Common.LOGIN_PENDING Then
            MsgBox("Cannot start as another strategy is loggin in")
            Exit Function
        End If

        If _cts Is Nothing Then _cts = New CancellationTokenSource
        _cts.Token.ThrowIfCancellationRequested()
        _lastException = Nothing

        Try
            EnableDisableUIEx(UIMode.Active, GetType(NFOStrategy))
            EnableDisableUIEx(UIMode.BlockOther, GetType(NFOStrategy))

            OnHeartbeat("Validating user settings")
            If File.Exists(NFOUserInputs.SettingsFileName) Then
                Dim fs As Stream = New FileStream(NFOUserInputs.SettingsFileName, FileMode.Open)
                Dim bf As BinaryFormatter = New BinaryFormatter()
                _nfoUserInputs = CType(bf.Deserialize(fs), NFOUserInputs)
                fs.Close()
                _nfoUserInputs.InstrumentsData = Nothing
                _nfoUserInputs.FillInstrumentDetails(_nfoUserInputs.InstrumentDetailsFilePath, _cts)
            Else
                Throw New ApplicationException("Settings file not found. Please complete your settings properly.")
            End If
            logger.Debug(Utilities.Strings.JsonSerialize(_nfoUserInputs))

            If Not Common.IsAliceUserDetailsPopulated(_commonControllerUserInput) Then Throw New ApplicationException("Cannot proceed without API user details being entered")
            Dim currentUser As AliceUser = Common.GetAliceCredentialsFromSettings(_commonControllerUserInput)
            logger.Debug(Utilities.Strings.JsonSerialize(currentUser))

            If _commonController IsNot Nothing Then
                _commonController.RefreshCancellationToken(_cts)
            Else
                _commonController = New AliceStrategyController(currentUser, _commonControllerUserInput, _cts)

                RemoveHandler _commonController.Heartbeat, AddressOf OnHeartbeat
                RemoveHandler _commonController.WaitingFor, AddressOf OnWaitingFor
                RemoveHandler _commonController.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
                RemoveHandler _commonController.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete
                RemoveHandler _commonController.HeartbeatEx, AddressOf OnHeartbeatEx
                RemoveHandler _commonController.WaitingForEx, AddressOf OnWaitingForEx
                RemoveHandler _commonController.DocumentRetryStatusEx, AddressOf OnDocumentRetryStatusEx
                RemoveHandler _commonController.DocumentDownloadCompleteEx, AddressOf OnDocumentDownloadCompleteEx
                RemoveHandler _commonController.TickerClose, AddressOf OnTickerClose
                RemoveHandler _commonController.TickerConnect, AddressOf OnTickerConnect
                RemoveHandler _commonController.TickerError, AddressOf OnTickerError
                RemoveHandler _commonController.TickerErrorWithStatus, AddressOf OnTickerErrorWithStatus
                RemoveHandler _commonController.TickerNoReconnect, AddressOf OnTickerNoReconnect
                RemoveHandler _commonController.FetcherError, AddressOf OnFetcherError
                RemoveHandler _commonController.CollectorError, AddressOf OnCollectorError
                RemoveHandler _commonController.NewItemAdded, AddressOf OnNewItemAdded
                RemoveHandler _commonController.SessionExpiry, AddressOf OnSessionExpiry
                RemoveHandler _commonController.EndOfTheDay, AddressOf OnEndOfTheDay

                AddHandler _commonController.Heartbeat, AddressOf OnHeartbeat
                AddHandler _commonController.WaitingFor, AddressOf OnWaitingFor
                AddHandler _commonController.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
                AddHandler _commonController.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete
                AddHandler _commonController.HeartbeatEx, AddressOf OnHeartbeatEx
                AddHandler _commonController.WaitingForEx, AddressOf OnWaitingForEx
                AddHandler _commonController.DocumentRetryStatusEx, AddressOf OnDocumentRetryStatusEx
                AddHandler _commonController.DocumentDownloadCompleteEx, AddressOf OnDocumentDownloadCompleteEx
                AddHandler _commonController.TickerClose, AddressOf OnTickerClose
                AddHandler _commonController.TickerConnect, AddressOf OnTickerConnect
                AddHandler _commonController.TickerError, AddressOf OnTickerError
                AddHandler _commonController.TickerErrorWithStatus, AddressOf OnTickerErrorWithStatus
                AddHandler _commonController.TickerNoReconnect, AddressOf OnTickerNoReconnect
                AddHandler _commonController.TickerReconnect, AddressOf OnTickerReconnect
                AddHandler _commonController.FetcherError, AddressOf OnFetcherError
                AddHandler _commonController.CollectorError, AddressOf OnCollectorError
                AddHandler _commonController.NewItemAdded, AddressOf OnNewItemAdded
                AddHandler _commonController.SessionExpiry, AddressOf OnSessionExpiry
                AddHandler _commonController.EndOfTheDay, AddressOf OnEndOfTheDay

                Dim currentAssembly As Assembly = Assembly.GetExecutingAssembly()
                Dim attribute As GuidAttribute = currentAssembly.GetCustomAttributes(GetType(GuidAttribute), True)(0)
                Dim toolID As String = attribute.Value
                Dim toolRunning As Boolean = Await _commonController.IsToolRunning(toolID).ConfigureAwait(False)
                If Not toolRunning Then Throw New ApplicationException("You version is expired. Please contact Algo2Trade.")

#Region "Login"
                Dim loginMessage As String = Nothing
                While True
                    _cts.Token.ThrowIfCancellationRequested()
                    _connection = Nothing
                    loginMessage = Nothing
                    Try
                        OnHeartbeat("Attempting to get connection to Alice API")
                        _cts.Token.ThrowIfCancellationRequested()
                        _connection = Await _commonController.LoginAsync().ConfigureAwait(False)
                        _cts.Token.ThrowIfCancellationRequested()
                    Catch cx As OperationCanceledException
                        loginMessage = cx.Message
                        logger.Error(cx)
                        Exit While
                    Catch ex As Exception
                        loginMessage = ex.Message
                        logger.Error(ex)
                    End Try
                    If _connection Is Nothing Then
                        If loginMessage IsNot Nothing AndAlso (loginMessage.ToUpper.Contains("password".ToUpper) OrElse loginMessage.ToUpper.Contains("api_key".ToUpper) OrElse loginMessage.ToUpper.Contains("username".ToUpper)) Then
                            'No need to retry as its a password failure
                            OnHeartbeat(String.Format("Loging process failed:{0}", loginMessage))
                            Exit While
                        Else
                            OnHeartbeat(String.Format("Loging process failed:{0} | Waiting for 10 seconds before retrying connection", loginMessage))
                            _cts.Token.ThrowIfCancellationRequested()
                            Await Task.Delay(10000, _cts.Token).ConfigureAwait(False)
                            _cts.Token.ThrowIfCancellationRequested()
                        End If
                    Else
                        Exit While
                    End If
                End While
                If _connection Is Nothing Then
                    If loginMessage IsNot Nothing Then
                        Throw New ApplicationException(String.Format("No connection to Alice API could be established | Details:{0}", loginMessage))
                    Else
                        Throw New ApplicationException("No connection to Alice API could be established")
                    End If
                End If
#End Region

                OnHeartbeat("Completing all pre-automation requirements")
                _cts.Token.ThrowIfCancellationRequested()
                Dim isPreProcessingDone As Boolean = Await _commonController.PrepareToRunStrategyAsync().ConfigureAwait(False)
                _cts.Token.ThrowIfCancellationRequested()

                If Not isPreProcessingDone Then Throw New ApplicationException("PrepareToRunStrategyAsync did not succeed, cannot progress")
            End If 'Common controller
            EnableDisableUIEx(UIMode.ReleaseOther, GetType(NFOStrategy))

            _nfoStrategyToExecute = New NFOStrategy(_commonController, 1, _nfoUserInputs, 5, _cts)
            OnHeartbeatEx(String.Format("Running strategy:{0}", _nfoStrategyToExecute.ToString), New List(Of Object) From {_nfoStrategyToExecute})

            _cts.Token.ThrowIfCancellationRequested()
            Await _commonController.SubscribeStrategyAsync(_nfoStrategyToExecute).ConfigureAwait(False)
            _cts.Token.ThrowIfCancellationRequested()

            _nfoTradableInstruments = _nfoStrategyToExecute.TradableStrategyInstruments
            SetObjectText_ThreadSafe(linklblNFOTradableInstrument, String.Format("Tradable Instruments: {0}", _nfoTradableInstruments.Count))
            SetObjectEnableDisable_ThreadSafe(linklblNFOTradableInstrument, True)
            _cts.Token.ThrowIfCancellationRequested()

            _nfoDashboadList = New BindingList(Of ActivityDashboard)(_nfoStrategyToExecute.SignalManager.ActivityDetails.Values.OrderBy(Function(x)
                                                                                                                                            Return x.SignalGeneratedTime
                                                                                                                                        End Function).ToList)
            SetSFGridDataBind_ThreadSafe(sfdgvNFOMainDashboard, _nfoDashboadList)
            SetSFGridFreezFirstColumn_ThreadSafe(sfdgvNFOMainDashboard)
            _cts.Token.ThrowIfCancellationRequested()

            Await _nfoStrategyToExecute.MonitorAsync().ConfigureAwait(False)
        Catch aex As AdapterBusinessException
            logger.Error(aex)
            If aex.ExceptionType = AdapterBusinessException.TypeOfException.PermissionException Then
                _lastException = aex
            Else
                GenerateTelegramMessageAsync(aex.Message)
                MsgBox(String.Format("The following error occurred: {0}", aex.Message), MsgBoxStyle.Critical)
            End If
        Catch fex As ForceExitException
            logger.Error(fex)
            _lastException = fex
        Catch cx As OperationCanceledException
            logger.Error(cx)
            GenerateTelegramMessageAsync(cx.Message)
            MsgBox(String.Format("The following error occurred: {0}", cx.Message), MsgBoxStyle.Critical)
        Catch ex As Exception
            logger.Error(ex)
            GenerateTelegramMessageAsync(ex.Message)
            MsgBox(String.Format("The following error occurred: {0}", ex.Message), MsgBoxStyle.Critical)
        Finally
            ProgressStatus("No pending actions")
            SetObjectText_ThreadSafe(linklblNFOTradableInstrument, String.Format("Tradable Instruments: {0}", 0))
            SetObjectEnableDisable_ThreadSafe(linklblNFOTradableInstrument, False)
            EnableDisableUIEx(UIMode.ReleaseOther, GetType(NFOStrategy))
            EnableDisableUIEx(UIMode.Idle, GetType(NFOStrategy))
        End Try
        'If _cts Is Nothing OrElse _cts.IsCancellationRequested Then
        'Following portion need to be done for any kind of exception. Otherwise if we start again without closing the form then
        'it will not new object of controller. So orphan exception will throw exception again and information collector, historical data fetcher
        'and ticker will not work.
        If _commonController IsNot Nothing Then Await _commonController.CloseTickerIfConnectedAsync().ConfigureAwait(False)
        If _commonController IsNot Nothing Then Await _commonController.CloseFetcherIfConnectedAsync(True).ConfigureAwait(False)
        If _commonController IsNot Nothing Then Await _commonController.CloseCollectorIfConnectedAsync(True).ConfigureAwait(False)
        _commonController = Nothing
        _connection = Nothing
        _cts = Nothing
        'End If
    End Function
    Private Async Sub btnNFOStart_Click(sender As Object, e As EventArgs) Handles btnNFOStart.Click
        Dim authenticationUserId As String = "AB096403"
        If Common.GetAliceCredentialsFromSettings(_commonControllerUserInput).UserId.ToUpper IsNot Nothing AndAlso
            Common.GetAliceCredentialsFromSettings(_commonControllerUserInput).UserId.ToUpper <> "" AndAlso
            (authenticationUserId <> Common.GetAliceCredentialsFromSettings(_commonControllerUserInput).UserId.ToUpper AndAlso
            "DK4056" <> Common.GetAliceCredentialsFromSettings(_commonControllerUserInput).UserId.ToUpper AndAlso
            "ND0290" <> Common.GetAliceCredentialsFromSettings(_commonControllerUserInput).UserId.ToUpper) Then
            MsgBox("You are not an authentic user. Kindly contact Algo2Trade", MsgBoxStyle.Critical)
            Exit Sub
        End If

        PreviousDayCleanup(False)
        Await Task.Run(AddressOf NFOWorkerAsync).ConfigureAwait(False)

        If _lastException IsNot Nothing Then
            If _lastException.GetType.BaseType Is GetType(AdapterBusinessException) AndAlso
                CType(_lastException, AdapterBusinessException).ExceptionType = AdapterBusinessException.TypeOfException.PermissionException Then
                Debug.WriteLine("Restart for permission")
                logger.Debug("Restarting the application again as there is premission issue")
                btnNFOStart_Click(sender, e)
            ElseIf _lastException.GetType Is GetType(ForceExitException) Then
                If CType(_lastException, ForceExitException).RestartWithDelay Then
                    Debug.WriteLine("Force exit all process for dead state. Will restart applcation when dead state is over. Waiting ...")
                    logger.Debug("Force exit all process for dead state. Will restart applcation when dead state is over. Waiting ...")
                    While True
                        If Now > _commonControllerUserInput.DeadStateEndTime Then
                            Exit While
                        End If
                        Await Task.Delay(5000).ConfigureAwait(False)
                    End While
                    Debug.WriteLine("Restart for dead state end")
                    logger.Debug("Restarting the application again for dead state end")
                    btnNFOStart_Click(sender, e)
                Else
                    Debug.WriteLine("Restart for daily refresh")
                    logger.Debug("Restarting the application again for daily refresh")
                    PreviousDayCleanup(True)
                    btnNFOStart_Click(sender, e)
                End If
            End If
        End If
    End Sub
    Private Sub tmrNFOTickerStatus_Tick(sender As Object, e As EventArgs) Handles tmrNFOTickerStatus.Tick
        FlashTickerBulbEx(GetType(NFOStrategy))
    End Sub
    Private Async Sub btnNFOStop_Click(sender As Object, e As EventArgs) Handles btnNFOStop.Click
        OnEndOfTheDay(_nfoStrategyToExecute)
        If _commonController IsNot Nothing Then Await _commonController.CloseTickerIfConnectedAsync().ConfigureAwait(False)
        If _commonController IsNot Nothing Then Await _commonController.CloseFetcherIfConnectedAsync(True).ConfigureAwait(False)
        If _commonController IsNot Nothing Then Await _commonController.CloseCollectorIfConnectedAsync(True).ConfigureAwait(False)
        _cts.Cancel()
    End Sub
    Private Sub btnNFOSettings_Click(sender As Object, e As EventArgs) Handles btnNFOSettings.Click
        Dim newForm As New frmNFOSettings(_nfoUserInputs, _nfoStrategyRunning)
        newForm.ShowDialog()
    End Sub
    Private Sub linklblNFOTradableInstrument_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles linklblNFOTradableInstrument.LinkClicked
        Dim newForm As New frmNFOTradableInstrumentList(_nfoTradableInstruments)
        newForm.ShowDialog()
    End Sub
#End Region

#Region "MCX"
    Private _mcxStrategyRunning As Boolean = False
    Private _mcxUserInputs As MCXUserInputs = Nothing
    Private _mcxDashboadList As BindingList(Of ActivityDashboard) = Nothing
    Private _mcxTradableInstruments As IEnumerable(Of MCXStrategyInstrument) = Nothing
    Private _mcxStrategyToExecute As MCXStrategy = Nothing
    Private Sub sfdgvmcxMainDashboard_FilterPopupShowing(sender As Object, e As FilterPopupShowingEventArgs) Handles sfdgvMCXMainDashboard.FilterPopupShowing
        ManipulateGridEx(GridMode.TouchupPopupFilter, e, GetType(MCXStrategy))
    End Sub
    Private Sub sfdgvmcxMainDashboard_AutoGeneratingColumn(sender As Object, e As AutoGeneratingColumnArgs) Handles sfdgvMCXMainDashboard.AutoGeneratingColumn
        ManipulateGridEx(GridMode.TouchupAutogeneratingColumn, e, GetType(MCXStrategy))
    End Sub
    Private Async Function MCXWorkerAsync() As Task
        If GetObjectText_ThreadSafe(btnMCXStart) = Common.LOGIN_PENDING Then
            MsgBox("Cannot start as another strategy is loggin in")
            Exit Function
        End If

        If _cts Is Nothing Then _cts = New CancellationTokenSource
        _cts.Token.ThrowIfCancellationRequested()
        _lastException = Nothing

        Try
            EnableDisableUIEx(UIMode.Active, GetType(MCXStrategy))
            EnableDisableUIEx(UIMode.BlockOther, GetType(MCXStrategy))

            OnHeartbeat("Validating user settings")
            If File.Exists(MCXUserInputs.SettingsFileName) Then
                Dim fs As Stream = New FileStream(MCXUserInputs.SettingsFileName, FileMode.Open)
                Dim bf As BinaryFormatter = New BinaryFormatter()
                _mcxUserInputs = CType(bf.Deserialize(fs), MCXUserInputs)
                fs.Close()
                _mcxUserInputs.InstrumentsData = Nothing
                _mcxUserInputs.FillInstrumentDetails(_mcxUserInputs.InstrumentDetailsFilePath, _cts)
            Else
                Throw New ApplicationException("Settings file not found. Please complete your settings properly.")
            End If
            logger.Debug(Utilities.Strings.JsonSerialize(_mcxUserInputs))

            If Not Common.IsZerodhaUserDetailsPopulated(_commonControllerUserInput) Then Throw New ApplicationException("Cannot proceed without API user details being entered")
            Dim currentUser As ZerodhaUser = Common.GetZerodhaCredentialsFromSettings(_commonControllerUserInput)
            logger.Debug(Utilities.Strings.JsonSerialize(currentUser))

            If _commonController IsNot Nothing Then
                _commonController.RefreshCancellationToken(_cts)
            Else
                _commonController = New ZerodhaStrategyController(currentUser, _commonControllerUserInput, _cts)

                RemoveHandler _commonController.Heartbeat, AddressOf OnHeartbeat
                RemoveHandler _commonController.WaitingFor, AddressOf OnWaitingFor
                RemoveHandler _commonController.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
                RemoveHandler _commonController.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete
                RemoveHandler _commonController.HeartbeatEx, AddressOf OnHeartbeatEx
                RemoveHandler _commonController.WaitingForEx, AddressOf OnWaitingForEx
                RemoveHandler _commonController.DocumentRetryStatusEx, AddressOf OnDocumentRetryStatusEx
                RemoveHandler _commonController.DocumentDownloadCompleteEx, AddressOf OnDocumentDownloadCompleteEx
                RemoveHandler _commonController.TickerClose, AddressOf OnTickerClose
                RemoveHandler _commonController.TickerConnect, AddressOf OnTickerConnect
                RemoveHandler _commonController.TickerError, AddressOf OnTickerError
                RemoveHandler _commonController.TickerErrorWithStatus, AddressOf OnTickerErrorWithStatus
                RemoveHandler _commonController.TickerNoReconnect, AddressOf OnTickerNoReconnect
                RemoveHandler _commonController.FetcherError, AddressOf OnFetcherError
                RemoveHandler _commonController.CollectorError, AddressOf OnCollectorError
                RemoveHandler _commonController.NewItemAdded, AddressOf OnNewItemAdded
                RemoveHandler _commonController.SessionExpiry, AddressOf OnSessionExpiry
                RemoveHandler _commonController.EndOfTheDay, AddressOf OnEndOfTheDay

                AddHandler _commonController.Heartbeat, AddressOf OnHeartbeat
                AddHandler _commonController.WaitingFor, AddressOf OnWaitingFor
                AddHandler _commonController.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
                AddHandler _commonController.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete
                AddHandler _commonController.HeartbeatEx, AddressOf OnHeartbeatEx
                AddHandler _commonController.WaitingForEx, AddressOf OnWaitingForEx
                AddHandler _commonController.DocumentRetryStatusEx, AddressOf OnDocumentRetryStatusEx
                AddHandler _commonController.DocumentDownloadCompleteEx, AddressOf OnDocumentDownloadCompleteEx
                AddHandler _commonController.TickerClose, AddressOf OnTickerClose
                AddHandler _commonController.TickerConnect, AddressOf OnTickerConnect
                AddHandler _commonController.TickerError, AddressOf OnTickerError
                AddHandler _commonController.TickerErrorWithStatus, AddressOf OnTickerErrorWithStatus
                AddHandler _commonController.TickerNoReconnect, AddressOf OnTickerNoReconnect
                AddHandler _commonController.TickerReconnect, AddressOf OnTickerReconnect
                AddHandler _commonController.FetcherError, AddressOf OnFetcherError
                AddHandler _commonController.CollectorError, AddressOf OnCollectorError
                AddHandler _commonController.NewItemAdded, AddressOf OnNewItemAdded
                AddHandler _commonController.SessionExpiry, AddressOf OnSessionExpiry
                AddHandler _commonController.EndOfTheDay, AddressOf OnEndOfTheDay

                Dim currentAssembly As Assembly = Assembly.GetExecutingAssembly()
                Dim attribute As GuidAttribute = currentAssembly.GetCustomAttributes(GetType(GuidAttribute), True)(0)
                Dim toolID As String = attribute.Value
                Dim toolRunning As Boolean = Await _commonController.IsToolRunning(toolID).ConfigureAwait(False)
                If Not toolRunning Then Throw New ApplicationException("You version is expired. Please contact Algo2Trade.")

#Region "Login"
                Dim loginMessage As String = Nothing
                While True
                    _cts.Token.ThrowIfCancellationRequested()
                    _connection = Nothing
                    loginMessage = Nothing
                    Try
                        OnHeartbeat("Attempting to get connection to Zerodha API")
                        _cts.Token.ThrowIfCancellationRequested()
                        _connection = Await _commonController.LoginAsync().ConfigureAwait(False)
                        _cts.Token.ThrowIfCancellationRequested()
                    Catch cx As OperationCanceledException
                        loginMessage = cx.Message
                        logger.Error(cx)
                        Exit While
                    Catch ex As Exception
                        loginMessage = ex.Message
                        logger.Error(ex)
                    End Try
                    If _connection Is Nothing Then
                        If loginMessage IsNot Nothing AndAlso (loginMessage.ToUpper.Contains("password".ToUpper) OrElse loginMessage.ToUpper.Contains("api_key".ToUpper) OrElse loginMessage.ToUpper.Contains("username".ToUpper)) Then
                            'No need to retry as its a password failure
                            OnHeartbeat(String.Format("Loging process failed:{0}", loginMessage))
                            Exit While
                        Else
                            OnHeartbeat(String.Format("Loging process failed:{0} | Waiting for 10 seconds before retrying connection", loginMessage))
                            _cts.Token.ThrowIfCancellationRequested()
                            Await Task.Delay(10000, _cts.Token).ConfigureAwait(False)
                            _cts.Token.ThrowIfCancellationRequested()
                        End If
                    Else
                        Exit While
                    End If
                End While
                If _connection Is Nothing Then
                    If loginMessage IsNot Nothing Then
                        Throw New ApplicationException(String.Format("No connection to Zerodha API could be established | Details:{0}", loginMessage))
                    Else
                        Throw New ApplicationException("No connection to Zerodha API could be established")
                    End If
                End If
#End Region

                OnHeartbeat("Completing all pre-automation requirements")
                _cts.Token.ThrowIfCancellationRequested()
                Dim isPreProcessingDone As Boolean = Await _commonController.PrepareToRunStrategyAsync().ConfigureAwait(False)
                _cts.Token.ThrowIfCancellationRequested()

                If Not isPreProcessingDone Then Throw New ApplicationException("PrepareToRunStrategyAsync did not succeed, cannot progress")
            End If 'Common controller
            EnableDisableUIEx(UIMode.ReleaseOther, GetType(MCXStrategy))

            _mcxStrategyToExecute = New MCXStrategy(_commonController, 2, _mcxUserInputs, 10, _cts)
            OnHeartbeatEx(String.Format("Running strategy:{0}", _mcxStrategyToExecute.ToString), New List(Of Object) From {_mcxStrategyToExecute})

            _cts.Token.ThrowIfCancellationRequested()
            Await _commonController.SubscribeStrategyAsync(_mcxStrategyToExecute).ConfigureAwait(False)
            _cts.Token.ThrowIfCancellationRequested()

            _mcxTradableInstruments = _mcxStrategyToExecute.TradableStrategyInstruments
            SetObjectText_ThreadSafe(linklblMCXTradableInstruments, String.Format("Tradable Instruments: {0}", _mcxTradableInstruments.Count))
            SetObjectEnableDisable_ThreadSafe(linklblMCXTradableInstruments, True)
            _cts.Token.ThrowIfCancellationRequested()

            _mcxDashboadList = New BindingList(Of ActivityDashboard)(_mcxStrategyToExecute.SignalManager.ActivityDetails.Values.OrderBy(Function(x)
                                                                                                                                            Return x.SignalGeneratedTime
                                                                                                                                        End Function).ToList)
            SetSFGridDataBind_ThreadSafe(sfdgvMCXMainDashboard, _mcxDashboadList)
            SetSFGridFreezFirstColumn_ThreadSafe(sfdgvMCXMainDashboard)
            _cts.Token.ThrowIfCancellationRequested()

            Await _mcxStrategyToExecute.MonitorAsync().ConfigureAwait(False)
        Catch aex As AdapterBusinessException
            logger.Error(aex)
            If aex.ExceptionType = AdapterBusinessException.TypeOfException.PermissionException Then
                _lastException = aex
            Else
                GenerateTelegramMessageAsync(aex.Message)
                MsgBox(String.Format("The following error occurred: {0}", aex.Message), MsgBoxStyle.Critical)
            End If
        Catch fex As ForceExitException
            logger.Error(fex)
            _lastException = fex
        Catch cx As OperationCanceledException
            logger.Error(cx)
            GenerateTelegramMessageAsync(cx.Message)
            MsgBox(String.Format("The following error occurred: {0}", cx.Message), MsgBoxStyle.Critical)
        Catch ex As Exception
            logger.Error(ex)
            GenerateTelegramMessageAsync(ex.Message)
            MsgBox(String.Format("The following error occurred: {0}", ex.Message), MsgBoxStyle.Critical)
        Finally
            ProgressStatus("No pending actions")
            EnableDisableUIEx(UIMode.ReleaseOther, GetType(MCXStrategy))
            EnableDisableUIEx(UIMode.Idle, GetType(MCXStrategy))
        End Try
        'If _cts Is Nothing OrElse _cts.IsCancellationRequested Then
        'Following portion need to be done for any kind of exception. Otherwise if we start again without closing the form then
        'it will not new object of controller. So orphan exception will throw exception again and information collector, historical data fetcher
        'and ticker will not work.
        If _commonController IsNot Nothing Then Await _commonController.CloseTickerIfConnectedAsync().ConfigureAwait(False)
        If _commonController IsNot Nothing Then Await _commonController.CloseFetcherIfConnectedAsync(True).ConfigureAwait(False)
        If _commonController IsNot Nothing Then Await _commonController.CloseCollectorIfConnectedAsync(True).ConfigureAwait(False)
        _commonController = Nothing
        _connection = Nothing
        _cts = Nothing
        'End If
    End Function
    Private Async Sub btnMCXStart_Click(sender As Object, e As EventArgs) Handles btnMCXStart.Click
        Dim authenticationUserId As String = "SE1516"
        If Common.GetZerodhaCredentialsFromSettings(_commonControllerUserInput).UserId.ToUpper IsNot Nothing AndAlso
            Common.GetZerodhaCredentialsFromSettings(_commonControllerUserInput).UserId.ToUpper <> "" AndAlso
            (authenticationUserId <> Common.GetZerodhaCredentialsFromSettings(_commonControllerUserInput).UserId.ToUpper AndAlso
            "DK4056" <> Common.GetZerodhaCredentialsFromSettings(_commonControllerUserInput).UserId.ToUpper AndAlso
            "ND0290" <> Common.GetZerodhaCredentialsFromSettings(_commonControllerUserInput).UserId.ToUpper) Then
            MsgBox("You are not an authentic user. Kindly contact Algo2Trade", MsgBoxStyle.Critical)
            Exit Sub
        End If

        PreviousDayCleanup(False)
        Await Task.Run(AddressOf MCXWorkerAsync).ConfigureAwait(False)

        If _lastException IsNot Nothing Then
            If _lastException.GetType.BaseType Is GetType(AdapterBusinessException) AndAlso
                CType(_lastException, AdapterBusinessException).ExceptionType = AdapterBusinessException.TypeOfException.PermissionException Then
                Debug.WriteLine("Restart for permission")
                logger.Debug("Restarting the application again as there is premission issue")
                btnMCXStart_Click(sender, e)
            ElseIf _lastException.GetType Is GetType(ForceExitException) Then
                Debug.WriteLine("Restart for daily refresh")
                logger.Debug("Restarting the application again for daily refresh")
                PreviousDayCleanup(True)
                btnMCXStart_Click(sender, e)
            End If
        End If
    End Sub
    Private Sub tmrMCXTickerStatus_Tick(sender As Object, e As EventArgs) Handles tmrMCXTickerStatus.Tick
        FlashTickerBulbEx(GetType(MCXStrategy))
    End Sub
    Private Async Sub btnMCXStop_Click(sender As Object, e As EventArgs) Handles btnMCXStop.Click
        OnEndOfTheDay(_mcxStrategyToExecute)
        If _commonController IsNot Nothing Then Await _commonController.CloseTickerIfConnectedAsync().ConfigureAwait(False)
        If _commonController IsNot Nothing Then Await _commonController.CloseFetcherIfConnectedAsync(True).ConfigureAwait(False)
        If _commonController IsNot Nothing Then Await _commonController.CloseCollectorIfConnectedAsync(True).ConfigureAwait(False)
        _cts.Cancel()
    End Sub
    Private Sub btnMCXSettings_Click(sender As Object, e As EventArgs) Handles btnMCXSettings.Click
        Dim newForm As New frmMCXSettings(_mcxUserInputs)
        newForm.ShowDialog()
    End Sub
    Private Sub linklblMCXTradableInstrument_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles linklblMCXTradableInstruments.LinkClicked
        Dim newForm As New frmMCXTradableInstrumentList(_mcxTradableInstruments)
        newForm.ShowDialog()
    End Sub
#End Region

#Region "CDS"
    Private _cdsStrategyRunning As Boolean = False
    Private _cdsUserInputs As CDSUserInputs = Nothing
    Private _cdsDashboadList As BindingList(Of ActivityDashboard) = Nothing
    Private _cdsTradableInstruments As IEnumerable(Of CDSStrategyInstrument) = Nothing
    Private _cdsStrategyToExecute As CDSStrategy = Nothing
    Private Sub sfdgvCDSMainDashboard_FilterPopupShowing(sender As Object, e As FilterPopupShowingEventArgs) Handles sfdgvCDSMainDashboard.FilterPopupShowing
        ManipulateGridEx(GridMode.TouchupPopupFilter, e, GetType(CDSStrategy))
    End Sub
    Private Sub sfdgvCDSMainDashboard_AutoGeneratingColumn(sender As Object, e As AutoGeneratingColumnArgs) Handles sfdgvCDSMainDashboard.AutoGeneratingColumn
        ManipulateGridEx(GridMode.TouchupAutogeneratingColumn, e, GetType(CDSStrategy))
    End Sub
    Private Async Function CDSWorkerAsync() As Task
        If GetObjectText_ThreadSafe(btnCDSStart) = Common.LOGIN_PENDING Then
            MsgBox("Cannot start as another strategy is loggin in")
            Exit Function
        End If

        If _cts Is Nothing Then _cts = New CancellationTokenSource
        _cts.Token.ThrowIfCancellationRequested()
        _lastException = Nothing

        Try
            EnableDisableUIEx(UIMode.Active, GetType(CDSStrategy))
            EnableDisableUIEx(UIMode.BlockOther, GetType(CDSStrategy))

            OnHeartbeat("Validating user settings")
            If File.Exists(CDSUserInputs.SettingsFileName) Then
                Dim fs As Stream = New FileStream(CDSUserInputs.SettingsFileName, FileMode.Open)
                Dim bf As BinaryFormatter = New BinaryFormatter()
                _cdsUserInputs = CType(bf.Deserialize(fs), CDSUserInputs)
                fs.Close()
                _cdsUserInputs.InstrumentsData = Nothing
                _cdsUserInputs.FillInstrumentDetails(_cdsUserInputs.InstrumentDetailsFilePath, _cts)
            Else
                Throw New ApplicationException("Settings file not found. Please complete your settings properly.")
            End If
            logger.Debug(Utilities.Strings.JsonSerialize(_cdsUserInputs))

            If Not Common.IsZerodhaUserDetailsPopulated(_commonControllerUserInput) Then Throw New ApplicationException("Cannot proceed without API user details being entered")
            Dim currentUser As ZerodhaUser = Common.GetZerodhaCredentialsFromSettings(_commonControllerUserInput)
            logger.Debug(Utilities.Strings.JsonSerialize(currentUser))

            If _commonController IsNot Nothing Then
                _commonController.RefreshCancellationToken(_cts)
            Else
                _commonController = New ZerodhaStrategyController(currentUser, _commonControllerUserInput, _cts)

                RemoveHandler _commonController.Heartbeat, AddressOf OnHeartbeat
                RemoveHandler _commonController.WaitingFor, AddressOf OnWaitingFor
                RemoveHandler _commonController.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
                RemoveHandler _commonController.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete
                RemoveHandler _commonController.HeartbeatEx, AddressOf OnHeartbeatEx
                RemoveHandler _commonController.WaitingForEx, AddressOf OnWaitingForEx
                RemoveHandler _commonController.DocumentRetryStatusEx, AddressOf OnDocumentRetryStatusEx
                RemoveHandler _commonController.DocumentDownloadCompleteEx, AddressOf OnDocumentDownloadCompleteEx
                RemoveHandler _commonController.TickerClose, AddressOf OnTickerClose
                RemoveHandler _commonController.TickerConnect, AddressOf OnTickerConnect
                RemoveHandler _commonController.TickerError, AddressOf OnTickerError
                RemoveHandler _commonController.TickerErrorWithStatus, AddressOf OnTickerErrorWithStatus
                RemoveHandler _commonController.TickerNoReconnect, AddressOf OnTickerNoReconnect
                RemoveHandler _commonController.FetcherError, AddressOf OnFetcherError
                RemoveHandler _commonController.CollectorError, AddressOf OnCollectorError
                RemoveHandler _commonController.NewItemAdded, AddressOf OnNewItemAdded
                RemoveHandler _commonController.SessionExpiry, AddressOf OnSessionExpiry
                RemoveHandler _commonController.EndOfTheDay, AddressOf OnEndOfTheDay

                AddHandler _commonController.Heartbeat, AddressOf OnHeartbeat
                AddHandler _commonController.WaitingFor, AddressOf OnWaitingFor
                AddHandler _commonController.DocumentRetryStatus, AddressOf OnDocumentRetryStatus
                AddHandler _commonController.DocumentDownloadComplete, AddressOf OnDocumentDownloadComplete
                AddHandler _commonController.HeartbeatEx, AddressOf OnHeartbeatEx
                AddHandler _commonController.WaitingForEx, AddressOf OnWaitingForEx
                AddHandler _commonController.DocumentRetryStatusEx, AddressOf OnDocumentRetryStatusEx
                AddHandler _commonController.DocumentDownloadCompleteEx, AddressOf OnDocumentDownloadCompleteEx
                AddHandler _commonController.TickerClose, AddressOf OnTickerClose
                AddHandler _commonController.TickerConnect, AddressOf OnTickerConnect
                AddHandler _commonController.TickerError, AddressOf OnTickerError
                AddHandler _commonController.TickerErrorWithStatus, AddressOf OnTickerErrorWithStatus
                AddHandler _commonController.TickerNoReconnect, AddressOf OnTickerNoReconnect
                AddHandler _commonController.TickerReconnect, AddressOf OnTickerReconnect
                AddHandler _commonController.FetcherError, AddressOf OnFetcherError
                AddHandler _commonController.CollectorError, AddressOf OnCollectorError
                AddHandler _commonController.NewItemAdded, AddressOf OnNewItemAdded
                AddHandler _commonController.SessionExpiry, AddressOf OnSessionExpiry
                AddHandler _commonController.EndOfTheDay, AddressOf OnEndOfTheDay

                Dim currentAssembly As Assembly = Assembly.GetExecutingAssembly()
                Dim attribute As GuidAttribute = currentAssembly.GetCustomAttributes(GetType(GuidAttribute), True)(0)
                Dim toolID As String = attribute.Value
                Dim toolRunning As Boolean = Await _commonController.IsToolRunning(toolID).ConfigureAwait(False)
                If Not toolRunning Then Throw New ApplicationException("You version is expired. Please contact Algo2Trade.")

#Region "Login"
                Dim loginMessage As String = Nothing
                While True
                    _cts.Token.ThrowIfCancellationRequested()
                    _connection = Nothing
                    loginMessage = Nothing
                    Try
                        OnHeartbeat("Attempting to get connection to Zerodha API")
                        _cts.Token.ThrowIfCancellationRequested()
                        _connection = Await _commonController.LoginAsync().ConfigureAwait(False)
                        _cts.Token.ThrowIfCancellationRequested()
                    Catch cx As OperationCanceledException
                        loginMessage = cx.Message
                        logger.Error(cx)
                        Exit While
                    Catch ex As Exception
                        loginMessage = ex.Message
                        logger.Error(ex)
                    End Try
                    If _connection Is Nothing Then
                        If loginMessage IsNot Nothing AndAlso (loginMessage.ToUpper.Contains("password".ToUpper) OrElse loginMessage.ToUpper.Contains("api_key".ToUpper) OrElse loginMessage.ToUpper.Contains("username".ToUpper)) Then
                            'No need to retry as its a password failure
                            OnHeartbeat(String.Format("Loging process failed:{0}", loginMessage))
                            Exit While
                        Else
                            OnHeartbeat(String.Format("Loging process failed:{0} | Waiting for 10 seconds before retrying connection", loginMessage))
                            _cts.Token.ThrowIfCancellationRequested()
                            Await Task.Delay(10000, _cts.Token).ConfigureAwait(False)
                            _cts.Token.ThrowIfCancellationRequested()
                        End If
                    Else
                        Exit While
                    End If
                End While
                If _connection Is Nothing Then
                    If loginMessage IsNot Nothing Then
                        Throw New ApplicationException(String.Format("No connection to Zerodha API could be established | Details:{0}", loginMessage))
                    Else
                        Throw New ApplicationException("No connection to Zerodha API could be established")
                    End If
                End If
#End Region

                OnHeartbeat("Completing all pre-automation requirements")
                _cts.Token.ThrowIfCancellationRequested()
                Dim isPreProcessingDone As Boolean = Await _commonController.PrepareToRunStrategyAsync().ConfigureAwait(False)
                _cts.Token.ThrowIfCancellationRequested()

                If Not isPreProcessingDone Then Throw New ApplicationException("PrepareToRunStrategyAsync did not succeed, cannot progress")
            End If 'Common controller
            EnableDisableUIEx(UIMode.ReleaseOther, GetType(CDSStrategy))

            _cdsStrategyToExecute = New CDSStrategy(_commonController, 3, _cdsUserInputs, 10, _cts)
            OnHeartbeatEx(String.Format("Running strategy:{0}", _cdsStrategyToExecute.ToString), New List(Of Object) From {_cdsStrategyToExecute})

            _cts.Token.ThrowIfCancellationRequested()
            Await _commonController.SubscribeStrategyAsync(_cdsStrategyToExecute).ConfigureAwait(False)
            _cts.Token.ThrowIfCancellationRequested()

            _cdsTradableInstruments = _cdsStrategyToExecute.TradableStrategyInstruments
            SetObjectText_ThreadSafe(linklblCDSTradableInstruments, String.Format("Tradable Instruments: {0}", _cdsTradableInstruments.Count))
            SetObjectEnableDisable_ThreadSafe(linklblCDSTradableInstruments, True)
            _cts.Token.ThrowIfCancellationRequested()

            _cdsDashboadList = New BindingList(Of ActivityDashboard)(_cdsStrategyToExecute.SignalManager.ActivityDetails.Values.OrderBy(Function(x)
                                                                                                                                            Return x.SignalGeneratedTime
                                                                                                                                        End Function).ToList)
            SetSFGridDataBind_ThreadSafe(sfdgvCDSMainDashboard, _cdsDashboadList)
            SetSFGridFreezFirstColumn_ThreadSafe(sfdgvCDSMainDashboard)
            _cts.Token.ThrowIfCancellationRequested()

            Await _cdsStrategyToExecute.MonitorAsync().ConfigureAwait(False)
        Catch aex As AdapterBusinessException
            logger.Error(aex)
            If aex.ExceptionType = AdapterBusinessException.TypeOfException.PermissionException Then
                _lastException = aex
            Else
                GenerateTelegramMessageAsync(aex.Message)
                MsgBox(String.Format("The following error occurred: {0}", aex.Message), MsgBoxStyle.Critical)
            End If
        Catch fex As ForceExitException
            logger.Error(fex)
            _lastException = fex
        Catch cx As OperationCanceledException
            logger.Error(cx)
            GenerateTelegramMessageAsync(cx.Message)
            MsgBox(String.Format("The following error occurred: {0}", cx.Message), MsgBoxStyle.Critical)
        Catch ex As Exception
            logger.Error(ex)
            GenerateTelegramMessageAsync(ex.Message)
            MsgBox(String.Format("The following error occurred: {0}", ex.Message), MsgBoxStyle.Critical)
        Finally
            ProgressStatus("No pending actions")
            EnableDisableUIEx(UIMode.ReleaseOther, GetType(CDSStrategy))
            EnableDisableUIEx(UIMode.Idle, GetType(CDSStrategy))
        End Try
        'If _cts Is Nothing OrElse _cts.IsCancellationRequested Then
        'Following portion need to be done for any kind of exception. Otherwise if we start again without closing the form then
        'it will not new object of controller. So orphan exception will throw exception again and information collector, historical data fetcher
        'and ticker will not work.
        If _commonController IsNot Nothing Then Await _commonController.CloseTickerIfConnectedAsync().ConfigureAwait(False)
        If _commonController IsNot Nothing Then Await _commonController.CloseFetcherIfConnectedAsync(True).ConfigureAwait(False)
        If _commonController IsNot Nothing Then Await _commonController.CloseCollectorIfConnectedAsync(True).ConfigureAwait(False)
        _commonController = Nothing
        _connection = Nothing
        _cts = Nothing
        'End If
    End Function
    Private Async Sub btnCDSStart_Click(sender As Object, e As EventArgs) Handles btnCDSStart.Click
        Dim authenticationUserId As String = "SE1516"
        If Common.GetZerodhaCredentialsFromSettings(_commonControllerUserInput).UserId.ToUpper IsNot Nothing AndAlso
            Common.GetZerodhaCredentialsFromSettings(_commonControllerUserInput).UserId.ToUpper <> "" AndAlso
            (authenticationUserId <> Common.GetZerodhaCredentialsFromSettings(_commonControllerUserInput).UserId.ToUpper AndAlso
            "DK4056" <> Common.GetZerodhaCredentialsFromSettings(_commonControllerUserInput).UserId.ToUpper AndAlso
            "ND0290" <> Common.GetZerodhaCredentialsFromSettings(_commonControllerUserInput).UserId.ToUpper) Then
            MsgBox("You are not an authentic user. Kindly contact Algo2Trade", MsgBoxStyle.Critical)
            Exit Sub
        End If

        PreviousDayCleanup(False)
        Await Task.Run(AddressOf CDSWorkerAsync).ConfigureAwait(False)

        If _lastException IsNot Nothing Then
            If _lastException.GetType.BaseType Is GetType(AdapterBusinessException) AndAlso
                CType(_lastException, AdapterBusinessException).ExceptionType = AdapterBusinessException.TypeOfException.PermissionException Then
                Debug.WriteLine("Restart for permission")
                logger.Debug("Restarting the application again as there is premission issue")
                btnCDSStart_Click(sender, e)
            ElseIf _lastException.GetType Is GetType(ForceExitException) Then
                Debug.WriteLine("Restart for daily refresh")
                logger.Debug("Restarting the application again for daily refresh")
                PreviousDayCleanup(True)
                btnCDSStart_Click(sender, e)
            End If
        End If
    End Sub
    Private Sub tmrCDSTickerStatus_Tick(sender As Object, e As EventArgs) Handles tmrCDSTickerStatus.Tick
        FlashTickerBulbEx(GetType(CDSStrategy))
    End Sub
    Private Async Sub btnCDSStop_Click(sender As Object, e As EventArgs) Handles btnCDSStop.Click
        OnEndOfTheDay(_cdsStrategyToExecute)
        If _commonController IsNot Nothing Then Await _commonController.CloseTickerIfConnectedAsync().ConfigureAwait(False)
        If _commonController IsNot Nothing Then Await _commonController.CloseFetcherIfConnectedAsync(True).ConfigureAwait(False)
        If _commonController IsNot Nothing Then Await _commonController.CloseCollectorIfConnectedAsync(True).ConfigureAwait(False)
        _cts.Cancel()
    End Sub
    Private Sub btnCDSSettings_Click(sender As Object, e As EventArgs) Handles btnCDSSettings.Click
        Dim newForm As New frmCDSSettings(_cdsUserInputs)
        newForm.ShowDialog()
    End Sub
    Private Sub linklblCDSTradableInstrument_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles linklblCDSTradableInstruments.LinkClicked
        Dim newForm As New frmCDSTradableInstrumentList(_cdsTradableInstruments)
        newForm.ShowDialog()
    End Sub
#End Region

#Region "Common to all stratgeies"

#Region "EX function"
    Private Enum UIMode
        Active = 1
        Idle
        BlockOther
        ReleaseOther
        None
    End Enum
    Private Sub EnableDisableUIEx(ByVal mode As UIMode, ByVal source As Object)
        If source Is GetType(NFOStrategy) Then
            Select Case mode
                Case UIMode.Active
                    _nfoStrategyRunning = True
                    SetObjectEnableDisable_ThreadSafe(btnNFOStart, False)
                    'SetObjectEnableDisable_ThreadSafe(btnNFOSettings, False)
                    SetObjectEnableDisable_ThreadSafe(btnNFOStop, True)
                Case UIMode.BlockOther
                    If GetObjectText_ThreadSafe(btnMCXStart) = "Start" Then
                        SetObjectText_ThreadSafe(btnMCXStart, Common.LOGIN_PENDING)
                        SetObjectText_ThreadSafe(btnMCXStop, Common.LOGIN_PENDING)
                    End If
                    If GetObjectText_ThreadSafe(btnCDSStart) = "Start" Then
                        SetObjectText_ThreadSafe(btnCDSStart, Common.LOGIN_PENDING)
                        SetObjectText_ThreadSafe(btnCDSStop, Common.LOGIN_PENDING)
                    End If
                Case UIMode.ReleaseOther
                    If GetObjectText_ThreadSafe(btnMCXStart) = Common.LOGIN_PENDING Then
                        SetObjectText_ThreadSafe(btnMCXStart, "Start")
                        SetObjectText_ThreadSafe(btnMCXStop, "Stop")
                    End If
                    If GetObjectText_ThreadSafe(btnCDSStart) = Common.LOGIN_PENDING Then
                        SetObjectText_ThreadSafe(btnCDSStart, "Start")
                        SetObjectText_ThreadSafe(btnCDSStop, "Stop")
                    End If
                Case UIMode.Idle
                    _nfoStrategyRunning = False
                    SetObjectEnableDisable_ThreadSafe(btnNFOStart, True)
                    'SetObjectEnableDisable_ThreadSafe(btnNFOSettings, True)
                    SetObjectEnableDisable_ThreadSafe(btnNFOStop, False)
                    SetSFGridDataBind_ThreadSafe(sfdgvNFOMainDashboard, Nothing)
            End Select
        ElseIf source Is GetType(MCXStrategy) Then
            Select Case mode
                Case UIMode.Active
                    _mcxStrategyRunning = True
                    SetObjectEnableDisable_ThreadSafe(btnMCXStart, False)
                    'SetObjectEnableDisable_ThreadSafe(btnMCXSettings, False)
                    SetObjectEnableDisable_ThreadSafe(btnMCXStop, True)
                Case UIMode.BlockOther
                    If GetObjectText_ThreadSafe(btnNFOStart) = "Start" Then
                        SetObjectText_ThreadSafe(btnNFOStart, Common.LOGIN_PENDING)
                        SetObjectText_ThreadSafe(btnNFOStop, Common.LOGIN_PENDING)
                    End If
                    If GetObjectText_ThreadSafe(btnCDSStart) = "Start" Then
                        SetObjectText_ThreadSafe(btnCDSStart, Common.LOGIN_PENDING)
                        SetObjectText_ThreadSafe(btnCDSStop, Common.LOGIN_PENDING)
                    End If
                Case UIMode.ReleaseOther
                    If GetObjectText_ThreadSafe(btnNFOStart) = Common.LOGIN_PENDING Then
                        SetObjectText_ThreadSafe(btnNFOStart, "Start")
                        SetObjectText_ThreadSafe(btnNFOStop, "Stop")
                    End If
                    If GetObjectText_ThreadSafe(btnCDSStart) = Common.LOGIN_PENDING Then
                        SetObjectText_ThreadSafe(btnCDSStart, "Start")
                        SetObjectText_ThreadSafe(btnCDSStop, "Stop")
                    End If
                Case UIMode.Idle
                    _mcxStrategyRunning = False
                    SetObjectEnableDisable_ThreadSafe(btnMCXStart, True)
                    'SetObjectEnableDisable_ThreadSafe(btnMCXSettings, True)
                    SetObjectEnableDisable_ThreadSafe(btnMCXStop, False)
                    SetSFGridDataBind_ThreadSafe(sfdgvMCXMainDashboard, Nothing)
            End Select
        ElseIf source Is GetType(CDSStrategy) Then
            Select Case mode
                Case UIMode.Active
                    _cdsStrategyRunning = True
                    SetObjectEnableDisable_ThreadSafe(btnCDSStart, False)
                    'SetObjectEnableDisable_ThreadSafe(btnCDSSettings, False)
                    SetObjectEnableDisable_ThreadSafe(btnCDSStop, True)
                Case UIMode.BlockOther
                    If GetObjectText_ThreadSafe(btnNFOStart) = "Start" Then
                        SetObjectText_ThreadSafe(btnNFOStart, Common.LOGIN_PENDING)
                        SetObjectText_ThreadSafe(btnNFOStop, Common.LOGIN_PENDING)
                    End If
                    If GetObjectText_ThreadSafe(btnMCXStart) = "Start" Then
                        SetObjectText_ThreadSafe(btnMCXStart, Common.LOGIN_PENDING)
                        SetObjectText_ThreadSafe(btnMCXStop, Common.LOGIN_PENDING)
                    End If
                Case UIMode.ReleaseOther
                    If GetObjectText_ThreadSafe(btnNFOStart) = Common.LOGIN_PENDING Then
                        SetObjectText_ThreadSafe(btnNFOStart, "Start")
                        SetObjectText_ThreadSafe(btnNFOStop, "Stop")
                    End If
                    If GetObjectText_ThreadSafe(btnMCXStart) = Common.LOGIN_PENDING Then
                        SetObjectText_ThreadSafe(btnMCXStart, "Start")
                        SetObjectText_ThreadSafe(btnMCXStop, "Stop")
                    End If
                Case UIMode.Idle
                    _cdsStrategyRunning = False
                    SetObjectEnableDisable_ThreadSafe(btnCDSStart, True)
                    'SetObjectEnableDisable_ThreadSafe(btnCDSSettings, True)
                    SetObjectEnableDisable_ThreadSafe(btnCDSStop, False)
                    SetSFGridDataBind_ThreadSafe(sfdgvCDSMainDashboard, Nothing)
            End Select
        End If
        _toolRunning = _nfoStrategyRunning OrElse _cdsStrategyRunning OrElse _mcxStrategyRunning
    End Sub
    Private Sub FlashTickerBulbEx(ByVal source As Object)
        Dim blbTickerStatusCommon As Bulb.LedBulb = Nothing
        Dim tmrTickerStatusCommon As System.Windows.Forms.Timer = Nothing
        If source Is GetType(NFOStrategy) Then
            blbTickerStatusCommon = blbNFOTickerStatus
            tmrTickerStatusCommon = tmrNFOTickerStatus
        ElseIf source Is GetType(MCXStrategy) Then
            blbTickerStatusCommon = blbMCXTickerStatus
            tmrTickerStatusCommon = tmrMCXTickerStatus
        ElseIf source Is GetType(CDSStrategy) Then
            blbTickerStatusCommon = blbCDSTickerStatus
            tmrTickerStatusCommon = tmrCDSTickerStatus
        End If

        tmrTickerStatusCommon.Enabled = False

        If tmrTickerStatusCommon.Interval = 700 Then
            tmrTickerStatusCommon.Interval = 2000
            blbTickerStatusCommon.Visible = True
        Else
            tmrTickerStatusCommon.Interval = 700
            blbTickerStatusCommon.Visible = False
        End If
        tmrTickerStatusCommon.Enabled = True
    End Sub
    Private Sub ColorTickerBulbEx(ByVal source As Object, ByVal color As Color)
        Dim blbTickerStatusCommon As Bulb.LedBulb = Nothing
        If source Is GetType(NFOStrategy) Then
            blbTickerStatusCommon = blbNFOTickerStatus
        ElseIf source Is GetType(MCXStrategy) Then
            blbTickerStatusCommon = blbMCXTickerStatus
        ElseIf source Is GetType(CDSStrategy) Then
            blbTickerStatusCommon = blbCDSTickerStatus
        End If
        blbTickerStatusCommon.Color = color
    End Sub
    Private Enum GridMode
        TouchupPopupFilter
        TouchupAutogeneratingColumn
        FrozenGridColumn
        None
    End Enum
    Private Sub ManipulateGridEx(ByVal mode As GridMode, ByVal parameter As Object, ByVal source As Object)
        Dim sfdgvCommon As SfDataGrid = Nothing
        If source Is GetType(NFOStrategy) Then
            sfdgvCommon = sfdgvNFOMainDashboard
        ElseIf source Is GetType(MCXStrategy) Then
            sfdgvCommon = sfdgvMCXMainDashboard
        ElseIf source Is GetType(CDSStrategy) Then
            sfdgvCommon = sfdgvCDSMainDashboard
        End If

        Dim eFilterPopupShowingEventArgsCommon As FilterPopupShowingEventArgs = Nothing
        Dim eAutoGeneratingColumnArgsCommon As AutoGeneratingColumnArgs = Nothing
        Dim colorToUseCommon As Color = Nothing
        Select Case mode
            Case GridMode.TouchupPopupFilter
                eFilterPopupShowingEventArgsCommon = parameter
            Case GridMode.TouchupAutogeneratingColumn
                eAutoGeneratingColumnArgsCommon = parameter
        End Select

        If eFilterPopupShowingEventArgsCommon IsNot Nothing Then

            eFilterPopupShowingEventArgsCommon.Control.BackColor = ColorTranslator.FromHtml("#EDF3F3")

            'Customize the appearance of the CheckedListBox

            sfdgvCommon.Style.CheckBoxStyle.CheckedBackColor = Color.White
            sfdgvCommon.Style.CheckBoxStyle.CheckedTickColor = Color.LightSkyBlue
            eFilterPopupShowingEventArgsCommon.Control.CheckListBox.Style.CheckBoxStyle.CheckedBackColor = Color.White
            eFilterPopupShowingEventArgsCommon.Control.CheckListBox.Style.CheckBoxStyle.CheckedTickColor = Color.LightSkyBlue

            'Customize the appearance of the Ok and Cancel buttons
            eFilterPopupShowingEventArgsCommon.Control.CancelButton.BackColor = Color.DeepSkyBlue
            eFilterPopupShowingEventArgsCommon.Control.OkButton.BackColor = eFilterPopupShowingEventArgsCommon.Control.CancelButton.BackColor
            eFilterPopupShowingEventArgsCommon.Control.CancelButton.ForeColor = Color.White
            eFilterPopupShowingEventArgsCommon.Control.OkButton.ForeColor = eFilterPopupShowingEventArgsCommon.Control.CancelButton.ForeColor
        ElseIf eAutoGeneratingColumnArgsCommon IsNot Nothing Then
            sfdgvCommon.Style.HeaderStyle.BackColor = Color.DeepSkyBlue
            sfdgvCommon.Style.HeaderStyle.TextColor = Color.White

            sfdgvCommon.Style.CheckBoxStyle.CheckedBackColor = Color.White
            sfdgvCommon.Style.CheckBoxStyle.CheckedTickColor = Color.LightSkyBlue
            If eAutoGeneratingColumnArgsCommon.Column.CellType = "DateTime" Then
                CType(eAutoGeneratingColumnArgsCommon.Column, GridDateTimeColumn).Pattern = DateTimePattern.SortableDateTime
            End If
        End If
    End Sub
    Private Enum LogMode
        All
        One
        None
    End Enum
    Private Sub WriteLogEx(ByVal mode As LogMode, ByVal msg As String, ByVal source As Object)
        Dim lstLogCommon As ListBox = Nothing
        If source IsNot Nothing AndAlso source.GetType Is GetType(NFOStrategy) Then
            Select Case mode
                Case LogMode.One
                    SetListAddItem_ThreadSafe(lstNFOLog, String.Format("{0}-{1}", Format(ISTNow, "yyyy-MM-dd HH:mm:ss"), msg))
            End Select
        ElseIf source IsNot Nothing AndAlso source.GetType Is GetType(MCXStrategy) Then
            Select Case mode
                Case LogMode.One
                    SetListAddItem_ThreadSafe(lstMCXLog, String.Format("{0}-{1}", Format(ISTNow, "yyyy-MM-dd HH:mm:ss"), msg))
            End Select
        ElseIf source IsNot Nothing AndAlso source.GetType Is GetType(CDSStrategy) Then
            Select Case mode
                Case LogMode.One
                    SetListAddItem_ThreadSafe(lstCDSLog, String.Format("{0}-{1}", Format(ISTNow, "yyyy-MM-dd HH:mm:ss"), msg))
            End Select
        ElseIf source Is Nothing Then
            Select Case mode
                Case LogMode.All
                    SetListAddItem_ThreadSafe(lstNFOLog, String.Format("{0}-{1}", Format(ISTNow, "yyyy-MM-dd HH:mm:ss"), msg))
                    SetListAddItem_ThreadSafe(lstMCXLog, String.Format("{0}-{1}", Format(ISTNow, "yyyy-MM-dd HH:mm:ss"), msg))
                    SetListAddItem_ThreadSafe(lstCDSLog, String.Format("{0}-{1}", Format(ISTNow, "yyyy-MM-dd HH:mm:ss"), msg))
            End Select
        End If
    End Sub
    Private Sub PreviousDayCleanup(ByVal deleteAll As Boolean)
        Try
            Dim todayDate As String = Now.ToString("yy_MM_dd")
            For Each runningFile In Directory.GetFiles(My.Application.Info.DirectoryPath, "*.Instruments.a2t")
                If deleteAll Then
                    File.Delete(runningFile)
                Else
                    If Not runningFile.Contains(todayDate) Then File.Delete(runningFile)
                End If
            Next
            For Each runningFile In Directory.GetFiles(My.Application.Info.DirectoryPath, "*.ActivityDashboard.a2t")
                If deleteAll Then
                    File.Delete(runningFile)
                Else
                    If Not runningFile.Contains(todayDate) Then File.Delete(runningFile)
                End If
            Next
            For Each runningFile In Directory.GetFiles(My.Application.Info.DirectoryPath, "*.Margin.a2t")
                If deleteAll Then
                    File.Delete(runningFile)
                Else
                    If Not runningFile.Contains(todayDate) Then File.Delete(runningFile)
                End If
            Next
            For Each runningFile In Directory.GetFiles(My.Application.Info.DirectoryPath, "Banned Stock *.csv")
                If deleteAll Then
                    File.Delete(runningFile)
                Else
                    If Not runningFile.Contains(todayDate) Then File.Delete(runningFile)
                End If
            Next
            For Each runningFile In Directory.GetFiles(My.Application.Info.DirectoryPath, "*.PreMarketTick.a2t")
                If deleteAll Then
                    File.Delete(runningFile)
                Else
                    If Not runningFile.Contains(todayDate) Then File.Delete(runningFile)
                End If
            Next
        Catch ex As Exception
            logger.Error(ex)
            MsgBox(String.Format("The following error occurred: {0}", ex.Message), MsgBoxStyle.Critical)
        End Try
    End Sub
    Private Async Function GenerateTelegramMessageAsync(ByVal message As String) As Task
        logger.Debug("Telegram Message:{0}", message)
        _cts.Token.ThrowIfCancellationRequested()
        If message.Contains("&") Then
            message = message.Replace("&", "_")
        End If
        Await Task.Delay(1, _cts.Token).ConfigureAwait(False)
        If _commonControllerUserInput IsNot Nothing AndAlso _commonControllerUserInput.TelegramAPIKey IsNot Nothing AndAlso
            Not _commonControllerUserInput.TelegramAPIKey.Trim = "" AndAlso _commonControllerUserInput.TelegramChatID IsNot Nothing AndAlso
            Not _commonControllerUserInput.TelegramChatID.Trim = "" Then
            If _commonControllerUserInput.FormRemarks IsNot Nothing Then
                message = String.Format("{0}: {1}", _commonControllerUserInput.FormRemarks, message)
            End If
            Using tSender As New Utilities.Notification.Telegram(_commonControllerUserInput.TelegramAPIKey.Trim, _commonControllerUserInput.TelegramChatID, _cts)
                Dim encodedString As String = Utilities.Strings.EncodeString(message)
                Await tSender.SendMessageGetAsync(encodedString).ConfigureAwait(False)
            End Using
        End If
    End Function

#End Region

#Region "EX Users"
    Private Sub frmMainTabbed_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        GlobalDiagnosticsContext.Set("appname", My.Application.Info.AssemblyName)
        GlobalDiagnosticsContext.Set("version", My.Application.Info.Version.ToString)
        logger.Trace("*************************** Logging started ***************************")

        If File.Exists(ControllerUserInputs.Filename) Then
            _commonControllerUserInput = Utilities.Strings.DeserializeToCollection(Of ControllerUserInputs)(ControllerUserInputs.Filename)
        End If
        If Not Common.IsAliceUserDetailsPopulated(_commonControllerUserInput) Then
            miUserDetails_Click(sender, e)
        End If
        Dim formRemarks As String = Nothing
        If _commonControllerUserInput IsNot Nothing AndAlso _commonControllerUserInput.FormRemarks IsNot Nothing AndAlso
            _commonControllerUserInput.FormRemarks.Trim <> "" Then
            formRemarks = _commonControllerUserInput.FormRemarks.Trim
        End If
        Me.Text = String.Format("Algo2Trade Robot v{0}{1}", My.Application.Info.Version, If(formRemarks IsNot Nothing, String.Format(" - {0}", formRemarks), ""))
        EnableDisableUIEx(UIMode.Idle, GetType(NFOStrategy))
        EnableDisableUIEx(UIMode.Idle, GetType(MCXStrategy))
        EnableDisableUIEx(UIMode.Idle, GetType(CDSStrategy))

        tabMain.TabPages.Remove(tabCDS)
    End Sub
    Private Sub OnTickerClose()
        ColorTickerBulbEx(GetType(NFOStrategy), Color.Pink)
        ColorTickerBulbEx(GetType(MCXStrategy), Color.Pink)
        ColorTickerBulbEx(GetType(CDSStrategy), Color.Pink)
        OnHeartbeat("Ticker:Closed")
    End Sub
    Private Sub OnTickerConnect()
        ColorTickerBulbEx(GetType(NFOStrategy), Color.Lime)
        ColorTickerBulbEx(GetType(MCXStrategy), Color.Lime)
        ColorTickerBulbEx(GetType(CDSStrategy), Color.Lime)
        OnHeartbeat("Ticker:Connected")
    End Sub
    Private Sub OnTickerErrorWithStatus(ByVal isConnected As Boolean, ByVal errorMsg As String)
        If Not isConnected Then
            ColorTickerBulbEx(GetType(NFOStrategy), Color.Pink)
            ColorTickerBulbEx(GetType(MCXStrategy), Color.Pink)
            ColorTickerBulbEx(GetType(CDSStrategy), Color.Pink)
        End If
    End Sub
    Private Sub OnTickerError(ByVal errorMsg As String)
        OnHeartbeat(String.Format("Ticker:Error:{0}", errorMsg))
    End Sub
    Private Sub OnTickerNoReconnect()
        'Nothing to do
    End Sub
    Private Sub OnTickerReconnect()
        ColorTickerBulbEx(GetType(NFOStrategy), Color.Yellow)
        ColorTickerBulbEx(GetType(MCXStrategy), Color.Yellow)
        ColorTickerBulbEx(GetType(CDSStrategy), Color.Yellow)
        OnHeartbeat("Ticker:Reconnecting")
    End Sub
    Private Sub OnFetcherError(ByVal instrumentIdentifier As String, ByVal errorMsg As String)
        OnHeartbeat(String.Format("Historical Data Fetcher: Error:{0}, InstrumentIdentifier:{1}", errorMsg, instrumentIdentifier))
    End Sub
    Private Sub OnCollectorError(ByVal errorMsg As String)
        OnHeartbeat(String.Format("Information Collector: Error:{0}", errorMsg))
    End Sub
    Public Sub ProgressStatus(ByVal msg As String)
        If Not msg.EndsWith("...") Then msg = String.Format("{0}...", msg)
        WriteLogEx(LogMode.All, msg, Nothing)
        logger.Info(msg)
    End Sub
    Public Sub ProgressStatusEx(ByVal msg As String, ByVal source As List(Of Object))
        If Not msg.EndsWith("...") Then msg = String.Format("{0}...", msg)
        If source Is Nothing Then
            WriteLogEx(LogMode.All, msg, Nothing)
        ElseIf source IsNot Nothing AndAlso source.Count > 0 Then
            For Each runningSource In source
                WriteLogEx(LogMode.One, msg, runningSource)
            Next
        End If
        logger.Info(msg)
    End Sub
    Private Sub OnHeartbeatEx(msg As String, ByVal source As List(Of Object))
        'Update detailed status on the first part, dont append if the text starts with <
        If msg.Contains("<<<") Then
            msg = Replace(msg, "<<<", Nothing)
            ProgressStatusEx(msg, source)
        Else
            ProgressStatusEx(msg, source)
        End If
        msg = Nothing
    End Sub
    Private Sub OnWaitingForEx(elapsedSecs As Integer, totalSecs As Integer, msg As String, ByVal source As List(Of Object))
        If msg.Contains("...") Then msg = msg.Replace("...", "")
        ProgressStatusEx(String.Format("{0}, waiting {1}/{2} secs", msg, elapsedSecs, totalSecs), source)
    End Sub
    Private Sub OnDocumentRetryStatusEx(currentTry As Integer, totalTries As Integer, ByVal source As List(Of Object))
        'ProgressStatusEx(String.Format("Try #{0}/{1}: Connecting", currentTry, totalTries), source)
    End Sub
    Private Sub OnDocumentDownloadCompleteEx(ByVal source As List(Of Object))
    End Sub
    Protected Overridable Sub OnNewItemAdded(ByVal item As ActivityDashboard)
        If item IsNot Nothing Then
            Select Case item.ParentStrategyInstrument.ParentStrategy.GetType
                Case GetType(NFOStrategy)
                    BindingListAdd_ThreadSafe(_nfoDashboadList, item)
                Case GetType(MCXStrategy)
                    BindingListAdd_ThreadSafe(_MCXdashboadList, item)
                Case GetType(CDSStrategy)
                    BindingListAdd_ThreadSafe(_CDSDashboadList, item)
                Case Else
                    Throw New NotImplementedException
            End Select
        End If
    End Sub
    Protected Sub OnSessionExpiry(ByVal runningStrategy As Strategy)
        Select Case runningStrategy.GetType
            Case GetType(NFOStrategy)
                SetSFGridDataBind_ThreadSafe(sfdgvNFOMainDashboard, Nothing)
                _nfoDashboadList = Nothing
                _nfoDashboadList = New BindingList(Of ActivityDashboard)(runningStrategy.SignalManager.ActivityDetails.Values.ToList)
                SetSFGridDataBind_ThreadSafe(sfdgvNFOMainDashboard, _nfoDashboadList)
                SetSFGridFreezFirstColumn_ThreadSafe(sfdgvNFOMainDashboard)
            Case GetType(MCXStrategy)
                SetSFGridDataBind_ThreadSafe(sfdgvMCXMainDashboard, Nothing)
                _MCXdashboadList = Nothing
                _MCXdashboadList = New BindingList(Of ActivityDashboard)(runningStrategy.SignalManager.ActivityDetails.Values.ToList)
                SetSFGridDataBind_ThreadSafe(sfdgvMCXMainDashboard, _MCXdashboadList)
                SetSFGridFreezFirstColumn_ThreadSafe(sfdgvMCXMainDashboard)
            Case GetType(CDSStrategy)
                SetSFGridDataBind_ThreadSafe(sfdgvCDSMainDashboard, Nothing)
                _CDSDashboadList = Nothing
                _CDSDashboadList = New BindingList(Of ActivityDashboard)(runningStrategy.SignalManager.ActivityDetails.Values.ToList)
                SetSFGridDataBind_ThreadSafe(sfdgvCDSMainDashboard, _CDSDashboadList)
                SetSFGridFreezFirstColumn_ThreadSafe(sfdgvCDSMainDashboard)
            Case Else
                Throw New NotImplementedException
        End Select
    End Sub
    Protected Sub OnEndOfTheDay(ByVal runningStrategy As Strategy)
        If runningStrategy IsNot Nothing AndAlso runningStrategy.ExportCSV Then
            Select Case runningStrategy.GetType
                Case GetType(NFOStrategy)
                    ExportDataToCSV(runningStrategy, Path.Combine(My.Application.Info.DirectoryPath, String.Format("NFO Order Book.csv")))
                Case GetType(MCXStrategy)
                    ExportDataToCSV(runningStrategy, Path.Combine(My.Application.Info.DirectoryPath, String.Format("MCX Order Book.csv")))
                Case GetType(CDSStrategy)
                    ExportDataToCSV(runningStrategy, Path.Combine(My.Application.Info.DirectoryPath, String.Format("CDS Signal Order Book.csv")))
                Case Else
                    Throw New NotImplementedException
            End Select
            runningStrategy.ExportCSV = False
        End If
    End Sub
#End Region

#Region "Export Grid"
    Private Sub ExportDataToCSV(ByVal runningStrategy As Strategy, ByVal fileName As String)
        'If runningStrategy IsNot Nothing AndAlso runningStrategy.SignalManager IsNot Nothing AndAlso
        '    runningStrategy.SignalManager.ActivityDetails IsNot Nothing AndAlso runningStrategy.SignalManager.ActivityDetails.Count > 0 Then
        '    OnHeartbeat("Exoprting data to csv")
        '    Dim dt As DataTable = Nothing
        '    For Each rowData In runningStrategy.SignalManager.ActivityDetails.Values.OrderBy(Function(x)
        '                                                                                         Return x.SignalGeneratedTime
        '                                                                                     End Function).ToList
        '        If dt Is Nothing Then
        '            dt = New DataTable
        '            dt.Columns.Add("Trading Date")
        '            dt.Columns.Add("Trading Symbol")
        '            dt.Columns.Add("Entry Direction")
        '            dt.Columns.Add("Entry Time")
        '            dt.Columns.Add("Exit Condition")
        '            dt.Columns.Add("Exit Time")
        '            dt.Columns.Add("Signal PL")
        '            dt.Columns.Add("Strategy Overall PL after brokerage")
        '            dt.Columns.Add("Strategy Max Drawup")
        '            dt.Columns.Add("Strategy Max Drawup Time")
        '            dt.Columns.Add("Strategy Max Drawdown")
        '            dt.Columns.Add("Strategy Max Drawdown Time")
        '        End If
        '        Dim row As System.Data.DataRow = dt.NewRow
        '        row("Trading Date") = Now.Date.ToString("dd-MM-yyyy")
        '        row("Trading Symbol") = rowData.TradingSymbol
        '        row("Strategy Overall PL after brokerage") = rowData.StrategyOverAllPLAfterBrokerage
        '        row("Strategy Max Drawup") = rowData.StrategyMaxDrawUp
        '        row("Strategy Max Drawup Time") = rowData.StrategyMaxDrawUpTime.ToString("HH:mm:ss")
        '        row("Strategy Max Drawdown") = rowData.StrategyMaxDrawDown
        '        row("Strategy Max Drawdown Time") = rowData.StrategyMaxDrawDownTime.ToString("HH:mm:ss")
        '        row("Signal PL") = rowData.SignalPL
        '        row("Entry Direction") = rowData.SignalDirection.ToString
        '        row("Entry Time") = rowData.EntryRequestTime.ToString("HH:mm:ss")
        '        row("Exit Time") = rowData.CancelRequestTime.ToString("HH:mm:ss")
        '        row("Exit Condition") = rowData.CancelRequestRemarks
        '        dt.Rows.Add(row)
        '    Next
        '    If dt IsNot Nothing Then
        '        Using csvCreator As New Utilities.DAL.CSVHelper(fileName, ",", _cts)
        '            csvCreator.GetCSVFromDataTable(dt)
        '        End Using
        '    End If
        'End If
    End Sub
#End Region

#End Region

End Class