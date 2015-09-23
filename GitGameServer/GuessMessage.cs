using Newtonsoft.Json.Linq;

namespace GitGameServer
{
    public class GuessMessage : Message
    {
        private int round;
        private string username;

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