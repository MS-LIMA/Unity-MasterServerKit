using System.Collections.Generic;
using System;
using System.Collections;
using System.Threading;

namespace Msk
{
    public class MskDispatcher
    {
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
                waitForMilliseconds = (int)((1f / m_dispatchRate) * 1000);
            }
        }

        private static int m_dispatchRate = 30;
        private static int waitForMilliseconds = (int)((1f / m_dispatchRate) * 1000);

        /// <summary>
        /// Initialize the dispatcher.
        /// </summary>
        public static void Initialize()
        {
            Thread main = new Thread(new ThreadStart(Update));
            main.Start();
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

        private static void InvokeCallbacksOnMain()
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
                        Console.WriteLine("Error : " + e.Message);
                    }
                }
            }
        }

        private static void Update()
        {
            while (true)
            {
                InvokeCallbacksOnMain();

                Thread.Sleep(waitForMilliseconds);
            }
        }
    }
}