using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace GitGameServer
{
    public class StateMessage : Message
    {
        public static StateMessage CreateSetup()
        {
            return new StateMessage("setup");
        }
        public static StateMessage CreateStarted()
        {
            return new StateMessage("started");
        }
        public static StateMessage CreateFinished()
        {
            return new StateMessage("finished");
        }

        private string state;

        protected override void toStream(Stream stream)
        {
            stream.Write(state);
        }
        public static StateMessage FromStream(DateTime timestamp, Stream stream)
        {
            return new StateMessage(timestamp, stream.ReadString());
        }

        private StateMessage(DateTime timestamp, string state)
            : base(timestamp)
        {
            this.state = state;
        }
        private StateMessage(string state)
        {
            this.state = state;
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
            return obj;
        }
    }
}
