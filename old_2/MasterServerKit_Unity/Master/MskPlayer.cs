using Msk;
using Newtonsoft.Json;

namespace Msk.Master
{
    public class MskPlayer : MskPlayerBase
    {
        /// <summary>
        /// Socket client of this player.
        /// </summary>
        public MskSocket Client { get; set; }

        /// <summary>
        /// Version of this player.
        /// </summary>
        public string Version { get; set; }

        public PlayerInfo PlayerInfo
        {
            get
            {
                return new PlayerInfo
                {
                    clientId = this.ClientId,
                    uuid = this.UUID,
                    nickname = this.Nickname,
                    roomName = this.RoomName,
                    customProperties = this.CustomProperties
                };
            }
        }

        public MskPlayer(MskSocket client, string version, int clientId, string uuid) : base(clientId, uuid)
        {
            this.Client = client;
            this.Version = version;
        }
    }
}
