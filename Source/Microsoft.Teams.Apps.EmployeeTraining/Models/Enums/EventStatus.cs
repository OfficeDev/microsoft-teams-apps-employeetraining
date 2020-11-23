// <copyright file="EventStatus.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Models
{
    /// <summary>
    /// Contains the values for event status.
    /// </summary>
    public enum EventStatus
    {
        /// <summary>
        /// Indicates that the event status is not specified.
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates that the event is in draft.
        /// </summary>
        Draft = 1,

        /// <summary>
        /// Indicates that the event is active.
        /// </summary>
        Active = 2,

        /// <summary>
        /// Indicates that the event has been cancelled.
        /// </summary>
        Cancelled = 3,

        /// <summary>
        /// Indicates that the event has been completed.
        /// </summary>
        Completed = 4,
    }
}