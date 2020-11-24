// <copyright file="EventOperationType.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Models.Enums
{
    /// <summary>
    /// Indicates the event operation
    /// </summary>
    public enum EventOperationType
    {
        /// <summary>
        /// No event operation
        /// </summary>
        None,

        /// <summary>
        /// The close event registration
        /// </summary>
        CloseRegistration,

        /// <summary>
        /// Cancel an event
        /// </summary>
        CancelEvent,
    }
}
