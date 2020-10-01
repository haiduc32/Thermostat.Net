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
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using ThermostatAutomation.Configuration;

namespace ThermostatAutomation.Controllers
{
    public class HomeController : Controller
    {
        private Engine _engine;
        private ILogger<HomeController> _logger;

        public HomeController(Engine engine, ILogger<HomeController> logger)
        {
            _engine = engine;
            _logger = logger;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            //SettingsModel settings;

            MongoClient _client;
            IMongoDatabase _db;

            _client = new MongoClient("mongodb://localhost:27017");
            _db = _client.GetDatabase("Thermostat");

            //var collection = _db.GetCollection<SettingsModel>("Settings");
            //var emptyFilter = FilterDefinition<SettingsModel>.Empty;
            ////new FilterDefinitionBuilder<SettingsModel>().
            //var result = await collection.FindAsync(emptyFilter);
            //settings = result.FirstOrDefault();

            //// TODO: this is a good candidate to move to startup?
            //if (settings == null)
            //{
            //    //create the settings
            //    settings = new SettingsModel();
            //    await collection.InsertOneAsync(settings);
            //}
           
            ViewData["Zones"] = Status.Instance.Zones;
            
            //TODO: show multiple zones in the UI?
            ViewData["HeatingStatus"] = _engine.Rules.Evaluate(channel: 0) ? "ON" : "OFF";

            //TODO: this is not exactly the MVC way, we should be making a new POST action for this
            if (Request.Method == "POST")
            {
                string newRule = Request.Form["SelectedRules"];
                if (newRule != _engine.Rules.GetType().Name)
                {
                    RuleItem rulesEngine = Status.Instance.Rules.Single(x => newRule == x.ClassName);
                    if (rulesEngine != null)
                    {
                        _engine.Enable(rulesEngine.ClassName);

                        Repository rep = new Repository();
                        var settings = rep.GetSettings();
                        settings.ActiveEngine = rulesEngine.ClassName;
                        rep.UpdateSettings(settings);
                    }
                }
            }
            
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
            Stopwatch s = new Stopwatch();
            s.Start();
            var telemetry = rep.GetOneDayTelemetry();
            s.Stop();
            _logger.LogInformation($"Getting telemetry for one day took {s.ElapsedMilliseconds}ms");


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

        public IActionResult Benchmark()
        {
            return View();
        }
    }


}
