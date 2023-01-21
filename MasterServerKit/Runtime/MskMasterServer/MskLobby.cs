using System.Collections.Generic;
using System;
using System.Linq;

namespace MasterServerKit.Master
{
    public class MskLobby
    {
        private static Dictionary<string, Lobby> lobbies = new Dictionary<string, Lobby>();

        private static Dictionary<MskClient, string> playerClients = new Dictionary<MskClient, string>();
        private static Dictionary<MskClient, string> roomClients = new Dictionary<MskClient, string>();

        private static Dictionary<MskClient, string> clients = new Dictionary<MskClient, string>();

        #region Lobby

        public class Lobby
        {
            public string Version { get; private set; }

            public Dictionary<int, MskPlayer> Players { get; private set; }
            public Dictionary<string, MskRoom> Rooms { get; private set; }

            public HashSet<MskClient> Clients { get; private set; }

            public Queue<int> RoomNumbers { get; private set; }

            public Lobby(string version)
            {
                this.Version = version;

                this.Players = new Dictionary<int, MskPlayer>();
                this.Rooms = new Dictionary<string, MskRoom>();

                this.Clients = new HashSet<MskClient>();
                this.RoomNumbers = new Queue<int>();
                for (int i = 0; i < MskConfig.Instance.roomNumbersPerLobby; i++)
                {
                    this.RoomNumbers.Enqueue(i);
                }
            }
        }

        private static Lobby FindLobby(string version)
        {
            if (lobbies.ContainsKey(version))
            {
                return lobbies[version];
            }

            return null;
        }

        private static Lobby CreateOrFindLobby(string version)
        {
            if (lobbies.ContainsKey(version))
            {
                return lobbies[version];
            }

            Lobby lobby = new Lobby(version);
            lobbies.Add(version, lobby);

            Console.WriteLine($"-> Lobby created : version {version}");

            return lobby;
        }

        private static void RemoveLobby(string version)
        {
            if (lobbies.ContainsKey(version))
            {
                Console.WriteLine($"-> Lobby removed : version {version}");
                lobbies.Remove(version);
            }
        }

        public static bool IsLobbyExist(string version)
        {
            return FindLobby(version) != null;
        }

        private static MskRoom FindRoom(string version, string roomName)
        {
            Lobby lobby = FindLobby(version);
            if (lobby != null)
            {
                if (lobby.Rooms.ContainsKey(roomName))
                {
                    return lobby.Rooms[roomName];
                }
            }

            return null;
        }

        #endregion


        #region Client Connection Control
        // Client
        public static void AddClient(MskClient client, string version)
        {
            clients.Add(client, version);
        }

        public static void AddClientToLobby(MskClient client, string version)
        {
            Lobby lobby = CreateOrFindLobby(version);
            lobby.Clients.Add(client);
        }

        public static void RemoveClient(MskClient client)
        {
            string version = clients[client];

            Lobby lobby = FindLobby(version);
            if (lobby != null)
            {
                lobby.Clients.Remove(client);
                if (lobby.Clients.Count <= 0)
                {
                    RemoveLobby(version);
                }
            }

            clients.Remove(client);
        }

        public static bool ContainsClient(MskClient client)
        {
            return clients.ContainsKey(client);
        }


        // Player
        public static void AddPlayerClient(MskClient client, string version)
        {
            playerClients.Add(client, version);
        }

        public static void AddPlayerToLobby(MskPlayer player, string version)
        {
            Lobby lobby = CreateOrFindLobby(version);
            lobby.Players.Add(player.Id, player);
        }

        public static void RemovePlayerClient(MskClient client)
        {
            MskPlayer player = FindPlayer(client);
            if (player != null)
            {
                if (player.InRoom)
                {
                    MskRoom room = FindRoom(player.Version, player.RoomName);
                    if (room != null)
                    {
                        room.RemovePlayer(player.Id);
                    }
                }

                Lobby lobby = FindLobby(player.Version);
                lobby.Players.Remove(player.Id);

                playerClients.Remove(client);
            }
        }

        public static bool IsPlayerClient(MskClient client)
        {
            return playerClients.ContainsKey(client);
        }


        // Room
        public static void AddRoomClient(MskClient client, string version)
        {
            roomClients.Add(client, version);
        }

        public static void RemoveRoomClient(MskClient client)
        {
            MskRoom room = FindRoom(client);
            if (room != null)
            {
                Lobby lobby = FindLobby(room.Version);
                if (lobby != null)
                {
                    lobby.Rooms.Remove(room.Name);

                    int roomNumber = -1;
                    if (int.TryParse(room.Name, out roomNumber))
                    {
                        lobby.RoomNumbers.Enqueue(roomNumber);
                    }

                    // Send room infos to players only in lobby.
                    SendRoomInfoToClients(room.Version);
                }

                roomClients.Remove(client);
            }
        }

        public static bool IsRoomClient(MskClient client)
        {
            return roomClients.ContainsKey(client);
        }


        // Find Player
        public static MskPlayer FindPlayer(MskClient client)
        {
            if (playerClients.ContainsKey(client))
            {
                string version = playerClients[client];
                Lobby lobby = FindLobby(version);
                if (lobby != null)
                {
                    if (lobby.Players.ContainsKey(client.clientId))
                    {
                        return lobby.Players[client.clientId];
                    }
                }
            }

            return null;
        }

        public static MskRoom FindRoom(MskClient client)
        {
            if (roomClients.ContainsKey(client))
            {
                string version = roomClients[client];
                Lobby lobby = FindLobby(version);
                if (lobby != null)
                {
                    foreach (MskRoom room in lobby.Rooms.Values)
                    {
                        if (room.RoomClient.clientId == client.clientId)
                        {
                            return room;
                        }
                    }
                }
            }

            return null;
        }

        #endregion

        #region Create Room

        public static void CreateRoom(MskClient roomClient, string version, string roomName, string roomOptions, string ip, ushort port)
        {
            Lobby lobby = FindLobby(version);

            using (RoomOptions options = JsonSerializer.FromJson<RoomOptions>(roomOptions))
            {
                MskRoom room = new MskRoom(roomClient, ip, port, version, roomName, options);
                lobby.Rooms.Add(roomName, room);

                // Send room infos to players only in lobby.
                SendRoomInfoToClients(version);
            }
        }

        public static bool IsRoomNumberMaxReached(string version)
        {
            Lobby lobby = FindLobby(version);
            return lobby.RoomNumbers.Count <= 0;
        }

        public static int GetRoomNumberInternal(string version)
        {
            Lobby lobby = FindLobby(version);
            if (lobby.RoomNumbers.Count > 0)
            {
                return lobby.RoomNumbers.Dequeue();
            }

            return -1;
        }

        public static bool IsRoomNameDuplicated(string version, string roomName)
        {
            Lobby lobby = FindLobby(version);
            return lobby.Rooms.ContainsKey(roomName);
        }


        #endregion

        #region Join Room

        public static void JoinRoom(MskClient client, string version, string roomName, string password, Action<bool, MskRoom, OpError> callback = null)
        {
            Lobby lobby = FindLobby(version);

            // If room name is empty or null, then
            if (string.IsNullOrEmpty(roomName))
            {
                // Join random room.
                JoinRandomRoom(lobby, client, callback);
            }
            else
            {
                MskRoom room = FindRoom(version, roomName);

                // Check room exists.
                if (room == null)
                {
                    callback?.Invoke(false, null, OpError.RoomNotFound);
                    return;
                }

                // Check room is joinable.
                CheckRoomJoinable(room, password, (success, opError) =>
                {
                    // If room is joinable, then
                    if (success)
                    {
                        // Join room.
                        JoinRoomInternal(lobby, room, client);
                    }

                    callback?.Invoke(success, success ? room : null, opError);
                });
            }
        }

        private static void JoinRandomRoom(Lobby lobby, MskClient client, Action<bool, MskRoom, OpError> callback = null)
        {
            // Loop for finding joinable rooms.
            foreach (MskRoom r in lobby.Rooms.Values)
            {
                if (r.IsPrivate)
                {
                    continue;
                }

                CheckRoomJoinable(r, "", (success, opError) =>
                {
                    // If room is joinable, then
                    if (success)
                    {
                        // Join room.
                        JoinRoomInternal(lobby, r, client);
                        callback?.Invoke(true, r, OpError.Success);
                        return;
                    }
                });
            }

            callback?.Invoke(false, null, OpError.RoomNotFound);
        }

        private static void JoinRoomInternal(Lobby lobby, MskRoom room, MskClient client)
        {
            if (lobby.Players.ContainsKey(client.clientId))
            {
                MskPlayer player = lobby.Players[client.clientId];
                player.RoomName = room.Name;

                room.AddPlayer(player);

                // Send room infos to players only in lobby.
                SendRoomInfoToClients(room.Version);
            }
        }

        private static void CheckRoomJoinable(MskRoom room, string password, Action<bool, OpError> callback = null)
        {
            // Check current players count and max players.
            if (room.Players.Count >= room.MaxPlayers)
            {
                callback?.Invoke(false, OpError.RoomIsFull);
                return;
            }

            // Check room has password.
            if (room.IsPasswordLock)
            {
                if (room.Password != password)
                {
                    callback?.Invoke(false, OpError.IncorrectPassword);
                    return;
                }
            }

            // Check room is open.
            if (!room.IsOpen)
            {
                callback?.Invoke(false, OpError.RoomIsClosed);
                return;
            }

            callback?.Invoke(true, OpError.Success);
        }

        #endregion

        #region Leave Room

        public static void LeaveRoom(MskClient client, string version, string roomName)
        {
            MskPlayer player = FindPlayer(client);
            if (player != null)
            {
                MskRoom room = FindRoom(version, roomName);
                if (room != null)
                {
                    if (room.Players.ContainsKey(player.Id))
                    {
                        room.Players.Remove(player.Id);

                        // Send room infos to players only in lobby.
                        SendRoomInfoToClients(version);
                    }
                }

                player.RoomName = "";
            }
        }

        #endregion

        #region Update Room Properties
        // Update Room Properties
        public static void UpdateRoomProperties(string version, string roomName, object data, OpRoomProperties op)
        {
            MskRoom room = FindRoom(version, roomName);
            if (room != null)
            {
                room.UpdateRoomProperties(data, op);

                // Send room infos to players only in lobby.
                SendRoomInfoToClients(version);
            }
        }

        public static void SetMaster(string version, string roomName, int clientId)
        {
            MskRoom room = FindRoom(version, roomName);
            if (room != null)
            {
                room.SetMaster(clientId);
            }
        }

        // Update Player Properties In Room
        public static void UpdateNicknameInRoom(string version, string roomName, int clientId, string nickname)
        {
            MskRoom room = FindRoom(version, roomName);
            if (room != null)
            {
                room.UpdatePlayerNickname(clientId, nickname);
            }
        }

        public static void UpdatePlayerCustomPropertiesInRoom(string version, string roomName, int clientId, string properties)
        {
            MskRoom room = FindRoom(version, roomName);
            if (room != null)
            {
                room.UpdatePlayerCustomProperties(clientId, properties);
            }
        }

        #endregion

        #region Lobby Info

        public static void GetPlayerCountInLobby(string version, Action<int> callback = null)
        {
            Lobby lobby = FindLobby(version);
            if (lobby != null)
            {
                callback?.Invoke(lobby.Players.Count);
            }
        }

        public static void GetPlayerListInLobby(string version, Action<PlayerInfo[]> callback = null)
        {
            Lobby lobby = FindLobby(version);
            if (lobby != null)
            {
                PlayerInfo[] playerInfos = lobby.Players.Values.Select(x => x.playerInfo).ToArray();

                callback?.Invoke(playerInfos);
            }
        }

        public static void GetRoomListInLobby(string version, Action<RoomInfo[]> callback = null)
        {
            Lobby lobby = FindLobby(version);
            if (lobby != null)
            {
                RoomInfo[] roomInfos = lobby.Rooms.Values.Where(x => !x.IsPrivate).Select(x => x.roomInfo).ToArray();

                callback?.Invoke(roomInfos);
            }
        }

        private static void SendRoomInfoToClients(string version)
        {
            Lobby lobby = FindLobby(version);
            if (lobby != null)
            {
                MskClient[] playerClients = lobby.Players.Values.Where(x => !x.InRoom).Select(x => x.Client).ToArray();
                if (playerClients.Length > 0)
                {
                    RoomInfo[] roomInfos = lobby.Rooms.Values.Where(x=>!x.IsPrivate).Select(x => x.roomInfo).ToArray();
                    string json = JsonSerializer.ToJson(roomInfos);

                    using (Packet packet = new Packet((int)OpResponse.OnRoomListGet))
                    {
                        packet.Write(json);
                        PacketSender.SendPacketToClients(playerClients, packet);
                    }
                }
            }
        }

        #endregion
    }

}
