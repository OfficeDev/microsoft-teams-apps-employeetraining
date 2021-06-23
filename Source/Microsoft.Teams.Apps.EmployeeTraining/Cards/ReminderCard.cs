// <copyright file="ReminderCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Cards
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Localization;
    using Microsoft.Teams.Apps.EmployeeTraining;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;

    /// <summary>
    /// Holds the method which returns reminder card
    /// </summary>
    public static class ReminderCard
    {
        /// <summary>
        /// Gets the reminder card with event details
        /// </summary>
        /// <param name="events">The list of events</param>
        /// <param name="localizer">The localizer for localizing content</param>
        /// <param name="applicationManifestId">Unique manifest Id used for side-loading app</param>
        /// <param name="notificationType">The type of notification being sent</param>
        /// <returns>If event details provided, then returns reminder card. Else returns empty card.</returns>
        public static Attachment GetCard(IEnumerable<EventEntity> events, IStringLocalizer<Strings> localizer, string applicationManifestId, NotificationType notificationType = NotificationType.Manual)
        {
            var textAlignment = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? AdaptiveHorizontalAlignment.Right : AdaptiveHorizontalAlignment.Left;
            if (events == null || !events.Any())
            {
                return new Attachment();
            }

            var cardTitle = string.Empty;

            switch (notificationType)
            {
                case NotificationType.Daily:
                    cardTitle = localizer.GetString("DailyReminderCardTitle");
                    break;

                case NotificationType.Weekly:
                    cardTitle = localizer.GetString("WeeklyReminderCardTitle");
                    break;

                default:
                    cardTitle = localizer.GetString("ReminderCardTitle");
                    break;
            }

            var cardBody = new List<AdaptiveElement>
            {
                new AdaptiveColumnSet
                {
                    Columns = new List<AdaptiveColumn>
                    {
                        new AdaptiveColumn
                        {
                            Items = new List<AdaptiveElement>
                            {
                                new AdaptiveTextBlock
                                {
                                    Text = cardTitle,
                                    Wrap = true,
                                    Size = AdaptiveTextSize.Large,
                                    Weight = AdaptiveTextWeight.Bolder,
                                    HorizontalAlignment = textAlignment,
                                },
                            },
                        },
                    },
                },
            };

            cardBody.AddRange(GetReminderCardElements(events, localizer).Select(cardElement => cardElement));

            AdaptiveCard reminderCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2))
            {
                Body = cardBody,
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveOpenUrlAction
                    {
                        Title = $"{localizer.GetString("ReminderCardRegisteredEventButton")}",
                        Url = new Uri($"https://teams.microsoft.com/l/entity/{applicationManifestId}/my-events"), // Open My events tab (deep link).
                    },
                },
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = reminderCard,
            };
        }

        /// <summary>
        /// Gets reminder card elements
        /// </summary>
        /// <param name="events">The list of events</param>
        /// <param name="localizer">The localizer for localizing content</param>
        /// <returns>Returns reminder card elements</returns>
        private static List<AdaptiveElement> GetReminderCardElements(IEnumerable<EventEntity> events, IStringLocalizer<Strings> localizer)
        {
            var textAlignment = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? AdaptiveHorizontalAlignment.Right : AdaptiveHorizontalAlignment.Left;
            List<AdaptiveElement> cardElements = new List<AdaptiveElement>();

            foreach (var eventDetails in events)
            {
                AdaptiveColumnSet adaptiveColumnSet = new AdaptiveColumnSet
                {
                    Columns = new List<AdaptiveColumn>
                    {
                        new AdaptiveColumn
                        {
                            Width = "45px",
                            PixelMinHeight = 45,
                            Items = !string.IsNullOrEmpty(eventDetails.Photo) ? new List<AdaptiveElement>
                            {
                                new AdaptiveImage
                                {
                                    Url = new Uri(eventDetails.Photo),
                                    HorizontalAlignment = AdaptiveHorizontalAlignment.Left,
                                    PixelHeight = 45,
                                    PixelWidth = 45,
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
                                    Weight = AdaptiveTextWeight.Bolder,
                                    Size = AdaptiveTextSize.Small,
                                    HorizontalAlignment = textAlignment,
                                },
                                new AdaptiveColumnSet
                                {
                                    Spacing = AdaptiveSpacing.None,
                                    Columns = new List<AdaptiveColumn>
                                    {
                                        new AdaptiveColumn
                                        {
                                            Width = AdaptiveColumnWidth.Auto,
                                            Items = new List<AdaptiveElement>
                                            {
                                                new AdaptiveTextBlock
                                                {
                                                    Text = eventDetails.CategoryName,
                                                    Wrap = true,
                                                    Color = AdaptiveTextColor.Warning,
                                                    Size = AdaptiveTextSize.Small,
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
                                                    Text = "| " + (eventDetails.Type == (int)EventType.InPerson ? eventDetails.Venue : localizer.GetString("TeamsMeetingText")),
                                                    Wrap = true,
                                                    HorizontalAlignment = textAlignment,
                                                    Size = AdaptiveTextSize.Small,
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
                                            Width = AdaptiveColumnWidth.Auto,
                                            Items = new List<AdaptiveElement>
                                            {
                                                new AdaptiveTextBlock
                                                {
                                                    Text = string.Format(CultureInfo.CurrentCulture, "{0} {1}-{2}", "{{DATE(" + eventDetails.StartDate.Value.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'", CultureInfo.InvariantCulture) + ", SHORT)}}", "{{TIME(" + eventDetails.StartTime.Value.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture) + ")}}", "{{TIME(" + eventDetails.EndTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture) + ")}}"),
                                                    Wrap = true,
                                                    Size = AdaptiveTextSize.Small,
                                                    HorizontalAlignment = textAlignment,
                                                },
                                            },
                                        },
                                    },
                                },
                            },
                        },
                    },
                };

                cardElements.Add(adaptiveColumnSet);
            }

            return cardElements;
        }
    }
}