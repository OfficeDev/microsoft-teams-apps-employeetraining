// <copyright file="Startup.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Azure.Services.AppAuthentication;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.IdentityModel.Logging;
    using Microsoft.Teams.Apps.EmployeeTraining.Authentication;
    using Microsoft.Teams.Apps.EmployeeTraining.Bot;
    using Microsoft.Teams.Apps.EmployeeTraining.Models.Configuration;

    /// <summary>
    /// The Startup class is responsible for configuring the DI container and acts as the composition root.
    /// </summary>
    public sealed class Startup
    {
        private readonly IConfiguration configuration;

        private IWebHostEnvironment CurrentEnvironment { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The environment provided configuration.</param>
        /// <param name="environment">The environment details</param>
#pragma warning disable SA1201 // Declare property before initializing in constructor
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
#pragma warning restore SA1201 // Declare property before initializing in constructor
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.CurrentEnvironment = environment;
            this.GetKeyVaultByManagedServiceIdentity();
            this.ValidateConfigurationSettings();
        }

        /// <summary>
        /// Configure the composition root for the application.
        /// </summary>
        /// <param name="services">The stub composition root.</param>
        /// <remarks>
        /// For more information see: https://go.microsoft.com/fwlink/?LinkID=398940.
        /// </remarks>
#pragma warning disable CA1506 // Composition root expected to have coupling with many components.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.RegisterConfidentialCredentialProvider(this.configuration);
            services.RegisterCredentialProviders(this.configuration);
            services.RegisterConfigurationSettings(this.configuration);
            services.RegisterRepositories();
            services.AddControllers();
            services.RegisterHelpers();
            services.AddSearchService(this.configuration);
            services.AddSingleton<TelemetryClient>();

            services
                .AddApplicationInsightsTelemetry(this.configuration.GetValue<string>("ApplicationInsights:InstrumentationKey"));

            // In production, the React files will be served from this directory.
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });

            services.RegisterBotStates(this.configuration);

            IdentityModelEventSource.ShowPII = false;
            services.RegisterAuthenticationServices(this.configuration);
            services.AddSingleton<IChannelProvider, SimpleChannelProvider>();
            services.AddSingleton<IMemoryCache, MemoryCache>();

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, EmployeeTrainingAdapterWithErrorHandler>();

            services.AddTransient<IBot, EmployeeTrainingActivityHandler>();

            // Create the Activity middle-ware that will be added to the middle-ware pipeline in the AdapterWithErrorHandler.
            services.AddSingleton<EmployeeTrainingActivityMiddleware>();
            services.AddTransient(serviceProvider => (BotFrameworkAdapter)serviceProvider.GetRequiredService<IBotFrameworkHttpAdapter>());

            // Add i18n.
            services.RegisterLocalizationSettings(this.configuration);
            services.AddSearchService(this.configuration);
        }
#pragma warning restore CA1506

        /// <summary>
        /// Configure the application request pipeline.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="env">Hosting Environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRequestLocalization();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpointRouteBuilder => endpointRouteBuilder.MapControllers());

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.EnvironmentName.ToUpperInvariant() == "DEVELOPMENT")
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }

        /// <summary>
        /// Validate whether the configuration settings are missing or not.
        /// </summary>
        private void ValidateConfigurationSettings()
        {
            var azureSettings = new AzureSettings();
            this.configuration.Bind("AzureAd", azureSettings);
            azureSettings.ClientId = this.configuration.GetValue<string>("MicrosoftAppId");

            if (string.IsNullOrWhiteSpace(azureSettings.ClientId))
            {
                throw new ApplicationException("AzureAD ClientId is missing in the configuration file.");
            }

            if (string.IsNullOrWhiteSpace(azureSettings.TenantId))
            {
                throw new ApplicationException("AzureAD TenantId is missing in the configuration file.");
            }

            if (string.IsNullOrWhiteSpace(azureSettings.ApplicationIdURI))
            {
                throw new ApplicationException("AzureAD ApplicationIdURI is missing in the configuration file.");
            }

            if (string.IsNullOrWhiteSpace(azureSettings.ValidIssuers))
            {
                throw new ApplicationException("AzureAD ValidIssuers is missing in the configuration file.");
            }

            if (string.IsNullOrWhiteSpace(this.configuration.GetValue<string>("App:ManifestId")))
            {
                throw new ApplicationException("Manifest Id is missing in the configuration file.");
            }

            if (this.configuration.GetValue<int?>("App:CacheDurationInMinutes") == null || this.configuration.GetValue<int>("App:CacheDurationInMinutes") < 1)
            {
                throw new ApplicationException("Invalid chache duration value in the configuration file.");
            }

            if (this.configuration.GetValue<int?>("App:EventsPageSize") == null || this.configuration.GetValue<int>("App:EventsPageSize") < 30)
            {
                throw new ApplicationException("Invalid events page size value in the configuration file. The minimum value must be 30.");
            }
        }

        /// <summary>
        /// Get KeyVault secrets and app settings values
        /// </summary>
        private void GetKeyVaultByManagedServiceIdentity()
        {
            AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();

            if (this.CurrentEnvironment.IsDevelopment())
            {
                Task<string> accessToken = azureServiceTokenProvider.GetAccessTokenAsync("https://vault.azure.net");
                accessToken.Wait();
            }

            using (var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback)))
            {
                var storageSecretValue = keyVaultClient.GetSecretAsync($"{this.configuration["KeyVault:BaseURL"]}{this.configuration["KeyVaultStrings:StorageConnection"]}");

                storageSecretValue.Wait();
                this.configuration["Storage:ConnectionString"] = storageSecretValue.Result.Value;

                var appIdSecretValue = keyVaultClient.GetSecretAsync($"{this.configuration["KeyVault:BaseURL"]}{this.configuration["KeyVaultStrings:MicrosoftAppId"]}");

                appIdSecretValue.Wait();
                this.configuration["MicrosoftAppId"] = appIdSecretValue.Result.Value;

                var appPasswordSecretValue = keyVaultClient.GetSecretAsync($"{this.configuration["KeyVault:BaseURL"]}{this.configuration["KeyVaultStrings:MicrosoftAppPassword"]}");

                appPasswordSecretValue.Wait();
                this.configuration["MicrosoftAppPassword"] = appPasswordSecretValue.Result.Value;

                var searchServiceNameSecretValue = keyVaultClient.GetSecretAsync($"{this.configuration["KeyVault:BaseURL"]}{this.configuration["KeyVaultStrings:SearchServiceName"]}");

                searchServiceNameSecretValue.Wait();
                this.configuration["SearchService:SearchServiceName"] = searchServiceNameSecretValue.Result.Value;

                var searchServiceAdminApiKeySecretValue = keyVaultClient.GetSecretAsync($"{this.configuration["KeyVault:BaseURL"]}{this.configuration["KeyVaultStrings:SearchServiceAdminApiKey"]}");

                searchServiceAdminApiKeySecretValue.Wait();
                this.configuration["SearchService:SearchServiceAdminApiKey"] = searchServiceAdminApiKeySecretValue.Result.Value;

                var searchServiceQueryApiKeySecretValue = keyVaultClient.GetSecretAsync($"{this.configuration["KeyVault:BaseURL"]}{this.configuration["KeyVaultStrings:SearchServiceQueryApiKey"]}");

                searchServiceQueryApiKeySecretValue.Wait();
                this.configuration["SearchService:SearchServiceQueryApiKey"] = searchServiceQueryApiKeySecretValue.Result.Value;
            }
        }
    }
}