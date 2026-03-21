using Microsoft.Extensions.Primitives;
using System.Collections.Concurrent;

namespace INest.Services.Tracker
{
    public class CacheTracker : ICacheTracker
    {
        private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _userTokens = new();

        public IChangeToken GetToken(Guid userId)
        {
            var cts = _userTokens.GetOrAdd(userId, _ => new CancellationTokenSource());
            return new CancellationChangeToken(cts.Token);
        }

        public void InvalidateUserCache(Guid userId)
        {
            if (_userTokens.TryRemove(userId, out var cts))
            {
                cts.Cancel();
                cts.Dispose();
            }
        }
    }
}
