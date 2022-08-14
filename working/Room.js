const Player = require("./Player");
const Op = require("./Op");
const Utility = require("./Utility");

class Room {
    constructor(creator, roomName, roomOptions){
        this.id = crypto.randomUUID();
        this.name = roomName;
        
        this.isSessionStarted= false;
        this.password = roomOptions.password;

        this.maxPlayer = roomOptions.maxPlayer;
        this.masterId = creator.id;
        
        this.players = { [creator.id] : creator};
        this.playerCount = 0;

        this.customProperties = roomOptions.customProperties;
        this.customPropertyKeysForLobby = roomOptions.customPropertyKeysForLobby;
    }

    IsLocked(){
        return (this.password ) ? true : false;
    }

    //#region Room control
    AddCallbacks(onPlayerCountChanged, onRoomRemoved, onRoomOptionsUpdated){
        this.onPlayerCountChanged = onPlayerCountChanged;
        this.onRoomRemoved = onRoomRemoved;
        this.onRoomOptionsUpdated = onRoomOptionsUpdated;
    }

    SendJsonObjectToPlayers(jObject){
        let players = Object.values(this.players);
        players.forEach(player => {
            SendJsonObject(player.socket, jObject);
        });
    }

    OnRoomOpRequested(){

    }

    //#endregion

    //#region Player control
    AddPlayer(player){
        this.players[player.id] = player;
        this.playerCount++;

        this.player.roomId = this.id;

        this.onPlayerCountChanged(this);
        this.SendJsonObjectToPlayers({
            "opResponseRoom" : OpResponseRoom.OnPlayerJoined,
            "player" : player
        });
    }

    RemovePlayer(player, disconnectionCause){
        delete this.players[player.id];
        this.playerCount--;

        this.player.roomId = null;

        if (this.playerCount > 0){

            this.onPlayerCountChanged(this);
            this.SendJsonObjectToPlayers({
                "opResponseRoom" : OpResponseRoom.OnPlayerLeft,
                "playerId" : player.id,
                "disconnectionCause" : disconnectionCause
            });

            if (player.id == this.masterId){
                let nextPlayer = Object.values(this.players)[0];
                this.masterId = nextPlayer.id;

                this.SendJsonObjectToPlayers({
                    "opResponseRoom" : OpResponseRoom.OnOwnerChanged,
                    "masterId" : this.masterId
                });
            }           
        }
        else{
            this.onRoomRemoved(this);
        }
    }
    //#endregion
}

module.exports = Room;