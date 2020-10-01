using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ThermostatAutomation.Middleware
{
    public sealed class ExceptionLogginMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public ExceptionLogginMiddleware(
            RequestDelegate next,
            ILoggerFactory loggerFactory)
        {
            _next = next;
            
            _logger = loggerFactory.
                    CreateLogger<ExceptionLogginMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception on the request.");
                
                throw;
            }
        }
    }
}
