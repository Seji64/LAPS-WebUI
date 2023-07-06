namespace LAPS_WebUI.Models
{
    public class ADComputer
    {
        public ADComputer(string name)
        {
            this.Name = name;
        }

        public string Name { get; set; }
        public List<LapsInformation>? LAPSInformations { get; set; }

        public bool Loading
        {
            get
            {
                if (LAPSInformations is null || !LAPSInformations.Any())
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
