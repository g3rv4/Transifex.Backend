using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Jil;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
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
        public async Task<IActionResult> Query()
        {
            // I'd love for this to eventually use a parser to define a query syntax... in the meantime,
            // I'm doing something that works now

            string body = new StreamReader(Request.Body).ReadToEnd();
            var model = JSON.Deserialize<QueryRequestViewModel>(body, Options.CamelCase);

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
                    Builders<TransifexString>.Filter.Regex(s => s.Translation.String, new BsonRegularExpression(model.StringRegex))
                );
            }
            if (model.IsReviewed.HasValue)
            {
                filters.Add(
                    Builders<TransifexString>.Filter.Eq(s => s.Reviewed, model.IsReviewed.Value)
                );
            }
            if (model.WithNonReviewedSuggestions || model.OnlSuggestionsFromUsers?.Any()== true)
            {
                var matchObject = new BsonDocument();
                if (model.WithNonReviewedSuggestions)
                {
                    matchObject.Add(new BsonElement("daysCmp", new BsonDocument("$gte", 0)));
                }
                if (model.OnlSuggestionsFromUsers?.Any()== true)
                {
                    matchObject.Add(new BsonElement("Suggestions.Username", new BsonDocument("$in", new BsonArray(model.OnlSuggestionsFromUsers))));
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

            var matching = await stringsCollection.FindAsync(
                Builders<TransifexString>.Filter.And(filters)
            );
            var result = await matching.ToListAsync();

            return Json(result);
        }
    }
}