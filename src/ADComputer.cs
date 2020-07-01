using Integrative.Lara;
using System;

namespace LAPS_WebUI
{
    public class ADComputer:BindableBase
    {

        public ADComputer(string name)
        {
            this.Name = name;
        }

        private string m_Name;

        public string Name
        {
            get => m_Name;
            set => SetProperty(ref m_Name, value);
        }

        private string m_LAPSPassword;

        public string LAPSPassword
        {
            get => m_LAPSPassword;
            set => SetProperty(ref m_LAPSPassword, value);
        }

        private DateTime m_LAPSPasswordExpireDate;

        public DateTime LAPSPasswordExpireDate
        {
            get => m_LAPSPasswordExpireDate;
            set => SetProperty(ref m_LAPSPasswordExpireDate, value);
        }

    }
}
