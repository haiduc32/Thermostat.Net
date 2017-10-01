using System;
using System.Collections.Generic;
using System.Text;

namespace AlexaV3
{
    public class Properties
    {
        /// <summary>
        /// Indicates the properties of the interface which are supported by this endpoint in the format "name":"<propertyName>". If you do not specify a reportable property of the interface in this array, the default is to assume that proactivelyReported and retrievable for that property are false.
        /// </summary>
        public List<NameValue> Supported { get; set; }

        /// <summary>
        /// Indicates whether the properties listed for this endpoint generate Change.Report events.
        /// </summary>
        public bool ProactivelyReported { get; set; }

        /// <summary>
        /// Indicates whether the properties listed for this endpoint can be retrieved for state reporting.
        /// </summary>
        public bool Retrievable { get; set; }
    }
}
