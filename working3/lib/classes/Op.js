export const OpRequest = {
    // Master
    connectToMaster : 1,
    
    // Lobby
    createRoom : 2,
    joinRoom : 3,
    joinRandomRoom : 4,
    leaveRoom : 5,

    // Room

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
}

export const DisconnectionCause = {
    leave : 1,
    kicked : 2,
    disconnectedFromMaster : 3
}