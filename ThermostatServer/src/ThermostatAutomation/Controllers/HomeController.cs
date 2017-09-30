using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ThermostatAutomation.Models;
using MongoDB.Driver;
using ThermostatAutomation.Rules;
using Microsoft.AspNetCore.Authorization;
using ThermostatAutomation.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ThermostatAutomation.Controllers
{
    public class HomeController : Controller
    {
        Engine _engine;

        public HomeController(Engine engine)
        {
            _engine = engine;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            SettingsModel settings;

            MongoClient _client;
            IMongoDatabase _db;

            _client = new MongoClient("mongodb://localhost:27017");
            _db = _client.GetDatabase("Thermostat");

            var collection = _db.GetCollection<SettingsModel>("Settings");
            var emptyFilter = FilterDefinition<SettingsModel>.Empty;
            //new FilterDefinitionBuilder<SettingsModel>().
            var result = await collection.FindAsync(emptyFilter);
            settings = result.FirstOrDefault();

            // TODO: this is a good candidate to move to startup?
            if (settings == null)
            {
                //create the settings
                settings = new SettingsModel();
                await collection.InsertOneAsync(settings);
            }
           
            ViewData["Zones"] = Status.Instance.Zones;
            
            //TODO: show multiple zones in the UI?
            ViewData["HeatingStatus"] = _engine.Rules.Evaluate(channel: 0) ? "ON" : "OFF";
            
            var rules = Status.Instance.Rules.Select(x => new SelectListItem {
                Text = x.FriendlyName,
                Value = x.ClassName,
                Selected = _engine.Rules.GetType().Name == x.ClassName
            });
            ViewData["Rules"] = rules;

            return View();
        }

        public IActionResult Stats()
        {
            Repository rep = new Repository();
            var telemetry = rep.GetOneDayTelemetry();
            telemetry = telemetry.GroupBy(x => x.Timestamp.ToString("MM/dd/yy H:mm")).Select(x => x.Merge()).ToList();

            //let's filter the telemetry (better do it on input?)

            foreach (TelemetryModel t in telemetry)
            {
                foreach (Zone z in t.Zones)
                {
                    if (z.Temperature.HasValue && (z.Temperature.Value > 30 || z.Temperature.Value < 12))
                    {
                        z.Temperature = null;
                        z.Timestamp = null;
                    }
                }
            }

            ViewData["Telemetry"] = telemetry;

            return View();
        }
        
        public IActionResult Error()
        {
            return View();
        }
    }
}
