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

        public GameSetup(string owner, string repo, IEnumerable<Octokit.GitHubCommit> commits)
        {
            this.hash = HashHelper.GetMD5($"{owner}/{repo}");

            this.owner = owner;
            this.repository = repo;

            this.commits = commits.ToArray();
            this.count = commits.Count(filter);
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
    }
}