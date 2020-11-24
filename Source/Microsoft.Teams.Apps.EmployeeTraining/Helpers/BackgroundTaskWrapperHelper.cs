// <copyright file="BackgroundTaskWrapperHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Helpers
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Wrapper class with properties and methods to manage Tasks.
    /// </summary>
    public class BackgroundTaskWrapperHelper : IBackgroundTaskWrapperHelper, IDisposable
    {
        /// <summary>
        /// Thread safe collection of tasks.
        /// </summary>
        private readonly BlockingCollection<Task> taskCollection;

        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundTaskWrapperHelper"/> class.
        /// </summary>
        public BackgroundTaskWrapperHelper() => this.taskCollection = new BlockingCollection<Task>();

        /// <summary>
        /// Method to add task to the task collection.
        /// </summary>
        /// <param name="task">represents one task.</param>
        public void Enqueue(Task task) => this.taskCollection.Add(task);

        /// <summary>
        /// Method to remove task from the task collection.
        /// </summary>
        /// <param name="token">cancellation token to signal cancellation to the executing method.</param>
        /// <returns>A task instance.</returns>
        public Task DequeueAsync(CancellationToken token) => this.taskCollection.Take(token);

        /// <summary>
        /// This code added to correctly implement the disposable pattern.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// This code added to correctly implement the disposable pattern.
        /// </summary>
        /// <param name="disposing">A boolean value to determine if a resource is to be disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    this.taskCollection.Dispose();
                }

                this.disposedValue = true;
            }
        }
    }
}