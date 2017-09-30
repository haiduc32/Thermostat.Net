/*
 * This are the rules that are specific to my home.
 * It's split in time intervals when I am home, and zones 
 * that must be heated dependign on the time of the day
 * and with what temperature.
 * 
 * Each user of the framework should have a similar class implemented
 * specific to his house and habbits.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ThermostatAutomation.Rules
{
    public class RadusRules : SimpleRulesEngine
    {
        const decimal ComfortTemp = 21.5m;

        /// <summary>
        /// Need a readable name for voice controll integration.
        /// </summary>
        public override string Name => "Daily";

        /// <summary>
        /// Overriding the max temp as it best suits me and I don't want accidentaly going over that limit.
        /// </summary>
        public override decimal MaxTemperature => 22.5m;

        protected override void BuildRules()
        {
            //add all the rules here, rule are evaluated in the order their added:
            // the first rule that matches will be executed, even if it overrides a rule added later.
            //morning rule
            Rules.Add(new Rule("Morning")
            {
                StartTime = new TimeSpan(5, 30, 0),
                EndTime = new TimeSpan(7, 00, 0),
                DaysOfTheWeek = WorkingDays,
                Zone = "Living",
                Temperature = ComfortTemp
            });
            //evening rule
            Rules.Add(new Rule("Evening")
            {
                StartTime = new TimeSpan(18, 00, 0),
                EndTime = new TimeSpan(22, 00, 0),
                DaysOfTheWeek = WorkingDays,
                Zone = "Office",
                Temperature = ComfortTemp
            });
            //bedtime rule (applies to all days)
            Rules.Add(new Rule("Bedtime")
            {
                StartTime = new TimeSpan(22, 00, 0),
                EndTime = new TimeSpan(22, 30, 0),
                Zone = "Bedroom",
                Temperature = 20m
            });
            //for the weekend keep it on all the time (except at night of course..)
            Rules.Add(new Rule("Weekend")
            {
                StartTime = new TimeSpan(7, 30, 0),
                EndTime = new TimeSpan(22, 00, 0),
                //DaysOfTheWeek = new List<DayOfWeek> { DayOfWeek.Saturday, DayOfWeek.Sunday },
                Zone = "Office",
                Temperature = ComfortTemp
            });

            // do we need a safety net? if no rule found apply this rule
            Rules.Add(new Rule("default")
            {
                Temperature = 18m
            });
        }
    }
}
