// <copyright file="IEventGraphHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Helpers
{
    using System.Threading.Tasks;
    using Microsoft.Graph;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;

    /// <summary>
    /// Provides helper methods to make Microsoft Graph API calls related to managing events.
    /// </summary>
    public interface IEventGraphHelper
    {
        /// <summary>
        /// Create teams event.
        /// </summary>
        /// <param name="eventEntity">Event details from user for which event needs to be created.</param>
        /// <returns>Created event details.</returns>
        Task<Event> CreateEventAsync(EventEntity eventEntity);

        /// <summary>
        /// Update teams event.
        /// </summary>
        /// <param name="eventEntity">Event details from user for which event needs to be updated.</param>
        /// <returns>Updated event details.</returns>
        Task<Event> UpdateEventAsync(EventEntity eventEntity);

        /// <summary>
        /// Cancel calendar event.
        /// </summary>
        /// <param name="eventGraphId">Event Id received from Graph.</param>
        /// <param name="createdByUserId">User Id who created event.</param>
        /// <param name="comment">Cancellation comment.</param>
        /// <returns>True if event cancellation is successful.</returns>
        Task<bool> CancelEventAsync(string eventGraphId, string createdByUserId, string comment);
    }
}
