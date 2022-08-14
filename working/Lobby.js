const Room = require("./Room");
const Player = require("./Player");
const Op = require("./Op");
const Utility = require("./Utility");

class Lobby {
    constructor(version, serverIp = "127.0.0.1", portMin = 24565, portMax = 25565) {
        this.version = version;

        this.rooms = {};

        this.players = {};
        this.playerCount = 0;

        this.serverIp = serverIp;
        this.portMin = portMin;
        this.portMax = portMax;
    }

    // Player control
    AddPlayer(player) {
        this.players[player.id] = player;
        this.playerCount++;
    }

    RemovePlayer(player) {
        this.players[player.id] = player;
        this.playerCount--;

        if (this.playerCount <= 0) {
            //remove lobby
        }
    }

    // Room control.................................................
    //#region Create room
    CreateRoom(creatorId, roomName, roomOptions) {
        let creator = this.players[creatorId];
        let room = new Room(
            creator,
            roomName,
            roomOptions
        );

        this.rooms[room.id] = room;
        this.UpdateRoomInfo(OpResponseRoom.OnCreated, room);

        return room;
    }

    OnRoomPlayerCountChanged(room) {

    }

    OnRoomRemoved(room) {

    }

    OnRoomOptionsChanged(room, updatedInfo) {

    }
    //#endregion

    //#region Join room
    JoinRoom(joinerId, roomId, password) {
        let room = this.rooms[roomId];
        if (room) {
            let canJoinRoom, failureCause = this.CanJoinRoom(room);
            if (canJoinRoom) {
                let joiner = this.players[joinerId];
                room.AddPlayer(joiner);

                this.UpdateRoomInfo(OpResponseRoom.OnPlayerJoined, room);
            }

            return canJoinRoom, failureCause;
        }
        else {
            return false, FailureCause.RoomIdNotFound;
        }
    }

    JoinRandomRoom(joinerId) {
        let indices = [];
        let roomList = Object.keys(this.rooms);
        let length = roomList.length;
        for (let i = 0; i < length; i++) {
            indices.push(i);
        }

        while (indices.length > 0) {
            let index = Math.floor(Math.random() * indices.length);
            let room = roomList[indices[index]];

            let canJoinRoom, failureCause = this.CanJoinRoom(room);
            if (canJoinRoom) {
                let joiner = this.players[joinerId];
                room.AddPlayer(joiner);

                this.UpdateRoomInfo(OpResponseRoom.OnPlayerJoined, room);

                return true, null;
            }

            indices.splice(index, 1);
        }

        return false, FailureCause.NoRoomToJoin;
    }

    CanJoinRoom(room, password) {
        if (room.playerCount >= room.maxPlayer) {
            return false, FailureCause.RoomIsFull;
        }

        if (room.IsLocked()) {
            if (room.password == password) {
                return true, null;
            }

            return false, FailureCause.IncorrectPassword;
        }

        return true, null;
    }
    //#endregion

    //#region Leave room
    LeaveRoom(leaverId, roomId, disconnectionCause) {
        let room = this.rooms[roomId];
        if (room) {
            let player = this.players[leaverId];
            if (player) {
                room.RemovePlayer(player, disconnectionCause);

                if (room.playerCount > 0) {
                    this.UpdateRoomInfo(OpResponseRoom.OnPlayerLeft, room);
                }
                else {
                    delete this.room[roomId];
                    this.UpdateRoomInfo(OpResponseRoom.OnRemoved, room);
                }
            }
        }
    }

    DisconnectPlayer(playerId, disconnectionCause) {
        let player = this.players[playerId];
        if (player) {
            if (player.IsInRoom()) {
                this.LeaveRoom(playerId, player.roomId, disconnectionCause)
            }

            this.RemovePlayer(player);
        }
    }
    //#endregion

    OnRoomOpRequested(opRequestRoom, requestedInfo){

    }
    
    // Lobby control.................................................
    UpdateRoomInfo(opResponseRoom, room) {
        let jObject = {
            "opResposne": OpResponse.OnRoomInfoUpdated,
            "opResponseRoom": opResponseRoom,
            "roomInfo": {
                "roomId": room.id
            }
        };

        switch (opResponseRoom) {
            case OpResponseRoom.OnCreated:
                jObject.roomInfo["roomName"] = room.name;
                jObject.roomInfo["playerCount"] = room.playerCount;
                jObject.roomInfo["maxPlayer"] = room.maxPlayer;
                jObject.roomInfo["isLocked"] = room.IsLocked();
                jObject.roomInfo["isSessionStarted"] = room.isSessionStarted;
                let customProperties = {};
                if (room.customPropertyKeysForLobby.length > 0) {
                    room.customPropertyKeysForLobby.forEach(key => {
                        customProperties[key] = room.customProperties[key];
                    });
                }
                jObject.roomInfo["customProperties"] = customProperties;
                break;
            case OpResponseRoom.OnPlayerJoined:
            case OpResponseRoom.OnPlayerLeft:
                jObject.roomInfo["playerCount"] = room.playerCount;
                break;
            case OpResponseRoom.OnRemoved:
                break;
        }

        this.SendJSONObjectToPlayersInLobby(jObject);
    }

    // Misc.........................................................
    SendJSONObjectToPlayersInLobby(jObject) {
        let players = Object.keys(this.players);
        players.forEach(player => {
            if (!player.IsInRoom()) {
                SendJsonObjectToSocket(player.socket, jObject);
            }
        })
    }
}

module.exports = Lobby;