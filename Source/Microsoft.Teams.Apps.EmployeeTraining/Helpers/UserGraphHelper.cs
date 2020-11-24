// <copyright file="UserGraphHelper.cs" company="Microsoft">
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
    /// Provides helper methods to make Microsoft Graph API calls related to users
    /// </summary>
    public class UserGraphHelper : IUserGraphHelper
    {
        /// <summary>
        /// MS Graph batch limit is 20
        /// https://docs.microsoft.com/en-us/graph/known-issues#json-batching.
        /// </summary>
        private const int BatchSplitCount = 20;

        /// <summary>
        /// Maximum result count for search user and group request.
        /// </summary>
        private const int MaxResultCountForUserOrGroupSearch = 10;

        /// <summary>
        /// Maximum result count for recent collaborators people request.
        /// </summary>
        private const int MaxResultCountForRecentCollaborators = 1000;

        /// <summary>
        /// The filter condition to get recent collaborators for sorting events those are popular in logged-in user's network
        /// </summary>
        private const string RecentCollaboratorsFilterForPopularInMyNetwork = "personType/class eq 'Person' and personType/subclass eq 'OrganizationUser'";

        /// <summary>
        /// Instance of graph service client for delegated requests.
        /// </summary>
        private readonly GraphServiceClient delegatedGraphClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserGraphHelper"/> class.
        /// </summary>
        /// <param name="tokenAcquisitionHelper">Helper to get user access token for specified Graph scopes.</param>
        /// <param name="httpContextAccessor">HTTP context accessor for getting user claims.</param>
        public UserGraphHelper(ITokenAcquisitionHelper tokenAcquisitionHelper, IHttpContextAccessor httpContextAccessor)
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
        /// Get recent collaborators used to sort events by 'Popular in my network'
        /// </summary>
        /// <returns>List of recent collaborators</returns>
        public async Task<IEnumerable<Person>> GetRecentCollaboratorsForPopularInMyNetworkAsync()
        {
            var recentCollaboratorsResult = await this.delegatedGraphClient.Me.People.Request()
                .Filter(RecentCollaboratorsFilterForPopularInMyNetwork)
                .Top(MaxResultCountForRecentCollaborators)
                .Select("id, scoredEmailAddresses")
                .GetAsync();

            return recentCollaboratorsResult.CurrentPage;
        }

        /// <summary>
        /// Get user display name.
        /// </summary>
        /// <param name="userObjectId">AAD Object id of user.</param>
        /// <returns>A task that returns user information.</returns>
        public async Task<User> GetUserAsync(string userObjectId)
        {
            return await this.delegatedGraphClient.Users[userObjectId].Request().GetAsync();
        }

        /// <summary>
        /// Get users information from graph API.
        /// </summary>
        /// <param name="userObjectIds">Collection of AAD Object ids of users.</param>
        /// <returns>A task that returns collection of user information.</returns>
        public async Task<IEnumerable<User>> GetUsersAsync(IEnumerable<string> userObjectIds)
        {
            userObjectIds = userObjectIds ?? throw new ArgumentNullException(nameof(userObjectIds));
            var userDetails = new List<User>();
            var userObjectIdsBatch = userObjectIds.ToList().SplitList(BatchSplitCount);

            BatchRequestContent batchRequestContent;
            foreach (var userObjectIdBatch in userObjectIdsBatch)
            {
                var batchIds = new List<string>();
                var userDetailsBatch = new List<User>();
                batchRequestContent = new BatchRequestContent();

                foreach (string userObjectId in userObjectIdBatch)
                {
                    var request = this.delegatedGraphClient
                        .Users[userObjectId]
                        .Request();

                    batchIds.Add(batchRequestContent.AddBatchRequestStep(request));
                }

                var response = await this.delegatedGraphClient.Batch.Request().PostAsync(batchRequestContent);
                for (int i = 0; i < batchIds.Count; i++)
                {
                    userDetailsBatch.Add(await response.GetResponseByIdAsync<User>(batchIds[i]));
                }

                userDetails.AddRange(userDetailsBatch);
                batchRequestContent.Dispose();
            }

            return userDetails;
        }

        /// <summary>
        /// Get top 10 users according to user search query.
        /// </summary>
        /// <param name="searchText">Search query entered by user.</param>
        /// <returns>List of users.</returns>
        public async Task<List<User>> SearchUsersAsync(string searchText)
        {
            searchText = searchText ?? throw new ArgumentNullException(nameof(searchText), "search text cannot be null");

            IGraphServiceUsersCollectionPage searchedUsers;
            if (searchText.Length > 0)
            {
                searchedUsers = await this.delegatedGraphClient.Users.Request()
                    .Top(MaxResultCountForUserOrGroupSearch)
                    .Filter($"startsWith(displayName,'{searchText}') or startsWith(mail,'{searchText}')")
                    .Select("id,displayName,userPrincipalName,mail")
                    .GetAsync();
            }
            else
            {
                searchedUsers = await this.delegatedGraphClient.Users.Request()
                    .Top(MaxResultCountForUserOrGroupSearch)
                    .Select("id,displayName,userPrincipalName,mail")
                    .GetAsync();
            }

            return searchedUsers.ToList();
        }
    }
}
