// <copyright file="IUserEventSearchService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;

    /// <summary>
    /// Helper for generating filter and search conditions for search service.
    /// </summary>
    public interface IUserEventSearchService
    {
        /// <summary>
        /// Get events as per user search text.
        /// </summary>
        /// <param name="searchParametersDto">Search parameters entered by user.</param>
        /// <returns>List of events.</returns>
        Task<IEnumerable<EventEntity>> GetEventsAsync(SearchParametersDto searchParametersDto);
    }
}