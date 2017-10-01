var https = require('https');
var REMOTE_CLOUD_BASE_PATH = "/thermostat/alexav3/handle";
var REMOTE_CLOUD_HOSTNAME = "haiduc32.go.ro";

exports.handler = function (request, context) {
    // we are just going to send the request down to the remoste cloud to be handled there
    // this function becomes just a proxy
    log("DEBUG", "Received request: ", JSON.stringify(request));
    var accessToken = request.directive.header.namespace === 'Alexa.Discovery' ?
        request.directive.payload.scope.token :
        request.directive.endpoint.scope.token;
    log("DEBUG", "Token is: ", accessToken);

    // Build the post string from an object
    var post_data = JSON.stringify(request.directive);

    // An object of options to indicate where to post to
    var post_options = {
        host: REMOTE_CLOUD_HOSTNAME,
        port: '443',
        path: REMOTE_CLOUD_BASE_PATH + '?accesstoken=' + encodeURIComponent(accessToken),
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Content-Length': post_data.length,
            'Accept': 'application/json'
        }
    };

    var post_request = https.request(post_options, function (res) {
        var body = '';

        res.on('data', function (chunk) {
            body += chunk;
        });

        res.on('end', function () {
            //context.done(body);
            log("DEBUG", "Received status code: ", JSON.stringify(res.statusCode));

            log("DEBUG", "Remote Cloud Response: ", body);

            context.succeed(JSON.parse(body));
        });

        res.on('error', function (e) {
            context.fail('error:' + e.message);
        });
    });

    // post the data
    post_request.write(post_data);
    post_request.end();
    
    function log(message, message1, message2) {
        console.log(message + message1 + message2);
    }
};