using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ThermostatAutomation.Models
{
    [BsonIgnoreExtraElements]
    public class RefreshTokenModel
    {
        public MongoDB.Bson.ObjectId _id { get; set; }

        public string Token { get; set; }

        public string Application { get; set; }
    }
}
