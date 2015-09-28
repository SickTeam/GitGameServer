using System.Collections.Generic;
using System.Linq;

namespace GitGameServer
{
    public class GameSetup : IGame
    {
        private readonly string hash;
        private readonly string token;

        private readonly string owner, repository;
        private Octokit.GitHubCommit[] commits;
        private int count;

        private List<Message> messages = new List<Message>();

        private GameSettings settings;
        private bool filter(Octokit.GitHubCommit commit)
        {
            if (settings.HasFlag(GameSettings.ExcludeMerges) && commit.Parents.Count >= 2)
                return false;

            return true;
        }

        private List<Models.Contributor> contributors;
        private List<User> users;

        public GameSetup(string owner, string repo, string token, IEnumerable<Octokit.GitHubCommit> commits)
        {
            this.hash = HashHelper.GetMD5($"{owner}/{repo}");

            this.owner = owner;
            this.repository = repo;
            this.token = token;

            this.commits = commits.ToArray();
            this.settings = GameSettings.None;
            this.count = commits.Count(filter);

            this.messages = new List<Message>();
            this.messages.Add(StateMessage.CreateSetup());

            this.contributors = new List<Models.Contributor>();
            this.users = new List<User>();

            Models.GameSettings settings = new Models.GameSettings();

            settings.ExcludeMerges = false;
            settings.LowerCase = false;
            settings.Contributors = GetContributors().Select(x => new Models.GameSettings.Contributor() { Name = x.Name, Active = x.Active }).ToArray();

            this.SetSettings(new Models.GameSettings() { });
        }

        string IGame.State => "setup";
        IEnumerable<Message> IGame.GetMessages() { foreach (var m in messages) yield return m; }
        private void Add(Message message)
        {
            this.messages.Add(message);
        }

        public string Hash => hash;
        public string Token => token;

        public string Owner => owner;
        public string Repository => repository;

        public int CommitCount => count;

        public GameSettings Settings
        {
            get { return settings; }
            set
            {
                settings = value;
                count = commits.Count(filter);
            }
        }

        public User AddUser(string username)
        {
            var user = User.Create(username);
            Add(new PlayerMessage(username));
            this.users.Add(user);
            return user;
        }
        public User Getuser(string userhash)
        {
            return users.FirstOrDefault(x => x.Hash == userhash);
        }

        public void SetSettings(Models.GameSettings change)
        {
            if (change.ExcludeMerges.HasValue)
                if (change.ExcludeMerges.Value)
                    settings |= GameSettings.ExcludeMerges;
                else
                    settings &= ~GameSettings.ExcludeMerges;

            if (change.LowerCase.HasValue)
                if (change.LowerCase.Value)
                    settings |= GameSettings.LowerCase;
                else
                    settings &= ~GameSettings.LowerCase;

            foreach (var c in change.Contributors)
                setContributor(c.Name, c.Active);

            this.messages.Add(new SetupMessage(change));
        }
        private void setContributor(string name, bool active)
        {
            for (int i = 0; i < contributors.Count; i++)
                if (contributors[i].Name == name)
                {
                    contributors[i].Active = active;
                    return;
                }

            foreach (var c in GetContributors())
                if (c.Name == name)
                {
                    contributors.Add(new Models.Contributor(name) { Active = active });
                    return;
                }

            return;
        }

        public Models.Contributor[] GetContributors()
        {
            List<Models.Contributor> list = new List<Models.Contributor>();
            foreach (var c in commits)
            {
                var con = list.Find(x => x.Name == c.Commit.Committer.Name);
                if (con == null)
                    list.Add(new Models.Contributor(c.Commit.Committer.Name) { Active = filter(c) });
                else if (!con.Active && filter(c))
                    con.Active = true;
            }
            foreach (var c in list)
            {
                var t = contributors.FirstOrDefault(x => x.Name == c.Name);
                if (t != null)
                    c.Active = t.Active;
            }

            return list.ToArray();
        }
        public IEnumerable<User> GetUsers()
        {
            foreach (var u in users)
                yield return u;
        }
        public IEnumerable<Octokit.GitHubCommit> GetCommits()
        {
            foreach (var c in commits)
                if (filter(c))
                    yield return c;
        }
    }
}
