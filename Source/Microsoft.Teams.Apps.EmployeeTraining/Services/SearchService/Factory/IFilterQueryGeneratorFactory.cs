// <copyright file="IFilterQueryGeneratorFactory.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Services.SearchService.Factory
{
    using Microsoft.Teams.Apps.EmployeeTraining.Models.Enums;
    using Microsoft.Teams.Apps.EmployeeTraining.Services.SearchService.Strategies;

    /// <summary>
    /// Generates filter query for fetching events.
    /// </summary>
    public interface IFilterQueryGeneratorFactory
    {
        /// <summary>
        /// Get filter query.
        /// </summary>
        /// <param name="eventSearchType">Search type</param>
        /// <returns>Filter query.</returns>
        public IFilterGeneratingStrategy GetStrategy(EventSearchType eventSearchType);
    }
}
