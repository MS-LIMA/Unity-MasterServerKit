using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using Newtonsoft.Json;

public class MasterServerWebSocket
{
    private static MasterServerWebSocket instance;
    private WebSocket ws;

    public static Action OnOpenWebSocket;
    public static Action<string> OnMessageWebSocket;
    public static Action OnCloseWebSocket;
    public static Action<string> OnErrorWebSocket;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    public static void Init()
    {
        if (instance == null)
        {
            instance = new MasterServerWebSocket();
        }
    }

    public static void ConnectToMasterServer(string ip, string port)
    {
        try
        {
            instance.ws = new WebSocket(string.Format("ws://{0}:{1}", ip, port));

            instance.ws.OnOpen += OnOpen;
            instance.ws.OnClose += OnClose;
            instance.ws.OnMessage += OnMessage;
            instance.ws.OnError += OnError;
            instance.ws.Connect();
        }
        catch
        {

        }
    }

    public static void SendMessage(object jObject)
    {
        instance.ws.Send(JsonConvert.SerializeObject(jObject));
    }

    private static void OnOpen(object sender, EventArgs e)
    {
        OnOpenWebSocket?.Invoke();
    }

    private static void OnError(object sender, ErrorEventArgs e)
    {
        OnErrorWebSocket?.Invoke(e.Message);
        Debug.Log("Error : " + e.Message);
    }

    private static void OnMessage(object sender, MessageEventArgs e)
    {
        OnMessageWebSocket?.Invoke(e.Data);
    }

    private static void OnClose(object sender, CloseEventArgs e)
    {
        OnCloseWebSocket?.Invoke();
    }
}
