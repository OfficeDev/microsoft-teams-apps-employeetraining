// <copyright file="CreateEventCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Cards
{
    using System.Collections.Generic;
    using System.Globalization;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Localization;
    using Microsoft.Teams.Apps.EmployeeTraining.Common;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;

    /// <summary>
    /// Creates card attachment for opening create event task module.
    /// </summary>
    public static class CreateEventCard
    {
        /// <summary>
        /// Get adaptive card attachment for opening create event task module
        /// </summary>
        /// <param name="localizer">String localizer for localizing user facing text.</param>
        /// <returns>An adaptive card attachment.</returns>
        public static Attachment GetCard(IStringLocalizer<Strings> localizer)
        {
            var textAlignment = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? AdaptiveHorizontalAlignment.Right : AdaptiveHorizontalAlignment.Left;
            var createEventCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = localizer.GetString("CreateEventCardTitleText"),
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

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = createEventCard,
            };
        }
    }
}
