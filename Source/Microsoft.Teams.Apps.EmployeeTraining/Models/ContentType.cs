// <copyright file="ContentType.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Models
{
    using System.IO;

    /// <summary>
    /// Supported file types for upload file to blob.
    /// </summary>
    public static class ContentType
    {
        /// <summary>
        /// Joint Photographic Experts Group; a lossy image format widely used to display photographic images format.
        /// </summary>
        public const string JPEG = "image/jpeg";

        /// <summary>
        /// Portable Network Graphics image; a lossless image format where all information is restored after decompressed while viewing.
        /// </summary>
        public const string PNG = "image/png";

        /// <summary>
        /// Get content type of image to be uploaded to blob.
        /// </summary>
        /// <param name="fileName">Full name of the file</param>
        /// <returns>Content type for a image.</returns>
        public static string GetFileContentType(string fileName)
        {
            string extension = Path.GetExtension(fileName);
            switch (extension)
            {
                case ImageType.PNG:
                    return PNG;

                case ImageType.JPEG:
                case ImageType.JPG:
                    return JPEG;
            }

            return null;
        }
    }
}