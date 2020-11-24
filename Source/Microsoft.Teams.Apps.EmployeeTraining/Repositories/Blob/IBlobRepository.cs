// <copyright file="IBlobRepository.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Repositories
{
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage.Blob;

    /// <summary>
    /// Interface for handling Azure Blob Storage operations like uploading and deleting images from blob.
    /// </summary>
    public interface IBlobRepository
    {
        /// <summary>
        /// Delete file from Azure Storage Blob container.
        /// </summary>
        /// <param name="blobFilePath">Blob URL file path on which file is uploaded.</param>
        /// <returns>Returns success if file deletion from blob is successful.</returns>
        Task<bool> DeleteAsync(string blobFilePath);

        /// <summary>
        /// Initialize a blob client for interacting with the blob service.
        /// </summary>
        /// <returns>Returns blob client for blob operations.</returns>
        CloudBlobClient InitializeBlobClient();

        /// <summary>
        /// Upload event image to blob container.
        /// </summary>
        /// <param name="fileStream">File stream of file to be uploaded on blob storage.</param>
        /// <param name="contentType">Content type to be set on blob.</param>
        /// <returns>Returns uploaded file blob URL.</returns>
        Task<string> UploadAsync(Stream fileStream, string contentType);
    }
}