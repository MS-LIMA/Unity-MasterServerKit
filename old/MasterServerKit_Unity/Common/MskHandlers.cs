using System;

namespace Msk
{
    // Server
    public delegate void PacketHandler(MskSocket tcp, Packet packet);
    public delegate void RequestHandlerBase(bool success, OpError error);

    // Client
    public delegate void OnConnectedToMaster();
    public delegate void OnDisconnectedFromMaster();
    public delegate void OnClientAcceptedOnMaster();
    public delegate void OnConnectToMasterFailed(OpError opError);
    public delegate void OnPlayerNicknameUpdated(MskPlayer player, string prevNickname);
    public delegate void OnPlayerCustomPropertiesUpdated(MskPlayer player, MskProperties props);
    public delegate void OnPlayerCountFetched(int playerCount);
    public delegate void OnPlayerCountInLobbyFetched(int playerCountInLobby);
    public delegate void OnPlayerListFetched(PlayerInfo[] players);
    public delegate void OnRoomListFetched(RoomInfo[] rooms);
    public delegate void OnMessageReceived(string senderUUID, string message);
    public delegate void OnSendMessageSuccess(string target, string message);
    public delegate void OnSendMessageFailed(string target, string message, OpError opError);

    public delegate void OnSpawnProcessStarted();
    public delegate void OnCreatedRoom(string roomName, string ip, ushort port);
    public delegate void OnCreateRoomFailed(OpError opError);
    public delegate void OnJoinedRoom();
    public delegate void OnJoinRoomFailed(OpError opError);
    public delegate void OnJoinRandomRoomFailed(OpError opError);
    public delegate void OnLeftRoom();

    public delegate void OnRoomRegistered();
    public delegate void OnPlayerJoined(MskPlayer player);
    public delegate void OnPlayerLeft(MskPlayer player);
    public delegate void OnPlayerKicked(MskPlayer player, string reason);
    public delegate void OnMasterChanged(MskPlayer prevMaster, MskPlayer newMaster);
    public delegate void OnRoomPropertiesUpdated(OpRoomProperties opRoom);
    public delegate void OnRoomCustomPropertiesUpdated(MskProperties props);
}