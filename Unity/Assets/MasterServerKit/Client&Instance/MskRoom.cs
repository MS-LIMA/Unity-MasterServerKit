using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace Msk
{
    public class MskRoom : MskRoomBase
    {
        /// <summary>
        /// Current players of this room.
        /// </summary>
        public Dictionary<int, MskPlayer> Players { get; private set; } = new Dictionary<int, MskPlayer>();

        /// <summary>
        /// Player count in this room.
        /// </summary>
        public int PlayerCount { get { return Players.Count; } }

        /// <summary>
        /// Master player of this room.
        /// </summary>
        public MskPlayer Master
        {
            get
            {
                if (Players.ContainsKey(MasterId))
                {
                    return Players[MasterId];
                }

                return null;
            }
        }

        /// <summary>
        /// Find the player in this room by client id.
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public MskPlayer FindPlayer(int clientId)
        {
            if (Players.ContainsKey(clientId))
            {
                return Players[clientId];
            }

            return null;
        }

        /// <summary>
        /// Find the player in this room by uuid.
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        public MskPlayer FindPlayer(string uuid)
        {
            foreach(MskPlayer p in Players.Values)
            {
                if (p.UUID == uuid)
                {
                    return p;
                }
            }

            return null;
        }

        /// <summary>
        /// Socket client id of the room.
        /// </summary>
        public int RoomClientId { get; set; }

        /// <summary>
        /// Socket client uuid of the room.
        /// </summary>
        public string RoomClientUUID { get; set; }

        /// <summary>
        /// Set current room to private.
        /// </summary>
        /// <param name="isPrivate"></param>
        public void SetPrivate(bool isPrivate)
        {
            if (MasterServerKit.IsClient)
            {
                MasterServerKit.Client.SetPrivate(isPrivate);
            }
            else if (MasterServerKit.IsInstance)
            {
                MasterServerKit.Instance.SetPrivate(isPrivate);
            }
        }

        /// <summary>
        /// Set current room to open.
        /// </summary>
        /// <param name="isOpen"></param>
        public void SetOpen(bool isOpen)
        {
            if (MasterServerKit.IsClient)
            {
                MasterServerKit.Client.SetOpen(isOpen);
            }
            else if (MasterServerKit.IsInstance)
            {
                MasterServerKit.Instance.SetOpen(isOpen);
            }
        }

        /// <summary>
        /// Set current room's password.
        /// </summary>
        /// <param name="password"></param>
        public void SetPassword(string password)
        {
            if (MasterServerKit.IsClient)
            {
                MasterServerKit.Client.SetPassword(password);
            }
            else if (MasterServerKit.IsInstance)
            {
                MasterServerKit.Instance.SetPassword(password);
            }
        }

        /// <summary>
        /// Set current room's maximum players.
        /// </summary>
        /// <param name="maxPlayers"></param>
        public void SetMaxPlayers(int maxPlayers)
        {
            if (MasterServerKit.IsClient)
            {
                MasterServerKit.Client.SetMaxPlayers(maxPlayers);
            }
            else if (MasterServerKit.IsInstance)
            {
                MasterServerKit.Instance.SetMaxPlayers(maxPlayers);
            }
        }

        /// <summary>
        /// Set current room's custom properties.
        /// </summary>
        /// <param name="properties"></param>
        public void SetRoomCustomProperties(MskProperties properties)
        {
            if (MasterServerKit.IsClient)
            {
                MasterServerKit.Client.SetRoomCustomProperties(properties);
            }
            else if (MasterServerKit.IsInstance)
            {
                MasterServerKit.Instance.SetRoomCustomProperties(properties);
            }
        }


        /// <summary>
        /// Deserialize json to set the player's properties. Do not call this method yourself.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static MskRoom DeserializeJson(string json)
        {
            MskRoom room = new MskRoom();
            JObject j = JObject.Parse(json);

            room.Name = j["Name"].Value<string>();
            room.Ip = j["Ip"].Value<string>();
            room.Port = j["Port"].Value<ushort>();
            room.IsPrivate = j["IsPrivate"].Value<bool>();
            room.IsOpen = j["IsOpen"].Value<bool>();
            room.IsPasswordLock = j["IsPasswordLock"].Value<bool>();
            room.MasterId = j["MasterId"].Value<int>();
            room.MaxPlayers = j["MaxPlayers"].Value<int>();
            room.CustomProperties = MskProperties.Deserialize(j["CustomProperties"].Value<string>());
            room.RoomClientId = j["RoomClientId"].Value<int>();
            room.RoomClientUUID = j["RoomClientUUID"].Value<string>();

            JArray players = JArray.Parse(j["Players"].Value<string>());
            foreach(JToken token in players)
            {
                MskPlayer player = MskPlayer.DeserializeJson(token.Value<string>());
                if (MasterServerKit.Client.LocalPlayer?.UUID == player.UUID)
                {
                    room.Players.Add(MasterServerKit.Client.LocalPlayer.ClientId, MasterServerKit.Client.LocalPlayer);
                }
                else
                {
                    room.Players.Add(player.ClientId, player);
                }              
            }

            return room;
        }
    }
}
