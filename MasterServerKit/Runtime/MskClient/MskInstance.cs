using System.Collections.Generic;
using System;
using UnityEngine;

namespace MasterServerKit
{
    public class MskInstance
    {
        // Properties
        /// <summary>
        /// Version of server. 
        /// </summary>
        public static string Version { get; set; }

        /// <summary>
        /// Is connected to the master serevr?
        /// </summary>
        public static bool IsConnected { get; private set; }

        /// <summary>
        /// Is connected to the lobby?
        /// </summary>
        public static bool InLobby { get; private set; }

        /// <summary>
        /// Current room's client id.
        /// </summary>
        public static int ClientId { get; private set; }


        // Room
        /// <summary>
        /// Current room.
        /// </summary>
        public static MskRoom Room { get; private set; }


        // Instance
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


        private static MskProperties args = new MskProperties();


        // Callbacks
        public static Action onConnectedToMaster;
        public static Action onConnectedToLobby;
        public static Action<OpError> onConnectToLobbyFailed;
        public static Action onRoomRegistered;
        public static Action<MskPlayer> onPlayerJoined;
        public static Action<MskPlayer> onPlayerLeft;
        public static Action<MskPlayer> onMasterChanged;
        public static Action<MskProperties> onRoomCustomPropertiesUpdated;
        public static Action<MskPlayer> onNicknameUpdated;
        public static Action<MskPlayer, MskProperties> onPlayerCustomPropertiesUpdated;

        #region Initialize
        private static void Initialize()
        {
            IsConnected = false;
            InLobby = false;

            InitializeArgs();
            InitailizeProperties();

            MskInstanceMono.Initialize();
            MskDispatcher.Initialize();
            MskSocket.Initialize(new Dictionary<int, MskSocket.PacketHandler>
            {
                {(int)OpResponse.OnConnectedToMaster, OnConnectedToMaster },
                {(int)OpResponse.OnConnectedToLobby, OnConnectedToLobby },
                {(int)OpResponse.OnConnectToLobbyFailed, OnConnectToLobbyFailed },
                {(int)OpResponse.OnRoomRegistered, OnRoomRegistered },
                {(int)OpResponse.OnPlayerJoined, OnPlayerJoined },
                {(int)OpResponse.OnPlayerLeft, OnPlayerLeft },
                {(int)OpResponse.OnMasterChanged, OnMasterChanged },
                {(int)OpResponse.OnRoomPropertiesUpdated, OnRoomPropertiesUpdated},
                {(int)OpResponse.OnNicknameUpdated, OnNicknameUpdated },
                {(int)OpResponse.OnPlayerCustomPropertiesUpdated, OnPlayerCustomPropertiesUpdated }
            });
            MskSocket.onDisconnected += OnDisconnectedFromMaster;
        }

        private static void InitializeArgs()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("-"))
                {
                    string key = args[i].Substring(1, args[i].Length - 1);
                    MskInstance.args.Add(key, args[i + 1]);
                }
            }
        }

        private static void InitailizeProperties()
        {
            Version = args.GetString("version");

            RoomOptions roomOptions = JsonSerializer.FromJson<RoomOptions>(MskInstance.args.GetString("roomOptions"));
            Room = new MskRoom(args.GetString("roomName"), roomOptions);
        }

        #endregion

        #region Connect To Master

        /// <summary>
        /// Try connecting to the master server with given ip and port.
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public static void ConnectToMasterServer(string ip, ushort port)
        {
            Initialize();

            MskSocket.Connect(ip, port);

            Console.Write("" +
                "___  ___  ___   _____  _____  _____ ______   _____  _____ ______  _   _  _____ ______   _   __ _____  _____\n" +
                "|  \\/  | / _ \\ /  ___||_   _||  ___|| ___ \\ /  ___||  ___|| ___ \\| | | ||  ___|| ___ \\ | | / /|_   _||_   _|\n" +
                "| .  . |/ /_\\ \\\\ `--.   | |  | |__  | |_/ / \\ `--. | |__  | |_/ /| | | || |__  | |_/ / | |/ /   | |    | |  \n" +
                "| |\\/| ||  _  | `--. \\  | |  |  __| |    /   `--. \\|  __| |    / | | | ||  __| |    /  |    \\   | |    | |  \n" +
                "| |  | || | | |/\\__/ /  | |  | |___ | |\\ \\  /\\__/ /| |___ | |\\ \\ \\ \\_/ /| |___ | |\\ \\  | |\\  \\ _| |_   | |  \n" +
                "\\_|  |_/\\_| |_/\\____/   \\_/  \\____/ \\_| \\_| \\____/ \\____/ \\_| \\_| \\___/ \\____/ \\_| \\_| \\_| \\_/ \\___/   \\_/  \n");
            Console.WriteLine("-> Connecting to Master Server.");
        }

        private static void OnConnectedToMaster(Packet packet)
        {
            IsConnected = true;
            MskSocket.IsConnected = true;

            int id = packet.ReadInt();
            ClientId = id;

            Console.WriteLine($"-> Connected to Master Server on {args.GetString("masterIp")}:{args.GetString("masterPort")}");

            MskInstanceMono.StartTtlFirstPlayerRoutine();
            MskInstanceMono.StartTtlEmptyRoomRoutine();

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
            Console.WriteLine($"-> Disconnected from Master Server.");
            Application.Quit();
        }

        #endregion

        #region Connect To Lobby

        /// <summary>
        /// Try connecting to the lobby. Lobby will be distinguished by version.
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
                packet.Write(false);
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

            Console.WriteLine($"-> Connect to lobby failed : {opError}");
            onConnectToLobbyFailed?.Invoke(opError);
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

            if (!InLobby)
            {
                Debug.LogError("You are not connected to the lobby!");
                return;
            }

            string ip = args.GetString("ip");
            string port = args.GetString("port");

            using (Packet packet = new Packet((int)OpRequest.RegisterRoom))
            {
                packet.Write(Version);
                packet.Write(Room.Name);
                packet.Write(args.GetString("roomOptions"));
                packet.Write(ip);
                packet.Write(port);

                MskSocket.SendData(packet);
            }
        }

        private static void OnRoomRegistered(Packet packet)
        {
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
                packet.Write(Room.Name);
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
                packet.Write(Room.Name);
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
                packet.Write(Room.Name);
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
                packet.Write(Room.Name);
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
                packet.Write(Room.Name);
                packet.Write((int)OpRoomProperties.UpdateCustomProperties);
                packet.Write(properties.SerializeJson());
                MskSocket.SendData(packet);
            }
        }

        private static void OnRoomPropertiesUpdated(Packet packet)
        {
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
            IsFirstPlayerJoined = true;

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

            MskPlayer player = Room.Players[clientId];
            Room.MasterId = clientId;

            onMasterChanged?.Invoke(player);
        }

        #endregion

        #region Player Custom Properties
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

            if (Room != null)
            {
                if (Room.Players.ContainsKey(clientId))
                {
                    Room.Players[clientId].Nickname = nickname;
                    onNicknameUpdated?.Invoke(Room.Players[clientId]);
                }
            }
        }

        private static void OnPlayerCustomPropertiesUpdated(Packet packet)
        {
            int clientId = packet.ReadInt();
            MskProperties props = MskProperties.Deserialize(packet.ReadString());

            if (Room != null)
            {
                if (Room.Players.ContainsKey(clientId))
                {
                    Room.Players[clientId].CustomProperties.Append(props);
                    onPlayerCustomPropertiesUpdated?.Invoke(Room.Players[clientId], props);
                }
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

            //if (!InLobby)
            //{
            //    Debug.LogError("You are not connected to the lobby!");
            //    return;
            //}

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

        #endregion
    }
}