using Integrative.Lara;
using System;
using System.Collections.Generic;
using System.Text;

namespace LAPS_WebUI.PageData
{
    class LoginData: BindableBase
    {

        string m_username = String.Empty;

        public string Username
        {
            get => m_username;
            set => SetProperty(ref m_username, value);
        }

        string m_password = String.Empty;

        public string Password
        {
            get => m_password;
            set => SetProperty(ref m_password, value);
        }

    }
}
