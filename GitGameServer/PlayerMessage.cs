using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace GitGameServer
{
    public class PlayerMessage : Message
    {
        private string username;

        protected override void toStream(Stream stream)
        {
            stream.Write(username);
        }
        public static PlayerMessage FromStream(DateTime timestamp, Stream stream)
        {
            return new PlayerMessage(timestamp, stream.ReadString());
        }

        private PlayerMessage(DateTime timestamp, string username)
            : base(timestamp)
        {
            this.username = username;
        }
        public PlayerMessage(string username)
        {
            this.username = username;
        }

        public override string Name => "player";
        public override string GetURL(string gameid)
        {
            return $"game/{gameid}/players";
        }

        public override JToken GetResource()
        {
            return new JObject() { { "username", username } };
        }
    }
}