// <copyright file="ActivityHandlerHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Helpers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.EmployeeTraining.Cards;
    using Microsoft.Teams.Apps.EmployeeTraining.Common;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;
    using Microsoft.Teams.Apps.EmployeeTraining.Models.Enums;
    using Microsoft.Teams.Apps.EmployeeTraining.Repositories;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Helper for handling bot related activities.
    /// </summary>
    public class ActivityHandlerHelper : IActivityHandlerHelper
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
        /// Provides insert and delete operations for team configuration entity.
        /// </summary>
        private readonly ILnDTeamConfigurationRepository teamConfigurationRepository;

        /// <summary>
        /// Provides insert and delete operations for user details entity.
        /// </summary>
        private readonly IUserConfigurationRepository userConfigurationRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityHandlerHelper"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="localizer">The current cultures' string localizer.</param>
        /// <param name="options">The options.</param>
        /// <param name="teamConfigurationRepository">Provides insert and delete operations for team configuration entity.</param>
        /// <param name="userConfigurationRepository">Provides insert and delete operations for user details entity.</param>
        public ActivityHandlerHelper(
            ILogger<EmployeeTrainingActivityHandler> logger,
            IStringLocalizer<Strings> localizer,
            IOptions<BotSettings> options,
            ILnDTeamConfigurationRepository teamConfigurationRepository,
            IUserConfigurationRepository userConfigurationRepository)
        {
            this.logger = logger;
            this.localizer = localizer;
            this.botOptions = options;
            this.teamConfigurationRepository = teamConfigurationRepository;
            this.userConfigurationRepository = userConfigurationRepository;
        }

        /// <summary>
        /// Sent welcome card to personal chat.
        /// </summary>
        /// <param name="turnContext">Provides context for a turn in a bot.</param>
        /// <returns>A task that represents a response.</returns>
        public async Task OnBotInstalledInPersonalAsync(ITurnContext<IConversationUpdateActivity> turnContext)
        {
            turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext), "Turncontext cannot be null");

            this.logger.LogInformation($"Bot added in personal scope for user {turnContext.Activity.From.AadObjectId}");
            var userWelcomeCardAttachment = WelcomeCard.GetWelcomeCardAttachmentForPersonal(
                    this.botOptions.Value.AppBaseUri,
                    localizer: this.localizer,
                    this.botOptions.Value.ManifestId);
            await turnContext.SendActivityAsync(MessageFactory.Attachment(userWelcomeCardAttachment));

            var activity = turnContext.Activity;
            User userEntity = new User
            {
                AADObjectId = activity.From.AadObjectId,
                ConversationId = activity.Conversation.Id,
                BotInstalledOn = DateTime.UtcNow,
                ServiceUrl = turnContext.Activity.ServiceUrl,
            };

            bool operationStatus = await this.userConfigurationRepository.UpsertUserConfigurationsAsync(userEntity);
            if (operationStatus)
            {
                this.logger.LogInformation($"Successfully stored bot installation state for user {activity.From.AadObjectId} in storage.");
            }
            else
            {
                this.logger.LogInformation($"Unable to store bot installation state for user {activity.From.AadObjectId} in storage.");
            }
        }

        /// <summary>
        /// Add user membership to storage if bot is installed in Team scope.
        /// </summary>
        /// <param name="turnContext">Provides context for a turn in a bot.</param>
        /// <returns>A task that represents a response.</returns>
        public async Task SendWelcomeCardInChannelAsync(ITurnContext<IConversationUpdateActivity> turnContext)
        {
            turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext), "Turncontext cannot be null");

            var userWelcomeCardAttachment = WelcomeCard.GetWelcomeCardAttachmentForTeam(this.botOptions.Value.AppBaseUri, this.localizer);
            await turnContext.SendActivityAsync(MessageFactory.Attachment(userWelcomeCardAttachment));
        }

        /// <summary>
        /// Send a welcome card if bot is installed in Team scope.
        /// </summary>
        /// <param name="turnContext">Provides context for a turn in a bot.</param>
        /// <returns>A task that represents a response.</returns>
        public async Task OnBotInstalledInTeamAsync(ITurnContext<IConversationUpdateActivity> turnContext)
        {
            turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext));

            // If bot added to team, add team tab configuration with service URL.
            await this.SendWelcomeCardInChannelAsync(turnContext);

            var activity = turnContext.Activity;

            // Storing team information to storage.
            var teamsDetails = activity.TeamsGetTeamInfo();

            if (teamsDetails == null)
            {
                this.logger.LogInformation($"Unable to store bot installation state for team {teamsDetails.Id} in storage. Team details is null.");
            }
            else
            {
                this.logger.LogInformation($"Bot added in team {teamsDetails.Id}");
                LnDTeam teamEntity = new LnDTeam
                {
                    TeamId = teamsDetails.Id,
                    BotInstalledOn = DateTime.UtcNow,
                    ServiceUrl = activity.ServiceUrl,
                };

                bool operationStatus = await this.teamConfigurationRepository.InsertLnDTeamConfigurationAsync(teamEntity);

                if (operationStatus)
                {
                    this.logger.LogInformation($"Successfully stored bot installation state for team {teamsDetails.Id} in storage.");
                }
                else
                {
                    this.logger.LogInformation($"Unable to store bot installation state for team {teamsDetails.Id} in storage.");
                }
            }
        }

        /// <summary>
        /// Remove user details from storage if bot is uninstalled from Team scope.
        /// </summary>
        /// <param name="turnContext">Provides context for a turn in a bot.</param>
        /// <returns>A task that represents a response.</returns>
        public async Task OnBotUninstalledFromTeamAsync(ITurnContext<IConversationUpdateActivity> turnContext)
        {
            turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext), "Turncontext cannot be null");

            var teamsChannelData = turnContext.Activity.GetChannelData<TeamsChannelData>();
            var teamId = teamsChannelData.Team.Id;
            this.logger.LogInformation($"Bot removed from team {teamId}");

            try
            {
                var teamEntity = await this.teamConfigurationRepository.GetTeamDetailsAsync(teamId);
                if (teamEntity == null)
                {
                    this.logger.LogError($"Could not find team with Id {teamId} for deletion.");
                    return;
                }

                // Deleting team information from storage when bot is uninstalled from a team.
                bool deletedTeamDetailsStatus = await this.teamConfigurationRepository.DeleteLnDTeamConfigurationsAsync(teamEntity);
                if (deletedTeamDetailsStatus)
                {
                    this.logger.LogError($"Deleted team details for team {teamId}");
                }
                else
                {
                    this.logger.LogError($"Unable to clear team details for team {teamId}");
                }
            }
#pragma warning disable CA1031 // Catching general exception to continue flow after logging it.
            catch (Exception ex)
#pragma warning restore CA1031 // Catching general exception to continue flow after logging it.
            {
                this.logger.LogError(ex, $"Failed to delete team details from storage for team {teamId} after bot is uninstalled");
            }
        }

        /// <summary>
        /// Process task module fetch request.
        /// </summary>
        /// <param name="turnContext">Provides context for a turn in a bot.</param>
        /// <returns>A task that represents a response.</returns>
        public async Task<TaskModuleResponse> OnTaskModuleFetchRequestAsync(ITurnContext<IInvokeActivity> turnContext)
        {
            turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext), "Turn context cannot be null");
            var member = await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id, CancellationToken.None);

            if (member == null)
            {
                return this.GetTaskModuleResponse(new Uri($"{this.botOptions.Value.AppBaseUri}/error"), this.localizer.GetString("ErrorTitle"));
            }

            var activity = turnContext.Activity as Activity;

            var activityValue = JObject.Parse(activity.Value?.ToString())["data"].ToString();

            AdaptiveSubmitActionData adaptiveTaskModuleCardAction = JsonConvert.DeserializeObject<AdaptiveSubmitActionData>(activityValue);

            if (adaptiveTaskModuleCardAction == null)
            {
                this.logger.LogInformation("Value obtained from task module fetch action is null");
            }

            var command = adaptiveTaskModuleCardAction.Command;
            Uri taskModuleRequestUrl;

            switch (command)
            {
                case BotCommands.EditEvent:
                    taskModuleRequestUrl = new Uri($"{this.botOptions.Value.AppBaseUri}/create-event?teamId={adaptiveTaskModuleCardAction.EventId}&eventId={adaptiveTaskModuleCardAction.EventId}");
                    return this.GetTaskModuleResponse(taskModuleRequestUrl, this.localizer.GetString("EditEventCardButton"));

                case BotCommands.CreateEvent:
                    taskModuleRequestUrl = new Uri($"{this.botOptions.Value.AppBaseUri}/create-event");
                    return this.GetTaskModuleResponse(taskModuleRequestUrl, this.localizer.GetString("CreateEventButtonWelcomeCard"));

                case BotCommands.CloseRegistration:
                    taskModuleRequestUrl = new Uri($"{this.botOptions.Value.AppBaseUri}/close-or-cancel-event?operationType={(int)EventOperationType.CloseRegistration}&eventId={adaptiveTaskModuleCardAction.EventId}&teamId={adaptiveTaskModuleCardAction.TeamId}");
                    return this.GetTaskModuleResponse(taskModuleRequestUrl, this.localizer.GetString("CloseRegistrationCardButton"));

                case BotCommands.RegisterForEvent:
                    taskModuleRequestUrl = new Uri($"{this.botOptions.Value.AppBaseUri}/register-remove?eventId={adaptiveTaskModuleCardAction.EventId}&teamId={adaptiveTaskModuleCardAction.TeamId}");
                    return this.GetTaskModuleResponse(taskModuleRequestUrl, this.localizer.GetString("RegisterButton"));

                default:
                    return this.GetTaskModuleResponse(new Uri($"{this.botOptions.Value.AppBaseUri}/error"), this.localizer.GetString("ErrorTitle"));
            }
        }

        /// <summary>
        /// Gets a task module response
        /// </summary>
        /// <param name="url">The task module request URL</param>
        /// <param name="title">The title of the task module</param>
        /// <returns>Task module response object</returns>
        public TaskModuleResponse GetTaskModuleResponse(Uri url, string title)
        {
            return new TaskModuleResponse
            {
                Task = new TaskModuleContinueResponse
                {
                    Value = new TaskModuleTaskInfo()
                    {
                        Url = url?.ToString(),
                        Height = 746,
                        Width = 600,
                        Title = title,
                    },
                },
            };
        }
    }
}
