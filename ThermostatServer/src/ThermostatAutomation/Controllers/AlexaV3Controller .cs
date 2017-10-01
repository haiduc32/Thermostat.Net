using AlexaV3;
using AlexaV3.Constants;
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
    //[AlexaOAuth]
    public class AlexaV3Controller : Controller
    {
        //values below or above the limit will be ignored
        private const decimal UpperLimit = 30m;

        private const decimal LowerLimit = 12m;

        private Engine _engine;

        public AlexaV3Controller(Engine engine)
        {
            _engine = engine;
        }

        [HttpPost]
        public IActionResult Handle([FromBody]Directive directive)
        {
            if (directive.Header.Namespace == Namespaces.Discovery)
            {
                return HandleDiscovery(directive);
            }
            else if (directive.Header.Namespace == Namespaces.Thermostat)
            {
                return HandleThermostat(directive);
            }

            return BadRequest();
        }

        private IActionResult HandleDiscovery(Directive directive)
        {
            var thermostatEndpoint = new Endpoint
            {
                EndpointId = "DefaultThermostat",
                FriendlyName = "Thermostat",
                Description = "A custom thermostat",
                ManufacturerName = "Thermostat.Net",
                DisplayCategories = new List<string> { DisplayCategories.Thermostat },
                Cookie = new object(),
                Capabilities = new List<Capability>
                {
                    new Capability
                    {
                        Type = "Alexa.Interface",
                        Interface = Interfaces.ThermostatController,
                        Properties = new Properties
                        {
                            Supported = new List<NameValue>
                            {
                                new NameValue { Name = "targetSetpoint"},
                                new NameValue { Name = "thermostatMode"}
                            },
                            ProactivelyReported = true,
                            Retrievable = true
                        }
                    }
                }
            };
            
            var response = new Response
            { 
                Event = new Event
                {
                    Header = new Header
                    {
                        Namespace = Namespaces.Discovery,
                        Name = Names.DiscoveryResponse, //TODO: this is inconsistent with documentation, should be just "Response"? There seems to be a inconsistence in the documentation as both versions can be seen
                        MessageId = Guid.NewGuid().ToString()
                    },
                    Payload = new Payload
                    {
                        Endpoints = new List<Endpoint>
                    {
                        thermostatEndpoint
                    }
                    }
                }
            };

            return Ok(response);
        }

        private IActionResult HandleThermostat(Directive directive)
        {
            if (directive.Header.Name == Names.SetTargetTemperature)
            {
                return HandleSetTargetTemperature(directive);
            }

            throw new InvalidOperationException("Received an unexpected Name: " + directive.Header.Name);
        }

        private IActionResult HandleSetTargetTemperature(Directive directive)
        {
            if (directive.Payload.TargetSetpoint.Scale != "CELSIUS") return BadRequest();

            decimal newValue = directive.Payload.TargetSetpoint.Value;
            if (newValue > UpperLimit || newValue < LowerLimit) return BadRequest();

            string activeZone = _engine.Rules.GetActiveZone(0); //TODO: unhardcode

            //NOTE: the value could be overriden here
            newValue = _engine.Rules.OverrideTargetTemperatureInZone(activeZone, newValue);

            var r = new Response
            {
                Context = new Context
                {
                    Properties = new List<ContextProperty>
                    {
                        new ContextProperty
                        {
                            Namespace = Namespaces.Thermostat,
                            Name = "targetSetpoint",
                            Value = new
                            {
                                Value = newValue,
                                Scale = "CELSIUS"
                            },
                            TimeOfSample = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.ffZ")
                        },
                        new ContextProperty
                        {
                            Namespace = Namespaces.Thermostat,
                            Name = "thermostatMode",
                            Value = "HEAT",
                            TimeOfSample = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.ffZ")
                        }
                    }
                },
                Event = new Event
                {
                    Header = new Header
                    {
                        Namespace = Namespaces.Alexa,
                        Name = Names.Response,
                        MessageId = Guid.NewGuid().ToString(),
                        CorrelationToken = directive.Header.CorrelationToken
                    },
                    Endpoint = new Endpoint
                    {
                        // Scope
                        EndpointId = directive.Endpoint.EndpointId
                    }
                },
                Payload = new Payload()
            };

            return Ok(r);
        }

    }
}
