using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using NServiceBus.Persistence.CosmosDB;
using NServiceBus.Pipeline;
using NServiceBusCosmosDemo.Commands;

namespace NServiceBusCosmosDemo.Behaviors
{
    public class QuoteNumberAsPartitionKeyBehavior : Behavior<IIncomingLogicalMessageContext>
    {
        public override Task Invoke(IIncomingLogicalMessageContext context, Func<Task> next)
        {
            if (context.Message.Instance is IProvideQuoteNumber provideQuoteNumber)
            {
                context.Extensions.Set(new PartitionKey(provideQuoteNumber.QuoteNumber));
            }
            
            return next();
        }
        
        #region Plumbing
        
        internal class Registration : RegisterStep
        {
            public Registration() :
                base(nameof(QuoteNumberAsPartitionKeyBehavior),
                    typeof(QuoteNumberAsPartitionKeyBehavior),
                    "Determines the PartitionKey from the logical message",
                    b => new QuoteNumberAsPartitionKeyBehavior())
            {
                InsertBefore(nameof(LogicalOutboxBehavior));
            }
        }
        
        #endregion
    }
}