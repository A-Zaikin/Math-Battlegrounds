using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace MathBattlegrounds
{
    public static class ServerInfo
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly string ip = "85.12.215.66";
        private static readonly string mainPort = "7777";
        public static string Port = "51152";
        public static string Token;
        public static TcpClient tcpClient;
        public static NetworkStream stream;
        public static Thread streamThread;
        public static RoundData CurrentRoundData;
        public static RoundResult CurrentRoundResult;
        public static int? CurrentPlayerId;
        public static bool GameEnded;

        private static readonly int headerLength = 4;
        public static bool StreamEnabled = true;

        public static void StartStreamThread()
        {
            tcpClient = new TcpClient(ip, int.Parse(Port));
            stream = tcpClient.GetStream();
            streamThread = new Thread(() =>
            {
                while (StreamEnabled)
                {
                    if (stream.DataAvailable)
                    {
                        var header = new byte[headerLength];
                        stream.Read(header, 0, headerLength);
                        var type = int.Parse(Encoding.UTF8.GetString(header.Take(1).ToArray(), 0, 1));
                        var length = int.Parse(Encoding.UTF8.GetString(header.Skip(1).ToArray(), 0, headerLength - 1));
                        var packet = new byte[length];
                        var bytesReceived = stream.Read(packet, 0, packet.Length);
                        var responseData = Encoding.UTF8.GetString(packet, 0, bytesReceived);
                        switch ((PacketType)type)
                        {
                            case PacketType.Pulse:
                                SendToServer("0", PacketType.Pulse);
                                break;
                            case PacketType.NewRound:
                                CurrentRoundData = JsonConvert.DeserializeObject<RoundData>(responseData);
                                break;
                            case PacketType.RoundResult:
                                CurrentRoundResult = JsonConvert.DeserializeObject<RoundResult>(responseData);
                                break;
                            case PacketType.PlayerConnected:
                                CurrentPlayerId = int.Parse(responseData);
                                break;
                        }
                    }
                    Thread.Sleep(100);
                }
            });
            streamThread.Start();
        }

        public static void SendToServer(string message, PacketType packetType)
        {
            var packet = new Packet(packetType, message);
            var builder = new StringBuilder();
            builder.Append((int)packet.Type);
            builder.Append(Encoding.UTF8.GetByteCount(packet.Data).ToString("D3"));
            builder.Append(packet.Data);
            var data = Encoding.UTF8.GetBytes(builder.ToString());
            stream.Write(data, 0, data.Length);
        }

        public static bool Authenticate(string username, string password)
        {
            var responseString = client.GetStringAsync($"http://{ip}:{mainPort}/api/authenticate?login={username}&password={password}");
            Token = responseString.Result;
            return true;
        }

        public static bool Register(string username, string password)
        {
            var responseString = client.GetStringAsync($"http://{ip}:{mainPort}/api/register?login={username}&password={password}");
            Token = responseString.Result;
            return true;
        }

        public static bool GetSessionPort()
        {
            var responseString = client.GetStringAsync($"http://{ip}:{mainPort}/api/matchmake?token={Token}");
            Port = responseString.Result;
            return true;
        }

        public static bool GetRoom(string code)
        {
            var responseString = client.GetStringAsync($"http://{ip}:{mainPort}/api/getroom?code={code}");
            Port = responseString.Result;
            return true;
        }

        public static string CreateRoom()
        {
            var responseString = client.GetStringAsync($"http://{ip}:{mainPort}/api/getroom?code=0");
            var result = responseString.Result.Split(',');
            Port = result[1];
            return result[0];
        }

        public static void SendAnswerData(AnswerData answer)
        {
            var message = JsonConvert.SerializeObject(answer);
            SendToServer(message, PacketType.Answer);
        }

        public static void SendNewPlayerInfo(string name)
        {
            SendToServer(name, PacketType.PlayerConnected);
        }
    }
}