using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using ThermostatAutomation.Models;
using ThermostatAutomation.Rules;


namespace ThermostatAutomation.Controllers
{
    [Route("api")]
    public class APIController : Controller
    {
        Engine _engine;

        public APIController(Engine engine)
        {
            _engine = engine;
        }

        [HttpGet("benchmark")]
        public IActionResult Benchmark()
        {
            return Ok();
        }

        // GET: api/
        [HttpGet("{channel?}")]
        public string Get(int channel = 0)
        {
            bool channelStatus = _engine.Rules.Evaluate(channel);

            Status.Instance.Channels[channel] = channelStatus;

            return channelStatus ? "ON" : "OFF";
        }

        // POST api/values
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value">Values come by the region index.</param>
        [HttpPost("temperature")]
        public void Post([FromBody]Zone value)
        {
            // this is a sensor check. in case the sensor fails, it can report negative values. 5 is put as a good limit.
            if (value.Temperature < 5)
            {
                return;
            }

            Zone r = Status.Instance.Zones.SingleOrDefault(x => x.Name == value.Name);

            if (r != null)
            {
                r.Temperature = value.Temperature;
                r.Timestamp = DateTime.Now;
            }
            else
            {
                value.Timestamp = DateTime.Now;
                Status.Instance.Zones.Add(value);
            }
        }
    }
}
