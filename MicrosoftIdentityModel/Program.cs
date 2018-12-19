using System;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security.Jwt;

namespace MicrosoftIdentityModel
{
    class Program
    {
        static void Main(string[] args)
        {
            var issuer = "https://id.somnething.com/sts";

            var validationParameters = new TokenValidationParameters
            {
                ValidAudience = issuer + "/resources",
                NameClaimType = "sub"
            };


            var tokenFormat = new JwtFormat(validationParameters,
                new OpenIdIssuerSecurityTokenProvider(issuer));

            var token = "token_Value";

            tokenFormat.Unprotect(token);

            Console.ReadLine();
        }
    }
}
