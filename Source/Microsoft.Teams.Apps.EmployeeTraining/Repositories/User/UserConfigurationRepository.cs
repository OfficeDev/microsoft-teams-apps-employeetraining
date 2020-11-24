// <copyright file="UserConfigurationRepository.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.EmployeeTraining.Helpers;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;
    using Microsoft.Teams.Apps.EmployeeTraining.Models.Configuration;

    /// <summary>
    /// This class manages storage operations related to user configurations.
    /// </summary>
    public class UserConfigurationRepository : BaseRepository<User>, IUserConfigurationRepository
    {
        /// <summary>
        /// Represents the entity name which is used to store user configurations.
        /// </summary>
        private const string UserConfiguration = "UserConfiguration";

        /// <summary>
        /// Initializes a new instance of the <see cref="UserConfigurationRepository"/> class.
        /// </summary>
        /// <param name="options">A set of key/value application configuration properties for Microsoft Azure Table storage.</param>
        /// <param name="logger">To send logs to the logger service.</param>
        public UserConfigurationRepository(
            IOptions<StorageSetting> options, ILogger<UserConfigurationRepository> logger)
            : base(options?.Value.ConnectionString, UserConfiguration, logger)
        {
        }

        /// <summary>
        /// Gets users' configuration details.
        /// </summary>
        /// <param name="userAADObjectIds">The user IDs of which configuration details need to get.</param>
        /// <returns>Returns users' configuration details.</returns>
        public async Task<IEnumerable<User>> GetUserConfigurationsAsync(IEnumerable<string> userAADObjectIds)
        {
            if (userAADObjectIds.IsNullOrEmpty())
            {
                return Enumerable.Empty<User>();
            }

            await this.EnsureInitializedAsync();

            var partitionKeyFilter = this.GetPartitionKeysFilter(userAADObjectIds);
            return await this.GetWithFilterAsync(partitionKeyFilter);
        }

        /// <summary>
        /// Insert or update a new user configuration details when user installs a Bot.
        /// </summary>
        /// <param name="userConfigurationDetails">The user configuration details.</param>
        /// <returns>Returns true if user configuration details inserted or updated successfully. Else returns false.</returns>
        public async Task<bool> UpsertUserConfigurationsAsync(User userConfigurationDetails)
        {
            if (userConfigurationDetails == null)
            {
                throw new ArgumentNullException(nameof(userConfigurationDetails), "The user configuration details should be provided");
            }

            await this.EnsureInitializedAsync();
            return await this.CreateOrUpdateAsync(userConfigurationDetails);
        }
    }
}
