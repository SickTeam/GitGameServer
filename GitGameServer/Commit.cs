using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GitGameServer
{
    public partial class Game
    {
        public class Commit
        {
            private readonly Game game;

            private string sha;
            private string message;
            private string username;

            private int added, removed;

            private GuessCollection guesses;

            public Commit(Game game, string sha, string message, string username, int added, int removed, byte[] guesses)
            {
                this.game = game;

                this.sha = sha;

                this.message = message;
                this.username = username;

                this.added = added;
                this.removed = removed;

                this.guesses = new GuessCollection(this, guesses);
            }

            public string Sha => sha;

            public string Message => message;
            public string Username => username;

            public int Added => added;
            public int Removed => removed;

            public GuessCollection Guesses => guesses;

            public class GuessCollection
            {
                private Commit commit;
                private byte[] guesses;

                public GuessCollection(Commit commit, byte[] guesses)
                {
                    this.commit = commit;
                    this.guesses = guesses;
                }

                public bool RoundDone => guesses.All(x => x != 0);

                public bool GetGuess(string username, out string guess)
                {
                    int user = Array.FindIndex(commit.game.users, x => x.Name == username);
                    if (user >= 0)
                    {
                        guess = guesses[user] == 0 ? null : commit.game.contributors[user];
                        return true;
                    }
                    else
                    {
                        guess = null;
                        return false;
                    }
                }
                public async Task<bool> SetGuess(string username, string guess)
                {
                    if (await commit.game.commits.GetCommit(commit.game.tableIndex) != commit)
                        return false;

                    int user = Array.FindIndex(commit.game.users, x => x.Name == username);
                    if (user == -1)
                        return false;

                    if (guesses[user] != 0)
                        return false;

                    int g = Array.IndexOf(commit.game.contributors, guess);
                    if (g == -1)
                        return false;

                    guesses[user] = (byte)(g + 1);
                    using (FileStream fs = new FileStream(commit.game.path, FileMode.Open, FileAccess.ReadWrite))
                    {
                        long offset = commit.game.tableStart + commit.game.tableIndex * commit.game.rowSize;
                        fs.Seek(offset + 40 + user, SeekOrigin.Begin);
                        var existing = fs.ReadByte();
                        if (existing > 0)
                            return false;
                        fs.Seek(-1, SeekOrigin.Current);
                        fs.WriteByte(guesses[user]);
                    }

                    return true;
                }
            }
        }
    }
}
