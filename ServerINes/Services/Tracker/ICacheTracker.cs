using Microsoft.Extensions.Primitives;

namespace INest.Services.Tracker
{
    public interface ICacheTracker
    {
        IChangeToken GetToken(Guid userId);
        void InvalidateUserCache(Guid userId);
    }
}
