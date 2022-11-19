import { Room, RoomOp, RoomParams } from "./Room";
import { Util } from '../utilities';
import { Player } from "./Player";

const ErrorCode = {
    IsFull : 1,
    IncorrectPassword : 2,

    NotFound : 255
}

export class Lobby {
    constructor(version, config){
        this.version = version;
        this.config = config;

        this.playerCount = 0;
        this.players = {};

        this.rooms = {};
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
        if (id in this.players){
            delete this.players[id];
            this.playerCount--;
        }
    }

    findPlayer = (id) => {
        return this.players[id];
    }


    // Room
    ////////////////////////////////////////////
    createRoom = (creator, roomOptions, customProperties, callback) => {
        const room = new Room(roomOptions, customProperties, this.removeEmptyRoom);
        room.setMaster(creator);

        const jObject = room.getJsonObjectForLobby(
            RoomParams.name, 
            RoomParams.isGameStart,
            RoomParams.isLocked,
            RoomParams.maxPlayerCount,
            RoomParams.playerCount,
            RoomParams.players
        ); 

        this.broadcastToOnlyInLobby(jObject);
        callback(true, room);
    }

    joinRoom = (joiner, roomData, callback) => {
        const room = this.rooms[roomData.id];
        if (!room){
            callback(ErrorCode.NotFound, null);
        }

        const {success, errorCode} = this.#canJoinRoom(roomData);
        if (success){
            this.#joinRoom(joiner, room, callback);
        }
        else{
            callback(errorCode, null);
        }
    }

    joinRandomRoom = (joiner, callback) => {
        let rooms = Object.values(this.rooms).filter(x=>!x.IsFull() && !x.isLocked());
        rooms = rooms.sort(() => 0.5 - Math.random());

        rooms.forEach(room => {
            if (this.#canJoinRoom(room, roomData))
            {
                this.#joinRoom(joiner, room, callback);
                return;
            }
        })

        callback(ErrorCode.NotFound, null);
    }

    #canJoinRoom = (room, roomData) => {
        if (room.isFull()){
            return [false, RoomParams.IsFull];
        }

        if (room.isLocked()){
            if (!roomData){
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

        const jObject = room.getJsonObjectForLobby(RoomParams.playerCount);
        this.broadcastToOnlyInLobby(jObject);
    }

    leaveRoom = (leaver, cause, callback) => {
        const room = this.rooms[leaver.roomId];
        if (room){
            room.removePlayer(leaver, cause);
            if (room.playerCount > 0) {
                this.broadcastToOnlyInLobby(room.getJsonObject(RoomParams.playerCount));
            }
        }

        callback(true);
    }

    removeEmptyRoom = (room) => {
        delete this.rooms[room.id];
        this.broadcastToOnlyInLobby({
            Op : RoomOp.OnRemoved,
            roomId : room.id
        });
    }

    // Broadcast
    /////////////////////////////////////////
    broadcastToOnlyInLobby = (object) => {
        const sockets = this.players.filter(x=>x.roomId).map(x=>x.socket);
        Util.sendJsonObjectToSockets(sockets, object);
    }

    // Socket control
    /////////////////////////////////////////
    onSocketMessage = (socket, json) => {

    }

    onSocketClose = (socket) => {

    }

}