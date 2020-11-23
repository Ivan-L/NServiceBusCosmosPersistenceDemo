using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NServiceBus;
using NServiceBusCosmosDemo.Commands;
using NServiceBusCosmosDemo.Domain;

namespace NServiceBusCosmosDemo
{
    public class StartupBackgroundService : BackgroundService
    {
        private readonly ILogger<StartupBackgroundService> _logger;
        private readonly Container _container;
        private readonly IMessageSession _messageSession;

        public StartupBackgroundService(ILogger<StartupBackgroundService> logger, Container container, IMessageSession messageSession)
        {
            _logger = logger;
            _container = container;
            _messageSession = messageSession;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var quoteNumber = RandomString();
            await CreateQuote(quoteNumber);
            await AcceptQuote(quoteNumber);

            var premiumTenantId = RandomString();
            var quoteNumber2 = RandomString();
            await CreateQuote(quoteNumber2, premiumTenantId);
            await AcceptQuote(quoteNumber2, premiumTenantId);
        }

        private async Task CreateQuote(string quoteNumber, string tenantId = null)
        {
            _logger.LogInformation($"Creating quote with quote number '{quoteNumber}'...");

            var tenantContainer = _container;
            if (tenantId != null)
            {
                var containerInfo = TenantContainerInformation.ForTenant(tenantId);
                var response = await tenantContainer.Database.CreateContainerIfNotExistsAsync(
                    containerInfo.ContainerName,
                    containerInfo.PartitionKeyPath.ToString(), 400);
                tenantContainer = response.Container;
            }

            try
            {
                await tenantContainer.CreateItemAsync(new Quote(quoteNumber));
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Error while creating quote.");
                throw;
            }
        }
        
        private async Task AcceptQuote(string quoteNumber, string tenantId = null)
        {
            _logger.LogInformation($"Sending an AcceptQuoteCommand for quote '{quoteNumber}'...");

            var sendOptions = new SendOptions();
            sendOptions.RouteToThisEndpoint();
            if (tenantId != null)
            {
                sendOptions.SetHeader("TenantId", tenantId);
            }

            await _messageSession.Send(new AcceptQuoteCommand(quoteNumber), sendOptions);

            _logger.LogInformation("Command sent.");
        }

        private static string RandomString()
        {
            return Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
        }
    }
}