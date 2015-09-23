using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace GitGameServer
{
    public class GuessMessage : Message
    {
        private int round;
        private string username;

        protected override void toStream(Stream stream)
        {
            stream.Write(round);
            stream.Write(username);
        }
        public static GuessMessage FromStream(DateTime timestamp, Stream stream)
        {
            return new GuessMessage(timestamp, stream.ReadInt32(), stream.ReadString());
        }

        private GuessMessage(DateTime timestamp, int round, string username)
            : base(timestamp)
        {
            this.round = round;
            this.username = username;
        }
        public GuessMessage(int round, string username)
        {
            this.round = round;
            this.username = username;
        }

        public override string Name => "roundguess";
        public override string GetURL(string gameid) => $"/game/{gameid}/rounds/{round}/guesses";

        public override JToken GetResource() => new JObject()
        {
            { "round", round },
            { "username", username }
        };
    }
}