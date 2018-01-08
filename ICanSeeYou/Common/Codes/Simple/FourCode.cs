/*----------------------------------------------------------------
        // Copyright (C) 2007 L3'Studio
        // ��Ȩ���С� 
        // �����ߣ�L3'Studio�Ŷ�
        // �ļ�����ThreeCode.cs
        // �ļ������������̳���DoubleCode��
//----------------------------------------------------------------*/

using System;

namespace ICanSeeYou.Codes
{
    /// <summary>
    /// ��ָ��(���������������ļ������ϴ���ָ��)
    /// </summary>
    [Serializable]
    public class FourCode : ThreeCode 
    {
        private string other;
        /// <summary>
        /// ָ��β��
        /// </summary>
        public string Other
        {
            get { return other ; }
            set { other  = value; }
        }

        public override string ToString()
        {
            return base.ToString() + ",Other=" + other ;
        }
    }
}
