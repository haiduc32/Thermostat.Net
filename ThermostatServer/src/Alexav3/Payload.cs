using System;
using System.Collections.Generic;
using System.Text;

namespace AlexaV3
{
    public class Payload
    {
        public Scope Scope { get; set; }
        public List<Endpoint> Endpoints { get; set; }
        public Temperature TargetSetpoint { get; set; }
        public string Timestamp { get; set; }
        public Cause Cause { get; set; }
    }
}
