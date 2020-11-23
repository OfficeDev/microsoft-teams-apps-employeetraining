// <copyright file="ITeamEventSearchService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;

    /// <summary>
    /// Event search helper to construct filter and search queries for operations of LnD team.
    /// </summary>
    public interface ITeamEventSearchService
    {
        /// <summary>
        /// Get LnD team events as per user search text.
        /// </summary>
        /// <param name="searchParametersDto">Search parameters entered by user.</param>
        /// <returns>List of events.</returns>
        Task<IEnumerable<EventEntity>> GetEventsAsync(SearchParametersDto searchParametersDto);
    }
}