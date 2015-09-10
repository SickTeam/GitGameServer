using GitGameServer.Models;
using Octokit;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace GitGameServer.Controllers
{
    [RoutePrefix("woot")]
    public class GameController : ApiController
    {
        [Route("game")]
        [HttpPost]
        public async Task<IHttpActionResult> CreateGame([FromBody]CreateGameInfo info)
        {
            GitHubClient client = new GitHubClient(new ProductHeaderValue("GitGameServer")) { Credentials = new Credentials(info.Token) };

            if (!(await client.Search.SearchUsers(new SearchUsersRequest(info.Owner))).Items.Any(x => x.Login == info.Owner))
                return BadRequest($"Unknown GitHub user; {info.Owner}.");

            if (!(await client.Repository.GetAllForUser(info.Owner)).Any(x => x.Name == info.Repo))
                return BadRequest($"Unknown GitHub repository; {info.Owner}/{info.Repo}.");

            var commits = await client.Repository.Commits.GetAll(info.Owner, info.Repo);

            GameSetup setup = new GameSetup(info.Owner, info.Repo, commits);
            GameManager.Singleton.AddSetup(setup);
            var user = setup.AddUser(info.Username);

            return Ok(new { gameid = setup.Hash, userid = user.Hash });
        }

        [Route("game/{gameid}/setup")]
        [HttpGet]
        public IHttpActionResult GetSetup([FromUri]string gameid)
        {
            throw new NotImplementedException();
        }
        [Route("game/{gameid}/setup")]
        [HttpPut]
        public IHttpActionResult GetSetup([FromUri]string gameid, [FromBody]GameSettings settings)
        {
            throw new NotImplementedException();
        }

        [Route("game/{gameid}/players")]
        [HttpPost]
        public IHttpActionResult AddUser([FromUri]string gameid, [FromBody]string username)
        {
            throw new NotImplementedException();
        }

        [Route("game/{gameid}/messages")]
        [HttpGet]
        public IHttpActionResult GetMessages([FromUri]string gameid)
        {
            var mod = Request.Headers.IfModifiedSince;
            throw new NotImplementedException();
        }

        [Route("game/{gameid}/state")]
        [HttpGet]
        public IHttpActionResult GetState([FromUri]string gameid)
        {
            throw new NotImplementedException();
        }
        [Route("game/{gameid}/state")]
        [HttpPut]
        public IHttpActionResult GetState([FromUri]string gameid, [FromBody]States state)
        {
            throw new NotImplementedException();
        }
    }
}