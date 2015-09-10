namespace GitGameServer
{
    public class Commit
    {
        private string message;
        private string username;

        private int added, removed;

        public Commit(string message, string username, int added, int removed)
        {
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
