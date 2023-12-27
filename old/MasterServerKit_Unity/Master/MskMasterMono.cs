using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Msk.Master
{
    public class MskMasterMono : MonoBehaviour
    {
        private static MskMasterMono m_instance;

        public static void Initialize()
        {
            if (m_instance != null)
            {
                return;
            }

            GameObject go = new GameObject("MskMasterMono");
            m_instance = go.AddComponent<MskMasterMono>();

            DontDestroyOnLoad(go);
        }

        //private void Update()
        //{
        //    lock (MasterServerKit.Master.ConnectedClients)
        //    {
        //        foreach (MskSocket client in MasterServerKit.Master.ConnectedClients.Values)
        //        {
        //            if (!client.IsConnected())
        //            {
        //                client.Disconnect();
        //            }
        //        }
        //    }
        //}

    }
}