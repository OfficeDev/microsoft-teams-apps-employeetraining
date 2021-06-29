// <copyright file="MessagingExtensionCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Cards
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Web;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Localization;
    using Microsoft.Teams.Apps.EmployeeTraining;
    using Microsoft.Teams.Apps.EmployeeTraining.Common;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;
    using Attachment = Microsoft.Bot.Schema.Attachment;
    using EventType = Microsoft.Teams.Apps.EmployeeTraining.Models.EventType;

    /// <summary>
    /// Holds the method which returns reminder card
    /// </summary>
    public static class MessagingExtensionCard
    {
        /// <summary>
        /// Sets the maximum number of characters for project title.
        /// </summary>
        private const int TitleMaximumLength = 40;

        /// <summary>
        /// Sets the maximum number of characters for project title.
        /// </summary>
        private const int CategoryMaximumLength = 20;

        /// <summary>
        /// Sets the maximum number of characters for project title.
        /// </summary>
        private const int LocationMaximumLength = 30;

        /// <summary>
        /// Get projects result for Messaging Extension.
        /// </summary>
        /// <param name="events">List of user search result.</param>
        /// <param name="applicationBasePath">Application base URL.</param>
        /// <param name="localizer">The localizer for localizing content</param>
        /// <param name="localDateTime">Indicates local date and time of end user.</param>
        /// <returns>If event details provided, then returns reminder card. Else returns empty card.</returns>
        public static MessagingExtensionResult GetCard(IEnumerable<EventEntity> events, string applicationBasePath, IStringLocalizer<Strings> localizer, DateTimeOffset? localDateTime)
        {
            events = events ?? throw new ArgumentNullException(nameof(events), "Event list cannot be null");

            MessagingExtensionResult composeExtensionResult = new MessagingExtensionResult
            {
                Type = "result",
                AttachmentLayout = AttachmentLayoutTypes.List,
                Attachments = new List<MessagingExtensionAttachment>(),
            };

            foreach (var eventDetails in events)
            {
                var card = GetEventDetailsAdaptiveCard(eventDetails, localizer, applicationBasePath);

                var previewCard = GetThumbnailCard(eventDetails, localDateTime, localizer);

                composeExtensionResult.Attachments.Add(new Attachment
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = card,
                }.ToMessagingExtensionAttachment(previewCard.ToAttachment()));
            }

            return composeExtensionResult;
        }

        /// <summary>
        /// Returns local date time for user by adding local timestamp (received from bot activity) offset to targeted date.
        /// </summary>
        /// <param name="dateTime">The date and time which needs to be converted to user local time.</param>
        /// <param name="userLocalTime">The sender's local time, as determined by the local timestamp of the activity.</param>
        /// <returns>User's local date and time.</returns>
        private static DateTime GetFormattedDateInUserTimeZone(DateTime dateTime, DateTimeOffset? userLocalTime)
        {
            // Adaptive card on mobile has a bug where it does not support DATE and TIME, so for now we convert the date and time manually.
            return dateTime.Add(userLocalTime?.Offset ?? TimeSpan.FromMinutes(0));
        }

        /// <summary>
        /// Create event details adaptive to be shown in compose box.
        /// </summary>
        /// <param name="eventDetails">Event details.</param>
        /// <param name="localizer">The localizer for localizing content</param>
        /// <param name="applicationBasePath">Application base URL.</param>
        /// <returns>An adaptive card with event details.</returns>
        private static AdaptiveCard GetEventDetailsAdaptiveCard(EventEntity eventDetails, IStringLocalizer<Strings> localizer, string applicationBasePath)
        {
            var textAlignment = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? AdaptiveHorizontalAlignment.Right : AdaptiveHorizontalAlignment.Left;
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveColumnSet
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Height = AdaptiveHeight.Auto,
                                Width = AdaptiveColumnWidth.Auto,
                                Items = !string.IsNullOrEmpty(eventDetails.Photo) ? new List<AdaptiveElement>
                                {
                                    new AdaptiveImage
                                    {
                                        Url = new Uri(eventDetails.Photo),
                                        HorizontalAlignment = AdaptiveHorizontalAlignment.Left,
                                        PixelHeight = 50,
                                        PixelWidth = 50,
                                    },
                                }
                                :
                                new List<AdaptiveElement>(),
                            },
                            new AdaptiveColumn
                            {
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = eventDetails.Name,
                                        Size = AdaptiveTextSize.Large,
                                        Weight = AdaptiveTextWeight.Bolder,
                                        HorizontalAlignment = textAlignment,
                                    },
                                    new AdaptiveTextBlock
                                    {
                                        Text = eventDetails.CategoryName,
                                        Wrap = true,
                                        Size = AdaptiveTextSize.Default,
                                        Weight = AdaptiveTextWeight.Bolder,
                                        Color = AdaptiveTextColor.Attention,
                                        Spacing = AdaptiveSpacing.Small,
                                        HorizontalAlignment = textAlignment,
                                    },
                                },
                            },
                        },
                    },
                    new AdaptiveColumnSet
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Width = "100px",
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = $"**{localizer.GetString("DateAndTimeLabel")}:** ",
                                        Wrap = true,
                                        HorizontalAlignment = textAlignment,
                                    },
                                },
                            },
                            new AdaptiveColumn
                            {
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = string.Format(CultureInfo.CurrentCulture, "{0} {1}-{2}", "{{DATE(" + eventDetails.StartDate.Value.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'", CultureInfo.InvariantCulture) + ", SHORT)}}", "{{TIME(" + eventDetails.StartTime.Value.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture) + ")}}", "{{TIME(" + eventDetails.EndTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture) + ")}}"),
                                        Wrap = true,
                                        HorizontalAlignment = textAlignment,
                                    },
                                },
                            },
                        },
                    },
                    new AdaptiveColumnSet
                    {
                        Spacing = AdaptiveSpacing.None,
                        IsVisible = eventDetails.Type == (int)EventType.InPerson,
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Width = "100px",
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = $"**{localizer.GetString("Venue")}:** ",
                                        Wrap = true,
                                        Weight = AdaptiveTextWeight.Bolder,
                                        HorizontalAlignment = textAlignment,
                                    },
                                },
                            },
                            new AdaptiveColumn
                            {
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = eventDetails.Venue,
                                        Wrap = true,
                                        HorizontalAlignment = textAlignment,
                                    },
                                },
                            },
                        },
                    },
                    new AdaptiveColumnSet
                    {
                        Spacing = AdaptiveSpacing.None,
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Width = "100px",
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = $"**{localizer.GetString("DescriptionLabelCard")}:** ",
                                        Wrap = true,
                                        Weight = AdaptiveTextWeight.Bolder,
                                        HorizontalAlignment = textAlignment,
                                    },
                                },
                            },
                            new AdaptiveColumn
                            {
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = eventDetails.Description,
                                        Wrap = true,
                                        HorizontalAlignment = textAlignment,
                                    },
                                },
                            },
                        },
                    },
                    new AdaptiveColumnSet
                    {
                        Spacing = AdaptiveSpacing.ExtraLarge,
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveImage
                                    {
                                        IsVisible = eventDetails.Audience == (int)EventAudience.Private,
                                        Url = new Uri($"{applicationBasePath}/images/Private.png"),
                                        PixelWidth = 84,
                                        PixelHeight = 32,
                                        HorizontalAlignment = AdaptiveHorizontalAlignment.Left,
                                    },
                                },
                            },
                        },
                    },
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = localizer.GetString("RegisterButton"),
                        Data = new AdaptiveSubmitActionData
                        {
                            MsTeams = new CardAction
                            {
                                Type = "task/fetch",
                                Text = localizer.GetString("RegisterButton"),
                            },
                            Command = BotCommands.RegisterForEvent,
                            EventId = eventDetails.EventId,
                            TeamId = eventDetails.TeamId,
                        },
                    },
                },
            };

            return card;
        }

        /// <summary>
        /// Create thumbnail card for messaging extension.
        /// </summary>
        /// <param name="eventDetails">Event details.</param>
        /// <param name="localDateTime">The sender's local time, as determined by the local timestamp of the activity.</param>
        /// <param name="localizer">Localization of strings</param>
        /// <returns>Thumbnail card.</returns>
        private static ThumbnailCard GetThumbnailCard(EventEntity eventDetails, DateTimeOffset? localDateTime, IStringLocalizer<Strings> localizer)
        {
            var titleString = eventDetails.Name.Length < TitleMaximumLength ? HttpUtility.HtmlEncode(eventDetails.Name) :
                $"{HttpUtility.HtmlEncode(eventDetails.Name.Substring(0, TitleMaximumLength))}...";
            var categoryString = !string.IsNullOrEmpty(eventDetails.CategoryName) ? eventDetails.CategoryName.Length < CategoryMaximumLength ? HttpUtility.HtmlEncode(eventDetails.CategoryName) :
                $"{HttpUtility.HtmlEncode(eventDetails.CategoryName.Substring(0, CategoryMaximumLength))}..." : string.Empty;
            var locationString = string.Empty;

            if (!string.IsNullOrEmpty(eventDetails.Venue))
            {
                locationString = eventDetails.Venue.Length < LocationMaximumLength ? HttpUtility.HtmlEncode(eventDetails.Venue) :
                    $"{HttpUtility.HtmlEncode(eventDetails.Venue.Substring(0, LocationMaximumLength))}...";
            }
            else
            {
                switch ((EventType)eventDetails.Type)
                {
                    case EventType.InPerson:
                        locationString = $"{localizer.GetString("TrainingTypeInPerson")}";
                        break;
                    case EventType.Teams:
                        locationString = $"{localizer.GetString("TeamsMeetingText")}";
                        break;
                    case EventType.LiveEvent:
                        locationString = $"{localizer.GetString("TrainingTypeLiveEvent")}";
                        break;
                }
            }

            var startDateInUserLocalTime = GetFormattedDateInUserTimeZone(eventDetails.StartDate.Value, localDateTime);
            var startTimeInUserLocalTime = GetFormattedDateInUserTimeZone(eventDetails.StartTime.Value, localDateTime);
            var endTimeInUserLocalTime = GetFormattedDateInUserTimeZone(eventDetails.EndTime, localDateTime);

            var trainingStartDateString = startDateInUserLocalTime.ToString("d", CultureInfo.CurrentCulture);
            var trainingStartTimeString = startTimeInUserLocalTime.ToString("t", CultureInfo.CurrentCulture);
            var trainingEndTimeString = endTimeInUserLocalTime.ToString("t", CultureInfo.CurrentCulture);

            var text = (EventAudience)eventDetails.Audience == EventAudience.Private
                ? $"<span style='color: #A72037; font-weight: 600;'>{HttpUtility.HtmlEncode(categoryString)} &nbsp;|</span>" +
                $"<span style='font-weight: 600;'>&nbsp;{HttpUtility.HtmlEncode(locationString)}&nbsp;|</span>" +
                $"<span style='font-weight: 600;'>&nbsp;{HttpUtility.HtmlEncode(localizer.GetString("AudiencePrivate"))}</span><br/>" +
                $"<span style='font-size: 11px; line-height: 22px;'>{HttpUtility.HtmlEncode(trainingStartDateString)}, " +
                $"{HttpUtility.HtmlEncode(trainingStartTimeString)}-{HttpUtility.HtmlEncode(trainingEndTimeString)}</span>"
                :
                $"<span style='color: #A72037; font-weight: 600;'>{HttpUtility.HtmlEncode(categoryString)} &nbsp;|</span>" +
                $"<span style='font-weight: 600;'>&nbsp;{HttpUtility.HtmlEncode(locationString)}</span><br/>" +
                $"<span style='font-size: 11px; line-height: 22px;'>{HttpUtility.HtmlEncode(trainingStartDateString)}, " +
                $"{HttpUtility.HtmlEncode(trainingStartTimeString)}-{HttpUtility.HtmlEncode(trainingEndTimeString)}</span>";

            return new ThumbnailCard
            {
                Title = $"<span style='font-weight: 600;'>{HttpUtility.HtmlEncode(titleString)}</span>",
                Text = text,
            };
        }
    }
}