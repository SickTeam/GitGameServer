namespace GitGameServer
{
    public partial class Game
    {
        public class Commit
        {
            private readonly Game game;

            private string message;
            private string username;

            private int added, removed;

            public Commit(Game game, string message, string username, int added, int removed)
            {
                this.game = game;

                this.message = message;
                this.username = username;

                this.added = added;
                this.removed = removed;
            }

            public string Message => message;
            public string Username => username;

            public int Added => added;
            public int Removed => removed;
        }
    }
}
