using UnityEngine;

namespace MasterServerKit
{
    public class MskPlayer
    {
        // Properties
        /// <summary>
        /// Player's id in master server.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Player's nickname.
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// Is local player?
        /// </summary>
        public bool IsLocal { get; set; }

        /// <summary>
        /// Player's custom properties. It will be synced in lobby and room.
        /// </summary>
        public MskProperties CustomProperties { get; set; }

        public MskPlayer(int id)
        {
            this.Id = id;
            this.Nickname = $"User[{id}]";

            this.IsLocal = false;
            this.CustomProperties = new MskProperties();
        }

        public void SetNickname(string nickname)
        {
            if (!IsLocal)
            {
                Debug.Log("Only local player's nickname can be set");
                return;
            }

            MskClient.SetNickname(nickname);
        }

        public void SetCustomProperties(MskProperties properties)
        {
            if (MskClient.Room != null)
            {
                MskClient.SetPlayerCustomProperties(this, properties);
                return;
            }

            if (MskInstance.Room != null)
            {
                MskClient.SetPlayerCustomProperties(this, properties);
                return;
            }

            if (MskClient.Player != null)
            {
                MskClient.SetPlayerCustomProperties(this, properties);
            }
        }
    }
}
