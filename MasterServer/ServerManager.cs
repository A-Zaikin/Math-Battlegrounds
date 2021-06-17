using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MasterServer
{
    static class ServerManager
    {
        private static int lastServerPlayerCount = 0;
        private static int lastServerPort = 10000;
        private static int lastPrivateRoomPort = 51151;

        private static Dictionary<int, PrivateRoom> privateRooms = new Dictionary<int, PrivateRoom>();
        private static Random codeGenerator = new Random();
        private static int GetNewRoomCode()
        {
            var code = codeGenerator.Next(10000, 99999);
            while (privateRooms.ContainsKey(code))
            {
                code = codeGenerator.Next(10000, 99999);
            }
            return code;
        }

        public static string GetRoom(int code)
        {
            if (code == 0)
            {
                var newCode = GetNewRoomCode();
                lastPrivateRoomPort++;
                //StartServer(lastPrivateRoomPort);
                privateRooms.Add(newCode, new PrivateRoom(lastPrivateRoomPort));
                return newCode + "," + lastPrivateRoomPort;
            }
            else
            {
                var port = privateRooms[code].Port.ToString();
                privateRooms[code].PlayerCount++;
                if (privateRooms[code].PlayerCount >= 6)
                {
                    privateRooms.Remove(code);
                }
                return port;
            }
        }

        public static int GetServer()
        {
            if (lastServerPlayerCount < 2 && lastServerPort > 10000)
            {
                lastServerPlayerCount++;
                return lastServerPort;
            }
            else
            {
                lastServerPort++;
                StartServer(lastServerPort);
                lastServerPlayerCount = 1;
                return lastServerPort;
            }
        }

        private static void StartServer(int port)
        {
            ProcessStartInfo pInfo = new ProcessStartInfo();
            pInfo.UseShellExecute = true;
            pInfo.CreateNoWindow = false;
            pInfo.WindowStyle = ProcessWindowStyle.Normal;
            pInfo.FileName = System.AppDomain.CurrentDomain.BaseDirectory + @"/GameServer/GameServer.exe";
            pInfo.ArgumentList.Add(port.ToString());
            Process.Start(pInfo);
        }
    }
}
