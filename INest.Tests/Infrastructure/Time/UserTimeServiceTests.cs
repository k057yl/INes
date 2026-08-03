using System.Runtime.InteropServices;
using INest.Data.Entities.Infrastructure;
using INest.Infrastructure.Time;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;

namespace INest.Tests.Infrastructure.Time
{
    public class UserTimeServiceTests
    {
        private readonly UserTimeService _timeService;

        private static string KyivTimeZoneId => RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "FLE Standard Time"
            : "Europe/Kyiv";

        public UserTimeServiceTests()
        {
            var loggerMock = Substitute.For<ILogger<UserTimeService>>();
            _timeService = new UserTimeService(loggerMock);
        }

        [Fact]
        public void GetLocalTime_ShouldFallbackToUtc_WhenTimeZoneIsInvalid()
        {
            // Arrange
            var user = new AppUser { TimeZoneId = "Invalid/NonExistent_TZ" };
            var utcNow = new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);

            // Act
            var localTime = _timeService.GetLocalTime(user, utcNow);

            // Assert
            localTime.ShouldBe(utcNow);
        }

        [Fact]
        public void IsAllowedToNotify_ShouldReturnTrue_WhenLocalHourIsAtOrAfterStartHour()
        {
            // Arrange
            var user = new AppUser { TimeZoneId = KyivTimeZoneId };
            // 07:00 UTC = 10:00 в Киеве (летнее время EEST UTC+3)
            var utcNow = new DateTime(2026, 6, 1, 7, 0, 0, DateTimeKind.Utc);

            // Act
            var isAllowed = _timeService.IsAllowedToNotify(user, utcNow, startHour: 9);

            // Assert
            isAllowed.ShouldBeTrue();
        }

        [Fact]
        public void IsAllowedToNotify_ShouldReturnFalse_WhenLocalHourIsBeforeStartHour()
        {
            // Arrange
            var user = new AppUser { TimeZoneId = KyivTimeZoneId };
            // 05:00 UTC = 08:00 в Киеве (летнее время EEST UTC+3) -> раньше 9:00
            var utcNow = new DateTime(2026, 6, 1, 5, 0, 0, DateTimeKind.Utc);

            // Act
            var isAllowed = _timeService.IsAllowedToNotify(user, utcNow, startHour: 9);

            // Assert
            isAllowed.ShouldBeFalse();
        }
    }
}