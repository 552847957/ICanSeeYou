/*----------------------------------------------------------------
        // Copyright (C) 2007 L3'Studio
        // ��Ȩ���С� 
        // �����ߣ���Զ��˰���ܿ�
        // �ļ�����DisksCode.cs
        // �ļ������������漰���ļ������ָ�"dos"ָ���ࡣ
//----------------------------------------------------------------*/

using System;

namespace ICanSeeYou.Codes
{
    /// <summary>
    /// "Dos"ָ����(��Ϊ���л�ָ���������ϴ���)
    /// </summary>
    [Serializable]
    public class PublicCode : BaseCode
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

        public PublicCode() { }
    }
}
