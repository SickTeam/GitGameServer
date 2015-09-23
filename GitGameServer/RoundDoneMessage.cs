using Newtonsoft.Json.Linq;

namespace GitGameServer
{
    public class RoundDoneMessage : Message
    {
        private int round;

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