using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LAPS_WebUI.Models
{
    public class ADComputer
    {
        public ADComputer(string name)
        {
            this.Name = name;
        }

        public string Name { get; set; }
        public string LAPSPassword { get; set; }
        public DateTime LAPSPasswordExpireDate { get; set; }
    }
}
