using Microsoft.Extensions.Primitives;

namespace INest.Infrastructure.Tracker
{
    public interface ICacheTracker
    {
        IChangeToken GetToken(Guid userId);
        void InvalidateUserCache(Guid userId);
    }
}
