// <copyright file="StorageSetting.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Models.Configuration
{
    /// <summary>
    /// A class which helps to provide storage settings.
    /// </summary>
    public class StorageSetting : BotSettings
    {
        /// <summary>
        /// Gets or sets storage connection string.
        /// </summary>
        public string ConnectionString { get; set; }
    }
}