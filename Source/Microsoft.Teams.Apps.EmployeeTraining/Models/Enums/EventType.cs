// <copyright file="EventType.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Models
{
    /// <summary>
    /// Contains the values for event types.
    /// </summary>
    public enum EventType
    {
        /// <summary>
        /// Indicates that the event type is not specified.
        /// </summary>
        None,

        /// <summary>
        /// Indicates that the event occurs in physical presence.
        /// </summary>
        InPerson = 1,

        /// <summary>
        /// Indicates that the event is a Microsoft Teams meeting.
        /// </summary>
        Teams = 2,

        /// <summary>
        /// Indicates that the event is a Microsoft Teams meeting.
        /// </summary>
        LiveEvent = 3,
    }
}