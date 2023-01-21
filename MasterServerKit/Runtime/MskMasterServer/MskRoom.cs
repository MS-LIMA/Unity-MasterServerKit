using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace MasterServerKit.Master
{
    public class MskRoom
    {
        // Json Properties
        public string Name { get; set; }
        public string Ip { get; set; }
        public ushort Port { get; set; }

        public bool IsPrivate { get; set; }
        public bool IsOpen { get; set; }
        public bool IsPasswordLock { get; set; }

        public int MasterId { get; set; }
        public int MaxPlayers { get; set; }
        public Dictionary<int, MskPlayer> Players { get; set; }

        public MskProperties CustomProperties { get; set; }


        // Non Json Properties
        [JsonIgnore]
        public MskClient RoomClient { get; set; }

        [JsonIgnore]
        public string Version { get; set; }

        [JsonIgnore]
        public string Password { get; set; }

        [JsonIgnore]
        public List<string> CustomPropertyKeysForLobby { get; set; }

        [JsonIgnore]
        public RoomInfo roomInfo;


        // Constructor
        public MskRoom(MskClient roomClient, string ip, ushort port, string version, string roomName, RoomOptions roomOptions)
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

            this.roomInfo = new RoomInfo
            {
                RoomName = this.Name,
                IsOpen = this.IsOpen,
                IsPasswordLock = this.IsPasswordLock,
                MaxPlayers = this.MaxPlayers,
                PlayerCount = this.Players.Count,
                CustomPropertiesInLobby = new MskProperties()
            };

            SetCustomPropertiesInLobby(this.CustomProperties);
        }

        private void SetCustomPropertiesInLobby(MskProperties properties)
        {
            foreach (string key in CustomPropertyKeysForLobby)
            {
                if (properties.ContainsKey(key))
                {
                    roomInfo.CustomPropertiesInLobby.Add(key, properties.GetString(key));
                }
            }
        }

        #region Player Control

        // Add Player
        public void AddPlayer(MskPlayer player)
        {
            // Send packets to clients.
            using (Packet packet = new Packet((int)OpResponse.OnPlayerJoined))
            {
                packet.Write(JsonSerializer.ToJson(player));

                PacketSender.SendPacketToClient(RoomClient, packet);
                PacketSender.SendPacketToClients(Players.Values.Select(x => x.Client).ToArray(), packet);
            }

            this.Players.Add(player.Id, player);
            this.roomInfo.PlayerCount = this.Players.Count;

            if (this.Players.Count <= 1)
            {
                SetMaster(player, false);
            }
        }

        // Remove Player
        public void RemovePlayer(int clientId)
        {
            MskPlayer player = this.Players[clientId];
            this.Players.Remove(clientId);

            using (Packet packet = new Packet((int)OpResponse.OnPlayerLeft))
            {
                packet.Write(clientId);

                PacketSender.SendPacketToClient(RoomClient, packet);
                PacketSender.SendPacketToClients(Players.Values.Select(x => x.Client).ToArray(), packet);
            }

            if (player.Id == this.MasterId)
            {
                if (this.Players.Count > 0)
                {
                    MskPlayer nextMaster = Players.Values.ToArray()[0];
                    SetMaster(nextMaster);
                }
            }
        }

        // Set Master
        public void SetMaster(MskPlayer player, bool sendToMaster = true)
        {
            this.MasterId = player.Id;
            
            // Send packet to clients
            using (Packet packet = new Packet((int)OpResponse.OnMasterChanged))
            {
                packet.Write(MasterId);

                MskClient[] clients = sendToMaster ?
                    Players.Values.Select(x => x.Client).ToArray() : Players.Values.Where(x => x.Id != this.MasterId).Select(x => x.Client).ToArray();
               
                PacketSender.SendPacketToClient(RoomClient, packet);
                PacketSender.SendPacketToClients(clients, packet);
            }
        }

        public void SetMaster(int clientId)
        {
            if (Players.ContainsKey(clientId))
            {
                MskPlayer newMaster = Players[clientId];
                SetMaster(newMaster);
            }

        }

        #endregion

        #region Room Properties
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
            this.roomInfo.MaxPlayers = maxPlayers;

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
            this.roomInfo.IsOpen = isOpen;

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
            this.roomInfo.IsPasswordLock = !string.IsNullOrEmpty(password);

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
            SetCustomPropertiesInLobby(updated);

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
        public void UpdatePlayerNickname(int clientId, string nickname)
        {
            MskClient[] clients = Players.Values.Where(x => x.Id != clientId).Select(x => x.Client).ToArray();

            using (Packet packet = new Packet((int)OpResponse.OnNicknameUpdated))
            {
                packet.Write(clientId);
                packet.Write(nickname);

                PacketSender.SendPacketToClient(RoomClient, packet);
                PacketSender.SendPacketToClients(clients, packet);
            }
        }

        public void UpdatePlayerCustomProperties(int clientId, string props)
        {
            MskClient[] clients = Players.Values.Where(x => x.Id != clientId).Select(x => x.Client).ToArray();

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