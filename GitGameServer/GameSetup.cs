using System.Collections.Generic;
using System.Linq;

namespace GitGameServer
{
    public class GameSetup
    {
        private readonly string hash;

        private readonly string owner, repository;
        private Octokit.GitHubCommit[] commits;
        private int count;

        private GameSettings settings;
        private bool filter(Octokit.GitHubCommit commit)
        {
            if (settings.HasFlag(GameSettings.ExcludeMerges) && commit.Parents.Count >= 2)
                return false;

            return true;
        }

        private List<Models.Contributor> contributors;
        private List<User> users;
        
        public GameSetup(string owner, string repo, IEnumerable<Octokit.GitHubCommit> commits)
        {
            this.hash = HashHelper.GetMD5($"{owner}/{repo}");

            this.owner = owner;
            this.repository = repo;

            this.commits = commits.ToArray();
            this.settings = GameSettings.None;
            this.count = commits.Count(filter);

            this.contributors = new List<Models.Contributor>();
            this.users = new List<User>();
        }

        public string Hash => hash;

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
            this.users.Add(user);
            return user;
        }
        public void SetContributor(string name, bool active)
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
            foreach(var c in list)
            {
                var t = contributors.FirstOrDefault(x => x.Name == c.Name);
                c.Active = t.Active;
            }

            return list.ToArray();
        }
    }
}
