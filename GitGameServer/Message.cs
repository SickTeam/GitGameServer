using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace GitGameServer
{
    public class Message
    {
        private DateTime timestamp;
        private string name;
        private string url;
        private JObject resource;

        public void ToStream(Stream stream)
        {
            stream.Write(timestamp.ToBinary());
            stream.Write(name);
            stream.Write(url);
            stream.WriteByte(resource == null ? (byte)0 : (byte)1);
            if (resource != null)
                stream.Write(resource.ToString(Newtonsoft.Json.Formatting.None));
        }
        public static Message FromStream(Stream stream)
        {
            long ticks = stream.ReadInt64();
            string name = stream.ReadString();
            string url = stream.ReadString();
            bool hasR = stream.ReadByte() == 1;

            return new Message(DateTime.FromBinary(ticks), name, url, hasR ? JObject.Parse(stream.ReadString()) : null);
        }

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