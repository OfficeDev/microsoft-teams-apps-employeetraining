// <copyright file="Constants.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Common
{
    /// <summary>
    /// Constant values that are used in multiple files.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Default value for channel activity to send notifications.
        /// </summary>
        public const string TeamsBotFrameworkChannelId = "msteams";

        /// <summary>
        /// Azure search service Index name for EventEntity table.
        /// </summary>
        public const string EventsIndex = "events-index";

        /// <summary>
        /// The partition key for Category entity
        /// </summary>
        public const string CategoryEntityPartitionKey = "LnDTeamCategories";
    }
}