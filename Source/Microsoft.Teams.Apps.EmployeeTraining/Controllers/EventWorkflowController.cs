// <copyright file="EventWorkflowController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.EmployeeTraining.Authentication;
    using Microsoft.Teams.Apps.EmployeeTraining.Helpers;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;
    using Microsoft.Teams.Apps.EmployeeTraining.Models.Enums;
    using Microsoft.Teams.Apps.EmployeeTraining.Services;

    /// <summary>
    /// Exposes APIs related to event operations.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(PolicyNames.MustBeLnDTeamMemberPolicy)]
    public class EventWorkflowController : BaseController
    {
        /// <summary>
        /// Logs errors and information
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Helper methods for CRUD operations on event.
        /// </summary>
        private readonly IEventWorkflowHelper eventWorkflowHelper;

        /// <summary>
        /// The helper class which manages LnD team related search service activities for events
        /// </summary>
        private readonly ITeamEventSearchService teamEventSearchService;

        /// <summary>
        /// Category helper for getting category names as per category Ids
        /// </summary>
        private readonly ICategoryHelper categoryHelper;

        /// <summary>
        /// The current culture's string localizer
        /// </summary>
        private readonly IStringLocalizer<Strings> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventWorkflowController"/> class.
        /// </summary>
        /// <param name="logger">The ILogger object which logs errors and information</param>
        /// <param name="telemetryClient">The Application Insights telemetry client</param>
        /// <param name="eventWorkflowHelper">Helper methods for CRUD operations on event.</param>
        /// <param name="teamEventSearchService">The team event search service dependency injection</param>
        /// <param name="categoryHelper">Category helper for getting category names as per category Ids</param>
        /// <param name="localizer">The current culture's string localizer.</param>
        public EventWorkflowController(
            ILogger<EventController> logger,
            TelemetryClient telemetryClient,
            IEventWorkflowHelper eventWorkflowHelper,
            ITeamEventSearchService teamEventSearchService,
            ICategoryHelper categoryHelper,
            IStringLocalizer<Strings> localizer)
            : base(telemetryClient)
        {
            this.logger = logger;
            this.eventWorkflowHelper = eventWorkflowHelper;
            this.teamEventSearchService = teamEventSearchService;
            this.categoryHelper = categoryHelper;
            this.localizer = localizer;
        }

        /// <summary>
        /// Save new event as draft.
        /// </summary>
        /// <param name="eventEntity">Event details entered by user.</param>
        /// <param name="teamId">Team Id for which event will be created.</param>
        /// <returns>Boolean indicating insert operation result.</returns>
        [HttpPost("create-draft")]
        public async Task<IActionResult> CreateDraftAsync([FromBody] EventEntity eventEntity, [FromQuery] string teamId)
        {
            this.RecordEvent("Create draft- The HTTP POST call to create draft has been initiated", new Dictionary<string, string>
            {
                { "teamId", teamId },
            });

            if (string.IsNullOrEmpty(teamId))
            {
                this.logger.LogError("Team Id is either null or empty");
                this.RecordEvent("Create draft- The HTTP POST call to create draft has been failed");
                return this.BadRequest(new ErrorResponse { Message = "Team Id is either null or empty" });
            }

            // Assigning created by and created dates. Other properties will be selected and copied in EventWorkflowHelper.
#pragma warning disable CA1062 // Null check is handled by data annotations at model level
            eventEntity.CreatedBy = this.UserAadId;
#pragma warning restore CA1062 // Null check is handled by data annotations at model level
            eventEntity.UpdatedBy = this.UserAadId;
            eventEntity.CreatedOn = DateTime.UtcNow;
            eventEntity.UpdatedOn = DateTime.UtcNow;
            eventEntity.TeamId = teamId;

            try
            {
                var result = await this.eventWorkflowHelper.CreateDraftEventAsync(eventEntity);

                if (!result)
                {
                    this.RecordEvent("Create draft- The HTTP POST call to create draft has been failed", new Dictionary<string, string>
                    {
                        { "teamId", teamId },
                    });
                    this.logger.LogInformation($"Failed to create draft event for user {this.UserAadId} and team {teamId}");
                }

                this.RecordEvent("Create draft- The HTTP POST call to create draft has been succeeded");

                return this.Ok(result);
            }
            catch (Exception ex)
            {
                this.RecordEvent("Create draft- The HTTP POST call to create draft has been failed", new Dictionary<string, string>
                {
                    { "teamId", teamId },
                });
                this.logger.LogError(ex, $"Error occurred while creating event as draft for user {this.UserAadId} and team {teamId}");
                throw;
            }
        }

        /// <summary>
        /// Update draft event details.
        /// </summary>
        /// <param name="eventEntity">Event details entered by user.</param>
        /// <param name="teamId">Team Id for which event will be created.</param>
        /// <returns>Boolean indicating update operation result.</returns>
        [HttpPatch("update-draft")]
        public async Task<IActionResult> UpdateDraftAsync([FromBody] EventEntity eventEntity, [FromQuery] string teamId)
        {
            this.RecordEvent("Update draft- The HTTP PATCH call to update draft has been initiated", new Dictionary<string, string>
            {
                { "eventId", eventEntity?.EventId },
                { "teamId", teamId },
            });

            if (string.IsNullOrEmpty(teamId))
            {
                this.logger.LogError("Team Id is either null or empty");
                this.RecordEvent("Update draft- The HTTP PATCH call to update draft has been failed", new Dictionary<string, string>
                {
                    { "eventId", eventEntity?.EventId },
                    { "teamId", teamId },
                });
                return this.BadRequest(new ErrorResponse { Message = "Team Id is either null or empty" });
            }

#pragma warning disable CA1062 // Null check is handled by data annotations at model level
            if (string.IsNullOrEmpty(eventEntity.EventId))
#pragma warning restore CA1062 // Null check is handled by data annotations at model level
            {
                this.logger.LogError("Event Id is null or empty");
                this.RecordEvent("Update draft- The HTTP PATCH call to update draft has been failed", new Dictionary<string, string>
                {
                    { "eventId", eventEntity?.EventId },
                    { "teamId", teamId },
                });
                return this.BadRequest(new ErrorResponse { Message = "Event Id is null or empty" });
            }

            // Assign logged in users' AAD object Id as it is available in controllers. All other properties will be mapped by EventWorkflowHelper.
            eventEntity.TeamId = teamId;
            eventEntity.UpdatedBy = this.UserAadId;

            try
            {
                var updateResult = await this.eventWorkflowHelper.UpdateDraftEventAsync(eventEntity);

                if (updateResult == null)
                {
                    this.RecordEvent("Update draft- The HTTP PATCH call to update draft has been failed", new Dictionary<string, string>
                    {
                        { "eventId", eventEntity?.EventId },
                        { "teamId", teamId },
                    });
                    this.logger.LogError($"Event {eventEntity.EventId} not found for team {teamId}");
                    return this.NotFound(new ErrorResponse { Message = $"Event {eventEntity.EventId} not found for team {teamId}" });
                }

                if (!(bool)updateResult)
                {
                    this.RecordEvent("Update draft- The HTTP PATCH call to update draft has been failed", new Dictionary<string, string>
                    {
                        { "eventId", eventEntity?.EventId },
                        { "teamId", teamId },
                    });
                    this.logger.LogInformation($"Failed to update draft event {eventEntity.EventId} for user {this.UserAadId} and team {teamId}");
                }

                this.RecordEvent("Update draft- The HTTP PATCH call to update draft has been succeeded", new Dictionary<string, string>
                {
                    { "eventId", eventEntity?.EventId },
                    { "teamId", teamId },
                });

                return this.Ok(updateResult);
            }
            catch (Exception ex)
            {
                this.RecordEvent("Update draft- The HTTP PATCH call to update draft has been failed", new Dictionary<string, string>
                {
                    { "eventId", eventEntity?.EventId },
                    { "teamId", teamId },
                });
                this.logger.LogError(ex, $"Error occurred while updating draft event {eventEntity.EventId} for user {this.UserAadId} and team {teamId}");
                throw;
            }
        }

        /// <summary>
        /// Save new event as draft.
        /// </summary>
        /// <param name="eventEntity">Event details entered by user.</param>
        /// <param name="teamId">Team Id for which event will be created.</param>
        /// <returns>Boolean indicating insert operation result.</returns>
        [HttpPost("create-event")]
        public async Task<IActionResult> CreateEventAsync([FromBody] EventEntity eventEntity, [FromQuery] string teamId)
        {
            this.RecordEvent("Create event- The HTTP POST call to create event has been initiated", new Dictionary<string, string>
            {
                { "eventId", eventEntity?.EventId },
                { "teamId", teamId },
            });

            if (string.IsNullOrEmpty(teamId))
            {
                this.logger.LogError("Team Id is either null or empty");
                this.RecordEvent("Create event- The HTTP POST call to create event has been failed", new Dictionary<string, string>
                {
                    { "eventId", eventEntity?.EventId },
                    { "teamId", teamId },
                });
                return this.BadRequest(new ErrorResponse { Message = "Team Id is either null or empty" });
            }

            var validationMessages = EventEntity.ValidateEventModel(eventEntity, this.localizer);
            if (validationMessages.Any())
            {
                this.logger.LogError("One or more validation failed for event details");
                this.RecordEvent("Create event- The HTTP POST call to create event has been failed", new Dictionary<string, string>
                {
                    { "eventId", eventEntity?.EventId },
                    { "teamId", teamId },
                });
                return this.BadRequest(new ErrorResponse { Message = "One or more validation errors occurred", Errors = validationMessages });
            }

            // Assign logged in users' AAD object Id as it is available in controllers. All other properties will be mapped by EventWorkflowHelper.
            eventEntity.CreatedBy = this.UserAadId;
            eventEntity.UpdatedBy = this.UserAadId;
            eventEntity.UpdatedOn = DateTime.UtcNow;
            eventEntity.TeamId = teamId;

            try
            {
                var result = await this.eventWorkflowHelper.CreateNewEventAsync(eventEntity, this.UserName);

                if (result == null)
                {
                    this.RecordEvent("Create event- The HTTP POST call to create event has been failed", new Dictionary<string, string>
                    {
                        { "eventId", eventEntity?.EventId },
                        { "teamId", teamId },
                    });
                    this.logger.LogInformation($"Event {eventEntity.EventId} could not be found for team {eventEntity.TeamId}");
                    return this.BadRequest(new ErrorResponse { Message = $"Event {eventEntity.EventId} not found for team {teamId}" });
                }

                if (!(bool)result)
                {
                    this.RecordEvent("Create event- The HTTP POST call to create event has been failed", new Dictionary<string, string>
                    {
                        { "eventId", eventEntity?.EventId },
                        { "teamId", teamId },
                    });
                    this.logger.LogInformation($"Unable to create new event {eventEntity.EventId} for team {teamId}");
                }

                this.RecordEvent("Create event- The HTTP POST call to create event has been succeeded", new Dictionary<string, string>
                {
                    { "eventId", eventEntity?.EventId },
                    { "teamId", teamId },
                });

                return this.Ok(result);
            }
            catch (Exception ex)
            {
                this.RecordEvent("Create event- The HTTP POST call to create event has been failed", new Dictionary<string, string>
                {
                    { "eventId", eventEntity?.EventId },
                    { "teamId", teamId },
                });
                this.logger.LogError(ex, $"Error occurred while creating event");
                throw;
            }
        }

        /// <summary>
        /// Gets LnD team events of particular status type for provided page number and search string entered by user.
        /// </summary>
        /// <param name="searchString">Search string entered by user.</param>
        /// <param name="pageCount">The page count for which post needs to be fetched</param>
        /// <param name="eventSearchType">The event search operation type. Refer <see cref="EventSearchType"/> for values.</param>
        /// <param name="teamId">Logged in user's team ID</param>
        /// <returns>The list of events</returns>
        [HttpGet]
        public async Task<IActionResult> GetEventsAsync(string searchString, int pageCount, int eventSearchType, string teamId)
        {
            this.RecordEvent("Get LnD Team Events- The HTTP call to GET events has been initiated");

            try
            {
                var searchParametersDto = new SearchParametersDto
                {
                    SearchString = searchString,
                    PageCount = pageCount,
                    SearchScope = (EventSearchType)eventSearchType,
                    TeamId = teamId,
                };

                var events = await this.teamEventSearchService.GetEventsAsync(searchParametersDto);

                this.RecordEvent("Get LnD Team Events- The HTTP call to GET events has succeeded");

                if (events == null || !events.Any())
                {
                    this.logger.LogInformation("The LnD team events are not available");
                    return this.Ok(new List<EventEntity>());
                }

                await this.categoryHelper.BindCategoryNameAsync(events);
                return this.Ok(events);
            }
            catch (Exception ex)
            {
                this.RecordEvent("Get LnD Team Events- The HTTP call to GET events has been failed");
                this.logger.LogError(ex, "Error occured while fetching LnD team events");
                throw;
            }
        }

        /// <summary>
        /// Handles request to update the event details
        /// </summary>
        /// <param name="eventEntity">The details of an event that needs to be updated</param>
        /// <param name="teamId">The logged-in user's team Id</param>
        /// <returns>Returns true if event details updated successfully. Else returns false.</returns>
        [HttpPatch("update-event")]
        public async Task<IActionResult> UpdateAsync([FromBody] EventEntity eventEntity, [FromQuery] string teamId)
        {
            this.RecordEvent("Update Event- The HTTP PATCH call to update event details has been initiated", new Dictionary<string, string>
            {
                { "eventId", eventEntity?.EventId },
                { "teamId", teamId },
            });

            if (string.IsNullOrEmpty(teamId))
            {
                this.logger.LogError("Team Id is either null or empty");
                this.RecordEvent("Update event- The HTTP POST call to create event has been failed", new Dictionary<string, string>
                {
                    { "eventId", eventEntity?.EventId },
                    { "teamId", teamId },
                });
                return this.BadRequest(new ErrorResponse { Message = "Team Id is either null or empty" });
            }

            try
            {
                // Validate event details.
                var validationMessages = EventEntity.ValidateEventModel(eventEntity, this.localizer, true);
                if (validationMessages.Any())
                {
                    this.logger.LogError("One or more validation failed for event details");
                    this.RecordEvent("Update event- The HTTP POST call to create event has been failed", new Dictionary<string, string>
                    {
                        { "eventId", eventEntity?.EventId },
                        { "teamId", teamId },
                    });
                    return this.BadRequest(new ErrorResponse { Message = "One or more validation errors occurred", Errors = validationMessages });
                }

                // Assign logged in users' AAD object Id as it is available in controllers. All other properties will be mapped by EventWorkflowHelper.
                eventEntity.UpdatedBy = this.UserAadId;
                eventEntity.UpdatedOn = DateTime.UtcNow;

                var result = await this.eventWorkflowHelper.UpdateEventAsync(eventEntity);

                if (result == null)
                {
                    this.RecordEvent("Update event- The HTTP PATCH call to update event has been failed", new Dictionary<string, string>
                    {
                        { "eventId", eventEntity?.EventId },
                        { "teamId", teamId },
                    });
                    this.logger.LogInformation($"Event {eventEntity.EventId} could not be found for team {eventEntity.TeamId}");
                    return this.BadRequest(new ErrorResponse { Message = $"Event {eventEntity.EventId} not found for team {teamId}" });
                }

                if (!(bool)result)
                {
                    this.RecordEvent("Update event- The HTTP POST call to create event has been failed", new Dictionary<string, string>
                    {
                        { "eventId", eventEntity?.EventId },
                        { "teamId", teamId },
                    });
                    this.logger.LogInformation($"Unable to update new event {eventEntity.EventId} for team {teamId}");
                }

                this.RecordEvent("Update event- The HTTP PATCH call to update event has been succeeded", new Dictionary<string, string>
                {
                    { "eventId", eventEntity?.EventId },
                    { "teamId", teamId },
                });

                return this.Ok(result);
            }
            catch (Exception ex)
            {
                this.RecordEvent("Update Event- The HTTP PATCH call to update event has been failed", new Dictionary<string, string>
                {
                    { "eventId", eventEntity?.EventId },
                    { "teamId", teamId },
                });
                this.logger.LogError(ex, $"Error occurred while updating event {eventEntity.EventId}");
                throw;
            }
        }

        /// <summary>
        /// Delete draft event.
        /// </summary>
        /// <param name="teamId">Team Id for which event will be created.</param>
        /// <param name="eventId">Event Id for event which needs to be deleted.</param>
        /// <returns>Boolean indicating delete operation result.</returns>
        [HttpDelete("delete-draft")]
        public async Task<IActionResult> DeleteDraftAsync(string teamId, string eventId)
        {
            this.RecordEvent("Delete draft- The HTTP DELETE call to delete draft has been initiated", new Dictionary<string, string>
            {
                { "eventId", eventId },
                { "teamId", teamId },
            });

            if (string.IsNullOrEmpty(teamId))
            {
                this.logger.LogError("Team Id is either null or empty");
                this.RecordEvent("Delete draft- The HTTP DELETE call to delete draft has been failed", new Dictionary<string, string>
                {
                    { "eventId", eventId },
                    { "teamId", teamId },
                });
                return this.BadRequest(new ErrorResponse { Message = "Team Id is either null or empty" });
            }

            if (string.IsNullOrEmpty(eventId))
            {
                this.logger.LogError("Event Id is null or empty");
                this.RecordEvent("Delete draft- The HTTP DELETE call to delete draft has been failed", new Dictionary<string, string>
                {
                    { "eventId", eventId },
                    { "teamId", teamId },
                });
                return this.BadRequest(new ErrorResponse { Message = "Event Id is null or empty" });
            }

            try
            {
                var deleteResult = await this.eventWorkflowHelper.DeleteDraftEventAsync(teamId, eventId);

                if (deleteResult == null)
                {
                    this.RecordEvent("Delete draft- The HTTP DELETE call to delete draft has been failed", new Dictionary<string, string>
                    {
                        { "eventId", eventId },
                        { "teamId", teamId },
                    });
                    this.logger.LogInformation($"Event {eventId} not found for team {teamId}");
                    return this.NotFound(new ErrorResponse { Message = $"Event {eventId} not found for team {teamId}" });
                }

                if ((bool)deleteResult)
                {
                    this.RecordEvent("Delete draft- The HTTP DELETE call to delete draft has been succeeded", new Dictionary<string, string>
                    {
                        { "eventId", eventId },
                        { "teamId", teamId },
                    });
                }
                else
                {
                    this.RecordEvent("Delete draft- The HTTP DELETE call to delete draft has been failed", new Dictionary<string, string>
                    {
                        { "eventId", eventId },
                        { "teamId", teamId },
                    });
                    this.logger.LogError($"Unable to delete draft event {eventId} for team {teamId}");
                }

                return this.Ok(deleteResult);
            }
            catch (Exception ex)
            {
                this.RecordEvent("Delete draft- The HTTP DELETE call to delete draft has been failed", new Dictionary<string, string>
                {
                    { "eventId", eventId },
                    { "teamId", teamId },
                });
                this.logger.LogError(ex, $"Error occurred while deleting draft event {eventId} not found for team {teamId}");
                throw;
            }
        }

        /// <summary>
        /// Handles request to close event registrations
        /// </summary>
        /// <param name="teamId">The LnD team Id</param>
        /// <param name="eventId">The event Id of which registrations needs to be closed</param>
        /// <returns>Returns true if event registration closed successfully. Else returns false.</returns>
        [HttpPatch("CloseEventRegistrations")]
        public async Task<IActionResult> CloseEventRegistrationsAsync(string teamId, string eventId)
        {
            try
            {
                this.RecordEvent("Close Event Registration- The HTTP PATCH call to close event registrations has been initiated", new Dictionary<string, string>
                {
                    { "eventId", eventId },
                    { "teamId", teamId },
                });

                bool isRegistrationClosedSuccessfully = await this.eventWorkflowHelper.CloseEventRegistrationsAsync(teamId, eventId, this.UserAadId);

                this.RecordEvent("Close Event Registration- The HTTP PATCH call to close event registrations has succeeded", new Dictionary<string, string>
                {
                    { "eventId", eventId },
                    { "teamId", teamId },
                });

                if (isRegistrationClosedSuccessfully)
                {
                    return this.Ok(isRegistrationClosedSuccessfully);
                }

                this.RecordEvent($"Close Event Registration- The HTTP PATCH call to close event registrations has failed", new Dictionary<string, string>
                {
                    { "eventId", eventId },
                    { "teamId", teamId },
                });
                return this.Ok(false);
            }
            catch (Exception ex)
            {
                this.RecordEvent("Close Event Registration- The HTTP PATCH call to close event registrations has failed", new Dictionary<string, string>
                {
                    { "eventId", eventId },
                    { "teamId", teamId },
                });
                this.logger.LogError(ex, $"Error occured while closing registrations for event {eventId}");
                throw;
            }
        }

        /// <summary>
        /// Handles request to cancel an event
        /// </summary>
        /// <param name="teamId">The LnD team Id</param>
        /// <param name="eventId">The event Id that needs to be cancelled</param>
        /// <returns>Returns true if event cancelled successfully. Else returns false.</returns>
        [HttpPatch("CancelEvent")]
        public async Task<IActionResult> CancelEventAsync(string teamId, string eventId)
        {
            try
            {
                this.RecordEvent("Cancel Event- The HTTP PATCH call to cancel event has been initiated", new Dictionary<string, string>
                {
                    { "eventId", eventId },
                    { "teamId", teamId },
                });

                bool isStatusUpdatedSuccessfully = await this.eventWorkflowHelper.UpdateEventStatusAsync(teamId, eventId, EventStatus.Cancelled, this.UserAadId);

                this.RecordEvent("Cancel Event- The HTTP PATCH call to cancel event has succeeded", new Dictionary<string, string>
                {
                    { "eventId", eventId },
                    { "teamId", teamId },
                });

                if (isStatusUpdatedSuccessfully)
                {
                    return this.Ok(isStatusUpdatedSuccessfully);
                }

                this.RecordEvent("Cancel Event- The HTTP PATCH call to cancel event has failed", new Dictionary<string, string>
                {
                    { "eventId", eventId },
                    { "teamId", teamId },
                });
                return this.Ok(false);
            }
            catch (Exception ex)
            {
                this.RecordEvent("Cancel Event- The HTTP PATCH call to cancel event has failed", new Dictionary<string, string>
                {
                    { "eventId", eventId },
                    { "teamId", teamId },
                });
                this.logger.LogError(ex, $"Error occured while updating event status to cancelled for event {eventId}");
                throw;
            }
        }

        /// <summary>
        /// Handles request to send reminder to the registered users for an event
        /// </summary>
        /// <param name="teamId">The LnD team Id</param>
        /// <param name="eventId">The event Id for which notification to send</param>
        /// <returns>Returns true if reminder sent successfully. Else returns false.</returns>
        [HttpPost("SendReminder")]
        public async Task<IActionResult> SendReminderAsync(string teamId, string eventId)
        {
            this.RecordEvent("Send Notification- The HTTP POST call to send notification has initiated", new Dictionary<string, string>
            {
                { "eventId", eventId },
                { "teamId", teamId },
            });

            if (string.IsNullOrEmpty(teamId))
            {
                this.logger.LogError("The team Id is null or empty");
                this.RecordEvent("Send Notification- The HTTP POST call to send notification has failed", new Dictionary<string, string>
                {
                    { "eventId", eventId },
                    { "teamId", teamId },
                });
                return this.BadRequest(new ErrorResponse { Message = "The valid team Id must be provided" });
            }

            if (string.IsNullOrEmpty(eventId))
            {
                this.logger.LogError("The event Id is null or empty");
                this.RecordEvent("Send Notification- The HTTP POST call to send notification has failed", new Dictionary<string, string>
                {
                    { "eventId", eventId },
                    { "teamId", teamId },
                });
                return this.BadRequest(new ErrorResponse { Message = "The valid event Id must be provided" });
            }

            try
            {
                await this.eventWorkflowHelper.SendReminderAsync(teamId, eventId);

                this.RecordEvent("Send Notification- The HTTP POST call to send notification has succeeded", new Dictionary<string, string>
                {
                    { "eventId", eventId },
                    { "teamId", teamId },
                });

                return this.Ok(true);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Error occured while sending notification for event {eventId}");
                this.RecordEvent("Send Notification- The HTTP POST call to send notification has failed", new Dictionary<string, string>
                {
                    { "eventId", eventId },
                    { "teamId", teamId },
                });
                throw;
            }
        }
    }
}
