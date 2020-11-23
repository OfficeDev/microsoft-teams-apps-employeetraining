// <copyright file="AuthenticationServiceCollectionExtensions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Authentication
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.Teams.Apps.EmployeeTraining.Helpers;
    using Microsoft.Teams.Apps.EmployeeTraining.Models.Configuration;

    /// <summary>
    /// Extension class for registering authentication services in Dependency Injection container.
    /// </summary>
    public static class AuthenticationServiceCollectionExtensions
    {
        /// <summary>
        /// Extension method to register the authentication services.
        /// </summary>
        /// <param name="services">IServiceCollection instance.</param>
        /// <param name="configuration">IConfiguration instance.</param>
        public static void RegisterAuthenticationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            // This works specifically for single tenant application.
            var azureSettings = new AzureSettings();
            configuration.Bind("AzureAd", azureSettings);
            services.AddAuthentication(options => { options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme; })
                .AddJwtBearer(options =>
                {
                    options.Authority = $"{azureSettings.Instance}/{azureSettings.TenantId}/v2.0";
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidAudiences = new List<string> { azureSettings.ClientId, azureSettings.ApplicationIdURI.ToUpperInvariant() },
                        ValidIssuers = (azureSettings.ValidIssuers
                            ?.Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                            ?.Select(p => p.Trim())).Select(validIssuer => validIssuer.Replace("TENANT_ID", azureSettings.TenantId, StringComparison.OrdinalIgnoreCase)),
                        AudienceValidator = AuthenticationServiceCollectionExtensions.AudienceValidator,
                    };
                });

            RegisterAuthorizationPolicy(services);
        }

        private static void RegisterAuthorizationPolicy(IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                var mustContainValidUserRequirement = new MustBeLnDTeamMemberRequirement();
                options.AddPolicy(
                    PolicyNames.MustBeLnDTeamMemberPolicy,
                    policyBuilder => policyBuilder.AddRequirements(mustContainValidUserRequirement));
            });

            services.AddSingleton<IAuthorizationHandler, MustBeLnDTeamMemberHandler>();
        }

        /// <summary>
        /// Check whether a audience is valid or not.
        /// </summary>
        /// <param name="tokenAudiences">A collection of audience token.</param>
        /// <param name="securityToken">A security token.</param>
        /// <param name="validationParameters">Contains a set of parameters that are used by a Microsoft.IdentityModel.Tokens.SecurityTokenHandler
        /// when validating a Microsoft.IdentityModel.Tokens.SecurityToken.</param>
        /// <returns>A boolean value represents validity of audience.</returns>
        private static bool AudienceValidator(
            IEnumerable<string> tokenAudiences,
            SecurityToken securityToken,
            TokenValidationParameters validationParameters)
        {
            if (tokenAudiences.IsNullOrEmpty())
            {
                throw new ApplicationException("No audience defined in token!");
            }

            var validAudiences = validationParameters.ValidAudiences;
            if (validAudiences.IsNullOrEmpty())
            {
                throw new ApplicationException("No valid audiences defined in validationParameters!");
            }

            return tokenAudiences.Intersect(tokenAudiences).Any();
        }
    }
}
