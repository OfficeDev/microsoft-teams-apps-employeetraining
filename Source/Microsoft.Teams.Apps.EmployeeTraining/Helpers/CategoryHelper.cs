// <copyright file="CategoryHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;
    using Microsoft.Teams.Apps.EmployeeTraining.Models.Enums;
    using Microsoft.Teams.Apps.EmployeeTraining.Repositories;
    using Microsoft.Teams.Apps.EmployeeTraining.Services;

    /// <summary>
    /// The helper method for managing categories.
    /// </summary>
    public class CategoryHelper : ICategoryHelper
    {
        /// <summary>
        /// The helper class which manages search service related activities for events.
        /// </summary>
        private readonly ITeamEventSearchService teamEventSearchService;

        /// <summary>
        /// Provides the methods for event category operations on storage.
        /// </summary>
        private readonly ICategoryRepository categoryRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryHelper"/> class.
        /// </summary>
        /// <param name="teamEventSearchService">The team event search service dependency injection.</param>
        /// <param name="categoryRepository">The category repository dependency injection.</param>
        public CategoryHelper(ITeamEventSearchService teamEventSearchService, ICategoryRepository categoryRepository)
        {
            this.teamEventSearchService = teamEventSearchService;
            this.categoryRepository = categoryRepository;
        }

        /// <summary>
        /// Checks whether the category is in-use in on of the events and updates category model.
        /// </summary>
        /// <param name="categories">The list of categories.</param>
        /// <returns>Returns a task which has validated all categories to check whether they are in-use in one of the events.</returns>
        public async Task CheckIfCategoryIsInUseAsync(IEnumerable<Category> categories)
        {
            categories = categories ?? throw new ArgumentNullException(nameof(categories), "Category list is null");

            var searchParametersDto = new SearchParametersDto
            {
                SearchString = string.Empty,
                PageCount = 0,
                SearchScope = EventSearchType.GetCategoryEvent,
                TeamId = "0",
            };

            foreach (var category in categories)
            {
                searchParametersDto.CategoryId = category.CategoryId;
                var eventDetails = await this.teamEventSearchService.GetEventsAsync(searchParametersDto);
                category.IsInUse = eventDetails != null && eventDetails.Any();
            }
        }

        /// <summary>
        /// Deletes categories which are not in-use in one of the events.
        /// </summary>
        /// <param name="categoryIds">Semicolon separated category Ids that needs to be deleted.</param>
        /// <returns>Returns true if delete category operation successful. Else returns false.</returns>
        public async Task<bool> DeleteCategoriesAsync(string categoryIds)
        {
            if (string.IsNullOrEmpty(categoryIds))
            {
                throw new ArgumentException("String containing category Ids separated by semicolon is either null or empty", nameof(categoryIds));
            }

            var isDeletedSuccessfully = true;
            var categoriesList = categoryIds.Split(",");

            var categories = categoriesList.Select(categoryId => new Category { CategoryId = categoryId }).ToList();
            await this.CheckIfCategoryIsInUseAsync(categories);

            var categoriesNotInUse = categories.Where(category => !category.IsInUse);

            if (!categoriesNotInUse.IsNullOrEmpty())
            {
                var categoriesToDelete = await this.categoryRepository.GetCategoriesByIdsAsync(categoriesNotInUse.Select(category => category.CategoryId).ToArray());
                isDeletedSuccessfully = await this.categoryRepository.DeleteCategoriesInBatchAsync(categoriesToDelete);
            }

            return isDeletedSuccessfully;
        }

        /// <summary>
        /// Binds the category details to the respective events
        /// </summary>
        /// <param name="events">The list of events</param>
        /// <returns>Returns the events binded with category details</returns>
        public async Task BindCategoryNameAsync(IEnumerable<EventEntity> events)
        {
            events = events ?? throw new ArgumentNullException(nameof(events), "Event list is null");

            var eventCategoryIds = events.Select(eventDetails => eventDetails?.CategoryId).ToArray();
            var categories = await this.categoryRepository.GetCategoriesByIdsAsync(eventCategoryIds);

            if (categories?.Count() > 0)
            {
                foreach (var eventDetails in events)
                {
                    eventDetails.CategoryName = categories.Where(category => category.CategoryId == eventDetails.CategoryId)?.FirstOrDefault()?.Name;
                }
            }
        }
    }
}