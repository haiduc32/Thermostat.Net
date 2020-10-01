using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ThermostatAutomation.Rules
{
    public interface IRulesEngine
    {
        string Name { get; }

        Repository Repository { get; set; }

        void Initialize();

        bool Evaluate(int channel = 0);

        ///// <summary>
        ///// Set will override the set temperature.
        ///// </summary>
        //decimal CurrentTargetTemperature { get; }

        decimal? GetTargetTemperatureInZone(string zoneName);

        decimal OverrideTargetTemperatureInZone(string zoneName, decimal targetTemperature);

        /// <summary>
        /// Resets the overrides. The default rules will take power.
        /// </summary>
        void ResetRules();

        string GetActiveZone(int channel);
    }
}
