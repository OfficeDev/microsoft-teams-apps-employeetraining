// <copyright file="WeekBeforeReminderStrategy.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Services.SearchService.Strategies
{
    using System;
    using System.Globalization;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;

    /// <summary>
    /// Generates filter query for fetching events to send week before notifications.
    /// </summary>
    public class WeekBeforeReminderStrategy : IFilterGeneratingStrategy
    {
        /// <inheritdoc/>
        public string GenerateFilterQuery(SearchParametersDto searchParametersDto)
        {
            var startDateForNextWeek = DateTime.UtcNow.Date.AddDays(7).Date;
            var endDateForNextWeek = startDateForNextWeek.AddDays(7).Date;

            return $"{nameof(EventEntity.Status)} eq {(int)EventStatus.Active} and " +
                $"{nameof(EventEntity.StartDate)} ge {startDateForNextWeek.ToString("O", CultureInfo.InvariantCulture)} and " +
                $"{nameof(EventEntity.StartDate)} le {endDateForNextWeek.ToString("O", CultureInfo.InvariantCulture)} and " +
                $"{nameof(EventEntity.RegisteredAttendeesCount)} gt 0";
        }
    }
}