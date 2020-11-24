// <copyright file="IUserConfigurationRepository.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;

    /// <summary>
    /// This interface lists all the methods which are used to manage storing and deleting user configurations.
    /// </summary>
    public interface IUserConfigurationRepository
    {
        /// <summary>
        /// Gets users' configuration details.
        /// </summary>
        /// <param name="userAADObjectIds">The user IDs of which configuration details need to get.</param>
        /// <returns>Returns users' configuration details.</returns>
        Task<IEnumerable<User>> GetUserConfigurationsAsync(IEnumerable<string> userAADObjectIds);

        /// <summary>
        /// Inserts or updates a new user configuration details when user installs a Bot.
        /// </summary>
        /// <param name="userConfigurationDetails">The user configuration details.</param>
        /// <returns>Returns true if configuration details inserted or updated successfully. Else returns false.</returns>
        Task<bool> UpsertUserConfigurationsAsync(User userConfigurationDetails);
    }
}