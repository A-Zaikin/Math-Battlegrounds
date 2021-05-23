using System.Net.Http;

namespace MathBattlegrounds
{
    public static class ServerInfo
    {
        private static readonly HttpClient client = new HttpClient();
        public static string Token;

        public static bool Authenticate()
        {
            var responseString = client.GetStringAsync("http://85.12.215.66:7777/api/authenticate?login=TEST&password=TEST");
            Token = responseString.Result;
            return true;
        }

    }
}