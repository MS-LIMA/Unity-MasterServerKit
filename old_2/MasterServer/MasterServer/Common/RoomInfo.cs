using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msk 
{
    public struct RoomInfo
    {
        /// <summary>
        /// Name of this room.
        /// </summary>
        public string name;

        /// <summary>
        /// Ip of this room.
        /// </summary>
        public string ip;

        /// <summary>
        /// Port of this room.
        /// </summary>
        public ushort port;

        /// <summary>
        /// Is room open?
        /// </summary>
        public bool isOpen;

        /// <summary>
        /// Does room have password?
        /// </summary>
        public bool isPasswordLock;

        /// <summary>
        /// Player count of this room.
        /// </summary>
        public int playerCount;

        /// <summary>
        /// Max player count of this room.
        /// </summary>
        public int maxPlayers;
        
        /// <summary>
        /// Custom properties of this room only available in lobby.
        /// </summary>
        public MskProperties customProperties;

        public static RoomInfo[] DeserializeInfos(string json)
        {
            return JToken.Parse(json).ToObject<RoomInfo[]>();
        }
    }
}