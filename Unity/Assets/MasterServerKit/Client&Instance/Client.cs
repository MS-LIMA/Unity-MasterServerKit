using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace Msk
{
    public partial class MasterServerKit
    {
        public class Client
        {
            private static bool m_isInitialized = false;
            private static bool m_isConnecting = false;

            /// <summary>
            /// Time out occurs after this seconds when connecting to the master server.
            /// </summary>
            public static float TimeOutConnectMaster { get; set; } = 5f;

            /// <summary>
            /// It tries to connect to master server by specific times.
            /// </summary>
            public static int TryCountConnectMaster { get; set; } = 3;

            /// <summary>
            /// Local player of this client.
            /// </summary>
            public static MskPlayer LocalPlayer { get; private set; } = new MskPlayer(true, -1, "");

            /// <summary>
            /// Current room where this client is joined in.
            /// </summary>
            public static MskRoom Room { get { return MasterServerKit.Room; } set { MasterServerKit.Room = value; } }

            /// <summary>
            /// Room list of this lobby.
            /// </summary>
            public static RoomInfo[] Rooms { get; private set; }


            /// <summary>
            /// Player list in this lobby.
            /// </summary>
            public static PlayerInfo[] Players { get; private set; }

            /// <summary>
            /// Player list in this lobby with uuid.
            /// </summary>
            public static Dictionary<string, PlayerInfo> PlayersDictionary { get; private set; }

            /// <summary>
            /// Current player count of this lobby who are not in the room.
            /// </summary>
            public static int PlayerCount { get; private set; }


            /// <summary>
            /// Current player count of this lobby including players in the room.
            /// </summary>
            public static int PlayerCountInLobby { get; private set; }


            // Callbacks
            public static OnConnectedToMaster onConnectedToMaster;
            public static OnClientAcceptedOnMaster onClientAcceptedOnMaster;
            public static OnConnectToMasterFailed onConnectToMasterFailed;
            public static OnDisconnectedFromMaster onDisconnectedFromMaster;

            public static OnSpawnProcessStarted onSpawnProcessStarted;
            public static OnCreatedRoom onCreatedRoom;
            public static OnCreateRoomFailed onCreateRoomFailed;
            public static OnJoinedRoom onJoinedRoom;
            public static OnJoinRoomFailed onJoinRoomFailed;
            public static OnJoinRandomRoomFailed onJoinRandomRoomFailed;
            public static OnLeftRoom onLeftRoom;

            public static OnPlayerJoined onPlayerJoined;
            public static OnPlayerLeft onPlayerLeft;
            public static OnPlayerKicked onPlayerKicked;
            public static OnMasterChanged onMasterChanged;
            public static OnRoomPropertiesUpdated onRoomPropertiesUpdated;
            public static OnRoomCustomPropertiesUpdated onRoomCustomPropertiesUpdated;

            public static OnPlayerNicknameUpdated onPlayerNicknameUpdated;
            public static OnPlayerCustomPropertiesUpdated onPlayerCustomPropertiesUpdated;
            public static OnPlayerCountFetched onPlayerCountFetched;
            public static OnPlayerCountInLobbyFetched onPlayerCountInLobbyFetched;
            public static OnPlayerListFetched onPlayerListFetched;
            public static OnRoomListFetched onRoomListFetched;
            public static OnMessageReceived onMessageReceived;
            public static OnSendMessageSuccess onSendMessageSuccess;
            public static OnSendMessageFailed onSendMessageFailed;


#if UNITY_EDITOR

            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
            private static void CleanUp()
            {
                m_isInitialized = false;
                m_isConnecting = false;
            }

#endif

            private static void Initialize()
            {
                if (m_isInitialized)
                {
                    return;
                }

                m_isInitialized = true;

                IsInstance = false;
                IsClient = true;

                MskDispatcher.Initialize();
                MskClientMono.Initialize();
                MskClientMono.onConnectFailed += () =>
                {
                    m_isConnecting = false;
                    Socket.Disconnect();
                    onConnectToMasterFailed?.Invoke(OpError.Timeout);
                };

                Rooms = new RoomInfo[] { };
                Players = new PlayerInfo[] { };
                PlayersDictionary = new Dictionary<string, PlayerInfo>();

                PlayerCount = 0;
                PlayerCountInLobby = 0;

                MskSocket.PacketHandlers = new Dictionary<int, PacketHandler>
            {
                {(int)OpResponse.OnConnectedToMaster, OnConnectedToMaster },
                {(int)OpResponse.OnClientAcceptedOnMaster,OnClientAcceptedOnMaster },
                {(int)OpResponse.OnConnectToMasterFailed, OnConnectToMasterFailed },
                {(int)OpResponse.OnSpawnProcessStarted, OnSpawnProcessStarted },
                {(int)OpResponse.OnCreatedRoom, OnCreatedRoom },
                {(int)OpResponse.OnCreateRoomFailed, OnCreateRoomFailed },
                {(int)OpResponse.OnJoinedRoom, OnJoinedRoom },
                {(int)OpResponse.OnJoinRoomFailed, OnJoinRoomFailed },
                {(int)OpResponse.OnJoinRandomRoomFailed, OnJoinRandomRoomFailed },
                {(int)OpResponse.OnLeftRoom, OnLeftRoom },
                {(int)OpResponse.OnPlayerJoined, OnPlayerJoined },
                {(int)OpResponse.OnPlayerLeft, OnPlayerLeft },
                {(int)OpResponse.OnPlayerKicked, OnPlayerKicked },
                {(int)OpResponse.OnMasterChanged, OnMasterChanged },
                {(int)OpResponse.OnRoomPropertiesUpdated, OnRoomPropertiesUpdated},
                {(int)OpResponse.OnNicknameUpdated, OnPlayerNicknameUpdated },
                {(int)OpResponse.OnPlayerCustomPropertiesUpdated, OnPlayerCustomPropertiesUpdated },
                {(int)OpResponse.OnPlayerCountFetched, OnPlayerCountFetched },
                {(int)OpResponse.OnPlayerCountInLobbyFetched, OnPlayerCountInLobbyFetched },
                {(int)OpResponse.OnPlayerListFetched, OnPlayerListFetched },
                {(int)OpResponse.OnRoomListFetched, OnRoomListFetched },
                {(int)OpResponse.OnMessageReceived, OnMessageReceived},
                {(int)OpResponse.OnSendMessageFailed, OnSendMessageFailed},
                {(int)OpResponse.OnSendMessageSuccess, OnSendMessageSuccess}
            };
            }

            #region Connection Control

            /// <summary>
            /// Try connecting to the master server with given ip and port.
            /// If the uuid is given, it will be used to identify user on the master server.
            /// </summary>
            public static void ConnectToMaster(string uuid = "")
            {
                if (MasterServerKit.IsConnected)
                {
                    return;
                }

                if (m_isConnecting)
                {
                    return;
                }

                m_isConnecting = true;

                Initialize();

                MskClientMono.StartTtlConnectToMasterRoutine();

                MasterServerKit.Version = MskConfigClient.Version;

                MasterServerKit.Socket.UUID = uuid;
                MasterServerKit.Socket.onDisconnected += OnDisconnectedFromMaster;

                string ip = MskConfigClient.Host;
                ushort port = MskConfigClient.Port;

                if (MskConfigClient.DnsForHost)
                {
                    IPAddress address = Dns.GetHostAddresses(ip)[0];
                    ip = address.ToString();
                }

                MasterServerKit.Socket.Connect(ip, port);
            }

            protected static void OnClientAcceptedOnMaster(MskSocket client, Packet packet)
            {
                MasterServerKit.Socket.ClientId = packet.ReadInt();

                using (Packet p = new Packet((int)OpRequest.ConnectToMaster))
                {
                    p.Write(MasterServerKit.Version);
                    p.Write(true);
                    p.Write(MasterServerKit.Socket.ClientId);
                    p.Write(MasterServerKit.Socket.UUID);
                    p.Write(LocalPlayer.Nickname);
                    p.Write(LocalPlayer.CustomProperties.SerializeJson());

                    client.SendData(p);
                }

                onClientAcceptedOnMaster?.Invoke();
            }

            protected static void OnConnectedToMaster(MskSocket client, Packet packet)
            {
                IsConnected = true;
                m_isConnecting = false;

                int clientId = packet.ReadInt();
                string uuid = packet.ReadString();

                Socket.ClientId = clientId;
                Socket.UUID = uuid;

                LocalPlayer.ClientId = Socket.ClientId;
                LocalPlayer.UUID = Socket.UUID;

                onConnectedToMaster?.Invoke();
            }

            private static void OnConnectToMasterFailed(MskSocket client, Packet packet)
            {
                IsConnected = false;
                m_isConnecting = false;
                LocalPlayer.RoomName = "";

                OpError opError = (OpError)packet.ReadInt();
                onConnectToMasterFailed?.Invoke(opError);
            }

            /// <summary>
            /// Try disconnecting from the master server.
            /// </summary>
            public static void Disconnect()
            {
                Socket?.Disconnect();
            }

            protected static void OnDisconnectedFromMaster(MskSocket socket)
            {
                if (IsConnected)
                {
                    MskDispatcher.EnqueueCallback(() =>
                    {
                        onDisconnectedFromMaster?.Invoke();
                    });
                }

                IsConnected = false;
                m_isConnecting = false;

                Room = null;
                LocalPlayer.UUID = "";
                LocalPlayer.RoomName = "";
                LocalPlayer.ClientId = -1;
                LocalPlayer.CustomProperties.Clear();

                PlayerCount = 0;
                PlayerCountInLobby = 0;

                Rooms = new RoomInfo[] { };
                Players = new PlayerInfo[] { };
                PlayersDictionary.Clear();
            }

            #endregion

            #region Create Room

            /// <summary>
            /// Try creating room. Room name must be unique since master server distinguish the room by its name. You can pass the room options for detail.
            /// </summary>
            /// <param name="roomName"></param>
            /// <param name="roomOptions"></param>
            public static void CreateRoom(string roomName, RoomOptions roomOptions = null)
            {
                if (!IsConnected)
                {
                    Debug.LogError("You are not connected to the master server!");
                    return;
                }

                if (roomOptions == null)
                {
                    roomOptions = new RoomOptions();
                }

                string temp = Utilities.ToJson(roomOptions);

                temp = temp.Replace("\\\"", "`");
                temp = temp.Replace("\"", "\\\"");
                temp = temp.Replace("`", "\\\"");

                using (Packet packet = new Packet((int)OpRequest.CreateRoom))
                {
                    packet.Write(Version);
                    packet.Write(roomName);
                    packet.Write(Utilities.ToJson(roomOptions));

                    Socket.SendData(packet);
                }
            }

            private static void OnSpawnProcessStarted(MskSocket client, Packet packet)
            {
                onSpawnProcessStarted?.Invoke();
            }

            private static void OnCreatedRoom(MskSocket client, Packet packet)
            {
                string roomName = packet.ReadString();
                string ip = packet.ReadString();
                ushort port = (ushort)packet.ReadShort();

                onCreatedRoom?.Invoke(roomName, ip, port);
            }

            private static void OnCreateRoomFailed(MskSocket client, Packet packet)
            {
                OpError opError = (OpError)packet.ReadInt();
                onCreateRoomFailed?.Invoke(opError);
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

                JoinRoomInternal("", null);
            }

            private static void JoinRoomInternal(string roomName, string password)
            {
                using (Packet packet = new Packet(string.IsNullOrEmpty(roomName) ? (int)OpRequest.JoinRandomRoom : (int)OpRequest.JoinRoom))
                {
                    packet.Write(Version);
                    packet.Write(roomName);
                    packet.Write(password);

                    Socket.SendData(packet);
                }
            }

            private static void OnJoinedRoom(MskSocket client, Packet packet)
            {
                string json = packet.ReadString();
                MskRoom room = MskRoom.DeserializeJson(json);

                Room = room;

                onJoinedRoom?.Invoke();
            }

            private static void OnJoinRoomFailed(MskSocket client, Packet packet)
            {
                Room = null;

                OpError opError = (OpError)packet.ReadInt();
                onJoinRoomFailed?.Invoke(opError);
            }

            private static void OnJoinRandomRoomFailed(MskSocket client, Packet packet)
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

                if (Room == null)
                {
                    Debug.LogError("You are not in a room!");
                    return;
                }


                using (Packet packet = new Packet((int)OpRequest.LeaveRoom))
                {
                    packet.Write(Version);
                    packet.Write(Room.Name);

                    Socket.SendData(packet);
                }

            }

            private static void OnLeftRoom(MskSocket client, Packet packet)
            {
                Room = null;
                LocalPlayer.RoomName = "";

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

                if (Room == null)
                {
                    Debug.LogError("Room propertie only be set in a room!");
                    return;
                }

                using (Packet packet = new Packet((int)OpRequest.UpdateRoomProperties))
                {
                    packet.Write(Version);
                    packet.Write(Room.Name);
                    packet.Write((int)OpRoomProperties.ChangePrivate);
                    packet.Write(isPrivate);

                    Socket.SendData(packet);
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


                if (Room == null)
                {
                    Debug.LogError("Room propertie only be set in a room!");
                    return;
                }

                using (Packet packet = new Packet((int)OpRequest.UpdateRoomProperties))
                {
                    packet.Write(Version);
                    packet.Write(Room.Name);
                    packet.Write((int)OpRoomProperties.ChangeOpen);
                    packet.Write(isOpen);

                    Socket.SendData(packet);
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

                if (Room == null)
                {
                    Debug.LogError("Room propertie only be set in a room!");
                    return;
                }

                using (Packet packet = new Packet((int)OpRequest.UpdateRoomProperties))
                {
                    packet.Write(Version);
                    packet.Write(Room.Name);
                    packet.Write((int)OpRoomProperties.ChangePassword);
                    packet.Write(password);

                    Socket.SendData(packet);
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

                if (Room == null)
                {
                    Debug.LogError("Room propertie only be set in a room!");
                    return;
                }

                using (Packet packet = new Packet((int)OpRequest.UpdateRoomProperties))
                {
                    packet.Write(Version);
                    packet.Write(Room.Name);
                    packet.Write((int)OpRoomProperties.ChangeMaxPlayers);
                    packet.Write(maxPlayers);

                    Socket.SendData(packet);
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

                if (Room == null)
                {
                    Debug.LogError("Room propertie only be set in a room!");
                    return;
                }

                using (Packet packet = new Packet((int)OpRequest.UpdateRoomProperties))
                {
                    packet.Write(Version);
                    packet.Write(Room.Name);
                    packet.Write((int)OpRoomProperties.UpdateCustomProperties);
                    packet.Write(properties.SerializeJson());

                    Socket.SendData(packet);
                }
            }

            private static void OnRoomPropertiesUpdated(MskSocket client, Packet packet)
            {
                OpRoomProperties op = (OpRoomProperties)packet.ReadInt();
                if (op == OpRoomProperties.ChangePrivate)
                {
                    Room.IsPrivate = packet.ReadBool();
                    onRoomPropertiesUpdated?.Invoke(op);
                }
                else if (op == OpRoomProperties.ChangeOpen)
                {
                    Room.IsOpen = packet.ReadBool();
                    onRoomPropertiesUpdated?.Invoke(op);
                }
                else if (op == OpRoomProperties.ChangeMaxPlayers)
                {
                    Room.MaxPlayers = packet.ReadInt();
                    onRoomPropertiesUpdated?.Invoke(op);
                }
                else if (op == OpRoomProperties.ChangePassword)
                {
                    Room.IsPasswordLock = packet.ReadBool();
                    onRoomPropertiesUpdated?.Invoke(op);
                }
                else if (op == OpRoomProperties.UpdateCustomProperties)
                {
                    string json = packet.ReadString();
                    MskProperties mskProperties = MskProperties.Deserialize(json);
                    Room.CustomProperties.Append(mskProperties);

                    onRoomPropertiesUpdated?.Invoke(op);
                    onRoomCustomPropertiesUpdated?.Invoke(mskProperties);
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

                if (Room == null)
                {
                    Debug.LogError("Room propertie only be set in a room!");
                    return;
                }

                using (Packet packet = new Packet((int)OpRequest.SetMaster))
                {
                    packet.Write(Version);
                    packet.Write(Room.Name);
                    packet.Write(player.ClientId);

                    Socket.SendData(packet);
                }
            }

            private static void OnMasterChanged(MskSocket client, Packet packet)
            {
                int clientId = packet.ReadInt();

                MskPlayer prevMaster = Room.Master;

                MskPlayer player = Room.Players[clientId];
                Room.MasterId = clientId;

                Console.WriteLine($"-> Master is changed from [{prevMaster?.ClientId}] to [{player.ClientId}]");
                onMasterChanged?.Invoke(prevMaster, player);
            }

            private static void OnPlayerJoined(MskSocket client, Packet packet)
            {
                string json = packet.ReadString();
                MskPlayer player = MskPlayer.DeserializeJson(json);

                Room.Players.Add(player.ClientId, player);

                Console.WriteLine($"-> Player[{player.ClientId}] joined room");
                onPlayerJoined?.Invoke(player);
            }

            private static void OnPlayerLeft(MskSocket client, Packet packet)
            {
                int clientId = packet.ReadInt();

                MskPlayer player = Room.Players[clientId];
                Room.Players.Remove(player.ClientId);

                Console.WriteLine($"-> Player[{player.ClientId}] left room");
                onPlayerLeft?.Invoke(player);
            }

            /// <summary>
            /// Kick player from a room. If reason is provided, it will be notified to other players including kicked player.
            /// </summary>
            /// <param name="player"></param>
            /// <param name="reason"></param>
            public static void KickPlayer(MskPlayer player, string reason = "")
            {
                if (!IsConnected)
                {
                    Debug.LogError("You are not connected to the master server!");
                    return;
                }

                if (Room == null)
                {
                    Debug.LogError("Room propertie only be set in a room!");
                    return;
                }

                if (player == null)
                {
                    return;
                }

                using (Packet packet = new Packet((int)OpRequest.KickPlayer))
                {
                    packet.Write(Version);
                    packet.Write(Room.Name);
                    packet.Write(player.ClientId);
                    packet.Write(reason);

                    Socket.SendData(packet);
                }
            }

            private static void OnPlayerKicked(MskSocket client, Packet packet)
            {
                int playerId = packet.ReadInt();
                string reason = packet.ReadString();

                MskPlayer player = Room.FindPlayer(playerId);
                if (player != null)
                {
                    onPlayerKicked?.Invoke(player, reason);

                    if (player.IsLocal)
                    {
                        OnLeftRoom(client, packet);
                    }
                }
            }

            #endregion

            #region Player Properties

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

                using (Packet p = new Packet((int)OpRequest.SetNickname))
                {
                    p.Write(Version);
                    p.Write(nickname);

                    Socket.SendData(p);
                }
            }

            private static void OnPlayerNicknameUpdated(MskSocket client, Packet packet)
            {
                int clientId = packet.ReadInt();
                string nickname = packet.ReadString();
                string prevNickname = packet.ReadString();

                if (LocalPlayer.ClientId == clientId)
                {
                    LocalPlayer.Nickname = nickname;
                    onPlayerNicknameUpdated?.Invoke(LocalPlayer, prevNickname);
                }
                else
                {
                    MskRoom room = Room;

                    if (room != null)
                    {
                        if (room.Players.ContainsKey(clientId))
                        {
                            MskPlayer player = room.Players[clientId];
                            player.Nickname = nickname;

                            onPlayerNicknameUpdated?.Invoke(player, prevNickname);
                        }
                    }
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

                using (Packet packet = new Packet((int)OpRequest.SetPlayerCustomProperties))
                {
                    packet.Write(Version);
                    packet.Write(player.ClientId);
                    packet.Write(properties.SerializeJson());

                    Socket.SendData(packet);
                }
            }

            private static void OnPlayerCustomPropertiesUpdated(MskSocket client, Packet packet)
            {
                int clientId = packet.ReadInt();
                MskProperties props = MskProperties.Deserialize(packet.ReadString());

                if (LocalPlayer.ClientId == clientId)
                {
                    LocalPlayer.CustomProperties.Append(props);
                    onPlayerCustomPropertiesUpdated?.Invoke(LocalPlayer, props);
                }
                else
                {
                    MskRoom room = MasterServerKit.Room;
                    if (room != null)
                    {
                        if (room.Players.ContainsKey(clientId))
                        {
                            MskPlayer player = room.Players[clientId];
                            player.CustomProperties.Append(props);

                            onPlayerCustomPropertiesUpdated?.Invoke(player, props);
                        }
                    }
                }
            }

            #endregion

            #region Lobby Info

            /// <summary>
            /// Get players count in current lobby. If success, <see cref="onPlayerCountFetched"/> will be invoked 
            /// and <see cref="PlayerCount"/> will be set.
            /// </summary>
            public static void FetchPlayerCount()
            {
                if (!IsConnected)
                {
                    Debug.LogError("You are not connected to the master server!");
                    return;
                }

                using (Packet packet = new Packet((int)OpRequest.FetchPlayerCount))
                {
                    packet.Write(Version);

                    Socket.SendData(packet);
                }
            }

            private static void OnPlayerCountFetched(MskSocket client, Packet packet)
            {
                int playerCount = packet.ReadInt();

                onPlayerCountFetched?.Invoke(playerCount);
            }

            /// <summary>
            /// Get players count in current lobby. If success, <see cref="onPlayerCountInLobbyFetched"/> will be invoked 
            /// and <see cref="PlayerCountInLobby"/> will be set.
            /// </summary>
            public static void FetchPlayerCountInLobby()
            {
                if (!IsConnected)
                {
                    Debug.LogError("You are not connected to the master server!");
                    return;
                }

                using (Packet packet = new Packet((int)OpRequest.FetchPlayerCountInLobby))
                {
                    packet.Write(Version);

                    Socket.SendData(packet);
                }
            }

            private static void OnPlayerCountInLobbyFetched(MskSocket client, Packet packet)
            {
                int playerCount = packet.ReadInt();

                onPlayerCountInLobbyFetched?.Invoke(playerCount);
            }

            /// <summary>
            /// Get player list in current lobby. If success, <see cref="onPlayerListFetched"/> will be invoked 
            /// and <see cref="Client.Players"/> will be set.
            /// </summary>
            public static void FetchPlayerList()
            {
                if (!IsConnected)
                {
                    Debug.LogError("You are not connected to the master server!");
                    return;
                }

                using (Packet packet = new Packet((int)OpRequest.FetchPlayerList))
                {
                    packet.Write(Version);

                    Socket.SendData(packet);
                }
            }

            private static void OnPlayerListFetched(MskSocket client, Packet packet)
            {
                string json = packet.ReadString();
                Players = PlayerInfo.DeserializeInfos(json);

                PlayersDictionary.Clear();
                foreach (PlayerInfo playerInfo in Players)
                {
                    if (!PlayersDictionary.ContainsKey(playerInfo.uuid))
                    {
                        PlayersDictionary.Add(playerInfo.uuid, playerInfo);
                    }
                }

                onPlayerListFetched?.Invoke(Players);
            }

            /// <summary>
            /// Get room list in current lobby. If success, <see cref="onRoomListFetched"/> will be invoked
            /// and <see cref="Rooms"/> will be set.
            /// </summary>
            public static void FetchRoomList()
            {
                if (!IsConnected)
                {
                    Debug.LogError("You are not connected to the master server!");
                    return;
                }

                using (Packet packet = new Packet((int)OpRequest.FetchRoomList))
                {
                    packet.Write(Version);

                    Socket.SendData(packet);
                }
            }

            private static void OnRoomListFetched(MskSocket client, Packet packet)
            {
                string json = packet.ReadString();
                Rooms = RoomInfo.DeserializeInfos(json);

                onRoomListFetched?.Invoke(Rooms);
            }

            /// <summary>
            /// Send message to target client. If targetUUID is null or empty, message will be send
            /// to all players in current lobby.
            /// </summary>
            /// <param name="target"></param>
            public static void SendMessage(string message, string targetUUID = "")
            {
                if (!IsConnected)
                {
                    Debug.LogError("Cannot send message when offline");
                    return;
                }

                using (Packet packet = new Packet((int)OpRequest.SendMessage))
                {
                    packet.Write(Version);
                    packet.Write(targetUUID);
                    packet.Write(message);
                    Socket.SendData(packet);
                }
            }

            private static void OnMessageReceived(MskSocket client, Packet packet)
            {
                string sender = packet.ReadString();
                string message = packet.ReadString();
                onMessageReceived?.Invoke(sender, message);
            }

            private static void OnSendMessageSuccess(MskSocket client, Packet packet)
            {
                string target = packet.ReadString();
                string message = packet.ReadString();

                onSendMessageSuccess?.Invoke(target, message);
            }

            private static void OnSendMessageFailed(MskSocket client, Packet packet)
            {
                OpError opError = (OpError)packet.ReadInt();
                string target = packet.ReadString();
                string message = packet.ReadString();

                onSendMessageFailed?.Invoke(target, message, opError);
            }


            #endregion
        }
    }
}