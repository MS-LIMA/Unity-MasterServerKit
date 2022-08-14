const OpRequest = {
    ConnectToMaster: 1,

    CreateRoom: 2,
    JoinRoom: 3,
    JoinRandomRoom: 4,
    LeaveRoom: 5,
    UpdateRoomList: 6,

    ChangeNickname: 7,
    UpdateCustomProperies: 8,

    Disconnect: 235
}

const OpResponse = {
    OnConnectedToMaster: 1,

    OnCreatedRoom: 2,
    OnCreatdRoomFailed: 3,

    OnJoinedRoom: 4,
    OnJoinedRoomFailed: 5,

    OnJoinRandomRoom: 6,
    OnJoinRandomRoomFailed: 7,

    OnLeftRoom: 8,
    OnLeaveRoomFailed: 9,

    OnRoomInfoUpdated: 10,
    OnRoomListUpdated: 11,


    OnDisconnectedToMaster : 15
}

const OpRequestRoom = {
    UpdateRoomName: 1,
    ChangeOpen: 2,
    ChangePassword: 3,
    ChangeMaxPlayer: 4,

    KickPlayer: 5,
    ChangeOwner: 6,

    UpdateCustomProperies: 7,

    StartGame: 8,
    StopGame: 9
}

const OpResponseRoom = {
    OnCreated,
    OnRemoved,
    OnPlayerJoined,
    OnPlayerLeft,
    OnOwnerChanged,
    OnRoomInfoUpdated,
    
}

const FailureCause = {
    RoomIsFull: 1,
    IncorrectPassword: 2,
    RoomIdNotFound : 3,
    NoRoomToJoin : 4,
}

const DisconnectionCause = {
    LeftRoom: 1,
    PlayerKicked: 2,
    Error : 3,
    LeftMasterServer : 4,
}