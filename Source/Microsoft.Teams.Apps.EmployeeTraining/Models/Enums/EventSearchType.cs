// <copyright file="EventSearchType.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Models.Enums
{
    /// <summary>
    /// Represent the search type based on which filters and query will be generated for search service.
    /// </summary>
    public enum EventSearchType
    {
        /// <summary>
        /// Represents operation type to get all public and private events for user.
        /// </summary>
        AllPublicPrivateEventsForUser,

        /// <summary>
        /// Represents operation type to get mandatory events for user.
        /// </summary>
        MandatoryEventsForUser,

        /// <summary>
        /// Represents operation type to get draft events for team.
        /// </summary>
        DraftEventsForTeam,

        /// <summary>
        /// Represents operation type to get active events for team.
        /// </summary>
        ActiveEventsForTeam,

        /// <summary>
        /// Represents operation type to get completed events for team.
        /// </summary>
        CompletedEventsForTeam,

        /// <summary>
        /// Represents operation type to get registered events for a user.
        /// </summary>
        RegisteredEventsForUser,

        /// <summary>
        /// Gets an event for mentioned category.
        /// </summary>
        GetCategoryEvent,

        /// <summary>
        /// Represents operation type to get completed events for a user.
        /// </summary>
        CompletedEventsForUser,

        /// <summary>
        /// Search event by name
        /// </summary>
        SearchByName,

        /// <summary>
        /// Search events to send reminder a day before it starts
        /// </summary>
        DayBeforeReminder,

        /// <summary>
        /// Search events to send reminder a week before it starts
        /// </summary>
        WeekBeforeReminder,
    }
}