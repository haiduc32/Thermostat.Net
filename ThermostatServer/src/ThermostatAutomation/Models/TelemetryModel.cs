using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ThermostatAutomation.Models
{
    public class TelemetryModel
    {
        public MongoDB.Bson.ObjectId _id { get; set; }

        public DateTime Timestamp { get; set; }

        public List<Zone> Zones { get; set; }
        
        public decimal? OutsideTemperature { get; set; }
    }
}
