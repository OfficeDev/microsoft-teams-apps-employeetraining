// <copyright file="UserEventsTest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Tests.Helpers
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Microsoft.Teams.Apps.EmployeeTraining.Helpers;
    using Microsoft.Teams.Apps.EmployeeTraining.Tests.TestData;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.EmployeeTraining.Tests.Providers;
    using Microsoft.Extensions.Localization;
    using Microsoft.Teams.Apps.EmployeeTraining.Repositories;
    using Microsoft.Teams.Apps.EmployeeTraining.Services;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;
    using System;

    [TestClass]
    public class UserEventsHelperTest
    {
        Mock<IEventRepository> eventStorageProvider;
        Mock<IEventSearchService> eventSearchServiceProvider;
        Mock<IUserEventSearchService> userEventSearchServiceHelper;
        Mock<IUserGraphHelper> userGraphHelper;
        Mock<IEventGraphHelper> eventGraphHelper;
        UserEventsHelper userEventsHelper;
        EventStorageProviderFake eventStorageProviderFake;
        Mock<ICategoryHelper> categoryHelper;
        Mock<INotificationHelper> notificationHelper;
        LnDTeamConfigurationStorageProviderFake lnDTeamConfigurationStorageProviderFake;
        Mock<IOptions<BotSettings>> botOptions;

        [TestInitialize]
        public void UserEventsHelperTestSetup()
        {
            var mock = new Mock<IStringLocalizer<Strings>>();
            string key = "Hello my dear friend!";
            var localizedString = new LocalizedString(key, key);
            mock.Setup(_ => _[key]).Returns(localizedString);
            var localizer = mock.Object;

            eventGraphHelper = new Mock<IEventGraphHelper>();
            userGraphHelper = new Mock<IUserGraphHelper>();
            eventStorageProvider = new Mock<IEventRepository>();
            eventSearchServiceProvider = new Mock<IEventSearchService>();
            categoryHelper = new Mock<ICategoryHelper>();
            notificationHelper = new Mock<INotificationHelper>();
            eventStorageProviderFake = new EventStorageProviderFake();
            lnDTeamConfigurationStorageProviderFake = new LnDTeamConfigurationStorageProviderFake();
            userEventSearchServiceHelper = new Mock<IUserEventSearchService>();
            botOptions = new Mock<IOptions<BotSettings>>();

            userEventsHelper = new UserEventsHelper(
                eventStorageProvider.Object,
                eventSearchServiceProvider.Object,
                userEventSearchServiceHelper.Object,
                userGraphHelper.Object,
                eventGraphHelper.Object,
                notificationHelper.Object,
                categoryHelper.Object,
                lnDTeamConfigurationStorageProviderFake,
                botOptions.Object,
                localizer);
        }

        [TestMethod]
        public async Task GetEventAsync()
        {
            var eventToFetch = EventWorkflowHelperData.eventEntity;
            this.eventStorageProvider
                .Setup(x => x.GetEventDetailsAsync(eventToFetch.EventId, eventToFetch.TeamId))
                .Returns(this.eventStorageProviderFake.GetEventDetailsAsync(eventToFetch.EventId, eventToFetch.TeamId));
            
            // Getting event details for AutoRegistered user.
            var Result = await this.userEventsHelper.GetEventAsync(eventToFetch.EventId, eventToFetch.TeamId, "a85c1ff9-7381-4721-bb7b-c8d9203d202c");

            Assert.AreEqual(Result.IsMandatoryForLoggedInUser && Result.IsLoggedInUserRegistered, true);
        }

        [TestMethod]
        public async Task RemoveEventFailAsync()
        {
            var eventToFetch = EventWorkflowHelperData.eventEntity;

            this.eventStorageProvider
                .Setup(x => x.GetEventDetailsAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(this.eventStorageProviderFake.GetEventDetailsAsync("activeEventId", eventToFetch.TeamId));

            this.eventGraphHelper
                .Setup(x => x.UpdateEventAsync(It.IsAny<EventEntity>()))
                .Returns(Task.FromResult(EventWorkflowHelperData.teamEvent));

            this.eventStorageProvider
                .Setup(x => x.UpsertEventAsync(It.IsAny<EventEntity>()))
                .Returns(this.eventStorageProviderFake.UpsertEventAsync(EventWorkflowHelperData.eventEntity));

            this.eventSearchServiceProvider
                .Setup(x => x.RunIndexerOnDemandAsync())
                .Returns(Task.FromResult(true));

            var userAADObjectIdToRegeister = new Guid().ToString();

            // Removing unregistered user.
            var Result = await this.userEventsHelper.UnregisterFromEventAsync(eventToFetch.EventId, eventToFetch.TeamId, userAADObjectIdToRegeister);

            Assert.AreEqual(Result, false);
        }
    }
}


