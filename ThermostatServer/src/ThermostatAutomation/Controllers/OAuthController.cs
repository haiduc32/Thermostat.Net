using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThermostatAutomation.Configuration;

namespace ThermostatAutomation.Controllers
{
    public class AlexaOAuthAttribute : AuthorizeAttribute
    {
        public AlexaOAuthAttribute() : base("AlexaJWT") {  }
    }

    public class AccessTokenRequest
    {
        public string grant_type { get; set; }

        public string refresh_token { get; set; }

        public string code { get; set; }

        public string redirect_uri { get; set; }

        public string scope { get; set; }
    }

    public class JWToken
    {
        public string Timestamp { get; set; }
        public string Application { get; set; }
        public int ExpiresIn { get; set; }
        public string Signature { get; set; }

        public void GenerateSignature(string key)
        {
            Signature = CalculateSignature(key);
        }

        private string CalculateSignature(string key)
        {
            string content = Timestamp + Application + ExpiresIn.ToString();
            byte[] contentBytes = Encoding.ASCII.GetBytes(content);

            byte[] keyBytes = Encoding.ASCII.GetBytes(key);
            System.Security.Cryptography.HMACSHA256 hmac = new System.Security.Cryptography.HMACSHA256(keyBytes);
            byte[] signatureBytes = hmac.ComputeHash(contentBytes);
            return Convert.ToBase64String(signatureBytes);
        }

        /// <summary>
        /// Validates the Signature of a JWT
        /// </summary>
        /// <param name="key">The application token signing key</param>
        public bool Validate(string key)
        {
            string calculatedSignature = CalculateSignature(key);
            return calculatedSignature == Signature;
        }
    }

    public class OAuthCodeStore
    {
        /// <summary>
        /// Key is the code, value is the application
        /// </summary>
        private Dictionary<string, string> _codes;

        public OAuthCodeStore()
        {
            _codes = new Dictionary<string, string>();
        }

        public string VerifyCode(string code)
        {
            string result;
            _codes.TryGetValue(code, out result);
            return result;
        }

        public void ClearCode(string code)
        {
            _codes.Remove(code);
        }

        public string GenerateCode(string application)
        {
            string code = Guid.NewGuid().ToString();
            _codes.Add(code, application);
            return code;
        }
    }

    /// <summary>
    /// This is a totally unsecure OAuth implementation.
    /// </summary>
    public class OAuthController : Controller
    {
        private AccountSettings _accountSettings;
        private OAuthCodeStore _codeStore;
        private Repository _repository;

        public OAuthController(IOptions<AccountSettings> accountSettings, OAuthCodeStore codeStore, Repository repository)
        {
            _accountSettings = accountSettings.Value;
            _codeStore = codeStore;
            _repository = repository;
        }

        [HttpPost]
        public async Task<IActionResult> AccessToken(AccessTokenRequest tokenRequest)
        {
            if (!string.IsNullOrEmpty(tokenRequest.refresh_token))
            {
                var refreshTokenModel = await _repository.GetRefreshToken(tokenRequest.refresh_token);
                if (refreshTokenModel == null)
                {
                    return BadRequest();
                }

                string tokenKey = _accountSettings.TokenKey;
                JWToken token = GenerateAccessToken(tokenKey);
                string serializedToken = JsonConvert.SerializeObject(token);

                var response = new
                {
                    access_token = serializedToken,
                    token_type = "example",
                    expires_in = 3600,
                    refresh_token = refreshTokenModel.Token
                };

                return Ok(response);
            }
            //TODO: code should be generated in memory and disposed of after one use
            else if (!string.IsNullOrEmpty(tokenRequest.code))
            {
                // validating that the code is a valid one
                if (_codeStore.VerifyCode(tokenRequest.code) == null)
                {
                    //TOOD: check the expected answer
                    return BadRequest();
                }

                //we need to make sure nobody else can re-use the same code twice
                _codeStore.ClearCode(tokenRequest.code);

                string tokenKey = _accountSettings.TokenKey;
                JWToken token = GenerateAccessToken(tokenKey);
                string serializedToken = JsonConvert.SerializeObject(token);
                string refreshToken = Guid.NewGuid().ToString();
                await _repository.AddRefreshToken(new Models.RefreshTokenModel { Application = "Alexa", Token = refreshToken });

                var response = new
                {
                    access_token = serializedToken,
                    token_type = "example",
                    expires_in = 3600,
                    refresh_token = refreshToken
                };

                return Ok(response);
            }

            return BadRequest();
        }

        private JWToken GenerateAccessToken(string key)
        {
            var token = new JWToken
            {
                Timestamp = DateTimeOffset.Now.ToString(),
                ExpiresIn = 3600,
                Application = "Alexa"
            };

            token.GenerateSignature(key);
            return token;
        }
    }
}
