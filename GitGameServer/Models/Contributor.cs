namespace GitGameServer.Models
{
    public class Contributor
    {
        public Contributor(string name)
        {
            this.Name = name;
            this.Active = true;
        }

        public string Name { get; private set; }
        public bool Active { get; set; }
    }
}