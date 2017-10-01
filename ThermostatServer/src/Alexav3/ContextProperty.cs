using System;
using System.Collections.Generic;
using System.Text;

namespace AlexaV3
{
    public class ContextProperty
    {
        public string Namespace { get; set; }
        public string Name { get; set; }
        public object Value { get; set; }

        public string TimeOfSample { get; set; }
        public int UncertaintyInMilliseconds { get; } = 500;
    }
}
