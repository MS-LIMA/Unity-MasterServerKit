using Newtonsoft.Json;

namespace MasterServerKit.Master
{
    public class MskPlayer
    {
        // Json Properties
        public int Id { get; set; }
        public string Nickname { get; set; }

        public MskProperties CustomProperties { get; set; }

        // Non Json Properties
        [JsonIgnore]
        public MskClient Client { get; set; }
        [JsonIgnore]
        public string Version { get; set; }

        [JsonIgnore]
        public string RoomName { get; set; }
        [JsonIgnore]
        public bool InRoom { get { return !string.IsNullOrEmpty(RoomName); } }


        // Cached
        public PlayerInfo playerInfo;

        public MskPlayer(MskClient client, string version, string nickname, MskProperties customProperties)
        {
            this.Client = client;
            this.Version = version;
            this.Id = client.clientId;
            this.Nickname = string.IsNullOrEmpty(nickname) ? $"User[{client.clientId}]" : nickname;
            this.CustomProperties = customProperties;


            this.playerInfo = new PlayerInfo
            {
                Id = this.Id,
                Nickname = this.Nickname,
                RoomName = this.RoomName,
                CustomProperties = this.CustomProperties,
            };
        }
    }
}
