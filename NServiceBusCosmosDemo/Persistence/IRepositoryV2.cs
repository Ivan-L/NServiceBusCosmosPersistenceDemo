using System.Threading.Tasks;

namespace NServiceBusCosmosDemo.Persistence
{
    public interface IRepositoryV2<TDocument>
    {
        Task<TDocument> Load(string id);

        void Upsert(TDocument document);
    }
}