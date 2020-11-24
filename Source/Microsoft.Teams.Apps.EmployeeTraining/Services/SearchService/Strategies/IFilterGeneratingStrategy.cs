// <copyright file="IFilterGeneratingStrategy.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Services.SearchService.Strategies
{
    using Microsoft.Teams.Apps.EmployeeTraining.Models;

    /// <summary>
    /// Generates filter query for various strategies.
    /// </summary>
    public interface IFilterGeneratingStrategy
    {
        /// <summary>
        /// Creates filter query.
        /// </summary>
        /// <param name="searchParametersDto">Search parameters.</param>
        /// <returns>Filter query.</returns>
        string GenerateFilterQuery(SearchParametersDto searchParametersDto);
    }
}
