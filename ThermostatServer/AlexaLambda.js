/**
This is the Lambda function for the Alexa skill.
*/

var https = require('https');
var REMOTE_CLOUD_BASE_PATH = "/thermostat/alexa/";
var REMOTE_CLOUD_HOSTNAME = "haiduc32.go.ro";

var log = log;
var generateControlError = generateControlError;

/**
 * Main entry point.
 * Incoming events from Alexa Lighting APIs are processed via this method.
 */
exports.handler = function (event, context) {
    // Warning! Logging this in production might be a security problem.
    log('Input', event);

    switch (event.header.namespace) {

        /**
         * The namespace of "Discovery" indicates a request is being made to the lambda for
         * discovering all appliances associated with the customer's appliance cloud account.
         * can use the accessToken that is made available as part of the payload to determine
         * the customer.
         */
        case 'Alexa.ConnectedHome.Discovery':
            handleDiscovery(event, context);
            break;

		/**
		 * The namespace of "Control" indicates a request is being made to us to turn a
		 * given device on, off or brighten. This message comes with the "appliance"
		 * parameter which indicates the appliance that needs to be acted on.
		 */
        case 'Alexa.ConnectedHome.Control':
            handleControl(event, context);
            break;

		/**
		 * We received an unexpected message
		 */
        default:
            // Warning! Logging this in production might be a security problem.
            log('Err', 'No supported namespace: ' + event.header.namespace);
            context.fail('Something went wrong');
            break;
    }
}

/**
 * This method is invoked when we receive a "Discovery" message from Alexa Connected Home Skill.
 * We are expected to respond back with a list of appliances that we have discovered for a given
 * customer. 
 */
function handleDiscovery(event, context) {
    var accessToken = event.payload.accessToken;
    accessToken = accessToken.trim();

    //interogate the remote service of connected devices
    var options = {
        hostname: REMOTE_CLOUD_HOSTNAME,
        port: 443,
        path: REMOTE_CLOUD_BASE_PATH + 'discover?accesstoken=' + accessToken,
        headers: {
            accept: 'application/json'
        }
    };

    var callback = function (response) {
        var str = '';

        response.on('data', function (chunk) {
            // TODO: Add string limit here
            str += chunk.toString('utf-8');
        });

        response.on('end', function () {
            /**
             * Test the response from remote endpoint (not shown) and craft a response message
             * back to Alexa Connected Home Skill
             */
            log('done with result');

            var appliances = JSON.parse(str);

            //Crafting the response header
            var headers = {
                namespace: 'Alexa.ConnectedHome.Discovery',
                name: 'DiscoverAppliancesResponse',
                payloadVersion: '2'
            };

            /**
             * Craft the final response back to Alexa Connected Home Skill. This will include all the 
             * discovered appliances.
             */
            var payloads = {
                discoveredAppliances: appliances
            };
            var result = {
                header: headers,
                payload: payloads
            };

            // Warning! Logging this in production might be a security problem.
            log('Discovery', result);

            context.succeed(result);
        });

        response.on('error', function (e) {
            log('Error', e.message);
            /**
             * Craft an error response back to Alexa Connected Home Skill
             */
            context.fail(generateControlError('DiscoverAppliancesRequest', 'DEPENDENT_SERVICE_UNAVAILABLE', 'Unable to connect to server'));
        });
    };

    //Make an HTTPS call to remote endpoint.
    https.get(options, callback)
        .on('error', function (e) {
            log('Error', e.message);
            //Craft an error response back to Alexa Connected Home Skill
            context.fail(generateControlError('DiscoverAppliancesRequest', 'DEPENDENT_SERVICE_UNAVAILABLE', 'Unable to connect to server'));
        }).end();

}

/**
 * Control events are processed here.
 * This is called when Alexa requests an action (IE turn off appliance).
 */
function handleControl(event, context) {
    /**
     * Retrieve the appliance id and accessToken from the incoming message.
     */
    var applianceId = event.payload.appliance.applianceId;
    var accessToken = event.payload.accessToken;
    var messageId = event.payload.messageId;
    accessToken = accessToken.trim();
    log('applianceId', applianceId);

    var basePath = '';

    if (event.header.name === 'TurnOnRequest') {
        basePath = REMOTE_CLOUD_BASE_PATH + 'on?applianceId=' + applianceId + '&accesstoken=' + accessToken;

        var options = {
            hostname: REMOTE_CLOUD_HOSTNAME,
            port: 443,
            path: basePath,
            headers: {
                accept: 'application/json'
            }
        };

        var callback = function (response) {
            var str = '';

            response.on('data', function (chunk) {
                // TODO: Add string limit here
                str += chunk.toString('utf-8');
            });

            response.on('end', function () {
                /**
                 * Test the response from remote endpoint (not shown) and craft a response message
                 * back to Alexa Connected Home Skill
                 */
                log('done with result');
                var headers = {
                    namespace: 'Alexa.ConnectedHome.Control',
                    name: 'TurnOnConfirmation',
                    payloadVersion: '2',
                    messageId: messageId
                };
                var payloads = {
                };
                var result = {
                    header: headers,
                    payload: payloads
                };

                // Warning! Logging this with production data might be a security problem.
                log('Done with result', result);
                context.succeed(result);
            });

            response.on('error', function (e) {
                log('Error', e.message);
                /**
                 * Craft an error response back to Alexa Connected Home Skill
                 */
                context.fail(generateControlError(event.header.name, 'DEPENDENT_SERVICE_UNAVAILABLE', 'Unable to connect to server'));
            });
        };

        /**
         * Make an HTTPS call to remote endpoint.
         */
        https.get(options, callback)
            .on('error', function (e) {
                log('Error', e.message);
                /**
                 * Craft an error response back to Alexa Connected Home Skill
                 */
                context.fail(generateControlError('TurnOnRequest', 'DEPENDENT_SERVICE_UNAVAILABLE', 'Unable to connect to server'));
            }).end();
    } else if (event.header.name === 'TurnOffRequest') {
        basePath = REMOTE_CLOUD_BASE_PATH + 'off?applianceId=' + applianceId + '&accesstoken=' + accessToken;

        var options = {
            hostname: REMOTE_CLOUD_HOSTNAME,
            port: 443,
            path: basePath,
            headers: {
                accept: 'application/json'
            }
        };

        var callback = function (response) {
            var str = '';

            response.on('data', function (chunk) {
                // TODO: Add string limit here
                str += chunk.toString('utf-8');
            });

            response.on('end', function () {
                /**
                 * Test the response from remote endpoint (not shown) and craft a response message
                 * back to Alexa Connected Home Skill
                 */
                log('done with result');
                var headers = {
                    namespace: 'Alexa.ConnectedHome.Control',
                    name: 'TurnOffConfirmation',
                    payloadVersion: '2',
                    messageId: messageId
                };
                var payloads = {
                };
                var result = {
                    header: headers,
                    payload: payloads
                };

                // Warning! Logging this with production data might be a security problem.
                log('Done with result', result);
                context.succeed(result);
            });

            response.on('error', function (e) {
                log('Error', e.message);
                /**
                 * Craft an error response back to Alexa Connected Home Skill
                 */
                context.fail(generateControlError(event.header.name, 'DEPENDENT_SERVICE_UNAVAILABLE', 'Unable to connect to server'));
            });
        };

        /**
         * Make an HTTPS call to remote endpoint.
         */
        https.get(options, callback)
            .on('error', function (e) {
                log('Error', e.message);
                /**
                 * Craft an error response back to Alexa Connected Home Skill
                 */
                context.fail(generateControlError('TurnOnRequest', 'DEPENDENT_SERVICE_UNAVAILABLE', 'Unable to connect to server'));
            }).end();
    } else if (event.header.name === 'SetTargetTemperatureRequest') {
        var targetTemp = event.payload.targetTemperature.value;
        basePath = REMOTE_CLOUD_BASE_PATH + 'settemp?applianceId=' + applianceId + '&accesstoken=' + accessToken + '&temp=' + targetTemp;

        var options = {
            hostname: REMOTE_CLOUD_HOSTNAME,
            port: 443,
            path: basePath,
            headers: {
                accept: 'application/json'
            }
        };

        var callback = function (response) {
            var str = '';

            response.on('data', function (chunk) {
                // TODO: Add string limit here
                str += chunk.toString('utf-8');
            });

            response.on('end', function () {
                /**
                 * Test the response from remote endpoint (not shown) and craft a response message
                 * back to Alexa Connected Home Skill
                 */
                log('done with result');
                var headers = {
                    namespace: 'Alexa.ConnectedHome.Control',
                    name: 'SetTargetTemperatureConfirmation',
                    payloadVersion: '2',
                    messageId: messageId
                };
                var payloads = {
                    targetTemperature: {
                        "value": targetTemp
                    },
                    temperatureMode: {
                        value: "AUTO"
                    },
                    previousState: JSON.parse(str)
                };
                var result = {
                    header: headers,
                    payload: payloads
                };

                // Warning! Logging this with production data might be a security problem.
                log('Done with result', result);
                context.succeed(result);
            });

            response.on('error', function (e) {
                log('Error', e.message);
                /**
                 * Craft an error response back to Alexa Connected Home Skill
                 */
                context.fail(generateControlError(event.header.name, 'DEPENDENT_SERVICE_UNAVAILABLE', 'Unable to connect to server'));
            });
        };

        /**
         * Make an HTTPS call to remote endpoint.
         */
        https.get(options, callback)
            .on('error', function (e) {
                log('Error', e.message);
                /**
                 * Craft an error response back to Alexa Connected Home Skill
                 */
                context.fail(generateControlError('SetTargetTemperatureRequest', 'DEPENDENT_SERVICE_UNAVAILABLE', 'Unable to connect to server'));
            }).end();
    } else if (event.header.name === 'IncrementTargetTemperatureRequest') {
        var deltaTemp = event.payload.deltaTemperature.value;
        basePath = REMOTE_CLOUD_BASE_PATH + 'inctemp?applianceId=' + applianceId + '&accesstoken=' + accessToken + '&delta=' + deltaTemp;

        var options = {
            hostname: REMOTE_CLOUD_HOSTNAME,
            port: 443,
            path: basePath,
            headers: {
                accept: 'application/json'
            }
        };

        var callback = function (response) {
            var str = '';

            response.on('data', function (chunk) {
                // TODO: Add string limit here
                str += chunk.toString('utf-8');
            });

            response.on('end', function () {
                var serviceData = JSON.parse(str);

                /**
                 * Test the response from remote endpoint (not shown) and craft a response message
                 * back to Alexa Connected Home Skill
                 */
                log('done with result');
                var headers = {
                    namespace: 'Alexa.ConnectedHome.Control',
                    name: 'IncrementTargetTemperatureConfirmation',
                    payloadVersion: '2',
                    messageId: messageId
                };
                var payloads = {
                    deltaTemperature: {
                        "value": deltaTemp
                    },
                    temperatureMode: {
                        value: "AUTO"
                    },
                    previousState: serviceData.previousState,
                    targetTemperature: serviceData.targetTemperature
                };
                var result = {
                    header: headers,
                    payload: payloads
                };

                // Warning! Logging this with production data might be a security problem.
                log('Done with result', result);
                context.succeed(result);
            });

            response.on('error', function (e) {
                log('Error', e.message);
                /**
                 * Craft an error response back to Alexa Connected Home Skill
                 */
                context.fail(generateControlError(event.header.name, 'DEPENDENT_SERVICE_UNAVAILABLE', 'Unable to connect to server'));
            });
        };

        /**
         * Make an HTTPS call to remote endpoint.
         */
        https.get(options, callback)
            .on('error', function (e) {
                log('Error', e.message);
                /**
                 * Craft an error response back to Alexa Connected Home Skill
                 */
                context.fail(generateControlError('IncrementTargetTemperatureConfirmation', 'DEPENDENT_SERVICE_UNAVAILABLE', 'Unable to connect to server'));
            }).end();
    } else if (event.header.name === 'DecrementTargetTemperatureRequest') {
        var deltaTemp = event.payload.deltaTemperature.value;
        basePath = REMOTE_CLOUD_BASE_PATH + 'dectemp?applianceId=' + applianceId + '&accesstoken=' + accessToken + '&delta=' + deltaTemp;

        var options = {
            hostname: REMOTE_CLOUD_HOSTNAME,
            port: 443,
            path: basePath,
            headers: {
                accept: 'application/json'
            }
        };

        var callback = function (response) {
            var str = '';

            response.on('data', function (chunk) {
                // TODO: Add string limit here
                str += chunk.toString('utf-8');
            });

            response.on('end', function () {
                var serviceData = JSON.parse(str);
                /**
                 * Test the response from remote endpoint (not shown) and craft a response message
                 * back to Alexa Connected Home Skill
                 */
                log('done with result');
                var headers = {
                    namespace: 'Alexa.ConnectedHome.Control',
                    name: 'DecrementTargetTemperatureConfirmation',
                    payloadVersion: '2',
                    messageId: messageId
                };
                var payloads = {
                    deltaTemperature: {
                        "value": deltaTemp
                    },
                    temperatureMode: {
                        value: "AUTO"
                    },
                    previousState: serviceData.previousState,
                    targetTemperature: serviceData.targetTemperature
                };
                var result = {
                    header: headers,
                    payload: payloads
                };

                // Warning! Logging this with production data might be a security problem.
                log('Done with result', result);
                context.succeed(result);
            });

            response.on('error', function (e) {
                log('Error', e.message);
                /**
                 * Craft an error response back to Alexa Connected Home Skill
                 */
                context.fail(generateControlError(event.header.name, 'DEPENDENT_SERVICE_UNAVAILABLE', 'Unable to connect to server'));
            });
        };

        /**
         * Make an HTTPS call to remote endpoint.
         */
        https.get(options, callback)
            .on('error', function (e) {
                log('Error', e.message);
                /**
                 * Craft an error response back to Alexa Connected Home Skill
                 */
                context.fail(generateControlError('DecrementTargetTemperatureConfirmation', 'DEPENDENT_SERVICE_UNAVAILABLE', 'Unable to connect to server'));
            }).end();
    }
    else {
        context.fail(generateControlError(event.header.name, 'UNSUPPORTED_OPERATION', 'Unrecognized operation'));
    }
}

/**
 * Utility functions.
 */
function log(title, msg) {
    console.log('*************** ' + title + ' *************');
    console.log(msg);
    console.log('*************** ' + title + ' End*************');
}

function generateControlError(name, code, description) {
    var headers = {
        namespace: 'Control',
        name: name,
        payloadVersion: '1'
    };

    var payload = {
        exception: {
            code: code,
            description: description
        }
    };

    var result = {
        header: headers,
        payload: payload
    };

    return result;
}