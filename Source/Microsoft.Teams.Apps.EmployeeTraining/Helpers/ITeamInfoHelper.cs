// <copyright file="ITeamInfoHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Helpers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Schema.Teams;

    /// <summary>
    /// Provides method to fetch member details from specific team.
    /// </summary>
    public interface ITeamInfoHelper
    {
        /// <summary>
        /// To fetch team member information for specified team.
        /// Return null if the member is not found in team id or either of the information is incorrect.
        /// Caller should handle null value to throw unauthorized if required
        /// </summary>
        /// <param name="teamId">Id of Team for which user is part of.</param>
        /// <param name="userId">Unique object identifier of user whos details needs to be fetched from team roaster.</param>
        /// <returns>Team channel information.</returns>
        Task<TeamsChannelAccount> GetTeamMemberAsync(string teamId, string userId);

        /// <summary>
        /// To fetch members of all LnD teams
        /// Return null if the members not found in team id or either of the information is incorrect.
        /// Caller should handle null value to throw unauthorized if required
        /// </summary>
        /// <returns>The LnD team members</returns>
        Task<List<TeamsChannelAccount>> GetAllLnDTeamMembersAsync();
    }
}