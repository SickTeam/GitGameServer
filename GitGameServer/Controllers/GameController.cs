using GitGameServer.Models;
using Newtonsoft.Json.Linq;
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

            GameSetup setup = new GameSetup(info.Owner, info.Repo, info.Token, commits);
            GameManager.Singleton.AddSetup(setup);
            var user = setup.AddUser(info.Username);

            return Ok(new { gameId = setup.Hash, userId = user.Hash });
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
                excludeMerges = settings.HasFlag(GameSettings.ExcludeMerges),
                lowerCase = settings.HasFlag(GameSettings.LowerCase)
            });
        }
        [Route("game/{gameid}/setup")]
        [HttpPut]
        public IHttpActionResult SetSetup([FromUri]string gameid, [FromBody]Models.GameSettings settings)
        {
            GameSetup setup;
            if (GameManager.Singleton.TryGetSetup(gameid, out setup))
            {
                JObject settingsJ = new JObject();
                if (settings.ExcludeMerges.HasValue)
                    settingsJ.Add("excludeMerges", settings.ExcludeMerges);
                if (settings.LowerCase.HasValue)
                    settingsJ.Add("lowerCase", settings.LowerCase);

                if (settings.Contributors.Length > 0)
                {
                    var cArr = new JArray();
                    foreach (var c in settings.Contributors)
                        cArr.Add(new JObject() { { "name", c.Name }, { "active", c.Active } });

                    settingsJ.Add("contributors", cArr);
                }

                setup.Settings = SetFlag(setup.Settings, GameSettings.ExcludeMerges, settings.ExcludeMerges);
                setup.Settings = SetFlag(setup.Settings, GameSettings.LowerCase, settings.LowerCase);

                foreach (var c in settings.Contributors)
                    setup.SetContributor(c.Name, c.Active);

                setup.Add(new SetupMessage(settings));

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
        [HttpGet]
        public IHttpActionResult GetUsers([FromUri]string gameid)
        {
            GameSetup setup;
            if (GameManager.Singleton.TryGetSetup(gameid, out setup))
            {
                return Ok(setup.GetUsers().Select(x => new { name = x.Name }));
            }
            else
                return BadRequest("No game with " + nameof(gameid) + " was found.");
        }

        [Route("game/{gameid}/players")]
        [HttpPost]
        public IHttpActionResult AddUser([FromUri]string gameid, [FromBody]string username)
        {
            GameSetup setup;
            if (GameManager.Singleton.TryGetSetup(gameid, out setup))
            {
                User user = setup.AddUser(username);
                setup.Add(new PlayerMessage(username));
                return Ok(new { userId = user.Hash });
            }
            else
                return BadRequest("No game with " + nameof(gameid) + " was found.");
        }

        [Route("game/{gameid}/messages")]
        [HttpGet]
        public IHttpActionResult GetMessages([FromUri]string gameid)
        {
            var modoffset = Request.Headers.IfModifiedSince;
            var mod = modoffset?.UtcDateTime;

            IGame game;
            if (GameManager.Singleton.TryGetGame(gameid, out game))
            {
                var messages = mod.HasValue ? game.GetMessagesSince(mod.Value) : game.GetMessages();
                var time = DateTime.UtcNow;
                JObject obj = new JObject()
                {
                    { "timestamp", time },
                    { "messages", new JArray(messages.Select(x=>x.ToJObject(gameid))) }
                };
                return Ok(obj);
            }
            else
                return BadRequest("No game with " + nameof(gameid) + " was found.");
        }

        [Route("game/{gameid}/state")]
        [HttpGet]
        public IHttpActionResult GetState([FromUri]string gameid)
        {
            IGame game;
            if (GameManager.Singleton.TryGetGame(gameid, out game))
                return Ok(new { state = game.State });
            else
                return BadRequest("No game with " + nameof(gameid) + " was found.");
        }
        [Route("game/{gameid}/state")]
        [HttpPut]
        public IHttpActionResult SetState([FromUri]string gameid, [FromBody]SetStates state)
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
            /* send: { guess: "mikaelec" } */
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

        [Route("game/{gameid}/rounds/{round}/guesses")]
        [HttpGet]
        public async Task<IHttpActionResult> GetGuesses([FromUri]string gameid, [FromUri]int round)
        {
            Game game;
            if (!GameManager.Singleton.TryGetGame(gameid, out game))
                return BadRequest($"No game with {nameof(gameid)} was found.");

            JArray arr = new JArray();

            var commit = await game.Commits.GetCommit(round - 1);
            foreach (var u in game.GetUserNames())
            {
                string guess;
                if (!commit.Guesses.GetGuess(u, out guess))
                    continue;
                bool hasguess = guess != null;

                JObject obj = new JObject()
                {
                    { "name", u },
                    { "hasGuess", hasguess }
                };

                if (game.Round > round)
                    obj.Add("guess", guess);

                arr.Add(obj);
            }

            return Ok(arr);
        }

        [Route("game/{gameid}/rounds/{round}")]
        [HttpGet]
        public async Task<IHttpActionResult> GetRound([FromUri]string gameid, [FromUri]int round)
        {
            Game game;
            if (!GameManager.Singleton.TryGetGame(gameid, out game))
                return BadRequest($"No game with {nameof(gameid)} was found.");

            JArray arr = new JArray();

            var commit = await game.Commits.GetCommit(round - 1);

            JObject obj = new JObject()
            {
                { "message", commit.Message },
                { "linesAdded", commit.Added },
                { "linesRemoved", commit.Removed }
            };

            if (game.Round > round)
            {
                obj.Add("committer", commit.Username);
                obj.Add("sha", commit.Sha);
            }

            return Ok(obj);
        }
    }
}
