using System;
using System.Collections.Generic;
using System.Text;

namespace AlexaV3
{
    public class Endpoint
    {
        /// <summary>
        /// A device identifier. The identifier must be unique across all devices owned by an end user within the domain for the skill. In addition, the identifier needs to be consistent across multiple discovery requests for the same device. An identifier can contain any letter or number and the following special characters: _ - = # ; : ? @ &. The identifier cannot exceed 256 characters.
        /// </summary>
        public string EndpointId { get; set; }

        /// <summary>
        /// The name of the device manufacturer. This value cannot exceed 128 characters.
        /// </summary>
        public string ManufacturerName { get; set; }

        /// <summary>
        /// The name used by the customer to identify the device. This value cannot exceed 128 characters and should not contain special characters or punctuation.
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        /// A human-readable description of the device. This value cannot exceed 128 characters. The description should contain the manufacturer name or how the device is connected. For example, “Smart Lock by Sample Manufacturer” or “WiFi Thermostat connected via SmartHub”. This value cannot exceed 128 characters.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Indicates the group name where the device should display in the Alexa app. Current supported values are ‘LIGHT’, ‘SMARTPLUG’, ‘SWITCH’, ‘CAMERA’, ‘DOOR’, “TEMPERATURE_SENSOR”, ‘THERMOSTAT’, ‘SMARTLOCK’, ‘SCENE_TRIGGER’, ‘ACTIVITY_TRIGGER’, ‘OTHER’. See Display categories
        /// </summary>
        public List<string> DisplayCategories { get; set; }

        //TODO: set it up
        public object Cookie { get; set; } // is optional, i'm going to skipt it

        /// <summary>
        /// An array of capability objects that represents actions particular device supports and can respond to. A capability object can contain different fields depending on the type.
        /// </summary>
        public List<Capability> Capabilities { get; set; }
    }
}
