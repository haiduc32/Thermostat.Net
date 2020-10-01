using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ThermostatAutomation.Middleware
{
    /// <summary>
    /// This is a logging middleware that streams to Debug
    /// </summary>
    public class MyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public MyMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            // don't know why this doesn't work
            _logger = loggerFactory.CreateLogger("http");
            //_logger = loggerFactory.CreateLogger<MyMiddleware>();
        }

        public async Task Invoke(HttpContext httpContext)
        {
            //_logger.LogWarning("Something fishy");
            MemoryStream ms = new MemoryStream();
            Stream outStream = httpContext.Response.Body;
            httpContext.Response.Body = ms;
            
            await _next(httpContext);

            ms.Seek(0, SeekOrigin.Begin);
            StreamReader sr = new StreamReader(ms);
            string result = sr.ReadToEnd();

            string traceMessage = (httpContext.Request.Path.HasValue ? httpContext.Request.Path.Value : "") + httpContext.Request.QueryString.Value +"\r\n";
            if ((httpContext.Request.ContentLength ?? 0) > 0 && httpContext.Request.Body.CanSeek)
            {
                httpContext.Request.Body.Seek(0, SeekOrigin.Begin);
                StreamReader bodyStream = new StreamReader(httpContext.Request.Body);
                traceMessage += bodyStream.ReadToEnd() + "\r\n";
            }
            if (string.IsNullOrEmpty(result))
            {
                traceMessage += "No response body.\r\n";
            }
            else
            {
                traceMessage += "Response: \r\n" + result + "\r\n";
            }
            _logger.LogInformation(traceMessage);
            Debug.WriteLine(result);

            ms.Seek(0, SeekOrigin.Begin);
            ms.CopyTo(outStream);
        }
    }
}
