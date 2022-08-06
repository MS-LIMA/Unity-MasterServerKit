const crypto = require('crypto');

function SendObject(socket, obj, replacer = null) {
    socket.send(JSON.stringify(obj, JSONReplacer));
}

function JSONReplacer(key, value) {
    if (key == "playerSockets") return undefined;
    else if (key == "socket") return undefined;
    else if (key == "password") return undefined;
    else if (key == "lobby") return undefined;
    else return value;
}

/////////////////////////////////////////////////
class Lobby {
    constructor(version, serverIp) {
        this.version = version;
        this.players = {};
        this.playersNotInRoom = {};
        this.sockets = {};
        this.socketsNotInRoom = {};
        this.rooms = {};

        this.serverIp = serverIp;
        this.portMin = 24565;
        this.portMax = 25565;
        this.portsUsed = new Set();
    }

    //#region Player
    AddPlayer(id, player, socket) {
        this.players[id] = player;
        this.playersNotInRoom[id] = player;
        this.sockets[id] = socket;
        this.socketsNotInRoom[id] = socket;
    }

    RemovePlayer(id) {
        delete this.players[id];
        delete this.playersNotInRoom[id];
        delete this.sockets[id];
        delete this.socketsNotInRoom[id];
    }

    FindPlayer(id) {
        return this.players[id];
    }
    //#endregion

    //#region Room
    AddRoom(room) {
        this.rooms[room.id] = room;
        delete this.playersNotInRoom[room.creatorId];
        delete this.socketsNotInRoom[room.creatorId];

        room.SetCallbacks(
            this.OnPlayerJoinedRoom,
            this.OnPlayerLeftRoom,
            this.OnRoomOptionsUpdated
        );

        this.UpdateRoomInfoInLobby(room, OpResponseRoom.OnCreated);
    }

    RemoveRoom(room) {
        delete this.rooms[room.id];

        if (this.portsUsed.has(room.serverInstanceSettings.port)) {
            this.portsUsed.delete(room.serverInstanceSettings.port);
        }

        this.UpdateRoomInfoInLobby(room, OpResponseRoom.OnRemoved);
    }

    FindRoom(roomId) {
        return this.rooms[roomId];
    }

    OnPlayerJoinedRoom(room, player) {
        delete this.playersNotInRoom[player.id];
        delete this.socketsNotInRoom[player.id];
        this.UpdateRoomInfoInLobby(room, OpResponse.OnPlayerJoined);
    }

    OnPlayerLeftRoom(room, player) {
        this.playersNotInRoom[player.id] = player;
        this.socketsNotInRoom[player.id] = player.socket;

        if (room.GetPlayerCount() > 0) {
            this.UpdateRoomInfoInLobby(room, OpResponse.OnPlayerLeft)
        }
        else {
            this.RemoveRoom(room);
        }
    }

    OnRoomOptionsUpdated(room, jObject) {
        this.UpdateRoomInfoInLobby(room, jObject.opResponseRoom, jObject);
    }

    UpdateRoomInfoInLobby(room, opResponseRoom, _jObject) {

        let jObject = {
            "opResponseRoom": opResponseRoom,
            "roomInfo": {
                "id": room.id
            }
        };

        switch (opResponseRoom) {
            case OpResponseRoom.OnCreated:
                let customProperties = {};
                room.customPropertyKeysForLobby.forEach(key => {
                    customProperties[key] = room.customProperties[key];
                });
                jObject.roomInfo["name"] = room.name;
                jObject.roomInfo["isPlaying"] = room.isPlaying;
                jObject.roomInfo["isOpen"] = room.isOpen;
                jObject.roomInfo["hasPassword"] = room.hasPassword;
                jObject.roomInfo["playerCount"] = room.GetPlayerCount();
                jObject.roomInfo["maxPlayer"] = room.maxPlayer;
                jObject.roomInfo["customProperties"] = customProperties;
                break;
            case OpResponseRoom.OnRemoved:
                break;
            case OpResponseRoom.OnPlayerJoined:
            case OpResponseRoom.OnPlayerLeft:
                jObject.roomInfo["playerCount"] = room.GetPlayerCount();
                break;
            case OpResponseRoom.OnRoomInfoUpdated:
                jObject.roomInfo["name"] = room.name;
                break;
            case OpResponseRoom.OnRoomOpenChanged:
                jObject.roomInfo["isOpen"] = room.isOpen;
                break;
            case OpResponseRoom.OnPasswordChanged:
                jObject.roomInfo["hasPassword"] = room.hasPassword;
                break;
            case OpResponseRoom.OnMaxPlayerChanged:
                jObject.roomInfo["maxPlayer"] = room.maxPlayer;
                break;
            case OpResponseRoom.OnCustomPropertiesUpdated:
                let customProperties = {};
                let changedPropertyKeys = Object.keys(_jObject.customProperties);
                room.customPropertyKeysForLobby.forEach(key => {
                    if (key in changedPropertyKeys) {
                        customProperties[key] = room.customProperties[key];
                    }
                });
                break;
        }

        this.SendObjectToPlayersNotInRoom(jObject);
    }

    SendObjectToPlayersNotInRoom(jObject) {
        Object.values(this.socketsNotInRoom).forEach(socket => {
            SendObject(socket, jObject);
        });
    }
    //#endregion

    //#region Lobby
    GetPlayerCount() {

    }

    GetPlayerCountOnlyInLobby() {

    }

    GetRoomList() {

    }

    //#endregion

    //#region Server Setings
    CreateServerInstanceSettings() {
        return new ServerInstanceSettings(
            this.serverIp,
            Math.floor(Math.random() * (this.portMax - this.portMin + 1)) + this.portMin,
            ""
        );
    }
    //#endregion
}

class Room {
    constructor(creator, name, roomOptions, serverInstanceSettings) {
        //Room Info
        this.id = crypto.randomUUID();
        this.name = (name) ? name : this.id;
        this.isPlaying = false;
        this.isOpen = roomOptions.isOpen;
        this.password = roomOptions.password;
        this.hasPassword = (this.password) ? true : false;

        this.maxPlayer = roomOptions.maxPlayer;
        this.ownerId = creator.id;
        this.players = { [creator.id]: creator };
        this.sockets = { [creator.id]: creator.socket };

        this.customProperties = roomOptions.customProperties;
        this.customPropertyKeysForLobby = roomOptions.customPropertyKeysForLobby;

        //Server info
        this.serverInstanceSettings = serverInstanceSettings;
    }

    SetCallbacks(onPlayerJoined, onPlayerLeft, onRoomOptionsUpdated) {
        this.onPlayerJoined = onPlayerJoined;
        this.onPlayerLeft = onPlayerLeft;
        this.onRoomOptionsUpdated = onRoomOptionsUpdated;
    }

    //#region Player
    AddPlayer(player, socket) {
        this.players[player.id] = player;
        this.sockets[player.id] = socket;

        this.onPlayerJoined(this, player);
        this.UpdateRoomInfo({
            "opResponseRoom": OpResponseRoom.OnPlayerJoined,
            "player": player
        });
    }

    RemovePlayer(player, disconnectionCause) {
        delete this.players[player.id];

        this.onPlayerLeft(this, player);
        this.UpdateRoomInfo({
            "opResponseRoom": OpResponseRoom.OnPlayerLeft,
            "playerId": player.id,
            "disconnectionCause": disconnectionCause
        });

        if (this.GetPlayerCount() > 0) {
            if (player.id == this.ownerId) {
                let nextPlayer = Object.values(this.players)[0];
                this.ownerId = nextPlayer.id;

                this.UpdateRoomInfo({
                    "opResponseRoom": OpResponseRoom.OnOwnerChanged,
                    "ownerId": this.ownerId
                });
            }
        }
    }

    GetPlayerCount() {
        return Object.keys(this.players).length;
    }

    //#endregion

    //#region Room
    UpdateRoomInfo(jObject) {
        Object.values(this.sockets).forEach(socket => {
            SendObject(socket, jObject);
        });
    }

    OnRoomOpRequested(opRequestRoom, jObject) {
        let _jObject = {};

        switch (opRequestRoom) {
            case OpRequestRoom.UpdateRoomName:
                this.roomName = jObject.roomName;
                _jObject["opResponseRoom"] = OpResponseRoom.OnRoomNameUpdated;
                _jObject["roomName"] = jObject.roomName;
                break;
            case OpRequestRoom.ChangeOpen:
                this.isOpen = jObject.isOpen;
                _jObject["opResponseRoom"] = OpResponseRoom.OnRoomOpenChanged;
                _jObject["isOpen"] = this.isOpen;
                break;
            case OpRequestRoom.ChangePassword:
                let password = jObject.password;
                this.password = password ? password : null;
                this.hasPassword = password ? true : false;
                _jObject["opResponseRoom"] = OpResponseRoom.OnPasswordChanged;
                _jObject["hasPassword"] = this.hasPassword;
                break;
            case OpRequestRoom.ChangeMaxPlayer:
                this.maxPlayer = jObject.maxPlayer;
                _jObject["opResponseRoom"] = OpResponseRoom.OnMaxPlayerChanged;
                _jObject["maxPlayer"] = this.maxPlayer;
                break;
            case OpRequestRoom.ChangeOwner:
                if (jObject.ownerId in Object.keys(this.players)) {
                    this.ownerId = jObject.ownerId;
                    _jObject["opResponseRoom"] = OpResponseRoom.OnOwnerChanged;
                    _jObject["ownerId"] = this.ownerId;
                }
                break;
            case OpRequestRoom.KickPlayer:
                if (jObject.playerId in Object.keys(this.players)) {
                    this.RemovePlayer(this.players[jObject.playerId], DisconnectionCause.PlayerKicked);
                }
                break;
            case OpRequestRoom.UpdateCustomProperies:
                Object.keys(jObject.customProperties).forEach(key => {
                    this.customProperties[key] = jObject.customProperties[key];
                });
                _jObject["opResponseRoom"] = OpResponseRoom.OnCustomPropertiesUpdated;
                _jObject["customProperties"] = jObject.customProperties;
                break;
        }

        this.onRoomOptionsUpdated(this, _jObject);
        this.UpdateRoomInfo(_jObject);
    }

    //#endregion

    //#region Server Instance
    CreateServerInstance() {

    }

    RemoveServerInstance() {

    }

    //#endregion
}

class ServerInstanceSettings {
    constructor(ip, port, instancePath) {
        this.ip = ip;
        this.port = port;
        this.instancePath = instancePath;
    }
}

class Player {
    constructor(id, version, nickname, customProperties, socket) {
        this.id = (id) ? id : crypto.randomUUID();
        this.version = version;
        this.nickname = nickname;
        this.roomId = null;
        this.customProperties = customProperties;

        this.socket = socket;
    }
}

///////////////////////////////////////////////////
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

    OnJoinedRandomRoom: 6,
    OnJoinedRandomRoomFailed: 7,

    OnLeftRoom: 8,
    OnLeaveRoomFailed: 9,

    OnRoomInfoUpdated: 10,
    OnRoomListUpdated: 11,

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
    OnCreated: 1,
    OnPlayerJoined: 2,
    OnPlayerLeft: 3,
    OnOwnerChanged: 4,

    OnRoomNameUpdated: 5,
    OnRoomOpenChanged: 6,
    OnPasswordChanged: 7,
    OnMaxPlayerChanged: 8,

    OnCustomPropertiesUpdated: 9,

    OnGameStarted: 10,
    OnGameStopped: 11,

    OnRemoved: 12
}

const FailureCause = {
    RoomIsFull: 1,
    IncorrectPassword: 2,
}

const DisconnectionCause = {
    LeaveRoom: 1,
    PlayerKicked: 2,
}

///////////////////////////////////////////////////
var ip = "127.0.0.1";
var port = 3333;
var wsServer = require('ws').Server;
var wss = new wsServer({ port: port });

var lobbies = {};

console.log('Server opened on port %d.', port);

wss.on('connection', function connection(socket) {
    console.log("Client connected");

    socket.on('message', (message) => {
        let jObject = JSON.parse(message);
        if (jObject.hasOwnProperty("opRequest")) {
            OnLobbyMessage(jObject.OpRequest, socket, jObject);
        }
        else if (jObject.hasOwnProperty("opRequestRoom")) {
            OnRoomMessage(jObject.opRequestRoom, socket, jObject);
        }
    });

    socket.on('close', () => {
        //OnClientDisconnected(ws);
    });
});


//////////////////////////////////////////////////
function OnLobbyMessage(opRequest, socket, jObject) {
    switch (opRequest) {
        case OpRequest.ConnectToMaster:
            ConnectToMaster(socket, jObject);
            break;
        case OpRequest.CreateRoom:
            CreateRoom(socket, jObject);
        case OpRequest.JoinRoom:
        case OpRequest.JoinRandomRoom:
            JoinRoom(socket, jObject, opRequest == OpRequest.JoinRandomRoom ? true : false);
            break;
        case OpRequest.LeaveRoom:
            LeaveRoom(socket, jObject);
            break;
    }
}

function OnRoomMessage(opRequestRoom, socket, jObject) {
    let lobby = lobbies[jObject.version];
    let room = lobby.FindRoom(jObject.roomId);

    if (room) {
        room.OnRoomOpRequested(opRequestRoom, jObject);
    }
}

//////////////////////////////////////////////////

//#region Connect To Master
function ConnectToMaster(socket, jObject) {
    let lobby = CreateOrFindLobby(jObject.version);
    let player = CreatePlayer(jObject.id,
        jObject.version,
        jObject.nickname,
        jObject.customProperties,
        socket
    );

    lobby.AddPlayer(player.id, socket, player);
    SendObject(socket, {
        "opResponse": OpResponse.OnConnectedToMaster,
        "player": player
    });
}

function CreateOrFindLobby(version) {
    if (version in lobbies) {
        return lobbies[version];
    }

    return new Lobby(version);
}

function CreatePlayer(id, version, nickname, customProperties, socket) {
    let player = new Player(id, version, nickname, customProperties, socket);
    socket.id = id;
    socket.version = version;

    return player;
}
//#endregion

//#region Create, Join, Leave Room
function CreateRoom(socket, jObject) {
    let lobby = lobbies[jObject.version];
    let player = lobby.FindPlayer(jObject.creatorId);

    let room = new Room(
        player,
        jObject.roomName,
        jObject.roomOptions,
        lobby.CreateServerInstanceSettings()
    );

    lobby.AddRoom(room);
    player.roomId = room.id;

    SendObject(socket, {
        "opResponse": OpResponse.OnCreatedRoom,
        "room": room
    });
}

function JoinRoom(socket, jObject, isRandom) {
    let lobby = lobbies[jObject.version];

    if (isRandom) {
        let roomFound = false;

        //let room = lobby.FindRoom(jObject.roomId);

    }
    else {
        let room = lobby.FindRoom(jObject.roomId);
        let canJoinRoom, failureCause = CanJoinRoom(room, jObject);

        if (canJoinRoom) {
            let player = lobby.FindPlayer(jObject.joinerId);
            player.roomId = room.id;

            room.AddPlayer(player, socket);

            SendObject(socket, {
                "opResponse": OpResponse.OnJoinedRoom,
                "room": room
            });
        }
        else {

        }
    }
}

function CanJoinRoom(room, jObject) {
    if (!room.isOpen) {
        return false;
    }
    if (room.GetPlayerCount() >= maxPlayer) {
        return false, FailureCause.RoomIsFull;
    }
    if (room.hasPassword) {
        if (room.password == jObject.password) {
            return true, null;
        }
        else {
            return false, FailureCause.IncorrectPassword;
        }
    }
}

function LeaveRoom(socket, jObject) {
    let lobby = lobbies[socket.version];
    let room = lobby.FindRoom(jObject.roomId);
    let player = room.FindPlayer(jObject.leaverId);

    if (room) {
        room.RemovePlayer(player, DisconnectionCause.LeaveRoom);
        player.roomId = null;

        SendObject({
            "opResponse": OpResponse.OnLeftRoom
        });
    }
    else {
    }
}
//#endregion