using System.Collections.Generic;
using System;
using System.Net.Sockets;

namespace Msk
{
    public class MskSocket
    {
        /// <summary>
        /// UUID of this socket.
        /// </summary>
        public string UUID { get; set; } = "";

        /// <summary>
        /// Client id of this socket.
        /// </summary>
        public int ClientId { get; set; } = -1;

        /// <summary>
        /// Version of this socket's game playing.
        /// </summary>
        public string Version { get; set; } = "";

        /// <summary>
        /// Tcp client control of this socket.
        /// </summary>
        public Tcp TcpControl { get; private set; }

        /// <summary>
        /// Packet handlers of this socket.
        /// </summary>
        public static Dictionary<int, PacketHandler> PacketHandlers { get; set; }

        /// <summary>
        /// Invoked when the socket is disconnected.
        /// </summary>
        public Action<MskSocket> onDisconnected;
        public Action onPacketSendComplete;


        #region Master Server Side

        /// <summary>
        /// Create new instance of <see cref="MskSocket"/>
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="packetHandlers"></param>
        public MskSocket(int clientId)
        {
            this.ClientId = clientId;
            this.TcpControl = new Tcp(this, PacketHandlers);
        }

        public void OnSocketDisconnected()
        {
            onDisconnected?.Invoke(this);
            onDisconnected = null;
            onPacketSendComplete = null;

            UUID = "";
            Version = "";

            DisconnectInternal();
        }

        /// <summary>
        /// Disconnect the socket.
        /// </summary>
        public void Disconnect()
        {
            onDisconnected?.Invoke(this);
            onDisconnected = null;
            onPacketSendComplete = null;

            UUID = "";
            Version = "";

            TcpControl.Disconnect();
        }

        private void DisconnectInternal()
        {
            TcpControl.Disconnect();
        }

        public void SetRandomUUID()
        {
            this.UUID = Guid.NewGuid().ToString();
        }

        #endregion

        #region Client Side

        /// <summary>
        /// Send a packet to the socket.
        /// </summary>
        /// <param name="packet"></param>
        public void SendData(Packet packet)
        {
            TcpControl.SendData(packet);
        }

        /// <summary>
        /// Connect the socket to the server.
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void Connect(string ip, ushort port)
        {
            TcpControl = new Tcp(this, PacketHandlers);
            TcpControl.Connect(ip, port);
        }

        #endregion

        #region Tcp

        public bool IsConnected()
        {
            try
            {
                TcpClient c = TcpControl.Socket;

                if (c != null && c.Client != null && c.Client.Connected)
                {
                    if (c.Client.Poll(0, SelectMode.SelectRead))
                        return !(c.Client.Receive(new byte[1], SocketFlags.Peek) == 0);

                    return true;
                }
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }

        public class Tcp
        {
            public TcpClient Socket { get { return m_socket; } }
            private TcpClient m_socket;

            private NetworkStream m_stream;
            private Packet m_receivedPacket;
            private byte[] m_receivedBuffer;
            private int m_bufferSize;

            private MskSocket m_client;
            private Dictionary<int, PacketHandler> m_packetHandlers;
            
            public Tcp(MskSocket client, Dictionary<int, PacketHandler> handlers, int bufferSize = 1024)
            {
                m_socket = null;
                m_stream = null;

                m_bufferSize = bufferSize;

                m_client = client;
                m_packetHandlers = handlers;
            }

            private void SetTimeout(TcpClient socket)
            {
                socket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 5000);
                socket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 5000);

                socket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                socket.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 10);
                socket.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 1);
                socket.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 5);

                //int size = sizeof(UInt32);
                //UInt32 on = 1;
                //UInt32 keepAliveInterval = 5000;   // Send a packet once every 5 seconds.
                //UInt32 retryInterval = 1000;        // If no response, resend every second.
                //byte[] inArray = new byte[size * 3];
                //Array.Copy(BitConverter.GetBytes(on), 0, inArray, 0, size);
                //Array.Copy(BitConverter.GetBytes(keepAliveInterval), 0, inArray, size, size);
                //Array.Copy(BitConverter.GetBytes(retryInterval), 0, inArray, size * 2, size);

                //socket.Client.IOControl(IOControlCode.KeepAliveValues, inArray, null);
            }

            public void Connect(TcpClient socket)
            {
                m_socket = socket;
                SetTimeout(socket);

                m_stream = socket.GetStream();
                m_receivedPacket = new Packet();
                m_receivedBuffer = new byte[m_bufferSize];

                m_stream.BeginRead(m_receivedBuffer, 0, m_bufferSize, OnReceive, null);
            }

            public void Connect(string ip, ushort port)
            {
                m_socket = new TcpClient();
                SetTimeout(m_socket);

                m_receivedBuffer = new byte[m_bufferSize];
                m_socket.BeginConnect(ip, port, OnConnected, m_socket);
            }

            private void OnConnected(IAsyncResult asyncResult)
            {
                m_socket.EndConnect(asyncResult);

                if (!m_socket.Connected)
                {
                    return;
                }

                m_stream = m_socket.GetStream();

                m_receivedPacket = new Packet();

                m_stream.BeginRead(m_receivedBuffer, 0, m_bufferSize, OnReceive, null);
            }

            public void SendData(Packet packet)
            {
                try
                {
                    if (m_socket != null)
                    {
                        packet.WriteLength();
                        m_stream.BeginWrite(packet.ToArray(), 0, packet.Length(), (async) =>
                        {
                            if (async.IsCompleted)
                            {
                                m_client.onPacketSendComplete?.Invoke();
                            }
                        }, null);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error Send Data{m_client.ClientId} : {e.Message}");
                    m_client.OnSocketDisconnected();
                }
            }

            private void OnReceive(IAsyncResult asyncResult)
            {
                try
                {
                    if (m_stream == null)
                    {
                        return;
                    }

                    int byteLength = m_stream.EndRead(asyncResult);
                    if (byteLength <= 0)
                    {
                        m_client.OnSocketDisconnected();
                        return;
                    }

                    byte[] data = new byte[byteLength];
                    Array.Copy(m_receivedBuffer, data, byteLength);

                    m_receivedPacket.Reset(HandleData(data));
                    m_stream.BeginRead(m_receivedBuffer, 0, m_bufferSize, OnReceive, null);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error Receive Data{m_client.ClientId} : {e.Message}");
                    m_client.OnSocketDisconnected();
                }
            }

            private bool HandleData(byte[] data)
            {
                int packetLength = 0;

                m_receivedPacket.SetBytes(data);
                if (m_receivedPacket.UnreadLength() >= 4)
                {
                    packetLength = m_receivedPacket.ReadInt();
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }

                while (packetLength > 0 && packetLength <= m_receivedPacket.UnreadLength())
                {
                    byte[] packetBytes = m_receivedPacket.ReadBytes(packetLength);
                    MskDispatcher.EnqueueCallback(() =>
                    {
                        using (Packet packet = new Packet(packetBytes))
                        {
                            int packetId = packet.ReadInt();
                            m_packetHandlers[packetId](m_client, packet);
                        }
                    });

                    packetLength = 0;
                    if (m_receivedPacket.UnreadLength() >= 4)
                    {
                        packetLength = m_receivedPacket.ReadInt();
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

            public void Disconnect()
            {
                try
                {
                    if (m_socket != null)
                    {
                        if (m_stream != null)
                            m_stream.Close();

                        m_socket.Close();

                        m_socket = null;
                        m_stream = null;

                        m_receivedPacket = null;
                        m_receivedBuffer = null;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error Socket Disconnection{m_client.ClientId} : {e.Message}");
                    m_client.OnSocketDisconnected();
                }
            }
        }

        #endregion
    }
}
