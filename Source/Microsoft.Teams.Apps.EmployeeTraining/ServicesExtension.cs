// <copyright file="ServicesExtension.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.EmployeeTraining
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Localization;
    using Microsoft.Azure.Search;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Azure;
    using Microsoft.Bot.Builder.BotFramework;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Identity.Client;
    using Microsoft.Teams.Apps.EmployeeTraining.Common;
    using Microsoft.Teams.Apps.EmployeeTraining.Helpers;
    using Microsoft.Teams.Apps.EmployeeTraining.Helpers.BackgroundService;
    using Microsoft.Teams.Apps.EmployeeTraining.Models;
    using Microsoft.Teams.Apps.EmployeeTraining.Models.Configuration;
    using Microsoft.Teams.Apps.EmployeeTraining.Repositories;
    using Microsoft.Teams.Apps.EmployeeTraining.Services;
    using Microsoft.Teams.Apps.EmployeeTraining.Services.SearchService.Factory;

    /// <summary>
    /// Class to extend ServiceCollection.
    /// </summary>
    public static class ServicesExtension
    {
        /// <summary>
        /// Adds application configuration settings to specified IServiceCollection.
        /// </summary>
        /// <param name="services">Collection of services.</param>
        /// <param name="configuration">Application configuration properties.</param>
        public static void RegisterConfigurationSettings(this IServiceCollection services, IConfiguration configuration)
        {
            string appBaseUrl = configuration.GetValue<string>("App:AppBaseUri");

            services.Configure<EmployeeTrainingActivityHandlerOptions>(options =>
            {
                options.AppBaseUri = appBaseUrl;
                options.TenantId = configuration.GetValue<string>("App:TenantId");
            });

            services.Configure<BotSettings>(options =>
            {
                options.AppBaseUri = appBaseUrl;
                options.MedianFirstRetryDelay = configuration.GetValue<double>("RetryPolicy:medianFirstRetryDelay");
                options.RetryCount = configuration.GetValue<int>("RetryPolicy:retryCount");
                options.ManifestId = configuration.GetValue<string>("App:ManifestId");
                options.MicrosoftAppId = configuration.GetValue<string>("MicrosoftAppId");
                options.MicrosoftAppPassword = configuration.GetValue<string>("MicrosoftAppPassword");
                options.CacheDurationInMinutes = configuration.GetValue<double>("App:CacheDurationInMinutes");
                options.EventsPageSize = configuration.GetValue<int>("App:EventsPageSize");
            });

            services.Configure<AzureSettings>(options =>
            {
                options.TenantId = configuration.GetValue<string>("AzureAd:TenantId");
                options.ClientId = configuration.GetValue<string>("MicrosoftAppId");
                options.ApplicationIdURI = configuration.GetValue<string>("AzureAd:ApplicationIdURI");
                options.ValidIssuers = configuration.GetValue<string>("AzureAd:ValidIssuers");
                options.Instance = configuration.GetValue<string>("AzureAd:Instance");
                options.GraphScope = configuration.GetValue<string>("AzureAd:GraphScope");
            });

            services.Configure<TelemetrySetting>(options =>
            {
                options.InstrumentationKey = configuration.GetValue<string>("ApplicationInsights:InstrumentationKey");
            });

            services.Configure<StorageSetting>(options =>
            {
                options.ConnectionString = configuration.GetValue<string>("Storage:ConnectionString");
            });

            services.Configure<SearchServiceSettings>(searchServiceSettings =>
            {
                searchServiceSettings.SearchServiceName = configuration.GetValue<string>("SearchService:SearchServiceName");
                searchServiceSettings.SearchServiceQueryApiKey = configuration.GetValue<string>("SearchService:SearchServiceQueryApiKey");
                searchServiceSettings.SearchServiceAdminApiKey = configuration.GetValue<string>("SearchService:SearchServiceAdminApiKey");
                searchServiceSettings.ConnectionString = configuration.GetValue<string>("Storage:ConnectionString");
            });
        }

        /// <summary>
        /// Adds helpers to specified IServiceCollection.
        /// </summary>
        /// <param name="services">Collection of services.</param>
        public static void RegisterHelpers(this IServiceCollection services)
        {
            services.AddSingleton<IActivityHandlerHelper, ActivityHandlerHelper>();
            services.AddSingleton<ITeamInfoHelper, TeamInfoHelper>();
            services.AddSingleton<ITokenAcquisitionHelper, TokenAcquisitionHelper>();
            services.AddSingleton<IUserEventSearchService, UserEventSearchService>();
            services.AddSingleton<ITeamEventSearchService, TeamEventSearchService>();
            services.AddScoped<IEventWorkflowHelper, EventWorkflowHelper>();
            services.AddSingleton<INotificationHelper, NotificationHelper>();
            services.AddSingleton<IBackgroundTaskWrapperHelper, BackgroundTaskWrapperHelper>();
            services.AddScoped<IUserEventsHelper, UserEventsHelper>();
            services.AddSingleton<ITeamEventSearchService, TeamEventSearchService>();
            services.AddSingleton<IUserEventSearchService, UserEventSearchService>();
            services.AddSingleton<ICategoryHelper, CategoryHelper>();
            services.AddSingleton<IFilterQueryGeneratorFactory, FilterQueryGeneratorFactory>();
            services.AddScoped<IEventGraphHelper, EventGraphHelper>();
            services.AddScoped<IUserGraphHelper, UserGraphHelper>();
            services.AddScoped<IGroupGraphHelper, GroupGraphHelper>();
            services.AddSingleton<IMessagingExtensionHelper, MessagingExtensionHelper>();
            services.AddHostedService<BackgroundNotificationHelper>();
        }

        /// <summary>
        /// Adds providers to specified IServiceCollection.
        /// </summary>
        /// <param name="services">Collection of services.</param>
        public static void RegisterRepositories(this IServiceCollection services)
        {
            services.AddSingleton<ILnDTeamConfigurationRepository, LnDTeamConfigurationRepository>();
            services.AddSingleton<IUserConfigurationRepository, UserConfigurationRepository>();
            services.AddSingleton<IEventRepository, EventRepository>();
            services.AddSingleton<ICategoryRepository, CategoryRepository>();
            services.AddSingleton<IBlobRepository, BlobRepository>();
        }

        /// <summary>
        /// Adds user state and conversation state to specified IServiceCollection.
        /// </summary>
        /// <param name="services">Collection of services.</param>
        /// <param name="configuration">Application configuration properties.</param>
        public static void RegisterBotStates(this IServiceCollection services, IConfiguration configuration)
        {
            // Create the User state. (Used in this bot's Dialog implementation.)
            services.AddSingleton<UserState>();

            // Create the Conversation state. (Used by the Dialog system itself.)
            services.AddSingleton<ConversationState>();

            // For conversation state.
            services.AddSingleton<IStorage>(new AzureBlobStorage(configuration.GetValue<string>("Storage:ConnectionString"), "bot-state"));
        }

        /// <summary>
        /// Adds Azure search service.
        /// </summary>
        /// <param name="services">Collection of services.</param>
        /// <param name="configuration">Application configuration properties.</param>
        public static void AddSearchService(this IServiceCollection services, IConfiguration configuration)
        {
            var isGCCHybrid = configuration.GetValue<bool>("DeploymentType:IsGCCHybrid");
            var searchDnsSuffix = isGCCHybrid ? "search.azure.us" : "search.windows.net";

#pragma warning disable CA2000 // This is singleton which has lifetime same as the app
            services.AddSingleton<ISearchServiceClient>(new SearchServiceClient(
                configuration.GetValue<string>("SearchService:SearchServiceName"),
                new SearchCredentials(configuration.GetValue<string>("SearchService:SearchServiceAdminApiKey"))));

            services.AddSingleton<ISearchServiceClient>(new SearchServiceClient(
                configuration.GetValue<string>("SearchService:SearchServiceName"),
                new SearchCredentials(configuration.GetValue<string>("SearchService:SearchServiceAdminApiKey")))
            {
                SearchDnsSuffix = searchDnsSuffix,
            });

            services.AddSingleton<ISearchIndexClient>(new SearchIndexClient(
                configuration.GetValue<string>("SearchService:SearchServiceName"),
                Constants.EventsIndex,
                new SearchCredentials(configuration.GetValue<string>("SearchService:SearchServiceQueryApiKey")))
            {
                SearchDnsSuffix = searchDnsSuffix,
            });
            services.AddSingleton<ISearchServiceClient>(new SearchServiceClient(configuration.GetValue<string>("SearchService:SearchServiceName"), new SearchCredentials(configuration.GetValue<string>("SearchService:SearchServiceAdminApiKey"))));
            services.AddSingleton<ISearchIndexClient>(new SearchIndexClient(configuration.GetValue<string>("SearchService:SearchServiceName"), Constants.EventsIndex, new SearchCredentials(configuration.GetValue<string>("SearchService:SearchServiceQueryApiKey"))));
            services.AddSingleton<IEventSearchService, EventSearchService>();
            services.AddSingleton<IUserEventSearchService, UserEventSearchService>();
            services.AddSingleton<ITeamEventSearchService, TeamEventSearchService>();
#pragma warning restore CA2000 // This is singleton which has lifetime same as the app
        }

        /// <summary>
        /// Add confidential credential provider to access api.
        /// </summary>
        /// <param name="services">Collection of services.</param>
        /// <param name="configuration">Application configuration properties.</param>
        public static void RegisterConfidentialCredentialProvider(this IServiceCollection services, IConfiguration configuration)
        {
            configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            IConfidentialClientApplication confidentialClientApp = ConfidentialClientApplicationBuilder.Create(configuration["MicrosoftAppId"])
                .WithClientSecret(configuration["MicrosoftAppPassword"])
                .Build();
            services.AddSingleton<IConfidentialClientApplication>(confidentialClientApp);
        }

        /// <summary>
        /// Adds credential providers for authentication.
        /// </summary>
        /// <param name="services">Collection of services.</param>
        /// <param name="configuration">Application configuration properties.</param>
        public static void RegisterCredentialProviders(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();
            services
                .AddSingleton(new MicrosoftAppCredentials(configuration.GetValue<string>("MicrosoftAppId"), configuration.GetValue<string>("MicrosoftAppPassword")));
        }

        /// <summary>
        /// Adds localization settings to specified IServiceCollection.
        /// </summary>
        /// <param name="services">Collection of services.</param>
        /// <param name="configuration">Application configuration properties.</param>
        public static void RegisterLocalizationSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.Configure<RequestLocalizationOptions>(options =>
            {
                var defaultCulture = CultureInfo.GetCultureInfo(configuration.GetValue<string>("i18n:DefaultCulture"));
                var supportedCultures = configuration.GetValue<string>("i18n:SupportedCultures").Split(',')
                    .Select(culture => CultureInfo.GetCultureInfo(culture))
                    .ToList();

                options.DefaultRequestCulture = new RequestCulture(defaultCulture);
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;

                options.RequestCultureProviders = new List<IRequestCultureProvider>
                {
                    new EmployeeTrainingLocalizationCultureProvider(),
                };
            });
        }
    }
}