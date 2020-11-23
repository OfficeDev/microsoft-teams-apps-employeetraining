// <copyright file="Program.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// The Program class is responsible for holding the entry point of the program.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The entry point for the program.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Build the web host for servicing HTTP requests.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns> The WebHostBuilder configured from the arguments with the composition root defined in <see cref="Startup" />.</returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
