using System;
using System.Collections.Generic;
using System.Text;

namespace AlexaV3
{
    public class Header
    {
        public string Namespace { get; set; }
        public string Name { get; set; }
        public string PayloadVersion { get; } = "3";
        public string MessageId { get; set; }
        public string CorrelationToken { get; set; }
    }
}
