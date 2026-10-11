namespace LAPS_WebUI.Models
{
    public class UserSessionOptions
    {
        public int IdleTimeoutMinutes { get; set; } = 60;
        public int LifetimeMinutes { get; set; } = 480;
    }
}
