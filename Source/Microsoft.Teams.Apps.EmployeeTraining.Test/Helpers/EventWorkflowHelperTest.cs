// <copyright file="EventWorkFlowHelperTest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Tests.Helpers
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections.Generic;
    using Moq;
    using Microsoft.Teams.Apps.EmployeeTraining.Helpers;
    using Microsoft.Extensions.Localization;
    using Microsoft.Teams.Apps.EmployeeTraining.Tests.TestData;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.EmployeeTraining.Tests.Providers;
    using System.Linq;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;
    using BotSchema = Microsoft.Bot.Schema;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.EmployeeTraining.Repositories;
    using Microsoft.Teams.Apps.EmployeeTraining.Services;

    [TestClass]
    public class EventWorkflowHelperTest
    {
        Mock<IEventRepository> eventStorageProvider;
        Mock<IEventSearchService> eventSearchServiceProvider;
        Mock<IEventGraphHelper> eventGraphHelper;
        Mock<IGroupGraphHelper> groupGraphHelper;
        Mock<IUserGraphHelper> userGraphHelper;
        Mock<INotificationHelper> notificationHelper;
        Mock<IUserConfigurationRepository> userStorageConfigurationProvider;
        Mock<ILnDTeamConfigurationRepository> lnDTeamConfigurationStorageProvider;
        Mock<ICategoryHelper> categoryHelper;
        EventWorkflowHelper eventWorkflowHelper;
        EventStorageProviderFake eventStorageProviderFake;
        UserConfigurationStorageProviderFake userConfigurationStorageProviderFake;
        Mock<IOptions<BotSettings>> mockBotSettings;

        [TestInitialize]
        public void EventWorkflowHelperTestSetup()
        {
            var localizer = new Mock<IStringLocalizer<Strings>>().Object;
            eventGraphHelper = new Mock<IEventGraphHelper>();
            userGraphHelper = new Mock<IUserGraphHelper>();
            groupGraphHelper = new Mock<IGroupGraphHelper>();
            notificationHelper = new Mock<INotificationHelper>();
            eventStorageProvider = new Mock<IEventRepository>();
            eventSearchServiceProvider = new Mock<IEventSearchService>();
            eventStorageProviderFake = new EventStorageProviderFake();
            userStorageConfigurationProvider = new Mock<IUserConfigurationRepository>();
            userConfigurationStorageProviderFake = new UserConfigurationStorageProviderFake();
            lnDTeamConfigurationStorageProvider = new Mock<ILnDTeamConfigurationRepository>();
            categoryHelper = new Mock<ICategoryHelper>();
            mockBotSettings = new Mock<IOptions<BotSettings>>();

            eventWorkflowHelper = new EventWorkflowHelper(
                eventStorageProvider.Object,
                eventSearchServiceProvider.Object,
                eventGraphHelper.Object,
                groupGraphHelper.Object,
                userStorageConfigurationProvider.Object,
                lnDTeamConfigurationStorageProvider.Object,
                categoryHelper.Object,
                localizer,
                userGraphHelper.Object,
                notificationHelper.Object,
                EventWorkflowHelperData.botOptions);
        }

        [TestMethod]
        public async Task UpdateDraftEventAsync()
        {
            var eventToUpdate = EventWorkflowHelperData.eventEntity;
            
            this.eventStorageProvider
                .Setup(x => x.GetEventDetailsAsync(eventToUpdate.EventId, eventToUpdate.TeamId))
                .Returns(this.eventStorageProviderFake.GetEventDetailsAsync(eventToUpdate.EventId, eventToUpdate.TeamId));
            
            this.eventStorageProvider
                .Setup(x => x.UpsertEventAsync(It.IsAny<EventEntity>()))
                .Returns(this.eventStorageProviderFake.UpsertEventAsync(EventWorkflowHelperData.eventEntity));

            this.eventSearchServiceProvider
                .Setup(x => x.RunIndexerOnDemandAsync())
                .Returns(Task.FromResult(true));

            var result = await this.eventWorkflowHelper.UpdateDraftEventAsync(EventWorkflowHelperData.eventEntity);
            Assert.AreEqual(result, true);
        }

        [TestMethod]
        public async Task DeleteDraftEventAsync()
        {
            var eventToDelete = EventWorkflowHelperData.eventEntity;

            this.eventStorageProvider
                .Setup(x => x.GetEventDetailsAsync(eventToDelete.EventId, eventToDelete.TeamId))
                .Returns(this.eventStorageProviderFake.GetEventDetailsAsync(eventToDelete.EventId, eventToDelete.TeamId));

            this.eventStorageProvider
                .Setup(x => x.UpsertEventAsync(It.IsAny<EventEntity>()))
                .Returns(this.eventStorageProviderFake.UpsertEventAsync(EventWorkflowHelperData.eventEntity));

            this.eventSearchServiceProvider
                .Setup(x => x.RunIndexerOnDemandAsync())
                .Returns(Task.FromResult(true));

            var result = await this.eventWorkflowHelper.DeleteDraftEventAsync(eventToDelete.TeamId, eventToDelete.EventId);
            Assert.AreEqual(result, true);
        }

        [TestMethod]
        public async Task UpdateEventAsync()
        {
            var eventToUpdate = EventWorkflowHelperData.eventEntity;

            this.eventStorageProvider
                .Setup(x => x.GetEventDetailsAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(this.eventStorageProviderFake.GetEventDetailsAsync("activeEventId", eventToUpdate.TeamId));

            this.eventGraphHelper
                .Setup(x => x.UpdateEventAsync(It.IsAny<EventEntity>()))
                .Returns(Task.FromResult(EventWorkflowHelperData.teamEvent));

            this.eventStorageProvider
                .Setup(x => x.UpsertEventAsync(It.IsAny<EventEntity>()))
                .Returns(this.eventStorageProviderFake.UpsertEventAsync(EventWorkflowHelperData.eventEntity));

            this.eventSearchServiceProvider
                .Setup(x => x.RunIndexerOnDemandAsync())
                .Returns(Task.FromResult(true));

            var result = await this.eventWorkflowHelper.UpdateEventAsync(EventWorkflowHelperData.eventEntity);
            Assert.AreEqual(result, true);
        }

        [TestMethod]
        public async Task CloseEventRegistrations()
        {
            var eventToCloseRegistration = EventWorkflowHelperData.eventEntity;

            this.eventStorageProvider
                .Setup(x => x.GetEventDetailsAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(this.eventStorageProviderFake.GetEventDetailsAsync(eventToCloseRegistration.EventId, eventToCloseRegistration.TeamId));

            this.eventStorageProvider
                .Setup(x => x.UpsertEventAsync(It.IsAny<EventEntity>()))
                .Returns(this.eventStorageProviderFake.UpsertEventAsync(EventWorkflowHelperData.eventEntity));

            this.eventSearchServiceProvider
                .Setup(x => x.RunIndexerOnDemandAsync())
                .Returns(Task.FromResult(true));

            var result = await this.eventWorkflowHelper.CloseEventRegistrationsAsync(eventToCloseRegistration.TeamId, eventToCloseRegistration.EventId, "8781d219-3920-4b4a-b280-48a17d2f23a6");
            Assert.AreEqual(result, false);
        }
        
        [TestMethod]
        public async Task CloseEventRegistrationsFail()
        {
            var eventToCloseRegistration = EventWorkflowHelperData.eventEntity;

            this.eventStorageProvider
                .Setup(x => x.GetEventDetailsAsync(eventToCloseRegistration.EventId, eventToCloseRegistration.TeamId))
                .Returns(this.eventStorageProviderFake.GetEventDetailsAsync("activeEventId", eventToCloseRegistration.TeamId));

            this.eventStorageProvider
                .Setup(x => x.UpsertEventAsync(It.IsAny<EventEntity>()))
                .Returns(this.eventStorageProviderFake.UpsertEventAsync(EventWorkflowHelperData.eventEntity));

            this.eventSearchServiceProvider
                .Setup(x => x.RunIndexerOnDemandAsync())
                .Returns(Task.FromResult(true));

            var result = await this.eventWorkflowHelper.CloseEventRegistrationsAsync(eventToCloseRegistration.TeamId, eventToCloseRegistration.EventId, "8781d219-3920-4b4a-b280-48a17d2f23a6");
            Assert.AreEqual(result, true);
        }

        [TestMethod]
        public async Task UpdateEventStatus()
        {
            var eventToCloseRegistration = EventWorkflowHelperData.eventEntity;

            this.eventStorageProvider
                .Setup(x => x.GetEventDetailsAsync(eventToCloseRegistration.EventId, eventToCloseRegistration.TeamId))
                .Returns(this.eventStorageProviderFake.GetEventDetailsAsync(eventToCloseRegistration.EventId, eventToCloseRegistration.TeamId));

            this.eventGraphHelper
                .Setup(x => x.CancelEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(true));

            this.eventStorageProvider
                .Setup(x => x.UpsertEventAsync(It.IsAny<EventEntity>()))
                .Returns(this.eventStorageProviderFake.UpsertEventAsync(EventWorkflowHelperData.eventEntity));

            this.eventSearchServiceProvider
                .Setup(x => x.RunIndexerOnDemandAsync())
                .Returns(Task.FromResult(true));

            var result = await this.eventWorkflowHelper.UpdateEventStatusAsync(eventToCloseRegistration.TeamId, eventToCloseRegistration.EventId, (EventStatus)2, "8781d219-3920-4b4a-b280-48a17d2f23a6");
            Assert.AreEqual(result, true);
        }
        
        [TestMethod]
        public async Task UpdateEventStatusFail()
        {
            var eventToCloseRegistration = EventWorkflowHelperData.eventEntity;

            this.eventStorageProvider
                .Setup(x => x.GetEventDetailsAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(this.eventStorageProviderFake.GetEventDetailsAsync("completedEventId", eventToCloseRegistration.TeamId));

            this.eventGraphHelper
                .Setup(x => x.CancelEventAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(true));

            this.eventStorageProvider
                .Setup(x => x.UpsertEventAsync(It.IsAny<EventEntity>()))
                .Returns(this.eventStorageProviderFake.UpsertEventAsync(EventWorkflowHelperData.eventEntity));

            this.eventSearchServiceProvider
                .Setup(x => x.RunIndexerOnDemandAsync())
                .Returns(Task.FromResult(true));

            var result = await this.eventWorkflowHelper.UpdateEventStatusAsync(eventToCloseRegistration.TeamId, eventToCloseRegistration.EventId, (EventStatus)2, "8781d219-3920-4b4a-b280-48a17d2f23a6");
            Assert.AreEqual(result, false);
        }

        [TestMethod]
        public async Task ExportEventDetailsToCSV()
        {
            var eventToExport = EventWorkflowHelperData.eventEntity;

            this.eventStorageProvider
                .Setup(x => x.GetEventDetailsAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(this.eventStorageProviderFake.GetEventDetailsAsync(eventToExport.EventId, eventToExport.TeamId));

            this.userGraphHelper
                .Setup(x => x.GetUsersAsync(It.IsAny<List<string>>()))
                .Returns(Task.FromResult(EventWorkflowHelperData.graphUsers as IEnumerable<Graph.User>));

            this.eventStorageProvider
                .Setup(x => x.UpsertEventAsync(It.IsAny<EventEntity>()))
                .Returns(this.eventStorageProviderFake.UpsertEventAsync(EventWorkflowHelperData.eventEntity));

            var result = await this.eventWorkflowHelper.ExportEventDetailsToCSVAsync(eventToExport.TeamId, eventToExport.EventId);
            Assert.AreEqual(result.Length > 0, true);
        }

        [TestMethod]
        public async Task CreateDraftEventAsync()
        {
            this.eventStorageProvider
                .Setup(x => x.UpsertEventAsync(It.IsAny<EventEntity>()))
                .Returns(this.eventStorageProviderFake.UpsertEventAsync(EventWorkflowHelperData.eventEntity));

            var Result = await this.eventWorkflowHelper.CreateDraftEventAsync(EventWorkflowHelperData.eventEntity);

            Assert.AreEqual(Result, true);
        }

        [TestMethod]
        public async Task CreateNewEventAsync()
        {
            this.eventStorageProvider
               .Setup(x => x.GetEventDetailsAsync(It.IsAny<string>(), It.IsAny<string>()))
               .Returns(this.eventStorageProviderFake.GetEventDetailsAsync(It.IsAny<string>(), It.IsAny<string>()));

            this.eventGraphHelper
                .Setup(x => x.CreateEventAsync(It.IsAny<EventEntity>()))
                .Returns(Task.FromResult(EventWorkflowHelperData.teamEvent));

            this.eventStorageProvider
                .Setup(x => x.UpsertEventAsync(It.IsAny<EventEntity>()))
                .Returns(this.eventStorageProviderFake.UpsertEventAsync(EventWorkflowHelperData.eventEntity));

            var events = EventWorkflowHelperData.eventEntities;
            var eventCategoryIds = events.Select(eventDetails => eventDetails?.CategoryId).ToArray();

            this.lnDTeamConfigurationStorageProvider
                .Setup(x => x.GetTeamDetailsAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(EventWorkflowHelperData.lndTeam));

            this.mockBotSettings
                .Setup(x => x.Value)
                .Returns(EventWorkflowHelperData.botOptions.Value);

            var result = await this.eventWorkflowHelper.CreateNewEventAsync(EventWorkflowHelperData.eventEntity, "");

            Assert.AreEqual(result, true);

        }
    }
}


