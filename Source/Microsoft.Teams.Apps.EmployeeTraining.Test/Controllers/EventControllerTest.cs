// <copyright file="EventControllerTest.cs" company="Microsoft">
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
    using Microsoft.Teams.Apps.EmployeeTraining.Models.Enums;
    using Microsoft.Teams.Apps.EmployeeTraining.Services;
    using Microsoft.Teams.Apps.EmployeeTraining.Tests.TestData;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Threading.Tasks;

    [TestClass]
    public class EventControllerTest
    {
        EventController eventController;
        Mock<ICategoryHelper> categoryHelper;
        Mock<IUserEventsHelper> userEventsHelper;
        Mock<IUserEventSearchService> userEventSearchService;
        TelemetryClient telemetryClient;

        [TestInitialize]
        public void EventControllerTestSetup()
        {
            var logger = new Mock<ILogger<EventController>>().Object;
            telemetryClient = new TelemetryClient(new TelemetryConfiguration());
            categoryHelper = new Mock<ICategoryHelper>();
            userEventsHelper = new Mock<IUserEventsHelper>();
            userEventSearchService = new Mock<IUserEventSearchService>();

            eventController = new EventController(
                logger,
                telemetryClient,
                userEventSearchService.Object,
                userEventsHelper.Object,
                categoryHelper.Object);

            var httpContext = MakeFakeContext();
            eventController.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [TestMethod]
        public async Task GetEventAsync_ReturnsOkResult()
        {
            this.userEventsHelper
                .Setup(u => u.GetEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(EventWorkflowHelperData.eventEntity));
            
            this.categoryHelper
                .Setup(e => e.BindCategoryNameAsync(It.IsAny<IEnumerable<EventEntity>>()))
                .Returns(Task.FromResult(true));

            var Result = (ObjectResult)await this.eventController.GetEventAsync(EventWorkflowHelperData.eventEntity.EventId, EventWorkflowHelperData.eventEntity.TeamId);

            Assert.AreEqual(Result.StatusCode, StatusCodes.Status200OK);
        }
        
        [TestMethod]
        public async Task GetEventsAsync_ReturnsOkResult()
        {
            this.userEventSearchService
                .Setup(u => u.GetEventsAsync(new SearchParametersDto()))
                .Returns(Task.FromResult(EventWorkflowHelperData.eventEntities as IEnumerable<EventEntity>));
            
            this.categoryHelper
                .Setup(e => e.BindCategoryNameAsync(It.IsAny<IEnumerable<EventEntity>>()))
                .Returns(Task.FromResult(true));

            var Result = (ObjectResult)await this.eventController.GetEventsAsync("random", 1, (int)EventSearchType.AllPublicPrivateEventsForUser, "random", "random", (int)SortBy.PopularityByRecentCollaborators);

            Assert.AreEqual(Result.StatusCode, StatusCodes.Status200OK);
        }
        
        [TestMethod]
        public async Task RegisterToEventAsync_ReturnsOkResult()
        {
            this.userEventsHelper
                .Setup(u => u.RegisterToEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(true));

            var Result = (ObjectResult)await this.eventController.RegisterToEventAsync(EventWorkflowHelperData.eventEntity.TeamId, EventWorkflowHelperData.eventEntity.EventId);

            Assert.AreEqual(Result.StatusCode, StatusCodes.Status200OK);
        }
        
        [TestMethod]
        public async Task UnregisterToEventAsync_ReturnsOkResult()
        {
            this.userEventsHelper
                .Setup(u => u.UnregisterFromEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(true));

            var Result = (ObjectResult)await this.eventController.UnregisterToEventAsync(EventWorkflowHelperData.eventEntity.TeamId, EventWorkflowHelperData.eventEntity.EventId);

            Assert.AreEqual(Result.StatusCode, StatusCodes.Status200OK);
        }
        
        [TestMethod]
        public async Task SearchEventAsync_ReturnsOkResult()
        {
            this.userEventSearchService
                .Setup(u => u.GetEventsAsync(new SearchParametersDto()))
                .Returns(Task.FromResult(EventWorkflowHelperData.eventEntities as IEnumerable<EventEntity>));

            var Result = (ObjectResult)await this.eventController.SearchEventAsync("random");

            Assert.AreEqual(Result.StatusCode, StatusCodes.Status200OK);
        }

        [TestMethod]
        public async Task AllMethods_InvalidArguments_ReturnsBadRequest()
        {
            var getEventAsyncResult = (ObjectResult)await this.eventController.GetEventAsync(EventWorkflowHelperData.eventEntity.EventId, string.Empty);
            var getEventsAsyncInvalidSortByResult = (ObjectResult)await this.eventController.GetEventsAsync("random", 1, (int)EventSearchType.AllPublicPrivateEventsForUser, "random", "random", 4);
            var getEventsAsyncInvalidEventSearchTypeResult = (ObjectResult)await this.eventController.GetEventsAsync("random", 1, 11, "random", "random", (int)SortBy.PopularityByRecentCollaborators);
            var registerToEventAsyncResult = (ObjectResult)await this.eventController.RegisterToEventAsync(string.Empty, EventWorkflowHelperData.eventEntity.EventId);
            var unregisterToEventAsyncResult = (ObjectResult)await this.eventController.UnregisterToEventAsync(EventWorkflowHelperData.eventEntity.TeamId, string.Empty);
            var searchEventAsyncResult = (ObjectResult)await this.eventController.SearchEventAsync(string.Empty);

            Assert.AreEqual(getEventAsyncResult.StatusCode, StatusCodes.Status400BadRequest);
            Assert.AreEqual(getEventsAsyncInvalidSortByResult.StatusCode, StatusCodes.Status400BadRequest);
            Assert.AreEqual(getEventsAsyncInvalidEventSearchTypeResult.StatusCode, StatusCodes.Status400BadRequest);
            Assert.AreEqual(registerToEventAsyncResult.StatusCode, StatusCodes.Status400BadRequest);
            Assert.AreEqual(unregisterToEventAsyncResult.StatusCode, StatusCodes.Status400BadRequest);
            Assert.AreEqual(searchEventAsyncResult.StatusCode, StatusCodes.Status400BadRequest);
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
