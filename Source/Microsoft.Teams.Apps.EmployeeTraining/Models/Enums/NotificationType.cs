// <copyright file="NotificationType.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Models
{
    /// <summary>
    /// Contains the values for event audience
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// Indicates that the notification type not mentioned
        /// </summary>
        None,

        /// <summary>
        /// Indicates that the notification to be sent daily
        /// </summary>
        Daily,

        /// <summary>
        /// Indicates that the notification to be sent weekly
        /// </summary>
        Weekly,

        /// <summary>
        /// Indicates that the notification to be sent manually from manage events tab
        /// </summary>
        Manual,
    }
}