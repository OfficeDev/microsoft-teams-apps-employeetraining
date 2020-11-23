// <copyright file="CustomAuthenticationProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Helpers
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.Graph;

    /// <summary>
    /// Custom authentication provider to add user access token in Microsoft Graph client.
    /// </summary>
    public class CustomAuthenticationProvider : IAuthenticationProvider
    {
        private readonly Func<Task<string>> acquireAccessToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomAuthenticationProvider"/> class.
        /// </summary>
        /// <param name="acquireAccessToken">Callback function to get token.</param>
        public CustomAuthenticationProvider(Func<Task<string>> acquireAccessToken)
        {
            this.acquireAccessToken = acquireAccessToken;
        }

        /// <summary>
        /// Get access token and add to authentication header.
        /// </summary>
        /// <param name="request">HTTP request.</param>
        /// <returns>A task.</returns>
        public async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            var accessToken = await this.acquireAccessToken.Invoke();

            // Add the token in the Authorization header
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer", accessToken);
        }
    }
}
