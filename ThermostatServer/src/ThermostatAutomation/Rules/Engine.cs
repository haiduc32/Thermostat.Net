using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ThermostatAutomation.Rules
{
    public static class Engine
    {
        private static readonly object _lockObject = new object();
        private static IRulesEngine _rulesEngine;

        public static Type SelectedRulesEngine { get; set; }

        public static Repository Repository { get; set; }

        public static IRulesEngine Instance
        {
            get
            {
                if (_rulesEngine == null)
                {
                    lock (_lockObject)
                    {
                        if (_rulesEngine == null)
                        {
                            if (SelectedRulesEngine == null)
                            {
                                throw new ArgumentNullException("SelectedRulesEngine property must be set before accessing the Instance property.");
                            }

                            _rulesEngine = (IRulesEngine)Activator.CreateInstance(SelectedRulesEngine);

                            _rulesEngine.Repository = Repository;
                            _rulesEngine.Initialize();
                        }
                    }
                }
                return _rulesEngine;
            }
        }
    }
}
