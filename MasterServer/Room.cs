using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MasterServer
{
    class PrivateRoom
    {
        public int PlayerCount = 1;
        public int Code;
        public int Port;

        public PrivateRoom(int port)
        {
            Port = port;
        }
    }
}
