using Newtonsoft.Json.Linq;

namespace GitGameServer
{
    public class PlayerMessage : Message
    {
        private string username;

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