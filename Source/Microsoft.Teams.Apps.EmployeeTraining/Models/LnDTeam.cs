// <copyright file="LnDTeam.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// This class is responsible to handle LnD team configurations details.
    /// </summary>
    public class LnDTeam : TableEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for each team.
        /// </summary>
        [Key]
        public string TeamId
        {
            get
            {
                return this.RowKey;
            }

            set
            {
                this.RowKey = value;
                this.PartitionKey = value;
            }
        }

        /// <summary>
        /// Gets or sets the date and time on which Bot has installed.
        /// </summary>
        public DateTime BotInstalledOn { get; set; }

        /// <summary>
        /// Gets or sets service url for bot.
        /// </summary>
        public string ServiceUrl { get; set; }
    }
}