using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ThermostatAutomation.Rules
{
    public interface IRulesEngine
    {
        bool Evaluate(int channel = 0);
    }
}
