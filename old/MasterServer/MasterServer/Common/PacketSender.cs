namespace Msk
{
    public class PacketSender
    {
        public static void SendPacketToClients(MskSocket[] clients, Packet packet)
        {
            foreach (MskSocket client in clients)
            {
                using (Packet _packet = new Packet())
                {
                    _packet.Write(packet.ToArray());
                    client.TcpControl.SendData(_packet);
                }
            }
        }

        public static void SendPacketToClient(MskSocket client, Packet packet)
        {
            using (Packet _packet = new Packet())
            {
                _packet.Write(packet.ToArray());
                client.TcpControl.SendData(_packet);
            }
        }
    }
}
