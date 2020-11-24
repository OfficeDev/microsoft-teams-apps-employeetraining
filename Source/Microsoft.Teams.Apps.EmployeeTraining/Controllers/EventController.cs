// <copyright file="EventController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.EmployeeTraining.Helpers;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;
    using Microsoft.Teams.Apps.EmployeeTraining.Models.Enums;
    using Microsoft.Teams.Apps.EmployeeTraining.Services;

    /// <summary>
    /// Exposes APIs related to event operations.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EventController : BaseController
    {
        /// <summary>
        /// Logs errors and information
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Search service to search and filter events.
        /// </summary>
        private readonly IUserEventSearchService userEventSearchService;

        /// <summary>
        /// The event helper for performing user operations on events created by LnD team
        /// </summary>
        private readonly IUserEventsHelper userEventsHelper;

        /// <summary>
        /// Category helper for fetching based on Ids, binding category names to events
        /// </summary>
        private readonly ICategoryHelper categoryHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventController"/> class.
        /// </summary>
        /// <param name="logger">The ILogger object which logs errors and information</param>
        /// <param name="telemetryClient">The Application Insights telemetry client</param>
        /// <param name="userEventSearchService">The user event search service helper dependency injection</param>
        /// <param name="userEventsHelper">The user events helper dependency injection</param>
        /// <param name="categoryHelper">Category helper for fetching based on Ids, binding category names to events</param>
        public EventController(
            ILogger<EventController> logger,
            TelemetryClient telemetryClient,
            IUserEventSearchService userEventSearchService,
            IUserEventsHelper userEventsHelper,
            ICategoryHelper categoryHelper)
            : base(telemetryClient)
        {
            this.logger = logger;
            this.userEventSearchService = userEventSearchService;
            this.userEventsHelper = userEventsHelper;
            this.categoryHelper = categoryHelper;
        }

        /// <summary>
        /// Get event details.
        /// </summary>
        /// <param name="eventId">Event Id for which details needs to be fetched.</param>
        /// <param name="teamId">Team Id with which event is associated.</param>
        /// <returns>Event details.</returns>
        [HttpGet]
        public async Task<IActionResult> GetEventAsync(string eventId, string teamId)
        {
            this.RecordEvent("Get event- The HTTP POST call to get event details has been initiated", new Dictionary<string, string>
            {
                { "eventId", eventId },
                { "teamId", teamId },
            });

            if (string.IsNullOrEmpty(eventId))
            {
                this.logger.LogError("Event Id is either null or empty");
                this.RecordEvent("Get event- The HTTP POST call to get event details has been failed", new Dictionary<string, string>
                {
                    { "eventId", eventId },
                    { "teamId", teamId },
                });
                return this.BadRequest(new { message = "Event Id is null or empty" });
            }

            if (string.IsNullOrEmpty(teamId))
            {
                this.logger.LogError("Team Id is either null or empty");
                this.RecordEvent("Get event- The HTTP POST call to get event details has been failed", new Dictionary<string, string>
                {
                    { "eventId", eventId },
                    { "teamId", teamId },
                });
                return this.BadRequest(new { message = "Team Id is null or empty" });
            }

            try
            {
                var eventDetails = await this.userEventsHelper.GetEventAsync(eventId, teamId, this.UserAadId);
                await this.categoryHelper.BindCategoryNameAsync(new List<EventEntity>() { eventDetails });

                this.RecordEvent("Get event- The HTTP POST call to get event details has been succeeded", new Dictionary<string, string>
                {
                    { "eventId", eventId },
                    { "teamId", teamId },
                });
                return this.Ok(eventDetails);
            }
            catch (Exception ex)
            {
                this.RecordEvent("Get event- The HTTP POST call to get event details has been failed", new Dictionary<string, string>
                {
                    { "eventId", eventId },
                    { "teamId", teamId },
                });
                this.logger.LogError(ex, $"Error occurred while fetching event details for event Id {eventId} team Id {teamId}");
                throw;
            }
        }

        /// <summary>
        /// Get user events as per user search text and filters
        /// </summary>
        /// <param name="searchString">Search string entered by user.</param>
        /// <param name="pageCount">>Page count for which post needs to be fetched.</param>
        /// <param name="eventSearchType">Event search operation type. Refer <see cref="EventSearchType"/> for values.</param>
        /// <param name="createdByFilter">Semicolon separated user AAD object identifier who created events.</param>
        /// <param name="categoryFilter">Semicolon separated category Ids.</param>
        /// <param name="sortBy">0 for recent and 1 for popular events. Refer <see cref="SortBy"/> for values.</param>
        /// <returns>List of user events</returns>
        [HttpGet("UserEvents")]
        public async Task<IActionResult> GetEventsAsync(string searchString, int pageCount, int eventSearchType, string createdByFilter, string categoryFilter, int sortBy)
        {
            this.RecordEvent("Get user events- The HTTP GET call to get user events has initiated");

            if (!Enum.IsDefined(typeof(EventSearchType), eventSearchType))
            {
                this.logger.LogError("Invalid event search type");
                this.RecordEvent("Get user events- The HTTP GET call to get user events has failed");
                return this.BadRequest(new ErrorResponse { Message = "The event search type was invalid" });
            }

            if (!Enum.IsDefined(typeof(SortBy), sortBy))
            {
                this.logger.LogError("Invalid sort by value");
                this.RecordEvent("Get user events- The HTTP GET call to get user events has failed");
                return this.BadRequest(new ErrorResponse { Message = "Provided sort by value was invalid" });
            }

            try
            {
                var userEvents = await this.userEventsHelper.GetEventsAsync(
                    searchString, pageCount, eventSearchType, this.UserAadId, createdByFilter, categoryFilter, sortBy);

                this.RecordEvent("Get user events- The HTTP GET call to get user events has succeeded");

                if (userEvents.IsNullOrEmpty())
                {
                    return this.Ok(new List<EventEntity>());
                }

                await this.categoryHelper.BindCategoryNameAsync(userEvents);
                return this.Ok(userEvents);
            }
            catch (Exception ex)
            {
                this.RecordEvent("Get user events- The HTTP GET call to get user events has failed");
                this.logger.LogError(ex, "Error occurred while fetching user events");
                throw;
            }
        }

        /// <summary>
        /// Search event as per user search input.
        /// </summary>
        /// <param name="search">Search string entered by user.</param>
        /// <returns>Event details.</returns>
        [HttpGet("search-by-title")]
        public async Task<IActionResult> SearchEventAsync(string search)
        {
            this.RecordEvent("Search event- The HTTP POST call to search event details has been initiated");

            if (string.IsNullOrEmpty(search))
            {
                this.logger.LogError("Search query is either null or empty");
                return this.BadRequest(new ErrorResponse { Message = "Search query is either null or empty" });
            }

            try
            {
                var searchParametersDto = new SearchParametersDto
                {
                    SearchString = search,
                    SearchScope = EventSearchType.SearchByName,
                    UserObjectId = this.UserAadId,
                };
                var searchedEvents = await this.userEventSearchService.GetEventsAsync(searchParametersDto);

                this.RecordEvent("Search event- The HTTP POST call to search event details has been succeeded");
                return this.Ok(searchedEvents);
            }
            catch (Exception ex)
            {
                this.RecordEvent("Search event- The HTTP POST call to search event details has been failed");
                this.logger.LogError(ex, "Error occurred while searching event");
                throw;
            }
        }

        /// <summary>
        /// Registers the user for an event
        /// </summary>
        /// <param name="teamId">The LnD team Id who created the event</param>
        /// <param name="eventId">The event Id</param>
        /// <returns>Returns true if registration done successfully. Else returns false.</returns>
        [HttpPost("RegisterToEvent")]
        public async Task<IActionResult> RegisterToEventAsync(string teamId, string eventId)
        {
            this.RecordEvent("Register to event- The HTTP POST call to register user for an event has initiated", new Dictionary<string, string>
            {
                { "eventId", eventId },
                { "teamId", teamId },
            });

            if (string.IsNullOrEmpty(teamId))
            {
                this.logger.LogError("Invalid team Id was provided");
                this.RecordEvent("Register to event- The HTTP POST call to register user for an event has failed", new Dictionary<string, string>
                {
                    { "eventId", eventId },
                    { "teamId", teamId },
                });
                return this.BadRequest(new ErrorResponse { Message = "Invalid team Id was provided" });
            }

            if (string.IsNullOrEmpty(eventId))
            {
                this.logger.LogError("Invalid event Id was provided");
                this.RecordEvent("Register to event- The HTTP POST call to register user for an event has failed", new Dictionary<string, string>
                {
                    { "eventId", eventId },
                    { "teamId", teamId },
                });
                return this.BadRequest(new ErrorResponse { Message = "Invalid event Id was provided" });
            }

            try
            {
                var isRegistrationSuccessful = await this.userEventsHelper.RegisterToEventAsync(teamId, eventId, this.UserAadId);

                this.RecordEvent("Register to event- The HTTP POST call to register user for an event has succeeded", new Dictionary<string, string>
                {
                    { "eventId", eventId },
                    { "teamId", teamId },
                });

                return this.Ok(isRegistrationSuccessful);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Error occurred while registering user {this.UserAadId} for event {eventId}");
                this.RecordEvent("Register to event- The HTTP POST call to register user for an event has failed", new Dictionary<string, string>
                {
                    { "eventId", eventId },
                    { "teamId", teamId },
                });
                throw;
            }
        }

        /// <summary>
        /// Unregisters the user for an event
        /// </summary>
        /// <param name="teamId">The LnD team Id who created the event</param>
        /// <param name="eventId">The event Id</param>
        /// <returns>Returns true if the user successfully unregistered for an event. Else returns false.</returns>
        [HttpPost("UnregisterToEvent")]
        public async Task<IActionResult> UnregisterToEventAsync(string teamId, string eventId)
        {
            this.RecordEvent("Unregister to event- The HTTP POST call to unregister user to an event has initiated", new Dictionary<string, string>
            {
                { "eventId", eventId },
                { "teamId", teamId },
            });

            if (string.IsNullOrEmpty(teamId))
            {
                this.logger.LogError("Invalid team Id was provided");
                this.RecordEvent("Invalid team Id was provided");
                return this.BadRequest(new ErrorResponse { Message = "Invalid team Id was provided" });
            }

            if (string.IsNullOrEmpty(eventId))
            {
                this.logger.LogError("Invalid event Id was provided");
                this.RecordEvent("Invalid event Id was provided");
                return this.BadRequest(new ErrorResponse { Message = "Invalid event Id was provided" });
            }

            try
            {
                var isUserRemovedFromEvent = await this.userEventsHelper.UnregisterFromEventAsync(teamId, eventId, this.UserAadId);

                this.RecordEvent("Unregister to event- The HTTP POST call to unregister user to an event has succeeded", new Dictionary<string, string>
                {
                    { "eventId", eventId },
                    { "teamId", teamId },
                });

                return this.Ok(isUserRemovedFromEvent);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Error occurred while unregistering user {this.UserAadId} for event {eventId}");
                this.RecordEvent("Unregister to event- The HTTP POST call to unregister user to an event has failed", new Dictionary<string, string>
                {
                    { "eventId", eventId },
                    { "teamId", teamId },
                });
                throw;
            }
        }
    }
}