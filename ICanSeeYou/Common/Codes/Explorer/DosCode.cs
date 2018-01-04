/*----------------------------------------------------------------
        // Copyright (C) 2007 L3'Studio
        // ��Ȩ���С� 
        // �����ߣ���Զ��˰���ܿ�
        // �ļ�����Publiccode.cs
        // �ļ�����������"ͨ��"ָ���࣬ͨ��Head���֡�
//----------------------------------------------------------------*/

using System;

namespace ICanSeeYou.Codes
{
    /// <summary>
    /// "Dos"ָ����(��Ϊ���л�ָ���������ϴ���)
    /// </summary>
    [Serializable]
    public class DosCode : BaseCode
    {
        private string  msg;
        /// <summary>
        ///Dosִ�н��
        /// </summary>
        public string  Msg
        {
            get { return msg ; }
            set { msg = value; }
        }

        public DosCode() { base.Head = CodeHead.SEND_DOS; }
    }
}
