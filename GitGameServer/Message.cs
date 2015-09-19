using Newtonsoft.Json.Linq;
using System;

namespace GitGameServer
{
    public class Message
    {
        private DateTime timestamp;
        private string name;
        private string url;
        private JObject resource;

        public Message(DateTime timestamp, string name, string url, JObject resource = null)
        {
            this.timestamp = timestamp;
            this.name = name;
            this.url = url;
            this.resource = resource;
        }

        public DateTime Timestamp => timestamp;
        public string Name => name;
        public string URL => url;
        
        public JObject ToJObject()
        {
            var obj = new JObject()
            {
                {"name", name },
                {"url", url }
            };

            if (resource != null)
                obj.Add("resource", resource);

            return obj;
        }
    }
}