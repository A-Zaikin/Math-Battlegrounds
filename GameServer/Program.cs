using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using MoonSharp.Interpreter;
using Newtonsoft.Json;
using System.Timers;

namespace GameServer
{
    class Program
    {
        private static readonly object _lock = new();
        private static Dictionary<int, PlayerData> players = new();
        private static List<Thread> threads = new();
        private static bool start = false;
        private static ConcurrentQueue<Packet> packetQueue = new();
        private static List<string> problems = new();
        private static int id = 0;
        private static Random random = new();
        private static PlayerData aiPlayerData = new PlayerData();
        private static int lastPlace = 8;
        static void Main(string[] args)
        {
            if (args.Length == 0)
                throw new ArgumentException();
            foreach (var file in Directory.GetFiles(@"./Problems"))
            {
                Console.WriteLine(file);
                problems.Add(file);
            }
            TcpListener server = null;
            try
            {
                var localAddr = IPAddress.Any;
                server = new TcpListener(localAddr, int.Parse(args[0]));
                server.Start();
                while (players.Count < 2)
                {
                    var client = server.AcceptTcpClient();
                    lock (_lock)
                    {
                        var newId = id;
                        id++;
                        var player = new PlayerData()
                        {
                            Client = client,
                            Health = 100,
                            Id = newId,
                            Name = "PLAYER" + newId
                        };
                        players.Add(newId, player);
                        SendPacket(player, new Packet(PacketType.PlayerConnected, newId.ToString()));
                        var thread = new Thread(HandleClient);
                        thread.Start(player);
                        threads.Add(thread);
                        Console.WriteLine("Player connected");
                    }
                }

                aiPlayerData.Id = -1;
                aiPlayerData.Health = 100;
                aiPlayerData.IsAi = true;
                aiPlayerData.Name = "AI";

                var pulseTimer = new System.Timers.Timer();
                pulseTimer.AutoReset = true;
                pulseTimer.Interval = 1000;
                pulseTimer.Elapsed += PulseTimer_Elapsed;
                pulseTimer.Enabled = true;
                start = true;
                lastPlace = players.Count;
                while (start)
                {
                    NewRound();
                    foreach (var player in players.Values)
                    {
                        if (player.Health <= 0 || !player.Connected)
                        {
                            players.Remove(player.Id);
                        }
                    }
                    if (players.Count == 1)
                    {
                        start = false;
                        SendPacket(players.Values.First(), new Packet(PacketType.GameEnded, "YOU WIN"));
                        Thread.Sleep(5000);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                if (server != null)
                    server.Stop();
            }
            Environment.Exit(0);
        }

        private static void PulseTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Broadcast(new Packet(PacketType.Pulse, "0"));
        }

        public static ProblemData GetProblem()
        {
            var problem = problems[random.Next(0, problems.Count)];
            var script = new Script();
            script.DoString(System.IO.File.ReadAllText(problem));
            var seed = random.Next(int.MinValue, int.MaxValue);
            //var seed = 4;
            var rightAnswer = ((int)script.Call(script.Globals["Solve"], seed).Number).ToString();
            var text = script.Call(script.Globals["GetText"], seed);
            var problemData = new ProblemData()
            {
                Text = text.String,
                Time = TimeSpan.FromSeconds(30),
                RightAnswer = rightAnswer
            };
            return problemData;
        }

        public static void NewRound()
        {
            var problemData = GetProblem();
            var sortedPlayers = players.Values.OrderByDescending(player => player.Health).ToList();
            if (sortedPlayers.Count() % 2 != 0)
            {
                sortedPlayers.Add(aiPlayerData);
            }
            var pairs = new Dictionary<int, PlayerData>();
            var evenPositioned = sortedPlayers.Where((o, i) => i % 2 == 0);
            var oddPositioned = sortedPlayers.Where((o, i) => i % 2 != 0);
            foreach (var pair in evenPositioned.Zip(oddPositioned, (even, odd) => (even, odd)))
            {
                pairs.Add(pair.even.Id, pair.odd);
                pairs.Add(pair.odd.Id, pair.even);
            }
            var roundData = new RoundData()
            {
                Pairs = pairs,
                Problem = problemData
            };
            Broadcast(new Packet(PacketType.NewRound, JsonConvert.SerializeObject(roundData)));
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var answers = new Dictionary<int, AnswerData>();
            while (/*stopwatch.Elapsed < problemData.Time && */answers.Count < players.Count)
            {
                while (packetQueue.TryDequeue(out var packet))
                {
                    if (packet.Type == PacketType.Answer)
                    {
                        var answer = JsonConvert.DeserializeObject<AnswerData>(packet.Data);
                        if (answer != null)
                            answers.Add(answer.PlayerId, answer);
                        if (answer != null && (pairs[answer.PlayerId].IsAi || answers.ContainsKey(pairs[answer.PlayerId].Id)))
                        {
                            CompareAnswers(pairs, answers, answer, problemData.RightAnswer);
                        }
                    }
                }
            }
        }

        public static void CompareAnswers(Dictionary<int, PlayerData> pairs, Dictionary<int, AnswerData> answers, AnswerData answer, string rightAnswer)
        {
            var firstPlayer = pairs[answer.PlayerId];
            var secondPlayer = players[answer.PlayerId];
            var secondPlayerAnswer = answer.Answer == rightAnswer;
            var result = new RoundResult()
            {
                PlayerResult = PlayerRoundResult.Tie,
                OtherPlayerResult = PlayerRoundResult.Tie
            };
            if (firstPlayer.IsAi)
            {
                if (!secondPlayerAnswer)
                {
                    secondPlayer.Health -= 50;
                    result.PlayerResult = PlayerRoundResult.Loss;
                }
                else
                {
                    result.PlayerResult = PlayerRoundResult.Tie;
                }
                result.PlayerHealth = secondPlayer.Health;
                result.OtherPlayerHealth = 100;
                if (result.PlayerHealth <= 0)
                {
                    result.PlayerPlace = 4;
                }
                SendPacket(secondPlayer, new Packet(PacketType.RoundResult, JsonConvert.SerializeObject(result)));
                return;
            }
            var firstPlayerAnswer = answers[firstPlayer.Id].Answer == rightAnswer;
            var firstPlayerTime = answers[firstPlayer.Id].Time;
            var secondPlayerTime = answer.Time;
            if (firstPlayerAnswer && secondPlayerAnswer)
            {
                if (firstPlayerTime < secondPlayerTime)
                {
                    result.PlayerResult = PlayerRoundResult.Win;
                    result.OtherPlayerResult = PlayerRoundResult.Loss;
                }
                else
                {
                    result.PlayerResult = PlayerRoundResult.Loss;
                    result.OtherPlayerResult = PlayerRoundResult.Win;
                }
            }
            else
            {
                if (firstPlayerAnswer != secondPlayerAnswer)
                {
                    result.PlayerResult = firstPlayerAnswer ? PlayerRoundResult.Win : PlayerRoundResult.Loss;
                    result.OtherPlayerResult = secondPlayerAnswer ? PlayerRoundResult.Win : PlayerRoundResult.Loss;
                }
            }

            if (result.PlayerResult == PlayerRoundResult.Loss)
                firstPlayer.Health -= 50;
            if (result.OtherPlayerResult == PlayerRoundResult.Loss)
                secondPlayer.Health -= 50;
            result.PlayerHealth = firstPlayer.Health;
            result.OtherPlayerHealth = secondPlayer.Health;
            if (result.PlayerHealth <= 0)
            {
                result.PlayerPlace = lastPlace;
            }
            SendPacket(firstPlayer, new Packet(PacketType.RoundResult, JsonConvert.SerializeObject(result)));
            if (result.OtherPlayerHealth <= 0)
            {
                result.PlayerPlace = lastPlace;
            }
            SendPacket(secondPlayer, new Packet(PacketType.RoundResult, JsonConvert.SerializeObject(result.Reversed())));
        }

        public static void HandleClient(object o)
        {
            var player = (PlayerData)o;
            var headerLength = 4;
            while (player.Connected)
            {
                try
                {
                    var stream = player.Client.GetStream();
                    var header = new byte[headerLength];
                    while (stream.DataAvailable)
                    {
                        stream.Read(header, 0, headerLength);
                        var type = int.Parse(Encoding.UTF8.GetString(header.Take(1).ToArray(), 0, 1));
                        var length = int.Parse(Encoding.UTF8.GetString(header.Skip(1).ToArray(), 0, headerLength - 1));
                        var data = new byte[length];
                        var bytesReceived = stream.Read(data, 0, data.Length);
                        var text = Encoding.UTF8.GetString(data, 0, bytesReceived);
                        var packet = new Packet((PacketType)type, text)
                        {
                            SenderId = player.Id
                        };
                        packetQueue.Enqueue(packet);
                        Console.WriteLine("Received packet of type " + type + " and size " + length + ". Contents : " + text);
                    }
                }
                catch
                {
                    break;
                }
            }
            player.Client.Close();
            Console.WriteLine("Player disconnected");
        }

        public static void Broadcast(Packet packet)
        {
            lock (_lock)
            {
                foreach (var player in players.Values)
                {
                    SendPacket(player, packet);
                }
            }
        }

        public static void SendPacket(PlayerData player, Packet packet)
        {
            lock (_lock)
            {
                try
                {
                    var builder = new StringBuilder();
                    builder.Append((int)packet.Type);
                    builder.Append(Encoding.UTF8.GetByteCount(packet.Data).ToString("D3"));
                    builder.Append(packet.Data);
                    var stream = player.Client.GetStream();
                    var data = Encoding.UTF8.GetBytes(builder.ToString());
                    stream.Write(data, 0, data.Length);
                }
                catch
                {
                    player.Connected = false;
                }
            }
        }
    }
}
