using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace GitGameServer
{
    public class Game
    {
        private readonly string path;
        private readonly string token;

        private readonly string owner, repository;

        public static Game FromSetup(GameSetup setup, string filepath)
        {
            throw new NotImplementedException();
        }

        public static Game FromFile(string filepath)
        {
            throw new NotImplementedException();
        }

        private Game(string path, string token, string owner, string repo)
        {
            this.path = path;
            this.token = token;

            this.owner = owner;
            this.repository = repo;
        }

        public string Owner => owner;
        public string Repository => repository;
    }
}