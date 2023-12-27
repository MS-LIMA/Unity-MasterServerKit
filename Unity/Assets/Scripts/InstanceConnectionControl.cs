using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Msk;

public class InstanceConnectionControl : MonoBehaviourMskInstanceCallbacks
{
    private void Start()
    {
        MasterServerKit.Instance.ConnectToMaster();
    }

    public override void OnConnectedToMaster()
    {
        // Invoked when connect to the master success.
    }

    public override void OnRoomRegistered()
    {
        // Invoked when the room is registerd to the master server.
    }
}
