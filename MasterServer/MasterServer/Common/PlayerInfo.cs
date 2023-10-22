using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Msk
{
    public struct PlayerInfo
    {
        /// <summary>
        /// Client id of this player.
        /// </summary>
        public int clientId;

        /// <summary>
        /// UUID of this player.
        /// </summary>
        public string uuid;

        /// <summary>
        /// Nickname of this player.
        /// </summary>
        public string nickname;

        /// <summary>
        /// Room name where this player is joined.
        /// </summary>
        public string roomName;

        /// <summary>
        /// Custom properties of this player.
        /// </summary>
        public MskProperties customProperties;

        /// <summary>
        /// Is this player in room?
        /// </summary>
        [JsonIgnore]     
        public bool InRoom
        {
            get
            {
                return !string.IsNullOrEmpty(roomName);
            }
        }

        public static PlayerInfo[] DeserializeInfos(string json)
        {
            return JToken.Parse(json).ToObject<PlayerInfo[]>();
        }
    }
}