using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ThermostatAutomation.Models
{
    [BsonIgnoreExtraElements]
    public class SettingsModel
    {
        public MongoDB.Bson.ObjectId _id { get; set; }

        public string ActiveEngine { get; set; }
    }
}
