using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace GitGameServer
{
    public class RoundStartMessage : Message
    {
        private int round;

        protected override void toStream(Stream stream)
        {
            stream.Write(round);
        }
        public static RoundStartMessage FromStream(DateTime timestamp, Stream stream)
        {
            return new RoundStartMessage(timestamp, stream.ReadInt32());
        }

        private RoundStartMessage(DateTime timestamp, int round)
            : base(timestamp)
        {
            this.round = round;
        }
        public RoundStartMessage(int round)
        {
            this.round = round;
        }
        
        public override string Name => "roundstart";
        public override string GetURL(string gameid)
        {
            return $"/game/{gameid}/rounds/{round}";
        }

        public override JToken GetResource()
        {
            return new JObject() { { "round", round } };
        }
    }
}