/*----------------------------------------------------------------
        // Copyright (C) 2007 L3'Studio
        // ��Ȩ���С� 
        // �����ߣ���Զ��˰���ܿ�
        // �ļ�����Servers.cs
        // �ļ������������ܵķ���ˣ������ļ�������Ļ���ͷ���ȵȣ���Ӧ�ͻ��˵����в���
//----------------------------------------------------------------*/

using System;
using System.Text;
using System.Windows.Forms;

using System.Net.Sockets;
using System.Threading;
using System.IO;

using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

using Server;
using ICanSeeYou.Codes;
using ICanSeeYou.Hooks;
using ICanSeeYou.Bases;
using ICanSeeYou.Windows;
using ICanSeeYou.Common;

namespace Servers
{
    /// <summary>
    /// �����ƶ�
    /// </summary>
    public class Servers
    {
        #region ���������Զ���
        /// <summary>
        /// �������
        /// </summary>
        private BaseServer mainServer;
        /// <summary>
        /// ��Ļ�����
        /// </summary>
        private ScreenServer screenServer;
        /// <summary>
        /// �ļ������
        /// </summary>
        private FileServer fileServer;
        /// <summary>
        /// �������߳�
        /// </summary>
        private Thread mainThread;
        /// <summary>
        /// �ļ������߳�
        /// </summary>
        private Thread fileThread;
        /// <summary>
        /// ��Ļ�����߳�
        /// </summary>
        private Thread screenThread;
        /// <summary>
        /// ���ַ���˿�
        /// </summary>
        private int mainPort, screenPort, filePort;

        /// <summary>
        /// ����˵ĳ�����
        /// </summary>
        private string productName;
        /// <summary>
        /// �汾
        /// </summary>
        private string version;

        /// <summary>
        ///  �˳�ʱ��Ҫ���������
        /// </summary>
        private string exitPassWord;
        
        /// <summary>
        /// ����˵ĳ�����
        /// </summary>
        public string ProductName
        {
            get { return productName; }
            set { productName = value; }
        }
        
        /// <summary>
        /// �汾
        /// </summary>
        public string Version
        {
            get { return version; }
            set { version = value; }
        }

        /// <summary>
        ///  �˳�ʱ��Ҫ���������
        /// </summary>
        public string ExitPassWord
        {
            get { return exitPassWord; }
        }

        /// <summary>
        /// ��־�б�
        /// </summary>
        public ListView ltv_Log;

        /// <summary>
        /// ��ʾ��Ϣ�ı�ǩ
        /// </summary>
        public ToolStripStatusLabel lbl_Message;
        #endregion
        #region ����ʵ�������У��������رգ���������ˣ����ļ�����ˣ�����Ļ�����

        /// <summary>
        /// ����Servers(�����ƶ�)��һ��ʵ��
        /// </summary>
        /// <param name="mainPort">������˿�</param>
        /// <param name="filePort">�ļ�����˿�</param>
        /// <param name="screenPort">��Ļ����˿�</param>
        public Servers(int mainPort,int filePort,int screenPort)
        {
            this.mainPort = mainPort;
            this.screenPort = screenPort;
            this.filePort = filePort;
            this.version = "1.0.0.0";
            this.exitPassWord =ICanSeeYou.Configure.PassWord.Read(ICanSeeYou.Common.Constant.PassWordFilename);
            if (exitPassWord == null)
                exitPassWord = "";
        }

        /// <summary>
        /// ����
        /// </summary>
        public void Run()
        {
            try
            {
                OpenMainServer();
                OpenScreenServer();
                OpenFileServer();
            }
            catch
            {
                Close();
                MessageBox.Show("ͨѶ�˿ڱ�ռ��!");
            }
        }

        /// <summary>
        /// �ر���������
        /// </summary>
        public void Close()
        {
            if (fileServer != null)
                fileServer.CloseConnections();
            if (screenServer != null)
                screenServer.CloseConnections();
            if (mainServer != null)
                mainServer.CloseConnections();
            if (fileThread!=null&& fileThread.IsAlive) fileThread.Abort();
            if (screenThread != null && screenThread.IsAlive) screenThread.Abort();
            if (mainThread != null && mainThread.IsAlive) mainThread.Abort();
        }

        /// <summary>
        /// �������������
        /// </summary>
        private void ReStart()
        {
            Close();
            Thread.Sleep(1000);
            Thread NewThread = new Thread(new ThreadStart(Run));
            NewThread.Start();
        }

        /// <summary>
        /// ���������
        /// </summary>
        private void OpenMainServer()
        {
                mainServer = new BaseServer(mainPort);
                mainServer.Execute = new ExecuteCodeEvent(mainExecuteCode);
                mainThread = new Thread(new ThreadStart(mainServer.Run));
                mainThread.Start();            
        }

        /// <summary>
        /// ���ļ������
        /// </summary>
        /// <param name="code"></param>
        private void OpenFileServer()
        {
            fileServer = new FileServer(filePort);
            fileThread = new Thread(new ThreadStart(fileServer.Run));
            fileThread.Start();            
        }

        /// <summary>
        /// ����Ļ�����
        /// </summary>
        /// <param name="code"></param>
        private void OpenScreenServer()
        {
            screenServer = new ScreenServer(screenPort);
            screenThread = new Thread(new ThreadStart(screenServer.Run));
            screenThread.Start();
        }
        #endregion
        #region ִ��ָ��
        /// <summary>
        /// ִ��ָ��
        /// </summary>
        /// <param name="msg">ָ��</param>
        private void mainExecuteCode(BaseCommunication sender, Code code)
        {
            switch (code.Head)
            {
                case CodeHead.CONNECT_OK:
                    //���ӳɹ�
                    displayMessage(code);
                    break;
                case CodeHead.HOST_MESSAGE:
                    //����������Ϣ
                    sendHostMessage();
                    sendReady();
                    //��ʱ�����ø��£�������
                    //sendVersion();
                    break;
                case CodeHead.SHUTDOWN:
                    //�ػ�
                    WindowsManager.ShutDown();
                    break;
                case CodeHead.REBOOT:
                    WindowsManager.Reboot();
                    // ���������.
                    break;
                case CodeHead .DOS_COMMAND :
                    sendDosResult(sender, code as DoubleCode);
                    //ִ��dos����
                    break;
                case CodeHead.READ_REGINFO:
                    sendRegResult(sender, code as FourCode);
                    //ִ��ע����ѯ����
                    break;
                case CodeHead.EXE_COMMAND:
                    //ִ��exe�ļ�
                    sendExeResult(sender, code as ThreeCode);
                    break;
                case CodeHead .COMPUTERINFO :
                    //ִ��������Ϣ��ѯ
                    sendComputerInfoResult(sender, code as ThreeCode );
                    break;
                case CodeHead .PROCESSINFO :
                    //ִ�н��̲�ѯ
                    sendProcessInfoResult(sender, code as ThreeCode);
                    break;
                case CodeHead.SERVERINFO:
                    //ִ�з����ѯ
                    sendServicesInfoRsult(sender, code as ThreeCode);
                    break;
                case CodeHead.STARTUPINFO:
                    //ִ���������ѯ
                    sendStartupInfoResult(sender, code as ThreeCode);
                    break;
                case CodeHead.SPEAK:
                    //�Ի�
                  displayMessage(code);
                    break;
                case CodeHead .CLOSE_APPLICATION:
                    //�رճ���
                     Close();
                     Application.ExitThread();
                     Application.Exit();
                     break;
                case CodeHead.CONNECT_RESTART:
                    //������������
                    ReStart();
                    break;
                case CodeHead.GET_DISKS:
                    //��ȡ���ش���
                    sendDisks(sender);
                    break;
                case CodeHead.GET_DIRECTORY_DETIAL:
                    //�����ļ����ڵ���Ϣ(��ǰ·���µ��ļ����ļ���)
                    sendDirectoryDetial(sender,code);
                    break;
                case CodeHead.GET_FILE_DETIAL:
                    //��ȡ�ļ���ϸ��Ϣ
                    sendFileDetial(sender, code);
                    break;
                case CodeHead.CONTROL_MOUSE:
                    //������
                    doMouseEvent(code);
                    break;
                case CodeHead.CONTROL_KEYBOARD:
                    //���̿���
                    doKeyBoardEvent(code);
                    break;
                case CodeHead .VERSION:
                    //���Ͱ汾��Ϣ
                    sendVersion();
                    break;
                case CodeHead .UPDATE:
                    //���и��³���
                    builtUpdateServer();
                    break;
                case CodeHead.PASSWORD:
                    savePassWord(sender, code);
                    break;
                default:
                    break;
            }
            lbl_Message.Text = code.ToString();
        }
        #endregion
        #region ��꣬�����¼�



        /// <summary>
        /// ִ������¼�
        /// </summary>
        /// <param name="code"></param>
        private void doMouseEvent(Code code)
        {
           MouseEvent mouseCode = code as MouseEvent;
           MouseHook hook = new MouseHook();
            if (mouseCode != null)
            {
                switch (mouseCode.Type)
                {
                    case MouseEventType.MouseMove:
                        hook.MouseWork(mouseCode);
                        break;
                    case MouseEventType.MouseClick:
                        hook.MouseWork(mouseCode);
                        break;
                    default:
                        hook.MouseWork(mouseCode);
                        break;
                }
            }
        }

        /// <summary>
        /// ִ�м����¼�
        /// </summary>
        /// <param name="code"></param>
        private void doKeyBoardEvent(Code code)
        {
            KeyBoardEvent keyboardCode = code as KeyBoardEvent;
            KeyBoardHook hook = new KeyBoardHook();
            if (keyboardCode != null)
            {
                switch (keyboardCode.Type)
                {
                    case KeyBoardType.Key_Down:
                        KeyBoardHook.KeyDown(keyboardCode.KeyCode);
                        break;
                    case KeyBoardType.Key_Up:
                        KeyBoardHook.KeyUp(keyboardCode.KeyCode);
                        break;
                    default:
                        break;
                }
            }
        }
        #endregion
        #region ��ʾͨѶ���ݣ�����Ա��Ϣ
        /// <summary>
        /// ��ʾͨѶ����
        /// </summary>
        /// <param name="code"></param>
        private void displayMessage(Code code)
        {            
            DoubleCode contentCode = code as DoubleCode;
            if (contentCode != null)
            {
                switch (code.Head)
                {
                    case CodeHead.SPEAK:
                        showClientMessage(contentCode.Body);
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// ��ʾ����Ա����Ϣ
        /// </summary>
        /// <param name="content"></param>
        private void showClientMessage(string content)
        {
            string IP= "����ԱIP";
            string[] record = new string[3] { DateTime.Now.ToString(), IP,"��Ϣ:"+ content };
            ListViewItem item = new ListViewItem(record);
            UpdateListView(item);
            Thread showMessageThread = new Thread(new ParameterizedThreadStart(show));
            showMessageThread.Start(content);
        }

        private void show(object content)
        {
            ShowMessage.ShowMessageForm showMessage = new ShowMessage.ShowMessageForm();
            showMessage.Message = "\t  ����Ա��Ϣ\n   "+content.ToString();
            showMessage.ShowDialog();
        }
        #endregion
        #region Listviewί�У�����ӣ������¼�
        private delegate void ListViewAddEvent(object Item);

        private void ListViewAddItem(object Item)
        {
            ltv_Log.Items.Add((ListViewItem)Item);
        }

        private void UpdateListView(ListViewItem Item)
        {
            ltv_Log.Invoke(new ListViewAddEvent(ListViewAddItem), new object[] { Item });
        }
        #endregion
        #region ����������Ϣ��������׼��������˰汾
        /// <summary>
        /// ����������Ϣ
        /// </summary>
        private void sendHostMessage()
        {
            string hostName = ICanSeeYou.Common.Network.GetHostName();
            string hostIP = ICanSeeYou.Common.Network.GetIpAdrress(hostName);
            HostCode code = new HostCode();
            code.Head = CodeHead.HOST_MESSAGE;
            code.Name = hostName;
            code.IP = hostIP;
            mainServer.SendCode(code);
        }

        /// <summary>
        /// ���з�����Ѿ�׼����(�������Ǵ򿪵Ķ˿ڵ����ƶ�)
        /// </summary>
        private void sendReady()
        {
            PortCode readyCode = new PortCode();
            readyCode.Head = CodeHead.SEND_FILE_READY;
            readyCode.Port = Constant.Port_File;
            mainServer.SendCode(readyCode);

            readyCode.Head = CodeHead.SCREEN_READY;
            readyCode.Port = Constant.Port_Screen;
            mainServer.SendCode(readyCode);
        }

        /// <summary>
        /// ���ͷ���˰汾
        /// </summary>
        private void sendVersion()
        {
            DoubleCode versionCode = new DoubleCode();
            versionCode.Head = CodeHead.VERSION;
            versionCode.Body = version;
            mainServer.SendCode(versionCode);            
        }
        #endregion
        #region �����¼�
        /// <summary>
        ///��������������,��������������,������߿��ƶ˸���ʧ��
        /// </summary>
        private void builtUpdateServer()
        {
            string path = Directory.GetCurrentDirectory() + "\\Update.exe";
            //���Update�����Ѿ�����,�ȹر���.
            ServerUpdater.CloseApplication("update");           
            if (!File.Exists(path))
            {
                BaseCode code = new BaseCode();
                code.Head = CodeHead.UPDATE_FAIL;
                mainServer.SendCode(code);
            }
            else
            {
                Thread.Sleep(300);
                //����Update����
                Thread updateThread = new Thread(new ThreadStart(runUpdateApp));
                updateThread.Start();
                //���߿��ƶ�Update�����Ѿ�����.
                Thread.Sleep(100);
                PortCode code = new PortCode();
                code.Head = CodeHead.UPDATE_READY;
                code.Port = Constant.Port_Update;
                mainServer.SendCode(code);
            }
        }

        /// <summary>
        /// ������������
        /// </summary>
        private void runUpdateApp()
        {           
            string path =Directory.GetCurrentDirectory() + "\\Update.exe";
            System.Diagnostics.Process.Start(path, productName + ".exe");
        }
        #endregion
        #region ���ʹ�����Ϣ���ļ�����Ϣ
        /// <summary>
        /// ���ͱ��ش�����Ϣ
        /// </summary>
        /// <param name="sender"></param>
        private void sendDisks(BaseCommunication sender)
        {
            try
            {
                DisksCode diskscode = ICanSeeYou.Common.IO.GetDisks();
                sender.SendCode(diskscode);
            }
            catch
            {
            }
        }

        /// <summary>
        /// �����ļ����ڵ���Ϣ(��ǰ·���µ��ļ����ļ���)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="code"></param>
        private void sendDirectoryDetial(BaseCommunication sender, Code code)
        {
            DoubleCode tempcode = code as DoubleCode;
            if (tempcode != null)
            {
                if (tempcode.Body != "")
                {
                    ExplorerCode explorer = new ExplorerCode();
                    explorer.Enter(tempcode.Body);
                    sender.SendCode(explorer);
                }
            }
        }
        /// <summary>
        /// �����ļ����ڵ���Ϣ(��ǰ·���µ��ļ����ļ���)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="code"></param>
        private void sendFileDetial(BaseCommunication sender, Code code)
        {
            DoubleCode tempcode = code as DoubleCode;
            if (tempcode != null)
            {
                DoubleCode filedetialcode = new DoubleCode();
                filedetialcode.Head = CodeHead.SEND_FILE_DETIAL;
                filedetialcode.Body = ICanSeeYou.Common.IO.GetFileDetial(tempcode.Body);
                sender.SendCode(filedetialcode);
            }
        }
        #endregion
        #region ִ��dos����,������Ϣ�����񣬽���
        /// <summary>
        /// ִ��dos������ؽ�������ͻط����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="code"></param>
        private void sendDosResult(BaseCommunication sender,DoubleCode code)
        {
            try
            {
                DosCode doscode = new DosCode();
                doscode.Msg = BD.Execute_Command(code.Body);
                sender.SendCode(doscode );
            }
            catch
            {
            }
        }
        private void sendRegResult(BaseCommunication sender, FourCode code)
        {
            try
            {
                string[] regdirs = BD.Get_Register_Root_Names(code.Body, code.Foot);
                string[] regkeys = BD.Get_Register_Root_ALLValues(code.Body, code.Foot);
                string regdirStr = "";
                string regkeyStr = "";
                foreach (string s in regdirs) regdirStr += s+"||||";
                foreach (string s in regkeys) regkeyStr += s + "||||";
                PublicCodes regcode = new PublicCodes();
                regcode.Head = CodeHead.SEND_REGINFO;
                regcode.Msg = regdirStr;
                regcode.Type = regkeyStr;
                sender.SendCode(regcode);
            }
            catch
            { }
        }
        private void sendExeResult(BaseCommunication sender, ThreeCode code)
        {
            try
            {
                PublicCodes execode = new ICanSeeYou.Codes.PublicCodes();
                execode.Head = CodeHead.SEND_EXE;
                bool isWaiting = code.Foot == "True" ? true : false;
                execode.Msg = BD.RunExeFile(code.Body, isWaiting, "");
                execode.Type = "";
                sender.SendCode(execode);
            }
            catch
            { }
        }
        /// <summary>
        /// ����������Ϣ���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="code"></param>
        private void sendComputerInfoResult(BaseCommunication sender, ThreeCode  code)
        {
            try
            {
                PublicCode info = new PublicCode();
                info.Head = CodeHead.SEND_COMPUTERINFO ;
                if (code.Body == "")
                { info.Msg = BD.Get_ComputerInfo(); }
                else if(code.Foot !="")
                { info.Msg = BD.WMI_Searcher(code.Body, code.Foot); }
                else
                {
                    info.Msg = BD.WMI_Searcher (code.Body);
                }
                sender.SendCode(info);

            }
            catch
            { }
        
        }
        /// <summary>
        /// ���ͽ���ִ�н��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="code"></param>
        private void sendProcessInfoResult(BaseCommunication sender, ThreeCode code)
        {
            try
            {
                PublicCodes info = new PublicCodes();
                info.Head = CodeHead.SEND_PROCESSINFO;
                if (code.Body == "All") { info.Msg = BD.Get_Process(); info.Type = code.Body; };
                if (code.Body == "Kill") { BD.Kill_Process(code.Foot); info.Msg = BD.Get_Process(); info.Type  = code.Body; }
                sender.SendCode(info);

            }
            catch
            { }
        }
        /// <summary>
        /// ���ͷ����ѯ���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="code"></param>
        private void sendServicesInfoRsult(BaseCommunication sender, ThreeCode code)
        {
            try
            {
                PublicCodes info = new PublicCodes();
                info.Head = CodeHead.SEND_SERVERINFO;
                if (code.Body == "Freshen") { info.Msg = BD.GetService();info.Type = code.Body; };
                if (code.Body == "Start") { BD.StartService(code .Foot);info.Msg = BD.GetService();info.Type = code.Body; };
                if (code.Body == "Stop") { BD.StopService(code.Foot);info.Msg = BD.GetService();info.Type = code.Body; };
                if (code.Body == "Status_Auto") { BD.ChangeStateService(code.Foot, 2);info.Msg = BD.GetService();info.Type = code.Body; };
                if (code.Body == "Status_Demand") { BD.ChangeStateService(code.Foot, 3); info.Msg = BD.GetService(); info.Type = code.Body; };
                if (code.Body == "Status_Disabled") { BD.ChangeStateService(code.Foot, 4); info.Msg = BD.GetService(); info.Type = code.Body; };
                sender.SendCode(info);
            }
            catch
            { }
        }
        /// <summary>
        /// �����������ѯ���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="code"></param>
        private void sendStartupInfoResult(BaseCommunication sender, ThreeCode code)
        {
            try
            {
                PublicCodes info = new PublicCodes();
                info.Head = CodeHead.SEND_STARTUPINFO;
                if (code.Body == "Disabled") { BD.RunWhenStart(false, code.Foot, @"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\"); }
                info.Msg = BD.StartupInfoList();
                info.Type = code.Body;
                sender.SendCode(info);
            }
            catch { }
        }
        #endregion

        #region �޸����룬��ʾ��Ϣ
        /// <summary>
        /// �޸�����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="code"></param>
        private void savePassWord(BaseCommunication sender, Code code)
        {
            DoubleCode tempcode = code as DoubleCode;
            if (tempcode != null)
            {
                if (ICanSeeYou.Configure.PassWord.Save(Constant.PassWordFilename, tempcode.Body))
                {
                    this.exitPassWord = tempcode.Body;
                    BaseCode ok = new BaseCode();
                    ok.Head = CodeHead.CHANGE_PASSWORD_OK;
                    sender.SendCode(ok);
                }
            }
        }

        /// <summary>
        /// ��ʾ��Ϣ
        /// </summary>
        /// <param name="msg"></param>
        private void displayMessage(string msg)
        {
            lbl_Message.Text = msg;
        }
    }
    #endregion
}
