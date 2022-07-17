using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;

public class MasterServer
{
    public static Player LocalPlayer { get; private set; }
    public static Room CurrentRoom { get; private set; }
    public static Dictionary<string, Room> Rooms { get; private set; }

    public static Action OnConncetedToMaster;
    public static Action OnCreatedRoom;
    public static Action OnLeftRoom;

    public static Action<Player> OnOwnerChanged;
    public static Action<Player> OnPlayerLeft;

    public static Action<RoomListOp, Room> OnRoomListUpdated;


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    public static void Init()
    {
        MasterServerWebSocket.OnMessageWebSocket += OnMessage;
        MasterServerWebSocket.OnOpenWebSocket += OnOpen;

        Rooms = new Dictionary<string, Room>();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void InitMono()
    {
        if (MasterServerMono.instance == null)
        {
            new GameObject("MasterServerMono").AddComponent<MasterServerMono>();
        }
    }

    //////////////////////////////////////////////



    //////////////////////////////////////////////
    public static void OnMessage(string data)
    {
        MasterServerMono.EnqueueCallback(() =>
        {
            dynamic jObject = JsonConvert.DeserializeObject<dynamic>(data);
            byte opCode = (byte)jObject.op;

            Debug.Log((Op)opCode);

            switch ((Op)opCode)
            {
                case Op.OnConnectedToMaster:
                    _OnConnectedToMaster();
                    break;
                case Op.OnCreatedRoom:
                    _OnCreatedRoom(jObject.room.ToString());
                    break;
                case Op.OnLeftRoom:
                    _OnLeftRoom();
                    break;
                case Op.OnRoomListUpdated:
                    List<byte>roomOpCodes = new List<byte>();
                    foreach(byte code in jObject.roomListOp)
                    {
                        roomOpCodes.Add(code);
                    }
                    _OnRoomListUpdated(roomOpCodes.ToArray(), jObject);
                    break;
                case Op.OnJoinedRoom:
                    _OnJoinedRoom((string)jObject.roomId);
                    break;
                default:
                    break;
            }
        });
    }

    public static void OnOpen()
    {
        MasterServerWebSocket.SendMessage(new Dictionary<string, object>
        {
            { "op" , Op.ConnectToMaster },
            { "player", LocalPlayer }
        });
    }

    //////////////////////////////////////////////
    /// <summary>
    /// Connect to master server.
    /// </summary>
    /// <param name="userId">Enter a unique player id. If left blank, random id will be given.</param>
    /// <param name="nickname"></param>
    public static void ConnectToMaster(string userId = null, string nickname = null)
    {
        CurrentRoom = null;
        LocalPlayer = new Player(userId, nickname, "", new Dictionary<string, object>());

        MasterServerSettings masterServerSettings = Resources.Load<MasterServerSettings>("MasterServerSettings");
        MasterServerWebSocket.ConnectToMasterServer(masterServerSettings.ip, masterServerSettings.port);
    }

    private static void _OnConnectedToMaster()
    {
        OnConncetedToMaster?.Invoke();
    }

    public static void CreateRoom(RoomOptions roomOptions)
    {
        MasterServerWebSocket.SendMessage(new Dictionary<string, object>()
        {
            { "op", Op.CreateRoom },
            { "roomOptions", roomOptions }
        });
    }

    private static void _OnCreatedRoom(string json)
    {
        CurrentRoom = JsonConvert.DeserializeObject<Room>(json);
        if (!Rooms.ContainsKey(CurrentRoom.RoomId))
        {
            Rooms.Add(CurrentRoom.RoomId, CurrentRoom);
        }

        OnCreatedRoom?.Invoke();
    }

    public static void LeaveRoom()
    {
        if (CurrentRoom == null)
        {
            return;
        }

        MasterServerWebSocket.SendMessage(new Dictionary<string, object>
        {
            {"op", Op.LeaveRoom },
            {"roomId", CurrentRoom.RoomId }
        });
    }

    private static void _OnLeftRoom()
    {
        OnLeftRoom?.Invoke();
    }

    private static void _OnLeftRoomFailed(FailureCause failureCause)
    {
    }

    public static void JoinRoom(string roomId)
    {
        MasterServerWebSocket.SendMessage(new Dictionary<string, object>
        {
            {"op", Op.JoinRoom },
            {"roomId", roomId }
        });
    }

    private static void _OnJoinedRoom(string roomId)
    {
        if (Rooms.ContainsKey(roomId))
        {
        
        }
        else
        {
            //request room
        }
    }

    private static void _OnRoomListUpdated(byte[] roomListOpCodes, dynamic jObject)
    {
        foreach (byte roomListOpCode in roomListOpCodes)
        {
            Room room = null;
            string roomId = null;

            RoomListOp roomListOp = (RoomListOp)roomListOpCode;

            switch (roomListOp)
            {
                case RoomListOp.OnCreated:
                    room = JsonConvert.DeserializeObject<Room>(jObject.room.ToString());
                    if (!Rooms.ContainsKey(room.RoomId))
                    {
                        Rooms.Add(room.RoomId, room);
                    }
                    OnRoomListUpdated?.Invoke(roomListOp, room);
                    break;
                case RoomListOp.OnRemoved:
                    roomId = (string)jObject.roomId;
                    if (Rooms.ContainsKey(roomId))
                    {
                        room = Rooms[roomId];
                        Rooms.Remove(roomId);
                        OnRoomListUpdated?.Invoke(roomListOp, room);
                    }
                    break;
                case RoomListOp.OnOwnerChanged:
                    roomId = (string)jObject.roomId;
                    if (Rooms.ContainsKey(roomId))
                    {
                        string ownerId = (string)jObject.ownerId;
                        room = Rooms[roomId];
                        room.SetOwner(room.Players[ownerId]);
                        OnOwnerChanged(room.Owner);
                    }
                    break;
                case RoomListOp.OnPlayerLeft:
                    roomId = (string)jObject.roomId;
                    if (Rooms.ContainsKey(roomId))
                    {
                        string playerLeftId = (string)jObject.playerLeftId;
                        Player playerLeft = room.Players[playerLeftId];

                        room = Rooms[roomId];
                        room.RemovePlayer(playerLeft);
                        OnPlayerLeft(playerLeft);
                    }
                    break;
            }
        }
    }
}
