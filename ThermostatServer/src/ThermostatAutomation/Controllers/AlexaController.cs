using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ThermostatAutomation.Controllers
{
    public class AlexaController : Controller
    {
        //values below or above the limit will be ignored
        private const decimal UpperLimit = 30m;

        private const decimal LowerLimit = 12m;

        [HttpGet]
        public IActionResult Discover(string accessToken)
        {
            /*
            var appliances = [];

            var applianceDiscovered = {
                applianceId: 'e145-4062-b31d-7ec2c146c5ea',
                manufacturerName: 'DummyInfo',
                modelName: 'ST01',
                version: 'VER01',
                friendlyName: 'Dummy light',
                friendlyDescription: 'the light in kitchen',
                isReachable: true,
                additionalApplianceDetails: {
            
                    'fullApplianceId': '2cd6b650-e145-4062-b31d-7ec2c146c5ea'
                }
            };
            appliances.push(applianceDiscovered);
            */

            //TODO: un-hardcode
            
            List<object> appliances = new List<object>();

            appliances.AddRange(Status.Instance.Zones.Select(x => new
            {
                applianceId = x.Name + "Thermostat",
                manufacturerName = "Thermosta.Net",
                modelName = "custom",
                version = "VER01",
                friendlyName = x.Name + " thermostat",
                friendlyDescription = "the thermostat in the " + x.Name,
                isReachable = true,
                actions = new List<string> { "setTargetTemperature", "incrementTargetTemperature", "decrementTargetTemperature" }
            }));

            //object officeThermostat = new
            //{
            //    applianceId = "1",
            //    manufacturerName = "Radu",
            //    modelName = "custom",
            //    version = "VER01",
            //    friendlyName = "Office thermostat",
            //    friendlyDescription = "the thermostat in the office",
            //    isReachable = true,
            //    actions = new List<string> { "setTargetTemperature", "incrementTargetTemperature", "decrementTargetTemperature" }
            //};
            //appliances.Add(officeThermostat);

            //object bedroomThermostat = new
            //{
            //    applianceId = "2",
            //    manufacturerName = "Radu",
            //    modelName = "custom",
            //    version = "VER01",
            //    friendlyName = "Bedroom thermostat",
            //    friendlyDescription = "the thermostat in the bedroom",
            //    isReachable = true,
            //    actions = new List<string> { "setTargetTemperature", "incrementTargetTemperature", "decrementTargetTemperature" }
            //};
            //appliances.Add(bedroomThermostat);

            //object heating = new
            //{
            //    applianceId = "3",
            //    manufacturerName = "Radu",
            //    modelName = "custom",
            //    version = "VER01",
            //    friendlyName = "Heating",
            //    friendlyDescription = "home heating",
            //    isReachable = true,
            //    actions = new List<string> { "turnOn", "turnOff" }
            //};
            //appliances.Add(heating);

            return Ok(appliances);
        }

        [HttpGet]
        public IActionResult On(string applianceId, string accessToken)
        {
            if (accessToken != "atoken")
            {
                return Forbid();
            }

            return Ok();
        }

        [HttpGet]
        public IActionResult Off(string applianceId, string accessToken)
        {
            if (accessToken != "atoken")
            {
                return Forbid();
            }

            return Ok();
        }

        [HttpGet]
        public IActionResult SetTemp(string applianceId, string accessToken, decimal temp)
        {
            if (accessToken != "atoken")
            {
                return Forbid();
            }

            if (temp < LowerLimit || temp > UpperLimit)
            {
                //TODO: return a bad response
            }

            object response = new
            {
                targetTemperature = new
                {
                    value = 21.0
                },
                mode = new
                {
                    value = "AUTO"
                }
            };

            return Ok(response);
        }

        [HttpGet]
        public IActionResult IncTemp(string applianceId, string accessToken, decimal delta)
        {
            if (accessToken != "atoken")
            {
                return Forbid();
            }

            object response = new
            {
                previousState = new
                {
                    targetTemperature = new
                    {
                        value = 21.0
                    },
                    mode = new
                    {
                        value = "AUTO"
                    }
                },
                targetTemperature = new
                {
                    value = 21m + delta
                }
            };

            return Ok(response);
        }

        [HttpGet]
        public IActionResult DecTemp(string applianceId, string accessToken, decimal delta)
        {
            if (accessToken != "atoken")
            {
                return Forbid();
            }

            object response = new
            {
                previousState = new
                {
                    targetTemperature = new
                    {
                        value = 21.0
                    },
                    mode = new
                    {
                        value = "AUTO"
                    }
                },
                targetTemperature = new
                {
                    value = 21m - delta
                }
            };

            return Ok(response);
        }
    }
}
