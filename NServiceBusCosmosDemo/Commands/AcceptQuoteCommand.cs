namespace NServiceBusCosmosDemo.Commands
{
    public class AcceptQuoteCommand : IProvideQuoteNumber
    {
        public AcceptQuoteCommand(string quoteNumber)
        {
            QuoteNumber = quoteNumber;
        }
        
        public string QuoteNumber { get; }
    }
}