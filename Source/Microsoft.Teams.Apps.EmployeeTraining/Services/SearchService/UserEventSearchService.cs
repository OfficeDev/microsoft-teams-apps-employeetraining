// <copyright file="UserEventSearchService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.EmployeeTraining.Helpers;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;
    using Microsoft.Teams.Apps.EmployeeTraining.Models.Enums;
    using Microsoft.Teams.Apps.EmployeeTraining.Services.SearchService.Factory;

    /// <summary>
    /// Event search helper to construct filter and search queries.
    /// </summary>
    public class UserEventSearchService : IUserEventSearchService
    {
        /// <summary>
        /// Azure Search service maximum search result count for events.
        /// </summary>
        private const int ApiSearchResultCount = 1500;

        /// <summary>
        /// Event search service to search and filter events.
        /// </summary>
        private readonly IEventSearchService eventSearchService;

        /// <summary>
        /// Generates filter query for fetching events.
        /// </summary>
        private readonly IFilterQueryGeneratorFactory filterQueryGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserEventSearchService"/> class.
        /// </summary>
        /// <param name="eventSearchService">Event search service to search and filter events.</param>
        /// <param name="filterQueryGenerator">Generates filter query for fetching events.</param>
        public UserEventSearchService(IEventSearchService eventSearchService, IFilterQueryGeneratorFactory filterQueryGenerator)
        {
            this.eventSearchService = eventSearchService;
            this.filterQueryGenerator = filterQueryGenerator;
        }

        /// <summary>
        /// Get events as per user search text.
        /// </summary>
        /// <param name="searchParametersDto">Search parameters entered by user.</param>
        /// <returns>List of events.</returns>
        public async Task<IEnumerable<EventEntity>> GetEventsAsync(Models.SearchParametersDto searchParametersDto)
        {
            searchParametersDto = searchParametersDto ?? throw new ArgumentNullException(nameof(searchParametersDto), "Search parameter is null");

            if (searchParametersDto.SearchScope != EventSearchType.DayBeforeReminder && searchParametersDto.SearchScope != EventSearchType.WeekBeforeReminder
                && string.IsNullOrEmpty(searchParametersDto.UserObjectId))
            {
                return Enumerable.Empty<EventEntity>();
            }

            searchParametersDto.SkipRecords = searchParametersDto.SortByFilter != (int)SortBy.PopularityByRecentCollaborators && searchParametersDto.SearchResultsCount != null ? searchParametersDto.PageCount * searchParametersDto.SearchResultsCount : null;

            var searchParameters = this.InitializeSearchParameters(searchParametersDto);
            var events = await this.eventSearchService.GetEventsAsync(searchParametersDto.SearchString.EscapeSpecialCharacters(), searchParameters);

            if (events.IsNullOrEmpty() || searchParametersDto.SearchScope == EventSearchType.DayBeforeReminder || searchParametersDto.SearchScope == EventSearchType.WeekBeforeReminder)
            {
                return events;
            }

            foreach (var eventDetails in events)
            {
                eventDetails.IsMandatoryForLoggedInUser = this.CheckIfMandatoryForLoggedInUser(eventDetails, searchParametersDto.UserObjectId);
                eventDetails.IsLoggedInUserRegistered = this.CheckIfLoggedInUserRegistered(eventDetails, searchParametersDto.UserObjectId);

                if (searchParametersDto.SortByFilter == (int)SortBy.PopularityByRecentCollaborators && !searchParametersDto.RecentCollaboratorIds.IsNullOrEmpty())
                {
                    if (!string.IsNullOrEmpty(eventDetails.RegisteredAttendees))
                    {
                        var registeredAttendees = eventDetails.RegisteredAttendees.Split(";");
                        var recentCollaborators = searchParametersDto.RecentCollaboratorIds.Intersect<string>(registeredAttendees);
                        eventDetails.LoggedInUserCollaboratorsCount = recentCollaborators?.Count() ?? 0;
                    }

                    if (!string.IsNullOrEmpty(eventDetails.AutoRegisteredAttendees))
                    {
                        var autoRegisteredAttendees = eventDetails.AutoRegisteredAttendees.Split(";");
                        var recentCollaborators = searchParametersDto.RecentCollaboratorIds.Intersect<string>(autoRegisteredAttendees);
                        eventDetails.LoggedInUserCollaboratorsCount += recentCollaborators?.Count() ?? 0;
                    }
                }
            }

            return searchParametersDto.SortByFilter == (int)SortBy.PopularityByRecentCollaborators ? events.OrderByDescending(userEvent => userEvent.LoggedInUserCollaboratorsCount).Skip(searchParametersDto.SkipRecords ?? 0).Take((int)searchParametersDto.SearchResultsCount) : events;
        }

        /// <summary>
        /// Check if logged-in user is mandatory for the event.
        /// </summary>
        /// <param name="eventDetails">Event details</param>
        /// <param name="userObjectId">Logged in user's AAD object identifier.</param>
        /// <returns>Boolean value</returns>
        private bool CheckIfMandatoryForLoggedInUser(EventEntity eventDetails, string userObjectId)
        {
            if (eventDetails.MandatoryAttendees != null && eventDetails.MandatoryAttendees.Contains(userObjectId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if logged-in user is registered for the event.
        /// </summary>
        /// <param name="eventDetails">Event details.</param>
        /// <param name="userObjectId">Logged in user's AAD object identifier.</param>
        /// <returns>Boolean value</returns>
        private bool CheckIfLoggedInUserRegistered(EventEntity eventDetails, string userObjectId)
        {
            if ((eventDetails.AutoRegisteredAttendees != null && eventDetails.AutoRegisteredAttendees.Contains(userObjectId, StringComparison.OrdinalIgnoreCase)) ||
                (eventDetails.RegisteredAttendees != null && eventDetails.RegisteredAttendees.Contains(userObjectId, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Initialization of search service parameters which will help in searching the documents.
        /// </summary>
        /// <param name="searchParametersDto">Search parameters.</param>
        /// <returns>Represents an search parameter object.</returns>
        private Azure.Search.Models.SearchParameters InitializeSearchParameters(Models.SearchParametersDto searchParametersDto)
        {
            Azure.Search.Models.SearchParameters searchParameters = new Azure.Search.Models.SearchParameters()
            {
                Top = searchParametersDto.SearchResultsCount.HasValue && searchParametersDto.SortByFilter != (int)SortBy.PopularityByRecentCollaborators ? searchParametersDto.SearchResultsCount : ApiSearchResultCount,
                Skip = searchParametersDto.SkipRecords.HasValue && searchParametersDto.SortByFilter != (int)SortBy.PopularityByRecentCollaborators ? searchParametersDto.SkipRecords : 0,
                Select = searchParametersDto.SearchScope == EventSearchType.SearchByName ?
                new[]
                {
                    nameof(EventEntity.Name),
                    nameof(EventEntity.EventId),
                }
                :
                new[]
                {
                    nameof(EventEntity.Audience),
                    nameof(EventEntity.CategoryId),
                    nameof(EventEntity.Description),
                    nameof(EventEntity.EndTime),
                    nameof(EventEntity.EventId),
                    nameof(EventEntity.MaximumNumberOfParticipants),
                    nameof(EventEntity.Name),
                    nameof(EventEntity.Photo),
                    nameof(EventEntity.SelectedColor),
                    nameof(EventEntity.StartDate),
                    nameof(EventEntity.StartTime),
                    nameof(EventEntity.TeamId),
                    nameof(EventEntity.Type),
                    nameof(EventEntity.MandatoryAttendees),
                    nameof(EventEntity.AutoRegisteredAttendees),
                    nameof(EventEntity.RegisteredAttendeesCount),
                    nameof(EventEntity.Venue),
                    nameof(EventEntity.RegisteredAttendees),
                },
                SearchFields = new[] { nameof(EventEntity.Name), nameof(EventEntity.Description) }, // default search event by name and description
                Filter = this.filterQueryGenerator.GetStrategy(searchParametersDto.SearchScope)?.GenerateFilterQuery(searchParametersDto),
            };

            searchParameters.OrderBy = searchParametersDto.SortByFilter == (int)SortBy.Recent ?
                new[] { $"{nameof(EventEntity.CreatedOn)} desc" } :
                new[] { $"{nameof(EventEntity.RegisteredAttendeesCount)} desc" };

            var filterConditions = this.GetFilterCondition(searchParametersDto.CreatedByFilter, searchParametersDto.CategoryFilter);

            if (!string.IsNullOrEmpty(filterConditions))
            {
                searchParameters.Filter += $" and {filterConditions}";
            }

            return searchParameters;
        }

        /// <summary>
        /// Generate filter condition based on selected filter parameters.
        /// </summary>
        /// <param name="createdByFilter">Semicolon separated user AAD object identifier who created events.</param>
        /// <param name="categoryFilter">Semicolon separated category Ids.</param>
        /// <returns>A string containing filter query for search service.</returns>
        private string GetFilterCondition(string createdByFilter = null, string categoryFilter = null)
        {
            string filterConditions = string.Empty;
            if (!string.IsNullOrEmpty(categoryFilter))
            {
                var categories = categoryFilter.Split(";");
                var categoryFilterStringBuilder = new StringBuilder($"{nameof(EventEntity.CategoryId)} eq '{categories[0]}'");
                for (int i = 1; i < categories.Length; i++)
                {
                    categoryFilterStringBuilder.Append($" or {nameof(EventEntity.CategoryId)} eq '{categories[i]}'");
                }

                filterConditions = $"({categoryFilterStringBuilder})";
            }

            if (!string.IsNullOrEmpty(createdByFilter))
            {
                var createdByUsers = createdByFilter.Split(";");
                var createdByFilterStringBuilder = new StringBuilder($"{nameof(EventEntity.CreatedBy)} eq '{createdByUsers[0]}'");

                for (int i = 1; i < createdByUsers.Length; i++)
                {
                    createdByFilterStringBuilder.Append($" or {nameof(EventEntity.CreatedBy)} eq '{createdByUsers[i]}'");
                }

                filterConditions += string.IsNullOrEmpty(filterConditions) ? $"({createdByFilterStringBuilder})" : $" and ({createdByFilterStringBuilder})";
            }

            return filterConditions;
        }
    }
}
