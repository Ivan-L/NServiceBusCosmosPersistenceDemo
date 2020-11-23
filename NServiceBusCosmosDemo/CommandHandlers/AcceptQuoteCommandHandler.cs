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
        private readonly IRepositoryV2<Quote> _repository;

        public AcceptQuoteCommandHandler(IRepositoryV2<Quote> repository)
        {
            _repository = repository;
        }

        public async Task Handle(AcceptQuoteCommand message, IMessageHandlerContext context)
        {
            var quote = await _repository.Load(message.QuoteNumber)
                .ConfigureAwait(false);
            
            quote.Accept();

            _repository.Upsert(quote);

            await context.Publish(new QuoteAcceptedEvent(message.QuoteNumber)).ConfigureAwait(false);
        }
    }
}