// <copyright file="UserGroupSearchResult.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Models
{
    /// <summary>
    /// Represents model for search result consisting users and groups.
    /// </summary>
    public class UserGroupSearchResult
    {
        /// <summary>
        /// Gets or sets display name for user or group.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets unique object identifier for user or group.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether boolean indicating whether entity is group.
        /// </summary>
        public bool IsGroup { get; set; }

        /// <summary>
        /// Gets or sets email address for user or group.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether user or group is set to mandatory.
        /// </summary>
        public bool IsMandatory { get; set; }
    }
}
