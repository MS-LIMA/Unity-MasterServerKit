using System.Collections.Generic;
using System;

namespace Msk
{
    public class RoomOptions : IDisposable
    {
        /// <summary>
        /// Max players count of this room.
        /// </summary>
        public int maxPlayers;

        /// <summary>
        /// Should this room be private? 
        /// </summary>
        public bool isPrivate;

        /// <summary>
        /// Is room open?
        /// </summary>
        public bool isOpen;

        /// <summary>
        /// Password of this room.
        /// </summary>
        public string password;

        /// <summary>
        /// Custom properties of this room.
        /// </summary>
        public MskProperties customProperties;

        /// <summary>
        /// Custom property keys for this room in lobby.
        /// Custom properties with these keys only be shown in the lobby.
        /// </summary>
        public string[] customPropertyKeysForLobby;

        public RoomOptions()
        {
            this.maxPlayers = 20;
            this.isPrivate = false;
            this.isOpen = true;
            this.password = "";
            this.customProperties = new MskProperties();
            this.customPropertyKeysForLobby = new string[] { };
        }

        public void Dispose()
        {
            this.password = null;
            this.customProperties = null;
            this.customPropertyKeysForLobby = null;
        }
    }
}
