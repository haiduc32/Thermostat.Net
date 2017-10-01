using System;
using System.Collections.Generic;
using System.Text;

namespace AlexaV3
{
    public class Capability
    {
        /// <summary>
        /// Indicates the type of capability, which determines what fields the capability has.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The qualified name of the Alexa.Interface that describes the actions for the device.
        /// </summary>
        public string Interface { get; set; }

        /// <summary>
        /// Indicates the interface version that this endpoint supports.
        /// </summary>
        public string Version { get; set; } = "3"; //currently supported version is 3

        /// <summary>
        /// Indicates the properties of the interface which are supported by this endpoint in the format "name":"&lt;propertyName&gt;". If you do not specify a reportable property of the interface in this array, the default is to assume that proactivelyReported and retrievable for that property are false.
        /// </summary>
        public Properties Properties { get; set; }
    }
}
