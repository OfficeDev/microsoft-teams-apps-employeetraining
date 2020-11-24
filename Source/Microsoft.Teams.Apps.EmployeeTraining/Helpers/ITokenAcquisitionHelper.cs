// <copyright file="ITokenAcquisitionHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Helpers
{
    using System.Threading.Tasks;

    /// <summary>
    /// Provides methods to fetch user and application access token for Graph scopes.
    /// </summary>
    public interface ITokenAcquisitionHelper
    {
        /// <summary>
        /// Get user Azure AD access token.
        /// </summary>
        /// <returns>Access token with Graph scopes.</returns>
        Task<string> GetApplicationAccessTokenAsync();

        /// <summary>
        /// Adds token to cache.
        /// </summary>
        /// <param name="userAadId">Azure AD object identifier for logged in user.</param>
        /// <param name="jwtToken">Id token of user.</param>
        /// <returns>Token with graph scopes.</returns>
        Task<string> GetUserAccessTokenAsync(string userAadId, string jwtToken);
    }
}