using System;
using INest.Infrastructure.Tracker;
using Shouldly;
using Xunit;

namespace INest.Tests.Infrastructure.Caching
{
    public class CacheTrackerTests
    {
        [Fact]
        public void GetToken_ShouldProvideToken_AndCancelItOnInvalidateUserCache()
        {
            // Arrange
            var tracker = new CacheTracker();
            var userId = Guid.NewGuid();

            // Act
            var token = tracker.GetToken(userId);
            token.HasChanged.ShouldBeFalse();

            tracker.InvalidateUserCache(userId);

            // Assert
            token.HasChanged.ShouldBeTrue();
        }
    }
}