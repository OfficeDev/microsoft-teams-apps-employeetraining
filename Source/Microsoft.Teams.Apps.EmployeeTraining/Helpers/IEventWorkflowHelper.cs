// <copyright file="IEventWorkflowHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Helpers
{
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;

    /// <summary>
    /// Helper for event CRUD operations.
    /// </summary>
    public interface IEventWorkflowHelper
    {
        /// <summary>
        /// Create new event as draft.
        /// </summary>
        /// <param name="eventEntity">Event details to be saved as draft.</param>
        /// <returns>Boolean indicating save operation result.</returns>
        Task<bool> CreateDraftEventAsync(EventEntity eventEntity);

        /// <summary>
        /// Update draft event.
        /// </summary>
        /// <param name="eventEntity">Draft event details to be updated.</param>
        /// <returns>Boolean indicating save operation result.</returns>
        Task<bool?> UpdateDraftEventAsync(EventEntity eventEntity);

        /// <summary>
        /// Create new event as draft.
        /// </summary>
        /// <param name="eventEntity">Event details to be saved as draft.</param>
        /// <param name="createdByName">Name of person who created event.</param>
        /// <returns>Boolean indicating save operation result.</returns>
        Task<bool?> CreateNewEventAsync(EventEntity eventEntity, string createdByName);

        /// <summary>
        /// Update event.
        /// </summary>
        /// <param name="eventEntity">Event details to be updated.</param>
        /// <returns>Boolean indicating save operation result.</returns>
        Task<bool?> UpdateEventAsync(EventEntity eventEntity);

        /// <summary>
        /// Delete draft event.
        /// </summary>
        /// <param name="teamId">Team Id by which event was created.</param>
        /// <param name="eventId">Event Id of event which needs to be deleted.</param>
        /// <returns>Boolean indicating delete operation result.</returns>
        Task<bool?> DeleteDraftEventAsync(string teamId, string eventId);

        /// <summary>
        /// Closes event registrations
        /// </summary>
        /// <param name="teamId">The LnD team Id</param>
        /// <param name="eventId">The event Id of which registrations to be closed</param>
        /// <param name="userAadId">The logged-in user's AAD Id</param>
        /// <returns>Returns true if event registrations closed successfully. Else returns false.</returns>
        Task<bool> CloseEventRegistrationsAsync(string teamId, string eventId, string userAadId);

        /// <summary>
        /// Updates the event status
        /// </summary>
        /// <param name="teamId">The LnD team Id</param>
        /// <param name="eventId">The event Id of which status to change</param>
        /// <param name="eventStatus">The event status to change</param>
        /// <param name="userAadId">The logged-in user's AAD Id</param>
        /// <returns>Returns true if event status updated successfully. Else returns false.</returns>
        Task<bool> UpdateEventStatusAsync(string teamId, string eventId, EventStatus eventStatus, string userAadId);

        /// <summary>
        /// Export event details to CSV
        /// </summary>
        /// <param name="teamId">The LnD team Id</param>
        /// <param name="eventId">The event Id of which details needs to be exported</param>
        /// <returns>Returns CSV data in stream</returns>
        Task<Stream> ExportEventDetailsToCSVAsync(string teamId, string eventId);

        /// <summary>
        /// Sends reminder to the registered users for an event
        /// </summary>
        /// <param name="teamId">The LnD team Id</param>
        /// <param name="eventId">The event Id for which notification to send</param>
        /// <returns>Returns the list of user Ids to whom notification send was failed</returns>
        Task SendReminderAsync(string teamId, string eventId);
    }
}