import { Room } from "./Room";
import { Util } from '../utilities';
import { Player } from "./Player";
import { OpResponse, RoomOp, RoomParams } from "./Op";

const ErrorCode = {
    IsFull: 1,
    IncorrectPassword: 2,

    NotFound: 255
}

export class Lobby {
    constructor(version, config, requestPort, returnPort) {
        this.version = version;
        this.config = config;

        this.playerCount = 0;
        this.players = {};

        this.rooms = {};

        this.requestPort = requestPort;
        this.returnPort = returnPort;
    }
    

    // Player
    /////////////////////////////////////////////
    createPlayer = (socket, data) => {
        return new Player(data.id, data.nickname, socket);
    }

    addPlayer = (player, callback) => {
        this.players[player.id] = player;
        this.playerCount++;

        callback(true);
    }

    removePlayer = (id) => {
        if (id in this.players) {
            delete this.players[id];
            this.playerCount--;
        }
    }

    findPlayer = (id) => {
        return this.players[id];
    }


    // Player method
    ////////////////////////////////////////////
    setNickname = (player, nickname, callback) => {
        player.nickname = nickname;
        if (player.roomId){
            const room = this.rooms[player.roomId];
            if (room){
                room.setPlayerCustomProperties(player, {
                    nickname : nickname
                });
            }
        }

        callback();
    }

    setPlayerCustomProperties = (player, customProperties) => {
        player.customProperties = customProperties;
        if (player.roomId){
            const room = this.rooms[player.roomId];
            if (room){
                room.setPlayerCustomProperties(player, customProperties);
            }
        }
    }


    // Room
    ////////////////////////////////////////////
    createRoom = (creator, roomOptions, customProperties, callback) => {
        const room = new Room(
            roomOptions,
            customProperties,
            {
                ip: this.config.ip,
                port: this.requestPort(),
                path: this.config.serverInstance.path,
                version: this.version,
                buildName: this.config.serverInstance.buildName
            },
            this.removeEmptyRoom
        );

        room.setMaster(creator);
        creator.roomId = room.id;

        const data = room.getRoomInfoForLobby(
            RoomParams.name,
            RoomParams.isGameStart,
            RoomParams.isLocked,
            RoomParams.maxPlayerCount,
            RoomParams.playerCount,
            RoomParams.players
        );
        const jObject = {
            opResponse: OpResponse.onRoomInfoChanged,
            roomOp: RoomOp.onCreated,
            roomInfo: data
        }

        this.broadcastToOnlyInLobby(jObject);
        callback(true, room);
    }

    joinRoom = (joiner, roomData, callback) => {
        const room = this.rooms[roomData.id];
        if (!room) {
            callback(ErrorCode.NotFound, null);
        }

        const { success, errorCode } = this.#canJoinRoom(roomData);
        if (success) {
            this.#joinRoom(joiner, room, callback);
        }
        else {
            callback(errorCode, null);
        }
    }

    joinRandomRoom = (joiner, callback) => {
        let rooms = Object.values(this.rooms).filter(x => !x.IsFull() && !x.isLocked());
        rooms = rooms.sort(() => 0.5 - Math.random());

        rooms.forEach(room => {
            if (this.#canJoinRoom(room, roomData)) {
                this.#joinRoom(joiner, room, callback);
                return;
            }
        })

        callback(ErrorCode.NotFound, null);
    }

    #canJoinRoom = (room, roomData) => {
        if (room.isFull()) {
            return [false, RoomParams.IsFull];
        }

        if (room.isLocked()) {
            if (!roomData) {
                return [false, RoomParams.IncorrectPassword];
            }

            return [roomData.password === room.password,
            roomData.password === room.password ? null : RoomParams.IncorrectPassword
            ];
        }

        return [true, null];
    }

    #joinRoom = (joiner, room, callback) => {
        room.addPlayer(joiner);
        callback(null, room);

        const data = room.getRoomInfoForLobby(RoomParams.playerCount);
        const jObject = {
            opResponse: OpResponse.onRoomInfoChanged,
            roomOp: RoomOp.onPlayerCountChanged,
            data: data
        };

        joiner.roomId = room.id;
        this.broadcastToOnlyInLobby(jObject);
    }

    leaveRoom = (leaver, cause, callback) => {
        const room = this.rooms[leaver.roomId];
        if (room) {
            room.removePlayer(leaver, cause);
            if (room.playerCount > 0) {
                const data = room.getRoomInfoForLobby(RoomParams.playerCount);
                const jObject = {
                    opResponse: OpResponse.onRoomInfoChanged,
                    roomOp: RoomOp.onPlayerCountChanged,
                    data: data
                };
            }
        }

        leaver.roomId = '';
        callback(true);
    }

    removeEmptyRoom = (room) => {
        delete this.rooms[room.id];
        this.broadcastToOnlyInLobby({
            opResponse: OpResponse.onRoomInfoChanged,
            roomOp: RoomOp.onRemoved,
            data: {
                roomId: room.id
            }
        });

        this.returnPort(room.port);
    }

    listRoomInfos = () => {
        let arr = [];
        this.rooms.forEach(x => {
            arr.push(x.getRoomInfoForLobby(
                RoomParams.name,
                RoomParams.isGameStart,
                RoomParams.isLocked,
                RoomParams.maxPlayerCount,
                RoomParams.playerCount,
                RoomParams.players
            ));
        })

        return arr;
    }

    // Room internal
    //////////////////////////////////////////
    kickPlayer = (roomId, requesterId, playerId) => {
        const room = this.rooms[roomId];
        if (!room) {
            return;
        }

        const player = this.players[playerId];

        if (player) {
            player.roomId = '';
            room.kickPlayer(requesterId, playerId);
        }
    }

    setMaster = (roomId, requesterId, targetId) => {
        const room = this.rooms[roomId];
        if (!room) {
            return;
        }

        room.setMaster(requesterId, targetId);
    }

    setRoomCustomProperties = (roomId, customProperties) => {
        const room = this.rooms[roomId];
        if (!room) {
            return;
        }

        room.setCustomProperties(customProperties, () => {
            const data = room.getRoomInfoForLobby(RoomParams.customProperties);
            if (data) {
                const jObject = {
                    opResponse: OpResponse.onRoomInfoChanged,
                    roomOp: RoomOp.onCustomPropertiesUpdated,
                    data: data
                };
                this.broadcastToOnlyInLobby(jObject);
            }
        });
    }

    setPassword = (roomId, password) => {
        const room = this.rooms[roomId];
        if (!room) {
            return;
        }

        room.setPassword(password, () => {
            this.broadcastToOnlyInLobby({
                opResponse: OpResponse.onRoomInfoChanged,
                roomOp: RoomOp.onPasswordChanged,
                data: room.getRoomInfoForLobby(RoomParams.isLocked)
            });
        });
    }

    setGameStart = (roomId, bool) => {
        const room = this.rooms[roomId];
        if (!room){
            return;
        }

        room.setGameStart(bool);
        this.broadcastToOnlyInLobby({
            opResponse : OpResponse.onRoomInfoChanged,
            roomOp : OpResponse.onGameStart,
            data : {
                bool : bool
            }
        });
    }

    startServerInstance = (roomId) => {
        const room = this.rooms[roomId];
        if (!room){
            return;
        }

        room.startServerInstance();
    }

    stopServerInstance = (roomId) => {
        const room = this.rooms[roomId];
        if (!room){
            return;
        }

        room.stopServerInstance();
    }


    // Broadcast
    /////////////////////////////////////////
    broadcastToOnlyInLobby = (object) => {
        const sockets = this.players.filter(x => x.roomId).map(x => x.socket);
        Util.sendJsonObjectToSockets(sockets, object);
    }

}