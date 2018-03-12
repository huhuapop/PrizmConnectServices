Imports System.ComponentModel
Imports System.Configuration.Install
Imports Microsoft.ApplicationBlocks.Data
Imports Microsoft.ApplicationBlocks

<RunInstaller(True)> Public Class ProjectInstaller
    Inherits System.Configuration.Install.Installer

#Region " �����������ɵĴ��� "

    Public Sub New()
        MyBase.New()

        '�õ�������������������ġ�
        InitializeComponent()

        '�� InitializeComponent() ����֮������κγ�ʼ��

    End Sub

    'Installer ��д dispose ����������б�
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    '���������������
    Private components As System.ComponentModel.IContainer

    'ע��: ���¹��������������������
    '����ʹ�������������޸Ĵ˹��̡�
    '��Ҫʹ�ô���༭�����޸�����
    Friend WithEvents ServiceProcessInstaller1 As System.ServiceProcess.ServiceProcessInstaller
    Friend WithEvents ServiceInstaller1 As System.ServiceProcess.ServiceInstaller
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.ServiceProcessInstaller1 = New System.ServiceProcess.ServiceProcessInstaller
        Me.ServiceInstaller1 = New System.ServiceProcess.ServiceInstaller
        '
        'ServiceProcessInstaller1
        '
        Me.ServiceProcessInstaller1.Account = System.ServiceProcess.ServiceAccount.LocalSystem
        Me.ServiceProcessInstaller1.Password = Nothing
        Me.ServiceProcessInstaller1.Username = Nothing
        '
        'ServiceInstaller1
        '
        Me.ServiceInstaller1.ServiceName = "PrizmConnectServices"
        Me.ServiceInstaller1.StartType = System.ServiceProcess.ServiceStartMode.Automatic
        'Me.ServiceInstaller1.ServicesDependedOn = New String() {"MSMQ", "MSSQLSERVER"}
        'Me.ServiceInstaller1.ServicesDependedOn = System.Configuration.ConfigurationSettings.AppSettings("DependedOn").Split(",")
        'EventLog.WriteEntry("PrizmConnectServices", System.Configuration.ConfigurationSettings.AppSettings("DependedOn"), EventLogEntryType.Information)
        '
        'ProjectInstaller
        '
        Me.Installers.AddRange(New System.Configuration.Install.Installer() {Me.ServiceProcessInstaller1, Me.ServiceInstaller1})

    End Sub

#End Region

End Class
