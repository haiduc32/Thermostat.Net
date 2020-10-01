using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ThermostatAutomation.Extensions
{
    public static class CommonExtensions
    {
        public static bool IsStale(this DateTime? timestamp)
        {
                return !timestamp.HasValue || timestamp.Value < DateTime.Now.AddMinutes(-5);
        }
    }
}
