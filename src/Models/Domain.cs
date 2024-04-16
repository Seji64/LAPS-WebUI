namespace LAPS_WebUI.Models
{
    public class Domain
    {
        public required string Name { get; set; }
        public required LdapOptions Ldap { get; set; }
        public LapsOptions Laps { get; set; } = new();
    }
}
