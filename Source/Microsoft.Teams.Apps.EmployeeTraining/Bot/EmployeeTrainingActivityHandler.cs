// <copyright file="EmployeeTrainingActivityHandler.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.EmployeeTraining.Cards;
    using Microsoft.Teams.Apps.EmployeeTraining.Helpers;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;
    using Newtonsoft.Json;

    /// <summary>
    /// The EmployeeTrainingActivityHandler is responsible for reacting to incoming events from Teams sent from BotFramework.
    /// </summary>
    public sealed class EmployeeTrainingActivityHandler : TeamsActivityHandler
    {
        /// <summary>
        /// A set of key/value application configuration properties for Activity settings.
        /// </summary>
        private readonly IOptions<BotSettings> botOptions;

        /// <summary>
        /// Instance to send logs to the Application Insights service.
        /// </summary>
        private readonly ILogger<EmployeeTrainingActivityHandler> logger;

        /// <summary>
        /// The current cultures' string localizer.
        /// </summary>
        private readonly IStringLocalizer<Strings> localizer;

        /// <summary>
        /// Instance of Application Insights Telemetry client.
        /// </summary>
        private readonly TelemetryClient telemetryClient;

        /// <summary>
        /// Provides helper methods for bot related activities.
        /// </summary>
        private readonly IActivityHandlerHelper activityHandlerHelper;

        /// <summary>
        /// Helper to send cards and display projects in messaging extension.
        /// </summary>
        private readonly IMessagingExtensionHelper messagingExtensionHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmployeeTrainingActivityHandler"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="localizer">The current cultures' string localizer.</param>
        /// <param name="telemetryClient">The Application Insights telemetry client. </param>
        /// <param name="options">The options.</param>
        /// <param name="activityHandlerHelper">Provides helper methods for bot related activities.</param>
        /// <param name="messagingExtensionHelper">Helper to send cards and display projects in messaging extension.</param>
        public EmployeeTrainingActivityHandler(
            ILogger<EmployeeTrainingActivityHandler> logger,
            IStringLocalizer<Strings> localizer,
            TelemetryClient telemetryClient,
            IOptions<BotSettings> options,
            IActivityHandlerHelper activityHandlerHelper,
            IMessagingExtensionHelper messagingExtensionHelper)
        {
            this.logger = logger;
            this.localizer = localizer;
            this.telemetryClient = telemetryClient;
            this.botOptions = options;
            this.activityHandlerHelper = activityHandlerHelper;
            this.messagingExtensionHelper = messagingExtensionHelper;
        }

        /// <summary>
        /// Invoked when members other than this bot (like a user) are removed from the conversation.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        protected override async Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            try
            {
                turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext));
                this.RecordEvent(nameof(this.OnConversationUpdateActivityAsync), turnContext);

                var activity = turnContext.Activity;
                this.logger.LogInformation($"conversationType: {activity.Conversation.ConversationType}, membersAdded: {activity.MembersAdded?.Count}, membersRemoved: {activity.MembersRemoved?.Count}");

                if (activity.Conversation.ConversationType == ConversationTypes.Personal)
                {
                    if (activity.MembersAdded != null && activity.MembersAdded.Any(member => member.Id == activity.Recipient.Id))
                    {
                        await this.activityHandlerHelper.OnBotInstalledInPersonalAsync(turnContext);
                    }
                }
                else if (activity.Conversation.ConversationType == ConversationTypes.Channel)
                {
                    if (activity.MembersAdded != null && activity.MembersAdded.Any(member => member.Id == activity.Recipient.Id))
                    {
                        await this.activityHandlerHelper.OnBotInstalledInTeamAsync(turnContext);
                    }
                    else if (activity.MembersRemoved != null && activity.MembersRemoved.Any(member => member.Id == activity.Recipient.Id))
                    {
                        await this.activityHandlerHelper.OnBotUninstalledFromTeamAsync(turnContext);
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Exception occurred while bot conversation update event.");
                throw;
            }
        }

        /// <summary>
        /// Handle when a message is addressed to the bot.
        /// </summary>
        /// <param name="turnContext">The turn context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>
        /// For more information on bot messaging in Teams, see the documentation
        /// https://docs.microsoft.com/en-us/microsoftteams/platform/bots/how-to/conversations/conversation-basics?tabs=dotnet#receive-a-message .
        /// </remarks>
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            try
            {
                turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext));
                var activity = turnContext.Activity;
                this.RecordEvent(nameof(this.OnMessageActivityAsync), turnContext);

                if (!string.IsNullOrEmpty(activity.Text))
                {
                    var command = activity.RemoveRecipientMention().Trim();

                    // Command to show card from where create event task module can be invoked.
                    if (command.Equals(this.localizer.GetString("BotCommandAddEvent"), StringComparison.CurrentCultureIgnoreCase))
                    {
                        var card = CreateEventCard.GetCard(this.localizer);
                        await turnContext.SendActivityAsync(MessageFactory.Attachment(card)).ConfigureAwait(false);
                    }
                    else
                    {
                        this.logger.LogInformation($"Received a command {command.ToUpperInvariant()} which is not supported.");
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error while message activity is received from the bot.");
                throw;
            }
        }

        /// <summary>
        /// Invoked when the user opens the Messaging Extension or searching for any content.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="query">Contains Messaging Extension query keywords.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>Messaging extension response object to fill compose extension section.</returns>
        /// <remarks>
        /// https://docs.microsoft.com/en-us/dotnet/api/microsoft.bot.builder.teams.teamsactivityhandler.onteamsmessagingextensionqueryasync?view=botbuilder-dotnet-stable.
        /// </remarks>
        protected override async Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(
            ITurnContext<IInvokeActivity> turnContext,
            MessagingExtensionQuery query,
            CancellationToken cancellationToken)
        {
            try
            {
                turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext));
                this.RecordEvent(nameof(this.OnTeamsMessagingExtensionQueryAsync), turnContext);

                var activity = turnContext.Activity;

                var messagingExtensionQuery = JsonConvert.DeserializeObject<MessagingExtensionQuery>(activity.Value.ToString());
                var searchQuery = this.messagingExtensionHelper.GetSearchResult(messagingExtensionQuery);

                return new MessagingExtensionResponse
                {
                    ComposeExtension = await this.messagingExtensionHelper.GetPostsAsync(
                        searchQuery,
                        messagingExtensionQuery.CommandId,
                        activity.From.AadObjectId,
                        Convert.ToInt32(messagingExtensionQuery.QueryOptions.Count, CultureInfo.InvariantCulture),
                        activity.LocalTimestamp),
                };
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Failed to handle the Messaging Extension command {turnContext.Activity.Name}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// This method is invoked when OnTurn method receives a fetch invoke activity.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="taskModuleRequest">Task module invoke request value payload.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents a task module response.</returns>
        /// <remarks>
        /// Reference link: https://docs.microsoft.com/en-us/dotnet/api/microsoft.bot.builder.teams.teamsactivityhandler.onteamstaskmodulefetchasync?view=botbuilder-dotnet-stable.
        /// </remarks>
        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            try
            {
                turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext));

                this.RecordEvent(nameof(this.OnTeamsTaskModuleFetchAsync), turnContext);

                return await this.activityHandlerHelper.OnTaskModuleFetchRequestAsync(turnContext);
            }
#pragma warning disable CA1031 // Catching general exception to redirect user to error page
            catch (Exception ex)
#pragma warning restore CA1031 // Catching general exception to redirect user to error page
            {
                this.logger.LogError(ex, $"Error while opening task module for user Id {turnContext.Activity.From.AadObjectId}");
                return this.activityHandlerHelper.GetTaskModuleResponse(new Uri($"{this.botOptions.Value.AppBaseUri}/error"), this.localizer.GetString("ErrorTitle"));
            }
        }

        /// <summary>
        /// Handle task module submit action.
        /// </summary>
        /// <param name="turnContext">turn context.</param>
        /// <param name="taskModuleRequest">task module request.</param>
        /// <param name="cancellationToken">cancellation token.</param>
        /// <returns>task module response.</returns>
        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            return default;
        }

        /// <summary>
        /// Records event data to Application Insights telemetry client
        /// </summary>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="turnContext">Provides context for a turn in a bot.</param>
        private void RecordEvent(string eventName, ITurnContext turnContext)
        {
            var teamsChannelData = turnContext.Activity.GetChannelData<TeamsChannelData>();

            this.telemetryClient.TrackEvent(eventName, new Dictionary<string, string>
            {
                { "userId", turnContext.Activity.From.AadObjectId },
                { "tenantId", turnContext.Activity.Conversation.TenantId },
                { "teamId", teamsChannelData?.Team?.Id },
                { "channelId", teamsChannelData?.Channel?.Id },
            });
        }
    }
}