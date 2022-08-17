const Player = require("./Player");
const Op = require("./Op");
const Utility = require("./Utility");
const Lobby = require("./Lobby");
const path = require("path");

class Room {
    constructor(creator, roomName, roomOptions, serverIp, port, lobby) {
        // Room infos
        this.id = crypto.randomUUID();
        this.name = roomName;

        this.isSessionStarted = false;
        this.password = roomOptions.password;
        this.isLocked = (this.password) ? true : false;

        this.maxPlayers = roomOptions.maxPlayers;
        this.masterId = creator.id;

        this.players = { [creator.id]: creator };
        this.playerCount = 0;

        this.customProperties = roomOptions.customProperties;
        this.customPropertyKeysForLobby = roomOptions.customPropertyKeysForLobby;

        // Server infos
        this.serverIp = serverIp,
        this.port = port;

        // Misc
        this.lobby = lobby;
    }

    //#region Room control
    SendJsonObjectToPlayers(jObject) {
        let players = Object.values(this.players);
        players.forEach(player => {
            SendJsonObject(player.socket, jObject);
        });
    }

    OnRoomOpRequested(opRequestRoom, requestInfo) {
        switch (opRequestRoom) {
            case OpRequestRoom.SetRoomName:
                this.name = requestInfo.roomName;
                this.SendJsonObjectToPlayers({
                    "opResponseRoom": OpResponseRoom.OnRoomNameSet,
                    "roomName": this.name
                });
                this.lobby.UpdateRoomInfo(OpResponseRoom.OnRoomNameSet, this);
                break;
            case OpRequestRoom.SetPassword:
                this.password = requestInfo.password;
                this.isLocked = (this.password) ? true : false;

                this.SendJsonObjectToPlayers({
                    "opResponseRoom": OpResponseRoom.OnPasswordSet,
                    "isLocked": isLocked
                });
                this.lobby.UpdateRoomInfo(OpResponseRoom.OnPasswordSet, this);
                break;
            case OpRequestRoom.SetMaster:
                this.masterId = requestInfo.masterId;
                this.SendJsonObjectToPlayers({
                    "opResponseRoom": OpResponseRoom.OnMasterChanged,
                    "masterId": this.masterId
                });
                break;
            case OpRequestRoom.SetMaxPlayers:
                this.maxPlayers = requestInfo.maxPlayers;
                this.SendJsonObjectToPlayers({
                    "opResponseRoom": OpResponseRoom.OnMaxPlayersSet,
                    "maxPlayers": this.maxPlayers
                });
                break;
            case OpRequestRoom.SetCustomProperties:
                let keys = Object.keys(requestInfo.customProperties);
                keys.forEach(key => {
                    this.customProperties[key] = requestInfo.customProperties[key];
                });
                this.SendJsonObjectToPlayers({
                    "opResponseRoom": OpResponseRoom.OnCustomPropertiesSet,
                    "customProperties": requestInfo.customProperties
                });
                this.lobby.UpdateRoomInfo(OpResponseRoom.OnCustomPropertiesSet, this, requestInfo.customProperties);
                break;
            case OpRequestRoom.KickPlayer:
                let playerId = requestInfo.playerId;
                if (playerId in this.players){
                    this.lobby.LeaveRoom(playerId, this.id, DisconnectionCause.KickedFromRoom);
                }
                break;
            case OpRequestRoom.SendChatMessage:
                this.SendJsonObjectToPlayers({
                    "opResponseRoom" : OpResponseRoom.OnChatMessage,
                    "message" : requestInfo.message
                });
                break;
        }
    }

    //#endregion

    //#region Player control
    AddPlayer(player) {
        this.players[player.id] = player;
        this.playerCount++;

        player.roomId = this.id;

        this.SendJsonObjectToPlayers({
            "opResponseRoom": OpResponseRoom.OnPlayerJoined,
            "player": player
        });
    }

    RemovePlayer(player, disconnectionCause) {
        delete this.players[player.id];
        this.playerCount--;

        player.roomId = null;

        if (this.playerCount > 0) {

            this.SendJsonObjectToPlayers({
                "opResponseRoom": OpResponseRoom.OnPlayerLeft,
                "playerId": player.id,
                "disconnectionCause": disconnectionCause
            });

            if (player.id == this.masterId) {
                let nextPlayer = Object.values(this.players)[0];
                this.masterId = nextPlayer.id;

                this.SendJsonObjectToPlayers({
                    "opResponseRoom": OpResponseRoom.OnMasterChanged,
                    "masterId": this.masterId
                });
            }
        }
        else {

        }

        if (disconnectionCause == DisconnectionCause.KickedFromRoom){
            SendJsonObjectToSocket(player.socket, {
                "opResponse": OpResponse.OnLeftRoom,
                "disconnectionCause" : DisconnectionCause.KickedFromRoom
            });
        }
    }
    //#endregion

    //#region Server control
    StartSession(){
        var fileName = "C:/servers/"+this.lobby.version+"/"+'server.exe -batchmode -port' + this.port;
        var dirName = path.dirname(fileName);

        var exec = require('child_process').exec;
        exec(fileName, {cwd :dirName});
    }

    StopSession(){

    }

    //#endregion
}

module.exports = Room;