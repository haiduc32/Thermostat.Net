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
            // TODO: I don't like the hardcoding here.. not sure what to do 
            else if (directive.Header.Name == Names.ReportState && directive.Endpoint.EndpointId == "DefaultThermostat")
            {
                return HandleThermostatReportState(directive);
            }
            else if (directive.Header.Namespace == Namespaces.SceneController)
            {
                return HandleSceneController(directive);
            }

            return BadRequest();
        }

        private IActionResult HandleDiscovery(Directive directive)
        {
            List<Endpoint> endpoints = new List<Endpoint>();
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
                        Type = "AlexaInterface",
                        Interface = Interfaces.ThermostatController,
                        Properties = new Properties
                        {
                            Supported = new List<NameValue>
                            {
                                new NameValue { Name = "targetSetpoint"},
                                new NameValue { Name = "thermostatMode"}
                            },
                            ProactivelyReported = false,
                            Retrievable = true
                        }
                    }
                }
            };
            endpoints.Add(thermostatEndpoint);

            //add available modes
            endpoints.AddRange(Status.Instance.Rules.Select(x => new Endpoint
            {
                EndpointId = x.ClassName + "ThermostaMode",
                FriendlyName = x.FriendlyName + " Thermostat Mode",
                Description = "A custom operation mode for the thermostat",
                ManufacturerName = "Thermostat.Net",
                DisplayCategories = new List<string> { DisplayCategories.ActivityTrigger },
                Cookie = new object(),
                Capabilities = new List<Capability>
                {
                    new Capability
                    {
                        Type = "AlexaInterface",
                        Interface = Interfaces.SceneController,
                        Properties = new Properties
                        {
                            Supported = new List<NameValue>
                            {
                                new NameValue { Name = "targetSetpoint"},
                                new NameValue { Name = "thermostatMode"}
                            },
                            ProactivelyReported = false,
                            SupportsDeactivation = false
                        }
                    }
                }
            }));
            
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
                        Endpoints = endpoints
                    }
                }
            };

            return Ok(response);
        }

        /// <summary>
        /// Handles Thermostat operations.
        /// </summary>
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
            Response r = BuildThermostatResponse(directive, newValue, Names.Response);
            
            return Ok(r);
        }
        
        private IActionResult HandleThermostatReportState(Directive directive)
        {
            //TODO: hardcoded the channel to zero but should figure it out from the thermostat
            decimal? target = _engine.Rules.GetTargetTemperatureInZone(_engine.Rules.GetActiveZone(0));

            if (!target.HasValue)
            {
                //TODO: should return a propper error here...
                return BadRequest();
            }

            var r = BuildThermostatResponse(directive, target.Value, Names.StateReport);

            return Ok(r);
        }

        private IActionResult HandleSceneController(Directive directive)
        {
            if (directive.Header.Name != "Activate") return BadRequest();

            RuleItem rulesEngine = Status.Instance.Rules.Single(x => directive.Endpoint.EndpointId == x.ClassName + "ThermostaMode");
            if (rulesEngine != null)
            {
                _engine.Enable(rulesEngine.ClassName);
            }

            return Ok(BuildSceneResponse(directive));
        }

        private Response BuildSceneResponse(Directive directive)
        {
            string timeOfSample = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.ffZ");

            var r = new Response
            {
                Context = new Context(),
                Event = new Event
                {
                    Header = new Header
                    {
                        Namespace = Namespaces.SceneController,
                        Name = "ActivationStarted",
                        MessageId = Guid.NewGuid().ToString(),
                        CorrelationToken = directive.Header.CorrelationToken
                    },
                    Endpoint = new Endpoint
                    {
                        // Scope
                        EndpointId = directive.Endpoint.EndpointId
                    },
                    Payload  = new Payload
                    {
                        Cause = new Cause
                        {
                            Type = "VOICE_INTERACTION"
                        },
                        Timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.ffZ")
        }
                }
            };

            return r;
        }

        private static Response BuildThermostatResponse(Directive directive, decimal newValue, string eventName)
        {
            string timeOfSample = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.ffZ");

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
                            TimeOfSample = timeOfSample
                        },
                        new ContextProperty
                        {
                            Namespace = Namespaces.Thermostat,
                            Name = "thermostatMode",
                            Value = "HEAT",
                            TimeOfSample = timeOfSample
                        },
                        new ContextProperty
                        {
                            Namespace = Namespaces.EndpointHealth,
                            Name = "connectivity",
                            Value = new
                            {
                                Value = "OK"
                            },
                            TimeOfSample = timeOfSample
                        }
                    }
                },
                Event = new Event
                {
                    Header = new Header
                    {
                        Namespace = Namespaces.Alexa,
                        Name = eventName, //Names.Response, //StateReport
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
            return r;
        }

    }
}
