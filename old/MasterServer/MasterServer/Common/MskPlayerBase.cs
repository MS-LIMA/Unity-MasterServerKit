using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Msk
{
    public class MskPlayerBase
    {
        // Properties
        /// <summary>
        /// Player's client id on master server.
        /// </summary>
        public int ClientId { get; set; }

        /// <summary>
        /// Player's uuid on master server.
        /// </summary>
        public string UUID { get; set; }

        /// <summary>
        /// Player's nickname.
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// Player's custom properties. It will be synced in lobby and room.
        /// </summary>
        public MskProperties CustomProperties { get; set; }

        /// <summary>
        /// Room name where the player has joined in.
        /// </summary>
        public string RoomName { get; set; }

        /// <summary>
        /// Is player in a room?
        /// </summary>
        public bool InRoom { get { return !string.IsNullOrEmpty(RoomName); } }


        public MskPlayerBase(int clientId, string uuid)
        {
            this.ClientId = clientId;
            this.UUID = uuid;

            this.Nickname = $"User[{clientId}]";
            this.CustomProperties = new MskProperties();
        }

        public virtual string SerializeJson()
        {
            JObject j = new JObject();
            j.Add("ClientId", ClientId);
            j.Add("UUID", UUID);
            j.Add("Nickname", Nickname);
            j.Add("CustomProperties", CustomProperties.SerializeJson());
            j.Add("RoomName", RoomName);

            return j.ToString();
        }
    }
}
