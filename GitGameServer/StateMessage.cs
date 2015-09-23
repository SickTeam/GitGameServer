using Newtonsoft.Json.Linq;
using System;
using System.IO;

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

        protected override void toStream(Stream stream)
        {
            stream.Write(state);
            stream.Write(round);
        }
        public static StateMessage FromStream(DateTime timestamp, Stream stream)
        {
            return new StateMessage(timestamp, stream.ReadString(), stream.ReadInt32());
        }

        private StateMessage(DateTime timestamp, string state, int round)
            : base(timestamp)
        {
            this.round = round;
        }
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
