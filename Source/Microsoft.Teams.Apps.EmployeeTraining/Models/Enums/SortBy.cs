// <copyright file="SortBy.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Models.Enums
{
    /// <summary>
    /// Represents 0 for recent and 1 for popular events.
    /// </summary>
    public enum SortBy
    {
        /// <summary>
        /// Represents default sorting of event by most recent first.
        /// </summary>
        Recent,

        /// <summary>
        /// Represents sorting of events by recent collaborators of logged-in user registered for an event.
        /// </summary>
        PopularityByRecentCollaborators,

        /// <summary>
        /// Represents sorting of events by most number of registered attendee for an event.
        /// </summary>
        PopularityByRegisteredUsers,
    }
}
