using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThermostatAutomation.Models;

namespace ThermostatAutomation
{
    public class Repository
    {
        private MongoClient _client;
        private IMongoDatabase _db;

        //TODO: make it thread safe!
        protected MongoClient Client
        {
            get
            {
                if (_client == null)
                {
                    _client = new MongoClient("mongodb://localhost:27017");
                }
                return _client;
            }
        }
        protected IMongoDatabase DB
        {
            get
            {
                if (_db == null)
                {
                    _db = Client.GetDatabase("Thermostat");
                }
                return _db;
            }
        }

        public SettingsModel GetSettings()
        {
            var collection = DB.GetCollection<SettingsModel>("Settings");
            var emptyFilter = FilterDefinition<SettingsModel>.Empty;
            //new FilterDefinitionBuilder<SettingsModel>().
            var result = collection.Find(emptyFilter);
            SettingsModel settings = result.FirstOrDefault();

            if (settings == null)
            {
                //create the settings
                settings = new SettingsModel();
                collection.InsertOne(settings);
            }

            return settings;
        }

        public void UpdateSettings(SettingsModel settings)
        {
            var collection = DB.GetCollection<SettingsModel>("Settings");

            var emptyFilter = FilterDefinition<SettingsModel>.Empty;
            
            //TODO: check it!
            collection.ReplaceOne(emptyFilter, settings);
        }

        public void AddTelemetry(TelemetryModel telemetry)
        {
            var collection = DB.GetCollection<TelemetryModel>("Telemetry");
            collection.InsertOne(telemetry);
        }

        public List<TelemetryModel> GetOneDayTelemetry()
        {
            var collection = DB.GetCollection<TelemetryModel>("Telemetry");

            DateTime lastDay = DateTime.Now.AddDays(-1);

            return (from t in collection.AsQueryable()
             where t.Timestamp > lastDay
             select t).ToList();
        }

        public void DeleteOverrides(string engineName)
        {
            var collection = DB.GetCollection<RuleOverrideModel>("RuleOverride");
            var filter = Builders<RuleOverrideModel>.Filter.In("EngineName", engineName);
            DeleteResult result = collection.DeleteMany(filter);
        }

        public void AddOverride(string engineName, RuleOverrideModel rule)
        {
            var collection = DB.GetCollection<RuleOverrideModel>("RuleOverride");

            collection.InsertOne(rule);
        }

        public List<RuleOverrideModel> GetOverrides(string engineName)
        {
            var collection = DB.GetCollection<RuleOverrideModel>("RuleOverride");
            var filter = Builders<RuleOverrideModel>.Filter.In("EngineName", engineName);
            return collection.Find(filter).ToList();
        }
    }
}
