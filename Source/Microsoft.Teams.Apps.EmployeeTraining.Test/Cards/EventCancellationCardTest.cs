// <copyright file="EventCancellationCardTest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Test.Cards
{
    using Moq;
    using Microsoft.Teams.Apps.EmployeeTraining.Cards;
    using Microsoft.Extensions.Localization;
    using AdaptiveCards;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Teams.Apps.EmployeeTraining.Tests.TestData;

    [TestClass]
    public class EventCancellationCardTest
    {
        Mock<IStringLocalizer<Strings>> localizer;

        [TestInitialize]
        public void EventCancellationCardTestSetup()
        {
            localizer = new Mock<IStringLocalizer<Strings>>();
        }

        [TestMethod]
        public void GetCard()
        {
            var Results = EventCancellationCard.GetCancellationCard(localizer.Object, EventWorkflowHelperData.validEventEntity, "random");

            Assert.AreEqual(Results.ContentType, AdaptiveCard.ContentType);
        }

    }
}
