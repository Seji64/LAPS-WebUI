using System.ComponentModel.DataAnnotations;

namespace LAPS_WebUI.Models
{
    public class UserLoginRequest
    {
        [Required]
        public string? Username { get; set; }
        [Required]
        public string? Password { get; set; }

        [Required]
        public string? DomainName { get; set; }
    }
}
