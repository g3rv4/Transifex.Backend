using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Transifex.Backend.Models.Services
{
    public class AllTheStringsResponse
    {
        public string[] Fields { get; set; }

        public dynamic[][] Objects { get; set; }

        private static Dictionary<string, int> NameToPosition;

        public IEnumerable<TransifexString> GetTransifexStrings()
        {
            if (NameToPosition == null)
            {
                NameToPosition = new Dictionary<string, int>();
                var properties = typeof(TransifexString).GetProperties();
                foreach (var property in properties)
                {
                    var attributes = property.GetCustomAttributes(typeof(DataMemberAttribute), true);
                    foreach (DataMemberAttribute dma in attributes)
                    {
                        NameToPosition[property.Name] = Array.IndexOf(Fields, dma.Name);
                    }
                }
            }

            return Objects.Select(o => new TransifexString(
                (int)o[NameToPosition[nameof(TransifexString.Id)]],
                (string)o[NameToPosition[nameof(TransifexString.String)]],
                (bool)o[NameToPosition[nameof(TransifexString.Reviewed)]]
            ));
        }
    }
}