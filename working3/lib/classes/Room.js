import {Util} from '../utilities';
const crypto = require('crypto');

export class Room {
    constructor(roomOptions, customProperties, onEmpty){
        // Room props
        this.id = crypto.randomUUID();
        this.name = roomOptions.roomName;

        this.isGameStart = false;
        this.isServerInstanceActive = false;

        // Players
        this.masterId = null;
        this.playerCount = 0;
        this.players = {};

        // Room options
        this.password = roomOptions.password;
        this.maxPlayerCount = roomOptions.maxPlayerCount;

        // Custom props
        this.customProperties = customProperties;
        this.customPropertyKeysFromLobby = roomOptions.customPropertyKeysFromLobby;

        // Callbacks
        this.onEmpty = onEmpty;
    }

    IsFull = () => {
        return this.playerCount >= this.maxPlayerCount;
    }

    IsLocked = () => {
        return this.password ? true : false;
    }

    GetJsonObject = (...args) => {
        let jObject = {
            id : this.id
        }

        if (RoomParams.name in args){
            jObject.name = this.name;
        }
        if (RoomParams.playerCount in args){
            jObject.playerCount = this.playerCount;
        }
        if (RoomParams.players in args){
            jObject.players = this.players;
        }
        if (RoomParams.maxPlayerCount in args){
            jObject.maxPlayerCount = this.maxPlayerCount;
        }
        if (RoomParams.isLocked in args){
            jObject.isLocked = this.IsLocked();
        }
        if (RoomParams.customProperties in args){
            let customProperties = [];
            this.customPropertyKeysFromLobby.forEach(key => {
                if (key in this.customProperties){
                    customProperties.push(this.customProperties[key]);
                }
            });
            jObject.customProperties = customProperties;
        }

        return jObject;
    }

    // Player
    //////////////////////////////////////////////////////
    SetMaster = (master) => {
        this.masterId = master.id;

        if (this.playerCount <= 0){
            this.players[master.id] = master;
            this.playerCount++;
            this.BroadcastToPlayers({
                roomOpInternal : RoomOpInternal.OnPlayerJoined,
                player : master
            });
        }

        this.BroadcastToPlayers({
            roomOpInternal : RoomOpInternal.OnMasterChanged,
            masterId : this.masterId
        });
        // Broadcast master;;;;
    }

    AddPlayer = (player) => {    
        this.players[player] = player;
        this.playerCount++;

        this.BroadcastToPlayers({
            roomOpInternal : RoomOpInternal.OnPlayerJoined,
            player : player
        });

        if (this.playerCount <= 1){
            this.SetMaster(player);
        }
    }

    RemovePlayer = (player, reason) => {
        if (player.id in this.players){
            delete this.players[player.id];
            this.playerCount--;
        }

        this.BroadcastToPlayers({
            roomOpInternal : RoomOpInternal.OnPlayerLeft,
            playerId : player.id,
            reason : reason
        });

        if (this.playerCount <= 0){
            onEmpty(this);
        }
        else{
            if (masterId === player.id){
                this.SetMaster((Object.values(this.players))[0]);
            }
        }
    }

    KickPlayer = (player) => {
        if (player.id === this.masterId){
            return;
        }

        this.RemovePlayer(player, 'kick');
    }

    // Broadcast
    ///////////////////////////////////////////////////
    BroadcastToPlayers = (object) => {
        const sockets = this.players.map(x=>x.socket);
        Util.SendJSONObjectToSockets(sockets, object);
    }
}

export const RoomParams = {
    name : 1,
    isGameStart : 2,
    playerCount : 3,
    players : 4,
    maxPlayerCount : 5,
    isLocked : 6,
    customProperties : 7
}

export const RoomOpInternal = {
    OnMasterChanged : 1,
    OnPlayerJoined : 2,
    OnPlayerLeft : 3,
}

export const RoomOp = {
    OnCreated : 1,
    OnRemoved : 2,

}