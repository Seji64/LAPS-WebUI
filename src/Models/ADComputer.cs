namespace LAPS_WebUI.Models
{
    public class ADComputer
    {
        public ADComputer(string name)
        {
            this.Name = name;
            this.LAPSPassword = String.Empty;
            this.LAPSPasswordExpireDate = DateTime.MinValue;
        }

        public string Name { get; set; }
        public string LAPSPassword { get; set; }
        public DateTime LAPSPasswordExpireDate { get; set; }
    }
}
