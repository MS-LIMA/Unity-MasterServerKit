const crypto = require('crypto');

/////////////////////////////////////////////////
class Lobby {
    constructor(version) {
        this.version = version;
        this.players = {};
        this.sockets = {};
        this.rooms = {};
    }

    AddPlayer(id, socket, player) {
        this.players[id] = player;
        this.sockets[id] = socket;
    }

    RemovePlayer(id) {
        if (id in this.players) {
            delete this.players[id];
        }
        if (id in this.sockets) {
            delete this.sockets[id];
        }
    }

    FindPlayer(id) {
        return this.players[id];
    }

    AddRoom(room) {
        this.rooms[room.id] = room;
        this.UpdateRoomInfoForLobby(room, RoomOpResponse.OnCreated);
    }

    FindRoom(roomId) {
        return this.rooms[roomId];
    }

    RemoveRoom(roomId) {
        if (roomId in this.rooms) {
            delete this.rooms[roomId];
        }
    }

    UpdateRoomInfoForLobby(room, roomOpResponse) {
        Object.values(this.sockets).forEach(socket => {
            if (socket.updateRoomList) {
                switch (roomOpResponse) {
                    case RoomOpResponse.OnCreated:
                        let roomInfo = room.CreateRoomInfo();
                        SendObject(socket, {
                            "opResponse": OpResponse.OnRoomInfoUpdated,
                            "roomOpResponse" : RoomOpResponse.OnCreated,
                            "roomInfo": roomInfo
                        });
                        break;
                    case RoomOpResponse.OnPlayerJoined:
                        SendObject(socket, {
                            "opResponse": OpResponse.OnRoomInfoUpdated,
                            "roomOpResponse" : RoomOpResponse.OnPlayerJoined,
                            "playerCount": room.GetPlayerCount()
                        });
                        break;
                    case RoomOpResponse.OnPlayerLeft:
                            SendObject(socket, {
                                "opResponse": OpResponse.OnRoomInfoUpdated,
                                "roomOpResponse" : room.GetPlayerCount() <= 0 ? RoomOpResponse.OnRemoved : RoomOpResponse.OnPlayerLeft,
                                "playerCount": room.GetPlayerCount()
                            });
                        break;
                }
            }
        });
    }
}

class Player {
    constructor(id, nickname, customProperties, socket) {
        this.id = (id) ? id : crypto.randomUUID();
        this.nickname = nickname;
        this.roomId = null;
        this.customProperties = customProperties;
        
        this.socket = socket;
    }
}

class Room {
    constructor(creator, name, roomOptions, lobby) {
        this.id = crypto.randomUUID();
        this.name = (name) ? name : this.id;
        this.isPlaying = false;
        this.isOpen = roomOptions.isOpen;
        this.password = roomOptions.password;
        this.havePassword = (this.password) ? true : false;
        this.maxPlayer = roomOptions.maxPlayer;
        this.ownerId = creator.id;
        this.players = { [creator.id]: creator };
        this.customProperties = roomOptions.customProperties;
        this.customPropertyKeysForLobby = roomOptions.customPropertyKeysForLobby;

        this.playerSockets = {[creator.id] : creator.socket};
        this.lobby = lobby;
    }

    AddPlayer(player) {
        this.players[player.id] = player;
        this.playerSockets[player.id] = socket;
        this.UpdateRoom({"joiner" : player}, RoomOpResponse.OnPlayerJoined);
    }

    RemovePlayer(player) {
        delete this.players[player.id] ;
        delete this.playerSockets[player.id];

        if (this.GetPlayerCount() <= 0){
        }
        else{

            this.UpdateRoom({"leaverId" : player.id}, RoomOpResponse.OnPlayerLeft);
            if (player.id == this.ownerId){
                let nextOwner = this.GetNextPlayer();
                this.ownerId = nextOwner.id;

                this.UpdateRoom({"ownerId" : this.ownerId}, RoomOpResponse.OnOwnerChanged);
            }
        }
    }

    GetNextPlayer(){
        return Object.keys(this.players)[0];
    }

    UpdateRoom(info, roomOpResponse) {
        Object.values(this.playerSockets).forEach(socket =>{
            switch(roomOpResponse){
                case RoomOpResponse.OnPlayerJoined:
                    SendObject(socket, {
                        "roomOpResponse" : RoomOpResponse.OnPlayerJoined,
                        "joiner" : info.joiner
                    });
                case RoomOpResponse.OnPlayerLeft:
                    SendObject(socket, {
                        "roomOpResponse" : RoomOpResponse.OnPlayerLeft,
                        "leaverId" : info.leaverId
                    });
                break;
                case RoomOpResponse.OnOwnerChanged:
                    SendObject(socket, {
                        "roomOpResponse" : RoomOpResponse.OnOwnerChanged,
                        "ownerId" : info.ownerId
                    });
                break;
            }  
        })
    }

    GetPlayerCount() {
        return Object.keys(this.players).length;
    }

    CreateRoomInfo() {

        let customPropertiesForLobby = {};
        this.customPropertyKeysForLobby.forEach(key => {
            if (key in this.customProperties) {
                customPropertiesForLobby[key] = this.customProperties[key];
            }
        });

        return ({
            "id": this.id,
            "name": this.name,
            "isPlaying": this.isPlaying,
            "isOpen": this.isOpen,
            "havePassword": this.havePassword,
            "maxPlayer": this.maxPlayer,
            "ownerId": this.ownerId,
            "playersCount": this.GetPlayerCount(),
            "customPropertiesForLobby": customPropertiesForLobby
        });
    }
}

const OpRequest = {
    ConnectToMaster: 1,

}

const OpResponse = {
    OnConnectedToMaster: 1,

    OnCreatedRoom: 2,
    OnCreatedRoomFailed: 3,

    OnJoinedRoom : 4,
    OnJoinRoomFailed : 5,

    OnLeftRoom : 6,
    OnLeaveRoomFailed : 7,

    OnRoomInfoUpdated: 10
}

const RoomOpResponse = {
    OnCreated: 1,
    OnNameChanged: 2,
    OnPlayingChanged: 3,
    OnOpenChanged: 4,
    OnMaxPlayerChanged: 5,
    OnPlayerJoined: 6,
    OnPlayerLeft: 7,
    OnOwnerChanged: 8,
    OnCustomPropertiesChanged: 9,
    OnRemoved: 10
}

const RoomOpRequest = {
    OnCreated: 1,
    OnNameChanged: 2,
    OnPlayingChanged: 3,
    OnOpenChanged: 4,
    OnMaxPlayerChanged: 5,
    OnPlayerJoined: 6,
    OnPlayerLeft: 7,
    OnOwnerChanged: 8,
    OnCustomPropertiesChanged: 9,
    OnRemoved: 10
}

/////////////////////////////////////////////////
function ConnectToMaster(socket, jObject) {

    let lobby = CreateOrFindLobby(jObject.version);
    let player = CreatePlayer(socket, jObject.version, jObject.id, jObject.nickname, jObject.customProperties);

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

function CreatePlayer(socket, version, id, nickname, customProperties) {

    let player = new Player(id, nickname, customProperties, socket);

    socket.id = id;
    socket.version = version;
    socket.player = player;
    socket.updateRoomList = true;

    return player;
}


/////////////////////////////////////////////////
function CreateRoom(socket, jObject) {
    let lobby = lobbies[jObject.version];
    let player = lobby.FindPlayer(jObject.creatorId);

    let room = new Room(
        player,
        jObject.name,
        jObject.roomOptions,
        lobby
    );

    lobby.AddRoom(room);
    player.roomId = room.id;
    //socket.updateRoomList = false;

    SendObject(socket, {
        "opResponse": OpResponse.OnCreatedRoom,
        "room": room
    });
}

function JoinRoom(socket, jObject) {
    let lobby = lobbies[jObject.version];
    let room = lobby.FindRoom(jObject.roomId);

    if (room) {
        let canJoinRoom, failureCause = CanJoinRoom(room, jObject);
        if (canJoinRoom) {
            let player = lobby.FindPlayer(jObject.joinerId);
            player.roomId = room.id;

            room.AddPlayer(player);
            lobby.UpdateRoomInfoForLobby(room, RoomOpResponse.OnPlayerJoined);

            SendObject(socket, {
                "opResponse" : OpResponse.OnJoinedRoom,
                "room" : room
            });
        }
        else {

        }
    }
    else {

    }
}

function CanJoinRoom(room, jObject) {

}

/////////////////////////////////////////////////
function LeaveRoom(socket, jObject){
    let lobby = lobbies[socket.version];
    let room = lobby.FindRoom(jObject.roomId);
    let player = room.FindPlayer(socket.id);

    if (room){
        room.RemovePlayer(player);
        lobby.UpdateRoomInfoForLobby(room, RoomOpResponse.OnPlayerLeft);

        player.roomId = null;
        SendObject({
            "opResponse" : OpResponse.OnLeftRoom
        });
    }
    else{

    }
}

/////////////////////////////////////////////////
function GetRoomList(socket, jObject){

}

/////////////////////////////////////////////////
function SetRoomCustomProperties(socket, jObject){

}


/////////////////////////////////////////////////
function ChangeRoomSettings(){

}



/////////////////////////////////////////////////
function ChangeNickname(){

}



/////////////////////////////////////////////////
var port = 3333;
var wsServer = require('ws').Server;
var wss = new wsServer({ port: port });

var lobbies = {};

console.log('Server opened on port %d.', port);

wss.on('connection', function connection(socket) {
    console.log("Client connected");

    socket.on('message', (message) => {
        let jObject = JSON.parse(message);

    });

    socket.on('close', () => {
        //OnClientDisconnected(ws);
    });
});


///////////////////////////////////////////////
function SendObject(socket, obj, replacer = null) {
    socket.send(JSON.stringify(obj, JSONReplacer));
}

function JSONReplacer(key,value)
{
    if (key=="playerSockets") return undefined;
    else if (key=="socket") return undefined;
    else if (key=="password") return undefined;
    else if (key=="lobby") return undefined;
    else return value;
}