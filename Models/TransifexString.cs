using System;
using System.Linq;
using System.Runtime.Serialization;
using Jil;
using Transifex.Backend.Helpers;

namespace Transifex.Backend.Models
{
    public class TransifexString
    {
        public class TranslationDetails
        {
            [JilDirective(Name = "last_update")]
            public DateTime LastUpdate { get; set; }

            public string String { get; set; }

            public string User { get; set; }
        }
        
        public class SuggestionData
        {
            public string Username { get; set; }

            public DateTime LastUpdate { get; set; }

            public string Suggestion { get; set; }
        }

        [DataMember(Name = "source_entity__id")]
        public int Id { get; set; }

        [DataMember(Name = "string")]
        public string String { get; set; }

        [DataMember(Name = "reviewed")]
        public bool Reviewed { get; set; }

        // members to be populated from the details json
        public TranslationDetails Translation { get; set; }

        public SuggestionData[] Suggestions{get;set;}

        public string Comment { get; set; }

        public string StringHash { get; set; }

        public string Key { get; set; }

        public TransifexString(int id, string str, bool reviewed)
        {
            Id = id;
            String = str;
            Reviewed = reviewed;
        }

        public void LoadDataFromDetails(TransifexStringDetails details){
            Comment = details.Details.Comment;
            StringHash = details.Details.StringHash;
            Key = details.Details.Key;

            Translation = details.Translations.GetValueOrDefault(5);
            Suggestions = details.Suggestions.Select(s => new SuggestionData{
                Username = s.Username,
                LastUpdate = s.LastUpdate,
                Suggestion = s.Entries[5]
            }).ToArray();
        }
    }
}