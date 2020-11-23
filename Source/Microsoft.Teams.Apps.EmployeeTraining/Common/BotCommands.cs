// <copyright file="BotCommands.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Common
{
    /// <summary>
    /// Class defining bot commands.
    /// </summary>
    public static class BotCommands
    {
        /// <summary>
        /// Bot help command on which card describing bot features is sent to user.
        /// </summary>
        public const string Help = "HELP";

        /// <summary>
        /// Command to invoke 'Edit Event' task module.
        /// </summary>
        public const string EditEvent = "EDITEVENT";

        /// <summary>
        /// Command to invoke 'Add event' task module
        /// </summary>
        public const string CreateEvent = "ADD EVENT";

        /// <summary>
        /// Command to invoke 'Close Registration' task module
        /// </summary>
        public const string CloseRegistration = "CLOSEREGISTRATION";

        /// <summary>
        /// Command to export event details.
        /// </summary>
        public const string RegisterForEvent = "REGISTERFOREVENT";

        /// <summary>
        /// Recent trainings command in the manifest file.
        /// </summary>
        public const string RecentTrainingsCommandId = "RECENT";

        /// <summary>
        /// Popular trainings command in the manifest file.
        /// </summary>
        public const string PopularTrainingsCommandId = "POPULAR";
    }
}