using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Example : MasterServerBehaviour
{
    public GameObject loadingPannel;
    public GameObject lobbyPannel;
    public GameObject roomPannel;

    public Transform roomListContentTransform;
    public GameObject roomListItem;
    private Dictionary<string, RoomListItem> roomListItems = new Dictionary<string, RoomListItem>();

    private void Start()
    {
        ConnectToMaster();
    }

    public void ConnectToMaster()
    {
        MasterServer.ConnectToMaster();
    }

    public void CreateRoom()
    {
        MasterServer.CreateRoom(new RoomOptions { roomName = "go" });
    }

    public void LeaveRoom()
    {
        MasterServer.LeaveRoom();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("On Connected To Master");
        loadingPannel.SetActive(false);
    }

    public override void OnCreatedRoom()
    {
        lobbyPannel.SetActive(false);
        roomPannel.SetActive(true);
    }

    public override void OnLeftRoom()
    {
        roomPannel.SetActive(false);
        lobbyPannel.SetActive(true);
    }

    public override void OnRoomListUpdated(RoomListOp roomListOp, Room room)
    {
        RoomListItem _roomListItem = null;

        switch (roomListOp)
        {
            case RoomListOp.OnCreated:
                _roomListItem = Instantiate(roomListItem, roomListContentTransform).GetComponent<RoomListItem>();
                _roomListItem.Init(room);

                roomListItems.Add(room.RoomId, _roomListItem);
                break;
            case RoomListOp.OnRemoved:
                if (roomListItems.ContainsKey(room.RoomId))
                {
                    _roomListItem = roomListItems[room.RoomId];

                    roomListItems.Remove(room.RoomId);
                    Destroy(_roomListItem.gameObject);
                }
                break;
        }
    }
}
