namespace NServiceBusCosmosDemo.Events
{
    public class QuoteAcceptedEvent
    {
        public QuoteAcceptedEvent(string quoteNumber)
        {
            QuoteNumber = quoteNumber;
        }
        
        public string QuoteNumber { get; }
    }
}