namespace MasterServerKit
{
    public struct RoomInfo
    {
        public string RoomName { get; set; }

        public int PlayerCount { get; set; }
        public int MaxPlayers { get; set; }

        public bool IsOpen { get; set; }
        public bool IsPasswordLock { get; set; }

        public MskProperties CustomPropertiesInLobby { get; set; }

        public RoomInfo(string roomName, int playerCount, int maxPlayers, bool isOpen, bool isPasswordLock, MskProperties properties)
        {
            this.RoomName = roomName;
            this.PlayerCount = playerCount;
            this.MaxPlayers = maxPlayers;
            this.IsOpen = isOpen;
            this.IsPasswordLock = isPasswordLock;
            this.CustomPropertiesInLobby = properties;
        }
    }
}
