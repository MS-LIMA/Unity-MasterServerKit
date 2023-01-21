namespace MasterServerKit.Master
{
    public class PacketSender
    {
        public static void SendPacketToClients(MskClient.TCP[] tcps, Packet packet)
        {
            foreach(MskClient.TCP tcp in tcps)
            {
                using(Packet _packet = new Packet())
                {
                    _packet.Write(packet.ToArray());
                    tcp.SendData(_packet);
                }
            }
        }

        public static void SendPacketToClients(MskClient[] clients, Packet packet)
        {
            foreach (MskClient client in clients)
            {
                using (Packet _packet = new Packet())
                {
                    _packet.Write(packet.ToArray());
                    client.tcp.SendData(_packet);
                }
            }
        }

        public static void SendPacketToClient(MskClient.TCP tcp, Packet packet)
        {
            using (Packet _packet = new Packet())
            {
                _packet.Write(packet.ToArray());
                tcp.SendData(_packet);
            }
        }

        public static void SendPacketToClient(MskClient client, Packet packet)
        {
            using (Packet _packet = new Packet())
            {
                _packet.Write(packet.ToArray());
                client.tcp.SendData(_packet);
            }
        }
    }
}
