import { OpReq, OpRes, OpRoomInfo, ErrorCause, Room, JSONUtility } from "../lib";

class Lobby {
    constructor(version) {
        this.version = version;

        this.rooms = {};
        this.players = {};
    }

    OnReqest(data, socket) {
        let opReq = data.opReq;
        switch (opReq) {
            case OpReq.CreateRoom:
                this.CreateRoom(data, socket);
                break;
            case OpReq.JoinRoom:
                this.JoinRoom(data, socket);
                break;
            case OpReq.JoinRandomRoom:
                this.JoinRandomRoom(data, socket);
                break;
            case OpReq.LeaveRoom:
                this.LeaveRoom(data, socket);
                break;
        }
    }

    GetSocketsNotInRoom() {
        let socketsNotInRoom = [];
        Object.values(this.players).forEach(player => {
            if (!player.IsInRoom()) {
                socketsNotInRoom.push(player.socket);
            }
        });

        return socketsNotInRoom;
    }

    SendJSONObjectToSocketNotInRoom(jObject) {
        let socketsNotInRoom = this.GetSocketsNotInRoom();
        if (socketsNotInRoom.length > 0) {
            socketsNotInRoom.forEach(socket => {
                SendJSONObjectToSocket(socket, jObject);
            });
        }
    }

    ///////////////////////////////////////////////////
    // Room Control
    ///////////////////////////////////////////////////
    CreateRoom(data, socket) {
        let player = this.players[data.CreatorId];
        let room = new Room(player, data.roomOptions);

        this.rooms[room.id] = room;
        this.player.roomId = room.id;

        // Send room created response to creator socket.
        SendJSONObjectToSocket(socket, {
            "opRes": OpRes.OnCreatedRoom,
            "room": room
        });

        // Send room joined response to creator socket.
        SendJSONObjectToSocket(socket, {
            "opRes": OpRes.OnJoinedRoom,
            "room": null
        });

        // Send room info updated to player sockets did not join a room.
        this.SendJSONObjectToSocketNotInRoom({
            "opRes": OpRes.OnRoomInfoUpdated,
            "opRoomInfo": OpRoomInfo.OnCreated,
            "roomInfo": {
                "id": room.id,
                "name": room.name,
                "isLocked": room.isLocked,
                "playerCount": room.playerCount,
                "maxPlayerCount": room.maxPlayerCount,
                "customPropertiesForLobby": room.GetCustomPropertiesForLobby()
            }
        });
    }

    JoinRoom(data, socket) {
        let player = this.players[data.joinerId];
        let room = this.rooms[data.roomId];
        if (room) {
            let canJoinRoom, errorCause = this.CanJoinRoom(data, room);
            if (canJoinRoom) {
                room.AddPlayer(player);
                player.roomId = room.id;

                // Send joined room response to joiner socket.
                SendJSONObjectToSocket(socket, {
                    "opRes": OpRes.OnJoinedRoom,
                    "room": room
                });

                // Send room info updated to player sockets did not join a room.
                this.SendJSONObjectToSocketNotInRoom({
                    "opRes": OpRes.OnRoomInfoUpdated,
                    "opRoomInfo": OpRoomInfo.OnPlayerCountChanged,
                    "roomInfo": {
                        "id": room.id,
                        "playerCount": room.playerCount
                    }
                });
            }
            else {
                SendJSONObjectToSocket(socket, {
                    "opRes": OpRes.OnJoinRandomRoomFailed,
                    "errorCause": errorCause
                });
            }
        }
        else {
            SendJSONObjectToSocket(socket, {
                "opRes": OpRes.OnJoinRandomRoomFailed,
                "errorCause": ErrorCause.NoRoomFound
            });
            //console.log("Error : cannot find room.");
        }
    }

    JoinRandomRoom(data, socket) {
        let player = this.players[data.joinerId];

        let indices = [];
        let roomList = Object.keys(this.rooms);
        let length = roomList.length;
        for (let i = 0; i < length; i++) {
            indices.push(i);
        }

        while (indices.length > 0) {
            let index = Math.floor(Math.random() * indices.length);
            let room = roomList[indices[index]];

            let canJoinRoom, errorCause = this.CanJoinRoom(data, room);
            if (canJoinRoom) {
                room.AddPlayer(player);
                player.roomId = room.id;

                // Send joined room response to joiner socket.
                SendJSONObjectToSocket(socket, {
                    "opRes": OpRes.OnJoinedRoom,
                    "room": room
                });

                // Send room info updated to player sockets did not join a room.
                this.SendJSONObjectToSocketNotInRoom({
                    "opRes": OpRes.OnRoomInfoUpdated,
                    "opRoomInfo": OpRoomInfo.OnPlayerCountChanged,
                    "roomInfo": {
                        "id": room.id,
                        "playerCount": room.playerCount
                    }
                });

                return;
            }

            indices.splice(index, 1);
        }

        // Send left room response to leaver socket.
        SendJSONObjectToSocket(socket, {
            "opRes": OpRes.OnJoinRandomRoomFailed,
            "errorCause": ErrorCause.NoRoomFound
        });
    }

    CanJoinRoom(data, room) {
        if (room.isLocked && (data.password != room.password)) {
            return false, ErrorCause.IncorrectPassword;
        }

        if (room.playerCount >= room.maxPlayerCount) {
            return false, ErrorCause.RoomIsFull;
        }

        return true, null;
    }

    LeaveRoom(data, socket) {
        let player = this.players[data.leaverId];
        let room = this.rooms[data.roomId];
        player.roomId = "";

        if (room) {
            room.RemovePlayer(player);

            // Send room info updated to player sockets did not join a room.
            let jObject = {
                "opRes": OpRes.OnRoomInfoUpdated,
                "roomInfo": {
                    "id": room.id,
                }
            };
            if (room.playerCount <= 0) {
                delete this.rooms[room.id];
                jObject["opRoomInfo"] = OpRoomInfo.OnRemoved;
            }
            else {
                jObject["opRoomInfo"] = OpRoomInfo.OnPlayerCountChanged;
                jObject.roomInfo["playerCount"] = room.playerCount;
            }

            this.SendJSONObjectToSocketNotInRoom(jObject);
        }

        // Send left room response to leaver socket.
        SendJSONObjectToSocket(socket, {
            "opRes": OpRes.OnLeftRoom
        });
    }
}