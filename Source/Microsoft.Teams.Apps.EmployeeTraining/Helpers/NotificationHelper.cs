// <copyright file="NotificationHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.EmployeeTraining.Common;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;
    using Polly;
    using Polly.Contrib.WaitAndRetry;
    using Polly.Retry;

    /// <summary>
    /// Helper for notification activities
    /// </summary>
    public class NotificationHelper : INotificationHelper
    {
        /// <summary>
        /// Default value for channel activity to send notifications
        /// </summary>
        private const string TeamsBotChannelId = "msteams";

        /// <summary>
        /// Represents retry delay
        /// </summary>
        private const int RetryDelay = 1500;

        /// <summary>
        /// Represents retry count
        /// </summary>
        private const int RetryCount = 2;

        /// <summary>
        /// Instance of IBot framework HTTP adapter.
        /// </summary>
        private readonly IBotFrameworkHttpAdapter botFrameworkHttpAdapter;

        /// <summary>
        /// Holds the Microsoft app credentials
        /// </summary>
        private readonly MicrosoftAppCredentials microsoftAppCredentials;

        /// <summary>
        /// Retry policy with jitter, retry twice with a jitter delay of up to 1 sec. Retry for HTTP 429(transient error)/502 bad gateway.
        /// </summary>
        /// <remarks>
        /// Reference: https://github.com/Polly-Contrib/Polly.Contrib.WaitAndRetry#new-jitter-recommendation.
        /// </remarks>
        private readonly AsyncRetryPolicy retryPolicy = Policy.Handle<ErrorResponseException>(
            ex => ex.Response.StatusCode == HttpStatusCode.TooManyRequests || ex.Response.StatusCode == HttpStatusCode.BadGateway)
            .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromMilliseconds(RetryDelay), RetryCount));

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationHelper"/> class.
        /// </summary>
        /// <param name="botFrameworkHttpAdapter">The bot adapter</param>
        /// <param name="microsoftAppCredentials">The microsoft app credentials</param>
        public NotificationHelper(
            IBotFrameworkHttpAdapter botFrameworkHttpAdapter,
            MicrosoftAppCredentials microsoftAppCredentials)
        {
            this.botFrameworkHttpAdapter = botFrameworkHttpAdapter;
            this.microsoftAppCredentials = microsoftAppCredentials;
        }

        /// <summary>
        /// Sends notification to the users.
        /// </summary>
        /// <param name="users">The users to which notification need to send</param>
        /// <param name="card">The notification card that to be send</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SendNotificationToUsersAsync(IEnumerable<User> users, Attachment card)
        {
            List<string> notificationFailedUserdIds = new List<string>();

            if (users.IsNullOrEmpty())
            {
                return;
            }

            foreach (var user in users)
            {
                try
                {
                    MicrosoftAppCredentials.TrustServiceUrl(user.ServiceUrl);

                    var conversationReference = new ConversationReference()
                    {
                        Bot = new ChannelAccount() { Id = $"28:{this.microsoftAppCredentials.MicrosoftAppId}" },
                        ChannelId = TeamsBotChannelId,
                        Conversation = new ConversationAccount() { Id = user.ConversationId },
                        ServiceUrl = user.ServiceUrl,
                    };

                    var botFrameworkAdapter = this.botFrameworkHttpAdapter as BotFrameworkAdapter;
                    ResourceResponse resourceResponse = null;

                    await this.retryPolicy.ExecuteAsync(async () =>
                    {
                        await botFrameworkAdapter.ContinueConversationAsync(
                          this.microsoftAppCredentials.MicrosoftAppId,
                          conversationReference,
                          async (turnContext, cancellationToken) =>
                          {
                              resourceResponse = await turnContext.SendActivityAsync(MessageFactory.Attachment(card), cancellationToken);
                          },
                          CancellationToken.None);
                    });

                    if (resourceResponse == null)
                    {
                        notificationFailedUserdIds.Add(user.AADObjectId);
                    }
                }
#pragma warning disable CA1031 // Caching general exception to continue sending notifications
                catch
#pragma warning restore CA1031 // Caching general exception to continue sending notifications
                {
                    notificationFailedUserdIds.Add(user.AADObjectId);
                }
            }
        }

        /// <summary>
        /// This method is used to send notifications to LnD team.
        /// </summary>
        /// <param name="team">The team to which notification need to send.</param>
        /// <param name="card">The notification card that to be send.</param>
        /// <param name="updateCard">Boolean indicating whether existing card needs to be updated.</param>
        /// <param name="activityId">Existing card activity Id required for updating card.</param>
        /// <returns>Task indicating result of asynchronous operation.</returns>
        public async Task<string> SendNotificationInTeamAsync(LnDTeam team, Attachment card, bool updateCard = false, string activityId = null)
        {
            team = team ?? throw new ArgumentNullException(nameof(team), "Team details cannot be null");
            card = card ?? throw new ArgumentNullException(nameof(card), "Card attachment cannot be null");

            if (updateCard && string.IsNullOrEmpty(activityId))
            {
                throw new ArgumentNullException(nameof(activityId), "Activity Id cannot be null in case of updating an existing card");
            }

            string serviceUrl = team.ServiceUrl;
            MicrosoftAppCredentials.TrustServiceUrl(serviceUrl);
            string teamsChannelId = team.TeamId;
            var conversationReference = new ConversationReference()
            {
                ChannelId = Constants.TeamsBotFrameworkChannelId,
                Bot = new ChannelAccount() { Id = this.microsoftAppCredentials.MicrosoftAppId },
                ServiceUrl = serviceUrl,
                Conversation = new ConversationAccount() { ConversationType = ConversationTypes.Channel, IsGroup = true, Id = teamsChannelId },
            };

            try
            {
                ResourceResponse resourceResponse = null;
                await ((BotFrameworkAdapter)this.botFrameworkHttpAdapter).ContinueConversationAsync(
                    this.microsoftAppCredentials.MicrosoftAppId,
                    conversationReference,
                    async (conversationTurnContext, conversationCancellationToken) =>
                    {
                        if (updateCard)
                        {
                            var activity = MessageFactory.Attachment(card);
                            activity.Id = activityId;
                            resourceResponse = await conversationTurnContext.UpdateActivityAsync(activity);
                        }
                        else
                        {
                            resourceResponse = await conversationTurnContext.SendActivityAsync(MessageFactory.Attachment(card));
                        }
                    },
                    CancellationToken.None);

                return resourceResponse.Id;
            }
#pragma warning disable CA1031 // Catching general exception to continue sending notifications
            catch
#pragma warning restore CA1031 // Catching general exception to continue sending notifications
            {
                return null;
            }
        }
    }
}