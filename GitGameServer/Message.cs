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
            if (this is GuessMessage) stream.WriteByte((byte)'g');
            if (this is PlayerMessage) stream.WriteByte((byte)'p');
            if (this is RoundDoneMessage) stream.WriteByte((byte)'d');
            if (this is RoundStartMessage) stream.WriteByte((byte)'s');
            if (this is SetupMessage) stream.WriteByte((byte)'e');
            if (this is StateMessage) stream.WriteByte((byte)'t');
            else
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
                case 'g': return GuessMessage.FromStream(timestamp, stream);
                case 'p': return PlayerMessage.FromStream(timestamp, stream);
                case 'd': return RoundDoneMessage.FromStream(timestamp, stream);
                case 's': return RoundStartMessage.FromStream(timestamp, stream);
                case 'e': return SetupMessage.FromStream(timestamp, stream);
                case 't': return StateMessage.FromStream(timestamp, stream);

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