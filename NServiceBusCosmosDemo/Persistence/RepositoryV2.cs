using System.Threading.Tasks;
using NServiceBus;

namespace NServiceBusCosmosDemo.Persistence
{
    public class RepositoryV2<TDocument> : IRepositoryV2<TDocument>
    {
        private readonly ICosmosStorageSession _cosmosStorageSession;

        public RepositoryV2(ICosmosStorageSession cosmosStorageSession)
        {
            _cosmosStorageSession = cosmosStorageSession;
        }

        public async Task<TDocument> Load(string id)
        {
            return await _cosmosStorageSession.Container
                .ReadItemAsync<TDocument>(id, _cosmosStorageSession.PartitionKey)
                .ConfigureAwait(false);
        }

        public void Upsert(TDocument document)
        {
            _cosmosStorageSession.Batch.UpsertItem(document);
        }
    }
}