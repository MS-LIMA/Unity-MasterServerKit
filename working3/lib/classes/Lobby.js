import { Room, RoomOp, RoomParams } from "./Room";
import { Util } from '../utilities';

const ErrorCode = {
    IsFull : 1,
    IncorrectPassword : 2,

    NotFound : 255
}

export class Lobby {
    constructor(version, config){
        this.version = version;
        
        this.playerCount = 0;
        this.players = {};

        this.rooms = {};
    }

    // Player
    /////////////////////////////////////////////
    AddPlayer = (player) => {
        this.players[player.id] = player;
        this.playerCount++;
    }

    RemovePlayer = (id) => {
        if (id in this.players){
            delete this.players[id];
            this.playerCount--;
        }
    }


    // Room
    ////////////////////////////////////////////
    CreateRoom = (creator, roomOption, customProperties, callback) => {
        const room = new Room(roomOption, customProperties, this.RemoveEmptyRoom);
        room.SetMaster(creator);

        const jObject = room.GetJsonObject(
            RoomParams.name, 
            RoomParams.isGameStart,
            RoomParams.isLocked,
            RoomParams.maxPlayerCount,
            RoomParams.playerCount,
            RoomParams.players
        ); 

        this.BroadcastToOnlyInLobby(jObject);
        callback(true, room);
    }

    JoinRoom = (joiner, roomData, callback) => {
        const room = this.rooms[roomData.id];
        if (!room){
            callback(ErrorCode.NotFound, null);
        }

        const {success, errorCode} = this.#CanJoinRoom(roomData);
        if (success){
            this.#JoinRoom(joiner, room, callback);
        }
        else{
            callback(errorCode, null);
        }
    }

    JoinRandomRoom = (joiner, callback) => {
        let rooms = Object.values(this.rooms).filter(x=>!x.IsFull() && !x.isLocked());
        rooms = rooms.sort(() => 0.5 - Math.random());

        rooms.forEach(room => {
            if (this.#CanJoinRoom(room, roomData))
            {
                this.#JoinRoom(joiner, room, callback);
                return;
            }
        })

        callback(ErrorCode.NotFound, null);
    }

    #CanJoinRoom = (room, roomData) => {
        if (room.IsFull()){
            return [false, RoomParams.IsFull];
        }

        if (room.IsLocked()){
            if (!roomData){
                return [false, RoomParams.IncorrectPassword];
            }

            return [roomData.password === room.password, 
                roomData.password === room.password ? null : RoomParams.IncorrectPassword
            ];
        }

        return [true, null];
    }

    #JoinRoom = (joiner, room, callback) => {
        room.AddPlayer(joiner);
        callback(null, room);

        const jObject = room.GetJsonObject(RoomParams.playerCount);
        this.BroadcastToOnlyInLobby(jObject);
    }

    LeaveRoom = (leaver, reason, callback) => {
        const room = this.rooms[leaver.roomId];
        if (room){
            room.RemovePlayer(leaver, reason);
            if (room.playerCount > 0) {
                this.BroadcastToOnlyInLobby(room.GetJsonObject(RoomParams.playerCount));
            }
        }

        callback(true);
    }

    RemoveEmptyRoom = (room) => {
        delete this.rooms[room.id];
        this.BroadcastToOnlyInLobby({
            Op : RoomOp.OnRemoved,
            roomId : room.id
        });
    }

    // Broadcast
    /////////////////////////////////////////
    BroadcastToOnlyInLobby = (object) => {
        const sockets = this.players.filter(x=>x.roomId).map(x=>x.socket);
        Util.SendJsonObjectToSockets(sockets, object);
    }

    // Socket control
    /////////////////////////////////////////
    onSocketMessage = (socket, json) => {

    }

    onSocketClose = (socket) => {

    }

}