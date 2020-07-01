using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LAPS_WebUI.WebServices
{
    [DataContract]
    class SearchResults
    {

        [DataMember]
        public List<SearchResult> results { get; set; } = new List<SearchResult>();

    }

    [DataContract]
    class SearchResult
    {
        [DataMember]
        public int id { get; set; }

        [DataMember]
        public string text { get; set; }

        [DataMember]
        public bool selected { get; set; }

        [DataMember]
        public bool disabled { get; set; }
    }
}
