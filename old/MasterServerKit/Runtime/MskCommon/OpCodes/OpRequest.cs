namespace MasterServerKit
{
    public enum OpRequest
    {
        // Client
        ConnectToLobby,

        CreateRoom,
        JoinRoom,
        JoinRandomRoom,
        LeaveRoom,

        SetNickname,
        SetPlayerCustomProperties,

        GetPlayerCount,
        GetPlayerList,
        GetRoomList,

        // Room
        UpdateRoomProperties,
        SetMaster,

        // Instnace
        RegisterRoom,
        UnregisterRoom,
    }
}