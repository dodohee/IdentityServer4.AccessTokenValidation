﻿using IdentityServer4.AccessTokenValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication.JwtBearer;
using IdentityModel.AspNet.OAuth2Introspection;
using IdentityModel.AspNet.ScopeValidation;
using Microsoft.AspNet.Http;
using System.Security.Claims;

namespace Microsoft.AspNet.Builder
{
    public static class IdentityServerAuthenticationExtensions
    {
        static Func<HttpRequest, string> _tokenRetriever = request => request.HttpContext.Items["idsrv4:tokenvalidation:token"] as string;

        public static IApplicationBuilder UseIdentityServerAuthentication(this IApplicationBuilder app, IdentityServerAuthenticationOptions options)
        {
            var combinedOptions = new CombinedAuthenticationOptions();
            combinedOptions.TokenRetriever = options.TokenRetriever;
            
            switch (options.SupportedTokens)
            {
                case SupportedTokens.Jwt:
                    combinedOptions.JwtBearerOptions = ConfigureJwt(options);
                    break;
                case SupportedTokens.Reference:
                    combinedOptions.IntrospectionOptions = ConfigureIntrospection(options);
                    break;
                case SupportedTokens.Both:
                    combinedOptions.JwtBearerOptions = ConfigureJwt(options);
                    combinedOptions.IntrospectionOptions = ConfigureIntrospection(options);
                    break;
                default:
                    throw new Exception("SupportedTokens has invalid value");
            }

            app.UseMiddleware<IdentityServerAuthenticationMiddleware>(app, combinedOptions);

            if (!string.IsNullOrWhiteSpace(options.ScopeName) || options.AdditionalScopes.Any())
            {
                var allowedScopes = new List<string>(options.AdditionalScopes);

                if (!string.IsNullOrWhiteSpace(options.ScopeName))
                {
                    allowedScopes.Add(options.ScopeName);
                }

                app.AllowScopes(new ScopeValidationOptions { AllowedScopes = allowedScopes });
            }

            return app;
        }

        private static OAuth2IntrospectionOptions ConfigureIntrospection(IdentityServerAuthenticationOptions options)
        {
            var introspectionOptions = new OAuth2IntrospectionOptions
            {
                AuthenticationScheme = options.AuthenticationScheme,
                Authority = options.Authority,
                ScopeName = options.ScopeName,
                ScopeSecret = options.ScopeSecret,

                AutomaticAuthenticate = options.AutomaticAuthenticate,
                AutomaticChallenge = options.AutomaticChallenge,

                NameClaimType = options.NameClaimType,
                RoleClaimType = options.RoleClaimType,

                TokenRetriever = _tokenRetriever,
                SaveTokenAsClaim = options.SaveTokenAsClaim
            };

            return introspectionOptions;
        }

        private static JwtBearerOptions ConfigureJwt(IdentityServerAuthenticationOptions options)
        {
            var jwtOptions = new JwtBearerOptions
            {
                AuthenticationScheme = options.AuthenticationScheme,
                Authority = options.Authority,
                RequireHttpsMetadata = false,


                AutomaticAuthenticate = options.AutomaticAuthenticate,
                AutomaticChallenge = options.AutomaticChallenge,

                RefreshOnIssuerKeyNotFound = true,

                Events = new JwtBearerEvents
                {
                    OnReceivingToken = e =>
                    {
                        e.Token = _tokenRetriever(e.Request);

                        return Task.FromResult(0);
                    },
                    OnValidatedToken = e =>
                    {
                        e.AuthenticationTicket.Principal.Identities.First().AddClaim(
                            new Claim("token", _tokenRetriever(e.Request)));

                        return Task.FromResult(0);
                    }
                }
            };

            jwtOptions.TokenValidationParameters.ValidateAudience = false;
            jwtOptions.TokenValidationParameters.NameClaimType = options.NameClaimType;
            jwtOptions.TokenValidationParameters.RoleClaimType = options.RoleClaimType;
            
            return jwtOptions;
        }
    }
}