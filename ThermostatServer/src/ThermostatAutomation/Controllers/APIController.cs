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
        [HttpGet("{zone?}")]
        public string Get(int zone = 0)
        {
            return Engine.Instance.Evaluate(zone) ? "ON" : "OFF";
        }

        //// GET api/values/5
        //[HttpGet("{id}")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

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

            MongoClient _client;
            IMongoDatabase _db;

            _client = new MongoClient("mongodb://localhost:27017");
            _db = _client.GetDatabase("Thermostat");

            var collection = _db.GetCollection<TelemetryModel>("Telemetry");

            TelemetryModel telemetry = new TelemetryModel { Zones = Status.Instance.Zones, Timestamp = DateTime.Now };
            Repository rep = new Repository();
            rep.AddTelemetry(telemetry);
        }   

        //// PUT api/values/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        //// DELETE api/values/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
