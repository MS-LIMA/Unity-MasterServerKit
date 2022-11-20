import { DisconnectionCause, Lobby, OpRequest, OpResponse } from "./classes";
import { Util } from './utilities';

var lobbies = {};
var config = {};

this.ports = [];

////////////////////////////////////////////////
const init = (_config) => {
    config = _config;
    for (let i = 0; i < config.serverInstance.maxInstanceCount; i++) {
        this.ports.push(i + config.serverInstance.portStart);
    }
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
        case OpRequest.setMaster:
            setMaster(socket, json);
            break;
        case OpRequest.kickPlayer:
            kickPlayer(socket, json);
            break;
        case OpRequest.setRoomCustomProperties:
            setRoomCustomProperties(socket, json);
            break;
        case OpRequest.setNickname:
            setNickname(socket, json);
            break;
        case OpRequest.setPlayerCustomProperties:
            setPlayerCustomProperties(socket, json);
            break;
        case OpRequest.setGameStart:
            setGameStart(socket, json);
            break;
        case OpRequest.startInstance:
            startInstance(socket, json);
            break;
        case OpRequest.stopInstance:
            stopInstance(socket, json);
            break;
        case OpRequest.listRoomInfos:
            listRoomInfos(socket);
            break;
        case OpRequest.getPlayerCount:
            getPlayerCount(socket);
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
    const { player, lobby } = findPlayerInLobby(socket, OpResponse.onCreateRoomFailed);
    if (player) {
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
}

const joinRoom = (socket, message) => {
    const { player, lobby } = findPlayerInLobby(socket, OpResponse.onJoinRoomFailed);

    if (player) {
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
}

const joinRandomRoom = (socket) => {
    const { player, lobby } = findPlayerInLobby(socket, OpResponse.onJoinRandomRoomFailed);
    if (player) {
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
        });
    }
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

const listRoomInfos = (socket) => {
    const lobby = createOrFindLobby(socket.version);
    if (lobby){
        const arr = lobby.listRoomInfos();
        Util.sendJsonObjectToSocket(socket, {
            opResponse : OpResponse.onRoomInfoslisted,
            data : {
                roomInfos : arr
            }
        });
    }
}

const getPlayerCount = (socket) => {
    const lobby = createOrFindLobby(socket.version);
    if (lobby){
        const cnt = lobby.playerCount;
        Util.sendJsonObjectToSocket(socket, {
            opResponse : OpResponse.onPlayerCountGathered,
            data : {
                playerCount : cnt
            }
        });
    }   
}

// 로비 채팅 만들기
// 룸 채팅 만들기

// Lobby utils
const createOrFindLobby = (version) => {
    if (version in lobbies) {
        return lobbies[version];
    }

    return new Lobby(version, config, requestPort, returnPort);
}

const findPlayerInLobby = (socket, opResponseFailure, errorCode = '') => {
    const lobby = createOrFindLobby(socket.version);
    const player = lobby.findPlayer(socket.id);

    if (!player) {
        Util.sendJsonObjectToSocket(socket, {
            opResponse: opResponseFailure,
            errorCode: errorCode
        });
        return [null, null];
    }

    return [player, lobby];
}

const requestPort = () => {
    if (this.ports.length <= 0) {
        console.log("MAX SERVER INSTANCE COUNT LIMIT");
        return;
    }

    const port = this.ports.pop();
    return port;
}

const returnPort = (port) => {
    this.ports.push(port);
}


// Room
////////////////////////////////////////////////
const startInstance = (socket, message) => {
    const lobby = this.lobbies[socket.version];
    lobby.startServerInstance(message.data.roomId);
}

const stopInstance = (socket, message) => {
    const lobby = this.lobbies[socket.version];
    lobby.stopServerInstance(message.data.roomId);
}

const setMaster = (socket, message) => {
    const lobby = createOrFindLobby(socket.version);
    const player = lobby.findPlayer(socket.id);

    if (!player) {
        return;
    }

    lobby.setMaster(message.data.roomId, message.data.requesterId, message.data.targetId);
}

const kickPlayer = (socket, message) => {
    const lobby = createOrFindLobby(socket.version);
    const player = lobby.findPlayer(socket.id);

    if (!player) {
        return;
    }

    lobby.kickPlayer(message.data.roomId, message.data.requesterId, message.data.playerId);
}

const setRoomCustomProperties = (socket, message) => {
    const lobby = createOrFindLobby(socket.version);
    const player = lobby.findPlayer(socket.id);

    if (!player) {
        return;
    }

    lobby.setRoomCustomProperties(message.data);
}

const setGameStart = (socket, message) => {
    const lobby = this.lobbies[socket.version];
    lobby.setGameStart(message.data.roomId, message.data.bool);
}


// Player
////////////////////////////////////////////////
const setNickname = (socket, message) => {
    const lobby = this.lobbies[socket.version];
    const player = lobby.findPlayer(socket.id);

    if (!player) {
        return;
    }

    lobby.setNickname(player, message.data.nickname, () => {
        Util.sendJsonObjectToSocket(socket, {
            opResponse: OpResponse.onNicknameChanged,
            data: {
                nickname: message.data.nickname
            }
        });
    })
}

const setPlayerCustomProperties = (socket, message) => {
    const lobby = this.lobbies[socket.version];
    const player = lobby.findPlayer(socket.id);

    if (!player) {
        return;
    }

    lobby.setPlayerCustomProperties(player, message.data.customProperties);
}

////////////////////////////////////////////////
module.exports = {
    init,
    onSocketMessage,
    onSocketClose
}