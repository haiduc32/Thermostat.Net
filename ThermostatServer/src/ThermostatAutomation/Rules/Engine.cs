using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ThermostatAutomation.Rules
{
    public class Engine
    {
        //private static readonly object _lockObject = new object();
        private IRulesEngine _rulesEngine;

        public Repository Repository { get; set; }

        public IRulesEngine Rules
        {
            get
            {
                if (_rulesEngine == null)
                {
                    throw new ArgumentNullException("SelectedRulesEngine property must be set before accessing the Instance property.");
                }
                return _rulesEngine;
            }
        }

        public void Enable(string selectedEngine)
        {
            Type engineType = Type.GetType("ThermostatAutomation.Rules." + selectedEngine);
            var newRulesEngine = (IRulesEngine)Activator.CreateInstance(engineType);

            newRulesEngine.Repository = Repository;
            newRulesEngine.Initialize();

            // switch to the new engine
            _rulesEngine = newRulesEngine;
        }
    }
}
