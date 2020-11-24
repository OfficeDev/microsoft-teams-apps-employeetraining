// <copyright file="IGroupGraphHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Helpers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Graph;

    /// <summary>
    /// Interface for making Microsoft Graph group API calls.
    /// </summary>
    public interface IGroupGraphHelper
    {
        /// <summary>
        /// Get group members for a group.
        /// </summary>
        /// <param name="groupId">AAD Object id of group.</param>
        /// <returns>A task that returns collection of group members.</returns>
        Task<IEnumerable<DirectoryObject>> GetGroupMembersAsync(string groupId);

        /// <summary>
        /// Get top 10 groups according to user search query.
        /// </summary>
        /// <param name="searchText">Search query entered by user.</param>
        /// <returns>List of users.</returns>
        Task<List<Group>> SearchGroupsAsync(string searchText);
    }
}