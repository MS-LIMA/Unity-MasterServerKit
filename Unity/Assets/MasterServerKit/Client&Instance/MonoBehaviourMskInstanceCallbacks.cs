using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Msk 
{
    public class MonoBehaviourMskInstanceCallbacks : MonoBehaviour
    {
        protected virtual void OnEnable()
        {
            MasterServerKit.Instance.onClientAcceptedOnMaster += OnClientAcceptedOnMaster;
            MasterServerKit.Instance.onConnectedToMaster += OnConnectedToMaster;
            MasterServerKit.Instance.onConnectToMasterFailed += OnConnectToMasterFailed;
            MasterServerKit.Instance.onRoomRegistered += OnRoomRegistered;
            MasterServerKit.Instance.onPlayerJoined += OnPlayerJoined;
            MasterServerKit.Instance.onPlayerLeft += OnPlayerLeft;
            MasterServerKit.Instance.onPlayerKicked += OnPlayerKicked;
            MasterServerKit.Instance.onMasterChanged += OnMasterChanged;
            MasterServerKit.Instance.onRoomPropertiesUpdated += OnRoomPropertiesUpdated;
            MasterServerKit.Instance.onRoomCustomPropertiesUpdated += OnRoomCustomPropertiesUpdated;
            MasterServerKit.Instance.onPlayerNicknameUpdated += OnPlayerNicknameUpdated;
            MasterServerKit.Instance.onPlayerCustomPropertiesUpdated += OnPlayerCustomPropertiesUpdated;
            MasterServerKit.Instance.onMessageReceived += OnMessageReceived;
            MasterServerKit.Instance.onSendMessageSuccess += OnSendMessageSuccess;
            MasterServerKit.Instance.onSendMessageFailed += OnSendMessageFailed;
        }

        protected virtual void OnDisable()
        {
            MasterServerKit.Instance.onClientAcceptedOnMaster -= OnClientAcceptedOnMaster;
            MasterServerKit.Instance.onConnectedToMaster -= OnConnectedToMaster;
            MasterServerKit.Instance.onConnectToMasterFailed -= OnConnectToMasterFailed;
            MasterServerKit.Instance.onRoomRegistered -= OnRoomRegistered;
            MasterServerKit.Instance.onPlayerJoined -= OnPlayerJoined;
            MasterServerKit.Instance.onPlayerLeft -= OnPlayerLeft;
            MasterServerKit.Instance.onPlayerKicked -= OnPlayerKicked;
            MasterServerKit.Instance.onMasterChanged -= OnMasterChanged;
            MasterServerKit.Instance.onRoomPropertiesUpdated -= OnRoomPropertiesUpdated;
            MasterServerKit.Instance.onRoomCustomPropertiesUpdated -= OnRoomCustomPropertiesUpdated;
            MasterServerKit.Instance.onPlayerNicknameUpdated -= OnPlayerNicknameUpdated;
            MasterServerKit.Instance.onPlayerCustomPropertiesUpdated -= OnPlayerCustomPropertiesUpdated;
            MasterServerKit.Instance.onMessageReceived -= OnMessageReceived;
            MasterServerKit.Instance.onSendMessageSuccess -= OnSendMessageSuccess;
            MasterServerKit.Instance.onSendMessageFailed -= OnSendMessageFailed;
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
        /// Invoked when registering room success.
        /// </summary>
        public virtual void OnRoomRegistered()
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
        public virtual void OnSendMessageFailed(string target, string message, OpError opError)
        {
        }
    }
}