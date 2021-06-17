using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GameServer
{
    class PlayerData
    {
        [JsonIgnore]
        public TcpClient Client;
        [JsonIgnore]
        public bool Connected = true;

        public int OpponentId;

        public int Health;
        public string Name;
        public int Id;
        public bool IsAi = false;
    }
}
