// <copyright file="WelcomeCard.cs" company="Microsoft">
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
    using Microsoft.Teams.Apps.EmployeeTraining.Common;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;

    /// <summary>
    /// Class that helps to return welcome card as attachment.
    /// </summary>
    public static class WelcomeCard
    {
        /// <summary>
        /// Get welcome card attachment to show on Microsoft Teams channel scope.
        /// </summary>
        /// <param name="applicationBasePath">Application base path to get the logo of the application.</param>
        /// <param name="localizer">The current cultures' string localizer.</param>
        /// <returns>Team's welcome card as attachment.</returns>
        public static Attachment GetWelcomeCardAttachmentForTeam(string applicationBasePath, IStringLocalizer<Strings> localizer)
        {
            var textAlignment = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? AdaptiveHorizontalAlignment.Right : AdaptiveHorizontalAlignment.Left;
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveColumnSet
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Width = AdaptiveColumnWidth.Auto,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveImage
                                    {
                                        Url = new Uri($"{applicationBasePath}/images/logo.png"),
                                        Size = AdaptiveImageSize.Medium,
                                    },
                                },
                            },
                            new AdaptiveColumn
                            {
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Weight = AdaptiveTextWeight.Bolder,
                                        Spacing = AdaptiveSpacing.None,
                                        Text = localizer.GetString("WelcomeCardTitle"),
                                        Wrap = true,
                                        HorizontalAlignment = textAlignment,
                                    },
                                    new AdaptiveTextBlock
                                    {
                                        Spacing = AdaptiveSpacing.None,
                                        Text = localizer.GetString("WelcomeCardTeamIntro"),
                                        Wrap = true,
                                        IsSubtle = true,
                                        HorizontalAlignment = textAlignment,
                                    },
                                },
                                Width = AdaptiveColumnWidth.Stretch,
                            },
                        },
                    },
                    new AdaptiveTextBlock
                    {
                        Text = localizer.GetString("WelcomeCardTeamHeading"),
                        Wrap = true,
                        HorizontalAlignment = textAlignment,
                    },
                    new AdaptiveTextBlock
                    {
                        Spacing = AdaptiveSpacing.Medium,
                        Text = localizer.GetString("WelcomeCardTeamPoint1"),
                        Wrap = true,
                        HorizontalAlignment = textAlignment,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = localizer.GetString("WelcomeCardTeamPoint2"),
                        Wrap = true,
                        HorizontalAlignment = textAlignment,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = localizer.GetString("WelcomeCardTeamPoint3"),
                        Wrap = true,
                        HorizontalAlignment = textAlignment,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = localizer.GetString("WelcomeCardTeamPoint4"),
                        Wrap = true,
                        HorizontalAlignment = textAlignment,
                    },
                    new AdaptiveTextBlock
                    {
                        Spacing = AdaptiveSpacing.Medium,
                        Text = string.Format(CultureInfo.CurrentCulture, localizer.GetString("WelcomeCardTeamContentFooter"), localizer.GetString("CreateEventButtonWelcomeCard")),
                        Wrap = true,
                        HorizontalAlignment = textAlignment,
                    },
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                         Title = localizer.GetString("CreateEventButtonWelcomeCard"),
                         Data = new AdaptiveSubmitActionData
                         {
                            MsTeams = new CardAction
                            {
                                Type = "task/fetch",
                                Text = localizer.GetString("CreateEventButtonWelcomeCard"),
                            },
                            Command = BotCommands.CreateEvent,
                         },
                    },
                },
            };

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };

            return adaptiveCardAttachment;
        }

        /// <summary>
        /// Get welcome card attachment to show on Microsoft Teams personal scope.
        /// </summary>
        /// <param name="applicationBasePath">Application base path to get the logo of the application.</param>
        /// <param name="localizer">The current cultures' string localizer.</param>
        /// <param name="applicationManifestId">Application manifest id.</param>
        /// <returns>User welcome card attachment.</returns>
        public static Attachment GetWelcomeCardAttachmentForPersonal(
            string applicationBasePath,
            IStringLocalizer<Strings> localizer,
            string applicationManifestId)
        {
            var textAlignment = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? AdaptiveHorizontalAlignment.Right : AdaptiveHorizontalAlignment.Left;
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveColumnSet
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Width = AdaptiveColumnWidth.Auto,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveImage
                                    {
                                        Url = new Uri($"{applicationBasePath}/images/logo.png"),
                                        Size = AdaptiveImageSize.Medium,
                                    },
                                },
                            },
                            new AdaptiveColumn
                            {
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Weight = AdaptiveTextWeight.Bolder,
                                        Spacing = AdaptiveSpacing.None,
                                        Text = localizer.GetString("WelcomeCardTitle"),
                                        Wrap = true,
                                        HorizontalAlignment = textAlignment,
                                    },
                                    new AdaptiveTextBlock
                                    {
                                        Spacing = AdaptiveSpacing.None,
                                        Text = localizer.GetString("WelcomeCardPersonalIntro"),
                                        Wrap = true,
                                        IsSubtle = true,
                                        HorizontalAlignment = textAlignment,
                                    },
                                },
                                Width = AdaptiveColumnWidth.Stretch,
                            },
                        },
                    },
                    new AdaptiveTextBlock
                    {
                        Spacing = AdaptiveSpacing.Medium,
                        Text = localizer.GetString("WelcomeCardPersonalPoint1"),
                        Wrap = true,
                        HorizontalAlignment = textAlignment,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = localizer.GetString("WelcomeCardPersonalPoint2"),
                        Wrap = true,
                        HorizontalAlignment = textAlignment,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = localizer.GetString("WelcomeCardPersonalPoint3"),
                        Wrap = true,
                        HorizontalAlignment = textAlignment,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = localizer.GetString("WelcomeCardPersonalPoint4"),
                        Wrap = true,
                        HorizontalAlignment = textAlignment,
                    },
                    new AdaptiveTextBlock
                    {
                        Spacing = AdaptiveSpacing.Medium,
                        Text = string.Format(CultureInfo.CurrentCulture, localizer.GetString("WelcomeCardPersonalContentFooter"), localizer.GetString("WelcomeCardPersonalDiscoverButtonText")),
                        Wrap = true,
                        HorizontalAlignment = textAlignment,
                    },
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveOpenUrlAction
                     {
                        Url = new Uri($"https://teams.microsoft.com/l/entity/{applicationManifestId}/discover-events"),
                        Title = $"{localizer.GetString("WelcomeCardPersonalDiscoverButtonText")}",
                     },
                },
            };
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };

            return adaptiveCardAttachment;
        }
    }
}