namespace MasterServerKit
{
    public struct PlayerInfo
    {
        public int Id { get; set; }
        public string Nickname { get; set; }
        public string RoomName { get; set; }
        public MskProperties CustomProperties { get; set; }

        public PlayerInfo(int id, string nickname, string roomName, MskProperties customProperties)
        {
            this.Id = id;
            this.Nickname = nickname;
            this.RoomName = roomName;
            this.CustomProperties = customProperties;
        }

        public string SerializeJson()
        {
            return JsonSerializer.ToJson(this);
        }
    }
}