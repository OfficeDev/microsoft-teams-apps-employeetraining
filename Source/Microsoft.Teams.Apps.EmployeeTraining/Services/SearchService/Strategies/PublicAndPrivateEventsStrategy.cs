// <copyright file="PublicAndPrivateEventsStrategy.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Services.SearchService.Strategies
{
    using System;
    using System.Globalization;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;

    /// <summary>
    /// Generates filter query for fetching all public and private events for user.
    /// </summary>
    public class PublicAndPrivateEventsStrategy : IFilterGeneratingStrategy
    {
        /// <inheritdoc/>
        public string GenerateFilterQuery(SearchParametersDto searchParametersDto)
        {
            searchParametersDto = searchParametersDto ?? throw new ArgumentNullException(nameof(searchParametersDto), "Search parameter is null");

            return $"(search.ismatch('{searchParametersDto.UserObjectId}', '{nameof(EventEntity.OptionalAttendees)}')" +
                $" or search.ismatch('{searchParametersDto.UserObjectId}', '{nameof(EventEntity.MandatoryAttendees)}')" +
                $" or {nameof(EventEntity.Audience)} eq {(int)EventAudience.Public})" +
                $" and {nameof(EventEntity.Status)} eq {(int)EventStatus.Active}" +
                $" and {nameof(EventEntity.EndDate)} ge {DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture)}";
        }
    }
}
