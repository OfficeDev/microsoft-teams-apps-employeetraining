// <copyright file="BaseRepository.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.EmployeeTraining.Helpers;

    /// <summary>
    /// Base repository for the data stored in the Azure Table Storage.
    /// </summary>
    /// <typeparam name="T">Entity class type.</typeparam>
    public class BaseRepository<T>
        where T : TableEntity, new()
    {
        /// <summary>
        /// Storage connection string.
        /// </summary>
        private readonly string connectionString;

        /// <summary>
        /// Logs errors and information.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseRepository{T}"/> class.
        /// Handles Microsoft Azure Table creation.
        /// </summary>
        /// <param name="connectionString">Connection string.</param>
        /// <param name="tableName">Azure Table storage table name.</param>
        /// <param name="logger">Logs errors and information.</param>
        public BaseRepository(
            string connectionString,
            string tableName,
            ILogger<BaseRepository<T>> logger)
        {
            this.InitializeTask = new Lazy<Task>(() => this.InitializeAsync());
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            this.TableName = tableName;
            this.logger = logger;
        }

        /// <summary>
        /// Gets or sets task for initialization.
        /// </summary>
        protected Lazy<Task> InitializeTask { get; set; }

        /// <summary>
        /// Gets or sets Microsoft Azure Table storage table name.
        /// </summary>
        protected string TableName { get; set; }

        /// <summary>
        /// Gets or sets a table in the Microsoft Azure Table storage.
        /// </summary>
        protected CloudTable CloudTable { get; set; }

        /// <summary>
        /// Create or update an entity in the table storage.
        /// </summary>
        /// <param name="entity">Entity to be created or updated.</param>
        /// <returns>A boolean that represents whether insert or update operation is succeeded.</returns>
        public async Task<bool> CreateOrUpdateAsync(T entity)
        {
            try
            {
                var operation = TableOperation.InsertOrReplace(entity);
                TableResult result = await this.CloudTable.ExecuteAsync(operation);
                return result.HttpStatusCode == (int)HttpStatusCode.NoContent;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Update an entity in the table storage.
        /// </summary>
        /// <param name="entity">Entity to be  updated.</param>
        /// <returns>A boolean that represents whether update operation is succeeded.</returns>
        public async Task<bool> UpdateAsync(T entity)
        {
            try
            {
                var operation = TableOperation.Replace(entity);
                TableResult result = await this.CloudTable.ExecuteAsync(operation);
                return result.HttpStatusCode == (int)HttpStatusCode.NoContent;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Delete an entity in the table storage.
        /// </summary>
        /// <param name="entity">Entity to be deleted.</param>
        /// <returns>A boolean that represents whether entity is deleted.</returns>
        public async Task<bool> DeleteAsync(T entity)
        {
            entity = entity ?? throw new ArgumentNullException(nameof(entity));

            try
            {
                var partitionKey = entity.PartitionKey;
                var rowKey = entity.RowKey;
                entity = await this.GetAsync(partitionKey, rowKey);
                if (entity == null)
                {
                    throw new KeyNotFoundException(
                        $"Not found in table storage. PartitionKey = {partitionKey}, RowKey = {rowKey}");
                }

                var operation = TableOperation.Delete(entity);
                var result = await this.CloudTable.ExecuteAsync(operation);
                return result.HttpStatusCode == (int)HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Get entities from the table storage in a partition with a filter.
        /// </summary>
        /// <param name="filter">Filter to the result.</param>
        /// <param name="partition">Partition key value (If null, value of filter parameter will be used for querying).</param>
        /// <returns>All data entities.</returns>
        public async Task<IEnumerable<T>> GetWithFilterAsync(string filter, string partition = null)
        {
            try
            {
                string combinedFilter = string.Empty;
                if (partition != null)
                {
                    var partitionKeyFilter = this.GetPartitionKeyFilter(partition);
                    combinedFilter = this.CombineFilters(filter, partitionKeyFilter);
                }
                else
                {
                    combinedFilter = filter;
                }

                var query = new TableQuery<T>().Where(combinedFilter);
                var entities = await this.ExecuteQueryAsync(query);
                return entities;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Get an entity by the keys in the table storage.
        /// </summary>
        /// <param name="partitionKey">The partition key of the entity.</param>
        /// <param name="rowKey">The row key for the entity.</param>
        /// <returns>The entity matching the keys.</returns>
        public async Task<T> GetAsync(string partitionKey, string rowKey)
        {
            try
            {
                var operation = TableOperation.Retrieve<T>(partitionKey, rowKey);
                var result = await this.CloudTable.ExecuteAsync(operation);
                return result.Result as T;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Get all data entities from the table storage in a partition.
        /// </summary>
        /// <param name="partition">Partition key value.</param>
        /// <param name="count">The max number of desired entities.</param>
        /// <returns>All data entities.</returns>
        public async Task<IEnumerable<T>> GetAllAsync(string partition, int? count = null)
        {
            try
            {
                var partitionKeyFilter = this.GetPartitionKeyFilter(partition);
                var query = new TableQuery<T>().Where(partitionKeyFilter);
                var entities = await this.ExecuteQueryAsync(query, count);
                return entities;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Insert or merge a batch of entities in Azure table storage.
        /// A batch can contain up to 100 entities.
        /// </summary>
        /// <param name="entities">Entities to be inserted or merged in Azure table storage.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task BatchDeleteAsync(IEnumerable<T> entities)
        {
            var array = entities.ToArray();
            try
            {
                for (var i = 0; i <= array.Length / 100; i++)
                {
                    var lowerBound = i * 100;
                    var upperBound = Math.Min(lowerBound + 99, array.Length - 1);
                    if (lowerBound > upperBound)
                    {
                        break;
                    }

                    var batchOperation = new TableBatchOperation();
                    for (var j = lowerBound; j <= upperBound; j++)
                    {
                        batchOperation.Delete(array[j]);
                    }

                    await this.CloudTable.ExecuteBatchAsync(batchOperation);
                }
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        /// <summary>
        /// Execute query in segmented manner.
        /// </summary>
        /// <param name="query">Table query.</param>
        /// <param name="count">Entities count per segment.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>All data entities.</returns>
        public async Task<IList<T>> ExecuteQueryAsync(
            TableQuery<T> query,
            int? count = null,
            CancellationToken ct = default)
        {
            query = query ?? throw new ArgumentNullException(nameof(query), "Query cannot be null");

            query.TakeCount = count;

            try
            {
                var result = new List<T>();
                TableContinuationToken token = null;

                do
                {
                    TableQuerySegment<T> seg = await this.CloudTable.ExecuteQuerySegmentedAsync<T>(query, token);
                    token = seg.ContinuationToken;
                    result.AddRange(seg);
                }
                while (token != null
                    && !ct.IsCancellationRequested
                    && (count == null || result.Count < count.Value));

                return result;
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        /// <summary>
        /// Ensures Microsoft Azure Table storage should be created before working on table.
        /// </summary>
        /// <returns>Represents an asynchronous operation.</returns>
        protected async Task EnsureInitializedAsync()
        {
            await this.InitializeTask.Value;
        }

        /// <summary>
        /// Get a filter that filters in the entities matching the incoming row keys.
        /// </summary>
        /// <param name="rowKeys">Row keys.</param>
        /// <returns>A filter that filters in the entities matching the incoming row keys.</returns>
        protected string GetRowKeysFilter(IEnumerable<string> rowKeys)
        {
            if (rowKeys.IsNullOrEmpty())
            {
                throw new ArgumentException("Row key array is either null or empty.", nameof(rowKeys));
            }

            try
            {
                var rowKeysFilter = string.Empty;
                foreach (var rowKey in rowKeys)
                {
                    var singleRowKeyFilter = TableQuery.GenerateFilterCondition(
                        nameof(TableEntity.RowKey),
                        QueryComparisons.Equal,
                        rowKey);

                    if (string.IsNullOrWhiteSpace(rowKeysFilter))
                    {
                        rowKeysFilter = singleRowKeyFilter;
                    }
                    else
                    {
                        rowKeysFilter = TableQuery.CombineFilters(rowKeysFilter, TableOperators.Or, singleRowKeyFilter);
                    }
                }

                return rowKeysFilter;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Get a filter that filters in the entities matching the incoming partition keys.
        /// </summary>
        /// <param name="partitionKeys">Partition keys.</param>
        /// <returns>A filter that filters in the entities matching the incoming partition keys.</returns>
        protected string GetPartitionKeysFilter(IEnumerable<string> partitionKeys)
        {
            if (partitionKeys.IsNullOrEmpty())
            {
                throw new ArgumentException("Partition key array is either null or empty.", nameof(partitionKeys));
            }

            try
            {
                var partitionKeysFilter = string.Empty;
                foreach (var partitionKey in partitionKeys)
                {
                    var singleRowKeyFilter = TableQuery.GenerateFilterCondition(
                        nameof(TableEntity.PartitionKey),
                        QueryComparisons.Equal,
                        partitionKey);

                    if (string.IsNullOrWhiteSpace(partitionKeysFilter))
                    {
                        partitionKeysFilter = singleRowKeyFilter;
                    }
                    else
                    {
                        partitionKeysFilter = TableQuery.CombineFilters(partitionKeysFilter, TableOperators.Or, singleRowKeyFilter);
                    }
                }

                return partitionKeysFilter;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Combines two filters.
        /// </summary>
        /// <param name="filter1">First filter query.</param>
        /// <param name="filter2">Second filter query.</param>
        /// <returns>Combines filter query.</returns>
        private string CombineFilters(string filter1, string filter2)
        {
            if (string.IsNullOrWhiteSpace(filter1) && string.IsNullOrWhiteSpace(filter2))
            {
                return string.Empty;
            }
            else if (string.IsNullOrWhiteSpace(filter1))
            {
                return filter2;
            }
            else if (string.IsNullOrWhiteSpace(filter2))
            {
                return filter1;
            }

            return TableQuery.CombineFilters(filter1, TableOperators.And, filter2);
        }

        /// <summary>
        /// Creates partition key filter.
        /// </summary>
        /// <param name="partition">Partition key.</param>
        /// <returns>Partition key filter.</returns>
        private string GetPartitionKeyFilter(string partition)
        {
            var filter = TableQuery.GenerateFilterCondition(
                nameof(TableEntity.PartitionKey),
                QueryComparisons.Equal,
                partition);
            return filter;
        }

        /// <summary>
        /// Create tables if it doesn't exist.
        /// </summary>
        /// <returns>Asynchronous task which represents table is created if its not existing.</returns>
        private async Task InitializeAsync()
        {
            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(this.connectionString);
                CloudTableClient cloudTableClient = storageAccount.CreateCloudTableClient();
                this.CloudTable = cloudTableClient.GetTableReference(this.TableName);
                await this.CloudTable.CreateIfNotExistsAsync();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error occurred while creating the table.");
                throw;
            }
        }
    }
}