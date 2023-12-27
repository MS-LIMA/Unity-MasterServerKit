using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Msk
{
    public class MonoBehaviourMskClientCallbacks : MonoBehaviour
    {
        protected virtual void OnEnable()
        {
            MasterServerKit.Client.onClientAcceptedOnMaster += OnClientAcceptedOnMaster;
            MasterServerKit.Client.onConnectedToMaster += OnConnectedToMaster;
            MasterServerKit.Client.onConnectToMasterFailed += OnConnectToMasterFailed;
            MasterServerKit.Client.onDisconnectedFromMaster += OnDisconnectedFromMaster;

            MasterServerKit.Client.onSpawnProcessStarted += OnSpawnProcessStarted;
            MasterServerKit.Client.onCreatedRoom += OnCreatedRoom;
            MasterServerKit.Client.onCreateRoomFailed += OnCreateRoomFailed;
            MasterServerKit.Client.onJoinedRoom += OnJoinedRoom;
            MasterServerKit.Client.onJoinRoomFailed += OnJoinRoomFailed;
            MasterServerKit.Client.onJoinRandomRoomFailed += OnJoinRandomRoomFailed;
            MasterServerKit.Client.onLeftRoom += OnLeftRoom;

            MasterServerKit.Client.onPlayerJoined += OnPlayerJoined;
            MasterServerKit.Client.onPlayerKicked += OnPlayerKicked;
            MasterServerKit.Client.onMasterChanged += OnMasterChanged;
            MasterServerKit.Client.onRoomPropertiesUpdated += OnRoomPropertiesUpdated;
            MasterServerKit.Client.onRoomCustomPropertiesUpdated += OnRoomCustomPropertiesUpdated;

            MasterServerKit.Client.onPlayerNicknameUpdated += OnPlayerNicknameUpdated;
            MasterServerKit.Client.onPlayerCustomPropertiesUpdated += OnPlayerCustomPropertiesUpdated;
            MasterServerKit.Client.onPlayerCountFetched += OnPlayerCountFetched;
            MasterServerKit.Client.onPlayerCountInLobbyFetched += OnPlayerCountInLobbyFetched;
            MasterServerKit.Client.onPlayerListFetched += OnPlayerListFetched;
            MasterServerKit.Client.onRoomListFetched += OnRoomListFetched;

            MasterServerKit.Client.onMessageReceived += OnMessageReceived;
            MasterServerKit.Client.onSendMessageSuccess += OnSendMessageSuccess;
            MasterServerKit.Client.onSendMessageFailed += OnSendMessageFailed;
        }

        protected virtual void OnDisable()
        {
            MasterServerKit.Client.onClientAcceptedOnMaster -= OnClientAcceptedOnMaster;
            MasterServerKit.Client.onConnectedToMaster -= OnConnectedToMaster;
            MasterServerKit.Client.onConnectToMasterFailed -= OnConnectToMasterFailed;
            MasterServerKit.Client.onDisconnectedFromMaster -= OnDisconnectedFromMaster;

            MasterServerKit.Client.onSpawnProcessStarted -= OnSpawnProcessStarted;
            MasterServerKit.Client.onCreatedRoom -= OnCreatedRoom;
            MasterServerKit.Client.onCreateRoomFailed -= OnCreateRoomFailed;
            MasterServerKit.Client.onJoinedRoom -= OnJoinedRoom;
            MasterServerKit.Client.onJoinRoomFailed -= OnJoinRoomFailed;
            MasterServerKit.Client.onJoinRandomRoomFailed -= OnJoinRandomRoomFailed;
            MasterServerKit.Client.onLeftRoom -= OnLeftRoom;

            MasterServerKit.Client.onPlayerJoined -= OnPlayerJoined;
            MasterServerKit.Client.onPlayerKicked -= OnPlayerKicked;
            MasterServerKit.Client.onMasterChanged -= OnMasterChanged;
            MasterServerKit.Client.onRoomPropertiesUpdated -= OnRoomPropertiesUpdated;
            MasterServerKit.Client.onRoomCustomPropertiesUpdated -= OnRoomCustomPropertiesUpdated;

            MasterServerKit.Client.onPlayerNicknameUpdated -= OnPlayerNicknameUpdated;
            MasterServerKit.Client.onPlayerCustomPropertiesUpdated -= OnPlayerCustomPropertiesUpdated;
            MasterServerKit.Client.onPlayerCountFetched -= OnPlayerCountFetched;
            MasterServerKit.Client.onPlayerCountInLobbyFetched -= OnPlayerCountInLobbyFetched;
            MasterServerKit.Client.onPlayerListFetched -= OnPlayerListFetched;
            MasterServerKit.Client.onRoomListFetched -= OnRoomListFetched;

            MasterServerKit.Client.onMessageReceived -= OnMessageReceived;
            MasterServerKit.Client.onSendMessageSuccess -= OnSendMessageSuccess;
            MasterServerKit.Client.onSendMessageFailed -= OnSendMessageFailed;
        }

        /// <summary>
        /// Invoked when a client socket is accepted on the master server.
        /// </summary>
        public virtual void OnClientAcceptedOnMaster()
        {
        }

        /// <summary>
        /// Invoked when a client is connected to the master server.
        /// Client's uuid will be set on this period.
        /// </summary>
        public virtual void OnConnectedToMaster()
        {
        }

        /// <summary>
        /// Invoked when connecting to master server failed.
        /// </summary>
        /// <param name="opError"></param>
        public virtual void OnConnectToMasterFailed(OpError opError)
        {
        }

        /// <summary>
        /// Invoked when disconnected from the master.
        /// </summary>
        public virtual void OnDisconnectedFromMaster()
        {
        }


        /// <summary>
        /// Invoked when a creating room process started in the master server.
        /// </summary>
        public virtual void OnSpawnProcessStarted()
        {
        }

        /// <summary>
        /// Invoked when room created.
        /// </summary>
        /// <param name="roomName"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public virtual void OnCreatedRoom(string roomName, string ip, ushort port)
        {
        }

        /// <summary>
        /// Invoked when create room failed.
        /// </summary>
        /// <param name="opError"></param>
        public virtual void OnCreateRoomFailed(OpError opError)
        {
        }

        /// <summary>
        /// Invoked when joined room.
        /// </summary>
        public virtual void OnJoinedRoom()
        {
        }

        /// <summary>
        /// Invoked when join room failed.
        /// </summary>
        public virtual void OnJoinRoomFailed(OpError opError)
        {
        }

        /// <summary>
        /// Invoked when join random room failed.
        /// </summary>
        /// <param name="opError"></param>
        public virtual void OnJoinRandomRoomFailed(OpError opError)
        {
        }

        /// <summary>
        /// Invoked when left room.
        /// </summary>
        public virtual void OnLeftRoom()
        {
        }

        /// <summary>
        /// Invoked when a new player joined.
        /// </summary>
        /// <param name="player"></param>
        public virtual void OnPlayerJoined(MskPlayer player)
        {
        }

        /// <summary>
        /// Invoked when a player left the room. 
        /// This method will not be called when a player is kicked from the room, instead <see cref="OnPlayerKicked"/>
        /// will be called.
        /// </summary>
        /// <param name="player"></param>
        public virtual void OnPlayerLeft(MskPlayer player)
        {
        }

        /// <summary>
        /// Invoked when a player is kicked from the room.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="reason"></param>
        public virtual void OnPlayerKicked(MskPlayer player, string reason)
        {
        }

        /// <summary>
        /// Invoked when a master has been changed.
        /// </summary>
        /// <param name="prevMaster"></param>
        /// <param name="nextMaster"></param>
        public virtual void OnMasterChanged(MskPlayer prevMaster, MskPlayer nextMaster)
        {
        }

        /// <summary>
        /// Invoked when room properties changed.
        /// </summary>
        /// <param name="opRoom"></param>
        public virtual void OnRoomPropertiesUpdated(OpRoomProperties opRoom)
        {
        }

        /// <summary>
        /// Invoked when a room's custom properties updated.
        /// </summary>
        /// <param name="props"></param>
        public virtual void OnRoomCustomPropertiesUpdated(MskProperties props)
        {
        }

        /// <summary>
        /// Invoked when a player's nickname updated.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="prevNickname"></param>
        public virtual void OnPlayerNicknameUpdated(MskPlayer player, string prevNickname)
        {
        }

        /// <summary>
        /// Invoked when a player's custom properties udpated.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="props"></param>
        public virtual void OnPlayerCustomPropertiesUpdated(MskPlayer player, MskProperties props)
        {
        }

        /// <summary>
        /// Invoked when player count fetched.
        /// </summary>
        /// <param name="playerCount"></param>
        public virtual void OnPlayerCountFetched(int playerCount)
        {
        }

        /// <summary>
        /// Invoked when player count in lobby fetched.
        /// </summary>
        /// <param name="playerCountInLobby"></param>
        public virtual void OnPlayerCountInLobbyFetched(int playerCountInLobby)
        {
        }

        /// <summary>
        /// Invoked when player list in lobby fetched.
        /// </summary>
        /// <param name="players"></param>
        public virtual void OnPlayerListFetched(PlayerInfo[] players)
        {
        }

        /// <summary>
        /// Invoked when room list fetched.
        /// </summary>
        /// <param name="rooms"></param>
        public virtual void OnRoomListFetched(RoomInfo[] rooms)
        {
        }

        /// <summary>
        /// Invoked when a message received.
        /// </summary>
        /// <param name="message"></param>
        public virtual void OnMessageReceived(string sender, string message)
        {
        }

        /// <summary>
        /// Invoked when send message success.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="message"></param>
        public virtual void OnSendMessageSuccess(string target, string message)
        {
        }

        /// <summary>
        /// Invoked when send message failed.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="message"></param>
        /// <param name="opError"></param>
        public virtual void OnSendMessageFailed(string target ,string message, OpError opError)
        {
        }









    }
}