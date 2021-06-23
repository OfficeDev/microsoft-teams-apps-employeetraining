// <copyright file="AutoRegisteredCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Cards
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Localization;
    using Microsoft.Teams.Apps.EmployeeTraining;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;

    /// <summary>
    /// Creates auto registration card attachment.
    /// </summary>
    public static class AutoRegisteredCard
    {
        /// <summary>
        /// Get adaptive card attachment for auto registered attendees.
        /// </summary>
        /// <param name="applicationBasePath">Base URL of application.</param>
        /// <param name="localizer">String localizer for localizing user facing text.</param>
        /// <param name="eventEntity">Event details which is cancelled.</param>
        /// <param name="applicationManifestId">The unique manifest ID for application</param>
        /// <returns>An adaptive card attachment.</returns>
        public static Attachment GetAutoRegisteredCard(string applicationBasePath, IStringLocalizer<Strings> localizer, EventEntity eventEntity, string applicationManifestId)
        {
            eventEntity = eventEntity ?? throw new ArgumentNullException(nameof(eventEntity), "Event details cannot be null");
            var textAlignment = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? AdaptiveHorizontalAlignment.Right : AdaptiveHorizontalAlignment.Left;

            AdaptiveCard autoRegisteredCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2))
            {
                Body = new List<AdaptiveElement>
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
                                        Text = $"**{localizer.GetString("AutoRegisteredHeader")}**",
                                        Size = AdaptiveTextSize.Large,
                                        Weight = AdaptiveTextWeight.Bolder,
                                        HorizontalAlignment = textAlignment,
                                    },
                                },
                            },
                            new AdaptiveColumn
                            {
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveImage
                                    {
                                        Url = new Uri($"{applicationBasePath}/images/Mandatory.png"),
                                        PixelWidth = 96,
                                        PixelHeight = 32,
                                        Spacing = AdaptiveSpacing.None,
                                        HorizontalAlignment = AdaptiveHorizontalAlignment.Right,
                                    },
                                },
                            },
                        },
                    },
                    new AdaptiveTextBlock
                    {
                        Text = $"{localizer.GetString("AutoRegisteredCardSubTitle")}",
                        Wrap = true,
                        Size = AdaptiveTextSize.Small,
                        Spacing = AdaptiveSpacing.None,
                        HorizontalAlignment = textAlignment,
                    },
                    new AdaptiveColumnSet
                    {
                        Spacing = AdaptiveSpacing.Small,
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Height = AdaptiveHeight.Auto,
                                Width = AdaptiveColumnWidth.Auto,
                                Items = !string.IsNullOrEmpty(eventEntity.Photo) ? new List<AdaptiveElement>
                                {
                                     new AdaptiveImage
                                     {
                                        Url = new Uri(eventEntity.Photo),
                                        HorizontalAlignment = textAlignment,
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
                                        Text = eventEntity.Name,
                                        Size = AdaptiveTextSize.Default,
                                        Weight = AdaptiveTextWeight.Bolder,
                                        HorizontalAlignment = textAlignment,
                                    },
                                    new AdaptiveTextBlock
                                    {
                                        Text = eventEntity.CategoryName,
                                        Wrap = true,
                                        Size = AdaptiveTextSize.Small,
                                        Weight = AdaptiveTextWeight.Bolder,
                                        Color = AdaptiveTextColor.Warning,
                                        Spacing = AdaptiveSpacing.Small,
                                        HorizontalAlignment = textAlignment,
                                    },
                                },
                            },
                        },
                    },
                    new AdaptiveColumnSet
                    {
                        Spacing = AdaptiveSpacing.Medium,
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Width = "100px",
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = $"**{localizer.GetString("DateAndTimeLabel")}:**",
                                        Wrap = true,
                                        Weight = AdaptiveTextWeight.Bolder,
                                        Size = AdaptiveTextSize.Small,
                                        HorizontalAlignment = textAlignment,
                                    },
                                },
                            },
                            new AdaptiveColumn
                            {
                                Spacing = AdaptiveSpacing.None,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = string.Format(CultureInfo.CurrentCulture, "{0} {1}-{2}", "{{DATE(" + eventEntity.StartDate.Value.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'", CultureInfo.InvariantCulture) + ", SHORT)}}", "{{TIME(" + eventEntity.StartTime.Value.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture) + ")}}", "{{TIME(" + eventEntity.EndTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture) + ")}}"),
                                        Wrap = true,
                                        Size = AdaptiveTextSize.Small,
                                        HorizontalAlignment = textAlignment,
                                    },
                                },
                            },
                        },
                    },
                    new AdaptiveColumnSet
                    {
                        Spacing = AdaptiveSpacing.Small,
                        Columns = eventEntity.Type != (int)EventType.InPerson ? new List<AdaptiveColumn>() : new List<AdaptiveColumn>
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
                                        Size = AdaptiveTextSize.Small,
                                        HorizontalAlignment = textAlignment,
                                     },
                                },
                            },
                            new AdaptiveColumn
                            {
                                Spacing = AdaptiveSpacing.None,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = eventEntity.Venue,
                                        Wrap = true,
                                        Size = AdaptiveTextSize.Small,
                                        HorizontalAlignment = textAlignment,
                                    },
                                },
                            },
                        },
                    },
                    new AdaptiveColumnSet
                    {
                        Spacing = AdaptiveSpacing.Small,
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
                                        Size = AdaptiveTextSize.Small,
                                        HorizontalAlignment = textAlignment,
                                    },
                                },
                            },
                            new AdaptiveColumn
                            {
                                Spacing = AdaptiveSpacing.None,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = eventEntity.Description,
                                        Wrap = true,
                                        Size = AdaptiveTextSize.Small,
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
                                    new AdaptiveActionSet
                                    {
                                        Actions = new List<AdaptiveAction>
                                        {
                                            new AdaptiveOpenUrlAction
                                            {
                                                Url = new Uri($"https://teams.microsoft.com/l/entity/{applicationManifestId}/my-events"),
                                                Title = $"{localizer.GetString("ReminderCardRegisteredEventButton")}",
                                            },
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = autoRegisteredCard,
            };
        }
    }
}