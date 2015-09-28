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

            private byte[] guesses;

            public Commit(Game game, string sha, string message, string username, int added, int removed, byte[] guesses)
            {
                this.game = game;

                this.sha = sha;

                this.message = message;
                this.username = username;

                this.added = added;
                this.removed = removed;

                this.guesses = guesses;
            }

            public string Sha => sha;

            public string Message => message;
            public string Username => username;

            public int Added => added;
            public int Removed => removed;

            public bool RoundDone => guesses.All(x => x != 0);

            public bool GetGuess(string username, out string guess)
            {
                int user = Array.FindIndex(game.users, x => x.Name == username);
                if (user >= 0)
                {
                    guess = guesses[user] == 0 ? null : game.contributors[user];
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
                int index = game.tableIndex;

                if (await game.commits.GetCommit(index) != this)
                    return false;

                int user = Array.FindIndex(game.users, x => x.Name == username);
                if (user == -1)
                    return false;

                if (guesses[user] != 0)
                    return false;

                int g = Array.IndexOf(game.contributors, guess);
                if (g == -1)
                    return false;

                guesses[user] = (byte)(g + 1);
                using (FileStream fs = new FileStream(game.path, FileMode.Open, FileAccess.ReadWrite))
                {
                    long offset = game.tableStart + index * game.rowSize;
                    fs.Seek(offset + 40 + user, SeekOrigin.Begin);
                    fs.WriteByte(guesses[user]);
                }

                game.Add(new GuessMessage(index + 1, username));

                if (RoundDone)
                    game.NextRound();

                return true;
            }
        }
    }
}
