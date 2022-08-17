const OpRequest = {
    ConnectToMaster: 1,

    CreateRoom: 2,
    JoinRoom: 3,
    JoinRandomRoom: 4,
    LeaveRoom: 5,

    GetRoomInfos: 6,
    GetPlayersCount : 7,

    ChangeNickname: 8,
    UpdateCustomProperies: 9,

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
    OnRoomInfosGet : 11,
    OnPlayersCountGet : 12,

    OnDisconnectedToMaster: 15
}

const OpRequestRoom = {
    SetRoomName: 1,
    SetPassword: 2,
    SetMaxPlayers: 3,
    SetMaster: 4,
    SetCustomProperties: 5,

    KickPlayer : 6,
    SendChatMessage : 7,

    StartSession: 10,
    StopSession: 11

}

const OpResponseRoom = {
    OnCreated : 1,
    OnRemoved : 2,

    OnPlayerJoined : 3,
    OnPlayerLeft : 4,

    OnRoomNameSet : 5,
    OnPasswordSet : 6,
    OnMasterChanged : 7,
    OnMaxPlayersSet : 8,
    OnCustomPropertiesSet : 9,

    OnChatMessage : 10,

}

const FailureCause = {
    RoomIsFull: 1,
    IncorrectPassword: 2,
    RoomIdNotFound: 3,
    NoRoomToJoin: 4,
}

const DisconnectionCause = {
    LeftRoom: 1,
    KickedFromRoom: 2,
    Error: 3,
    LeftMasterServer: 4,
}