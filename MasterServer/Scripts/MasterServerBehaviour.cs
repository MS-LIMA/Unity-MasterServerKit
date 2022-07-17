using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterServerBehaviour : MonoBehaviour
{
    public virtual void OnEnable()
    {
        MasterServer.OnConncetedToMaster += OnConnectedToMaster;
        MasterServer.OnCreatedRoom += OnCreatedRoom;
        MasterServer.OnLeftRoom += OnLeftRoom;
        MasterServer.OnRoomListUpdated += OnRoomListUpdated;
    }

    public virtual void OnDisable()
    {
        MasterServer.OnConncetedToMaster -= OnConnectedToMaster;
        MasterServer.OnCreatedRoom -= OnCreatedRoom;
        MasterServer.OnLeftRoom -= OnLeftRoom;
        MasterServer.OnRoomListUpdated -= OnRoomListUpdated;
    }

    public virtual void OnConnectedToMaster()
    {
    }

    public virtual void OnCreatedRoom()
    {

    }

    public virtual void OnLeftRoom()
    {
    }

    public virtual void OnRoomListUpdated(RoomListOp roomListOp, Room room)
    {
    }
}
