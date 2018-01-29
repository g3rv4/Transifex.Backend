using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace Transifex.Backend.Services
{
    public interface IRedisService
    {
        IDatabase GetDatabase();
    }

    public class RedisService : IRedisService
    {
        private ConnectionMultiplexer Connection { get; set; }
        private int DatabaseId { get; set; }

        public RedisService(IConfiguration _configuration)
        {
            Connection = ConnectionMultiplexer.Connect(_configuration.GetValue<string>("REDIS_CONNECTION"));
            DatabaseId = _configuration.GetValue<int>("REDIS_DATABASE");
        }

        IDatabase IRedisService.GetDatabase()
        {
            return Connection.GetDatabase(DatabaseId);
        }
    }
}