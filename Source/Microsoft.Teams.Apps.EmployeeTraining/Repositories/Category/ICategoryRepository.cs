// <copyright file="ICategoryRepository.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;

    /// <summary>
    /// Interface for event categories provider which helps in retrieving, storing, updating and deleting category details.
    /// </summary>
    public interface ICategoryRepository
    {
        /// <summary>
        /// Get all categories.
        /// </summary>
        /// <returns>A collection of categories.</returns>
        Task<IEnumerable<Category>> GetCategoriesAsync();

        /// <summary>
        /// Get category details
        /// </summary>
        /// <param name="categoryId">The category Id that needs to be fetched.</param>
        /// <returns>Returns category details.</returns>
        Task<Category> GetCategoryAsync(string categoryId);

        /// <summary>
        /// Create or update a category.
        /// </summary>
        /// <param name="categoryDetails">The details of a category that needs to be created or updated.</param>
        /// <returns>Returns true if a category created or updated successfully. Else returns false.</returns>
        Task<bool> UpsertCategoryAsync(Category categoryDetails);

        /// <summary>
        /// Get categories matching list of category Ids.
        /// </summary>
        /// <param name="categoryIds">List of category Ids.</param>
        /// <returns>List of categories.</returns>
        Task<IEnumerable<Category>> GetCategoriesByIdsAsync(string[] categoryIds);

        /// <summary>
        /// Delete categories in batch.
        /// </summary>
        /// <param name="categories">List of categories which needs to be deleted.</param>
        /// <returns>Returns true if categories deleted successfully. Else returns false.</returns>
        Task<bool> DeleteCategoriesInBatchAsync(IEnumerable<Category> categories);
    }
}