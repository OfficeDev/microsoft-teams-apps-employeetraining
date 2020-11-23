// <copyright file="StorageOptions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Common.Models
{
    /// <summary>
    /// Provides application setting related to Azure Table Storage.
    /// </summary>
    public class StorageOptions
    {
        /// <summary>
        /// Gets or sets Azure Table Storage connection string.
        /// </summary>
        public string ConnectionString { get; set; }
    }
}
