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
            GameSettings settings;
            GameSetup setup;
            if (GameManager.Singleton.TryGetSetup(gameid, out setup))
                settings = setup.Settings;
            else
                return BadRequest("No game with " + nameof(gameid) + " was found.");

            return Ok(new
            {
                contributors = setup.GetContributors(),
                excludemerges = settings.HasFlag(GameSettings.ExcludeMerges),
                lowercase = settings.HasFlag(GameSettings.LowerCase)
            });
        }
        [Route("game/{gameid}/setup")]
        [HttpPut]
        public IHttpActionResult SetSetup([FromUri]string gameid, [FromBody]Models.GameSettings settings)
        {
            GameSetup setup;
            if (GameManager.Singleton.TryGetSetup(gameid, out setup))
            {
                setup.Settings = SetFlag(setup.Settings, GameSettings.ExcludeMerges, settings.ExcludeMerges);
                setup.Settings = SetFlag(setup.Settings, GameSettings.LowerCase, settings.LowerCase);

                foreach (var c in settings.Contributors)
                    setup.SetContributor(c.Name, c.Active);

                return Ok();
            }
            else
                return BadRequest("No game with " + nameof(gameid) + " was found.");
        }

        public GameSettings SetFlag(GameSettings value, GameSettings flag, bool? on)
        {
            if (on.HasValue)
            {
                int v = (int)value;
                int f = (int)flag;

                if (on.Value)
                    v |= f;
                else
                    v &= ~f;

                value = (GameSettings)v;
            }
            return value;
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
        public IHttpActionResult SetState([FromUri]string gameid, [FromBody]SetStates state)
        {
            throw new NotImplementedException();
        }
    }
}