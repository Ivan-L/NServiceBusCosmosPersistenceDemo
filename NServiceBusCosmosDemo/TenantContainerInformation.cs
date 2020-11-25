using NServiceBus;

namespace NServiceBusCosmosDemo
{
    internal static class TenantContainerInformation
    {
        private const string PartitionKeyPath = "/QuoteNumber";
        
        public static readonly ContainerInformation Default = new ContainerInformation("Quotes", new PartitionKeyPath(PartitionKeyPath));

        public static ContainerInformation ForTenant(string tenantId)
        {
            return new ContainerInformation($"Quotes_Tenant_{tenantId}", new PartitionKeyPath(PartitionKeyPath));
        }
    }
}