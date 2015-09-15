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

        private User[] users;
        private string[] contributors;

        private long tableStart;
        private int rowSize;
        private int rowCount;
        private int tableIndex;

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
            using (FileStream fs = new FileStream(filepath, FileMode.Open))
            {
                string owner = fs.ReadString();
                string repo = fs.ReadString();
                string token = fs.ReadString();
                byte set = (byte)fs.ReadInt32();

                Game game = new Game(filepath, token, owner, repo);

                game.users = new User[fs.ReadInt32()];
                for (int i = 0; i < game.users.Length; i++)
                    game.users[i] = User.FromStream(fs);

                game.contributors = new string[fs.ReadInt32()];
                for (int i = 0; i < game.contributors.Length; i++)
                    game.contributors[i] = fs.ReadString();

                game.rowCount = fs.ReadInt32();

                game.tableStart = fs.Position;
                game.rowSize = 40 + game.users.Length;

                for (game.tableIndex = 0; game.tableIndex < game.rowCount; game.tableIndex++)
                {
                    fs.Seek(40, SeekOrigin.Current);
                    bool stop = false;

                    for (int i = 0; i < game.users.Length; i++)
                        if (fs.ReadByte() == 0)
                        {
                            stop = true;
                            fs.Seek(-(40 + i + 1), SeekOrigin.Current);
                            break;
                        }

                    if (stop)
                        break;
                }

                return game;
            }
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