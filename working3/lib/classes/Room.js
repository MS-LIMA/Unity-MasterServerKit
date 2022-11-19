import {Util} from '../utilities';
const crypto = require('crypto');

export class Room {
    constructor(roomOptions, customProperties, config, onEmpty){
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

        // Server instance setting
        this.ip = config.ip;
        this.port = config.port;
        this.path = config.path + '/' + config.version+'';
        this.exec = null;
    }

    isFull = () => {
        return this.playerCount >= this.maxPlayerCount;
    }

    isLocked = () => {
        return this.password ? true : false;
    }

    getJsonObjectForLobby = (...args) => {
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
            jObject.isLocked = this.isLocked();
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

    getJsonObjectForRoom = () => {
        return {
            id : this.id,
            name : this.name,

            isGameStart : this.isGameStart,
            masterId : this.masterId,
            playerCount : this.playerCount,
            players : this.players,
            maxPlayerCount : this.maxPlayerCount,
            customProperties : this.customProperties
        }
    }

    // Server instance
    //////////////////////////////////////////////////////
    startInstance = () => {
        this.exec = require('child_process').exec;
        this.exec(this.path + `-batchmode -port ${this.port} -ip ${this.ip}`, (err, stdout, stderr) => {});
    }

    stopInstance = () => {

    }


    // Player
    //////////////////////////////////////////////////////
    setMaster = (master) => {
        this.masterId = master.id;

        if (this.playerCount <= 0){
            this.players[master.id] = master;
            this.playerCount++;
            this.broadcastToPlayers({
                roomOpInternal : RoomOpInternal.OnPlayerJoined,
                player : master
            });
        }

        this.broadcastToPlayers({
            roomOpInternal : RoomOpInternal.OnMasterChanged,
            masterId : this.masterId
        });
    }

    addPlayer = (player) => {    
        this.players[player] = player;
        this.playerCount++;

        this.broadcastToPlayers({
            roomOpInternal : RoomOpInternal.OnPlayerJoined,
            player : player
        });

        if (this.playerCount <= 1){
            this.setMaster(player);
        }
    }

    removePlayer = (player, reason) => {
        if (player.id in this.players){
            delete this.players[player.id];
            this.playerCount--;
        }

        this.broadcastToPlayers({
            roomOpInternal : RoomOpInternal.OnPlayerLeft,
            playerId : player.id,
            reason : reason
        });

        if (this.playerCount <= 0){
            onEmpty(this);
        }
        else{
            if (masterId === player.id){
                this.setMaster((Object.values(this.players))[0]);
            }
        }
    }

    kickPlayer = (player) => {
        if (player.id === this.masterId){
            return;
        }

        this.removePlayer(player, 'kick');
    }

    // Broadcast
    ///////////////////////////////////////////////////
    broadcastToPlayers = (object) => {
        const sockets = this.players.map(x=>x.socket);
        Util.sendJSONObjectToSockets(sockets, object);
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