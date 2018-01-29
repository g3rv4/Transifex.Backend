using System;
using System.Collections.Generic;
using Jil;
using MongoDB.Bson.Serialization.Attributes;
using Transifex.Backend.Helpers.MongoDb;

namespace Transifex.Backend.Models
{
    public class TransifexStringDetails
    {
        public class Translation
        {
            [JilDirective(Name = "last_update")]
            public DateTime LastUpdate { get; set; }

            public string String { get; set; }

            public string User { get; set; }
        }

        public class DetailData
        {
            public string Comment { get; set; }
            public string Key { get; set; }
        }

        [BsonSerializer(typeof(DictionarySerializerInt<Translation>))]
        public Dictionary<int, Translation> Translations { get; set; }

        public DetailData Details { get; set; }
    }
}