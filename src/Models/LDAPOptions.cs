using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LAPS_WebUI.Models
{
    public class LDAPOptions
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public bool UseSSL { get; set; }
        public bool TrustAllCertificates { get; set; }
        public string SearchBase { get; set; }
    }
}
