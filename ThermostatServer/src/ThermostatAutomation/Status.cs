﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ThermostatAutomation
{
    public class Zone
    {
        public string Name { get; set; }

        public DateTime? Timestamp { get; set; }

        public decimal? Temperature { get; set; }
    }

    public class Status
    {
        private static Status _instance;
        private static Object _lock = new Object();

        public DateTime? OutsideTemperatureTimestamp { get; set; }

        public decimal? OutsideTemperature { get; set; }

        public List<Zone> Zones { get;  } = new List<Zone>();

        public string TargetZone { get; set; }

        /// <summary>
        /// By default TargetTemperature is set to 12 deg Celsius.
        /// </summary>
        public decimal TargetTemperature { get; set; } = 12;

        public Settings Settings { get; set; }

        public static Status Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new Status();

                            //TODO: load the last known status
                            //_instance.Zones.Add(new Zone());

                        }
                    }
                }

                return _instance;
            }
        }

        public bool VacationMode { get; internal set; }
    }
}
