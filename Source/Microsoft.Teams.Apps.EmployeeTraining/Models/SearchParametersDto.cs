// <copyright file="SearchParametersDto.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Models
{
    using System.Collections.Generic;
    using Microsoft.Teams.Apps.EmployeeTraining.Models.Enums;

    /// <summary>
    /// This class is responsible to store the search parameters.
    /// </summary>
    public class SearchParametersDto
    {
        /// <summary>
        /// Gets or sets scope of the search.
        /// </summary>
        public string SearchString { get; set; }

        /// <summary>
        /// Gets or sets scope of the search.
        /// </summary>
        public EventSearchType SearchScope { get; set; }

        /// <summary>
        /// Gets or sets scope of the search.
        /// </summary>
        public IEnumerable<string> RecentCollaboratorIds { get; set; } = null;

        /// <summary>
        /// Gets or sets Azure Active Directory object id of the user..
        /// </summary>
        public string UserObjectId { get; set; }

        /// <summary>
        /// Gets or sets event sort by filter.
        /// </summary>
        public int SortByFilter { get; set; }

        /// <summary>
        /// Gets or sets number of search results.
        /// </summary>
        public int? SearchResultsCount { get; set; } = null;

        /// <summary>
        /// Gets or sets page count for which post needs to be fetched.
        /// </summary>
        public int? PageCount { get; set; } = null;

        /// <summary>
        /// Gets or sets number of search results to skip.
        /// </summary>
        public int? SkipRecords { get; set; } = null;

        /// <summary>
        /// Gets or sets query for filter.
        /// </summary>
        public string FilterQuery { get; set; } = null;

        /// <summary>
        /// Gets or sets created by for filter.
        /// </summary>
        public string CreatedByFilter { get; set; } = null;

        /// <summary>
        /// Gets or sets category by for filter.
        /// </summary>
        public string CategoryFilter { get; set; } = null;

        /// <summary>
        /// Gets or sets logged in user's team Id.
        /// </summary>
        public string TeamId { get; set; }

        /// <summary>
        /// Gets or sets event category Id.
        /// </summary>
        public string CategoryId { get; set; } = "0";
    }
}
