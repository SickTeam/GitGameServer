using GitGameServer.Models;
using Newtonsoft.Json.Linq;
using Octokit;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace GitGameServer.Controllers
{
    [RoutePrefix("woot")]
    public class GameController : ApiController
    {
        private IHttpActionResult useGame(string gameid, Func<IGame, IHttpActionResult> method) => useGame<IGame>(gameid, method);
        private IHttpActionResult useGame<T>(string gameid, Func<T, IHttpActionResult> method) where T : IGame
        {
            IGame game;
            if (GameManager.Singleton.TryGetGame(gameid, out game))
            {
                if (game is T)
                    return method((T)game);
                else
                    return ResponseMessage(new HttpResponseMessage(HttpStatusCode.Forbidden)
                    { ReasonPhrase = $@"Game is currently in state ""{game.State}"" which does not support your current request." });
            }
            else
                return ResponseMessage(new HttpResponseMessage(HttpStatusCode.NotFound)
                { ReasonPhrase = $@"No game with id ""{gameid}"" was found." });
        }

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

            GameSetup setup = new GameSetup(info.Owner, info.Repo, info.Token, commits);
            GameManager.Singleton.AddSetup(setup);
            var user = setup.AddUser(info.Username);

            return Ok(new { gameId = setup.Hash, userId = user.Hash });
        }

        [Route("game/{gameid}/setup")]
        [HttpPut]
        public IHttpActionResult SetSetup([FromUri]string gameid, [FromBody]Models.GameSettings settings)
        {
            return useGame<GameSetup>(gameid, game =>
            {
                game.SetSettings(settings);
                return Ok();
            });
        }

        [Route("game/{gameid}/players")]
        [HttpPost]
        public IHttpActionResult AddUser([FromUri]string gameid, [FromBody]string username)
        {
            return useGame<GameSetup>(gameid, game => Ok(new { userId = game.AddUser(username).Hash }));
        }

        [Route("game/{gameid}/messages")]
        [HttpGet]
        public IHttpActionResult GetMessages([FromUri]string gameid)
        {
            return useGame(gameid, game =>
            {
                var modified = Request.Headers.IfModifiedSince?.UtcDateTime;
                var messages = modified.HasValue ? game.GetMessagesSince(modified.Value) : game.GetMessages();

                return Ok(new
                {
                    timestamp = DateTime.UtcNow,
                    messages = messages.Select(x => x.ToJObject(gameid)).ToArray()
                });
            });
        }

        [Route("game/{gameid}/state")]
        [HttpGet]
        public IHttpActionResult GetState([FromUri]string gameid)
        {
            return useGame(gameid, game => Ok(new { state = game.State }));
        }
        [Route("game/{gameid}/state")]
        [HttpPut]
        public IHttpActionResult SetState([FromUri]string gameid, [FromBody]SetStates? state)
        {
            GameSetup setup;
            if (GameManager.Singleton.TryGetSetup(gameid, out setup))
            {
                if (state == SetStates.start)
                {
                    GameManager.Singleton.StartGame(setup);
                    return Ok();
                }
                else
                    return BadRequest("Invalid game state.");
            }
            else
                return BadRequest("No game with " + nameof(gameid) + " was found.");
        }

        [Route("game/{gameid}/rounds/{round}/guesses")]
        [HttpPost]
        public async Task<IHttpActionResult> MakeGuess([FromUri]string gameid, [FromUri]int round, [FromBody]GuessInput guess)
        {
            var userhash = Request.Headers.Authorization.Scheme;

            Game game;
            if (!GameManager.Singleton.TryGetGame(gameid, out game))
                return BadRequest($"No game with {nameof(gameid)} was found.");

            var user = game.GetUser(userhash);

            var commit = await game.Commits.GetCommit(round - 1);
            if (await commit.Guesses.SetGuess(user.Name, guess.Guess))
            {
                game.Add(new GuessMessage(round, user.Name));
                if (commit.Guesses.RoundDone)
                    game.NextRound();
                return Ok();
            }
            else
                return BadRequest();
        }
    }
}
