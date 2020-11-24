// <copyright file="UsersController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.EmployeeTraining.Helpers;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;

    /// <summary>
    /// Exposes APIs related to event operations.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : BaseController
    {
        /// <summary>
        /// Logs errors and information
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Graph helper for users API.
        /// </summary>
        private IUserGraphHelper userGraphHelper;

        /// <summary>
        /// Graph API helper for fetching group related data.
        /// </summary>
        private IGroupGraphHelper groupGraphHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsersController"/> class.
        /// </summary>
        /// <param name="logger">The ILogger object which logs errors and information</param>
        /// <param name="telemetryClient">The Application Insights telemetry client</param>
        /// <param name="userGraphHelper">Graph helper for users API.</param>
        /// <param name="groupGraphHelper">Graph API helper for fetching group related data.</param>
        public UsersController(
            ILogger<UsersController> logger,
            TelemetryClient telemetryClient,
            IUserGraphHelper userGraphHelper,
            IGroupGraphHelper groupGraphHelper)
            : base(telemetryClient)
        {
            this.logger = logger;
            this.userGraphHelper = userGraphHelper;
            this.groupGraphHelper = groupGraphHelper;
        }

        /// <summary>
        /// The HTTP GET call to get all event categories
        /// </summary>
        /// <param name="searchText">Search text entered by user.</param>
        /// <returns>Returns the list of categories sorted by category name if request processed successfully. Else, it throws an exception.</returns>
        [HttpGet]
        [ResponseCache(Duration = 86400)] // cache for 1 day
        public async Task<IActionResult> SearchUsersrAndGroups(string searchText)
        {
            searchText ??= string.Empty;
            this.RecordEvent("Search users and group - The HTTP call to GET users/groups has been initiated");
            try
            {
                List<UserGroupSearchResult> searchResults = new List<UserGroupSearchResult>();
                List<Graph.User> users = new List<Graph.User>();
                List<Graph.Group> groups = new List<Graph.Group>();

                var getUsersTask = this.userGraphHelper.SearchUsersAsync(searchText);
                var getGroupsTask = this.groupGraphHelper.SearchGroupsAsync(searchText);
                await Task.WhenAll(getUsersTask, getGroupsTask);

                users = getUsersTask.Result;
                groups = getGroupsTask.Result;

                searchResults.AddRange(users.Select(user => new UserGroupSearchResult
                {
                    DisplayName = user.DisplayName,
                    Id = user.Id,
                    IsGroup = false,
                    Email = user.Mail,
                }));
                searchResults.AddRange(groups?.Select(group => new UserGroupSearchResult
                {
                    DisplayName = group.DisplayName,
                    Id = group.Id,
                    IsGroup = true,
                    Email = group.Mail,
                }));

                this.RecordEvent("Search users and group - The HTTP call to GET users/groups succeeded");

                return this.Ok(searchResults.OrderBy(userAndGroup => userAndGroup.DisplayName));
            }
            catch (Exception ex)
            {
                this.RecordEvent("Search users and group - The HTTP call to GET users/groups failed");
                this.logger.LogError(ex, "Error occurred while fetching users/groups");
                throw;
            }
        }

        /// <summary>
        /// Get user profiles by user object Ids.
        /// </summary>
        /// <param name="userIds">List of user object Ids.</param>
        /// <returns>List of user profiles.</returns>
        [HttpPost]
        [ResponseCache(Duration = 1209600)] // Cache data for 14 days.
        public async Task<IActionResult> GetUsersProfiles([FromBody] List<string> userIds)
        {
            this.RecordEvent("Get users profiles - The HTTP call to GET users profiles has been initiated");

            if (userIds == null || !userIds.Any())
            {
                this.RecordEvent("Get users profiles - The HTTP call to GET users profiles has been failed");
                this.logger.LogError("User Id list cannot be null or empty");
                return this.BadRequest(new { message = "User Id list cannot be null or empty" });
            }

            try
            {
                var userProfiles = await this.userGraphHelper.GetUsersAsync(userIds);
                this.RecordEvent("Get users profiles - The HTTP call to GET users profiles has been succeeded");

                if (userProfiles != null)
                {
                    return this.Ok(userProfiles.Select(user => new { user.DisplayName, user.Id }).ToList());
                }

                return this.Ok(new List<User>());
            }
            catch (Exception ex)
            {
                this.RecordEvent("Get users profiles - The HTTP call to GET users profiles has been failed");
                this.logger.LogError(ex, "Error occurred while fetching users profiles");
                throw;
            }
        }
    }
}