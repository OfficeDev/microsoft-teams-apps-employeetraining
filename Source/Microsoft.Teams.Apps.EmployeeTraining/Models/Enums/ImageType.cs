// <copyright file="ImageType.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Models
{
    /// <summary>
    /// Supported image types for event.
    /// </summary>
    public static class ImageType
    {
        /// <summary>
        /// Joint Photographic Experts Group; a lossy image format widely used to display photographic images format.
        /// </summary>
        public const string JPEG = ".jpeg";

        /// <summary>
        /// Joint Photographic Experts Group; a lossy image format widely used to display photographic images format.
        /// </summary>
        public const string JPG = ".jpg";

        /// <summary>
        /// Portable Network Graphics image; a lossless image format where all information is restored after decompressed while viewing.
        /// </summary>
        public const string PNG = ".png";
    }
}
