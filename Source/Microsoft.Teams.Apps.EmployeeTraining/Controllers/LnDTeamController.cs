// <copyright file="LnDTeamController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.EmployeeTraining.Helpers;

    /// <summary>
    /// The controller handles the data requests related to categories
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LnDTeamController : BaseController
    {
        /// <summary>
        /// Logs errors and information
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The helper class which provides methods to manage team channel details
        /// </summary>
        private readonly ITeamInfoHelper teamInfoHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="LnDTeamController"/> class.
        /// </summary>
        /// <param name="logger">The ILogger object which logs errors and information</param>
        /// <param name="telemetryClient">The Application Insights telemetry client</param>
        /// <param name="teamInfoHelper">The team info helper dependency injection</param>
        public LnDTeamController(
            ILogger<CategoryController> logger,
            TelemetryClient telemetryClient,
            ITeamInfoHelper teamInfoHelper)
            : base(telemetryClient)
        {
            this.logger = logger;
            this.teamInfoHelper = teamInfoHelper;
        }

        /// <summary>
        /// Gets all LnD teams' members
        /// </summary>
        /// <returns>Returns all LnD teams' members</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllLnDTeamMembersAsync()
        {
            try
            {
                this.RecordEvent("Get All LnD teams' members- The HTTP GET call to get all LnD teams' members has initiated");

                var allLnDTeamMembers = await this.teamInfoHelper.GetAllLnDTeamMembersAsync();

                this.RecordEvent("Get All LnD teams' members- The HTTP GET call to get all LnD teams' members has succeeded");

                if (allLnDTeamMembers == null)
                {
                    return this.NoContent();
                }

                var result = allLnDTeamMembers.Select(member => new { member.AadObjectId, member.Name }).Distinct();
                return this.Ok(result.OrderBy(member => member.Name));
            }
            catch (Exception ex)
            {
                this.RecordEvent("Get All LnD teams' members- The HTTP GET call to get all LnD teams' members has failed");
                this.logger.LogError(ex, "Error occurred while fetching all LnD teams' members");
                throw;
            }
        }
    }
}
