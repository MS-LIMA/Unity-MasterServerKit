using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Msk 
{
    public partial class MasterServerKit
    {
        /// <summary>
        /// Version of the master server.
        /// </summary>
        public static string Version { get; set; }

        /// <summary>
        /// Is connected to the master server?
        /// </summary>
        public static bool IsConnected { get; set; } = false;

        /// <summary>
        /// Is this server client?
        /// </summary>
        public static bool IsInstance { get; set; } = false;

        /// <summary>
        /// Is this player client?
        /// </summary>
        public static bool IsClient { get; set; } = false;

        /// <summary>
        /// Socket of this client.
        /// </summary>
        public static MskSocket Socket { get; set; } = new MskSocket(-1);

        /// <summary>
        /// Current room where this client is joined in.
        /// </summary>
        public static MskRoom Room { get; internal set; }

#if UNITY_EDITOR

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void CleanUp()
        {
            IsConnected = false;

            Room = null;

            Socket?.Disconnect();
        }

#endif


    }
}