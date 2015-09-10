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
        private Dictionary<string, GameSetup> setups;

        private GameManager(string path)
        {
            this.path = path;
            this.setups = new Dictionary<string, GameSetup>();
        }

        public void AddSetup(GameSetup setup)
        {
            setups.Add(setup.Hash, setup);
        }
    }
}
