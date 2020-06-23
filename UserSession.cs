using Integrative.Lara;
using LAPS_WebUI.PageData;

namespace LAPS_WebUI
{
    static class UserSession
    {

        readonly static SessionLocal<bool> m_loggedIn = new SessionLocal<bool>();
        readonly static SessionLocal<LoginData> m_loginData = new SessionLocal<LoginData>();
        public static bool LoggedIn
        {
            get
            {
                try
                {
                    return m_loggedIn.Value;
                }
                catch (NoCurrentSessionException)
                {
                    return false;
                }
            }
            set => m_loggedIn.Value = value;
        }

        public static LoginData loginData
        {
            get => m_loginData.Value;
            set => m_loginData.Value = value;
        }

    }
}
