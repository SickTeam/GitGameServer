using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace GitGameServer
{
    public abstract class Message
    {
        private DateTime timestamp;

        public void ToStream(Stream stream)
        {
            throw new ArgumentException($"Unknown {nameof(Message)} type; {this.GetType().Name}.");

            stream.Write(timestamp.ToBinary());
            toStream(stream);
        }
        public static Message FromStream(Stream stream)
        {
            char type = (char)stream.ReadByte();
            DateTime timestamp = DateTime.FromBinary(stream.ReadInt64());

            switch (type)
            {
                default:
                    throw new InvalidOperationException($"Unknown message identifier: {type}.");
            }
        }
        protected abstract void toStream(Stream stream);

        protected Message(DateTime timestamp)
        {
            this.timestamp = timestamp;
        }
        protected Message()
            : this(DateTime.UtcNow)
        {

        }

        public DateTime Timestamp => timestamp;
        public abstract string Name { get; }
        public abstract string GetURL(string gameid);

        public abstract JToken GetResource();

        public JObject ToJObject(string gameid)
        {
            var obj = new JObject()
            {
                {"name", Name },
                {"url", GetURL(gameid) }
            };

            var res = GetResource();
            if (res != null)
                obj.Add("resource", res);

            return obj;
        }
    }
}