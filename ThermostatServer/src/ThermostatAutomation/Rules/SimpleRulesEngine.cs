
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ThermostatAutomation.Rules
{
    public class Rule
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public List<DayOfWeek> DaysOfTheWeek { get; set; }

        public string Zone { get; set; }
        public decimal Temperature { get; set; }
    }

    public abstract class SimpleRulesEngine : IRulesEngine
    {
        protected static List<DayOfWeek> WorkingDays = new List<DayOfWeek>
            {
                DayOfWeek.Monday,
                DayOfWeek.Tuesday,
                DayOfWeek.Wednesday,
                DayOfWeek.Thursday,
                DayOfWeek.Friday
            };

        protected List<Rule> Rules { get; set; } = new List<Rule>();

        public SimpleRulesEngine()
        {
            

            // all rules are evaluated in order. once a rule satisfies the conditions
            // the rest of the rules are not evaluated
            Rules = new List<Rule>();

        }

        public bool Evaluate(int channel)
        {
            // the current version does not have pre-emptive start. ex: if at 6:00 the expected temp
            // is 20* and current is 12*, heating will start only at 6:00, instead of estimating the
            // time to heat and starting the heating in advance.
            // this is a feature that needs someway to extrapolate the expected heating time
            // depending on current temperature inside and outside and the expected temperature.
            // need more data to figure out a formula.
            ChannelSettings settings = Status.Instance.Settings.Channels.SingleOrDefault(x => x.Number == channel.ToString());
            DateTime now = DateTime.Now;
            
            //check what rule applies now (in theory we should always have at least the safety net rule)
            Rule activeRule = Rules.First(rule => IsInTimeInterval(rule) && IsInDayOfTheWeek(rule) && IsInChannel(rule.Zone, settings));

            //try to get the temperature for the targeted zone 
            Zone activeZone = Status.Instance.Zones.SingleOrDefault(x => x.Name == activeRule.Zone) ??
                Status.Instance.Zones.FirstOrDefault(x => IsInChannel(x.Name, settings) && !IsStale(x) && x.Temperature.HasValue);

            if (!activeZone.Temperature.HasValue)
            {
                //this situation happens when there are absolutely no values
                // we'll just wait till there are any values.
                return false;
            }

            return activeZone.Temperature.Value < activeRule.Temperature;
        }

        private bool IsStale(Zone zone) => !zone.Timestamp.HasValue || zone.Timestamp.Value.AddMinutes(-5) < DateTime.Now;

        private bool IsInTimeInterval(Rule rule) => (rule.StartTime == rule.EndTime) || (DateTime.Today + rule.StartTime <= DateTime.Now && DateTime.Now < DateTime.Today + rule.EndTime);

        private bool IsInDayOfTheWeek(Rule rule) => rule.DaysOfTheWeek == null || rule.DaysOfTheWeek.Contains(DateTime.Today.DayOfWeek);

        private bool IsInChannel(string zone, ChannelSettings settings)
        {
            //catch all
            if (String.IsNullOrEmpty(zone)) return true;

            return settings.Zones.Any(x => x == zone);
        }

    }
}
