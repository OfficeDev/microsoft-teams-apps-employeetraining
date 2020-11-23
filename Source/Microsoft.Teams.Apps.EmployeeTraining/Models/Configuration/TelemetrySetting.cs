// <copyright file="TelemetrySetting.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Models
{
    /// <summary>
    /// A class which helps to provide Telemetry settings for application.
    /// </summary>
    public class TelemetrySetting
    {
        /// <summary>
        /// Gets or sets Application Insights instrumentation key.
        /// </summary>
        public string InstrumentationKey { get; set; }
    }
}