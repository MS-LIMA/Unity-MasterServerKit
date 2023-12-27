using UnityEngine;
using Newtonsoft.Json.Linq;

namespace Msk
{
    public class MskPlayer : MskPlayerBase
    {
        /// <summary>
        /// Is local player?
        /// </summary>
        public bool IsLocal { get; private set; }

        public MskPlayer(bool isLocal, int clientId, string uuid) : base(clientId, uuid)
        {
            this.IsLocal = isLocal;
        }

        /// <summary>
        /// Set this player's custom properties.
        /// </summary>
        /// <param name="properties"></param>
        public void SetCustomProperties(MskProperties properties)
        {
            if (MasterServerKit.IsClient)
            {
                MasterServerKit.Client.SetPlayerCustomProperties(this, properties);
            }
            else if (MasterServerKit.IsInstance)
            {
                MasterServerKit.Instance.SetPlayerCustomProperties(this, properties);
            }
        }

        /// <summary>
        /// Deserialize json to set the player's properties. Do not call this method yourself.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static MskPlayer DeserializeJson(string json)
        {           
            JObject j = JObject.Parse(json);

            int clientId = j["ClientId"].Value<int>();
            string uuid = j["UUID"].Value<string>();
            string nickname = j["Nickname"].Value<string>();
            MskProperties props = MskProperties.Deserialize(j["CustomProperties"].Value<string>());
            string roomName = j["RoomName"].Value<string>();

            if (MasterServerKit.Client.LocalPlayer?.UUID == uuid)
            {
                MskPlayer player = MasterServerKit.Client.LocalPlayer;
                player.RoomName = roomName;

                return player;
            }
            else
            {
                MskPlayer player = new MskPlayer(false, clientId, uuid);
                player.Nickname = nickname;
                player.CustomProperties.Append(props);
                player.RoomName = roomName;

                return player;
            }
        }
    }
}
