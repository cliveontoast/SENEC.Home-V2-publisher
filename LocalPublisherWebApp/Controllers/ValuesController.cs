using System.Collections.Generic;
using LocalPublisherWebApp.Extensions;
using Microsoft.AspNetCore.Mvc;
using SenecSource;

namespace LocalPublisherWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ISenecSettings _senecSettings;

        public ValuesController(
            ISenecSettings senecSettings)
        {
            _senecSettings = senecSettings;
        }
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // TODO make this a Post call
        [HttpGet("ip/{value}")]
        public ActionResult<string> Get(string value)
        {
            _senecSettings.IP = value;
            AppSettingWriter.SetAppSettingValue("SenecIP", value);
            return Ok();
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
