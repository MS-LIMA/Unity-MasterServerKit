using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Msk
{
    [CreateAssetMenu(fileName = "MskConfigClient", menuName = "Master Server kit/Create Client Config", order = int.MaxValue)]
    public class MskConfigClient : ScriptableObject
    {
        [Header("Server Instance Config")]
        [SerializeField]
        [Tooltip("Version of the game. Master server will distinguish users and rooms by this version." +
            " Only the users and rooms with same version can know each others.")]
        private string m_version = "";

        /// <summary>
        /// Version of the game. Master server will distinguish users and rooms by this version.
        /// Only the users and rooms with same version can know each others.
        /// </summary>
        public static string Version { get { return Instance.m_version; } }

        [Space]
        [SerializeField]
        [Tooltip("Is the host uses dns? If true, it will be parsed to the IPv6. Otherwise, it will be treated as IPv6.")]
        private bool m_dnsForHost = false;

        /// <summary>
        /// Is the host uses dns? If true, it will be parsed to the IPv6. Otherwise, it will be treated as IPv6.
        /// </summary>
        public static bool DnsForHost { get { return Instance.m_dnsForHost; } } 

        [SerializeField]
        [Tooltip("Address of the master server.")]
        private string m_host = "127.0.0.1";

        /// <summary>
        /// Address of the master server.
        /// </summary>
        public static string Host { get { return Instance.m_host; } }


        [SerializeField]
        [Tooltip("Port number of the master server.")]
        private ushort m_port = 20000;

        /// <summary>
        /// Port number of the master server.
        /// </summary>
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