using Newtonsoft.Json.Linq;

namespace GitGameServer
{
    public class RoundStartMessage : Message
    {
        private int round;

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