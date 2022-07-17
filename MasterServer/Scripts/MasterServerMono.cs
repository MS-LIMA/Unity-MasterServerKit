using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MasterServerMono : MonoBehaviour
{
    public static MasterServerMono instance;
    private Queue<Action> callbacks = new Queue<Action>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Update()
    {
        lock (callbacks)
        {
            while(callbacks.Count > 0)
            {
                Action action = callbacks.Dequeue();
                action.Invoke();
            }
        }
    }

    public static void EnqueueCallback(Action action)
    {
        if (instance == null)
        {
            return;
        }

        lock (instance.callbacks)
        {
            instance.callbacks.Enqueue(action);
        }
    }
}
