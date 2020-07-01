using System.Runtime.Serialization;

namespace LAPS_WebUI.WebServices
{
    [DataContract]
    class SearchRequest
    {
        [DataMember]
        public string term { get; set; }

        [DataMember]
        public string q { get; set; }

        [DataMember]
        public string _type { get; set; }

        [DataMember]
        public int page { get; set; }
    }
}
