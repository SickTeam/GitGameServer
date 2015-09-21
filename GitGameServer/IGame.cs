using System;
using System.Collections.Generic;
using System.Linq;

namespace GitGameServer
{
    public interface IGame
    {
        string State { get; }

        IEnumerable<Message> GetMessages();
    }

    public static class IGameExtension
    {
        public static IEnumerable<Message> GetMessagesSince(this IGame game, DateTime since)
        {
            return game.GetMessages().SkipWhile(x => x.Timestamp < since);
        }
    }
}
