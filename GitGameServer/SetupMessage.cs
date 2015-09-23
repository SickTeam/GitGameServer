using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace GitGameServer
{
    public class SetupMessage : Message
    {
        private Models.GameSettings settings;

        protected override void toStream(Stream stream)
        {
            stream.Write(settings.ExcludeMerges);
            stream.Write(settings.LowerCase);

            stream.Write(settings.Contributors.Length);
            for (int i = 0; i < settings.Contributors.Length; i++)
            {
                stream.Write(settings.Contributors[i].Name);
                stream.Write(settings.Contributors[i].Active);
            }
        }
        public static SetupMessage FromStream(DateTime timestamp, Stream stream)
        {
            bool? ex = stream.ReadNullBoolean();
            bool? low = stream.ReadNullBoolean();

            int count = stream.ReadInt32();
            var contributors = new Models.GameSettings.Contributor[count];
            for (int i = 0; i < count; i++)
            {
                var name = stream.ReadString();
                var active = stream.ReadBoolean();
                contributors[i] = new Models.GameSettings.Contributor() { Name = name, Active = active };
            }

            var settings = new Models.GameSettings()
            {
                ExcludeMerges = ex,
                LowerCase = low,
                Contributors = contributors
            };

            return new SetupMessage(timestamp, settings);
        }

        private SetupMessage(DateTime timestamp, Models.GameSettings settings)
            : base(timestamp)
        {
            this.settings = settings;
        }
        public SetupMessage(Models.GameSettings settings)
        {
            this.settings = settings;
        }

        public override string Name => "setup";
        public override string GetURL(string gameid) => $"/game/{gameid}/setup";
        public override JToken GetResource()
        {
            var obj = new JObject();
            if (settings.ExcludeMerges.HasValue)
                obj.Add("excludemerges", settings.ExcludeMerges.Value);
            if (settings.LowerCase.HasValue)
                obj.Add("lowercase", settings.LowerCase.Value);
            if (settings.Contributors?.Length > 0)
            {
                var arr = new JArray();

                foreach (var c in settings.Contributors)
                    arr.Add(new JObject() { { "name", c.Name }, { "active", c.Active } });

                obj.Add("contributors", arr);
            }
            return obj;
        }
    }
}