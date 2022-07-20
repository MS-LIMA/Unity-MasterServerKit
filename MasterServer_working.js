const crypto = require('crypto');

/////////////////////////////////////////////////
class Room {
    constructor(roomName, isOpen, password, maxPlayer, ownerId, playerInfos, customProperties) {
        this.id = crypto.randomUUID();
        this.roomName = roomName;
        this.isOpen = isOpen;
        this.password = password;
        this.passwordEnabled = (password === "") ? false : true;
        this.maxPlayer = maxPlayer;
        this.ownerId = ownerId;
        this.playerInfos = playerInfos;
        this.customProperties = customProperties;
    }

    GetPlayerCount() {
        return Object.keys(this.players).length;
    }

    CreateRoomInfo(){
        return new RoomInfo(this.id, this.roomName, this.isOpen, this.passwordEnabled,
            this.GetPlayerCount(), this.maxPlayer, this.customProperties);
    }
}

class RoomInfo {
    constructor(id, roomName, isOpen, passwordEnabled, playerCount,
        maxPlayer, customPropertiesForLobby) {
        this.id = id;
        this.roomName = roomName;
        this.isOpen = isOpen;
        this.passwordEnabled = passwordEnabled;
        this.playerCount = playerCount;
        this.maxPlayer = maxPlayer;
        this.customPropertiesForLobby = customPropertiesForLobby;
    }
}

class Player {
    constructor(id, nickname, roomId, customProperties, webSocket) {
        this.id = (!id) ? crypto.randomUUID() : id;
        this.nickname = nickname;
        this.roomId = roomId;
        this.customProperties = customProperties;
        this.webSocket = webSocket;
    }

    CreatePlayerInfo() {
        return new PlayerInfo(this.id, this.nickname, this.customProperties);
    }
}

class PlayerInfo {
    constructor(id, nickname, customProperties) {
        this.id = id;
        this.nickname = nickname;
        this.customProperties = customProperties;
    }
}

class RoomOptions {
    constructor(roomName, password, maxPlayer, customProperties) {
        this.roomName = roomName;
        this.password = password;
        this.maxPlayer = maxPlayer;
        this.customProperties = customProperties;
    }
}

const Op = {
    ConnectToMaster = 0,
    OnConnectedToMaster = 1,

    CreateRoom = 2,
    OnCreatedRoom = 3,
    OnCreateRoomFailed = 4,


}

const RoomOp = {
    OnPlayerJoined,
}

const FailureCause = {
    AlreadyInRoom = 4,
    RoomIsFull = 5,
    IncorrectPassword = 6,
}

/////////////////////////////////////////////////
//#region Connect to master server
function ConnectToMaster(webSocket, jObject) {

    let player = CreatePlayer(
        jObject.id,
        jObject.nickname,
        "",
        jObject.customProperties,
        webSocket);
    let playerInfo = player.CreatePlayerInfo();

    SendMessage(webSocket, {
        "op": OnConnectedToMaster,
        "playerInfo": playerInfo
    });
}

function CreatePlayer(id, nickname, roomId, customProperties, webSocket) {

    let player = new Player(id, nickname, roomId, customProperties, webSocket);

    webSocket.id = id;

    players[id] = player;
    playersNotInRoom[id] = player;

    return player;
}
//#endregion

//#region Create Room
function CreateRoom(webSocket, jObject) {

    let owner = players[jObject.creatorId];
    if (!owner) {
        //Error
        return;
    }

    let roomOptions = jObject.roomOptions;
    let room = new Room(
        roomOptions.roomName,
        roomOptions.isOpen,
        roomOptions.password,
        roomOptions.maxPlayer,
        owner.id,
        { [owner.id]: owner.CreatePlayerInfo() },
        roomOptions.customProperties
    );

    rooms[room.id] = room;
    owner.roomId = room.id;
    delete playersNotInRoom[owner.id];

    OnCreatedRoom(webSocket, room);
}

function OnCreatedRoom(webSocket, room) {

    SendMessage(webSocket, {
        "op": OnCreatedRoom,
        "room": room
    })

    let roomInfo = room.CreateRoomInfo();
    Object.values(playersNotInRoom).forEach(x => {
        UpdateRoomInfoForLobby(x.webSocket, roomInfo, RoomOp.OnPlayerJoined);
    });
}

//#endregion

//#region Join Room
function JoinRoom(webSocket, jObject) {

    let joiner = players[jObject.joinerId];
    let room = rooms[jObject.roomId];

    if (!joiner) {
        //Error
        return;
    }

    if (!room) {
        //ERROR
        return;
    }

    let canJoinRoom, failureCause = CanJoinRoom(joiner, room, jObject.password);
    if (canJoinRoom) {

        room.playerInfos[joiner.id] = joiner;
        joiner.roomId = room.id;
        delete playersNotInRoom[joiner.id];

        OnJoinedRoom(webSocket, room);
    }
    else {
        OnJoinRoomFailed(webSocket, failureCause);
    }
}

function OnJoinedRoom(webSocket, room){

    SendMessage(webSocket, {
        "op" : Op.OnJoinedRoom,
        "room" : room
    });

    let players = playersInRoom[room.id];
    players.forEach(x => {
        UpdateRoom(x.webSocket, room, RoomOp.OnPlayerJoined);
    });

    let roomInfo = room.CreateRoomInfo();
    Object.values(playersNotInRoom).forEach(x => {
        UpdateRoomInfoForLobby(x.webSocket, roomInfo, RoomOp.OnPlayerJoined);
    });
}

function OnJoinRoomFailed(webSocket, failureCause){
    switch (failureCause) {
        case FailureCause.IncorrectPassword:
            break;
        case FailureCause.AlreadyInRoom:
            break;
        case FailureCause.RoomIsFull:
            break;
    }
}

function CanJoinRoom(joiner, room, password) {

    if (joiner.roomId) {
        return false, FailureCause.AlreadyInRoom;
    }

    if (room.GetPlayerCount() >= room.maxPlayer) {
        return false, FailureCause.RoomIsFull;
    }

    if (room.passwordEnabled) {
        if (room.password == password) {
            return true, null;
        }
        else {
            return false, FailureCause.IncorrectPassword;
        }
    }

    return true, null;
}

//#endregion

//#region Update rooms
function UpdateRoom(webSocket, room, roomOp) {

}

function UpdateRoomInfoForLobby(webSocket, roomInfo, roomOp) {

}

function GetRoomInfoList() {

}

//#endregion


//#region Disconnect


//#endregion


/////////////////////////////////////////////////
var port = 3333;
var wsServer = require('ws').Server;
var wss = new wsServer({ port: port });

var players = {};
var playersInRoom = {};
var playersNotInRoom = {};
var rooms = {};

console.log('Server opened on port %d.', port);

wss.on('connection', function connection(webSocket) {
    console.log("Client connected");

    ws.on('message', (message) => {
        let jObject = JSON.parse(message);
        switch (jObject.Op = Op.ConnectToMaster) {
            case Op.ConnectToMaster:
                ConnectToMaster(webSocket, jObject);
                break;
        }

    });

    ws.on('close', () => {
        //OnClientDisconnected(ws);
    });
});

function SendMessage(ws, obj) {
    ws.send(JSON.stringify(obj));
}