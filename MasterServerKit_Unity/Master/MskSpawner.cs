using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System;

namespace Msk.Master
{
    public class MskSpawner
    {
        /// <summary>
        /// Room processes by the game version.
        /// </summary>
        public static Dictionary<string, List<RoomProcess>> RoomProcesses { get; private set; } = new Dictionary<string, List<RoomProcess>>();
        public class RoomProcess
        {
            /// <summary>
            /// Process of this room
            /// </summary>
            public Process Process { get; private set; }

            /// <summary>
            /// Port of this room.
            /// </summary>
            public ushort Port { get; private set; }

            /// <summary>
            /// Room's client id.
            /// </summary>
            public int RoomClientId { get; private set; }

            /// <summary>
            /// Version of this room.
            /// </summary>
            public string Version { get; private set; }

            /// <summary>
            /// Name of this room.
            /// </summary>
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

        /// <summary>
        /// Spawn requests that the clients have requested.
        /// </summary>
        public static List<SpawnRequest> SpawnRequests { get; private set; } = new List<SpawnRequest>();
        public class SpawnRequest
        {
            /// <summary>
            /// Is spawn request aborted?
            /// </summary>
            public bool IsAbort { get; set; }

            /// <summary>
            /// Process of server instance.
            /// </summary>
            public Process Process { get; private set; }

            /// <summary>
            /// Port number of this request.
            /// </summary>
            public ushort Port { get; private set; }

            /// <summary>
            /// Version of this server intance.
            /// </summary>
            public string Version { get; private set; }

            /// <summary>
            /// Room name of this server instance.
            /// </summary>
            public string RoomName { get; private set; }

            /// <summary>
            /// The client who requested spawn a server instance. 
            /// This can be null if this client has been disconnected after the request.
            /// </summary>
            public MskSocket RequestedBy { get; private set; }

            public SpawnRequest(Process process, ushort port, MskSocket client, string version, string roomName)
            {
                this.IsAbort = false;

                this.Process = process;
                this.Port = port;

                this.RequestedBy = client;
                this.RequestedBy.onDisconnected += (socket) =>
                {
                    RequestedBy = null;
                };

                this.Version = version;
                this.RoomName = roomName;
            }
        }


        /// <summary>
        /// List of ports that can be usable.
        /// </summary>
        public static Queue<ushort> Ports { get; private set; } = new Queue<ushort>();

        /// <summary>
        /// Initialize the spawner.
        /// </summary>
        /// <param name="portStart"></param>
        /// <param name="portLength"></param>
        public static void Initialize()
        {
            for (int i = 0; i < MskConfigSpawner.MaxInstanceCount; i++)
            {
                ushort port = (ushort)(i + MskConfigSpawner.PortStart);
                Ports.Enqueue(port);
            }
        }

        /// <summary>
        /// Find the spawn request by the version and room name.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="roomName"></param>
        /// <returns></returns>
        public static SpawnRequest FindSpawnRequest(string version, string roomName)
        {
            foreach (SpawnRequest s in SpawnRequests)
            {
                if (s.Version == version && s.RoomName == roomName)
                {
                    return s;
                }
            }

            return null;
        }

        /// <summary>
        /// Find the spawn request by the client's uuid.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="roomName"></param>
        /// <returns></returns>
        public static SpawnRequest FindSpawnRequest(string uuid)
        {
            foreach (SpawnRequest s in SpawnRequests)
            {
                if (s.RequestedBy?.UUID == uuid)
                {
                    return s;
                }
            }

            return null;
        }

        private static List<RoomProcess> FindOrCreateRoomProcessesList(string version)
        {
            if (RoomProcesses.ContainsKey(version))
            {
                return RoomProcesses[version];
            }

            RoomProcesses.Add(version, new List<RoomProcess>());
            return RoomProcesses[version];
        }

        public static RoomProcess FindRoomProcess(string version, string roomName)
        {
            if (RoomProcesses.ContainsKey(version))
            {
                List<RoomProcess> processes = RoomProcesses[version];
                foreach(RoomProcess roomProcess in processes)
                {
                    if (roomProcess.RoomName == roomName)
                    {
                        return roomProcess;
                    }
                }
            }

            return null;
        }

        public static bool IsSpawnRequestAborted(string version, string roomName)
        {
            SpawnRequest spawnRequest = FindSpawnRequest(version, roomName);
            if (spawnRequest != null)
            {
                return spawnRequest.IsAbort;
            }

            return true;
        }

        public static void RemoveSpawnProcess(string version, string roomName, bool killProcess = false)
        {
            SpawnRequest spawnRequest = FindSpawnRequest(version, roomName);
            if (spawnRequest != null)
            {
                SpawnRequests.Remove(spawnRequest);

                if (killProcess) 
                {
                    spawnRequest.Process.Kill();
                }
            }
        }

        public static MskSocket GetPlayerClientRequestedCreatingRoom(string version, string roomName)
        {
            SpawnRequest spawnRequest = FindSpawnRequest(version, roomName);
            if (spawnRequest != null)
            {
                return spawnRequest.RequestedBy;
            }

            return null;
        }

        #region Spawn Control

        /// <summary>
        /// Request creating a room.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="version"></param>
        /// <param name="roomName"></param>
        /// <param name="roomOptions"></param>
        /// <param name="requestHandlerBase"></param>
        public static void RequestCreateRoom(MskSocket client, string version, string roomName, string roomOptions,
            RequestHandlerBase requestHandlerBase = null)
        {
            try
            {
                SpawnRequest spawnRequest = FindSpawnRequest(version, roomName);

                // Check spawn request is duplicated by version and room name.
                if (spawnRequest != null)
                {
                    // Is client request multiple times?
                    if (spawnRequest.RequestedBy.UUID == client.UUID)
                    {
                        requestHandlerBase?.Invoke(false, OpError.SpawnRequestDuplicated);
                    }
                    // Is room name is duplicated?
                    else
                    {
                        requestHandlerBase?.Invoke(false, OpError.RoomNameDuplicated);
                    }

                    return;
                }


                // Check room name duplicated.
                if (FindRoomProcess(version, roomName) != null)
                {
                    requestHandlerBase?.Invoke(false, OpError.RoomNameDuplicated);
                }


                // Check maximum instance count reached.
                if (Ports.Count <= 0)
                {
                    requestHandlerBase?.Invoke(false, OpError.MaxRoomCountReached);
                    return;
                }


                // Set server instance's args.
                string ip = MskConfigSpawner.Ip;
                ushort port = Ports.Dequeue();

                string masterIp = MskConfigMaster.Ip;
                ushort masterPort = MskConfigMaster.Port;
                string path = $"{MskConfigSpawner.ServerInstancePath}/" + (MskConfigSpawner.UseVersionInInstancePath ? $"{version}/" : "")
                    + MskConfigSpawner.InstanceFileName;

#if UNITY_STANDALONE_WIN
                string args =
                    $"-version {version} -masterIp {masterIp} -masterPort {masterPort} " +
                    $"-ip {ip} -port {port} -roomName \"{roomName}\" " +
                    $"-roomOptions \"{roomOptions}\"";
#elif UNITY_STANDALONE_LINUX
                roomOptions = roomOptions.Replace("\\\"", "`");
                roomOptions = roomOptions.Replace("\"", "\\\"");

                string args =
                    $"-version {version} -masterIp {masterIp} -masterPort {masterPort} " +
                    $"-ip {ip} -port {port} -roomName \"{roomName}\" " +
                    $"-roomOptions \"{roomOptions}\"";
#else
                string args = "";
#endif
                // Start room process.
                Process process = new Process();
                process.StartInfo.FileName = path;
                process.StartInfo.Arguments = args;
                process.EnableRaisingEvents = true;
                process.Exited += (sender, e) =>
                {
                    OnProcessExited(client.UUID);
                };

                process.Start();


                // Create spawn request.
                spawnRequest = new SpawnRequest(process, port, client, version, roomName);
                SpawnRequests.Add(spawnRequest);

                requestHandlerBase?.Invoke(true, OpError.Success);
            }
            catch
            {
                Console.WriteLine("-> Error : Create room instance failed. Internal error");
                requestHandlerBase(false, OpError.InternalError);
            }
        }


        public static void RegisterRoomProcess(MskSocket roomClient, string version, string roomName, 
            RequestHandlerBase requestHandlerBase = null)
        {
            // Find spawn request.
            SpawnRequest spawnRequest = FindSpawnRequest(version, roomName);
            if (spawnRequest == null)
            {
                requestHandlerBase?.Invoke(false, OpError.InternalError);
                return;
            }

            // Add room process.
            RoomProcess roomProcess = new RoomProcess(spawnRequest.Process, spawnRequest.Port, roomClient.ClientId, version, roomName);
            List<RoomProcess> roomProcesses = FindOrCreateRoomProcessesList(version);
            roomProcesses.Add(roomProcess);

            SpawnRequests.Remove(spawnRequest);

            requestHandlerBase?.Invoke(true, OpError.Success);
        }

        /// <summary>
        /// Invoked when process is exited. Managing process has exited before the room registers itself to the master server.
        /// </summary>
        /// <param name="clientUuid"></param>
        private static void OnProcessExited(string uuid)
        {
            // Check spawn request exists.
            if (FindSpawnRequest(uuid) != null)
            {
                // Notify to the client that room process has been shut down.

                MskSocket client = MasterServerKit.Master.FindClient(uuid);
                if (client != null)
                {
                    using (Packet packet = new Packet((int)OpResponse.OnCreateRoomFailed))
                    {
                        packet.Write((int)OpError.InternalError);
                        client.SendData(packet);
                    }
                }
            }
        }

        /// <summary>
        /// Is this client requested creating room?
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public static bool IsClientRequestedCreateRoom(int clientId)
        {
            foreach(SpawnRequest s in SpawnRequests)
            {
                if (s.RequestedBy.ClientId == clientId)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Abort creating room with a client id who requested.
        /// </summary>
        /// <param name="clientId"></param>
        public static void AbortCreateRoom(int clientId)
        {
            foreach (SpawnRequest s in SpawnRequests)
            {
                if (s.RequestedBy.ClientId == clientId)
                {
                    s.IsAbort = true;
                    return;
                }
            }
        }

        /// <summary>
        /// Remove room process by room client id and version.
        /// </summary>
        /// <param name="RoomClientId"></param>
        /// <param name="version"></param>
        public static void RemoveRoomProcess(int RoomClientId, string version)
        {
            List<RoomProcess> processes = RoomProcesses[version];
            for(int i = 0; i < processes.Count; i++)
            {
                RoomProcess roomProcess = processes[i];
                if (roomProcess.RoomClientId == RoomClientId)
                {
                    processes.RemoveAt(i);
                    break;
                }
            }

            if (processes.Count <= 0)
            {
                RoomProcesses.Remove(version);
            }
        }

#endregion











    }
}
