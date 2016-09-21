using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ThermostatAutomation.Models;
using MongoDB.Driver;
using ThermostatAutomation.Rules;

namespace ThermostatAutomation.Controllers
{
    public class HomeController : Controller
    {
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

            if (settings == null)
            {
                //create the settings
                settings = new SettingsModel();
                await collection.InsertOneAsync(settings);
            }
            
            // Load the last known average temperature
            // Side thought: we might need to load several settings to make sure we get the telemetry from all areas
            var telemetryCollection = _db.GetCollection<TelemetryModel>("Telemetry");

            TelemetryModel telemetry = (from t in telemetryCollection.AsQueryable()
                orderby t.Timestamp descending
                //where t.RoomTemperature[1].HasValue
                select t).FirstOrDefault();

            ViewData["Zones"] = Status.Instance.Zones;

            BasicEngine engine = new BasicEngine();
            //TODO: show multiple zones in the UI?
            ViewData["HeatingStatus"] = engine.Evaluate(0) ? "ON" : "OFF";

            return View();
        }
        
        public IActionResult Error()
        {
            return View();
        }
    }
}
