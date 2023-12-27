using Msk.Master;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Msk
{
    public class MskRoomBase
    {
        /// <summary>
        /// Name of this room.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Ip of this room.
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// Port of this room.
        /// </summary>
        public ushort Port { get; set; }

        /// <summary>
        /// Is room private? If it is true, the room will not be listed in the lobby.
        /// </summary>
        public bool IsPrivate { get; set; }

        /// <summary>
        /// Is room open?
        /// </summary>
        public bool IsOpen { get; set; }

        /// <summary>
        /// Does room have password?
        /// </summary>
        public bool IsPasswordLock { get; set; }

        /// <summary>
        /// Master client id of this room.
        /// </summary>
        public int MasterId { get; set; }

        /// <summary>
        /// Max player count of this room.
        /// </summary>
        public int MaxPlayers { get; set; }

        /// <summary>
        /// Custom properties of this room.
        /// </summary>
        public MskProperties CustomProperties { get; set; }

        public virtual string SerializeJson()
        {
            return "";
        }
    }
}