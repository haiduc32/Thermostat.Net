using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ThermostatAutomation.Models
{
    public class SettingsModel
    {
        public MongoDB.Bson.ObjectId _id { get; set; }

        public bool VacationMode { get; set; } = false;

        public decimal TargetTemperature { get; set; } = 18;

        /// <summary>
        /// Zero based index.
        /// </summary>
        public string TargetZone { get; set; }
    }
}
