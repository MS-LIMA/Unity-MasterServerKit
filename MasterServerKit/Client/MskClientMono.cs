using System.Collections;
using UnityEngine;
using System;

namespace Msk
{
    public class MskClientMono : MonoBehaviourMskClientCallbacks
    {
        private static MskClientMono m_instance;

        private static IEnumerator m_ttlConnectoToMasterRoutine;

        public static Action onConnectFailed;
        
        protected override void OnDisable()
        {
            base.OnDisable();

            m_instance = null;
            m_ttlConnectoToMasterRoutine = null;
        }

        public static void Initialize()
        {
            if (m_instance != null)
            {
                return;
            }

            GameObject go = new GameObject("MskClientMono");
            m_instance = go.AddComponent<MskClientMono>();

            DontDestroyOnLoad(go);
        }

        public static void StartTtlConnectToMasterRoutine()
        {
            if (m_instance == null)
            {
                return;
            }

            if (m_ttlConnectoToMasterRoutine != null)
            {
                m_instance.StopCoroutine(m_ttlConnectoToMasterRoutine);
            }

            m_ttlConnectoToMasterRoutine = TtlConnectToMasterRoutine();
            m_instance.StartCoroutine(m_ttlConnectoToMasterRoutine);
        }

        private static IEnumerator TtlConnectToMasterRoutine()
        {
            float ttl = MasterServerKit.Client.TimeOutConnectMaster;
            int tryCount = MasterServerKit.Client.TryCountConnectMaster;

            for (int i = 0; i < tryCount; i++)
            {
                yield return new WaitForSecondsRealtime(ttl);

                if (!MasterServerKit.IsConnected)
                {
                    onConnectFailed?.Invoke();

                    Debug.Log("Connect to master failed - trial " + (i + 1));
                }
                else
                {
                    break;
                }
            }

            m_ttlConnectoToMasterRoutine = null;
        }

        public override void OnConnectedToMaster()
        {
            if (m_ttlConnectoToMasterRoutine != null)
            {
                StopCoroutine(m_ttlConnectoToMasterRoutine);
                m_ttlConnectoToMasterRoutine = null;
            }
        }
    }
}
