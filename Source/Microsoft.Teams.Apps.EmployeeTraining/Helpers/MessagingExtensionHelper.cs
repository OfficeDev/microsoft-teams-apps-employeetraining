// <copyright file="MessagingExtensionHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.EmployeeTraining.Cards;
    using Microsoft.Teams.Apps.EmployeeTraining.Common;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;
    using Microsoft.Teams.Apps.EmployeeTraining.Models.Enums;
    using Microsoft.Teams.Apps.EmployeeTraining.Repositories;
    using Microsoft.Teams.Apps.EmployeeTraining.Services;

    /// <summary>
    /// Class that handles the search activities for Messaging Extension.
    /// </summary>
    public class MessagingExtensionHelper : IMessagingExtensionHelper
    {
        /// <summary>
        /// Search text parameter name in the manifest file.
        /// </summary>
        private const string SearchTextParameterName = "searchText";

        /// <summary>
        /// Instance of Search service for working with storage.
        /// </summary>
        private readonly IUserEventSearchService userEventSearchService;

        /// <summary>
        /// Sets the base path.
        /// </summary>
        private readonly string applicationBasePath;

        /// <summary>
        /// The current cultures' string localizer.
        /// </summary>
        private readonly IStringLocalizer<Strings> localizer;

        /// <summary>
        /// Storage service provider for categories table
        /// </summary>
        private readonly ICategoryRepository categoryRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingExtensionHelper"/> class.
        /// </summary>
        /// <param name="localizer">The current cultures' string localizer.</param>
        /// <param name="userEventSearchService">The team post search service dependency injection.</param>
        /// <param name="options">A set of key/value application configuration properties for activity handler.</param>
        /// <param name="categoryRepository">The category storage provider dependency injection</param>
        public MessagingExtensionHelper(
            IStringLocalizer<Strings> localizer,
            IUserEventSearchService userEventSearchService,
            IOptions<BotSettings> options,
            ICategoryRepository categoryRepository)
        {
            options = options ?? throw new ArgumentNullException(nameof(options), "Bot settings cannot be null");
            this.localizer = localizer;
            this.userEventSearchService = userEventSearchService;
            this.applicationBasePath = options.Value.AppBaseUri;
            this.categoryRepository = categoryRepository;
        }

        /// <summary>
        /// Get the results from Azure Search service and populate the result (card + preview).
        /// </summary>
        /// <param name="query">Query which the user had typed in Messaging Extension search field.</param>
        /// <param name="commandId">Command id to determine which tab in Messaging Extension has been invoked.</param>
        /// <param name="userObjectId">Azure Active Directory id of the user.</param>
        /// <param name="count">Number of search results to return.</param>
        /// <param name="localDateTime">Indicates local date and time of end user.</param>
        /// <returns><see cref="Task"/>Returns Messaging Extension result object, which will be used for providing the card.</returns>
        public async Task<MessagingExtensionResult> GetPostsAsync(
           string query,
           string commandId,
           string userObjectId,
           int count,
           DateTimeOffset? localDateTime)
        {
            MessagingExtensionResult composeExtensionResult = new MessagingExtensionResult
            {
                Type = "result",
                AttachmentLayout = AttachmentLayoutTypes.List,
                Attachments = new List<MessagingExtensionAttachment>(),
            };
            IEnumerable<EventEntity> trainingResults;

            SearchParametersDto searchParamsDto;

            // commandId should be equal to Id mentioned in Manifest file under composeExtensions section.
            switch (commandId?.ToUpperInvariant())
            {
                case BotCommands.RecentTrainingsCommandId:
                    searchParamsDto = new SearchParametersDto
                    {
                        SearchString = query,
                        SearchResultsCount = count,
                        SearchScope = EventSearchType.AllPublicPrivateEventsForUser,
                        UserObjectId = userObjectId,
                    };
                    trainingResults = await this.userEventSearchService.GetEventsAsync(searchParamsDto);
                    await this.BindCategoryDetails(trainingResults);
                    composeExtensionResult = MessagingExtensionCard.GetCard(trainingResults, this.applicationBasePath, this.localizer, localDateTime);
                    break;

                case BotCommands.PopularTrainingsCommandId:
                    searchParamsDto = new SearchParametersDto
                    {
                        SearchString = query,
                        SearchResultsCount = count,
                        SearchScope = EventSearchType.AllPublicPrivateEventsForUser,
                        UserObjectId = userObjectId,
                        SortByFilter = (int)SortBy.PopularityByRegisteredUsers,
                    };
                    trainingResults = await this.userEventSearchService.GetEventsAsync(searchParamsDto);
                    await this.BindCategoryDetails(trainingResults);
                    composeExtensionResult = MessagingExtensionCard.GetCard(trainingResults, this.applicationBasePath, this.localizer, localDateTime);
                    break;

                default:
                    break;
            }

            return composeExtensionResult;
        }

        /// <summary>
        /// Get the value of the searchText parameter in the Messaging Extension query.
        /// </summary>
        /// <param name="query">Contains Messaging Extension query keywords.</param>
        /// <returns>A value of the searchText parameter.</returns>
        public string GetSearchResult(MessagingExtensionQuery query)
        {
            return query?.Parameters.FirstOrDefault(parameter => parameter.Name.Equals(SearchTextParameterName, StringComparison.OrdinalIgnoreCase))?.Value?.ToString();
        }

        /// <summary>
        /// Binds the category details to the respective events
        /// </summary>
        /// <param name="events">The list of events</param>
        /// <returns>Returns the events binded with category details</returns>
        private async Task BindCategoryDetails(IEnumerable<EventEntity> events)
        {
            if (events.IsNullOrEmpty())
            {
                return;
            }

            var eventCategoryIds = events.Select(eventDetails => eventDetails?.CategoryId).ToArray();
            var categories = await this.categoryRepository.GetCategoriesByIdsAsync(eventCategoryIds);

            if (categories?.Count() > 0)
            {
                foreach (var eventDetails in events)
                {
                    eventDetails.CategoryName = categories.Where(category => category.CategoryId == eventDetails.CategoryId)?.FirstOrDefault()?.Name;
                }
            }
        }
    }
}