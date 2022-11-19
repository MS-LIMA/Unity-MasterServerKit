import { DisconnectionCause, Lobby, OpRequest, OpResponse, RoomParams } from "./classes";
import { Util } from './utilities';

var lobbies = {};
var config = {};

////////////////////////////////////////////////
const init = (_config) => {
    config = _config;
}

////////////////////////////////////////////////
const onSocketMessage = (socket, message) => {
    let json = JSON.parse(message);
    switch (json.opRequest) {
        case OpRequest.connectToMaster:
            connectToMaster(socket, json);
            break;
        case OpRequest.createRoom:
            createRoom(socket, json);
            break;
        case OpRequest.joinRoom:
            joinRoom(socket, json);
            break;
        case OpRequest.joinRandomRoom:
            joinRandomRoom(socket, json);
            break;
        case OpRequest.leaveRoom:
            leaveRoom(socket);
            break;
    }
}

const onSocketClose = (socket) => {
    removeplayer(socket);
}

// Lobby
////////////////////////////////////////////////
const connectToMaster = (socket, message) => {
    const lobby = createOrFindLobby(socket.version);
    const player = lobby.createPlayer(socket, message.data);

    lobby.addPlayer(player, (success) => {
        if (success) {
            socket.id = player.id,
                socket.version = message.version,
                Util.sendJsonObjectToSocket(socket, {
                    opResponse: OpResponse.onConnectedToMaster,
                    player: player
                });
        }
        else {
            Util.sendJsonObjectToSocket(socket, {
                opResponse: OpResponse.onConnectToMasterFailed
            })
        }
    })
}

const createRoom = (socket, message) => {
    const lobby = createOrFindLobby(socket.version);
    const player = lobby.findPlayer(socket.id);

    if (!player) {
        Util.sendJsonObjectToSocket(socket, {
            opResponse: onCreateRoomFailed,
            ErrorCode: ''
        });
        return;
    }

    lobby.createRoom(player,
        message.data.roomOptions,
        message.data.customProperties,
        (success, room) => {
            if (success) {
                const jObject = room.getJsonObjectForRoom();
                Util.sendJsonObjectToSocket(socket, {
                    opResponse: OpResponse.onCreatedRoom,
                    room: jObject
                });
            }
            else {
                Util.sendJsonObjectToSocket(socket, {
                    opResponse: OpResponse.onCreateRoomFailed,
                    ErrorCode: ''
                });
            }
        });
}

const joinRoom = (socket, message) => {
    const lobby = createOrFindLobby(socket.version);
    const player = lobby.findPlayer(socket.id);

    if (!player) {
        Util.sendJsonObjectToSocket(socket, {
            opResponse: OpResponse.onJoinRoomFailed,
            ErrorCode: ''
        });
        return;
    }

    lobby.joinRoom(player, message.data, (err, room) => {
        if (err) {
            Util.sendJsonObjectToSocket(socket, {
                opResponse: OpResponse.onJoinRoomFailed,
                errorCode: err
            });
            return;
        }

        Util.sendJsonObjectToSocket(socket, {
            opResponse: OpResponse.onJoinedRoom,
            room: room.getJsonObjectForRoom()
        });
    });
}

const joinRandomRoom = (socket) => {
    const lobby = createOrFindLobby(socket.version);
    const player = lobby.findPlayer(socket.id);

    if (!player) {
        Util.sendJsonObjectToSocket(socket, {
            opResponse: OpResponse.onJoinRandomRoomFailed,
            errorCode: ''
        });
        return;
    }

    lobby.joinRandomRoom(player, (err, room) => {
        if (err) {
            Util.sendJsonObjectToSocket(socket, {
                opResponse: OpResponse.onJoinRandomRoomFailed,
                errorCode: err
            });
            return;
        }

        Util.sendJsonObjectToSocket(socket, {
            opResponse: OpResponse.onJoinedRandomRoom,
            room: room.getJsonObjectForRoom()
        });
    })
}

const leaveRoom = (socket) => {
    const lobby = createOrFindLobby(socket.version);
    const player = lobby.findPlayer(socket.id);

    if (!player) {
        Util.sendJsonObjectToSocket(socket, {
            opResponse: OpResponse.onLeftRoom,
        });
        return;
    }

    lobby.leaveRoom(player, DisconnectionCause.leave, (success) => {
        Util.sendJsonObjectToSocket(socket, {
            opResponse: OpResponse.onLeftRoom
        });
    })
}

const removeplayer = (socket) => {
    const lobby = createOrFindLobby(socket.version);
    const player = lobby.findPlayer(socket.id);

    lobby.leaveRoom(player, DisconnectionCause.disconnectedFromMaster, (success) => {
    });

    lobby.removeplayer(socket);
}

const createOrFindLobby = (version) => {
    if (version in lobbies) {
        return lobbies[version];
    }

    return new Lobby(version, config);
}

// Room
////////////////////////////////////////////////


////////////////////////////////////////////////
module.exports = {
    init,
    onSocketMessage,
    onSocketClose
}