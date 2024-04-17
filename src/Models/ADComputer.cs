namespace LAPS_WebUI.Models
{
    public class ADComputer
    {
        public ADComputer(string DistinguishedName, string name)
        {
            this.DistinguishedName = DistinguishedName;
            this.Name = name;
        }

        public string Name { get; set; }

        public string DistinguishedName { get; set; }
        public List<LapsInformation>? LAPSInformations { get; set; }
        public bool FailedToRetrieveLAPSDetails { get; set; }
        public bool Loading
        {
            get
            {
                if (LAPSInformations is null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
