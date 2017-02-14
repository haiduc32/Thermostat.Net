using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ThermostatAutomation.Models;

namespace ThermostatAutomation.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null, string state = null, string client_id = null, string response_type = null, string scope = null, string redirect_uri = null)
        {
            
            ViewData["state"] = state;
            ViewData["client_id"] = client_id;
            ViewData["response_type"] = response_type;
            ViewData["scope"] = scope;
            ViewData["redirect_uri"] = redirect_uri;

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model, [FromQuery]string returnUrl = null, [FromQuery]string state = null, [FromQuery]string client_id = null, [FromQuery]string response_type = null, [FromQuery]string scope = null, [FromQuery]string redirect_uri = null)
        {
            ViewData["state"] = state;
            ViewData["client_id"] = client_id;
            ViewData["response_type"] = response_type;
            ViewData["scope"] = scope;
            ViewData["redirect_uri"] = redirect_uri;

            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                string user = Startup.Configuration.GetSection("Account").GetValue<string>("User");
                string password = Startup.Configuration.GetSection("Account").GetValue<string>("Password");
                if (model.User != user || model.Password != password)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }

                const string Issuer = "https://mydomain.com";

                var claims = new List<Claim> {
                    new Claim(ClaimTypes.Name, "admin", ClaimValueTypes.String, Issuer)
                };

                var userIdentity = new ClaimsIdentity(claims, "Passport");

                var userPrincipal = new ClaimsPrincipal(userIdentity);

                //should be some kind of identifier.. we'll just hard code it to 1 and call it a day.
                string code = Startup.Configuration.GetSection("Account").GetValue<string>("OAuthCode");

                await HttpContext.Authentication.SignInAsync("Cookie", userPrincipal,
                    new AuthenticationProperties
                    {
                        ExpiresUtc = DateTime.UtcNow.AddMinutes(20),
                        IsPersistent = false,
                        AllowRefresh = false
                    });

                if (!string.IsNullOrEmpty(returnUrl))
                {
                    return RedirectToLocal(returnUrl);
                }

                if (!string.IsNullOrEmpty(redirect_uri))
                {
                    
                    return Redirect(redirect_uri+(redirect_uri.IndexOf('?') >= 0 ? "&" : "?") +"state=" + state + "&code=" + code);
                }

                //redirect to home page
                return RedirectToLocal();
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        private IActionResult RedirectToLocal(string returnUrl = null)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
    }
}
