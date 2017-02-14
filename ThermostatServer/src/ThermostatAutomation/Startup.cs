using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ThermostatAutomation.Rules;
using ThermostatAutomation.Models;
using Microsoft.AspNetCore.Http;

namespace ThermostatAutomation
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                builder.AddApplicationInsightsSettings(developerMode: true);
            }
            Configuration = builder.Build();

            // Load the settings
            Repository r = new Repository();
            SettingsModel settingsDb = r.GetSettings();
            if (settingsDb != null) 
            {
                Status.Instance.TargetZone = settingsDb.TargetZone;
                Status.Instance.TargetTemperature = settingsDb.TargetTemperature;
                Status.Instance.VacationMode = settingsDb.VacationMode;
            }
            
            var config = builder.Build();
            Settings settings = new Settings();
            config.GetSection("Thermostat").Bind(settings);
            Status.Instance.Settings = settings;

            foreach (ChannelSettings channel in settings.Channels)
            {
                Status.Instance.Channels.Add(false);

                foreach (string zone in channel.Zones)
                {
                    Status.Instance.Zones.Add(new Zone { Name = zone });
                }
            }

            // Set the engine that we want to use
            Engine.SelectedRulesEngine = typeof(RadusRules);
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddApplicationInsightsTelemetry(Configuration);

            services.AddMvc();
            //services.Configure<IISOptions>( options => options.)
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseOpenIdConnectServer(options => OpenIdConfig.Setup(options));

            app.UseApplicationInsightsRequestTelemetry();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseApplicationInsightsExceptionTelemetry();

            app.UseStaticFiles();

            app.UseCookieAuthentication(new CookieAuthenticationOptions()
            {
                AuthenticationScheme = "Cookie",
                LoginPath = new PathString("/Account/Login"),
                AccessDeniedPath = new PathString("/Account/Forbidden/"),
                AutomaticAuthenticate = true,
                AutomaticChallenge = true
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
