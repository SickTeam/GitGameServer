using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitGameServer
{
    public partial class Game : IGame
    {
        private readonly string path;
        private readonly Octokit.GitHubClient client;

        private readonly string owner, repository;
        private readonly string hash;

        private User[] users;
        private string[] contributors;

        private CommitCollection commits;

        private List<Message> messages = new List<Message>();

        private long tableStart;
        private int rowSize;
        private int rowCount;
        private int tableIndex;

        public static Game FromSetup(GameSetup setup, string filepath)
        {
            using (FileStream fs = new FileStream(filepath, FileMode.CreateNew))
            {
                fs.Write(setup.Hash);
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
                var mess = (setup as IGame).GetMessages().ToArray();
                foreach (var m in mess)
                    m.ToStream(fs);
            }

            return Game.FromFile(filepath);
        }
        public static Game FromFile(string filepath)
        {
            using (FileStream fs = new FileStream(filepath, FileMode.Open))
            {
                string hash = fs.ReadString();
                string owner = fs.ReadString();
                string repo = fs.ReadString();
                string token = fs.ReadString();
                byte set = (byte)fs.ReadInt32();

                Game game = new Game(filepath, token, owner, repo, hash);

                game.users = new User[fs.ReadInt32()];
                for (int i = 0; i < game.users.Length; i++)
                    game.users[i] = User.FromStream(fs);

                game.contributors = new string[fs.ReadInt32()];
                for (int i = 0; i < game.contributors.Length; i++)
                    game.contributors[i] = fs.ReadString();

                game.rowCount = fs.ReadInt32();
                game.commits = new CommitCollection(game);

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

                fs.Seek(game.tableStart + game.rowCount * game.rowSize, SeekOrigin.Begin);
                while (fs.Position < fs.Length)
                    game.messages.Add(Message.FromStream(fs));

                return game;
            }
        }

        private Game(string path, string token, string owner, string repo, string hash)
        {
            this.path = path;
            this.client = new Octokit.GitHubClient(new Octokit.ProductHeaderValue("GitGameServer")) { Credentials = new Octokit.Credentials(token) };

            this.owner = owner;
            this.repository = repo;
        }

        string IGame.State => tableIndex >= rowCount ? "started" : "finished";
        IEnumerable<Message> IGame.GetMessages()
        {
            foreach (var m in messages)
                yield return m;
        }
        public void Add(Message message)
        {
            this.messages.Add(message);
        }

        public string Owner => owner;
        public string Repository => repository;

        public int Round => tableIndex + 1;

        public void NextRound()
        {
            messages.Add(new RoundDoneMessage(Round));
            tableIndex++;
            if (tableIndex >= rowCount)
                messages.Add(StateMessage.CreateFinished());
            else
                messages.Add(new RoundStartMessage(Round));
        }

        public User GetUser(string hash)
        {
            for (int i = 0; i < users.Length; i++)
                if (users[i].Hash == hash)
                    return users[i];

            return null;
        }
        public string[] GetUserNames()
        {
            return users.Select(x => x.Name).ToArray();
        }

        public CommitCollection Commits => commits;

        public class CommitCollection
        {
            private Commit[] commits;
            private Game game;

            public CommitCollection(Game game)
            {
                this.game = game;
                this.commits = new Commit[game.rowCount];
            }

            public async Task<Commit> GetCommit(int index)
            {
                if (commits[index] == null)
                {
                    string sha;
                    byte[] answers = new byte[game.users.Length];
                    using (FileStream fs = new FileStream(game.path, FileMode.Open))
                    {
                        fs.Seek(game.tableStart + index * game.rowSize, SeekOrigin.Current);

                        byte[] buffer = new byte[40];
                        fs.Read(buffer, 0, 40);
                        sha = Encoding.ASCII.GetString(buffer, 0, 40);
                        fs.Read(answers, 0, answers.Length);
                    }

                    var c = await game.client.Repository.Commits.Get(game.owner, game.repository, sha);
                    commits[index] = new Commit(game, sha, c.Commit.Message, c.Commit.Committer.Name, c.Stats.Additions, c.Stats.Deletions, answers);
                }
                return commits[index];
            }
        }
    }
}
