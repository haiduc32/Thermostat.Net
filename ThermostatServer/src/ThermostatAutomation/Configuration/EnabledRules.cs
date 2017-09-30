using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ThermostatAutomation.Configuration
{
    public class EnabledRules : List<RuleItem>
    {
        /// <summary>
        /// Gets the first item that is set as Default, or first item in the list, or null otherwise.
        /// </summary>
        public RuleItem Default => this.FirstOrDefault(x => x.Default) ?? this.FirstOrDefault();
    }

    public class RuleItem
    {
        public string FriendlyName { get; set; }
        public string ClassName { get; set; }
        public bool Default { get; set; }
    }
}
