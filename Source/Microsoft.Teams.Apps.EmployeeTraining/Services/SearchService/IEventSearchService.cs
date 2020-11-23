// <copyright file="IEventSearchService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Azure.Search.Models;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;

    /// <summary>
    /// Event search service provider to fetch events based on search and filter criteria.
    /// </summary>
    public interface IEventSearchService
    {
        /// <summary>
        /// Get event list as per search and filter criteria.
        /// </summary>
        /// <param name="searchQuery">Query which the user had typed in Messaging Extension search field.</param>
        /// <param name="searchParameters">Search parameters for enhanced searching.</param>
        /// <returns>List of events.</returns>
        Task<IEnumerable<EventEntity>> GetEventsAsync(string searchQuery, SearchParameters searchParameters);

        /// <summary>
        /// Run the indexer on demand.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        Task RunIndexerOnDemandAsync();
    }
}