using System.Runtime.Serialization;

namespace Transifex.Backend.Models
{
    public class TransifexString
    {
        [DataMember(Name = "source_entity__id")]
        public int Id { get; set; }

        [DataMember(Name = "string")]
        public string String { get; set; }

        [DataMember(Name = "reviewed")]
        public bool Reviewed { get; set; }

        [DataMember(Name = "translation")]
        public string Translation { get; set; }

        public TransifexStringDetails Details { get; set; }

        public TransifexString(int id, string str, bool reviewed, string translation)
        {
            Id = id;
            String = str;
            Reviewed = reviewed;
            Translation = translation;
        }
    }
}