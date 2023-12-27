using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Msk
{
    [CreateAssetMenu(fileName = "MskConfigMaster", menuName = "Master Server kit/Create Spawner Master", order = int.MaxValue)]
    public class MskConfigMaster : ScriptableObject
    {
        [Header("Server Instance Config")]
        [SerializeField] private bool m_isIpDomain = false;
        public static bool IsIpDomain { get { return Instance.m_isIpDomain; } } 

        [SerializeField] private string m_ip = "127.0.0.1";
        public static string Ip { get { return Instance.m_ip; } }


        [SerializeField] private ushort m_port = 20000;
        public static ushort Port { get { return Instance.m_port; } }


        [SerializeField] private ushort m_maxConnections = 100;
        public static ushort MaxConnections { get { return Instance.m_maxConnections; } }




        protected static MskConfigMaster m_instance;
        public static MskConfigMaster Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = Resources.Load<MskConfigMaster>("MskConfigMaster");
                }

                return m_instance;
            }
        }
    }
}