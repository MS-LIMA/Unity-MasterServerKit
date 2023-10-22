using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Msk
{
    [CreateAssetMenu(fileName = "MskConfigSpawner", menuName = "Master Server kit/Create Spawner Config", order = int.MaxValue)]
    public class MskConfigSpawner : ScriptableObject
    {
        [Header("Server Instance Config")]
        [SerializeField] private string m_ip = "127.0.0.1";
        public static string Ip { get { return Instance.m_ip; } }

        [SerializeField] private ushort m_portStart = 25000;
        public static ushort PortStart { get { return Instance.m_portStart; } }

        [Space]
        [SerializeField] private ushort maxInstanceCount = 100;
        public static ushort MaxInstanceCount { get { return Instance.maxInstanceCount; } }

        [Space]
        [SerializeField] private string m_serverInstancePath = "";
        public static string ServerInstancePath { get { return Instance.m_serverInstancePath; } }

        [SerializeField] private string m_instanceFileName = "";
        public static string InstanceFileName { get { return Instance.m_instanceFileName; } }


        [SerializeField] private bool m_useVersionInInstancePath = false;
        public static bool UseVersionInInstancePath { get { return Instance.m_useVersionInInstancePath; } }


        protected static MskConfigSpawner m_instance;
        public static MskConfigSpawner Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = Resources.Load<MskConfigSpawner>("MskConfigSpawner");
                }

                return m_instance;
            }
        }
    }
}