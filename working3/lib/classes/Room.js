import { Util } from '../utilities';
import { RoomOp, RoomOpInternal, ServerInstanceOp } from './Op';
const crypto = require('crypto');

export class Room {
    constructor(roomOptions, customProperties, config, onEmpty) {
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
        this.path = config.path + '/' + config.version + '';
        this.exec = null;

        this.initSocket();
    }

    isFull = () => {
        return this.playerCount >= this.maxPlayerCount;
    }

    isLocked = () => {
        return this.password ? true : false;
    }

    getRoomInfoForLobby = (...args) => {
        let jObject = {
            id: this.id
        }

        if (RoomParams.name in args) {
            jObject.name = this.name;
        }
        if (RoomParams.playerCount in args) {
            jObject.playerCount = this.playerCount;
        }
        if (RoomParams.players in args) {
            jObject.players = this.players;
        }
        if (RoomParams.maxPlayerCount in args) {
            jObject.maxPlayerCount = this.maxPlayerCount;
        }
        if (RoomParams.isLocked in args) {
            jObject.isLocked = this.isLocked();
        }
        if (RoomParams.customProperties in args) {
            let customProperties = [];
            this.customPropertyKeysFromLobby.forEach(key => {
                if (key in this.customProperties) {
                    customProperties.push(this.customProperties[key]);
                }
            });
            jObject.customProperties = customProperties;
        }

        return jObject;
    }

    getRoomInfoForRoom = () => {
        return {
            id: this.id,
            name: this.name,

            isGameStart: this.isGameStart,
            masterId: this.masterId,
            playerCount: this.playerCount,
            players: this.players,
            maxPlayerCount: this.maxPlayerCount,
            customProperties: this.customProperties,

            isServerInstanceActive : this.isServerInstanceActive,
            ip: this.ip,
            port : this.port
        }
    }


    // Room options
    //////////////////////////////////////////////////////
    setCustomProperties = (customProperties, callback) => {
        this.customProperties = {
            ...this.customProperties,
            ...customProperties
        };

        this.broadcastToPlayers({
            roomOpInternal : RoomOpInternal.onCustomPropertiesUpdated,
            customProperties : this.customProperties
        });

        callback();
    }

    setPassword = (password, callback) => {
        this.password = password;
        this.broadcastToPlayers({
            roomOpInternal:RoomOpInternal.onPasswordChanged,
            isLocked : this.isLocked()
        });

        callback();
    }

    setGameStart = (bool) => {
        this.isGameStart = bool;
        this.broadcastToPlayers({
            roomOpInternal : RoomOpInternal.onGameStart,
            isStart : bool
        });
    }


    // Server instance
    //////////////////////////////////////////////////////
    initSocket = () => {
        var wsServer = require('ws').Server;
        var wss = new wsServer({ port: this.port });
    
        console.log('Server Instance opened on port %d.', this.port);
    
        wss.on('connection', function connection(socket) {    
            console.log("Server Instance connected");        
            socket.on('message', (message) => {
                const jObject = JSON.parse(message);
                if (jObject.serverInstanceOp === ServerInstanceOp.active){
                    onServerInstanceActive();
                }
                else{
                    onServerInstanceInActive();
                }
            });      
            socket.on('close', () => {
                onServerInstanceInActive();
            });
        }); 
    }

    startServerInstance = () => {
        this.exec = require('child_process').exec;
        this.exec(this.path + `-batchmode -port ${this.port} -ip ${this.ip}`, (err, stdout, stderr) => { });

        this.exec.on('close', this.onServerInstanceInActive);
    }

    stopServerInstance = () => {
        this.exec.kill();
    }

    onServerInstanceActive = () => {
        this.isServerInstanceActive = true;

        this.broadcastToPlayers({
        roomOpInternal : RoomOpInternal.onServerInstanceActive,
        ip : this.ip,
        port : this.port
        });
    }

    onServerInstanceInActive = (code = '') => {
        this.isServerInstanceActive = false;

        this.broadcastToPlayers({
            roomOpInternal : RoomOpInternal.onServerInstanceInActive,
        });
    }


    // Player
    //////////////////////////////////////////////////////
    setMaster = (master) => {
        this.masterId = master.id;

        if (this.playerCount <= 0) {
            this.players[master.id] = master;
            this.playerCount++;
            this.broadcastToPlayers({
                roomOpInternal: RoomOpInternal.onPlayerJoined,
                player: master
            });
        }

        this.broadcastToPlayers({
            roomOpInternal: RoomOpInternal.onMasterChanged,
            masterId: this.masterId
        });
    }

    setMaster = (requesterId, targetId) => {
        if (requesterId !== this.masterId){
            return;
        }

        const player = this.players[targetId];
        if (player){
            this.setMaster(player);
        }
    }

    addPlayer = (player) => {
        this.players[player] = player;
        this.playerCount++;

        this.broadcastToPlayers({
            roomOpInternal: RoomOpInternal.onPlayerJoined,
            player: player
        });

        if (this.playerCount <= 1) {
            this.setMaster(player);
        }
    }

    removePlayer = (player, reason) => {
        if (player.id in this.players) {
            delete this.players[player.id];
            this.playerCount--;
        }

        this.broadcastToPlayers({
            roomOpInternal: RoomOpInternal.onPlayerLeft,
            playerId: player.id,
            reason: reason
        });

        if (this.playerCount <= 0) {
            onEmpty(this);
        }
        else {
            if (masterId === player.id) {
                this.setMaster((Object.values(this.players))[0]);
            }
        }
    }

    kickPlayer = (requesterId, playerId) => {
        if (requesterId !== masterId) {
            return;
        }

        if (playerId === this.masterId) {
            return;
        }

        const player = this.players[playerId];
        if (player) {
            this.removePlayer(player, 'kick');
        }
    }

    setPlayerCustomProperties = (player, customProperties) => {
        this.broadcastToPlayers({
            roomOpInternal : RoomOpInternal.onPlayerCustomPropertiesUpdated,
            playerId : player.id,
            customProperties : customProperties
        });
    }


    // Broadcast
    ///////////////////////////////////////////////////
    broadcastToPlayers = (object) => {
        const sockets = this.players.map(x => x.socket);
        Util.sendJSONObjectToSockets(sockets, object);
    }

    broadcastToPlayer = (player, object) => {
        Util.sendJSONObjectToSocket(player.socket, object);
    }
}

