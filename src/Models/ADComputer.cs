namespace LAPS_WebUI.Models
{
    public class AdComputer(string distinguishedName, string name)
    {
        public string Name { get; set; } = name;
        public string DistinguishedName { get; set; } = distinguishedName;
        public List<LapsInformation>? LapsInformations { get; set; }
        public bool FailedToRetrieveLapsDetails { get; set; }
        public bool Loading => LapsInformations is null;
    }
}
