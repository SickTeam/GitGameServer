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

        public Contributor[] Contributors { get; set; }
        
        public bool IncludeMerges { get; set; }
        public bool LowerCase { get; set; }
    }
}