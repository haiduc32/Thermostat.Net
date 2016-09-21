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
        public RadusRules() : base()
        {
            //add all the rules here
            //morning rule
            Rules.Add(new Rule
            {
                StartTime = new TimeSpan(6, 30, 0),
                EndTime = new TimeSpan(7, 20, 0),
                DaysOfTheWeek = WorkingDays,
                Zone = "Living",
                Temperature = 23m
            });
            //evening rule
            Rules.Add(new Rule
            {
                StartTime = new TimeSpan(18, 30, 0),
                EndTime = new TimeSpan(22, 30, 0),
                DaysOfTheWeek = WorkingDays,
                Zone = "Office",
                Temperature = 22m
            });
            //night rule (applies to all days)
            Rules.Add(new Rule
            {
                StartTime = new TimeSpan(18, 30, 0),
                EndTime = new TimeSpan(22, 30, 0),
                Zone = "Bedroom",
                Temperature = 20m
            });
            //for the weekend keep it on all the time
            Rules.Add(new Rule
            {
                DaysOfTheWeek = new List<DayOfWeek> { DayOfWeek.Saturday, DayOfWeek.Sunday },
                Zone = "Office",
                Temperature = 22m
            });

            // do we need a safety net? if no rule found apply this rule
            Rules.Add(new Rule
            {
                Temperature = 18m
            });
        }
    }
}
