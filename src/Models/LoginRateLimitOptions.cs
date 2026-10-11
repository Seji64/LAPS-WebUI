namespace LAPS_WebUI.Models
{
    public class LoginRateLimitOptions
    {
        public bool Enabled { get; set; }
        public int MaxFailedAttempts { get; set; } = 10;
        public int WindowMinutes { get; set; } = 15;
    }
}
