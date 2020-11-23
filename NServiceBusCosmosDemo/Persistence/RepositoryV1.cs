using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace NServiceBusCosmosDemo.Persistence
{
    public class RepositoryV1<TDocument> : IRepositoryV1<TDocument>
    {
        private readonly Container _container;

        public RepositoryV1(Container container)
        {
            _container = container;
        }

        public async Task<TDocument> Load(string id, PartitionKey partitionKey)
        {
            return await _container.ReadItemAsync<TDocument>(id, partitionKey).ConfigureAwait(false);
        }

        public async Task Upsert(TDocument document)
        {
            await _container.UpsertItemAsync(document).ConfigureAwait(false);
        }
    }
}