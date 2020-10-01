using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace ThermostatAutomation
{
    public class BackgroundService : HostedService
    {
        private ILogger<BackgroundService> _logger;

        public BackgroundService(ILogger<BackgroundService> logger, ILoggerFactory loggerFactory)
        {
            _logger = logger;
            loggerFactory.CreateLogger<BackgroundService>().LogInformation(".ctor");
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Started BackgroundService on Environment: {Startup.HostingEnvironment.EnvironmentName}");

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(60), cancellationToken);

                try
                {
                    await Status.Instance.SaveTelemetryAsync();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error when trying to save telemetry.");
                }
            }
        }
    }
}
