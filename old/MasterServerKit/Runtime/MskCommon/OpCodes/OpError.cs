namespace MasterServerKit
{
    public enum OpError
    {
        // Success
        Success,

        // Master
        MaxConnectionsReached,
        LobbyNotFound,

        // Lobby
        AlreadyInLobby,

        // Create Room
        MaxiumInstanceCountReached,
        MaximumRoomNumberReached,
        RoomNameDuplicated,
        SpawnRequestDuplicated,

        // Room
        RoomNotFound,
        RoomIsFull,
        IncorrectPassword,
        RoomIsClosed,

        // Error
        InternalError,
    }
}
