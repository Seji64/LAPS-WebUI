using Integrative.Lara;

namespace LAPS_WebUI
{
    internal class LAPSData : BindableBase
    {
        
        ADComputer m_selectedADComputer;
        public ADComputer selectedADComputer
        {
            get => m_selectedADComputer;

            set => SetProperty(ref m_selectedADComputer, value);
        }

        public string LAPSPassword
        {
            get
            {
                if (selectedADComputer != null)
                {
                    return selectedADComputer.LAPSPassword;
                }
                else
                {
                    return "N/A";
                }
            }
        }

        public string ComputerName
        {
            get
            {
                if (selectedADComputer != null)
                {
                    return selectedADComputer.Name;
                }
                else
                {
                    return "N/A";
                }
            }
        }

        public string LAPSPasswordExpireDate
        {
            get
            {

                if (selectedADComputer != null)
                {
                    return selectedADComputer.LAPSPasswordExpireDate.ToString("dd.MM.yyy hh:mm:ss");
                }
                else
                {
                    return "N/A";
                }
            }
        }

        private string m_ErrorMessage = "_";

        public string ErrorMessage
        {
            get => m_ErrorMessage;
            set => SetProperty(ref m_ErrorMessage, value);
        }
    }
}
