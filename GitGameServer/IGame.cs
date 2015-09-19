using System.Collections.Generic;

namespace GitGameServer
{
    public interface IGame
    {
        string State { get; }

        IEnumerable<Message> GetMessages();
    }
}
