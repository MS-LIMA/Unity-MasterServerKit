using System;

namespace Msk
{
    // Server
    public delegate void PacketHandler(MskSocket tcp, Packet packet);
    public delegate void RequestHandlerBase(bool success, OpError error);
}