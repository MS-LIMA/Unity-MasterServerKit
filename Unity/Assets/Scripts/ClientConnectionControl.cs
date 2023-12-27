using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Msk;

public class ClientConnectionControl : MonoBehaviourMskClientCallbacks
{
    private void Start()
    {
        MasterServerKit.Client.ConnectToMaster();
    }

    public override void OnConnectedToMaster()
    {
        // Invoked when connect to the master success.
    }
}
