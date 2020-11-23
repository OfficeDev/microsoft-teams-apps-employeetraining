// <copyright file="EventSearchService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Azure.Search;
    using Microsoft.Azure.Search.Models;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Rest.Azure;
    using Microsoft.Teams.Apps.EmployeeTraining.Common;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;
    using Microsoft.Teams.Apps.EmployeeTraining.Models.Configuration;
    using Microsoft.Teams.Apps.EmployeeTraining.Repositories;
    using Polly;
    using Polly.Contrib.WaitAndRetry;
    using Polly.Retry;
    using Index = Microsoft.Azure.Search.Models.Index;

    /// <summary>
    /// Event Search service which helps in creating index, indexer and data source if it doesn't exist
    /// for indexing table which will be used for searching and filtering events.
    /// </summary>
    public class EventSearchService : IDisposable, IEventSearchService
    {
        /// <summary>
        /// Azure Search service indexer name.
        /// </summary>
        private const string IndexerName = "events-indexer";

        /// <summary>
        /// Azure Search service data source name.
        /// </summary>
        private const string DataSourceName = "events-storage";

        /// <summary>
        /// Table name where team post data will get saved.
        /// </summary>
        private const string EventTableName = nameof(EventEntity);

        /// <summary>
        /// Retry policy with jitter.
        /// </summary>
        /// <remarks>
        /// Reference: https://github.com/Polly-Contrib/Polly.Contrib.WaitAndRetry#new-jitter-recommendation.
        /// </remarks>
        private readonly AsyncRetryPolicy retryPolicy;

        /// <summary>
        /// Used to initialize task.
        /// </summary>
        private readonly Lazy<Task> initializeTask;

        /// <summary>
        /// Instance of Azure Search service client.
        /// </summary>
        private readonly ISearchServiceClient searchServiceClient;

        /// <summary>
        /// Instance of Azure Search index client.
        /// </summary>
        private readonly ISearchIndexClient searchIndexClient;

        /// <summary>
        /// Instance of event repository to update post and get information of posts.
        /// </summary>
        private readonly IEventRepository eventRepository;

        /// <summary>
        /// Instance to send logs to the Application Insights service.
        /// </summary>
        private readonly ILogger<EventSearchService> logger;

        /// <summary>
        /// Represents a set of key/value application configuration properties.
        /// </summary>
        private readonly SearchServiceSettings options;

        /// <summary>
        /// Flag: Has Dispose already been called?
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventSearchService"/> class.
        /// </summary>
        /// <param name="optionsAccessor">A set of key/value application configuration properties.</param>
        /// <param name="eventRepository">Event repository dependency injection.</param>
        /// <param name="logger">Instance to send logs to the Application Insights service.</param>
        /// <param name="searchServiceClient">Search service client dependency injection.</param>
        /// <param name="searchIndexClient">Search index client dependency injection.</param>
        public EventSearchService(
            IOptions<SearchServiceSettings> optionsAccessor,
            IEventRepository eventRepository,
            ILogger<EventSearchService> logger,
            ISearchServiceClient searchServiceClient,
            ISearchIndexClient searchIndexClient)
        {
            optionsAccessor = optionsAccessor ?? throw new ArgumentNullException(nameof(optionsAccessor));

            this.options = optionsAccessor.Value;
            var searchServiceValue = this.options.SearchServiceName;
            this.initializeTask = new Lazy<Task>(() => this.InitializeAsync());
            this.eventRepository = eventRepository;
            this.logger = logger;
            this.searchServiceClient = searchServiceClient;
            this.searchIndexClient = searchIndexClient;
            this.retryPolicy = Policy.Handle<CloudException>(
                ex => (int)ex.Response.StatusCode == StatusCodes.Status409Conflict ||
                (int)ex.Response.StatusCode == StatusCodes.Status429TooManyRequests)
                .WaitAndRetryAsync(Backoff.LinearBackoff(TimeSpan.FromMilliseconds(2000), 2));
        }

        /// <summary>
        /// Get event list as per search and filter criteria.
        /// </summary>
        /// <param name="searchQuery">Query which the user had typed in Messaging Extension search field.</param>
        /// <param name="searchParameters">Search parameters for enhanced searching.</param>
        /// <returns>List of events.</returns>
        public async Task<IEnumerable<EventEntity>> GetEventsAsync(
            string searchQuery,
            SearchParameters searchParameters)
        {
            await this.EnsureInitializedAsync();

            var postSearchResult = await this.searchIndexClient.Documents.SearchAsync<EventEntity>(searchQuery, searchParameters);

            SearchContinuationToken continuationToken = null;
            var events = new List<EventEntity>();

            if (postSearchResult?.Results != null)
            {
                events.AddRange(postSearchResult.Results.Select(p => p.Document));
                continuationToken = postSearchResult.ContinuationToken;
            }

            while (continuationToken != null)
            {
                var searchResult = await this.searchIndexClient.Documents.ContinueSearchAsync<EventEntity>(continuationToken);

                if (searchResult?.Results != null)
                {
                    events.AddRange(searchResult.Results.Select(p => p.Document));
                    continuationToken = searchResult.ContinuationToken;
                }
            }

            return events;
        }

        /// <summary>
        /// Run the indexer on demand.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task RunIndexerOnDemandAsync()
        {
            // Retry once after 1 second if conflict occurs during indexer run.
            // If conflict occurs again means another index run is in progress and it will index data for which first failure occurred.
            // Hence ignore second conflict and continue.
            var requestId = Guid.NewGuid().ToString();

            try
            {
                await this.retryPolicy.ExecuteAsync(async () =>
                {
                    try
                    {
                        this.logger.LogInformation($"On-demand indexer run request #{requestId} - start");
                        await this.searchServiceClient.Indexers.RunAsync(IndexerName);
                        this.logger.LogInformation($"On-demand indexer run request #{requestId} - complete");
                    }
                    catch (CloudException ex)
                    {
                        this.logger.LogError(ex, $"Failed to run on-demand indexer run for request #{requestId}: {ex.Message}");
                        throw;
                    }
                });
            }
            catch (CloudException ex)
            {
                this.logger.LogError(ex, $"Failed to run on-demand indexer for retry. Request #{requestId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Dispose search service instance.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected implementation of Dispose pattern.
        /// </summary>
        /// <param name="disposing">True if already disposed else false.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                this.searchServiceClient.Dispose();
                this.searchIndexClient.Dispose();
            }

            this.disposed = true;
        }

        /// <summary>
        /// Creates Index, Data Source and Indexer for search service.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task RecreateSearchServiceIndexAsync()
        {
            try
            {
                await this.CreateSearchIndexAsync();
                await this.CreateDataSourceAsync();
                await this.CreateIndexerAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Create index, indexer and data source if doesn't exist.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task InitializeAsync()
        {
            try
            {
                // Table initialization is required here before creating search index or data source or indexer.
                await this.eventRepository.GetEventDetailsAsync("eventid", "teamId");
                await this.RecreateSearchServiceIndexAsync();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Failed to initialize Azure Search Service: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Create index in Azure Search service if it doesn't exist.
        /// </summary>
        /// <returns><see cref="Task"/> That represents index is created if it is not created.</returns>
        private async Task CreateSearchIndexAsync()
        {
            if (await this.searchServiceClient.Indexes.ExistsAsync(Constants.EventsIndex))
            {
                await this.searchServiceClient.Indexes.DeleteAsync(Constants.EventsIndex);
            }

            var tableIndex = new Index()
            {
                Name = Constants.EventsIndex,
                Fields = FieldBuilder.BuildForType<EventEntity>(),
            };
            await this.searchServiceClient.Indexes.CreateAsync(tableIndex);
        }

        /// <summary>
        /// Create data source if it doesn't exist in Azure Search service.
        /// </summary>
        /// <returns><see cref="Task"/> That represents data source is added to Azure Search service.</returns>
        private async Task CreateDataSourceAsync()
        {
            if (await this.searchServiceClient.DataSources.ExistsAsync(DataSourceName))
            {
                return;
            }

            var dataSource = DataSource.AzureTableStorage(
                DataSourceName,
                this.options.ConnectionString,
                EventTableName,
                query: null,
                new SoftDeleteColumnDeletionDetectionPolicy("IsRemoved", true));

            await this.searchServiceClient.DataSources.CreateAsync(dataSource);
        }

        /// <summary>
        /// Create indexer if it doesn't exist in Azure Search service.
        /// </summary>
        /// <returns><see cref="Task"/> That represents indexer is created if not available in Azure Search service.</returns>
        private async Task CreateIndexerAsync()
        {
            if (await this.searchServiceClient.Indexers.ExistsAsync(IndexerName))
            {
                await this.searchServiceClient.Indexers.DeleteAsync(IndexerName);
            }

            var indexer = new Indexer()
            {
                Name = IndexerName,
                DataSourceName = DataSourceName,
                TargetIndexName = Constants.EventsIndex,
            };

            await this.searchServiceClient.Indexers.CreateAsync(indexer);
            await this.searchServiceClient.Indexers.RunAsync(IndexerName);
        }

        /// <summary>
        /// Initialization of InitializeAsync method which will help in indexing.
        /// </summary>
        /// <returns>Represents an asynchronous operation.</returns>
        private Task EnsureInitializedAsync()
        {
            return this.initializeTask.Value;
        }
    }
}