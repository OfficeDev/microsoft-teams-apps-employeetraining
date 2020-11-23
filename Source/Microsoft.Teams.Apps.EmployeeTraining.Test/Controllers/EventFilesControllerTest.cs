// <copyright file="EventFilesControllerTest.cs" company="Microsoft">
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
    using Microsoft.Teams.Apps.EmployeeTraining.Repositories;
    using Microsoft.Teams.Apps.EmployeeTraining.Tests.TestData;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System.IO;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Threading.Tasks;

    /// <summary>
    /// The controller handles the data requests related to categories
    /// </summary>
    [TestClass]
    public class EventFilesControllerTest
    {
        EventFilesController eventFilesController;
        Mock<IEventWorkflowHelper> eventWorkFlowHelper;
        Mock<IBlobRepository> blobStoragePrvider;
        TelemetryClient telemetryClient;

        [TestInitialize]
        public void EventFilesControllerTestSetup()
        {
            var logger = new Mock<ILogger<EventFilesController>>().Object;
            telemetryClient = new TelemetryClient(new TelemetryConfiguration());
            blobStoragePrvider = new Mock<IBlobRepository>();
            eventWorkFlowHelper = new Mock<IEventWorkflowHelper>();

            eventFilesController = new EventFilesController(
                logger,
                telemetryClient,
                blobStoragePrvider.Object,
                eventWorkFlowHelper.Object);

            var httpContext = MakeFakeContext();
            eventFilesController.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [TestMethod]
        public async Task UploadImageAsync_ReturnsOkResult()
        {

            this.blobStoragePrvider
                .Setup(b => b.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>()))
                .Returns(Task.FromResult("blobUrl"));

            var Result = (ObjectResult)await this.eventFilesController.UploadImageAsync( EventWorkflowHelperData.fileInfo, EventWorkflowHelperData.validEventEntity.TeamId);

            Assert.AreEqual(Result.StatusCode, StatusCodes.Status200OK);
        }

        [TestMethod]
        public async Task ExportEventDetailsToCSV_ReturnsOkResult()
        {

            this.eventWorkFlowHelper
                .Setup(e => e.ExportEventDetailsToCSVAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new MemoryStream() as Stream));

            var Result = (FileStreamResult)await this.eventFilesController.ExportEventDetailsToCSV(EventWorkflowHelperData.validEventEntity.TeamId, EventWorkflowHelperData.validEventEntity.EventId);

            Assert.AreEqual(Result.ContentType == "text/csv", true);
        }

        [TestMethod]
        public async Task AllMethods_InvalidArguments_ReturnsBadRequest()
        {
            var exportEventDetailsToCSVResult = (ObjectResult)await this.eventFilesController.ExportEventDetailsToCSV("", "");
            var uploadImageAsyncResult = (ObjectResult)await this.eventFilesController.UploadImageAsync(null, EventWorkflowHelperData.validEventEntity.TeamId);

            Assert.AreEqual(exportEventDetailsToCSVResult.StatusCode, StatusCodes.Status400BadRequest);
            Assert.AreEqual(uploadImageAsyncResult.StatusCode, StatusCodes.Status400BadRequest);
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
