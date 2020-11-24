// <copyright file="EmployeeTrainingActivityHandlerOptions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.EmployeeTraining.Models.Configuration
{
    /// <summary>
    /// This class provide options for the <see cref="EmployeeTrainingActivityHandlerOptions" /> bot.
    /// </summary>
    public sealed class EmployeeTrainingActivityHandlerOptions
    {
        /// <summary>
        /// Gets or sets application base URL used to return success or failure task module result.
        /// </summary>
        public string AppBaseUri { get; set; }

        /// <summary>
        /// Gets or sets tenant id.
        /// </summary>
        public string TenantId { get; set; }
    }
}
