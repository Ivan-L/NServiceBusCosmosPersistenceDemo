using System;
using Newtonsoft.Json;

namespace NServiceBusCosmosDemo.Domain
{
    public class Quote
    {
        public Quote(string quoteNumber)
        {
            Id = quoteNumber;
            QuoteNumber = quoteNumber;
        }

        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty]
        public string QuoteNumber { get; }
        
        [JsonProperty]
        public bool IsAccepted { get; private set; }

        public void Accept()
        {
            if (IsAccepted)
            {
                throw new InvalidOperationException("This quote has already been accepted.");
            }

            IsAccepted = true;
        }
    }
}