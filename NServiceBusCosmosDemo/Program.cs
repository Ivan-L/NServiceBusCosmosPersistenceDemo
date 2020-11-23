using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NServiceBus;
using NServiceBusCosmosDemo.Behaviors;
using NServiceBusCosmosDemo.Domain;
using NServiceBusCosmosDemo.Persistence;

namespace NServiceBusCosmosDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseNServiceBus(ctx =>
                {
                    var endpointConfiguration = new EndpointConfiguration("cosmos-demo");
                    endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
                    endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);

                    if (Environment.UserInteractive && Debugger.IsAttached)
                    {
                        endpointConfiguration.UseTransport<LearningTransport>();
                        endpointConfiguration.EnableInstallers();
                    }

                    var conventions = endpointConfiguration.Conventions();
                    conventions.DefiningCommandsAs(type =>
                        type.Namespace?.Contains(".Commands") == true);
                    conventions.DefiningEventsAs(type =>
                        type.Namespace?.Contains(".Events") == true);

                    // Start of Cosmos persistence configuration
                    var config = ctx.Configuration
                        .GetSection("Cosmos")
                        .Get<CosmosConfiguration>();
                    var cosmosClient = new CosmosClient(config.AccountEndpoint, config.AuthKey);

                    endpointConfiguration.UsePersistence<CosmosPersistence>()
                        .CosmosClient(cosmosClient)
                        .DatabaseName(config.DatabaseName)
                        .DefaultContainer("Quotes", "/QuoteNumber");
                    endpointConfiguration.EnableOutbox();
                    endpointConfiguration.Pipeline.Register(new QuoteNumberAsPartitionKeyBehavior.Registration());
                    // End of Cosmos persistence configuration

                    return endpointConfiguration;
                })
                .ConfigureServices((ctx, services) =>
                {
                    var config = ctx.Configuration
                        .GetSection("Cosmos")
                        .Get<CosmosConfiguration>();
                    var cosmosClient = new CosmosClient(config.AccountEndpoint, config.AuthKey);
                    
                    // For the V1 repository implementation, we register a singleton Container.
                    // We also use this container to seed the database.
                    services.AddSingleton(cosmosClient.GetContainer(config.DatabaseName, config.DefaultContainer));
                    services.AddScoped<IRepositoryV1<Quote>, RepositoryV1<Quote>>();

                    // For the V2 implementation, we use the ICosmosStorageSession provided by the
                    // NServiceBus Cosmos DB persistence package.
                    services.AddScoped<IRepositoryV2<Quote>, RepositoryV2<Quote>>();

                    // This hosted service will seed our DB and enqueue messages for us
                    services.AddHostedService<StartupBackgroundService>();
                });
        
        #region Plumbing
        
        private static async Task OnCriticalError(ICriticalErrorContext context)
        {
            // TODO: decide if stopping the endpoint and exiting the process is the best response to a critical error
            // https://docs.particular.net/nservicebus/hosting/critical-errors
            try
            {
                await context.Stop();
            }
            finally
            {
                FailFast($"Critical error, shutting down: {context.Error}", context.Exception);
            }
        }

        private static void FailFast(string message, Exception exception)
        {
            try
            {
                // TODO: decide what kind of last resort logging is necessary
                // TODO: when using an external logging framework it is important to flush any pending entries prior to calling FailFast
                // https://docs.particular.net/nservicebus/hosting/critical-errors#when-to-override-the-default-critical-error-action
            }
            finally
            {
                Environment.FailFast(message, exception);
            }
        }
        
        #endregion
    }
}