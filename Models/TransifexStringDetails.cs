using System;
using System.Collections.Generic;
using Jil;
using MongoDB.Bson.Serialization.Attributes;
using Transifex.Backend.Helpers.MongoDb;

namespace Transifex.Backend.Models
{
    public class TransifexStringDetails
    {
        public class DetailData
        {
            public string Comment { get; set; }

            [JilDirective(Name = "string_hash")]
            public string StringHash { get; set; }

            public string Key { get; set; }
        }

        public class SuggestionData
        {
            [JilDirective(Name = "user__username")]
            public string Username { get; set; }

            [JilDirective(Name = "last_update")]
            public DateTime LastUpdate { get; set; }

            public Dictionary<int, string> Entries { get; set; }
        }

        public Dictionary<int, TransifexString.TranslationDetails> Translations { get; set; }

        public DetailData Details { get; set; }

        public SuggestionData[] Suggestions { get; set; }
    }
}