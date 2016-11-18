using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ThermostatAutomation
{
    public static class Extensions
    {
        public static bool IsStale(this DateTime? timestamp)
        {
                return !timestamp.HasValue || timestamp.Value < DateTime.Now.AddMinutes(-5);
        }
    }
}
