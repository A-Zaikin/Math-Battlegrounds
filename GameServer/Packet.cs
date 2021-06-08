using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    class Packet
    {
        public int SenderId;
        public PacketType Type;
        public string Data;

        public Packet(PacketType type, string data)
        {
            Type = type;
            Data = data;
        }
    }
}
