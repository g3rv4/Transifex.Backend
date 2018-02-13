using System;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Transifex.Backend.Helpers;
using Transifex.Backend.Models;
using Transifex.Backend.Services;
using Transifex.Backend.ViewModels.Home;

namespace Transifex.Backend.Controllers
{
    public class HomeController : Controller
    {
        private IMongoDbService _mongoDbService { get; }

        public HomeController(IMongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        [HttpPost]
        [Route("api/home/query")]
        [ProducesResponseType(typeof(List<TransifexString>), 200)]
        public async Task<IActionResult> Query([FromBody] QueryRequestViewModel model)
        {
            // I'd love for this to eventually use a parser to define a query syntax... in the meantime,
            // I'm doing something that works now

            var stringsCollection = _mongoDbService.GetCollection<TransifexString>();

            // let's build the mongodb filters... maybe they should go in a service? probably
            var filters = new List<FilterDefinition<TransifexString>> { Builders<TransifexString>.Filter.Empty };
            if (model.StringRegex.HasValue())
            {
                filters.Add(
                    Builders<TransifexString>.Filter.Regex(s => s.String, new BsonRegularExpression(model.StringRegex))
                );
            }
            if (model.TranslationRegex.HasValue())
            {
                filters.Add(
                    Builders<TransifexString>.Filter.Regex(s => s.Translation.String, new BsonRegularExpression(model.TranslationRegex))
                );
            }
            if (model.IsReviewed.HasValue)
            {
                filters.Add(
                    Builders<TransifexString>.Filter.Eq(s => s.Reviewed, model.IsReviewed.Value)
                );
            }
            if (model.WithNonReviewedSuggestions == true || model.OnlySuggestionsFromUsers?.Any()== true)
            {
                var matchObject = new BsonDocument();
                if (model.WithNonReviewedSuggestions == true)
                {
                    matchObject.Add(new BsonElement("daysCmp", new BsonDocument("$gte", 0)));
                }
                if (model.OnlySuggestionsFromUsers?.Any()== true)
                {
                    matchObject.Add(new BsonElement("Suggestions.Username", new BsonDocument("$in", new BsonArray(model.OnlySuggestionsFromUsers))));
                }

                var matchingIds = await stringsCollection.AggregateAsync<BsonDocument>(
                    new BsonDocument[]
                    {
                        new BsonDocument("$unwind", "$Suggestions"),
                            new BsonDocument("$project", new BsonDocument
                            { { "_id", 1 }, { "Suggestions.Username", 1 },
                                {
                                    "daysCmp",
                                    new BsonDocument("$cmp", new BsonArray { "$Suggestions.LastUpdate", "$Translation.LastUpdate" })
                                }
                            }),
                            new BsonDocument("$match", matchObject)
                    }
                );
                var docs = (await matchingIds.ToListAsync()).Select(doc => doc.GetValue("_id").AsInt32).ToList();
                filters.Add(
                    Builders<TransifexString>.Filter.In(s => s.Id, docs)
                );
            }
            if (model.OnlyTranslationsFromUsers?.Any()== true)
            {
                filters.Add(
                    Builders<TransifexString>.Filter.In(s => s.Translation.User, model.OnlyTranslationsFromUsers)
                );
            }

            IAsyncCursor<TransifexString> matching;

            var oneSecond = TimeSpan.FromSeconds(1);
            using(var cancellationTokenSource = new CancellationTokenSource(oneSecond))
            {
                matching = await stringsCollection.FindAsync(
                    Builders<TransifexString>.Filter.And(filters),
                    new FindOptions<TransifexString> { MaxTime = oneSecond, MaxAwaitTime = oneSecond },
                    cancellationToken : cancellationTokenSource.Token
                );
            }
            var result = matching.ToEnumerable().Take(200);
            return Json(result);
        }
    }
}