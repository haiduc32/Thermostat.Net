using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ThermostatAutomation.Rules
{
    public class Fixed : SimpleRulesEngine
    {
        const decimal ComfortTemp = 21.0m;

        /// <summary>
        /// Need a readable name for voice controll integration.
        /// </summary>
        public override string Name => "Fixed";

        /// <summary>
        /// Overriding the max temp as it best suits me and I don't want accidentaly going over that limit.
        /// </summary>
        public override decimal MaxTemperature => 22.5m;

        protected override void BuildRules()
        {
            // This needs to be a constant temp
            Rules.Add(new Rule("default")
            {
                Temperature = ComfortTemp
            });
        }
    }
}
