using INest.Data.Entities.Infrastructure;

namespace INest.Infrastructure.Time
{
    public interface IUserTimeService
    {
        bool IsAllowedToNotify(AppUser user, DateTime nowUtc, int startHour = 9);
        DateTime GetLocalTime(AppUser user, DateTime nowUtc);
    }
}
