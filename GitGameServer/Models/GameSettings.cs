using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GitGameServer.Models
{
    public class GameSettings
    {
        public class Contributor
        {
            public string Name { get; set; }
            public bool Active { get; set; }
        }

        public GameSettings()
        {
            this.Contributors = new Contributor[0];
            this.ExcludeMerges = null;
            this.LowerCase = null;
        }

        public Contributor[] Contributors { get; set; }
        
        public bool? ExcludeMerges { get; set; }
        public bool? LowerCase { get; set; }
    }
}