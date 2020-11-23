// <copyright file="StringExtension.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining.Helpers
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// This class lists the extension methods for string data type
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// Escaping unsafe, reserved and special characters that requires escaping includes
        /// + - &amp; | ! ( ) { } [ ] ^ " ~ * ? : \ /
        /// </summary>
        /// <param name="value">The string value</param>
        /// <returns>Returns string escaping unsafe, reserved and special characters.</returns>
        public static string EscapeSpecialCharacters(this string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                value = value.Replace("*", string.Empty, StringComparison.InvariantCulture).Trim();
                string pattern = @"([_|\\@&\?\*\+!-:~'\^/(){}<>#&\[\]])";
                string substitution = "\\$&";
                value = Regex.Replace(value, pattern, substitution);
                value = value.Any(ch => !char.IsLetterOrDigit(ch)) ? value += "\\" + "*" : value += "*";
            }

            return value;
        }
    }
}
