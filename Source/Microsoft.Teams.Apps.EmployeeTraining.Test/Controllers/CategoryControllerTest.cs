// <copyright file="CategoryControllerTest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Test.Controllers
{
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.EmployeeTraining.Controllers;
    using Microsoft.Teams.Apps.EmployeeTraining.Helpers;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;
    using Microsoft.Teams.Apps.EmployeeTraining.Repositories;
    using Microsoft.Teams.Apps.EmployeeTraining.Tests.Providers;
    using Microsoft.Teams.Apps.EmployeeTraining.Tests.TestData;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Threading.Tasks;

    /// <summary>
    /// The controller handles the data requests related to categories.
    /// </summary>
    [TestClass]
    public class CategoryControllerTest
    {
        CategoryController categoryController;
        Mock<ICategoryHelper> categoryHelper;
        Mock<ICategoryRepository> categoryStorageProvider;
        CategoryRepositoryFake categoryStorageProviderFake;
        TelemetryClient telemetryClient;

        [TestInitialize]
        public void CategoryControllerTestSetup()
        {
            var logger = new Mock<ILogger<CategoryController>>().Object;
            categoryStorageProviderFake = new CategoryRepositoryFake();
            categoryHelper = new Mock<ICategoryHelper>();
            categoryStorageProvider = new Mock<ICategoryRepository>();
            telemetryClient = new TelemetryClient(new TelemetryConfiguration());

            categoryController = new CategoryController(
                logger,
                telemetryClient,
                categoryStorageProvider.Object,
                categoryHelper.Object);

            var httpContext = MakeFakeContext();
            categoryController.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [TestMethod]
        public async Task GetCategoriesAsync_ReturnsOkResult()
        {
            this.categoryStorageProvider
                .Setup(c => c.GetCategoriesAsync())
                .Returns(this.categoryStorageProviderFake.GetCategoriesAsync());

            this.categoryHelper
                .Setup(c => c.CheckIfCategoryIsInUseAsync(It.IsAny<IEnumerable<Category>>()))
                .Returns(Task.FromResult(true));
            
            var Result = (ObjectResult)await this.categoryController.GetCategoriesAsync();
            
            Assert.AreEqual(Result.StatusCode, StatusCodes.Status200OK);
        }
        
        [TestMethod]
        public async Task GetCategoriesForEventsAsync_ReturnsOkResult()
        {
            this.categoryStorageProvider
                .Setup(c => c.GetCategoriesAsync())
                .Returns(this.categoryStorageProviderFake.GetCategoriesAsync());
            
            var Result = (ObjectResult)await this.categoryController.GetCategoriesToCreateEventAsync();
            
            Assert.AreEqual(Result.StatusCode, StatusCodes.Status200OK);
        }
        
        [TestMethod]
        public async Task CreateCategoryAsync_ReturnsOkResult()
        {
            this.categoryStorageProvider
                .Setup(c => c.UpsertCategoryAsync(It.IsAny<Category>()))
                .Returns(this.categoryStorageProviderFake.UpsertCategoryAsync(EventWorkflowHelperData.category));
            
            var Result = (ObjectResult)await this.categoryController.CreateCategoryAsync(EventWorkflowHelperData.category, EventWorkflowHelperData.validEventEntity.TeamId);
            
            Assert.AreEqual(Result.StatusCode, StatusCodes.Status200OK);
        }
        
        [TestMethod]
        public async Task UpdateCategoryAsync_ReturnsOkResult()
        {
            this.categoryStorageProvider
                .Setup(c => c.GetCategoryAsync(It.IsAny<string>()))
                .Returns(this.categoryStorageProviderFake.GetCategoryAsync(EventWorkflowHelperData.category.CategoryId));
            
            this.categoryStorageProvider
                .Setup(c => c.UpsertCategoryAsync(It.IsAny<Category>()))
                .Returns(this.categoryStorageProviderFake.UpsertCategoryAsync(EventWorkflowHelperData.category));
            
            var Result = (ObjectResult)await this.categoryController.UpdateCategoryAsync(EventWorkflowHelperData.category, EventWorkflowHelperData.validEventEntity.TeamId);
            
            Assert.AreEqual(Result.StatusCode, StatusCodes.Status200OK);
        }
        
        [TestMethod]
        public async Task DeleteCategoriesAsync_ReturnsOkResult()
        {
            var events = EventWorkflowHelperData.eventEntities;
            var eventCategoryIds = events.Select(eventDetails => eventDetails?.CategoryId).ToArray();

            this.categoryStorageProvider
                .Setup(c => c.GetCategoriesByIdsAsync(It.IsAny<string[]>()))
                .Returns(this.categoryStorageProviderFake.GetCategoriesByIdsAsync(eventCategoryIds));
            
            this.categoryStorageProvider
                .Setup(c => c.DeleteCategoriesInBatchAsync(It.IsAny<IEnumerable<Category>>()))
                .Returns(this.categoryStorageProviderFake.DeleteCategoriesInBatchAsync(EventWorkflowHelperData.categoryList));
            
            var Result = (ObjectResult)await this.categoryController.DeleteCategoriesAsync(EventWorkflowHelperData.validEventEntity.TeamId,EventWorkflowHelperData.category.CategoryId);
            
            Assert.AreEqual(Result.StatusCode, StatusCodes.Status200OK);
        }

        [TestMethod]
        public async Task AllMethods_InvalidArguments_ReturnsBadRequest()
        {
            var getCategoriesAsyncResult = (ObjectResult)await this.categoryController.GetCategoriesAsync();
            var deleteCategoriesAsyncResult = (ObjectResult)await this.categoryController.DeleteCategoriesAsync(EventWorkflowHelperData.validEventEntity.TeamId, null);
            var updateCategoryAsyncResult = (ObjectResult)await this.categoryController.UpdateCategoryAsync(EventWorkflowHelperData.category, "");
            var createCategoryAsyncResult = (ObjectResult)await this.categoryController.CreateCategoryAsync(null, EventWorkflowHelperData.validEventEntity.TeamId);

            Assert.AreEqual(deleteCategoriesAsyncResult.StatusCode, StatusCodes.Status400BadRequest);
            Assert.AreEqual(updateCategoryAsyncResult.StatusCode, StatusCodes.Status400BadRequest);
            Assert.AreEqual(createCategoryAsyncResult.StatusCode, StatusCodes.Status400BadRequest);
        }

        /// <summary>
        /// Make fake HTTP context for unit testing.
        /// </summary>
        /// <returns></returns>
        private static HttpContext MakeFakeContext()
        {
            var userAadObjectId = "<<AAD object id>>";
            var context = new Mock<HttpContext>();
            var request = new Mock<HttpContext>();
            var response = new Mock<HttpContext>();
            var user = new Mock<ClaimsPrincipal>();
            var identity = new Mock<IIdentity>();
            var claim = new Claim[]
            {
                new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", userAadObjectId),
            };

            context.Setup(ctx => ctx.User).Returns(user.Object);
            user.Setup(ctx => ctx.Identity).Returns(identity.Object);
            user.Setup(ctx => ctx.Claims).Returns(claim);
            identity.Setup(id => id.IsAuthenticated).Returns(true);
            identity.Setup(id => id.Name).Returns("test");
            return context.Object;
        }
    }
}
