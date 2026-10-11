using LAPS_WebUI.Models;
using Microsoft.Extensions.Options;
using System.Threading.RateLimiting;

namespace LAPS_WebUI.Services
{
    // Limits failed logins per client IP (opt-in), so one source can't spray passwords or lock out AD accounts in bulk.
    // Per IP rather than per user: a per-user limit would let anyone lock a user out of the app, and AD lockout already guards single accounts.
    public class LoginThrottleService
    {
        private readonly bool _enabled;
        private readonly PartitionedRateLimiter<string> _failedLogins;

        public LoginThrottleService(IOptions<LoginRateLimitOptions> options)
        {
            LoginRateLimitOptions rateLimit = options.Value;
            _enabled = rateLimit.Enabled;
            _failedLogins = PartitionedRateLimiter.Create<string, string>(remoteIp =>
                RateLimitPartition.GetFixedWindowLimiter(remoteIp, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = rateLimit.MaxFailedAttempts,
                    Window = TimeSpan.FromMinutes(rateLimit.WindowMinutes)
                }));
        }

        public bool IsBlocked(string? remoteIp) => _enabled && _failedLogins.GetStatistics(Key(remoteIp))?.CurrentAvailablePermits == 0;

        public void RegisterFailure(string? remoteIp)
        {
            if (_enabled)
            {
                _failedLogins.AttemptAcquire(Key(remoteIp)).Dispose();
            }
        }

        private static string Key(string? remoteIp) => remoteIp ?? "unknown";
    }
}
