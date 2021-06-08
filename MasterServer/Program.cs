using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using MySql.Data.MySqlClient;
using MySql.Data;
using System.Threading.Tasks;
using System.Net.Sockets;
using Grapevine;
using System.Runtime.InteropServices;
using BCrypt.Net;
namespace MasterServer
{
    [RestResource]
    public class RestResource
    {
        [RestRoute("Get", "/api/test")]
        public async Task Test(IHttpContext context)
        {
            await context.Response.SendResponseAsync("Test").ConfigureAwait(false);
        }

        [RestRoute("Get", "/api/register")]
        public async Task Register(IHttpContext context)
        {
            var login = context.Request.QueryString["login"];
            var password = context.Request.QueryString["password"];
            password = BCrypt.Net.BCrypt.HashPassword(password);
            var command = new MySqlCommand();
            command.CommandText = "INSERT INTO useraccounts(login, password) VALUES(@login, @password)";
            command.Parameters.AddWithValue("@login", login);
            command.Parameters.AddWithValue("@password", password);
            using (DatabaseInterface.SendCommand(command));
            await context.Response.SendResponseAsync("Registered").ConfigureAwait(false);
        }

        [RestRoute("Get", "/api/authenticate")]
        public async Task Authenticate(IHttpContext context)
        {
            Console.WriteLine(context.Request.RemoteEndPoint.Address);
            var login = context.Request.QueryString["login"];
            var password = context.Request.QueryString["password"];
            var checkDataCommand = new MySqlCommand();
            checkDataCommand.CommandText = "SELECT password FROM useraccounts WHERE login = @login";
            checkDataCommand.Parameters.AddWithValue("@login", login);
            var responseText = "Error";
            var hash = "";
            using (var reader = DatabaseInterface.SendCommand(checkDataCommand))
            {
                reader.Read();
                hash = reader.GetString(0);
            }
            if (BCrypt.Net.BCrypt.Verify(password, hash))
            {
                var authToken = Guid.NewGuid().ToString();
                responseText = authToken;
                var storeAuthTokenCommand = new MySqlCommand();
                storeAuthTokenCommand.CommandText = "UPDATE useraccounts SET currentAuthToken = @token WHERE login = @login";
                storeAuthTokenCommand.Parameters.AddWithValue("@token", authToken);
                storeAuthTokenCommand.Parameters.AddWithValue("@login", login);
                using (DatabaseInterface.SendCommand(storeAuthTokenCommand)) ;
            }
            await context.Response.SendResponseAsync(responseText).ConfigureAwait(false);
        }

        [RestRoute("Get", "/api/matchmake")]
        public async Task MatchMake(IHttpContext context)
        {
            //Console.WriteLine(context.Request.RemoteEndPoint.Address);
            var command = new MySqlCommand();
            command.CommandText = "SELECT rating FROM useraccounts WHERE currentAuthToken = @token";
            command.Parameters.AddWithValue("@token", context.Request.QueryString["token"]);
            var rating = 0;
            using (var reader = DatabaseInterface.SendCommand(command))
            {
                reader.Read();
                rating = reader.GetInt32(0);
            }
            var playerData = new PlayerData()
            {
                ip = context.Request.RemoteEndPoint.Address.ToString(),
                rating = rating
            };
            await context.Response.SendResponseAsync(ServerManager.GetServer(playerData).ToString()).ConfigureAwait(false);
        }

        [RestRoute("Get", "/api/update")]
        public async Task Update(IHttpContext context)
        {
            //Console.WriteLine(context.Request.RemoteEndPoint.Address);
            var playerData = new PlayerData()
            {
                ip = context.Request.RemoteEndPoint.Address.ToString()
            };
            await context.Response.SendResponseAsync("").ConfigureAwait(false);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            DatabaseInterface.Connect("server=127.0.0.1;uid=root;pwd=VjELBM3pOkpHXq2N7lsM;database=main");
            using (var server = RestServerBuilder.UseDefaults().Build())
            {
                server.Prefixes.Clear();
                //server.Prefixes.Add("http://127.0.0.1:7777/");
                server.Prefixes.Add("http://+:7777/");
                server.Prefixes.Add("http://*:7777/");
                server.Start();
                Console.WriteLine("Press enter to stop the server");
                Console.ReadLine();
            }

            //try
            //{
            //    
            //    MySqlCommand cmd = new MySqlCommand();
            //    cmd.CommandText = "useraccounts";
            //    cmd.Connection = connection;
            //    cmd.CommandType = CommandType.TableDirect;
            //    MySqlDataReader reader = cmd.ExecuteReader();
            //    while (reader.Read())
            //    {
            //        Console.WriteLine(reader[0].ToString(), reader[1].ToString(), reader[2].ToString());
            //    }
            //}
            //catch (MySql.Data.MySqlClient.MySqlException ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}
            //Console.ReadKey();
            //Console.WriteLine("Hello World!");
        }
    }
}
