using System;
using System.IO;

namespace GitGameServer
{
    public class User : IEquatable<User>
    {
        private string hash;
        private string name;

        private User(string hash, string name)
        {
            this.hash = hash;
            this.name = name;
        }

        public static User Create(string name)
        {
            return new User(HashHelper.GetMD5(name), name);
        }
        public static User FromStream(Stream stream)
        {
            string hash = stream.ReadString();
            string name = stream.ReadString();

            return new User(hash, name);
        }
        public void ToStream(Stream stream)
        {
            stream.Write(hash);
            stream.Write(name);
        }

        public string Hash => hash;
        public string Name => name;

        public override bool Equals(object obj)
        {
            if (obj is User)
                return Equals((User)obj);
            else
                return false;
        }
        public bool Equals(User other)
        {
            return hash.Equals(other?.hash);
        }
        public override int GetHashCode()
        {
            return hash.GetHashCode();
        }
    }
}