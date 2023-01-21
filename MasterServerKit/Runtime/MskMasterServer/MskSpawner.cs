using System.Collections.Generic;
using System;
using System.Diagnostics;

using TCP = MasterServerKit.Master.MskClient.TCP;

namespace MasterServerKit.Master
{
    public class MskSpawner
    {
        private static Dictionary<string, Dictionary<string, RoomProcess>> roomProcessesByLobby = new Dictionary<string, Dictionary<string, RoomProcess>>();
        private static Dictionary<int, RoomProcess> roomProcessesByRoomClient = new Dictionary<int, RoomProcess>();
        private static HashSet<SpawnRequest> spawnRequests = new HashSet<SpawnRequest>();

        private static Queue<ushort> ports = new Queue<ushort>();

        #region Classes

        public class RoomProcess
        {
            public Process Process { get; private set; }
            public ushort Port { get; private set; }

            public int RoomClientId { get; private set; }

            public string Version { get; private set; }
            public string RoomName { get; private set; }             

            public RoomProcess(Process process, ushort port, int roomClientId, string version, string roomName)
            {
                this.Process = process;
                this.Port = port;

                this.RoomClientId = roomClientId;

                this.Version = version;
                this.RoomName = roomName;         
            }
        }

        public class SpawnRequest
        {
            public bool IsAbort { get; set; }

            public Process Process { get; private set; }
            public ushort Port { get; private set; }

            public int ClientId { get; set; }

            public string Version { get; private set; }
            public string RoomName { get; private set; }

            public SpawnRequest(Process process, ushort port, string version, string roomName, int clientId)
            {
                this.IsAbort = false;

                this.Process = process;
                this.Port = port;

                this.ClientId = clientId;

                this.Version = version;
                this.RoomName = roomName;
            }
        }

        private static Dictionary<string, RoomProcess> CreateOrFindRoomProcesses(string version)
        {
            if (roomProcessesByLobby.ContainsKey(version))
            {
                return roomProcessesByLobby[version];
            }

            Dictionary<string, RoomProcess> roomProcesses = new Dictionary<string, RoomProcess>();
            roomProcessesByLobby.Add(version, roomProcesses);

            return roomProcesses;
        }

        private static Dictionary<string, RoomProcess> FindRoomProcesses(string version)
        {
            if (roomProcessesByLobby.ContainsKey(version))
            {
                return roomProcessesByLobby[version];
            }

            return null;
        }

        private static SpawnRequest FindSpawnRequest(string version, string roomName)
        {
            foreach (SpawnRequest s in spawnRequests)
            {
                if (s.Version == version && s.RoomName == roomName)
                {
                    return s;
                }
            }

            return null;
        }

        #endregion

        #region Initialize

        public static void Initialize()
        {
            for(int i = 0; i < MskConfig.Instance.maxInstanceCount; i++)
            {
                ports.Enqueue((ushort)(i + MskConfig.Instance.serverInstancePortStart));
            }
        }

        #endregion

        #region Spawn Control

        public static void CreateRoomInstance(TCP tcp, string version, string roomName, string roomOptions, Action<bool, OpError> callback = null)
        {
            try
            {
                // Check spawn request is duplicated by version and room name.
                if (FindSpawnRequest(version, roomName) != null)
                {
                    callback?.Invoke(false, OpError.SpawnRequestDuplicated);
                    return;
                }


                // Check maximum instance count reached.
                if (ports.Count <= 0)
                {
                    callback?.Invoke(false, OpError.MaxiumInstanceCountReached);
                    return;
                }

                string ip = MskConfig.Instance.serverInstanceIp;
                ushort port = ports.Dequeue();

                string masterIp = MskConfig.Instance.masterServerIp;
                ushort masterPort = MskConfig.Instance.masterServerPort;

                string path = $"{MskConfig.Instance.serverInstancePath}/";
                if (MskConfig.Instance.useVersionInInstancePath)
                {
                    path += $"{version}/";
                }
                path += $"{MskConfig.Instance.instanceExeName}";

                string args =
                    $"-version {version} -masterIp {masterIp} -masterPort {masterPort} " +
                    $"-ip {ip} -port {port} -roomName \"{roomName}\" " +
                    $"-roomOptions \"{roomOptions}\"";

                // Start room process
                Process process = Process.Start(path, args);

                SpawnRequest spawnRequest = new SpawnRequest(process, port, version, roomName, tcp.clientId);
                spawnRequests.Add(spawnRequest);

                callback?.Invoke(true, OpError.Success);
            }
            catch (Exception e)
            {
                Console.WriteLine("-> Error : Create room instance failed. Internal error");
                callback?.Invoke(false, OpError.InternalError);
            }
        }

        public static void SetRoomProcess(TCP tcp, string version, string roomName, Action<int> callback = null)
        {
            SpawnRequest spawnRequest = FindSpawnRequest(version, roomName);
            if (spawnRequest != null)
            {
                // Add room process to dictionaries.
                RoomProcess roomProcess = new RoomProcess(spawnRequest.Process, spawnRequest.Port, tcp.clientId, version, roomName);
                roomProcessesByRoomClient.Add(tcp.clientId, roomProcess);

                Dictionary<string, RoomProcess> roomProcesses = CreateOrFindRoomProcesses(version);
                roomProcesses.Add(roomName, roomProcess);

                // Get room create requestor id.
                int requestorId = spawnRequest.ClientId;

                // Remove spawn request from hashset.
                spawnRequests.Remove(spawnRequest);

                callback?.Invoke(requestorId);
            }
        }

        public static int FindClientRequestCreateRoom(string version, string roomName)
        {
            SpawnRequest spawnRequest = FindSpawnRequest(version, roomName);
            if (spawnRequest != null)
            {
                return spawnRequest.ClientId;
            }

            return -1;
        }

        #endregion

        #region Remove Control

        public static bool IsClientRequestedCreateRoom(int clientId)
        {
            foreach (SpawnRequest s in spawnRequests)
            {
                if (s.ClientId == clientId)
                {
                    return true;
                }
            }

            return false;
        }

        public static void AbortCreateRoom(int clientId)
        {
            foreach (SpawnRequest s in spawnRequests)
            {
                if (s.ClientId == clientId)
                {
                    s.IsAbort = true;
                }
            }
        }

        public static void RemoveRoomProcess(int roomClientId)
        {
            if (roomProcessesByRoomClient.ContainsKey(roomClientId))
            {
                RoomProcess roomProcess = roomProcessesByRoomClient[roomClientId];
                if (roomProcess != null)
                {
                    RemoveRoomProcess(roomProcess.Version, roomProcess.RoomName);

                    ports.Enqueue(roomProcess.Port);
                    roomProcessesByRoomClient.Remove(roomClientId);
                }
            }
        }

        private static void RemoveRoomProcess(string version, string roomName)
        {
            Dictionary<string, RoomProcess> roomProcesses = FindRoomProcesses(version);
            if (roomProcesses != null)
            {
                roomProcesses.Remove(roomName);
                if (roomProcesses.Count <= 0)
                {
                    roomProcessesByLobby.Remove(version);
                }
            }
        }

        public static bool IsSpawnRequestAborted(string version, string roomName)
        {
            foreach (SpawnRequest s in spawnRequests)
            {
                if (s.Version == version && s.RoomName == roomName && s.IsAbort)
                {
                    return true;
                }
            }

            return false;
        }

        public static void RemoveSpawnProcess(string version, string roomName)
        {
            SpawnRequest spawnRequest = FindSpawnRequest(version, roomName);
            if (spawnRequest != null)
            {
                spawnRequests.Remove(spawnRequest);
                ports.Enqueue(spawnRequest.Port);

                spawnRequest.Process.Kill();
            }
        }

        #endregion
    }
}
