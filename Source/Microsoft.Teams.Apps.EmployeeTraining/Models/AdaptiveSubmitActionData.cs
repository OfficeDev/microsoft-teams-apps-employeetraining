// <copyright file="AdaptiveSubmitActionData.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Models
{
    using Microsoft.Bot.Schema;
    using Newtonsoft.Json;

    /// <summary>
    /// Adaptive submit card action data to post event and adaptive card related data.
    /// </summary>
    public class AdaptiveSubmitActionData
    {
        /// <summary>
        /// Gets or sets the Teams-specific action.
        /// </summary>
        [JsonProperty("msteams")]
        public CardAction MsTeams { get; set; }

        /// <summary>
        /// Gets or sets event Id
        /// </summary>
        public string EventId { get; set; }

        /// <summary>
        /// Gets or sets command which get recognised by Bot
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// Gets or sets team id for aligning personal goal.
        /// </summary>
        public string TeamId { get; set; }
    }
}