using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

namespace MasterServerKit
{
    public class MskSocket : MonoBehaviour
    {
        // Networking
        public static bool IsConnected { get; set; } = false;

        private static TCP tcp;

        public delegate void PacketHandler(Packet packet);
        private static Dictionary<int, PacketHandler> packetHandlers;

        public static Action onDisconnected;

        // Class
        #region TCP Class
        public class TCP
        {
            public TcpClient socket;

            private NetworkStream stream;
            private Packet receivedPacket;
            private byte[] receivedBuffer;

            public void Connect(string ip, ushort port)
            {
                socket = new TcpClient();

                receivedBuffer = new byte[1024];
                socket.BeginConnect(ip, port, OnConnected, socket);
            }

            public void SendData(Packet packet)
            {
                try
                {
                    if (socket != null)
                    {
                        packet.WriteLength();
                        stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                    }
                }
                catch (Exception e)
                {
                    Debug.Log($"Error : {e.Message}");
                }
            }

            private void OnConnected(IAsyncResult asyncResult)
            {
                socket.EndConnect(asyncResult);

                if (!socket.Connected)
                {
                    return;
                }

                stream = socket.GetStream();

                receivedPacket = new Packet();

                stream.BeginRead(receivedBuffer, 0, 1024, OnReceive, null);
            }

            private void OnReceive(IAsyncResult asyncResult)
            {
                try
                {
                    int byteLength = stream.EndRead(asyncResult);
                    if (byteLength <= 0)
                    {
                        MskSocket.Disconnect();
                        return;
                    }

                    byte[] data = new byte[byteLength];
                    Array.Copy(receivedBuffer, data, byteLength);

                    receivedPacket.Reset(HandleData(data));
                    stream.BeginRead(receivedBuffer, 0, 1024, OnReceive, null);
                }
                catch (Exception e)
                {
                    Disconnect();
                }
            }

            private bool HandleData(byte[] data)
            {
                int packetLength = 0;

                receivedPacket.SetBytes(data);
                if (receivedPacket.UnreadLength() >= 4)
                {
                    packetLength = receivedPacket.ReadInt();
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }

                while (packetLength > 0 && packetLength <= receivedPacket.UnreadLength())
                {
                    byte[] packetBytes = receivedPacket.ReadBytes(packetLength);
                    MskDispatcher.EnqueueCallback(() =>
                    {
                        using (Packet packet = new Packet(packetBytes))
                        {
                            int packetId = packet.ReadInt();
                            packetHandlers[packetId](packet);
                        }
                    });

                    packetLength = 0;
                    if (receivedPacket.UnreadLength() >= 4)
                    {
                        packetLength = receivedPacket.ReadInt();
                        if (packetLength <= 0)
                        {
                            return true;
                        }
                    }
                }

                if (packetLength <= 1)
                {
                    return true;
                }

                return false;
            }

            private void Disconnect()
            {
                MskSocket.Disconnect();

                stream = null;
                receivedPacket = null;
                receivedBuffer = null;
                socket = null;
            }
        }
        #endregion

        public static void Initialize(Dictionary<int, PacketHandler> packetHandlers)
        {
            MskDispatcher.Initialize();
            MskSocket.packetHandlers = packetHandlers;
        }

        public static void SendData(Packet packet)
        {
            tcp.SendData(packet);
        }

        public static void Connect(string ip, ushort port)
        {
            tcp = new TCP();
            tcp.Connect(ip, port);
        }

        public static void Disconnect()
        {
            if (IsConnected)
            {
                IsConnected = false;
                tcp.socket.Close();

                onDisconnected?.Invoke();
            }
        }
    }
}