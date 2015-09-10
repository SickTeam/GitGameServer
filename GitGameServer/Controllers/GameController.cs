﻿using GitGameServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;

namespace GitGameServer.Controllers
{
    [RoutePrefix("woot")]
    public class GameController : ApiController
    {
        [Route("game")]
        [HttpPost]
        public IHttpActionResult CreateGame([FromBody]CreateGameInfo info)
        {
            throw new NotImplementedException();
        }

        [Route("game/{gameid}/setup")]
        [HttpGet]
        public IHttpActionResult GetSetup()
        {
            throw new NotImplementedException();
        }
        [Route("game/{gameid}/setup")]
        [HttpPut]
        public IHttpActionResult GetSetup([FromBody]GameSettings settings)
        {
            throw new NotImplementedException();
        }

        [Route("game/{gameid}/players")]
        [HttpPost]
        public IHttpActionResult AddUser([FromBody]string username)
        {
            throw new NotImplementedException();
        }

        [Route("game/{gameid}/messages")]
        [HttpGet]
        public IHttpActionResult GetMessages()
        {
            var mod = Request.Headers.IfModifiedSince;
            throw new NotImplementedException();
        }

        [Route("game/{gameid}/state")]
        [HttpGet]
        public IHttpActionResult GetState()
        {
            throw new NotImplementedException();
        }
        [Route("game/{gameid}/state")]
        [HttpPut]
        public IHttpActionResult GetState([FromBody]States state)
        {
            throw new NotImplementedException();
        }
    }
}