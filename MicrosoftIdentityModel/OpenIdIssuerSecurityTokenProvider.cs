using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin.Security.Jwt;
using SecurityKey = Microsoft.IdentityModel.Tokens.SecurityKey;

namespace MicrosoftIdentityModel
{
    public class OpenIdIssuerSecurityTokenProvider : IIssuerSecurityKeyProvider
    {
        private readonly ConfigurationManager<OpenIdConnectConfiguration> _configurationManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Object" /> class.
        /// </summary>
        public OpenIdIssuerSecurityTokenProvider(string issuer)
        {
            Issuer = issuer;

            _configurationManager =
                new ConfigurationManager<OpenIdConnectConfiguration>(issuer + "/XXXXXconfigurationXXXX",
                    new HttpClient(new RetryingHandler(new HttpClientHandler(), TimeSpan.FromSeconds(3)))
                    {
                        Timeout = TimeSpan.FromSeconds(10)
                    });

            var a = _configurationManager.GetConfigurationAsync().Result;
        }

        public string Issuer { get; }

        /// <summary>
        ///     Gets the issuer the credentials are for.
        /// </summary>
        /// <value>
        ///     The issuer the credentials are for.
        /// </value>
        public IEnumerable<SecurityKey> SecurityKeys => from key in _configurationManager
                .GetConfigurationAsync()
                .Result
                .JsonWebKeySet
                .Keys
            select new MySecurityKey(key.Kid);

        ///// <summary>
        /////     Gets all known security tokens.
        ///// </summary>
        ///// <value>
        /////     All known security tokens.
        ///// </value>
        //public IEnumerable<X509SecurityToken> SecurityTokens => from key in _configurationManager
        //        .GetConfigurationAsync()
        //        .Result
        //        .JsonWebKeySet
        //        .Keys
        //    select new X509SecurityToken(new X509Certificate2(Convert.FromBase64String(key.X5c.First())), key.Kid);

    }
}
