// <copyright file="BackgroundNotificationHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Helpers.BackgroundService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Rest.Azure;
    using Microsoft.Teams.Apps.EmployeeTraining.Cards;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;
    using Microsoft.Teams.Apps.EmployeeTraining.Models.Enums;
    using Microsoft.Teams.Apps.EmployeeTraining.Repositories;
    using Microsoft.Teams.Apps.EmployeeTraining.Services;
    using Microsoft.WindowsAzure.Storage;

    /// <summary>
    /// This class inherits IHostedService and implements the methods related to background tasks for sending event reminders.
    /// </summary>
    public class BackgroundNotificationHelper : BackgroundService
    {
        /// <summary>
        /// Instance to send logs to the Application Insights service.
        /// </summary>
        private readonly ILogger<BackgroundNotificationHelper> logger;

        /// <summary>
        /// The user event search service to generate query and fetch results.
        /// </summary>
        private readonly IUserEventSearchService userEventSearchService;

        /// <summary>
        /// Instance of notification helper which helps in sending notifications.
        /// </summary>
        private readonly INotificationHelper notificationHelper;

        /// <summary>
        /// A set of key/value application configuration properties for activity settings
        /// </summary>
        private readonly IOptions<BotSettings> botOptions;

        /// <summary>
        /// The current culture's string localizer
        /// </summary>
        private readonly IStringLocalizer<Strings> localizer;

        /// <summary>
        /// Provides repository operations for user entity
        /// </summary>
        private readonly IUserConfigurationRepository userConfigurationRepository;

        /// <summary>
        /// Category helper to fetch and bind category name by Id.
        /// </summary>
        private readonly ICategoryHelper categoryHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundNotificationHelper"/> class.
        /// BackgroundService class that inherits IHostedService and implements the methods related to sending notification tasks.
        /// </summary>
        /// <param name="logger">Instance to send logs to the Application Insights service.</param>
        /// <param name="notificationHelper">Helper to send notification in channels.</param>
        /// <param name="userEventSearchService">The user event search service to generate query and fetch results.</param>
        /// <param name="botOptions">A set of key/value application configuration properties for activity settings</param>
        /// <param name="localizer">The current culture's string localizer</param>
        /// <param name="userConfigurationRepository">The user repository for user related operations on storage</param>
        /// <param name="categoryHelper">Category helper to fetch and bind category name by Id.</param>
        public BackgroundNotificationHelper(
            ILogger<BackgroundNotificationHelper> logger,
            INotificationHelper notificationHelper,
            IUserEventSearchService userEventSearchService,
            IOptions<BotSettings> botOptions,
            IStringLocalizer<Strings> localizer,
            IUserConfigurationRepository userConfigurationRepository,
            ICategoryHelper categoryHelper)
        {
            this.logger = logger;
            this.notificationHelper = notificationHelper;
            this.userEventSearchService = userEventSearchService;
            this.botOptions = botOptions;
            this.localizer = localizer;
            this.userConfigurationRepository = userConfigurationRepository;
            this.categoryHelper = categoryHelper;
        }

        /// <summary>
        ///  This method is called when the Microsoft.Extensions.Hosting.IHostedService starts.
        ///  The implementation should return a task that represents the lifetime of the long
        ///  running operation(s) being performed.
        /// </summary>
        /// <param name="stoppingToken">Triggered when Microsoft.Extensions.Hosting.IHostedService.StopAsync(System.Threading.CancellationToken) is called.</param>
        /// <returns>A System.Threading.Tasks.Task that represents the long running operations.</returns>
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    this.logger.LogInformation($"Notification Hosted Service is running at: {DateTimeOffset.UtcNow} UTC.");

                    var reminderSearchParametersDto = new SearchParametersDto
                    {
                        SearchString = "*",
                        PageCount = null,
                        UserObjectId = null,
                        SearchScope = EventSearchType.DayBeforeReminder,
                    };

                    var dailyReminderEvents = await this.userEventSearchService.GetEventsAsync(reminderSearchParametersDto);
                    if (!dailyReminderEvents.IsNullOrEmpty())
                    {
                        await this.SendReminder(dailyReminderEvents, NotificationType.Daily);
                    }

                    if (DateTimeOffset.UtcNow.DayOfWeek == DayOfWeek.Monday)
                    {
                        reminderSearchParametersDto.SearchScope = EventSearchType.WeekBeforeReminder;

                        var weeklyReminderEvents = await this.userEventSearchService.GetEventsAsync(reminderSearchParametersDto);
                        if (!weeklyReminderEvents.IsNullOrEmpty())
                        {
                            await this.SendReminder(weeklyReminderEvents, NotificationType.Weekly);
                        }
                    }
                }
                catch (CloudException ex)
                {
                    this.logger.LogError(ex, $"Error occurred while accessing search service: {ex.Message} at: {DateTimeOffset.UtcNow}");
                }
                catch (StorageException ex)
                {
                    this.logger.LogError(ex, $"Error occurred while accessing storage: {ex.Message} at: {DateTimeOffset.UtcNow}");
                }
#pragma warning disable CA1031 // Catching general exceptions that might arise during execution to avoid blocking next run.
                catch (Exception ex)
#pragma warning restore CA1031 // Catching general exceptions that might arise during execution to avoid blocking next run.
                {
                    this.logger.LogError(ex, "Error occurred while running digest notification service.");
                }

                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }

        /// <summary>
        /// Sends cancellation notification to the registered users for an event
        /// </summary>
        /// <param name="eventDetails">List of events details.</param>
        /// <param name="notificationType">The type of notification being sent</param>
        private async Task SendReminder(IEnumerable<EventEntity> eventDetails, NotificationType notificationType)
        {
            var userList = new List<string>();

            await this.categoryHelper.BindCategoryNameAsync(eventDetails);
            userList.AddRange(eventDetails.Select(eventdetails => eventdetails.GetAttendees()).First());
            if (userList.Any())
            {
                userList = userList.Distinct().ToList();
                var registeredAttendees = await this.userConfigurationRepository.GetUserConfigurationsAsync(userList);

                foreach (var user in registeredAttendees)
                {
                    var filteredEvents = eventDetails.Where(eventDetails => (eventDetails.AutoRegisteredAttendees != null && eventDetails.AutoRegisteredAttendees.Contains(user.AADObjectId, StringComparison.OrdinalIgnoreCase)) ||
                        (eventDetails.RegisteredAttendees != null && eventDetails.RegisteredAttendees.Contains(user.AADObjectId, StringComparison.OrdinalIgnoreCase)));

                    if (!filteredEvents.IsNullOrEmpty())
                    {
                        var card = ReminderCard.GetCard(filteredEvents, this.localizer, this.botOptions.Value.ManifestId, notificationType);
                        await this.notificationHelper.SendNotificationToUsersAsync(new List<User> { user }, card);
                    }
                }
            }
        }
    }
}
