// <copyright file="TeamCompletedEventsStrategy.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Services.SearchService.Strategies
{
    using System;
    using System.Globalization;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;

    /// <summary>
    /// Generates filter query to fetch completed events for team.
    /// </summary>
    public class TeamCompletedEventsStrategy : IFilterGeneratingStrategy
    {
        /// <inheritdoc/>
        public string GenerateFilterQuery(SearchParametersDto searchParametersDto)
        {
            searchParametersDto = searchParametersDto ?? throw new ArgumentNullException(nameof(searchParametersDto), "Search parameter is null");

            return $"({nameof(EventEntity.TeamId)} eq '{searchParametersDto.TeamId}' and {nameof(EventEntity.Status)} eq {(int)EventStatus.Active} " +
                        $"and {nameof(EventEntity.EndDate)} le {DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture)}) " +
                        $"or ({nameof(EventEntity.Status)} eq {(int)EventStatus.Cancelled} and {nameof(EventEntity.TeamId)} eq '{searchParametersDto.TeamId}')";
        }
    }
}