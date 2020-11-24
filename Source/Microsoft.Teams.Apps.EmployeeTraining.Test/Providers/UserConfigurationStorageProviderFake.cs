using System;
using System.Collections.Generic;
using Microsoft.Teams.Apps.EmployeeTraining.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Teams.Apps.EmployeeTraining.Repositories;

namespace Microsoft.Teams.Apps.EmployeeTraining.Tests.Providers
{
    public class UserConfigurationStorageProviderFake : IUserConfigurationRepository
    {
        public readonly List<User> users;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserConfigurationStorageProvider"/> class.
        /// </summary>
        /// <param name="options">A set of key/value application configuration properties for Microsoft Azure Table storage</param>
        /// <param name="logger">To send logs to the logger service</param>
        public UserConfigurationStorageProviderFake()
        {
            users = new List<User>()
            {
                new User
                {
                    AADObjectId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2baeee",
                    PartitionKey ="ad4b2b43-1cb5-408d-ab8a-17e28edac2baeee222"
                },
                new User
                {
                    AADObjectId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2baeee-122",
                    PartitionKey ="ad4b2b43-1cb5-408d-ab8a-17e28edac2baeee-3333"
                },
                new User
                {
                    AADObjectId = "ad4b2b43-1cb5-408d-ab8a-17e28edac2baeee-898",
                    PartitionKey ="ad4b2b43-1cb5-408d-ab8a-17e28edac2baeee-909"
                }
            };
        }

        /// <summary>
        /// Gets users' configuration details
        /// </summary>
        /// <param name="userAADObjectIds">The user IDs of which configuration details need to get</param>
        /// <returns>Returns users' configuration details</returns>
        public async Task<IEnumerable<User>> GetUserConfigurationsAsync(IEnumerable<string> userAADObjectIds)
        {
            if (userAADObjectIds == null || !userAADObjectIds.Any())
            {
                return null;
            }
            List<User> matchedUsers = new List<User>(); 
            foreach(string objectId in userAADObjectIds)
            {
                var user = this.users.FirstOrDefault(u => u.AADObjectId == objectId);
                if (user != null)
                {
                    matchedUsers.Add(user);
                }
            }
            return await Task.Run(() => matchedUsers) as IEnumerable<User>;
        }

        /// <summary>
        /// Delete user configuration details when user uninstalls a Bot
        /// </summary>
        /// <param name="userAADObjectId">The user's AAD object Id</param>
        /// <returns>Returns true if the user configuration details deleted successfully. Else returns false.</returns>
        public async Task<bool> DeleteUserConfigurations(string userAADObjectId)
        {
            if (string.IsNullOrEmpty(userAADObjectId))
            {
                throw new ArgumentException("The user Id should have a valid value", nameof(userAADObjectId));
            }

            var matchedUser = this.users.FirstOrDefault(u => u.AADObjectId == userAADObjectId);
            this.users.Remove(matchedUser); 
            return await Task.Run(() => true);
        }

        /// <summary>
        /// Insert or update a new user configuration details when user installs a Bot
        /// </summary>
        /// <param name="userConfigurationDetails">The user configuration details</param>
        /// <returns>Returns true if user configuration details inserted or updated successfully. Else returns false.</returns>
        public async Task<bool> UpsertUserConfigurationsAsync(User userConfigurationDetails)
        {
            if (userConfigurationDetails == null)
            {
                throw new ArgumentNullException(nameof(userConfigurationDetails), "The user configuration details should be provided");
            }
            this.users.Add(userConfigurationDetails);
            return await Task.Run(() => true);
        }
    }
}
