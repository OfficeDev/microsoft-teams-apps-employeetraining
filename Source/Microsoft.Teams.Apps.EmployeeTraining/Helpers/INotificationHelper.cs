// <copyright file="INotificationHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Helpers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;

    /// <summary>
    /// Helper for notification activities
    /// </summary>
    public interface INotificationHelper
    {
        /// <summary>
        /// Sends notification to the users.
        /// </summary>
        /// <param name="users">The users to which notification need to send</param>
        /// <param name="card">The notification card that to be send</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SendNotificationToUsersAsync(IEnumerable<User> users, Attachment card);

        /// <summary>
        /// This method is used to send notifications to all channels of a group activity.
        /// </summary>
        /// <param name="team">The team to which notification need to send</param>
        /// <param name="card">The notification card that to be send</param>
        /// <param name="updateCard">Boolean indicating whether existing card needs to be updated</param>
        /// <param name="activityId">Existing card activity Id required for updating card</param>
        /// <returns>Task indicating result of asynchronous operation.</returns>
        Task<string> SendNotificationInTeamAsync(LnDTeam team, Attachment card, bool updateCard = false, string activityId = null);
    }
}