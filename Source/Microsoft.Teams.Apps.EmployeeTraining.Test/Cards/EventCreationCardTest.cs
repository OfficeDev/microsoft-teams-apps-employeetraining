// <copyright file="EventCreationCardTest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Test.Cards
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Microsoft.Teams.Apps.EmployeeTraining.Tests.TestData;
    using Microsoft.Teams.Apps.EmployeeTraining.Cards;
    using Microsoft.Extensions.Localization;
    using AdaptiveCards;

    [TestClass]
    public class EventCreationCardTest
    {
        Mock<IStringLocalizer<Strings>> localizer;

        [TestInitialize]
        public void EventCreationCardTestSetup()
        {
            localizer = new Mock<IStringLocalizer<Strings>>();
        }

        [TestMethod]
        public void GetEventCreationCardForTeam()
        {
            var Results = EventDetailsCard.GetEventCreationCardForTeam("https://www.example.com", localizer.Object, EventWorkflowHelperData.validEventEntity, "random");

            Assert.AreEqual(Results.ContentType, AdaptiveCard.ContentType);
        }

    }
}
