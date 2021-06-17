﻿using System.Text.Json.Serialization;

namespace GameServer
{
    class RoundResult
    {
        public PlayerRoundResult PlayerResult;
        public PlayerRoundResult OtherPlayerResult;
        public int PlayerHealth;
        public int OtherPlayerHealth;
        public int PlayerPlace = 0;
        [JsonIgnore]
        public int OtherPlayerPlace = 0;
        public RoundResult Reversed()
        {
            var reversed = new RoundResult();
            reversed.PlayerResult = OtherPlayerResult;
            reversed.OtherPlayerResult = PlayerResult;
            reversed.PlayerHealth = OtherPlayerHealth;
            reversed.OtherPlayerHealth = PlayerHealth;
            reversed.PlayerPlace = OtherPlayerPlace;
            reversed.OtherPlayerPlace = PlayerPlace;
            return reversed;
        }
    }
}
