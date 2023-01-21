using System.Collections.Generic;
using System;
using UnityEngine;

namespace MasterServerKit
{
    public class MskClient
    {
        // Properties
        /// <summary>
        /// Version of server. 
        /// </summary>
        public static string Version { get { return MskConfig.Instance.version; } }

        /// <summary>
        /// Is connected to the master serevr?
        /// </summary>
        public static bool IsConnected { get; private set; }
        private static bool isInitialized = false;

        /// <summary>
        /// Is connected to the lobby?
        /// </summary>
        public static bool InLobby { get; private set; }

        // Player
        /// <summary>
        /// Local player. Value will be null if master server is not connected.
        /// </summary>
        public static MskPlayer Player { get; private set; }


        // Room
        /// <summary>
        /// Current joined room. If player did not joined a room, the value will be null.
        /// </summary>
        public static MskRoom Room { get; private set; }

        /// <summary>
        /// Is in room?
        /// </summary>
        public static bool InRoom { get { return Room != null; } }


        // Lobby
        /// <summary>
        /// Player count in lobby. Call <see cref="GetPlayerCount"/> to get player count in lobby.
        /// </summary>
        public static int PlayerCountInLobby { get; private set; }

        /// <summary>
        /// Player list in lobby. Call <see cref="GetPlayerList"/> to get player list in lobby.
        /// </summary>
        public static PlayerInfo[] PlayerList { get; private set; }

        /// <summary>
        /// Room list in lobby. Call <see cref="GetRoomList"/> to get room list in lobby.
        /// </summary>
        public static RoomInfo[] RoomList { get; private set; }


        // Callbacks
        public static Action onConnectedToMaster;
        public static Action onDisconnectedFromMaster;
        public static Action onConnectedToLobby;
        public static Action<OpError> onConnectToLobbyFailed;
        public static Action onSpawnProcessStarted;
        public static Action<string, string, ushort> onCreatedRoom;
        public static Action<OpError> onCreatRoomFailed;
        public static Action onJoinedRoom;
        public static Action<OpError> onJoinRoomFailed;
        public static Action<OpError> onJoinRandomRoomFailed;
        public static Action onLeftRoom;
        public static Action<MskPlayer> onPlayerJoined;
        public static Action<MskPlayer> onPlayerLeft;
        public static Action<MskPlayer> onMasterChanged;
        public static Action<MskProperties> onRoomCustomPropertiesUpdated;
        public static Action<MskPlayer> onNicknameUpdated;
        public static Action<MskPlayer, MskProperties> onPlayerCustomPropertiesUpdated;

        public static Action<int> onPlayerCountGet;
        public static Action<PlayerInfo[]> onPlayerListGet;
        public static Action<RoomInfo[]> onRoomListGet;

        #region Initialize
        private static void Initialize()
        {
            IsConnected = false;
            InLobby = false;

            Player = null;
            Room = null;

            PlayerCountInLobby = 0;
            RoomList = new RoomInfo[] { };
            PlayerList = new PlayerInfo[] { };

            MskDispatcher.Initialize();
            MskSocket.Initialize(new Dictionary<int, MskSocket.PacketHandler>
            {
                {(int)OpResponse.OnConnectedToMaster, OnConnectedToMaster },
                {(int)OpResponse.OnConnectedToLobby, OnConnectedToLobby },
                {(int)OpResponse.OnConnectToLobbyFailed, OnConnectToLobbyFailed },
                {(int)OpResponse.OnCreatedRoom, OnCreatedRoom },
                {(int)OpResponse.OnCreateRoomFailed, OnCreateRoomFailed },
                {(int)OpResponse.OnJoinedRoom, OnJoinedRoom },
                {(int)OpResponse.OnJoinRoomFailed, OnJoinRoomFailed },
                {(int)OpResponse.OnJoinRandomRoomFailed, OnJoinRandomRoomFailed },
                {(int)OpResponse.OnLeftRoom, OnLeftRoom },
                {(int)OpResponse.OnPlayerJoined, OnPlayerJoined },
                {(int)OpResponse.OnPlayerLeft, OnPlayerLeft },
                {(int)OpResponse.OnMasterChanged, OnMasterChanged },
                {(int)OpResponse.OnRoomPropertiesUpdated, OnRoomPropertiesUpdated},
                {(int)OpResponse.OnNicknameUpdated, OnNicknameUpdated },
                {(int)OpResponse.OnPlayerCustomPropertiesUpdated, OnPlayerCustomPropertiesUpdated },
                {(int)OpResponse.OnPlayerCountGet, OnPlayerCountGet },
                {(int)OpResponse.OnPlayerListGet, OnPlayerListGet },
                {(int)OpResponse.OnRoomListGet, OnRoomListGet }

            });
            MskSocket.onDisconnected += OnDisconnectedFromMaster;
        }

        #endregion

        #region Connection To Master

        /// <summary>
        /// Try connecting to the master server with given ip and port.
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public static void ConnectToMasterServer(string ip, ushort port)
        {
            if (IsConnected)
            {
                return;
            }

            if (!isInitialized)
            {
                isInitialized = true;
                Initialize();
            }

            MskSocket.Connect(ip, port);
        }

        private static void OnConnectedToMaster(Packet packet)
        {
            IsConnected = true;
            MskSocket.IsConnected = true;

            int id = packet.ReadInt();

            Player = new MskPlayer(id);
            Player.IsLocal = true;

            onConnectedToMaster?.Invoke();
        }

        /// <summary>
        /// Try disconnecting from the master server.
        /// </summary>
        public static void Disconnect()
        {
            if (!IsConnected)
            {
                return;
            }

            if (MskSocket.IsConnected)
            {
                MskSocket.Disconnect();
            }
        }

        private static void OnDisconnectedFromMaster()
        {
            IsConnected = false;
            InLobby = false;

            Player = null;
            Room = null;

            PlayerCountInLobby = 0;
            RoomList = new RoomInfo[] { };
            PlayerList = new PlayerInfo[] { };

            onDisconnectedFromMaster?.Invoke();
        }

        #endregion

        #region Connect To Lobby
        /// <summary>
        /// Try connecting to the lobby. Lobby will be distinguished by version. Change <see cref="MskPlayer.Nickname"/> and 
        /// <see cref="MskPlayer.CustomProperties"/> to set player's nickname and custom properties before connecting to the lobby.
        /// </summary>
        public static void ConnectToLobby()
        {
            if (!IsConnected)
            {
                Debug.LogError("You are not connected to the master server!");
                return;
            }

            using (Packet packet = new Packet((int)OpRequest.ConnectToLobby))
            {
                packet.Write(Version);
                packet.Write(true);
                packet.Write(Player.Nickname);
                packet.Write(Player.CustomProperties.SerializeJson());

                MskSocket.SendData(packet);
            }
        }

        private static void OnConnectedToLobby(Packet packet)
        {
            InLobby = true;

            onConnectedToLobby?.Invoke();
        }

        private static void OnConnectToLobbyFailed(Packet packet)
        {
            InLobby = false;

            OpError opError = (OpError)packet.ReadInt();
            onConnectToLobbyFailed?.Invoke(opError);
        }

        #endregion

        #region Create Room

        /// <summary>
        /// Try creating room. If room name is not given, master server will create room number for new rooms.
        /// </summary>
        /// <param name="roomName"></param>
        /// <param name="roomOptions"></param>
        public static void CreateRoom(string roomName = "", RoomOptions roomOptions = null)
        {
            if (!IsConnected)
            {
                Debug.LogError("You are not connected to the master server!");
                return;
            }

            if (!InLobby)
            {
                Debug.LogError("You are not connected to the lobby!");
                return;
            }

            if (roomOptions == null)
            {
                roomOptions = new RoomOptions();
            }

            using (Packet packet = new Packet((int)OpRequest.CreateRoom))
            {
                packet.Write(Version);
                packet.Write(roomName);
                packet.Write(JsonSerializer.ToJson(roomOptions));

                MskSocket.SendData(packet);
            }
        }

        private static void OnSpawnProcessStarted(Packet packet)
        {
            onSpawnProcessStarted?.Invoke();
        }

        private static void OnCreatedRoom(Packet packet)
        {
            string roomName = packet.ReadString();
            string ip = packet.ReadString();
            ushort port = (ushort)packet.ReadShort();

            onCreatedRoom?.Invoke(roomName, ip, port);
        }

        private static void OnCreateRoomFailed(Packet packet)
        {
            OpError opError = (OpError)packet.ReadInt();
            onCreatRoomFailed?.Invoke(opError);
        }

        #endregion

        #region Join Room
        /// <summary>
        /// Try joining to room. If room is locked by password, correct password should be provided to join the room.
        /// </summary>
        /// <param name="roomName"></param>
        /// <param name="password"></param>
        public static void JoinRoom(string roomName, string password = null)
        {
            if (!IsConnected)
            {
                Debug.LogError("You are not connected to the master server!");
                return;
            }

            if (!InLobby)
            {
                Debug.LogError("You are not connected to the lobby!");
                return;
            }

            if (string.IsNullOrEmpty(roomName))
            {
                Debug.LogError("Room name should not be null or empty!");
                return;
            }

            JoinRoomInternal(roomName, password);
        }

        /// <summary>
        /// Try joining random room. Only room which are non private, no password and open will be considered.
        /// </summary>
        public static void JoinRandomRoom()
        {
            if (!IsConnected)
            {
                Debug.LogError("You are not connected to the master server!");
                return;
            }

            if (!InLobby)
            {
                Debug.LogError("You are not connected to the lobby!");
                return;
            }

            JoinRoomInternal("", null);
        }

        private static void JoinRoomInternal(string roomName, string password)
        {
            using (Packet packet = new Packet(string.IsNullOrEmpty(roomName) ? (int)OpRequest.JoinRandomRoom : (int)OpRequest.JoinRoom))
            {
                packet.Write(Version);
                packet.Write(roomName);
                packet.Write(password);

                MskSocket.SendData(packet);
            }
        }

        private static void OnJoinedRoom(Packet packet)
        {
            string json = packet.ReadString();
            MskRoom room = JsonSerializer.FromJson<MskRoom>(json);

            Room = room;

            onJoinedRoom?.Invoke();
        }

        private static void OnJoinRoomFailed(Packet packet)
        {
            OpError opError = (OpError)packet.ReadInt();
            onJoinRoomFailed?.Invoke(opError);
        }

        private static void OnJoinRandomRoomFailed(Packet packet)
        {
            OpError opError = (OpError)packet.ReadInt();
            onJoinRandomRoomFailed?.Invoke(opError);
        }

        #endregion

        #region Leave Room
        /// <summary>
        /// Try leaving current room.
        /// </summary>
        public static void LeaveRoom()
        {
            if (!IsConnected)
            {
                Debug.LogError("You are not connected to the master server!");
                return;
            }

            if (!InLobby)
            {
                Debug.LogError("You are not connected to the lobby!");
                return;
            }

            if (Room == null)
            {
                Debug.LogError("You are not in a room!");
                return;
            }


            using (Packet packet = new Packet((int)OpRequest.LeaveRoom))
            {
                packet.Write(Version);
                packet.Write(Room.Name);

                MskSocket.SendData(packet);
            }

        }

        private static void OnLeftRoom(Packet packet)
        {
            onLeftRoom?.Invoke();
        }

        #endregion

        #region Room Properties
        /// <summary>
        /// Set current room to private.
        /// </summary>
        /// <param name="isPrivate"></param>
        public static void SetPrivate(bool isPrivate)
        {
            if (!IsConnected)
            {
                Debug.LogError("You are not connected to the master server!");
                return;
            }

            if (!InLobby)
            {
                Debug.LogError("You are not connected to the lobby!");
                return;
            }

            if (Room == null)
            {
                Debug.LogError("Room propertie only be set in a room!");
                return;
            }

            using (Packet packet = new Packet((int)OpRequest.UpdateRoomProperties))
            {
                packet.Write(Version);
                packet.Write((int)OpRoomProperties.ChangePrivate);
                packet.Write(isPrivate);
                MskSocket.SendData(packet);
            }
        }

        /// <summary>
        /// Set current room to open.
        /// </summary>
        /// <param name="isOpen"></param>
        public static void SetOpen(bool isOpen)
        {
            if (!IsConnected)
            {
                Debug.LogError("You are not connected to the master server!");
                return;
            }

            if (!InLobby)
            {
                Debug.LogError("You are not connected to the lobby!");
                return;
            }

            if (Room == null)
            {
                Debug.LogError("Room propertie only be set in a room!");
                return;
            }

            using (Packet packet = new Packet((int)OpRequest.UpdateRoomProperties))
            {
                packet.Write(Version);
                packet.Write((int)OpRoomProperties.ChangeOpen);
                packet.Write(isOpen);
                MskSocket.SendData(packet);
            }
        }

        /// <summary>
        /// Set current room's password.
        /// </summary>
        /// <param name="password"></param>
        public static void SetPassword(string password)
        {
            if (!IsConnected)
            {
                Debug.LogError("You are not connected to the master server!");
                return;
            }

            if (!InLobby)
            {
                Debug.LogError("You are not connected to the lobby!");
                return;
            }

            if (Room == null)
            {
                Debug.LogError("Room propertie only be set in a room!");
                return;
            }

            using (Packet packet = new Packet((int)OpRequest.UpdateRoomProperties))
            {
                packet.Write(Version);
                packet.Write((int)OpRoomProperties.ChangePassword);
                packet.Write(password);
                MskSocket.SendData(packet);
            }
        }

        /// <summary>
        /// Set current room's maximum players.
        /// </summary>
        /// <param name="maxPlayers"></param>
        public static void SetMaxPlayers(int maxPlayers)
        {
            if (!IsConnected)
            {
                Debug.LogError("You are not connected to the master server!");
                return;
            }

            if (!InLobby)
            {
                Debug.LogError("You are not connected to the lobby!");
                return;
            }

            if (Room == null)
            {
                Debug.LogError("Room propertie only be set in a room!");
                return;
            }

            using (Packet packet = new Packet((int)OpRequest.UpdateRoomProperties))
            {
                packet.Write(Version);
                packet.Write((int)OpRoomProperties.ChangeMaxPlayers);
                packet.Write(maxPlayers);
                MskSocket.SendData(packet);
            }
        }

        /// <summary>
        /// Set current room's custom properties.
        /// </summary>
        /// <param name="properties"></param>
        public static void SetRoomCustomProperties(MskProperties properties)
        {
            if (!IsConnected)
            {
                Debug.LogError("You are not connected to the master server!");
                return;
            }

            if (!InLobby)
            {
                Debug.LogError("You are not connected to the lobby!");
                return;
            }

            if (Room == null)
            {
                Debug.LogError("Room propertie only be set in a room!");
                return;
            }

            using (Packet packet = new Packet((int)OpRequest.UpdateRoomProperties))
            {
                packet.Write(Version);
                packet.Write((int)OpRoomProperties.UpdateCustomProperties);
                packet.Write(properties.SerializeJson());
                MskSocket.SendData(packet);
            }
        }

        /// <summary>
        /// Set master to given player.
        /// </summary>
        /// <param name="player"></param>
        public static void SetMaster(MskPlayer player)
        {
            if (!IsConnected)
            {
                Debug.LogError("You are not connected to the master server!");
                return;
            }

            if (!InLobby)
            {
                Debug.LogError("You are not connected to the lobby!");
                return;
            }

            if (Room == null)
            {
                Debug.LogError("Room propertie only be set in a room!");
                return;
            }

            using (Packet packet = new Packet((int)OpRequest.SetMaster))
            {
                packet.Write(Version);
                packet.Write(Room.Name);
                packet.Write(player.Id);
                MskSocket.SendData(packet);
            }
        }

        private static void OnRoomPropertiesUpdated(Packet packet)
        {
            if (Room == null)
            {
                return;
            }

            OpRoomProperties op = (OpRoomProperties)packet.ReadInt();
            if (op == OpRoomProperties.ChangePrivate)
            {
                Room.IsPrivate = packet.ReadBool();
            }
            else if (op == OpRoomProperties.ChangeOpen)
            {
                Room.IsOpen = packet.ReadBool();
            }
            else if (op == OpRoomProperties.ChangeMaxPlayers)
            {
                Room.MaxPlayers = packet.ReadInt();
            }
            else if (op == OpRoomProperties.ChangePassword)
            {
                Room.IsPasswordLock = packet.ReadBool();
            }
            else if (op == OpRoomProperties.UpdateCustomProperties)
            {
                MskProperties mskProperties = MskProperties.Deserialize((string)packet.ReadString());
                Room.CustomProperties.Append(mskProperties);

                onRoomCustomPropertiesUpdated?.Invoke(mskProperties);
            }
        }

        #endregion

        #region Player Connection Control
        private static void OnPlayerJoined(Packet packet)
        {
            string json = packet.ReadString();
            MskPlayer player = JsonSerializer.FromJson<MskPlayer>(json);

            Room.Players.Add(player.Id, player);

            onPlayerJoined?.Invoke(player);
        }

        private static void OnPlayerLeft(Packet packet)
        {
            int clientId = packet.ReadInt();

            MskPlayer player = Room.Players[clientId];
            Room.Players.Remove(player.Id);

            onPlayerLeft?.Invoke(player);
        }

        private static void OnMasterChanged(Packet packet)
        {
            int clientId = packet.ReadInt();

            Debug.Log("MASTER " + clientId);

            MskPlayer player = Room.Players[clientId];
            Room.MasterId = clientId;

            onMasterChanged?.Invoke(player);
        }

        #endregion

        #region Player Custom Properties
        /// <summary>
        /// Set local player's nickname. If local player is in room,
        /// other players in the room will also be notified.
        /// </summary>
        /// <param name="nickname"></param>
        public static void SetNickname(string nickname)
        {
            if (!IsConnected)
            {
                Debug.LogError("You are not connected to the master server!");
                return;
            }

            if (!InLobby)
            {
                Debug.LogError("You are not connected to the lobby!");
                return;
            }

            using (Packet packet = new Packet((int)OpRequest.SetNickname))
            {
                packet.Write(Version);
                packet.Write(nickname);
                MskSocket.SendData(packet);
            }
        }

        /// <summary>
        /// Set player's custom properties. If local player's custom properties are updated,
        /// other players in the room will also be notified.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="properties"></param>
        public static void SetPlayerCustomProperties(MskPlayer player, MskProperties properties)
        {
            if (!IsConnected)
            {
                Debug.LogError("You are not connected to the master server!");
                return;
            }

            if (!InLobby)
            {
                Debug.LogError("You are not connected to the lobby!");
                return;
            }

            using (Packet packet = new Packet((int)OpRequest.SetPlayerCustomProperties))
            {
                packet.Write(Version);
                packet.Write(player.Id);
                packet.Write(properties.SerializeJson());
                MskSocket.SendData(packet);
            }
        }

        private static void OnNicknameUpdated(Packet packet)
        {
            int clientId = packet.ReadInt();
            string nickname = packet.ReadString();

            if (Player.Id == clientId)
            {
                Player.Nickname = nickname;
                onNicknameUpdated?.Invoke(Player);
            }
            else
            {
                if (Room != null)
                {
                    if (Room.Players.ContainsKey(clientId))
                    {
                        Room.Players[clientId].Nickname = nickname;
                        onNicknameUpdated?.Invoke(Room.Players[clientId]);
                    }
                }
            }
        }

        private static void OnPlayerCustomPropertiesUpdated(Packet packet)
        {
            int clientId = packet.ReadInt();
            MskProperties props = MskProperties.Deserialize(packet.ReadString());

            if (Player.Id == clientId)
            {
                Player.CustomProperties.Append(props);
                onPlayerCustomPropertiesUpdated?.Invoke(Player, props);
            }
            else
            {
                if (Room != null)
                {
                    if (Room.Players.ContainsKey(clientId))
                    {
                        Room.Players[clientId].CustomProperties.Append(props);
                        onPlayerCustomPropertiesUpdated?.Invoke(Room.Players[clientId], props);
                    }
                }
            }
        }

        #endregion

        #region Lobby Info
        /// <summary>
        /// Get players count in current lobby. If success, <see cref="onPlayerCountGet"/> will be invoked 
        /// and <see cref="PlayerCountInLobby"/> will be set.
        /// </summary>
        public static void GetPlayerCount()
        {
            if (!IsConnected)
            {
                Debug.LogError("You are not connected to the master server!");
                return;
            }

            if (!InLobby)
            {
                Debug.LogError("You are not connected to the lobby!");
                return;
            }

            using (Packet packet = new Packet((int)OpRequest.GetPlayerCount))
            {
                packet.Write(Version);
                MskSocket.SendData(packet);
            }
        }

        private static void OnPlayerCountGet(Packet packet)
        {
            int playerCount = packet.ReadInt();
            onPlayerCountGet?.Invoke(playerCount);
        }

        /// <summary>
        /// Get player list in current lobby. If success, <see cref="onPlayerListGet"/> will be invoked 
        /// and <see cref="PlayerList"/> will be set.
        /// </summary>
        public static void GetPlayerList()
        {
            if (!IsConnected)
            {
                Debug.LogError("You are not connected to the master server!");
                return;
            }

            if (!InLobby)
            {
                Debug.LogError("You are not connected to the lobby!");
                return;
            }

            using (Packet packet = new Packet((int)OpRequest.GetPlayerList))
            {
                packet.Write(Version);
                MskSocket.SendData(packet);
            }
        }

        private static void OnPlayerListGet(Packet packet)
        {
            string json = packet.ReadString();
            PlayerInfo[] playerInfos = JsonSerializer.FromJson<PlayerInfo[]>(json);

            onPlayerListGet?.Invoke(playerInfos);
        }

        /// <summary>
        /// Get room list in current lobby. If success, <see cref="onRoomListGet"/> will be invoked
        /// and <see cref="RoomList"/> will be set.
        /// </summary>
        public static void GetRoomList()
        {
            if (!IsConnected)
            {
                Debug.LogError("You are not connected to the master server!");
                return;
            }

            if (!InLobby)
            {
                Debug.LogError("You are not connected to the lobby!");
                return;
            }

            using (Packet packet = new Packet((int)OpRequest.GetRoomList))
            {
                packet.Write(Version);
                MskSocket.SendData(packet);
            }
        }

        private static void OnRoomListGet(Packet packet)
        {
            string json = packet.ReadString();
            RoomInfo[] roomInfos = JsonSerializer.FromJson<RoomInfo[]>(json);

            onRoomListGet?.Invoke(roomInfos);
        }

        #endregion
    }
}