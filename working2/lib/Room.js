import { OpReq, OpRes, Room, JSONUtility } from "../lib";

class Room {
    constructor(creator, roomOptions) {
        this.isSessionStarted = false;

        this.id = crypto.randomUUID();
        this.name = roomOptions.name;

        this.password = roomOptions.password;
        this.isLocked = (this.password) ? true : false;

        this.masterId = creator.id;
        this.playerCount = 1;
        this.maxPlayerCount = roomOptions.maxPlayerCount;
        this.players = { [creator.id]: creator };

        this.customProperties = roomOptions.customProperties;
        this.customPropertyKeysForLobby = roomOptions.customPropertyKeysForLobby;
    }

    GetCustomPropertiesForLobby() {
        if (this.customPropertyKeysForLobby.length > 0) {
            let customPropertiesForLobby = {};
            this.customPropertyKeysForLobby.forEach(key => {
                if (key in this.customProperties) {
                    customPropertiesForLobby[key] = this.customProperties[key];
                }
            });

            return customPropertiesForLobby;
        }

        return null;
    }

    AddPlayer(player) {
        if (this.playerCount <= 0) {

        }
        else {
            /////////////////////////////////////////
            // Send joiner info to other players.
            let jObject = {
                "opRes": OpRes.OnPlayerJoined,
                "joiner": player
            };
            Object.values(this.players).forEach(_player => {
                SendJSONObjectToSocket(socket, jObject);
            });
            ///////////////////////////////////////////

            this.players[player.id] = player;
        }

        this.playerCount++;
    }

    RemovePlayer(player) {
        if (this.playerCount > 1) {
            //////////////////////////////////////////
            // Send leaver info to other players.
            let jObject = {
                "opRes": OpRes.OnPlayerLeft,
                "leaverId": player.id
            };
            Object.values(this.players).forEach(_player => {
                if (_player.id != player.id) {
                    SendJSONObjectToSocket(socket, jObject);
                }
            });
        }

        delete this.players[player.id];
    }
}