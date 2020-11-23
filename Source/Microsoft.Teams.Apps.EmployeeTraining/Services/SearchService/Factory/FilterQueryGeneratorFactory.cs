// <copyright file="FilterQueryGeneratorFactory.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Services.SearchService.Factory
{
    using System.Collections.Generic;
    using Microsoft.Teams.Apps.EmployeeTraining.Models.Enums;
    using Microsoft.Teams.Apps.EmployeeTraining.Services.SearchService.Strategies;

    /// <summary>
    /// Factory to get filter query according to strategies.
    /// </summary>
    public class FilterQueryGeneratorFactory : IFilterQueryGeneratorFactory
    {
        private readonly IDictionary<EventSearchType, IFilterGeneratingStrategy> strategies;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterQueryGeneratorFactory"/> class.
        /// </summary>
        public FilterQueryGeneratorFactory()
        {
            this.strategies = new Dictionary<EventSearchType, IFilterGeneratingStrategy>()
            {
                { EventSearchType.AllPublicPrivateEventsForUser, new PublicAndPrivateEventsStrategy() },
                { EventSearchType.MandatoryEventsForUser, new MandatoryEventsStrategy() },
                { EventSearchType.RegisteredEventsForUser, new RegisteredEventsStrategy() },
                { EventSearchType.CompletedEventsForUser, new CompletedEventsStrategy() },
                { EventSearchType.DayBeforeReminder, new DayBeforeReminderStrategy() },
                { EventSearchType.WeekBeforeReminder, new WeekBeforeReminderStrategy() },
                { EventSearchType.DraftEventsForTeam, new TeamDraftEventStrategy() },
                { EventSearchType.ActiveEventsForTeam, new TeamActiveEventsStrategy() },
                { EventSearchType.CompletedEventsForTeam, new TeamCompletedEventsStrategy() },
                { EventSearchType.GetCategoryEvent, new TeamCategoryEventsStrategy() },
            };
        }

        /// <inheritdoc/>
        public IFilterGeneratingStrategy GetStrategy(EventSearchType eventSearchType)
        {
            return this.strategies.ContainsKey(eventSearchType) ? this.strategies[eventSearchType] : null;
        }
    }
}
