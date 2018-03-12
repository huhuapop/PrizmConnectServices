Imports System.ServiceProcess
Imports Microsoft.ApplicationBlocks.Data
Imports Microsoft.ApplicationBlocks
Imports System.Messaging
Imports System.Threading
Imports System.Data.SqlClient
Imports System.Xml
Imports System.Xml.XPath
Imports System.IO
Imports System.Text
Imports System.Diagnostics
Imports Microsoft.Win32



Public Class svcPrizmConnectServices
    Inherits System.ServiceProcess.ServiceBase

#Region " 组件设计器生成的代码 "

    Public Sub New()
        MyBase.New()

        ' 该调用是组件设计器所必需的。
        InitializeComponent()

        ' 在 InitializeComponent() 调用之后添加任何初始化

    End Sub

    'UserService 重写 dispose 以清理组件列表。
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    ' 进程的主入口点
    <MTAThread()> _
    Shared Sub Main()
        Dim ServicesToRun() As System.ServiceProcess.ServiceBase

        ' 在同一进程中可以运行不止一个 NT 服务。若要将
        ' 另一个服务添加到此进程，请更改下行以
        ' 创建另一个服务对象。例如，
        '
        '   ServicesToRun = New System.ServiceProcess.ServiceBase () {New Service1, New MySecondUserService}
        '
        ServicesToRun = New System.ServiceProcess.ServiceBase() {New svcPrizmConnectServices}

        System.ServiceProcess.ServiceBase.Run(ServicesToRun)
    End Sub

    '组件设计器所必需的
    Private components As System.ComponentModel.IContainer

    '注意: 以下过程是组件设计器所必需的
    ' 可以使用组件设计器修改此过程。
    ' 不要使用代码编辑器修改它。
    Friend WithEvents tmCheck As System.Timers.Timer
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.tmCheck = New System.Timers.Timer
        CType(Me.tmCheck, System.ComponentModel.ISupportInitialize).BeginInit()
        '
        'tmCheck
        '
        Me.tmCheck.Interval = 1000
        '
        'svcPrizmConnectServices
        '
        Me.ServiceName = "PrizmConnectServices"
        CType(Me.tmCheck, System.ComponentModel.ISupportInitialize).EndInit()

    End Sub

#End Region

#Region "定义"
    Dim prizmConnect() As ConnectClass
    Dim ConnectCount As Int32
    Dim m_ConnectStructure() As ConnectStructure
    Dim IsDebug As Boolean

    Public Structure ConnectStructure
        Dim Name As String
        Dim ConnectionString As String
        Dim RollOn As Int32
        Dim RollOff As Int32
        Dim QueuePath As String
        Dim SleepTime As Int32
        Dim DeleteSleepTime As Int32
        Dim RefreshInterval As Int32

        Dim IsReceived As Int32 '= 0   '1 receive, 0 not receive
        Dim IsClean As Int32 '= 0      '1 cleaning ,0 not clean 
        'Dim NotReflashTimeStart As String
        'Dim NotReflashTimeEnd As String

        Dim InsertStoredProcedure As String
        Dim UpdateStoredProcedure As String
        Dim DeleteStoredProcedure As String
        Dim BaseStoredProcedure As String

        Dim HeadKeyWord_connect As String '/connect
        Dim HeadKeyWord_name As String 'name
        Dim HeadReceive_name As String 'UltraInterface
        Dim PrimaryKeyWord_name As String '/daily
        Dim PrimaryKeyWord_action As String 'action
    End Structure
#End Region

    Protected Overrides Sub OnStart(ByVal args() As String)
        ' 在此处添加启动服务的代码。此方法应设置具体的操作
        ' 以便服务可以执行它的工作。
        '#If DEBUGSERVICE Then
        ' System.Diagnostics.Debugger.Launch()
        '#End If
        Try
            Thread.Sleep(10000)
            IsDebug = False
            LoadConfig()
            GetStart()
            tmCheck.Interval = 60 * 1000
            tmCheck.Start()
        Catch ex As Exception
            EventLog.WriteEntry("PrizmConnectServices", ex.StackTrace.ToString() + ex.Message.ToString(), EventLogEntryType.Error)
        Finally
        End Try
    End Sub

    Protected Overrides Sub OnStop()
        ' 在此处添加代码以执行停止服务所需的关闭操作。
        AtDispose()
    End Sub

    Private Sub AtDispose()
        Try
            For i As Int32 = 0 To ConnectCount - 1
                prizmConnect(i).AtStop()
                prizmConnect(i) = Nothing
            Next
            tmCheck.Close()
            GC.Collect(GC.MaxGeneration)
        Catch ex As Exception
            EventLog.WriteEntry("PrizmConnectServices", ex.StackTrace.ToString() + ex.Message.ToString(), EventLogEntryType.Error)
        Finally
        End Try
    End Sub

    Private Sub LoadConfig()
        Try
            Dim configPath As String = System.Configuration.ConfigurationSettings.AppSettings("xmlPath")
            IsDebug = CType(System.Configuration.ConfigurationSettings.AppSettings("IsDebug"), Boolean)
            Dim xmldoc As XmlDocument = New XmlDocument
            xmldoc.Load(configPath)

            EventLog.WriteEntry("PrizmConnectServices", configPath + xmldoc.InnerXml, EventLogEntryType.Information)

            ConnectCount = Convert.ToInt32(xmldoc.SelectSingleNode("/PrizmConnectServices/ServicesNumber").InnerXml)

            m_ConnectStructure = New ConnectStructure(ConnectCount) {}

            Dim xmlnodes As XmlNodeList = xmldoc.SelectNodes("/PrizmConnectServices/PrizmConnectService")
            Dim node As XmlNode
            For Each node In xmlnodes
                'EventLog.WriteEntry("PrizmConnectServices", node.InnerXml, EventLogEntryType.Information)
                Dim id As Int32 = Convert.ToInt32(node.SelectSingleNode("id").InnerXml)
                m_ConnectStructure(id - 1) = New ConnectStructure

                m_ConnectStructure(id - 1).Name = node.SelectSingleNode("Name").InnerXml
                m_ConnectStructure(id - 1).ConnectionString = node.SelectSingleNode("ConnectionString").InnerXml
                m_ConnectStructure(id - 1).RollOn = node.SelectSingleNode("RollOn").InnerXml
                m_ConnectStructure(id - 1).RollOff = node.SelectSingleNode("RollOff").InnerXml
                m_ConnectStructure(id - 1).QueuePath = node.SelectSingleNode("QueuePath").InnerXml
                m_ConnectStructure(id - 1).SleepTime = node.SelectSingleNode("SleepTime").InnerXml
                m_ConnectStructure(id - 1).DeleteSleepTime = node.SelectSingleNode("DeleteSleepTime").InnerXml
                m_ConnectStructure(id - 1).RefreshInterval = node.SelectSingleNode("RefreshInterval").InnerXml
                m_ConnectStructure(id - 1).IsReceived = node.SelectSingleNode("IsReceived").InnerXml
                m_ConnectStructure(id - 1).IsClean = node.SelectSingleNode("IsClean").InnerXml
                m_ConnectStructure(id - 1).InsertStoredProcedure = node.SelectSingleNode("InsertStoredProcedure").InnerXml
                m_ConnectStructure(id - 1).UpdateStoredProcedure = node.SelectSingleNode("UpdateStoredProcedure").InnerXml
                m_ConnectStructure(id - 1).DeleteStoredProcedure = node.SelectSingleNode("DeleteStoredProcedure").InnerXml
                m_ConnectStructure(id - 1).BaseStoredProcedure = node.SelectSingleNode("BaseStoredProcedure").InnerXml
                m_ConnectStructure(id - 1).HeadKeyWord_connect = node.SelectSingleNode("HeadKeyWord_connect").InnerXml
                m_ConnectStructure(id - 1).HeadKeyWord_name = node.SelectSingleNode("HeadKeyWord_name").InnerXml
                m_ConnectStructure(id - 1).HeadReceive_name = node.SelectSingleNode("HeadReceive_name").InnerXml
                m_ConnectStructure(id - 1).PrimaryKeyWord_name = node.SelectSingleNode("PrimaryKeyWord_name").InnerXml
                m_ConnectStructure(id - 1).PrimaryKeyWord_action = node.SelectSingleNode("PrimaryKeyWord_action").InnerXml
                ' EventLog.WriteEntry("PrizmConnectServices", m_ConnectStructure(id).QueuePath, EventLogEntryType.Information)
            Next
            'EventLog.WriteEntry("PrizmConnectServices", m_ConnectStructure(0).QueuePath, EventLogEntryType.Information)
        Catch ex As Exception
            EventLog.WriteEntry("PrizmConnectServices", ex.StackTrace.ToString() + ex.Message.ToString(), EventLogEntryType.Error)
        Finally
        End Try
    End Sub

    Private Sub GetStart()
        Try
            prizmConnect = New ConnectClass(ConnectCount) {}

            For i As Int32 = 0 To ConnectCount - 1
                'EventLog.WriteEntry("PrizmConnectServices", m_ConnectStructure(i).Name, EventLogEntryType.Information)
                prizmConnect(i) = New ConnectClass(m_ConnectStructure(i).Name, _
                                                    m_ConnectStructure(i).ConnectionString, _
                                                    m_ConnectStructure(i).RollOn, _
                                                    m_ConnectStructure(i).RollOff, _
                                                    m_ConnectStructure(i).QueuePath, _
                                                    m_ConnectStructure(i).SleepTime, _
                                                    m_ConnectStructure(i).DeleteSleepTime, _
                                                    m_ConnectStructure(i).RefreshInterval, _
                                                    m_ConnectStructure(i).IsReceived, _
                                                    m_ConnectStructure(i).IsClean, _
                                                    m_ConnectStructure(i).InsertStoredProcedure, _
                                                    m_ConnectStructure(i).UpdateStoredProcedure, _
                                                    m_ConnectStructure(i).DeleteStoredProcedure, _
                                                    m_ConnectStructure(i).BaseStoredProcedure, _
                                                    m_ConnectStructure(i).HeadKeyWord_connect, _
                                                    m_ConnectStructure(i).HeadKeyWord_name, _
                                                    m_ConnectStructure(i).HeadReceive_name, _
                                                    m_ConnectStructure(i).PrimaryKeyWord_name, _
                                                    m_ConnectStructure(i).PrimaryKeyWord_action, _
                                                    IsDebug _
                                                    )

            Next

            For i As Int32 = 0 To ConnectCount - 1
                prizmConnect(i).AtStart()
                EventLog.WriteEntry("PrizmConnectServices", m_ConnectStructure(i).Name, EventLogEntryType.Information)
            Next
        Catch ex As Exception
            EventLog.WriteEntry("PrizmConnectServices", ex.StackTrace.ToString() + ex.Message.ToString(), EventLogEntryType.Error)
        Finally
        End Try
    End Sub

    Private Sub tmCheck_Elapsed(ByVal sender As System.Object, ByVal e As System.Timers.ElapsedEventArgs) Handles tmCheck.Elapsed
        tmCheck.Stop()
        Dim xmldoc As XmlDocument
        Dim mq As System.Messaging.MessageQueue
        Dim sendQueue As String
        Dim _encoding As System.Text.Encoding
        Dim MsgHead As String
        Dim message As Message
        Try
            Dim TestStr As String = ""

            '''<?xml version="1.0"?>
            '<connect version="1.0" name="UltraInterface">
            '	<table action="replace"/>
            '	<field />
            '</connect>
            For i As Int32 = 0 To ConnectCount - 1
                TestStr = "<connect version=" + """1.0""" + " name=" + """UltraInterface""" + "><" + m_ConnectStructure(i).PrimaryKeyWord_name.Replace("/", "") + " action=" + """delete""" + "/></connect>"
                xmldoc = New XmlDocument
                'EventLog.WriteEntry("PrizmConnectServices", TestStr, EventLogEntryType.Information)
                xmldoc.LoadXml(TestStr)

                sendQueue = m_ConnectStructure(i).QueuePath  '"FormatName:" + Global.GetSysOptionValue("AAS_MAN_QUEUE") '.Replace("DIRECT=TCP:", "").Replace("DIRECT=OS:", "")
                _encoding = System.Text.Encoding.UTF8

                MsgHead = m_ConnectStructure(i).Name
                message = New Message
                message.BodyStream = New System.IO.MemoryStream(_encoding.GetBytes(xmldoc.OuterXml))
                mq = New System.Messaging.MessageQueue
                mq.Path = sendQueue
                mq.Send(message, MsgHead)

            Next


            '_encoding = System.Text.Encoding.UTF8



            'MsgHead = "Test"
            'message = New Message
            'message.BodyStream = New System.IO.MemoryStream(_encoding.GetBytes(xmldoc.OuterXml))
            'mq = New System.Messaging.MessageQueue
            'mq.Path = sendQueue
            'mq.Send(message, MsgHead)
            ''''<?xml version="1.0"?>
            ''<connect version="1.0" name="UltraInterface">
            ''	<table action="replace"/>
            ''	<field />
            ''</connect>
            'TestStr = "<connect version=" + """1.0""" + " name=" + """UltraInterface""" + "><table action=" + """delete""" + "/></connect>"
            ''EventLog.WriteEntry("PrizmConnectServices", TestStr, EventLogEntryType.Information)
            'xmldoc = New XmlDocument
            'xmldoc.LoadXml(TestStr)

            'sendQueue = m_ConnectStructure(1).QueuePath  '"FormatName:" + Global.GetSysOptionValue("AAS_MAN_QUEUE") '.Replace("DIRECT=TCP:", "").Replace("DIRECT=OS:", "")

            '_encoding = System.Text.Encoding.UTF8

            'MsgHead = "Test"
            'message = New Message
            'message.BodyStream = New System.IO.MemoryStream(_encoding.GetBytes(xmldoc.OuterXml))
            'mq = New System.Messaging.MessageQueue
            'mq.Path = sendQueue
            'mq.Send(message, MsgHead)

        Catch ex As Exception
            EventLog.WriteEntry("PrizmConnectServices", ex.StackTrace.ToString() + ex.Message.ToString(), EventLogEntryType.Error)
        Finally
            message = Nothing
            mq.Close()
            mq = Nothing
            xmldoc = Nothing
        End Try
        tmCheck.Start()
    End Sub
End Class

Public Class ConnectClass
    Dim Name As String
    Dim prizmQueue As MessageQueue
    'Dim prizmThread As Thread
    Dim ConnectionString As String
    Dim RollOn As Int32
    Dim RollOff As Int32
    Dim QueuePath As String
    Dim SleepTime As Int32
    Dim DeleteSleepTime As Int32
    Dim RefreshInterval As Int32

    Dim refreshTimer As System.Threading.Timer
    Dim IsReceived As Int32 '= 0   '1 receive, 0 not receive
    Dim IsClean As Int32 '= 0      '1 cleaning ,0 not clean 
    'Dim NotReflashTimeStart As String
    'Dim NotReflashTimeEnd As String

    Dim InsertStoredProcedure As String
    Dim UpdateStoredProcedure As String
    Dim DeleteStoredProcedure As String
    Dim BaseStoredProcedure As String

    Dim HeadKeyWord_connect As String '/connect
    Dim HeadKeyWord_name As String 'name
    Dim HeadReceive_name As String 'UltraInterface
    Dim PrimaryKeyWord_name As String '/daily
    Dim PrimaryKeyWord_action As String 'action


    Dim ClassState As Boolean
    Dim IsDebug As Boolean

    Dim sqlconn As SqlConnection
    Dim sqlcomm As SqlCommand
    Dim sqlda As SqlDataAdapter
    Dim ds As DataSet
    Dim ConnectStateThread As Thread
    Dim IsCheckConnect As Boolean
    Dim IsConnect As Boolean

    Public Sub New(ByVal _Name As String, _
                    ByVal _ConnectionString As String, _
                    ByVal _RollOn As String, _
                    ByVal _RollOff As String, _
                    ByVal _QueuePath As String, _
                    ByVal _SleepTime As String, _
                    ByVal _DeleteSleepTime As String, _
                    ByVal _RefreshInterval As String, _
                    ByVal _IsReceived As String, _
                    ByVal _IsClean As String, _
                    ByVal _InsertStoredProcedure As String, _
                    ByVal _UpdateStoredProcedure As String, _
                    ByVal _DeleteStoredProcedure As String, _
                     ByVal _BaseStoredProcedure As String, _
                    ByVal _HeadKeyWord_connect As String, _
                    ByVal _HeadKeyWord_name As String, _
                    ByVal _HeadReceive_name As String, _
                    ByVal _PrimaryKeyWord_name As String, _
                    ByVal _PrimaryKeyWord_action As String, _
                    ByVal _IsDebug As Boolean _
                   )
        Name = _Name
        ConnectionString = _ConnectionString
        RollOn = _RollOn
        RollOff = _RollOff
        QueuePath = _QueuePath
        SleepTime = _SleepTime
        DeleteSleepTime = _DeleteSleepTime
        RefreshInterval = _RefreshInterval
        IsReceived = _IsReceived
        IsClean = _IsClean
        InsertStoredProcedure = _InsertStoredProcedure
        UpdateStoredProcedure = _UpdateStoredProcedure
        DeleteStoredProcedure = _DeleteStoredProcedure
        BaseStoredProcedure = _BaseStoredProcedure
        HeadKeyWord_connect = _HeadKeyWord_connect
        HeadKeyWord_name = _HeadKeyWord_name
        HeadReceive_name = _HeadReceive_name
        PrimaryKeyWord_name = _PrimaryKeyWord_name
        PrimaryKeyWord_action = _PrimaryKeyWord_action
        IsDebug = _IsDebug
        EventLog.WriteEntry("PrizmConnectServices", _Name, EventLogEntryType.Information)
    End Sub

    Public Sub AtStart()
        GetQueue()
        'prizmThread = New Thread(AddressOf GetQueue)
        'prizmThread.Priority = ThreadPriority.Highest
        'prizmThread.Start()
       
        ClassState = True 'true mean thread is ok;false mean thread has problem ,need restart
        IsCheckConnect = CType(System.Configuration.ConfigurationSettings.AppSettings("IsCheckConnect"), Boolean)
        ConnectStateThread = New Thread(AddressOf ConnectState)
        ConnectStateThread.Start()
    End Sub

    Public Sub AtStop()
        'prizmThread.Abort()
        'prizmThread = Nothing
        If (sqlconn.State = ConnectionState.Open) Then
            sqlconn.Close()
            sqlconn = Nothing
            sqlcomm = Nothing
            sqlda = Nothing
            ds = Nothing
        End If
        prizmQueue.Close()

        prizmQueue = Nothing
        ClassState = False
    End Sub

    Public ReadOnly Property GetState() As Boolean
        Get
            Return ClassState
        End Get
    End Property

    Private Sub GetQueue()
        Try
            EventLog.WriteEntry("PrizmConnectServices", "PrizmConnectServices" + Name + ":log ", EventLogEntryType.Information)
            If (Not EventLog.SourceExists("PrizmConnectServices" + Name)) Then
                EventLog.WriteEntry("PrizmConnectServices", Name + ":Create Log", EventLogEntryType.Warning)
                EventLog.CreateEventSource("PrizmConnectServices" + Name, Name)
                ' Console.WriteLine("CreatingEventSource")
            End If
            If Not (MessageQueue.Exists(QueuePath)) Then
                ' EventLog.WriteEntry("PrizmConnectServices", Name + ":" + "Please Check Queue!", EventLogEntryType.Error)
                EventLog.WriteEntry("PrizmConnectServices", Name + ":" + "Create Queue," + QueuePath, EventLogEntryType.Warning)
                MessageQueue.Create(QueuePath)
                Dim mqTemp As MessageQueue = New MessageQueue(QueuePath)
                mqTemp.SetPermissions("Everyone", System.Messaging.MessageQueueAccessRights.FullControl)
                mqTemp.SetPermissions("ANONYMOUS", System.Messaging.MessageQueueAccessRights.FullControl)
                If Not (mqTemp Is Nothing) Then
                    mqTemp.Close()
                    mqTemp.Dispose()
                End If
                mqTemp = Nothing

            End If


            sqlconn = New SqlConnection(ConnectionString)
            sqlcomm = New SqlCommand(BaseStoredProcedure, sqlconn)
            sqlcomm.CommandType = CommandType.StoredProcedure
            sqlda = New SqlDataAdapter(sqlcomm)
            ds = New DataSet
            sqlda.Fill(ds)

            If (sqlconn.State = ConnectionState.Open) Then
                sqlconn.Close()
                sqlconn = Nothing
                sqlcomm = Nothing
                sqlda = Nothing
                ds = Nothing
            End If

            prizmQueue = New MessageQueue(QueuePath)

            prizmQueue.EnableConnectionCache = True

            If (prizmQueue.CanRead = False) Then
                EventLog.WriteEntry("PrizmConnectServices" + Name, "Please Check the Config!", EventLogEntryType.Error)
            End If

            AddHandler prizmQueue.ReceiveCompleted, AddressOf PVGReceiveCompleted
            prizmQueue.BeginReceive()
        Catch ex As Exception
            EventLog.WriteEntry("PrizmConnectServices", Name + ":" + ex.StackTrace.ToString() + ex.Message.ToString(), EventLogEntryType.Error)
        Finally
        End Try
    End Sub

    Private Sub PVGReceiveCompleted(ByVal sender As System.Object, ByVal e As System.Messaging.ReceiveCompletedEventArgs)
        Dim sReader As BinaryReader
        Dim sMsg As Message
        Dim MsgByte() As Byte
        Dim xml As String = ""

        Try
            IsReceived = 1
            Dim queue As MessageQueue = CType(sender, MessageQueue)
            sMsg = queue.EndReceive(e.AsyncResult)

            sReader = New BinaryReader(sMsg.BodyStream)
            MsgByte = sReader.ReadBytes(sMsg.BodyStream.Length)
            xml = Encoding.UTF8.GetString(MsgByte)
            Dim ReturnValue As Boolean = OrderXML(xml)
            EventLog.WriteEntry("PrizmConnectServices" + Name, "Receive Data:" + Chr(13) + Chr(10) + "Time:" + Now.ToString("yyyyMMdd-HHmmss:fff") + Chr(13) + Chr(10) + xml, EventLogEntryType.Information)

        Catch ex As Exception
            EventLog.WriteEntry("PrizmConnectServices" + Name, ex.StackTrace.ToString() + ex.Message.ToString(), EventLogEntryType.Error)
        Finally
            xml = ""
            sReader.Close()
            sMsg.Dispose()
            sReader = Nothing
            sMsg = Nothing
            GC.Collect(GC.MaxGeneration)

            Thread.Sleep(SleepTime)
            'Thread.Sleep(ConfigurationManager.Items.Item("SleepTime"))

            prizmQueue.BeginReceive()
        End Try
    End Sub

    Private Overloads Function ExecuteDataset(ByVal pConnectString As String, ByVal pStoredProcedure As String, ByRef pParameter() As SqlParameter) As DataSet
        Dim PReturn As DataSet
        Try
            sqlconn = New SqlConnection(ConnectionString)
            sqlcomm = New SqlCommand(pStoredProcedure, sqlconn)
            sqlcomm.CommandType = CommandType.StoredProcedure
            For i As Int32 = 0 To pParameter.Length - 1
                sqlcomm.Parameters.Add(pParameter(i))
            Next
            '
            'sqlcomm.Parameters.Add("@xml", SqlDbType.NVarChar, 4000).Value = Xml
            'sqlcomm.Parameters.Add(PReturn)
            sqlda = New SqlDataAdapter(sqlcomm)
            PReturn = New DataSet
            sqlda.Fill(PReturn)
        Catch ex As Exception
            EventLog.WriteEntry("PrizmConnectServices" + Name, ex.StackTrace.ToString() + ex.Message.ToString(), EventLogEntryType.Error)
        Finally
            If (sqlconn.State = ConnectionState.Open) Then
                sqlconn.Close()
                sqlconn = Nothing
                sqlcomm = Nothing
                sqlda = Nothing
                ds = Nothing
            End If
        End Try

        Return PReturn
    End Function

    Private Sub ConnectState()
        IsConnect = False
        Dim ErrorCount As Int32 = 0
        While (IsCheckConnect)
            Dim _sqlconn As SqlConnection = Nothing
            Dim _cmd As SqlCommand = Nothing
            Try

                _sqlconn = New SqlConnection(ConnectionString)
                _cmd = New SqlCommand
                _sqlconn.Open()
                _cmd.CommandText = "Select Getdate()"
                _cmd.Connection = _sqlconn
                _cmd.CommandTimeout = 10 * 1000

                _sqlconn.Close()
                _cmd = Nothing
                IsConnect = True
            Catch ex As Exception
                IsConnect = False
                If (ErrorCount = 0) Then
                    EventLog.WriteEntry("PrizmConnectServices" + Name, ex.StackTrace.ToString() + ex.Message.ToString(), EventLogEntryType.Error)
                End If
                If (ErrorCount > 6) Then
                    ErrorCount = 0
                Else
                    ErrorCount += 1
                End If

            Finally
                If Not (_cmd Is Nothing) Then
                    _cmd = Nothing
                End If
                If Not (_sqlconn Is Nothing) Then
                    If (_sqlconn.State = ConnectionState.Open) Then
                        sqlconn.Close()
                    End If
                    sqlconn = Nothing
                End If
            End Try
            Thread.Sleep(10 * 1000)
        End While
    End Sub

    Private Function OrderXML(ByVal xml As String)
        Dim ReturnValue As Boolean = False
        Dim xmldoc As New XmlDocument
        Try
            xmldoc.LoadXml(xml)

            Dim Ultraname As String = xmldoc.SelectSingleNode(HeadKeyWord_connect).Attributes(HeadKeyWord_name).InnerXml

            If Not (Ultraname = HeadReceive_name) Then
                Exit Function
            End If
            ' Route the dailies
            If (xmldoc.SelectSingleNode(HeadKeyWord_connect + PrimaryKeyWord_name) Is Nothing) Then
                Exit Try
            End If
            Try
                While (IsConnect = False)
                    Thread.Sleep(5 * 1000)
                End While
                Dim action As String = xmldoc.SelectSingleNode(HeadKeyWord_connect + PrimaryKeyWord_name).Attributes(PrimaryKeyWord_action).InnerXml.ToLower
                Dim pReturn As SqlParameter
                If (IsDebug) Then
                    EventLog.WriteEntry("PrizmConnectServices" + Name, Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " Action:" + action, EventLogEntryType.Information)
                End If
                ds = New DataSet
                sqlconn = New SqlConnection(ConnectionString)
                Select Case action
                    Case "replace", "insert"
                        pReturn = New SqlParameter
                        pReturn.Direction = ParameterDirection.ReturnValue

                        'ds = SqlHelper.ExecuteDataset(ConnectionString, CommandType.StoredProcedure, InsertStoredProcedure, New SqlParameter("@xml", xml), pReturn)

                        sqlcomm = New SqlCommand(InsertStoredProcedure, sqlconn)
                        sqlcomm.CommandType = CommandType.StoredProcedure
                        sqlcomm.Parameters.Add("@xml", SqlDbType.NText, 10000000).Value = xml
                        sqlcomm.Parameters.Add(pReturn)
                        sqlda = New SqlDataAdapter(sqlcomm)
                        ds = New DataSet
                        sqlda.Fill(ds)

                        'Dim dt As DataTable
                        'If Not (ds Is Nothing) Then
                        '    dt = ds.Tables(0)
                        '    Dim ReturnValuestr As String = ""
                        '    ReturnValuestr = dt.Rows(0)(0).ToString()

                        '    EventLog.WriteEntry("PrizmConnectServices" + Name, Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + Name + " Add New " + " Success!" + Chr(13) + Chr(10) + "ReturnCode:" + ReturnValuestr.ToString + Chr(13) + Chr(10) + xml, EventLogEntryType.Information)

                        'End If


                        If (pReturn.Value Is Nothing) Then
                        Else
                            If (Convert.ToInt32(pReturn.Value) > 0) Then
                                If (IsDebug) Then
                                    EventLog.WriteEntry("PrizmConnectServices" + Name, Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + Name + " Add New " + " Success!" + Chr(13) + Chr(10) + "ReturnCode:" + pReturn.Value.ToString + Chr(13) + Chr(10) + xml, EventLogEntryType.Information)
                                End If

                            ElseIf (Convert.ToInt32(pReturn.Value) = 0) Then
                                If (IsDebug) Then
                                    EventLog.WriteEntry("PrizmConnectServices" + Name, Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + Name + " already Exist,Update the flight data!" + Chr(13) + Chr(10) + "ReturnCode:" + pReturn.Value.ToString + Chr(13) + Chr(10) + xml, EventLogEntryType.Warning)
                                End If
                            Else
                                EventLog.WriteEntry("PrizmConnectServices" + Name, Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + Name + " Data Error!" + Chr(13) + Chr(10) + "ReturnCode:" + pReturn.Value.ToString + Chr(13) + Chr(10) + xml, EventLogEntryType.Error)
                            End If

                        End If
                        Exit Select
                    Case "update"
                        pReturn = New SqlParameter
                        pReturn.Direction = ParameterDirection.ReturnValue

                        'ds = SqlHelper.ExecuteDataset(ConnectionString, CommandType.StoredProcedure, UpdateStoredProcedure, New SqlParameter("@xml", xml), pReturn)

                        sqlcomm = New SqlCommand(UpdateStoredProcedure, sqlconn)
                        sqlcomm.CommandType = CommandType.StoredProcedure
                        sqlcomm.Parameters.Add("@xml", SqlDbType.NText, 10000000).Value = xml
                        sqlcomm.Parameters.Add(pReturn)
                        sqlda = New SqlDataAdapter(sqlcomm)
                        ds = New DataSet
                        sqlda.Fill(ds)

                        If (pReturn.Value Is Nothing) Then
                        Else
                            If (Convert.ToInt32(pReturn.Value) > 0) Then
                                If (IsDebug) Then
                                    EventLog.WriteEntry("PrizmConnectServices" + Name, Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + Name + " Add New  Success!" + Chr(13) + Chr(10) + "ReturnCode:" + pReturn.Value.ToString + Chr(13) + Chr(10) + xml, EventLogEntryType.Information)
                                End If
                            ElseIf (Convert.ToInt32(pReturn.Value) = 0) Then
                                If (IsDebug) Then
                                    EventLog.WriteEntry("PrizmConnectServices" + Name, Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + Name + " Update the  data!" + Chr(13) + Chr(10) + "ReturnCode:" + pReturn.Value.ToString + Chr(13) + Chr(10) + xml, EventLogEntryType.Information)
                                End If
                            Else
                                EventLog.WriteEntry("PrizmConnectServices" + Name, Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + Name + " Update the  data Error!" + Chr(13) + Chr(10) + "ReturnCode:" + pReturn.Value.ToString + Chr(13) + Chr(10) + xml, EventLogEntryType.Error)
                            End If

                        End If
                        Exit Select
                    Case "delete"
                        pReturn = New SqlParameter
                        pReturn.Direction = ParameterDirection.ReturnValue

                        'ds = SqlHelper.ExecuteDataset(ConnectionString, CommandType.StoredProcedure, DeleteStoredProcedure, New SqlParameter("@xml", xml), pReturn)

                        sqlcomm = New SqlCommand(DeleteStoredProcedure, sqlconn)
                        sqlcomm.CommandType = CommandType.StoredProcedure
                        sqlcomm.Parameters.Add("@xml", SqlDbType.NText, 10000000).Value = xml
                        sqlcomm.Parameters.Add(pReturn)
                        sqlda = New SqlDataAdapter(sqlcomm)
                        ds = New DataSet
                        sqlda.Fill(ds)
                        ' sqlcomm.ExecuteNonQuery()

                        If (pReturn.Value Is Nothing) Then
                        Else
                            If (Convert.ToInt32(pReturn.Value) > 0) Then
                                If (IsDebug) Then
                                    EventLog.WriteEntry("PrizmConnectServices" + Name, Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + Name + "Delete   Success!" + Chr(13) + Chr(10) + "ReturnCode:" + pReturn.Value.ToString + Chr(13) + Chr(10) + xml, EventLogEntryType.Information)
                                End If
                            Else
                                If (IsDebug) Then
                                    EventLog.WriteEntry("PrizmConnectServices" + Name, Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + Name + "Delete  Error!" + Chr(13) + Chr(10) + "ReturnCode:" + pReturn.Value.ToString + Chr(13) + Chr(10) + xml, EventLogEntryType.Error)
                                End If
                            End If

                        End If
                        Exit Select
                    Case Else
                        Exit Select
                End Select

            Catch ex As Exception
                EventLog.WriteEntry("PrizmConnectServices" + Name, ex.StackTrace.ToString() + ex.Message.ToString(), EventLogEntryType.Error)
                Throw
            End Try
            ReturnValue = True
        Catch ex As Exception
            EventLog.WriteEntry("PrizmConnectServices" + Name, ex.StackTrace.ToString() + ex.Message.ToString(), EventLogEntryType.Error)
            ReturnValue = False
        Finally
            If (sqlconn.State = ConnectionState.Open) Then
                sqlconn.Close()
                sqlconn = Nothing
                sqlcomm = Nothing
                sqlda = Nothing
                ds = Nothing
            End If
            ' EventLog.WriteEntry("PrizmConnect Message", Now.ToString("yyyyMMdd-HHmmss:fff") + "---" + xml, EventLogEntryType.Information)
            xmldoc = Nothing
            xml = ""
            IsReceived = 0
            'Thread.Sleep(ConfigurationManager.Items.Item("SleepTime"))
        End Try
        Return ReturnValue
    End Function

    Private Function ExecBaseConnecttion() As Boolean
        Dim ReturnValue As Boolean = False
        Dim SCON As SqlConnection = Nothing
        Dim SCom As SqlCommand = New SqlCommand
        Try
            SCON = New SqlConnection(ConnectionString)
            SCON.Open()
            SCom.CommandText = "Select getdate()"
            SCom.Connection = SCON
            SCom.CommandType = CommandType.Text
            SCom.ExecuteNonQuery()
            SCON.Close()
        Catch ex As Exception

        Finally
        End Try
        Return ReturnValue
    End Function

End Class
