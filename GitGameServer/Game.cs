using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            using (FileStream fs = new FileStream(filepath, FileMode.CreateNew))
            {
                fs.Write(setup.Owner);
                fs.Write(setup.Repository);
                fs.Write(setup.Token);
                fs.Write((int)setup.Settings);

                var users = setup.GetUsers().ToArray();
                fs.Write(users.Length);
                foreach (var u in users)
                    u.ToStream(fs);

                var contributors = setup.GetContributors().Where(c => c.Active).ToArray();
                fs.Write(contributors.Length);
                foreach (var c in contributors)
                    fs.Write(c.Name);

                var commits = setup.GetCommits().ToArray();
                fs.Write(commits.Length);
                foreach (var c in commits)
                {
                    byte[] text = Encoding.ASCII.GetBytes(c.Sha);
                    fs.Write(text, 0, 40);
                    fs.WriteZeros(users.Length);
                }
            }

            return Game.FromFile(filepath);
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