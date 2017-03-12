using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ThermostatAutomation.Controllers
{
    public class AccessTokenRequest
    {
        public string grant_type { get; set; }

        public string refresh_token { get; set; }

        public string code { get; set; }

        public string redirect_uri { get; set; }

        public string scope { get; set; }
    }

    /// <summary>
    /// This is a totally unsecure OAuth implementation.
    /// </summary>
    public class OAuthController : Controller
    {
        [HttpPost]
        public IActionResult AccessToken(AccessTokenRequest tokenRequest)
        {
            if (tokenRequest.refresh_token == "rtoken")
            {
                var response = new
                {
                    access_token = "atoken",
                    token_type = "example",
                    expires_in = 3600,
                    refresh_token = "rtoken"
                };

                return Ok(response);
            }
            else if (tokenRequest.code == "1")
            {
                var response = new
                {
                    access_token = "atoken",
                    token_type = "example",
                    expires_in = 3600,
                    refresh_token = "rtoken"
                };

                return Ok(response);
            }

            return BadRequest();
        }
    }
}
