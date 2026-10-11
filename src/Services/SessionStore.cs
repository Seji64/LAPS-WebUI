using LAPS_WebUI.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace LAPS_WebUI.Services
{
    // Keeps logged-in users' credentials in server memory; the browser only holds a random session ID
    public class SessionStore(IOptions<UserSessionOptions> options)
    {
        private readonly MemoryCache _sessions = new(new MemoryCacheOptions());

        public string Create(UserSession session)
        {
            string sessionId = RandomNumberGenerator.GetHexString(64);
            _sessions.Set(sessionId, session, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(options.Value.IdleTimeoutMinutes),
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(options.Value.LifetimeMinutes)
            });
            return sessionId;
        }

        public UserSession? Get(string sessionId) => _sessions.TryGetValue(sessionId, out UserSession? session) ? session : null;

        public void Remove(string sessionId) => _sessions.Remove(sessionId);
    }
}
