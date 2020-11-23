// <copyright file="CategoryRepository.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.EmployeeTraining.Common;
    using Microsoft.Teams.Apps.EmployeeTraining.Helpers;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;
    using Microsoft.Teams.Apps.EmployeeTraining.Models.Configuration;

    /// <summary>
    /// This class manages the category data in Azure Table Storage.
    /// </summary>
    public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
    {
        /// <summary>
        /// Represents the entity name which is used to store event categories.
        /// </summary>
        private const string CategoryEntityName = "Categories";

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryRepository"/> class.
        /// </summary>
        /// <param name="options">A set of key/value application configuration properties for Microsoft Azure Table Storage.</param>
        /// <param name="logger">To send logs to the logger service.</param>
        public CategoryRepository(
            IOptions<StorageSetting> options, ILogger<CategoryRepository> logger)
            : base(options?.Value.ConnectionString, CategoryEntityName, logger)
        {
        }

        /// <summary>
        /// Get all categories.
        /// </summary>
        /// <returns>A collection of categories.</returns>
        public async Task<IEnumerable<Category>> GetCategoriesAsync()
        {
            await this.EnsureInitializedAsync();
            return await this.GetAllAsync(Constants.CategoryEntityPartitionKey);
        }

        /// <summary>
        /// Get category details.
        /// </summary>
        /// <param name="categoryId">The category Id that needs to be fetched.</param>
        /// <returns>Returns category details.</returns>
        public async Task<Category> GetCategoryAsync(string categoryId)
        {
            if (string.IsNullOrEmpty(categoryId))
            {
                throw new ArgumentNullException(nameof(categoryId));
            }

            await this.EnsureInitializedAsync();
            return await this.GetAsync(Constants.CategoryEntityPartitionKey, categoryId);
        }

        /// <summary>
        /// This method inserts a new category in Azure Table Storage if it is not already exists. Else updates the existing one.
        /// </summary>
        /// <param name="categoryDetails">The category details that needs to be created or updated.</param>
        /// <returns>Returns true if category created or updated successfully. Else, returns false.</returns>
        public async Task<bool> UpsertCategoryAsync(Category categoryDetails)
        {
            if (categoryDetails == null)
            {
                throw new ArgumentException("The category details should be provided", nameof(categoryDetails));
            }

            await this.EnsureInitializedAsync();
            return await this.CreateOrUpdateAsync(categoryDetails);
        }

        /// <summary>
        /// Get categories matching list of category Ids.
        /// </summary>
        /// <param name="categoryIds">List of category Ids.</param>
        /// <returns>List of categories.</returns>
        public async Task<IEnumerable<Category>> GetCategoriesByIdsAsync(string[] categoryIds)
        {
            if (categoryIds.IsNullOrEmpty())
            {
                throw new ArgumentException("Category Ids should be provided", nameof(categoryIds));
            }

            await this.EnsureInitializedAsync();
            var filterQuery = this.GetRowKeysFilter(categoryIds);
            return await this.GetWithFilterAsync(filterQuery, Constants.CategoryEntityPartitionKey);
        }

        /// <summary>
        /// Delete categories in batch.
        /// </summary>
        /// <param name="categories">List of categories which needs to be deleted.</param>
        /// <returns>Returns true if categories deleted successfully. Else returns false.</returns>
        public async Task<bool> DeleteCategoriesInBatchAsync(IEnumerable<Category> categories)
        {
            if (categories.IsNullOrEmpty())
            {
                throw new ArgumentException("Categories cannot be empty", nameof(categories));
            }

            await this.EnsureInitializedAsync();
            await this.BatchDeleteAsync(categories);
            return true;
        }
    }
}
