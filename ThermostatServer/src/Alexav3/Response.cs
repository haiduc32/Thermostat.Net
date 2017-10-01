using System;
using System.Collections.Generic;
using System.Text;

namespace AlexaV3
{
    public class Response
    {
        public Context Context { get; set; }

        public Event Event { get; set; }
        public Payload Payload { get; set; }
    }
}
