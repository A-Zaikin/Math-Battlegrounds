using System.Net.Http;

namespace MathBattlegrounds
{
    public static class ServerInfo
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly string ip = "85.12.215.66:7777";
        public static string Token;

        public static bool Authenticate(string username, string password)
        {
            var responseString = client.GetStringAsync($"http://{ip}/api/authenticate?login={username}&password={password}");
            Token = responseString.Result;
            return true;
        }

        public static bool Register(string username, string password)
        {
            var responseString = client.GetStringAsync($"http://{ip}/api/register?login={username}&password={password}");
            Token = responseString.Result;
            return true;
        }
    }
}