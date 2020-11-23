// <copyright file="ICategoryHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Helpers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;

    /// <summary>
    /// Lists the helper methods for managing categories for events.
    /// </summary>
    public interface ICategoryHelper
    {
        /// <summary>
        /// Validates whether the category is in-use in on of the events.
        /// </summary>
        /// <param name="categories">The list of categories.</param>
        /// <returns>Returns a task which has validated all categories to check whether they are in-use in one of the events.</returns>
        Task CheckIfCategoryIsInUseAsync(IEnumerable<Category> categories);

        /// <summary>
        /// Deletes categories which are not in-use in one of the events.
        /// </summary>
        /// <param name="categoryIds">The category Ids that needs to be deleted.</param>
        /// <returns>Returns true if delete category operation successful. Else returns false.</returns>
        Task<bool> DeleteCategoriesAsync(string categoryIds);

        /// <summary>
        /// Binds the category details to the respective events
        /// </summary>
        /// <param name="events">The list of events</param>
        /// <returns>Returns the events binded with category details</returns>
        Task BindCategoryNameAsync(IEnumerable<EventEntity> events);
    }
}