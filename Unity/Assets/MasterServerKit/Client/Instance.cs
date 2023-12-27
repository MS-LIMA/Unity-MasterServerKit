using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Msk
{
    public partial class MasterServerKit
    {
        public class Instance
        {
            /// <summary>
            /// Time to live until first player joined. If any first player is not joined,
            /// server instance will automatically shut down after this seconds.
            /// </summary>
            public static float TtlUntilFirstPlayer { get; set; } = 10f;


            /// <summary>
            /// Time to live until last player left. Server instance will automatically shut down
            /// when room is empty after last player left.
            /// </summary>
            public static float TtlEmptyRoom { get; set; } = 0f;


            /// <summary>
            /// Is first player joined?
            /// </summary>
            public static bool IsFirstPlayerJoined { get; private set; }

            public static bool IsRoomRegistered { get; private set; }

            private static bool m_isInitialized = false;
            private static MskProperties m_args = new MskProperties();
            public static MskProperties Args { get { return m_args; } }


            // Callbacks
            public static OnClientAcceptedOnMaster onClientAcceptedOnMaster;
            public static OnConnectedToMaster onConnectedToMaster;
            public static OnConnectToMasterFailed onConnectToMasterFailed;

            public static OnRoomRegistered onRoomRegistered;
            public static OnPlayerJoined onPlayerJoined;
            public static OnPlayerLeft onPlayerLeft;
            public static OnPlayerKicked onPlayerKicked;
            public static OnMasterChanged onMasterChanged;
            public static OnRoomPropertiesUpdated onRoomPropertiesUpdated;
            public static OnRoomCustomPropertiesUpdated onRoomCustomPropertiesUpdated;
            public static OnPlayerNicknameUpdated onPlayerNicknameUpdated;
            public static OnPlayerCustomPropertiesUpdated onPlayerCustomPropertiesUpdated;
            public static OnMessageReceived onMessageReceived;
            public static OnSendMessageSuccess onSendMessageSuccess;
            public static OnSendMessageFailed onSendMessageFailed;

#if UNITY_EDITOR

            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
            private static void CleanUp()
            {
                m_isInitialized = false;
            }
#endif
            #region Initialize

            public static void Initialize()
            {
                if (m_isInitialized)
                {
                    return;
                }

                m_isInitialized = true;

                IsInstance = true;
                IsClient = false;

                InitializeArgs();
                InitailizeProperties();

                MskInstanceMono.Initialize();
                MskDispatcher.Initialize();

                MskSocket.PacketHandlers = new Dictionary<int, PacketHandler>
                {
                {(int)OpResponse.OnConnectedToMaster, OnConnectedToMaster },
                {(int)OpResponse.OnClientAcceptedOnMaster, OnClientAcceptedOnMaster },
                {(int)OpResponse.OnConnectToMasterFailed, OnConnectToMasterFailed },
                {(int)OpResponse.OnRoomRegistered, OnRoomRegistered },
                {(int)OpResponse.OnPlayerJoined, OnPlayerJoined },
                {(int)OpResponse.OnPlayerLeft, OnPlayerLeft },
                {(int)OpResponse.OnPlayerKicked, OnPlayerKicked },
                {(int)OpResponse.OnMasterChanged, OnMasterChanged },
                {(int)OpResponse.OnRoomPropertiesUpdated, OnRoomPropertiesUpdated},
                {(int)OpResponse.OnNicknameUpdated, OnNicknameUpdated },
                {(int)OpResponse.OnPlayerCustomPropertiesUpdated, OnPlayerCustomPropertiesUpdated },
                {(int)OpResponse.OnMessageReceived, OnMessageReceived},
                {(int)OpResponse.OnSendMessageFailed, OnSendMessageFailed},
                {(int)OpResponse.OnSendMessageSuccess, OnSendMessageSuccess}
                };
            }

            public static void InitializeArgs()
            {
                string[] args = Environment.GetCommandLineArgs();
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].StartsWith("-"))
                    {
                        string key = args[i].Substring(1, args[i].Length - 1);
                        Instance.m_args.Add(key, args[i + 1]);

                        //Debug.Log($"arg{i} - {key}: {args[i + 1]}" );
                    }
                }
            }

            private static void InitailizeProperties()
            {
                try
                {
                    Version = m_args.GetString("version");
                 
                    string temp = Instance.m_args.GetString("roomOptions");
                    temp = temp.Replace("`", "\\\"");

                    Instance.m_args.Add("roomOptions", temp);
                    Debug.Log(Instance.m_args.GetString("roomOptions"));

                    RoomOptions roomOptions = Utilities.FromJson<RoomOptions>(temp);
                    Room = new MskRoom
                    {
                        Name = m_args.GetString("roomName"),
                        MaxPlayers = roomOptions.maxPlayers,
                        IsPrivate = roomOptions.isPrivate,
                        IsOpen = roomOptions.isOpen,
                        CustomProperties = roomOptions.customProperties,
                        IsPasswordLock = !string.IsNullOrEmpty(roomOptions.password)
                    };
                }
                catch
                {
                    Application.Quit();
                }
            }

            #endregion

            #region Connect To Master

            /// <summary>
            /// Try connecting to the master server with given ip and port.
            /// </summary>
            /// <param name="ip"></param>
            /// <param name="port"></param>
            public static void ConnectToMaster(string ip, ushort port)
            {
                if (IsConnected)
                {
                    return;
                }

                Initialize();

                MasterServerKit.Version = MskConfigClient.Version;
                MasterServerKit.Socket.onDisconnected += OnDisconnectedFromMaster;

                Socket.Connect(ip, port);

                Console.Write("" +
                    "___  ___  ___   _____  _____  _____ ______   _____  _____ ______  _   _  _____ ______   _   __ _____  _____\n" +
                    "|  \\/  | / _ \\ /  ___||_   _||  ___|| ___ \\ /  ___||  ___|| ___ \\| | | ||  ___|| ___ \\ | | / /|_   _||_   _|\n" +
                    "| .  . |/ /_\\ \\\\ `--.   | |  | |__  | |_/ / \\ `--. | |__  | |_/ /| | | || |__  | |_/ / | |/ /   | |    | |  \n" +
                    "| |\\/| ||  _  | `--. \\  | |  |  __| |    /   `--. \\|  __| |    / | | | ||  __| |    /  |    \\   | |    | |  \n" +
                    "| |  | || | | |/\\__/ /  | |  | |___ | |\\ \\  /\\__/ /| |___ | |\\ \\ \\ \\_/ /| |___ | |\\ \\  | |\\  \\ _| |_   | |  \n" +
                    "\\_|  |_/\\_| |_/\\____/   \\_/  \\____/ \\_| \\_| \\____/ \\____/ \\_| \\_| \\___/ \\____/ \\_| \\_| \\_| \\_/ \\___/   \\_/  \n");

                Console.WriteLine("-> Connecting to Master Server.");
            }

            /// <summary>
            /// Try connecting to the master server with given ip and port.
            /// </summary>
            /// <param name="ip"></param>
            /// <param name="port"></param>
            public static void ConnectToMaster()
            {
                if (IsConnected)
                {
                    return;
                }

                Initialize();

                MasterServerKit.Version = MskConfigClient.Version;
                MasterServerKit.Socket.onDisconnected += OnDisconnectedFromMaster;

                string ip = m_args.GetString("masterIp");
                ushort port = (ushort)m_args.GetShort("masterPort");

                Socket.Connect(ip, port);

                Console.Write("" +
                    "___  ___  ___   _____  _____  _____ ______   _____  _____ ______  _   _  _____ ______   _   __ _____  _____\n" +
                    "|  \\/  | / _ \\ /  ___||_   _||  ___|| ___ \\ /  ___||  ___|| ___ \\| | | ||  ___|| ___ \\ | | / /|_   _||_   _|\n" +
                    "| .  . |/ /_\\ \\\\ `--.   | |  | |__  | |_/ / \\ `--. | |__  | |_/ /| | | || |__  | |_/ / | |/ /   | |    | |  \n" +
                    "| |\\/| ||  _  | `--. \\  | |  |  __| |    /   `--. \\|  __| |    / | | | ||  __| |    /  |    \\   | |    | |  \n" +
                    "| |  | || | | |/\\__/ /  | |  | |___ | |\\ \\  /\\__/ /| |___ | |\\ \\ \\ \\_/ /| |___ | |\\ \\  | |\\  \\ _| |_   | |  \n" +
                    "\\_|  |_/\\_| |_/\\____/   \\_/  \\____/ \\_| \\_| \\____/ \\____/ \\_| \\_| \\___/ \\____/ \\_| \\_| \\_| \\_/ \\___/   \\_/  \n");
                Console.WriteLine("-> Connecting to Master Server.");
            }

            private static void OnClientAcceptedOnMaster(MskSocket client, Packet packet)
            {
                int clientId = packet.ReadInt();

                using (Packet p = new Packet((int)OpRequest.ConnectToMaster))
                {
                    p.Write(Version);
                    p.Write(false);
                    p.Write(clientId);
                    p.Write("");

                    Socket.SendData(p);
                }

                onClientAcceptedOnMaster?.Invoke();
            }

            private static void OnConnectedToMaster(MskSocket client, Packet packet)
            {
                IsConnected = true;

                int id = packet.ReadInt();

                Socket.ClientId = id;

                Console.WriteLine($"-> Connected to Master Server on {m_args.GetString("masterIp")}:{m_args.GetString("masterPort")}");

                MskInstanceMono.StartTtlFirstPlayerRoutine();
                MskInstanceMono.StartTtlEmptyRoomRoutine();

                onConnectedToMaster?.Invoke();
            }

            private static void OnConnectToMasterFailed(MskSocket client, Packet packet)
            {
                OpError opError = (OpError)packet.ReadInt();

                Console.WriteLine($"-> Connect to lobby failed : {opError}");
                onConnectToMasterFailed?.Invoke(opError);

                Application.Quit();
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

                Socket.Disconnect();
            }

            private static void OnDisconnectedFromMaster(MskSocket socket)
            {
                Console.WriteLine($"-> Disconnected from Master Server.");
                Application.Quit();
            }

            #endregion

            #region Register Room

            /// <summary>
            /// Register current room to master server.
            /// </summary>
            public static void RegisterRoom()
            {
                if (!IsConnected)
                {
                    Debug.LogError("You are not connected to the master server!");
                    return;
                }

                string ip = m_args.GetString("ip");
                ushort port = (ushort)m_args.GetShort("port");

                using (Packet packet = new Packet((int)OpRequest.RegisterRoom))
                {
                    packet.Write(Version);
                    packet.Write(Room.Name);
                    packet.Write(m_args.GetString("roomOptions"));
                    packet.Write(ip);
                    packet.Write(port);

                    Socket.SendData(packet);
                }
            }

            private static void OnRoomRegistered(MskSocket client, Packet packet)
            {
                IsRoomRegistered = true;

                Console.WriteLine("-> Room has been successfully registered.");
                onRoomRegistered?.Invoke();
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
                    Console.WriteLine($"-> Room is set to IsPrivate = {Room.IsPrivate}");
                }
                else if (op == OpRoomProperties.ChangeOpen)
                {
                    Room.IsOpen = packet.ReadBool();
                    onRoomPropertiesUpdated?.Invoke(op);
                    Console.WriteLine($"-> Room is set to IsOpen = {Room.IsOpen}");
                }
                else if (op == OpRoomProperties.ChangeMaxPlayers)
                {
                    Room.MaxPlayers = packet.ReadInt();
                    onRoomPropertiesUpdated?.Invoke(op);
                    Console.WriteLine($"-> Room is set to MaxPlayers = {Room.MaxPlayers}");
                }
                else if (op == OpRoomProperties.ChangePassword)
                {
                    Room.IsPasswordLock = packet.ReadBool();
                    onRoomPropertiesUpdated?.Invoke(op);
                    Console.WriteLine($"-> Room is set to IsPasswordLock = {Room.IsPasswordLock}");
                }
                else if (op == OpRoomProperties.UpdateCustomProperties)
                {
                    string json = packet.ReadString();
                    MskProperties mskProperties = MskProperties.Deserialize(json);
                    Room.CustomProperties.Append(mskProperties);

                    Console.WriteLine($"-> Room's custom properties updated. {json}");

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

            #endregion

            #region Player Control

            private static void OnPlayerJoined(MskSocket client, Packet packet)
            {
                IsFirstPlayerJoined = true;

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
                    Console.WriteLine($"-> Player[{player.ClientId}] kicked from room");
                    onPlayerKicked?.Invoke(player, reason);
                }
            }

            #endregion

            #region Player Properties

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

            private static void OnNicknameUpdated(MskSocket client, Packet packet)
            {
                int clientId = packet.ReadInt();
                string nickname = packet.ReadString();
                string prevNickname = packet.ReadString();

                if (Room != null)
                {
                    if (Room.Players.ContainsKey(clientId))
                    {
                        Room.Players[clientId].Nickname = nickname;
                        onPlayerNicknameUpdated?.Invoke(Room.Players[clientId], prevNickname);
                    }
                }
            }

            private static void OnPlayerCustomPropertiesUpdated(MskSocket client, Packet packet)
            {
                int clientId = packet.ReadInt();
                string json = packet.ReadString();
                MskProperties props = MskProperties.Deserialize(json);

                if (Room != null)
                {
                    if (Room.Players.ContainsKey(clientId))
                    {
                        Room.Players[clientId].CustomProperties.Append(props);
                        onPlayerCustomPropertiesUpdated?.Invoke(Room.Players[clientId], props);

                        Console.WriteLine($"-> Player[{clientId}]'s custom properties updated. {json}");
                    }
                    else
                    {
                        Console.WriteLine($"-> Cannot update Player[{clientId}]'s custom properties({json}). Player not found.");
                    }
                }
                else
                {
                    Console.WriteLine($"-> Cannot update Player[{clientId}]'s custom properties({json}). Room not found.");
                }
            }

            #endregion

            #region Lobby Info

            /// <summary>
            /// Send message to target client. If targetId = -1, message will be send
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
