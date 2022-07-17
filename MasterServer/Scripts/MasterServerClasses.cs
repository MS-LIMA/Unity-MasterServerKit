using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;

public class Player
{
    [JsonProperty] private string id;
    [JsonProperty] private string nickname;
    [JsonProperty] private string roomId;
    [JsonProperty] private Dictionary<string, object> customProperties;

    [JsonIgnore] public string Id { get { return id; } }
    [JsonIgnore] public string Nickname { get { return nickname; } }
    [JsonIgnore] public string RoomId { get { return roomId; } }
    [JsonIgnore] public Dictionary<string, object> CustomProperties { get { return customProperties; } }

    public Player(string id, string nickname, string roomId, Dictionary<string, object> customProperties)
    {
        this.id = string.IsNullOrEmpty(id) ? Guid.NewGuid().ToString() : id;
        this.nickname = string.IsNullOrEmpty(nickname) ? "" : nickname;
        this.roomId = roomId;
        this.customProperties = customProperties;
    }
}

public class Room
{
    [JsonProperty] private string roomId;
    [JsonProperty] private string roomName;
    [JsonProperty] private string password;
    [JsonProperty] private string passwordEnabled;
    [JsonProperty] private Player owner;
    [JsonProperty] private int maxPlayer;
    [JsonProperty] private Dictionary<string, Player> players;
    [JsonProperty] private Dictionary<string, object> customProperties;

    [JsonIgnore] public string RoomId { get { return roomId; } }
    [JsonIgnore] public string RoomName { get { return roomName; } }
    [JsonIgnore] public string PasswordEnabled { get { return passwordEnabled; } }
    [JsonIgnore] public Player Owner { get { return owner; } }
    [JsonIgnore] public int MaxPlayer { get { return maxPlayer; } }
    [JsonIgnore] public int PlayerCount { get { return players.Count; } }
    [JsonIgnore] public Dictionary<string, Player> Players { get { return players;} set { players = value; } }
    [JsonIgnore] public Dictionary<string, object> CustomProperties { get { return customProperties; } }

    /// <summary>
    /// Do not use this function to set owner. Use MasterServer.SetOwner() instead.
    /// </summary>
    /// <param name="player"></param>
    public void SetOwner(Player player)
    {
        owner = player;
    }

    /// <summary>
    /// Do not use this method to remove player.
    /// </summary>
    /// <param name="player"></param>
    public void RemovePlayer(Player player)
    {
        players.Remove(player.Id);
    }
}

public class RoomOptions
{
    public string roomName = "";
    public string password = "";
    public int maxPlayer = 20;
    public Dictionary<string, object> customProperties;
}

public enum Op : byte
{
    ConnectToMaster,
    OnConnectedToMaster,

    CreateRoom,
    OnCreatedRoom,
    OnCreateRoomFailed,

    JoinRoom,
    OnJoinedRoom,
    OnJoinRoomFailed,

    LeaveRoom,
    OnLeftRoom,
    OnLeaveRoomFailed,

    OnRoomListUpdated,

}

public enum RoomListOp : byte
{
    OnCreated,
    OnRoomNameEdit,
    OnPasswordEdit,
    OnPasswordEnabled,
    OnOwnerChanged,
    OnMaxPlayerChanged,
    OnPlayerJoined,
    OnPlayerLeft,
    OnCustomPropertiesChanged,
    OnRemoved
}

public enum FailureCause :byte
{
    RoomIdNotFound,
    RoomIdNotSame,
    NotInRoom
}