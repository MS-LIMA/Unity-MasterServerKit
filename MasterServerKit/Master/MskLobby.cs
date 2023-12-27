using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Msk.Master
{
    public class MskLobby
    {
        /// <summary>
        /// The version of this lobby.
        /// </summary>
        public string Version { get; private set; }

        /// <summary>
        /// Players playing in this lobby.
        /// </summary>
        public DictionaryTwoKey<int, string, MskPlayer> Players { get; private set; }

        /// <summary>
        /// Rooms in this lobby.
        /// </summary>
        public Dictionary<string, MskRoom> Rooms { get; private set; }

        /// <summary>
        /// Socket clients in this lobby.
        /// </summary>
        public Dictionary<int, MskSocket> Clients { get; private set; }

        public MskLobby(string version)
        {
            this.Version = version;

            this.Players = new DictionaryTwoKey<int, string, MskPlayer>();
            this.Rooms = new Dictionary<string, MskRoom>();
            this.Clients = new Dictionary<int, MskSocket>();
        }

        /// <summary>
        /// Add player to the lobby.
        /// </summary>
        /// <param name="player"></param>
        public void AddPlayer(MskPlayer player)
        {
            if (!Players.Contains(player.ClientId))
            {
                Players.Add(player.ClientId, player.UUID, player);
            }
        }

        /// <summary>
        /// Remove player from the lobby.
        /// </summary>
        /// <param name="clientId"></param>
        public void RemovePlayer(int clientId)
        {
            if (Players.Contains(clientId))
            {
                Players.Remove(clientId);
            }
        }

        /// <summary>
        /// Add room to the lobby.
        /// </summary>
        /// <param name="room"></param>
        public void AddRoom(MskRoom room)
        {
            if (!Rooms.ContainsKey(room.Name))
            {
                Rooms.Add(room.Name, room);
                NotifyRoomListChanged();
            }
        }

        /// <summary>
        /// Remove room from the lobby.
        /// </summary>
        /// <param name="room"></param>
        public void RemoveRoom(MskRoom room)
        {
            if (Rooms.ContainsKey(room.Name))
            {
                Rooms.Remove(room.Name);
                NotifyRoomListChanged();

                if (room.Players.Count > 0)
                {
                    using (Packet packet = new Packet((int)OpResponse.OnLeftRoom))
                    {
                        packet.Write((int)OpError.InternalError);

                        MskSocket[] clients = room.Players.Values.Select(x => x.Client).ToArray();
                        PacketSender.SendPacketToClients(clients, packet);
                    }
                }
            }
        }

        /// <summary>
        /// Notify room list to players not in room.
        /// </summary>
        public void NotifyRoomListChanged()
        {
            MskSocket[] playerClients = Players.ToArray().Where(x => !x.InRoom).Select(x => x.Client).ToArray();
            if (playerClients.Length > 0)
            {
                RoomInfo[] roomInfos = Rooms.Values.Where(x => !x.IsPrivate).Select(x => x.RoomInfo).ToArray();
                string json = Utilities.ToJson(roomInfos);

                using (Packet packet = new Packet((int)OpResponse.OnRoomListFetched))
                {
                    packet.Write(json);
                    PacketSender.SendPacketToClients(playerClients, packet);
                }
            }
        }
    }
}