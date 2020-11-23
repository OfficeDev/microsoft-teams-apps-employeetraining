// <copyright file="AuthenticationMetadataControllerTest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Test.Controllers
{
    using System.Security.Claims;
    using System.Security.Principal;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Teams.Apps.EmployeeTraining.Controllers;
    using Microsoft.Teams.Apps.EmployeeTraining.Tests.TestData;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Controller to handle authentication API operations.
    /// </summary>
    [TestClass]
    public class AuthenticationMetadataControllerTest
    {
        AuthenticationMetadataController controller;

        [TestInitialize]
        public void AuthenticationMetadataControllerTestSetup()
        {
            controller = new AuthenticationMetadataController(EventWorkflowHelperData.azureSettings, EventWorkflowHelperData.botOptions);

            var httpContext = MakeFakeContext();
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = httpContext;
        }

        [TestMethod]
        public void ConsentUrl_NotNull_Success()
        {
            var okResult = this.controller.GetConsentUrl("Test", "Test");
            Assert.IsNotNull(okResult);
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
