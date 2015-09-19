using System.Collections.Generic;
using System.IO;
using System.Web.Hosting;

namespace GitGameServer
{
    public class GameManager
    {
        private static GameManager manager = null;
        public static GameManager Singleton
        {
            get
            {
                if (manager != null)
                    return manager;

                string path = HostingEnvironment.MapPath("/App_Data");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                manager = new GameManager(path);
                return manager;
            }
        }

        private readonly string path;
        private Dictionary<string, Game> games;
        private Dictionary<string, GameSetup> setups;

        private GameManager(string path)
        {
            this.path = path;
            this.games = new Dictionary<string, Game>();
            this.setups = new Dictionary<string, GameSetup>();
        }

        private string getFilePath(string hash)
        {
            return Path.ChangeExtension(Path.Combine(path, hash), "json");
        }

        public void AddSetup(GameSetup setup)
        {
            setups.Add(setup.Hash, setup);
        }
        public bool TryGetSetup(string hash, out GameSetup setup)
        {
            return setups.TryGetValue(hash, out setup);
        }

        public Game StartGame(GameSetup setup)
        {
            string hash = setup.Hash;
            string filepath = getFilePath(hash);

            setups.Remove(hash);
            Game game = Game.FromSetup(setup, filepath);
            games.Add(hash, game);

            return game;
        }
        public bool TryGetGame(string hash, out Game game)
        {
            if (games.TryGetValue(hash, out game))
                return true;

            if (!File.Exists(getFilePath(hash)))
                return false;

            game = Game.FromFile(getFilePath(hash));
            games.Add(hash, game);
            return true;
        }

        public bool TryGetGame(string hash, out IGame game)
        {
            GameSetup _setup;
            Game _game;

            if (setups.TryGetValue(hash, out _setup))
            {
                game = _setup;
                return true;
            }
            else if (games.TryGetValue(hash, out _game))
            {
                game = _game;
                return true;
            }
            else
            {
                string filepath = getFilePath(hash);
                if (!File.Exists(filepath))
                {
                    game = null;
                    return false;
                }
                else
                {
                    _game = Game.FromFile(filepath);
                    games.Add(hash, _game);
                    game = _game;
                    return true;
                }
            }
        }
    }
}
