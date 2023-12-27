using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Msk
{
    [CreateAssetMenu(fileName = "MskConfigClient", menuName = "Master Server kit/Create Client Config", order = int.MaxValue)]
    public class MskConfigClient : ScriptableObject
    {
        [Header("Server Instance Config")]
        [SerializeField] private string m_version = "";
        public static string Version { get { return Instance.m_version; } }



        [SerializeField] private bool m_isIpDomain = false;
        public static bool IsIpDomain { get { return Instance.m_isIpDomain; } } 

        [SerializeField] private string m_ip = "127.0.0.1";
        public static string Ip { get { return Instance.m_ip; } }


        [SerializeField] private ushort m_port = 20000;
        public static ushort Port { get { return Instance.m_port; } }


        protected static MskConfigClient m_instance;
        public static MskConfigClient Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = Resources.Load<MskConfigClient>("MskConfigClient");
                }

                return m_instance;
            }
        }
    }
}