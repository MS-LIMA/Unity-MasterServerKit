///////////////////////////////////////////////////////////////////////////
const lobby = require("./Lobby");
const op = require("./Op");

///////////////////////////////////////////////////////////////////////////
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
        OnSocketDisconnected(socket);
    });
});

///////////////////////////////////////////////////////////////////////////
function OnLobbyMessage(opRequest, socket, jObject) {
    switch (opRequest) {
        case OpRequest.ConnectToMaster:
            OnConnectToMasterRequested(socket, jObject);
            break;
        case OpRequest.CreateRoom:
            OnCreateRoomRequested(socket, jObject);
            break;
        case OpRequest.JoinRoom:
            OnJoinRoomRequested(socket, jObject);
            break;
        case OpRequest.JoinRandomRoom:
            OnJoinRandomRoomRequested(socket, jObject);
            break;
        case OpRequest.LeaveRoom:
            OnLeaveRoomRequested(socket, jObject);
            break;
        case OpRequest.Disconnect:
            OnDisconnectRequested(socket, jObject);
            break;
    }
}

function OnRoomMessage(opRequestRoom, socket, jObject){
    let lobby = FindLobby(socket.version);
    if (lobby){
        lobby.OnRoomOpRequested(opRequestRoom, jObject.requestedInfo);
    }
}

// Connect to master server
function OnConnectToMasterRequested(socket, jObject) {
    let lobby = CreateOrFindLobby(jObject.version);
    let player = CreatePlayer(
        jObject.id,
        jObject.version,
        jObject.nickname,
        jObject.customProperties,
        socket
    );

    lobby.AddPlayer(id, player);
    SendJsonObjectToSocket(socket, {
        "opResponse": OpResponse.OnConnectedToMaster,
        "player": player
    });
}

function FindLobby(version) {
    return lobbies[version];
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

// Create room
function OnCreateRoomRequested(socket, jObject) {
    let lobby = FindLobby(socket.version);
    if (lobby) {
        let room = lobby.CreateRoom(jObject.creatorId, jObject.roomName, jObject.roomOptions);
        SendJsonObjectToSocket(socket, {
            "opResponse": OpResponse.OnCreatedRoom,
            "room": room
        });
    }
}

// Join room
function OnJoinRoomRequested(socket, jObject) {
    let lobby = FindLobby(socket.version);
    if (lobby) {
        roomJoined, failureCause = lobby.JoinRoom(jObject.joinerId, jObject.roomId, joinerId.password);
        if (roomJoined) {
            SendJsonObjectToSocket(socket, {
                "opResponse": OpResponse.OnJoinedRoom,
                "room": room
            });
        }
        else {
            SendJsonObjectToSocket(socket, {
                "opResponse": OpResponse.OnJoinRoomFailed,
                "failureCause": failureCause
            });
        }
    }
}

// Join random room
function OnJoinRandomRoomRequested(socket, jObject) {
    let lobby = FindLobby(socket.version);
    if (lobby) {
        roomJoined, failureCause = lobby.JoinRandomRoom(jObject.joinerId);
        if (roomJoined) {
            SendJsonObjectToSocket(socket, {
                "opResponse": OpResponse.OnJoinedRoom,
                "room": room
            });
        }
        else {
            SendJsonObjectToSocket(socket, {
                "opResponse": OpResponse.OnJoinRandomRoomFailed, 
                "failureCause": failureCause
            });
        }
    }
}

// Leave room
function OnLeaveRoomRequested(socket, jObject) {
    let lobby = FindLobby(socket.version);
    if (lobby) {
        lobby.LeaveRoom(jObject.leaverId, jObject.roomId, DisconnectionCause.LeftRoom);
        SendJsonObjectToSocket(socket, {
            "opResponse": OpResponse.OnLeftRoom,
        });
    }
}

// 
// Disconnect from master server.
function OnDisconnectRequested(socket, jObject) {
    let lobby = FindLobby(socket.version);
    if (lobby){
        lobby.DisconnectPlayer(jObject.playerId);
        SendJsonObjectToSocket(socket, {
            "opResponse" : OpResponse.OnDisconnectedToMaster
        });

        socket.close();
    }
}

function OnSocketDisconnected(socket){
    let lobby = FindLobby(socket.version);
    if (lobby){
        lobby.DisconnectPlayer(socket.id);
    }
}
