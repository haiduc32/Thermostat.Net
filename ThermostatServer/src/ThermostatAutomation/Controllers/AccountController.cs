using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ThermostatAutomation.Configuration;
using ThermostatAutomation.Models;

namespace ThermostatAutomation.Controllers
{
    public class AccountController : Controller
    {
        private AccountSettings _accountSettings;
        private OAuthCodeStore _codeStore;

        public AccountController(IOptions<AccountSettings> accountSettings, OAuthCodeStore codeStore)
        {
            _accountSettings = accountSettings.Value;
            _codeStore = codeStore;
        }

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
                string user = _accountSettings.User;
                string password = _accountSettings.Password;
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

                // we hardcoded alexa but maybe it is not the best idea..
                string code = _codeStore.GenerateCode("Alexa");

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userPrincipal,
                    new AuthenticationProperties
                    {
                        ExpiresUtc = DateTime.UtcNow.AddMinutes(20),
                        IsPersistent = true,
                        AllowRefresh = true
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
