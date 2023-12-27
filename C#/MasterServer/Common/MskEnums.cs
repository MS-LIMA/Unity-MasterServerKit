namespace Msk
{
    public enum OpError
    {
        /// <summary>
        /// No error, success.
        /// </summary>
        Success,

        /// <summary>
        /// Time out
        /// </summary>
        Timeout,
    
        /// <summary>
        /// Max connection reached in the master server.
        /// </summary>
        MaxConnectionReached,

        /// <summary>
        /// Client's uuid is duplicated.
        /// </summary>
        AuthIdDuplicated,

        /// <summary>
        /// Room name must not be null.
        /// </summary>
        RoomNameNull,

        /// <summary>
        /// Is room name duplicated for a new room?
        /// </summary>
        RoomNameDuplicated,

        /// <summary>
        /// Is spawn request duplicated for a same client?
        /// </summary>
        SpawnRequestDuplicated,

        /// <summary>
        /// Is maximum room count reached?
        /// </summary>
        MaxRoomCountReached,

        /// <summary>
        /// The room trying to join is full.
        /// </summary>
        RoomIsFull,

        /// <summary>
        /// Provided password is not correct.
        /// </summary>
        IncorrectPassword,

        /// <summary>
        /// Room is not open.
        /// </summary>
        RoomIsClosed,

        /// <summary>
        /// Internal error.
        /// </summary>
        InternalError,
        
        /// <summary>
        /// Lobby is not found.
        /// </summary>
        LobbyNotFound,

        /// <summary>
        /// Room is not found.
        /// </summary>
        RoomNotFound,

        /// <summary>
        /// Target is not found.
        /// </summary>
        TargetNotFound,
    }

    public enum OpRoomProperties
    {
        ChangeMaxPlayers,
        ChangePrivate,
        ChangeOpen,
        ChangePassword,
        UpdateCustomProperties,
    }

    public enum OpResponse
    {
        // Common
        OnClientAcceptedOnMaster,
        OnConnectedToMaster,
        OnConnectToMasterFailed,

        // Client
        OnSpawnProcessStarted,
        OnCreatedRoom,
        OnCreateRoomFailed,

        OnJoinedRoom,
        OnJoinRoomFailed,
        OnJoinRandomRoomFailed,

        OnLeftRoom,

        OnNicknameUpdated,
        OnPlayerCustomPropertiesUpdated,

        OnPlayerCountFetched,
        OnPlayerCountInLobbyFetched,
        OnPlayerListFetched,
        OnRoomListFetched,

        OnPlayerFound,
        OnFindPlayerFailed,

        OnMessageReceived,
        OnSendMessageSuccess,
        OnSendMessageFailed,

        // Room
        OnRoomPropertiesUpdated,

        OnPlayerJoined,
        OnPlayerLeft,
        OnPlayerKicked,
        OnMasterChanged,

        // Instance
        OnRoomRegistered,
    }

    public enum OpRequest
    {
        // Master Connection 
        ConnectToMaster,

        // Create Room
        CreateRoom,

        // Join Room
        JoinRoom,
        JoinRandomRoom,

        // Leave Room
        LeaveRoom,

        // Room Control
        UpdateRoomProperties,
        SetMaster,
        KickPlayer,

        // Player properties
        SetNickname,
        SetPlayerCustomProperties,

        // Lobby
        SendMessage,
        FetchPlayerCount,
        FetchPlayerCountInLobby,
        FetchPlayerList,
        FetchRoomList,

        // Instnace
        RegisterRoom,
        UnregisterRoom,
    }
}


