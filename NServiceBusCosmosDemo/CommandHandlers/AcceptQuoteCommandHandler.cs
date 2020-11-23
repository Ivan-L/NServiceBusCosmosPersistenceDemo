using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using NServiceBus;
using NServiceBusCosmosDemo.Commands;
using NServiceBusCosmosDemo.Domain;
using NServiceBusCosmosDemo.Events;
using NServiceBusCosmosDemo.Persistence;

namespace NServiceBusCosmosDemo.CommandHandlers
{
    public class AcceptQuoteCommandHandler : IHandleMessages<AcceptQuoteCommand>
    {
        private readonly IRepositoryV1<Quote> _repository;

        public AcceptQuoteCommandHandler(IRepositoryV1<Quote> repository)
        {
            _repository = repository;
        }

        public async Task Handle(AcceptQuoteCommand message, IMessageHandlerContext context)
        {
            var quote = await _repository.Load(message.QuoteNumber, new PartitionKey(message.QuoteNumber))
                .ConfigureAwait(false);
            
            quote.Accept();

            await _repository.Upsert(quote).ConfigureAwait(false);
            
            throw new Exception("Something went wrong.");

            await context.Publish(new QuoteAcceptedEvent(message.QuoteNumber)).ConfigureAwait(false);
        }
    }
}