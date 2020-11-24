// <copyright file="EventFilesController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.EmployeeTraining.Authentication;
    using Microsoft.Teams.Apps.EmployeeTraining.Helpers;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;
    using Microsoft.Teams.Apps.EmployeeTraining.Repositories;

    /// <summary>
    /// Exposes APIs to upload and download event files.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class EventFilesController : BaseController
    {
        /// <summary>
        /// Provider for handling Azure Blob Storage operations like uploading and deleting files from blob.
        /// </summary>
        private readonly IBlobRepository blobRepository;

        /// <summary>
        /// Sends logs to the Application Insights service.
        /// </summary>
        private readonly ILogger<EventFilesController> logger;

        /// <summary>
        /// Helper methods for CRUD operations on event.
        /// </summary>
        private readonly IEventWorkflowHelper eventWorkflowHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventFilesController"/> class.
        /// </summary>
        /// <param name="logger">The ILogger object which logs errors and information</param>
        /// <param name="telemetryClient">The Application Insights telemetry client</param>
        /// <param name="blobRepository">Repository for handling Azure Blob Storage operations like uploading and deleting files from blob.</param>
        /// <param name = "eventWorkflowHelper" > Helper methods for CRUD operations on event.</param>
        public EventFilesController(
            ILogger<EventFilesController> logger,
            TelemetryClient telemetryClient,
            IBlobRepository blobRepository,
            IEventWorkflowHelper eventWorkflowHelper)
            : base(telemetryClient)
        {
            this.logger = logger;
            this.blobRepository = blobRepository;
            this.eventWorkflowHelper = eventWorkflowHelper;
        }

        /// <summary>
        /// Method to upload event image to blob storage.
        /// </summary>
        /// <param name="fileInfo">File information to be uploaded on blob.</param>
        /// <param name="teamId">Team Id for which photo needs to upload.</param>
        /// <returns>Returns blob URL for uploaded image.</returns>
        [HttpPost("upload-image")]
        [Authorize(PolicyNames.MustBeLnDTeamMemberPolicy)]
#pragma warning disable CA1801 // Required to validate whether user is part of particular team.
        public async Task<IActionResult> UploadImageAsync(IFormFile fileInfo, string teamId)
#pragma warning restore CA1801 // Required to validate whether user is part of particular team.
        {
            this.RecordEvent("Upload image- The HTTP POST call to upload event image has been initiated");

            if (fileInfo == null)
            {
                this.logger.LogInformation("File information received for uploading to blob is null.");
                this.RecordEvent("Upload image- The HTTP POST call to upload event image has been failed");
                return this.BadRequest(new ErrorResponse { Message = "File information received for uploading to blob is null." });
            }

            try
            {
                string contentType = ContentType.GetFileContentType(fileInfo.FileName);
                using Stream fileStream = fileInfo.OpenReadStream();
                var blobUri = await this.blobRepository.UploadAsync(fileStream, contentType);

                this.RecordEvent("Upload image- The HTTP POST call to upload event image has been succeeded");

                return this.Ok(blobUri);
            }
            catch (Exception ex)
            {
                this.RecordEvent("Upload image- The HTTP POST call to upload event image has been failed");
                this.logger.LogError(ex, "Error while uploading image to blob.");
                throw;
            }
        }

        /// <summary>
        /// Handles request to export event details to CSV
        /// </summary>
        /// <param name="teamId">The LnD team Id</param>
        /// <param name="eventId">The event Id of which details needs to be exported</param>
        /// <returns>Returns CSV data in file stream</returns>
        [HttpGet("ExportEventDetailsToCSV")]
        public async Task<ActionResult> ExportEventDetailsToCSV(string teamId, string eventId)
        {
            this.RecordEvent("Export Event Details- The HTTP GET call to export event details to CSV has been initiated", new Dictionary<string, string>
            {
                { "eventId", eventId },
                { "teamId", teamId },
            });

            if (string.IsNullOrEmpty(teamId))
            {
                this.logger.LogError("The team Id is null or empty");
                this.RecordEvent("Export Event Details- The HTTP GET call to export event details to CSV has been failed", new Dictionary<string, string>
                {
                    { "eventId", eventId },
                    { "teamId", teamId },
                });
                return this.BadRequest(new ErrorResponse { Message = "The valid team Id must be provided" });
            }

            if (string.IsNullOrEmpty(eventId))
            {
                this.logger.LogError("The event Id is null or empty");
                this.RecordEvent("Export Event Details- The HTTP GET call to export event details to CSV has been failed", new Dictionary<string, string>
                {
                    { "eventId", eventId },
                    { "teamId", teamId },
                });
                return this.BadRequest(new ErrorResponse { Message = "The valid event Id must be provided" });
            }

            try
            {
                var csvData = await this.eventWorkflowHelper.ExportEventDetailsToCSVAsync(teamId, eventId);

                this.RecordEvent("Export Event Details- The HTTP GET call to export event details to CSV has succeeded", new Dictionary<string, string>
                {
                    { "eventId", eventId },
                    { "teamId", teamId },
                });

                if (csvData == null)
                {
                    return this.NoContent();
                }

                return new FileStreamResult(csvData, "text/csv");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Error occured while exporting details for event {eventId}");
                this.RecordEvent("Export Event Details- The HTTP GET call to export event details to CSV has failed", new Dictionary<string, string>
                {
                    { "eventId", eventId },
                    { "teamId", teamId },
                });
                throw;
            }
        }
    }
}
