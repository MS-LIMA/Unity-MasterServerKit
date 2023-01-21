using System.Collections.Generic;
using System;
using System.Net.Sockets;

namespace MasterServerKit.Master
{
    public class MskClient
    {
        public int clientId { get; private set; }
        public TCP tcp;

        public delegate void PacketHandler(TCP tcp, Packet packet);
        private static Dictionary<int, PacketHandler> packetHandlers;

        public MskClient(int clientId)
        {
            this.clientId = clientId;
            this.tcp = new TCP(clientId);
        }

        public class TCP
        {
            public TcpClient socket { get; private set; }

            public int clientId { get; private set; }

            private NetworkStream stream;
            private Packet receivedPacket;
            private byte[] receivedBuffer;

            public TCP(int clientId)
            {
                this.socket = null;
                this.clientId = clientId;
            }

            public void Connect(TcpClient socket)
            {
                this.socket = socket;
                stream = socket.GetStream();

                receivedPacket = new Packet();
                receivedBuffer = new byte[1024];

                stream.BeginRead(receivedBuffer, 0, 1024, OnReceive, null);
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
                    Console.WriteLine($"Error : {e.Message}");
                }
            }

            private void OnReceive(IAsyncResult asyncResult)
            {
                try
                {
                    int byteLength = stream.EndRead(asyncResult);
                    if (byteLength <= 0)
                    {
                        MskMaster.FindClient(clientId).Disconnect();
                        return;
                    }

                    byte[] data = new byte[byteLength];
                    Array.Copy(receivedBuffer, data, byteLength);

                    receivedPacket.Reset(HandleData(data));
                    stream.BeginRead(receivedBuffer, 0, 1024, OnReceive, null);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error : {e.Message}");
                    MskMaster.FindClient(clientId).Disconnect();
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
                            packetHandlers[packetId](this, packet);
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

            public void Disconnect()
            {
                socket.Close();

                stream = null;
                receivedPacket = null;
                receivedBuffer = null;

                socket = null;
            }
        }

        public static void SetPacketHandlers(Dictionary<int, PacketHandler> packetHandlers)
        {
            MskClient.packetHandlers = packetHandlers;
        }

        private void Disconnect()
        {
            MskMaster.OnClientDisconnected(clientId);
            tcp.Disconnect();
        }
    }
}