using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GameServer
{
    class ProblemData
    {
        [JsonIgnore]
        public string RightAnswer;
        public string Text;
        public TimeSpan Time;
    }
}
