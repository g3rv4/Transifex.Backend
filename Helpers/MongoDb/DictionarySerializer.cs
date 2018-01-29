using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;

namespace Transifex.Backend.Helpers.MongoDb
{
    // from https://stackoverflow.com/a/32469616/920295
    public abstract class DictionarySerializer<K, V> : DictionarySerializerBase<Dictionary<K, V>>
    {
        public DictionarySerializer(): base(DictionaryRepresentation.Document) { }

        protected override Dictionary<K, V> CreateInstance()
        {
            return new Dictionary<K, V>();
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Dictionary<K, V> value)
        {
            if (value != null)
            {
                var dic = value.ToDictionary(d => d.Key.ToString(), d => d.Value);
                BsonSerializer.Serialize<Dictionary<string, V>>(context.Writer, dic);
            }
            else
            {
                BsonSerializer.Serialize<object>(context.Writer, null);
            }
        }
        public override Dictionary<K, V> Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var dic = BsonSerializer.Deserialize<Dictionary<string, V>>(context.Reader);
            return dic?.ToDictionary(e => Parse(e.Key), e => e.Value);
        }

        protected virtual K Parse(string s)
        {
            throw new NotImplementedException();
        }
    }

    public class DictionarySerializerInt<V> : DictionarySerializer<int, V>
    {
        protected override int Parse(string s)
        {
            if (int.TryParse(s, out var res))
            {
                return res;
            }
            throw new ArgumentException();
        }
    }
}