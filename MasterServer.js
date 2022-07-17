const crypto = require('crypto');

const { read } = require('fs');
const { json } = require('node:stream/consumers');
const { join } = require('path');

/////////////////////////////////////////////////////////////////
class Player
{
    constructor(id, nickname, roomId, customProperties)
    {
        this.id = id;
        this.nickname = nickname;
        this.roomId = roomId;
        this.customProperties = customProperties;
    }
}

class Room
{
    constructor(roomName, password, passwordEnabled, owner, maxPlayer, players, customProperties)
    {
        this.roomId = crypto.randomUUID();
        this.roomName = roomName;
        this.password = password;
        this.passwordEnabled = passwordEnabled;
        this.owner = owner;
        this.maxPlayer = maxPlayer;
        this.players = players;
        this.customProperties = customProperties;
    }

    GetPlayerCount() {
        return Object.keys(this.players).length;
    }

    GetFirstPlayer() {
        if (this.GetPlayerCount() > 0)
            {
                return Object.values(this.players)[0];
            }
        else{
            return null;
        }
    }
}

class RoomOptions
{
    constructor(roomName, password, maxPlayer, customProperties)
    {
        this.roomName = roomName;
        this.password = password;
        this.maxPlayer = maxPlayer;
        this.customProperties = customProperties;
    }
}

const Op = {
    ConnectToMaster: 0,
    OnConnectedToMaster: 1,

    CreateRoom: 2,
    OnCreatedRoom: 3,
    OnCreateRoomFailed: 4,

    JoinRoom : 5,
    OnJoinedRoom : 6,
    OnJoinRoomFailed : 7,

    LeaveRoom : 8,
    OnLeftRoom : 9,
    OnLeaveRoomFailed : 10,

    OnRoomListUpdated: 11
}

const RoomListOp = {
    OnCreated: 0,
    OnRoomNameEdit: 1,
    OnPasswordEdit: 2,
    OnPasswordEnabled: 3,
    OnOwnerChanged: 4,
    OnMaxPlayerChanged: 5,
    OnPlayerJoined: 6,
    OnPlayerLeft: 7,
    OnCustomPropertiesChanged: 8,
    OnRemoved: 9
}

const FailureCause = {
    RoomIdNotFound : 0,
    RoomIdNotSame : 1,
    NotInRoom : 2,
    RoomNotFound : 3,
    RoomIsFull : 4,

}

/////////////////////////////////////////////////////////////////
function ConnectToMaster(ws, jObject)
{
    CreatePlayer(ws, jObject);
    ws.send(JSON.stringify({
        "op": Op.OnConnectedToMaster
    }));
}

function CreatePlayer(ws, jObject)
{
    let playerJObject = jObject.player;
    let player = new Player(
        playerJObject.id,
        playerJObject.nickname,
        playerJObject.roomId,
        playerJObject.customProperties
    );

    ws.id = player.id;
    ws.automaticallyUpdateRoomList = true;

    players[player.id] = player;
    playerWebSockets[player.id] = ws;

    return player;
}

function CreateRoom(ws, jObject)
{
    let roomOptions = jObject.roomOptions;
    let room = new Room(
        roomOptions.roomName,
        roomOptions.password,
        (roomOptions.password === "") ? false : true,
        players[ws.id],
        roomOptions.maxPlayer,
        { [ws.id]: players[ws.id] },
        {}
    );

    rooms[room.roomId] = room;
    ws.send(JSON.stringify({
        "op": Op.OnCreatedRoom,
        "room": room
    }));

    players[ws.id].roomId = room.roomId;

    ws.automaticallyUpdateRoomList = false;
    UpdateRoomList(RoomListOp.OnCreated, room);

    console.log("Player(%s) has created a room(%s).", ws.id, room.roomId);
}

function LeaveRoom(ws, jObject){
    let roomId = jObject.roomId;
    let player = players[ws.id];

    // If player is not in room.
    if (player.roomId === ""){
        SendMessage(ws, {
            "op" : Op.OnLeaveRoomFailed,
            "failureCause" : FailureCause.NotInRoom
        });

        return;
    }

    // If player requests the wrong id which is different with the room player joined in.
    if (player.roomId != roomId){
        SendMessage(ws, {
            "op" : Op.OnLeaveRoomFailed,
            "failureCause" : FailureCause.RoomIdNotSame
        });

        return; 
    }

    if (roomId in rooms){
        let room = rooms[roomId];
        delete room.players[player.id];

        if (room.GetPlayerCount() <= 0){
            delete rooms[room.id];
            UpdateRoomList(RoomListOp.OnRemoved, room);
        }
        else{
            let jObject = {
                "op" : Op.OnRoomListUpdated,
                "roomListOp" : []
            };

            if (room.owner.id == player.id){
                let nextPlayer = room.GetFirstPlayer();
                room.owner = nextPlayer;

                jObject["roomListOp"].push(RoomListOp.OnOwnerChanged);
                jObject["ownerId"] = room.owner.id;
            }

            jObject["roomListOp"].push(RoomListOp.OnPlayerLeft);
            jObject["playerLeftId"] = player.id;

            UpdateRoomList(jObject);
            //방에 있는 사람들한테도 알려줘야함.
        }

        SendMessage(ws, {"op" : Op.OnLeftRoom});
        console.log("Player(%s) has left the room(%s)", player.id, roomId);
    }
    // Room not found.
    else{
        SendMessage(ws, {
            "op" : Op.OnLeaveRoomFailed,
            "failureCause" : FailureCause.RoomIdNotFound
        });
    }
}

function JoinRoom(ws, jObject){
    let roomId = jObject.roomId;
    if (roomId in rooms){
        let room = rooms[roomId];
        if (room.GetPlayerCount >= room.maxPlayer){
                //full;
        }
        else{
            room.players[ws.id] = players[ws.id];
            SendMessage(ws, {
                "op" : Op.OnJoinedRoom,
                "roomId" : roomId
            });

            UpdateRoomList(RoomListOp.OnJoinedRoom, {"roomId" : roomId, "player" : players[ws.id]});
        }
    }
    else{
            //Room not found
    }
}

function UpdateRoomList(jObject){
    Object.values(playerWebSockets).forEach(x =>
        {
            if (x.automaticallyUpdateRoomList)
            {
                SendMessage(x, jObject);
            }
        });
}

function UpdateRoomList(roomListOp, jObject)
{
    Object.values(playerWebSockets).forEach(x =>
    {
        if (x.automaticallyUpdateRoomList)
        {
            switch (roomListOp)
            {
                case RoomListOp.OnCreated:
                    x.send(JSON.stringify({
                        "op" : Op.OnRoomListUpdated,
                        "roomListOp" : [RoomListOp.OnCreated],
                        "room" : jObject
                    }));
                    break;
                case RoomListOp.OnRemoved:
                    SendMessage(x, {
                        "op" : Op.OnRoomListUpdated,
                        "roomListOp" : [RoomListOp.OnRemoved],
                        "roomId" : jObject.roomId
                    });
                case RoomListOp.OnPlayerJoined:
                    SendMessage(x, {
                        "op" : Op.OnRoomListUpdated,
                        "roomListOp" : [RoomListOp.OnPlayerJoined],
                        "roomId" : jObject["roomId"],
                        "player" : jObject["player"]
                    });
                default:
                    break;
            }
        }
    });
}

function UpdateRoom()
{

}

function OnClientDisconnected(ws)
{

}

/////////////////////////////////////////////////////////////////
var port = 3333;
var wsServer = require('ws').Server;
var wss = new wsServer({ port: port });

var players = {};
var playerWebSockets = {};
var rooms = {};

console.log('Server opened on port %d.', port);

wss.on('connection', function connection(ws)
{
    console.log("Client connected");

    ws.on('message', (message) =>
    {
        let jObject = JSON.parse(message);
        switch (jObject.op)
        {
            case Op.ConnectToMaster:
                ConnectToMaster(ws, jObject);
                break;
            case Op.CreateRoom:
                CreateRoom(ws, jObject);
                break;
            case Op.LeaveRoom:
                LeaveRoom(ws, jObject);
                break;
            case Op.JoinRoom:
                JoinRoom(ws, jObject);
            default:
                break;
        }
    });

    ws.on('close', () =>
    {
        OnClientDisconnected(ws);
    });
});

function SendMessage(ws, obj){
    ws.send(JSON.stringify(obj));
}