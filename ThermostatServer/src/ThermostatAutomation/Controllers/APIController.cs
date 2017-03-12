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
        // GET: api/
        [HttpGet("{channel?}")]
        public string Get(int channel = 0)
        {
            bool channelStatus = Engine.Instance.Evaluate(channel);

            Status.Instance.Channels[channel] = channelStatus;
            Status.Instance.SaveTelemetry();

            return channelStatus ? "ON" : "OFF";
        }

        // POST api/values
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value">Values come by the region index.</param>
        [HttpPost("temperature")]
        public async void Post([FromBody]Zone value)
        {
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

            Status.Instance.SaveTelemetry();
        }
    }
}
