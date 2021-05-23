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

        public static int GetServer(PlayerData player)
        {
            if (lastServerPlayerCount < 6 && lastServerPort > 10000)
            {
                lastServerPlayerCount++;
                return lastServerPort;
            }
            else
            {
                lastServerPort++;
                ProcessStartInfo pInfo = new ProcessStartInfo();
                pInfo.UseShellExecute = true;
                pInfo.CreateNoWindow = false;
                pInfo.WindowStyle = ProcessWindowStyle.Normal;
                pInfo.FileName = System.AppDomain.CurrentDomain.BaseDirectory + @"/GameServer/GameServer.exe";
                pInfo.ArgumentList.Add(lastServerPort.ToString());
                Process.Start(pInfo);
                lastServerPlayerCount = 1;
                return lastServerPort;
            }
        }
    }
}
