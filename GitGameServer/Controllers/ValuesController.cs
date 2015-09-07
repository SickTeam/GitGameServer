using System;
using System.Collections.Generic;
using System.Web.Http;
using GitGameServer.Models;

namespace GitGameServer.Controllers
{
    [RoutePrefix("api/values")]
    public class ValuesController : ApiController
    {
        #region private fields
        private int _nextId;
        private IDictionary<int, ValueModel> _values;
        #endregion

        public ValuesController()
        {
            seed();
        }

        [Route("")]
        [HttpGet]
        public IHttpActionResult GetValues()
        {
            return Ok(_values);
        }

        [Route("{id}")]
        [HttpGet]
        public IHttpActionResult GetValue(int id)
        {
            var result = _values[id];

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [Route("")]
        [HttpPost]
        public IHttpActionResult PostValue([FromBody]ValueModel value)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                _values.Add(_nextId++, value);
                return Ok();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("{id}")]
        [HttpPut]
        public IHttpActionResult PutValue(int id, [FromBody]ValueModel value)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                _values[id] = value;
                return Ok();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("{id}")]
        [HttpDelete]
        public IHttpActionResult DeleteValue(int id)
        {
            try
            {
                _values.Remove(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("secret")]
        [HttpGet]
        [Authorize]
        public IHttpActionResult SecretValue()
        {
            return Ok("Nah nah! You'll never get this!");
        }

        #region private methods
        private void seed()
        {
            _nextId = 1;

            _values = new Dictionary<int, ValueModel>();
            _values.Add(_nextId++,
                new ValueModel
                {
                    X = "abe",
                    Y = "abe"
                });
            _values.Add(_nextId++,
                new ValueModel
                {
                    X = "abe2",
                    Y = "abe2"
                });
        }
        #endregion
    }
}
