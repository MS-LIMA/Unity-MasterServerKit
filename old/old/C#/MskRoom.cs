using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Msk.Master
{
    public class MskRoom : MskRoomBase
    {
        /// <summary>
        /// Players of this room.
        /// </summary>
        public Dictionary<int, MskPlayer> Players { get; set; }

        /// <summary>
        /// Room's socket client.
        /// </summary>
        public MskSocket RoomClient { get; set; }

        /// <summary>
        /// Version of this room.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Password of this room.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Custom property keys for lobby.
        /// </summary>
        public string[] CustomPropertyKeysForLobby { get; set; }

        /// <summary>
        /// Custom properties for lobby.
        /// </summary>
        private MskProperties m_customPropertiesForLobby;

        /// <summary>
        /// Get room info for the lobby.
        /// </summary>
        public RoomInfo RoomInfo
        {
            get
            {
                return new RoomInfo
                {
                    name = Name,
                    ip = Ip,
                    port = Port,
                    isOpen = IsOpen,
                    isPasswordLock = IsPasswordLock,
                    playerCount = Players.Count,
                    maxPlayers = MaxPlayers,
                    customProperties = m_customPropertiesForLobby
                };
            }
        }

        public MskRoom(MskSocket roomClient, string ip, ushort port, string version, string roomName, RoomOptions roomOptions)
        {
            this.RoomClient = roomClient;
            this.Ip = ip;
            this.Port = port;

            this.Version = version;

            this.Name = roomName;
            this.IsPrivate = roomOptions.isPrivate;
            this.IsOpen = roomOptions.isOpen;
            this.Password = roomOptions.password;
            this.IsPasswordLock = !string.IsNullOrEmpty(roomOptions.password);

            this.CustomProperties = roomOptions.customProperties;
            this.CustomPropertyKeysForLobby = roomOptions.customPropertyKeysForLobby;

            this.MasterId = -1;
            this.MaxPlayers = roomOptions.maxPlayers;
            this.Players = new Dictionary<int, MskPlayer>();

            this.m_customPropertiesForLobby = new MskProperties();
            SetCustomPropertiesForLobby(this.CustomProperties);
        }

        private void SetCustomPropertiesForLobby(MskProperties properties)
        {
            foreach (string key in CustomPropertyKeysForLobby)
            {
                if (properties.ContainsKey(key))
                {
                    m_customPropertiesForLobby.Add(key, properties.GetString(key));
                }
            }
        }

        public override string SerializeJson()
        {
            JObject j = new JObject();
            j.Add("Name", Name);
            j.Add("Ip", Ip);
            j.Add("Port", Port);
            j.Add("IsPrivate", IsPrivate);
            j.Add("IsOpen", IsOpen);
            j.Add("IsPasswordLock", IsPasswordLock);
            j.Add("MasterId", MasterId);
            j.Add("MaxPlayers", MaxPlayers);
            j.Add("CustomProperties", CustomProperties.SerializeJson());
            j.Add("RoomClientId", RoomClient.ClientId);
            j.Add("RoomClientUUID", RoomClient.UUID);

            JArray players = new JArray();
            foreach(MskPlayer p in Players.Values)
            {
                players.Add(p.SerializeJson());
            }

            j.Add("Players", players.ToString());

            return j.ToString();
        }

        #region Player Control

        /// <summary>
        /// Add player to the room.
        /// </summary>
        /// <param name="player"></param>
        public void AddPlayer(MskPlayer player)
        {
            // Send packets to clients.
            using (Packet packet = new Packet((int)OpResponse.OnPlayerJoined))
            {
                packet.Write(player.SerializeJson());

                PacketSender.SendPacketToClient(RoomClient, packet);
                PacketSender.SendPacketToClients(Players.Values.Select(x => x.Client).ToArray(), packet);
            }

            this.Players.Add(player.ClientId, player);

            // If the player count is only one, then
            if (this.Players.Count <= 1)
            {
                // Set this player to master.
                this.MasterId = player.ClientId;

                // Send packet to room.
                using (Packet packet = new Packet((int)OpResponse.OnMasterChanged))
                {
                    packet.Write(MasterId);
                    PacketSender.SendPacketToClient(RoomClient, packet);
                }
            }
        }

        /// <summary>
        /// Remove player from the room.
        /// </summary>
        /// <param name="clientId"></param>
        public void RemovePlayer(int clientId)
        {
            if (Players.ContainsKey(clientId))
            {
                MskPlayer player = this.Players[clientId];
                this.Players.Remove(clientId);

                using (Packet packet = new Packet((int)OpResponse.OnPlayerLeft))
                {
                    packet.Write(clientId);

                    PacketSender.SendPacketToClient(RoomClient, packet);
                    PacketSender.SendPacketToClients(Players.Values.Select(x => x.Client).ToArray(), packet);
                }

                if (player.ClientId == this.MasterId)
                {
                    if (this.Players.Count > 0)
                    {
                        MskPlayer nextMaster = Players.Values.ToArray()[0];
                        SetMaster(nextMaster);
                    }
                }
            }
        }

        /// <summary>
        /// Set the master client of the room.
        /// </summary>
        /// <param name="player"></param>
        public void SetMaster(MskPlayer player)
        {
            this.MasterId = player.ClientId;

            // Send packet to clients
            using (Packet packet = new Packet((int)OpResponse.OnMasterChanged))
            {
                packet.Write(MasterId);

                MskSocket[] clients = Players.Values.Select(x => x.Client).ToArray();

                PacketSender.SendPacketToClient(RoomClient, packet);
                PacketSender.SendPacketToClients(clients, packet);
            }
        }

        /// <summary>
        /// Set the master client of the room.
        /// </summary>
        /// <param name="player"></param>
        public void SetMaster(int clientId)
        {
            if (Players.ContainsKey(clientId))
            {
                MskPlayer newMaster = Players[clientId];
                SetMaster(newMaster);
            }
        }

        /// <summary>
        /// Kick player from the room.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="reason"></param>
        public void KickPlayer(int clientId, string reason)
        {
            if (Players.ContainsKey(clientId))
            {
                using (Packet packet = new Packet((int)OpResponse.OnPlayerKicked))
                {
                    packet.Write(clientId);
                    packet.Write(reason);

                    PacketSender.SendPacketToClient(RoomClient, packet);
                    PacketSender.SendPacketToClients(Players.Values.Select(x => x.Client).ToArray(), packet);
                }

                RemovePlayer(clientId);
            }
        }

        #endregion

        #region Room Properties

        /// <summary>
        /// Update room's properties.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="op"></param>
        public void UpdateRoomProperties(object data, OpRoomProperties op)
        {
            if (op == OpRoomProperties.ChangePrivate)
            {
                ChangePrivate((bool)data, op);
            }
            else if (op == OpRoomProperties.ChangeMaxPlayers)
            {
                ChangeMaxPlayers((int)data, op);
            }
            else if (op == OpRoomProperties.ChangeOpen)
            {
                ChangeOpen((bool)data, op);
            }
            else if (op == OpRoomProperties.ChangePassword)
            {
                ChangePassword((string)data, op);
            }
            else if (op == OpRoomProperties.UpdateCustomProperties)
            {
                UpdateRoomProperties((string)data, op);
            }
        }

        private void ChangePrivate(bool isPrivate, OpRoomProperties op)
        {
            this.IsPrivate = IsPrivate;

            using (Packet packet = new Packet((int)OpResponse.OnRoomPropertiesUpdated))
            {
                packet.Write((int)op);
                packet.Write(isPrivate);

                PacketSender.SendPacketToClient(RoomClient, packet);
                PacketSender.SendPacketToClients(Players.Values.Select(x => x.Client).ToArray(), packet);
            }
        }

        private void ChangeMaxPlayers(int maxPlayers, OpRoomProperties op)
        {
            if (maxPlayers < Players.Count)
            {
                return;
            }

            this.MaxPlayers = maxPlayers;

            using (Packet packet = new Packet((int)OpResponse.OnRoomPropertiesUpdated))
            {
                packet.Write((int)op);
                packet.Write(maxPlayers);

                PacketSender.SendPacketToClient(RoomClient, packet);
                PacketSender.SendPacketToClients(Players.Values.Select(x => x.Client).ToArray(), packet);
            }
        }

        private void ChangeOpen(bool isOpen, OpRoomProperties op)
        {
            this.IsOpen = isOpen;

            using (Packet packet = new Packet((int)OpResponse.OnRoomPropertiesUpdated))
            {
                packet.Write((int)op);
                packet.Write(isOpen);

                PacketSender.SendPacketToClient(RoomClient, packet);
                PacketSender.SendPacketToClients(Players.Values.Select(x => x.Client).ToArray(), packet);
            }
        }

        private void ChangePassword(string password, OpRoomProperties op)
        {
            this.Password = password;
            this.IsPasswordLock = !string.IsNullOrEmpty(password);

            using (Packet packet = new Packet((int)OpResponse.OnRoomPropertiesUpdated))
            {
                packet.Write((int)op);
                packet.Write(this.IsPasswordLock);

                PacketSender.SendPacketToClient(RoomClient, packet);
                PacketSender.SendPacketToClients(Players.Values.Select(x => x.Client).ToArray(), packet);
            }
        }

        private void UpdateRoomProperties(string props, OpRoomProperties op)
        {
            MskProperties updated = MskProperties.Deserialize(props);
            this.CustomProperties.Append(updated);
            SetCustomPropertiesForLobby(updated);

            using (Packet packet = new Packet((int)OpResponse.OnRoomPropertiesUpdated))
            {
                packet.Write((int)op);
                packet.Write(props);

                PacketSender.SendPacketToClient(RoomClient, packet);
                PacketSender.SendPacketToClients(Players.Values.Select(x => x.Client).ToArray(), packet);
            }
        }

        #endregion

        #region Player Properties

        /// <summary>
        /// Update player's nickname.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="nickname"></param>
        public void UpdatePlayerNickname(int clientId, string nickname, string prevNickname)
        {
            MskSocket[] clients = Players.Values.Where(x => x.ClientId != clientId).Select(x => x.Client).ToArray();

            using (Packet packet = new Packet((int)OpResponse.OnNicknameUpdated))
            {
                packet.Write(clientId);
                packet.Write(nickname);
                packet.Write(prevNickname);

                PacketSender.SendPacketToClient(RoomClient, packet);
                PacketSender.SendPacketToClients(clients, packet);
            }
        }

        /// <summary>
        /// Update player's custom properties.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="props"></param>
        public void UpdatePlayerCustomProperties(int clientId, string props)
        {
            MskSocket[] clients = Players.Values.Where(x => x.ClientId != clientId).Select(x => x.Client).ToArray();

            using (Packet packet = new Packet((int)OpResponse.OnPlayerCustomPropertiesUpdated))
            {
                packet.Write(clientId);
                packet.Write(props);

                PacketSender.SendPacketToClient(RoomClient, packet);
                PacketSender.SendPacketToClients(clients, packet);
            }
        }

        #endregion
    }
}