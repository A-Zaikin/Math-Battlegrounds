using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Transactions;
using MoonSharp.Interpreter;

namespace GameServer
{
    class Program
    {
        private static readonly object _lock = new object();
        private static List<TcpClient> clients = new List<TcpClient>();
        private static List<Thread> threads = new List<Thread>();
        private static bool start = false;
        static void Main(string[] args)
        {
            var script = new Script();

            script.DoString(System.IO.File.ReadAllText(@"./Problems/test.txt"));
            while (true)
            {
                var res = script.Call(script.Globals["Solve"], 4);
                var text = script.Call(script.Globals["GetText"], 4);
                Console.WriteLine(text.String);
                Console.WriteLine(res.Number.ToString());
            }
            if (args.Length == 0)
                throw new ArgumentException();
            TcpListener server = null;
            try
            {
                var localAddr = IPAddress.Any;
                server = new TcpListener(localAddr, int.Parse(args[0]));
                server.Start();
                while (clients.Count < 2)
                {
                    var client = server.AcceptTcpClient();
                    lock (_lock)
                    {
                        clients.Add(client);
                        var thread = new Thread(HandleClient);
                        thread.Start(client);
                        threads.Add(thread);
                    }
                }
                start = true;
                
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
        }

        public static void HandleClient(object o)
        {
            var client = (TcpClient)o;
            while (true)
            {
                var stream = client.GetStream();
                var data = new byte[64];
                StringBuilder builder = new StringBuilder();
                int bytes = 0;
                do
                {
                    bytes = stream.Read(data, 0, data.Length);
                    builder.Append(Encoding.ASCII.GetString(data, 0, bytes));
                }
                while (stream.DataAvailable);
                Console.WriteLine(builder.ToString());
            }
            client.Close();
        }

        public static void Broadcast(string message)
        {
            lock (_lock)
            {
                foreach (var client in clients)
                {
                    var stream = client.GetStream();
                    var data = Encoding.ASCII.GetBytes(message);
                    stream.Write(data, 0, data.Length);
                }
            }
        }
    }
}
