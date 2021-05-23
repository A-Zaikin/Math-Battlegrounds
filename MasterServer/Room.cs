using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MasterServer
{
    class Room
    {
        private List<PlayerData> players = new List<PlayerData>();
        private long id;

        private Room()
        {

        }

        public long GetId()
        {
            return id;
        }

        public int GetPlayerCount()
        {
            return players.Count;
        }

        public void AddPlayer(PlayerData player)
        {
            players.Add(player);
        }

        public static Room CreateRoom(long id)
        {
            var room = new Room
            {
                id = id,
            };
            return room;
        }

        public void Update(int playerId)
        {

        }

        private void Start()
        {

        }
    }
}
