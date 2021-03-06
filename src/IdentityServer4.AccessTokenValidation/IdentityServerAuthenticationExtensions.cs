﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Builder
{
    public static class IdentityServerAuthenticationExtensions
    {
        public static AuthenticationBuilder AddIdentityServerAuthentication(this AuthenticationBuilder builder)
            => builder.AddIdentityServerAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme);

        public static AuthenticationBuilder AddIdentityServerAuthentication(this AuthenticationBuilder builder, string authenticationScheme)
            => builder.AddIdentityServerAuthentication(authenticationScheme, configureOptions: null);

        public static AuthenticationBuilder AddIdentityServerAuthentication(this AuthenticationBuilder builder, Action<IdentityServerAuthenticationOptions> configureOptions) =>
            builder.AddIdentityServerAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddIdentityServerAuthentication(this AuthenticationBuilder builder, string authenticationScheme, Action<IdentityServerAuthenticationOptions> configureOptions)
        {
            builder.AddJwtBearer(IdentityServerAuthenticationDefaults.JwtAuthenticationScheme, configureOptions: null);
            builder.AddOAuth2Introspection(IdentityServerAuthenticationDefaults.IntrospectionAuthenticationScheme, null, configureOptions: null);

            builder.Services.AddSingleton<IConfigureOptions<JwtBearerOptions>, ConfigureInternalOptions>();
            builder.Services.AddSingleton<IConfigureOptions<OAuth2IntrospectionOptions>, ConfigureInternalOptions>();

            IdentityServerAuthenticationOptions.EffectiveScheme = authenticationScheme;
            return builder.AddScheme<IdentityServerAuthenticationOptions, IdentityServerAuthenticationHandler>(authenticationScheme, configureOptions);
        }

        public static AuthenticationBuilder AddIdentityServerAuthentication(this AuthenticationBuilder builder, string authenticationScheme, 
            Action<IdentityServerAuthenticationOptions> configureOptions,
            Action<JwtBearerOptions> jwtBearerOptions,
            Action<OAuth2IntrospectionOptions> introspectionOptions)
        {
            if (jwtBearerOptions != null)
            {
                builder.AddJwtBearer(IdentityServerAuthenticationDefaults.JwtAuthenticationScheme, jwtBearerOptions);
            }

            if (introspectionOptions != null)
            {
                builder.AddOAuth2Introspection(IdentityServerAuthenticationDefaults.IntrospectionAuthenticationScheme, null, introspectionOptions);
            }

            IdentityServerAuthenticationOptions.EffectiveScheme = authenticationScheme;
            return builder.AddScheme<IdentityServerAuthenticationOptions, IdentityServerAuthenticationHandler>(authenticationScheme, configureOptions);
        }
    }
}