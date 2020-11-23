// <copyright file="EventAudience.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Models
{
    /// <summary>
    /// Contains the values for event audience.
    /// </summary>
    public enum EventAudience
    {
        /// <summary>
        /// Indicates that the event audience is not specified.
        /// </summary>
        None,

        /// <summary>
        /// Indicates that the event is public.
        /// </summary>
        Public = 1,

        /// <summary>
        /// Indicates that the event is private.
        /// </summary>
        Private = 2,
    }
}