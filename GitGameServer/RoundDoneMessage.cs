using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace GitGameServer
{
    public class RoundDoneMessage : Message
    {
        private int round;

        protected override void toStream(Stream stream)
        {
            stream.Write(round);
        }
        public static RoundDoneMessage FromStream(DateTime timestamp, Stream stream)
        {
            return new RoundDoneMessage(timestamp, stream.ReadInt32());
        }

        private RoundDoneMessage(DateTime timestamp, int round)
            : base(timestamp)
        {
            this.round = round;
        }
        public RoundDoneMessage(int round)
        {
            this.round = round;
        }

        public override string Name => "rounddone";
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