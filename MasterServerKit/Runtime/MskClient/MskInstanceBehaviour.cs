using UnityEngine;

namespace MasterServerKit
{
    public class MskInstanceBehaviour : MonoBehaviour
    {
        protected virtual void OnEnable()
        {
            MskInstance.onConnectedToMaster += OnConnectedToMaster;
            MskInstance.onConnectedToLobby += OnConnectedToLobby;
            MskInstance.onConnectToLobbyFailed += OnConnectToLobbyFailed;
            MskInstance.onRoomRegistered += OnRoomRegistered;
            MskInstance.onPlayerJoined += OnPlayerJoined;
            MskInstance.onPlayerLeft += OnplayerLeft;
            MskInstance.onMasterChanged += OnMasterChanged;
            MskInstance.onRoomCustomPropertiesUpdated += OnRoomCustomPropertiesUpdated;
            MskInstance.onNicknameUpdated += OnNicknameUpdated;
            MskInstance.onPlayerCustomPropertiesUpdated += OnPlayerCustomPropertiesUpdated;
        }

        protected virtual void OnDisable()
        {
            MskInstance.onConnectedToMaster -= OnConnectedToMaster;
            MskInstance.onConnectedToLobby -= OnConnectedToLobby;
            MskInstance.onConnectToLobbyFailed -= OnConnectToLobbyFailed;
            MskInstance.onRoomRegistered -= OnRoomRegistered;
            MskInstance.onPlayerJoined -= OnPlayerJoined;
            MskInstance.onPlayerLeft -= OnplayerLeft;
            MskInstance.onMasterChanged -= OnMasterChanged;
            MskInstance.onRoomCustomPropertiesUpdated -= OnRoomCustomPropertiesUpdated;
            MskInstance.onNicknameUpdated -= OnNicknameUpdated;
            MskInstance.onPlayerCustomPropertiesUpdated -= OnPlayerCustomPropertiesUpdated;
        }

        /// <summary>
        /// Invoked when connecting to master success.
        /// </summary>
        public virtual void OnConnectedToMaster()
        {

        }

        /// <summary>
        /// Invoked when connecting to lobby success.
        /// </summary>
        public virtual void OnConnectedToLobby()
        {

        }

        /// <summary>
        /// Invoked when connecting to lobby failed.
        /// </summary>
        /// <param name="opError"></param>
        public virtual void OnConnectToLobbyFailed(OpError opError)
        {

        }

        /// <summary>
        /// Invoked when room is registered to the master server.
        /// </summary>
        public virtual void OnRoomRegistered()
        {

        }

        /// <summary>
        /// Invoked when new player joined current room.
        /// </summary>
        /// <param name="player"></param>
        public virtual void OnPlayerJoined(MskPlayer player)
        {

        }

        /// <summary>
        /// Invoked when player left current room.
        /// </summary>
        /// <param name="player"></param>
        public virtual void OnplayerLeft(MskPlayer player)
        {

        }

        /// <summary>
        /// Invoked when master changed.
        /// </summary>
        /// <param name="player"></param>
        public virtual void OnMasterChanged(MskPlayer player)
        {

        }

        /// <summary>
        /// Invoked when room's custom properties are updated.
        /// </summary>
        /// <param name="updatedProperties"></param>
        public virtual void OnRoomCustomPropertiesUpdated(MskProperties updatedProperties)
        {

        }

        /// <summary>
        /// Invoked when player's nickname updated.
        /// </summary>
        /// <param name="player"></param>
        public virtual void OnNicknameUpdated(MskPlayer player)
        {

        }

        /// <summary>
        /// Invoked when player's custom properties are updated.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="updatedProperties"></param>
        public virtual void OnPlayerCustomPropertiesUpdated(MskPlayer player, MskProperties updatedProperties)
        {

        }
    }
}