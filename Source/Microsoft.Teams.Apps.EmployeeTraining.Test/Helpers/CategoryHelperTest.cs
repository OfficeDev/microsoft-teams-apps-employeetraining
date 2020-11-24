// <copyright file="CategoryHelperTest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Tests.Helpers
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Microsoft.Teams.Apps.EmployeeTraining.Tests.TestData;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.EmployeeTraining.Tests.Providers;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;
    using System.Linq;
    using Microsoft.Teams.Apps.EmployeeTraining.Repositories;
    using Microsoft.Teams.Apps.EmployeeTraining.Services;
    using Microsoft.Teams.Apps.EmployeeTraining.Helpers;
    using System.Collections.Generic;

    [TestClass]
    public class CategoryHelperTest
    {
        CategoryHelper categoryHelper;
        Mock<ICategoryRepository> categoryStorageProvider;
        CategoryStorageProviderFake categoryStorageProviderFake;
        Mock<ITeamEventSearchService> teamEventSearchService;

        [TestInitialize]
        public void CategoryHelperTestSetup()
        {
            categoryStorageProvider = new Mock<ICategoryRepository>();
            teamEventSearchService = new Mock<ITeamEventSearchService>();
            this.categoryStorageProviderFake = new CategoryStorageProviderFake();

            categoryHelper = new CategoryHelper(
                teamEventSearchService.Object,
                categoryStorageProvider.Object);
        }

        [TestMethod]
        public async Task ValidateCategoryInUse_NotInUse()
        {
            const string categoryToTest = "ad4b2b43-1cb5-408d-ab8a-17e28edac1ba";
            var searchParamsDto = new SearchParametersDto();

            this.teamEventSearchService
                .Setup(t => t.GetEventsAsync(searchParamsDto))
                .Returns(Task.FromResult(EventWorkflowHelperData.eventEntities.Where(e => e.CategoryId == categoryToTest)));

            await this.categoryHelper.CheckIfCategoryIsInUseAsync(EventWorkflowHelperData.categoryList);

            Assert.AreEqual(false, EventWorkflowHelperData.categoryList.Where(e => e.CategoryId == categoryToTest).FirstOrDefault().IsInUse);
        }

        [TestMethod]
        public async Task DeleteCategoriesAsync()
        {
            const string categoryToTest = "ad4b2b43-1cb5-408d-ab8a-17e28edac1ba,ad4b2b43-1cb5-408d-ab8a-17e28edac2ba";
            var searchParamsDto = new SearchParametersDto();

            this.teamEventSearchService
                .Setup(t => t.GetEventsAsync(searchParamsDto))
                .Returns(Task.FromResult(EventWorkflowHelperData.eventEntities.Where(e => e.CategoryId == categoryToTest)));

            this.categoryStorageProvider
                .Setup(t => t.GetCategoriesByIdsAsync(It.IsAny<string[]>()))
                .Returns(Task.FromResult(EventWorkflowHelperData.categoryList as IEnumerable<Category>));
            this.categoryStorageProvider
                .Setup(t => t.DeleteCategoriesInBatchAsync(It.IsAny<IEnumerable<Category>>()))
                .Returns(Task.FromResult(true));

            var Result = await this.categoryHelper.DeleteCategoriesAsync(categoryToTest);

            Assert.AreEqual(true, Result);
        }

        [TestMethod]
        public async Task BindCategoryDetailsAsync()
        {
            var events = EventWorkflowHelperData.eventEntities;
            var eventCategoryIds = events.Select(eventDetails => eventDetails?.CategoryId).ToArray();

            this.categoryStorageProvider
                .Setup(x => x.GetCategoriesByIdsAsync(eventCategoryIds))
                .Returns(this.categoryStorageProviderFake.GetCategoriesByIdsAsync(eventCategoryIds));

            await this.categoryHelper.BindCategoryNameAsync(events);

            Assert.AreEqual(true, events[0].CategoryName != "");

        }
    }
}

