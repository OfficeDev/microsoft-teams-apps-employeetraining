// <copyright file="IEventRepository.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Repositories
{
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;

    /// <summary>
    /// Interface for event repository which helps in retrieving, storing and updating event details.
    /// </summary>
    public interface IEventRepository
    {
        /// <summary>
        /// Get event details
        /// </summary>
        /// <param name="eventId">Event Id for a event.</param>
        /// <param name="teamId">The team Id of which events needs to be fetched</param>
        /// <returns>A collection of events</returns>
        Task<EventEntity> GetEventDetailsAsync(string eventId, string teamId);

        /// <summary>
        /// Create or update an event
        /// </summary>
        /// <param name="eventDetails">The details of an event that need to be created or updated</param>
        /// <returns>Returns true if an event has created or updated successfully. Else returns false.</returns>
        Task<bool> UpsertEventAsync(EventEntity eventDetails);

        /// <summary>
        /// This method updates an event.
        /// </summary>
        /// <param name="eventDetails">The details of an event that needs to be created or updated</param>
        /// <returns>Returns true if event updated successfully. Else, returns false.</returns>
        Task<bool> UpdateEventAsync(EventEntity eventDetails);
    }
}