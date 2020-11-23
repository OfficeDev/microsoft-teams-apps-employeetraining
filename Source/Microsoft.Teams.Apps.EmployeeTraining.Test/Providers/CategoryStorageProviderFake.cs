using Castle.Core.Internal;
using Microsoft.Teams.Apps.EmployeeTraining.Models;
using Microsoft.Teams.Apps.EmployeeTraining.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Teams.Apps.EmployeeTraining.Tests.Providers
{
    public class CategoryStorageProviderFake : ICategoryRepository
    {
        public readonly List<Category> categories;

        public CategoryStorageProviderFake()
        {
            categories = new List<Category>()
            {
                new Category
                {
                    CategoryId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2ba",
                    Name = "Test_Category_1",
                    Description = "Description",
                    CreatedBy = "ad4b2b43-1cb5-408d-ab8a-17e28edacabc",
                    CreatedOn = DateTime.UtcNow,
                    UpdatedOn = DateTime.UtcNow
                },
                new Category
                {
                    CategoryId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2baeee",
                    Name = "Test_Category_2"
                },
                new Category
                {
                    CategoryId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2baeeefg",
                    Name = "Test_Category_3"
                }
            };
        }

        /// <summary>
        /// Get all categories.
        /// </summary>
        /// <returns>A collection of categories.</returns>
        public async Task<IEnumerable<Category>> GetCategoriesAsync()
        {
            var categories = this.categories;
            return await Task.Run(() => categories) as IEnumerable<Category>;
            
        }

        /// <summary>
        /// Get category details.
        /// </summary>
        /// <param name="categoryId">The category Id that needs to be fetched.</param>
        /// <returns>Returns category details.</returns>
        public async Task<Category> GetCategoryAsync(string categoryId)
        {
            var categories = this.categories;
            return await Task.Run(() => categories.FirstOrDefault());
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

            bool value = true;
            var testValue = await Task.Run(() => value);
            return true;
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
            List<Category> categories = new List<Category>();

            foreach(string categoryId in categoryIds)
            {
                var category = this.categories.FirstOrDefault(c => c.CategoryId == categoryId);
                if (category != null)
                {
                    categories.Add(category);
                }
            }
            return await Task.Run(() => categories) as IEnumerable<Category>;
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

            bool value = true;
            var testValue = await Task.Run(() => value);
            return true;
        }
    }
}
