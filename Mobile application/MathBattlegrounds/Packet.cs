namespace MathBattlegrounds
{
    class Packet
    {
        public PacketType Type;
        public string Data;

        public Packet(PacketType type, string data)
        {
            Type = type;
            Data = data;
        }
    }
}