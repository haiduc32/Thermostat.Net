using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ThermostatAutomation.Rules
{
    public class DayAndNight : SimpleRulesEngine
    {
        const decimal ComfortTemp = 21.5m;

        /// <summary>
        /// Need a readable name for voice controll integration.
        /// </summary>
        public override string Name => "Day And Night";

        /// <summary>
        /// Overriding the max temp as it best suits me and I don't want accidentaly going over that limit.
        /// </summary>
        public override decimal MaxTemperature => 22.5m;

        protected override void BuildRules()
        {
            //add all the rules here, rule are evaluated in the order their added:
            // the first rule that matches will be executed, even if it overrides a rule added later.
            //day rule
            Rules.Add(new Rule("Day")
            {
                StartTime = new TimeSpan(6, 00, 0),
                EndTime = new TimeSpan(21, 00, 0),
                //DaysOfTheWeek = WorkingDays,
                Zone = "Office",
                Temperature = ComfortTemp
            });
            //night rule
            Rules.Add(new Rule("Night")
            {
                StartTime = new TimeSpan(21, 00, 0),
                EndTime = new TimeSpan(6, 00, 0),
                //DaysOfTheWeek = WorkingDays,
                Zone = "Bedroom",
                Temperature = 18.5m
            });

            // do we need a safety net? if no rule found apply this rule
            Rules.Add(new Rule("default")
            {
                Temperature = 18m
            });
        }
    }
}
