// <copyright file="TeamInfoHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.EmployeeTraining.Common;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;
    using Microsoft.Teams.Apps.EmployeeTraining.Repositories;
    using Polly;
    using Polly.Contrib.WaitAndRetry;
    using Polly.Retry;

    /// <summary>
    /// Class that handles the helper methods to fetch team channel information.
    /// </summary>
    public class TeamInfoHelper : ITeamInfoHelper
    {
        /// <summary>
        /// Bot adapter.
        /// </summary>
        private readonly IBotFrameworkHttpAdapter botAdapter;

        /// <summary>
        /// Provider to fetch team details from Azure Storage.
        /// </summary>
        private readonly ILnDTeamConfigurationRepository teamConfigurationRepository;

        /// <summary>
        /// Microsoft application credentials.
        /// </summary>
        private readonly MicrosoftAppCredentials microsoftAppCredentials;

        /// <summary>
        /// Logger implementation to send logs to the logger service.
        /// </summary>
        private readonly ILogger<TeamInfoHelper> logger;

        /// <summary>
        /// Retry policy with jitter, retry thrice with a jitter delay of up to 1 sec. Retry for null reference exception as storing team info and fetching it for config tab may conflict.
        /// </summary>
        /// <remarks>
        /// Reference: https://github.com/Polly-Contrib/Polly.Contrib.WaitAndRetry#new-jitter-recommendation.
        /// </remarks>
        private readonly AsyncRetryPolicy retryPolicy = Policy.Handle<NullReferenceException>()
            .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 3));

        /// <summary>
        /// The memory cache to hold team members for the period of 30 minutes
        /// </summary>
        private IMemoryCache cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamInfoHelper"/> class.
        /// </summary>
        /// <param name="botAdapter">Bot adapter.</param>
        /// <param name="teamConfigurationRepository">Repository to fetch team details from Azure Storage.</param>
        /// <param name="microsoftAppCredentials">Microsoft application credentials.</param>
        /// <param name="logger">Logger implementation to send logs to the logger service.</param>
        /// <param name="cache">The memory cache to hold team members for the period of 30 minutes</param>
        public TeamInfoHelper(
            IBotFrameworkHttpAdapter botAdapter,
            ILnDTeamConfigurationRepository teamConfigurationRepository,
            MicrosoftAppCredentials microsoftAppCredentials,
            ILogger<TeamInfoHelper> logger,
            IMemoryCache cache)
        {
            this.botAdapter = botAdapter;
            this.teamConfigurationRepository = teamConfigurationRepository;
            this.microsoftAppCredentials = microsoftAppCredentials;
            this.logger = logger;
            this.cache = cache;
        }

        /// <summary>
        /// To fetch team member information for specified team.
        /// Return null if the member is not found in team id or either of the information is incorrect.
        /// Caller should handle null value to throw unauthorized if required.
        /// </summary>
        /// <param name="teamId">Team id.</param>
        /// <param name="userId">User object id.</param>
        /// <returns>Returns team member information.</returns>
        public async Task<TeamsChannelAccount> GetTeamMemberAsync(string teamId, string userId)
        {
            TeamsChannelAccount teamMember = new TeamsChannelAccount();

            try
            {
                await this.retryPolicy.ExecuteAsync(async () =>
                {
                    var teamDetails = await this.teamConfigurationRepository.GetTeamDetailsAsync(teamId);
                    if (teamDetails == null)
                    {
                        teamMember = null;
                        return;
                    }

                    string serviceUrl = teamDetails.ServiceUrl;

                    var conversationReference = new ConversationReference
                    {
                        ChannelId = Constants.TeamsBotFrameworkChannelId,
                        ServiceUrl = serviceUrl,
                    };

                    await ((BotFrameworkAdapter)this.botAdapter).ContinueConversationAsync(
                        this.microsoftAppCredentials.MicrosoftAppId,
                        conversationReference,
                        async (context, token) =>
                        {
                            teamMember = await TeamsInfo.GetTeamMemberAsync(context, userId, teamId, CancellationToken.None);
                        }, default);
                });
            }
#pragma warning disable CA1031 // Catching general exceptions to log exception details in telemetry client.
            catch (Exception ex)
#pragma warning restore CA1031 // Catching general exceptions to log exception details in telemetry client.
            {
                this.logger.LogError(ex, $"Error occurred while fetching team member for team: {teamId} - user object id: {userId} ");

                // Return null if the member is not found in team id or either of the information is incorrect.
                // Caller should handle null value to throw unauthorized if required.
                return null;
            }

            return teamMember;
        }

        /// <summary>
        /// To fetch members of all LnD teams
        /// Return null if the members not found in team id or either of the information is incorrect.
        /// Caller should handle null value to throw unauthorized if required
        /// </summary>
        /// <returns>The LnD team members</returns>
        public async Task<List<TeamsChannelAccount>> GetAllLnDTeamMembersAsync()
        {
            var cachedMembers = this.cache.Get("all-team-members");

            if (cachedMembers != null)
            {
                return cachedMembers as List<TeamsChannelAccount>;
            }

            List<TeamsChannelAccount> allLnDTeamMembers = null;

            try
            {
                var lnDTeams = await this.teamConfigurationRepository.GetTeamsAsync();

                if (lnDTeams.IsNullOrEmpty())
                {
                    return allLnDTeamMembers;
                }

                allLnDTeamMembers = new List<TeamsChannelAccount>();

                foreach (var teamDetails in lnDTeams)
                {
                    try
                    {
                        await this.retryPolicy.ExecuteAsync(async () =>
                        {
                            if (teamDetails == null)
                            {
                                this.logger.LogError($"GetAllLnDTeamMembersAsync- The team details are not available for team {teamDetails.TeamId}");
                                return;
                            }

                            string serviceUrl = teamDetails.ServiceUrl;

                            var conversationReference = new ConversationReference()
                            {
                                ChannelId = Constants.TeamsBotFrameworkChannelId,
                                Bot = new ChannelAccount() { Id = this.microsoftAppCredentials.MicrosoftAppId },
                                ServiceUrl = serviceUrl,
                                Conversation = new ConversationAccount() { ConversationType = ConversationTypes.Channel, IsGroup = true, Id = teamDetails.TeamId },
                            };

                            await ((BotFrameworkAdapter)this.botAdapter).ContinueConversationAsync(
                                this.microsoftAppCredentials.MicrosoftAppId,
                                conversationReference,
                                async (context, token) =>
                                {
                                    var members = new List<TeamsChannelAccount>();
                                    string continuationToken = null;

                                    do
                                    {
                                        var currentPage = await TeamsInfo.GetPagedMembersAsync(context, 100, continuationToken, token);
                                        continuationToken = currentPage.ContinuationToken;
                                        members.AddRange(currentPage.Members);
                                    }
                                    while (continuationToken != null);

                                    allLnDTeamMembers.AddRange(members);
                                }, default);
                        });
                    }
#pragma warning disable CA1031 // Catching general exception to continue the execution
                    catch (Exception ex)
#pragma warning restore CA1031 // Catching general exception to continue the execution
                    {
                        this.logger.LogError(ex, $"Error while getting team members for team {teamDetails.TeamId}");
                    }
                }
            }
#pragma warning disable CA1031 // Catching general exceptions to log exception details in telemetry client.
            catch (Exception ex)
#pragma warning restore CA1031 // Catching general exceptions to log exception details in telemetry client.
            {
                this.logger.LogError(ex, $"Error occurred while fetching LnD teams' members for one or multiple teams");
                return allLnDTeamMembers;
            }

            this.cache.Set("all-team-members", allLnDTeamMembers.Count == 0 ? null : allLnDTeamMembers, TimeSpan.FromMinutes(30));

            return allLnDTeamMembers;
        }
    }
}
