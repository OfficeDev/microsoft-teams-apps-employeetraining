// <copyright file="IBackgroundTaskWrapperHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Helpers
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Wrapper interface methods to manage tasks
    /// </summary>
    public interface IBackgroundTaskWrapperHelper
    {
        /// <summary>
        /// Method to add task to the task collection.
        /// </summary>
        /// <param name="task">represents one task.</param>
        public void Enqueue(Task task);

        /// <summary>
        /// Method to remove task from the task collection.
        /// </summary>
        /// <param name="token">cancellation token to signal cancellation to the executing method.</param>
        /// <returns>A task instance.</returns>
        public Task DequeueAsync(CancellationToken token);
    }
}