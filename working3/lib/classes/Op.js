export const OpRequest = {
    // Master
    connectToMaster : 1,
    
    // Lobby
    createRoom : 2,
    joinRoom : 3,
    joinRandomRoom : 4,
    leaveRoom : 5,
    listRoomInfos : 6,
    getPlayerCount : 7,

    // Room
    setMaster : 10,
    kickPlayer : 11,
    setRoomCustomProperties : 12,
    setGameStart : 13,

    startInstance : 14,
    stopInstance : 15,

    // Player
    setNickname : 20,
    setPlayerCustomProperties : 21,

    // Misc
}

export const OpResponse = {
    onConnectedToMaster : 1,
    onConnectToMasterFailed : 2,
    onCreatedRoom : 3,
    onCreateRoomFailed : 4,
    onJoinedRoom : 5,
    onJoinedRandomRoom : 6,
    onJoinRoomFailed : 7,
    onJoinRandomRoomFailed : 8,
    onLeftRoom : 9,

    // Player
    onNicknameChanged : 100,

    onRoomInfoChanged : 200,
    onRoomInfoslisted : 201,
    onPlayerCountGathered : 202,
}

export const DisconnectionCause = {
    leave : 1,
    kicked : 2,
    disconnectedFromMaster : 3
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
    onMasterChanged : 1,
    onPlayerJoined : 2,
    onPlayerLeft : 3,
    onCustomPropertiesUpdated : 4,
    onPasswordChanged : 5,
    onPlayerCustomPropertiesUpdated : 6,
    onServerInstanceActive : 7,
    onServerInstanceInActive : 8,
    onGameStart : 9,
}

export const RoomOp = {
    onCreated : 1,
    onRemoved : 2,
    onPlayerCountChanged : 3,
    onCustomPropertiesUpdated : 4,
    onPasswordChanged : 5,
    onGameStart : 6,
}

export const ServerInstanceOp = {
    active : 1,
    inActive : 2
}