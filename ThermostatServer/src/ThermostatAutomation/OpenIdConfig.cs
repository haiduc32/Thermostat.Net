//https://github.com/openiddict/openiddict-core


//using AspNet.Security.OpenIdConnect.Extensions;
//using AspNet.Security.OpenIdConnect.Primitives;
//using AspNet.Security.OpenIdConnect.Server;
//using Microsoft.AspNetCore.Authentication;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Http.Authentication;
//using Microsoft.Extensions.Configuration;
//using Microsoft.IdentityModel.Tokens;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Security.Claims;
//using System.Threading.Tasks;

//namespace ThermostatAutomation
//{
//    public static class OpenIdConfig
//    {
//        public static void Setup(OpenIdConnectServerOptions options)
//        {
//            options.TokenEndpointPath = "/oauth/accesstoken";
//            options.AuthorizationEndpointPath = "/account/Login";
//            options.AllowInsecureHttp = true;
//            options.ApplicationCanDisplayErrors = true;

//            byte[] securityKey = System.Text.Encoding.UTF8.GetBytes("My very secret key that must be taken fro mthe config.");
//            options.SigningCredentials.AddKey(new SymmetricSecurityKey(securityKey));


//            // Implement OnValidateTokenRequest to support flows using the token endpoint.
//            options.Provider.OnValidateTokenRequest = context =>
//            {
//                // Reject token requests that don't use grant_type=password or grant_type=refresh_token.
//                if (!context.Request.IsAuthorizationCodeGrantType()/*.IsPasswordGrantType()*/ && !context.Request.IsRefreshTokenGrantType())
//                {
//                    context.Reject(
//                        error: OpenIdConnectConstants.Errors.UnsupportedGrantType,
//                        description: "Only grant_type=authorization_code and refresh_token " +
//                                     "requests are accepted by this server.");

//                    return Task.FromResult(0);
//                }

//                // Note: you can skip the request validation when the client_id
//                // parameter is missing to support unauthenticated token requests.
//                // if (string.IsNullOrEmpty(context.ClientId)) {
//                //     context.Skip();
//                // 
//                //     return Task.FromResult(0);
//                // }

//                // Note: to mitigate brute force attacks, you SHOULD strongly consider applying
//                // a key derivation function like PBKDF2 to slow down the secret validation process.
//                // You SHOULD also consider using a time-constant comparer to prevent timing attacks.
//                //if (string.Equals(context.ClientId, "client_id", StringComparison.Ordinal) &&
//                //    string.Equals(context.ClientSecret, "client_secret", StringComparison.Ordinal))
//                string code = context.Request.Code;

//                if (string.Equals(code, Startup.Configuration.GetSection("Account").GetValue<string>("OAuthCode"), StringComparison.Ordinal))
//                {
//                    context.Validate();
//                }

//                // Note: if Validate() is not explicitly called,
//                // the request is automatically rejected.
//                return Task.FromResult(0);
//            };

//            // Implement OnHandleTokenRequest to support token requests.
//            options.Provider.OnHandleTokenRequest = context => {
//                // Only handle grant_type=password token requests and let the
//                // OpenID Connect server middleware handle the other grant types.
//                if (context.Request.IsAuthorizationCodeGrantType())//.IsPasswordGrantType())
//                {
//                    // Implement context.Request.Username/context.Request.Password validation here.
//                    // Note: you can call context Reject() to indicate that authentication failed.
//                    // Using password derivation and time-constant comparer is STRONGLY recommended.
//                    string code = context.Request.Code;

//                    if (!string.Equals(code, Startup.Configuration.GetSection("Account").GetValue<string>("OAuthCode"), StringComparison.Ordinal))
//                    {
//                        context.Reject(
//                            error: OpenIdConnectConstants.Errors.InvalidGrant,
//                            description: "Invalid user credentials.");

//                        return Task.FromResult(0);
//                    }

//                    //if (!string.Equals(context.Request.Username, "haiduc32", StringComparison.Ordinal) ||
//                    //    !string.Equals(context.Request.Password, "password", StringComparison.Ordinal))
//                    //{
//                    //    context.Reject(
//                    //        error: OpenIdConnectConstants.Errors.InvalidGrant,
//                    //        description: "Invalid user credentials.");

//                    //    return Task.FromResult(0);
//                    //}

//                    var identity = new ClaimsIdentity(context.Options.AuthenticationScheme);
//                    identity.AddClaim(ClaimTypes.NameIdentifier, Startup.Configuration.GetSection("Account").GetValue<string>("OAuthCode"));

//                    // By default, claims are not serialized in the access/identity tokens.
//                    // Use the overload taking a "destinations" parameter to make sure
//                    // your claims are correctly inserted in the appropriate tokens.
//                    identity.AddClaim("urn:customclaim", "value",
//                        OpenIdConnectConstants.Destinations.AccessToken,
//                        OpenIdConnectConstants.Destinations.IdentityToken);

//                    var ticket = new AuthenticationTicket(
//                        new ClaimsPrincipal(identity),
//                        new AuthenticationProperties(),
//                        context.Options.AuthenticationScheme);

//                    // Call SetScopes with the list of scopes you want to grant
//                    // (specify offline_access to issue a refresh token).
//                    ticket.SetScopes(
//                        OpenIdConnectConstants.Scopes.Profile,
//                        OpenIdConnectConstants.Scopes.OfflineAccess);
                    

//                    context.Validate(ticket);
//                }

//                return Task.FromResult(0);
//            };

//        }
//    }
//}
