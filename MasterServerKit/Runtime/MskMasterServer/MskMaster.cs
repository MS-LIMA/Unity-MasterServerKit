using System.Collections.Generic;
using System.Net.Sockets;
using System;
using System.Net;

using TCP = MasterServerKit.Master.MskClient.TCP;

namespace MasterServerKit.Master
{
    public class MskMaster
    {
        private static TcpListener tcpListner;
        private static Dictionary<int, MskClient> clients = new Dictionary<int, MskClient>();
        private static Dictionary<int, MskClient> connectedClients = new Dictionary<int, MskClient>();

        #region Initialize

        private static void Initialize()
        {
            MskDispatcher.Initialize();
            MskSpawner.Initialize();

            MskClient.SetPacketHandlers(new Dictionary<int, MskClient.PacketHandler>
            {
                {(int)OpRequest.ConnectToLobby, MskMaster.OnConnectToLobbyRequested },
                {(int)OpRequest.CreateRoom, MskMaster.OnCreateRoomRequested },
                {(int)OpRequest.RegisterRoom, MskMaster.OnRoomRegisterRequested },
                {(int)OpRequest.JoinRoom, MskMaster.OnJoinRoomRequested },
                {(int)OpRequest.JoinRandomRoom, MskMaster.OnJoinRoomRequested },
                {(int)OpRequest.LeaveRoom, MskMaster.OnLeaveRoomRequested},
                {(int)OpRequest.UpdateRoomProperties, MskMaster.OnUpdateRoomPropertiesRequested },
                {(int)OpRequest.SetNickname, MskMaster.OnUpdateNicknameRequested },
                {(int)OpRequest.SetPlayerCustomProperties, MskMaster.OnUpdateCustomPropertiesRequested},
                {(int)OpRequest.SetMaster, MskMaster.OnSetMasterRequested },
                {(int)OpRequest.GetPlayerCount, MskMaster.OnGetPlayerCountRequested },
                {(int)OpRequest.GetPlayerList, MskMaster.OnGetPlayerListRequested },
                {(int)OpRequest.GetRoomList, MskMaster.OnGetRoomListRequested }
            });

            for (int i = 0; i < MskConfig.Instance.maxConnections; i++)
            {
                clients.Add(i, new MskClient(i));
            }
        }

        #endregion

        #region Socket Control

        public static void Start()
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

            string host = MskConfig.Instance.masterServerIp;
            ushort port = MskConfig.Instance.masterServerPort;

            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse(host), port);
        
            try
            {
                tcpListner = new TcpListener(iPEndPoint);
                tcpListner.Start();
                tcpListner.BeginAcceptTcpClient(new AsyncCallback(OnClientConnected), tcpListner);

                Console.WriteLine($"-> Master server started on {host}:{port}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"-> Error : {e.Message}");
            }
        }

        private static void OnClientConnected(IAsyncResult asyncResult)
        {
            TcpClient client = tcpListner.EndAcceptTcpClient(asyncResult);
            tcpListner.BeginAcceptTcpClient(new AsyncCallback(OnClientConnected), null);

            //Console.WriteLine($"Incoming connection from {client.Client.RemoteEndPoint}...");

            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].tcp.socket == null)
                {
                    clients[i].tcp.Connect(client);

                    if (!connectedClients.ContainsKey(i))
                    {
                        connectedClients.Add(i, clients[i]);
                    }

                    OnClientConnectedToMaster(clients[i].clientId);

                    return;
                }
            }

            using (Packet packet = new Packet((int)OpResponse.OnConnectedToMasterFailed))
            {
                packet.WriteLength();
                client.GetStream().BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
            }

            client.Close();

            Console.WriteLine($"-> {client.Client.RemoteEndPoint} failed to connect : Max connections reached.");
        }

        public static void OnClientDisconnected(int id)
        {
            if (connectedClients.ContainsKey(id))
            {
                connectedClients.Remove(id);
                OnClientDisconnectedFromMaster(id);
            }
        }

        public static MskClient FindClient(int id)
        {
            return clients[id];
        }

        #endregion

        #region Connection To Master

        private static void OnClientConnectedToMaster(int clientId)
        {
            Console.WriteLine($"-> Client[{clientId}] has been connected : Clients count {connectedClients.Count}");

            using (Packet packet = new Packet((int)OpResponse.OnConnectedToMaster))
            {
                packet.Write(clientId);
                clients[clientId].tcp.SendData(packet);
            }
        }

        private static void OnClientDisconnectedFromMaster(int clientId)
        {
            Console.WriteLine($"-> Client[{clientId}] has been disconnected : Clients count {connectedClients.Count}");

            MskClient client = FindClient(clientId);

            // Check disconnected client is a player.
            if (MskLobby.IsPlayerClient(client))
            {
                RemovePlayerClient(clientId);
            }

            // Check disconnected client is a room.
            if (MskLobby.IsRoomClient(client))
            {
                RemoveRoomClient(clientId);
            }

            // If disconnected client is connected to lobby, then
            if (MskLobby.ContainsClient(client))
            {
                // Remove from lobby.
                MskLobby.RemoveClient(client);
            }
        }

        private static void RemovePlayerClient(int clientId)
        {
            MskClient client = FindClient(clientId);

            // Remove player client.
            MskLobby.RemovePlayerClient(client);

            // If client has requested creating rooms which are not registered, then
            if (MskSpawner.IsClientRequestedCreateRoom(clientId))
            {
                // Abort creating room.
                MskSpawner.AbortCreateRoom(clientId);
            }
        }

        private static void RemoveRoomClient(int clientId)
        {
            MskClient client = FindClient(clientId);

            // Remove room client.
            MskLobby.RemoveRoomClient(client);

            // Remove room process from spawner
            MskSpawner.RemoveRoomProcess(clientId);
        }

        #endregion

        #region Connect To Lobby
        public static void OnConnectToLobbyRequested(TCP tcp, Packet packet)
        {
            string version = packet.ReadString();
            bool isPlayer = packet.ReadBool();

            // Check client is connected to lobby
            MskClient client = FindClient(tcp.clientId);
            if (MskLobby.ContainsClient(client))
            {
                using (Packet p = new Packet((int)OpResponse.OnConnectToLobbyFailed))
                {
                    p.Write((int)OpError.AlreadyInLobby);
                    tcp.SendData(p);
                }

                return;
            }

            // Add client to lobby.
            MskLobby.AddClient(client, version);
            MskLobby.AddClientToLobby(client, version);

            // If client is a player, then
            if (isPlayer)
            {
                string nickname = packet.ReadString();
                string customProps = packet.ReadString();
                MskProperties customProperties = MskProperties.Deserialize(customProps);

                MskPlayer player = new MskPlayer(client, version, nickname, customProperties);

                // Add player to lobby.
                MskLobby.AddPlayerClient(client, version);
                MskLobby.AddPlayerToLobby(player, version);

                using (Packet p = new Packet((int)OpResponse.OnConnectedToLobby))
                {
                    tcp.SendData(p);
                }
            }
            // If client is a room instance, then
            else
            {
                // Add room client to lobby.
                MskLobby.AddRoomClient(client, version);

                using (Packet p = new Packet((int)OpResponse.OnConnectedToLobby))
                {
                    tcp.SendData(p);
                }
            }
        }

        #endregion

        #region Create & Register Room

        public static void OnCreateRoomRequested(TCP tcp, Packet packet)
        {
            string version = packet.ReadString();
            string roomName = packet.ReadString();
            string roomOptions = packet.ReadString();

            // Check lobby exsits.
            if (!MskLobby.IsLobbyExist(version))
            {
                using (Packet p = new Packet((int)OpResponse.OnCreateRoomFailed))
                {
                    p.Write((int)OpError.LobbyNotFound);
                    tcp.SendData(p);
                }

                return;
            }

            // Check room name is null.
            if (string.IsNullOrEmpty(roomName))
            {
                // Check room number maximum reached
                if (MskLobby.IsRoomNumberMaxReached(version))
                {
                    using (Packet p = new Packet((int)OpResponse.OnCreateRoomFailed))
                    {
                        p.Write((int)OpError.LobbyNotFound);
                        tcp.SendData(p);
                    }
                    return;
                }
            }

            // Check room name is duplicated.
            if (MskLobby.IsRoomNameDuplicated(version, roomName))
            {
                using (Packet p = new Packet((int)OpResponse.OnCreateRoomFailed))
                {
                    p.Write((int)OpError.RoomNameDuplicated);
                    tcp.SendData(p);
                }
                return;
            }

            // Create room server instance.
            MskSpawner.CreateRoomInstance(tcp, version, roomName, roomOptions, (success, opError) =>
            {
                if (success)
                {
                    using (Packet p = new Packet((int)OpResponse.OnSpawnProcessStarted))
                    {
                        tcp.SendData(p);
                    }
                    return;
                }

                using (Packet p = new Packet((int)OpResponse.OnCreateRoomFailed))
                {
                    p.Write((int)opError);
                    tcp.SendData(p);
                }
            });         
        }

        public static void OnRoomRegisterRequested(TCP tcp, Packet packet)
        {
            string version = packet.ReadString();
            string roomName = packet.ReadString();
            string roomOptions = packet.ReadString();
            string ip = packet.ReadString();
            ushort port = (ushort)packet.ReadShort();

            // Check spawn request is aborted.
            if (MskSpawner.IsSpawnRequestAborted(version, roomName))
            {
                MskSpawner.RemoveSpawnProcess(version, roomName);
                return;
            }


            MskClient client = FindClient(MskSpawner.FindClientRequestCreateRoom(version, roomName));

            // Check lobby exsits.
            if (!MskLobby.IsLobbyExist(version))
            {
                if (client != null)
                {
                    using (Packet p = new Packet((int)OpResponse.OnCreateRoomFailed))
                    {
                        p.Write((int)OpError.LobbyNotFound);
                        client.tcp.SendData(p);
                    }
                }

                MskSpawner.RemoveSpawnProcess(version, roomName);
                return;
            }

            // Check room name is duplicated.
            if (MskLobby.IsRoomNameDuplicated(version, roomName))
            {
                if (client != null)
                {
                    using (Packet p = new Packet((int)OpResponse.OnCreateRoomFailed))
                    {
                        p.Write((int)OpError.RoomNameDuplicated);
                        client.tcp.SendData(p);
                    }
                }

                MskSpawner.RemoveSpawnProcess(version, roomName);
                return;
            }



            // Create room in requested version and room name.
            MskLobby.CreateRoom(FindClient(tcp.clientId), version, roomName, roomOptions, ip, port);

            // Set room process
            MskSpawner.SetRoomProcess(tcp, version, roomName, (clientId) =>
            {
                // Send success packet to room instance.
                using (Packet p = new Packet((int)OpResponse.OnRoomRegistered))
                {
                    tcp.SendData(p);
                }

                // Send success packet to client who requested creating room.
                MskClient client = FindClient(clientId);
                if (client != null)
                {
                    using (Packet p = new Packet((int)OpResponse.OnCreatedRoom))
                    {
                        p.Write(roomName);
                        p.Write(ip);
                        p.Write(port);

                        client.tcp.SendData(p);
                    }
                }
            });
        }

        #endregion

        #region Join Room

        public static void OnJoinRoomRequested(TCP tcp, Packet packet)
        {
            string version = packet.ReadString();
            string roomName = packet.ReadString();
            string password = packet.ReadString();

            bool isRandomJoin = string.IsNullOrEmpty(roomName);

            // Check lobby exists.
            if (!MskLobby.IsLobbyExist(version))
            {
                using (Packet p = new Packet(isRandomJoin ? (int)OpResponse.OnJoinRandomRoomFailed : (int)OpResponse.OnJoinRoomFailed))
                {
                    p.Write((int)OpError.LobbyNotFound);
                    tcp.SendData(p);
                }

                return;
            }

            MskClient client = FindClient(tcp.clientId);

            // Join Room
            MskLobby.JoinRoom(client, version, roomName, password, (success, room, opError) =>
            {
                if (success)
                {
                    string json = JsonSerializer.ToJson(room);
                    using (Packet p = new Packet((int)OpResponse.OnJoinedRoom))
                    {
                        p.Write(json);
                        tcp.SendData(p);
                    }

                    return;
                }

                using (Packet p = new Packet(isRandomJoin ? (int)OpResponse.OnJoinRandomRoomFailed : (int)OpResponse.OnJoinRoomFailed))
                {
                    p.Write((int)opError);
                    tcp.SendData(p);
                }
            });
        }

        #endregion

        #region Leave Room

        public static void OnLeaveRoomRequested(TCP tcp, Packet packet)
        {
            string version = packet.ReadString();
            string roomName = packet.ReadString();

            MskClient client = FindClient(tcp.clientId);

            // Leave Room
            MskLobby.LeaveRoom(client, version, roomName);

            // Send left room packet to client.
            using (Packet p = new Packet((int)OpResponse.OnLeftRoom))
            {
                tcp.SendData(p);
            }
        }

        #endregion

        #region Room Properties
        private static void OnUpdateRoomPropertiesRequested(TCP tcp, Packet packet)
        {
            string version = packet.ReadString();
            string roomName = packet.ReadString();

            OpRoomProperties op = (OpRoomProperties)packet.ReadInt();
            if (op == OpRoomProperties.ChangeMaxPlayers)
            {
                MskLobby.UpdateRoomProperties(version, roomName, packet.ReadInt(), op);
            }
            else if (op == OpRoomProperties.ChangePassword)
            {
                MskLobby.UpdateRoomProperties(version, roomName, packet.ReadString(), op);
            }
            else if (op == OpRoomProperties.ChangePrivate)
            {
                MskLobby.UpdateRoomProperties(version, roomName, packet.ReadBool(), op);
            }
            else if (op == OpRoomProperties.ChangeOpen)
            {
                MskLobby.UpdateRoomProperties(version, roomName, packet.ReadBool(), op);
            }
            else if (op == OpRoomProperties.UpdateCustomProperties)
            {
                MskLobby.UpdateRoomProperties(version, roomName, packet.ReadString(), op);
            }
        }

        #endregion

        #region Player Properties
        private static void OnUpdateNicknameRequested(TCP tcp, Packet packet)
        {
            string version = packet.ReadString();
            string nickname = packet.ReadString();

            MskClient client = FindClient(tcp.clientId);

            // Find player
            MskPlayer player = MskLobby.FindPlayer(client);
            if (player != null)
            {
                player.Nickname = nickname;
                player.playerInfo.Nickname = nickname;

                // Send success packet to given client.
                using (Packet p = new Packet((int)OpResponse.OnNicknameUpdated))
                {
                    p.Write(player.Id);
                    p.Write(nickname);

                    tcp.SendData(p);
                }

                // If player is in room, then
                if (player.InRoom)
                {
                    // Send success packet to other clients in room.
                    MskLobby.UpdateNicknameInRoom(version, player.RoomName, player.Id, nickname);
                }
            }
        }

        private static void OnUpdateCustomPropertiesRequested(TCP tcp, Packet packet)
        {
            string version = packet.ReadString();
            int clientId = packet.ReadInt();
            string json = packet.ReadString();

            MskProperties properties = MskProperties.Deserialize(json);

            MskClient client = FindClient(clientId);

            // Find player.
            MskPlayer player = MskLobby.FindPlayer(client);
            if (player != null)
            {
                player.CustomProperties.Append(properties);

                // Send success packet to given client.
                using (Packet p = new Packet((int)OpResponse.OnPlayerCustomPropertiesUpdated))
                {
                    p.Write(player.Id);
                    p.Write(json);

                    tcp.SendData(p);
                }

                // If player is in room, then
                if (player.InRoom)
                {
                    // Send success packet to other clients in room.
                    MskLobby.UpdatePlayerCustomPropertiesInRoom(version, player.RoomName, player.Id, json);
                }
            }
        }

        private static void OnSetMasterRequested(TCP tcp, Packet packet)
        {
            string version = packet.ReadString();
            string roomName = packet.ReadString();
            int clientId = packet.ReadInt();

            MskLobby.SetMaster(version, roomName, clientId);
        }

        #endregion

        #region Lobby Info
        private static void OnGetPlayerCountRequested(TCP tcp, Packet packet)
        {
            string version = packet.ReadString();
            MskLobby.GetPlayerCountInLobby(version, (count) =>
            {
                using (Packet packet = new Packet((int)OpResponse.OnPlayerCountGet))
                {
                    packet.Write(count);
                    tcp.SendData(packet);
                }
            });
        }

        private static void OnGetPlayerListRequested(TCP tcp, Packet packet)
        {
            string version = packet.ReadString();
            MskLobby.GetPlayerListInLobby(version, (playerInfos) =>
            {
                using (Packet packet = new Packet((int)OpResponse.OnPlayerCountGet))
                {
                    string json = JsonSerializer.ToJson(playerInfos);
                    packet.Write(json);
                    tcp.SendData(packet);
                }
            });
        }

        private static void OnGetRoomListRequested(TCP tcp, Packet packet)
        {
            string version = packet.ReadString();
            MskLobby.GetPlayerCountInLobby(version, (roomInfos) =>
            {
                using (Packet packet = new Packet((int)OpResponse.OnPlayerCountGet))
                {
                    string json = JsonSerializer.ToJson(roomInfos);
                    packet.Write(json);
                    tcp.SendData(packet);
                }
            });
        }

        #endregion
    }
}
