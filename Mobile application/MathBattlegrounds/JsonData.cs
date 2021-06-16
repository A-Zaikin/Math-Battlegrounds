using System;
using System.Collections.Generic;

namespace MathBattlegrounds
{
    public class AnswerData
    {
        public string Answer;
        public TimeSpan Time;
        public int PlayerId;

        public AnswerData(string answer, TimeSpan time)
        {
            Answer = answer;
            Time = time;
        }
    }

    public class RoundData
    {
        public ProblemData Problem;
        public Dictionary<int, PlayerData> Pairs;

        public RoundData(ProblemData problem, Dictionary<int, PlayerData> pairs)
        {
            Problem = problem;
            Pairs = pairs;
        }
    }

    public class ProblemData
    {
        public string Text;
        public TimeSpan Time;

        public ProblemData(string text, TimeSpan time)
        {
            Text = text;
            Time = time;
        }
    }

    public class PlayerData
    {
        public int Health;
        public string Name;
        public int Id;
        public int OpponentId;
        public bool IsAi = false;

        public PlayerData(int health, string name, int id)
        {
            Health = health;
            Name = name;
            Id = id;
        }
    }

    public class RoundResult
    {
        public PlayerRoundResult PlayerResult;
        public PlayerRoundResult OtherPlayerResult;
        public int PlayerHealth;
        public int OtherPlayerHealth;
        public int PlayerPlace;
    }

    public enum PlayerRoundResult
    {
        Win,
        Loss,
        Tie
    }
}