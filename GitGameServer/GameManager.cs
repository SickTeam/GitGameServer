using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
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

        private GameManager(string path)
        {
            this.path = path;
        }
    }
}