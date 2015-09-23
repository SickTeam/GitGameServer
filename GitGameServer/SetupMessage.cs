using Newtonsoft.Json.Linq;

namespace GitGameServer
{
    public class SetupMessage : Message
    {
        private Models.GameSettings settings;

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