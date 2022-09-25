const OpReq = {
    //Lobby Request


    //Room Request
    CreateRoom : 20,
    JoinRoom : 21,
    JoinRandomRoom : 22,
    LeaveRoom : 23,



    //Player Request

    
}

const OpRes = {

    // Lobby response.
    OnRoomInfoUpdated : 10,


    // Room response.
    OnCreatedRoom : 20,
    OnJoinedRoom : 21,
    OnJoinRoomFailed : 22,
    OnJoinRandomRoomFailed : 23,
    OnLeftRoom : 24,

    OnPlayerJoined : 30,
    OnPlayerLeft : 31,
    // ERRORRR
    OnError : 100,
}

const OpRoomInfo = {
    OnCreated : 1,
    OnRemoved : 2,
    OnPlayerCountChanged : 3,
}

const ErrorCause = {
    IncorrectPassword : 1,
    RoomIsFull : 2,
    NoRoomFound : 3,
}

module.exports = OpReq;
module.exports = OpRes;
module.exports = OpRoomInfo;
module.exports = ErrorCause;