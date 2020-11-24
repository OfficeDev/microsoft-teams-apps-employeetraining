// <copyright file="ReminderCardTest.cs" company="Microsoft">
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
    using System.Collections.Generic;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;

    [TestClass]
    public class ReminderCardTest
    {
        Mock<IStringLocalizer<Strings>> localizer;

        [TestInitialize]
        public void ReminderCardTestSetup()
        {
            localizer = new Mock<IStringLocalizer<Strings>>();
        }

        [TestMethod]
        public void GetCard()
        {
            var Results = ReminderCard.GetCard( new List<EventEntity> { EventWorkflowHelperData.validEventEntity }, localizer.Object, "random", NotificationType.Manual);

            Assert.AreEqual(Results.ContentType, AdaptiveCard.ContentType);
        }

    }
}
