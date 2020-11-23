// <copyright file="UsersControllerTest.cs" company="Microsoft">
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
    using Microsoft.Teams.Apps.EmployeeTraining.Tests.TestData;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Threading.Tasks;

    [TestClass]
    public class GroupControllerTest
    {
        GroupController groupController;
        Mock<IGroupGraphHelper> groupGraphHelper;
        TelemetryClient telemetryClient;

        [TestInitialize]
        public void GroupControllerTestSetup()
        {
            var logger = new Mock<ILogger<GroupController>>().Object;
            telemetryClient = new TelemetryClient(new TelemetryConfiguration());
            groupGraphHelper = new Mock<IGroupGraphHelper>();
            groupController = new GroupController(
                logger,
                telemetryClient,
                groupGraphHelper.Object);

            var httpContext = MakeFakeContext();
            groupController.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }
        
        [TestMethod]
        public async Task GetGroupMembers_ReturnsOkResult()
        {
            this.groupGraphHelper
                .Setup(g => g.GetGroupMembersAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(EventWorkflowHelperData.graphGroupDirectoryObject as IEnumerable<Graph.DirectoryObject>));

            var Result = (ObjectResult)await this.groupController.GetMembersAsync("random");

            Assert.AreEqual(Result.StatusCode, StatusCodes.Status200OK);
        }

        [TestMethod]
        public async Task AllMethods_InvalidArguments_ReturnsBadRequest()
        {
            var getGroupMembersResult = (ObjectResult)await this.groupController.GetMembersAsync(string.Empty);
            Assert.AreEqual(getGroupMembersResult.StatusCode, StatusCodes.Status400BadRequest);
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
