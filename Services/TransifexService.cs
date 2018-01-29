using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jil;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Transifex.Backend.Models;
using Transifex.Backend.Models.Services;

namespace Transifex.Backend.Services
{
    public interface ITransifexService
    {
        Task<IEnumerable<TransifexString>> GetAllStringsAsync();
        Task UpdateStringsDatabaseAsync();

    }

    public class TransifexService : ITransifexService
    {
        private static HttpClient _HttpClient { get; set; }
        private static string GetDetailsUrlForStringId(int stringId)=>
            $"_/editor/ajax/stack-exchange/stack-overflow-es/string_detail/es/{stringId}/?details&escape";

        private IRedisService _redisService { get; }
        private IConfiguration _configurationService { get; }
        private IMongoDbService _mongoDbService { get; }

        public TransifexService(IRedisService redisService, IConfiguration configurationService, IMongoDbService mongoDbService)
        {
            _redisService = redisService;
            _configurationService = configurationService;
            _mongoDbService = mongoDbService;
        }

        public async Task<IEnumerable<TransifexString>> GetAllStringsAsync()
        {
            var response = await GetUrlAsync("/_/editor/ajax/stack-exchange/stack-overflow-es/string_list/english/es/");
            var deserialized = JSON.Deserialize<AllTheStringsResponse>(response, Options.CamelCase);
            return deserialized.GetTransifexStrings();
        }

        public async Task UpdateStringsDatabaseAsync()
        {
            var strings = await GetAllStringsAsync();

            // the first block does all the http requests, in a lot of threads
            var fetchDetailsBlock = new TransformBlock<TransifexString, (TransifexString str, string content)>(async str =>
                {
                    // GetUrlAsync uses the cache on dev
                    var content = await GetUrlAsync(GetDetailsUrlForStringId(str.Id));
                    return (str, content);
                }, new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = 32
                });

            var stringsCollection = _mongoDbService.GetCollection<TransifexString>();
            var upsertOptions = new UpdateOptions { IsUpsert = true };

            // as we get the http responses back, we update the mongodb database            
            var storeInMongoBlock = new ActionBlock<(TransifexString str, string content)>(async data =>
            {
                var details = JSON.Deserialize<TransifexStringDetails>(data.content, Options.ISO8601CamelCase);
                data.str.Details = details;
                await stringsCollection.ReplaceOneAsync(
                    Builders<TransifexString>.Filter.Eq(s => s.Id, data.str.Id), // filter
                    data.str, // new value
                    upsertOptions
                );
            }, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 8
            });

            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
            fetchDetailsBlock.LinkTo(storeInMongoBlock, linkOptions);

            foreach (var str in strings)
            {
                await fetchDetailsBlock.SendAsync(str);
            }

            fetchDetailsBlock.Complete();
            try
            {
                await storeInMongoBlock.Completion;
            }
            catch (AggregateException ex)
            {
                throw;
            }
        }

        private HttpClient GetHttpClient()
        {
            if (_HttpClient == null)
            {
                var baseAddress = new Uri("https://www.transifex.com");

                var cookieContainer = new CookieContainer();
                cookieContainer.Add(baseAddress, new Cookie("sessionid", _configurationService.GetValue<string>("TRANSIFEX_SESSION")));

                var handler = new HttpClientHandler() { CookieContainer = cookieContainer };
                _HttpClient = new HttpClient(handler) { BaseAddress = baseAddress };
            }

            return _HttpClient;
        }

        private async Task<string> GetUrlAsync(string url)
        {
            var db = _redisService.GetDatabase();
            string cached = await db.StringGetAsync("url:" + url);
            if (cached != null)
            {
                return cached;
            }

            string content = null;

            using(var client = GetHttpClient())
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                content = await response.Content.ReadAsStringAsync();
            }

            // do not expire stuff in local (https://youtu.be/8iagmMy7JEE?t=1m11s)
            if (_configurationService.GetValue<string>("ASPNETCORE_ENVIRONMENT")== "Development")
            {
                await db.StringSetAsync("url:" + url, content);
            }
            else
            {
                await db.StringSetAsync("url:" + url, content, TimeSpan.FromMinutes(60));
            }
            return content;
        }
    }
}