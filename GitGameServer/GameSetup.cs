using System.Collections.Generic;
using System.Linq;

namespace GitGameServer
{
    public class GameSetup
    {
        private readonly string hash;

        private readonly string owner, repository;
        private Octokit.GitHubCommit[] commits;

        public GameSetup(string owner, string repo, IEnumerable<Octokit.GitHubCommit> commits)
        {
            this.hash = HashHelper.GetMD5($"{owner}/{repo}");

            this.owner = owner;
            this.repository = repo;

            this.commits = commits.ToArray();
        }

        public string Hash => hash;

        public string Owner => owner;
        public string Repository => repository;
        
        public IEnumerable<Commit> GetCommits()
        {
            foreach (var c in commits)
                yield return new Commit(c.Commit.Message, c.Commit.User.Login, c.Stats.Additions, c.Stats.Deletions);
        }
    }
}