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
            var quoteNumber = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            
            _logger.LogInformation($"Creating quote with quote number '{quoteNumber}'...");

            try
            {
                await _container.CreateItemAsync(new Quote(quoteNumber), cancellationToken: stoppingToken);
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Error while creating quote.");
                throw;
            }
            
            _logger.LogInformation($"Sending an AcceptQuoteCommand for quote '{quoteNumber}'..."); 
            
            await _messageSession.SendLocal(new AcceptQuoteCommand(quoteNumber));
            
            _logger.LogInformation("Command sent."); 
        }
    }
}