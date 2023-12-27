using System.Collections.Generic;
using System;
using UnityEngine;
using System.Collections;

namespace Msk
{
    public class MskDispatcher : MonoBehaviour
    {
        private static MskDispatcher m_instance;
        private static Queue<Action> m_callbacks = new Queue<Action>();

        /// <summary>
        /// How many dispatches occur per second?
        /// </summary>
        public static int DispatchRate
        {
            get { return m_dispatchRate; }
            set 
            {
                if (value <= 0)
                {
                    return;
                }

                m_dispatchRate = value;
                m_dispatchInterval = 1f / value;
                waitForSeconds = new WaitForSeconds(m_dispatchInterval);
            }
        }

        private static int m_dispatchRate = 30;
        private static float m_dispatchInterval = 1f / 30;
        private static WaitForSeconds waitForSeconds = new WaitForSeconds(1f / 30);

        /// <summary>
        /// Initialize the dispatcher.
        /// </summary>
        public static void Initialize()
        {
            if (m_instance == null)
            {
                GameObject go = new GameObject("MskDispatcher");
                m_instance = go.AddComponent<MskDispatcher>();

                m_instance.StartCoroutine(nameof(Routine));

                DontDestroyOnLoad(go);
            }
        }

        /// <summary>
        /// Enqueue callbacks to the dispatcher.
        /// </summary>
        /// <param name="callback"></param>
        public static void EnqueueCallback(Action callback)
        {
            lock (m_callbacks)
            {
                m_callbacks.Enqueue(callback);
            }
        }

        private void InvokeCallbacksOnMain()
        {
            lock (m_callbacks)
            {
                while (m_callbacks.Count > 0)
                {
                    try
                    {
                        Action callback = m_callbacks.Dequeue();
                        callback?.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Error : " + e.Message);
                    }
                }
            }
        }

        private IEnumerator Routine()
        {
            while (true)
            {
                InvokeCallbacksOnMain();

                yield return waitForSeconds;
            }
        }
    }
}