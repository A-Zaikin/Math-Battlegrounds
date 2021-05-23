using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
namespace MasterServer
{
    class DatabaseInterface
    {
        private static MySqlConnection connection;

        public static void Connect(string connectionString)
        {
            connection = new MySqlConnection(connectionString);
            connection.Open();
        }

        public static MySqlDataReader SendCommand(MySqlCommand command)
        {
            command.Connection = connection;
            return command.ExecuteReader();
        }
    }
}
