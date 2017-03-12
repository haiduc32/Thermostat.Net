using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ThermostatAutomation.Models
{
    public class RuleOverrideModel
    {
        public MongoDB.Bson.ObjectId _id { get; set; }
        
        public string EngineName { get; set; }

        public string RuleName { get; set; }

        public string Zone { get; set; }

        public decimal Temperature { get; set; }
    }
}
