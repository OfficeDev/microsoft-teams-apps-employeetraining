// <copyright file="User.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// This class is used to store user configurations.
    /// </summary>
    public class User : TableEntity
    {
        /// <summary>
        /// Gets or sets the Azure Active Directory user Id.
        /// </summary>
        [Key]
        public string AADObjectId
        {
            get
            {
                return this.PartitionKey;
            }

            set
            {
                this.PartitionKey = value;
                this.RowKey = value;
            }
        }

        /// <summary>
        /// Gets or sets user conversation Id.
        /// </summary>
        public string ConversationId { get; set; }

        /// <summary>
        /// Gets or sets service URL.
        /// </summary>
        public string ServiceUrl { get; set; }

        /// <summary>
        /// Gets or sets the date and time on which Bot has installed.
        /// </summary>
        public DateTime BotInstalledOn { get; set; }
    }
}
