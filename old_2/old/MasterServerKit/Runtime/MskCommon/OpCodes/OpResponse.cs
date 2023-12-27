namespace MasterServerKit
{
    public enum OpResponse
    {
        // Common
        OnConnectedToMaster,
        OnConnectedToMasterFailed,

        // Client
        OnConnectedToLobby,
        OnConnectToLobbyFailed,

        OnSpawnProcessStarted,
        OnCreatedRoom,
        OnCreateRoomFailed,

        OnJoinedRoom,
        OnJoinRoomFailed,
        OnJoinRandomRoomFailed,

        OnLeftRoom,

        OnNicknameUpdated,
        OnPlayerCustomPropertiesUpdated,

        OnPlayerCountGet,
        OnPlayerListGet,
        OnRoomListGet,

        // Room
        OnRoomPropertiesUpdated,

        OnPlayerJoined,
        OnPlayerLeft,
        OnMasterChanged,

        // Instance
        OnRoomRegistered,
    }
}