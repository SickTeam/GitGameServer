using Newtonsoft.Json.Linq;

namespace GitGameServer
{
    public class StateMessage : Message
    {
        public static StateMessage CreateStarted(int round)
        {
            return new StateMessage("started", round);
        }
        public static StateMessage CreateFinished()
        {
            return new StateMessage("finished", 0);
        }

        private string state;
        private int round;
        
        private StateMessage(string state, int round)
        {
            this.state = state;
            this.round = round;
        }

        public override string Name => "state";
        public override string GetURL(string gameid)
        {
            return $"/game/{gameid}/state";
        }

        public override JToken GetResource()
        {
            var obj = new JObject();
            obj.Add("state", state);
            if (round > 0)
                obj.Add("round", round);
            return obj;
        }
    }
}
