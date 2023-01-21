using System.Collections;
using UnityEngine;

namespace MasterServerKit
{
    public class MskInstanceMono : MonoBehaviour
    {
        private static MskInstanceMono instance;
        private float timer = 0f;

        public static void Initialize()
        {
            if (instance != null)
            {
                return;
            }

            GameObject go = new GameObject("MskInstanceMono");
            instance = go.AddComponent<MskInstanceMono>();

            DontDestroyOnLoad(go);
        }

        public static void StartTtlFirstPlayerRoutine()
        {
            instance.StartCoroutine(instance.TtlFirstPlayerRoutine(MskInstance.TtlUntilFirstPlayer));
        }

        private IEnumerator TtlFirstPlayerRoutine(float time)
        {
            yield return new WaitForSeconds(time);

            if (!MskInstance.IsFirstPlayerJoined)
            {
                Application.Quit();
            }
        }

        public static void StartTtlEmptyRoomRoutine()
        {
            instance.StartCoroutine(instance.TtlEmptyRoomRoutine());
        }

        private IEnumerator TtlEmptyRoomRoutine()
        {
            while (true)
            {
                if (!MskInstance.IsFirstPlayerJoined)
                {
                    yield return null;
                }
                else
                {
                    if (MskInstance.Room.Players.Count <= 0)
                    {
                        timer += Time.deltaTime;
                        if (timer >= MskInstance.TtlEmptyRoom)
                        {
                            Application.Quit();
                            yield break;
                        }
                    }
                    else
                    {
                        timer = 0f;
                    }

                    yield return null;
                }
            }
        }
    }
}
