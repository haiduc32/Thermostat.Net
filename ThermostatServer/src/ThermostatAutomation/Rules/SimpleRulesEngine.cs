
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThermostatAutomation.Configuration;
using ThermostatAutomation.Extensions;
using ThermostatAutomation.Models;

namespace ThermostatAutomation.Rules
{
    public class Rule
    {
        /// <summary>
        /// Must be an unique name within a set of rules.
        /// </summary>
        public string Name { get; private set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public List<DayOfWeek> DaysOfTheWeek { get; set; }

        public string Zone { get; set; }
        public decimal Temperature { get; set; }

        public Rule(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// This is a simple engine base for buildig time interval rules.
    /// </summary>
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

        public abstract string Name { get; }

        protected List<Rule> Rules { get; } = new List<Rule>();

        public Repository Repository { get; set; }

        /// <summary>
        /// Minimum allowed temperature.
        /// </summary>
        public virtual decimal MinTemperature { get { return 14m; } }

        /// <summary>
        /// Maximum allowed temperature.
        /// </summary>
        public virtual decimal MaxTemperature { get { return 28m; } }
        

        public SimpleRulesEngine()
        {
            // all rules are evaluated in order. once a rule satisfies the conditions
            // the rest of the rules are not evaluated
            //Rules = new List<Rule>();
        }

        public virtual void Initialize()
        {
            BuildRules();
            ApplyOverrides();
        }

        public bool Evaluate(int channel)
        {
            //TODO: to avoid noise-spikes, implement a filter to verify that the trend is raising/falling for a few consecutive readings -> go into the historic data 

            // the current version does not have pre-emptive start. ex: if at 6:00 the expected temp
            // is 20* and current is 12*, heating will start only at 6:00, instead of estimating the
            // time to heat and starting the heating in advance.
            // this is a feature that needs someway to extrapolate the expected heating time
            // depending on current temperature inside and outside and the expected temperature.
            // need more data to figure out a formula.
            ChannelSettings settings = Status.Instance.Settings.Channels.SingleOrDefault(x => x.Number == channel.ToString());
            DateTime now = DateTime.Now;
            
            //check what rule applies now (in theory we should always have at least the safety net rule)
            Rule activeRule = ActiveRule(settings);

            //try to get the temperature for the targeted zone 
            Zone activeZone = Status.Instance.Zones.SingleOrDefault(x => x.Name == activeRule.Zone && !x.Timestamp.IsStale() && x.Temperature.HasValue) ??
                Status.Instance.Zones.FirstOrDefault(x => IsInChannel(x.Name, settings) && !x.Timestamp.IsStale() && x.Temperature.HasValue);

            if (activeZone == null || !activeZone.Temperature.HasValue)
            {
                //this situation happens when there are absolutely no values
                // we'll just wait till there are any values.
                return false;
            }

            return activeZone.Temperature.Value < activeRule.Temperature;
        }

        public decimal OverrideTargetTemperatureInZone(string zoneName, decimal targetTemperature)
        {
            //settings are detected based on the zone that is overriden
            ChannelSettings settings = Status.Instance.Settings.Channels.SingleOrDefault(x => x.Zones.Contains(zoneName));
            Rule activeRule = ActiveRule(settings);

            //ensure the temperature is valid, and override it if not
            targetTemperature = ValidateTemperature(targetTemperature);

            activeRule.Zone = zoneName;
            activeRule.Temperature = targetTemperature;

            Repository.AddOverride(Name, new Models.RuleOverrideModel
            {
                EngineName = Name,
                RuleName = activeRule.Name,
                Temperature = targetTemperature,
                Zone = zoneName
            });

            return targetTemperature;
        }

        public decimal? GetTargetTemperatureInZone(string zoneName)
        {
            ChannelSettings settings = Status.Instance.Settings.Channels.SingleOrDefault(x => x.Zones.Contains(zoneName));
            Rule activeRule = ActiveRule(settings);

            return activeRule.Temperature;
        }

        public void ResetRules()
        {
            Repository.DeleteOverrides(Name);
            Rules.Clear();
            BuildRules();
            
        }

        protected abstract void BuildRules();

        private decimal ValidateTemperature(decimal temperature)
        {
            if (temperature < MinTemperature) return MinTemperature;
            if (temperature > MaxTemperature) return MaxTemperature;

            return temperature;
        }

        private bool IsInTimeInterval(Rule rule) => (rule.StartTime == rule.EndTime) || (DateTime.Today + rule.StartTime <= DateTime.Now && DateTime.Now < DateTime.Today + rule.EndTime);

        private bool IsInDayOfTheWeek(Rule rule) => rule.DaysOfTheWeek == null || rule.DaysOfTheWeek.Contains(DateTime.Today.DayOfWeek);

        private bool IsInChannel(string zone, ChannelSettings settings)
        {
            //catch all
            if (String.IsNullOrEmpty(zone)) return true;

            return settings.Zones.Any(x => x == zone);
        }

        private Rule ActiveRule(ChannelSettings settings)
        {
             return Rules.First(rule => IsInTimeInterval(rule) && IsInDayOfTheWeek(rule) && IsInChannel(rule.Zone, settings));
        }

        private void ApplyOverrides()
        {
            List<RuleOverrideModel> overrides = Repository.GetOverrides(Name);
            foreach (RuleOverrideModel o in overrides)
            {
                Rule rule = Rules.SingleOrDefault(x => x.Name == o.RuleName);
                if (rule != null)
                {
                    rule.Zone = o.Zone;
                    rule.Temperature = o.Temperature;
                }
            }
        }
    }
}
