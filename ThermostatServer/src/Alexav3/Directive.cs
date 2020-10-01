using System;

namespace AlexaV3
{
    public class Directive
    {
        public Header Header { get; set; }
        public Payload Payload { get; set; }
        public Endpoint Endpoint { get; set; }
    }
}
