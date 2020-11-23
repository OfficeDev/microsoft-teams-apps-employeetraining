// <copyright file="TeamEventSearchService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.EmployeeTraining.Helpers;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;
    using Microsoft.Teams.Apps.EmployeeTraining.Models.Enums;
    using Microsoft.Teams.Apps.EmployeeTraining.Services.SearchService.Factory;

    /// <summary>
    /// Event search helper to construct filter and search queries.
    /// </summary>
    public class TeamEventSearchService : ITeamEventSearchService
    {
        /// <summary>
        /// Azure Search service maximum search result count for team post entity.
        /// </summary>
        private const int ApiSearchResultCount = 1500;

        /// <summary>
        /// Event search service to search and filter events.
        /// </summary>
        private readonly IEventSearchService eventSearchService;

        /// <summary>
        /// A set of key/value application configuration properties for Activity settings.
        /// </summary>
        private readonly IOptions<BotSettings> botOptions;

        /// <summary>
        /// Generates filter query for fetching events.
        /// </summary>
        private readonly IFilterQueryGeneratorFactory filterQueryGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamEventSearchService"/> class.
        /// </summary>
        /// <param name="eventSearchService">Event search provider to search and filter events.</param>
        /// <param name="botOptions">A set of key/value application configuration properties for activity handler.</param>
        /// <param name="filterQueryGenerator">Generates filter query for fetching events.</param>
        public TeamEventSearchService(IEventSearchService eventSearchService, IOptions<BotSettings> botOptions, IFilterQueryGeneratorFactory filterQueryGenerator)
        {
            this.eventSearchService = eventSearchService;
            this.botOptions = botOptions;
            this.filterQueryGenerator = filterQueryGenerator;
        }

        /// <summary>
        /// Get LnD team events as per user search text.
        /// </summary>
        /// <param name="searchParametersDto">Search parameters entered by user.</param>
        /// <returns>List of events.</returns>
        public async Task<IEnumerable<EventEntity>> GetEventsAsync(Models.SearchParametersDto searchParametersDto)
        {
            searchParametersDto = searchParametersDto ?? throw new ArgumentNullException(nameof(searchParametersDto), "Search parameters are null");

            searchParametersDto.SkipRecords = searchParametersDto.PageCount * this.botOptions.Value.EventsPageSize;
            searchParametersDto.SearchResultsCount = this.botOptions.Value.EventsPageSize;

            var searchParameters = this.InitializeSearchParameters(searchParametersDto);

            var events = await this.eventSearchService.GetEventsAsync(searchParametersDto.SearchString.EscapeSpecialCharacters(), searchParameters);

            return events;
        }

        /// <summary>
        /// Initialization of search service parameters which will help in searching the documents.
        /// </summary>
        /// /// <param name="searchParametersDto">Search parameters entered by user.</param>
        /// <returns>Represents an search parameter object.</returns>
        private Azure.Search.Models.SearchParameters InitializeSearchParameters(Models.SearchParametersDto searchParametersDto)
        {
            Azure.Search.Models.SearchParameters searchParameters = new Azure.Search.Models.SearchParameters()
            {
                Top = searchParametersDto.SearchResultsCount ?? ApiSearchResultCount,
                Skip = searchParametersDto.SkipRecords ?? 0,
                Select = new[]
                {
                    nameof(EventEntity.Audience),
                    nameof(EventEntity.CategoryId),
                    nameof(EventEntity.CreatedBy),
                    nameof(EventEntity.CreatedOn),
                    nameof(EventEntity.Description),
                    nameof(EventEntity.EndTime),
                    nameof(EventEntity.EventId),
                    nameof(EventEntity.IsAutoRegister),
                    nameof(EventEntity.MaximumNumberOfParticipants),
                    nameof(EventEntity.MeetingLink),
                    nameof(EventEntity.Name),
                    nameof(EventEntity.Photo),
                    nameof(EventEntity.StartDate),
                    nameof(EventEntity.StartTime),
                    nameof(EventEntity.Status),
                    nameof(EventEntity.TeamId),
                    nameof(EventEntity.Type),
                    nameof(EventEntity.UpdatedBy),
                    nameof(EventEntity.UpdatedOn),
                    nameof(EventEntity.Venue),
                    nameof(EventEntity.EndDate),
                    nameof(EventEntity.IsRegistrationClosed),
                    nameof(EventEntity.RegisteredAttendeesCount),
                },
                SearchFields = new[] { nameof(EventEntity.Name), nameof(EventEntity.Description) }, // default search event by name
                Filter = this.filterQueryGenerator.GetStrategy(searchParametersDto.SearchScope)?.GenerateFilterQuery(searchParametersDto),
            };

            if (searchParametersDto.SearchScope == EventSearchType.GetCategoryEvent)
            {
                searchParameters.Top = 1;
                searchParameters.Select = new[]
                {
                    nameof(EventEntity.CategoryId),
                    nameof(EventEntity.EventId),
                };
            }

            searchParameters.OrderBy = new[] { $"{nameof(EventEntity.CreatedOn)} desc" };

            return searchParameters;
        }
    }
}
