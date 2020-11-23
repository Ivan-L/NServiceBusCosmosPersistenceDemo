using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace NServiceBusCosmosDemo.Persistence
{
    public interface IRepositoryV1<TDocument>
    {
        Task<TDocument> Load(string id, PartitionKey partitionKey);
        
        Task Upsert(TDocument document);
    }
}