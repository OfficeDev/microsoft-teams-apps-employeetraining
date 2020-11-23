// <copyright file="EmployeeTrainingActivityMiddleware.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Bot
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.EmployeeTraining;
    using Microsoft.Teams.Apps.EmployeeTraining.Models.Configuration;

    /// <summary>
    /// Represents middleware that can operate on incoming activities.
    /// </summary>
    public class EmployeeTrainingActivityMiddleware : IMiddleware
    {
        /// <summary>
        /// Represents unique id of a Tenant.
        /// </summary>
        private readonly string tenantId;

        /// <summary>
        /// The current cultures' string localizer.
        /// </summary>
        private readonly IStringLocalizer<Strings> localizer;

        /// <summary>
        /// Represents a set of key/value application configuration properties for Azure.
        /// </summary>
        private readonly IOptions<AzureSettings> options;

        /// <summary>
        /// Sends logs to the Application Insights service.
        /// </summary>
        private readonly ILogger<EmployeeTrainingActivityMiddleware> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmployeeTrainingActivityMiddleware"/> class.
        /// </summary>
        /// <param name="options"> A set of key/value application configuration properties.</param>
        /// <param name="logger">Sends logs to the Application Insights service.</param>
        /// <param name="localizer">The current cultures' string localizer.</param>
        public EmployeeTrainingActivityMiddleware(IOptions<AzureSettings> options, ILogger<EmployeeTrainingActivityMiddleware> logger, IStringLocalizer<Strings> localizer)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.logger = logger;
            this.localizer = localizer;
            this.tenantId = this.options.Value.TenantId;
        }

        /// <summary>
        ///  Processes an incoming activity in middle-ware.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="next">The delegate to call to continue the bot middle-ware pipeline.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns><see cref="Task"/> A task that represents the work queued to execute.</returns>
        /// <remarks>
        /// Middle-ware calls the next delegate to pass control to the next middle-ware in
        /// the pipeline. If middle-ware doesn’t call the next delegate, the adapter does
        /// not call any of the subsequent middle-ware’s request handlers or the bot’s receive
        /// handler, and the pipeline short circuits.
        /// The turnContext provides information about the incoming activity, and other data
        /// needed to process the activity.
        /// </remarks>
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            turnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext));
            next = next ?? throw new ArgumentNullException(nameof(next));

            // An explicit check is required for activity type: 'Event' for requests coming from client application extensions.
            // Refer https://github.com/Microsoft/botframework-sdk/blob/master/specs/botframework-activity/botframework-activity.md#event-activity
            // To read more about Bot Framework -- Activity
            if (turnContext.Activity.Type != ActivityTypes.Event && !this.IsActivityFromExpectedTenant(turnContext))
            {
                this.logger.LogWarning($"Unexpected tenant id {turnContext.Activity.Conversation?.TenantId}");
                if (turnContext.Activity.Type == ActivityTypes.Message)
                {
                    await turnContext.SendActivityAsync(this.localizer.GetString("InvalidTenantText"), cancellationToken: cancellationToken);
                }

                return;
            }
            else
            {
                await next(cancellationToken);
            }
        }

        /// <summary>
        /// Verify if the tenant Id in the message is the same tenant Id used when application was configured.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <returns>True if context is from expected tenant else false.</returns>
        private bool IsActivityFromExpectedTenant(ITurnContext turnContext)
        {
            return turnContext.Activity.Conversation.TenantId == this.tenantId;
        }
    }
}