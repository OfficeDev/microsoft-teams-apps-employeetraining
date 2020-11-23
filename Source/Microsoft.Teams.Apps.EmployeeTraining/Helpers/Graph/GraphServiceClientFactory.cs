// <copyright file="GraphServiceClientFactory.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Helpers
{
    extern alias BetaLib;

    using System;
    using System.Threading.Tasks;
    using Microsoft.Graph;
#pragma warning disable SA1135 // Referring BETA package of MS Graph SDK.
    using Beta = BetaLib.Microsoft.Graph;
#pragma warning restore SA1135 // Referring BETA package of MS Graph SDK.

    /// <summary>
    /// Provides Microsoft Graph client for API calls.
    /// </summary>
    public static class GraphServiceClientFactory
    {
        /// <summary>
        /// Get Microsoft Graph service client.
        /// </summary>
        /// <param name="acquireAccessToken">Callback method to get access token.</param>
        /// <returns>Microsoft Graph service client instance.</returns>
        public static GraphServiceClient GetAuthenticatedGraphClient(
                Func<Task<string>> acquireAccessToken)
        {
            return new GraphServiceClient(new CustomAuthenticationProvider(acquireAccessToken));
        }

        /// <summary>
        /// Get Microsoft Graph Beta service client.
        /// </summary>
        /// <param name="acquireAccessToken">Callback method to get access token.</param>
        /// <returns>Microsoft Graph service client instance.</returns>
        public static Beta.GraphServiceClient GetAuthenticatedBetaGraphClient(
                Func<Task<string>> acquireAccessToken)
        {
            return new Beta.GraphServiceClient(new CustomAuthenticationProvider(acquireAccessToken));
        }
    }
}
