using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ThermostatAutomation.Middleware
{
    public class MyMiddleware
    {
        private readonly RequestDelegate _next;

        public MyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {

            MemoryStream ms = new MemoryStream();
            Stream outStream = httpContext.Response.Body;
            httpContext.Response.Body = ms;


            await _next(httpContext);

            ms.Seek(0, SeekOrigin.Begin);
            StreamReader sr = new StreamReader(ms);
            string result = sr.ReadToEnd();

            Debug.WriteLine(result);

            ms.Seek(0, SeekOrigin.Begin);
            ms.CopyTo(outStream);
        }
    }
}
