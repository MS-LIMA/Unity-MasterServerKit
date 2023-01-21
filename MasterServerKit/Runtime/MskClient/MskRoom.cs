using System.Collections.Generic;

namespace MasterServerKit
{
    public class MskRoom
    {
        // Room Properties
        /// <summary>
        /// Current room's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Is room is private?
        /// </summary>
        public bool IsPrivate { get; set; }

        /// <summary>
        /// Is room is open?
        /// </summary>
        public bool IsOpen { get; set; }

        /// <summary>
        /// Is room is password locked?
        /// </summary>
        public bool IsPasswordLock { get; set; }

        /// <summary>
        /// Room's custom properties.
        /// </summary>
        public MskProperties CustomProperties { get; set; }


        // Player Properties
        /// <summary>
        /// Master player's id.
        /// </summary>
        public int MasterId { get; set; }

        /// <summary>
        /// Maximum player count of current room.
        /// </summary>
        public int MaxPlayers { get; set; }

        /// <summary>
        /// Player count in current room.
        /// </summary>
        public int PlayerCount { get { return Players.Count; } }

        /// <summary>
        /// Player list in current room.
        /// </summary>
        public Dictionary<int, MskPlayer> Players { get; set; }

        /// <summary>
        /// Master player in current room.
        /// </summary>
        public MskPlayer MasterPlayer
        {
            get
            {
                if (Players.ContainsKey(MasterId))
                {
                    return Players[MasterId];
                }

                return null;
            }
        }


        // Constructor
        public MskRoom()
        {
        }

        public MskRoom(string roomName, RoomOptions roomOptions)
        {
            this.Name = roomName;
            this.IsPrivate = roomOptions.isPrivate;
            this.IsOpen = roomOptions.isOpen;
            this.IsPasswordLock = !string.IsNullOrEmpty(roomOptions.password);

            this.CustomProperties = roomOptions.customProperties;

            this.MasterId = -1;
            this.MaxPlayers = roomOptions.maxPlayers;
            this.Players = new Dictionary<int, MskPlayer>();
        }

        /// <summary>
        /// Set current room to private.
        /// </summary>
        /// <param name="isPrivate"></param>
        public void SetPrivate(bool isPrivate)
        {
            if (MskClient.Room != null)
            {
                MskClient.SetPrivate(isPrivate);
            }
            else if (MskInstance.Room != null)
            {
                MskInstance.SetPrivate(isPrivate);
            }
        }

        /// <summary>
        /// Set current room to open.
        /// </summary>
        /// <param name="isOpen"></param>
        public void SetOpen(bool isOpen)
        {
            if (MskClient.Room != null)
            {
                MskClient.SetOpen(isOpen);
            }
            else if (MskInstance.Room != null)
            {
                MskInstance.SetOpen(isOpen);
            }
        }

        /// <summary>
        /// Set current room's password.
        /// </summary>
        /// <param name="password"></param>
        public void SetPassword(string password)
        {
            if (MskClient.Room != null)
            {
                MskClient.SetPassword(password);
            }
            else if (MskInstance.Room != null)
            {
                MskInstance.SetPassword(password);
            }
        }

        /// <summary>
        /// Set current room's maximum players.
        /// </summary>
        /// <param name="maxPlayers"></param>
        public void SetMaxPlayers(int maxPlayers)
        {
            if (MskClient.Room != null)
            {
                MskClient.SetMaxPlayers(maxPlayers);
            }
            else if (MskInstance.Room != null)
            {
                MskInstance.SetMaxPlayers(maxPlayers);
            }
        }

        /// <summary>
        /// Set current room's custom properties.
        /// </summary>
        /// <param name="properties"></param>
        public void SetCustomProperties(MskProperties properties)
        {
            if (MskClient.Room != null)
            {
                MskClient.SetRoomCustomProperties(properties);
            }
            else if (MskInstance.Room != null)
            {
                MskInstance.SetRoomCustomProperties(properties);
            }
        }

        /// <summary>
        /// Set master to given player.
        /// </summary>
        /// <param name="player"></param>
        public void SetMaster(MskPlayer player)
        {
            if (MskClient.Room != null)
            {
                MskClient.SetMaster(player);
            }
            else if (MskInstance.Room != null)
            {
                MskInstance.SetMaster(player);
            }
        }
    }
}