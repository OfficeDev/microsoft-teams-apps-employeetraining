// <copyright file="Category.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.Azure.Search;
    using Microsoft.Teams.Apps.EmployeeTraining.Common;

    /// <summary>
    /// This class holds the details of an event category.
    /// </summary>
    public class Category : TableEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Category"/> class.
        /// </summary>
        public Category()
        {
            this.ConstantValue = Constants.CategoryEntityPartitionKey;
        }

        /// <summary>
        /// Gets or sets the unique Id of a category.
        /// </summary>
        public string CategoryId
        {
            get
            {
                return this.RowKey;
            }

            set
            {
                this.RowKey = value;
            }
        }

        /// <summary>
        /// Gets the constant value which is the partition key.
        /// </summary>
        [Required]
        public string ConstantValue
        {
            get { return this.PartitionKey; }
            private set { this.PartitionKey = value; }
        }

        /// <summary>
        /// Gets or sets the category name.
        /// </summary>
        [Required]
        [MaxLength(100)]
        [IsSearchable]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the category description.
        /// </summary>
        [Required]
        [MaxLength(300)]
        [IsSearchable]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a category is currently in use in one of the events.
        /// </summary>
        [NotMapped]
        public bool IsInUse { get; set; }

        /// <summary>
        /// Gets or sets the user name who created the category.
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the date and time on which category has created.
        /// </summary>
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// Gets or sets the user name who updated the category.
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Gets or sets the date and time on which category has updated.
        /// </summary>
        public DateTime UpdatedOn { get; set; }
    }
}