// <copyright file="LnDTeamController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Test.Controllers
{
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Bot.Schema.Teams;
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
    public class LnDTeamControllerTest
    {
        LnDTeamController lnDTeamController;
        Mock<ITeamInfoHelper> teamInfoHelper;
        TelemetryClient telemetryClient;

        [TestInitialize]
        public void LnDTeamControllerTestSetup()
        {
            var logger = new Mock<ILogger<CategoryController>>().Object;
            telemetryClient = new TelemetryClient(new TelemetryConfiguration());
            teamInfoHelper = new Mock<ITeamInfoHelper>();

            lnDTeamController = new LnDTeamController(
                logger,
                telemetryClient,
                teamInfoHelper.Object);

            var httpContext = MakeFakeContext();
            lnDTeamController.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [TestMethod]
        public async Task GetAllLnDTeamMembersAsync_ReturnsOkResult()
        {
            this.teamInfoHelper
                .Setup(t => t.GetAllLnDTeamMembersAsync())
                .Returns(Task.FromResult(EventWorkflowHelperData.teamsChannelAccount));

            var Result = (ObjectResult)await this.lnDTeamController.GetAllLnDTeamMembersAsync();

            Assert.AreEqual(Result.StatusCode, StatusCodes.Status200OK);
        }

        [TestMethod]
        public async Task GetAllLnDTeamMembersAsync_ReturnsNoContent()
        {
            List<TeamsChannelAccount> nullTeamChannelAccount = null;
            this.teamInfoHelper
                .Setup(t => t.GetAllLnDTeamMembersAsync())
                .Returns(Task.FromResult(nullTeamChannelAccount));

            var Result = (NoContentResult)await this.lnDTeamController.GetAllLnDTeamMembersAsync();

            Assert.AreEqual(Result.StatusCode, StatusCodes.Status204NoContent);
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
