// <copyright file="MustBeLnDTeamMemberRequirement.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Authentication
{
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// This authorization class implements the marker interface
    /// <see cref="IAuthorizationRequirement"/> to check if user meets teams member specific requirements
    /// for accesing resources.
    /// </summary>
    public class MustBeLnDTeamMemberRequirement : IAuthorizationRequirement
    {
    }
}
