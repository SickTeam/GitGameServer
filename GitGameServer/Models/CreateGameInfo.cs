﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GitGameServer.Models
{
    public class CreateGameInfo
    {
        public string Owner { get; set; }
        public string Repo { get; set; }
        public string Username { get; set; }
        public string Token { get; set; }
    }
}