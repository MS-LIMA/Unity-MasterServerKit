using System.Collections.Generic;
using System;
using UnityEngine;

public class MskDispatcher : MonoBehaviour
{
    private static MskDispatcher instance;
    private static Queue<Action> callbacks = new Queue<Action>();
    
    private void Update()
    {
        InvokeCallbacksOnMain();
    }

    private void InvokeCallbacksOnMain()
    {
        lock (callbacks)
        {
            while(callbacks.Count > 0)
            {
                Action callback = callbacks.Dequeue();
                callback?.Invoke();
            }
        }
    }

    public static void Initialize()
    {
        if (instance == null)
        {
            GameObject go = new GameObject("MskDispatcher");
            instance = go.AddComponent<MskDispatcher>();

            DontDestroyOnLoad(go);
        }
    }

    public static void EnqueueCallback(Action callback)
    {
        lock (callbacks)
        {
            callbacks.Enqueue(callback);
        }
    }
}
