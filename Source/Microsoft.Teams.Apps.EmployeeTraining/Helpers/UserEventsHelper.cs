// <copyright file="UserEventsHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.EmployeeTraining.Cards;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;
    using Microsoft.Teams.Apps.EmployeeTraining.Models.Enums;
    using Microsoft.Teams.Apps.EmployeeTraining.Repositories;
    using Microsoft.Teams.Apps.EmployeeTraining.Services;
    using Polly;
    using Polly.Contrib.WaitAndRetry;
    using Polly.Retry;

    /// <summary>
    /// The helper class to manage the events operations done by the user
    /// </summary>
    public class UserEventsHelper : IUserEventsHelper
    {
        /// <summary>
        /// Provides the methods for event related operations on storage.
        /// </summary>
        private readonly IEventRepository eventRepository;

        /// <summary>
        /// Search service to filter and search events.
        /// </summary>
        private readonly IEventSearchService eventSearchService;

        /// <summary>
        /// Search service to filter and search events for end user.
        /// </summary>
        private readonly IUserEventSearchService userEventSearchService;

        /// <summary>
        /// Helper to use Microsoft Graph users api.
        /// </summary>
        private readonly IUserGraphHelper userGraphHelper;

        /// <summary>
        /// Helper to use Microsoft Graph events api.
        /// </summary>
        private readonly IEventGraphHelper eventGraphHelper;

        /// <summary>
        /// Helper to send notifications to user and team.
        /// </summary>
        private readonly INotificationHelper notificationHelper;

        /// <summary>
        /// Helper to bind category name by Id.
        /// </summary>
        private readonly ICategoryHelper categoryHelper;

        /// <summary>
        /// Team configuration repository for storing and updating team information.
        /// </summary>
        private readonly ILnDTeamConfigurationRepository lnDTeamConfigurationRepository;

        /// <summary>
        /// Represents a set of key/value application configuration properties for bot.
        /// </summary>
        private readonly IOptions<BotSettings> botOptions;

        /// <summary>
        /// The current culture's string localizer.
        /// </summary>
        private readonly IStringLocalizer<Strings> localizer;

        /// <summary>
        /// Retry policy with linear backoff, retry twice with a jitter delay of up to 1 sec. Retry for HTTP 412(precondition failed).
        /// </summary>
        /// <remarks>
        /// Reference: https://github.com/Polly-Contrib/Polly.Contrib.WaitAndRetry#new-jitter-recommendation.
        /// </remarks>
        private readonly AsyncRetryPolicy retryPolicy = Policy.Handle<StorageException>(ex => ex.RequestInformation.HttpStatusCode == (int)StatusCodes.Status412PreconditionFailed)
                .WaitAndRetryAsync(Backoff.LinearBackoff(TimeSpan.FromMilliseconds(250), 25));

        /// <summary>
        /// Initializes a new instance of the <see cref="UserEventsHelper"/> class.
        /// </summary>
        /// <param name="eventRepository">Provides the methods for event related operations on storage.</param>
        /// <param name="eventSearchService">Search service to filter and search events.</param>
        /// <param name="userEventSearchService">Search service to filter and search events for end user.</param>
        /// <param name="userGraphHelper">Helper to use Microsoft Graph users api.</param>
        /// <param name="eventGraphHelper">Helper to use Microsoft Graph events api.</param>
        /// <param name="notificationHelper">Helper to send notifications to user and team.</param>
        /// <param name="categoryHelper">Helper to bind category name by Id.</param>
        /// <param name="lnDTeamConfigurationRepository">Team configuration repository for storing and updating team information.</param>
        /// <param name="botOptions">Represents a set of key/value application configuration properties for bot.</param>
        /// <param name="localizer">The current culture's string localizer.</param>
        public UserEventsHelper(
            IEventRepository eventRepository,
            IEventSearchService eventSearchService,
            IUserEventSearchService userEventSearchService,
            IUserGraphHelper userGraphHelper,
            IEventGraphHelper eventGraphHelper,
            INotificationHelper notificationHelper,
            ICategoryHelper categoryHelper,
            ILnDTeamConfigurationRepository lnDTeamConfigurationRepository,
            IOptions<BotSettings> botOptions,
            IStringLocalizer<Strings> localizer)
        {
            this.eventRepository = eventRepository;
            this.eventSearchService = eventSearchService;
            this.userEventSearchService = userEventSearchService;
            this.userGraphHelper = userGraphHelper;
            this.eventGraphHelper = eventGraphHelper;
            this.notificationHelper = notificationHelper;
            this.categoryHelper = categoryHelper;
            this.lnDTeamConfigurationRepository = lnDTeamConfigurationRepository;
            this.botOptions = botOptions;
            this.localizer = localizer;
        }

        /// <summary>
        /// Get event details.
        /// </summary>
        /// <param name="eventId">Event Id for which details needs to be fetched.</param>
        /// <param name="teamId">Team Id with which event is associated.</param>
        /// <param name="userObjectId">The user Id</param>
        /// <returns>Event details.</returns>
        public async Task<EventEntity> GetEventAsync(string eventId, string teamId, string userObjectId)
        {
            if (string.IsNullOrEmpty(eventId))
            {
                return null;
            }

            if (string.IsNullOrEmpty(teamId))
            {
                return null;
            }

            if (string.IsNullOrEmpty(userObjectId))
            {
                return null;
            }

            var eventDetails = await this.eventRepository.GetEventDetailsAsync(eventId, teamId);

            if (eventDetails != null)
            {
                // If user is present in auto registered attendees column means user is mandatory and registered for that event.
                if (eventDetails.AutoRegisteredAttendees != null
                    && eventDetails.AutoRegisteredAttendees.Contains(userObjectId, StringComparison.OrdinalIgnoreCase))
                {
                    eventDetails.IsMandatoryForLoggedInUser = true;
                    eventDetails.IsLoggedInUserRegistered = true;
                }

                // If user is present in registered attendees column means user is registered for that event but not mandatory.
                if (eventDetails.RegisteredAttendees != null
                    && eventDetails.RegisteredAttendees.Contains(userObjectId, StringComparison.OrdinalIgnoreCase))
                {
                    eventDetails.IsLoggedInUserRegistered = true;
                }

                if (eventDetails.Audience == (int)EventAudience.Private)
                {
                    // If user is present in mandatory attendees column means user is mandatory for that event and can register for it.
                    if (eventDetails.MandatoryAttendees != null
                        && eventDetails.MandatoryAttendees.Contains(userObjectId, StringComparison.OrdinalIgnoreCase))
                    {
                        eventDetails.IsMandatoryForLoggedInUser = true;
                        eventDetails.CanLoggedInUserRegister = true;
                    }

                    // If user is present in optional attendees column means user is optional for that event and can register for it.
                    if (eventDetails.OptionalAttendees != null
                        && eventDetails.OptionalAttendees.Contains(userObjectId, StringComparison.OrdinalIgnoreCase))
                    {
                        eventDetails.CanLoggedInUserRegister = true;
                    }
                }
                else
                {
                    eventDetails.CanLoggedInUserRegister = true;
                }
            }

            return eventDetails;
        }

        /// <summary>
        /// Registers the user for an event
        /// </summary>
        /// <param name="teamId">The LnD team Id who created the event</param>
        /// <param name="eventId">The event Id</param>
        /// <param name="userAADObjectId">The user Id</param>
        /// <returns>Returns true if registration done successfully. Else returns false.</returns>
        public async Task<bool> RegisterToEventAsync(string teamId, string eventId, string userAADObjectId)
        {
            if (string.IsNullOrEmpty(teamId) || string.IsNullOrEmpty(eventId) || string.IsNullOrEmpty(userAADObjectId))
            {
                return false;
            }

            // Retry policy to handle consistency while updating event.
            return await this.retryPolicy.ExecuteAsync<bool>(async () =>
            {
                var eventDetails = await this.eventRepository.GetEventDetailsAsync(eventId, teamId);

                // Return false if any one of the following condition matches:
                // 1. Event status is other than active
                // 2. Registration for the event is closed by LnD team
                // 3. Registered attendees count reached maximum participants limit
                // 4. Event end date is past date (i.e. it is completed event)
                if (eventDetails == null
                    || eventDetails.Status != (int)EventStatus.Active
                    || eventDetails.IsRegistrationClosed
                    || eventDetails.RegisteredAttendeesCount >= eventDetails.MaximumNumberOfParticipants
                    || eventDetails.EndDate < DateTime.UtcNow)
                {
                    return false;
                }

                // Return false if any one of the following condition matches:
                // Event is private and logged in users' Id is not present in either mandatory attendees or optional attendees
                // If condition is true, means user is not added for this private event hence cannot register.
                if (eventDetails.Audience == (int)EventAudience.Private
                    && eventDetails.MandatoryAttendees != null
                    && !eventDetails.MandatoryAttendees.Contains(userAADObjectId, StringComparison.OrdinalIgnoreCase)
                    && eventDetails.OptionalAttendees != null
                    && !eventDetails.OptionalAttendees.Contains(userAADObjectId, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                // If user is already present in registered attendees coulmn or auto registered attendees column then return false.
                if ((eventDetails.AutoRegisteredAttendees != null
                    && eventDetails.AutoRegisteredAttendees.Contains(userAADObjectId, StringComparison.OrdinalIgnoreCase))
                    || (eventDetails.RegisteredAttendees != null
                    && eventDetails.RegisteredAttendees.Contains(userAADObjectId, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }

                if (string.IsNullOrEmpty(eventDetails.RegisteredAttendees))
                {
                    eventDetails.RegisteredAttendees = userAADObjectId;
                }
                else
                {
                    eventDetails.RegisteredAttendees = string.Join(";", eventDetails.RegisteredAttendees, userAADObjectId);
                }

                eventDetails.RegisteredAttendeesCount += 1;

                var isRegisteredSuccessfully = await this.eventRepository.UpdateEventAsync(eventDetails);

                if (isRegisteredSuccessfully)
                {
                    var isGraphEventUpdated = await this.UpdateGraphEvent(eventDetails);

                    if (isGraphEventUpdated)
                    {
                        await this.UpdateEventNotificationInTeam(eventDetails);
                        await this.eventSearchService.RunIndexerOnDemandAsync();
                    }

                    return isGraphEventUpdated;
                }

                return false;
            });
        }

        /// <summary>
        /// Unregisters the user for an event
        /// </summary>
        /// <param name="teamId">The LnD team Id who created the event</param>
        /// <param name="eventId">The event Id</param>
        /// <param name="userAADObjectId">The user Id</param>
        /// <returns>Returns true if the user successfully unregistered for an event. Else returns false.</returns>
        public async Task<bool> UnregisterFromEventAsync(string teamId, string eventId, string userAADObjectId)
        {
            if (string.IsNullOrEmpty(teamId) || string.IsNullOrEmpty(eventId) || string.IsNullOrEmpty(userAADObjectId))
            {
                return false;
            }

            // Retry policy to handle consistency while updating event.
            return await this.retryPolicy.ExecuteAsync<bool>(async () =>
            {
                var eventDetails = await this.eventRepository.GetEventDetailsAsync(eventId, teamId);

                // If event is completed then return false.
                if (eventDetails == null
                    || eventDetails.Status != (int)EventStatus.Active
                    || eventDetails.EndDate < DateTime.UtcNow)
                {
                    return false;
                }

                // Return false if any one of the following condition matches:
                // Event is private and logged in users' Id is not present in either mandatory attendees or optional attendees
                // If condition is true, means user is not added for this private event hence cannot un-register.
                if (eventDetails.Audience == (int)EventAudience.Private
                    && eventDetails.MandatoryAttendees != null
                    && !eventDetails.MandatoryAttendees.Contains(userAADObjectId, StringComparison.OrdinalIgnoreCase)
                    && eventDetails.OptionalAttendees != null
                    && !eventDetails.OptionalAttendees.Contains(userAADObjectId, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                bool isRegisteredUser = false;

                if (eventDetails.AutoRegisteredAttendees != null
                    && eventDetails.AutoRegisteredAttendees.Contains(userAADObjectId, StringComparison.OrdinalIgnoreCase))
                {
                    isRegisteredUser = true;

                    var autoRegisteredAttendees = eventDetails.AutoRegisteredAttendees.Split(";");
                    eventDetails.AutoRegisteredAttendees = string.Join(";", autoRegisteredAttendees.Where(x => x != userAADObjectId));
                }

                if (eventDetails.RegisteredAttendees != null
                    && eventDetails.RegisteredAttendees.Contains(userAADObjectId, StringComparison.OrdinalIgnoreCase))
                {
                    isRegisteredUser = true;

                    var registeredAttendees = eventDetails.RegisteredAttendees.Split(";");
                    eventDetails.RegisteredAttendees = string.Join(";", registeredAttendees.Where(x => x != userAADObjectId));
                }

                if (!isRegisteredUser)
                {
                    return false;
                }

                eventDetails.RegisteredAttendeesCount -= 1;

                var isRegisteredSuccessfully = await this.eventRepository.UpdateEventAsync(eventDetails);

                if (isRegisteredSuccessfully)
                {
                    var isGraphEventUpdated = await this.UpdateGraphEvent(eventDetails);

                    if (isGraphEventUpdated)
                    {
                        await this.UpdateEventNotificationInTeam(eventDetails);
                        await this.eventSearchService.RunIndexerOnDemandAsync();
                    }

                    return isGraphEventUpdated;
                }

                return false;
            });
        }

        /// <summary>
        /// Get user events as per user search text and filters
        /// </summary>
        /// <param name="searchString">Search string entered by user.</param>
        /// <param name="pageCount">>Page count for which post needs to be fetched.</param>
        /// <param name="eventSearchType">Event search operation type. Refer <see cref="EventSearchType"/> for values.</param>
        /// <param name="userObjectId">Logged in user's AAD object identifier.</param>
        /// <param name="createdByFilter">Semicolon separated user AAD object identifier who created events.</param>
        /// <param name="categoryFilter">Semicolon separated category Ids.</param>
        /// <param name="sortBy">0 for recent and 1 for popular events. Refer <see cref="SortBy"/> for values.</param>
        /// <returns>List of user events</returns>
        public async Task<IEnumerable<EventEntity>> GetEventsAsync(string searchString, int pageCount, int eventSearchType, string userObjectId, string createdByFilter, string categoryFilter, int sortBy)
        {
            var recentCollaboratorIds = Enumerable.Empty<string>();

            if (sortBy == (int)SortBy.PopularityByRecentCollaborators)
            {
                recentCollaboratorIds = await this.GetTopRecentCollaboratorsAsync();
            }

            var paramsDto = new SearchParametersDto
            {
                SearchString = searchString,
                PageCount = pageCount,
                SearchScope = (EventSearchType)eventSearchType,
                UserObjectId = userObjectId,
                CreatedByFilter = createdByFilter,
                CategoryFilter = categoryFilter,
                SortByFilter = sortBy,
                RecentCollaboratorIds = recentCollaboratorIds,
                SearchResultsCount = this.botOptions.Value.EventsPageSize,
            };

            var userEvents = await this.userEventSearchService.GetEventsAsync(paramsDto);

            return userEvents;
        }

        /// <summary>
        /// Update graph event
        /// </summary>
        /// <param name="eventToUpsert">The event to be updated</param>
        /// <returns>Returns boolean indicating whether update operation is successful</returns>
        private async Task<bool> UpdateGraphEvent(EventEntity eventToUpsert)
        {
            eventToUpsert.NumberOfOccurrences = Convert.ToInt32(eventToUpsert.EndDate.Value.Subtract(eventToUpsert.StartDate.Value).TotalDays) + 1;

            // Create event using MS Graph.
            var graphEventResult = await this.eventGraphHelper.UpdateEventAsync(eventToUpsert);

            if (graphEventResult == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sends notification in team after event in created
        /// </summary>
        /// <param name="eventDetails">Event details.</param>
        /// <returns>Returns true if notification sent successfully. Else returns false.</returns>
        private async Task<string> UpdateEventNotificationInTeam(EventEntity eventDetails)
        {
            if (eventDetails == null)
            {
                return null;
            }

            await this.categoryHelper.BindCategoryNameAsync(new List<EventEntity>() { eventDetails });
            var teamDetails = await this.lnDTeamConfigurationRepository.GetTeamDetailsAsync(eventDetails.TeamId);

            var createdByName = await this.userGraphHelper.GetUserAsync(eventDetails.CreatedBy);

            var notificationCard = EventDetailsCard.GetEventCreationCardForTeam(this.botOptions.Value.AppBaseUri, this.localizer, eventDetails, createdByName?.DisplayName);

            if (string.IsNullOrEmpty(eventDetails.TeamCardActivityId))
            {
                return await this.notificationHelper.SendNotificationInTeamAsync(teamDetails, notificationCard);
            }
            else
            {
                return await this.notificationHelper.SendNotificationInTeamAsync(teamDetails, notificationCard, true, eventDetails.TeamCardActivityId);
            }
        }

        /// <summary>
        /// Get top recent collaborators ordered by relevance score in descending order
        /// </summary>
        /// <returns>Returns the list of top collaborators</returns>
        private async Task<IEnumerable<string>> GetTopRecentCollaboratorsAsync()
        {
            var recentCollaborators = await this.userGraphHelper.GetRecentCollaboratorsForPopularInMyNetworkAsync();

            if (recentCollaborators != null && recentCollaborators.Any())
            {
                var collaboratorsWithTopScoredEmailAddress = recentCollaborators
                    .Select(collaborator => new { id = collaborator.Id, topScoredEmailAddress = collaborator.ScoredEmailAddresses?.OrderByDescending(scoredEmailAddress => scoredEmailAddress.RelevanceScore)?.First() });

                var topCollaborators = collaboratorsWithTopScoredEmailAddress?
                    .OrderByDescending(collaborator => collaborator.topScoredEmailAddress.RelevanceScore)
                    .Select(collaborator => collaborator.id)
                    .Take(20);

                return topCollaborators;
            }

            return null;
        }
    }
}