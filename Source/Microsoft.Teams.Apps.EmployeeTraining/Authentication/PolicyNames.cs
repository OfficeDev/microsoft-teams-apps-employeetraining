// <copyright file="PolicyNames.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Authentication
{
    /// <summary>
    /// This class lists the names of the custom authorization policies in the project.
    /// </summary>
    public static class PolicyNames
    {
        /// <summary>
        /// The name of the authorization policy, MustBeLnDTeamMemberPolicy. Indicates that user must be a valid team member of LnD team.
        /// </summary>
        public const string MustBeLnDTeamMemberPolicy = "MustBeLnDTeamMemberPolicy";
    }
}
