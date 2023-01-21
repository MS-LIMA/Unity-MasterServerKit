using System.Collections.Generic;
using System;

namespace MasterServerKit
{
    public class RoomOptions : IDisposable
    {
        public int maxPlayers;

        public bool isPrivate;
        public bool isOpen;

        public string password;

        public MskProperties customProperties;
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
