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
    public class PublicCodes : PublicCode 
    {
        private string  type;
        /// <summary>
        ///Dosִ�н��
        /// </summary>
        public string  Type
        {
            get { return type ; }
            set { type  = value; }
        }

        public PublicCodes() { }
    }
}
