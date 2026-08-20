using INest.Data.Entities.Infrastructure;

namespace INest.Infrastructure.Time
{
    public class UserTimeService : IUserTimeService
    {
        private readonly ILogger<UserTimeService> _logger;

        public UserTimeService(ILogger<UserTimeService> logger)
        {
            _logger = logger;
        }

        public bool IsAllowedToNotify(AppUser user, DateTime nowUtc, int startHour = 9)
        {
            var localTime = GetLocalTime(user, nowUtc);
            return localTime.Hour >= startHour;
        }

        public DateTime GetLocalTime(AppUser user, DateTime nowUtc)
        {
            string userTzId = string.IsNullOrWhiteSpace(user.TimeZoneId) ? "Europe/Kyiv" : user.TimeZoneId;

            if (!TimeZoneInfo.TryFindSystemTimeZoneById(userTzId, out var tzInfo))
            {
                if (userTzId == "Europe/Kyiv" && TimeZoneInfo.TryFindSystemTimeZoneById("FLE Standard Time", out var winTz))
                {
                    tzInfo = winTz;
                }
                else
                {
                    tzInfo = TimeZoneInfo.Utc;
                }
            }

            return TimeZoneInfo.ConvertTimeFromUtc(nowUtc, tzInfo);
        }
    }
}