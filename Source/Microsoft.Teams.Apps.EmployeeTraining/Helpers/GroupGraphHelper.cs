// <copyright file="GroupGraphHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Graph;

    /// <summary>
    /// Provides helper methods to make Microsoft Graph API calls related to groups
    /// </summary>
    public class GroupGraphHelper : IGroupGraphHelper
    {
        /// <summary>
        /// Maximum result count for search user and group request.
        /// </summary>
        private const int MaxResultCountForUserOrGroupSearch = 10;

        /// <summary>
        /// Instance of graph service client for delegated requests.
        /// </summary>
        private readonly GraphServiceClient delegatedGraphClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupGraphHelper"/> class.
        /// </summary>
        /// <param name="tokenAcquisitionHelper">Helper to get user access token for specified Graph scopes.</param>
        /// <param name="httpContextAccessor">HTTP context accessor for getting user claims.</param>
        public GroupGraphHelper(ITokenAcquisitionHelper tokenAcquisitionHelper, IHttpContextAccessor httpContextAccessor)
        {
            httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

            var oidClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";
            var userObjectId = httpContextAccessor.HttpContext.User.Claims?
                .FirstOrDefault(claim => oidClaimType.Equals(claim.Type, StringComparison.OrdinalIgnoreCase))?.Value;

            if (!string.IsNullOrEmpty(userObjectId))
            {
                var jwtToken = AuthenticationHeaderValue.Parse(httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString()).Parameter;

                this.delegatedGraphClient = GraphServiceClientFactory.GetAuthenticatedGraphClient(async () =>
                {
                    return await tokenAcquisitionHelper.GetUserAccessTokenAsync(userObjectId, jwtToken);
                });
            }
        }

        /// <summary>
        /// Get group members for a group.
        /// </summary>
        /// <param name="groupId">AAD Object id of group.</param>
        /// <returns>A task that returns collection of group members.</returns>
        public async Task<IEnumerable<DirectoryObject>> GetGroupMembersAsync(string groupId)
        {
            groupId = groupId ?? throw new ArgumentNullException(nameof(groupId));
            var result = await this.delegatedGraphClient
                .Groups[groupId].TransitiveMembers
                .Request().GetAsync();

            var groupMembers = new List<DirectoryObject>();
            while (result?.Count > 0)
            {
                groupMembers.AddRange(result);
                if (result.NextPageRequest != null)
                {
                    result = await result.NextPageRequest.GetAsync();
                }
                else
                {
                    break;
                }
            }

            groupMembers = groupMembers.Where(user => user.ODataType == "#microsoft.graph.user").ToList();

            return groupMembers;
        }

        /// <summary>
        /// Get top 10 groups according to user search query.
        /// </summary>
        /// <param name="searchText">Search query entered by user.</param>
        /// <returns>List of users.</returns>
        public async Task<List<Group>> SearchGroupsAsync(string searchText)
        {
            searchText = searchText ?? throw new ArgumentNullException(nameof(searchText), "search text cannot be null");
            IGraphServiceGroupsCollectionPage groups;

            if (searchText.Length > 0)
            {
                groups = await this.delegatedGraphClient.Groups.Request()
                    .Top(MaxResultCountForUserOrGroupSearch)
                    .Filter($"startsWith(displayName,'{searchText}') or startsWith(mail,'{searchText}')")
                    .Select("id,displayName,userPrincipalName,mail")
                    .GetAsync();
            }
            else
            {
                groups = await this.delegatedGraphClient.Groups.Request()
                    .Top(MaxResultCountForUserOrGroupSearch)
                    .Select("id,displayName,userPrincipalName,mail")
                    .GetAsync();
            }

            return groups.ToList();
        }
    }
}
