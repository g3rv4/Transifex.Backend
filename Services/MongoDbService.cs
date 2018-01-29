using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Transifex.Backend.Services
{
    public interface IMongoDbService
    {
        IMongoCollection<T> GetCollection<T>();
    }

    public class MongoDbService : IMongoDbService
    {
        private MongoClient Client { get; set; }
        private string Database { get; set; }

        public MongoDbService(IConfiguration _configuration)
        {
            Client = new MongoClient(_configuration.GetValue<string>("MONGO_SERVER"));
            Database = _configuration.GetValue<string>("MONGO_DATABASE");
        }

        public IMongoDatabase GetDatabase()
        {
            return Client.GetDatabase(Database);
        }

        public IMongoCollection<TransifexString> GetCollection<TransifexString>()
        {
            var client = GetDatabase();
            return client.GetCollection<TransifexString>("strings");
        }
    }
}