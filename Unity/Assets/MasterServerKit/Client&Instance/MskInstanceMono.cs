using System.Collections;
using UnityEngine;

namespace Msk
{
    public class MskInstanceMono : MonoBehaviour
    {
        private static MskInstanceMono m_instance;
        private float m_timer = 0f;

        public static void Initialize()
        {
            if (m_instance != null)
            {
                return;
            }

            GameObject go = new GameObject("MskInstanceMono");
            m_instance = go.AddComponent<MskInstanceMono>();

            DontDestroyOnLoad(go);

            m_instance.Invoke(nameof(KillAfterTimer), 60f);
        }

        private void KillAfterTimer()
        {
            if (!MasterServerKit.Instance.IsFirstPlayerJoined || !MasterServerKit.IsConnected || !MasterServerKit.Instance.IsRoomRegistered)
            {
                Application.Quit();
            }
        }

        public static void StartTtlFirstPlayerRoutine()
        {
            m_instance.StartCoroutine(m_instance.TtlFirstPlayerRoutine(MasterServerKit.Instance.TtlUntilFirstPlayer));
        }

        private IEnumerator TtlFirstPlayerRoutine(float time)
        {
            yield return new WaitForSeconds(time);

            if (!MasterServerKit.Instance.IsFirstPlayerJoined)
            {
                Application.Quit();
            }
        }

        public static void StartTtlEmptyRoomRoutine()
        {
            m_instance.StartCoroutine(m_instance.TtlEmptyRoomRoutine());
        }

        private IEnumerator TtlEmptyRoomRoutine()
        {
            while (true)
            {
                if (!MasterServerKit.Instance.IsFirstPlayerJoined)
                {
                    yield return null;
                }
                else
                {
                    if (MasterServerKit.Room.Players.Count <= 0)
                    {
                        m_timer += Time.deltaTime;
                        if (m_timer >= MasterServerKit.Instance.TtlEmptyRoom)
                        {
                            Application.Quit();
                            yield break;
                        }
                    }
                    else
                    {
                        m_timer = 0f;
                    }

                    yield return null;
                }
            }
        }
    }
}
