﻿using System;
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
using ThermostatAutomation.Middleware;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Hosting;
using NLog.Web;
using NLog.Extensions.Logging;
using ThermostatAutomation.Configuration;
using Newtonsoft.Json;
using ThermostatAutomation.Controllers;

namespace ThermostatAutomation
{
    public class Startup
    {
        // ugly, I know
        private IConfigurationRoot _configuration;
        private Engine _engine;


        public static IHostingEnvironment HostingEnvironment { get; set; }

        public Startup(IHostingEnvironment env)
        {
            HostingEnvironment = env;
            env.ConfigureNLog("nlog.config");

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
            

            _configuration = builder.Build();

            // Load the settings
            Repository r = new Repository();
            SettingsModel settingsDb = r.GetSettings();
            if (settingsDb != null) 
            {
                //Status.Instance.TargetZone = settingsDb.TargetZone;
                //Status.Instance.TargetTemperature = settingsDb.TargetTemperature;
                //Status.Instance.VacationMode = settingsDb.VacationMode;
            }
            
            var config = _configuration;

            EnabledRules rules = new EnabledRules();
            config.GetSection("EnabledRules").Bind(rules);
            Status.Instance.Rules = rules;

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
            //TODO: 

            string selectedEnginename = settingsDb?.ActiveEngine ?? rules.Default?.ClassName;
            //TODO: if null throw an error and shutdown gracefully, or fallback to some default?
            

            // Set the engine that we want to use
            //_engine.SelectedRulesEngine = engineType;
            _engine = new Engine();
            _engine.Repository = r;
            _engine.Enable(selectedEnginename);
            
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            // https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/dependency-injection
            services.Configure<EnabledRules>(_configuration.GetSection("EnabledRules"));
            services.Configure<Settings>(_configuration.GetSection("Thermostat"));
            services.Configure<AccountSettings>(_configuration.GetSection("Account"));

            // Uncomment to add settings from code
            //services.Configure<SampleWebSettings>(settings =>
            //{
            //    oauthCodes = new Dictionary
            //    settings.Updates = 17;
            //});

            //call this in case you need aspnet-user-authtype/aspnet-user-identity
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            //services.AddSingleton(_configuration);
            services.AddSingleton<OAuthCodeStore>();

            // add repository as a service
            services.AddTransient<Repository>();
            services.AddSingleton(_engine);

            // Add framework services.
            services.AddApplicationInsightsTelemetry(_configuration);

            services.AddMvc();
            //services.Configure<IISOptions>( options => options.)

            services.AddAuthorization(options =>
                {
                    options.AddPolicy("passport", policy => policy.RequireClaim("passport"));
                });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(o => 
                    {
                        o.LoginPath = new PathString("/Account/Login");
                        o.AccessDeniedPath = new PathString("/Account/Forbidden/");
                        o.Cookie.Name = "cookie";
                    });

            // add Alexa JWT token validation 
            //TOOD: create a separate nuget for that
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AlexaJWT",
                    policy => policy.RequireAssertion(context =>
                        {
                            if (context.Resource is Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext mvcContext)
                            {
                                // Examine MVC specific things like routing data.
                                var tokenData = mvcContext.HttpContext.Request.Query["accessToken"].FirstOrDefault();
                                if (tokenData == null) return false;

                                JWToken token = JsonConvert.DeserializeObject<JWToken>(tokenData);

                                //TODO: something else..
                                return token.Validate(_configuration.GetSection("Account").GetValue<string>("TokenKey"));
                            }
                            return false;
                        }
                    ));
            });

            // Add our BackgroundService which will be saving telemetry at regular intervals
            services.AddSingleton<IHostedService, BackgroundService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            //loggerFactory.AddConsole(Configuration.GetSection("/Account/Login"));
            //loggerFactory.AddDebug();

            app.UseAuthentication();

            //add NLog to ASP.NET Core
            loggerFactory.AddNLog();

            //add NLog.Web
            app.AddNLogWeb();

            // Hopefully this is the middleware that logs the errors at a global level

            
            app.UseMiddleware<MyMiddleware>();
            
            //app.UseOpenIdConnectServer(options => OpenIdConfig.Setup(options));
            

            if (env.IsDevelopment())
            {
                //app.UseExceptionHandler("/Home/Error");
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseMiddleware<ExceptionLogginMiddleware>();
            

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
