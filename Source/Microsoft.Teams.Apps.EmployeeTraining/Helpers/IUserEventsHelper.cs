// <copyright file="IUserEventsHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Helpers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;
    using Microsoft.Teams.Apps.EmployeeTraining.Models.Enums;

    /// <summary>
    /// The helper methods for the operations that user can perform on the events
    /// </summary>
    public interface IUserEventsHelper
    {
        /// <summary>
        /// Get event details.
        /// </summary>
        /// <param name="eventId">Event Id for which details needs to be fetched.</param>
        /// <param name="teamId">Team Id with which event is associated.</param>
        /// <param name="userObjectId">The user Id</param>
        /// <returns>Event details.</returns>
        Task<EventEntity> GetEventAsync(string eventId, string teamId, string userObjectId);

        /// <summary>
        /// Get user events as per user search text and filters
        /// </summary>
        /// <param name="searchString">Search string entered by user.</param>
        /// <param name="pageCount">>Page count for which post needs to be fetched.</param>
        /// <param name="eventSearchType">Event search operation type. Refer <see cref="EventSearchType"/> for values.</param>
        /// <param name="userObjectId">Logged in user's AAD object identifier.</param>
        /// <param name="createdByFilter">Semicolon separated user AAD object identifier who created events.</param>
        /// <param name="categoryFilter">Semicolon separated category Ids.</param>
        /// <param name="sortBy">0 for recent and 1 for popular events. Refer <see cref="SortBy"/> for values.</param>
        /// <returns>List of user events</returns>
        Task<IEnumerable<EventEntity>> GetEventsAsync(string searchString, int pageCount, int eventSearchType, string userObjectId, string createdByFilter, string categoryFilter, int sortBy);

        /// <summary>
        /// Registers the user for an event
        /// </summary>
        /// <param name="teamId">The LnD team Id who created the event</param>
        /// <param name="eventId">The event Id</param>
        /// <param name="userAADObjectId">The user Id</param>
        /// <returns>Returns true if registration done successfully. Else returns false.</returns>
        Task<bool> RegisterToEventAsync(string teamId, string eventId, string userAADObjectId);

        /// <summary>
        /// Unregisters the user for an event
        /// </summary>
        /// <param name="teamId">The LnD team Id who created the event</param>
        /// <param name="eventId">The event Id</param>
        /// <param name="userAADObjectId">The user Id</param>
        /// <returns>Returns true if the user successfully unregistered for an event. Else returns false.</returns>
        Task<bool> UnregisterFromEventAsync(string teamId, string eventId, string userAADObjectId);
    }
}