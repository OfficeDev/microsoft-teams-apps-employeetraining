// <copyright file="MustBeLnDTeamMemberHandler.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Authentication
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.EmployeeTraining.Helpers;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;

    /// <summary>
    /// This authorization handler is created to handle project creator's user policy.
    /// The class implements AuthorizationHandler for handling MustBeLnDTeamMemberUserPolicyRequirement authorization.
    /// </summary>
    public class MustBeLnDTeamMemberHandler : AuthorizationHandler<MustBeLnDTeamMemberRequirement>
    {
        /// <summary>
        /// Cache for storing authorization result.
        /// </summary>
        private readonly IMemoryCache memoryCache;

        /// <summary>
        /// A set of key/value application configuration properties for Activity settings.
        /// </summary>
        private readonly IOptions<BotSettings> botOptions;

        /// <summary>
        /// Provides method to fetch bot installation details for team.
        /// </summary>
        private readonly ITeamInfoHelper teamsInfoHelper;

        /// <summary>
        /// HTTP context accessor to get HTTP context object.
        /// </summary>
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="MustBeLnDTeamMemberHandler"/> class.
        /// </summary>
        /// <param name="memoryCache">Memory cache instance for caching authorization result.</param>
        /// <param name="botOptions">A set of key/value application configuration properties for activity handler.</param>
        /// <param name="teamsInfoHelper">Provides method to fetch bot installation details for team.</param>
        /// <param name="httpContextAccessor">HTTP context accessor to get HTTP context object.</param>
        public MustBeLnDTeamMemberHandler(
           IMemoryCache memoryCache,
           IOptions<BotSettings> botOptions,
           ITeamInfoHelper teamsInfoHelper,
           IHttpContextAccessor httpContextAccessor)
        {
            this.memoryCache = memoryCache;
            this.httpContextAccessor = httpContextAccessor;
            this.botOptions = botOptions ?? throw new ArgumentNullException(nameof(botOptions));
            this.teamsInfoHelper = teamsInfoHelper;
        }

        /// <summary>
        /// This method handles the authorization requirement.
        /// </summary>
        /// <param name="context">AuthorizationHandlerContext instance.</param>
        /// <param name="requirement">IAuthorizationRequirement instance.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            MustBeLnDTeamMemberRequirement requirement)
        {
            context = context ?? throw new ArgumentNullException(nameof(context));

            string teamId = string.Empty;
            var oidClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";

            var claim = context.User.Claims.FirstOrDefault(p => oidClaimType.Equals(p.Type, StringComparison.OrdinalIgnoreCase));

            var httpContext = this.httpContextAccessor.HttpContext;

            // Wrap the request stream so that we can rewind it back to the start for regular request processing.
            httpContext.Request.EnableBuffering();

            if (!string.IsNullOrEmpty(httpContext.Request.QueryString.Value))
            {
                // Check for query string parameter 'teamId'
                var requestQuery = httpContext.Request.Query;
                teamId = requestQuery.Where(queryData => queryData.Key == "teamId")
                    .Select(queryData => queryData.Value.ToString()).FirstOrDefault();
            }
            else
            {
                context.Fail();
            }

            if (await this.ValidateUserIsPartOfTeamAsync(teamId, claim.Value))
            {
                context.Succeed(requirement);
            }
        }

        /// <summary>
        /// Check if a user is a member of a certain team.
        /// </summary>
        /// <param name="teamId">The team id that the validator uses to check if the user is a member of the team. </param>
        /// <param name="userAadObjectId">The user's Azure Active Directory object id.</param>
        /// <returns>The flag indicates that the user is a part of certain team or not.</returns>
        private async Task<bool> ValidateUserIsPartOfTeamAsync(string teamId, string userAadObjectId)
        {
            // The key is generated by combining teamId and user object id.
            bool isCacheEntryExists = this.memoryCache.TryGetValue(this.GetCacheKey(teamId, userAadObjectId), out bool isUserValidMember);

            if (!isCacheEntryExists)
            {
                // If cache duration is not specified then by default cache for 60 minutes
                var cacheDurationInMinutes = TimeSpan.FromMinutes(this.botOptions.Value.CacheDurationInMinutes);
                cacheDurationInMinutes = cacheDurationInMinutes.Minutes <= 0 ? TimeSpan.FromMinutes(60) : cacheDurationInMinutes;

                var teamMember = await this.teamsInfoHelper.GetTeamMemberAsync(teamId, userAadObjectId);
                isUserValidMember = teamMember != null;
                this.memoryCache.Set(this.GetCacheKey(teamId, userAadObjectId), isUserValidMember, cacheDurationInMinutes);
            }

            return isUserValidMember;
        }

        /// <summary>
        /// // Generate key by combining teamId and user object id.
        /// </summary>
        /// <param name="teamId">The team id that the validator uses to check if the user is a member of the team. </param>
        /// <param name="userAadObjectId">The user's Azure Active Directory object id.</param>
        /// <returns>Generated key.</returns>
        private string GetCacheKey(string teamId, string userAadObjectId)
        {
            return "_tm$" + teamId + "$" + userAadObjectId;
        }
    }
}
