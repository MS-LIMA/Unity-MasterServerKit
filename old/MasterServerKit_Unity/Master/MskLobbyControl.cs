using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Msk.Master
{
    public class MskLobbyControl
    {
        /// <summary>
        /// Lobbies in this master server.
        /// </summary>
        public static Dictionary<string, MskLobby> Lobbies { get; private set; } = new Dictionary<string, MskLobby>();

        /// <summary>
        /// Player's socket client of this master server.
        /// </summary>
        public static HashSet<MskSocket> PlayerClients { get; private set; } = new HashSet<MskSocket>();
        
        /// <summary>
        /// Room's socket client of this master server.
        /// </summary>
        public static HashSet<MskSocket> RoomClients { get; private set; } = new HashSet<MskSocket>();

        /// <summary>
        /// Clients of this master server.
        /// </summary>
        public static HashSet<MskSocket> Clients { get; private set; } = new HashSet<MskSocket>();


        #region Lobby Control

        /// <summary>
        /// Does this version of the lobby exist?
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public static bool IsLobbyExists(string version)
        {
            return Lobbies.ContainsKey(version);
        }

        /// <summary>
        /// Find the lobby by the version.
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public static MskLobby FindLobby(string version)
        {
            if (Lobbies.ContainsKey(version))
            {
                return Lobbies[version];
            }

            return null;
        }

        /// <summary>
        /// Create or find the lobby by the version.
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        private static MskLobby CreateOrFindLobby(string version)
        {
            if (Lobbies.ContainsKey(version))
            {
                return Lobbies[version];
            }

            MskLobby lobby = new MskLobby(version);
            Lobbies.Add(version, lobby);

            Console.WriteLine($"-> Lobby created : version {version}");

            return lobby;
        }

        /// <summary>
        /// Remove the lobby by the version.
        /// </summary>
        /// <param name="version"></param>
        private static void RemoveLobby(string version)
        {
            if (Lobbies.ContainsKey(version))
            {
                Console.WriteLine($"-> Lobby removed : version {version}");
                Lobbies.Remove(version);
            }
        }

        /// <summary>
        /// Find the room by version and room name.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="roomName"></param>
        /// <returns></returns>
        public static MskRoom FindRoom(string version, string roomName)
        {
            MskLobby lobby = FindLobby(version);
            if (lobby != null)
            {
                if (lobby.Rooms.ContainsKey(roomName))
                {
                    return lobby.Rooms[roomName];
                }
            }

            return null;
        }

        /// <summary>
        /// Find the player in the master server.
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static MskPlayer FindPlayer(MskSocket client)
        {
            MskLobby lobby = FindLobby(client.Version);
            if (lobby != null)
            {
                if (lobby.Players.Contains(client.ClientId))
                {
                    return lobby.Players.Get(client.ClientId);
                }
            }

            return null;
        }

        /// <summary>
        /// Find the room in the master server.
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static MskRoom FindRoom(MskSocket client)
        {
            MskLobby lobby = FindLobby(client.Version);
            if (lobby != null)
            {
                foreach (MskRoom room in lobby.Rooms.Values)
                {
                    if (room.RoomClient.ClientId == client.ClientId)
                    {
                        return room;
                    }
                }
            }

            return null;
        }

        #endregion

        #region Client Connection Control

        /// <summary>
        /// Does master server contain the client? 
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static bool ContainsClient(MskSocket client)
        {
            return Clients.Contains(client);
        }

        /// <summary>
        /// Is this client is room client?
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static bool IsRoomClient(MskSocket client)
        {
            return RoomClients.Contains(client);
        }

        /// <summary>
        /// Is this client is player client?
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static bool IsPlayerClient(MskSocket client)
        {
            return PlayerClients.Contains(client);
        }

        /// <summary>
        /// Add the client to this master server.
        /// It will automatically check whether this client is player or room,
        /// and add to the lobby.
        /// </summary>
        /// <param name="client"></param>
        public static void AddClient(MskSocket client)
        {
            if (!Clients.Contains(client))
            {
                Clients.Add(client);
            }
        }

        /// <summary>
        /// Remove the client from this master server.
        /// It will automatically check whether this client is player or room,
        /// and remove from the lobby.
        /// </summary>
        /// <param name="client"></param>
        public static void RemoveClient(MskSocket client)
        {
            Clients.Remove(client);

            if (IsPlayerClient(client))
            {
                RemovePlayerClient(client);
            }
            else if (IsRoomClient(client))
            {
                RemoveRoomClient(client);
            }
        }

        // Player
        //.........................................................................

        /// <summary>
        /// Add Player to a lobby.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="version"></param>
        public static void AddPlayerToLobby(MskPlayer player, string version)
        {
            MskLobby lobby = CreateOrFindLobby(version);
            lobby?.AddPlayer(player);
        }

        /// <summary>
        /// Add player client to the lobby control and lobby.
        /// </summary>
        /// <param name="client"></param>
        public static void AddPlayerClient(MskSocket client)
        {
            if (!PlayerClients.Contains(client))
            {
                PlayerClients.Add(client);
            }

            MskLobby lobby = CreateOrFindLobby(client.Version);
            if (!lobby.Clients.ContainsKey(client.ClientId))
            {
                lobby.Clients.Add(client.ClientId, client);
            }
        }

        /// <summary>
        /// Remove player client from the lobby control and lobby.
        /// </summary>
        /// <param name="client"></param>
        private static void RemovePlayerClient(MskSocket client)
        {
            MskPlayer player = FindPlayer(client);
            if (player != null)
            {
                if (player.InRoom)
                {
                    MskRoom room = FindRoom(player.Version, player.RoomName);
                    if (room != null)
                    {
                        room.RemovePlayer(player.ClientId);
                    }
                }

                MskLobby lobby = FindLobby(player.Version);
                if (lobby != null)
                {
                    lobby.RemovePlayer(client.ClientId);
                    lobby.Clients.Remove(player.ClientId);

                    if (lobby.Clients.Count <= 0)
                    {
                        RemoveLobby(lobby.Version);
                    }
                }
            }

            PlayerClients.Remove(client);
        }


        // Room
        //.........................................................................

        /// <summary>
        /// Add room client to the lobby control and lobby.
        /// </summary>
        /// <param name="roomClient"></param>
        public static void AddRoomClient(MskSocket roomClient)
        {
            if (!RoomClients.Contains(roomClient))
            {
                RoomClients.Add(roomClient);
            }

            MskLobby lobby = CreateOrFindLobby(roomClient.Version);
            if (!lobby.Clients.ContainsKey(roomClient.ClientId))
            {
                lobby.Clients.Add(roomClient.ClientId, roomClient);
            }
        }

        /// <summary>
        /// Remove room client from the lobby control and lobby.
        /// </summary>
        /// <param name="roomClient"></param>
        private static void RemoveRoomClient(MskSocket roomClient)
        {
            MskRoom room = FindRoom(roomClient);
            if (room != null)
            {
                MskLobby lobby = FindLobby(room.Version);
                if (lobby != null)
                {
                    lobby.RemoveRoom(room);
                    lobby.Clients.Remove(room.RoomClient.ClientId);

                    if (lobby.Clients.Count <= 0)
                    {
                        RemoveLobby(lobby.Version);
                    }
                }
            }

            RoomClients.Remove(roomClient);
        }

        #endregion

        #region Create Room

        /// <summary>
        /// Create room in the lobby.
        /// </summary>
        /// <param name="roomClient"></param>
        /// <param name="version"></param>
        /// <param name="roomName"></param>
        /// <param name="roomOptions"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public static void CreateRoom(MskSocket roomClient, string version, string roomName, string roomOptions, string ip, ushort port)
        {
            MskLobby lobby = FindLobby(version);

            if (lobby != null)
            {
                using (RoomOptions options = Utilities.FromJson<RoomOptions>(roomOptions))
                {
                    MskRoom room = new MskRoom(roomClient, ip, port, version, roomName, options);
                    lobby.AddRoom(room);
                }
            }
        }

        #endregion

        #region Join Room

        /// <summary>
        /// Try to join the room.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="version"></param>
        /// <param name="roomName"></param>
        /// <param name="password"></param>
        /// <param name="callback"></param>
        public static void JoinRoom(MskSocket client, string version, string roomName, string password, Action<bool, MskRoom, OpError> callback = null)
        {
            MskLobby lobby = FindLobby(version);

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

        private static void JoinRandomRoom(MskLobby lobby, MskSocket client, Action<bool, MskRoom, OpError> callback = null)
        {
            MskRoom[] rooms = Utilities.Shuffle(lobby.Rooms.Values.ToArray());
            bool isSuccess = false;

            // Loop for finding joinable rooms.
            foreach (MskRoom r in rooms)
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

                        isSuccess = true;
                    }
                });
            }

            if (!isSuccess)
            {
                callback?.Invoke(false, null, OpError.RoomNotFound);
            }
        }

        private static void JoinRoomInternal(MskLobby lobby, MskRoom room, MskSocket client)
        {
            if (lobby.Players.Contains(client.ClientId))
            {
                MskPlayer player = lobby.Players.Get(client.ClientId);
                player.RoomName = room.Name;

                room.AddPlayer(player);
                lobby.NotifyRoomListChanged();
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

        /// <summary>
        /// Remove player in the room.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="version"></param>
        /// <param name="roomName"></param>
        public static void LeaveRoom(MskSocket client, string version, string roomName)
        {
            MskPlayer player = FindPlayer(client);
            if (player != null)
            {
                MskRoom room = FindRoom(version, roomName);
                if (room != null)
                {
                    room.RemovePlayer(client.ClientId);

                    MskLobby lobby = FindLobby(version);
                    lobby?.NotifyRoomListChanged();
                }

                player.RoomName = "";
            }
        }

        #endregion

        #region Update Room Properties

        /// <summary>
        /// Update room's properties.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="roomName"></param>
        /// <param name="data"></param>
        /// <param name="op"></param>
        public static void UpdateRoomProperties(string version, string roomName, object data, OpRoomProperties op)
        {
            MskRoom room = FindRoom(version, roomName);
            if (room != null)
            {
                room.UpdateRoomProperties(data, op);

                MskLobby lobby = FindLobby(version);
                lobby?.NotifyRoomListChanged();
            }
        }

        /// <summary>
        /// Set room's new master client.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="roomName"></param>
        /// <param name="clientId"></param>
        public static void SetMaster(string version, string roomName, int clientId)
        {
            MskRoom room = FindRoom(version, roomName);
            if (room != null)
            {
                room.SetMaster(clientId);
            }
        }

        /// <summary>
        /// Kick a player from the room.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="roomName"></param>
        /// <param name="playerId"></param>
        /// <param name="reason"></param>
        public static void KickPlayer(string version, string roomName, int playerId, string reason)
        {
            MskRoom room = FindRoom(version, roomName);
            if (room != null)
            {
                room.KickPlayer(playerId, reason);
            }
        }

        /// <summary>
        /// Notify player's nickname in a room.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="roomName"></param>
        /// <param name="clientId"></param>
        /// <param name="nickname"></param>
        public static void NotifyNicknameChangedInRoom(string version, string roomName, int clientId, string nickname, string prevNickname)
        {
            MskRoom room = FindRoom(version, roomName);
            if (room != null)
            {
                room.UpdatePlayerNickname(clientId, nickname, prevNickname);
            }
        }

        /// <summary>
        /// Notify player's custom properties changed in a room.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="roomName"></param>
        /// <param name="clientId"></param>
        /// <param name="properties"></param>
        public static void NotifyPlayerCustomPropertiesChangedInRoom(string version, string roomName, int clientId, string properties)
        {
            MskRoom room = FindRoom(version, roomName);
            if (room != null)
            {
                room.UpdatePlayerCustomProperties(clientId, properties);
            }
        }

        #endregion

        #region Lobby Info

        /// <summary>
        /// Get players count in the lobby who are not in a room.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="callback"></param>
        public static void GetPlayerCount(string version, Action<int> callback = null)
        {
            MskLobby lobby = FindLobby(version);
            if (lobby != null)
            {
                callback?.Invoke(lobby.Players.ToArray().Select(x=>!x.InRoom).Count());
            }
        }

        /// <summary>
        /// Get all players count in the lobby including in a room.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="callback"></param>
        public static void GetPlayerCountInLobby(string version, Action<int> callback = null)
        {
            MskLobby lobby = FindLobby(version);
            if (lobby != null)
            {
                callback?.Invoke(lobby.Players.Count);
            }
        }

        /// <summary>
        /// Get all player list in the lobby.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="callback"></param>
        public static void GetPlayerListInLobby(string version, Action<PlayerInfo[]> callback = null)
        {
            MskLobby lobby = FindLobby(version);
            if (lobby != null)
            {
                PlayerInfo[] playerInfos = lobby.Players.ToArray().Select(x => x.PlayerInfo).ToArray();

                callback?.Invoke(playerInfos);
            }
        }

        /// <summary>
        /// Get all room list in the lobby.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="callback"></param>
        public static void GetRoomListInLobby(string version, Action<RoomInfo[]> callback = null)
        {
            MskLobby lobby = FindLobby(version);
            if (lobby != null)
            {
                RoomInfo[] roomInfos = lobby.Rooms.Values.Where(x => !x.IsPrivate).Select(x => x.RoomInfo).ToArray();

                callback?.Invoke(roomInfos);
            }
        }

        /// <summary>
        /// Send message to specific client.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="targetUUID"></param>
        /// <param name="message"></param>
        /// <param name="callback"></param>
        public static void SendMessage(MskSocket client, string version, string targetUUID, string message, 
            Action<bool, OpError> callback = null)
        {
            MskLobby lobby = FindLobby(version);
            if (lobby != null)
            {
                // If target uuid is null
                if (string.IsNullOrEmpty(targetUUID))
                {
                    bool success = false;
                    OpError op = OpError.TargetNotFound;

                    // Send message to all players in lobby. If player is in the room, it will also send the message to the room client.
                    MskSocket[] playerClients = lobby.Players.ToArray().Select(x => x.Client).ToArray();
                    if (playerClients.Length > 0)
                    {
                        using (Packet packet = new Packet((int)OpResponse.OnMessageReceived))
                        {
                            packet.Write(client.UUID);
                            packet.Write(message);
                            PacketSender.SendPacketToClients(playerClients, packet);
                        }

                        success = true;
                        op = OpError.Success;
                    }

                    MskPlayer player = FindPlayer(client);
                    if (player != null)
                    {
                        if (player.InRoom)
                        {
                            MskRoom room = FindRoom(version, player.RoomName);
                            if (room != null)
                            {
                                using (Packet packet = new Packet((int)OpResponse.OnMessageReceived))
                                {
                                    packet.Write(client.UUID);
                                    packet.Write(message);
                                    PacketSender.SendPacketToClient(room.RoomClient, packet);
                                }

                                success = true;
                                op = OpError.Success;
                            }
                        }
                    }

                    callback?.Invoke(success, op);
                }
                else
                {
                    // If target is a player.
                    MskPlayer player = lobby.Players.Get(targetUUID);
                    if (player != null)
                    {
                        using (Packet packet = new Packet((int)OpResponse.OnMessageReceived))
                        {
                            packet.Write(client.UUID);
                            packet.Write(message);
                            PacketSender.SendPacketToClient(player.Client, packet);
                        }

                        callback?.Invoke(true, OpError.Success);

                        return;
                    }

                    // If target is a room.
                    foreach(MskSocket roomClient in RoomClients)
                    {
                        if (roomClient.UUID == targetUUID)
                        {
                            using (Packet packet = new Packet((int)OpResponse.OnMessageReceived))
                            {
                                packet.Write(client.UUID);
                                packet.Write(message);
                                PacketSender.SendPacketToClient(roomClient, packet);
                            }

                            callback?.Invoke(true, OpError.Success);

                            return;
                        }
                    }

                    callback?.Invoke(false, OpError.TargetNotFound);
                }
            }
        }

        #endregion
    }
}
