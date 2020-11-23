// <copyright file="EventWorkflowControllerTest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Test.Controllers
{
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.EmployeeTraining.Controllers;
    using Microsoft.Teams.Apps.EmployeeTraining.Helpers;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;
    using Microsoft.Teams.Apps.EmployeeTraining.Services;
    using Microsoft.Teams.Apps.EmployeeTraining.Tests.TestData;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Threading.Tasks;

    /// <summary>
    /// Exposes APIs related to event operations.
    /// </summary>
    [TestClass]
    public class EventWorkflowControllerTest
    {
        EventWorkflowController eventWorkflowController;
        Mock<IEventWorkflowHelper> eventWorkflowHelper;
        Mock<ICategoryHelper> categoryHelper;
        Mock<ITeamEventSearchService> teamEventSearchService;
        TelemetryClient telemetryClient;

        private class ValidationMessage 
        {
           public List<string> errors = new List<string>();
        };

        [TestInitialize]
        public void EventWorkflowControllerTestSetup()
        {
            var localizerMock = new Mock<IStringLocalizer<Strings>>();
            var logger = new Mock<ILogger<EventController>>().Object;
            eventWorkflowHelper = new Mock<IEventWorkflowHelper>();
            categoryHelper = new Mock<ICategoryHelper>();
            teamEventSearchService = new Mock<ITeamEventSearchService>();
            telemetryClient = new TelemetryClient(new TelemetryConfiguration());

            eventWorkflowController = new EventWorkflowController(
                logger,
                telemetryClient,
                eventWorkflowHelper.Object,
                teamEventSearchService.Object,
                categoryHelper.Object,
                localizerMock.Object);

            var httpContext = MakeFakeContext();
            eventWorkflowController.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [TestMethod]
        public async Task CreateDraftAsync_ReturnsOkResult()
        {
            this.eventWorkflowHelper
                .Setup(x => x.CreateDraftEventAsync(It.IsAny<EventEntity>()))
                .Returns(Task.FromResult(true));

            var Result = (ObjectResult)await this.eventWorkflowController.CreateDraftAsync(EventWorkflowHelperData.eventEntity, EventWorkflowHelperData.eventEntity.TeamId);
            Assert.AreEqual(Result.StatusCode, StatusCodes.Status200OK);

        }
        
        [TestMethod]
        public async Task UpdateDraftAsync_ReturnsOkResult()
        {
            bool? workHelperResult = true;
            this.eventWorkflowHelper
                .Setup(x => x.UpdateDraftEventAsync(It.IsAny<EventEntity>()))
                .Returns(Task.FromResult(workHelperResult));

            var Result = (ObjectResult)await this.eventWorkflowController.UpdateDraftAsync(EventWorkflowHelperData.eventEntity, EventWorkflowHelperData.eventEntity.TeamId);
            Assert.AreEqual(Result.StatusCode, StatusCodes.Status200OK);
        }
        
        [TestMethod]
        public async Task CreateEventAsync_ReturnsOkResult()
        {
            bool? workHelperResult = true;
            this.eventWorkflowHelper
                .Setup(x => x.CreateNewEventAsync(It.IsAny<EventEntity>(), It.IsAny<string>()))
                .Returns(Task.FromResult(workHelperResult));

            var Result = (ObjectResult)await this.eventWorkflowController.CreateEventAsync(EventWorkflowHelperData.validEventEntity, EventWorkflowHelperData.validEventEntity.TeamId);
            Assert.AreEqual(Result.StatusCode, StatusCodes.Status200OK);
        }

        [TestMethod]
        public async Task CreateEventAsync_InvalidEntiiy_ReturnsBadRequest()
        {
            bool? workHelperResult = true;
            this.eventWorkflowHelper
                .Setup(x => x.CreateNewEventAsync(It.IsAny<EventEntity>(), It.IsAny<string>()))
                .Returns(Task.FromResult(workHelperResult));

            var Result = (ObjectResult)await this.eventWorkflowController.CreateEventAsync(EventWorkflowHelperData.eventEntity, EventWorkflowHelperData.validEventEntity.TeamId);
            var validationMessages = JsonConvert.DeserializeObject<ValidationMessage>(JObject.FromObject(Result.Value).ToString());
            
            Assert.AreEqual(Result.StatusCode, StatusCodes.Status400BadRequest);
            Assert.AreEqual(validationMessages.errors.Contains("Event start date must be future date."),true);
            Assert.AreEqual(validationMessages.errors.Contains("Invalid event type value. Event type should be in-between 1 to 3"),true);
            Assert.AreEqual(validationMessages.errors.Contains("Invalid Audience value. It should be either 1 or 2"),true);
        }
        
        [TestMethod]
        public async Task UpdateAsync_InvalidEntiiy_ReturnsBadRequest()
        {
            bool? workHelperResult = true;
            this.eventWorkflowHelper
                .Setup(x => x.UpdateEventAsync(It.IsAny<EventEntity>()))
                .Returns(Task.FromResult(workHelperResult));

            var Result = (ObjectResult)await this.eventWorkflowController.UpdateAsync(EventWorkflowHelperData.eventEntity, EventWorkflowHelperData.validEventEntity.TeamId);
            var validationMessages = JsonConvert.DeserializeObject<ValidationMessage>(JObject.FromObject(Result.Value).ToString());
            
            Assert.AreEqual(Result.StatusCode, StatusCodes.Status400BadRequest);
            Assert.AreEqual(validationMessages.errors.Contains("Invalid event type value. Event type should be in-between 1 to 3"),true);
            Assert.AreEqual(validationMessages.errors.Contains("Invalid Audience value. It should be either 1 or 2"),true);
        }
        
        [TestMethod]
        public async Task UpdateAsync_ReturnsOkResult()
        {
            bool? workHelperResult = true;
            this.eventWorkflowHelper
                .Setup(x => x.UpdateEventAsync(It.IsAny<EventEntity>()))
                .Returns(Task.FromResult(workHelperResult));

            var Result = (ObjectResult)await this.eventWorkflowController.UpdateAsync(EventWorkflowHelperData.validEventEntity, EventWorkflowHelperData.validEventEntity.TeamId);
            Assert.AreEqual(Result.StatusCode, StatusCodes.Status200OK);
        }

        [TestMethod]
        public async Task CloseEventRegistrationsAsync_ReturnsOkResult()
        {
            this.eventWorkflowHelper
                .Setup(x => x.CloseEventRegistrationsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(true));

            var Result = (ObjectResult)await this.eventWorkflowController.CloseEventRegistrationsAsync(EventWorkflowHelperData.validEventEntity.TeamId, EventWorkflowHelperData.validEventEntity.EventId);
            Assert.AreEqual(Result.StatusCode, StatusCodes.Status200OK);
        }

        [TestMethod]
        public async Task CancelEventAsync_ReturnsOkResult()
        {
            this.eventWorkflowHelper
                .Setup(x => x.UpdateEventStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<EventStatus>(), It.IsAny<string>()))
                .Returns(Task.FromResult(true));

            var Result = (ObjectResult)await this.eventWorkflowController.CancelEventAsync(EventWorkflowHelperData.validEventEntity.TeamId, EventWorkflowHelperData.validEventEntity.EventId);
            Assert.AreEqual(Result.StatusCode, StatusCodes.Status200OK);
        }

        [TestMethod]
        public async Task SendReminder_ReturnsOkResult()
        {
            this.eventWorkflowHelper
                .Setup(x => x.SendReminderAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new List<string>()));

            var Result = (ObjectResult)await this.eventWorkflowController.CancelEventAsync(EventWorkflowHelperData.validEventEntity.TeamId, EventWorkflowHelperData.validEventEntity.EventId);
            Assert.AreEqual(Result.StatusCode, StatusCodes.Status200OK);
        }

        [TestMethod]
        public async Task DeleteDraftAsync_ReturnsOkResult()
        {
            bool? workHelperResult = true;
            this.eventWorkflowHelper
                .Setup(x => x.DeleteDraftEventAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(workHelperResult));

            var Result = (ObjectResult)await this.eventWorkflowController.DeleteDraftAsync(EventWorkflowHelperData.eventEntity.TeamId, EventWorkflowHelperData.eventEntity.EventId);
            Assert.AreEqual(Result.StatusCode, StatusCodes.Status200OK);
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
