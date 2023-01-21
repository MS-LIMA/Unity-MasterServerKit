using UnityEngine;

namespace MasterServerKit
{
    public class MskClientBehaviour : MonoBehaviour
    {
        protected virtual void OnEnable()
        {
            MskClient.onConnectedToMaster += OnConnectedToMaster;
            MskClient.onDisconnectedFromMaster += OnDisconncetedFromMaster;
            MskClient.onConnectedToLobby += OnConnectedToLobby;
            MskClient.onConnectToLobbyFailed += OnConnectToLobbyFailed;
            MskClient.onSpawnProcessStarted += OnSpawnProcessStarted;
            MskClient.onCreatedRoom += OnCreatedRoom;
            MskClient.onCreatRoomFailed += OnCreateRoomFailed;
            MskClient.onJoinedRoom += OnJoinedRoom;
            MskClient.onJoinRoomFailed += OnJoinRoomFailed;
            MskClient.onJoinRandomRoomFailed += OnJoinRandomRoomFailed;
            MskClient.onLeftRoom += OnLeftRoom;
            MskClient.onPlayerJoined += OnPlayerJoined;
            MskClient.onPlayerLeft += OnplayerLeft;
            MskClient.onMasterChanged += OnMasterChanged;
            MskClient.onRoomCustomPropertiesUpdated += OnRoomCustomPropertiesUpdated;
            MskClient.onNicknameUpdated += OnNicknameUpdated;
            MskClient.onPlayerCustomPropertiesUpdated += OnPlayerCustomPropertiesUpdated;
            MskClient.onPlayerCountGet += OnPlayerCountGet;
            MskClient.onPlayerListGet += OnPlayerListGet;
            MskClient.onRoomListGet += OnRoomListGet;
        }

        protected virtual void OnDisable()
        {
            MskClient.onConnectedToMaster -= OnConnectedToMaster;
            MskClient.onDisconnectedFromMaster -= OnDisconncetedFromMaster;
            MskClient.onConnectedToLobby -= OnConnectedToLobby;
            MskClient.onConnectToLobbyFailed -= OnConnectToLobbyFailed;
            MskClient.onSpawnProcessStarted += OnSpawnProcessStarted;
            MskClient.onCreatedRoom -= OnCreatedRoom;
            MskClient.onCreatRoomFailed -= OnCreateRoomFailed;
            MskClient.onJoinedRoom -= OnJoinedRoom;
            MskClient.onJoinRoomFailed -= OnJoinRoomFailed;
            MskClient.onJoinRandomRoomFailed -= OnJoinRandomRoomFailed;
            MskClient.onLeftRoom -= OnLeftRoom;
            MskClient.onPlayerJoined -= OnPlayerJoined;
            MskClient.onPlayerLeft -= OnplayerLeft;
            MskClient.onMasterChanged -= OnMasterChanged;
            MskClient.onRoomCustomPropertiesUpdated -= OnRoomCustomPropertiesUpdated;
            MskClient.onNicknameUpdated -= OnNicknameUpdated;
            MskClient.onPlayerCustomPropertiesUpdated -= OnPlayerCustomPropertiesUpdated;
            MskClient.onPlayerCountGet -= OnPlayerCountGet;
            MskClient.onPlayerListGet -= OnPlayerListGet;
            MskClient.onRoomListGet -= OnRoomListGet;
        }

        /// <summary>
        /// Invoked when connecting to master success.
        /// </summary>
        public virtual void OnConnectedToMaster()
        {

        }

        /// <summary>
        /// Invoked when disconnected from master server.
        /// </summary>
        public virtual void OnDisconncetedFromMaster()
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
        /// Invoked when creating room process started.
        /// </summary>
        public virtual void OnSpawnProcessStarted()
        {

        }

        /// <summary>
        /// Invoked when creating room success.
        /// </summary>
        /// <param name="roomName"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public virtual void OnCreatedRoom(string roomName, string ip, ushort port)
        {

        }

        /// <summary>
        /// Invoked when creating room failed.
        /// </summary>
        /// <param name="opError"></param>
        public virtual void OnCreateRoomFailed(OpError opError)
        {

        }

        /// <summary>
        /// Invoked when joining room success.
        /// </summary>
        public virtual void OnJoinedRoom()
        {

        }

        /// <summary>
        /// Invoked when joining room failed.
        /// </summary>
        /// <param name="opError"></param>
        public virtual void OnJoinRoomFailed(OpError opError)
        {

        }

        /// <summary>
        /// Invoked when joining random room failed.
        /// </summary>
        /// <param name="opError"></param>
        public virtual void OnJoinRandomRoomFailed(OpError opError)
        {

        }

        /// <summary>
        /// Invoked when left current room.
        /// </summary>
        public virtual void OnLeftRoom()
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

        /// <summary>
        /// Invoked when player count in lobby get.
        /// </summary>
        /// <param name="playerCount"></param>
        public virtual void OnPlayerCountGet(int playerCount)
        {

        }

        /// <summary>
        /// Invoked when player list in lobby get.
        /// </summary>
        /// <param name="playerInfos"></param>
        public virtual void OnPlayerListGet(PlayerInfo[] playerInfos)
        {

        }

        /// <summary>
        /// Invoked when room list in lobby get.
        /// </summary>
        /// <param name="roomInfos"></param>
        public virtual void OnRoomListGet(RoomInfo[] roomInfos)
        {

        }
    }
}