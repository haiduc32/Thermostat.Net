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
            //new FilterDefinitionBuilder<SettingsModel>().
            var result = collection.Find(emptyFilter);
            SettingsModel foundSettings = result.FirstOrDefault();

            settings._id = foundSettings._id;
            collection.ReplaceOne(emptyFilter, settings, new UpdateOptions { IsUpsert = true });
        }

        public async Task AddTelemetryAsync(TelemetryModel telemetry)
        {
            var collection = DB.GetCollection<TelemetryModel>("Telemetry");
            await collection.InsertOneAsync(telemetry);
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
            var filter = Builders<RuleOverrideModel>.Filter.Eq("EngineName", engineName);
            DeleteResult result = collection.DeleteMany(filter);
        }

        public void AddOverride(string engineName, RuleOverrideModel rule)
        {
            var collection = DB.GetCollection<RuleOverrideModel>("RuleOverride");
            
            var engineNameFilter = Builders<RuleOverrideModel>.Filter.Eq("EngineName", engineName);
            var ruleNameFilter = Builders<RuleOverrideModel>.Filter.Eq("RuleName", rule.RuleName);
            var filter = Builders<RuleOverrideModel>.Filter.And(engineNameFilter, ruleNameFilter);
            var r  = collection.Find(filter);
            var existingOverride = r.SingleOrDefault();
            if (existingOverride != null)
            {
                rule._id = existingOverride._id;
                collection.ReplaceOne(filter, rule, new UpdateOptions { IsUpsert = true });
            }
            else
            {
                collection.InsertOne(rule); 
            }
            //add or update
            
        }

        public List<RuleOverrideModel> GetOverrides(string engineName)
        {
            var collection = DB.GetCollection<RuleOverrideModel>("RuleOverride");

            var result = (from o in collection.AsQueryable()
                          where o.EngineName == engineName
                          select o).ToList();

            //var filter = Builders<RuleOverrideModel>.Filter.In("EngineName", engineName);

            //return collection.Find(filter).ToList();
            return result;
        }

        public async Task AddRefreshToken(RefreshTokenModel token)
        {
            var collection = DB.GetCollection<RefreshTokenModel>("RefreshToken");
            await collection.InsertOneAsync(token);
        }

        public async Task<RefreshTokenModel> GetRefreshToken(string tokenValue)
        {
            var collection = DB.GetCollection<RefreshTokenModel>("RefreshToken");

            var filter = Builders<RefreshTokenModel>.Filter.Eq("Token", tokenValue);
            var r = await collection.FindAsync(filter);
            RefreshTokenModel result = await r.SingleOrDefaultAsync();
            return result;
        }
    }
}
