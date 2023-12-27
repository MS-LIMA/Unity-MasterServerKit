using System.Collections.Generic;
using System.Net.Sockets;
using System;
using System.Net;
using System.IO;

namespace Msk.Master
{
    public partial class MasterServerKit
    {
        public class Master
        {
            private static TcpListener tcpListner;
            public static Dictionary<int, MskSocket> Clients { get; private set; } = new Dictionary<int, MskSocket>();
            public static Dictionary<int, MskSocket> ConnectedClients { get; private set; } = new Dictionary<int, MskSocket>();
            public static Config Config { get; private set; }

            private static bool m_isInitialized = false;

            #region Initialize

            private static void Initialize()
            {
                if (m_isInitialized)
                {
                    return;
                }

                m_isInitialized = true;

                string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string strWorkPath = Path.GetDirectoryName(strExeFilePath);
                string configPath = strWorkPath + "/config.json";

                if (new FileInfo(configPath).Exists)
                {
                    Console.WriteLine("-> Config Found");

                    string configJson = File.ReadAllText(configPath);
                    Config = Utilities.FromJson<Config>(configJson);
                }
                else
                {
                    Console.WriteLine("-> Config not found : Creating new config file");

                    Config = new Config();

                    string text = Utilities.ToJson(Config);
                    File.WriteAllText(configPath, text, System.Text.Encoding.Default);

                    Console.WriteLine(text);
                }
                             
                MskDispatcher.DispatchRate = Config.DispatchRate;

                MskDispatcher.Initialize();
                MskSpawner.Initialize();

                MskSocket.PacketHandlers = new Dictionary<int, PacketHandler>
                {
                    {(int)OpRequest.ConnectToMaster, OnConnectToMasterRequested },
                    {(int)OpRequest.CreateRoom, Master.OnCreateRoomRequested },
                    {(int)OpRequest.RegisterRoom, Master.OnRoomRegisterRequested },
                    {(int)OpRequest.JoinRoom, Master.OnJoinRoomRequested },
                    {(int)OpRequest.JoinRandomRoom, Master.OnJoinRoomRequested },
                    {(int)OpRequest.LeaveRoom, Master.OnLeaveRoomRequested},
                    {(int)OpRequest.UpdateRoomProperties, Master.OnUpdateRoomPropertiesRequested },
                    {(int)OpRequest.SetNickname, Master.OnSetNicknameRequested },
                    {(int)OpRequest.SetPlayerCustomProperties, Master.OnSetCustomPropertiesRequested},
                    {(int)OpRequest.SetMaster, Master.OnSetMasterRequested },
                    {(int)OpRequest.KickPlayer, Master.OnKickPlayerRequested },
                    {(int)OpRequest.FetchPlayerCount, Master.OnFetchPlayerCountRequested },
                    {(int)OpRequest.FetchPlayerCountInLobby, Master.OnFetchPlayerCountInLobbyRequested },
                    {(int)OpRequest.FetchPlayerList, Master.OnFetchPlayerListRequested },
                    {(int)OpRequest.FetchRoomList, Master.OnFetchRoomListRequested },
                    {(int)OpRequest.SendMessage, Master.OnSendMessageRequested }
                };

                for (int i = 0; i < Config.MaxConnectionsToMaster; i++)
                {
                    MskSocket client = new MskSocket(i);
                    Clients.Add(i, client);
                }
            }

            #endregion

            #region Socket Control

            /// <summary>
            /// Start the master server.
            /// </summary>
            public static void StartMasterServer()
            {
                Initialize();

                Console.Write("" +
                    "___  ___  ___   _____  _____  _____ ______   _____  _____ ______  _   _  _____ ______   _   __ _____  _____\n" +
                    "|  \\/  | / _ \\ /  ___||_   _||  ___|| ___ \\ /  ___||  ___|| ___ \\| | | ||  ___|| ___ \\ | | / /|_   _||_   _|\n" +
                    "| .  . |/ /_\\ \\\\ `--.   | |  | |__  | |_/ / \\ `--. | |__  | |_/ /| | | || |__  | |_/ / | |/ /   | |    | |  \n" +
                    "| |\\/| ||  _  | `--. \\  | |  |  __| |    /   `--. \\|  __| |    / | | | ||  __| |    /  |    \\   | |    | |  \n" +
                    "| |  | || | | |/\\__/ /  | |  | |___ | |\\ \\  /\\__/ /| |___ | |\\ \\ \\ \\_/ /| |___ | |\\ \\  | |\\  \\ _| |_   | |  \n" +
                    "\\_|  |_/\\_| |_/\\____/   \\_/  \\____/ \\_| \\_| \\____/ \\____/ \\_| \\_| \\___/ \\____/ \\_| \\_| \\_| \\_/ \\___/   \\_/  \n");

                Console.WriteLine("-> Starting master server...");

                string host = Config.MasterHost;
                ushort port = Config.MasterPort;

                IPAddress address = Config.IsHostDNS ? Dns.GetHostAddresses(host)[0] : IPAddress.Parse(host);
                IPEndPoint iPEndPoint = new IPEndPoint(address, port);

                try
                {
                    tcpListner = new TcpListener(iPEndPoint);
                    tcpListner.Start();
                    tcpListner.BeginAcceptTcpClient(new AsyncCallback(OnTcpClientConnected), tcpListner);

                    Console.WriteLine($"-> Master server started on {host}:{port}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"-> Error : {e.Message}");
                }
            }

            /// <summary>
            /// Invoked when a new tcp client has been connected.
            /// </summary>
            /// <param name="asyncResult"></param>
            private static void OnTcpClientConnected(IAsyncResult asyncResult)
            {
                try
                {
                    TcpClient client = tcpListner.EndAcceptTcpClient(asyncResult);
                    tcpListner.BeginAcceptTcpClient(new AsyncCallback(OnTcpClientConnected), null);

                    for (int i = 0; i < Clients.Count; i++)
                    {
                        // If there is a available client.
                        if (Clients[i].TcpControl.Socket == null)
                        {
                            Clients[i].TcpControl.Connect(client);
                            ConnectedClients.Add(i, Clients[i]);

                            using (Packet packet = new Packet((int)OpResponse.OnClientAcceptedOnMaster))
                            {
                                packet.Write(i);
                                Clients[i].TcpControl.SendData(packet);
                                Clients[i].onDisconnected += OnTcpClientDisconnected;
                            }

                            Console.WriteLine($"-> Client[{i}] has been accepted, waiting for authentication: Clients count {ConnectedClients.Count}");

                            return;
                        }
                    }

                    // Send a maximum client reached packet and disconnect the socket.
                    using (Packet packet = new Packet((int)OpResponse.OnConnectToMasterFailed))
                    {
                        packet.Write((int)OpError.MaxConnectionReached);
                        packet.WriteLength();
                        client.GetStream().BeginWrite(packet.ToArray(), 0, packet.Length(), (asyncReult =>
                        {
                            client.Close();
                        }), null);
                    }

                    Console.WriteLine($"-> {IPAddress.Parse(((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString())} " +
                        $"failed to connect : Max connections reached.");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"-> Error : {e.Message}");
                }
            }

            /// <summary>
            /// Invoked when a tcp client has been disconnected.
            /// </summary>
            /// <param name="client"></param>
            public static void OnTcpClientDisconnected(MskSocket client)
            {
                if (ConnectedClients.ContainsKey(client.ClientId))
                {
                    ConnectedClients.Remove(client.ClientId);

                    OnClientDisconnectedFromMaster(client);
                }
            }

            /// <summary>
            /// Find socket client by client id.
            /// </summary>
            /// <param name="id"></param>
            /// <returns></returns>
            public static MskSocket FindClient(int id)
            {
                return Clients[id];
            }

            /// <summary>
            /// Find socket client by uuid
            /// </summary>
            /// <param name="uuid"></param>
            /// <returns></returns>
            public static MskSocket FindClient(string uuid)
            {
                foreach (MskSocket client in Clients.Values)
                {
                    if (client.UUID == uuid)
                    {
                        return client;
                    }
                }

                return null;
            }

            #endregion

            #region Connection To Master

            private static void OnConnectToMasterRequested(MskSocket client, Packet packet)
            {
                string version = packet.ReadString();
                version = string.IsNullOrEmpty(version) ? "-1" : version;

                bool isPlayer = packet.ReadBool();

                int clientId = packet.ReadInt();
                string uuid = packet.ReadString();

                if (!string.IsNullOrEmpty(uuid))
                {
                    // Check auth uuid is duplicated
                    foreach (MskSocket c in ConnectedClients.Values)
                    {
                        if (c.UUID == uuid)
                        {
                            //client.onPacketSendComplete += () =>
                            //{
                            //    client.Disconnect();
                            //};

                            //using (Packet p = new Packet((int)OpResponse.OnConnectToMasterFailed))
                            //{
                            //    p.Write((int)OpError.AuthIdDuplicated);
                            //    client.SendData(p);
                            //}

                            //return;

                            Console.WriteLine($"-> New client[{client.ClientId}]'s auth id is duplicated with Client[{c.ClientId}] : Killing Client[{c.ClientId}]");

                            c.Disconnect();

                            break;
                        }
                    }

                    client.UUID = uuid;
                }
                else
                {
                    client.SetRandomUUID();
                }

                client.Version = version;

                // Add client to the lobby control and lobby.
                MskLobbyControl.AddClient(client);

                // If client is player
                if (isPlayer)
                {
                    string nickname = packet.ReadString();
                    MskProperties customProperties = MskProperties.Deserialize(packet.ReadString());

                    MskPlayer player = new MskPlayer(client, version, client.ClientId, client.UUID);
                    player.CustomProperties = customProperties;
                    player.Nickname = string.IsNullOrEmpty(nickname) ? $"User {client.ClientId}" : nickname;

                    MskLobbyControl.AddPlayerClient(client);
                    MskLobbyControl.AddPlayerToLobby(player, version);
                }
                // else if client is room instance
                else
                {
                    MskLobbyControl.AddRoomClient(client);
                }

                using (Packet p = new Packet((int)OpResponse.OnConnectedToMaster))
                {
                    p.Write(client.ClientId);
                    p.Write(client.UUID);
                    client.SendData(p);
                }

                Console.WriteLine($"-> Client[{client.ClientId}] has been connected : Clients count {ConnectedClients.Count}");
            }

            private static void OnClientDisconnectedFromMaster(MskSocket client)
            {
                Console.WriteLine($"-> Client[{client.ClientId}] has been disconnected : Clients count {ConnectedClients.Count}");

                // Check disconnected client is a player.
                if (MskLobbyControl.IsPlayerClient(client))
                {
                    RemovePlayerClient(client);
                }

                // Check disconnected client is a room.
                if (MskLobbyControl.IsRoomClient(client))
                {
                    RemoveRoomClient(client);
                }
            }

            private static void RemovePlayerClient(MskSocket client)
            {
                // Remove player client.
                MskLobbyControl.RemoveClient(client);

                // If client has requested creating rooms which are not registered, then
                if (MskSpawner.IsClientRequestedCreateRoom(client.ClientId))
                {
                    // Abort creating room.
                    MskSpawner.AbortCreateRoom(client.ClientId);
                }
            }

            private static void RemoveRoomClient(MskSocket client)
            {
                // Remove room process from spawner
                MskSpawner.RemoveRoomProcess(client.ClientId, client.Version);

                // Remove room client.
                MskLobbyControl.RemoveClient(client);
            }

            #endregion

            #region Create & Register Room

            public static void OnCreateRoomRequested(MskSocket client, Packet packet)
            {
                string version = packet.ReadString();
                string roomName = packet.ReadString();
                string roomOptions = packet.ReadString();


                // Check lobby exsits.
                if (!MskLobbyControl.IsLobbyExists(version))
                {
                    using (Packet p = new Packet((int)OpResponse.OnCreateRoomFailed))
                    {
                        p.Write((int)OpError.LobbyNotFound);
                        client.SendData(p);
                    }

                    return;
                }


                // Check room name is null.
                if (string.IsNullOrEmpty(roomName))
                {
                    using (Packet p = new Packet((int)OpResponse.OnCreateRoomFailed))
                    {
                        p.Write((int)OpError.RoomNameNull);
                        client.SendData(p);
                    }

                    return;
                }


                // Check room name is duplicated.
                if (MskLobbyControl.FindRoom(version, roomName) != null)
                {
                    using (Packet p = new Packet((int)OpResponse.OnCreateRoomFailed))
                    {
                        p.Write((int)OpError.RoomNameDuplicated);
                        client.SendData(p);
                    }
                    return;
                }


                // Create room server instance.
                MskSpawner.RequestCreateRoom(client, version, roomName, roomOptions, (success, opError) =>
                {
                    if (success)
                    {
                        using (Packet p = new Packet((int)OpResponse.OnSpawnProcessStarted))
                        {
                            client.SendData(p);
                        }
                    }
                    else
                    {
                        using (Packet p = new Packet((int)OpResponse.OnCreateRoomFailed))
                        {
                            p.Write((int)opError);
                            client.SendData(p);
                        }
                    }
                });
            }

            public static void OnRoomRegisterRequested(MskSocket client, Packet packet)
            {
                string version = packet.ReadString();
                string roomName = packet.ReadString();
                string roomOptions = packet.ReadString();
                string ip = packet.ReadString();
                ushort port = (ushort)packet.ReadShort();

                MskSocket playerClient = FindClient(MskSpawner.GetPlayerClientRequestedCreatingRoom(version, roomName));

                // Check spawn request is aborted.
                if (MskSpawner.IsSpawnRequestAborted(version, roomName) || playerClient == null)
                {
                    MskSpawner.RemoveSpawnProcess(version, roomName, true);
                    return;
                }

                // Check lobby exsits.
                if (!MskLobbyControl.IsLobbyExists(version))
                {
                    using (Packet p = new Packet((int)OpResponse.OnCreateRoomFailed))
                    {
                        p.Write((int)OpError.LobbyNotFound);
                        playerClient.SendData(p);
                    }

                    MskSpawner.RemoveSpawnProcess(version, roomName);
                    return;
                }

                // Check room name is duplicated.
                if (MskLobbyControl.FindRoom(version, roomName) != null)
                {
                    using (Packet p = new Packet((int)OpResponse.OnCreateRoomFailed))
                    {
                        p.Write((int)OpError.RoomNameDuplicated);
                        playerClient.SendData(p);
                    }

                    MskSpawner.RemoveSpawnProcess(version, roomName, true);
                    return;
                }

                // Create room in requested version and room name.
                MskLobbyControl.CreateRoom(client, version, roomName, roomOptions, ip, port);

                // Set room process
                MskSpawner.RegisterRoomProcess(client, version, roomName, (success, opError) =>
                {
                    if (success)
                    {
                        // Send success packet to room instance.
                        using (Packet p = new Packet((int)OpResponse.OnRoomRegistered))
                        {
                            client.SendData(p);
                        }

                        // Send success packet to client who requested creating room.
                        using (Packet p = new Packet((int)OpResponse.OnCreatedRoom))
                        {
                            p.Write(roomName);
                            p.Write(ip);
                            p.Write(port);

                            playerClient.SendData(p);
                        }

                        MskSpawner.RemoveSpawnProcess(version, roomName);
                    }
                    else
                    {
                        MskSpawner.RemoveSpawnProcess(version, roomName, true);
                    }
                });
            }

            #endregion

            #region Join Room

            public static void OnJoinRoomRequested(MskSocket client, Packet packet)
            {
                string version = packet.ReadString();
                string roomName = packet.ReadString();
                string password = packet.ReadString();

                bool isRandomJoin = string.IsNullOrEmpty(roomName);

                // Check lobby exists.
                if (!MskLobbyControl.IsLobbyExists(version))
                {
                    using (Packet p = new Packet(isRandomJoin ? (int)OpResponse.OnJoinRandomRoomFailed : (int)OpResponse.OnJoinRoomFailed))
                    {
                        p.Write((int)OpError.LobbyNotFound);
                        client.SendData(p);
                    }

                    return;
                }

                // Join Room
                MskLobbyControl.JoinRoom(client, version, roomName, password, (success, room, opError) =>
                {
                    if (success)
                    {
                        string json = room.SerializeJson();
                        using (Packet p = new Packet((int)OpResponse.OnJoinedRoom))
                        {
                            p.Write(json);
                            client.SendData(p);
                        }

                        return;
                    }

                    using (Packet p = new Packet(isRandomJoin ? (int)OpResponse.OnJoinRandomRoomFailed : (int)OpResponse.OnJoinRoomFailed))
                    {
                        p.Write((int)opError);
                        client.SendData(p);
                    }
                });
            }

            #endregion

            #region Leave Room

            public static void OnLeaveRoomRequested(MskSocket client, Packet packet)
            {
                string version = packet.ReadString();
                string roomName = packet.ReadString();

                // Leave Room
                MskLobbyControl.LeaveRoom(client, version, roomName);

                // Send left room packet to client.
                using (Packet p = new Packet((int)OpResponse.OnLeftRoom))
                {
                    client.SendData(p);
                }
            }

            #endregion

            #region Player Properties

            private static void OnSetNicknameRequested(MskSocket client, Packet packet)
            {
                string version = packet.ReadString();
                string nickname = packet.ReadString();

                // Find player
                MskPlayer player = MskLobbyControl.FindPlayer(client);
                if (player != null)
                {
                    string prevNickname = player.Nickname;
                    player.Nickname = nickname;

                    // Send success packet to given client.
                    using (Packet p = new Packet((int)OpResponse.OnNicknameUpdated))
                    {
                        p.Write(player.ClientId);
                        p.Write(nickname);
                        p.Write(prevNickname);

                        client.SendData(p);
                    }

                    // If player is in room, then
                    if (player.InRoom)
                    {
                        // Send success packet to other clients in room.
                        MskLobbyControl.NotifyNicknameChangedInRoom(version, player.RoomName, player.ClientId, nickname, prevNickname);
                    }
                }
            }

            private static void OnSetCustomPropertiesRequested(MskSocket client, Packet packet)
            {
                string version = packet.ReadString();
                int clientId = packet.ReadInt();
                string json = packet.ReadString();

                MskProperties properties = MskProperties.Deserialize(json);

                // Find player.
                MskPlayer player = MskLobbyControl.FindPlayer(client);
                if (player != null)
                {
                    player.CustomProperties.Append(properties);

                    // Send success packet to given client.
                    using (Packet p = new Packet((int)OpResponse.OnPlayerCustomPropertiesUpdated))
                    {
                        p.Write(player.ClientId);
                        p.Write(json);

                        client.SendData(p);
                    }

                    // If player is in room, then
                    if (player.InRoom)
                    {
                        // Send success packet to other clients in room.
                        MskLobbyControl.NotifyPlayerCustomPropertiesChangedInRoom(version, player.RoomName, player.ClientId, json);
                    }
                }
            }

            #endregion

            #region Room Properties

            private static void OnUpdateRoomPropertiesRequested(MskSocket client, Packet packet)
            {
                string version = packet.ReadString();
                string roomName = packet.ReadString();

                OpRoomProperties op = (OpRoomProperties)packet.ReadInt();
                if (op == OpRoomProperties.ChangeMaxPlayers)
                {
                    MskLobbyControl.UpdateRoomProperties(version, roomName, packet.ReadInt(), op);
                }
                else if (op == OpRoomProperties.ChangePassword)
                {
                    MskLobbyControl.UpdateRoomProperties(version, roomName, packet.ReadString(), op);
                }
                else if (op == OpRoomProperties.ChangePrivate)
                {
                    MskLobbyControl.UpdateRoomProperties(version, roomName, packet.ReadBool(), op);
                }
                else if (op == OpRoomProperties.ChangeOpen)
                {
                    MskLobbyControl.UpdateRoomProperties(version, roomName, packet.ReadBool(), op);
                }
                else if (op == OpRoomProperties.UpdateCustomProperties)
                {
                    MskLobbyControl.UpdateRoomProperties(version, roomName, packet.ReadString(), op);
                }
            }

            private static void OnSetMasterRequested(MskSocket client, Packet packet)
            {
                string version = packet.ReadString();
                string roomName = packet.ReadString();
                int clientId = packet.ReadInt();

                MskLobbyControl.SetMaster(version, roomName, clientId);
            }

            private static void OnKickPlayerRequested(MskSocket client, Packet packet)
            {
                string version = packet.ReadString();
                string roomName = packet.ReadString();
                int playerId = packet.ReadInt();
                string reason = packet.ReadString();

                MskLobbyControl.KickPlayer(version, roomName, playerId, reason);
            }

            #endregion

            #region Lobby Info

            private static void OnFetchPlayerCountRequested(MskSocket client, Packet packet)
            {
                string version = packet.ReadString();
                MskLobbyControl.GetPlayerCount(version, (count) =>
                {
                    using (Packet p = new Packet((int)OpResponse.OnPlayerCountFetched))
                    {
                        p.Write(count);
                        client.SendData(p);
                    }
                });
            }

            private static void OnFetchPlayerCountInLobbyRequested(MskSocket client, Packet packet)
            {
                string version = packet.ReadString();
                MskLobbyControl.GetPlayerCountInLobby(version, (count) =>
                {
                    using (Packet p = new Packet((int)OpResponse.OnPlayerCountInLobbyFetched))
                    {
                        p.Write(count);
                        client.SendData(p);
                    }
                });
            }

            private static void OnFetchPlayerListRequested(MskSocket client, Packet packet)
            {
                string version = packet.ReadString();
                MskLobbyControl.GetPlayerListInLobby(version, (playerInfos) =>
                {
                    using (Packet p = new Packet((int)OpResponse.OnPlayerListFetched))
                    {
                        string json = Utilities.ToJson(playerInfos);
                        p.Write(json);

                        client.SendData(p);
                    }
                });
            }

            private static void OnFetchRoomListRequested(MskSocket client, Packet packet)
            {
                string version = packet.ReadString();
                MskLobbyControl.GetRoomListInLobby(version, (roomInfos) =>
                {
                    using (Packet p = new Packet((int)OpResponse.OnRoomListFetched))
                    {
                        string json = Utilities.ToJson(roomInfos);
                        p.Write(json);

                        client.SendData(p);
                    }
                });
            }

            private static void OnSendMessageRequested(MskSocket client, Packet packet)
            {
                string version = packet.ReadString();
                string target = packet.ReadString();
                string message = packet.ReadString();

                MskLobbyControl.SendMessage(client, version, target, message, (success, opError) =>
                {
                    if (success)
                    {
                        using (Packet p = new Packet((int)OpResponse.OnSendMessageSuccess))
                        {
                            p.Write(target);
                            p.Write(message);
                            client.SendData(p);
                        }
                    }
                    else
                    {
                        using (Packet p = new Packet((int)OpResponse.OnSendMessageFailed))
                        {
                            p.Write((int)opError);
                            p.Write(target);
                            p.Write(message);
                            client.SendData(p);
                        }
                    }
                });


            }

            #endregion
        }
    }
}