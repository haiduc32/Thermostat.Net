using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThermostatAutomation.Configuration;
using ThermostatAutomation.Rules;

namespace ThermostatAutomation.Controllers
{
    [AlexaOAuth]
    public class AlexaController : Controller
    {
        //values below or above the limit will be ignored
        private const decimal UpperLimit = 30m;

        private const decimal LowerLimit = 12m;

        private Engine _engine;

        public AlexaController(Engine engine)
        {
            _engine = engine;
        }

        [HttpGet]
        public IActionResult Discover(string accessToken)
        {
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
                actions = new List<string> { "setTargetTemperature", "getTargetTemperature", "getTemperatureReading", "incrementTargetTemperature", "decrementTargetTemperature" },
                applianceTypes = new List<string> { "THERMOSTAT" }
            }));

            appliances.Add(Status.Instance.Rules.Select(x => new
            {
                applianceId = x.ClassName + "Engine",
                manufacturerName = "Thermosta.Net",
                modelName = "custom",
                version = "VER01",
                friendlyName = x.FriendlyName,
                friendlyDescription = x.FriendlyName,
                isReachable = true,
                actions = new List<string> { "turnOn"/*, "turnOff"*/ }, // we don't need to turn it off as we only switch between different engines
                applianceTypes = new List<string> { "SCENE_TRIGGER" }
            }));

            return Ok(appliances);
        }

        [HttpGet]
        public IActionResult On(string applianceId, string accessToken)
        {
            RuleItem rulesEngine = Status.Instance.Rules.Single(x => applianceId == x.ClassName);
            if (rulesEngine != null)
            {
                _engine.Enable(rulesEngine.ClassName);
            }

            return Ok();
        }

        [HttpGet]
        public IActionResult Off(string applianceId, string accessToken)
        {
            // not sure if we need it at all, as we don't have anything to turn off at the moment
            return BadRequest();
        }

        [HttpGet]
        public IActionResult SetTemp(string applianceId, string accessToken, decimal temp)
        {
            string zone = applianceId.Substring(0, applianceId.IndexOf("Thermostat"));

            decimal setTemperature = _engine.Rules.OverrideTargetTemperatureInZone(zone, temp);

            object response = new
            {
                targetTemperature = new
                {
                    value = setTemperature
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
            return OffsetTemperature(applianceId, delta);
        }

        [HttpGet]
        public IActionResult DecTemp(string applianceId, string accessToken, decimal delta)
        {
            return OffsetTemperature(applianceId, -delta);
        }

        private IActionResult OffsetTemperature(string applianceId, decimal delta)
        {
            string zone = applianceId.Substring(0, applianceId.IndexOf("Thermostat"));
            decimal? currentTemperature = _engine.Rules.GetTargetTemperatureInZone(zone);

            if (currentTemperature.HasValue)
            {
                decimal newTemperature = _engine.Rules.OverrideTargetTemperatureInZone(zone, currentTemperature.Value + delta);

                object response = new
                {
                    previousState = new
                    {
                        targetTemperature = new
                        {
                            value = currentTemperature.Value
                        },
                        mode = new
                        {
                            value = "AUTO"
                        }
                    },
                    targetTemperature = new
                    {
                        value = newTemperature
                    }
                };

                return Ok(response);
            }
            else
            {
                //TODO: some error?
                return Ok();
            }
        }
    }
}
