// <copyright file="CategoryController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.EmployeeTraining.Authentication;
    using Microsoft.Teams.Apps.EmployeeTraining.Helpers;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;
    using Microsoft.Teams.Apps.EmployeeTraining.Repositories;

    /// <summary>
    /// The controller handles the data requests related to categories.
    /// </summary>
    [Route("api/category")]
    [ApiController]
    public class CategoryController : BaseController
    {
        /// <summary>
        /// Logs errors and information.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Provides the methods for event category operations on storage.
        /// </summary>
        private readonly ICategoryRepository categoryStorageProvider;

        /// <summary>
        /// Provides the helper methods for managing categories.
        /// </summary>
        private readonly ICategoryHelper categoryHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryController"/> class.
        /// </summary>
        /// <param name="logger">The ILogger object which logs errors and information.</param>
        /// <param name="telemetryClient">The Application Insights telemetry client.</param>
        /// <param name="categoryStorageProvider">The category storage provider dependency injection.</param>
        /// <param name="categoryHelper">The category helper dependency injection.</param>
        public CategoryController(
            ILogger<CategoryController> logger,
            TelemetryClient telemetryClient,
            ICategoryRepository categoryStorageProvider,
            ICategoryHelper categoryHelper)
            : base(telemetryClient)
        {
            this.logger = logger;
            this.categoryStorageProvider = categoryStorageProvider;
            this.categoryHelper = categoryHelper;
        }

        /// <summary>
        /// The HTTP GET call to get all event categories.
        /// </summary>
        /// <returns>Returns the list of categories sorted by category name if request processed successfully. Else, it throws an exception.</returns>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetCategoriesAsync()
        {
            this.RecordEvent("Get all categories- The HTTP call to GET all categories has been initiated");

            try
            {
                var categories = await this.categoryStorageProvider.GetCategoriesAsync();

                this.RecordEvent("Get all categories- The HTTP call to GET all categories succeeded");

                if (categories.IsNullOrEmpty())
                {
                    this.logger.LogInformation("Categories are not available");
                    return this.Ok(new List<Category>());
                }

                await this.categoryHelper.CheckIfCategoryIsInUseAsync(categories);

                return this.Ok(categories.OrderBy(category => category.Name));
            }
            catch (Exception ex)
            {
                this.RecordEvent("Get all categories- The HTTP call to GET all categories has been failed");
                this.logger.LogError(ex, "Error occurred while fetching all categories");
                throw;
            }
        }

        /// <summary>
        /// The HTTP GET call to get all event categories.
        /// </summary>
        /// <returns>Returns the list of categories sorted by category name if request processed successfully. Else, it throws an exception.</returns>
        [Authorize]
        [HttpGet("get-categories-for-event")]
        public async Task<IActionResult> GetCategoriesToCreateEventAsync()
        {
            this.RecordEvent("Get all categories- The HTTP call to GET all categories has been initiated");

            try
            {
                var categories = await this.categoryStorageProvider.GetCategoriesAsync();

                this.RecordEvent("Get all categories- The HTTP call to GET all categories succeeded");

                if (categories.IsNullOrEmpty())
                {
                    this.logger.LogInformation("Categories are not available");
                    return this.Ok(new List<Category>());
                }

                return this.Ok(categories.OrderBy(category => category.Name));
            }
            catch (Exception ex)
            {
                this.RecordEvent("Get all categories- The HTTP call to GET all categories has been failed");
                this.logger.LogError(ex, "Error occurred while fetching all categories");
                throw;
            }
        }

        /// <summary>
        /// The HTTP POST call to create a new category.
        /// </summary>
        /// <param name="categoryDetails">The category details that needs to be created.</param>
        /// <param name="teamId">The LnD team Id.</param>
        /// <returns>Returns true in case if category created successfully. Else returns false.</returns>
        [Authorize(PolicyNames.MustBeLnDTeamMemberPolicy)]
        [HttpPost]
        public async Task<IActionResult> CreateCategoryAsync([FromBody] Category categoryDetails, string teamId)
        {
            if (string.IsNullOrEmpty(teamId))
            {
                this.logger.LogError("Team Id is either null or empty");
                return this.BadRequest(new ErrorResponse { Message = "Team Id is either null or empty" });
            }

            if (categoryDetails == null)
            {
                this.logger.LogError("The category details must be provided");
                return this.BadRequest(new ErrorResponse { Message = "The category details must be provided" });
            }

            Category category = new Category
            {
#pragma warning disable CA1062 // Null check is handled by data annotations at model level
                CategoryId = Convert.ToString(Guid.NewGuid(), CultureInfo.InvariantCulture),
#pragma warning restore CA1062 // Null check is handled by data annotations at model level
                Name = categoryDetails.Name.Trim(),
                Description = categoryDetails.Description.Trim(),
                CreatedBy = this.UserAadId,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow,
            };

            this.RecordEvent("Create category- The HTTP POST call to create a category has been initiated");

            try
            {
                var isCategoryCreated = await this.categoryStorageProvider.UpsertCategoryAsync(category);
                this.RecordEvent("Create category- The HTTP POST call to create a category has succeeded");

                return this.Ok(isCategoryCreated);
            }
            catch (Exception ex)
            {
                this.RecordEvent("Create category- The HTTP POST call to create a category has been failed");
                this.logger.LogError(ex, "Error occurred while creating a category");
                throw;
            }
        }

        /// <summary>
        /// The HTTP PATCH call to update a category.
        /// </summary>
        /// <param name="categoryDetails">The category details that needs to be updated.</param>
        /// <param name="teamId">The LnD team Id.</param>
        /// <returns>Returns true in case if category updated successfully. Else returns false.</returns>
        [Authorize(PolicyNames.MustBeLnDTeamMemberPolicy)]
        [HttpPatch]
        public async Task<IActionResult> UpdateCategoryAsync([FromBody] Category categoryDetails, string teamId)
        {
            this.RecordEvent("Update category- The HTTP PATCH call to update a category has been initiated");

            if (string.IsNullOrEmpty(teamId))
            {
                this.logger.LogError("Team Id is either null or empty");
                this.RecordEvent("Update category- The HTTP PATCH call to update a category has been initiated");
                return this.BadRequest(new ErrorResponse { Message = "Team Id is either null or empty" });
            }

            try
            {
#pragma warning disable CA1062 // Null check is handled by data annotations at model level
                var categoryData = await this.categoryStorageProvider.GetCategoryAsync(categoryDetails.CategoryId);
#pragma warning restore CA1062 // Null check is handled by data annotations at model level

                if (categoryData == null)
                {
                    this.RecordEvent(string.Format(CultureInfo.InvariantCulture, "Update category- The HTTP PATCH call to update a category has failed since the category Id {0} was not found for the team Id {1} and user Id {2}", categoryDetails.CategoryId, teamId, this.UserAadId));
                    return this.Ok(false);
                }

                categoryData.Name = categoryDetails.Name;
                categoryData.Description = categoryDetails.Description;
                categoryData.UpdatedBy = this.UserAadId;
                categoryData.UpdatedOn = DateTime.UtcNow;

                var isCategoryUpdated = await this.categoryStorageProvider.UpsertCategoryAsync(categoryData);

                if (!isCategoryUpdated)
                {
                    this.RecordEvent("Update category- The category update was unsuccessful");
                }

                this.RecordEvent("Update category- The category has been updated successfully");
                return this.Ok(isCategoryUpdated);
            }
            catch (Exception ex)
            {
                this.RecordEvent("Update category- The HTTP PATCH call to update a category has been failed");
                this.logger.LogError(ex, "Error occurred while updating a category");
                throw;
            }
        }

        /// <summary>
        /// The HTTP DELETE call to delete the categories.
        /// </summary>
        /// <param name="teamId">The team Id from which categories need to be deleted.</param>
        /// <param name="categoryIds">The comma separated category Ids to be deleted.</param>
        /// <returns>Returns true if categories deleted successfully. Else returns false.</returns>
        [Authorize(PolicyNames.MustBeLnDTeamMemberPolicy)]
        [HttpDelete]
        public async Task<IActionResult> DeleteCategoriesAsync(string teamId, string categoryIds)
        {
            if (string.IsNullOrEmpty(teamId))
            {
                this.logger.LogError("Team Id is either null or empty");
                return this.BadRequest(new ErrorResponse { Message = "Team Id is either null or empty" });
            }

            if (string.IsNullOrEmpty(categoryIds))
            {
                this.logger.LogError("String containing category Ids is either null or empty");
                return this.BadRequest(new ErrorResponse { Message = "String containing category Ids is either null or empty" });
            }

            this.RecordEvent("Delete categories- The HTTP call to delete categories has been initiated");

            try
            {
                var categoriesList = categoryIds.Split(",");
                var categories = categoriesList.Select(categoryId => new Category { CategoryId = categoryId }).ToList();

                await this.categoryHelper.CheckIfCategoryIsInUseAsync(categories);

                var categoriesNotInUse = categories.Where(category => !category.IsInUse);

                if (categoriesNotInUse != null && categoriesNotInUse.Any())
                {
                    var updatedCategories = await this.categoryStorageProvider.GetCategoriesByIdsAsync(categoriesNotInUse.Select(category => category.CategoryId).ToArray());

                    var isDeleteSuccessful = await this.categoryStorageProvider.DeleteCategoriesInBatchAsync(updatedCategories);

                    if (!isDeleteSuccessful)
                    {
                        this.RecordEvent("Delete categories- The delete categories operation was unsuccessful");
                    }

                    this.RecordEvent("Delete categories- The categories has been deleted successfully");

                    return this.Ok(isDeleteSuccessful);
                }

                return this.Ok(false);
            }
            catch (Exception ex)
            {
                this.RecordEvent("Delete categories- The HTTP call to delete categories has been failed");
                this.logger.LogError(ex, "Error occurred while deleting categories");
                throw;
            }
        }
    }
}
