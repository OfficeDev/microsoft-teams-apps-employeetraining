// <copyright file="ILnDTeamConfigurationRepository.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;

    /// <summary>
    /// This interface lists all the methods which are used to manage storing and deleting LnD team configurations.
    /// </summary>
    public interface ILnDTeamConfigurationRepository
    {
        /// <summary>
        /// Inserts a new LnD team configuration details when LnD team install the Bot.
        /// </summary>
        /// <param name="teamDetails">The LnD team configuration details.</param>
        /// <returns>Returns true if configuration details inserted successfully. Else returns false.</returns>
        Task<bool> InsertLnDTeamConfigurationAsync(LnDTeam teamDetails);

        /// <summary>
        /// Delete LnD team configuration details when LnD team uninstalls a Bot.
        /// </summary>
        /// <param name="teamDetails">The LnD team details which needs to be deleted.</param>
        /// <returns>Returns true if configuration details deleted successfully. Else returns false.</returns>
        Task<bool> DeleteLnDTeamConfigurationsAsync(LnDTeam teamDetails);

        /// <summary>
        /// Get team details.
        /// </summary>
        /// <param name="teamId">The team Id of which details needs to be fetched.</param>
        /// <returns>Team details object.</returns>
        Task<LnDTeam> GetTeamDetailsAsync(string teamId);

        /// <summary>
        /// Gets all LnD teams
        /// </summary>
        /// <returns>Returns list of LnD teams</returns>
        Task<IEnumerable<LnDTeam>> GetTeamsAsync();
    }
}