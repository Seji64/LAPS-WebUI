using Microsoft.AspNetCore.Server.IIS.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace LAPS_WebUI
{

    public class LDAP
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public bool UseSSL { get; set; }
    }

    public class Settings
    {
       public Settings()
        {
            ThisInstance = this;
        }
        public static Settings ThisInstance { get; private set; }

        public LDAP LDAP { get; set; }

        public string ListenAddress { get; set; } = "127.0.0.1";

        public int ListenPort { get; set; } = 1337;

    }
}
