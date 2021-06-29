// <copyright file="EventEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.Azure.Search;
    using Microsoft.Extensions.Localization;

    /// <summary>
    /// This class is responsible to store the event details.
    /// </summary>
    public class EventEntity : TableEntity
    {
        /// <summary>
        /// Gets or sets the event Id GUID.
        /// </summary>
        [Key]
        public string EventId
        {
            get { return this.RowKey; }
            set { this.RowKey = value; }
        }

        /// <summary>
        /// Gets or sets team Id.
        /// </summary>
        [IsFilterable]
        public string TeamId
        {
            get { return this.PartitionKey; }
            set { this.PartitionKey = value; }
        }

        /// <summary>
        /// Gets or sets event Id received from Graph.
        /// </summary>
        public string GraphEventId { get; set; }

        /// <summary>
        /// Gets or sets activity Id of card sent in LnD team chat. This is used to update card when event details are updated.
        /// </summary>
        public string TeamCardActivityId { get; set; }

        /// <summary>
        /// Gets or sets an event name
        /// </summary>
        [Required]
        [MaxLength(100)]
        [IsSearchable]
        [IsFilterable]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets event description.
        /// </summary>
        [IsSearchable]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets event photo.
        /// </summary>
        public string Photo { get; set; }

        /// <summary>
        /// Gets or sets event color
        /// </summary>
        public string SelectedColor { get; set; }

        /// <summary>
        /// Gets or sets start date and time of an event.
        /// </summary>
        [IsFilterable]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets start time of an event.
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// Gets or sets end time of an event.
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Gets or sets end date and time of an event.
        /// </summary>
        [IsFilterable]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an event get registered automatically.
        /// </summary>
        public bool IsAutoRegister { get; set; }

        /// <summary>
        /// Gets or sets the event type. Ref: <see cref="EventType"/>
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// Gets or sets the meeting link for an event.
        /// </summary>
        public string MeetingLink { get; set; }

        /// <summary>
        /// Gets or sets the venue for an event.
        /// </summary>
        [IsFilterable]
        public string Venue { get; set; }

        /// <summary>
        /// Gets or sets the event category Id.
        /// </summary>
        [IsFilterable]
        public string CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the event category name.
        /// </summary>
        [NotMapped]
        public string CategoryName { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of participants who can join the event.
        /// </summary>
        public int MaximumNumberOfParticipants { get; set; }

        /// <summary>
        /// Gets or sets the audience to which event is visible. Ref: <see cref="EventAudience"/>
        /// </summary>
        [IsFilterable]
        public int Audience { get; set; }

        /// <summary>
        /// Gets or sets registered attendees count for an event used for searching and filtering.
        /// </summary>
        [IsFilterable]
        [IsSortable]
        public int RegisteredAttendeesCount { get; set; }

        /// <summary>
        /// Gets or sets semicolon separated user object identifiers for users for whom event is mandatory.
        /// </summary>
        [IsFilterable]
        [IsSearchable]
        public string MandatoryAttendees { get; set; }

        /// <summary>
        /// Gets or sets semicolon separated user object identifiers for users for whom event is optional.
        /// </summary>
        [IsFilterable]
        [IsSearchable]
        public string OptionalAttendees { get; set; }

        /// <summary>
        /// Gets or sets semicolon separated user object identifiers for users who registered for the event.
        /// </summary>
        [IsFilterable]
        [IsSearchable]
        public string RegisteredAttendees { get; set; }

        /// <summary>
        /// Gets or sets semicolon separated user object identifiers for users who auto registered for the event.
        /// </summary>
        [IsFilterable]
        [IsSearchable]
        public string AutoRegisteredAttendees { get; set; }

        /// <summary>
        /// Gets or sets selected list of users or groups in JSON string (used to persist selected groups and user information while editing event).
        /// </summary>
        public string SelectedUserOrGroupListJSON { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether event has been deleted.
        /// </summary>
        [IsFilterable]
        public bool IsRemoved { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether event registration has been closed.
        /// </summary>
        [IsFilterable]
        public bool IsRegistrationClosed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the event is mandatory for logged-in user
        /// </summary>
        [NotMapped]
        public bool IsMandatoryForLoggedInUser { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a logged-in user registered for an event
        /// </summary>
        [NotMapped]
        public bool IsLoggedInUserRegistered { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a logged-in user can register for an event
        /// </summary>
        [NotMapped]
        public bool CanLoggedInUserRegister { get; set; }

        /// <summary>
        /// Gets or sets the status of an event. Ref: <see cref="EventStatus"/>
        /// </summary>
        [IsFilterable]
        public int Status { get; set; }

        /// <summary>
        /// Gets or sets the date and time on which the event has created.
        /// </summary>
        [IsSortable]
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// Gets or sets the user name who created the event.
        /// </summary>
        [IsFilterable]
        public string CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the date and time on which the event details updated.
        /// </summary>
        public DateTime UpdatedOn { get; set; }

        /// <summary>
        /// Gets or sets the user name who updated the event details.
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Gets or sets the number of occurrences for event.
        /// </summary>
        [NotMapped]
        public int NumberOfOccurrences { get; set; }

        /// <summary>
        /// Gets or sets the total count of recent collaborators of logged-in user
        /// </summary>
        [NotMapped]
        public int LoggedInUserCollaboratorsCount { get; set; }

        /// <summary>
        /// Validate event details model.
        /// </summary>
        /// <param name="eventDetails">Event details which needs to be saved.</param>
        /// <param name="localizer">The current culture's string localizer.</param>
        /// <param name="isUpdate">Set whether an event to be updated</param>
        /// <returns>Error message if any or null.</returns>
        public static List<string> ValidateEventModel(EventEntity eventDetails, IStringLocalizer<Strings> localizer, bool isUpdate = false)
        {
            const short EventVenueMaxLetters = 200;
            const short EventDescriptionMaxLetters = 1000;

            eventDetails = eventDetails ?? throw new ArgumentNullException(nameof(eventDetails), "Event details is null");

            Uri uriResult;
            List<string> validationMessages = new List<string>();

            if (string.IsNullOrEmpty(eventDetails.Name))
            {
                validationMessages.Add(localizer.GetString("ErrorEventNameRequired"));
            }

            // Description validation
            if (string.IsNullOrEmpty(eventDetails.Description))
            {
                validationMessages.Add(localizer.GetString("ErrorEventDescriptionRequired"));
            }
            else if (eventDetails.Description.Length > EventDescriptionMaxLetters)
            {
                validationMessages.Add(localizer.GetString("ErrorEventDescriptionLength", EventDescriptionMaxLetters));
            }

            // Either image URL or color is required.
            if (string.IsNullOrEmpty(eventDetails.Photo))
            {
                if (string.IsNullOrEmpty(eventDetails.SelectedColor))
                {
                    validationMessages.Add(localizer.GetString("ErrorEventPhotoOrColorRequired"));
                }
            }

            // If user selected image then validate URL.
            if (!string.IsNullOrEmpty(eventDetails.Photo))
            {
                bool result = Uri.TryCreate(eventDetails.Photo, UriKind.Absolute, out uriResult) &&
                    (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                if (!result)
                {
                    validationMessages.Add(localizer.GetString("ErrorEventPhotoURLInvalid"));
                }
            }

            // Start date validation
            if (eventDetails.StartDate == null)
            {
                validationMessages.Add(localizer.GetString("ErrorEventStartDateRequired"));
            }
            else if (!(isUpdate && eventDetails.StartDate < DateTime.UtcNow))
            {
                if (eventDetails.StartDate?.Date < DateTime.UtcNow.Date)
                {
                    validationMessages.Add(localizer.GetString("ErrorEventStartDateInvalid"));
                }

                // Event time validation
                if (eventDetails.StartTime == null)
                {
                    validationMessages.Add(localizer.GetString("ErrorEventStartTimeRequired"));
                }
                else if (eventDetails.EndTime == null)
                {
                    validationMessages.Add(localizer.GetString("ErrorEventEndTimeRequired"));
                }
                else
                {
                    if (eventDetails.EndTime < eventDetails.StartTime)
                    {
                        validationMessages.Add(localizer.GetString("ErrorEventEndTimeInvalid"));
                    }
                }
            }

            // Event type validation
            if (eventDetails.Type < (int)EventType.InPerson || eventDetails.Type > (int)EventType.LiveEvent)
            {
                validationMessages.Add(localizer.GetString("ErrorEventTypeInvalid"));
            }

            // Meeting link validation
            if (eventDetails.Type == (int)EventType.LiveEvent)
            {
                if (string.IsNullOrEmpty(eventDetails.MeetingLink))
                {
                    validationMessages.Add(localizer.GetString("ErrorMeetingLinkRequired"));
                }

                var result = Uri.TryCreate(eventDetails.MeetingLink, UriKind.Absolute, out uriResult) &&
                (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

                if (!result)
                {
                    validationMessages.Add(localizer.GetString("ErrorMeetingLinkInvalid"));
                }
            }

            // Venue validation
            if (eventDetails.Type == (int)EventType.InPerson)
            {
                if (string.IsNullOrEmpty(eventDetails.Venue))
                {
                    validationMessages.Add(localizer.GetString("ErrorEventVenueRequired"));
                }

                if (eventDetails.Venue.Length > EventVenueMaxLetters)
                {
                    validationMessages.Add(localizer.GetString("ErrorEventVenueInvalid", EventVenueMaxLetters));
                }
            }

            // Event category validation
            if (string.IsNullOrEmpty(eventDetails.CategoryId))
            {
                validationMessages.Add(localizer.GetString("ErrorEventCategoryRequired"));
            }

            // Max participants validation
            if (eventDetails.MaximumNumberOfParticipants < 1)
            {
                validationMessages.Add(localizer.GetString("ErrorEventMaxParticipantsInvalid"));
            }

            // Audience validation
            if (eventDetails.Audience < (int)EventAudience.Public || eventDetails.Audience > (int)EventAudience.Private)
            {
                validationMessages.Add(localizer.GetString("ErrorEventAudienceInvalid"));
            }

            return validationMessages;
        }

        /// <summary>
        /// Gets the event attendees
        /// </summary>
        /// <returns>Returns the list of event attendees.</returns>
        public IEnumerable<string> GetAttendees()
        {
            var eventAttendees = new List<string>();

            if (this.RegisteredAttendeesCount > 0)
            {
                if (!string.IsNullOrEmpty(this.RegisteredAttendees))
                {
                    eventAttendees.AddRange(this.RegisteredAttendees.Split(";"));
                }

                if (!string.IsNullOrEmpty(this.AutoRegisteredAttendees))
                {
                    eventAttendees.AddRange(this.AutoRegisteredAttendees.Split(";"));
                }
            }

            return eventAttendees;
        }
    }
}