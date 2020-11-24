// <copyright file="IActivityHandlerHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Helpers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;

    /// <summary>
    /// Helper for handling bot related activities.
    /// </summary>
    public interface IActivityHandlerHelper
    {
        /// <summary>
        /// Sent welcome card to personal chat.
        /// </summary>
        /// <param name="turnContext">Provides context for a turn in a bot.</param>
        /// <returns>A task that represents a response.</returns>
        Task OnBotInstalledInPersonalAsync(ITurnContext<IConversationUpdateActivity> turnContext);

        /// <summary>
        /// Send a welcome card if bot is installed in Team scope.
        /// </summary>
        /// <param name="turnContext">Provides context for a turn in a bot.</param>
        /// <returns>A task that represents a response.</returns>
        Task OnBotInstalledInTeamAsync(ITurnContext<IConversationUpdateActivity> turnContext);

        /// <summary>
        /// Remove user details from storage if bot is uninstalled from Team scope.
        /// </summary>
        /// <param name="turnContext">Provides context for a turn in a bot.</param>
        /// <returns>A task that represents a response.</returns>
        Task OnBotUninstalledFromTeamAsync(ITurnContext<IConversationUpdateActivity> turnContext);

        /// <summary>
        /// Add user membership to storage if bot is installed in Team scope.
        /// </summary>
        /// <param name="turnContext">Provides context for a turn in a bot.</param>
        /// <returns>A task that represents a response.</returns>
        Task SendWelcomeCardInChannelAsync(ITurnContext<IConversationUpdateActivity> turnContext);

        /// <summary>
        /// Process task module fetch request.
        /// </summary>
        /// <param name="turnContext">Provides context for a turn in a bot.</param>
        /// <returns>A task that represents a response.</returns>
        Task<TaskModuleResponse> OnTaskModuleFetchRequestAsync(ITurnContext<IInvokeActivity> turnContext);

        /// <summary>
        /// Gets a task module response
        /// </summary>
        /// <param name="url">The task module request URL</param>
        /// <param name="title">The title of the task module</param>
        /// <returns>Task module response object</returns>
        TaskModuleResponse GetTaskModuleResponse(Uri url, string title);
    }
}