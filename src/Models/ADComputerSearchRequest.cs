using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LAPS_WebUI.Models
{
    public class ADComputerSearchRequest
    {
        [Required]
        public ADComputer SelectedADComputer { get; set; }
    }
}
