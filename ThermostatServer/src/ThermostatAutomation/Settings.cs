using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ThermostatAutomation
{
    public class ChannelSettings
    {
        public string Number { get; set; }

        public List<string> Zones { get; set; }
    }

    public class Settings
    {
        public List<ChannelSettings> Channels { get; set; }
    }
}
